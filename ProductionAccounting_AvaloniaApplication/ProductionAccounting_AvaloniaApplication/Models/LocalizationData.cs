using ProductionAccounting_AvaloniaApplication.Services.Interfaces;

namespace ProductionAccounting_AvaloniaApplication.Models;

public class LocalizationData(AuthorizationUserControl _authorizationUserControl) : ILocalizationData
{
    public IAuthorizationUserControl AuthorizationUserControl { get; set; } = _authorizationUserControl;
}

public class AuthorizationUserControl(TextBlock _textBlock, Button _button) : IAuthorizationUserControl
{
    public ITextBlock TextBlock { get; set; } = _textBlock;
    public IButton Button { get; set; } = _button;
}

public class Common() : ICommon
{
}

public class Button() : IButton
{
}

public class Label() : ILabel
{
}

public class TextBox() : ITextBox
{
}

public class TextBlock() : ITextBlock
{
}

public class CheckBox() : ICheckBox
{
}