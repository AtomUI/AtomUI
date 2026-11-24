using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="ProgressBarToken"/>.</summary>
[ControlTokenRule]
internal sealed class ProgressBarTokenRule : IControlTokenRule<PrimeNGToken, ProgressBarToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<ProgressBarToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGProgressBarTokens.EvaluateGlobal(in context, global);
        LineProgressTokenCalculator.Calculate(in context, global, token);
        token.LineBorderRadius      = new CornerRadius(100); // magic for capsule shape, should be a very large number
        token.LineStrokeThicknessLG = 8;   // antd progress line default
        token.LineStrokeThickness   = 6;
        token.LineStrokeThicknessSM = 4;
        token.LineProgressPadding = global.UniformlyPaddingXXS / 2;
        token.InnerLabelColor = global.ColorTextLabel;
        token.InnerLabelEmptyOpacity = 1;
        token.ProgressActiveMotionDuration = TimeSpan.FromSeconds(2.4); // antd progressActiveMotionDuration: '2.4s'
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGProgressBarTokens.EvaluateOwn(in context, global, token);
    }
}
