using MsBox.Avalonia;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;

namespace ProductionAccounting_AvaloniaApplication.ViewModels;

public class ConfirmDeleteWorkshopDepartmentWindowViewModel : ViewModelBase
{
    private double _id = 0;
    public double Id
    {
        get => _id;
        set => this.RaiseAndSetIfChanged(ref _id, value);
    }

    private string _type = string.Empty;
    public string Type
    {
        get => _type;
        set => this.RaiseAndSetIfChanged(ref _type, value);
    }

    public bool Delete()
    {
        try
        {
            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                connection.Open();
                try
                {
                    string sql1 = "DELETE FROM public.departments WHERE id = @id";
                    using (var command1 = new NpgsqlCommand(sql1, connection))
                    {
                        command1.Parameters.AddWithValue("@id", Id);
                        command1.ExecuteNonQuery();
                    }

                    Loges.LoggingProcess(level: LogLevel.INFO,
                        message: $"Deleted position {Id}, Type {Type}");

                    return true;
                }
                catch (Exception ex)
                {
                    MessageBoxManager.GetMessageBoxStandard("Message", "Удалите записи связанные сним").ShowWindowAsync();

                    Loges.LoggingProcess(level: LogLevel.ERROR,
                        ex: ex);
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.ERROR,
                ex: ex);
            return false;
        }
    }
}
