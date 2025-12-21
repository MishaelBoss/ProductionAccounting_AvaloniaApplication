using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class AddOperationUserControlViewModel : ViewModelBase
{
    private string _messageerror = string.Empty;
    public string Messageerror
    {
        get => _messageerror;
        set => this.RaiseAndSetIfChanged(ref _messageerror, value);
    }

    private double SubProductId { get; }
    private double SubProductOperationId { get; }

    private string _operationName = string.Empty;
    [UsedImplicitly]
    public string OperationName
    {
        get => _operationName;
        set
        {
            this.RaiseAndSetIfChanged(ref _operationName, value);
            this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
        }
    }

    private string _operationCode = string.Empty;
    [UsedImplicitly]
    public string OperationCode
    {
        get => _operationCode;
        set
        {
            this.RaiseAndSetIfChanged(ref _operationCode, value);
            this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
        }
    }

    private decimal _operationPrice;
    [UsedImplicitly]
    public decimal OperationPrice
    {
        get => _operationPrice;
        set
        {
            this.RaiseAndSetIfChanged(ref _operationPrice, value);
            this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
        }
    }

    private decimal _operationTime;
    [UsedImplicitly]
    public decimal OperationTime
    {
        get => _operationTime;
        set
        {
            this.RaiseAndSetIfChanged(ref _operationTime, value);
            this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
        }
    }

    public List<string> AvailableUnits { get; } =
    [
        "кг",
        "шт"
    ];

    private string _selectedUnit = "кг";
    [UsedImplicitly]
    public string SelectedUnit
    {
        get => _selectedUnit;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedUnit, value);
            this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
        }
    }

    private string _operationDescription = string.Empty;
    [UsedImplicitly]
    public string OperationDescription
    {
        get => _operationDescription;
        set
        {
            this.RaiseAndSetIfChanged(ref _operationDescription, value);
            this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
        }
    }

    private decimal _operationQuantity = 1;
    [UsedImplicitly]
    public decimal OperationQuantity
    {
        get => _operationQuantity;
        set
        {
            this.RaiseAndSetIfChanged(ref _operationQuantity, value);
            this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
        }
    }

    public AddOperationUserControlViewModel(double subProductId,
    double subProductOperationId = 0,
    string operationName = "",
    string operationCode = "",
    decimal operationPrice = 0,
    decimal operationTime = 0,
    string operationDescription = "",
    decimal operationQuantity = 0)
    {
        SubProductId = subProductId;
        SubProductOperationId = subProductOperationId;

        OperationName = operationName;
        OperationCode = operationCode;
        OperationDescription = operationDescription;

        OperationPrice = operationPrice > 0 ? operationPrice : 1.0m;
        OperationTime = operationTime > 0 ? operationTime : 1.0m;
        OperationQuantity = operationQuantity > 0 ? operationQuantity : 1.0m;

        SelectedUnit = "шт";
    }

    public static ICommand CancelCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseSubOperationStatusMessage(false)));

    public ICommand ConfirmCommand
        => new RelayCommand(async () => await SaveAndLinkAsync());

    public bool IsActiveConfirmButton
        => !string.IsNullOrWhiteSpace(SelectedUnit)
        && !string.IsNullOrWhiteSpace(OperationName)
        && OperationPrice > 0
        && !string.IsNullOrWhiteSpace(OperationCode)
        && OperationTime > 0
        && !string.IsNullOrWhiteSpace(OperationDescription)
        && OperationQuantity > 0;

    private async Task SaveAndLinkAsync()
    {
        try
        {
            if (!IsActiveConfirmButton) 
            {
                Messageerror = "Заполните все обязательные поля";
                return;
            }

            double operationId = await CreateOrEditOperationAsync();
            
            if (operationId > 0)
            {
                bool linked = await LinkOperationToSubProductAsync(operationId, OperationQuantity);
                
                if (linked)
                {
                    Messageerror = string.Empty;
                    WeakReferenceMessenger.Default.Send(new OpenOrCloseSubOperationStatusMessage(false));
                }
                else
                {
                    Messageerror = "Ошибка при связывании операции с подмаркой";
                }
            }
            else
            {
                Messageerror = "Ошибка при создании операции";
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.Error, ex: ex);
            Messageerror = $"Ошибка: {ex.Message}";
        }
    }

    private async Task<double> CreateOrEditOperationAsync()
    {
        try
        {
            var isNewOperation = SubProductOperationId == 0;
            double operationId;

            const string sqlAddOperation = "INSERT INTO public.operation (name, operation_code, price, unit, time_required, description) " +
                "VALUES (@name, @operation_code, @price, @unit, @time_required, @description) RETURNING id";

            const string sqlUpdateOperation = "UPDATE public.operation SET name = @name, operation_code = @operation_code, price = @price, unit = @unit, time_required = @time_required, description = @description WHERE id = @operation_id";

            const string rateSql = @"INSERT INTO public.work_rate (work_type, rate, use_tonnage, coefficient) " +
                "VALUES (@work_type, @rate, @use_tonnage, @coefficient)";

            await using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();
            if (isNewOperation)
            {
                await using var command = new NpgsqlCommand(sqlAddOperation, connection);
                command.Parameters.AddWithValue("@name", OperationName.Trim());
                command.Parameters.AddWithValue("@operation_code", OperationCode.Trim());
                command.Parameters.AddWithValue("@price", OperationPrice);
                command.Parameters.AddWithValue("@unit", SelectedUnit);
                command.Parameters.AddWithValue("@time_required", OperationTime);
                command.Parameters.AddWithValue("@description", OperationDescription.Trim());

                var result = await command.ExecuteScalarAsync();
                operationId = result != null && result != DBNull.Value ? Convert.ToDouble(result) : 0;
            }
            else
            {
                const string getOperationIdSql = "SELECT operation_id FROM public.sub_product_operations WHERE id = @id";
                await using (var getIdCommand = new NpgsqlCommand(getOperationIdSql, connection))
                {
                    getIdCommand.Parameters.AddWithValue("@id", SubProductOperationId);

                    var idResult = await getIdCommand.ExecuteScalarAsync();

                    if (idResult == null || idResult == DBNull.Value)
                    {
                        Loges.LoggingProcess(LogLevel.Warning,
                            $"Не найдена операция для связи ID: {SubProductOperationId}");
                        return 0;
                    }

                    operationId = Convert.ToDouble(idResult);
                }

                if (operationId > 0)
                {
                    await using var command = new NpgsqlCommand(sqlUpdateOperation, connection);
                    command.Parameters.AddWithValue("@name", OperationName.Trim());
                    command.Parameters.AddWithValue("@operation_code", OperationCode.Trim());
                    command.Parameters.AddWithValue("@price", OperationPrice);
                    command.Parameters.AddWithValue("@unit", SelectedUnit);
                    command.Parameters.AddWithValue("@time_required", OperationTime);
                    command.Parameters.AddWithValue("@description", OperationDescription.Trim());
                    command.Parameters.AddWithValue("@operation_id", operationId);

                    var rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        Loges.LoggingProcess(LogLevel.Warning,
                            $"Не удалось обновить операцию ID: {operationId}");
                        return 0;
                    }
                }
            }

            if (!(operationId > 0)) return operationId;
            try
            {
                await using var rateCommand = new NpgsqlCommand(rateSql, connection);
                var useTonnage = SelectedUnit == "кг";
                var coefficient = OperationName.Contains("Зачистка") ||
                                      OperationName.Contains("Сборка") ||
                                      OperationName.Contains("Сварка") ? 1.5m : 1.0m;

                rateCommand.Parameters.AddWithValue("@work_type", OperationName.Trim());
                rateCommand.Parameters.AddWithValue("@rate", OperationPrice);
                rateCommand.Parameters.AddWithValue("@use_tonnage", useTonnage);
                rateCommand.Parameters.AddWithValue("@coefficient", coefficient);

                await rateCommand.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Loges.LoggingProcess(LogLevel.Warning,
                    $"Error while maintaining the tariff: {ex.Message}");
            }

            return operationId;
        }
        catch (PostgresException ex)
        {
            Loges.LoggingProcess(LogLevel.Error,
                $"PostgreSQL error: {ex.MessageText}", ex: ex);
            Messageerror = $"Error DB: {ex.MessageText}";
            return 0;
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.Error,
                $"General error: {ex.Message}", ex: ex);
            Messageerror = $"Error: {ex.Message}";
            return 0;
        }
    }

    private async Task<bool> LinkOperationToSubProductAsync(double operationId, decimal quantity)
    {
        try
        {
            if (SubProductOperationId > 0)
            {
                const string sqlUpdate = "UPDATE public.sub_product_operations " +
                    "SET planned_quantity = @quantity " +
                    "WHERE id = @sub_product_operation_id AND operation_id = @operation_id";

                await using var connection = new NpgsqlConnection(Arguments.Connection);
                await connection.OpenAsync();
                await using var command = new NpgsqlCommand(sqlUpdate, connection);
                command.Parameters.AddWithValue("@sub_product_operation_id", SubProductOperationId);
                command.Parameters.AddWithValue("@operation_id", operationId);
                command.Parameters.AddWithValue("@quantity", quantity);

                var rowsAffected = await command.ExecuteNonQueryAsync();

                WeakReferenceMessenger.Default.Send(new RefreshSubProductOperationsMessage(SubProductId));
                
                ClearForm();
                
                return rowsAffected > 0;
            }
            else
            {
                const string sqlInsert = "INSERT INTO public.sub_product_operations (sub_product_id, operation_id, planned_quantity, notes, status) " +
                    "VALUES (@sub_product_id, @operation_id, @quantity, '', 'planned') RETURNING id";

                await using var connection = new NpgsqlConnection(Arguments.Connection);
                await connection.OpenAsync();
                await using var command = new NpgsqlCommand(sqlInsert, connection);
                command.Parameters.AddWithValue("@sub_product_id", SubProductId);
                command.Parameters.AddWithValue("@operation_id", operationId);
                command.Parameters.AddWithValue("@quantity", quantity);

                var result = await command.ExecuteScalarAsync();

                WeakReferenceMessenger.Default.Send(new RefreshSubProductOperationsMessage(SubProductId));
                
                ClearForm();

                return result != null && result != DBNull.Value;
            }
        }
        catch (PostgresException ex)
        {
            Loges.LoggingProcess(level: LogLevel.Warning, ex: ex);
            return false;
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.Error, ex: ex);
            return false;
        }
    }

    private void ClearForm()
    {
        OperationName = string.Empty;
        OperationCode = string.Empty;
        OperationPrice = 0;
        OperationTime = 0;
        OperationQuantity = 1;
        OperationDescription = string.Empty;
        SelectedUnit = "шт";
        Messageerror = string.Empty;
    }
}