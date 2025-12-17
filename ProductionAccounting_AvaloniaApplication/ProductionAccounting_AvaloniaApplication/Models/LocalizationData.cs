using Newtonsoft.Json;

namespace ProductionAccounting_AvaloniaApplication.Models;

internal class LocalizationData
{
    [JsonProperty("AuthorizationUserControl")]
    public AuthorizationUserControl AuthorizationUserControl { get; set; }
    
    [JsonProperty("RightBoardUserControl")]
    public RightBoardUserControl RightBoardUserControl { get; set; }
}

internal class AuthorizationUserControl
{
    [JsonProperty("TextBlock")]
    public TextBlock TextBlock { get; set; }

    [JsonProperty("Button")]
    public Button Button { get; set; }
}

internal class RightBoardUserControl
{
    [JsonProperty("Button")]
    public RightBoardButton Button { get; set; }
    
    [JsonProperty("TextBlock")]
    public TextBlock TextBlock { get; set; }
}

internal class RightBoardButton
{
    [JsonProperty("AdminPanel")]
    public string AdminPanel { get; set; }
    
    [JsonProperty("Manager")]
    public string Manager { get; set; }
    
    [JsonProperty("Timesheet")]
    public string Timesheet { get; set; }
    
    [JsonProperty("Windows user")]
    public string WindowsUser { get; set; }
    
    [JsonProperty("Shipments")]
    public string Shipments { get; set; }
    
    [JsonProperty("Salary")]
    public string Salary { get; set; }

    [JsonProperty("Login")]
    public string Login { get; set; }

    [JsonProperty("Profile")]
    public string Profile { get; set; }

    [JsonProperty("Settings")]
    public string Settings { get; set; }
}

internal class Button
{
    [JsonProperty("Login")]
    public string Login { get; set; }
}

internal class TextBlock
{
    [JsonProperty("Login")]
    public string Login { get; set; }

    [JsonProperty("Name")]
    public string Name { get; set; }

    [JsonProperty("Password")]
    public string Password { get; set; }

    [JsonProperty("EnterYourName")]
    public string EnterYourName { get; set; }

    [JsonProperty("EnterYourPassword")]
    public string EnterYourPassword { get; set; }

    [JsonProperty("MainMenu")]
    public string MainMenu { get; set; }
}