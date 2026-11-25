using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Primitives;

public class TokenOwnerForwardingTests
{
    static TokenOwnerForwardingTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Forwarding_Without_A_Resolvable_Owner_Leaves_The_Part_To_Inheritance()
    {
        // A host that has no owner and no registered identity has nothing to forward. The part must then keep the
        // owner it inherits from its own ancestors: a local null would block that inheritance.
        Dispatcher.UIThread.Invoke(() =>
        {
            var part = new Border();
            var button = new Button { Content = part };
            var window = new AvaloniaWindow { Content = button, Width = 200, Height = 100 };
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                button.ApplyTemplate();
                window.UpdateLayout();
                Dispatcher.UIThread.RunJobs();
                var inherited = ThemeScope.GetTokenOwner(part).ShouldNotBeNull("the part inherits the Button's injected owner");

                using var forwarding = TokenOwnerForwarding.Forward(new Border(), part);

                part.IsSet(ThemeScope.TokenOwnerProperty).ShouldBeFalse("no owner resolves: no local value may block inheritance");
                ThemeScope.GetTokenOwner(part).ShouldBe(inherited, "inheritance still reaches the part while forwarding is active");
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Forwarding_A_Registered_Host_Sets_The_Host_Identity_And_Disposing_Restores_Inheritance()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var part = new Border();
            var button = new Button { Content = part };
            var window = new AvaloniaWindow { Content = button, Width = 200, Height = 100 };
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                button.ApplyTemplate();
                window.UpdateLayout();
                Dispatcher.UIThread.RunJobs();
                var inherited = ThemeScope.GetTokenOwner(part).ShouldNotBeNull();

                // An unattached LineEdit host forwards its registered identity (the one its default theme would inject).
                var forwarding = TokenOwnerForwarding.Forward(new LineEdit(), part);
                var forwarded = ThemeScope.GetTokenOwner(part).ShouldNotBeNull();
                forwarded.ShouldNotBe(inherited);
                forwarded.Id.ShouldBe("LineEdit");

                forwarding.Dispose();
                ThemeScope.GetTokenOwner(part).ShouldBe(inherited, "disposing clears the forwarded value and inheritance returns");
            }
            finally
            {
                window.Close();
            }
        });
    }
}
