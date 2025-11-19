using ProductionAccounting_AvaloniaApplication.Services.Interfaces;

namespace ProductionAccounting_AvaloniaApplication.Models;

public class TimesheetStatus : ITimesheetStatus
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;

    public override string ToString() 
        => $"{Code} - {Name}";
}
