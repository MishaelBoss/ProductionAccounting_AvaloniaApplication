using CommunityToolkit.Mvvm.Input;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Models;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class AddWorkUserControlViewModel : ViewModelBase, INotifyPropertyChanging
{
    private DateTimeOffset _selectedDate = new(DateTime.UtcNow);
    public DateTimeOffset SelectedDate
    {
        get => _selectedDate;
        set => this.RaiseAndSetIfChanged(ref _selectedDate, value);
    }

    private int _selectedShift = 1;
    public int SelectedShift
    {
        get => _selectedShift;
        set => this.RaiseAndSetIfChanged(ref _selectedShift, value);
    }

    private ObservableCollection<ComboBoxProduct> _comboBoxProducts = [];
    public ObservableCollection<ComboBoxProduct> ComboBoxProducts
    {
        get => _comboBoxProducts;
        set => this.RaiseAndSetIfChanged(ref _comboBoxProducts, value);
    }

    private ComboBoxProduct? _selectedProduct;
    public ComboBoxProduct? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            if (_selectedProduct != value)
            {
                _selectedProduct = value;
                OnPropertyChanged(nameof(SelectedProduct));
                UpdateCalculatedAmount();
            }
        }
    }

    private ObservableCollection<ComboBoxOperation> _comboBoxOperations = [];
    public ObservableCollection<ComboBoxOperation> ComboBoxOperations
    {
        get => _comboBoxOperations;
        set => this.RaiseAndSetIfChanged(ref _comboBoxOperations, value);
    }

    private ComboBoxOperation? _selectedOperation;
    public ComboBoxOperation? SelectedOperation
    {
        get => _selectedOperation;
        set
        {
            if (_selectedOperation != value)
            {
                _selectedOperation = value;
                OnPropertyChanged(nameof(SelectedOperation));
            }
        }
    }

    private decimal _quantity;
    public decimal Quantity
    {
        get => _quantity;
        set
        {
            this.RaiseAndSetIfChanged(ref _quantity, value);
            UpdateCalculatedAmount();
        }
    }

    private string _description = string.Empty;
    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    private decimal _totalEarnings;
    public decimal TotalEarnings
    {
        get => _totalEarnings;
        set => this.RaiseAndSetIfChanged(ref _totalEarnings, value);
    }

    public ICommand ClearFormCommand
        => new RelayCommand(() => ClearForm());

    public decimal CalculatedAmount
    {
        get
        {
            if (SelectedProduct == null || Quantity <= 0) return 0;
            return SelectedProduct.PricePerUnit * Quantity * SelectedProduct.Coefficient;
        }
    }

    public bool HasSelectedProduct => SelectedProduct != null;

    public string SelectedProductCoefficient => SelectedProduct?.Coefficient.ToString("N2") ?? "1.00";

    public string QuantityHint => SelectedProduct?.MaxQuantity > 0 ? $"Максимальное количество: {SelectedProduct.MaxQuantity}" : "Введите количество (можно дробное: 0.5, 1.25 и т.д.)";

    public string CalculationFormula
    {
        get
        {
            if (SelectedProduct == null || Quantity <= 0) return "";
            return $"{Quantity} × {SelectedProduct.PricePerUnit:N2} × {SelectedProduct.Coefficient:N2}";
        }
    }

    public async Task LoadListComboBoxAsync()
    {
        try
        {
            var sqlProducts = @"SELECT id, name, article, mark, coefficient, price_per_unit, price_per_kg, unit FROM public.product WHERE is_active = true ORDER BY name";
            string sqlOperations = @"SELECT id, name, price, unit FROM public.operation WHERE is_active = true ORDER BY name";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand(sqlProducts, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        ComboBoxProducts.Add(new ComboBoxProduct(
                            reader.IsDBNull(0) ? 0 : reader.GetDouble(0),
                            reader.IsDBNull(1) ? "Не указано" : reader.GetString(1),
                            reader.IsDBNull(2) ? "" : reader.GetString(2),
                            reader.IsDBNull(3) ? "" : reader.GetString(3),
                            reader.IsDBNull(4) ? 1 : reader.GetDecimal(4),
                            reader.IsDBNull(5) ? 0 : reader.GetDecimal(5),
                            reader.IsDBNull(6) ? 0 : reader.GetDecimal(6),
                            reader.IsDBNull(7) ? "шт." : reader.GetString(7)
                        ));
                    }
                }

                using (var command = new NpgsqlCommand(sqlOperations, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        ComboBoxOperations.Add(new ComboBoxOperation(
                            reader.GetDouble(0),
                            reader.GetString(1),
                            reader.GetDecimal(2),
                            reader.GetString(3)
                        ));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.WARNING,
                ex: ex);
        }
    }

    public bool Upload()
    {
        try
        {
            if (SelectedProduct == null || Quantity == 0 || SelectedOperation == null) return false;

            try
            {
                string sql = @"INSERT INTO public.production (user_id, product_id, operation_id, quantity, production_date, shift, amount, notes)" +
                            "VALUES (@user_id, @product_id, @operation_id, @quantity, @production_date, @shift, @amount, @notes)";

                using (var connection = new NpgsqlConnection(Arguments.connection))
                {
                    connection.Open();
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        try
                        {
                            command.Parameters.AddWithValue("@user_id", ManagerCookie.GetIdUser);
                            command.Parameters.AddWithValue("@product_id", (int)SelectedProduct.Id);
                            command.Parameters.AddWithValue("@operation_id", (int)SelectedOperation.Id);
                            command.Parameters.AddWithValue("@quantity", Quantity);
                            command.Parameters.AddWithValue("@production_date", SelectedDate.DateTime);
                            command.Parameters.AddWithValue("@shift", SelectedShift);
                            command.Parameters.AddWithValue("@amount", CalculatedAmount);
                            command.Parameters.AddWithValue("@notes", Description ?? "");

                            int rowsAffected = command.ExecuteNonQuery();
                            return rowsAffected > 0;
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
        SelectedProduct = null;
        SelectedOperation = null;
        SelectedDate = new DateTimeOffset(DateTime.UtcNow);
        Description = string.Empty;
        Quantity = 0;
        TotalEarnings = 0;
        SelectedShift = 1;
    }

    private void UpdateCalculatedAmount()
    {
        this.RaisePropertyChanged(nameof(CalculatedAmount));
        this.RaisePropertyChanged(nameof(QuantityHint));
        this.RaisePropertyChanged(nameof(HasSelectedProduct));
        this.RaisePropertyChanged(nameof(SelectedProductCoefficient));
        this.RaisePropertyChanged(nameof(CalculationFormula));
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}