namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class OpenOrCloseAddUserStatusMessage
{
    public bool ShouldOpen { get; }

    public OpenOrCloseAddUserStatusMessage(bool shouldOpen)
    {
        ShouldOpen = shouldOpen;
    }
}
