using AtomUI.Controls.Themes.PrimeNG;
using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="SkeletonToken"/>.</summary>
[ControlTokenRule]
internal sealed class SkeletonTokenRule : IControlTokenRule<PrimeNGToken, SkeletonToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<SkeletonToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGSkeletonTokens.EvaluateGlobal(in context, global);
        SkeletonTokenCalculator.Calculate(in context, global, token);
        token.GradientFromColor = global.ColorFillContent;
        token.GradientToColor = global.ColorFill;
        token.ParagraphLineHeight = global.ControlHeight / 2;

        token.ParagraphMarginTop    = new Thickness(0, global.UniformlyMarginLG + global.UniformlyMarginXXS, 0, 0);
        token.AvatarMarginRight = new Thickness(0, 0, global.UniformlyMargin, 0);
        token.ParagraphLineRoundCornerRadius = new CornerRadius(token.ParagraphLineHeight / 2);
        token.LoadingBackgroundStart  = CreateLoadingBackground(-3.0, 1.0, token.GradientFromColor, token.GradientToColor);
        token.LoadingBackgroundMiddle = CreateLoadingBackground(-1.5, 2.5, token.GradientFromColor, token.GradientToColor);
        token.LoadingBackgroundEnd    = CreateLoadingBackground(0.0, 4.0, token.GradientFromColor, token.GradientToColor);
        token.ImageColor              = Color.FromRgb(0xbf, 0xbf, 0xbf);

        var imageSizeBase = global.ControlHeight * 1.5;
        token.ImageSize             = imageSizeBase;
        token.ImageContainerSize    = imageSizeBase * 2;
        token.ImageContainerMaxSize = imageSizeBase * 4;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGSkeletonTokens.EvaluateOwn(in context, global, token);
        // PrimeNG: the formulas that read a value the preset replaced above run again with it, as the previous
        // staged evaluation did (its second round kept the preset's value).
        token.LoadingBackgroundStart  = CreateLoadingBackground(-3.0, 1.0, token.GradientFromColor, token.GradientToColor);
        token.LoadingBackgroundMiddle = CreateLoadingBackground(-1.5, 2.5, token.GradientFromColor, token.GradientToColor);
        token.LoadingBackgroundEnd    = CreateLoadingBackground(0.0, 4.0, token.GradientFromColor, token.GradientToColor);
    }



    private static LinearGradientBrush CreateLoadingBackground(double startX, double endX, Color fromColor, Color toColor)
    {
        return new LinearGradientBrush
        {
            StartPoint = new RelativePoint(startX, 0.5, RelativeUnit.Relative),
            EndPoint   = new RelativePoint(endX, 0.5, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop { Offset = 0.25, Color = fromColor },
                new GradientStop { Offset = 0.37, Color = toColor },
                new GradientStop { Offset = 0.63, Color = fromColor }
            }
        };
    }
}
