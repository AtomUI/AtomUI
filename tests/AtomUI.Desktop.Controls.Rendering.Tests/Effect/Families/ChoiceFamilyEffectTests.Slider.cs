using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.VisualTree;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Slider and its registered SliderTrack / SliderThumb parts. The parts are template parts of the Slider only: their themes
/// read the Slider's TokenResource, so <c>Controls[Slider]</c> reaches them. Mark Tokens need marks (two of them inside the
/// included range, two outside); hover Tokens need the Slider, the track or the thumb hovered; the vertical padding needs a
/// vertical Slider.
/// </summary>
public partial class ChoiceFamilyEffectTests
{
    public static IEnumerable<object[]> SliderRows() => MapRows("Slider");

    [Theory]
    [MemberData(nameof(SliderRows))]
    public void Slider_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var marks      = false;
        var vertical   = false;
        var disabled   = false;
        var autoLength = false;
        Action<Control>? arrange = null;
        switch (token)
        {
            case "MarkBorderColor" or "MarkBorderColorActive" or "MarkSize" or "ColorBgElevated" or "ColorText"
                or "ColorBorderSecondary":
                marks = true;
                break;
            case "MarkBorderColorHover" or "ColorFillContentHover":
                marks   = true;
                arrange = static c => Pseudo(TrackOf(c), ":pointerover");
                break;
            case "RailHoverBg" or "ColorFillSecondary":
                arrange = static c => Pseudo(TrackOf(c), ":pointerover");
                break;
            case "TrackHoverBg" or "ThumbCircleBorderHoverColor" or "ColorPrimaryBorderHover":
                arrange = static c => Pseudo(c, ":pointerover");
                break;
            case "ThumbCircleBorderActiveColor" or "ThumbCircleBorderThicknessHover" or "ThumbCircleSizeHover"
                or "ThumbOutlineColor" or "ThumbOutlineThickness" or "ColorPrimary" or "WaveAnimationRange":
                arrange = static c => Pseudo(ThumbOf(c), ":pointerover");
                break;
            case "ThumbCircleBorderColorDisabled" or "TrackBgDisabled" or "ColorBgContainerDisabled":
                disabled = true;
                break;
            case "ColorTextDisabled":
                // The disabled mark label and the disabled thumb / mark border (derived from it).
                marks    = true;
                disabled = true;
                break;
            case "SliderPaddingVertical":
                vertical = true;
                break;
            case "SliderTrackSize":
                // The cross size is SliderTrackSize plus the derived padding, which always adds up to ControlHeight; the
                // track size shows as the minimum length of a Slider that is not stretched (2 x SliderTrackSize).
                autoLength = true;
                break;
        }

        Run(() =>
        {
            var slider = new Slider { Value = 50 };
            if (vertical)
            {
                slider.Orientation = Orientation.Vertical;
                slider.Height      = 200;
            }
            else if (autoLength)
            {
                slider.HorizontalAlignment = HorizontalAlignment.Left;
            }
            else
            {
                slider.Width = 240;
            }
            if (marks)
            {
                slider.Marks =
                [
                    new SliderMark("0", 0),
                    new SliderMark("20", 20),
                    new SliderMark("80", 80),
                    new SliderMark("100", 100)
                ];
            }
            slider.IsEnabled = !disabled;
            return slider;
        }, arrange, SliderTokens.Identity, token);
    }

    private static SliderTrack TrackOf(Control slider) => slider.GetVisualDescendants().OfType<SliderTrack>().First();

    private static SliderThumb ThumbOf(Control slider) => slider.GetVisualDescendants().OfType<SliderThumb>().First();
}
