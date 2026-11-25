using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Xunit;
using AvaloniaBorder = Avalonia.Controls.Border;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Badge members of the tag / badge family. The badges run standalone (no decorated target), so the indicator is laid out
/// inside the badge's bounds; the indicator shadows (a ColorBorderBg ring) are compared on a dark backdrop where a white
/// ring is visible, and the comparison region grows by a few pixels to include the ring outside the indicator.
/// </summary>
public partial class TagBadgeFamilyEffectTests
{
    private const double ShadowRegionInflation = 4;

    // ---- CountBadge ---------------------------------------------------------------------------

    public static IEnumerable<object[]> CountBadgeRows() => MapRows("CountBadge");

    [Theory]
    [MemberData(nameof(CountBadgeRows))]
    public void CountBadge_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Action<CountBadge> setup = token switch
        {
            "IndicatorHeightSM" or "TextFontSizeSM" or "CornerRadiusSM" => static b => b.Size = CountBadgeSize.Small,
            // An overflowing count is wider than the minimum width, so the horizontal text padding shows.
            "TextPadding" or "UniformlyPaddingXXS" => static b => b.Count = 1000,
            _ => static _ => { }
        };
        var options = token switch
        {
            // The default radius (the indicator height) already rounds the indicator into a capsule; a larger radius looks
            // the same, so the row squares the corners instead.
            "CornerRadius" or "CornerRadiusSM" => new EffectOptions { Perturb = static _ => new CornerRadius(0) },
            "ShadowColor" or "ColorBorderBg" => new EffectOptions { RegionInflation = ShadowRegionInflation },
            "ShadowSize" or "LineWidth" => OnDarkBackdrop<CountBadge>(),
            _ => null
        };

        Run(() => Backdrop(token is "ShadowSize" or "LineWidth", () =>
        {
            var badge = new CountBadge { Count = 5, IsMotionEnabled = false };
            setup(badge);
            return badge;
        }), null, CountBadgeTokens.Identity, token, options);
    }

    // ---- DotBadge -----------------------------------------------------------------------------

    public static IEnumerable<object[]> DotBadgeRows() => MapRows("DotBadge");

    [Theory]
    [MemberData(nameof(DotBadgeRows))]
    public void DotBadge_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Action<DotBadge> setup = token switch
        {
            "ColorSuccess" => static b => b.Status = DotBadgeStatus.Success,
            "ColorWarning" => static b => b.Status = DotBadgeStatus.Warning,
            "ColorInfo" => static b => b.Status = DotBadgeStatus.Processing,
            "ColorTextPlaceholder" => static b => b.Status = DotBadgeStatus.Default,
            _ => static _ => { }
        };
        var options = token switch
        {
            "ShadowColor" or "ColorBorderBg" => new EffectOptions { RegionInflation = ShadowRegionInflation },
            "ShadowSize" or "LineWidth" => OnDarkBackdrop<DotBadge>(),
            _ => null
        };

        Run(() => Backdrop(token is "ShadowSize" or "LineWidth", () =>
        {
            var badge = new DotBadge { Text = "Dot Badge", IsMotionEnabled = false };
            setup(badge);
            return badge;
        }), null, DotBadgeTokens.Identity, token, options);
    }

    // ---- RibbonBadge --------------------------------------------------------------------------

    public static IEnumerable<object[]> RibbonBadgeRows() => MapRows("RibbonBadge");

    [Theory]
    [MemberData(nameof(RibbonBadgeRows))]
    public void RibbonBadge_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var options = token switch
        {
            // The ribbon offset is the size of the folded corner under the ribbon.
            "Offset" => new EffectOptions { Perturb = static value => (Point)value + new Point(4, 4) },
            "CornerDarkenAmount" => new EffectOptions { Perturb = static value => (int)value + 30 },
            // The fold is scaled vertically by 0.75 (antd scaleY(.75)); a full-height fold is visibly taller.
            "CornerTransform" => new EffectOptions
            {
                Perturb = static _ => new ImmutableTransform(Matrix.CreateScale(1, 2))
            },
            _ => null
        };

        Run(static () => new RibbonBadge { Text = "Ribbon Badge" }, null, RibbonBadgeTokens.Identity, token, options);
    }

    // ---- helpers ------------------------------------------------------------------------------

    /// <summary>The badge, or the badge on a dark backdrop when its white shadow ring must be visible.</summary>
    private static Control Backdrop(bool dark, Func<Control> createBadge)
    {
        var badge = createBadge();
        return dark
            ? new AvaloniaBorder { Background = Brushes.Black, Padding = new Thickness(12), Child = badge }
            : badge;
    }

    private static EffectOptions OnDarkBackdrop<TBadge>()
        where TBadge : Control =>
        new()
        {
            Target          = static root => ((AvaloniaBorder)root).Child as TBadge ?? throw new InvalidOperationException(),
            RegionInflation = ShadowRegionInflation
        };
}
