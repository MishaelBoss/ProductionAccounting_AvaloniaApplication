namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class OpenOrCloseAddDepartmentStatusMessage
{
    public bool ShouldOpen { get; }

    public OpenOrCloseAddDepartmentStatusMessage(bool shouldOpen)
    {
        ShouldOpen = shouldOpen;
    }
}
