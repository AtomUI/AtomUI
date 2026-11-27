using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using AtomUI.Controls;

namespace AtomUI.Icons;

/// <summary>
/// Base class of the generators for the common package layout, where the SVG files sit in folders that
/// are named after their theme. It writes the icon kind enum, the provider factory and the catalog.
/// The source of the icon classes is left to the derived class.
/// </summary>
public abstract class DefaultIconPackageGenerator : AbstractIconPackageGenerator
{
    /// <summary>
    /// Creates a generator that scans <paramref name="sourcePath" /> and writes into <paramref name="targetPath" />.
    /// </summary>
    /// <param name="sourcePath">
    /// Directory that is scanned for SVG source files.
    /// </param>
    /// <param name="targetPath">
    /// Directory of the icon package project that receives the generated files.
    /// </param>
    /// <param name="generatedIconsDir">
    /// Name of the directory under <paramref name="targetPath" /> that holds the generated icon class files,
    /// or <see langword="null" /> to use <see cref="AbstractIconPackageGenerator.DefaultSavedDirectory" />.
    /// </param>
    public DefaultIconPackageGenerator(string sourcePath, string targetPath, string? generatedIconsDir = null)
        : base(sourcePath, targetPath, generatedIconsDir)
    {
    }
    
    /// <summary>
    /// Enumerates every <c>.svg</c> file of the directory and of its subdirectories.
    /// </summary>
    /// <param name="sourcePath">
    /// Directory to scan.
    /// </param>
    /// <remarks>
    /// <see cref="IconFileInfo.Name" /> is the file name without extension in PascalCase: a hyphen before a
    /// letter or digit is dropped and that letter or digit is upper-cased.
    /// <see cref="IconFileInfo.ThemeType" /> is parsed from the name of the folder that holds the file,
    /// ignoring case; a folder with any other name counts as <see cref="IconThemeType.Filled" />.
    /// <see cref="IconFileInfo.Category" /> is left <see langword="null" />.
    /// </remarks>
    protected override IEnumerable<IconFileInfo> ScanIconFilesRecursively(string sourcePath)
    {
        foreach (var svgFilePath in Directory.EnumerateFiles(sourcePath,  "*.svg"))
        {
            var name = NormalizeClassName(Path.GetFileNameWithoutExtension(svgFilePath));
            var parentDir = Path.GetFileName(Path.GetDirectoryName(svgFilePath));
            Debug.Assert(!string.IsNullOrEmpty(parentDir));
            var iconTheme = IconThemeType.Filled;
            if (Enum.TryParse<IconThemeType>(parentDir, true, out var value))
            {
                iconTheme = value;
            }
            yield return new IconFileInfo()
            {
                Name      = name,
                FilePath  =  svgFilePath,
                ThemeType = iconTheme
            };
        }
        
        foreach (var subPath in Directory.EnumerateDirectories(sourcePath))
        {
            foreach (var svgFilePath in ScanIconFilesRecursively(subPath))
            {
                yield return svgFilePath;
            }
        }
    }

    private static string NormalizeClassName(string name)
    {
        name = Regex.Replace(name, @"-([a-zA-Z0-9])",
            match => match.Groups[1].ToString().ToUpper());
        name = name[0].ToString().ToUpper() + name.Substring(1);
        return name;
    }

    /// <summary>
    /// Writes the enum that lists every icon: one member per entry of
    /// <see cref="AbstractIconPackageGenerator.IconFiles" />, with values from 1 on.
    /// </summary>
    /// <remarks>
    /// The file is <c>{PackageName}IconKind.g.cs</c> in
    /// <see cref="AbstractIconPackageGenerator.GeneratedIconsPath" />, and an existing file is overwritten.
    /// A member is named after the icon name followed by the theme.
    /// </remarks>
    protected override async Task GenerateIconPackageKindAsync()
    {
        await using var output = new FileStream(Path.Combine(GeneratedIconsPath, $"{PackageName}IconKind.g.cs"), FileMode.Create, FileAccess.Write);
        var sourceText  = new StringBuilder();
        sourceText.AppendLine("///");
        sourceText.AppendLine("/// This code is auto generated. Do not amend.");
        sourceText.AppendLine("///");
        sourceText.AppendLine($"namespace {PackageNamespace};");
        sourceText.AppendLine($"public enum {PackageName}IconKind");
        sourceText.AppendLine("{");
        for (var i = 0; i < IconFiles.Count; ++i)
        {
            var info = IconFiles[i];
            sourceText.AppendLine($"    {info.Name}{info.ThemeType} = {i + 1},");
        }
        
        sourceText.AppendLine("}");
        await output.WriteAsync(Encoding.UTF8.GetBytes(sourceText.ToString()));
    }

    /// <summary>
    /// Writes the factory part of the <c>{PackageName}IconProvider</c> partial class, which maps an icon
    /// kind to the type of the icon class and creates the instance.
    /// </summary>
    /// <remarks>
    /// The file is <c>{PackageName}IconProvider.Factory.g.cs</c> in
    /// <see cref="AbstractIconPackageGenerator.TargetPath" />, and an existing file is overwritten.
    /// The lookups are split into switch methods that each handle 64 icons. In the generated code, a kind
    /// that matches no icon throws <see cref="InvalidOperationException" />.
    /// </remarks>
    protected override async Task GenerateIconProviderFactoryAsync()
    {
        const int iconFactoryChunkSize = 64;

        await using var output = new FileStream(Path.Combine(TargetPath, $"{PackageName}IconProvider.Factory.g.cs"),
            FileMode.Create, FileAccess.Write);
        var chunkCount = (IconFiles.Count + iconFactoryChunkSize - 1) / iconFactoryChunkSize;
        var sourceText = new StringBuilder();
        sourceText.AppendLine("// This code is auto generated. Do not modify.");
        sourceText.AppendLine("using System;");
        sourceText.AppendLine("using System.Diagnostics.CodeAnalysis;");
        sourceText.AppendLine("using AtomUI.Controls;");
        sourceText.AppendLine("");
        sourceText.AppendLine($"namespace {PackageNamespace};");
        sourceText.AppendLine("");
        sourceText.AppendLine($"public partial class {PackageName}IconProvider");
        sourceText.AppendLine("{");
        sourceText.AppendLine($"    private const int IconFactoryIconCount = {IconFiles.Count};");
        sourceText.AppendLine($"    private const int IconFactoryChunkSize = {iconFactoryChunkSize};");
        sourceText.AppendLine("");
        sourceText.AppendLine($"    private static int GetIconIndex({PackageName}IconKind kind)");
        sourceText.AppendLine("    {");
        sourceText.AppendLine("        var index = (int)kind - 1;");
        sourceText.AppendLine("        if ((uint)index >= IconFactoryIconCount)");
        sourceText.AppendLine("        {");
        sourceText.AppendLine("            throw new InvalidOperationException($\"Icon kind {kind} does not exist\");");
        sourceText.AppendLine("        }");
        sourceText.AppendLine("");
        sourceText.AppendLine("        return index;");
        sourceText.AppendLine("    }");
        sourceText.AppendLine("");
        sourceText.AppendLine("    [UnconditionalSuppressMessage(\"Trimming\", \"IL2063\",");
        sourceText.AppendLine("        Justification = \"Every switch arm returns typeof(...) for a generated icon class with a public parameterless constructor.\")]");
        sourceText.AppendLine("    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]");
        sourceText.AppendLine($"    private static Type GetIconType({PackageName}IconKind kind)");
        sourceText.AppendLine("    {");
        sourceText.AppendLine("        var index = GetIconIndex(kind);");
        sourceText.AppendLine("        return GetIconTypeChunk(index / IconFactoryChunkSize, kind);");
        sourceText.AppendLine("    }");
        sourceText.AppendLine("");
        sourceText.AppendLine("    [UnconditionalSuppressMessage(\"Trimming\", \"IL2063\",");
        sourceText.AppendLine("        Justification = \"Every switch arm returns typeof(...) for a generated icon class with a public parameterless constructor.\")]");
        sourceText.AppendLine("    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]");
        sourceText.AppendLine($"    private static Type GetIconTypeChunk(int chunkIndex, {PackageName}IconKind kind)");
        sourceText.AppendLine("    {");
        sourceText.AppendLine("        return chunkIndex switch");
        sourceText.AppendLine("        {");
        for (var i = 0; i < chunkCount; i++)
        {
            sourceText.AppendLine($"            {i} => GetIconTypeChunk{i}(kind),");
        }
        sourceText.AppendLine("            _ => throw new ArgumentOutOfRangeException(nameof(chunkIndex))");
        sourceText.AppendLine("        };");
        sourceText.AppendLine("    }");
        sourceText.AppendLine("");
        sourceText.AppendLine($"    private static Icon CreateIcon({PackageName}IconKind kind)");
        sourceText.AppendLine("    {");
        sourceText.AppendLine("        var index = GetIconIndex(kind);");
        sourceText.AppendLine("        return CreateIconChunk(index / IconFactoryChunkSize, kind);");
        sourceText.AppendLine("    }");
        sourceText.AppendLine("");
        sourceText.AppendLine($"    private static Icon CreateIconChunk(int chunkIndex, {PackageName}IconKind kind)");
        sourceText.AppendLine("    {");
        sourceText.AppendLine("        return chunkIndex switch");
        sourceText.AppendLine("        {");
        for (var i = 0; i < chunkCount; i++)
        {
            sourceText.AppendLine($"            {i} => CreateIconChunk{i}(kind),");
        }
        sourceText.AppendLine("            _ => throw new ArgumentOutOfRangeException(nameof(chunkIndex))");
        sourceText.AppendLine("        };");
        sourceText.AppendLine("    }");
        for (var i = 0; i < chunkCount; i++)
        {
            sourceText.AppendLine("");
            sourceText.AppendLine("    [UnconditionalSuppressMessage(\"Trimming\", \"IL2063\",");
            sourceText.AppendLine("        Justification = \"Every switch arm returns typeof(...) for a generated icon class with a public parameterless constructor.\")]");
            sourceText.AppendLine("    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]");
            sourceText.AppendLine($"    private static Type GetIconTypeChunk{i}({PackageName}IconKind kind)");
            sourceText.AppendLine("    {");
            sourceText.AppendLine("        switch (kind)");
            sourceText.AppendLine("        {");
            foreach (var info in IconFiles.Skip(i * iconFactoryChunkSize).Take(iconFactoryChunkSize))
            {
                var iconClassName = $"{info.Name}{info.ThemeType}";
                sourceText.AppendLine($"            case {PackageName}IconKind.{iconClassName}: return typeof({iconClassName});");
            }
            sourceText.AppendLine("            default: throw new InvalidOperationException($\"Icon kind {kind} does not exist\");");
            sourceText.AppendLine("        }");
            sourceText.AppendLine("    }");
            sourceText.AppendLine("");
            sourceText.AppendLine($"    private static Icon CreateIconChunk{i}({PackageName}IconKind kind)");
            sourceText.AppendLine("    {");
            sourceText.AppendLine("        return kind switch");
            sourceText.AppendLine("        {");
            foreach (var info in IconFiles.Skip(i * iconFactoryChunkSize).Take(iconFactoryChunkSize))
            {
                var iconClassName = $"{info.Name}{info.ThemeType}";
                sourceText.AppendLine($"            {PackageName}IconKind.{iconClassName} => new {iconClassName}(),");
            }
            sourceText.AppendLine("            _ => throw new InvalidOperationException($\"Icon kind {kind} does not exist\")");
            sourceText.AppendLine("        };");
            sourceText.AppendLine("    }");
        }
        sourceText.AppendLine("}");
        await output.WriteAsync(Encoding.UTF8.GetBytes(sourceText.ToString()));
    }

    /// <summary>
    /// Writes the catalog that lists every icon with its class name, theme, kind, class type and a delegate
    /// that creates the instance.
    /// </summary>
    /// <remarks>
    /// The file is <c>{PackageName}IconCatalog.g.cs</c> in
    /// <see cref="AbstractIconPackageGenerator.TargetPath" />, and an existing file is overwritten.
    /// It declares the record <c>{PackageName}IconInfo</c> and the static class
    /// <c>{PackageName}IconCatalog</c>, whose <c>GetIcons()</c> method returns the icons in the order of
    /// <see cref="AbstractIconPackageGenerator.IconFiles" />. Every 64 entries are created together when
    /// they are first used.
    /// </remarks>
    protected override async Task GenerateIconCatalogAsync()
    {
        const int iconCatalogChunkSize = 64;

        await using var output = new FileStream(Path.Combine(TargetPath, $"{PackageName}IconCatalog.g.cs"),
            FileMode.Create, FileAccess.Write);
        var chunkCount = (IconFiles.Count + iconCatalogChunkSize - 1) / iconCatalogChunkSize;
        var sourceText = new StringBuilder();
        sourceText.AppendLine("// This code is auto generated. Do not modify.");
        sourceText.AppendLine("using System;");
        sourceText.AppendLine("using System.Collections;");
        sourceText.AppendLine("using System.Collections.Generic;");
        sourceText.AppendLine("using AtomUI.Controls;");
        sourceText.AppendLine("");
        sourceText.AppendLine($"namespace {PackageNamespace};");
        sourceText.AppendLine("");
        sourceText.AppendLine($"public readonly record struct {PackageName}IconInfo(");
        sourceText.AppendLine("    string Name,");
        sourceText.AppendLine("    IconThemeType ThemeType,");
        sourceText.AppendLine($"    {PackageName}IconKind Kind,");
        sourceText.AppendLine("    Type IconType,");
        sourceText.AppendLine("    Func<Icon> Creator);");
        sourceText.AppendLine("");
        sourceText.AppendLine($"public static class {PackageName}IconCatalog");
        sourceText.AppendLine("{");
        sourceText.AppendLine($"    private const int IconCount = {IconFiles.Count};");
        sourceText.AppendLine($"    private const int IconChunkSize = {iconCatalogChunkSize};");
        sourceText.AppendLine("");
        for (var i = 0; i < chunkCount; i++)
        {
            sourceText.AppendLine($"    private static readonly Lazy<{PackageName}IconInfo[]> IconChunk{i} = new(CreateIconChunk{i});");
        }
        sourceText.AppendLine("");
        sourceText.AppendLine($"    private static readonly IReadOnlyList<{PackageName}IconInfo> Icons = new IconCatalogList();");
        sourceText.AppendLine("");
        sourceText.AppendLine($"    public static IReadOnlyList<{PackageName}IconInfo> GetIcons()");
        sourceText.AppendLine("    {");
        sourceText.AppendLine("        return Icons;");
        sourceText.AppendLine("    }");
        sourceText.AppendLine("");
        sourceText.AppendLine($"    private sealed class IconCatalogList : IReadOnlyList<{PackageName}IconInfo>");
        sourceText.AppendLine("    {");
        sourceText.AppendLine("        public int Count => IconCount;");
        sourceText.AppendLine("");
        sourceText.AppendLine($"        public {PackageName}IconInfo this[int index]");
        sourceText.AppendLine("        {");
        sourceText.AppendLine("            get");
        sourceText.AppendLine("            {");
        sourceText.AppendLine("                if ((uint)index >= IconCount)");
        sourceText.AppendLine("                {");
        sourceText.AppendLine("                    throw new ArgumentOutOfRangeException(nameof(index));");
        sourceText.AppendLine("                }");
        sourceText.AppendLine("");
        sourceText.AppendLine("                return GetIconChunk(index / IconChunkSize)[index % IconChunkSize];");
        sourceText.AppendLine("            }");
        sourceText.AppendLine("        }");
        sourceText.AppendLine("");
        sourceText.AppendLine($"        public IEnumerator<{PackageName}IconInfo> GetEnumerator()");
        sourceText.AppendLine("        {");
        sourceText.AppendLine("            for (var i = 0; i < Count; i++)");
        sourceText.AppendLine("            {");
        sourceText.AppendLine("                yield return this[i];");
        sourceText.AppendLine("            }");
        sourceText.AppendLine("        }");
        sourceText.AppendLine("");
        sourceText.AppendLine("        IEnumerator IEnumerable.GetEnumerator()");
        sourceText.AppendLine("        {");
        sourceText.AppendLine("            return GetEnumerator();");
        sourceText.AppendLine("        }");
        sourceText.AppendLine("    }");
        sourceText.AppendLine("");
        sourceText.AppendLine($"    private static {PackageName}IconInfo[] GetIconChunk(int chunkIndex)");
        sourceText.AppendLine("    {");
        sourceText.AppendLine("        return chunkIndex switch");
        sourceText.AppendLine("        {");
        for (var i = 0; i < chunkCount; i++)
        {
            sourceText.AppendLine($"            {i} => IconChunk{i}.Value,");
        }
        sourceText.AppendLine("            _ => throw new ArgumentOutOfRangeException(nameof(chunkIndex))");
        sourceText.AppendLine("        };");
        sourceText.AppendLine("    }");
        for (var i = 0; i < chunkCount; i++)
        {
            sourceText.AppendLine("");
            sourceText.AppendLine($"    private static {PackageName}IconInfo[] CreateIconChunk{i}()");
            sourceText.AppendLine("    {");
            sourceText.AppendLine("        return [");
            foreach (var info in IconFiles.Skip(i * iconCatalogChunkSize).Take(iconCatalogChunkSize))
            {
                var iconClassName = $"{info.Name}{info.ThemeType}";
                sourceText.AppendLine($"            new(\"{iconClassName}\", IconThemeType.{info.ThemeType}, {PackageName}IconKind.{iconClassName}, typeof({iconClassName}), static () => new {iconClassName}()),");
            }
            sourceText.AppendLine("        ];");
            sourceText.AppendLine("    }");
        }
        sourceText.AppendLine("}");
        await output.WriteAsync(Encoding.UTF8.GetBytes(sourceText.ToString()));
    }
}
