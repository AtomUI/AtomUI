using System.Globalization;
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
    internal static void Write(GenerationOutput output, Compilation compilation, ThemeBuildContracts contracts,
        IEnumerable<(string Path, string Text)> files, string assemblyName, string packageId)
    {
        var programs = new List<string>();
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var file in files)
        {
            CompiledThemeInput input;
            try { input = CompiledThemeInput.Decode(file.Text); }
            catch (Exception error) { Error(output, file.Path, 0, 0, error.Message); continue; }
            if (!ids.Add(input.Id)) { Error(output, input.SourcePath, 0, 0, "Duplicate compiled theme Id: " + input.Id); continue; }
            var describe = new StringBuilder("        builder.SetAlgorithms(" + Algorithms(input.Algorithms) + ");\n");
            var write = new StringBuilder();
            var fields = new List<string> { "compiled-abi-v1", ThemeLiteralCodec.Identity, "generator-compiled-theme-v1.1", input.SourceIdentity, input.ContentDigest,
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
                    var value = BindLiteral(compilation, contract, literal.Value);
                    var type = contract.ValueType;
                    var method = own ? "Own" : controlId.HasValue ? "ControlGlobal" : "Global";
                    var prefix = controlId.HasValue ? controlId.Value.ToString(CultureInfo.InvariantCulture) + ", " : "";
                    describe.Append("        builder.Require").Append(method).Append('<').Append(type).Append(">(").Append(prefix).Append(tokenId)
                        .Append(", ").Append(contract.KeyType).Append('.').Append(contract.KeyMember).Append(", ").Append(ThemeBuildContracts.Literal(contract.Name));
                    if (!own) { describe.Append(", (global::AtomUI.Theme.Schema.TokenStage)").Append(contract.Stage); }
                    describe.AppendLine(", 1);");
                    write.Append("        writer.Set").Append(method).Append('<').Append(type).Append(">(").Append(prefix).Append(tokenId)
                        .Append(", ").Append(value.Expression).Append(", ").Append(ThemeBuildContracts.Literal(value.Canonical)).AppendLine(");");
                    fields.AddRange(contract.Fields); fields.Add(method); fields.Add(prefix); fields.Add(value.Canonical); fields.Add(value.Expression);
                    if (!controlId.HasValue && literal.Name == "ColorPrimary" && ThemeLiteralCodec.NormalizeType(type) == "Avalonia.Media.Color") { accent = value.Expression; }
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
                describe.Append("        builder.RequireControl(").Append(controlId).Append(", ").Append(owner.IdentityProvider).Append(".Identity, ")
                    .Append(ThemeBuildContracts.Literal(owner.Catalog)).Append(", ").Append(ThemeBuildContracts.Literal(owner.Id)).Append(", ")
                    .Append(ThemeBuildContracts.Literal(owner.OwnerAssembly)).Append(", ").Append(ThemeBuildContracts.Literal(owner.OwnerType))
                    .Append(", (global::AtomUI.Theme.Configuration.ControlAlgorithmMode)").Append(control.Mode).Append(", ").Append(Algorithms(control.Algorithms)).AppendLine(");");
                fields.AddRange(owner.Fields); fields.Add(control.Mode.ToString(CultureInfo.InvariantCulture)); fields.AddRange(control.Algorithms.Select(a => a.ToString(CultureInfo.InvariantCulture)));
                foreach (var literal in control.Tokens) { Bind(literal, controlId, owner); }
            }
            if (!valid) { continue; }
            var programName = "CompiledThemeProgram_" + programs.Count;
            var source = new StringBuilder("internal sealed class " + programName + " : " + Compiled + "CompiledThemeProgram\n{\n");
            source.Append("    internal ").Append(programName).Append("() : base(1, ").Append(ThemeBuildContracts.Literal(input.SourceIdentity))
                .Append(", ").Append(ThemeBuildContracts.Literal(input.ContentDigest)).Append(", ").Append(ThemeBuildContracts.Literal(input.ContentDigest))
                .Append(", ").Append(ThemeBuildContracts.Literal(ThemeBuildContracts.Digest(fields))).Append(", ").Append(ThemeBuildContracts.Literal(input.Id))
                .Append(", ").Append(ThemeBuildContracts.Literal(input.Name)).Append(", (global::AtomUI.Theme.ThemeAppearance)").Append(input.Appearance)
                .Append(", ").Append(input.IsDefault ? "true" : "false").Append(", ").Append(accent).AppendLine(") { }");
            source.Append("    protected override void Describe(").Append(Compiled).Append("CompiledThemeBindingBuilder builder)\n    {\n").Append(describe).AppendLine("    }");
            source.Append("    protected override void WriteStaticDefaults(").Append(Compiled).Append("CompiledThemeValueWriter writer)\n    {\n").Append(write).AppendLine("    }");
            source.AppendLine("}");
            programs.Add(source.ToString());
        }
        if (programs.Count == 0) { return; }
        var result = new StringBuilder("// <auto-generated />\n#nullable enable\nnamespace " + GeneratedCodeNamespace.ForAssembly(assemblyName) + ";\n\npublic static class GeneratedThemePackageRegistration\n{\n");
        result.Append("    public static ").Append(Compiled).Append("CompiledThemePackage CreatePackage() => new ").Append(Compiled).Append("CompiledThemePackage(")
            .Append(ThemeBuildContracts.Literal(packageId)).Append(", new ").Append(Compiled).Append("CompiledThemeProgram[] { ")
            .Append(string.Join(", ", Enumerable.Range(0, programs.Count).Select(i => "new CompiledThemeProgram_" + i + "()"))).AppendLine(" });");
        result.AppendLine("    public static void Register(global::AtomUI.Theme.IThemeManagerBuilder builder) => builder.AddCompiledThemePackage(CreatePackage());\n}");
        foreach (var program in programs) { result.AppendLine(program); }
        output.AddSource("GeneratedThemePackageRegistration.g.cs", GeneratedSourceText.From(result.ToString()));
    }
    private static string Algorithms(IEnumerable<int> values) => "new global::AtomUI.Theme.Algorithms.ThemeAlgorithm[] { " + string.Join(", ", values.Select(v => "(global::AtomUI.Theme.Algorithms.ThemeAlgorithm)" + v)) + " }";
    internal static (string Expression, string Canonical) BindLiteral(Compilation compilation, ThemeBuildToken token, string text)
    {
        var type = ThemeLiteralCodec.NormalizeType(token.ValueType);
        if (type.StartsWith("System.Nullable<", StringComparison.Ordinal)) { type = type.Substring(16, type.Length - 17); }
        var symbol = ThemeBuildContracts.Resolve(compilation, token.ValueType, token.ValueAssembly);
        if (symbol?.TypeKind == TypeKind.Enum)
        {
            if (type is not "AtomUI.LineStyle" and not "Avalonia.Media.FontWeight" and not "Avalonia.Media.PenLineCap" and not "Avalonia.Media.PenLineJoin")
            { throw new NotSupportedException("No built-in enum literal converter exists for '" + type + "'."); }
            return BindEnumLiteral(symbol, type, text);
        }
        var literal = ThemeLiteralCodec.Parse(type, text);
        return (EmitValue(type, literal.Value), literal.CanonicalValue);
    }
    private static (string Expression, string Canonical) BindEnumLiteral(INamedTypeSymbol symbol, string type, string text)
    {
        var fields = symbol.GetMembers().OfType<IFieldSymbol>().Where(field => field.HasConstantValue).ToArray();
        if (type == "AtomUI.LineStyle")
        {
            var exact = fields.FirstOrDefault(field => field.Name == text) ?? throw new FormatException("Unknown line style literal.");
            return ("global::" + type + "." + exact.Name, exact.Name);
        }

        var literal = text.Trim();
        long numeric;
        if (literal.IndexOf(',') >= 0)
        {
            numeric = 0;
            foreach (var part in literal.Split(','))
            {
                var name = part.Trim();
                var member = fields.FirstOrDefault(field => field.Name == name) ?? throw new FormatException("Unknown enum member.");
                numeric |= Convert.ToInt64(member.ConstantValue, CultureInfo.InvariantCulture);
            }
        }
        else if (long.TryParse(literal, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            numeric = parsed;
        }
        else
        {
            var member = fields.FirstOrDefault(field => field.Name == literal) ?? throw new FormatException("Unknown enum member.");
            numeric = Convert.ToInt64(member.ConstantValue, CultureInfo.InvariantCulture);
        }
        // Enum.IsDefined accepts only a declared value. Enum.ToString canonicalizes
        // aliases to the first declared member for these built-in non-flags enums.
        var canonical = fields.FirstOrDefault(field => Convert.ToInt64(field.ConstantValue, CultureInfo.InvariantCulture) == numeric)
            ?? throw new FormatException("Undefined enum value.");
        return ("global::" + type + "." + canonical.Name, canonical.Name);
    }
    private static string EmitValue(string type, object? neutral)
    {
        type = ThemeLiteralCodec.NormalizeType(type);
        if (type.StartsWith("System.Nullable<", StringComparison.Ordinal)) { type = type.Substring(16, type.Length - 17); }
        if (neutral is null) { return "null"; }
        var validKind = type switch
        {
            "System.Boolean" => neutral is bool,
            "System.Int32" => neutral is int,
            "System.Double" => neutral is double,
            "System.Single" => neutral is float,
            "System.TimeSpan" => neutral is long,
            "System.String" or "Avalonia.Media.FontFamily" => neutral is string,
            "Avalonia.Media.Color" or "Avalonia.Media.IBrush" or "Avalonia.Media.IImmutableBrush" or "Avalonia.Media.SolidColorBrush" => neutral is uint,
            "Avalonia.Thickness" or "Avalonia.CornerRadius" => neutral is double[] geometry && geometry.Length == 4,
            "Avalonia.Point" or "Avalonia.Size" => neutral is double[] pair && pair.Length == 2,
            "Avalonia.Media.Immutable.ImmutableTransform" => neutral is double[] matrix && matrix.Length == 6,
            "AtomUI.Dimension" => neutral is double[] dimension && dimension.Length == 2,
            "Avalonia.Animation.Easings.Easing" => neutral is string || neutral is double[] spline && spline.Length == 4,
            "Avalonia.Media.BoxShadow" => neutral is ThemeShadowValue,
            "Avalonia.Media.BoxShadows" => neutral is ThemeShadowValue[],
            "AtomUI.TextDecorationInfo" => neutral is ThemeTextDecorationValue,
            _ => false
        };
        if (!validKind) { throw new InvalidDataException("Neutral value kind does not match " + type); }
        string expression;
        switch (neutral)
        {
            case string value when type == "Avalonia.Media.FontFamily":
                // Approved immutable-description boundary: Avalonia parses inside its public constructor.
                expression = "new global::Avalonia.Media.FontFamily(" + ThemeBuildContracts.Literal(value) + ")"; break;
            case string value when type == "Avalonia.Animation.Easings.Easing": expression = "new global::Avalonia.Animation.Easings." + value + "()"; break;
            case string value: expression = ThemeBuildContracts.Literal(value); break;
            case int number: expression = number.ToString(CultureInfo.InvariantCulture); break;
            case bool value: expression = value ? "true" : "false"; break;
            case double value: expression = Number(value); break;
            case float value: expression = float.IsNaN(value) ? "global::System.Single.NaN" : float.IsPositiveInfinity(value) ? "global::System.Single.PositiveInfinity" : float.IsNegativeInfinity(value) ? "global::System.Single.NegativeInfinity" : value.ToString("R", CultureInfo.InvariantCulture) + "F"; break;
            case long value: expression = "new global::System.TimeSpan(" + value.ToString(CultureInfo.InvariantCulture) + "L)"; break;
            case uint value:
                expression = "new global::Avalonia.Media.Color(" + (value >> 24) + ", " + ((value >> 16) & 255) + ", " + ((value >> 8) & 255) + ", " + (value & 255) + ")";
                if (type == "Avalonia.Media.SolidColorBrush") { expression = "new global::Avalonia.Media.SolidColorBrush(" + expression + ")"; }
                else if (type is "Avalonia.Media.IBrush" or "Avalonia.Media.IImmutableBrush") { expression = "new global::Avalonia.Media.Immutable.ImmutableSolidColorBrush(" + expression + ")"; }
                break;
            case ThemeTextDecorationValue value:
                expression = "new global::AtomUI.TextDecorationInfo { Color = new global::Avalonia.Media.Color(" + (value.Color >> 24) + ", " + ((value.Color >> 16) & 255) + ", " + ((value.Color >> 8) & 255) + ", " + (value.Color & 255) + "), LineType = (global::AtomUI.TextDecorationLine)" + value.LineType + ", LineStyle = (global::AtomUI.LineStyle)" + value.LineStyle + ", Thickness = " + value.Thickness + " }";
                break;
            case ThemeShadowValue value: expression = Shadow(value); break;
            case ThemeShadowValue[] values:
                expression = values.Length == 0 ? "new global::Avalonia.Media.BoxShadows()" : values.Length == 1 ? "new global::Avalonia.Media.BoxShadows(" + Shadow(values[0]) + ")" :
                    "new global::Avalonia.Media.BoxShadows(" + Shadow(values[0]) + ", new global::Avalonia.Media.BoxShadow[] { " + string.Join(", ", values.Skip(1).Select(Shadow)) + " })";
                break;
            case double[] values:
                if (type == "AtomUI.Dimension") { expression = "new global::AtomUI.Dimension(" + Number(values[0]) + ", (global::AtomUI.DimensionUnitType)" + values[1] + ")"; }
                else
                {
                    var constructor = type == "Avalonia.Animation.Easings.Easing" ? "Avalonia.Animation.Easings.SplineEasing" : type == "Avalonia.Media.Immutable.ImmutableTransform" ? "Avalonia.Matrix" : type;
                    expression = "new global::" + constructor + "(" + string.Join(", ", values.Select(Number)) + ")";
                    if (type == "Avalonia.Animation.Easings.Easing") { expression = "new global::Avalonia.Animation.Easings.SplineEasing(new global::Avalonia.Animation.KeySpline(" + string.Join(", ", values.Select(Number)) + "))"; }
                    if (type == "Avalonia.Media.Immutable.ImmutableTransform") { expression = "new global::Avalonia.Media.Immutable.ImmutableTransform(" + expression + ")"; }
                }
                break;
            default: throw new NotSupportedException("Unsupported neutral literal value.");
        }
        return expression;
    }
    private static string Shadow(ThemeShadowValue value) => "new global::Avalonia.Media.BoxShadow { OffsetX = " + Number(value.OffsetX) + ", OffsetY = " + Number(value.OffsetY) + ", Blur = " + Number(value.Blur) + ", Spread = " + Number(value.Spread) +
        ", Color = new global::Avalonia.Media.Color(" + (value.Color >> 24) + ", " + ((value.Color >> 16) & 255) + ", " + ((value.Color >> 8) & 255) + ", " + (value.Color & 255) + "), IsInset = " + (value.IsInset ? "true" : "false") + " }";
    private static string Number(double value) => double.IsNaN(value) ? "global::System.Double.NaN" : double.IsPositiveInfinity(value) ? "global::System.Double.PositiveInfinity" : double.IsNegativeInfinity(value) ? "global::System.Double.NegativeInfinity" : value.ToString("R", CultureInfo.InvariantCulture) + "D";
}
