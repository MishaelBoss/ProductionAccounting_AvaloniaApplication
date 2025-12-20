namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class OpenOrCloseUserStatusMessage(bool shouldOpen, double? id = null, string? login = null, string? firstName = null, string? lastName = null, string? middleName = null, decimal? baseSalary = null, string? email = null, string? phone = null)
{
    public bool ShouldOpen { get; } = shouldOpen;
    public double Id { get; } = id ?? 0;
    public string Login { get; } = login ?? string.Empty;
    public string FirstUsername { get; } = firstName ?? string.Empty;
    public string LastUsername { get; } = lastName ?? string.Empty;
    public string MiddleName { get; } = middleName ?? string.Empty;
    public decimal BaseSalary { get; } = baseSalary ?? 1.0m;
    public string Email { get; } = email ?? string.Empty;
    public string Phone { get; } = phone ?? string.Empty;
}
