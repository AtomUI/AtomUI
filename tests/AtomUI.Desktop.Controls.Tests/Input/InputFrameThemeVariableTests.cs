using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUILineEdit = AtomUI.Desktop.Controls.LineEdit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Input;

/// <summary>
/// The input frame's variant x status surface values are theme variables read from the owner's effective Tokens; these
/// tests pin that scoped configuration, per-control overrides and runtime changes reach every state.
/// </summary>
public class InputFrameThemeVariableTests
{
    static InputFrameThemeVariableTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Scoped_Config_Reaches_Each_Status_And_Interaction_State()
    {
        var lineEdit = new AtomUILineEdit { Width = 180, IsMotionEnabled = false };
        var provider = new ThemeConfigProvider
        {
            Config = new ThemeConfigBuilder()
                     .WithToken(nameof(SharedTokenKind.ColorError), "#722ed1")
                     .WithToken(nameof(SharedTokenKind.ColorErrorBorderHover), "#9254de")
                     .WithToken(nameof(SharedTokenKind.ColorWarning), "#13c2c2")
                     .WithToken(nameof(SharedTokenKind.ColorWarningBorderHover), "#36cfc9")
                     .Build(),
            Child = lineEdit
        };

        ShowInWindow(provider, () =>
        {
            var frame = FindFrame(lineEdit);
            var surface = FindSurface(lineEdit);

            lineEdit.Status = InputControlStatus.Error;
            Dispatcher.UIThread.RunJobs();
            ColorOf(surface.BorderBrush).ShouldBe(Resource(lineEdit, SharedTokenKind.ColorError));
            ColorOf(surface.BorderBrush).ShouldBe(Color.Parse("#722ed1"));

            frame.IsInnerBoxHover = true;
            Dispatcher.UIThread.RunJobs();
            ColorOf(surface.BorderBrush).ShouldBe(Color.Parse("#9254de"));

            lineEdit.Status = InputControlStatus.Warning;
            Dispatcher.UIThread.RunJobs();
            ColorOf(surface.BorderBrush).ShouldBe(Color.Parse("#36cfc9"));

            frame.IsInnerBoxHover = false;
            Dispatcher.UIThread.RunJobs();
            ColorOf(surface.BorderBrush).ShouldBe(Color.Parse("#13c2c2"));

            lineEdit.Status = InputControlStatus.Default;
            Dispatcher.UIThread.RunJobs();
            ColorOf(surface.BorderBrush).ShouldBe(Resource(lineEdit, SharedTokenKind.ColorBorder));
        });
    }

    [Fact]
    public void Per_Control_Override_Reaches_Filled_Status_Backgrounds()
    {
        var lineEdit = new AtomUILineEdit
        {
            Width           = 180,
            IsMotionEnabled = false,
            StyleVariant    = InputControlStyleVariant.Filled,
            Status          = InputControlStatus.Error
        };
        var control = new ControlThemeConfigBuilder()
                      .WithToken(nameof(SharedTokenKind.ColorErrorBg), "#f9f0ff")
                      .WithToken(nameof(SharedTokenKind.ColorErrorBgHover), "#efdbff")
                      .Build();
        var provider = new ThemeConfigProvider
        {
            Config = new ThemeConfigBuilder().WithControl(LineEditTokens.Identity, control).Build(),
            Child  = lineEdit
        };

        ShowInWindow(provider, () =>
        {
            var frame = FindFrame(lineEdit);
            var surface = FindSurface(lineEdit);

            ColorOf(surface.Background).ShouldBe(Color.Parse("#f9f0ff"));
            ColorOf(surface.BorderBrush).ShouldBe(Color.Parse("#f9f0ff"));

            frame.IsInnerBoxHover = true;
            Dispatcher.UIThread.RunJobs();
            ColorOf(surface.Background).ShouldBe(Color.Parse("#efdbff"));

            frame.IsInputFocusWithin = true;
            Dispatcher.UIThread.RunJobs();
            ColorOf(surface.Background).ShouldBe(Colors.Transparent);
            ColorOf(surface.BorderBrush).ShouldBe(Resource(lineEdit, SharedTokenKind.ColorError));
        });
    }

    [Fact]
    public void Replacing_Scoped_Config_Updates_Current_State()
    {
        var lineEdit = new AtomUILineEdit { Width = 180, IsMotionEnabled = false };
        var provider = new ThemeConfigProvider
        {
            Config = new ThemeConfigBuilder().WithToken(nameof(SharedTokenKind.ColorPrimary), "#eb2f96").Build(),
            Child  = lineEdit
        };

        ShowInWindow(provider, () =>
        {
            var frame = FindFrame(lineEdit);
            var surface = FindSurface(lineEdit);
            frame.IsInputFocusWithin = true;
            Dispatcher.UIThread.RunJobs();
            ColorOf(surface.BorderBrush).ShouldBe(Resource(lineEdit, SharedTokenKind.ColorPrimary));
            var first = ColorOf(surface.BorderBrush);

            provider.Config = new ThemeConfigBuilder().WithToken(nameof(SharedTokenKind.ColorPrimary), "#52c41a").Build();
            Dispatcher.UIThread.RunJobs();

            ColorOf(surface.BorderBrush).ShouldNotBe(first);
            ColorOf(surface.BorderBrush).ShouldBe(Resource(lineEdit, SharedTokenKind.ColorPrimary));
        });
    }

    [Fact]
    public void Variant_Switch_Rebinds_Only_The_Variables_The_New_Variant_Reads()
    {
        var lineEdit = new AtomUILineEdit { Width = 180, IsMotionEnabled = false };

        ShowInWindow(lineEdit, () =>
        {
            var frame = FindFrame(lineEdit);
            var surface = FindSurface(lineEdit);
            frame.FrameActiveShadow.ShouldNotBeNull();
            frame.FrameHoverBackground.ShouldBeNull();

            lineEdit.StyleVariant = InputControlStyleVariant.Filled;
            Dispatcher.UIThread.RunJobs();
            frame.FrameActiveShadow.ShouldBeNull();
            frame.FrameHoverBackground.ShouldNotBeNull();
            ColorOf(surface.Background).ShouldBe(Resource(lineEdit, SharedTokenKind.ColorFillTertiary));

            lineEdit.StyleVariant = InputControlStyleVariant.Borderless;
            Dispatcher.UIThread.RunJobs();
            frame.FrameBorderBrush.ShouldBeNull();
            frame.FrameBackground.ShouldBeNull();
            ColorOf(surface.BorderBrush).ShouldBe(Colors.Transparent);
            ColorOf(surface.Background).ShouldBe(Colors.Transparent);

            lineEdit.StyleVariant = InputControlStyleVariant.Outlined;
            Dispatcher.UIThread.RunJobs();
            ColorOf(surface.BorderBrush).ShouldBe(Resource(lineEdit, SharedTokenKind.ColorBorder));
            ColorOf(surface.Background).ShouldBe(Resource(lineEdit, SharedTokenKind.ColorBgContainer));
        });
    }

    [Fact]
    public void Detached_Frame_Has_No_Resolved_Surface_Variables()
    {
        // Before the frame reaches a themed tree its variables resolve nothing, so the projections fall back to the
        // frame's base values exactly as unresolved Token setters did.
        var frame = new AddOnDecoratedBox();
        frame.FrameBorderBrush.ShouldBeNull();
        frame.FrameBackground.ShouldBeNull();
    }

    private static Color Resource(Control control, SharedTokenKind kind)
    {
        control.TryFindResource(kind, out var value).ShouldBeTrue();
        return ColorOf(value as IBrush);
    }

    private static AddOnDecoratedBox FindFrame(Control control) =>
        control.GetVisualDescendants().OfType<AddOnDecoratedBox>().First();

    private static PixelAlignedBorder FindSurface(Control control) =>
        control.GetVisualDescendants().OfType<PixelAlignedBorder>().First(item => item.Name == "PART_ContentFrame");

    private static Color ColorOf(IBrush? brush)
    {
        brush.ShouldNotBeNull();
        brush.ShouldBeAssignableTo<ISolidColorBrush>();
        return ((ISolidColorBrush)brush!).Color;
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow { Width = 240, Height = 120, Content = content };
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
