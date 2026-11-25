using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Rate and its identity-less RateItemsControl / RateItem, which read the Rate's TokenResource. The Rate shows two and a
/// half selected stars, so the star colour and the background colour are both visible; the hover scale needs a hovered
/// item and the dashed focus outline (BorderThickness) the keyboard focus start item.
/// </summary>
public partial class ChoiceFamilyEffectTests
{
    public static IEnumerable<object[]> RateRows() => MapRows("Rate");

    [Theory]
    [MemberData(nameof(RateRows))]
    public void Rate_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var size = token switch
        {
            "StarSizeSM" or "ControlHeightSM" => SizeType.Small,
            "StarSizeLG" or "ControlHeightLG" => SizeType.Large,
            _ => SizeType.Middle
        };
        Action<Control>? arrange = token switch
        {
            "StarHoverScale" => static c => Pseudo(FirstItem(c), ":pointerover"),
            "BorderThickness" => static c => FirstItem(c).IsFocusStartItem = true,
            _ => null
        };
        Run(() => new Rate { IsAllowHalf = true, Value = 2.5, SizeType = size }, arrange, RateTokens.Identity, token);
    }

    private static RateItem FirstItem(Control rate) => rate.GetVisualDescendants().OfType<RateItem>().First();
}
