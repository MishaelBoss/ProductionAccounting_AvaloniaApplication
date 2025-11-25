namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class OpenOrCloseAddPositionStatusMessage
{
    public bool ShouldOpen { get; }

    public OpenOrCloseAddPositionStatusMessage(bool shouldOpen)
    {
        ShouldOpen = shouldOpen;
    }
}
