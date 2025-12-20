using Newtonsoft.Json;

namespace ProductionAccounting_AvaloniaApplication.Models;

internal class LocalizationData
{
    [JsonProperty("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("AuthorizationUserControl")]
    public AuthorizationUserControl? AuthorizationUserControl { get; set; }
    
    [JsonProperty("RightBoardUserControl")]
    public RightBoardUserControl? RightBoardUserControl { get; set; }

    [JsonProperty("ProfilePageUserControlView")]
    public ProfilePageUserControlView? ProfilePageUserControlView { get; set; }

    [JsonProperty("SettingsPageUserControlView")]
    public SettingsPageUserControlView? SettingsPageUserControlView { get; set; }
}

internal class AuthorizationUserControl
{
    [JsonProperty("TextBlock")]
    public TextBlock? TextBlock { get; set; }

    [JsonProperty("Button")]
    public Button? Button { get; set; }
}

internal class RightBoardUserControl
{
    [JsonProperty("Button")]
    public RightBoardButton? Button { get; set; }
    
    [JsonProperty("TextBlock")]
    public TextBlock? TextBlock { get; set; }
}

internal class ProfilePageUserControlView
{
    [JsonProperty("Button")]
    public Button? Button { get; set; }
    
    [JsonProperty("TabItem")]
    public TabItem? TabItem { get; set; }
}

internal class SettingsPageUserControlView
{
    [JsonProperty("Button")]
    public SettingsButton? Button { get; set; }

    [JsonProperty("TextBlock")]
    public SettingsTextBlock? TextBlock { get; set; }
}

internal class RightBoardButton
{
    [JsonProperty("AdminPanel")]
    public string AdminPanel { get; set; } = string.Empty;

    [JsonProperty("Manager")]
    public string Manager { get; set; } = string.Empty;

    [JsonProperty("ReferenceBooks")]
    public string ReferenceBooks { get; set; } = string.Empty;

    [JsonProperty("Timesheet")]
    public string Timesheet { get; set; } = string.Empty;

    [JsonProperty("WindowsUser")]
    public string WindowsUser { get; set; } = string.Empty;

    [JsonProperty("Shipments")]
    public string Shipments { get; set; } = string.Empty;

    [JsonProperty("Salary")]
    public string Salary { get; set; } = string.Empty;

    [JsonProperty("Login")]
    public string Login { get; set; } = string.Empty;

    [JsonProperty("Profile")]
    public string Profile { get; set; } = string.Empty;

    [JsonProperty("Settings")]
    public string Settings { get; set; } = string.Empty;
}

internal class Button
{
    [JsonProperty("Login")]
    public string Login { get; set; } = string.Empty;

    [JsonProperty("Logout")]
    public string Logout { get; set; } = string.Empty;
}

internal class SettingsButton 
{
    [JsonProperty("TestConnection")]
    public string TestConnection { get; set; } = string.Empty;

    [JsonProperty("Restart")]
    public string Restart { get; set; } = string.Empty;
}

internal class SettingsTextBlock
{
    [JsonProperty("FileSettings")]
    public string FileSettings { get; set; } = string.Empty;

    [JsonProperty("Logs")]
    public string Logs { get; set; } = string.Empty;

    [JsonProperty("Language")]
    public string Language { get; set; } = string.Empty;

    [JsonProperty("Updates")]
    public string Updates { get; set; } = string.Empty;

    [JsonProperty("CheckForApplicationUpdates")]
    public string CheckForApplicationUpdates { get; set; } = string.Empty;

    [JsonProperty("DatabaseConnection")]
    public string DatabaseConnection { get; set; } = string.Empty;
}

internal class TextBlock
{
    [JsonProperty("Login")]
    public string Login { get; set; } = string.Empty;

    [JsonProperty("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("Password")]
    public string Password { get; set; } = string.Empty;

    [JsonProperty("EnterYourName")]
    public string EnterYourName { get; set; } = string.Empty;

    [JsonProperty("EnterYourPassword")]
    public string EnterYourPassword { get; set; } = string.Empty;

    [JsonProperty("MainMenu")]
    public string MainMenu { get; set; } = string.Empty;
}

internal class TabItem
{
    [JsonProperty("Tables")]
    public string Tables { get; set; } = string.Empty;

    [JsonProperty("ProductionHistory")]
    public string ProductionHistory { get; set; } = string.Empty;
}