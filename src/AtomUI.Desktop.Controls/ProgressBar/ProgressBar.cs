using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Resources;
using Avalonia.LogicalTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A line progress bar. In the <c>FontAndStrokePresets</c> mode, which its theme sets, it reads the stroke
/// thickness of the three size presets, the inner padding, the info margin and the candidates of the inner
/// label color from Tokens; without that mode the stroke thickness falls back to the built-in defaults and no
/// Token spacing or label color is applied.
/// </summary>
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

    /// <summary>
    /// Returns the Token set with the font size presets followed by the keys of the stroke thickness of the three
    /// size presets, the inner padding, the info margin and the two candidates of the inner label color in the
    /// <c>FontAndStrokePresets</c> mode, and without own keys in any other mode.
    /// </summary>
    private protected override OwnerTokenSet CreateLineSizeTokenSet() =>
        CreateLineSizeTokenSetFromKeys(LineSizeTokenResourceMode == LineSizeTokenMode.FontAndStrokePresets ? s_progressKeys : []);

    /// <summary>
    /// Reads the stroke thickness presets, the inner padding, the info margin and the candidates of the inner label
    /// color from the Token set in the <c>FontAndStrokePresets</c> mode. A new stroke thickness is applied to the
    /// effective size type and the stroke thickness at once. Without that mode the stroke presets return to the
    /// built-in defaults and no Token spacing or label color is applied. The base member is called with
    /// <see langword="true" /> also when the spacing changed.
    /// </summary>
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

    /// <summary>
    /// Puts the stroke thickness presets back to the built-in defaults and removes the Token candidates of the inner
    /// label color, then calls the base method.
    /// </summary>
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _strokePresets = s_defaultStrokes;
        SetInnerLabelColorCandidates(null, null);
        base.OnDetachedFromLogicalTree(e);
    }

    /// <summary>
    /// Returns the thickness of the bar line for a size type: the preset read from the Tokens in the
    /// <c>FontAndStrokePresets</c> mode, or otherwise the built-in default.
    /// </summary>
    private protected override double QueryLineStrokeThickness(SizeType sizeType) => sizeType switch
    {
        SizeType.Large => _strokePresets.Large,
        SizeType.Small => _strokePresets.Small,
        _              => _strokePresets.Middle
    };
}
