using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// PrimeNG motion. A preset's <c>transition.duration</c> only times the CSS transition of state colors (Nora: 0s, see
/// <c>PrimeNGToken.HasInstantTransitions</c>); the enter / leave animations of <c>@primeuix/styles</c> 3.1.1 are
/// fixed and the same in every preset: anchored overlays (select, popover, menu, confirm popup) and dialogs fade and
/// scale in over 300ms, the drawer slides over 0.5s, collapsible content (accordion, panel, tree) over 0.2s, the toast
/// moves over its own <c>toast.transition.duration</c> 0.3s, the tabs ink bar over 250ms, the carousel over 500ms and
/// the tooltip fades in over 250ms; the toggle switch knob slides over its <c>slide.duration</c> (Nora 0.1s). When a
/// preset sets the state transitions to 0, the stages of those owners restore the duration their animation reads in
/// their own scope, so the colors stay instant while the components still animate.
/// </summary>
internal static class PrimeNGMotion
{
    internal static readonly TimeSpan AnchoredOverlay = TimeSpan.FromMilliseconds(300);
    internal static readonly TimeSpan Dialog          = TimeSpan.FromMilliseconds(300);
    internal static readonly TimeSpan Drawer          = TimeSpan.FromMilliseconds(500);
    internal static readonly TimeSpan Collapsible     = TimeSpan.FromMilliseconds(200);
    internal static readonly TimeSpan Toast           = TimeSpan.FromMilliseconds(300);
    internal static readonly TimeSpan TabsInkBar      = TimeSpan.FromMilliseconds(250);
    internal static readonly TimeSpan Carousel        = TimeSpan.FromMilliseconds(500);
    internal static readonly TimeSpan ToolTipFade     = TimeSpan.FromMilliseconds(250);
    internal static readonly TimeSpan ToggleSlide     = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Whether the preset makes state transitions instant, so component animations need their own durations.
    /// </summary>
    internal static bool HasInstantTransitions(in TokenEvaluationContext<PrimeNGToken> context) =>
        context.Language.HasInstantTransitions(context.Variant);

    /// <summary>
    /// Restores the scope's <c>MotionDurationMid</c> and / or <c>MotionDurationSlow</c> (whichever the owner's
    /// animation reads) under a preset with instant state transitions, unless motion is turned off.
    /// </summary>
    internal static void Restore(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal,
                                 TimeSpan? mid = null, TimeSpan? slow = null)
    {
        if (!controlGlobal.EnableMotion || !HasInstantTransitions(context))
        {
            return;
        }
        if (mid is { } m)
        {
            controlGlobal.MotionDurationMid = m;
        }
        if (slow is { } s)
        {
            controlGlobal.MotionDurationSlow = s;
        }
    }
}
