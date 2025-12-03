namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class OpenOrCloseAddSubProductStatusMessage
{
    public bool ShouldOpen { get; }
    public double TaskId { get; }

    public OpenOrCloseAddSubProductStatusMessage(bool shouldOpen, double? taskId = null)
    {
        ShouldOpen = shouldOpen;
        TaskId = taskId ?? 0;
    }
}
