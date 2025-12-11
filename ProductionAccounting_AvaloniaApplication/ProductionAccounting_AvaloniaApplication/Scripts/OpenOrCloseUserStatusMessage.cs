namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class OpenOrCloseUserStatusMessage
{
    public bool ShouldOpen { get; }
    public double Id { get; }
    public string Login { get; }
    public string FirstUsername { get; }
    public string LastUsername { get; }
    public string MiddleName { get; }
    public decimal BaseSalary { get; }
    public string Email { get; }
    public string Phone { get; }

    public OpenOrCloseUserStatusMessage(bool shouldOpen, double? id = null, string? login = null, string? firstName = null, string? lastName = null, string? middleName = null, decimal? baseSalary = null, string? email = null, string? phone = null)
    {
        ShouldOpen = shouldOpen;
        Id = id ?? 0;
        Login = login ?? string.Empty;
        FirstUsername = firstName ?? string.Empty;
        LastUsername = lastName ?? string.Empty;
        MiddleName = middleName ?? string.Empty;
        BaseSalary = baseSalary ?? 1.0m;
        Email = email ?? string.Empty;
        Phone = phone ?? string.Empty;
    }
}
