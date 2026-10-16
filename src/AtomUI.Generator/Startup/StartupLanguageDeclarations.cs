using AtomUI.Build.Tasks.LocalizationBuild;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AtomUI.Generator;

internal static class StartupLanguageDeclarations
{
    private static readonly Lazy<IReadOnlyDictionary<string, string>> KnownTags = new(ReadKnownTags);

    // Only symbol-bound AtomUI APIs prove a language. Runtime expressions remain unproved.
    internal static SortedDictionary<string, Location> Read(Compilation compilation)
    {
        var result = new SortedDictionary<string, Location>(StringComparer.Ordinal) { ["en-US"] = Location.None };
        foreach (var tree in compilation.SyntaxTrees)
        {
            var model = compilation.GetSemanticModel(tree);
            foreach (var invocation in tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                var name = invocation.Expression switch
                {
                    MemberAccessExpressionSyntax member => member.Name.Identifier.ValueText,
                    MemberBindingExpressionSyntax member => member.Name.Identifier.ValueText,
                    SimpleNameSyntax simple => simple.Identifier.ValueText,
                    _ => null
                };
                if (name != "UseLanguages")
                {
                    continue;
                }
                if (model.GetSymbolInfo(invocation).Symbol is not IMethodSymbol method || method.Name != "UseLanguages" ||
                    method.ContainingType.ToDisplayString() != "AtomUI.AtomUIBuilderLocalizationExtensions" || !IsStartupConfiguration(invocation, model))
                {
                    continue;
                }
                foreach (var argument in invocation.ArgumentList.Arguments)
                {
                    ReadExpression(argument.Expression, model, result);
                }
            }
        }
        return result;
    }

    private static bool IsStartupConfiguration(InvocationExpressionSyntax invocation, SemanticModel model)
    {
        foreach (var ancestor in invocation.Ancestors())
        {
            if (ancestor is AnonymousFunctionExpressionSyntax lambda)
            {
                return lambda.Parent is ArgumentSyntax argument && argument.Parent?.Parent is InvocationExpressionSyntax call &&
                       model.GetSymbolInfo(call).Symbol is IMethodSymbol target && target.Name == "UseAtomUI" &&
                       target.ContainingType.ToDisplayString() == "AtomUI.AppBuilderExtensions";
            }
            if (ancestor is MethodDeclarationSyntax method)
            {
                return model.GetDeclaredSymbol(method) is IMethodSymbol declared && declared.Parameters.Any(parameter =>
                    parameter.Type.ToDisplayString() == "AtomUI.IAtomUIBuilder");
            }
        }
        return false;
    }

    private static void ReadExpression(ExpressionSyntax expression, SemanticModel model, IDictionary<string, Location> result)
    {
        string? value = null;
        if (model.GetSymbolInfo(expression).Symbol is IPropertySymbol property &&
            property.ContainingType.ToDisplayString() == "AtomUI.Localization.LanguageTags" && KnownTags.Value.TryGetValue(property.Name, out var known))
        {
            value = known;
        }
        else if (expression is InvocationExpressionSyntax parse && parse.ArgumentList.Arguments.Count == 1 &&
                 model.GetSymbolInfo(parse).Symbol is IMethodSymbol parser && parser.Name == "Parse" &&
                 parser.ContainingType.ToDisplayString() == "AtomUI.Localization.LanguageTag" &&
                 model.GetConstantValue(parse.ArgumentList.Arguments[0].Expression) is { HasValue: true, Value: string literal } &&
                 Bcp47LanguageTagParser.TryParse(literal, out var canonical))
        {
            value = canonical;
        }
        if (value is not null)
        {
            result[value] = expression.GetLocation();
            return;
        }
        IEnumerable<ExpressionSyntax> children = expression switch
        {
            ArrayCreationExpressionSyntax { Initializer: { } initializer } => initializer.Expressions,
            ImplicitArrayCreationExpressionSyntax array => array.Initializer.Expressions,
            CollectionExpressionSyntax collection => collection.Elements.OfType<ExpressionElementSyntax>().Select(element => element.Expression),
            ParenthesizedExpressionSyntax parenthesized => new[] { parenthesized.Expression },
            _ => Array.Empty<ExpressionSyntax>()
        };
        foreach (var child in children)
        {
            ReadExpression(child, model, result);
        }
    }

    private static IReadOnlyDictionary<string, string> ReadKnownTags()
    {
        using var stream = typeof(StartupLanguageDeclarations).Assembly.GetManifestResourceStream("AtomUI.LanguageTags.tsv")!;
        using var reader = new StreamReader(stream);
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        while (reader.ReadLine() is { } line)
        {
            if (line.Length == 0 || line[0] == '#')
            {
                continue;
            }
            var fields = line.Split('\t');
            if (fields.Length >= 2 && Bcp47LanguageTagParser.TryParse(fields[1], out var tag))
            {
                result.Add(fields[0], tag);
            }
        }
        return result;
    }
}
