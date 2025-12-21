using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;
using ProductionAccounting_AvaloniaApplication.Views.Control;
using ReactiveUI;

namespace ProductionAccounting_AvaloniaApplication.ViewModels
{
    public class MainWindowViewModel : ViewModelBase,
        IRecipient<OpenOrCloseUserStatusMessage>, 
        IRecipient<OpenOrCloseAddDepartmentStatusMessage>, 
        IRecipient<OpenOrCloseSubOperationStatusMessage>,
        IRecipient<OpenOrCloseProductStatusMessage>, 
        IRecipient<OpenOrCloseAddPositionStatusMessage>, 
        IRecipient<OpenOrCloseAddSubProductStatusMessage>, 
        IRecipient<OpenOrCloseProductViewStatusMessage>, 
        IRecipient<OpenOrCloseEmployeeAssignmentMasterSubMarkStatusMessage>,
        IRecipient<OpenOrCloseCompleteWorkFormStatusMessage>
    {
        private readonly AddUsersUserControl _addUsers = new();
        private readonly AddDepartmentUserControl _addDepartment = new();
        private readonly AddPositionUserControl _addPosition = new();
        private readonly AddOperationUserControl _addOperation = new();
        private readonly AddProductUserControl _addProduct = new();
        private readonly AddSubProductUserControl _addSubProduct = new();
        private readonly EmployeeAssignmentMasterSubMarkUserControl _employeeAssignmentMasterSubMarkUserControl = new();
        private readonly CompleteWorkFormUserControl _completeWorkFormUserControl = new();

        public Grid? ContentCenter = null;

        public RightBoardUserControlViewModel RightBoardUserControlViewModel { get; }

        private readonly ViewPageUserControlViewModel _viewPageUserControlViewModel = new();

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
                var productViewModel = new ProductViewUserControlViewModel(message.Name, message.ProductId, message.Mark, message.Coefficient, message.Notes, message.Status);

                _viewPageUserControlViewModel.ShowViewModel(productViewModel);

                CurrentPage = _viewPageUserControlViewModel;
            }
        }

        public void Receive(OpenOrCloseEmployeeAssignmentMasterSubMarkStatusMessage message)
        {
            if (message.ShouldOpen) ShowEmployeeAssignmentMasterSubMarkUserControl(message.ProductId, message.SubProductId);
            else CloseEmployeeAssignmentMasterSubMarkUserControl();
        }

        public void Receive(OpenOrCloseCompleteWorkFormStatusMessage message)
        {
            if (message.ShouldOpen) ShowCompleteWorkFormUserControl(message.TaskName, message.PlannedQuantity, message.ProductId, message.OperationId, message.SubProductOperationId, message.UserId);
            else CloseCompleteWorkFormUserControl();
        }

        public void Receive(OpenOrCloseAddDepartmentStatusMessage message)
        {
            if (message.ShouldOpen) ShowAddDepartmentUserControl();
            else CloseAddDepartmentUserControl();
        }

        public void Receive(OpenOrCloseSubOperationStatusMessage message)
        {
            if (message.ShouldOpen) ShowOperationUsersUserControl(message.SubProductId, message.SubProductOperationId, message.OperationName, message.OperationCode, message.OperationPrice, message.OperationTime, message.OperationDescription, message.OperationQuantity);
            else CloseOperationUsersUserControl();
        }

        public void Receive(OpenOrCloseProductStatusMessage message)
        {
            if (message.ShouldOpen) ShowProductUsersUserControl(message.Id, message.ProductName, message.ProductArticle, message.ProductDescription, message.ProductPricePerUnit, message.ProductPricePerKg, message.ProductMark, message.ProductCoefficient);
            else CloseProductUsersUserControl();
        }

        public void Receive(OpenOrCloseAddPositionStatusMessage message)
        {
            if (message.ShouldOpen) ShowAddPositionUserControl();
            else CloseAddPositionUserControl();
        }

        public void Receive(OpenOrCloseUserStatusMessage message)
        {
            if (message.ShouldOpen) ShowAddUsersUserControl(message.Id, message.Login, message.FirstUsername, message.LastUsername, message.MiddleName, message.BaseSalary, message.Email, message.Phone);
            else CloseAddUsersUserControl();
        }

        public void Receive(OpenOrCloseAddSubProductStatusMessage message)
        {
            if (message.ShouldOpen) ShowAddSubProductUserControl(message.TaskId);
            else CloseAddSubProductUserControl();
        }

        private void ShowAddUsersUserControl(double? id = null, string? login = null, string? firstName = null, string? lastName = null, string? middleName = null, decimal? baseSalary = null, string? email = null, string? phone = null)
        {
            if (ContentCenter == null) return;
            var userControl = new AddUsersUserControl { DataContext = new AddUsersUserControlViewModel(id, login, firstName, lastName, middleName, baseSalary, email, phone) };

            if (ContentCenter.Children.Contains(userControl))
            {
                return;
            }

            if (userControl.Parent is Panel currentParent) currentParent.Children.Remove(userControl);
            ContentCenter.Children.Clear();
            ContentCenter.Children.Add(userControl);
        }

        private void CloseAddUsersUserControl()
        {
            if (ContentCenter == null) return;
            ContentCenter.Children.Clear();
            if (_addUsers.Parent == ContentCenter) ContentCenter.Children.Remove(_addUsers);
        }

        private void ShowAddDepartmentUserControl()
        {
            if (ContentCenter == null) return;
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

        private void CloseAddDepartmentUserControl()
        {
            if (ContentCenter == null) return;
            ContentCenter.Children.Clear();
            if (_addDepartment.Parent == ContentCenter) ContentCenter.Children.Remove(_addDepartment);
        }

        private void ShowAddPositionUserControl()
        {
            if (ContentCenter == null) return;
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

        private void CloseAddPositionUserControl()
        {
            if (ContentCenter == null) return;
            ContentCenter.Children.Clear();
            if (_addPosition.Parent == ContentCenter) ContentCenter.Children.Remove(_addPosition);
        }

        private void ShowOperationUsersUserControl(double subProductId, double subProductOperationId, string operationName, string operationCode, decimal operationPrice, decimal operationTime, string operationDescription, decimal operationQuantity)
        {
            if (ContentCenter == null) return;
            var userControl = new AddOperationUserControl() {DataContext = new AddOperationUserControlViewModel(subProductId, subProductOperationId, operationName, operationCode, operationPrice, operationTime, operationDescription, operationQuantity) };

            if (ContentCenter.Children.Contains(userControl)) return;

            if (userControl.Parent is Panel currentParent) currentParent.Children.Remove(userControl);
            ContentCenter.Children.Clear();
            ContentCenter.Children.Add(userControl);
        }

        private void CloseOperationUsersUserControl()
        {
            if (ContentCenter == null) return;
            ContentCenter.Children.Clear();
            if (_addOperation.Parent == ContentCenter) ContentCenter.Children.Remove(_addOperation);
        }

        private void ShowProductUsersUserControl(
            double? id = null,
            string? productName = null,
            string? productArticle = null,
            string? productDescription = null,
            decimal? productPricePerUnit = 0,
            decimal? productPricePerKg = null,
            string? productMark = null,
            decimal? productCoefficient = null)
        {
            if (ContentCenter == null) return;
            var userControl = new AddProductUserControl { DataContext = new AddProductUserControlViewModel(id, productName, productArticle, productDescription, productPricePerUnit, productPricePerKg, productMark, productCoefficient) };

            if (ContentCenter.Children.Contains(userControl)) return;
            if (userControl.Parent is Panel currentParent) currentParent.Children.Remove(userControl);
            ContentCenter.Children.Clear();
            ContentCenter.Children.Add(userControl);
        }

        private void CloseProductUsersUserControl()
        {
            if (ContentCenter == null) return;
            ContentCenter.Children.Clear();
            if (_addProduct.Parent == ContentCenter) ContentCenter.Children.Remove(_addProduct);
        }

        private void ShowAddSubProductUserControl(double taskId)
        {
            if (ContentCenter == null) return;
            var userControl = new AddSubProductUserControl { DataContext = new AddSubProductUserControlViewModel(taskId) };

            if (ContentCenter.Children.Contains(userControl)) return;

            ContentCenter.Children.Clear();
            ContentCenter.Children.Add(userControl);
        }

        private void CloseAddSubProductUserControl()
        {
            if (ContentCenter == null) return;
            ContentCenter.Children.Clear();
            if (_addSubProduct.Parent == ContentCenter) ContentCenter.Children.Remove(_addSubProduct);
        }

        private void ShowEmployeeAssignmentMasterSubMarkUserControl(double productId, double subProductId)
        {
            if (ContentCenter == null) return;
            var userControl = new EmployeeAssignmentMasterSubMarkUserControl { DataContext = new EmployeeAssignmentMasterSubMarkUserControlViewModel(productId, subProductId) };

            if (ContentCenter.Children.Contains(userControl))
            {
                return;
            }

            if (userControl.Parent is Panel currentParent) currentParent.Children.Remove(userControl);
            ContentCenter.Children.Clear();
            ContentCenter.Children.Add(userControl);
        }

        private void CloseEmployeeAssignmentMasterSubMarkUserControl()
        {
            if (ContentCenter == null) return;
            ContentCenter.Children.Clear();
            if (_employeeAssignmentMasterSubMarkUserControl.Parent == ContentCenter) ContentCenter.Children.Remove(_employeeAssignmentMasterSubMarkUserControl);
        }

        private void ShowCompleteWorkFormUserControl(string taskName, decimal assignedQuantity, double productId, double operationId, double subProductOperationId, double userId)
        {
            if (ContentCenter == null) return;
            var userControl = new CompleteWorkFormUserControl { DataContext = new CompleteWorkFormUserControlViewModel(taskName, assignedQuantity, productId, operationId, subProductOperationId, userId) };

            if (ContentCenter.Children.Contains(userControl)) return;

            if (userControl.Parent is Panel currentParent) currentParent.Children.Remove(userControl);
            ContentCenter.Children.Clear();
            ContentCenter.Children.Add(userControl);
        }

        private void CloseCompleteWorkFormUserControl()
        {
            if (ContentCenter == null) return;
            ContentCenter.Children.Clear();
            if (_completeWorkFormUserControl.Parent == ContentCenter) ContentCenter.Children.Remove(_completeWorkFormUserControl);
        }
    }
}
