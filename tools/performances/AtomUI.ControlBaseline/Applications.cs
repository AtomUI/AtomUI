using Avalonia;
using Avalonia.Themes.Fluent;

namespace AtomUI.ControlBaseline;

// The generator completes the AtomUI service registration hook on the partial application class.
public sealed partial class AtomUIBaselineApplication : Application;

// The stock reference application must stay free of the AtomUI generated startup hooks.
public sealed class FluentBaselineApplication : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
    }
}
