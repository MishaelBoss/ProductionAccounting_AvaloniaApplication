using ProductionAccounting_AvaloniaApplication.Services.Interfaces;

namespace ProductionAccounting_AvaloniaApplication.Models;

public class ComboBoxTypeRolsUser(double id, string title) : IComboBoxTypeRolsUser
{
    public double Id { get; set; } = id;
    public string Title { get; set; } = title;

    public override string ToString()
        => Title;
}
