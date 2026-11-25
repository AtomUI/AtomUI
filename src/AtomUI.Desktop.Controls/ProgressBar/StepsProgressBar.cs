using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Resources;
using Avalonia.LogicalTree;

namespace AtomUI.Desktop.Controls;

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

    private protected override OwnerTokenSet CreateLineSizeTokenSet() =>
        CreateLineSizeTokenSetFromKeys(LineSizeTokenResourceMode == LineSizeTokenMode.FontAndStepPresets ? s_stepKeys : []);

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

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        SetResourceStepSize(DEFAULT_CHUNK_SPACE, 0);
        base.OnDetachedFromLogicalTree(e);
    }
}
