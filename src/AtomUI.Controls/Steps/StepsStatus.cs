// Keep the CLR name used by existing Desktop clients.
namespace AtomUI.Desktop.Controls;

/// <summary>
/// Status of a step in a steps control. It decides the look of the step indicator and of the title of the step.
/// </summary>
public enum StepsStatus
{
    /// <summary>
    /// The step has not started yet. The steps control gives this status to the steps after the current one.
    /// </summary>
    Wait,
    /// <summary>
    /// The step is in progress. The steps control gives this status to the current step by default.
    /// </summary>
    Process,
    /// <summary>
    /// The step is done. The steps control gives this status to the steps before the current one.
    /// </summary>
    Finish,
    /// <summary>
    /// The step failed. The current step gets this status when the status of the steps control is set to it.
    /// </summary>
    Error
}
