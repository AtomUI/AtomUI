using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Rendering.Composition;
using Avalonia.VisualTree;

namespace AtomUI.GalleryPerformance;

/// <summary>
/// Stops the endless loading-icon rotations (<see cref="Icon.LoadingAnimation"/>) before a pixel capture. The rotation
/// is a compositor animation that runs whenever the icon is visible and does not follow the motion switch, so two
/// captures of one build can catch it at angles the two-capture animation mask does not cover. Freezing leaves every
/// icon at angle 0, so every capture, of either build, draws the same frame. The text caret of a focused text box
/// blinks on a timer for the same reason; freezing stops the blink with the caret shown.
/// </summary>
/// <remarks>Opt-in through <see cref="Argument"/>; a baseline build must support it for the captures to match.</remarks>
internal static class SnapshotFreeze
{
    public const string Argument = "--freeze-loading-icons";

    /// <summary>
    /// Stops every loading rotation under <paramref name="root"/> and resets the icons to angle 0, in two composition
    /// batches separated by <paramref name="pump"/>; returns how many icons were rotating.
    /// </summary>
    /// <remarks>
    /// The icon stops the rotation and sets <c>RotationAngle</c> to 0, but 0 is already the client-side value, so no
    /// change is sent and the render side keeps the angle the spin had reached. Writing the angle in the same batch as
    /// the stop is not enough either: the render side may apply the write before the stop. So the stop is committed
    /// first (<paramref name="pump"/> renders one frame), then the angle is written in its own batch.
    /// </remarks>
    /// <param name="root">The captured top level.</param>
    /// <param name="pump">Runs the dispatcher and renders at least one frame.</param>
    public static int FreezeLoadingIcons(Visual root, Action pump)
    {
        FreezeCarets(root);
        var icons = root.GetSelfAndVisualDescendants()
                        .OfType<Icon>()
                        .Where(icon => icon.LoadingAnimation != IconAnimation.None)
                        .ToList();
        if (icons.Count == 0)
        {
            return 0;
        }

        foreach (var icon in icons)
        {
            icon.LoadingAnimation = IconAnimation.None;
        }
        pump();

        // Only icons that are drawn: a hidden icon's angle never shows, and touching its visual would only mark a
        // region dirty. Partial repaints blend anti-aliased edges at the region borders differently from a full
        // repaint, so callers repaint the whole frame afterwards (see the tools' capture code).
        foreach (var icon in icons.Where(icon => icon.IsEffectivelyVisible))
        {
            if (ElementComposition.GetElementVisual(icon) is { } visual)
            {
                // A different value first, so the final 0 is a change and is sent.
                visual.RotationAngle = 0.001f;
                visual.RotationAngle = 0;
            }
        }
        pump();
        return icons.Count;
    }

    // Without a blink interval the presenter runs no blink timer; ShowCaret then leaves the caret shown, whatever
    // phase the blink had reached. Only presenters of a focused text box draw a caret.
    private static void FreezeCarets(Visual root)
    {
        foreach (var presenter in root.GetSelfAndVisualDescendants().OfType<TextPresenter>())
        {
            presenter.CaretBlinkInterval = TimeSpan.Zero;
            if (presenter.TemplatedParent is TextBox { IsFocused: true })
            {
                presenter.ShowCaret();
            }
        }
    }
}
