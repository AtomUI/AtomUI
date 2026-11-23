using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// RadioButton and RadioButtonGroup. The dot only renders while checked; its size is DotSize while enabled and
/// RadioSize - 2 x DotPadding while disabled, so the dot metrics need the matching state.
/// </summary>
public partial class ChoiceFamilyEffectTests
{
    public static IEnumerable<object[]> RadioButtonRows() => MapRows("RadioButton");

    [Theory]
    [MemberData(nameof(RadioButtonRows))]
    public void RadioButton_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Action<RadioButton>? init = token switch
        {
            // Checked: the RadioBgColor fill, the RadioColor dot and its DotSize (FontSizeLG / LineWidth derive it).
            "RadioBgColor" or "RadioColor" or "DotSize" or "ColorPrimary" or "ColorWhite" or "LineWidth" or "FontSizeLG" =>
                static r => r.IsChecked = true,
            // Disabled and checked: the dot is RadioSize - 2 x DotPadding and painted DotColorDisabled.
            "DotPadding" or "DotColorDisabled" or "ColorTextDisabled" or "ColorBgContainerDisabled" => static r =>
            {
                r.IsChecked = true;
                r.IsEnabled = false;
            },
            _ => null
        };
        Run(() =>
        {
            var radio = new RadioButton { Content = "Radio" };
            init?.Invoke(radio);
            return radio;
        }, token is "IndicatorHoverBorderColor" ? static c => Pseudo(c, ":pointerover") : null,
            RadioButtonTokens.Identity, token);
    }

    public static IEnumerable<object[]> RadioButtonGroupRows() => MapRows("RadioButtonGroup");

    [Theory]
    [MemberData(nameof(RadioButtonGroupRows))]
    public void RadioButtonGroup_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        // SpacingXS spaces the group's items: the second item moves.
        Run(static () => new RadioButtonGroup
        {
            ItemsSource = new[]
            {
                new RadioButtonOption { Content = "Apple" },
                new RadioButtonOption { Content = "Pear" },
                new RadioButtonOption { Content = "Orange" }
            }
        }, null, RadioButtonGroupTokens.Identity, token);
    }
}
