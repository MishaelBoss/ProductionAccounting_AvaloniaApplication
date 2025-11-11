using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.ComponentModel;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class AddDepartmentUserControlViewModel : ViewModelBase, INotifyPropertyChanged
{
    private string _messageerror = string.Empty;
    public string Messageerror
    {
        get => _messageerror;
        set => this.RaiseAndSetIfChanged(ref _messageerror, value);
    }

    private string _department = string.Empty;
    public string Department
    {
        get => _department;
        set 
        {
            if (_department != value) 
            {
                _department = value;
                OnPropertyChanged(nameof(Department));
                OnPropertyChanged(nameof(IsActiveConfirmButton));
            }
        }
    }

    public bool IsActiveConfirmButton 
        => !string.IsNullOrEmpty(Department);

    public bool Upload() {
        try
        {
            if (string.IsNullOrEmpty(Department))
            {
                Messageerror = "Не все поля заполнены";
                return false;
            }

            try
            {
                string sql = "INSERT INTO public.departments (type) VALUES (@type)";

                using (var connection = new NpgsqlConnection(Arguments.connection))
                {
                    connection.Open();
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        try
                        {
                            command.Parameters.AddWithValue("@type", Department);
                            command.ExecuteNonQuery();

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
                Messageerror = "Неизвестная ошибка";
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

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
