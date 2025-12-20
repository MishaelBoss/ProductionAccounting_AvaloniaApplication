using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class CompleteWorkFormUserControlViewModel : ViewModelBase
{
    public CompleteWorkFormUserControlViewModel(string taskName, decimal plannedQuantity, double productId, double operationId, double subProductOperationId, double userId)
    {
        TaskName = taskName;
        PlannedQuantity = plannedQuantity;
        ProductId = productId;
        OperationId = operationId;
        SubProductOperationId = subProductOperationId;
        UserId = userId;

        Loges.LoggingProcess(LogLevel.INFO,
                $"taskName: {taskName} assignedQuantity: {plannedQuantity} productId: {productId} operationId: {operationId} subProductOperationId: {subProductOperationId} userId {userId}");
    }

    public double AssignmentId { get; }
    public string TaskName { get; } = string.Empty;
    public decimal PlannedQuantity { get; }
    public double ProductId { get; }
    public double OperationId { get; }
    public double SubProductOperationId { get; }
    public double UserId { get; }

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

    private ICommand ConfirmCommand 
        => new RelayCommand(async () => await ConfirmCompleteAsync());

    private static ICommand CancelCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseCompleteWorkFormStatusMessage(false)));

    private async Task ConfirmCompleteAsync()
    {
        try
        {
            using (var connection = new NpgsqlConnection(Arguments.Connection))
            {
                await connection.OpenAsync();

                string rateSql = @"SELECT wr.rate, wr.use_tonnage, wr.coefficient 
                             FROM public.work_rate wr
                             JOIN public.operation o ON o.name = wr.work_type
                             WHERE o.id = @operation_id";

                decimal rate = 0;
                bool useTonnage = false;
                decimal coefficient = 1.0m;

                using (var rateCommand = new NpgsqlCommand(rateSql, connection))
                {
                    rateCommand.Parameters.AddWithValue("@operation_id", OperationId);
                    using var reader = await rateCommand.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        rate = reader.GetDecimal(0);
                        useTonnage = reader.GetBoolean(1);
                        coefficient = reader.GetDecimal(2);
                    }
                }

                decimal calculatedAmount = 0;
                string formula = "";

                if (useTonnage)
                {
                    string tonnageSql = @"SELECT sp.planned_weight 
                                    FROM public.sub_products sp
                                    JOIN public.sub_product_operations spo ON spo.sub_product_id = sp.id
                                    WHERE spo.id = @sub_product_op_id";

                    decimal tonnage = 0;
                    using (var tonnageCommand = new NpgsqlCommand(tonnageSql, connection))
                    {
                        tonnageCommand.Parameters.AddWithValue("@sub_product_op_id", SubProductOperationId);
                        var result = await tonnageCommand.ExecuteScalarAsync();
                        tonnage = result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                    }

                    calculatedAmount = rate * tonnage / 1000 * CompletedToday * coefficient;
                    formula = $"Тариф: {rate} × Тоннаж: {tonnage} / 1000 × Кол-во: {CompletedToday} × Коэф: {coefficient} = {calculatedAmount}";
                }
                else
                {
                    calculatedAmount = rate * CompletedToday;
                    formula = $"Тариф: {rate} × Кол-во: {CompletedToday} = {calculatedAmount}";
                }

                string sqlProduction = @"INSERT INTO public.production 
                                   (user_id, product_id, operation_id, quantity, production_date, 
                                    notes, status, amount, tonnage, calculated_amount, calculation_formula)
                                   VALUES (@user_id, @product_id, @operation_id, @qty, CURRENT_DATE, 
                                           @notes, 'completed', @amount, @tonnage, @calculated_amount, @formula)";

                using (var command = new NpgsqlCommand(sqlProduction, connection))
                {
                    command.Parameters.AddWithValue("@user_id", UserId);
                    command.Parameters.AddWithValue("@product_id", ProductId);
                    command.Parameters.AddWithValue("@operation_id", OperationId);
                    command.Parameters.AddWithValue("@qty", CompletedToday);
                    command.Parameters.AddWithValue("@notes", Notes ?? "");
                    command.Parameters.AddWithValue("@amount", calculatedAmount);
                    command.Parameters.AddWithValue("@calculated_amount", calculatedAmount);
                    command.Parameters.AddWithValue("@formula", formula);

                    if (useTonnage)
                    {
                        string tonnageSql = @"SELECT sp.planned_weight 
                                        FROM public.sub_products sp
                                        JOIN public.sub_product_operations spo ON spo.sub_product_id = sp.id
                                        WHERE spo.id = @sub_product_op_id";

                        using var tonnageCommand = new NpgsqlCommand(tonnageSql, connection);
                        tonnageCommand.Parameters.AddWithValue("@sub_product_op_id", SubProductOperationId);
                        var tonnageResult = await tonnageCommand.ExecuteScalarAsync();
                        var tonnage = tonnageResult != null && tonnageResult != DBNull.Value ? Convert.ToDecimal(tonnageResult) : 0;
                        command.Parameters.AddWithValue("@tonnage", tonnage);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@tonnage", DBNull.Value);
                    }

                    await command.ExecuteNonQueryAsync();
                }

                string sqlUpdate = @"UPDATE public.sub_product_operations 
                               SET completed_quantity = completed_quantity + @qty 
                               WHERE id = @op_id";

                using var command2 = new NpgsqlCommand(sqlUpdate, connection);
                command2.Parameters.AddWithValue("@qty", CompletedToday);
                command2.Parameters.AddWithValue("@op_id", SubProductOperationId);
                await command2.ExecuteNonQueryAsync();
            }

            WeakReferenceMessenger.Default.Send(new RefreshEmployeeTasksMessage());
            WeakReferenceMessenger.Default.Send(new OpenOrCloseCompleteWorkFormStatusMessage(false));
        }
        catch (NpgsqlException ex)
        {
            Loges.LoggingProcess(LogLevel.CRITICAL, "Connection or request error", ex: ex);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.WARNING, "Error loading users by IDs", ex: ex);
        }
    }
}
