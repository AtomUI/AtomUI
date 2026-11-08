using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Diagnostics;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Theme;

/// <summary>
/// Compiled AXAML guard for the binding priority of generated <c>XxxTokenResource</c> and
/// <c>SharedTokenResource</c> reads: Template priority on ControlTemplate nodes (triggered styles override
/// them, plain style setters do not), unchanged priority everywhere else.
/// </summary>
[Collection(ControlEffectiveGlobalBindingTestCollection.Name)]
public class TokenResourcePriorityAxamlTests
{
    private static readonly Color s_triggeredBackground = Color.Parse("#00FF00");
    private static readonly Color s_triggeredBorder = Color.Parse("#0000FF");

    static TokenResourcePriorityAxamlTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Template_Node_Token_Reads_Take_Template_Priority_And_Yield_To_Triggered_Styles()
    {
        RunInWindow(view =>
        {
            var host = view.FindControl<ContentControl>("TemplatedHost").ShouldNotBeNull();
            var frame = TemplatePart(host, "PART_Frame");
            var tokenBackground = ResourceColor(frame, ButtonTokenKind.DefaultBg);
            var tokenBorder = ResourceColor(frame, SharedTokenKind.ColorBorder);

            BrushColor(frame.Background).ShouldBe(tokenBackground);
            BrushColor(frame.BorderBrush).ShouldBe(tokenBorder);
            frame.GetDiagnostic(Border.BackgroundProperty).Priority.ShouldBe(BindingPriority.Template);
            frame.GetDiagnostic(Border.BorderBrushProperty).Priority.ShouldBe(BindingPriority.Template);

            host.IsEnabled = false;
            Dispatcher.UIThread.RunJobs();

            BrushColor(frame.Background).ShouldBe(s_triggeredBackground);
            BrushColor(frame.BorderBrush).ShouldBe(s_triggeredBorder);

            host.IsEnabled = true;
            Dispatcher.UIThread.RunJobs();

            BrushColor(frame.Background).ShouldBe(tokenBackground);
            BrushColor(frame.BorderBrush).ShouldBe(tokenBorder);
        });
    }

    [Fact]
    public void Template_Node_Token_Reads_Still_Win_Over_Non_Triggered_Style_Setters()
    {
        RunInWindow(view =>
        {
            var plain = TemplatePart(view.FindControl<ContentControl>("TemplatedHost").ShouldNotBeNull(), "PART_Plain");

            BrushColor(plain.Background).ShouldBe(ResourceColor(plain, ButtonTokenKind.DefaultBg));
            plain.GetDiagnostic(Border.BackgroundProperty).Priority.ShouldBe(BindingPriority.Template);
        });
    }

    [Fact]
    public void Token_Reads_Outside_Templates_Keep_Their_Priority()
    {
        RunInWindow(view =>
        {
            var direct = view.FindControl<Border>("DirectBorder").ShouldNotBeNull();
            var themed = view.FindControl<Border>("ThemedBorder").ShouldNotBeNull();
            var tokenBackground = ResourceColor(direct, ButtonTokenKind.DefaultBg);

            direct.GetDiagnostic(Border.BackgroundProperty).Priority.ShouldBe(BindingPriority.LocalValue);
            themed.GetDiagnostic(Border.BackgroundProperty).Priority.ShouldBe(BindingPriority.Style);
            BrushColor(themed.Background).ShouldBe(tokenBackground);

            // A local value still beats a triggered style outside templates.
            direct.IsEnabled = false;
            Dispatcher.UIThread.RunJobs();
            BrushColor(direct.Background).ShouldBe(tokenBackground);
        });
    }

    private static void RunInWindow(Action<TokenResourcePriorityView> assert)
    {
        var view = new TokenResourcePriorityView();
        var provider = new ThemeConfigProvider { Child = view };
        provider.ThemeChangeFailed += static (_, args) =>
        {
            throw new InvalidOperationException(args.Exception?.Message ?? "Theme compilation failed.");
        };
        var window = new AvaloniaWindow { Width = 240, Height = 160, Content = provider };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assert(view);
        }
        finally
        {
            window.Close();
        }
    }

    private static Border TemplatePart(ContentControl host, string name) =>
        host.GetVisualDescendants().OfType<Border>().Single(border => border.Name == name);

    private static Color ResourceColor(StyledElement element, object key)
    {
        element.TryFindResource(key, element.ActualThemeVariant, out var value).ShouldBeTrue();
        return value switch
        {
            ISolidColorBrush brush => brush.Color,
            Color color => color,
            _ => throw new InvalidOperationException($"Resource '{key}' is not a colour: {value}.")
        };
    }

    private static Color BrushColor(IBrush? brush) => brush.ShouldBeAssignableTo<ISolidColorBrush>()!.Color;
}
