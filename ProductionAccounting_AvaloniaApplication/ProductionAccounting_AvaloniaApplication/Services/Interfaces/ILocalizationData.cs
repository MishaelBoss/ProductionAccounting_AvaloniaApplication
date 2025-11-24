using Newtonsoft.Json;

namespace ProductionAccounting_AvaloniaApplication.Services.Interfaces;

public interface ILocalizationData
{
    [JsonProperty("AuthorizationUserControl")]
    public IAuthorizationUserControl AuthorizationUserControl { get; set; }
}

public interface IAuthorizationUserControl {
    [JsonProperty("TextBlock")]
    public ITextBlock TextBlock { get; set; }

    [JsonProperty("Button")]
    public IButton Button { get; set; }
}

public interface ICommon
{
}

public interface IButton
{
}

public interface ILabel
{
}

public interface ITextBox
{
}

public interface ITextBlock
{
}

public interface ICheckBox
{
}