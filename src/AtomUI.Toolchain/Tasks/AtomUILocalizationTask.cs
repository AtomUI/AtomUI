using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

public abstract class AtomUILocalizationTask : ITask
{
    public IBuildEngine BuildEngine { get; set; } = null!;

    public ITaskHost HostObject { get; set; } = null!;

    public abstract bool Execute();

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
