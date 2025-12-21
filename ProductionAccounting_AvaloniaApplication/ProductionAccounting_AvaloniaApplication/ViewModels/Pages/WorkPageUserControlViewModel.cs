using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.Views.Control;
using System;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

public class WorkPageUserControlViewModel : ViewModelBase
{
    public Grid? WorkContent { get; set; }

    private readonly WorkUserControl _workUser = new();
    private readonly WorkMasterUserControl _workMaster = new();

    public static string Greeting 
        => $"Добро пожаловать, {ManagerCookie.GetFirstName} {ManagerCookie.GetLastName}!";

    public static DateTime CurrentDate 
        => DateTime.Now;

    public void ShowWork() 
    {
        if (ManagerCookie.IsUserLoggedIn() && ManagerCookie.IsEmployee) ShowWorkUserUserControl();
        if (ManagerCookie.IsUserLoggedIn() && (ManagerCookie.IsAdministrator || ManagerCookie.IsMaster)) ShowWorkMasterUserControl();
    }

    private void ShowWorkUserUserControl()
    {
        if (WorkContent == null) return;
        if (WorkContent.Children.Contains(_workUser)) return;

        if (WorkContent.Parent is Panel currentParent) currentParent.Children.Remove(_workUser);
        WorkContent.Children.Clear();
        WorkContent.Children.Add(_workUser);
    }

    private void ShowWorkMasterUserControl()
    {
        if (WorkContent == null) return;
        if (WorkContent.Children.Contains(_workMaster)) return;

        if (WorkContent.Parent is Panel currentParent) currentParent.Children.Remove(_workMaster);
        WorkContent.Children.Clear();
        WorkContent.Children.Add(_workMaster);
    }
}
