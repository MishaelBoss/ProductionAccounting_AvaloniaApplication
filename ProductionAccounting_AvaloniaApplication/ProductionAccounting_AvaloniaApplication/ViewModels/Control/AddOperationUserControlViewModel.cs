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
    private readonly double _subProductId;
    
    public AddOperationUserControlViewModel(double subProductId)
    {
        _subProductId = subProductId;
        ClearForm();
    }

    public ICommand CancelCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseAddOperationStatusMessage(false)) );

    public ICommand ConfirmCommand
        => new RelayCommand(async () => 
        { 
            if ( await SaveAndLinkAsync() ) 
            { 
                WeakReferenceMessenger.Default.Send(new OpenOrCloseAddOperationStatusMessage(false));
            } 
        });

    private string _messageerror = string.Empty;
    public string Messageerror
    {
        get => _messageerror;
        set => this.RaiseAndSetIfChanged(ref _messageerror, value);
    }

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

            double operationId = await CreateOperationAsync();
            
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

    private async Task<double> CreateOperationAsync()
    {
        try
        {
            string sql = @"INSERT INTO public.operation (name, operation_code, price, unit, time_required, description) VALUES (@name, @operation_code, @price, @unit, @time_required, @description) RETURNING id";
            
            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@name", OperationName.Trim());
                    command.Parameters.AddWithValue("@operation_code", OperationCode.Trim());
                    command.Parameters.AddWithValue("@price", OperationPrice);
                    command.Parameters.AddWithValue("@unit", SelectedUnit);
                    command.Parameters.AddWithValue("@time_required", OperationTime);
                    command.Parameters.AddWithValue("@description", OperationDescription.Trim());

                    var result = await command.ExecuteScalarAsync();
                    return result != null && result != DBNull.Value ? Convert.ToDouble(result) : 0;
                }
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.ERROR, ex: ex);
            return 0;
        }
    }

    private async Task<bool> LinkOperationToSubProductAsync(double operationId, decimal quantity)
    {
        try
        {
            string sql = @"INSERT INTO public.sub_product_operations (sub_product_id, operation_id, planned_quantity, notes, status) VALUES (@sub_product_id, @operation_id, @quantity, '', 'planned') RETURNING id";
            
            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@sub_product_id", _subProductId);
                    command.Parameters.AddWithValue("@operation_id", operationId);
                    command.Parameters.AddWithValue("@quantity", quantity);
                    
                    var result = await command.ExecuteScalarAsync();

                    WeakReferenceMessenger.Default.Send(new RefreshSubProductOperationsMessage(_subProductId));

                    return result != null && result != DBNull.Value;
                }
            }
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