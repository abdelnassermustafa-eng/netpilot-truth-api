using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using TruthDoctor.Services;
using System.Text.Json;

namespace TruthDoctor;

public partial class MainWindow : Window
{
    private readonly ApiService _apiService = new();

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object? sender, RoutedEventArgs e)
    {
        var usernameBox = this.FindControl<TextBox>("UsernameBox");
        var passwordBox = this.FindControl<TextBox>("PasswordBox");
        var statusText = this.FindControl<TextBlock>("StatusText");

        if (usernameBox == null || passwordBox == null || statusText == null)
            return;

        var username = usernameBox.Text ?? "";
        var password = passwordBox.Text ?? "";

        statusText.Text = "Logging in...";

        var success = await _apiService.LoginAsync(username, password);

        if (!success)
        {
            statusText.Text = "Login failed";
            return;
        }

        var validation = await _apiService.GetValidationAsync();
        Console.WriteLine("RAW VALIDATION JSON:");
        Console.WriteLine(validation);


        // Pretty-format JSON
        string formattedJson = validation;

        try
        {
            var jsonDoc = JsonDocument.Parse(validation);
            formattedJson = JsonSerializer.Serialize(
                jsonDoc,
                new JsonSerializerOptions { WriteIndented = true }
            );
        }
        catch
        {
            // Keep raw text if parsing fails
        }

        statusText.Text = "Login successful";

        // Open validation window
        var validationWindow = new ValidationWindow(formattedJson);
        await validationWindow.ShowDialog(this);
    }
}
