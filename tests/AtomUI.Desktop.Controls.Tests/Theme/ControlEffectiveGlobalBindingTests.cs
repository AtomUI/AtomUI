using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.DesignTokens;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIDrawer = AtomUI.Desktop.Controls.Drawer;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Theme;

[Collection(ControlEffectiveGlobalBindingTestCollection.Name)]
public class ControlEffectiveGlobalBindingTests
{
    static ControlEffectiveGlobalBindingTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Motion_Binding_Uses_The_Control_Effective_Global_Token()
    {
        var drawer = new AtomUIDrawer();
        var provider = new ThemeConfigProvider
        {
            Child = drawer,
            Config = new ThemeConfigBuilder()
                     .WithToken(nameof(DesignToken.EnableMotion), "false")
                     .WithControl(
                         DrawerTokens.Identity,
                         new ControlThemeConfigBuilder()
                             .WithAlgorithm(ControlAlgorithmMode.Disabled)
                             .WithToken(nameof(DesignToken.EnableMotion), "true")
                             .Build())
                     .Build()
        };
        provider.ThemeChangeFailed += static (_, args) =>
        {
            throw new InvalidOperationException(args.Exception?.Message ?? "Theme compilation failed.");
        };

        ShowInWindow(provider, () => drawer.IsMotionEnabled.ShouldBeTrue());
    }

    [Fact]
    public void Internal_Part_Binding_Uses_The_Public_Control_Effective_Global_Token()
    {
        // The time view is an identity-less part of the picker popup: its separator width reads the hosting picker's
        // effective LineWidth (the picker forwards its Token owner to the popup presenter).
        var picker = new TimePicker { IsPopupPinnedOpen = true };
        // The popup opens in the overlay layer of the layer manager (the plain test window has none).
        var layers = new Avalonia.Controls.Primitives.VisualLayerManager { Child = picker };
        typeof(Avalonia.Controls.Primitives.VisualLayerManager)
            .GetProperty("EnablePopupOverlayLayer",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .SetValue(layers, true);
        var provider = new ThemeConfigProvider
        {
            Child = layers,
            Config = new ThemeConfigBuilder()
                     .WithToken(nameof(DesignToken.LineWidth), "1")
                     .WithControl(
                         TimePickerTokens.Identity,
                         new ControlThemeConfigBuilder()
                             .WithAlgorithm(ControlAlgorithmMode.Disabled)
                             .WithToken(nameof(DesignToken.LineWidth), "4")
                             .Build())
                     .Build()
        };
        provider.ThemeChangeFailed += static (_, args) =>
        {
            throw new InvalidOperationException(args.Exception?.Message ?? "Theme compilation failed.");
        };

        ShowInWindow(provider, () =>
        {
            var timeView = TopLevel.GetTopLevel(picker)!.GetVisualDescendants().OfType<TimeView>().Single();
            timeView.SpacerWidth.ShouldBe(4d);
        });
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width = 240,
            Height = 160,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
        }
    }
}

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class ControlEffectiveGlobalBindingTestCollection
{
    public const string Name = "ControlEffectiveGlobalBinding";
}
