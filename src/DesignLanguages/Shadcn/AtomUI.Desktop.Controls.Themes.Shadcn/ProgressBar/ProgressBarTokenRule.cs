using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="ProgressBarToken"/>.</summary>
[ControlTokenRule]
internal sealed class ProgressBarTokenRule : IControlTokenRule<ShadcnToken, ProgressBarToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<ProgressBarToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        LineProgressTokenCalculator.Calculate(in context, global, token);
        token.LineBorderRadius      = new CornerRadius(100); // magic for capsule shape, should be a very large number
        token.LineStrokeThicknessLG = 8;   // antd progress line default
        token.LineStrokeThickness   = 6;
        token.LineStrokeThicknessSM = 4;
        token.LineProgressPadding = global.UniformlyPaddingXXS / 2;
        token.InnerLabelColor = global.ColorTextLabel;
        token.InnerLabelEmptyOpacity = 1;
        token.ProgressActiveMotionDuration = TimeSpan.FromSeconds(2.4); // antd progressActiveMotionDuration: '2.4s'
        // bg-primary / bg-muted over the whole line family, one bar height per style at every size (the registry has no size
        // variants); rounded-full keeps the capsule sentinel, literal radii clamp the same way, rounded-none is square.
        var progressLanguage = context.Language;
        token.DefaultColor          = progressLanguage.Primary;
        token.RemainingColor        = progressLanguage.Muted;
        token.LineStrokeThicknessLG = progressLanguage.ProgressHeight;
        token.LineStrokeThickness   = progressLanguage.ProgressHeight;
        token.LineStrokeThicknessSM = progressLanguage.ProgressHeight;
        token.LineBorderRadius = progressLanguage.ProgressRadius >= 100
            ? new CornerRadius(100)
            : new CornerRadius(progressLanguage.ProgressRadius);
    }
}
