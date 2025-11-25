namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class OpenOrCloseAddOperationStatusMessage
{
    public bool ShouldOpen { get; }

    public OpenOrCloseAddOperationStatusMessage(bool shouldOpen)
    {
        ShouldOpen = shouldOpen;
    }
}
