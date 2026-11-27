using System.Text;
using AtomUI.Controls;

namespace AtomUI.Icons.AntDesign.Generator;

/// <summary>
/// Generates the C# source of the Ant Design icon package from its SVG files. It specializes
/// <see cref="DefaultIconPackageGenerator" /> to write every icon class as a subclass of <c>AntDesignIcon</c>.
/// The drawing instructions of that class take their brush from the icon theme.
/// </summary>
public class AntDesignGenerator : DefaultIconPackageGenerator
{
    private List<string> _twoToneTplPrimaryColors;
    private readonly List<string> _twoToneTplSecondaryColors;
    
    /// <summary>
    /// Creates the generator of the <c>AntDesign</c> package in the namespace <c>AtomUI.Icons.AntDesign</c>.
    /// </summary>
    /// <param name="sourcePath">
    /// The directory that the SVG files are read from.
    /// </param>
    /// <param name="targetPath">
    /// The project directory that the generated source files are written to.
    /// </param>
    public AntDesignGenerator(string sourcePath, string targetPath)
        : base(sourcePath, targetPath)
    {
        PackageName              = "AntDesign";
        PackageNamespace         = "AtomUI.Icons.AntDesign";
        _twoToneTplPrimaryColors = ["#333"];
        _twoToneTplSecondaryColors = [
            "#E6E6E6",
            "#D9D9D9",
            "#D8D8D8"
        ];
    }
    
    /// <summary>
    /// Entry point that generates the icon package into <c>src/AtomUI.Icons.AntDesign</c> from the SVG files in
    /// <c>src/AtomUI.Icons.AntDesign.Generator/Assets/Svg</c>, both resolved four directories above the current one.
    /// </summary>
    /// <param name="args">
    /// Not read.
    /// </param>
    /// <returns>
    /// 0 when the package was generated. When the generation fails, the error message is written to the standard
    /// error stream and 1 is returned; a <c>DEBUG</c> build rethrows the exception instead.
    /// </returns>
    public static async Task<int> Main(string[] args)
    {
        try
        {
            var targetProjectPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../../../../src/AtomUI.Icons.AntDesign"));
            var sourceProjectPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../../../../src/AtomUI.Icons.AntDesign.Generator"));
            var sourcePath        = Path.Combine(sourceProjectPath, "Assets/Svg");
            var generator         = new AntDesignGenerator(sourcePath, targetProjectPath);
            await generator.GenerateAsync();
            return 0;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Generate error: {e.Message}");
#if DEBUG
            throw;
#else
            return 1;
#endif
        }
    }
    
    /// <summary>
    /// Writes the source of the class of one icon as a subclass of <c>AntDesignIcon</c> that keeps its drawing
    /// instructions in a static array.
    /// </summary>
    /// <param name="iconFileInfo">
    /// The icon whose class is written; its name and theme form the class name.
    /// </param>
    /// <param name="output">
    /// The stream that the source is written to as UTF-8 text; the caller disposes it.
    /// </param>
    /// <remarks>
    /// <para>
    /// Every <c>PathElement</c> of the SVG file becomes one <c>PathDrawingInstruction</c>, with the transform of the
    /// element when it has one; other graphic elements are skipped.
    /// </para>
    /// <para>
    /// The brush follows the theme: <c>IconBrushType.Fill</c> for a filled icon and <c>IconBrushType.Stroke</c> for an
    /// outlined icon. In a two-tone icon a path takes <c>IconBrushType.Fill</c> when its fill color is one of
    /// <c>#E6E6E6</c>, <c>#D9D9D9</c> and <c>#D8D8D8</c>, and <c>IconBrushType.Stroke</c> otherwise.
    /// </para>
    /// </remarks>
    protected override async Task GenerateIconPackageClass(IconFileInfo iconFileInfo, Stream output)
    {
        var      sourceText = new StringBuilder();
        sourceText.AppendLine("// This code is auto generated. Do not modify.");
        sourceText.AppendLine("// Copyright (c) Qinware Technologies Co., Ltd. 2019-2025. All rights reserved.");
        sourceText.AppendLine("// Copyright (c) Ant Design Authors 2025 https://github.com/ant-design/ant-design");
        sourceText.AppendLine("");
        sourceText.AppendLine("using Avalonia;");
        sourceText.AppendLine("using System;");
        sourceText.AppendLine("using Avalonia.Media;");
        sourceText.AppendLine("using AtomUI.Controls;");
        sourceText.AppendLine("using AtomUI.Media;");
        sourceText.AppendLine($"namespace {PackageNamespace};");
        sourceText.AppendLine("");
        var svgSource     = await File.ReadAllTextAsync(iconFileInfo.FilePath);
        var    svgParsedInfo = SvgParser.Parse(svgSource);
        var    viewBox       = svgParsedInfo.ViewBox;
        var    className     = $"{iconFileInfo.Name}{iconFileInfo.ThemeType}";
        sourceText.AppendLine($"public class {className} : AntDesignIcon");
        sourceText.AppendLine(@"{");
        sourceText.AppendLine($"    public {className}()");
        sourceText.AppendLine(@"    {");
        sourceText.AppendLine($"        IconTheme = IconThemeType.{iconFileInfo.ThemeType};");
        sourceText.AppendLine($"        ViewBox = new Rect({viewBox.X}, {viewBox.Y}, {viewBox.Width}, {viewBox.Height});");
        sourceText.AppendLine(@"    }");
        sourceText.AppendLine(@"");
        sourceText.AppendLine(@"    public override Icon CreateInstance()");
        sourceText.AppendLine(@"    {");
        sourceText.AppendLine($"        return new {className}();");
        sourceText.AppendLine(@"    }");
        sourceText.AppendLine(@"");
        sourceText.AppendLine(@"    private static readonly DrawingInstruction[] StaticInstructions = [");
        for (var i = 0; i < svgParsedInfo.GraphicElements.Count; i++)
        {
            var graphicElement  = svgParsedInfo.GraphicElements[i];
            if (graphicElement is PathElement pathElement)
            {
                sourceText.AppendLine(@"        new PathDrawingInstruction()");
                sourceText.AppendLine(@"        {");
                sourceText.AppendLine($"            Data = StreamGeometry.Parse(\"{pathElement.Data}\"),");
                if (iconFileInfo.ThemeType == IconThemeType.Filled)
                {
                    sourceText.AppendLine($"            FillBrush = IconBrushType.Fill,");
                }
                else if (iconFileInfo.ThemeType == IconThemeType.Outlined)
                {
                    sourceText.AppendLine($"            FillBrush = IconBrushType.Stroke,");
                }
                else if (iconFileInfo.ThemeType == IconThemeType.TwoTone)
                {
                    var isPrimary = !(pathElement.FillColor != null &&
                                      _twoToneTplSecondaryColors.Contains(pathElement.FillColor));
                    if (isPrimary)
                    {
                        sourceText.AppendLine($"            FillBrush = IconBrushType.Stroke,");
                    }
                    else
                    {
                        sourceText.AppendLine($"            FillBrush = IconBrushType.Fill,");
                    }
                }
               
                if (!string.IsNullOrEmpty(pathElement.Transform))
                {
                    sourceText.AppendLine($"            Transform = TransformParser.Parse(\"{pathElement.Transform}\").Value");
                }
                sourceText.AppendLine(@"        }");
            }
                
            if (i != svgParsedInfo.GraphicElements.Count - 1)
            {
                sourceText.Append(", ");
            }
        }
        sourceText.AppendLine(@"    ];");
        sourceText.AppendLine(@"");
        sourceText.AppendLine(@"    protected override IList<DrawingInstruction> DrawingInstructions => StaticInstructions;");
        sourceText.AppendLine("}");
        sourceText.AppendLine("");
        
        await output.WriteAsync(Encoding.UTF8.GetBytes(sourceText.ToString()));
    }
}
