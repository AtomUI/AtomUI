using System.Text;
using AtomUI.Generator.Diagnostics;
using AtomUI.SourceGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AtomUI.Generator.DesignLanguages;

/// <summary>
/// Emits a lazy, language-and-control-conditional registration from a language package's behavior declarations.
/// </summary>
[Generator]
public sealed class ControlBehaviorRegistrationGenerator : IIncrementalGenerator
{
    internal const string AttributeName = "AtomUI.Controls.ControlBehaviorContributionAttribute";
    private const string PackageAttribute = "AtomUI.Theme.Languages.ControlValuePackageRegistrationAttribute";
    private const string PackageType = "global::AtomUI.Theme.Languages.ControlValuePackage";
    private const string LanguageGroupName = "BehaviorLanguageMap";
    private const string ControlGroupPrefix = "BehaviorMap";
    internal const string Registration = "GeneratedControlBehaviorRegistration";
    private const string Descriptor = "global::AtomUI.Controls.ControlBehaviorDescriptor";
    private const string Fragment = "global::AtomUI.Registration.ControlValueFragmentAttribute";
    private const string Runtime = "global::AtomUI.Registration.ControlRegistrationRuntime";
    private const string TypeMapping = "global::System.Runtime.InteropServices.TypeMapping";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var declarations = context.SyntaxProvider.ForAttributeWithMetadataName(
            AttributeName,
            static (node, _) => node is ClassDeclarationSyntax,
            static (input, _) => RegistrationType.MetadataNameOf((INamedTypeSymbol)input.TargetSymbol)).Collect();
        var ruleDeclarations = context.SyntaxProvider.ForAttributeWithMetadataName(
            TargetMarkConstants.ControlTokenRuleAttribute,
            static (node, _) => node is ClassDeclarationSyntax,
            static (_, _) => true).Collect();
        var output = context.CompilationProvider
            .Combine(declarations)
            .Combine(ruleDeclarations)
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Select(static (input, token) => Build(
                input.Left.Left.Left,
                input.Left.Left.Right,
                !input.Left.Right.IsEmpty,
                input.Right.GlobalOptions,
                token));
        context.RegisterSourceOutput(output, static (context, result) => result.Write(context));
    }

    internal static bool HasValidContributions(Compilation compilation, CancellationToken cancellationToken)
    {
        var attributeType = compilation.GetTypeByMetadataName(AttributeName);
        var behaviorBase = compilation.GetTypeByMetadataName("AtomUI.Controls.ControlBehavior`1");
        var languageBase = compilation.GetTypeByMetadataName("AtomUI.Theme.DesignTokens.LanguageDesignToken");
        if (attributeType is null || behaviorBase is null || languageBase is null)
        {
            return false;
        }

        foreach (var type in RegistrationModelBuilder.Types(compilation.Assembly.GlobalNamespace))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var attribute = type.GetAttributes().FirstOrDefault(
                value => SymbolEqualityComparer.Default.Equals(value.AttributeClass, attributeType));
            if (attribute is null)
            {
                continue;
            }

            var language = attribute.ConstructorArguments.Length == 1 ? attribute.ConstructorArguments[0].Value as ITypeSymbol : null;
            var control = FindControl(type, behaviorBase);
            if (language is not null &&
                SemanticPartTypeResolver.IsAssignableTo(language, languageBase) &&
                control is not null &&
                !type.IsAbstract &&
                !type.IsGenericType &&
                RegistrationModelBuilder.Accessible(type) &&
                type.InstanceConstructors.Any(static ctor =>
                    ctor.Parameters.Length == 0 && ctor.DeclaredAccessibility == Accessibility.Public))
            {
                return true;
            }
        }
        return false;
    }

    internal static bool HasGeneratedThemePackage(
        Compilation compilation,
        Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions options,
        string generatedNamespace)
    {
        if (compilation.GetTypeByMetadataName("AtomUI.Theme.Resources.TokenResourceExtension`1") is null ||
            compilation.GetTypeByMetadataName("AtomUI.Registration.ControlPackageMarkerAttribute") is null ||
            compilation.GetTypeByMetadataName("AtomUI.Registration.ControlPackageDefinition") is null ||
            !RegistrationFrameworkResolver.TryResolve(compilation, options, out _, out _))
        {
            return false;
        }

        foreach (var type in RegistrationModelBuilder.Types(compilation.Assembly.GlobalNamespace))
        {
            if (type.Name.StartsWith("GeneratedThemeAssetResource_", StringComparison.Ordinal) &&
                string.Equals(type.ContainingNamespace?.ToDisplayString(), generatedNamespace, StringComparison.Ordinal))
            {
                return true;
            }
        }
        return false;
    }

    private static GenerationResult Build(
        Compilation compilation,
        IEnumerable<string> declarations,
        bool hasTokenRules,
        Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions options,
        CancellationToken cancellationToken)
    {
        var output = new GenerationOutput();
        var behaviorBase = compilation.GetTypeByMetadataName("AtomUI.Controls.ControlBehavior`1");
        var languageBase = compilation.GetTypeByMetadataName("AtomUI.Theme.DesignTokens.LanguageDesignToken");
        if (behaviorBase is null || languageBase is null)
        {
            return output.Freeze();
        }

        var entries = new List<Contribution>();
        var keys = new HashSet<(string Language, string Control)>();
        foreach (var name in declarations.Distinct(StringComparer.Ordinal).OrderBy(static name => name, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (compilation.GetTypeByMetadataName(name) is not { } behavior)
            {
                continue;
            }
            var attribute = behavior.GetAttributes().First(value => value.AttributeClass?.ToDisplayString() == AttributeName);
            var language = attribute.ConstructorArguments.Length == 1 ? attribute.ConstructorArguments[0].Value as ITypeSymbol : null;
            var control = FindControl(behavior, behaviorBase);
            if (language is null || !SemanticPartTypeResolver.IsAssignableTo(language, languageBase) || control is null ||
                behavior.IsAbstract || behavior.IsGenericType || !RegistrationModelBuilder.Accessible(behavior) ||
                !behavior.InstanceConstructors.Any(static constructor =>
                    constructor.Parameters.Length == 0 && constructor.DeclaredAccessibility == Accessibility.Public))
            {
                Report($"'{name}' must be a concrete ControlBehavior<TControl> with a public parameterless constructor and a valid language token type.");
                continue;
            }
            if (control is INamedTypeSymbol controlSymbol &&
                controlSymbol.ContainingNamespace.GetTypeMembers(controlSymbol.Name + "Behavior")
                    .FirstOrDefault(type => SemanticPartTypeResolver.IsAssignableTo(type, behaviorBase.Construct(control))) is { } contract &&
                !SemanticPartTypeResolver.IsAssignableTo(behavior, contract))
            {
                Report($"'{name}' must inherit the control's declared policy contract '{contract.ToDisplayString()}'.");
                continue;
            }

            var controlName = control.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var languageName = language.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if (!keys.Add((languageName, controlName)))
            {
                Report($"Both '{name}' and another declaration register '{controlName}' under '{languageName}'.");
                continue;
            }
            var isExternalOwner = !SymbolEqualityComparer.Default.Equals(control.ContainingAssembly, compilation.Assembly);
            entries.Add(new Contribution(
                controlName,
                languageName,
                behavior.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                control.ContainingAssembly.Name,
                isExternalOwner));

            void Report(string message) => output.ReportDiagnostic(Diagnostic.Create(
                AtomUIDiagnosticDescriptors.InvalidControlBehaviorContribution,
                behavior.Locations.FirstOrDefault(static location => location.IsInSource) ?? Location.None,
                message));
        }

        if (entries.Count != 0)
        {
            var generatedNamespace = GeneratedCodeNamespace.ForAssembly(compilation.AssemblyName);
            var useTypeMap = compilation.GetTypeByMetadataName("AtomUI.Registration.ControlValueMapAttribute") is not null &&
                RegistrationFrameworkResolver.TryResolve(compilation, options, out var framework, out _) &&
                framework == AtomUI.Build.Tasks.Registration.RegistrationFrameworkFamily.OfficialTypeMap;
            var emitStandaloneValuePackages = !hasTokenRules && compilation.GetTypeByMetadataName(PackageAttribute) is not null;
            var hasThemePackage = emitStandaloneValuePackages && HasGeneratedThemePackage(compilation, options, generatedNamespace);
            output.AddSource(
                Registration + ".g.cs",
                GeneratedSourceText.From(Write(
                    generatedNamespace,
                    compilation.Assembly.Name,
                    entries,
                    useTypeMap,
                    emitStandaloneValuePackages,
                    hasThemePackage)));
        }
        return output.Freeze();
    }

    private static ITypeSymbol? FindControl(INamedTypeSymbol behavior, INamedTypeSymbol behaviorBase)
    {
        for (var type = behavior.BaseType; type is not null; type = type.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, behaviorBase))
            {
                return type.TypeArguments[0];
            }
        }
        return null;
    }

    private static string Write(
        string targetNamespace,
        string assemblyName,
        IReadOnlyList<Contribution> entries,
        bool useTypeMap,
        bool emitStandaloneValuePackages,
        bool hasThemePackage = false)
    {
        var source = new StringBuilder("// <auto-generated />\n#nullable enable\n");
        var languageGroups = entries
            .GroupBy(static item => item.Language, StringComparer.Ordinal)
            .OrderBy(static group => group.Key, StringComparer.Ordinal)
            .Select(static group => (Language: group.Key, Items: (IReadOnlyList<Contribution>)group.ToArray()))
            .ToArray();
        var standalonePackages = emitStandaloneValuePackages
            ? entries.Where(static item => item.IsExternalOwner)
                .Select(static item => (item.ControlAssembly, item.Language))
                .Distinct()
                .OrderBy(static pair => pair.ControlAssembly, StringComparer.Ordinal)
                .ThenBy(static pair => pair.Language, StringComparer.Ordinal)
                .ToArray()
            : Array.Empty<(string ControlAssembly, string Language)>();

        for (var index = 0; index < standalonePackages.Length; index++)
        {
            var member = StandaloneMember(index, standalonePackages.Length);
            source.Append("[assembly: global::").Append(PackageAttribute).Append("(typeof(global::")
                .Append(targetNamespace).Append('.').Append(Registration).Append("), ")
                .Append(ThemeBuildContracts.Literal(member));
            if (useTypeMap)
            {
                source.Append(", ").Append(ThemeBuildContracts.Literal("Resolve" + member));
            }
            source.AppendLine(")]");
        }

        if (useTypeMap)
        {
            var owner = "global::" + targetNamespace + "." + Registration + ".";
            source.AppendLine("#pragma warning disable IL2026, IL3050, SYSLIB5004 // NativeAOT TypeMap metadata.");
            if (standalonePackages.Length != 0)
            {
                source.Append("[assembly: global::AtomUI.Registration.ControlValueMap(typeof(")
                    .Append(owner).Append("BehaviorPackageMap))]").AppendLine();
                for (var index = 0; index < standalonePackages.Length; index++)
                {
                    var member = StandaloneMember(index, standalonePackages.Length);
                    source.Append("[assembly: global::System.Runtime.InteropServices.TypeMap<")
                        .Append(owner).Append("BehaviorPackageMap>(")
                        .Append(ThemeBuildContracts.Literal(member)).Append(", typeof(")
                        .Append(owner).Append("BehaviorPackageFragment").Append(index).Append("), typeof(")
                        .Append(standalonePackages[index].Language).AppendLine("))]");
                }
            }
            source.Append("[assembly: global::AtomUI.Registration.ControlValueMap(typeof(")
                .Append(owner).Append(LanguageGroupName).AppendLine("))]");
            for (var groupIndex = 0; groupIndex < languageGroups.Length; groupIndex++)
            {
                source.Append("[assembly: global::System.Runtime.InteropServices.TypeMap<")
                    .Append(owner).Append(LanguageGroupName).Append(">(\"")
                    .Append(groupIndex).Append("\", typeof(")
                    .Append(owner).Append("LanguageProxy").Append(groupIndex).Append("), typeof(")
                    .Append(languageGroups[groupIndex].Language).AppendLine("))]");
                source.Append("[assembly: global::AtomUI.Registration.ControlValueMap(typeof(")
                    .Append(owner).Append(ControlGroupPrefix).Append(groupIndex).AppendLine("))]");
                for (var index = 0; index < languageGroups[groupIndex].Items.Count; index++)
                {
                    source.Append("[assembly: global::System.Runtime.InteropServices.TypeMap<")
                        .Append(owner).Append(ControlGroupPrefix).Append(groupIndex).Append(">(\"")
                        .Append(index).Append("\", typeof(")
                        .Append(owner).Append("Proxy").Append(groupIndex).Append('_').Append(index).Append("), typeof(")
                        .Append(languageGroups[groupIndex].Items[index].Control).AppendLine("))]");
                }
            }
        }

        source.Append("namespace ").Append(targetNamespace).AppendLine(";");
        source.Append("public static class ").AppendLine(Registration).AppendLine("{");
        source.AppendLine("    public static void Register(global::AtomUI.Theme.IThemeManagerBuilder builder)");
        source.AppendLine("    {");
        source.AppendLine("        global::System.ArgumentNullException.ThrowIfNull(builder);");
        if (useTypeMap)
        {
            source.AppendLine("        if (" + Runtime + ".UsesNativeValueRules)");
            source.AppendLine("        {");
            source.AppendLine("            var languageMap = " + TypeMapping + ".GetOrCreateExternalTypeMapping<" + LanguageGroupName + ">();");
            for (var groupIndex = 0; groupIndex < languageGroups.Length; groupIndex++)
            {
                source.Append("            if (").Append(Fragment).Append("<global::System.Action<global::AtomUI.Theme.IThemeManagerBuilder>>.Resolve(languageMap, \"")
                    .Append(groupIndex).Append("\") is { } group").Append(groupIndex).AppendLine(")");
                source.AppendLine("            {");
                source.Append("                group").Append(groupIndex).AppendLine("(builder);");
                source.AppendLine("            }");
            }
            source.AppendLine("            return;");
            source.AppendLine("        }");
        }
        for (var groupIndex = 0; groupIndex < languageGroups.Length; groupIndex++)
        {
            source.Append("        RegisterLanguageGroup").Append(groupIndex).AppendLine("(builder);");
        }
        source.AppendLine("    }");

        if (standalonePackages.Length != 0)
        {
            if (useTypeMap)
            {
                source.AppendLine("    public sealed class BehaviorPackageMap { private BehaviorPackageMap() { } }");
                source.Append("    private static class LinkedBehaviorPackages { internal static readonly global::System.Collections.Generic.IReadOnlyDictionary<string, global::System.Type> Map = ")
                    .Append(TypeMapping).AppendLine(".GetOrCreateExternalTypeMapping<BehaviorPackageMap>(); }");
            }
            for (var index = 0; index < standalonePackages.Length; index++)
            {
                var (controlAssembly, language) = standalonePackages[index];
                var member = StandaloneMember(index, standalonePackages.Length);
                var langGroupIndex = Array.FindIndex(languageGroups, group => group.Language == language);
                source.Append("    public static ").Append(PackageType).Append(' ').Append(member)
                    .Append(" => BehaviorPackageHolder").Append(index).AppendLine(".Value;");
                if (useTypeMap)
                {
                    source.Append("    public static ").Append(PackageType).Append("? Resolve").Append(member).Append("() => ")
                        .Append(Runtime).Append(".UsesNativeValueRules ? ").Append(Fragment).Append('<').Append(PackageType)
                        .Append(">.Resolve(LinkedBehaviorPackages.Map, ").Append(ThemeBuildContracts.Literal(member)).Append(") : ")
                        .Append(member).AppendLine(";");
                }
                source.Append("    private static class BehaviorPackageHolder").Append(index).AppendLine();
                source.AppendLine("    {");
                source.Append("        internal static readonly ").Append(PackageType).Append(" Value = new(")
                    .Append(ThemeBuildContracts.Literal(assemblyName)).Append(", ")
                    .Append(ThemeBuildContracts.Literal(controlAssembly)).Append(", typeof(")
                    .Append(language).Append("), RegisterStandalonePackage").Append(index)
                    .AppendLine(hasThemePackage ? ", PackageMetadata.Definition);" : ");");
                source.AppendLine("    }");
                source.Append("    private static void RegisterStandalonePackage").Append(index)
                    .Append("(global::AtomUI.Theme.IThemeManagerBuilder builder) => ");
                if (useTypeMap)
                {
                    source.Append('(').Append(Runtime).Append(".UsesNativeValueRules ? (global::System.Action<global::AtomUI.Theme.IThemeManagerBuilder>)RegisterLinkedGroup")
                        .Append(langGroupIndex).Append(" : RegisterLanguageGroup").Append(langGroupIndex).AppendLine(")(builder);");
                    source.Append("    [BehaviorPackageFragment").Append(index).Append("] internal sealed class BehaviorPackageFragment").Append(index)
                        .Append(" : ").Append(Fragment).Append('<').Append(PackageType).AppendLine(">");
                    source.AppendLine("    {");
                    source.Append("        public override ").Append(PackageType).Append(" Value => ").Append(member).AppendLine(";");
                    source.AppendLine("    }");
                }
                else
                {
                    source.Append("RegisterLanguageGroup").Append(langGroupIndex).AppendLine("(builder);");
                }
            }
        }

        if (useTypeMap)
        {
            source.AppendLine("    public sealed class " + LanguageGroupName + " { private " + LanguageGroupName + "() { } }");
        }

        for (var groupIndex = 0; groupIndex < languageGroups.Length; groupIndex++)
        {
            var items = languageGroups[groupIndex].Items;
            source.Append("    internal static void RegisterLanguageGroup").Append(groupIndex)
                .AppendLine("(global::AtomUI.Theme.IThemeManagerBuilder builder)");
            source.AppendLine("    {");
            for (var index = 0; index < items.Count; index++)
            {
                source.Append("        builder.AddControlBehavior(Definition").Append(groupIndex).Append('_').Append(index).AppendLine(".Value);");
            }
            source.AppendLine("    }");

            if (useTypeMap)
            {
                source.Append("    public sealed class ").Append(ControlGroupPrefix).Append(groupIndex)
                    .Append(" { private ").Append(ControlGroupPrefix).Append(groupIndex).AppendLine("() { } }");
                source.Append("    internal static void RegisterLinkedGroup").Append(groupIndex)
                    .AppendLine("(global::AtomUI.Theme.IThemeManagerBuilder builder)");
                source.AppendLine("    {");
                source.Append("        var map = ").Append(TypeMapping).Append(".GetOrCreateExternalTypeMapping<")
                    .Append(ControlGroupPrefix).Append(groupIndex).AppendLine(">();");
                for (var index = 0; index < items.Count; index++)
                {
                    source.Append("        if (").Append(Fragment).Append('<').Append(Descriptor).Append(">.Resolve(map, \"")
                        .Append(index).Append("\") is { } entry").Append(index).AppendLine(")");
                    source.AppendLine("        {");
                    source.Append("            builder.AddControlBehavior(entry").Append(index).AppendLine(");");
                    source.AppendLine("        }");
                }
                source.AppendLine("    }");
                source.Append("    [LanguageProxy").Append(groupIndex).Append("] internal sealed class LanguageProxy").Append(groupIndex)
                    .Append(" : ").Append(Fragment).AppendLine("<global::System.Action<global::AtomUI.Theme.IThemeManagerBuilder>>");
                source.AppendLine("    {");
                source.Append("        public override global::System.Action<global::AtomUI.Theme.IThemeManagerBuilder> Value => RegisterLinkedGroup")
                    .Append(groupIndex).AppendLine(";");
                source.AppendLine("    }");
            }

            for (var index = 0; index < items.Count; index++)
            {
                var entry = items[index];
                source.Append("    private static class Definition").Append(groupIndex).Append('_').Append(index).AppendLine();
                source.AppendLine("    {");
                source.Append("        internal static readonly ").Append(Descriptor).Append(" Value = ").Append(Descriptor)
                    .Append(".Create<").Append(entry.Control).Append(", ").Append(entry.Language).Append(", ")
                    .Append(entry.Type).AppendLine(">();");
                source.AppendLine("    }");
                if (useTypeMap)
                {
                    source.Append("    [Proxy").Append(groupIndex).Append('_').Append(index).Append("] internal sealed class Proxy")
                        .Append(groupIndex).Append('_').Append(index).Append(" : ").Append(Fragment).Append('<').Append(Descriptor).AppendLine(">");
                    source.AppendLine("    {");
                    source.Append("        public override ").Append(Descriptor).Append(" Value => Definition")
                        .Append(groupIndex).Append('_').Append(index).AppendLine(".Value;");
                    source.AppendLine("    }");
                }
            }
        }
        source.AppendLine("}");
        if (useTypeMap)
        {
            source.AppendLine("#pragma warning restore IL2026, IL3050, SYSLIB5004");
        }
        return source.ToString();
    }

    private static string StandaloneMember(int index, int count) =>
        count == 1 ? "BehaviorPackage" : "BehaviorPackage" + index.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private sealed record Contribution(string Control, string Language, string Type, string ControlAssembly, bool IsExternalOwner);
}
