using System.Diagnostics;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

namespace ProductionAccounting_AvaloniaApplication.Views.Pages;

public partial class SettingsPageUserControlView : UserControl
{
    public SettingsPageUserControlView()
    {
        InitializeComponent();
        DataContext = new SettingsPageUserControlViewModel();
    }

    private void OpenFileSetting_Click(object? sender, RoutedEventArgs e)
    {
        var ConfigFile = Path.Combine(Paths.Settings, Files.ConfigSettingsFileName);

        if (!File.Exists(ConfigFile)) return;
        Process.Start(new ProcessStartInfo { FileName = ConfigFile, UseShellExecute = true });
    }

    private void OpenFolderLoges_Click(object? sender, RoutedEventArgs e)
    {
        if (!Path.Exists(Paths.Log)) return;
        Process.Start(new ProcessStartInfo { FileName = Paths.Log, UseShellExecute = true });
    }

    private void CheckUpdate(object? sender, RoutedEventArgs e)
    {
        //_ = ManagerUpdate.CheckUpdate();
    }

    private void OpenFolderLanguage_Click(object? sender, RoutedEventArgs e)
    {
        if (!Path.Exists(Paths.Localization)) Directory.CreateDirectory(Paths.Localization);
        Process.Start(new ProcessStartInfo { FileName = Paths.Localization, UseShellExecute = true });
    }
}