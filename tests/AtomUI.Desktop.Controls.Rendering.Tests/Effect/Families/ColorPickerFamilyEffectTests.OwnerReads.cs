using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

public partial class ColorPickerFamilyEffectTests
{
    private const string ThemeRoot = "src/AtomUI.Desktop.Controls.ColorPicker/Themes/";
    private static readonly InputFamilyEffectTests.OwnerReadList[] s_parts =
    [
        new(ThemeRoot + "AbstractColorPickerTheme.axaml", ["BorderRadius", "BorderRadiusLG", "BorderRadiusSM", "BorderThickness", "ColorBgContainerDisabled", "ColorBgElevated", "ColorBorder", "ColorError", "ColorPrimaryHover", "ColorText", "ColorTextDisabled", "ColorWarning", "ControlHeightXS", "EnableMotion", "FontSize", "FontSizeLG", "FontSizeSM", "UniformlyMarginXXS"]),
        new(ThemeRoot + "ColorBlockTheme.axaml", ["BorderRadius", "BorderRadiusSM", "BorderRadiusXS", "BorderThickness", "ColorBorder", "ColorError", "ColorFillSecondary", "ControlHeight", "ControlHeightSM", "ControlHeightXS", "LineWidthBold"]),
        new(ThemeRoot + "ColorPickerPaletteGroupTheme.axaml", ["SpacingXXS"]),
        new(ThemeRoot + "PaletteColorItemTheme.axaml", ["BorderRadiusSM", "BorderThickness", "ColorFill", "ColorTextSecondary", "ColorWhite", "LineWidth"]),
        new(ThemeRoot + "ColorView/AbstractColorPickerViewTheme.axaml", ["ColorBorderSecondary", "EnableMotion", "PaddingXXS", "SpacingSM", "SpacingXS"]),
        new(ThemeRoot + "ColorView/ColorSpectrumTheme.axaml", ["BorderRadiusSM", "LineWidthBold"]),
        new(ThemeRoot + "ColorSlider/ColorSliderTheme.axaml", ["ColorFillSecondary"]),
        new(ThemeRoot + "ColorSlider/GradientColorSliderTheme.axaml", ["ColorFillSecondary"]),
        new(ThemeRoot + "ColorSlider/ColorSliderThumbTheme.axaml", ["BorderThickness", "ColorBgElevated", "ColorFillSecondary", "ColorPrimaryActive"]),
    ];

    [Fact]
    public void Owner_Reads_Match_Real_Primitives_And_Embedded_Editor()
    {
        var repository = new DirectoryInfo(AppContext.BaseDirectory);
        while (repository is not null && !File.Exists(Path.Combine(repository.FullName, "AtomUI.slnx")))
        {
            repository = repository.Parent;
        }
        repository.ShouldNotBeNull();
        Directory.EnumerateFiles(Path.Combine(repository.FullName, ThemeRoot), "*.axaml", SearchOption.AllDirectories)
            .Where(path => ThemeOwnerReads.Parse(File.ReadAllText(path)).Count > 0)
            .Select(path => Path.GetRelativePath(repository.FullName, path).Replace('\\', '/')).Order()
            .ShouldBe(s_parts.Select(part => part.Theme).Order());
        foreach (var part in s_parts.Append(InputFamilyEffectTests.s_textBox))
        {
            ThemeOwnerReads.Read(part.Theme).Order().ShouldBe(part.Tokens.Order(), part.Theme);
        }
    }

    private static IEnumerable<string> OwnerGlobals(string id) =>
        (id.EndsWith("View", StringComparison.Ordinal) ? s_parts.Skip(1) : s_parts.Take(2))
        .SelectMany(part => part.Tokens).Append("MotionDurationMid").Distinct();
}
