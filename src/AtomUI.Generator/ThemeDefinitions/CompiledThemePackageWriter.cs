using System.Globalization;
using AtomUI.Theme.Definitions.Compiled;
using System.Text;
using AtomUI.SourceGeneration;
using AtomUI.Theme.Build;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AtomUI.Generator;

/// <summary>
/// Emits the typed theme programs (compiled theme ABI 4) of the package's theme definitions: every token is bound at
/// build time against the referenced build contracts (the language and its language tokens, the global tokens, the
/// controls and their control tokens) and written as a typed literal. Axes are checked against the language at startup.
/// </summary>
internal static class CompiledThemePackageWriter
{
    private const string Compiled = "global::AtomUI.Theme.Definitions.Compiled.";
    private static readonly DiagnosticDescriptor Failure = Diagnostics.AtomUIDiagnosticDescriptors.CompiledThemeBindingFailed;
    internal static void Error(GenerationOutput output, string? path, int line, int column, string message) =>
        Report(output, Failure, path, line, column, message);

    private static void Report(GenerationOutput output, DiagnosticDescriptor descriptor, string? path, int line, int column,
        params object?[] arguments)
    {
        var position = new LinePosition(Math.Max(0, line - 1), Math.Max(0, column - 1));
        output.ReportDiagnostic(Diagnostic.Create(descriptor, string.IsNullOrEmpty(path) ? Location.None : Location.Create(path!, new TextSpan(0, 0), new LinePositionSpan(position, position)), arguments));
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
            if (!contracts.Languages.ContainsKey(input.Language))
            {
                Error(output, input.SourcePath, input.Line, input.Column,
                    "Design language '" + input.Language + "' is not declared by any referenced language package.");
                continue;
            }

            var describe = new StringBuilder();
            var write = new StringBuilder();
            var fields = new List<string> { "compiled-abi-v4", ThemeLiteralCodec.Identity, "generator-compiled-theme-v3", input.SourceIdentity,
                input.ContentDigest, input.Id, input.Name, input.Language, input.Style, input.Appearance.ToString(CultureInfo.InvariantCulture),
                input.Density };
            fields.AddRange(contracts.Languages[input.Language].Fields);
            var tokenId = 0;
            var controlId = 0;
            var valid = true;
            var accent = "null";
            var locked = new List<string>();

            void Bind(ThemeLiteralInput literal, string kind, int? control, ThemeBuildOwner? owner, bool isBase)
            {
                ThemeBuildToken? contract;
                string method;
                if (kind == "language")
                {
                    if (!contracts.Tokens.TryGetValue(ThemeBuildContracts.LanguageCatalog + "\u001f" + input.Language + "\u001f" + literal.Name, out contract))
                    { Error(output, input.SourcePath, literal.Line, literal.Column, "'" + literal.Name + "' is not a language Token of '" + input.Language + "'."); valid = false; return; }
                    method = "Language";
                }
                else
                {
                    var globalKey = "\u001f\u001f" + literal.Name;
                    contract = null;
                    var own = owner is not null && contracts.Tokens.TryGetValue(owner.Key + "\u001f" + literal.Name, out contract);
                    if (!own && !contracts.Tokens.TryGetValue(globalKey, out contract))
                    { Error(output, input.SourcePath, literal.Line, literal.Column, "Unknown Token '" + literal.Name + "'."); valid = false; return; }
                    if (own && contracts.Tokens.ContainsKey(globalKey))
                    { Error(output, input.SourcePath, literal.Line, literal.Column, "Ambiguous control/global Token '" + literal.Name + "'."); valid = false; return; }
                    method = own ? "Own" : control.HasValue ? "ControlGlobal" : "Global";
                }
                if (literal.Locked)
                {
                    if (!isBase || control.HasValue || contract!.Stage != 0)
                    { Error(output, input.SourcePath, literal.Line, literal.Column, "Locked applies only to a seed in the base section; '" + literal.Name + "' is not one."); valid = false; return; }
                    locked.Add(literal.Name);
                }
                if (contract!.Contract != 1)
                { Error(output, input.SourcePath, literal.Line, literal.Column, "Token '" + literal.Name + "' has no static literal capability (" + contract.ValueType + ")."); valid = false; return; }
                try
                {
                    var value = BindLiteral(compilation, contract, literal);
                    var type = contract.ValueType;
                    var prefix = control.HasValue ? control.Value.ToString(CultureInfo.InvariantCulture) + ", " : "";
                    describe.Append("        builder.Require").Append(method).Append('<').Append(type).Append(">(").Append(prefix).Append(tokenId);
                    if (method != "Language")
                    {
                        describe.Append(", ").Append(contract.KeyType).Append('.').Append(contract.KeyMember);
                    }
                    describe.Append(", ").Append(ThemeBuildContracts.Literal(contract.Name));
                    if (method != "Own") { describe.Append(", (global::AtomUI.Theme.Schema.TokenStage)").Append(contract.Stage); }
                    describe.AppendLine(", 1);");
                    write.Append("        writer.Set").Append(method).Append('<').Append(type).Append(">(").Append(prefix).Append(tokenId)
                        .Append(", ").Append(value.Expression).Append(", ").Append(ThemeBuildContracts.Literal(value.Canonical)).AppendLine(");");
                    fields.AddRange(contract.Fields); fields.Add(method); fields.Add(prefix); fields.Add(value.Canonical); fields.Add(value.Expression);
                    fields.Add(literal.Locked ? "locked" : "");
                    if (isBase && method == "Global" && literal.Name == "ColorPrimary" &&
                        ThemeLiteralCodec.NormalizeType(type) == "Avalonia.Media.Color") { accent = value.Expression; }
                    requirements.Add(new CompiledThemeRequirement { ThemeId = input.Id,
                        Catalog = method == "Language" ? ThemeBuildContracts.LanguageCatalog : owner?.Catalog ?? "",
                        ControlId = method == "Language" ? input.Language : owner?.Id ?? "",
                        OwnerAssemblyName = owner is null ? "" : new System.Reflection.AssemblyName(owner.OwnerAssembly).Name!, OwnerMetadataName = owner?.OwnerType ?? "",
                        Scope = method switch { "Own" => "own", "ControlGlobal" => "controlGlobal", "Language" => "language", _ => "global" },
                        TokenName = contract.Name, ValueType = contract.ValueType, ValueContractVersion = contract.Contract, Stage = contract.Stage,
                        ResourceKeyType = contract.KeyType, ResourceKeyMember = contract.KeyMember });
                    tokenId++;
                }
                catch (Exception error) { Error(output, input.SourcePath, literal.Line, literal.Column, "Invalid static literal for '" + literal.Name + "': " + error.Message); valid = false; }
            }

            void Section(int index, ThemeSectionInput section, ThemeConditionInput? condition)
            {
                describe.Append("        builder.BeginSection(").Append(index).Append(", ")
                    .Append(condition is null || condition.Style.Length == 0 ? "null" : ThemeBuildContracts.Literal(condition.Style)).Append(", ")
                    .Append(condition?.Appearance ?? -1).Append(", ")
                    .Append(condition is null || condition.Density.Length == 0 ? "null" : ThemeBuildContracts.Literal(condition.Density)).Append(", ")
                    .Append(condition is null || condition.Idiom.Length == 0 ? "null" : ThemeBuildContracts.Literal(condition.Idiom)).AppendLine(");");
                write.Append("        writer.BeginSection(").Append(index).AppendLine(");");
                fields.Add("section"); fields.Add(index.ToString(CultureInfo.InvariantCulture));
                if (condition is not null)
                {
                    fields.Add(condition.Style); fields.Add(condition.Appearance.ToString(CultureInfo.InvariantCulture));
                    fields.Add(condition.Density); fields.Add(condition.Idiom);
                }
                foreach (var literal in section.Tokens) { Bind(literal, "global", null, null, condition is null); }
                foreach (var literal in section.LanguageTokens) { Bind(literal, "language", null, null, condition is null); }
                var controlKeys = new HashSet<string>(StringComparer.Ordinal);
                foreach (var control in section.Controls)
                {
                    var key = control.Catalog + "\u001f" + control.Id;
                    if (!controlKeys.Add(key) || !contracts.Owners.TryGetValue(key, out var owner))
                    { Error(output, input.SourcePath, control.Line, control.Column, "Unknown or repeated Control '" + control.Catalog + ":" + control.Id + "'."); valid = false; continue; }
                    if (condition is not null && (control.Appearance != -1 || control.Density.Length != 0))
                    { Error(output, input.SourcePath, control.Line, control.Column, "Control '" + control.Catalog + ":" + control.Id + "' cannot fix an axis in a condition section."); valid = false; continue; }
                    var id = controlId++;
                    describe.Append("        builder.RequireControl(").Append(id).Append(", ").Append(owner.IdentityProvider).Append(".Identity, ")
                        .Append(ThemeBuildContracts.Literal(owner.Catalog)).Append(", ").Append(ThemeBuildContracts.Literal(owner.Id)).Append(", ")
                        .Append(ThemeBuildContracts.Literal(owner.OwnerAssembly)).Append(", ").Append(ThemeBuildContracts.Literal(owner.OwnerType))
                        .Append(", ").Append(control.Appearance).Append(", ")
                        .Append(control.Density.Length == 0 ? "null" : ThemeBuildContracts.Literal(control.Density)).AppendLine(");");
                    requirements.Add(new CompiledThemeRequirement { ThemeId = input.Id, Catalog = owner.Catalog, ControlId = owner.Id,
                        OwnerAssemblyName = new System.Reflection.AssemblyName(owner.OwnerAssembly).Name!, OwnerMetadataName = owner.OwnerType, Scope = "control" });
                    fields.AddRange(owner.Fields); fields.Add(control.Appearance.ToString(CultureInfo.InvariantCulture)); fields.Add(control.Density);
                    foreach (var literal in control.Tokens) { Bind(literal, "control", id, owner, condition is null); }
                }
            }

            Section(0, input.Base, null);
            for (var index = 0; index < input.Conditions.Length; index++)
            {
                Section(index + 1, input.Conditions[index].Section, input.Conditions[index]);
            }
            if (!valid) { continue; }

            var programName = "CompiledThemeProgram_" + programs.Count;
            var source = new StringBuilder("internal sealed class " + programName + " : " + Compiled + "CompiledThemeProgram\n{\n");
            source.Append("    internal ").Append(programName).Append("() : base(4, ").Append(ThemeBuildContracts.Literal(input.SourceIdentity))
                .Append(", ").Append(ThemeBuildContracts.Literal(input.ContentDigest)).Append(", ").Append(ThemeBuildContracts.Literal(input.ContentDigest))
                .Append(", ").Append(ThemeBuildContracts.Literal(ThemeBuildContracts.Digest(fields))).Append(", ").Append(ThemeBuildContracts.Literal(input.Id))
                .Append(", ").Append(ThemeBuildContracts.Literal(input.Name)).Append(", ").Append(ThemeBuildContracts.Literal(input.Language))
                .Append(", ").Append(input.Style.Length == 0 ? "null" : ThemeBuildContracts.Literal(input.Style))
                .Append(", ").Append(input.Appearance)
                .Append(", ").Append(input.Density.Length == 0 ? "null" : ThemeBuildContracts.Literal(input.Density))
                .Append(", new string[] { ").Append(string.Join(", ", locked.Select(ThemeBuildContracts.Literal))).Append(" }")
                .Append(", ").Append(accent).AppendLine(") { }");
            source.AppendLine("    protected override bool IsBackgroundPrepareSafe => true;");
            source.Append("    protected override void Describe(").Append(Compiled).Append("CompiledThemeBindingBuilder builder)\n    {\n").Append(describe).AppendLine("    }");
            source.Append("    protected override void WriteStaticDefaults(").Append(Compiled).Append("CompiledThemeValueWriter writer)\n    {\n").Append(write).AppendLine("    }");
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
