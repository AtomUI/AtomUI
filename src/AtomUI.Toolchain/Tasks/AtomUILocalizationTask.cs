using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

/// <summary>
/// Base class of the MSBuild tasks that export language templates and prepare language packages. It supplies the
/// build engine and logs errors and warnings under the <c>Localization</c> subcategory.
/// </summary>
public abstract class AtomUILocalizationTask : ITask
{
    /// <summary>
    /// The build engine that receives the messages and diagnostics the task logs. The build host assigns it before
    /// <see cref="Execute" /> runs.
    /// </summary>
    public IBuildEngine BuildEngine { get; set; } = null!;

    /// <summary>
    /// The host object MSBuild passes to the task. It satisfies <see cref="ITask" />; no localization task reads it.
    /// </summary>
    public ITaskHost HostObject { get; set; } = null!;

    /// <summary>
    /// Runs the task.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> on success; <see langword="false" /> when the task failed and logged the reason.
    /// </returns>
    public abstract bool Execute();

    /// <summary>
    /// Logs a build error under the <c>Localization</c> subcategory, attributed to the runtime type of the task.
    /// </summary>
    /// <param name="code">
    /// The diagnostic code, such as <c>ATOMUILOC005</c>.
    /// </param>
    /// <param name="file">
    /// The file the error belongs to, or an empty string when no file applies.
    /// </param>
    /// <param name="line">
    /// The line of the error, used as both its start and end line. Pass 1 when the error has no position.
    /// </param>
    /// <param name="column">
    /// The column of the error, used as both its start and end column. Pass 1 when the error has no position.
    /// </param>
    /// <param name="message">
    /// The message shown for the error.
    /// </param>
    protected void LogError(
        string code,
        string file,
        int line,
        int column,
        string message)
    {
        BuildEngine.LogErrorEvent(new BuildErrorEventArgs(
            subcategory: "Localization",
            code,
            file,
            line,
            column,
            line,
            column,
            message,
            helpKeyword: null,
            senderName: GetType().Name));
    }

    /// <summary>
    /// Logs a build warning under the <c>Localization</c> subcategory, attributed to the runtime type of the task.
    /// </summary>
    /// <param name="code">
    /// The diagnostic code of the warning.
    /// </param>
    /// <param name="file">
    /// The file the warning belongs to, or an empty string when no file applies.
    /// </param>
    /// <param name="line">
    /// The line of the warning, used as both its start and end line. Pass 1 when the warning has no position.
    /// </param>
    /// <param name="column">
    /// The column of the warning, used as both its start and end column. Pass 1 when the warning has no position.
    /// </param>
    /// <param name="message">
    /// The message shown for the warning.
    /// </param>
    protected void LogWarning(
        string code,
        string file,
        int line,
        int column,
        string message)
    {
        BuildEngine.LogWarningEvent(new BuildWarningEventArgs(
            subcategory: "Localization",
            code,
            file,
            line,
            column,
            line,
            column,
            message,
            helpKeyword: null,
            senderName: GetType().Name));
    }
}
