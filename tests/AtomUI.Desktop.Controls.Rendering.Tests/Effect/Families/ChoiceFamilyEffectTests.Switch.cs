using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// ToggleSwitch and its identity-less SwitchKnob. Small-size Tokens need <c>SizeType=Small</c>; the content margins and font
/// sizes need on/off text, the icon sizes an icon content; the loading indicator Tokens need <c>IsLoading</c>. The loading
/// arc rotates with a looping animation, so loading rows slow the loop to a day per turn: both frames then show the arc at
/// the same angle and only the Token changes it.
/// </summary>
public partial class ChoiceFamilyEffectTests
{
    public static IEnumerable<object[]> ToggleSwitchRows() => MapRows("ToggleSwitch");

    [Theory]
    [MemberData(nameof(ToggleSwitchRows))]
    public void ToggleSwitch_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var steps   = new List<Action<ToggleSwitch>>();
        var loading = false;
        Action<Control>? arrange = null;

        void Small() => steps.Add(static s => s.SizeType = CustomizableSizeType.Small);
        void Checked() => steps.Add(static s => s.IsChecked = true);
        void Text() => steps.Add(static s =>
        {
            s.OnContent  = "ON";
            s.OffContent = "OFF";
        });
        void IconContent() => steps.Add(static s =>
        {
            s.OnContent  = new CheckOutlined();
            s.OffContent = new CloseOutlined();
        });

        if (token.EndsWith("SM", StringComparison.Ordinal) || token is "ControlHeight" or "UniformlyPaddingXXS")
        {
            // TrackHeightSM is ControlHeight / 2 and the small icon is TrackHeightSM - UniformlyPaddingXXS.
            Small();
        }

        switch (token)
        {
            case "ExtraInfoFontSize" or "ExtraInfoFontSizeSM" or "InnerMaxMargin" or "InnerMaxMarginSM" or "InnerMinMargin"
                or "InnerMinMarginSM" or "ColorTextLightSolid" or "FontSizeSM":
                Text();
                break;
            case "ContentIconSize" or "ContentIconSizeSM" or "ControlHeight" or "UniformlyPaddingXXS":
                IconContent();
                break;
            case "SwitchColor" or "ColorPrimary" or "HandleCheckedBg":
                Checked();
                break;
            case "ColorPrimaryHover":
                Checked();
                arrange = static c => Pseudo(c, ":pointerover");
                break;
            case "ColorTextTertiary":
                arrange = static c => Pseudo(c, ":pointerover");
                break;
            case "SwitchDisabledOpacity" or "OpacityLoading":
                steps.Add(static s => s.IsEnabled = false);
                break;
            case "OffStateLoadIndicatorColor" or "LineWidth" or "LoadingAnimationDuration":
                loading = true;
                break;
        }

        if (loading)
        {
            steps.Add(static s => s.IsLoading = true);
            if (token != "LoadingAnimationDuration")
            {
                arrange = static c => FreezeLoadingArc(c);
            }
        }

        // The knob sizes are Size values, which the perturbation table does not cover: double both sides.
        var options = token is "HandleSize" or "HandleSizeSM"
            ? new EffectOptions { Perturb = static value => new Size(((Size)value).Width * 2, ((Size)value).Height * 2) }
            : null;
        Run(() =>
        {
            var toggle = new ToggleSwitch();
            foreach (var step in steps)
            {
                step(toggle);
            }
            return toggle;
        }, arrange, ToggleSwitchTokens.Identity, token, options);
    }

    /// <summary>Slows the looping loading rotation to one turn a day, so two captures show the arc at the same angle.</summary>
    private static void FreezeLoadingArc(Control toggle)
    {
        var knob = toggle.GetVisualDescendants().OfType<SwitchKnob>().First();
        knob.SetValue(SwitchKnob.LoadingAnimationDurationProperty, TimeSpan.FromDays(1));
    }
}
