using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class AddOperationUserControlViewModel : ViewModelBase
{    
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

        OperationName = operationName ?? string.Empty;
        OperationCode = operationCode ?? string.Empty;
        OperationDescription = operationDescription ?? string.Empty;

        OperationPrice = operationPrice > 0 ? operationPrice : 1.0m;
        OperationTime = operationTime > 0 ? operationTime : 1.0m;
        OperationQuantity = operationQuantity > 0 ? operationQuantity : 1.0m;

        SelectedUnit = "шт";
    }

    public ICommand CancelCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseSubOperationStatusMessage(false)) );

    public ICommand ConfirmCommand
        => new RelayCommand(async () => 
        { 
            if ( await SaveAndLinkAsync() ) 
            { 
                WeakReferenceMessenger.Default.Send(new OpenOrCloseSubOperationStatusMessage(false));
            } 
        });

    private string _messageerror = string.Empty;
    public string Messageerror
    {
        get => _messageerror;
        set => this.RaiseAndSetIfChanged(ref _messageerror, value);
    }

    private double SubProductId { get; set; }
    private double SubProductOperationId { get; set; }

    private string _operationName = string.Empty;
    public string OperationName
    {
        get => _operationName;
        set
        {
            if (_operationName != value)
            {
                _operationName = value;
                OnPropertyChanged(nameof(OperationName));
                OnPropertyChanged(nameof(IsActiveConfirmButton));
            }
        }
    }

    private string _operationCode = string.Empty;
    public string OperationCode
    {
        get => _operationCode;
        set
        {
            if (_operationCode != value)
            {
                _operationCode = value;
                OnPropertyChanged(nameof(OperationCode));
                OnPropertyChanged(nameof(IsActiveConfirmButton));
            }
        }
    }

    private decimal _operationPrice;
    public decimal OperationPrice
    {
        get => _operationPrice;
        set
        {
            if (_operationPrice != value)
            {
                _operationPrice = value;
                OnPropertyChanged(nameof(OperationPrice));
                OnPropertyChanged(nameof(IsActiveConfirmButton));
            }
        }
    }

    private decimal _operationTime;
    public decimal OperationTime
    {
        get => _operationTime;
        set
        {
            if (_operationTime != value)
            {
                _operationTime = value;
                OnPropertyChanged(nameof(OperationTime));
                OnPropertyChanged(nameof(IsActiveConfirmButton));
            }
        }
    }

    public List<string> AvailableUnits { get; } = new List<string>
    {
        "кг",
        "шт"
    };

    private string _selectedUnit = "кг";
    public string SelectedUnit
    {
        get => _selectedUnit;
        set
        {
            if (_selectedUnit != value)
            {
                _selectedUnit = value;
                OnPropertyChanged(nameof(SelectedUnit));
                OnPropertyChanged(nameof(IsActiveConfirmButton));
            }
        }
    }

    private string _operationDescription = string.Empty;
    public string OperationDescription
    {
        get => _operationDescription;
        set
        {
            if (_operationDescription != value)
            {
                _operationDescription = value;
                OnPropertyChanged(nameof(OperationDescription));
                OnPropertyChanged(nameof(IsActiveConfirmButton));
            }
        }
    }

    private decimal _operationQuantity = 1;
    public decimal OperationQuantity
    {
        get => _operationQuantity;
        set
        {
            if (_operationQuantity != value)
            {
                _operationQuantity = value;
                OnPropertyChanged(nameof(OperationQuantity));
                OnPropertyChanged(nameof(IsActiveConfirmButton));
            }
        }
    }

    public bool IsActiveConfirmButton
        => !string.IsNullOrWhiteSpace(SelectedUnit)
        && !string.IsNullOrWhiteSpace(OperationName)
        && OperationPrice > 0
        && !string.IsNullOrWhiteSpace(OperationCode)
        && OperationTime > 0
        && !string.IsNullOrWhiteSpace(OperationDescription)
        && OperationQuantity > 0;

    private async Task<bool> SaveAndLinkAsync()
    {
        try
        {
            if (!IsActiveConfirmButton) 
            {
                Messageerror = "Заполните все обязательные поля";
                return false;
            }

            double operationId = await CreateOrEditOperationAsync();
            
            if (operationId > 0)
            {
                bool linked = await LinkOperationToSubProductAsync(operationId, OperationQuantity);
                
                if (linked)
                {
                    Messageerror = string.Empty;
                    return true;
                }
                else
                {
                    Messageerror = "Ошибка при связывании операции с подмаркой";
                    return false;
                }
            }
            else
            {
                Messageerror = "Ошибка при создании операции";
                return false;
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.ERROR, ex: ex);
            Messageerror = $"Ошибка: {ex.Message}";
            return false;
        }
    }

    private async Task<double> CreateOrEditOperationAsync()
    {
        try
        {
            bool isNewOperation = SubProductOperationId == 0;
            double operationId = 0;

            string sqlAddOperation = "INSERT INTO public.operation (name, operation_code, price, unit, time_required, description) " +
                "VALUES (@name, @operation_code, @price, @unit, @time_required, @description) RETURNING id";

            string sqlUpdateOperation = "UPDATE public.operation SET name = @name, operation_code = @operation_code, price = @price, unit = @unit, time_required = @time_required, description = @description WHERE id = @operation_id";

            string rateSql = @"INSERT INTO public.work_rate (work_type, rate, use_tonnage, coefficient) " +
                "VALUES (@work_type, @rate, @use_tonnage, @coefficient)";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();
                if (isNewOperation)
                {                    
                    using (var command = new NpgsqlCommand(sqlAddOperation, connection))
                    {
                        command.Parameters.AddWithValue("@name", OperationName.Trim());
                        command.Parameters.AddWithValue("@operation_code", OperationCode.Trim());
                        command.Parameters.AddWithValue("@price", OperationPrice);
                        command.Parameters.AddWithValue("@unit", SelectedUnit);
                        command.Parameters.AddWithValue("@time_required", OperationTime);
                        command.Parameters.AddWithValue("@description", OperationDescription.Trim());

                        var result = await command.ExecuteScalarAsync();
                        operationId = result != null && result != DBNull.Value ? Convert.ToDouble(result) : 0;
                    }
                }
                else
                {
                    string getOperationIdSql = "SELECT operation_id FROM public.sub_product_operations WHERE id = @id";
                    using (var getIdCommand = new NpgsqlCommand(getOperationIdSql, connection))
                    {
                        getIdCommand.Parameters.AddWithValue("@id", SubProductOperationId);

                        var idResult = await getIdCommand.ExecuteScalarAsync();

                        if (idResult == null || idResult == DBNull.Value)
                        {
                            Loges.LoggingProcess(LogLevel.WARNING,
                                $"Не найдена операция для связи ID: {SubProductOperationId}");
                            return 0;
                        }

                        operationId = Convert.ToDouble(idResult);
                    }

                    if (operationId > 0)
                    {
                        using (var command = new NpgsqlCommand(sqlUpdateOperation, connection))
                        {
                            command.Parameters.AddWithValue("@name", OperationName.Trim());
                            command.Parameters.AddWithValue("@operation_code", OperationCode.Trim());
                            command.Parameters.AddWithValue("@price", OperationPrice);
                            command.Parameters.AddWithValue("@unit", SelectedUnit);
                            command.Parameters.AddWithValue("@time_required", OperationTime);
                            command.Parameters.AddWithValue("@description", OperationDescription.Trim());
                            command.Parameters.AddWithValue("@operation_id", operationId);

                            int rowsAffected = await command.ExecuteNonQueryAsync();

                            if (rowsAffected == 0)
                            {
                                Loges.LoggingProcess(LogLevel.WARNING,
                                    $"Не удалось обновить операцию ID: {operationId}");
                                return 0;
                            }
                        }
                    }
                }

                if (operationId > 0)
                {
                    try
                    {
                        using (var rateCommand = new NpgsqlCommand(rateSql, connection))
                        {
                            bool useTonnage = SelectedUnit == "кг";
                            decimal coefficient = OperationName.Contains("Зачистка") ||
                                                OperationName.Contains("Сборка") ||
                                                OperationName.Contains("Сварка") ? 1.5m : 1.0m;

                            rateCommand.Parameters.AddWithValue("@work_type", OperationName.Trim());
                            rateCommand.Parameters.AddWithValue("@rate", OperationPrice);
                            rateCommand.Parameters.AddWithValue("@use_tonnage", useTonnage);
                            rateCommand.Parameters.AddWithValue("@coefficient", coefficient);

                            await rateCommand.ExecuteNonQueryAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        Loges.LoggingProcess(LogLevel.WARNING,
                            $"Error while maintaining the tariff: {ex.Message}");
                    }
                }

                return operationId;
            }
        }
        catch (PostgresException ex)
        {
            Loges.LoggingProcess(LogLevel.ERROR,
                $"PostgreSQL error: {ex.MessageText}", ex: ex);
            Messageerror = $"Error DB: {ex.MessageText}";
            return 0;
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.ERROR,
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
                string sqlUpdate = "UPDATE public.sub_product_operations " +
                    "SET planned_quantity = @quantity " +
                    "WHERE id = @sub_product_operation_id AND operation_id = @operation_id";

                using (var connection = new NpgsqlConnection(Arguments.connection))
                {
                    await connection.OpenAsync();
                    using (var command = new NpgsqlCommand(sqlUpdate, connection))
                    {
                        command.Parameters.AddWithValue("@sub_product_operation_id", SubProductOperationId);
                        command.Parameters.AddWithValue("@operation_id", operationId);
                        command.Parameters.AddWithValue("@quantity", quantity);

                        int rowsAffected = await command.ExecuteNonQueryAsync();

                        WeakReferenceMessenger.Default.Send(new RefreshSubProductOperationsMessage(SubProductId));

                        return rowsAffected > 0;
                    }
                }
            }
            else
            {
                string sqlInsert = "INSERT INTO public.sub_product_operations (sub_product_id, operation_id, planned_quantity, notes, status) " +
                    "VALUES (@sub_product_id, @operation_id, @quantity, '', 'planned') RETURNING id";

                using (var connection = new NpgsqlConnection(Arguments.connection))
                {
                    await connection.OpenAsync();
                    using (var command = new NpgsqlCommand(sqlInsert, connection))
                    {
                        command.Parameters.AddWithValue("@sub_product_id", SubProductId);
                        command.Parameters.AddWithValue("@operation_id", operationId);
                        command.Parameters.AddWithValue("@quantity", quantity);

                        var result = await command.ExecuteScalarAsync();

                        WeakReferenceMessenger.Default.Send(new RefreshSubProductOperationsMessage(SubProductId));

                        return result != null && result != DBNull.Value;
                    }
                }
            }
        }
        catch (PostgresException ex)
        {
            Loges.LoggingProcess(level: LogLevel.WARNING, ex: ex);
            return false;
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.ERROR, ex: ex);
            return false;
        }
    }

    public void ClearForm()
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

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}