using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MsBox.Avalonia;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Models;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class AddProductUserControlViewModel : ViewModelBase, INotifyPropertyChanged
{
    public AddProductUserControlViewModel(
        double? id = null, 
        string? productName = null, 
        string? productArticle = null, 
        string? productDescription = null,
        decimal? productPricePerUnit = 0, 
        decimal? productPricePerKg = null,
        string? productMark = null,
        decimal? productCoefficient = null) 
    { 
        Id = id;
        ProductName = productName ?? string.Empty;
        ProductArticle = productArticle ?? string.Empty;
        ProductDescription = productDescription ?? string.Empty;
        ProductPricePerUnit = productPricePerUnit ?? 1.0m;
        ProductPricePerKg = productPricePerKg ?? 1.0m;
        ProductMark = productMark ?? string.Empty;
        ProductCoefficient = productCoefficient ?? 1.0m;

        _ = LoadListUsersAsync();
    }

    public ICommand CancelCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseProductStatusMessage(false)) );

    public ICommand ConfirmCommand
        => new RelayCommand(async () => await SaveAsync());

    public double? Id { get; set; }
        
    private string _messageerror = string.Empty;
    public string Messageerror
    {
        get => _messageerror;
        set => this.RaiseAndSetIfChanged(ref _messageerror, value);
    }

    private string _productName = string.Empty;
    public string ProductName
    {
        get => _productName;
        set
        {
            if (_productName != value)
            {
                _productName = value;
                OnPropertyChanged(nameof(ProductName));
                OnPropertyChanged(nameof(IsActiveConfirmButton));
            }
        }
    }

    private string _productArticle = string.Empty;
    public string ProductArticle
    {
        get => _productArticle;
        set
        {
            if (_productArticle != value)
            {
                _productArticle = value;
                OnPropertyChanged(nameof(ProductArticle));
                OnPropertyChanged(nameof(IsActiveConfirmButton));
            }
        }
    }

    private string _productDescription = string.Empty;
    public string ProductDescription
    {
        get => _productDescription;
        set
        {
            if (_productDescription != value)
            {
                _productDescription = value;
                OnPropertyChanged(nameof(ProductDescription));
                OnPropertyChanged(nameof(IsActiveConfirmButton));
            }
        }
    }

    private decimal _productPricePerUnit;
    public decimal ProductPricePerUnit
    {
        get => _productPricePerUnit;
        set
        {
            if (_productPricePerUnit != value)
            {
                _productPricePerUnit = value;
                OnPropertyChanged(nameof(ProductPricePerUnit));
                OnPropertyChanged(nameof(IsActiveConfirmButton));
            }
        }
    }

    private decimal _productPricePerKg;
    public decimal ProductPricePerKg
    {
        get => _productPricePerKg;
        set
        {
            if (_productPricePerKg != value)
            {
                _productPricePerKg = value;
                OnPropertyChanged(nameof(ProductPricePerKg));
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

    private string _productMark = string.Empty;
    public string ProductMark
    {
        get => _productMark;
        set
        {
            if (_productMark != value)
            {
                _productMark = value;
                OnPropertyChanged(nameof(ProductMark));
                OnPropertyChanged(nameof(IsActiveConfirmButton));
            }
        }
    }

    private decimal _productCoefficient = 1.0m;
    public decimal ProductCoefficient
    {
        get => _productCoefficient;
        set
        {
            if (_productCoefficient != value)
            {
                _productCoefficient = value;
                OnPropertyChanged(nameof(ProductCoefficient));
                OnPropertyChanged(nameof(IsActiveConfirmButton));
            }
        }
    }

    public ObservableCollection<ComboBoxUser> Users { get; } = [];

    private ComboBoxUser? _selectedUser;
    public ComboBoxUser? SelectedUser
    {
        get => _selectedUser;
        set
        {
            if (_selectedUser != value)
            {
                _selectedUser = value;
                OnPropertyChanged(nameof(SelectedUser));
                OnPropertyChanged(nameof(IsActiveConfirmButton));
            }
        }
    }

    private bool DesignedFields 
        => !string.IsNullOrEmpty(ProductName)
        && !string.IsNullOrEmpty(ProductArticle)
        && !string.IsNullOrEmpty(ProductDescription)
        && ProductPricePerUnit != 0
        && ProductPricePerKg != 0
        && SelectedUnit != null
        && ProductCoefficient != 0
        && !string.IsNullOrEmpty(ProductMark)
        && SelectedUser != null;

    public bool IsActiveConfirmButton
        => DesignedFields;

    private async Task LoadListUsersAsync()
    {
        try
        {
            Users.Clear();

            string sqlUsers = @"
                                SELECT 
                                    u.id, 
                                    u.first_name, 
                                    u.last_name, 
                                    u.middle_name, 
                                    u.login 
                                FROM public.user AS u
                                JOIN public.user_to_user_type AS uut ON u.id = uut.user_id
                                JOIN public.user_type AS ut ON uut.user_type_id = ut.id
                                WHERE u.is_active = true 
                                    AND ut.type_user = 'Администратор'
                                    OR ut.type_user = 'Мастер'
                                ORDER BY u.last_name, u.first_name";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand(sqlUsers, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var user = new ComboBoxUser(
                            reader.GetDouble(0),
                            reader.GetString(1),
                            reader.GetString(2),
                            reader.GetString(3),
                            reader.GetString(4)
                        );

                        Users.Add(user);
                    }
                }
            }
        }
        catch (PostgresException ex)
        {
            Loges.LoggingProcess(level: LogLevel.WARNING,
                ex: ex);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.WARNING,
                ex: ex);
        }
    }

    public async Task SaveAsync()
    {
        if(!ManagerCookie.IsUserLoggedIn() && !ManagerCookie.IsManager || !ManagerCookie.IsAdministrator) return;

        try
        {
            if (!DesignedFields) return;

            try
            {
                string addProductSql = "INSERT INTO public.product (name, article, description, price_per_unit, price_per_kg, unit, category_id, mark, coefficient)" +
                            "VALUES (@name, @article, @desc, @priceUnit, @priceKg, @unit, @categoryId, @mark, @coefficient) RETURNING id";
                string updateProductSql = "UPDATE public.product " +
                    "SET name = @name, article = @article, description = @desc, price_per_unit = @priceUnit, price_per_kg = @priceKg, unit = @unit, updated_at = CURRENT_DATE, category_id = @categoryId, mark = @mark, coefficient = @coefficient" +
                    "WHERE id = @id";

                string sql = string.Empty;

                if (Id == 0) sql = addProductSql;
                else sql = updateProductSql;

                using (var connection = new NpgsqlConnection(Arguments.connection))
                {
                    await connection.OpenAsync();
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        try
                        {
                            if (Id == 0)
                            {
                                try
                                {
                                    command.Parameters.AddWithValue("@name", ProductName.Trim());
                                    command.Parameters.AddWithValue("@article", ProductArticle?.Trim() ?? "");
                                    command.Parameters.AddWithValue("@desc", ProductDescription?.Trim() ?? "");
                                    command.Parameters.AddWithValue("@priceUnit", ProductPricePerUnit);
                                    command.Parameters.AddWithValue("@priceKg", ProductPricePerKg);
                                    command.Parameters.AddWithValue("@unit", SelectedUnit);
                                    command.Parameters.AddWithValue("@categoryId", DBNull.Value);
                                    command.Parameters.AddWithValue("@mark", ProductMark?.Trim() ?? "");
                                    command.Parameters.AddWithValue("@coefficient", ProductCoefficient);

                                    var result = await command.ExecuteScalarAsync();

                                    if (result == null || result == DBNull.Value) return;

                                    double newProductId = Convert.ToDouble(result);

                                    await CreateTaskForMasterAsync(newProductId);
                                }
                                catch (PostgresException ex) 
                                {
                                    Loges.LoggingProcess(level: LogLevel.WARNING,
                                        ex: ex);
                                }
                                catch (Exception ex) 
                                {
                                    Loges.LoggingProcess(level: LogLevel.WARNING,
                                        ex: ex);
                                }
                            }
                            else 
                            {
                                try
                                {
                                    command.Parameters.AddWithValue("@id", Id ?? 0);
                                    command.Parameters.AddWithValue("@name", ProductName.Trim());
                                    command.Parameters.AddWithValue("@article", ProductArticle?.Trim() ?? "");
                                    command.Parameters.AddWithValue("@desc", ProductDescription?.Trim() ?? "");
                                    command.Parameters.AddWithValue("@priceUnit", ProductPricePerUnit);
                                    command.Parameters.AddWithValue("@priceKg", ProductPricePerKg);
                                    command.Parameters.AddWithValue("@unit", SelectedUnit);
                                    command.Parameters.AddWithValue("@categoryId", DBNull.Value);
                                    command.Parameters.AddWithValue("@mark", ProductMark?.Trim() ?? "");
                                    command.Parameters.AddWithValue("@coefficient", ProductCoefficient);

                                    await command.ExecuteNonQueryAsync();
                                }
                                catch (PostgresException ex)
                                {
                                    Loges.LoggingProcess(level: LogLevel.WARNING,
                                        ex: ex);
                                }
                                catch (Exception ex)
                                {
                                    Loges.LoggingProcess(level: LogLevel.WARNING,
                                        ex: ex);
                                }
                            }

                            WeakReferenceMessenger.Default.Send(new RefreshProductListMessage());
                            WeakReferenceMessenger.Default.Send(new OpenOrCloseProductStatusMessage(false));

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
        catch (PostgresException ex)
        {
            Loges.LoggingProcess(level: LogLevel.WARNING,
                ex: ex);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.WARNING,
                ex: ex);
        }
    }

    private async Task CreateTaskForMasterAsync(double productId)
    {
        try
        {
            string sql = "INSERT INTO public.product_tasks (product_id, created_by, assigned_by) " +
                "VALUES (@pid, @created_by, @assigned_by)";
            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@pid", productId);
                    command.Parameters.AddWithValue("@created_by", ManagerCookie.GetIdUser ?? 0);
                    command.Parameters.AddWithValue("@assigned_by", SelectedUser!.Id);

                    MessageBoxManager.GetMessageBoxStandard("sad", SelectedUser!.Id.ToString());

                    await command.ExecuteNonQueryAsync();
                }
            }
        }
        catch (PostgresException ex)
        {
            Loges.LoggingProcess(level: LogLevel.WARNING,
                ex: ex);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.WARNING, ex: ex);
        }
    }

    public void ClearForm()
    {
        ProductName = string.Empty;
        ProductArticle = string.Empty;
        ProductMark = string.Empty;
        ProductDescription = string.Empty;
        ProductPricePerUnit = 0.0m;
        ProductPricePerKg = 0.0m;
        ProductCoefficient = 1.0m;
        SelectedUnit = "шт.";
        SelectedUser = null;
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
