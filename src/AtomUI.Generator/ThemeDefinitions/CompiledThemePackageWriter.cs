using System.Globalization;
using AtomUI.Theme.Definitions.Compiled;
using System.Text;
using AtomUI.SourceGeneration;
using AtomUI.Theme.Build;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AtomUI.Generator;

internal static class CompiledThemePackageWriter
{
    private const string Compiled = "global::AtomUI.Theme.Definitions.Compiled.";
    private static readonly DiagnosticDescriptor Failure = Diagnostics.AtomUIDiagnosticDescriptors.CompiledThemeBindingFailed;
    internal static void Error(GenerationOutput output, string? path, int line, int column, string message)
    {
        var position = new LinePosition(Math.Max(0, line - 1), Math.Max(0, column - 1));
        output.ReportDiagnostic(Diagnostic.Create(Failure, string.IsNullOrEmpty(path) ? Location.None : Location.Create(path!, new TextSpan(0, 0), new LinePositionSpan(position, position)), message));
    }
    internal static CompiledThemeManifest? Write(GenerationOutput output, Compilation compilation, ThemeBuildContracts contracts,
        IEnumerable<(string Path, string Text)> files, string assemblyName, string packageId)
    {
        var programs = new List<string>();
        var requirements = new List<CompiledThemeRequirement>();
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var file in files)
        {
            CompiledThemeInput input;
            try { input = CompiledThemeInput.Decode(file.Text); }
            catch (Exception error) { Error(output, file.Path, 0, 0, error.Message); continue; }
            if (!ids.Add(input.Id)) { Error(output, input.SourcePath, 0, 0, "Duplicate compiled theme Id: " + input.Id); continue; }
            var describe = new StringBuilder("        builder.SetAlgorithms(" + Algorithms(input.Algorithms) + ");\n");
            var write = new StringBuilder();
            var currentDescribe = describe;
            var currentWrite = write;
            var controlDescriptions = new List<string>();
            var controlWrites = new List<string>();
            var fields = new List<string> { "compiled-abi-v1", ThemeLiteralCodec.Identity, "generator-compiled-theme-v2", input.SourceIdentity, input.ContentDigest,
                input.Id, input.Name, input.Appearance.ToString(CultureInfo.InvariantCulture), input.IsDefault.ToString() };
            fields.AddRange(input.Algorithms.Select(a => a.ToString(CultureInfo.InvariantCulture)));
            var tokenId = 0;
            var valid = true;
            var accent = "null";
            void Bind(ThemeLiteralInput literal, int? controlId, ThemeBuildOwner? owner)
            {
                var globalKey = "\u001f\u001f" + literal.Name;
                ThemeBuildToken? contract = null;
                var own = owner is not null && contracts.Tokens.TryGetValue(owner.Key + "\u001f" + literal.Name, out contract);
                if (!own && !contracts.Tokens.TryGetValue(globalKey, out contract))
                { Error(output, input.SourcePath, literal.Line, literal.Column, "Unknown Token '" + literal.Name + "'."); valid = false; return; }
                if (own && contracts.Tokens.ContainsKey(globalKey))
                { Error(output, input.SourcePath, literal.Line, literal.Column, "Ambiguous Own/Global Token '" + literal.Name + "'."); valid = false; return; }
                if (contract!.Contract != 1)
                { Error(output, input.SourcePath, literal.Line, literal.Column, "Token '" + literal.Name + "' has no v1 static literal capability (" + contract.ValueType + ")."); valid = false; return; }
                try
                {
                    var value = BindLiteral(compilation, contract, literal);
                    var type = contract.ValueType;
                    var method = own ? "Own" : controlId.HasValue ? "ControlGlobal" : "Global";
                    var prefix = controlId.HasValue ? controlId.Value.ToString(CultureInfo.InvariantCulture) + ", " : "";
                    currentDescribe.Append("        builder.Require").Append(method).Append('<').Append(type).Append(">(").Append(prefix).Append(tokenId)
                        .Append(", ").Append(contract.KeyType).Append('.').Append(contract.KeyMember).Append(", ").Append(ThemeBuildContracts.Literal(contract.Name));
                    if (!own) { currentDescribe.Append(", (global::AtomUI.Theme.Schema.TokenStage)").Append(contract.Stage); }
                    currentDescribe.AppendLine(", 1);");
                    currentWrite.Append("        writer.Set").Append(method).Append('<').Append(type).Append(">(").Append(prefix).Append(tokenId)
                        .Append(", ").Append(value.Expression).Append(", ").Append(ThemeBuildContracts.Literal(value.Canonical)).AppendLine(");");
                    fields.AddRange(contract.Fields); fields.Add(method); fields.Add(prefix); fields.Add(value.Canonical); fields.Add(value.Expression);
                    if (!controlId.HasValue && literal.Name == "ColorPrimary" && ThemeLiteralCodec.NormalizeType(type) == "Avalonia.Media.Color") { accent = value.Expression; }
                    requirements.Add(new CompiledThemeRequirement { ThemeId = input.Id, Catalog = owner?.Catalog ?? "", ControlId = owner?.Id ?? "",
                        OwnerAssemblyName = owner is null ? "" : new System.Reflection.AssemblyName(owner.OwnerAssembly).Name!, OwnerMetadataName = owner?.OwnerType ?? "",
                        Scope = own ? "own" : controlId.HasValue ? "controlGlobal" : "global", TokenName = contract.Name,
                        ValueType = contract.ValueType, ValueContractVersion = contract.Contract, Stage = contract.Stage,
                        ResourceKeyType = contract.KeyType, ResourceKeyMember = contract.KeyMember });
                    tokenId++;
                }
                catch (Exception error) { Error(output, input.SourcePath, literal.Line, literal.Column, "Invalid static literal for '" + literal.Name + "': " + error.Message); valid = false; }
            }
            foreach (var literal in input.Tokens) { Bind(literal, null, null); }
            var controlIds = new HashSet<string>(StringComparer.Ordinal);
            for (var controlId = 0; controlId < input.Controls.Length; controlId++)
            {
                var control = input.Controls[controlId];
                var key = control.Catalog + "\u001f" + control.Id;
                if (!controlIds.Add(key) || !contracts.Owners.TryGetValue(key, out var owner) || control.Mode < 0 || control.Mode > 3)
                { Error(output, input.SourcePath, control.Line, control.Column, "Unknown, repeated or invalid Control '" + control.Catalog + ":" + control.Id + "'."); valid = false; continue; }
                currentDescribe = new StringBuilder("        builder.SetAlgorithms(" + Algorithms(input.Algorithms) + ");\n");
                currentWrite = new StringBuilder();
                var controlDeclaration = new StringBuilder();
                controlDeclaration.Append("        builder.RequireControl(").Append(controlId).Append(", ").Append(owner.IdentityProvider).Append(".Identity, ")
                    .Append(ThemeBuildContracts.Literal(owner.Catalog)).Append(", ").Append(ThemeBuildContracts.Literal(owner.Id)).Append(", ")
                    .Append(ThemeBuildContracts.Literal(owner.OwnerAssembly)).Append(", ").Append(ThemeBuildContracts.Literal(owner.OwnerType))
                    .Append(", (global::AtomUI.Theme.Configuration.ControlAlgorithmMode)").Append(control.Mode).Append(", ").Append(Algorithms(control.Algorithms)).AppendLine(");");
                describe.Append(controlDeclaration);
                currentDescribe.Append(controlDeclaration);
                requirements.Add(new CompiledThemeRequirement { ThemeId = input.Id, Catalog = owner.Catalog, ControlId = owner.Id,
                    OwnerAssemblyName = new System.Reflection.AssemblyName(owner.OwnerAssembly).Name!, OwnerMetadataName = owner.OwnerType, Scope = "control" });
                fields.AddRange(owner.Fields); fields.Add(control.Mode.ToString(CultureInfo.InvariantCulture)); fields.AddRange(control.Algorithms.Select(a => a.ToString(CultureInfo.InvariantCulture)));
                foreach (var literal in control.Tokens) { Bind(literal, controlId, owner); }
                controlDescriptions.Add(currentDescribe.ToString());
                controlWrites.Add(currentWrite.ToString());
            }
            if (!valid) { continue; }
            var programName = "CompiledThemeProgram_" + programs.Count;
            var source = new StringBuilder("internal sealed class " + programName + " : " + Compiled + "CompiledThemeProgram\n{\n");
            source.Append("    internal ").Append(programName).Append("() : base(1, ").Append(ThemeBuildContracts.Literal(input.SourceIdentity))
                .Append(", ").Append(ThemeBuildContracts.Literal(input.ContentDigest)).Append(", ").Append(ThemeBuildContracts.Literal(input.ContentDigest))
                .Append(", ").Append(ThemeBuildContracts.Literal(ThemeBuildContracts.Digest(fields))).Append(", ").Append(ThemeBuildContracts.Literal(input.Id))
                .Append(", ").Append(ThemeBuildContracts.Literal(input.Name)).Append(", (global::AtomUI.Theme.ThemeAppearance)").Append(input.Appearance)
                .Append(", ").Append(input.IsDefault ? "true" : "false").Append(", ").Append(accent).AppendLine(") { }");
            source.AppendLine("    protected override bool IsBackgroundPrepareSafe => true;");
            source.Append("    protected override void Describe(").Append(Compiled).Append("CompiledThemeBindingBuilder builder)\n    {\n").Append(describe).AppendLine("    }");
            source.Append("    protected override void WriteStaticDefaults(").Append(Compiled).Append("CompiledThemeValueWriter writer)\n    {\n").Append(write).AppendLine("    }");
            source.Append("    protected override void DescribeControl(int controlId, ").Append(Compiled).AppendLine("CompiledThemeBindingBuilder builder)\n    {\n        switch (controlId)\n        {");
            for (var index = 0; index < controlDescriptions.Count; index++) { source.Append("            case ").Append(index).AppendLine(":").Append(controlDescriptions[index]).AppendLine("                break;"); }
            source.AppendLine("            default: throw new global::System.ArgumentOutOfRangeException(nameof(controlId));\n        }\n    }");
            source.Append("    protected override void WriteControlDefaults(int controlId, ").Append(Compiled).AppendLine("CompiledThemeValueWriter writer)\n    {\n        switch (controlId)\n        {");
            for (var index = 0; index < controlWrites.Count; index++) { source.Append("            case ").Append(index).AppendLine(":").Append(controlWrites[index]).AppendLine("                break;"); }
            source.AppendLine("            default: throw new global::System.ArgumentOutOfRangeException(nameof(controlId));\n        }\n    }");
            source.AppendLine("}");
            programs.Add(source.ToString());
        }
        if (programs.Count == 0) { return null; }
        var manifest = new CompiledThemeManifest { Requirements = requirements.ToArray() };
        var result = new StringBuilder("// <auto-generated />\n#nullable enable\n[assembly: " + Compiled + "CompiledThemeManifestAttribute(1, " + ThemeBuildContracts.Literal(packageId) + ", " + ThemeBuildContracts.Literal(manifest.Encode()) + ")]\nnamespace " + GeneratedCodeNamespace.ForAssembly(assemblyName) + ";\n\npublic static class GeneratedThemePackageRegistration\n{\n");
        result.Append("    public static ").Append(Compiled).Append("CompiledThemePackage CreatePackage() => new ").Append(Compiled).Append("CompiledThemePackage(")
            .Append(ThemeBuildContracts.Literal(packageId)).Append(", new ").Append(Compiled).Append("CompiledThemeProgram[] { ")
            .Append(string.Join(", ", Enumerable.Range(0, programs.Count).Select(i => "new CompiledThemeProgram_" + i + "()"))).AppendLine(" });");
        result.AppendLine("    public static void Register(global::AtomUI.Theme.IThemeManagerBuilder builder) => builder.AddCompiledThemePackage(CreatePackage());\n}");
        foreach (var program in programs) { result.AppendLine(program); }
        output.AddSource("GeneratedThemePackageRegistration.g.cs", GeneratedSourceText.From(result.ToString()));
        return manifest;
    }
    private static string Algorithms(IEnumerable<int> values) => "new global::AtomUI.Theme.Algorithms.ThemeAlgorithm[] { " + string.Join(", ", values.Select(v => "(global::AtomUI.Theme.Algorithms.ThemeAlgorithm)" + v)) + " }";
    internal static (string Expression, string Canonical) BindLiteral(Compilation compilation, ThemeBuildToken token, ThemeLiteralInput literal)
    {
        var type = ThemeLiteralCodec.NormalizeType(token.ValueType);
        if (type.StartsWith("System.Nullable<", StringComparison.Ordinal)) { type = type.Substring(16, type.Length - 17); }
        if (ThemeBuildContracts.Resolve(compilation, token.ValueType, token.ValueAssembly) is null)
        { throw new InvalidOperationException("The token value type is unavailable."); }
        var value = literal.Values.SingleOrDefault(candidate => candidate.Type == type)
            ?? throw new FormatException("Core codec rejected the literal for '" + type + "'.");
        return (EmitValue(type, value), value.Canonical);
    }

    private static string EmitValue(string type, BuiltInThemeLiteralValue value)
    {
        string Color(long color) => "new global::Avalonia.Media.Color(" + ((color >> 24) & 255) + ", " + ((color >> 16) & 255) + ", " + ((color >> 8) & 255) + ", " + (color & 255) + ")";
        string Numbers() => string.Join(", ", value.Numbers.Select(Number));
        switch (value.Kind)
        {
            case "string": return ThemeBuildContracts.Literal(value.Text);
            case "int": return value.Integer.ToString(CultureInfo.InvariantCulture);
            case "bool": return value.Integer == 0 ? "false" : "true";
            case "double": return Number(value.Numbers[0]);
            case "float": return "(float)" + Number(value.Numbers[0]);
            case "duration": return "new global::System.TimeSpan(" + value.Integer.ToString(CultureInfo.InvariantCulture) + "L)";
            case "font": return "new global::Avalonia.Media.FontFamily(" + ThemeBuildContracts.Literal(value.Text) + ")";
            case "enum": return "(global::" + type + ")" + value.Integer.ToString(CultureInfo.InvariantCulture);
            case "color":
                var color = Color(value.Integer);
                if (type == "Avalonia.Media.Color") { return color; }
                return "new global::" + (type == "Avalonia.Media.SolidColorBrush" ? type : "Avalonia.Media.Immutable.ImmutableSolidColorBrush") + "(" + color + ")";
            case "numbers":
                if (type == "AtomUI.Dimension") { return "new global::AtomUI.Dimension(" + Number(value.Numbers[0]) + ", (global::AtomUI.DimensionUnitType)" + value.Numbers[1] + ")"; }
                if (type == "Avalonia.Media.Immutable.ImmutableTransform") { return "new global::Avalonia.Media.Immutable.ImmutableTransform(new global::Avalonia.Matrix(" + Numbers() + "))"; }
                return "new global::" + type + "(" + Numbers() + ")";
            case "decoration":
                return "new global::AtomUI.TextDecorationInfo { Color = " + Color(value.Integer) + ", LineType = (global::AtomUI.TextDecorationLine)" + value.Numbers[0] + ", LineStyle = (global::AtomUI.LineStyle)" + value.Numbers[1] + ", Thickness = " + value.Numbers[2] + " }";
            case "shadow":
                return "new global::Avalonia.Media.BoxShadow { OffsetX = " + Number(value.Numbers[0]) + ", OffsetY = " + Number(value.Numbers[1]) + ", Blur = " + Number(value.Numbers[2]) + ", Spread = " + Number(value.Numbers[3]) + ", Color = " + Color(value.Integer) + ", IsInset = " + (value.Numbers[4] == 0 ? "false" : "true") + " }";
            case "shadows":
                if (value.Items.Length == 0) { return "new global::Avalonia.Media.BoxShadows()"; }
                var first = EmitValue("Avalonia.Media.BoxShadow", value.Items[0]);
                return value.Items.Length == 1 ? "new global::Avalonia.Media.BoxShadows(" + first + ")" : "new global::Avalonia.Media.BoxShadows(" + first + ", new global::Avalonia.Media.BoxShadow[] { " + string.Join(", ", value.Items.Skip(1).Select(item => EmitValue("Avalonia.Media.BoxShadow", item))) + " })";
            case "spline": return "new global::Avalonia.Animation.Easings.SplineEasing(new global::Avalonia.Animation.KeySpline(" + Numbers() + "))";
            case "spring": return "new global::Avalonia.Animation.Easings.SpringEasing(" + Numbers() + ")";
            case "easing":
                if (!value.Text.StartsWith("Avalonia.Animation.Easings.", StringComparison.Ordinal) || value.Text.Any(character => !(char.IsLetterOrDigit(character) || character == '.')))
                { throw new InvalidDataException("Invalid built-in easing type."); }
                return "new global::" + value.Text + "()";
            default: throw new InvalidDataException("Unknown compiled value kind: " + value.Kind);
        }
    }

    private static string Number(double value) => double.IsNaN(value) ? "global::System.Double.NaN" : double.IsPositiveInfinity(value) ? "global::System.Double.PositiveInfinity" : double.IsNegativeInfinity(value) ? "global::System.Double.NegativeInfinity" : value.ToString("R", CultureInfo.InvariantCulture) + "D";
}
