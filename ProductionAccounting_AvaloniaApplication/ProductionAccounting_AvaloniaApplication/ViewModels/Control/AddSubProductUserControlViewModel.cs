using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Models;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class AddSubProductUserControlViewModel(double taskId) : ViewModelBase
{
    private double TaskId { get; set; } = taskId;

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
        set => SetAndNotify(ref _title, value);
    }

    private decimal? _quantity;
    [UsedImplicitly]
    public decimal? Quantity
    {
        get => _quantity;
        set => SetAndNotify(ref _quantity, value);
    }

    private decimal? _tonnage;
    [UsedImplicitly]
    public decimal? Tonnage
    {
        get => _tonnage;
        set => SetAndNotify(ref _tonnage, value);
    }

    private string? _notes;
    [UsedImplicitly]
    public string? Notes
    {
        get => _notes;
        set => this.RaiseAndSetIfChanged(ref _notes, value);
    }

    private bool DesignedFields
        => !string.IsNullOrWhiteSpace(Title)
        && Quantity > 0
        && Tonnage > 0;

    public bool IsActiveConfirmButton
        => DesignedFields;

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

    private void SetAndNotify<T>(ref T field, T value)
    {
        this.RaiseAndSetIfChanged(ref field, value);
        this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
    }

    private async Task SaveCurrentSubProductAsync()
    {
        if (!DesignedFields)
        {
            Messageerror = "Не все поля заполнены";
            return;
        }
        
        try
        {
            const string sql = "INSERT INTO public.sub_products (product_task_id, name, quantity, tonnage, notes, work_type_id, assigned_to_user_id) VALUES (@task_id, @name, @quantity, @tonnage, @notes, @work_type_id, @assigned_to_user_id)";

            await using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@task_id", TaskId);
            command.Parameters.AddWithValue("@name", Title ?? string.Empty);
            command.Parameters.AddWithValue("@quantity", Quantity ?? 1);
            command.Parameters.AddWithValue("@tonnage", Tonnage ?? 1);
            command.Parameters.AddWithValue("@notes", Notes ?? "");
            command.Parameters.AddWithValue("@work_type_id", SelectedWorkType?.Id ?? 0);
            command.Parameters.AddWithValue("@assigned_to_user_id", SelectedEmployee?.Id ?? 0);

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
        Quantity = 0;
        Tonnage = 0;
        Notes = string.Empty;
    }
}
