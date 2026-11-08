using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AtomUIButton = AtomUI.Desktop.Controls.Button;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Buttons;

/// <summary>
/// Scope contracts of the Button family's code-side Token reads: variant colours resolve through the same resource keys
/// as the ControlTheme (so Application and scoped resources apply), DropdownButton reads its own Effective Global Tokens,
/// and the FontWeight and motion switches follow the member's configuration.
/// </summary>
[Collection(ButtonThemeScopeTestCollection.Name)]
public class ButtonFamilyTokenScopeTests
{
    private static readonly Color Override = Color.Parse("#00b96b");
    private static readonly Color UpdatedOverride = Color.Parse("#ff4d4f");

    static ButtonFamilyTokenScopeTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Scoped_Own_Token_Resource_Reaches_The_Variant_Background_And_Follows_Changes()
    {
        var button = new AtomUIButton { Content = "Scoped", IsMotionEnabled = false };
        var host   = new Border { Child = button };
        host.Resources[ButtonTokenKind.DefaultBg] = new SolidColorBrush(Override);

        Show(host, () =>
        {
            BrushColor(button.Background).ShouldBe(Override);

            host.Resources[ButtonTokenKind.DefaultBg] = new SolidColorBrush(UpdatedOverride);
            Flush();

            BrushColor(button.Background).ShouldBe(UpdatedOverride);
        });
    }

    [Fact]
    public void Scoped_Effective_Global_Resource_Reaches_The_Primary_Variant()
    {
        var button = new AtomUIButton { Content = "Scoped", ButtonType = ButtonType.Primary, IsMotionEnabled = false };
        var host   = new Border { Child = button };
        host.Resources[ControlTokenResourceKey.Global(ButtonTokens.Identity, SharedTokenKind.ColorPrimary)] =
            new SolidColorBrush(Override);

        Show(host, () => BrushColor(button.Background).ShouldBe(Override));
    }

    [Fact]
    public void DropdownButton_Variant_Colours_Follow_DropdownButton_Not_Button()
    {
        var dropdown = new DropdownButton { Content = "Dropdown", ButtonType = ButtonType.Primary, IsMotionEnabled = false };
        var provider = Provider(dropdown, (DropdownButtonTokens.Identity, "ColorPrimary", "#00b96b"));

        Show(provider, () =>
        {
            BrushColor(dropdown.Background).ShouldBe(Override);

            provider.Config = Config((ButtonTokens.Identity, "ColorPrimary", "#ff4d4f"));
            Flush();

            BrushColor(dropdown.Background).ShouldNotBe(UpdatedOverride);
            BrushColor(dropdown.Background).ShouldNotBe(Override);
        });
    }

    [Fact]
    public void DropdownButton_Motion_Switch_Reads_The_DropdownButton_Effective_Global()
    {
        var dropdown = new DropdownButton { Content = "Dropdown" };
        var button   = new AtomUIButton { Content = "Button" };
        var provider = Provider(
            new StackPanel { Children = { dropdown, button } },
            (DropdownButtonTokens.Identity, "EnableMotion", "false"));

        Show(provider, () =>
        {
            dropdown.IsMotionEnabled.ShouldBeFalse();
            button.IsMotionEnabled.ShouldBeTrue();

            provider.Config = Config((ButtonTokens.Identity, "EnableMotion", "false"));
            Flush();

            dropdown.IsMotionEnabled.ShouldBeTrue();
            button.IsMotionEnabled.ShouldBeFalse();
        });
    }

    [Fact]
    public void Button_FontWeight_Follows_The_Own_Token()
    {
        var button   = new AtomUIButton { Content = "Weight" };
        var provider = Provider(button, (ButtonTokens.Identity, "FontWeight", "500"));

        Show(provider, () =>
        {
            button.FontWeight.ShouldBe(FontWeight.Medium);

            provider.Config = Config();
            Flush();

            button.FontWeight.ShouldBe(FontWeight.Normal);
        });
    }

    [Fact]
    public void Local_FontWeight_Wins_Over_The_Own_Token()
    {
        var button   = new AtomUIButton { Content = "Weight", FontWeight = FontWeight.Bold };
        var provider = Provider(button, (ButtonTokens.Identity, "FontWeight", "500"));

        Show(provider, () => button.FontWeight.ShouldBe(FontWeight.Bold));
    }

    private static ThemeConfigProvider Provider(
        Control content,
        params (ControlTokenIdentity Identity, string Token, string Value)[] entries)
    {
        var provider = new ThemeConfigProvider { Child = content, Config = Config(entries) };
        provider.ThemeChangeFailed += static (_, args) =>
            throw new InvalidOperationException(args.Exception?.Message ??
                                                string.Join("; ", args.Diagnostics.Select(d => d.Message)));
        return provider;
    }

    private static ThemeConfig Config(params (ControlTokenIdentity Identity, string Token, string Value)[] entries)
    {
        var builder = new ThemeConfigBuilder();
        foreach (var group in entries.GroupBy(static entry => entry.Identity))
        {
            var control = new ControlThemeConfigBuilder();
            foreach (var entry in group)
            {
                control.WithToken(entry.Token, entry.Value);
            }

            builder.WithControl(group.Key, control.Build());
        }

        return builder.Build();
    }

    private static void Show(Control content, Action assertion)
    {
        var window = new AvaloniaWindow { Width = 400, Height = 300, Content = content };
        try
        {
            window.Show();
            Flush();
            assertion();
        }
        finally
        {
            window.Close();
        }
    }

    private static void Flush()
    {
        for (var i = 0; i < 5; i++)
        {
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static Color? BrushColor(IBrush? brush) => (brush as ISolidColorBrush)?.Color;
}
