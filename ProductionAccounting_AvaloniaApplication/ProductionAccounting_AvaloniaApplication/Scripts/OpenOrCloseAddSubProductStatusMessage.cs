namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class OpenOrCloseAddSubProductStatusMessage(bool shouldOpen, double? taskId = null)
{
    public bool ShouldOpen { get; } = shouldOpen;
    public double TaskId { get; } = taskId ?? 0;
}
