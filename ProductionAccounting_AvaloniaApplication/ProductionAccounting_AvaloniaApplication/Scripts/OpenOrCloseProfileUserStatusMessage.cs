namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class OpenOrCloseProfileUserStatusMessage
{
    public bool ShouldOpen { get; }
    public double UserId { get; }

    public OpenOrCloseProfileUserStatusMessage(bool shouldOpen, double? userId = null)
    {
        ShouldOpen = shouldOpen;
        UserId = userId ?? 0;
    }
}
