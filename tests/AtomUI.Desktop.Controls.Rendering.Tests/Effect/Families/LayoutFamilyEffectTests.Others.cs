using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using Xunit;
using AvaloniaBorder = Avalonia.Controls.Border;
using AvaloniaTextBlock = Avalonia.Controls.TextBlock;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Separator, Space (its Own add-on Tokens style the CompactSpaceAddOn), CompactSpaceAddOn, Masonry and GroupBox.
/// </summary>
public partial class LayoutFamilyEffectTests
{
    // ---- Separator ------------------------------------------------------------------------------------------------------

    internal static Separator CreateSeparator(
        string? title = "Title",
        CustomizableSizeType size = CustomizableSizeType.Middle,
        SeparatorTitlePosition position = SeparatorTitlePosition.Center,
        bool plain = false)
    {
        return new Separator
        {
            Width         = 360,
            Title         = title,
            SizeType      = size,
            TitlePosition = position,
            IsPlain       = plain
        };
    }

    /// <summary>A vertical separator between two texts: its inline margin moves the second text.</summary>
    private static Control CreateVerticalSeparatorRow() =>
        new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                new AvaloniaTextBlock { Text = "Text" },
                new Separator { Orientation = Orientation.Vertical },
                new AvaloniaTextBlock { Text = "Link" }
            }
        };

    public static IEnumerable<object[]> SeparatorRows() => MapRows("Separator");

    [Theory]
    [MemberData(nameof(SeparatorRows))]
    public void Separator_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        switch (token)
        {
            case "VerticalMarginInline" or "UniformlyMarginXS":
                Run(CreateVerticalSeparatorRow, null, SeparatorTokens.Identity, token,
                    new EffectOptions
                    {
                        Target = static row => ((StackPanel)row).Children.OfType<Separator>().Single(),
                        Region = static separator => (Control)separator.Parent!
                    });
                return;
            case "HorizontalMarginBlockSM":
                Run(static () => CreateSeparator(null, CustomizableSizeType.Small), null, SeparatorTokens.Identity, token);
                return;
            case "HorizontalMarginBlockLG" or "UniformlyMarginLG":
                Run(static () => CreateSeparator(null, CustomizableSizeType.Large), null, SeparatorTokens.Identity, token);
                return;
            case "HorizontalMarginBlock":
                Run(static () => CreateSeparator(null), null, SeparatorTokens.Identity, token);
                return;
            // The title padding is in em: a wide padding visibly shortens both 1px rails.
            case "TextPaddingInline":
                Run(static () => CreateSeparator(), null, SeparatorTokens.Identity, token,
                    new EffectOptions { Perturb = static _ => 6d });
                return;
            case "OrientationMarginPercent":
                Run(static () => CreateSeparator(position: SeparatorTitlePosition.Left), null, SeparatorTokens.Identity, token);
                return;
            case "FontSize":
                Run(static () => CreateSeparator(plain: true), null, SeparatorTokens.Identity, token);
                return;
            // The default 500 and the table perturbation (bold) render with the same face of the test font: perturb to a
            // light title instead.
            case "TitleFontWeight":
                Run(static () => CreateSeparator(), null, SeparatorTokens.Identity, token,
                    new EffectOptions { Perturb = static _ => FontWeight.Light });
                return;
            default:
                // A centred title: the with-text gutter margin, the text padding, the title weight / size / colour, the rails.
                Run(static () => CreateSeparator(), null, SeparatorTokens.Identity, token);
                return;
        }
    }

    // ---- Space / CompactSpaceAddOn ---------------------------------------------------------------------------------------

    /// <summary>A Space of three blocks above a compact space whose first item is an add-on.</summary>
    internal static Control CreateSpaceHost(
        CustomizableSizeType spaceSize = CustomizableSizeType.Small,
        CustomizableSizeType addOnSize = CustomizableSizeType.Middle,
        InputControlStyleVariant variant = InputControlStyleVariant.Outlined,
        InputControlStatus status = InputControlStatus.Default,
        bool enabled = true)
    {
        var space = new Space { SizeType = spaceSize };
        for (var index = 0; index < 3; index++)
        {
            space.Children.Add(new AvaloniaBorder { Width = 40, Height = 24, Background = Brushes.LightSteelBlue });
        }

        var compact = new CompactSpace { SizeType = addOnSize, IsEnabled = enabled };
        compact.Children.Add(new CompactSpaceAddOn { Content = "http://", StyleVariant = variant, Status = status });
        compact.Children.Add(new LineEdit { Width = 160, Text = "atomui.net" });
        return new StackPanel { Spacing = 16, Children = { space, compact } };
    }

    private static CompactSpaceAddOn AddOnOf(Control any) =>
        TopLevel.GetTopLevel(any)!.GetVisualDescendants().OfType<CompactSpaceAddOn>().First();

    public static IEnumerable<object[]> SpaceRows() => MapRows("Space");

    [Theory]
    [MemberData(nameof(SpaceRows))]
    public void Space_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var spaceSize = token switch
        {
            "GapMiddleSize" or "Spacing"  => CustomizableSizeType.Middle,
            "GapLargeSize" or "SpacingLG" => CustomizableSizeType.Large,
            _                             => CustomizableSizeType.Small
        };
        var addOnSize = token switch
        {
            "AddOnPaddingSM" or "ControlPaddingHorizontalSM" => CustomizableSizeType.Small,
            "AddOnPaddingLG" or "ControlPaddingHorizontal"   => CustomizableSizeType.Large,
            _                                                 => CustomizableSizeType.Middle
        };
        // The add-on Tokens are Space Own Tokens styling the CompactSpaceAddOn: compare the add-on.
        var addOnToken = token is "AddonBg" or "ColorFillAlter" or "AddOnPadding" or "AddOnPaddingSM" or "AddOnPaddingLG" or
            "UniformlyPaddingSM" or "ControlPaddingHorizontalSM" or "ControlPaddingHorizontal";
        Run(() => CreateSpaceHost(spaceSize, addOnSize), null, SpaceTokens.Identity, token,
            new EffectOptions
            {
                Target = static root => ((StackPanel)root).Children.OfType<Space>().Single(),
                Region = addOnToken ? AddOnOf : null
            });
    }

    public static IEnumerable<object[]> CompactSpaceAddOnRows() => MapRows("CompactSpaceAddOn");

    [Theory]
    [MemberData(nameof(CompactSpaceAddOnRows))]
    public void CompactSpaceAddOn_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var size = token switch
        {
            "FontSizeLG" or "BorderRadiusLG" or "ControlHeightLG" => CustomizableSizeType.Large,
            "FontSizeSM" or "BorderRadiusSM" or "ControlHeightSM" => CustomizableSizeType.Small,
            _                                                     => CustomizableSizeType.Middle
        };
        var variant = token is "ColorErrorBg" or "ColorWarningBg" ? InputControlStyleVariant.Filled : InputControlStyleVariant.Outlined;
        var status = token switch
        {
            "ColorErrorText" or "ColorError" or "ColorErrorBg"       => InputControlStatus.Error,
            "ColorWarningText" or "ColorWarning" or "ColorWarningBg" => InputControlStatus.Warning,
            _                                                        => InputControlStatus.Default
        };
        var enabled = token is not ("ColorTextDisabled" or "ColorBgContainerDisabled");
        Run(() => CreateSpaceHost(addOnSize: size, variant: variant, status: status, enabled: enabled), null,
            CompactSpaceAddOnTokens.Identity, token,
            new EffectOptions
            {
                Target  = AddOnOf,
                Perturb = token.StartsWith("BorderRadius", StringComparison.Ordinal) ? static _ => new CornerRadius(14) : null
            });
    }

    // ---- Masonry --------------------------------------------------------------------------------------------------------

    public static IEnumerable<object[]> MasonryRows() => MapRows("Masonry");

    [Theory]
    [MemberData(nameof(MasonryRows))]
    public void Masonry_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        // The item appear / glide (MotionDurationSlow) and leave (MotionDurationFast) durations: TimeSpan read-backs.
        Run(static () =>
        {
            var masonry = new Masonry { Width = 360, ColumnCount = 2 };
            masonry.Items.Add(new AvaloniaBorder { Height = 40, Background = Brushes.LightSteelBlue });
            masonry.Items.Add(new AvaloniaBorder { Height = 60, Background = Brushes.LightSteelBlue });
            return masonry;
        }, null, MasonryTokens.Identity, token);
    }

    // ---- GroupBox -------------------------------------------------------------------------------------------------------

    internal static GroupBox CreateGroupBox() =>
        new()
        {
            Width       = 360,
            HeaderTitle = "Group title",
            HeaderIcon  = new SettingOutlined(),
            Content     = new AvaloniaTextBlock { Text = "Group content" }
        };

    public static IEnumerable<object[]> GroupBoxRows() => MapRows("GroupBox");

    [Theory]
    [MemberData(nameof(GroupBoxRows))]
    public void GroupBox_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(CreateGroupBox, null, GroupBoxTokens.Identity, token,
            token == "BorderRadius" ? new EffectOptions { Perturb = static _ => new CornerRadius(16) } : null);
    }
}
