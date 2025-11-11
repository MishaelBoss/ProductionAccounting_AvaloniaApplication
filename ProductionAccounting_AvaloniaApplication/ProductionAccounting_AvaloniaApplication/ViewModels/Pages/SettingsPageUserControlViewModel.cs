using System.IO;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

public class SettingsPageUserControlViewModel : ViewModelBase
{
    private string _pathToSettingsFile = Path.GetFullPath(Path.Combine(Paths.Settings, Files.ConfigSettingsFileName));
    public string PathToSettingsFile
    {
        get => _pathToSettingsFile;
        set => this.RaiseAndSetIfChanged(ref _pathToSettingsFile, value);
    }

    // private ObservableCollection<ConfigLocalization> _comboBoxItems = [];
    // public ObservableCollection<ConfigLocalization> ComboBoxItems
    // {
    //     get => _comboBoxItems;
    //     set
    //     {
    //         if (_comboBoxItems != value)
    //         {
    //             _comboBoxItems = value;
    //             OnPropertyChanged(nameof(ComboBoxItems));
    //         }
    //     }
    // }

    // private ConfigLocalization? _selectedComboBoxItem;
    // public ConfigLocalization? SelectedComboBoxItem
    // {
    //     get => _selectedComboBoxItem;
    //     set
    //     {
    //         if (_selectedComboBoxItem != value)
    //         {
    //             _selectedComboBoxItem = value;
    //             OnPropertyChanged(nameof(SelectedComboBoxItem));
    //             SelectLanguage();
    //         }
    //     }
    // }

    // public SettingsPageViewModel() {
    //     try
    //     {
    //         string json = File.ReadAllText(Path.Combine(Paths.Localization, "config.json"));
    //         var data = JObject.Parse(json);
    //         foreach (var app in data.Properties())
    //         {
    //             if (File.Exists(Path.Combine(Paths.Localization, app.Value.ToString())))
    //             {
    //                 var localization = new ConfigLocalization(
    //                     name: app.Name,
    //                     path: app.Value.ToString()
    //                 );
    //                 ComboBoxItems.Add(localization);
    //             }
    //             else continue;
    //         }
    //     }
    //     catch (FileNotFoundException)
    //     {
    //         Loges.LoggingProcess(level: LogLevel.ERROR,
    //             "file not found json");
    //     }
    //     catch (JsonException ex)
    //     {
    //         Loges.LoggingProcess(level: LogLevel.WARNING,
    //             ex: ex);
    //     }
    //     catch (Exception ex)
    //     {
    //         Loges.LoggingProcess(level: LogLevel.ERROR,
    //             ex: ex);
    //     }
    // }

    // public void SelectLanguage() {
    //     ManagerSettingsJson.Change(SelectedComboBoxItem?.Path ?? string.Empty);
    // }
}
