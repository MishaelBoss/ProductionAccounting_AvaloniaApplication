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

public class AddProductUserControlViewModel : ViewModelBase, INotifyPropertyChanged
{
    public ICommand CancelCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseAddProductStatusMessage(false)) );

    public ICommand ConfirmCommand
        => new RelayCommand(async () => 
        { 
            if ( await SaveAsync() ) 
            { 
                WeakReferenceMessenger.Default.Send(new RefreshProductListMessage());
                WeakReferenceMessenger.Default.Send(new OpenOrCloseAddProductStatusMessage(false));
            } 
        });
        
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

    private decimal _productCoefficient = 1m;
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

    public bool IsActiveConfirmButton
        => !string.IsNullOrEmpty(ProductName)
        && !string.IsNullOrEmpty(ProductArticle)
        && !string.IsNullOrEmpty(ProductDescription)
        && ProductPricePerUnit != 0
        && ProductPricePerKg != 0
        && SelectedUnit != null
        && ProductCoefficient != 0
        && !string.IsNullOrEmpty(ProductMark);

    public async Task<bool> SaveAsync()
    {
        if(!ManagerCookie.IsUserLoggedIn() && !ManagerCookie.IsManager || !ManagerCookie.IsAdministrator) return false;

        try
        {
            if (string.IsNullOrEmpty(ProductName) || string.IsNullOrEmpty(ProductArticle) || string.IsNullOrEmpty(ProductDescription) || string.IsNullOrEmpty(ProductMark) || ProductPricePerUnit == 0 || ProductPricePerKg == 0 || SelectedUnit == null || ProductCoefficient == 0) return false;

            try
            {
                string sql = "INSERT INTO public.product (name, article, description, price_per_unit, price_per_kg, unit, category_id, mark, coefficient)" +
                            "VALUES (@name, @article, @desc, @priceUnit, @priceKg, @unit, @categoryId, @mark, @coefficient)";
                using (var connection = new NpgsqlConnection(Arguments.connection))
                {
                    await connection.OpenAsync();
                    using (var command = new NpgsqlCommand(sql, connection))
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
        ProductName = string.Empty;
        ProductArticle = string.Empty;
        ProductMark = string.Empty;
        ProductDescription = string.Empty;
        ProductPricePerUnit = 0;
        ProductPricePerKg = 0;
        ProductCoefficient = 1.0m;
        SelectedUnit = "шт.";
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
