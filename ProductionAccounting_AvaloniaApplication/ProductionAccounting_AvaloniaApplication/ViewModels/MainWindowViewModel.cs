using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Templateds;
using ReactiveUI;

namespace ProductionAccounting_AvaloniaApplication.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public RightBoardTemplatedControlViewModel rightBoardTemplatedControlViewModel { get; }
        public Grid? CenterContent { get; set; } = null;

        private ViewModelBase? _currentPage;
        public ViewModelBase? CurrentPage
        {
            get => _currentPage;
            set => this.RaiseAndSetIfChanged(ref _currentPage, value);
        }

        public MainWindowViewModel()
        {
            rightBoardTemplatedControlViewModel = new RightBoardTemplatedControlViewModel(this);
        }
    }
}
