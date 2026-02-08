using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Text.Json;
using TruthDoctor.Services;

namespace TruthDoctor;

public partial class ValidationWindow : Window
{
    private readonly ApiService? _apiService;

    // Required by Avalonia runtime loader
    public ValidationWindow()
    {
        InitializeComponent();
    }

    // Used by your application logic
    public ValidationWindow(string json, ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
        LoadValidation(json);
    }


    private async void OnRefreshClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var scoreText = this.FindControl<TextBlock>("ScoreText");

        if (_apiService == null)
        {
            if (scoreText != null)
                scoreText.Text = "Refresh unavailable (no API connection)";
            return;
        }

        try
        {
            var json = await _apiService.GetValidationAsync();
            LoadValidation(json);
        }
        catch (Exception ex)
        {
            if (scoreText != null)
                scoreText.Text = "Refresh failed: " + ex.Message;
        }
    }


    private void LoadValidation(string json)
    {
        var scoreText = this.FindControl<TextBlock>("ScoreText");
        var passText = this.FindControl<TextBlock>("PassText");
        var failText = this.FindControl<TextBlock>("FailText");
        var timestampText = this.FindControl<TextBlock>("TimestampText");
        var statusText = this.FindControl<TextBlock>("ConnectionStatusText");
        var resultsGrid = this.FindControl<DataGrid>("ResultsGrid");

        if (scoreText == null || passText == null ||
            failText == null || resultsGrid == null || statusText == null)
            return;

        try
        {
            if (json.StartsWith("Error"))
            {
                statusText.Text = "🔴 API unreachable";
                return;
            }

            var doc = JsonDocument.Parse(json);

            statusText.Text = "🟢 Connected";

            // Timestamp
            if (timestampText != null &&
                doc.RootElement.TryGetProperty("timestamp", out var ts))
            {
                var rawTs = ts.GetString() ?? "";
                if (DateTime.TryParse(rawTs, out var parsed))
                    timestampText.Text = "Last checked: " +
                        parsed.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
                else
                    timestampText.Text = "Last checked: " + rawTs;
            }

            var data = doc.RootElement.GetProperty("data");
            var summary = data.GetProperty("summary");
            var results = data.GetProperty("results");

            int score = summary.GetProperty("score").GetInt32();
            int pass = summary.GetProperty("pass").GetInt32();
            int fail = summary.GetProperty("fail").GetInt32();

            if (scoreText != null)
                scoreText.Text = $"Score: {score}%";

            if (passText != null)
                passText.Text = $"Pass: {pass}";

            if (failText != null)
                failText.Text = $"Fail: {fail}";

            var rows = new List<ValidationRow>();

            foreach (var r in results.EnumerateArray())
            {
                rows.Add(new ValidationRow
                {
                    Rule = r.GetProperty("rule").GetString() ?? "",
                    Resource = r.GetProperty("resourceId").GetString() ?? "",
                    Status = r.GetProperty("status").GetString() ?? "",
                    Severity = r.GetProperty("severity").GetString() ?? "",
                    Message = r.GetProperty("message").GetString() ?? ""
                });
            }

            resultsGrid.ItemsSource = rows;
        }
        catch
        {
            statusText.Text = "🟡 Session expired or invalid response";
            scoreText.Text = "Validation error";
        }
    }

}
