using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;
using ProductionAccounting_AvaloniaApplication.Views.Control;
using System.ComponentModel;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

public class AdminPageUserControlViewModel : ViewModelBase, INotifyPropertyChanged, IRecipient<OpenEditStatusMessage>
{
    public AdminPageUserControlViewModel()
    {
        WeakReferenceMessenger.Default.Register(this);
    }
    
    public void Receive(OpenEditStatusMessage message)
    {
        if (message.ShouldOpen) ShowEditUsersUserControl(message.UserId);
        else CloseEditUsersUserControl();
    }

    public Grid? Content { get; set; } = null;

    private readonly AddUsersUserControl _addUsers = new();
    private readonly EditUsersUserControl _editUsers = new();
    private readonly AddDepartmentUserControl _addDepartment = new();
    private readonly AddPositionUserControl _addPosition = new();
    private readonly AddOperationUserControl _addOperation = new();
    private readonly AddProductUserControl _addProduct = new();

    public ICommand OpenAddUsers
        => new RelayCommand(() => ShowAddUsersUserControl());

    public ICommand OpenDepartmentCommand
        => new RelayCommand(() => ShowAddDepartmentUserControl());

    public ICommand OpenPositionCommand
        => new RelayCommand(() => ShowAddPositionUserControl());

    public ICommand AddOperationCommand
        => new RelayCommand(() => ShowOperationUsersUserControl());

    public ICommand AddProductCommand
        => new RelayCommand(() => ShowProductUsersUserControl());

    public void ShowAddUsersUserControl()
    {
        if (Content != null)
        {
            if (Content.Children.Contains(_addUsers))
            {
                _ = _addUsers.RefreshDataAsync();
                return;
            }

            if (_addUsers.Parent is Panel currentParent) currentParent.Children.Remove(_addUsers);
            Content.Children.Clear();
            Content.Children.Add(_addUsers);

            _ = _addUsers.RefreshDataAsync();
        }
    }

    public void CloseAddUsersUserControl()
    {
        if (Content != null)
        {
            Content.Children.Clear();
            if (_addUsers.Parent == Content) Content.Children.Remove(_addUsers);
        }
    }

    public void ShowAddDepartmentUserControl()
    {
        if (Content != null)
        {
            if (Content.Children.Contains(_addDepartment))
            {
                _addDepartment.RefreshDataAsync();
                return;
            }

            if (_addDepartment.Parent is Panel currentParent) currentParent.Children.Remove(_addDepartment);
            Content.Children.Clear();
            Content.Children.Add(_addDepartment);

            _addDepartment.RefreshDataAsync();
        }
    }

    public void CloseAddDepartmentUserControl()
    {
        if (Content != null)
        {
            Content.Children.Clear();
            if (_addDepartment.Parent == Content) Content.Children.Remove(_addDepartment);
        }
    }

    public void ShowAddPositionUserControl()
    {
        if (Content != null)
        {
            if (Content.Children.Contains(_addPosition))
            {
                _addPosition.RefreshDataAsync();
                return;
            }

            if (_addPosition.Parent is Panel currentParent) currentParent.Children.Remove(_addPosition);
            Content.Children.Clear();
            Content.Children.Add(_addPosition);

            _addPosition.RefreshDataAsync();
        }
    }

    public void CloseAddPositionUserControl()
    {
        if (Content != null)
        {
            Content.Children.Clear();
            if (_addPosition.Parent == Content) Content.Children.Remove(_addPosition);
        }
    }

    public void ShowEditUsersUserControl(double userID)
    {
        if (Content != null)
        {
            var editViewModel = new EditUsersUserControlViewModel(userID);
            _editUsers.DataContext = editViewModel;

            if (Content.Children.Contains(_editUsers)) 
            {
                _ = _editUsers.RefreshDataAsync();
                return;
            }

            if (_editUsers.Parent is Panel currentParent) currentParent.Children.Remove(_editUsers);
            Content.Children.Clear();
            Content.Children.Add(_editUsers);

            _ = _editUsers.RefreshDataAsync();
        }
    }

    public void CloseEditUsersUserControl()
    {
        if (Content != null)
        {
            Content.Children.Clear();
            if (_editUsers.Parent == Content) Content.Children.Remove(_editUsers);
        }
    }

    public void ShowOperationUsersUserControl()
    {
        if (Content != null)
        {
            if (Content.Children.Contains(_addOperation))
            {
                _ = _addOperation.RefreshDataAsync();
                return;
            }

            if (_addOperation.Parent is Panel currentParent) currentParent.Children.Remove(_addOperation);
            Content.Children.Clear();
            Content.Children.Add(_addOperation);

            _ = _addOperation.RefreshDataAsync();
        }
    }

    public void CloseOperationUsersUserControl()
    {
        if (Content != null)
        {
            Content.Children.Clear();
            if (_addOperation.Parent == Content) Content.Children.Remove(_addOperation);
        }
    }

    public void ShowProductUsersUserControl()
    {
        if (Content != null)
        {
            if (Content.Children.Contains(_addProduct))
            {
                _ = _addProduct.RefreshDataAsync();
                return;
            }

            if (_addProduct.Parent is Panel currentParent) currentParent.Children.Remove(_addProduct);
            Content.Children.Clear();
            Content.Children.Add(_addProduct);

            _ = _addProduct.RefreshDataAsync();
        }
    }

    public void CloseProductUsersUserControl()
    {
        if (Content != null)
        {
            Content.Children.Clear();
            if (_addProduct.Parent == Content) Content.Children.Remove(_addProduct);
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}