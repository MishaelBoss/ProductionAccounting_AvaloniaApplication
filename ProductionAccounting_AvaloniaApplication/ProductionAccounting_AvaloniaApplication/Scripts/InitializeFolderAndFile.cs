using System.IO;

namespace ProductionAccounting_AvaloniaApplication.Scripts;

internal abstract class InitializeFolderAndFile
{
    public static void Initialize()
    {
        if (!Directory.Exists(Paths.Settings)) Directory.CreateDirectory(Paths.Settings);

        if (!File.Exists(Path.Combine(Paths.Settings, Files.ConfigSettingsFileName))) ManagerSettingsJson.CreateSettings();
    }
}
