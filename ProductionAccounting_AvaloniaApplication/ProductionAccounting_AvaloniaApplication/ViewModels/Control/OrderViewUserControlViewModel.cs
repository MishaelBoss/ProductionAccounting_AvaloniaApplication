using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using JetBrains.Annotations;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.Views.Control;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using static ProductionAccounting_AvaloniaApplication.ViewModels.Control.NotFoundUserControlViewModel;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class OrderViewUserControlViewModel : ViewModelBase, IRecipient<RefreshSubProductListMessage>
{
    public string Name { get; }
    public double ProductId { get; }
    public string Mark { get; }
    public decimal Coefficient { get; }
    public string Notes { get; }
    public string Status { get; set; }

    private double? _subProductId = 0;
    public double? SubProductId
    {
        get => _subProductId;
        set => this.RaiseAndSetIfChanged(ref _subProductId, value);
    }

    private string? _subProductName = string.Empty;
    public string? SubProductName
    {
        get => _subProductName;
        set => this.RaiseAndSetIfChanged(ref _subProductName, value);
    }

    private decimal? _subProductPlannedQuantity = 0;
    public decimal? SubProductPlannedQuantity
    {
        get => _subProductPlannedQuantity;
        set => this.RaiseAndSetIfChanged(ref _subProductPlannedQuantity, value);
    }

    private decimal? _subProductPlannedWeight = 0;
    public decimal? SubProductPlannedWeight
    {
        get => _subProductPlannedWeight;
        set => this.RaiseAndSetIfChanged(ref _subProductPlannedWeight, value);
    }

    private string? _subProductNotes = string.Empty;
    public string? SubProductNotes
    {
        get => _subProductNotes;
        set => this.RaiseAndSetIfChanged(ref _subProductNotes, value);
    }

    public OrderViewUserControlViewModel(string name, double productId, string mark, decimal coefficient, string notes, string status)
    {
        Name = name;
        ProductId = productId;
        Mark = mark;
        Coefficient = coefficient;
        Notes = notes;
        Status = status;

        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    public void Receive(RefreshSubProductListMessage message)
    {
        _ = LoadSubProductAsync();
    }

    public StackPanel? SubProductContent { get; set; } = null;
    public StackPanel? SubProductOperation { get; set; } = null;

    private readonly List<CartSubProductUserControl> _subProductList = [];
    private readonly List<CartSubProductOperationUserControl> _subProductOperationList = [];

    public ICommand AddSubProductCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseAddSubProductStatusMessage(true, ProductId)));
    
    [UsedImplicitly]
    public static bool IsAdministratorOrMasterAndManager
        => ManagerCookie.IsUserLoggedIn();

    public bool IsAdministratorOrMasterAndCanCompleteTask
        => IsAdministratorOrMasterAndManager 
        && Status != "completed";

    public async Task LoadSubProductAsync()
    {
        StackPanelHelper.ClearAndRefreshStackPanel(SubProductContent, _subProductList);

        try
        {
            const string sql = "SELECT id, product_task_id, name, notes, tonnage, quantity, created_at FROM public.sub_products WHERE product_task_id = @product_task_id";

            await using (var connection = new NpgsqlConnection(Arguments.Connection))
            {
                await connection.OpenAsync();

                await using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("@product_task_id", ProductId);

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var viewModel = new CartSubProductUserControlViewModel(
                        id: reader.GetDouble(0),
                        productTaskId: reader.GetDouble(1),
                        name: reader.GetString(2),
                        notes: reader.GetString(3),
                        quantity: reader.GetDecimal(4),
                        tonnage: reader.GetDecimal(5),
                        createAt: reader.GetDateTime(6)
                    );

                    var userControl = new CartSubProductUserControl()
                    {
                        DataContext = viewModel,
                    };

                    _subProductList.Add(userControl);
                }
            }

            StackPanelHelper.RefreshStackPanelContent(SubProductContent, _subProductList);

            if (_subProductList.Count == 0) ItemNotFoundException.Show(SubProductContent, ErrorLevel.NotFound);
        }
        catch (NpgsqlException ex)
        {
            StackPanelHelper.ClearAndRefreshStackPanel(SubProductContent, _subProductList);
            ItemNotFoundException.Show(SubProductContent, ErrorLevel.NoConnectToDb);

            Loges.LoggingProcess(LogLevel.Error,
                "Connection or request error",
                ex: ex);
        }
        catch (Exception ex)
        {
            StackPanelHelper.ClearAndRefreshStackPanel(SubProductContent, _subProductList);
            ItemNotFoundException.Show(SubProductContent, ErrorLevel.NoConnectToDb);
            Loges.LoggingProcess(LogLevel.Error, ex: ex);
        }
    }
}