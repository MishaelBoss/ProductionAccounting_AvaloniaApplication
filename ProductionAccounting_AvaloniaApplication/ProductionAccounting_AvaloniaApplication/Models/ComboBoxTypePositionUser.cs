using ProductionAccounting_AvaloniaApplication.Services.Interfaces;

namespace ProductionAccounting_AvaloniaApplication.Models;

public class ComboBoxTypePositionUser(double id, string title) : IComboBoxTypePositionUser
{
    public double Id { get; set; } = id;
    public string Title { get; set; } = title;

    public override string ToString()
        => Title;
}
