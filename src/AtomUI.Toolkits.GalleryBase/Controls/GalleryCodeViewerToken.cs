using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Toolkits.GalleryBase.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class GalleryCodeViewerToken : AbstractControlDesignToken, IControlTokenDefaults
{
    public double CodeFontSize { get; set; }
    public FontFamily CodeFontFamily { get; set; } = FontFamily.Default;

    [ControlTokenDefault(nameof(CodeFontSize))]
    private static double DefaultCodeFontSize(in ControlTokenDefaultContext context) => 13;

    [ControlTokenDefault(nameof(CodeFontFamily))]
    private static FontFamily DefaultCodeFontFamily(in ControlTokenDefaultContext context) =>
        new("Cascadia Code, Consolas, Menlo, monospace");
}
