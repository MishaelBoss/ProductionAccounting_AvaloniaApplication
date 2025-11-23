using Avalonia.Controls;
using Avalonia.Interactivity;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication;

public partial class ProfileViewUserControl : UserControl
{
    public ProfileViewUserControl()
    {
        InitializeComponent();
        DataContext = new ProfileViewUserControlViewModel();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e) 
    {
        if (DataContext is not ProfileViewUserControlViewModel viewModel) return;
        viewModel.TableUserContent = TableUserContent;
        viewModel.TasksUserContent = TasksUserContent;
    }

    public async void RefreshDataAsync(double userID) 
    {
        if (DataContext is not ProfileViewUserControlViewModel viewModel) return;
        viewModel.UserID = userID;
        viewModel.InitDateUserAsync();
    }
}