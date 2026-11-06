// Explicit real-Win32 global-type retirement specialist; see README.md.
using System.Diagnostics;
using System.Runtime.CompilerServices;
using AtomUI;
using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Styling;
using Avalonia.VisualTree;
using D = AtomUI.Desktop.Controls;

namespace Win31;

internal static class GlobalStyleVerification
{
    internal static async Task Run(D.Window window)
    {
        window.Width = 900;
        window.Height = 700;
        var split = new D.SplitButton { Content = "Split", Width = 140 };
        Control[] controls =
        [
            new D.Button { Content = "Button" }, new D.DropdownButton { Content = "Dropdown", Height = double.NaN },
            new D.HyperLinkButton { Content = "Hyper", Height = double.NaN }, split,
            new D.TextBox { Text = "draft", Width = 150 }, new D.SearchEdit { Text = "search", Width = 150 },
            new D.TextArea { Text = "area", Width = 150 }, new D.NumericUpDown { Value = 42, Width = 150 },
            new D.ComboBox { Width = 150 }, new D.Select { Width = 150 }, new D.Cascader { Width = 150 },
            new D.ToggleSwitch(), new D.CompactSpaceAddOn { Content = "addon" },
            new D.ButtonSpinner { Content = "spinner" }, new D.SpinIndicator(), new D.Space()
        ];
        var panel = new WrapPanel { Children = { } };
        foreach (var control in controls)
        {
            control.Margin = new Thickness(6);
            control.VerticalAlignment = VerticalAlignment.Top;
            panel.Children.Add(control);
        }
        window.Content = panel;
        var checks = 0;
        void Require(bool condition, string message)
        {
            checks++;
            if (!condition) { throw new InvalidOperationException("G1 " + message); }
        }
        foreach (var dark in new[] { false, true })
        {
            var result = await Application.Current!.GetThemeManager()!.ApplyThemeAsync(
                new ThemeRequest(IThemeManager.DEFAULT_THEME_ID,
                    new ThemeConfigBuilder().WithAlgorithms(dark ? [ThemeAlgorithm.Default, ThemeAlgorithm.Dark] : [ThemeAlgorithm.Default]).Build(),
                    ThemeTransitionReason.UserRequest));
            Require(result.Status is ThemeTransitionStatus.Committed or ThemeTransitionStatus.NoOp, "theme committed");
            foreach (var size in new[] { CustomizableSizeType.Large, CustomizableSizeType.Middle, CustomizableSizeType.Small, CustomizableSizeType.Custom })
            {
                foreach (var control in controls.OfType<ICustomizableSizeTypeAware>()) { control.SizeType = size; }
                await Task.Delay(300);
                Require(controls.Length == 16 && controls.All(c => TopLevel.GetTopLevel(c) == window), "16 same real attached roots");
                Require(Math.Abs(window.ClientSize.Width - 900) < 0.01 && Math.Abs(window.ClientSize.Height - 700) < 0.01, "900x700 client");
                var primary = split.GetVisualDescendants().OfType<D.Button>().Single(x => x.Name == "PART_PrimaryButton");
                var secondary = split.GetVisualDescendants().OfType<D.Button>().Single(x => x.Name == "PART_SecondaryButton");
                var before = controls.OfType<TemplatedControl>().Select(Snapshot).ToArray();
                var primaryBefore = Snapshot(primary);
                var secondaryBefore = Snapshot(secondary);
                var baselineDesired = split.DesiredSize;
                var styles = new Styles
                {
                    Style<D.Button>(), Style<D.DropdownButton>(), Style<D.HyperLinkButton>(), Style<D.SplitButton>(),
                    Style<D.TextBox>(), Style<D.SearchEdit>(), Style<D.TextArea>(), Style<D.NumericUpDown>(),
                    Style<D.ComboBox>(), Style<D.Select>(), Style<D.Cascader>(), Style<D.ToggleSwitch>(),
                    Style<D.CompactSpaceAddOn>(), Style<D.ButtonSpinner>(), Style<D.SpinIndicator>()
                };
                window.Styles.Add(styles);
                await Task.Delay(300);
                if (size == CustomizableSizeType.Custom)
                {
                    Require(primary.FontSize == 23 && secondary.FontSize == 23, "global Button type style reaches both template Buttons");
                    Require(primary.Padding == new Thickness(17, 9), "global Button padding reaches template");
                }
                foreach (var width in new[] { 140d, 180d, 140d })
                {
                    split.Width = width;
                    await Task.Delay(60);
                    Require(primary.TranslatePoint(default, split)!.Value.X + primary.Bounds.Width <= split.Bounds.Width + 0.01, "primary within fixed width " + width);
                    Require(secondary.TranslatePoint(default, split)!.Value.X + secondary.Bounds.Width <= split.Bounds.Width + 0.01, "secondary within fixed width " + width);
                }
                var watch = Stopwatch.StartNew();
                window.Styles.Remove(styles);
                var retirement = watch.Elapsed;
                await Task.Delay(300);
                Require(controls.OfType<TemplatedControl>().Select(Snapshot).SequenceEqual(before), "every root restored " + size);
                Require(Snapshot(primary) == primaryBefore && Snapshot(secondary) == secondaryBefore, "exact template font/padding/radius restoration " + size);
                Require(split.DesiredSize == baselineDesired, "fixed-width DesiredSize including Margin restored");
                Require(((D.TextBox)controls[4]).Text == "draft" && ((D.SearchEdit)controls[5]).Text == "search"
                    && ((D.TextArea)controls[6]).Text == "area" && ((D.NumericUpDown)controls[7]).Value == 42, "draft and value preserved");
                split.ClearValue(Layoutable.WidthProperty);
                split.ClearValue(Layoutable.MinWidthProperty);
                split.ClearValue(Layoutable.HeightProperty);
                await Task.Delay(120);
                Require(primary.FontSize == primaryBefore.Font && secondary.FontSize == secondaryBefore.Font, "Auto geometry keeps restored typography");
                foreach (var width in new[] { 140d, 180d, 140d })
                {
                    split.Width = width;
                    await Task.Delay(60);
                    Require(secondary.TranslatePoint(default, split)!.Value.X + secondary.Bounds.Width <= split.Bounds.Width + 0.01, "restored layout within " + width);
                }
                Console.WriteLine($"G1_STAGE dark={dark} size={size} primary={primary.FontSize}/{primary.Padding} secondary={secondary.FontSize}/{secondary.Padding} desired={split.DesiredSize} elapsed={retirement.TotalMilliseconds:F1}");
            }
        }
        if (!RuntimeFeature.IsDynamicCodeSupported)
        {
            Require(!Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Any(m => string.Equals(m.ModuleName, "coreclr.dll", StringComparison.OrdinalIgnoreCase)), "no CLR module");
        }
        Console.WriteLine($"G1_PASS checks={checks} roots=16 themes=2 sizes=4 profile=global-types dynamic={RuntimeFeature.IsDynamicCodeSupported}");
    }

    private static Style Style<T>() where T : TemplatedControl => new(selector => selector.OfType<T>())
    {
        Setters =
        {
            new Setter(TemplatedControl.FontSizeProperty, 23d),
            new Setter(TemplatedControl.PaddingProperty, new Thickness(17, 9)),
            new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(7))
        }
    };

    private static (double Font, Thickness Padding, CornerRadius Radius) Snapshot(TemplatedControl control) =>
        (control.FontSize, control.Padding, control.CornerRadius);
}
