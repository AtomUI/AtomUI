using System.Globalization;
using AtomUI.Generator.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AtomUI.Generator;

internal static class ControlTokenDefaultModel
{
    private const string DefaultAttribute = "AtomUI.Theme.DesignTokens.ControlTokenDefaultAttribute";
    private const string ContextType = "AtomUI.Theme.DesignTokens.ControlTokenDefaultContext";
    private const string DefaultsInterface = "AtomUI.Theme.DesignTokens.IControlTokenDefaults";

    internal static ControlTokenDefaultPlan? Build(INamedTypeSymbol terminal,
        IReadOnlyList<ControlTokenLayerInfo> layers, Compilation compilation,
        CancellationToken cancellationToken, ICollection<Diagnostic> diagnostics,
        IReadOnlyList<ControlTokenAliasInfo> aliases, out bool isDependencyDefaults)
    {
        var versions = layers.Select(layer => Version(layer.Symbol)).ToArray();
        isDependencyDefaults = versions.Any(static version => version == 2);
        var initialErrors = diagnostics.Count;
        void Error(DiagnosticDescriptor descriptor, ISymbol symbol, string detail)
        {
            var location = symbol.Locations.FirstOrDefault(static location => location.IsInSource);
            if (location is null)
            {
                detail += $"; metadata declaration '{symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)}'";
            }
            diagnostics.Add(Diagnostic.Create(descriptor, location ?? Location.None, terminal.ToDisplayString(), detail));
        }

        if (versions.Any(static version => version is not (1 or 2)) ||
            versions.Distinct().Count() != 1)
        {
            Error(AtomUIDiagnosticDescriptors.ControlTokenDefaultVersion, terminal,
                "all definition layers must select the same supported evaluation version");
            return null;
        }
        if (!isDependencyDefaults)
        {
            foreach (var layer in layers)
            {
                if (Revision(layer.Symbol) != 0 || layer.Symbol.GetMembers().OfType<IMethodSymbol>().Any(HasDefault))
                {
                    Error(AtomUIDiagnosticDescriptors.ControlTokenDefaultVersion, layer.Symbol,
                        "WholeEvaluatorV1 cannot declare dependency defaults or a layer revision");
                }
            }
            return null;
        }

        foreach (var layer in layers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (Revision(layer.Symbol) <= 0)
            {
                Error(AtomUIDiagnosticDescriptors.ControlTokenDefaultVersion, layer.Symbol,
                    "DependencyDefaultsV2 requires a positive revision on every layer");
            }
            if (layer.Symbol.GetMembers("CalculateTokenValues").Any() || layer.Symbol.GetMembers("EvaluateDefaults").Any() ||
                layer.Symbol.GetMembers().OfType<IMethodSymbol>().Any(static method =>
                    method.ExplicitInterfaceImplementations.Any(static implementation =>
                        implementation.ContainingType.ToDisplayString() == DefaultsInterface)))
            {
                Error(AtomUIDiagnosticDescriptors.ControlTokenDefaultVersion, layer.Symbol,
                    "the generated defaults entry and the legacy calculation method cannot be handwritten in V2");
            }
        }
        if (!terminal.IsAbstract &&
            (!terminal.DeclaringSyntaxReferences.Select(reference => reference.GetSyntax(cancellationToken))
                .OfType<ClassDeclarationSyntax>().All(static declaration => declaration.Modifiers.Any(SyntaxKind.PartialKeyword)) ||
             !terminal.AllInterfaces.Any(static type => type.ToDisplayString() == DefaultsInterface)))
        {
            Error(AtomUIDiagnosticDescriptors.ControlTokenDefaultVersion, terminal,
                "a V2 terminal must be partial and implement IControlTokenDefaults");
        }

        var properties = layers.SelectMany(static layer => layer.Symbol.GetMembers().OfType<IPropertySymbol>())
            .Where(static property => !property.GetAttributes().Any(static attribute =>
                attribute.AttributeClass?.ToDisplayString() == TargetMarkConstants.NotTokenDefinitionAttribute))
            .GroupBy(static property => property.Name, StringComparer.Ordinal)
            .ToDictionary(static group => group.Key, static group => group.First(), StringComparer.Ordinal);
        var defaults = new Dictionary<string, (IMethodSymbol Method, string[] Inputs)>(StringComparer.Ordinal);
        var aliasNames = new HashSet<string>(aliases.Select(static alias => alias.Name), StringComparer.Ordinal);
        foreach (var layer in layers)
        {
            var local = new HashSet<string>(StringComparer.Ordinal);
            foreach (var method in layer.Symbol.GetMembers().OfType<IMethodSymbol>().Where(HasDefault))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var attribute = method.GetAttributes().First(static attribute =>
                    attribute.AttributeClass?.ToDisplayString() == DefaultAttribute);
                var name = attribute.ConstructorArguments.FirstOrDefault().Value as string;
                var inputArgument = attribute.ConstructorArguments.Length > 1 ? attribute.ConstructorArguments[1] : default;
                if (name is null || !properties.TryGetValue(name, out var target) ||
                    inputArgument.Kind != TypedConstantKind.Array || inputArgument.IsNull ||
                    inputArgument.Values.Any(static item => item.Value is not string))
                {
                    Error(AtomUIDiagnosticDescriptors.ControlTokenDefaultDeclaration, method,
                        "a default must name an existing Own Token and a non-null list of Own inputs");
                    continue;
                }
                var inputs = inputArgument.Values.Select(static item => (string)item.Value!).ToArray();
                if (aliasNames.Contains(name) || inputs.Any(aliasNames.Contains))
                {
                    Error(AtomUIDiagnosticDescriptors.ControlTokenAliasDeclaration, method,
                        $"'{name}' cannot define an alias default or depend on an alias; use canonical Own fields");
                    continue;
                }
                var replaces = attribute.NamedArguments.Any(static pair =>
                    pair.Key == "ReplacesInheritedDefault" && pair.Value.Value is true);
                var inherited = defaults.ContainsKey(name);
                if (!local.Add(name) || replaces != inherited ||
                    !compilation.IsSymbolAccessibleWithin(method, terminal))
                {
                    Error(AtomUIDiagnosticDescriptors.ControlTokenDefaultDeclaration, method,
                        $"'{name}' has a duplicate, inaccessible or incorrectly marked inherited default");
                    continue;
                }
                var missing = inputs.FirstOrDefault(input => !properties.ContainsKey(input));
                if (missing is not null)
                {
                    Error(AtomUIDiagnosticDescriptors.ControlTokenDefaultDependency, method,
                        $"'{name}' references unknown Own input '{missing}'");
                    continue;
                }
                if (!method.IsStatic || method.Arity != 0 || method.IsAsync || method.IsVararg || method.ReturnsByRef ||
                    method.ReturnsByRefReadonly || method.MethodKind != MethodKind.Ordinary ||
                    !SymbolEqualityComparer.IncludeNullability.Equals(method.ReturnType, target.Type) ||
                    method.Parameters.Length != inputs.Length + 1 ||
                    method.Parameters[0].RefKind != RefKind.In ||
                    method.Parameters[0].Type.ToDisplayString() != ContextType ||
                    method.Parameters.Skip(1).Where((parameter, index) => parameter.RefKind != RefKind.None ||
                        !SymbolEqualityComparer.IncludeNullability.Equals(parameter.Type, properties[inputs[index]].Type)).Any())
                {
                    Error(AtomUIDiagnosticDescriptors.ControlTokenDefaultSignature, method,
                        $"'{name}' requires an exact return type, in context and typed Own parameters in declaration order");
                    continue;
                }
                defaults[name] = (method, inputs);
            }
        }

        if (!terminal.IsAbstract)
        {
            foreach (var name in properties.Keys.Where(name => !aliasNames.Contains(name)).OrderBy(static name => name, StringComparer.Ordinal))
            {
                if (!defaults.ContainsKey(name))
                {
                    Error(AtomUIDiagnosticDescriptors.ControlTokenDefaultDeclaration, properties[name],
                        $"'{name}' has no valid default declaration");
                }
            }
            if (properties.Count == 0)
            {
                Error(AtomUIDiagnosticDescriptors.ControlTokenDefaultDeclaration, terminal,
                    "a dependency-defaults terminal requires Own Tokens");
            }
        }
        if (diagnostics.Count != initialErrors || terminal.IsAbstract)
        {
            return null;
        }

        var names = properties.Keys.OrderBy(static name => name, StringComparer.Ordinal).ToArray();
        var order = new List<string>();
        var visiting = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        bool Visit(string name)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (visited.Contains(name))
            {
                return true;
            }
            if (!visiting.Add(name))
            {
                Error(AtomUIDiagnosticDescriptors.ControlTokenDefaultDependency, defaults[name].Method,
                    $"default dependency cycle includes '{name}' ({string.Join(", ", visiting.OrderBy(static value => value, StringComparer.Ordinal))})");
                return false;
            }
            foreach (var input in defaults[name].Inputs.Distinct(StringComparer.Ordinal).OrderBy(static input => input, StringComparer.Ordinal))
            {
                if (!Visit(input))
                {
                    return false;
                }
            }
            visiting.Remove(name);
            visited.Add(name);
            order.Add(name);
            return true;
        }
        foreach (var name in names)
        {
            if (!aliasNames.Contains(name) && !Visit(name))
            {
                return null;
            }
        }

        var hash = 14695981039346656037UL;
        void Add(string value)
        {
            foreach (var character in value)
            {
                hash = unchecked((hash ^ character) * 1099511628211UL);
            }
            hash = unchecked(hash * 1099511628211UL);
        }
        Add("DependencyDefaultsV2");
        foreach (var layer in layers)
        {
            Add(layer.Symbol.ToDisplayString(GeneratorSymbolDisplay.FullyQualifiedType));
            Add(Revision(layer.Symbol).ToString(CultureInfo.InvariantCulture));
        }
        var steps = new List<ControlTokenDefaultStep>();
        foreach (var name in order)
        {
            var entry = defaults[name];
            var declaringType = entry.Method.ContainingType.ToDisplayString(GeneratorSymbolDisplay.FullyQualifiedType);
            Add(name);
            Add(entry.Method.Name);
            Add(entry.Method.ReturnType.ToDisplayString(GeneratorSymbolDisplay.FullyQualifiedType));
            Add(declaringType);
            foreach (var input in entry.Inputs)
            {
                Add(input);
                Add(properties[input].Type.ToDisplayString(GeneratorSymbolDisplay.FullyQualifiedType));
            }
            steps.Add(new ControlTokenDefaultStep(name, Array.IndexOf(names, name), declaringType,
                entry.Method.Name, entry.Inputs));
        }
        return new ControlTokenDefaultPlan(hash == 0 ? 1 : hash, steps);
    }

    private static bool HasDefault(IMethodSymbol method) => method.GetAttributes().Any(static attribute =>
        attribute.AttributeClass?.ToDisplayString() == DefaultAttribute);

    private static AttributeData Marker(INamedTypeSymbol type) => type.GetAttributes().First(static attribute =>
        attribute.AttributeClass?.ToDisplayString() == TargetMarkConstants.ControlDesignTokenAttribute);

    private static int Version(INamedTypeSymbol type) => Marker(type).ConstructorArguments.Length == 0
        ? 1 : Marker(type).ConstructorArguments[0].Value is int version ? version : -1;

    private static int Revision(INamedTypeSymbol type) => Marker(type).NamedArguments
        .FirstOrDefault(static pair => pair.Key == "Revision").Value.Value is int revision ? revision : 0;
}
