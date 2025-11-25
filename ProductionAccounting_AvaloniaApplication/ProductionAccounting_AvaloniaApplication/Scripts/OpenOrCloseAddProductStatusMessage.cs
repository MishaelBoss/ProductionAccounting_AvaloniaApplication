namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class OpenOrCloseAddProductStatusMessage
{
    public bool ShouldOpen { get; }

    public OpenOrCloseAddProductStatusMessage(bool shouldOpen)
    {
        ShouldOpen = shouldOpen;
    }
}
