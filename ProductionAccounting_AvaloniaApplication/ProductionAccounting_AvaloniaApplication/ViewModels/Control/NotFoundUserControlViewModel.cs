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
        Error = level switch
        {
            ErrorLevel.NoConnectToDB => "Не подключен к базе данных",
            ErrorLevel.NotFound => "Ничего не найдено",
            _ => "Неизвестная ошибка",
        };
    }
}
