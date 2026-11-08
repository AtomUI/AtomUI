using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Schema;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Xunit;
using AtomCheckBox = AtomUI.Desktop.Controls.CheckBox;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Token effect tests of the choice family (docs/engineering/development/theme-effect-tests.md): CheckBox, CheckBoxGroup,
/// RadioButton, RadioButtonGroup, ToggleSwitch, Slider (with its registered SliderTrack / SliderThumb parts) and Rate.
/// Every Active Own Token and every consumed global Token of each member visibly changes the member when overridden through
/// <c>Controls[member]</c>. The rows come from the generated consumption map. The identity-less parts of the family
/// (CheckBoxIndicator, RadioIndicator, SwitchKnob, RateItem, RateItemsControl) have a single host and read that host's
/// TokenResource explicitly, so their reads are map rows of the host and no owner-read lists are needed.
/// </summary>
public partial class ChoiceFamilyEffectTests
{
    // ---- CheckBox ---------------------------------------------------------------------------

    public static IEnumerable<object[]> CheckBoxRows() => MapRows("CheckBox");

    [Theory]
    [MemberData(nameof(CheckBoxRows))]
    public void CheckBox_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Action<Control>? init = token switch
        {
            // The checked indicator: primary fill, the white mark and its size.
            "CheckedMarkSize" or "ColorPrimary" or "ColorWhite" => static c => ((AtomCheckBox)c).IsChecked = true,
            // The tristate mark is a FontSizeLG-derived square.
            "IndicatorTristateMarkSize" or "FontSizeLG" => static c =>
            {
                ((AtomCheckBox)c).IsThreeState = true;
                ((AtomCheckBox)c).IsChecked    = null;
            },
            "ColorTextDisabled" or "ColorBgContainerDisabled" => static c =>
            {
                ((AtomCheckBox)c).IsChecked = true;
                c.IsEnabled                 = false;
            },
            "ColorPrimaryHover" => static c => ((AtomCheckBox)c).IsChecked = true,
            _ => null
        };
        Action<Control>? arrange = token is "ColorPrimaryHover" ? static c => Pseudo(c, ":pointerover") : null;
        Run(() =>
        {
            var checkBox = new AtomCheckBox { Content = "CheckBox" };
            init?.Invoke(checkBox);
            return checkBox;
        }, arrange, CheckBoxTokens.Identity, token);
    }

    // ---- CheckBoxGroup ----------------------------------------------------------------------

    public static IEnumerable<object[]> CheckBoxGroupRows() => MapRows("CheckBoxGroup");

    [Theory]
    [MemberData(nameof(CheckBoxGroupRows))]
    public void CheckBoxGroup_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        // SpacingXS spaces the group's items: the second item moves.
        Run(static () => new CheckBoxGroup
        {
            ItemsSource = new[]
            {
                new CheckBoxOption { Content = "Apple" },
                new CheckBoxOption { Content = "Pear" },
                new CheckBoxOption { Content = "Orange" }
            }
        }, null, CheckBoxGroupTokens.Identity, token);
    }

    // ---- shared helpers -----------------------------------------------------------------------

    private static void Run(
        Func<Control> create,
        Action<Control>? arrange,
        ControlTokenIdentity identity,
        string token,
        EffectOptions? options = null)
    {
        TokenEffectHarness.AssertHasEffect(create, identity, token, arrange, options: options);
    }

    private static void Pseudo(Control control, string pseudoClass) =>
        ((IPseudoClasses)control.Classes).Add(pseudoClass);

    private static IEnumerable<object[]> MapRows(string controlId)
    {
        try
        {
            return EffectTestData.For("AtomUI", controlId).ToArray();
        }
        catch (ArgumentException)
        {
            // Registered parts whose visuals follow their host read the host's TokenResource and have no rows of their own.
            return [];
        }
    }
}
