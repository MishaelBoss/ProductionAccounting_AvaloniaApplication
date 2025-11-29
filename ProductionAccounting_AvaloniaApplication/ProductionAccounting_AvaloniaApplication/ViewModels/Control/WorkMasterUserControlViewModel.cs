using Avalonia.Controls;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.Views.Control;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using static ProductionAccounting_AvaloniaApplication.ViewModels.Control.NotFoundUserControlViewModel;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class WorkMasterUserControlViewModel : ViewModelBase, INotifyPropertyChanging
{
    public StackPanel? CartTasks { get; set; } = null;
    private List<CartProductUserControl> productList = [];

    /*public WorkMasterUserControlViewModel() 
    {
        _ = LoadListComboBoxAsync();

        SelectedDate = DateTimeOffset.Now;
        SelectedShift = Shifts.FirstOrDefault();
    }

    private ObservableCollection<ComboBoxUserAddWork> _comboBoxUsers = [];
    public ObservableCollection<ComboBoxUserAddWork> ComboBoxUsers
    {
        get => _comboBoxUsers;
        set => this.RaiseAndSetIfChanged(ref _comboBoxUsers, value);
    }

    private ComboBoxUserAddWork? _selectedUser;
    public ComboBoxUserAddWork? SelectedUser
    {
        get => _selectedUser;
        set
        {
            if (_selectedUser != value)
            {
                _selectedUser = value;
                OnPropertyChanged(nameof(SelectedUser));
            }
        }
    }

    private DateTimeOffset _selectedDate = new (DateTime.UtcNow);
    public DateTimeOffset SelectedDate
    {
        get => _selectedDate;
        set => this.RaiseAndSetIfChanged(ref _selectedDate, value);
    }

    public ObservableCollection<ComboBoxShift> Shifts { get; } =
    [
        new ComboBoxShift { Id = 1, Name = "Смена 1", Hours = "08:00-16:00" },
        new ComboBoxShift { Id = 2, Name = "Смена 2", Hours = "16:00-00:00" },
        new ComboBoxShift { Id = 3, Name = "Смена 3", Hours = "00:00-08:00" }
    ];

    private ComboBoxShift? _selectedShift;
    public ComboBoxShift? SelectedShift
    {
        get => _selectedShift;
        set
        {
            if (_selectedShift != value)
            {
                _selectedShift = value;
                OnPropertyChanged(nameof(SelectedShift));
            }
        }
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
        => new RelayCommand(ClearForm);

    public ICommand ConfirmCommand
        => new RelayCommand(async () => { await SaveAsync(); ClearForm(); });

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
            string sqlUsers = @"SELECT id, first_name, last_name, middle_name, login FROM public.user WHERE is_active = true ORDER BY login";

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

                using (var command = new NpgsqlCommand(sqlUsers, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        ComboBoxUsers.Add(new ComboBoxUserAddWork(
                            reader.GetDouble(0),
                            reader.GetString(1),
                            reader.GetString(2),
                            reader.GetString(3),
                            reader.GetString(4)
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

    public async Task SaveAsync()
    {
        try
        {
            if (!ManagerCookie.IsUserLoggedIn() || SelectedProduct == null || Quantity == 0 || SelectedOperation == null || SelectedUser == null || SelectedShift == null) return;

            try
            {
                string sql = @"INSERT INTO public.production (user_id, product_id, operation_id, quantity, production_date, shift, amount, notes, created_by, status)" +
                            "VALUES (@user_id, @product_id, @operation_id, @quantity, @production_date, @shift, @amount, @notes, @created_by, @status)";

                using (var connection = new NpgsqlConnection(Arguments.connection))
                {
                    await connection.OpenAsync();
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        try
                        {
                            command.Parameters.AddWithValue("@user_id", SelectedUser.Id);
                            command.Parameters.AddWithValue("@product_id", (int)SelectedProduct.Id);
                            command.Parameters.AddWithValue("@operation_id", (int)SelectedOperation.Id);
                            command.Parameters.AddWithValue("@quantity", Quantity);
                            command.Parameters.AddWithValue("@production_date", SelectedDate.DateTime);
                            command.Parameters.AddWithValue("@shift", SelectedShift.Id);
                            command.Parameters.AddWithValue("@amount", CalculatedAmount);
                            command.Parameters.AddWithValue("@notes", Description ?? "");
                            command.Parameters.AddWithValue("@created_by", ManagerCookie.GetIdUser ?? 0);
                            command.Parameters.AddWithValue("@status", "issued");

                            await command.ExecuteNonQueryAsync();

                            ClearForm();
                        }
                        catch (Exception ex)
                        {
                            Loges.LoggingProcess(level: LogLevel.ERROR,
                                ex: ex);
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
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.WARNING,
                ex: ex);
        }
    }

    public void ClearForm()
    {
        SelectedUser = null;
        SelectedProduct = null;
        SelectedOperation = null;
        SelectedDate = new DateTimeOffset(DateTime.UtcNow);
        Description = string.Empty;
        Quantity = 0;
        TotalEarnings = 0;
        SelectedShift = Shifts.FirstOrDefault();
    }

    private void UpdateCalculatedAmount()
    {
        this.RaisePropertyChanged(nameof(CalculatedAmount));
        this.RaisePropertyChanged(nameof(QuantityHint));
        this.RaisePropertyChanged(nameof(HasSelectedProduct));
        this.RaisePropertyChanged(nameof(SelectedProductCoefficient));
        this.RaisePropertyChanged(nameof(CalculationFormula));
    }*/

    public async Task LoadTasksAsync()
    {
        StackPanelHelper.ClearAndRefreshStackPanel<CartProductUserControl>(CartTasks, productList);

        try
        {
            string sql = @"
                    SELECT 
                        pt.id,
                        pt.product_id,
                        p.name,
                        COALESCE(p.mark, '') AS mark,
                        p.article,
                        p.unit,
                        p.price_per_unit,
                        p.coefficient,
                        COALESCE(pt.status, 'new') AS status,
                        pt.created_at
                    FROM public.product_tasks pt
                    JOIN public.product p ON p.id = pt.product_id
                    WHERE COALESCE(pt.status, 'new') IN ('new', 'in_progress')
                      AND p.is_active = true
                    ORDER BY pt.created_at DESC";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(sql, connection)) 
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var viewModel = new CartProductUserControlViewModel()
                            {
                                Id = reader.GetDouble(0),
                                ProductId = reader.GetDouble(1),
                                Name = reader.GetString(2),
                                Mark = reader.GetString(3),
                                Article = reader.GetString(4),
                                Unit = reader.GetString(5),
                                PricePerUnit = reader.GetInt32(6),
                                Coefficient = reader.GetInt32(7),
                                Status = reader.GetString(8),
                                CreatedAt = reader.GetDateTime(9)
                            };

                            var userControl = new CartProductUserControl()
                            {
                                DataContext = viewModel,
                            };

                            productList.Add(userControl);
                        }
                    }
                }
            }

            StackPanelHelper.RefreshStackPanelContent<CartProductUserControl>(CartTasks, productList);

            if (productList.Count == 0) ItemNotFoundException.Show(CartTasks, ErrorLevel.NotFound);
        }
        catch (NpgsqlException ex)
        {
            StackPanelHelper.ClearAndRefreshStackPanel<CartProductUserControl>(CartTasks, productList);
            ItemNotFoundException.Show(CartTasks, ErrorLevel.NoConnectToDB);
            Loges.LoggingProcess(LogLevel.CRITICAL, "Error connect to DB", ex: ex);
        }
        catch (Exception ex)
        {
            StackPanelHelper.ClearAndRefreshStackPanel<CartProductUserControl>(CartTasks, productList);
            Loges.LoggingProcess(LogLevel.WARNING, 
                ex: ex);
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
