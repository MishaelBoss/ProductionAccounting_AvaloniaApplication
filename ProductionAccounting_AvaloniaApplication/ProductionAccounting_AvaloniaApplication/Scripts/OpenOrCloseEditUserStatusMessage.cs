namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class OpenOrCloseEditUserStatusMessage
{    
    public bool ShouldOpen { get; }
    public double UserId { get; }

    public OpenOrCloseEditUserStatusMessage(bool shouldOpen, double? userId = null)
    {
        ShouldOpen = shouldOpen;
        UserId = userId ?? 0;
    }
}