using ProductionAccounting_AvaloniaApplication.Scripts;
using System;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

public class WorkPageUserControlViewModel : ViewModelBase
{
    public static string Greeting 
        => $"Добро пожаловать, {ManagerCookie.GetFirstName} {ManagerCookie.GetLastName}!";

    public static DateTime CurrentDate 
        => DateTime.Now;
}
