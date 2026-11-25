using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Resources;
using Avalonia.LogicalTree;

namespace AtomUI.Desktop.Controls;

public partial class ProgressBar : AbstractGeneralProgressBar
{
    private static readonly object[] s_progressKeys =
    [
        ProgressBarTokenKind.LineStrokeThicknessLG,
        ProgressBarTokenKind.LineStrokeThickness,
        ProgressBarTokenKind.LineStrokeThicknessSM,
        ProgressBarTokenKind.LineProgressPadding,
        ProgressBarTokenKind.LineExtraInfoMargin,
        ProgressBarTokenKind.InnerLabelColor,
        ControlTokenResourceKey.Global(ProgressBarTokens.Identity, SharedTokenKind.ColorTextLightSolid)
    ];

    private static readonly (double Large, double Middle, double Small) s_defaultStrokes =
        (LARGE_STROKE_THICKNESS, MIDDLE_STROKE_THICKNESS, SMALL_STROKE_THICKNESS);
    private (double Large, double Middle, double Small) _strokePresets = s_defaultStrokes;

    static ProgressBar()
    {
        AffectsMeasure<ProgressBar>(EffectiveStrokeThicknessProperty);
    }

    public ProgressBar()
    {
    }

    private protected override OwnerTokenSet CreateLineSizeTokenSet() =>
        CreateLineSizeTokenSetFromKeys(LineSizeTokenResourceMode == LineSizeTokenMode.FontAndStrokePresets ? s_progressKeys : []);

    private protected override void OnLineSizeTokensChanged(bool fontPresetsChanged)
    {
        var usesProgressTokens = LineSizeTokenResourceMode == LineSizeTokenMode.FontAndStrokePresets;
        var next = usesProgressTokens
            ? (ReadLineSizeToken(LineFontPresetCount, s_defaultStrokes.Large),
               ReadLineSizeToken(LineFontPresetCount + 1, s_defaultStrokes.Middle),
               ReadLineSizeToken(LineFontPresetCount + 2, s_defaultStrokes.Small))
            : s_defaultStrokes;
        var spacingChanged = usesProgressTokens
            ? SetResourceLineSpacing(ReadLineSizeToken(LineFontPresetCount + 3, 0), ReadLineSizeToken(LineFontPresetCount + 4, 0))
            : SetResourceLineSpacing(null, null);
        if (next != _strokePresets)
        {
            _strokePresets = next;
            _sizeTypeThresholdValuesInitialized = false;
            UpdateEffectiveSizeTypeFromExplicitDimension();
            CalculateStrokeThickness();
        }
        base.OnLineSizeTokensChanged(fontPresetsChanged || spacingChanged);
        SetInnerLabelColorCandidates(
            usesProgressTokens ? ReadLineColorToken(LineFontPresetCount + 5) : null,
            usesProgressTokens ? ReadLineColorToken(LineFontPresetCount + 6) : null);
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _strokePresets = s_defaultStrokes;
        SetInnerLabelColorCandidates(null, null);
        base.OnDetachedFromLogicalTree(e);
    }

    private protected override double QueryLineStrokeThickness(SizeType sizeType) => sizeType switch
    {
        SizeType.Large => _strokePresets.Large,
        SizeType.Small => _strokePresets.Small,
        _              => _strokePresets.Middle
    };
}
