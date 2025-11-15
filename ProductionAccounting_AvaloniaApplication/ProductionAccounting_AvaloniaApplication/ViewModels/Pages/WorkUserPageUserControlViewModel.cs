using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.Views.Control;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

public class WorkUserPageUserControlViewModel : ViewModelBase, INotifyPropertyChanged
{
    public Grid? ContentCenter = null;

    private readonly AddWorkUserControl _addWork = new();

    public WorkUserPageUserControlViewModel() 
    {
        if (!ManagerCookie.IsUserLoggedIn() && ManagerCookie.CurrentUserTypeId == 0) return;
        EmployeeName = $"{ManagerCookie.GetFirstName} {ManagerCookie.GetLastName}";
    }

    public ICommand AddWorkCommand
        => new RelayCommand(() => ShowAddWorkUserControl());

    public ICommand ViewWorkHistoryCommand
        => new RelayCommand(() => MessageBoxManager.GetMessageBoxStandard("", "").ShowWindowAsync());

    public ICommand OpenReportsCommand
        => new RelayCommand(() => MessageBoxManager.GetMessageBoxStandard("", "").ShowWindowAsync());

    public string Greeting => $"Добро пожаловать, {EmployeeName}!";

    public string EmployeeName { get; set; } = string.Empty;

    public DateTime CurrentDate => DateTime.Now;

    public void ShowAddWorkUserControl()
    {
        if (ContentCenter != null)
        {
            if (ContentCenter.Children.Contains(_addWork))
            {
                _ = _addWork.RefreshDataAsync();
                return;
            }

            if (_addWork.Parent is Panel currentParent) currentParent.Children.Remove(_addWork);
            ContentCenter.Children.Clear();
            ContentCenter.Children.Add(_addWork);

            _ = _addWork.RefreshDataAsync();
        }
    }

    public void CloseAddWorkUserControl()
    {
        if (ContentCenter != null)
        {
            ContentCenter.Children.Clear();
            if (_addWork.Parent == ContentCenter) ContentCenter.Children.Remove(_addWork);
        }
    }
}
