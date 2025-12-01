using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;
using ProductionAccounting_AvaloniaApplication.Views.Control;
using ReactiveUI;
using System;

namespace ProductionAccounting_AvaloniaApplication.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IRecipient<OpenOrCloseAddUserStatusMessage>, IRecipient<OpenOrCloseStatusMessage>, IRecipient<OpenOrCloseAddDepartmentStatusMessage>, IRecipient<OpenOrCloseAddOperationStatusMessage>, IRecipient<OpenOrCloseAddProductStatusMessage>, IRecipient<OpenOrCloseAddPositionStatusMessage>, IRecipient<OpenOrCloseTaskDateilStatusMessage>, IRecipient<OpenOrCloseProductViewStatusMessage>
    {
        public event Action? LoginStatusChanged;

        private readonly AddUsersUserControl _addUsers = new();
        private readonly EditUsersUserControl _editUsers = new();
        private readonly AddDepartmentUserControl _addDepartment = new();
        private readonly AddPositionUserControl _addPosition = new();
        private readonly AddOperationUserControl _addOperation = new();
        private readonly AddProductUserControl _addProduct = new();
        private readonly TaskDetailUserControl _taskDetail = new();

        public Grid? ContentCenter = null;

        public RightBoardUserControlViewModel RightBoardUserControlViewModel { get; }
        private readonly AuthorizationUserControl _authorization = new();
        private readonly ProfileUserUserControl _profileUser = new();

        private readonly ViewPageUserControlViewModel viewPageUserControlViewModel = new();

        private ViewModelBase? _currentPage;
        public ViewModelBase? CurrentPage
        {
            get => _currentPage;
            set => this.RaiseAndSetIfChanged(ref _currentPage, value);
        }

        public MainWindowViewModel()
        {
            RightBoardUserControlViewModel = new RightBoardUserControlViewModel(this);

            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        public void Receive(OpenOrCloseProductViewStatusMessage message)
        {
            if (message.ShouldOpen)
            {
                var productViewModel = new ProductViewUserControlViewModel(message.ProductId);

                viewPageUserControlViewModel.ShowViewModel(productViewModel);

                CurrentPage = viewPageUserControlViewModel;
            }
            else
            {
                //CloseProfileUserControl();
            }
        }

        public void Receive(OpenOrCloseStatusMessage message)
        {
            if (message.ShouldOpen) ShowEditUsersUserControl(message.UserId);
            else CloseEditUsersUserControl();
        }

        public void Receive(OpenOrCloseAddDepartmentStatusMessage message)
        {
            if (message.ShouldOpen) ShowAddDepartmentUserControl();
            else CloseAddDepartmentUserControl();
        }

        public void Receive(OpenOrCloseAddOperationStatusMessage message)
        {
            if (message.ShouldOpen) ShowOperationUsersUserControl();
            else CloseOperationUsersUserControl();
        }

        public void Receive(OpenOrCloseAddProductStatusMessage message)
        {
            if (message.ShouldOpen) ShowProductUsersUserControl();
            else CloseProductUsersUserControl();
        }

        public void Receive(OpenOrCloseAddPositionStatusMessage message)
        {
            if (message.ShouldOpen) ShowAddPositionUserControl();
            else CloseAddPositionUserControl();
        }

        public void Receive(OpenOrCloseAddUserStatusMessage message)
        {
            if (message.ShouldOpen) ShowAddUsersUserControl();
            else CloseAddUsersUserControl();
        }

        public void Receive(OpenOrCloseTaskDateilStatusMessage message)
        {
            if (message.ShouldOpen) ShowTaskDetailUserControl(message.TaskId);
            else CloseTaskDetailUserControl();
        }

        public void NotifyLoginStatusChanged()
        {
            LoginStatusChanged?.Invoke();
        }

        public void ShowAuthorization()
        {
            if (ContentCenter != null)
            {
                if (ContentCenter.Children.Contains(_authorization)) {
                    _authorization.RefreshData();
                    return;
                }

                if (_authorization.Parent is Panel currentParent) currentParent.Children.Remove(_authorization);
                ContentCenter.Children.Clear();
                ContentCenter.Children.Add(_authorization);

                _authorization.RefreshData();
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

        public void ShowProfileUser()
        {
            if (ContentCenter != null)
            {
                if (ContentCenter.Children.Contains(_profileUser)) {
                    _ = _profileUser.RefreshDataAsync();
                    return;
                }

                if (_profileUser.Parent is Panel currentParent) currentParent.Children.Remove(_profileUser);
                ContentCenter.Children.Clear();
                ContentCenter.Children.Add(_profileUser);

                _ = _profileUser.RefreshDataAsync();
            }
        }

        public void CloseProfileUser()
        {
            if (ContentCenter != null)
            {
                ContentCenter.Children.Clear();
                if (_profileUser.Parent == ContentCenter) ContentCenter.Children.Remove(_profileUser);
            }
        }

        public void ShowAddUsersUserControl()
        {
            if (ContentCenter != null)
            {
                if (ContentCenter.Children.Contains(_addUsers))
                {
                    _ = _addUsers.RefreshDataAsync();
                    return;
                }

                if (_addUsers.Parent is Panel currentParent) currentParent.Children.Remove(_addUsers);
                ContentCenter.Children.Clear();
                ContentCenter.Children.Add(_addUsers);

                _ = _addUsers.RefreshDataAsync();
            }
        }

        public void CloseAddUsersUserControl()
        {
            if (ContentCenter != null)
            {
                ContentCenter.Children.Clear();
                if (_addUsers.Parent == ContentCenter) ContentCenter.Children.Remove(_addUsers);
            }
        }

        public void ShowAddDepartmentUserControl()
        {
            if (ContentCenter != null)
            {
                if (ContentCenter.Children.Contains(_addDepartment))
                {
                    _addDepartment.RefreshDataAsync();
                    return;
                }

                if (_addDepartment.Parent is Panel currentParent) currentParent.Children.Remove(_addDepartment);
                ContentCenter.Children.Clear();
                ContentCenter.Children.Add(_addDepartment);

                _addDepartment.RefreshDataAsync();
            }
        }

        public void CloseAddDepartmentUserControl()
        {
            if (ContentCenter != null)
            {
                ContentCenter.Children.Clear();
                if (_addDepartment.Parent == ContentCenter) ContentCenter.Children.Remove(_addDepartment);
            }
        }

        public void ShowAddPositionUserControl()
        {
            if (ContentCenter != null)
            {
                if (ContentCenter.Children.Contains(_addPosition))
                {
                    _addPosition.RefreshDataAsync();
                    return;
                }

                if (_addPosition.Parent is Panel currentParent) currentParent.Children.Remove(_addPosition);
                ContentCenter.Children.Clear();
                ContentCenter.Children.Add(_addPosition);

                _addPosition.RefreshDataAsync();
            }
        }

        public void CloseAddPositionUserControl()
        {
            if (ContentCenter != null)
            {
                ContentCenter.Children.Clear();
                if (_addPosition.Parent == ContentCenter) ContentCenter.Children.Remove(_addPosition);
            }
        }

        public void ShowEditUsersUserControl(double userID)
        {
            if (ContentCenter != null)
            {
                var editViewModel = new EditUsersUserControlViewModel(userID);
                _editUsers.DataContext = editViewModel;

                if (ContentCenter.Children.Contains(_editUsers)) 
                {
                    _ = _editUsers.RefreshDataAsync();
                    return;
                }

                if (_editUsers.Parent is Panel currentParent) currentParent.Children.Remove(_editUsers);
                ContentCenter.Children.Clear();
                ContentCenter.Children.Add(_editUsers);

                _ = _editUsers.RefreshDataAsync();
            }
        }

        public void CloseEditUsersUserControl()
        {
            if (ContentCenter != null)
            {
                ContentCenter.Children.Clear();
                if (_editUsers.Parent == ContentCenter) ContentCenter.Children.Remove(_editUsers);
            }
        }

        public void ShowOperationUsersUserControl()
        {
            if (ContentCenter != null)
            {
                if (ContentCenter.Children.Contains(_addOperation))
                {
                    _addOperation.RefreshData();
                    return;
                }

                if (_addOperation.Parent is Panel currentParent) currentParent.Children.Remove(_addOperation);
                ContentCenter.Children.Clear();
                ContentCenter.Children.Add(_addOperation);

                _addOperation.RefreshData();
            }
        }

        public void CloseOperationUsersUserControl()
        {
            if (ContentCenter != null)
            {
                ContentCenter.Children.Clear();
                if (_addOperation.Parent == ContentCenter) ContentCenter.Children.Remove(_addOperation);
            }
        }

        public void ShowProductUsersUserControl()
        {
            if (ContentCenter != null)
            {
                if (ContentCenter.Children.Contains(_addProduct))
                {
                    _addProduct.RefreshData();
                    return;
                }

                if (_addProduct.Parent is Panel currentParent) currentParent.Children.Remove(_addProduct);
                ContentCenter.Children.Clear();
                ContentCenter.Children.Add(_addProduct);

                _addProduct.RefreshData();
            }
        }

        public void CloseProductUsersUserControl()
        {
            if (ContentCenter != null)
            {
                ContentCenter.Children.Clear();
                if (_addProduct.Parent == ContentCenter) ContentCenter.Children.Remove(_addProduct);
            }
        }

        public void ShowTaskDetailUserControl(double _taskId)
        {
            if (ContentCenter != null)
            {
                var viewModel = new TaskDetailUserControlViewModel(_taskId);
                var userControl = new TaskDetailUserControl { DataContext = viewModel };

                if (ContentCenter.Children.Contains(userControl)) return;

                ContentCenter.Children.Clear();
                ContentCenter.Children.Add(userControl);

                //_addProduct.RefreshData();
            }
        }

        public void CloseTaskDetailUserControl()
        {
            if (ContentCenter != null)
            {
                ContentCenter.Children.Clear();
                if (_taskDetail.Parent == ContentCenter) ContentCenter.Children.Remove(_taskDetail);
            }
        }
    }
}
