using ReactiveUI;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

public class ViewPageUserControlViewModel : ViewModelBase
{
    private ViewModelBase? _currentInnerContent = null;
    public ViewModelBase? CurrentInnerContent
    {
        get => _currentInnerContent;
        set => this.RaiseAndSetIfChanged(ref _currentInnerContent, value);
    }

    public void ShowViewModel(ViewModelBase viewModel)
    {
        CurrentInnerContent = viewModel;
    }
}
