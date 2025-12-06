using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MsBox.Avalonia;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class CompleteWorkFormUserControlViewModel : ViewModelBase
{
    public CompleteWorkFormUserControlViewModel(double assignmentId, string taskName, decimal assignedQuantity, double productId, double operationId, double subProductOperationId)
    {
        AssignmentId = assignmentId;
        TaskName = taskName;
        AssignedQuantity = assignedQuantity;
        ProductId = productId;
        OperationId = operationId;
        SubProductOperationId = subProductOperationId;

        Loges.LoggingProcess(LogLevel.INFO,
                $"assignmentId: {assignmentId} taskName: {taskName} assignedQuantity: {assignedQuantity} productId: {productId} operationId: {operationId} subProductOperationId: {subProductOperationId}");
    }

    public double AssignmentId { get; }
    public string TaskName { get; } = string.Empty;
    public decimal AssignedQuantity { get; }
    public double ProductId { get; }
    public double OperationId { get; }
    public double SubProductOperationId { get; }

    private decimal _completedToday;
    public decimal CompletedToday
    {
        get => _completedToday;
        set => this.RaiseAndSetIfChanged(ref _completedToday, value);
    }

    private string? _notes;
    public string? Notes
    {
        get => _notes;
        set => this.RaiseAndSetIfChanged(ref _notes, value);
    }

    public ICommand ConfirmCommand 
        => new RelayCommand(async () => await ConfirmCompleteAsync());

    public ICommand CancelCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseCompleteWorkFormStatusMessage(false)));

    private async Task ConfirmCompleteAsync()
    {
        try
        {
            using (var connection = new NpgsqlConnection(Arguments.connection)) 
            {
                await connection.OpenAsync();

                string sqlProduction = "INSERT INTO public.production (user_id, product_id, operation_id, quantity, production_date, notes, status)" +
                                        "VALUES (@user_id, @product_id, @operation_id, @qty, CURRENT_DATE, @notes, 'completed')";

                using (var command = new NpgsqlCommand(sqlProduction, connection))
                {
                    command.Parameters.AddWithValue("@user_id", ManagerCookie.GetIdUser ?? 0);
                    command.Parameters.AddWithValue("@product_id", ProductId);
                    command.Parameters.AddWithValue("@operation_id", OperationId);
                    command.Parameters.AddWithValue("@qty", CompletedToday);
                    command.Parameters.AddWithValue("@notes", Notes ?? "");

                    await command.ExecuteNonQueryAsync();
                }

                string sqlUpdate = @"UPDATE public.sub_product_operations SET completed_quantity = completed_quantity + @qty WHERE id = @op_id";

                using (var command2 = new NpgsqlCommand(sqlUpdate, connection))
                {
                    command2.Parameters.AddWithValue("@qty", CompletedToday);
                    command2.Parameters.AddWithValue("@op_id", SubProductOperationId);

                    await command2.ExecuteNonQueryAsync();
                }
            }

            WeakReferenceMessenger.Default.Send(new RefreshEmployeeTasksMessage());
            WeakReferenceMessenger.Default.Send(new OpenOrCloseCompleteWorkFormStatusMessage(false));
        }
        catch (NpgsqlException ex)
        {
            Loges.LoggingProcess(LogLevel.CRITICAL,
                "Connection or request error",
                ex: ex);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.WARNING,
                "Error loading users by IDs",
                ex: ex);
        }
    }
}
