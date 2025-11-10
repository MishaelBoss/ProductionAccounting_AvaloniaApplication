using ReactiveUI;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class NotFoundUserControlViewModel : ViewModelBase
{
private string _error = string.Empty;
    public string Error {
        get => _error;
        set => this.RaiseAndSetIfChanged(ref _error, value);
    }

    public enum ErrorLevel
    {
        NoConnectToDB,
        NotFound
    }

    public NotFoundUserControlViewModel(ErrorLevel level)
    {
        switch (level)
        {
            case ErrorLevel.NoConnectToDB:
                Error = "Не подключен к базе данных";
                break;
            case ErrorLevel.NotFound:
                Error = "Ничего не найдно";
                break;
            default:
                Error = "Неизвестная ошибка";
                break;
        }
    }
}
