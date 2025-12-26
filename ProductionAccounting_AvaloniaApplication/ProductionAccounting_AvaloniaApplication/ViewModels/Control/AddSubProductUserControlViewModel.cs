using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class AddSubProductUserControlViewModel(double taskId) : ViewModelBase
{
    private double TaskId { get; } = taskId;
    
    private string _messageerror = string.Empty;
    public string Messageerror
    {
        get => _messageerror;
        set => this.RaiseAndSetIfChanged(ref _messageerror, value);
    }

    private string? _title;
    [UsedImplicitly]
    public string? Title 
    {
        get => _title;
        set 
        {
            this.RaiseAndSetIfChanged(ref _title, value);
            this.RaisePropertyChanged(nameof(CanSaveCurrentSubProduct));
        }
    }

    private decimal? _plannedQuantity = 1;
    [UsedImplicitly]
    public decimal? PlannedQuantity
    {
        get => _plannedQuantity;
        set
        {
            this.RaiseAndSetIfChanged(ref _plannedQuantity, value);
            this.RaisePropertyChanged(nameof(CanSaveCurrentSubProduct));
        }
    }

    private string? _notes;
    [UsedImplicitly]
    public string? Notes
    {
        get => _notes;
        set
        {
            this.RaiseAndSetIfChanged(ref _notes, value);
            this.RaisePropertyChanged(nameof(CanSaveCurrentSubProduct));
        }
    }

    public bool CanSaveCurrentSubProduct
        => !string.IsNullOrWhiteSpace(Title)
        && PlannedQuantity > 0;

    public ICommand SaveCurrentSubProductCommand
        => new RelayCommand(async void () =>
        {
            try
            {
                await SaveCurrentSubProductAsync();
            }
            catch (Exception ex)
            {
                Loges.LoggingProcess(level: LogLevel.Critical, 
                    ex: ex, 
                    message: "Error add or update");
            }
        });

    public static ICommand CancelCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseAddSubProductStatusMessage(false)));

    private async Task SaveCurrentSubProductAsync()
    {
        if (string.IsNullOrEmpty(Title) || PlannedQuantity == 0 )
        {
            Messageerror = "Не все поля заполнены";
            return;
        }
        
        try
        {
            const string sql = "INSERT INTO public.sub_products (product_task_id, name, planned_quantity, notes) VALUES (@task_id, @name, @qty, @notes)";

            await using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@task_id", TaskId);
            command.Parameters.AddWithValue("@name", Title ?? string.Empty);
            command.Parameters.AddWithValue("@qty", PlannedQuantity ?? 1);
            command.Parameters.AddWithValue("@notes", Notes ?? "");

            await command.ExecuteNonQueryAsync();

            WeakReferenceMessenger.Default.Send(new RefreshSubProductListMessage());
            WeakReferenceMessenger.Default.Send(new OpenOrCloseAddSubProductStatusMessage(false));

            ClearForm();
        }
        catch (PostgresException ex)
        {
            Messageerror = "Ошибка базы данных";
            
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
            Messageerror = "Неизвестная ошибка";
            Loges.LoggingProcess(LogLevel.Error, 
                ex: ex);
        }
    }

    private void ClearForm() 
    {
        Title = string.Empty;
        PlannedQuantity = 1;
        Notes = string.Empty;
    }
}
