using System.Diagnostics;
using System.Reflection;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using SkiaSharp;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Scope isolation of the select family (theme-effect-tests.md, family gate). The input surface is the shared,
/// identity-less input frame, which reads the hosting member's effective globals; drop-down content opens inside a
/// registered Popup and reads the member's TokenResource explicitly. A <c>Controls[X]</c> override must reach X only,
/// the root Token reaches every member, and registered controls composed into a member keep their own identity.
/// </summary>
public partial class SelectFamilyEffectTests
{
    private const string IsolationColor = "#ff00b96b";

    [Fact]
    public void Select_Option_In_The_Open_Dropdown_Follows_Controls_Select_Not_ComboBox()
    {
        var select = new Select { Width = 200, OptionsSource = s_selectOptions, SelectedOption = s_selectOptions[1] };
        select.IsPopupPinnedOpen = true;
        WithControls([select], overlay: true, (provider, capture) =>
        {
            WaitForPopup(select);
            var option = TopLevel.GetTopLevel(select)!.GetVisualDescendants().OfType<SelectCandidateListItem>()
                                .First(static item => item.IsSelected);
            // The option is in the popup layer, not under the Select, and the registered Popup is its Token owner: owner
            // reads inside the drop-down would follow Controls[Popup], so the option reads SelectTokenResource explicitly.
            option.GetVisualAncestors().ShouldNotContain(select);
            ThemeScope.GetTokenOwner(option).ShouldBe(PopupTokens.Identity);
            using var baseline = capture();

            provider.Config = IsolationConfig(control: (ComboBoxTokens.Identity, "ControlItemBgActive"));
            using var comboBox = capture();
            ChangedRatioOf(baseline, comboBox, option).ShouldBe(0d, "Controls[ComboBox] reached the Select option.");

            provider.Config = IsolationConfig(control: (SelectTokens.Identity, "ControlItemBgActive"));
            using var own = capture();
            ChangedRatioOf(baseline, own, option).ShouldBeGreaterThan(TokenEffectHarness.DefaultThresholdRatio,
                "Controls[Select].ControlItemBgActive did not reach the selected option in the drop-down.");

            provider.Config = IsolationConfig(control: (SelectTokens.Identity, "ColorText"));
            using var text = capture();
            ChangedRatioOf(baseline, text, TopLevel.GetTopLevel(select)!.GetVisualDescendants()
                                                  .OfType<SelectCandidateListItem>()
                                                  .First(static item => !item.IsSelected && item.IsEnabled))
                .ShouldBeGreaterThan(0d, "Controls[Select].ColorText did not reach an option in the drop-down.");
        });
    }

    [Fact]
    public void Controls_X_ColorBorder_Changes_Only_The_Surface_Of_X()
    {
        var select     = new Select { Width = 140 };
        var comboBox   = new ComboBox { Width = 140 };
        var cascader   = new Cascader { Width = 140 };
        var treeSelect = new TreeSelect { Width = 140 };
        var lineEdit   = new LineEdit { Width = 140 };
        Control[] all = [select, comboBox, cascader, treeSelect, lineEdit];
        var cases = new (ControlTokenIdentity Identity, Control Target)[]
        {
            (SelectTokens.Identity, select), (ComboBoxTokens.Identity, comboBox), (CascaderTokens.Identity, cascader),
            (TreeSelectTokens.Identity, treeSelect), (LineEditTokens.Identity, lineEdit)
        };

        WithControls(all, overlay: false, (provider, capture) =>
        {
            foreach (var (identity, target) in cases)
            {
                provider.Config = IsolationConfig();
                using var before = capture();
                provider.Config = IsolationConfig(control: (identity, "ColorBorder"));
                using var after = capture();

                foreach (var control in all)
                {
                    var ratio = ChangedRatioOf(before, after, control);
                    if (ReferenceEquals(control, target))
                    {
                        ratio.ShouldBeGreaterThan(TokenEffectHarness.ThinLineThresholdRatio,
                            $"Controls[{identity}].ColorBorder did not reach {control.GetType().Name}.");
                    }
                    else
                    {
                        ratio.ShouldBe(0d, $"Controls[{identity}].ColorBorder leaked into {control.GetType().Name}.");
                    }
                }
            }
        });
    }

    [Fact]
    public void Root_ColorBorder_Changes_Every_Member_Surface()
    {
        Control[] all =
        [
            new Select { Width = 140 }, new ComboBox { Width = 140 }, new Cascader { Width = 140 },
            new TreeSelect { Width = 140 }, new LineEdit { Width = 140 }
        ];
        WithControls(all, overlay: false, (provider, capture) =>
        {
            using var before = capture();
            provider.Config = IsolationConfig(root: "ColorBorder");
            using var after = capture();
            foreach (var control in all)
            {
                ChangedRatioOf(before, after, control).ShouldBeGreaterThan(TokenEffectHarness.ThinLineThresholdRatio,
                    $"Root ColorBorder did not reach {control.GetType().Name}.");
            }
        });
    }

    [Fact]
    public void ComboBox_Item_Keeps_Its_Identity_For_Globals_And_Follows_The_ComboBox_Own_Tokens()
    {
        // ComboBoxItem is registered: its globals follow Controls[ComboBoxItem], not Controls[ComboBox]; the item
        // colours are ComboBox Own Tokens and follow the ComboBox.
        var comboBox = CreateComboBox(4);
        comboBox.SelectedIndex     = 1;
        comboBox.IsPopupPinnedOpen = true;
        WithControls([comboBox], overlay: true, (provider, capture) =>
        {
            WaitForPopup(comboBox);
            var item = TopLevel.GetTopLevel(comboBox)!.GetVisualDescendants().OfType<ComboBoxItem>()
                               .First(static candidate => candidate.IsSelected);
            ThemeScope.GetTokenOwner(item).ShouldBe(ComboBoxItemTokens.Identity);
            using var baseline = capture();

            provider.Config = IsolationConfig(control: (ComboBoxTokens.Identity, "BorderRadiusSM"), value: "12");
            using var hostRadius = capture();
            ChangedRatioOf(baseline, hostRadius, item).ShouldBe(0d, "Controls[ComboBox].BorderRadiusSM reached the item.");

            provider.Config = IsolationConfig(control: (ComboBoxItemTokens.Identity, "BorderRadiusSM"), value: "12");
            using var itemRadius = capture();
            ChangedRatioOf(baseline, itemRadius, item).ShouldBeGreaterThan(0d);

            provider.Config = IsolationConfig(control: (ComboBoxTokens.Identity, "ItemSelectedBgColor"));
            using var hostOwn = capture();
            ChangedRatioOf(baseline, hostOwn, item).ShouldBeGreaterThan(TokenEffectHarness.DefaultThresholdRatio);
        });
    }

    [Fact]
    public void Cascader_View_And_Items_Keep_Their_Identity_And_Follow_The_Cascader_Own_Tokens()
    {
        // The drop-down view and its items are registered: Controls[Cascader] reaches them only through the Cascader Own
        // Tokens they read (option colours and metrics).
        var cascader = new Cascader { Width = 200, OptionsSource = CascaderOptions(4) };
        cascader.IsPopupPinnedOpen = true;
        WithControls([cascader], overlay: true, (provider, capture) =>
        {
            WaitForPopup(cascader);
            var view = TopLevel.GetTopLevel(cascader)!.GetVisualDescendants().OfType<CascaderView>().First();
            var item = view.GetVisualDescendants().OfType<CascaderViewItem>().First(IsZhejiang);
            ThemeScope.GetTokenOwner(view).ShouldBe(CascaderViewTokens.Identity);
            ThemeScope.GetTokenOwner(item).ShouldBe(CascaderViewItemTokens.Identity);
            using var baseline = capture();

            provider.Config = IsolationConfig(control: (CascaderTokens.Identity, "ColorIcon"));
            using var hostGlobal = capture();
            ChangedRatioOf(baseline, hostGlobal, item).ShouldBe(0d, "Controls[Cascader].ColorIcon reached the item icon.");

            provider.Config = IsolationConfig(control: (CascaderViewItemTokens.Identity, "ColorIcon"));
            using var itemGlobal = capture();
            ChangedRatioOf(baseline, itemGlobal, item).ShouldBeGreaterThan(0d);

            provider.Config = IsolationConfig(control: (CascaderTokens.Identity, "ItemHeaderSpacing"), value: "24");
            using var hostOwn = capture();
            ChangedRatioOf(baseline, hostOwn, item).ShouldBeGreaterThan(0d);
        });
    }

    private static ThemeConfig IsolationConfig(
        (ControlTokenIdentity Identity, string Token)? control = null,
        string? root = null,
        string? value = null)
    {
        var builder = new ThemeConfigBuilder().WithToken(nameof(SharedTokenKind.EnableMotion), "false");
        if (root is not null)
        {
            builder.WithToken(root, value ?? IsolationColor);
        }
        if (control is { } entry)
        {
            builder.WithControl(entry.Identity,
                new ControlThemeConfigBuilder().WithToken(entry.Token, value ?? IsolationColor).Build());
        }

        return builder.Build();
    }

    private static void WithControls(Control[] controls, bool overlay, Action<ThemeConfigProvider, Func<SKBitmap>> body)
    {
        var panel = new WrapPanel
        {
            Orientation         = Orientation.Horizontal,
            ItemSpacing         = 24,
            LineSpacing         = 24,
            Margin              = new Thickness(24),
            Width               = 720,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment   = VerticalAlignment.Top
        };
        foreach (var control in controls)
        {
            control.VerticalAlignment = VerticalAlignment.Top;
            panel.Children.Add(control);
        }

        var provider = new ThemeConfigProvider { Config = IsolationConfig(), Child = panel };
        Control content = provider;
        if (overlay)
        {
            var layers = new VisualLayerManager { Child = provider };
            typeof(VisualLayerManager)
                .GetProperty("EnablePopupOverlayLayer", BindingFlags.Instance | BindingFlags.NonPublic)
                .ShouldNotBeNull()
                .SetValue(layers, true);
            content = layers;
        }

        var window = new AvaloniaWindow { Width = 800, Height = 500, Content = content };
        try
        {
            window.Show();
            SettleWindow(window);
            body(provider, () =>
            {
                SettleWindow(window);
                return FrameCapture.Capture(window);
            });
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static void SettleWindow(AvaloniaWindow window)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < TimeSpan.FromMilliseconds(200))
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Thread.Sleep(5);
        }
        FrameCapture.Refresh(window);
    }

    private static double ChangedRatioOf(SKBitmap before, SKBitmap after, Control control)
    {
        var window = (Visual)TopLevel.GetTopLevel(control)!;
        var origin = control.TranslatePoint(new Point(), window).ShouldNotBeNull();
        var region = new SKRectI(
            (int)Math.Floor(origin.X),
            (int)Math.Floor(origin.Y),
            (int)Math.Ceiling(origin.X + control.Bounds.Width),
            (int)Math.Ceiling(origin.Y + control.Bounds.Height));
        var changed = FrameCapture.CountDifferentPixels(before, after, region, TokenEffectHarness.DefaultChannelTolerance);
        return (double)changed / (region.Width * region.Height);
    }
}
