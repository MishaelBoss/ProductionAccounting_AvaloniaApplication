using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class AddDepartmentUserControlViewModel : ViewModelBase, INotifyPropertyChanged
{
    private string _messageerror = string.Empty;
    public string Messageerror
    {
        get => _messageerror;
        set => this.RaiseAndSetIfChanged(ref _messageerror, value);
    }

    private string _department = string.Empty;
    public string Department
    {
        get => _department;
        set 
        {
            this.RaiseAndSetIfChanged(ref _department, value);
            this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
        }
    }

    public static ICommand CancelCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseAddDepartmentStatusMessage(false)));

    public ICommand ConfirmCommand
        => new RelayCommand(async () => await SaveAsync());

    public bool IsActiveConfirmButton 
        => !string.IsNullOrEmpty(Department);

    public async Task SaveAsync() {

        if (string.IsNullOrEmpty(Department))
        {
            Messageerror = "Не все поля заполнены";
            return;
        }

        try
        {
            const string sql = "INSERT INTO public.departments (type) VALUES (@type)";

            await using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("@type", Department);
            await command.ExecuteNonQueryAsync();

            WeakReferenceMessenger.Default.Send(new RefreshDepartmentListMessage());
            WeakReferenceMessenger.Default.Send(new OpenOrCloseAddDepartmentStatusMessage(false));
        }
        catch (PostgresException ex)
        {
            Loges.LoggingProcess(
                level: LogLevel.Warning,
                ex: ex,
                message: $"Error DB (SQLState: {ex.SqlState}): {ex.MessageText}");

            Loges.LoggingProcess(
                level: LogLevel.Warning,
                ex: ex,
                message: $"Error DB (Detail: {ex.Detail})");

            Loges.LoggingProcess(level: LogLevel.Warning,
                ex: ex);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.Warning,
                ex: ex);
        }
    }
}
