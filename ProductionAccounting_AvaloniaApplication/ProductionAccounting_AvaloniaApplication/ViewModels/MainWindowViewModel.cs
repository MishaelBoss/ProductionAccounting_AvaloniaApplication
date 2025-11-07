using ProductionAccounting_AvaloniaApplication.ViewModels.Control;
using ReactiveUI;

namespace ProductionAccounting_AvaloniaApplication.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public RightBoardUserControlViewModel RightBoardUserControlViewModel { get; }

        private ViewModelBase? _currentPage;
        public ViewModelBase? CurrentPage
        {
            get => _currentPage;
            set => this.RaiseAndSetIfChanged(ref _currentPage, value);
        }

        public MainWindowViewModel()
        {
            RightBoardUserControlViewModel = new RightBoardUserControlViewModel(this);
        }
    }
}
