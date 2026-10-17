using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

var options = new Dictionary<string, List<string>>();
for (var index = 0; index < args.Length; index += 2)
{
    if (index + 1 >= args.Length)
    {
        throw new ArgumentException("Options require values.");
    }
    if (!options.TryGetValue(args[index], out var values))
    {
        values = [];
        options.Add(args[index], values);
    }
    values.Add(args[index + 1]);
}
var files = options["--path"]
    .SelectMany(path => Directory.Exists(path) ? Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories) : [path])
    .Distinct().Order().ToArray();
var issues = new List<object>();
var inventory = new List<object>();
var snapshots = new SortedDictionary<string, object>();
var controls = 0;
foreach (var file in files)
{
    var text = File.ReadAllText(file);
    var tree = CSharpSyntaxTree.ParseText(text, new CSharpParseOptions(LanguageVersion.Preview), file);
    var root = tree.GetCompilationUnitRoot();
    var directives = root.DescendantTrivia(descendIntoTrivia: true)
        .Select(trivia => trivia.GetStructure())
        .OfType<DirectiveTriviaSyntax>()
        .Where(directive => directive is RegionDirectiveTriviaSyntax or EndRegionDirectiveTriviaSyntax)
        .OrderBy(directive => directive.SpanStart).ToArray();
    string[] Regions(SyntaxNode node)
    {
        var stack = new List<string>();
        foreach (var directive in directives.TakeWhile(item => item.SpanStart < node.SpanStart))
        {
            if (directive is RegionDirectiveTriviaSyntax region)
            {
                stack.Add(region.ToString()["#region".Length..].Trim());
            }
            else if (stack.Count > 0)
            {
                stack.RemoveAt(stack.Count - 1);
            }
        }
        return stack.ToArray();
    }
    bool In(SyntaxNode node, string region) => Regions(node).Contains(region);
    void Issue(SyntaxNode node, string rule, string member, string message)
    {
        issues.Add(new { file, line = tree.GetLineSpan(node.Span).StartLinePosition.Line + 1, rule, member, message });
    }
    foreach (var diagnostic in tree.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error))
    {
        Issue(root, "syntax", "", diagnostic.ToString());
    }
    foreach (var statement in root.DescendantNodes().OfType<StatementSyntax>())
    {
        StatementSyntax? body = statement switch
        {
            IfStatementSyntax value => value.Statement,
            ForStatementSyntax value => value.Statement,
            CommonForEachStatementSyntax value => value.Statement,
            WhileStatementSyntax value => value.Statement,
            DoStatementSyntax value => value.Statement,
            UsingStatementSyntax value => value.Statement,
            LockStatementSyntax value => value.Statement,
            FixedStatementSyntax value => value.Statement,
            _ => null
        };
        if (body is not null and not BlockSyntax)
        {
            Issue(statement, "control-flow-braces", statement.Kind().ToString(), "Control-flow bodies require braces.");
        }
    }
    foreach (var clause in root.DescendantNodes().OfType<ElseClauseSyntax>())
    {
        if (clause.Statement is not BlockSyntax and not IfStatementSyntax)
        {
            Issue(clause, "control-flow-braces", "else", "Else bodies require braces.");
        }
    }
    var typeSnapshots = new List<object>();
    foreach (var type in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
    {
        var fields = type.Members.OfType<FieldDeclarationSyntax>().ToArray();
        var properties = type.Members.OfType<PropertyDeclarationSyntax>().ToDictionary(member => member.Identifier.ValueText);
        var registrations = fields.Where(IsRegistration).ToArray();
        var headerEnd = type.OpenBraceToken.IsKind(SyntaxKind.OpenBraceToken) ? type.OpenBraceToken.SpanStart : type.Span.End;
        var header = text[type.SpanStart..headerEnd];
        typeSnapshots.Add(new
        {
            type = type.Identifier.ValueText,
            header = Fingerprint(CSharpSyntaxTree.ParseText(header + "{}").GetRoot()),
            members = type.Members.Select(member => new { name = Name(member), hash = Fingerprint(member) })
                .OrderBy(member => member.name).ThenBy(member => member.hash).ToArray()
        });
        inventory.Add(new
        {
            file, type = type.Identifier.ValueText, start = type.OpenBraceToken.Span.End, end = type.CloseBraceToken.SpanStart,
            registrations = registrations.Select(Name).ToArray(),
            members = type.Members.Select(member => new
            {
                name = Name(member), kind = member.Kind().ToString(), access = Access(member),
                region = Regions(member), start = member.SpanStart, end = member.Span.End,
                code = member.WithLeadingTrivia(member.GetLeadingTrivia().Where(trivia => trivia.GetStructure() is not RegionDirectiveTriviaSyntax and not EndRegionDirectiveTriviaSyntax))
                    .WithTrailingTrivia(member.GetTrailingTrivia().Where(trivia => trivia.GetStructure() is not RegionDirectiveTriviaSyntax and not EndRegionDirectiveTriviaSyntax)).ToFullString().Trim(),
            }).ToArray()
        });
        if (registrations.Length == 0)
        {
            continue;
        }
        controls++;
        var directBacking = new HashSet<string>();
        foreach (var field in registrations)
        {
            var expected = Access(field) switch
            {
                "public" or "protected" => "公共属性定义",
                "internal" => "内部属性定义",
                _ => null
            };
            foreach (var variable in field.Declaration.Variables)
            {
                var name = variable.Identifier.ValueText;
                if (expected is not null && !In(field, expected))
                {
                    Issue(field, "property-region", name, $"Registration belongs in {expected}.");
                }
                var propertyName = name.EndsWith("Property", StringComparison.Ordinal) ? name[..^8] : name;
                if (!properties.TryGetValue(propertyName, out var property))
                {
                    continue; // Inherited/attached properties may intentionally have no local wrapper.
                }
                var wrapperRegion = Access(property) switch
                {
                    "public" or "protected" => "公共属性定义",
                    "internal" => "内部属性定义",
                    _ => expected
                };
                if (wrapperRegion is not null && !In(property, wrapperRegion))
                {
                    Issue(property, "wrapper-region", propertyName, $"CLR wrapper belongs in {wrapperRegion}.");
                }
                if (!field.Declaration.Type.ToString().Contains("DirectProperty", StringComparison.Ordinal))
                {
                    continue;
                }
                var references = property.DescendantNodes().OfType<IdentifierNameSyntax>()
                    .Select(identifier => identifier.Identifier.ValueText).ToHashSet();
                foreach (var backing in fields.Where(candidate => !IsRegistration(candidate) &&
                             candidate.Declaration.Variables.Any(value => references.Contains(value.Identifier.ValueText))))
                {
                    foreach (var value in backing.Declaration.Variables)
                    {
                        directBacking.Add(value.Identifier.ValueText);
                    }
                    if (wrapperRegion is not null && !In(backing, wrapperRegion))
                    {
                        Issue(backing, "direct-backing-region", Name(backing), $"DirectProperty backing field belongs beside {propertyName} in {wrapperRegion}.");
                    }
                    var between = type.Members.Where(member => member.SpanStart > backing.SpanStart && member.SpanStart < property.SpanStart);
                    if (backing.SpanStart > property.SpanStart || between.Any(member => member is not FieldDeclarationSyntax))
                    {
                        Issue(backing, "direct-backing-adjacency", Name(backing), $"Keep backing field adjacent to {propertyName}.");
                    }
                }
            }
        }
        foreach (var access in new[] { "public", "internal", "protected" })
        {
            var group = registrations.Where(field => Access(field) == access).ToArray();
            var ordered = group.SelectMany(field => field.Declaration.Variables)
                .Select(variable => variable.Identifier.ValueText)
                .Where(name => name.EndsWith("Property", StringComparison.Ordinal)).Select(name => name[..^8])
                .Where(properties.ContainsKey).Where(name => Access(properties[name]) == access).ToArray();
            var actual = ordered.OrderBy(name => properties[name].SpanStart).ToArray();
            if (!ordered.SequenceEqual(actual))
            {
                Issue(type, "wrapper-order", type.Identifier.ValueText, $"{access} wrappers must follow registration order: {string.Join(", ", ordered)}.");
            }
            if (ordered.Length > 0 && ordered.Any(name => properties[name].SpanStart < group.Max(field => field.Span.End)))
            {
                Issue(type, "registration-block", type.Identifier.ValueText, $"Keep {access} registrations before their wrappers.");
            }
        }
        var firstConstructor = type.Members.OfType<ConstructorDeclarationSyntax>().FirstOrDefault();
        foreach (var member in type.Members)
        {
            var regions = Regions(member);
            if (member is EventDeclarationSyntax or EventFieldDeclarationSyntax && Access(member) is "public" or "protected" && !In(member, "公共事件定义"))
            {
                Issue(member, "event-region", Name(member), "Public events belong in 公共事件定义.");
            }
            if (member is FieldDeclarationSyntax field && !IsRegistration(field) &&
                !field.Declaration.Variables.All(variable => directBacking.Contains(variable.Identifier.ValueText)))
            {
                if (regions.Any(region => region is "公共属性定义" or "内部属性定义"))
                {
                    Issue(member, "runtime-field-region", Name(member), "Ordinary runtime fields do not belong in property contract regions.");
                }
                if (firstConstructor is not null && member.SpanStart > firstConstructor.SpanStart)
                {
                    Issue(member, "runtime-field-order", Name(member), "Runtime fields belong before constructors.");
                }
            }
            if (member is ConstructorDeclarationSyntax && type.Members.Any(other => other.SpanStart > member.SpanStart &&
                    (IsRegistration(other) || other is PropertyDeclarationSyntax && Regions(other).Any(region => region is "公共属性定义" or "内部属性定义"))))
            {
                Issue(member, "constructor-order", Name(member), "Constructors belong after property contracts.");
            }
            if (Access(member) == "internal" && member is MethodDeclarationSyntax or EventDeclarationSyntax or EventFieldDeclarationSyntax &&
                !regions.Any(region => region is "内部协作 API" or "内部事件定义"))
            {
                Issue(member, "internal-api-region", Name(member), "Internal collaboration methods/events require a dedicated region.");
            }
        }
        var firstPublic = type.Members.FirstOrDefault(member =>
            (IsRegistration(member) && Access(member) is "public" or "protected") ||
            (member is PropertyDeclarationSyntax && Access(member) == "public"));
        if (firstPublic is not null)
        {
            foreach (var before in type.Members.TakeWhile(member => member != firstPublic))
            {
                if (!In(before, "公共属性定义"))
                {
                    Issue(before, "contract-prelude", Name(before), "Public property contract must be the first member section.");
                }
            }
        }
        var explicitMembers = type.Members.Where(IsExplicitInterface).ToArray();
        foreach (var member in explicitMembers)
        {
            if (!Regions(member).Any(region => region.StartsWith("实现 ", StringComparison.Ordinal)))
            {
                Issue(member, "interface-region", Name(member), "Explicit interface implementation requires its interface region.");
            }
        }
        if (explicitMembers.Length > 0)
        {
            var firstInterface = explicitMembers.Min(member => member.SpanStart);
            foreach (var member in type.Members.OfType<MethodDeclarationSyntax>())
            {
                var insideInterface = Regions(member).Any(region => region.StartsWith("实现 ", StringComparison.Ordinal));
                if (Access(member) is "public" or "protected" && member.SpanStart > firstInterface && !insideInterface)
                {
                    Issue(member, "api-before-interface", Name(member), "Control public/protected API belongs before interface implementations.");
                }
                if (Access(member) == "private" && member.SpanStart < firstInterface)
                {
                    Issue(member, "private-after-interface", Name(member), "Private implementation belongs after interface contracts.");
                }
            }
        }
    }
    snapshots[file] = new
    {
        imports = root.Usings.Select(item => item.ToString()).ToArray(),
        types = typeSnapshots,
        otherDeclarations = root.DescendantNodes().OfType<EnumDeclarationSyntax>().Select(Fingerprint).ToArray()
    };
}
var settings = new JsonSerializerOptions { WriteIndented = true };
foreach (var name in new[] { "--inventory", "--snapshot" })
{
    if (options.TryGetValue(name, out var values))
    {
        File.WriteAllText(values.Single(), JsonSerializer.Serialize(name == "--inventory" ? (object)inventory : snapshots, settings));
    }
}
Console.WriteLine(JsonSerializer.Serialize(new { files = files.Length, controls, issues }, settings));
return issues.Count == 0 ? 0 : 1;

static bool IsRegistration(MemberDeclarationSyntax member)
{
    return member is FieldDeclarationSyntax field && field.Declaration.Type.DescendantNodesAndSelf().OfType<GenericNameSyntax>()
        .Any(name => name.Identifier.ValueText is "StyledProperty" or "DirectProperty" or "AttachedProperty");
}

static bool IsExplicitInterface(MemberDeclarationSyntax member)
{
    return member switch
    {
        MethodDeclarationSyntax value => value.ExplicitInterfaceSpecifier is not null,
        PropertyDeclarationSyntax value => value.ExplicitInterfaceSpecifier is not null,
        EventDeclarationSyntax value => value.ExplicitInterfaceSpecifier is not null,
        _ => false
    };
}

static string Access(MemberDeclarationSyntax member)
{
    var modifiers = member switch
    {
        BaseFieldDeclarationSyntax value => value.Modifiers,
        BasePropertyDeclarationSyntax value => value.Modifiers,
        BaseMethodDeclarationSyntax value => value.Modifiers,
        BaseTypeDeclarationSyntax value => value.Modifiers,
        _ => default
    };
    return modifiers.Any(SyntaxKind.PublicKeyword) ? "public" :
        modifiers.Any(SyntaxKind.ProtectedKeyword) ? "protected" :
        modifiers.Any(SyntaxKind.InternalKeyword) ? "internal" : "private";
}

static string Name(MemberDeclarationSyntax member)
{
    return member switch
    {
        BaseFieldDeclarationSyntax value => string.Join(",", value.Declaration.Variables.Select(variable => variable.Identifier.ValueText)),
        BaseTypeDeclarationSyntax value => value.Identifier.ValueText,
        PropertyDeclarationSyntax value => value.Identifier.ValueText,
        MethodDeclarationSyntax value => value.Identifier.ValueText,
        ConstructorDeclarationSyntax value => value.Identifier.ValueText,
        EventDeclarationSyntax value => value.Identifier.ValueText,
        _ => member.Kind().ToString()
    };
}

static string Fingerprint(SyntaxNode node)
{
    var tokens = string.Join("\0", node.DescendantTokens().Select(token => token.Kind() + ":" + token.Text));
    return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(tokens)));
}
