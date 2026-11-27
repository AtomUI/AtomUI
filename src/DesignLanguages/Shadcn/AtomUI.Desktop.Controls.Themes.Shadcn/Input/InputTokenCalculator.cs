using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The global tokens the shadcn input family sees: the focused border
/// (<see cref="DesignToken.InputActiveBorderColor" />) is the <c>--ring</c> variable (not the theme primary), the
/// resting border the <c>--input</c> variable, the dark field background the style's share of the input variable,
/// and one radius class at every size (the registry input has no size variants).
/// </summary>
internal static class InputTokenCalculator
{
    public static void CalculateGlobal(in TokenEvaluationContext<ShadcnToken> context, DesignToken global)
    {
        var field = context.Language.GetFieldRecipe(global, context.Variant);
        global.InputActiveBorderColor = field.FocusBorderColor;
        global.ColorPrimaryHover      = field.FocusBorderColor;
        global.ColorBorder            = field.BorderColor;
        global.ColorBgContainer       = field.OutlinedBackground;
        global.BorderRadiusMD         = field.CornerRadius;
        global.BorderRadiusLG         = field.CornerRadius;
        global.BorderRadiusSM         = field.CornerRadius;
    }

    /// <summary>
    /// <see cref="CalculateGlobal" /> plus the field frame of the style: Sera draws only the bottom border without a
    /// focus shadow, Luma and Rhea fill the field with half the input color.
    /// </summary>
    public static (bool Transparent, bool Bottom) CalculateFieldGlobal(in TokenEvaluationContext<ShadcnToken> context, DesignToken global)
    {
        CalculateGlobal(in context, global);
        var field = context.Language.GetFieldRecipe(global, context.Variant);
        if (field.BottomBorderOnly)
        {
            global.ColorPrimaryHover        = global.ColorBorder;
            global.InputActiveShadow        = field.FocusRing;
            global.InputErrorActiveShadow   = default;
            global.InputWarningActiveShadow = default;
        }
        else if (field.TransparentBorder)
        {
            global.ColorBgContainer = field.GetBackground(global.ColorBorder);
        }

        return (field.TransparentBorder, field.BottomBorderOnly);
    }
}

/// <summary>
/// Shapes shared by the shadcn floating surfaces.
/// </summary>
internal static class ShadcnOverlayShapes
{
    /// <summary>
    /// TooltipPrimitive.Arrow: a size-2.5 square rotated 45° with rounded-[2px] corners, moved up by half its size plus
    /// 2px, so 5√2 - 2 of its unrounded corner shows beyond the box: a right-angled arrow twice that wide with a 2px
    /// tip and no flare.
    /// </summary>
    public static readonly double ArrowSize = 2 * (5 * Math.Sqrt(2) - 2);

    /// <summary>
    /// The <c>rounded-[2px]</c> corner of the arrow square.
    /// </summary>
    public const double ArrowTipRadius = 2;
}
