using System.Text;
using AtomUI.Generator.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using AtomUI.SourceGeneration;

namespace AtomUI.Generator.Startup;

/// <summary>
/// Generates a <c>RegisterServices</c> override for every concrete partial Avalonia <c>Application</c>. The override
/// calls <c>ApplicationStartup.Attach(this)</c> after <c>base.RegisterServices()</c>.
/// </summary>
/// <remarks>
/// <para>
/// A class whose handwritten override already calls <c>Attach</c> is skipped. A concrete class whose handwritten
/// override does not call it is reported with the error ATOMUIGEN039.
/// </para>
/// <para>
/// Abstract, generic and nested classes get no generated override, and the runtime guard explains how to fix that.
/// An abstract class is not reported for a missing <c>Attach</c> either: it cannot be instantiated and cannot fix
/// itself, and its derived classes get the hook from this generator.
/// </para>
/// </remarks>
[Generator]
public sealed class ApplicationStartupHookGenerator : IIncrementalGenerator
{
    private const string ApplicationMetadataName = "Avalonia.Application";
    private const string StartupMetadataName = "AtomUI.ApplicationStartup";

    /// <summary>
    /// Registers the collection of the candidate application classes and the step that reports ATOMUIGEN039 or adds
    /// the generated source for a class. Both run only when the compilation contains the
    /// <c>AtomUI.ApplicationStartup</c> type.
    /// </summary>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var hasStartup = context.CompilationProvider
            .Select(static (compilation, _) => compilation.GetTypeByMetadataName(StartupMetadataName) is not null);
        var candidates = context.SyntaxProvider.CreateSyntaxProvider(
                static (node, _) => node is ClassDeclarationSyntax,
                static (syntaxContext, token) => Analyze(syntaxContext, token))
            .Where(static item => item is not null)
            .Collect();
        context.RegisterSourceOutput(candidates.Combine(hasStartup), static (production, input) =>
        {
            if (!input.Right)
            {
                return;
            }
            foreach (var item in input.Left.Distinct())
            {
                if (item!.MissingAttach)
                {
                    production.ReportDiagnostic(Diagnostic.Create(
                        AtomUIDiagnosticDescriptors.StartupHookMissingAttach, item.Location, item.TypeName));
                    continue;
                }
                if (!item.NeedsHook && item.ImageClassName is null)
                {
                    continue;
                }
                production.AddSource(item.HintName, Write(item));
            }
        });
    }

    private static HookTarget? Analyze(GeneratorSyntaxContext context, CancellationToken token)
    {
        var declaration = (ClassDeclarationSyntax)context.Node;
        if (context.SemanticModel.GetDeclaredSymbol(declaration, token) is not INamedTypeSymbol symbol ||
            !InheritsFromApplication(symbol) || !IsPrimaryDeclaration(symbol, declaration))
        {
            return null;
        }

        var location = declaration.Identifier.GetLocation();
        var typeName = symbol.ToDisplayString();
        var ns = symbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : symbol.ContainingNamespace.ToDisplayString();
        var hintName = (ns.Length == 0 ? "" : ns + ".") + symbol.MetadataName.Replace('`', '_') + ".StartupHook.g.cs";
        var eligible = !symbol.IsAbstract && !symbol.IsGenericType && symbol.ContainingType is null && IsPartial(symbol, token);
        var imageClass = eligible && context.SemanticModel.Compilation.GetTypeByMetadataName("AtomUI.Registration.IGeneratedStartupImageHost") is not null
            ? "global::" + GeneratedCodeNamespace.ForAssembly(symbol.ContainingAssembly.Name) + ".GeneratedApplicationStartupImage"
            : null;

        // Only overrides declared by the type itself are counted. An override in an abstract base class stays in
        // effect because the derived class calls it through base, so it does not decide whether the derived class
        // still needs a generated hook.
        var ownOverrides = symbol.GetMembers("RegisterServices")
            .OfType<IMethodSymbol>()
            .Where(static method => method.IsOverride && method.Parameters.Length == 0)
            .ToArray();
        if (ownOverrides.Length != 0)
        {
            var callsAttach = ownOverrides.Any(method => CallsAttach(method, context.SemanticModel, token));
            return new HookTarget(typeName, ns, symbol.Name, hintName, location,
                NeedsHook: false,
                MissingAttach: !callsAttach && !symbol.IsAbstract, ImageClassName: imageClass);
        }

        if (!eligible)
        {
            return null;
        }

        return new HookTarget(typeName, ns, symbol.Name, hintName, location,
            NeedsHook: true, MissingAttach: false, ImageClassName: imageClass);
    }

    // Decides by symbol, not by text: the same Attach can be called through an alias, a qualified name or
    // a using static directive, and text matching would report the alias form as a missing Attach.
    private static bool CallsAttach(IMethodSymbol method, SemanticModel semanticModel, CancellationToken token)
    {
        foreach (var reference in method.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax(token) is not MethodDeclarationSyntax declaration)
            {
                continue;
            }
            var declarationModel = semanticModel.Compilation.GetSemanticModel(declaration.SyntaxTree);
            foreach (var invocation in declaration.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                if (declarationModel.GetSymbolInfo(invocation, token).Symbol is IMethodSymbol invoked &&
                    invoked.Name == "Attach" &&
                    invoked.ContainingType?.ToDisplayString() == StartupMetadataName)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static bool InheritsFromApplication(INamedTypeSymbol symbol)
    {
        for (var current = symbol.BaseType; current is not null; current = current.BaseType)
        {
            if (current.ToDisplayString() == ApplicationMetadataName)
            {
                return true;
            }
        }
        return false;
    }

    private static bool IsPrimaryDeclaration(INamedTypeSymbol symbol, ClassDeclarationSyntax declaration)
    {
        var primary = symbol.DeclaringSyntaxReferences.FirstOrDefault();
        return primary is not null &&
               primary.Span == declaration.Span &&
               primary.SyntaxTree == declaration.SyntaxTree;
    }

    private static bool IsPartial(INamedTypeSymbol symbol, CancellationToken token) =>
        symbol.DeclaringSyntaxReferences.All(reference =>
            reference.GetSyntax(token) is ClassDeclarationSyntax part && part.Modifiers.Any(SyntaxKind.PartialKeyword));

    private static Microsoft.CodeAnalysis.Text.SourceText Write(HookTarget target)
    {
        var source = new StringBuilder();
        source.AppendLine("// <auto-generated />");
        source.AppendLine("#nullable enable");
        var indent = string.Empty;
        if (target.Namespace.Length != 0)
        {
            source.Append("namespace ").AppendLine(target.Namespace);
            source.AppendLine("{");
            indent = "    ";
        }
        source.Append(indent).Append("partial class ").Append(target.ClassName);
        if (target.ImageClassName is not null)
        {
            source.Append(" : global::AtomUI.Registration.IGeneratedStartupImageHost");
        }
        source.AppendLine();
        source.Append(indent).AppendLine("{");
        if (target.NeedsHook)
        {
            source.Append(indent).AppendLine("    public override void RegisterServices()");
            source.Append(indent).AppendLine("    {");
            source.Append(indent).AppendLine("        base.RegisterServices();");
            source.Append(indent).AppendLine("        global::AtomUI.ApplicationStartup.Attach(this);");
            source.Append(indent).AppendLine("    }");
        }
        if (target.ImageClassName is { } image)
        {
            source.Append(indent).Append("    global::AtomUI.Registration.StartupImage global::AtomUI.Registration.IGeneratedStartupImageHost.GetStartupImage() => ")
                .Append(image).AppendLine(".Instance;");
        }
        source.Append(indent).AppendLine("}");
        if (target.Namespace.Length != 0)
        {
            source.AppendLine("}");
        }
        return GeneratedSourceText.From(source.ToString());
    }

    private sealed record HookTarget(string TypeName, string Namespace, string ClassName, string HintName,
        Location Location, bool NeedsHook, bool MissingAttach, string? ImageClassName = null);
}
