using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AtomUI.Generator;

internal sealed class ThemeBuildToken
{
    internal string Catalog = "";
    internal string OwnerId = "";
    internal string Name = "";
    internal string ValueType = "";
    internal string ValueAssembly = "";
    internal string KeyType = "";
    internal string KeyAssembly = "";
    internal string KeyMember = "";
    internal int Stage;
    internal int Contract;
    internal string Key => Catalog + "\u001f" + OwnerId + "\u001f" + Name;
    internal string[] Fields => new[] { Catalog, OwnerId, Name, ValueType, ValueAssembly, KeyType, KeyAssembly, KeyMember, Stage.ToString(CultureInfo.InvariantCulture), Contract.ToString(CultureInfo.InvariantCulture) };
}
internal sealed class ThemeBuildOwner
{
    internal string Catalog = "";
    internal string Id = "";
    internal string OwnerAssembly = "";
    internal string OwnerType = "";
    internal string IdentityProvider = "";
    internal string Digest = "";
    internal string Key => Catalog + "\u001f" + Id;
    internal string[] Fields => new[] { Catalog, Id, OwnerAssembly, OwnerType, IdentityProvider, Digest };
}
internal sealed class ThemeBuildContracts
{
    private const string Prefix = "AtomUI.Theme.Definitions.Compiled.";
    internal readonly Dictionary<string, ThemeBuildToken> Tokens = new(StringComparer.Ordinal);
    internal readonly Dictionary<string, ThemeBuildOwner> Owners = new(StringComparer.Ordinal);
    internal readonly List<ThemeBuildToken> LocalTokens = new();
    internal readonly List<ThemeBuildOwner> LocalOwners = new();
    internal static bool HasAbi(Compilation compilation) => compilation.GetTypeByMetadataName(Prefix + "ThemeBuildTokenAttribute") is not null &&
        compilation.GetTypeByMetadataName("AtomUI.Theme.Schema.TokenDescriptor")?.GetMembers("CreateGenerated").OfType<IMethodSymbol>().Any(m => m.Parameters.Length == 7) == true;
    internal static ThemeBuildContracts Build(Compilation compilation, string catalog, IEnumerable<SchemaTokenInfo> globalTokens,
        IEnumerable<ControlThemeInfo> controls, GenerationOutput output)
    {
        var result = new ThemeBuildContracts();
        foreach (var assembly in compilation.SourceModule.ReferencedAssemblySymbols)
        {
            foreach (var attribute in assembly.GetAttributes())
            {
                var name = attribute.AttributeClass?.ToDisplayString();
                if (name != Prefix + "ThemeBuildOwnerAttribute" && name != Prefix + "ThemeBuildTokenAttribute") { continue; }
                try
                {
                    var values = attribute.ConstructorArguments;
                    if (values.Length == 0 || values[0].Value is not int version || version != 1) { throw new InvalidOperationException("Unsupported Token build metadata version."); }
                    string S(int index) => values[index].Value as string ?? throw new InvalidOperationException("Invalid string metadata field.");
                    int N(int index) => values[index].Value is int value ? value : throw new InvalidOperationException("Invalid integer metadata field.");
                    if (name == Prefix + "ThemeBuildOwnerAttribute")
                    {
                        if (values.Length != 7) { throw new InvalidOperationException("Invalid owner metadata arity."); }
                        var owner = new ThemeBuildOwner { Catalog = S(1), Id = S(2), OwnerAssembly = S(3), OwnerType = S(4), IdentityProvider = S(5), Digest = S(6) };
                        if (string.IsNullOrWhiteSpace(owner.Catalog) || string.IsNullOrWhiteSpace(owner.Id) || owner.Digest.Length != 64) { throw new InvalidOperationException("Invalid owner identity/digest."); }
                        var type = ResolveOwner(compilation, owner.OwnerType, owner.OwnerAssembly);
                        if (type is null) { throw new InvalidOperationException("Owner definition assembly/type is unavailable: " + owner.OwnerType); }
                        var provider = assembly.GetTypeByMetadataName(owner.IdentityProvider.Replace("global::", ""));
                        var identity = provider?.GetMembers("Identity").SingleOrDefault();
                        var identityType = identity switch
                        {
                            IPropertySymbol property when property.IsStatic && property.GetMethod?.DeclaredAccessibility == Accessibility.Public => property.Type,
                            IFieldSymbol field when field.IsStatic && field.IsReadOnly => field.Type,
                            _ => null
                        };
                        if (provider is null || !IsPublic(provider) || identity?.DeclaredAccessibility != Accessibility.Public || identityType is null ||
                            !SymbolEqualityComparer.Default.Equals(identityType, compilation.GetTypeByMetadataName("AtomUI.Theme.Schema.ControlTokenIdentity")))
                        { throw new InvalidOperationException("Invalid public identity provider: " + owner.IdentityProvider); }
                        result.Add(owner);
                    }
                    else
                    {
                        if (values.Length != 11) { throw new InvalidOperationException("Invalid Token metadata arity."); }
                        var token = new ThemeBuildToken { Catalog = S(1), OwnerId = S(2), Name = S(3), ValueType = S(4), ValueAssembly = S(5), KeyType = S(6), KeyAssembly = S(7), KeyMember = S(8), Stage = N(9), Contract = N(10) };
                        if (string.IsNullOrWhiteSpace(token.Name) || token.Name != token.KeyMember || token.Stage < 0 || token.Stage > 3 || token.Contract < 0 || token.Contract > 1 || (token.Catalog.Length == 0) != (token.OwnerId.Length == 0))
                        { throw new InvalidOperationException("Invalid Token contract."); }
                        var valueType = Resolve(compilation, token.ValueType, token.ValueAssembly);
                        var keyType = Resolve(compilation, token.KeyType, token.KeyAssembly);
                        if (valueType is null || keyType is null || keyType.TypeKind != TypeKind.Enum || (token.Contract == 1 && !IsPublic(valueType)) || !IsPublic(keyType) || keyType.GetMembers(token.KeyMember).OfType<IFieldSymbol>().SingleOrDefault()?.HasConstantValue != true)
                        { throw new InvalidOperationException("Unavailable value/resource-key contract: " + token.Name); }
                        if (token.Contract == 1 && !ThemeLiteralCodec.Supports(token.ValueType)) { throw new InvalidOperationException("Unknown v1 literal capability: " + token.ValueType); }
                        result.Add(token);
                    }
                }
                catch (Exception error) { CompiledThemePackageWriter.Error(output, null, 0, 0, "Invalid Token build metadata in '" + assembly.Identity + "': " + error.Message); }
            }
        }
        foreach (var token in globalTokens.OrderBy(t => t.Name, StringComparer.Ordinal))
        {
            result.AddLocal(CreateToken(compilation, token, "", "", "global::AtomUI.Theme.Resources.SharedTokenKind"));
        }
        foreach (var control in controls.Where(c => c.HasDescriptor).OrderBy(c => c.ControlMetadataName, StringComparer.Ordinal))
        {
            var own = control.OwnSchemaTokens.OrderBy(t => t.Name, StringComparer.Ordinal).Select(t => CreateToken(compilation, t, catalog, control.ControlName,
                "global::" + control.ControlNamespace + ".DesignTokens." + control.TokenKindType)).ToArray();
            foreach (var token in own) { result.AddLocal(token); }
            var ownerType = compilation.GetTypeByMetadataName(control.ControlMetadataName)!;
            var owner = new ThemeBuildOwner { Catalog = catalog, Id = control.ControlName,
                OwnerAssembly = ownerType.ContainingAssembly.Identity.ToString(), OwnerType = control.ControlMetadataName,
                IdentityProvider = "global::" + control.ControlNamespace + ".DesignTokens." + control.TokensType,
                Digest = Digest(new[] { catalog, control.ControlName, ownerType.ContainingAssembly.Identity.ToString(), control.ControlMetadataName }.Concat(own.SelectMany(t => t.Fields))) };
            result.Add(owner); result.LocalOwners.Add(owner);
        }
        foreach (var token in result.Tokens.Values.Where(t => t.Catalog.Length != 0))
        {
            if (!result.Owners.ContainsKey(token.Catalog + "\u001f" + token.OwnerId)) { CompiledThemePackageWriter.Error(output, null, 0, 0, "Token metadata has no owner: " + token.Key); }
        }
        return result;
    }
    private static ThemeBuildToken CreateToken(Compilation compilation, SchemaTokenInfo info, string catalog, string owner, string key)
    {
        var type = ResolvePropertyType(compilation, info);
        return new ThemeBuildToken { Catalog = catalog, OwnerId = owner, Name = info.Name, ValueType = info.ValueType,
            ValueAssembly = (type is INamedTypeSymbol named && named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T ? named.TypeArguments[0].ContainingAssembly : type?.ContainingAssembly)?.Identity.ToString() ?? "",
            KeyType = key, KeyAssembly = compilation.Assembly.Identity.ToString(), KeyMember = info.Name, Stage = (int)info.Stage,
            Contract = ThemeLiteralCodec.Supports(info.ValueType) ? 1 : 0 };
    }
    internal static ITypeSymbol? ResolvePropertyType(Compilation compilation, SchemaTokenInfo info)
    {
        for (var type = compilation.GetTypeByMetadataName(info.DeclaringType.Replace("global::", "")); type is not null; type = type.BaseType)
        {
            var property = type.GetMembers(info.Name).OfType<IPropertySymbol>().FirstOrDefault();
            if (property is not null) { return property.Type; }
        }
        return null;
    }
    internal static INamedTypeSymbol? Resolve(Compilation compilation, string expression, string assemblyIdentity)
    {
        var name = ThemeLiteralCodec.NormalizeType(expression);
        if (name.StartsWith("System.Nullable<", StringComparison.Ordinal)) { name = name.Substring(16, name.Length - 17); }
        var assemblies = compilation.SourceModule.ReferencedAssemblySymbols.Concat(new[] { compilation.Assembly });
        var resolved = assemblies.Where(assembly => MatchesAssembly(assembly.Identity, assemblyIdentity))
            .Select(assembly => assembly.GetTypeByMetadataName(name)).FirstOrDefault(type => type is not null);
        if (resolved is not null) { return resolved; }
        // Runtime compilations may expose forwarded BCL values through the core library,
        // whereas reference-assembly metadata names System.Runtime. Keep the signed facade
        // boundary, but never require its assembly version to equal the current target.
        if (!AssemblyIdentity.TryParseDisplayName(assemblyIdentity, out var declared) || declared.Name != "System.Runtime" ||
            !declared.PublicKeyToken.SequenceEqual(new byte[] { 0xb0, 0x3f, 0x5f, 0x7f, 0x11, 0xd5, 0x0a, 0x3a }))
        { return null; }
        var frameworkType = name switch
        {
            "System.String" => compilation.GetSpecialType(SpecialType.System_String),
            "System.Int32" => compilation.GetSpecialType(SpecialType.System_Int32),
            "System.Double" => compilation.GetSpecialType(SpecialType.System_Double),
            "System.Single" => compilation.GetSpecialType(SpecialType.System_Single),
            "System.Boolean" => compilation.GetSpecialType(SpecialType.System_Boolean),
            "System.TimeSpan" => compilation.GetTypeByMetadataName("System.TimeSpan"),
            _ => null
        };
        return frameworkType?.TypeKind == TypeKind.Error ? null : frameworkType;
    }
    internal static INamedTypeSymbol? ResolveOwner(Compilation compilation, string expression, string assemblyIdentity) =>
        Resolve(compilation, expression, assemblyIdentity);

    private static bool MatchesAssembly(AssemblyIdentity current, string declaredIdentity)
    {
        return AssemblyIdentity.TryParseDisplayName(declaredIdentity, out var declared) &&
               current.Name == declared.Name && current.CultureName == declared.CultureName &&
               current.PublicKeyToken.SequenceEqual(declared.PublicKeyToken);
    }
    private static bool IsPublic(ITypeSymbol type)
    {
        for (var current = type; current is not null; current = current.ContainingType) { if (current.DeclaredAccessibility != Accessibility.Public) { return false; } }
        return true;
    }
    private void Add(ThemeBuildOwner owner)
    {
        if (Owners.ContainsKey(owner.Key)) { throw new InvalidOperationException("Duplicate owner build contract: " + owner.Key); }
        Owners.Add(owner.Key, owner);
    }
    private void Add(ThemeBuildToken token)
    {
        if (Tokens.ContainsKey(token.Key) || Tokens.Values.Any(t => t.Catalog == token.Catalog && t.OwnerId == token.OwnerId && t.KeyType == token.KeyType && t.KeyAssembly == token.KeyAssembly && t.KeyMember == token.KeyMember))
        { throw new InvalidOperationException("Duplicate Token name/resource-key contract: " + token.Key); }
        Tokens.Add(token.Key, token);
    }
    private void AddLocal(ThemeBuildToken token) { Add(token); LocalTokens.Add(token); }
    internal void Write(GenerationOutput output)
    {
        var source = new StringBuilder("// <auto-generated />\n");
        foreach (var owner in LocalOwners) { source.Append("[assembly: global::").Append(Prefix).Append("ThemeBuildOwnerAttribute(1, ").Append(string.Join(", ", owner.Fields.Select(Literal))).AppendLine(")]"); }
        foreach (var token in LocalTokens)
        {
            source.Append("[assembly: global::").Append(Prefix).Append("ThemeBuildTokenAttribute(1, ").Append(string.Join(", ", token.Fields.Take(8).Select(Literal)))
                .Append(", ").Append(token.Stage).Append(", ").Append(token.Contract).AppendLine(")]");
        }
        output.AddSource("GeneratedThemeBuildContracts.g.cs", GeneratedSourceText.From(source.ToString()));
    }
    internal static string Literal(string text) => SymbolDisplay.FormatLiteral(text, true);
    internal static string Digest(IEnumerable<string> fields)
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, new UTF8Encoding(false), true)) { foreach (var field in fields) { writer.Write(field); } }
        using var sha = SHA256.Create();
        return BitConverter.ToString(sha.ComputeHash(stream.ToArray())).Replace("-", "");
    }
}
