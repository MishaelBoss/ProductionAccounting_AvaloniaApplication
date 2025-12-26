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
        IRecipient<OpenOrCloseOrderStatusMessage>, 
        IRecipient<OpenOrCloseAddSubProductStatusMessage>, 
        IRecipient<OpenOrCloseOrderViewStatusMessage>, 
        IRecipient<OpenOrCloseWorkTypeStatusMessage>
    {
        private readonly AddUsersUserControl _addUsers = new();
        private readonly AddOrderUserControl _addOrder = new();
        private readonly AddSubProductUserControl _addSubProduct = new();
        private readonly AddOrEditWorkTypeUserControl _addOrEditWorkTypeUserControl = new();

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

        public void Receive(OpenOrCloseOrderViewStatusMessage message)
        {
            if (message.ShouldOpen)
            {
                var productViewModel = new OrderViewUserControlViewModel(message.Name, message.ProductId, message.Mark, message.Coefficient, message.Notes, message.Status);

                _viewPageUserControlViewModel.ShowViewModel(productViewModel);

                CurrentPage = _viewPageUserControlViewModel;
            }
        }

        public void Receive(OpenOrCloseOrderStatusMessage message)
        {
            if (message.ShouldOpen) ShowOrderUsersUserControl(message.Id, message.Name, message.Counterparties, message.Description, message.TotalWeight, message.Coefficient);
            else CloseOrderUsersUserControl();
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

        public void Receive(OpenOrCloseWorkTypeStatusMessage message)
        {
            if (message.ShouldOpen) ShowAddOrEditWorkTypeUserControl(message.Id, message.Type, message.Coefficient, message.Measurement, message.Rate);
            else CloseAddOrEditWorkTypeUserControl();
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

        private void ShowOrderUsersUserControl(
            double? id = null,
            string? name = null,
            string? counterparties = null,
            string? description = null,
            decimal? totalWeight = null,
            decimal? coefficient = null)
        {
            if (ContentCenter == null) return;
            var userControl = new AddOrderUserControl { DataContext = new AddOrderUserControlViewModel(id, name, counterparties, description, totalWeight, coefficient) };

            if (ContentCenter.Children.Contains(userControl)) return;
            if (userControl.Parent is Panel currentParent) currentParent.Children.Remove(userControl);
            ContentCenter.Children.Clear();
            ContentCenter.Children.Add(userControl);
        }

        private void CloseOrderUsersUserControl()
        {
            if (ContentCenter == null) return;
            ContentCenter.Children.Clear();
            if (_addOrder.Parent == ContentCenter) ContentCenter.Children.Remove(_addOrder);
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

        private void ShowAddOrEditWorkTypeUserControl(double id, string type, decimal coefficient, string measurement, decimal rate)
        {
            if (ContentCenter == null) return;
            var userControl = new AddOrEditWorkTypeUserControl { DataContext = new AddOrEditWorkTypeUserControlViewModel(id, type, coefficient, measurement, rate) };

            if (ContentCenter.Children.Contains(userControl)) return;

            if (userControl.Parent is Panel currentParent) currentParent.Children.Remove(userControl);
            ContentCenter.Children.Clear();
            ContentCenter.Children.Add(userControl);
        }

        private void CloseAddOrEditWorkTypeUserControl()
        {
            if (ContentCenter == null) return;
            ContentCenter.Children.Clear();
            if (_addOrEditWorkTypeUserControl.Parent == ContentCenter) ContentCenter.Children.Remove(_addOrEditWorkTypeUserControl);
        }
    }
}
