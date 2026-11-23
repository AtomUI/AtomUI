using AtomUI.Generator.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace AtomUI.Generator.DesignToken;

/// <summary>
/// Emits the implementing half of every partial token property of the global, control and language token classes: a
/// plain field read and a setter that skips frozen properties. Uses <c>RegisterSourceOutput</c> so the implementation is visible at IDE design time.
/// </summary>
[Generator]
public sealed class TokenPropertyImplementationGenerator : IIncrementalGenerator
{
    internal const string ModelStepName = "TokenPropertyImplementationModel";

    private static readonly SymbolDisplayFormat TypeFormat = SymbolDisplayFormat.FullyQualifiedFormat
        .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    public void Initialize(IncrementalGeneratorInitializationContext initContext)
    {
        var controlTokens = initContext.SyntaxProvider.ForAttributeWithMetadataName(
                TargetMarkConstants.ControlDesignTokenAttribute,
                static (node, _) => node is ClassDeclarationSyntax,
                static (context, token) => CreateModel(context, TargetMarkConstants.ControlDesignTokenAttribute, isControlToken: true, token))
            .WithTrackingName(ModelStepName);
        var globalTokens = initContext.SyntaxProvider.ForAttributeWithMetadataName(
                TargetMarkConstants.GlobalDesignTokenAttribute,
                static (node, _) => node is ClassDeclarationSyntax,
                static (context, token) => CreateModel(context, TargetMarkConstants.GlobalDesignTokenAttribute, isControlToken: false, token))
            .WithTrackingName(ModelStepName);

        var languageTokens = initContext.SyntaxProvider.ForAttributeWithMetadataName(
                TargetMarkConstants.LanguageDesignTokenAttribute,
                static (node, _) => node is ClassDeclarationSyntax,
                static (context, token) => CreateModel(context, TargetMarkConstants.LanguageDesignTokenAttribute, isControlToken: true, token))
            .WithTrackingName(ModelStepName);

        initContext.RegisterSourceOutput(controlTokens, static (context, model) => Emit(context, model));
        initContext.RegisterSourceOutput(globalTokens, static (context, model) => Emit(context, model));
        initContext.RegisterSourceOutput(languageTokens, static (context, model) => Emit(context, model));
    }

    private static void Emit(SourceProductionContext context, TokenTypeImplementationModel model)
    {
        foreach (var diagnostic in model.Diagnostics)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                AtomUIDiagnosticDescriptors.TokenPropertyNotPartial,
                Location.Create(diagnostic.FilePath, diagnostic.Span, diagnostic.LineSpan),
                diagnostic.PropertyName,
                diagnostic.TypeName));
        }

        if (model.Properties.Count != 0)
        {
            context.AddSource(model.HintName, GeneratedSourceText.From(TokenPropertyImplementationWriter.Write(model)));
        }
    }

    // One model per attributed type declaration. A partial type is reported once per attributed declaration;
    // each declaration owns the properties declared inside it, and the first attributed declaration also owns
    // the properties of the type's unattributed declarations, so every property is emitted exactly once.
    private static TokenTypeImplementationModel CreateModel(
        GeneratorAttributeSyntaxContext context,
        string attributeName,
        bool isControlToken,
        CancellationToken cancellationToken)
    {
        var type = (INamedTypeSymbol)context.TargetSymbol;
        var declaration = (ClassDeclarationSyntax)context.TargetNode;
        var attributed = GetAttributedDeclarations(type, attributeName, cancellationToken);
        var isPrimary = attributed.Count == 0 || SameNode(attributed[0], declaration);
        var namespaceName = type.ContainingNamespace.IsGlobalNamespace ? null : type.ContainingNamespace.ToDisplayString();
        var hintName = GetHintName(type.Name, namespaceName, isControlToken, declaration, attributed);
        if (type.ContainingType is not null || type.IsGenericType)
        {
            // Nested and generic token types are rejected by the token model diagnostics.
            return new TokenTypeImplementationModel(hintName, null, type.Name,
                new ValueArray<TokenPropertyImplementation>([]), new ValueArray<TokenPropertyDiagnostic>([]));
        }

        var classIsPartial = declaration.Modifiers.Any(SyntaxKind.PartialKeyword);
        var typeName = type.ToDisplayString();
        var candidates = new List<(PropertyDeclarationSyntax Syntax, IPropertySymbol Symbol)>();
        foreach (var member in type.GetMembers())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (member is not IPropertySymbol { IsStatic: false, IsIndexer: false, IsImplicitlyDeclared: false } property ||
                property.DeclaringSyntaxReferences.Length == 0 ||
                property.DeclaringSyntaxReferences[0].GetSyntax(cancellationToken) is not PropertyDeclarationSyntax syntax ||
                syntax.Parent is not ClassDeclarationSyntax owner)
            {
                continue;
            }

            // Decide ownership from syntax first; attributes are only read for properties this declaration emits.
            if (!SameNode(owner, declaration) && (!isPrimary || attributed.Any(item => SameNode(item, owner))))
            {
                continue;
            }

            if (!HasNotTokenDefinition(property))
            {
                candidates.Add((syntax, property));
            }
        }

        candidates.Sort(static (left, right) =>
        {
            var byPath = string.CompareOrdinal(left.Syntax.SyntaxTree.FilePath, right.Syntax.SyntaxTree.FilePath);
            return byPath != 0 ? byPath : left.Syntax.SpanStart.CompareTo(right.Syntax.SpanStart);
        });

        var properties = new List<TokenPropertyImplementation>();
        var diagnostics = new List<TokenPropertyDiagnostic>();
        foreach (var (syntax, property) in candidates)
        {
            if (!classIsPartial || !syntax.Modifiers.Any(SyntaxKind.PartialKeyword) || syntax.AccessorList is null)
            {
                var location = syntax.Identifier.GetLocation();
                diagnostics.Add(new TokenPropertyDiagnostic(syntax.Identifier.ValueText, typeName,
                    location.SourceTree?.FilePath ?? string.Empty, location.SourceSpan, location.GetLineSpan().Span));
                continue;
            }

            properties.Add(new TokenPropertyImplementation(
                syntax.Identifier.Text,
                string.Join(" ", syntax.Modifiers.Select(static modifier => modifier.Text)),
                property.Type.ToDisplayString(TypeFormat),
                new ValueArray<TokenAccessorImplementation>(syntax.AccessorList.Accessors.Select(static accessor =>
                    new TokenAccessorImplementation(
                        string.Join(" ", accessor.Modifiers.Select(static modifier => modifier.Text)),
                        accessor.Keyword.Text)))));
        }

        return new TokenTypeImplementationModel(
            hintName,
            namespaceName,
            type.Name,
            new ValueArray<TokenPropertyImplementation>(properties),
            new ValueArray<TokenPropertyDiagnostic>(diagnostics));
    }

    private static List<ClassDeclarationSyntax> GetAttributedDeclarations(
        INamedTypeSymbol type,
        string attributeName,
        CancellationToken cancellationToken)
    {
        var result = new List<ClassDeclarationSyntax>();
        foreach (var attribute in type.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() == attributeName &&
                attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken).Parent?.Parent is ClassDeclarationSyntax owner &&
                !result.Any(item => SameNode(item, owner)))
            {
                result.Add(owner);
            }
        }

        result.Sort(static (left, right) =>
        {
            var byPath = string.CompareOrdinal(left.SyntaxTree.FilePath, right.SyntaxTree.FilePath);
            return byPath != 0 ? byPath : left.SpanStart.CompareTo(right.SpanStart);
        });
        return result;
    }

    // Control tokens: "<Namespace>.<TypeName>.TokenProperties.g.cs". The global token is split over many
    // attributed declarations, so it gets one file per declaration, named after the declaring source file
    // ("DesignToken.Seed.cs" -> "<Namespace>.DesignToken.Seed.TokenProperties.g.cs"). The namespace keeps
    // same-named token types of one assembly from colliding on the hint name.
    private static string GetHintName(
        string typeName,
        string? namespaceName,
        bool isControlToken,
        ClassDeclarationSyntax declaration,
        IReadOnlyList<ClassDeclarationSyntax> attributed)
    {
        const string suffix = ".TokenProperties.g.cs";
        var prefix = namespaceName is null ? string.Empty : Sanitize(namespaceName) + ".";
        if (isControlToken || attributed.Count == 0)
        {
            return prefix + Sanitize(typeName) + suffix;
        }

        var stems = attributed.Select(item => FileStem(typeName, item)).ToArray();
        var index = 0;
        for (var i = 0; i < attributed.Count; i++)
        {
            if (SameNode(attributed[i], declaration))
            {
                index = i;
                break;
            }
        }

        var stem = stems[index];
        if (stems.Count(item => item == stem) > 1)
        {
            stem += "." + stems.Take(index).Count(item => item == stem).ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        return prefix + stem + suffix;
    }

    private static string FileStem(string typeName, ClassDeclarationSyntax declaration)
    {
        var path = declaration.SyntaxTree.FilePath;
        var stem = string.IsNullOrEmpty(path) ? typeName : System.IO.Path.GetFileNameWithoutExtension(path);
        if (stem != typeName && !stem.StartsWith(typeName + ".", StringComparison.Ordinal))
        {
            stem = typeName + "." + stem;
        }

        return Sanitize(stem);
    }

    private static string Sanitize(string value)
    {
        var chars = value.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (!char.IsLetterOrDigit(chars[i]) && chars[i] is not ('.' or '_' or '-'))
            {
                chars[i] = '_';
            }
        }

        return new string(chars);
    }

    private static bool SameNode(SyntaxNode left, SyntaxNode right) =>
        left.SyntaxTree == right.SyntaxTree && left.Span == right.Span;

    private static bool HasNotTokenDefinition(IPropertySymbol property)
    {
        foreach (var attribute in property.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() == TargetMarkConstants.NotTokenDefinitionAttribute)
            {
                return true;
            }
        }

        return false;
    }
}

internal sealed record TokenTypeImplementationModel(
    string HintName,
    string? Namespace,
    string TypeName,
    ValueArray<TokenPropertyImplementation> Properties,
    ValueArray<TokenPropertyDiagnostic> Diagnostics);

internal sealed record TokenPropertyImplementation(
    string Name,
    string Modifiers,
    string Type,
    ValueArray<TokenAccessorImplementation> Accessors);

internal sealed record TokenAccessorImplementation(string Modifiers, string Keyword);

internal sealed record TokenPropertyDiagnostic(
    string PropertyName,
    string TypeName,
    string FilePath,
    TextSpan Span,
    LinePositionSpan LineSpan);
