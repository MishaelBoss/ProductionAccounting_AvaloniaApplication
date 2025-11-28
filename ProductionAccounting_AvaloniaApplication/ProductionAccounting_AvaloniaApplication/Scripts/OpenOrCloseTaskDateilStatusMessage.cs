namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class OpenOrCloseTaskDateilStatusMessage
{
    public bool ShouldOpen { get; }
    public double TaskId { get; }

    public OpenOrCloseTaskDateilStatusMessage(bool shouldOpen, double? taskId = null)
    {
        ShouldOpen = shouldOpen;
        TaskId = taskId ?? 0;
    }
}
