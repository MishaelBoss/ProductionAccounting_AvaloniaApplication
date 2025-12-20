using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.ViewModels;
using ProductionAccounting_AvaloniaApplication.Views;
using QuestPDF.Infrastructure;

namespace ProductionAccounting_AvaloniaApplication
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                QuestPDF.Settings.License = LicenseType.Community;
                InitializeFolderAndFile.Initialize();
                ManagerSettingsJson.InitializeSettings();
                ManagerCookie.IsUserLoggedIn();

                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

    }
}