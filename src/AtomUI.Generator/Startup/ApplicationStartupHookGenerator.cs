using System.Collections.Immutable;
using System.Text;
using AtomUI.Generator.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using AtomUI.SourceGeneration;

namespace AtomUI.Generator.Startup;

/// <summary>
/// 为每个具体的 partial Avalonia Application 生成 RegisterServices 重写，在 base.RegisterServices() 之后
/// 调用 AtomUIStartup.Attach(this)。已手写重写且调用了 Attach 的跳过；具体类手写重写未调用 Attach 的报
/// ATOMUIGEN039。抽象、泛型、嵌套类不生成，由运行时守卫给出修复说明；抽象类自身也不报缺少 Attach，
/// 它无法实例化也无法自行修复，其派生类由本生成器补齐挂接。
/// </summary>
[Generator]
public sealed class ApplicationStartupHookGenerator : IIncrementalGenerator
{
    private const string ApplicationMetadataName = "Avalonia.Application";
    private const string StartupMetadataName = "AtomUI.AtomUIStartup";

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

        // 只统计类型自身声明的重写：抽象基类里的重写由派生类经 base 调用继续生效，
        // 因此不影响派生类是否还需要生成挂接。
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

    // 按符号判定，而不是文本：开发者可能用别名、限定名或 using static 调用同一个 Attach，
    // 文本匹配会把别名写法误报成「缺少 Attach」。
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
            source.Append(indent).AppendLine("        global::AtomUI.AtomUIStartup.Attach(this);");
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
