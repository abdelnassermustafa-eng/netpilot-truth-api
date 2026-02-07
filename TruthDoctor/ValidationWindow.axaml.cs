using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace TruthDoctor;

public partial class ValidationWindow : Window
{
    public ValidationWindow(string json)
    {
        InitializeComponent();
        LoadValidation(json);
    }

    private void LoadValidation(string json)
    {
        var scoreText = this.FindControl<TextBlock>("ScoreText");
        var passText = this.FindControl<TextBlock>("PassText");
        var failText = this.FindControl<TextBlock>("FailText");
        var timestampText = this.FindControl<TextBlock>("TimestampText");
        var resultsGrid = this.FindControl<DataGrid>("ResultsGrid");

        if (scoreText == null || passText == null || failText == null || resultsGrid == null)
            return;

        try
        {
            var doc = JsonDocument.Parse(json);

            // Timestamp (top-level)
            if (timestampText != null && doc.RootElement.TryGetProperty("timestamp", out var ts))
            {
                var rawTs = ts.GetString() ?? "";
                if (DateTime.TryParse(rawTs, out var parsed))
                    timestampText.Text = "Last checked: " + parsed.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
                else
                    timestampText.Text = "Last checked: " + rawTs;
            }

            var data = doc.RootElement.GetProperty("data");
            var summary = data.GetProperty("summary");
            var results = data.GetProperty("results");

            int score = summary.GetProperty("score").GetInt32();
            int pass = summary.GetProperty("pass").GetInt32();
            int fail = summary.GetProperty("fail").GetInt32();

            scoreText.Text = $"Score: {score}%";
            passText.Text = $"Pass: {pass}";
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
        catch (Exception ex)
        {
            scoreText.Text = "Failed to parse validation data: " + ex.Message;
        }
    }
}
