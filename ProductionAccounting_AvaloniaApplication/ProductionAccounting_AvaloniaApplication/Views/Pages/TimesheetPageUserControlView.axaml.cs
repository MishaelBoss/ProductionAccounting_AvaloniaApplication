using System.Linq;
using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

namespace ProductionAccounting_AvaloniaApplication.Views.Pages;

public partial class TimesheetPageUserControlView : UserControl
{
    public TimesheetPageUserControlView()
    {
        InitializeComponent();
        DataContext = new TimesheetPageUserControlViewModel();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e) 
    {
        if (DataContext is not TimesheetPageUserControlViewModel viewModel) return;
        viewModel.CartTimesheet = CartTimesheet;
        viewModel.LoadDataForRange(DateTime.Today, DateTime.Today);
    }

    private void CalendarDateTimesheetList_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
    {        
        if (DataContext is not TimesheetPageUserControlViewModel viewModel) return;
        if (CalendarDateTimesheetList?.SelectedDates.Count == 1) viewModel.LoadDataForRange(CalendarDateTimesheetList.SelectedDates[0], CalendarDateTimesheetList.SelectedDates[0]);
        else if (CalendarDateTimesheetList?.SelectedDates.Count > 1) viewModel.LoadDataForRange(CalendarDateTimesheetList.SelectedDates.Min(), CalendarDateTimesheetList.SelectedDates.Max());
    }
}