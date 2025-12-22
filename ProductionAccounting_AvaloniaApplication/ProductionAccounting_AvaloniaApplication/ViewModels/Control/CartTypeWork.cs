using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProductionAccounting_AvaloniaApplication.Scripts;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class CartTypeWork(double id, string type, decimal coefficient, string measurement, decimal rate) : ViewModelBase
{
    public double Id { get; } = id;
    public string Type { get; } = type;
    public decimal Coefficient { get; } = coefficient;
    public string Measurement { get; } = measurement;
    public decimal Rate { get; } = rate;

    public ICommand EditCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseWorkTypeStatusMessage(true, id, type, coefficient, measurement, rate)));

    public ICommand DeleteCommand
        => new RelayCommand(() => {
            var viewModel = new ConfirmDeleteWindowViewModel(Id, Type, "DELETE FROM public.work_type WHERE id = @id", () => WeakReferenceMessenger.Default.Send(new RefreshWorkTypeListMessage()));

            var window = new ConfirmDeleteWindow()
            {
                DataContext = viewModel,
            };

            viewModel.SetWindow(window);

            window.Show();
        });
}
