using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.Views.Control;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using static ProductionAccounting_AvaloniaApplication.ViewModels.Control.NotFoundUserControlViewModel;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class TaskDetailUserControlViewModel : ViewModelBase, INotifyPropertyChanged, IRecipient<RefreshSubProductListMessage>
{
    public TaskDetailUserControlViewModel(double taskId)
    {
        TaskId = taskId;

        WeakReferenceMessenger.Default.Register(this);
    }

    public void Receive(RefreshSubProductListMessage message)
    {
        _ = LoadSubProductAsync();
    }

    public StackPanel? HomeMainContent { get; set; } = null;

    private List<CartSubProductUserControl> subProductList = [];

    public double TaskId { get; }

    private string? _title = string.Empty;
    public string? Title 
    {
        get => _title;
        set 
        {
            if (_title != value) 
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(CanSaveCurrentSubProduct));
            }
        }
    }

    private decimal? _plannedQuantity = 1;
    public decimal? PlannedQuantity
    {
        get => _plannedQuantity;
        set
        {
            if (_plannedQuantity != value)
            {
                _plannedQuantity = value;
                OnPropertyChanged(nameof(PlannedQuantity));
                OnPropertyChanged(nameof(CanSaveCurrentSubProduct));
            }
        }
    }

    private string? _notes = string.Empty;
    public string? Notes
    {
        get => _notes;
        set
        {
            if (_notes != value)
            {
                _notes = value;
                OnPropertyChanged(nameof(Notes));
                OnPropertyChanged(nameof(CanSaveCurrentSubProduct));
            }
        }
    }

    public bool CanSaveCurrentSubProduct
        => !string.IsNullOrWhiteSpace(Title) && PlannedQuantity > 0;

    public ICommand SaveCurrentSubProductCommand
        => new RelayCommand(async () => await SaveCurrentSubProductAsync());

    private async Task SaveCurrentSubProductAsync()
    {
        try
        {
            string sql = "INSERT INTO public.sub_products (product_task_id, name, planned_quantity, notes) VALUES (@task_id, @name, @qty, @notes)";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand(sql, connection)) 
                {
                    command.Parameters.AddWithValue("@task_id", TaskId);
                    command.Parameters.AddWithValue("@name", Title ?? string.Empty);
                    command.Parameters.AddWithValue("@qty", PlannedQuantity ?? 1);
                    command.Parameters.AddWithValue("@notes", Notes ?? "");

                    await command.ExecuteNonQueryAsync();

                    await LoadSubProductAsync();

                    ClearForm();
                }
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.ERROR, ex: ex);
        }
    }

    public async Task LoadSubProductAsync() 
    {
        StackPanelHelper.ClearAndRefreshStackPanel<CartSubProductUserControl>(HomeMainContent, subProductList);

        try
        {
            string sql = "SELECT id, product_task_id, name, planned_quantity, planned_weight, notes, created_at FROM public.sub_products WHERE product_task_id = @product_task_id";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@product_task_id", TaskId);

                    using (var reader = await command.ExecuteReaderAsync()) 
                    {
                        while (await reader.ReadAsync()) 
                        {
                            var viewModel = new CartSubProductUserControlViewModel()
                            {
                                Id = reader.GetDouble(0),
                                ProductTaskId = reader.GetDouble(1),
                                Name = reader.GetString(2),
                                PlannedQuantity = reader.GetDouble(3),
                                PlannedWeight = reader.GetDouble(4),
                                Notes = reader.GetString(5),
                            };

                            var userControl = new CartSubProductUserControl()
                            {
                                DataContext = viewModel,
                            };

                            subProductList.Add(userControl);
                        }
                    }
                }
            }

            StackPanelHelper.RefreshStackPanelContent<CartSubProductUserControl>(HomeMainContent, subProductList);

            if (subProductList.Count == 0) ItemNotFoundException.Show(HomeMainContent, ErrorLevel.NotFound);
        }
        catch (Exception ex)
        {
            StackPanelHelper.ClearAndRefreshStackPanel<CartSubProductUserControl>(HomeMainContent, subProductList);
            ItemNotFoundException.Show(HomeMainContent, ErrorLevel.NoConnectToDB);
            Loges.LoggingProcess(LogLevel.ERROR, ex: ex);
        }
    }

    private void ClearForm() 
    {
        Title = string.Empty;
        PlannedQuantity = 1;
        Notes = string.Empty;
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
