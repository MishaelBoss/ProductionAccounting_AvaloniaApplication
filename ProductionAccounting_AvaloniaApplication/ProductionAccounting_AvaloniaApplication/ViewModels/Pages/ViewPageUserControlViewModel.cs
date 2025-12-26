using JetBrains.Annotations;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

public class ViewPageUserControlViewModel : ViewModelBase
{
    private ViewModelBase? _currentInnerContent;
    [UsedImplicitly]
    public ViewModelBase? CurrentInnerContent
    {
        get => _currentInnerContent;
        private set => this.RaiseAndSetIfChanged(ref _currentInnerContent, value);
    }

    public void ShowViewModel(ViewModelBase? viewModel)
    {
        if (viewModel is null)
        {
            Loges.LoggingProcess(level: LogLevel.Warning, "Attempted to show a null view model");
            return;
        }

        if (ReferenceEquals(CurrentInnerContent, viewModel)) return;

        CurrentInnerContent = viewModel;
    }
}
