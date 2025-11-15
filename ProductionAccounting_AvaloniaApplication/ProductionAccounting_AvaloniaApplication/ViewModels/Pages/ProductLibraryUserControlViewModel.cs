using Avalonia.Controls;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.View.Control;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;
using ProductionAccounting_AvaloniaApplication.Views.Control;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using static ProductionAccounting_AvaloniaApplication.ViewModels.Control.NotFoundUserControlViewModel;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

public class ProductLibraryUserControlViewModel : ViewModelBase, INotifyPropertyChanged
{
    public StackPanel? MainContent { get; set; } = null;

    private List<CartProductUserControl> _productList = [];

    private string _search = string.Empty;
    public string Search
    {
        get => _search;
        set
        {
            if (_search != value)
            {
                _search = value;
                OnPropertyChanged(nameof(Search));
                PerformSearchListProduct();
            }
        }
    }

    private async void PerformSearchListProduct()
    {
        if (string.IsNullOrWhiteSpace(Search))
        {
            GetListProduct();
            return;
        }

        await SearchProductAsync($"%{Search}%");
    }

    public async void GetListProduct()
    {
        await SearchProductAsync("%");
    }

    private async Task SearchProductAsync(string search)
    {
        //if (!ManagerCookie.IsUserLoggedIn()) return;

        ClearResults();

        try
        {
            string sql = "SELECT * FROM public.\"product\" WHERE name ILIKE @name OR article ILIKE @article";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@name", search);
                    command.Parameters.AddWithValue("@article", search);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            double dbid = reader.IsDBNull(0) ? 0 : reader.GetDouble(0);
                            string dbname = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                            string dbarticle = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                            string dbpricePerUnit = reader.IsDBNull(4) ? string.Empty : reader.GetDecimal(4).ToString();
                            string dbunit = reader.IsDBNull(6) ? string.Empty : reader.GetString(6);
                            string dbcoefficient = reader.IsDBNull(12) ? string.Empty : reader.GetInt32(12).ToString();

                            var viewModel = new CartProductUserControlViewModel
                            {
                                ProductID = dbid,
                                Name = dbname,
                                Article = dbarticle,
                                PricePerUnit = dbpricePerUnit,
                                Unit = dbunit,
                                Coefficient = dbcoefficient
                            };

                            var userControl = new CartProductUserControl
                            {
                                DataContext = viewModel
                            };

                            _productList.Add(userControl);
                        }
                    }
                }
            }

            UpdateUI();

            if (_productList.Count == 0) ShowErrorUserControl(ErrorLevel.NotFound);
        }
        catch (NpgsqlException ex)
        {
            ClearResults();
            ShowErrorUserControl(ErrorLevel.NoConnectToDB);

            Loges.LoggingProcess(LogLevel.CRITICAL,
                "Connection or request error",
                ex: ex);
        }
    }

    private void ClearResults()
    {
        _productList.Clear();
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (MainContent != null)
        {
            MainContent.Children.Clear();
            foreach (CartProductUserControl item in _productList)
            {
                MainContent.Children.Add(item);
            }
        }
    }

    private void ShowErrorUserControl(ErrorLevel level)
    {
        if (MainContent != null)
        {
            var notFoundUserControlViewModel = new NotFoundUserControlViewModel(level);
            var notFoundUserControl = new NotFoundUserControl { DataContext = notFoundUserControlViewModel };

            if (MainContent.Children.Contains(notFoundUserControl)) return;

            MainContent.Children.Clear();
            MainContent.Children.Add(notFoundUserControl);
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
