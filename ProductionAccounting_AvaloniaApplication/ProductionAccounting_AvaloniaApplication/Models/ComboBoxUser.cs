using ProductionAccounting_AvaloniaApplication.Services.Interfaces;

namespace ProductionAccounting_AvaloniaApplication.Models;

public class ComboBoxUser(double id, string firstName, string lastName, string MiddleName, string login) : IComboBoxUser
{
    public double Id { get; set; } = id;
    public string FirstName { get; set; } = firstName;
    public string LastName { get; set; } = lastName;
    public string MiddleName { get; set; } = MiddleName;
    public string Login { get; set; } = login;

    public string FullName
        => $"{FirstName} {LastName} {MiddleName} ({Login})";
}
