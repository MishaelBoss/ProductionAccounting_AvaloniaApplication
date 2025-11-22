namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class OpenOrCloseStatusMessage
{    
    public bool ShouldOpen { get; }
    public double UserId { get; }

    public OpenOrCloseStatusMessage(bool shouldOpen, double? userId = null)
    {
        ShouldOpen = shouldOpen;
        UserId = userId ?? 0;
    }
}