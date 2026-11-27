using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Resources;
using Avalonia.LogicalTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A line progress bar divided into equal steps. In the <c>FontAndStepPresets</c> mode, which its theme sets, it
/// reads the info margin, the gap between steps and the minimum width of a step from the
/// <see cref="StepsProgressBarToken" />; without that mode the built-in defaults are used.
/// </summary>
public partial class StepsProgressBar : AbstractGeneralStepsProgressBar
{
    private static readonly object[] s_stepKeys =
    [
        StepsProgressBarTokenKind.LineExtraInfoMargin,
        StepsProgressBarTokenKind.ProgressStepMarginInlineEnd,
        StepsProgressBarTokenKind.ProgressStepMinWidth
    ];

    public StepsProgressBar()
    {
    }

    /// <summary>
    /// Returns the Token set with the font size presets followed by the keys of the info margin, the gap between
    /// steps and the minimum width of a step in the <c>FontAndStepPresets</c> mode, and without own keys in any
    /// other mode.
    /// </summary>
    private protected override OwnerTokenSet CreateLineSizeTokenSet() =>
        CreateLineSizeTokenSetFromKeys(LineSizeTokenResourceMode == LineSizeTokenMode.FontAndStepPresets ? s_stepKeys : []);

    /// <summary>
    /// Reads the info margin, the gap between steps and the minimum width of a step from the Token set in the
    /// <c>FontAndStepPresets</c> mode and applies them. Without that mode the margin is cleared, the gap returns to
    /// the built-in default and the minimum width is 0. The base member is called with <see langword="true" /> also
    /// when this spacing or size changed.
    /// </summary>
    private protected override void OnLineSizeTokensChanged(bool fontPresetsChanged)
    {
        var usesStepTokens = LineSizeTokenResourceMode == LineSizeTokenMode.FontAndStepPresets;
        var margin = usesStepTokens ? ReadLineSizeToken(LineFontPresetCount, 0) : (double?)null;
        var gap = usesStepTokens ? ReadLineSizeToken(LineFontPresetCount + 1, DEFAULT_CHUNK_SPACE) : DEFAULT_CHUNK_SPACE;
        var width = usesStepTokens ? ReadLineSizeToken(LineFontPresetCount + 2, 0) : 0;
        var spacingChanged = SetResourceLineSpacing(null, margin);
        var sizeChanged = SetResourceStepSize(gap, width);
        base.OnLineSizeTokensChanged(fontPresetsChanged || spacingChanged || sizeChanged);
    }

    /// <summary>
    /// Sets the step gap back to the built-in default and the minimum step width to 0, then calls the base method.
    /// </summary>
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        SetResourceStepSize(DEFAULT_CHUNK_SPACE, 0);
        base.OnDetachedFromLogicalTree(e);
    }
}
