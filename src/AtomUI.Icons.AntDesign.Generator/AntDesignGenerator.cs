using System.Text;
using AtomUI.Controls;

namespace AtomUI.Icons.AntDesign.Generator;

public class AntDesignGenerator : DefaultIconPackageGenerator
{
    private List<string> _twoToneTplPrimaryColors;
    private readonly List<string> _twoToneTplSecondaryColors;
    
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
        catch  (Exception e)
        {
            Console.Error.WriteLine($"Generate error: {e.Message}");
#if DEBUG
            throw;
#endif
            return 1;
        }
    }
    
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