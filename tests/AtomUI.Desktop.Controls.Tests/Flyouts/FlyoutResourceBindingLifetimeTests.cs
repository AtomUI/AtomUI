using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Flyouts;

public class FlyoutResourceBindingLifetimeTests
{
    [Fact]
    public void Flyout_Does_Not_Discard_Global_Token_Binding_Disposables()
    {
        var source = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Flyouts/Flyout.cs"));

        Regex.IsMatch(source, @"^\s*TokenResourceBinder\.CreateGlobalTokenBinding\(this,.*;\s*$", RegexOptions.Multiline)
             .ShouldBeFalse("global resource bindings subscribe to application styles and must have a release path");
    }

    [Fact]
    public void Reusable_Flyout_Does_Not_Keep_A_Retired_Owner_Alive()
    {
        AvaloniaTestApp.EnsureInitialized();
        var window = new AtomUI.Desktop.Controls.Window { Width = 700, Height = 600 };
        window.Show();
        var retired = RetireOwner(window);
        try
        {
            for (var i = 0; i < 5; i++)
            {
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                Dispatcher.UIThread.RunJobs();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            retired.Owner.IsAlive.ShouldBeFalse("a reusable Flyout must release the retired owner's positioning context");
            GC.KeepAlive(retired.Flyout);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (Flyout Flyout, WeakReference Owner) RetireOwner(AtomUI.Desktop.Controls.Window window)
    {
        var flyout = new Flyout { Content = new Border { Width = 120, Height = 80 } };
        var owner = new FlyoutHost { Flyout = flyout, IsMotionEnabled = false, Width = 160, Height = 40 };
        window.Content = owner;
        Dispatcher.UIThread.RunJobs();
        flyout.ShowAt(owner);
        Dispatcher.UIThread.RunJobs();
        flyout.Hide();
        Dispatcher.UIThread.RunJobs();
        window.Content = new Border();
        Dispatcher.UIThread.RunJobs();
        return (flyout, new WeakReference(owner));
    }

    private static string GetRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not find repository file: {relativePath}");
    }
}
