using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class AddOrEditWorkTypeUserControlViewModel : ViewModelBase
{
    private readonly double? Id;

    private string _messageerror = string.Empty;
    [UsedImplicitly]
    public string Messageerror
    {
        get => _messageerror;
        set => this.RaiseAndSetIfChanged(ref _messageerror, value);
    }

    private string _type;
    [UsedImplicitly]
    public string Type
    {
        get => _type;
        set => SetAndNotify(ref _type, value);
    }

    private decimal _coefficient;
    [UsedImplicitly]
    public decimal Coefficient
    {
        get => _coefficient;
        set => SetAndNotify(ref _coefficient, value);
    }

    [UsedImplicitly]
    public List<string> MeasurementUnits { get; } =
    [
        "кг",
        "шт"
    ];

    private string? _selectMeasurementUnit;
    [UsedImplicitly]
    public string? SelectMeasurementUnit
    {
        get => _selectMeasurementUnit;
        set => SetAndNotify(ref _selectMeasurementUnit, value);
    }

    private decimal _rate;
    [UsedImplicitly]
    public decimal Rate
    {
        get => _rate;
        set => SetAndNotify(ref _rate, value);
    }

    public AddOrEditWorkTypeUserControlViewModel(double? id = null, string? type = null, decimal? coefficient = null, string? measurement = null, decimal? rate = null)
    {
        Id = id;
        _type = type ?? string.Empty;
        _coefficient = coefficient ?? 0m;
        _rate = rate ?? 0m;

        if (!string.IsNullOrEmpty(measurement)) _selectMeasurementUnit = MeasurementUnits.Find(x => x == measurement) ?? MeasurementUnits[0];
        else _selectMeasurementUnit = MeasurementUnits[0];
    }

    public ICommand ConfirmCommand
        => new RelayCommand(() => _ = SaveAsync());

    public ICommand CancelCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseWorkTypeStatusMessage(false)));

    private void SetAndNotify<T>(ref T field, T value)
    {
        this.RaiseAndSetIfChanged(ref field, value);
        this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
    }

    private bool DesignedFields
        => !string.IsNullOrWhiteSpace(Type)
        && SelectMeasurementUnit != null
        && Coefficient >= 0.1m
        && Coefficient <= 1
        && Rate > 0;

    public bool IsActiveConfirmButton
        => DesignedFields;

    private async Task SaveAsync() 
    {
        if (!DesignedFields) 
        {
            Messageerror = "Не все поля заполнены";
            return;
        }

        const string addWorkType = "INSERT INTO public.work_type (type, coefficient, measurement, rate) VALUES (@type, @coefficient, @measurement, @rate)";
        const string updateWorkType = "UPDATE public.work_type SET type=@type, coefficient=@coefficient, measurement=@measurement, rate=@rate WHERE id = @id";

        var sql = (Id != null && Id != 0) ? updateWorkType : addWorkType;

        try
        {
            await using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("@type", Type);
            command.Parameters.AddWithValue("@coefficient", Coefficient);
            command.Parameters.AddWithValue("@measurement", SelectMeasurementUnit ?? MeasurementUnits[0]);
            command.Parameters.AddWithValue("@rate", Rate);

            if (Id != null && Id != 0) command.Parameters.AddWithValue("@id", Id);

            await command.ExecuteNonQueryAsync();

            WeakReferenceMessenger.Default.Send(new RefreshWorkTypeListMessage());
            WeakReferenceMessenger.Default.Send(new OpenOrCloseWorkTypeStatusMessage(false));

            ClearForm();
        }
        catch (Exception ex) 
        {
            Messageerror = "Что то пошло не так";
            Loges.LoggingProcess(level: LogLevel.Error, 
                ex: ex);
        }
    }

    private void ClearForm() 
    {
        Messageerror = string.Empty;
        Type = string.Empty;
        Coefficient = 0.1m;
        SelectMeasurementUnit = MeasurementUnits[0];
        Rate = 0;
    }
}
