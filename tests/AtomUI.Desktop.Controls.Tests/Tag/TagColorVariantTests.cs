using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Media;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUITag = AtomUI.Desktop.Controls.Tag;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Tag;

public class TagColorVariantTests
{
    static TagColorVariantTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Default_Color_Uses_The_Three_Variant_Visuals()
    {
        var filled = new AtomUITag { Text = "Filled", Variant = TagVariant.Filled };
        var solid = new AtomUITag { Text = "Solid", Variant = TagVariant.Solid };
        var outlined = new AtomUITag { Text = "Outlined", Variant = TagVariant.Outlined };

        ShowInWindow(new StackPanel { Children = { filled, solid, outlined } }, () =>
        {
            var colorBorder = GetThemeColor(SharedTokenKind.ColorBorder);
            var colorBgSolid = GetThemeColor(SharedTokenKind.ColorBgSolid);

            GetBrushColor(filled.BorderBrush).ShouldBe(Colors.Transparent);
            GetBrushColor(outlined.BorderBrush).ShouldBe(colorBorder);
            GetBrushColor(solid.BorderBrush).ShouldBe(Colors.Transparent);
            GetBrushColor(solid.Background).ShouldBe(colorBgSolid);
            GetBrushColor(filled.Background).ShouldBe(GetBrushColor(outlined.Background));
            GetBrushColor(filled.Foreground).ShouldNotBe(GetBrushColor(solid.Foreground));
            filled.BorderThickness.ShouldBe(outlined.BorderThickness);
            solid.BorderThickness.ShouldBe(outlined.BorderThickness);
            outlined.BorderThickness.ShouldNotBe(new Thickness());
        });
    }

    [Fact]
    public void Default_Background_Composites_ColorFillTertiary_On_Container()
    {
        var tag = new AtomUITag { Text = "Default" };

        ShowInWindow(tag, () =>
        {
            var fillTertiary = GetThemeColor(SharedTokenKind.ColorFillTertiary);
            var bgContainer  = GetThemeColor(SharedTokenKind.ColorBgContainer);

            GetBrushColor(tag.Background)
                .ShouldBe(ColorUtils.OnBackground(fillTertiary, bgContainer));
        });
    }

    [Theory]
    [InlineData(TagVariant.Filled)]
    [InlineData(TagVariant.Solid)]
    [InlineData(TagVariant.Outlined)]
    public void Preset_Color_Uses_The_Selected_Variant(TagVariant variant)
    {
        var tag = new AtomUITag
        {
            TagColor = "red",
            Variant  = variant,
            Text     = "Red"
        };

        ShowInWindow(tag, () =>
        {
            var palette = AtomUI.Theme.Algorithms.PresetPalettes.GetPresetPalette(
                AtomUI.Theme.Algorithms.PresetPrimaryColor.Red);
            var colors = palette.ColorSequence;

            GetBrushColor(tag.Background).ShouldBe(
                variant == TagVariant.Solid ? colors[5] : colors[0]);
            GetBrushColor(tag.Foreground).ShouldBe(
                variant == TagVariant.Solid ? GetThemeColor(SharedTokenKind.ColorTextLightSolid) : colors[6]);
            GetBrushColor(tag.BorderBrush).ShouldBe(variant switch
            {
                TagVariant.Solid    => colors[5],
                TagVariant.Outlined => colors[2],
                _                   => Colors.Transparent
            });
            tag.Classes.Contains(TagPseudoClass.PresetColor).ShouldBeTrue();
            tag.Classes.Contains(TagPseudoClass.StatusColor).ShouldBeFalse();
            tag.Classes.Contains(TagPseudoClass.CustomColor).ShouldBeFalse();
        });
    }

    [Theory]
    [InlineData(TagVariant.Filled)]
    [InlineData(TagVariant.Solid)]
    [InlineData(TagVariant.Outlined)]
    public void Info_And_Processing_Use_The_Same_Status_Tokens(TagVariant variant)
    {
        var info = new AtomUITag { TagColor = "info", Variant = variant };
        var processing = new AtomUITag { TagColor = "processing", Variant = variant };

        ShowInWindow(new StackPanel { Children = { info, processing } }, () =>
        {
            GetBrushColor(info.Foreground).ShouldBe(GetBrushColor(processing.Foreground));
            GetBrushColor(info.Background).ShouldBe(GetBrushColor(processing.Background));
            GetBrushColor(info.BorderBrush).ShouldBe(GetBrushColor(processing.BorderBrush));
            GetBrushColor(info.Foreground).ShouldBe(
                variant == TagVariant.Solid
                    ? GetThemeColor(SharedTokenKind.ColorTextLightSolid)
                    : GetThemeColor(SharedTokenKind.ColorInfo));
            GetBrushColor(info.Background).ShouldBe(
                variant == TagVariant.Solid
                    ? GetThemeColor(SharedTokenKind.ColorInfo)
                    : GetThemeColor(SharedTokenKind.ColorInfoBg));
            GetBrushColor(info.BorderBrush).ShouldBe(variant switch
            {
                TagVariant.Solid    => GetThemeColor(SharedTokenKind.ColorInfo),
                TagVariant.Outlined => GetThemeColor(SharedTokenKind.ColorInfoBorder),
                _                   => Colors.Transparent
            });
        });
    }

    [Theory]
    [InlineData(TagVariant.Filled)]
    [InlineData(TagVariant.Solid)]
    [InlineData(TagVariant.Outlined)]
    public void Custom_Color_Uses_The_Hsl_Light_Background(TagVariant variant)
    {
        var color = Color.Parse("#1677ff");
        var tag = new AtomUITag
        {
            TagColor = color.ToString(),
            Variant  = variant,
            Text     = "Custom"
        };

        ShowInWindow(tag, () =>
        {
            var hsl = new HslColor(color);
            var lightBackground = new HslColor(hsl.A, hsl.H, hsl.S, 0.95).ToRgb();

            GetBrushColor(tag.Background).ShouldBe(
                variant == TagVariant.Solid ? color : lightBackground);
            GetBrushColor(tag.Foreground).ShouldBe(
                variant == TagVariant.Solid ? GetThemeColor(SharedTokenKind.ColorTextLightSolid) : color);
            GetBrushColor(tag.BorderBrush).ShouldBe(
                variant == TagVariant.Outlined ? color : Colors.Transparent);
            tag.Classes.Contains(TagPseudoClass.CustomColor).ShouldBeTrue();
        });
    }

    [Fact]
    public void Clearing_Or_Invalidating_Color_Restores_Default_State()
    {
        var tag = new AtomUITag
        {
            TagColor = "#1677ff",
            Variant  = TagVariant.Outlined,
            Text     = "Default"
        };

        ShowInWindow(tag, () =>
        {
            tag.TagColor = null;
            Dispatcher.UIThread.RunJobs();

            GetBrushColor(tag.Background).ShouldNotBe(Color.Parse("#1677ff"));
            GetBrushColor(tag.BorderBrush).ShouldBe(GetThemeColor(SharedTokenKind.ColorBorder));
            tag.Classes.Contains(TagPseudoClass.CustomColor).ShouldBeFalse();
            tag.Classes.Contains(TagPseudoClass.PresetColor).ShouldBeFalse();
            tag.Classes.Contains(TagPseudoClass.StatusColor).ShouldBeFalse();

            tag.TagColor = "not-a-color";
            Dispatcher.UIThread.RunJobs();

            GetBrushColor(tag.Background).ShouldNotBe(Color.Parse("#1677ff"));
            GetBrushColor(tag.BorderBrush).ShouldBe(GetThemeColor(SharedTokenKind.ColorBorder));
            tag.Classes.Contains(TagPseudoClass.CustomColor).ShouldBeFalse();
        });
    }

    [Fact]
    public void Local_Foreground_Wins_Over_Computed_Color()
    {
        var localForeground = new SolidColorBrush(Colors.Purple);
        var tag = new AtomUITag
        {
            TagColor  = "#1677ff",
            Variant   = TagVariant.Solid,
            Foreground = localForeground,
            Text      = "Local"
        };

        ShowInWindow(tag, () =>
        {
            tag.Foreground.ShouldBeSameAs(localForeground);
        });
    }

    [Fact]
    public void Changing_Variant_Recalculates_The_Current_Color()
    {
        var tag = new AtomUITag
        {
            TagColor = "#1677ff",
            Variant  = TagVariant.Filled,
            Text     = "Dynamic"
        };

        ShowInWindow(tag, () =>
        {
            var lightBackground = GetBrushColor(tag.Background);
            lightBackground.ShouldNotBe(Color.Parse("#1677ff"));

            tag.Variant = TagVariant.Solid;
            Dispatcher.UIThread.RunJobs();
            GetBrushColor(tag.Background).ShouldBe(Color.Parse("#1677ff"));
            GetBrushColor(tag.BorderBrush).ShouldBe(Colors.Transparent);

            tag.Variant = TagVariant.Outlined;
            Dispatcher.UIThread.RunJobs();
            GetBrushColor(tag.Background).ShouldBe(lightBackground);
            GetBrushColor(tag.BorderBrush).ShouldBe(Color.Parse("#1677ff"));
        });
    }

    [Fact]
    public void Icons_Use_The_Effective_Tag_Foreground()
    {
        var tag = new AtomUITag
        {
            TagColor   = "#1677ff",
            Variant    = TagVariant.Solid,
            IsClosable = true,
            Icon       = new PathIcon { Data = StreamGeometry.Parse("M0,0 L1,1") },
            Text       = "Icon"
        };

        ShowInWindow(tag, () =>
        {
            var foreground = GetBrushColor(tag.Foreground);
            var iconPresenter = tag.GetVisualDescendants()
                                   .OfType<IconPresenter>()
                                   .Single(presenter => ReferenceEquals(presenter.Icon, tag.Icon));
            var closeButton = tag.GetVisualDescendants()
                                 .OfType<AbstractIconButton>()
                                 .Single(button => button.Name == "PART_CloseButton");

            GetBrushColor(iconPresenter.IconBrush).ShouldBe(foreground);
            GetBrushColor(closeButton.IconBrush).ShouldBe(foreground);
        });
    }

    [Fact]
    public void Preset_Color_Tracks_Light_And_Dark_Theme_Snapshots()
    {
        var tag = new AtomUITag
        {
            TagColor = "red",
            Variant  = TagVariant.Outlined,
            Text     = "Theme"
        };
        var provider = new ThemeConfigProvider
        {
            Child  = tag,
            Config = new ThemeConfigBuilder().WithAlgorithms(ThemeAlgorithm.Default, ThemeAlgorithm.Dark).Build()
        };

        ShowInWindow(provider, () =>
        {
            AssertPresetPalette(tag, isDark: true);

            provider.Config = new ThemeConfigBuilder().WithAlgorithms(ThemeAlgorithm.Default).Build();
            Dispatcher.UIThread.RunJobs();

            AssertPresetPalette(tag, isDark: false);
        });
    }

    private static Color GetThemeColor(object key)
    {
        var application = Application.Current.ShouldNotBeNull();
        application!.TryGetResource(key, application.ActualThemeVariant, out var value).ShouldBeTrue();
        value.ShouldBeAssignableTo<IBrush>();
        return GetBrushColor((IBrush)value!);
    }

    private static Color GetBrushColor(IBrush? brush)
    {
        brush.ShouldNotBeNull();
        brush.ShouldBeAssignableTo<ISolidColorBrush>();
        return ((ISolidColorBrush)brush!).Color;
    }

    private static void AssertPresetPalette(AtomUITag tag, bool isDark)
    {
        var colors = AtomUI.Theme.Algorithms.PresetPalettes
                           .GetPresetPalette(
                               AtomUI.Theme.Algorithms.PresetPrimaryColor.Red,
                               isDark)
                           .ColorSequence;

        GetBrushColor(tag.Background).ShouldBe(colors[0]);
        GetBrushColor(tag.Foreground).ShouldBe(colors[6]);
        GetBrushColor(tag.BorderBrush).ShouldBe(colors[2]);
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 720,
            Height  = 240,
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
            Dispatcher.UIThread.RunJobs();
        }
    }
}
