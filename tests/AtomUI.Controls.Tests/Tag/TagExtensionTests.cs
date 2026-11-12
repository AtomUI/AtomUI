using AtomUI.Controls.Commons;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Controls.Tests.Tag;

public class TagExtensionTests
{
    private const string CloseButtonName = "PART_CloseButton";
    private const string CloseButtonHostName = "PART_CloseButtonHost";

    static TagExtensionTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    private sealed class TestCloseButton : AbstractIconButton
    {
    }

    // Close button created on demand into PART_CloseButtonHost.
    private sealed class TestTag : AbstractTag
    {
        public TestTag()
        {
            Template = new FuncControlTemplate<TestTag>((_, scope) =>
            {
                var host = new Decorator { Name = CloseButtonHostName };
                scope.Register(host.Name, host);
                return new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children    = { new TextBlock { Text = "tag" }, host }
                };
            });
        }

        internal TagColorTable? Table => ColorTable;
        internal bool IsTrackingTheme => IsThemeTracked;

        protected override AbstractIconButton? CreateCloseButton()
        {
            return new TestCloseButton();
        }
    }

    // Close button declared statically in the template, as the Desktop Tag does.
    private sealed class StaticCloseTag : AbstractTag
    {
        public StaticCloseTag()
        {
            Template = new FuncControlTemplate<StaticCloseTag>((_, scope) =>
            {
                var close = new TestCloseButton { Name = CloseButtonName };
                scope.Register(close.Name, close);
                return new StackPanel { Children = { close } };
            });
        }
    }

    private sealed class TestWindowContext : IDisposable
    {
        private readonly AvaloniaWindow _window;

        internal TestWindowContext(Control content)
        {
            _window = new AvaloniaWindow { Width = 400, Height = 200, Content = content };
            _window.Show();
            Dispatcher.UIThread.RunJobs();
        }

        public void Dispose()
        {
            _window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static void Pump(Control root)
    {
        Dispatcher.UIThread.RunJobs();
        root.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }

    private static Color Global(Control owner, SharedTokenKind kind)
    {
        var snapshot = ThemeScope.ResolveContext(owner).ShouldNotBeNull().Snapshot;
        return new ThemeTokenResolver().GetGlobal<Color>(snapshot, (int)kind);
    }

    private static Color BrushColor(IBrush? brush)
    {
        return brush.ShouldBeAssignableTo<ISolidColorBrush>().ShouldNotBeNull().Color;
    }

    private static async Task ApplyAppearanceAsync(ThemeAppearance appearance)
    {
        var manager = Application.Current!.GetThemeManager().ShouldNotBeNull();
        var algorithms = appearance == ThemeAppearance.Dark
            ? new[] { ThemeAlgorithm.Default, ThemeAlgorithm.Dark }
            : new[] { ThemeAlgorithm.Default };
        var result = await manager.ApplyThemeAsync(
            new ThemeRequest(
                IThemeManager.DEFAULT_THEME_ID,
                new ThemeConfigBuilder().WithAlgorithms(algorithms).Build(),
                ThemeTransitionReason.UserRequest),
            TestContext.Current.CancellationToken);
        result.Status.ShouldBeOneOf(ThemeTransitionStatus.Committed, ThemeTransitionStatus.NoOp);
        Dispatcher.UIThread.RunJobs();
    }

    private static AbstractIconButton? FindCloseButton(Control tag)
    {
        return tag.GetVisualDescendants().OfType<AbstractIconButton>()
                  .SingleOrDefault(button => button.Name == CloseButtonName);
    }

    [Fact]
    public void Existing_variants_unchanged()
    {
        ((int)TagVariant.Filled).ShouldBe(0);
        ((int)TagVariant.Solid).ShouldBe(1);
        ((int)TagVariant.Outlined).ShouldBe(2);
        ((int)TagVariant.Outline).ShouldBe(3);
        Enum.GetValues<TagVariant>().Length.ShouldBe(4);

        var red = PresetPalettes.GetPresetPalette(PresetPrimaryColor.Red).ColorSequence;
        foreach (var variant in new[] { TagVariant.Filled, TagVariant.Solid, TagVariant.Outlined })
        {
            var preset = new TestTag { TagColor = "red", Variant = variant };
            var status = new TestTag { TagColor = "processing", Variant = variant };
            var host = new StackPanel { Children = { preset, status } };
            using var context = new TestWindowContext(host);
            Pump(host);

            BrushColor(preset.Background).ShouldBe(variant == TagVariant.Solid ? red[5] : red[0]);
            BrushColor(preset.Foreground).ShouldBe(
                variant == TagVariant.Solid ? Global(preset, SharedTokenKind.ColorTextLightSolid) : red[6]);
            BrushColor(preset.BorderBrush).ShouldBe(variant switch
            {
                TagVariant.Solid    => red[5],
                TagVariant.Outlined => red[2],
                _                   => Colors.Transparent
            });
            BrushColor(status.Background).ShouldBe(
                Global(status, variant == TagVariant.Solid ? SharedTokenKind.ColorInfo : SharedTokenKind.ColorInfoBg));
            preset.Classes.Contains(TagPseudoClass.PresetColor).ShouldBeTrue();
            status.Classes.Contains(TagPseudoClass.StatusColor).ShouldBeTrue();
        }
    }

    [Fact]
    public void Outline_variant_colors()
    {
        var red = PresetPalettes.GetPresetPalette(PresetPrimaryColor.Red).ColorSequence;
        var custom = Color.Parse("#1677ff");
        var cases = new (string Color, Func<Control, Color> Expected)[]
        {
            ("red", _ => red[5]),
            ("success", owner => Global(owner, SharedTokenKind.ColorSuccess)),
            ("primary", owner => Global(owner, SharedTokenKind.ColorPrimary)),
            ("danger", owner => Global(owner, SharedTokenKind.ColorError)),
            (custom.ToString(), _ => custom)
        };

        foreach (var (color, expected) in cases)
        {
            var tag = new TestTag { TagColor = color, Variant = TagVariant.Outline };
            using var context = new TestWindowContext(tag);
            Pump(tag);

            BrushColor(tag.Background).ShouldBe(Colors.Transparent, color);
            BrushColor(tag.BorderBrush).ShouldBe(expected(tag), color);
            BrushColor(tag.Foreground).ShouldBe(expected(tag), color);
        }
    }

    [Theory]
    [InlineData("primary", SharedTokenKind.ColorPrimary, SharedTokenKind.ColorPrimaryBg, SharedTokenKind.ColorPrimaryBorder)]
    [InlineData("danger", SharedTokenKind.ColorError, SharedTokenKind.ColorErrorBg, SharedTokenKind.ColorErrorBorder)]
    [InlineData("Danger", SharedTokenKind.ColorError, SharedTokenKind.ColorErrorBg, SharedTokenKind.ColorErrorBorder)]
    public void Primary_and_danger_resolve(
        string name, SharedTokenKind color, SharedTokenKind background, SharedTokenKind border)
    {
        var filled = new TestTag { TagColor = name, Variant = TagVariant.Filled };
        var solid = new TestTag { TagColor = name, Variant = TagVariant.Solid };
        var outlined = new TestTag { TagColor = name, Variant = TagVariant.Outlined };
        var host = new StackPanel { Children = { filled, solid, outlined } };
        using var context = new TestWindowContext(host);
        Pump(host);

        BrushColor(filled.Background).ShouldBe(Global(filled, background));
        BrushColor(filled.Foreground).ShouldBe(Global(filled, color));
        BrushColor(filled.BorderBrush).ShouldBe(Colors.Transparent);

        BrushColor(solid.Background).ShouldBe(Global(solid, color));
        BrushColor(solid.Foreground).ShouldBe(Global(solid, SharedTokenKind.ColorTextLightSolid));
        BrushColor(solid.BorderBrush).ShouldBe(Global(solid, color));

        BrushColor(outlined.Background).ShouldBe(Global(outlined, background));
        BrushColor(outlined.Foreground).ShouldBe(Global(outlined, color));
        BrushColor(outlined.BorderBrush).ShouldBe(Global(outlined, border));

        filled.Classes.Contains(TagPseudoClass.StatusColor).ShouldBeTrue();
        filled.Classes.Contains(TagPseudoClass.PresetColor).ShouldBeFalse();
        filled.Classes.Contains(TagPseudoClass.CustomColor).ShouldBeFalse();
    }

    [Fact]
    public void Empty_color_skips_snapshot()
    {
        var empty = new TestTag();
        var whitespace = new TestTag { TagColor = "  " };
        var defaultName = new TestTag { TagColor = "default" };
        var host = new StackPanel { Children = { empty, whitespace, defaultName } };
        using var context = new TestWindowContext(host);
        Pump(host);

        foreach (var tag in new[] { empty, whitespace, defaultName })
        {
            tag.Table.ShouldBeNull();
            tag.IsTrackingTheme.ShouldBeFalse();
        }

        empty.TagColor = "red";
        Pump(host);
        empty.Table.ShouldNotBeNull();
        empty.IsTrackingTheme.ShouldBeTrue();
        BrushColor(empty.Foreground).ShouldBe(PresetPalettes.GetPresetPalette(PresetPrimaryColor.Red).ColorSequence[6]);

        empty.TagColor = null;
        Pump(host);
        empty.Table.ShouldBeNull();
        empty.IsTrackingTheme.ShouldBeFalse();
        empty.Classes.Contains(TagPseudoClass.PresetColor).ShouldBeFalse();
    }

    [Fact]
    public async Task Color_table_is_shared_per_snapshot()
    {
        await ApplyAppearanceAsync(ThemeAppearance.Light);
        var first = new TestTag { TagColor = "red" };
        var second = new TestTag { TagColor = "success", Variant = TagVariant.Solid };
        var third = new TestTag { TagColor = "primary" };
        var host = new StackPanel { Children = { first, second, third } };
        using var context = new TestWindowContext(host);
        Pump(host);

        try
        {
            var light = first.Table.ShouldNotBeNull();
            second.Table.ShouldBeSameAs(light);
            third.Table.ShouldBeSameAs(light);

            await ApplyAppearanceAsync(ThemeAppearance.Dark);
            var dark = first.Table.ShouldNotBeNull();
            dark.ShouldNotBeSameAs(light);
            second.Table.ShouldBeSameAs(dark);
            third.Table.ShouldBeSameAs(dark);
        }
        finally
        {
            await ApplyAppearanceAsync(ThemeAppearance.Light);
        }
    }

    [Fact]
    public async Task Color_follows_theme_switch_while_attached()
    {
        await ApplyAppearanceAsync(ThemeAppearance.Light);
        var tag = new TestTag { TagColor = "success" };
        using var context = new TestWindowContext(tag);
        Pump(tag);

        try
        {
            var light = BrushColor(tag.Foreground);
            light.ShouldBe(Global(tag, SharedTokenKind.ColorSuccess));

            await ApplyAppearanceAsync(ThemeAppearance.Dark);
            var dark = BrushColor(tag.Foreground);
            dark.ShouldNotBe(light);
            dark.ShouldBe(Global(tag, SharedTokenKind.ColorSuccess));
        }
        finally
        {
            await ApplyAppearanceAsync(ThemeAppearance.Light);
        }
    }

    [Fact]
    public async Task Color_is_current_after_theme_switch_while_detached()
    {
        await ApplyAppearanceAsync(ThemeAppearance.Light);
        var colored = new TestTag { TagColor = "success" };
        var emptyThenColored = new TestTag();
        var host = new StackPanel { Children = { colored, emptyThenColored } };
        using var context = new TestWindowContext(host);
        Pump(host);
        var light = BrushColor(colored.Foreground);

        try
        {
            host.Children.Clear();
            Pump(host);
            colored.IsTrackingTheme.ShouldBeFalse();
            await ApplyAppearanceAsync(ThemeAppearance.Dark);

            // The tag without a color is given one while detached, then both re-attach after the switch.
            emptyThenColored.TagColor = "success";
            host.Children.Add(colored);
            host.Children.Add(emptyThenColored);
            Pump(host);

            var dark = Global(colored, SharedTokenKind.ColorSuccess);
            dark.ShouldNotBe(light);
            BrushColor(colored.Foreground).ShouldBe(dark);
            BrushColor(emptyThenColored.Foreground).ShouldBe(dark);
            colored.Table.ShouldBeSameAs(emptyThenColored.Table);
            colored.IsTrackingTheme.ShouldBeTrue();
        }
        finally
        {
            await ApplyAppearanceAsync(ThemeAppearance.Light);
        }
    }

    [Fact]
    public void Close_button_created_on_demand()
    {
        var closedCount = 0;
        var tag = new TestTag { Text = "close" };
        tag.Closed += (_, _) => closedCount++;
        using var context = new TestWindowContext(tag);
        Pump(tag);

        FindCloseButton(tag).ShouldBeNull();
        tag.GetVisualDescendants().OfType<Decorator>().Single(host => host.Name == CloseButtonHostName)
           .Child.ShouldBeNull();

        tag.IsClosable = true;
        Pump(tag);
        var button = FindCloseButton(tag).ShouldNotBeNull();
        button.Icon.ShouldNotBeNull();
        button.TemplatedParent.ShouldBeSameAs(tag);
        button.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent, button));
        closedCount.ShouldBe(1);

        // Toggling back removes the button and detaches its handler.
        tag.IsClosable = false;
        Pump(tag);
        FindCloseButton(tag).ShouldBeNull();
        button.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent, button));
        closedCount.ShouldBe(1);

        tag.IsClosable = true;
        Pump(tag);
        var recreated = FindCloseButton(tag).ShouldNotBeNull();
        recreated.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent, recreated));
        closedCount.ShouldBe(2);
    }

    [Fact]
    public void Close_button_exists_from_first_template_pass_when_closable()
    {
        var tag = new TestTag { IsClosable = true };
        using var context = new TestWindowContext(tag);
        Pump(tag);

        FindCloseButton(tag).ShouldNotBeNull();
    }

    [Fact]
    public void Static_close_button_template_keeps_working()
    {
        var closedCount = 0;
        var tag = new StaticCloseTag { IsClosable = true };
        tag.Closed += (_, _) => closedCount++;
        using var context = new TestWindowContext(tag);
        Pump(tag);

        var button = FindCloseButton(tag).ShouldNotBeNull();
        button.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent, button));
        closedCount.ShouldBe(1);

        // A statically declared close button is never removed by the control.
        tag.IsClosable = false;
        Pump(tag);
        FindCloseButton(tag).ShouldBeSameAs(button);
    }
}
