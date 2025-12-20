namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class OpenOrCloseProfileUserStatusMessage(bool shouldOpen, double? userId = null)
{
    public bool ShouldOpen { get; } = shouldOpen;
    public double UserId { get; } = userId ?? 0;
}
