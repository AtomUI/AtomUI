using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Themes.Shadcn.Algorithms.Controls;

/// <summary>
/// The control-global overrides shared by the shadcn input family: the focused border is the <c>--ring</c>
/// variable (shadcn's focus ring colour, not the theme primary), the resting border the <c>--input</c> variable
/// and the field background the style's input fill (Nova: transparent in light, input 30% in dark; design 5.5).
/// </summary>
internal static class ShadcnInputStage
{
    public static void Apply(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        var dark    = context.Appearance == ThemeAppearance.Dark;
        var surface = ShadcnBaseColors.Resolve(controlGlobal.ColorNeutral, dark);
        controlGlobal.ColorPrimary      = surface.Ring;
        controlGlobal.ColorPrimaryHover = surface.Ring;
        controlGlobal.ColorBorder       = surface.Input;
        controlGlobal.ColorBgContainer  = dark
            ? ShadcnBaseColors.CompositeOver(surface.Input, 0.3, surface.Background)
            : Colors.Transparent;
    }
}

/// <summary>LineEdit under the shadcn styles: see <see cref="ShadcnInputStage"/>.</summary>
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova.Dark", 1)]
internal sealed class ShadcnLineEditTokens : IControlTokenAlgorithm<LineEditToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        ShadcnInputStage.Apply(context, controlGlobal);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, LineEditToken token)
    {
    }
}

/// <summary>TextArea under the shadcn styles: see <see cref="ShadcnInputStage"/>.</summary>
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova.Dark", 1)]
internal sealed class ShadcnTextAreaTokens : IControlTokenAlgorithm<TextAreaToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        ShadcnInputStage.Apply(context, controlGlobal);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, TextAreaToken token)
    {
    }
}

/// <summary>Select under the shadcn styles: see <see cref="ShadcnInputStage"/>.</summary>
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova.Dark", 1)]
internal sealed class ShadcnSelectTokens : IControlTokenAlgorithm<SelectToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        ShadcnInputStage.Apply(context, controlGlobal);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, SelectToken token)
    {
    }
}

/// <summary>Cascader under the shadcn styles: see <see cref="ShadcnInputStage"/>.</summary>
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova.Dark", 1)]
internal sealed class ShadcnCascaderTokens : IControlTokenAlgorithm<CascaderToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        ShadcnInputStage.Apply(context, controlGlobal);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, CascaderToken token)
    {
    }
}

/// <summary>DatePicker under the shadcn styles: see <see cref="ShadcnInputStage"/>.</summary>
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova.Dark", 1)]
internal sealed class ShadcnDatePickerTokens : IControlTokenAlgorithm<DatePickerToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        ShadcnInputStage.Apply(context, controlGlobal);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, DatePickerToken token)
    {
    }
}

/// <summary>TimePicker under the shadcn styles: see <see cref="ShadcnInputStage"/>.</summary>
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova.Dark", 1)]
internal sealed class ShadcnTimePickerTokens : IControlTokenAlgorithm<TimePickerToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        ShadcnInputStage.Apply(context, controlGlobal);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, TimePickerToken token)
    {
    }
}

/// <summary>AutoComplete under the shadcn styles: see <see cref="ShadcnInputStage"/>.</summary>
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova.Dark", 1)]
internal sealed class ShadcnAutoCompleteTokens : IControlTokenAlgorithm<AutoCompleteToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        ShadcnInputStage.Apply(context, controlGlobal);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, AutoCompleteToken token)
    {
    }
}

/// <summary>Mentions under the shadcn styles: see <see cref="ShadcnInputStage"/>.</summary>
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova.Dark", 1)]
internal sealed class ShadcnMentionsTokens : IControlTokenAlgorithm<MentionsToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        ShadcnInputStage.Apply(context, controlGlobal);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, MentionsToken token)
    {
    }
}
