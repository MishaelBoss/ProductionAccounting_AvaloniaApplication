using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class AddSubProductUserControlViewModel : ViewModelBase, INotifyPropertyChanged
{
    public AddSubProductUserControlViewModel(double taskId)
    {
        TaskId = taskId;
    }

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

    public ICommand CancelCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseAddSubProductStatusMessage(false)));

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

                    //await LoadSubProductAsync();

                    WeakReferenceMessenger.Default.Send(new RefreshSubProductListMessage());
                    WeakReferenceMessenger.Default.Send(new OpenOrCloseAddSubProductStatusMessage(false));

                    ClearForm();
                }
            }
        }
        catch (Exception ex)
        {
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
