using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Models;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class AddProductUserControlViewModel : ViewModelBase
{
    private double? Id { get; }
        
    private string _messageerror = string.Empty;
    public string Messageerror
    {
        get => _messageerror;
        set => this.RaiseAndSetIfChanged(ref _messageerror, value);
    }

    private string _productName = string.Empty;
    [UsedImplicitly]
    public string ProductName
    {
        get => _productName;
        set
        {
            this.RaiseAndSetIfChanged(ref _productName, value);
            this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
        }
    }

    private string _productArticle = string.Empty;
    [UsedImplicitly]
    public string ProductArticle
    {
        get => _productArticle;
        set
        {
            this.RaiseAndSetIfChanged(ref _productArticle, value);
            this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
        }
    }

    private string _productDescription = string.Empty;
    [UsedImplicitly]
    public string ProductDescription
    {
        get => _productDescription;
        set
        {
            this.RaiseAndSetIfChanged(ref _productDescription, value);
            this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
        }
    }

    private decimal _productPricePerUnit;
    [UsedImplicitly]
    public decimal ProductPricePerUnit
    {
        get => _productPricePerUnit;
        set
        {
            this.RaiseAndSetIfChanged(ref _productPricePerUnit, value);
            this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
        }
    }

    private decimal _productPricePerKg;
    [UsedImplicitly]
    public decimal ProductPricePerKg
    {
        get => _productPricePerKg;
        set
        {
            this.RaiseAndSetIfChanged(ref _productPricePerKg, value);
            this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
        }
    }

    [UsedImplicitly]
    public List<string> AvailableUnits { get; } =
    [
        "кг",
        "шт"
    ];

    private string? _selectedUnit;
    [UsedImplicitly]
    public string? SelectedUnit
    {
        get => _selectedUnit;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedUnit, value);
            this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
        }
    }

    private string _productMark = string.Empty;
    [UsedImplicitly]
    public string ProductMark
    {
        get => _productMark;
        set
        {
            this.RaiseAndSetIfChanged(ref _productMark, value);
            this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
        }
    }

    private decimal _productCoefficient = 1.0m;
    [UsedImplicitly]
    public decimal ProductCoefficient
    {
        get => _productCoefficient;
        set
        {
            this.RaiseAndSetIfChanged(ref _productCoefficient, value);
            this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
        }
    }

    [UsedImplicitly]
    public ObservableCollection<ComboBoxUser> Users { get; } = [];
    
    private ComboBoxUser? _selectedUser;
    [UsedImplicitly]
    public ComboBoxUser? SelectedUser
    {
        get => _selectedUser;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedUser, value);
            this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
        }
    }

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

    public static ICommand CancelCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseProductStatusMessage(false)));

    public ICommand ConfirmCommand
        => new RelayCommand(async void () =>
        {
            try
            {
                await SaveAsync();
            }
            catch (Exception ex)
            {
                Loges.LoggingProcess(level: LogLevel.Critical, 
                    ex: ex, 
                    message: "Failed to update or add");
                Messageerror = "Failed to update or add";
            }
        });

    private bool DesignedFields 
        => !string.IsNullOrEmpty(ProductName)
        && !string.IsNullOrEmpty(ProductArticle)
        && !string.IsNullOrEmpty(ProductDescription)
        && !string.IsNullOrEmpty(SelectedUnit)
        && !string.IsNullOrEmpty(ProductMark)
        && ProductPricePerUnit != 0
        && ProductPricePerKg != 0
        && ProductCoefficient != 0
        && SelectedUser != null;

    public bool IsActiveConfirmButton
        => DesignedFields;

    private async Task LoadListUsersAsync()
    {
        try
        {
            Users.Clear();

            const string sqlUsers = @"
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

            await using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(sqlUsers, connection);
            await using var reader = await command.ExecuteReaderAsync();
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
        catch (PostgresException ex)
        {
            Loges.LoggingProcess(level: LogLevel.Warning,
                ex: ex);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.Warning,
                ex: ex);
        }
    }

    private async Task SaveAsync()
    {
        if(!ManagerCookie.IsUserLoggedIn()) return;
        
        if (!DesignedFields) return;
        
        try
        {
                const string addProductSql = "INSERT INTO public.product (name, article, description, price_per_unit, price_per_kg, unit, category_id, mark, coefficient) " +
                            "VALUES (@name, @article, @desc, @priceUnit, @priceKg, @unit, @categoryId, @mark, @coefficient) RETURNING id";
                const string updateProductSql = "UPDATE public.product " +
                    "SET name = @name, article = @article, description = @desc, price_per_unit = @priceUnit, price_per_kg = @priceKg, unit = @unit, updated_at = CURRENT_DATE, category_id = @categoryId, mark = @mark, coefficient = @coefficient " +
                    "WHERE id = @id";
                
                var sql = Id != 0 ? updateProductSql : addProductSql;

                await using var connection = new NpgsqlConnection(Arguments.Connection);
                await connection.OpenAsync();
                await using var command = new NpgsqlCommand(sql, connection);
                
                command.Parameters.AddWithValue("@name", ProductName.Trim());
                command.Parameters.AddWithValue("@article", ProductArticle.Trim());
                command.Parameters.AddWithValue("@desc", ProductDescription.Trim());
                command.Parameters.AddWithValue("@priceUnit", ProductPricePerUnit);
                command.Parameters.AddWithValue("@priceKg", ProductPricePerKg);
                command.Parameters.AddWithValue("@unit", SelectedUnit ?? AvailableUnits[0]);
                command.Parameters.AddWithValue("@categoryId", DBNull.Value);
                command.Parameters.AddWithValue("@mark", ProductMark.Trim());
                command.Parameters.AddWithValue("@coefficient", ProductCoefficient);
                
                if (Id == 0)
                {
                    var result = await command.ExecuteScalarAsync();

                    if (result == null || Convert.IsDBNull(result)) return;

                    var newProductId = Convert.ToDouble(result);

                    await CreateTaskForMasterAsync(newProductId);
                }
                else
                {
                    if(Id is null or 0) return;
                    
                    command.Parameters.AddWithValue("@id", Id);
                    await command.ExecuteNonQueryAsync();
                }

                WeakReferenceMessenger.Default.Send(new RefreshProductListMessage());
                WeakReferenceMessenger.Default.Send(new OpenOrCloseProductStatusMessage(false));
                ClearForm();
        }
        catch (PostgresException ex)
        {
            Loges.LoggingProcess(
                level: LogLevel.Warning, 
                ex: ex,
                message: $"Error DB (SQLState: {ex.SqlState}): {ex.MessageText}" );
                            
            Loges.LoggingProcess(
                level: LogLevel.Warning, 
                ex: ex,
                message: $"Error DB (Detail: {ex.Detail})" );
            
            Loges.LoggingProcess(level: LogLevel.Warning,
                ex: ex);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.Warning,
                ex: ex);
        }
    }

    private async Task CreateTaskForMasterAsync(double productId)
    {
        try
        {
            const string sql = "INSERT INTO public.product_tasks (product_id, created_by, assigned_by) " +
                "VALUES (@pid, @created_by, @assigned_by)";
            
            await using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@pid", productId);
            command.Parameters.AddWithValue("@created_by", ManagerCookie.GetIdUser ?? 0);
            if (SelectedUser == null || SelectedUser.Id == 0)
            {
                Messageerror = "Не выбран пользователь";
                return;
            }
            command.Parameters.AddWithValue("@assigned_by", SelectedUser.Id);
            
            await command.ExecuteNonQueryAsync();
        }
        catch (PostgresException ex)
        {
            Loges.LoggingProcess(level: LogLevel.Warning,
                ex: ex);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.Warning, ex: ex);
        }
    }

    private void ClearForm()
    {
        ProductName = string.Empty;
        ProductArticle = string.Empty;
        ProductMark = string.Empty;
        ProductDescription = string.Empty;
        ProductPricePerUnit = 0.0m;
        ProductPricePerKg = 0.0m;
        ProductCoefficient = 1.0m;
        SelectedUnit = AvailableUnits[0]; 
        SelectedUser = null;
    }
}
