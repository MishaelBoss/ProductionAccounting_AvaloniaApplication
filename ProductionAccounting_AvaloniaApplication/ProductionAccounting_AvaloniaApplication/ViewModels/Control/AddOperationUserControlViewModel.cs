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
    public ICommand CancelCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseAddOperationStatusMessage(false)) );

    public ICommand ConfirmCommand
        => new RelayCommand(async () => 
        { 
            if ( await SaveAsync() ) 
            { 
                WeakReferenceMessenger.Default.Send(new RefreshOperationListMessage());
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

    private string _selectedUnit = string.Empty;
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

    public bool IsActiveConfirmButton
        => SelectedUnit != null
        && !string.IsNullOrEmpty(OperationName)
        && OperationPrice != 0
        && !string.IsNullOrEmpty(OperationCode)
        && OperationTime != 0
        && !string.IsNullOrEmpty(OperationDescription);

    public async Task<bool> SaveAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(OperationName) || OperationTime == 0 || string.IsNullOrEmpty(OperationCode) || OperationPrice == 0 || SelectedUnit == null || string.IsNullOrEmpty(OperationDescription)) return false;

            try
            {
                string sql = "INSERT INTO public.operation (name, operation_code, price, unit, time_required, description)" +
                        "VALUES (@name, @operation_code, @price, @unit, @time_required, @description)";
                using (var connection = new NpgsqlConnection(Arguments.connection))
                {
                    await connection.OpenAsync();
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        try
                        {
                            command.Parameters.AddWithValue("@name", OperationName);
                            command.Parameters.AddWithValue("@operation_code", OperationCode);
                            command.Parameters.AddWithValue("@price", OperationPrice);
                            command.Parameters.AddWithValue("@unit", SelectedUnit);
                            command.Parameters.AddWithValue("@time_required", OperationTime);
                            command.Parameters.AddWithValue("@description", OperationDescription);

                            await command.ExecuteNonQueryAsync();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            Loges.LoggingProcess(level: LogLevel.ERROR,
                                ex: ex);
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Loges.LoggingProcess(level: LogLevel.WARNING,
                    ex: ex);
                return false;
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.WARNING,
                ex: ex);
            return false;
        }
    }

    public void ClearForm()
    {
        OperationName = string.Empty;
        OperationCode = string.Empty;
        OperationPrice = 0;
        OperationTime = 0;
        OperationDescription = string.Empty;
        SelectedUnit = "шт";
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}