using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;
using ReactiveUI;

namespace ProductionAccounting_AvaloniaApplication.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public Grid? ContentCenter = null;

        public RightBoardUserControlViewModel RightBoardUserControlViewModel { get; }
        private readonly AuthorizationUserControl _authorization = new();

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

        public void ShowAuthorization()
        {
            if (ContentCenter != null)
            {
                if (ContentCenter.Children.Contains(_authorization)) return;

                if (_authorization.Parent is Panel currentParent) currentParent.Children.Remove(_authorization);
                ContentCenter.Children.Clear();
                ContentCenter.Children.Add(_authorization);
            }
        }

        public void CloseAuthorization()
        {
            if (ContentCenter != null)
            {
                ContentCenter.Children.Clear();
                if (_authorization.Parent == ContentCenter) ContentCenter.Children.Remove(_authorization);
            }
        }
    }
}
