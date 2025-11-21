namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class OpenEditStatusMessage
{    
    public bool ShouldOpen { get; }
    public double UserId { get; }

    public OpenEditStatusMessage(bool shouldOpen, double? userId = null)
    {
        ShouldOpen = shouldOpen;
        UserId = userId ?? 0;
    }
}