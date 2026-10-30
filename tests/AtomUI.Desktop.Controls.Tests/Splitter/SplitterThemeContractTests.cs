using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Splitter;

public class SplitterThemeContractTests
{
    [Fact]
    public void Splitter_Exposes_Line_Style_Api_And_Forwards_To_Internal_Parts()
    {
        var splitterSource = ReadRepoFile("src/AtomUI.Desktop.Controls/Splitter/Splitter.cs");
        var panelSource    = ReadRepoFile("src/AtomUI.Desktop.Controls/Splitter/SplitterPanel.cs");
        var handleSource   = ReadRepoFile("src/AtomUI.Desktop.Controls/Splitter/SplitterHandle.cs");
        var dragBarSource  = ReadRepoFile("src/AtomUI.Desktop.Controls/Splitter/SplitterDragBar.cs");

        splitterSource.ShouldContain("public static readonly StyledProperty<double> LineThicknessProperty");
        splitterSource.ShouldContain("public static readonly StyledProperty<CornerRadius> LineCornerRadiusProperty");
        splitterSource.ShouldContain("public static readonly StyledProperty<IReadOnlyList<double>?> BorderDashArrayProperty");
        splitterSource.ShouldContain("public static readonly StyledProperty<double> BorderDashOffsetProperty");
        splitterSource.ShouldContain("public double LineThickness");
        splitterSource.ShouldContain("public CornerRadius LineCornerRadius");
        splitterSource.ShouldContain("public IReadOnlyList<double>? BorderDashArray");
        splitterSource.ShouldContain("public double BorderDashOffset");

        panelSource.ShouldContain("Splitter.LineThicknessProperty.AddOwner<SplitterPanel>()");
        panelSource.ShouldContain("Splitter.LineCornerRadiusProperty.AddOwner<SplitterPanel>()");
        panelSource.ShouldContain("handle.LineThickness     = LineThickness;");
        panelSource.ShouldContain("handle.LineCornerRadius = LineCornerRadius;");

        handleSource.ShouldContain("Splitter.LineThicknessProperty.AddOwner<SplitterHandle>()");
        handleSource.ShouldContain("Splitter.LineCornerRadiusProperty.AddOwner<SplitterHandle>()");
        handleSource.ShouldContain("public CornerRadius LineCornerRadius");

        dragBarSource.ShouldContain("Splitter.LineThicknessProperty.AddOwner<SplitterDragBar>()");
        dragBarSource.ShouldContain("Splitter.LineCornerRadiusProperty.AddOwner<SplitterDragBar>()");
        dragBarSource.ShouldContain("public CornerRadius LineCornerRadius");
    }

    [Fact]
    public void Splitter_Theme_Binds_Root_Frame_And_Line_Style_Through_Template()
    {
        var splitterTheme = ReadRepoFile("src/AtomUI.Desktop.Controls/Splitter/Themes/SplitterTheme.axaml");
        var handleTheme   = ReadRepoFile("src/AtomUI.Desktop.Controls/Splitter/Themes/SplitterHandleTheme.axaml");
        var dragBarTheme  = ReadRepoFile("src/AtomUI.Desktop.Controls/Splitter/Themes/SplitterDragBarTheme.axaml");

        splitterTheme.ShouldContain("Background=\"{TemplateBinding Background}\"");
        splitterTheme.ShouldContain("BorderBrush=\"{TemplateBinding BorderBrush}\"");
        splitterTheme.ShouldContain("BorderThickness=\"{TemplateBinding BorderThickness}\"");
        splitterTheme.ShouldContain("CornerRadius=\"{TemplateBinding CornerRadius}\"");
        splitterTheme.ShouldContain("StrokeDashArray=\"{TemplateBinding BorderDashArray}\"");
        splitterTheme.ShouldContain("StrokeDaskOffset=\"{TemplateBinding BorderDashOffset}\"");
        splitterTheme.ShouldContain("LineThickness=\"{TemplateBinding LineThickness}\"");
        splitterTheme.ShouldContain("LineCornerRadius=\"{TemplateBinding LineCornerRadius}\"");
        splitterTheme.ShouldContain("<Setter Property=\"LineThickness\" Value=\"{atom:SplitterTokenResource HandleLineThickness}\" />");
        splitterTheme.ShouldContain("<Setter Property=\"LineCornerRadius\" Value=\"{atom:SharedTokenResource BorderRadiusXS}\" />");
        splitterTheme.ShouldNotContain("ControlTokenScope.Identity");

        handleTheme.ShouldContain("CornerRadius=\"{TemplateBinding LineCornerRadius}\"");
        handleTheme.ShouldContain("LineCornerRadius=\"{TemplateBinding LineCornerRadius}\"");
        handleTheme.ShouldNotContain("<Setter Property=\"LineThickness\" Value=\"{atom:SplitterTokenResource SplitBarSize}\" />");

        dragBarTheme.ShouldContain("CornerRadius=\"{TemplateBinding LineCornerRadius}\"");
        dragBarTheme.ShouldContain("<Setter Property=\"LineThickness\" Value=\"{atom:SplitterTokenResource HandleLineThickness}\" />");
        dragBarTheme.ShouldContain("<Setter Property=\"LineCornerRadius\" Value=\"{atom:SharedTokenResource BorderRadiusXS}\" />");
        dragBarTheme.ShouldNotContain("ControlTokenScope.Identity");
        dragBarTheme.ShouldContain("<Setter Property=\"Width\" Value=\"{Binding LineThickness, RelativeSource={RelativeSource TemplatedParent}}\" />");
        dragBarTheme.ShouldContain("<Setter Property=\"Height\" Value=\"{Binding LineThickness, RelativeSource={RelativeSource TemplatedParent}}\" />");
        dragBarTheme.ShouldNotContain("<Setter Property=\"Width\" Value=\"{atom:SplitterTokenResource SplitBarSize}\" />");
        dragBarTheme.ShouldNotContain("<Setter Property=\"Height\" Value=\"{atom:SplitterTokenResource SplitBarSize}\" />");
    }

    private static string ReadRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate);
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not find repository file: {relativePath}");
    }
}
