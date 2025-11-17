using ProductionAccounting_AvaloniaApplication.Services.Interfaces;

namespace ProductionAccounting_AvaloniaApplication.Models;

public class ComboBoxUserAddWork(double id, string firstName, string lastName, string MiddleName, string login) : IComboBoxUserAddWork
{
    public double Id { get; set; } = id;
    public string FirstName { get; set; } = firstName;
    public string LastName { get; set; } = lastName;
    public string MiddleName { get; set; } = MiddleName;
    public string Login { get; set; } = login;

    public string FullName
        => $"{FirstName} {LastName} {MiddleName}" + Login;

/*    public override string ToString()
        => $"{FirstName} {LastName} {MiddleName}" + Login;*/
}
