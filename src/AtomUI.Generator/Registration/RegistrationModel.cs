using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AtomUI.Generator;

// Bound registration facts contain values only. Roslyn symbols and XML nodes never escape binding.
// AssemblyQualifiedName below is the compiler-symbol identity for opaque keys/generated names;
// only Core materialization reads the canonical runtime AQN used by the final schema fingerprint.
internal sealed record RegistrationType(string Name, string MetadataName, string AssemblyQualifiedName)
{
    internal string AssemblyIdentity => AssemblyQualifiedName.Substring(MetadataName.Length + 2);

    internal static RegistrationType From(INamedTypeSymbol symbol) => new(
        symbol.ToDisplayString(GeneratorSymbolDisplay.FullyQualifiedType), MetadataNameOf(symbol),
        MetadataNameOf(symbol) + ", " + symbol.ContainingAssembly.Identity.GetDisplayName());

    internal static string MetadataNameOf(INamedTypeSymbol symbol)
    {
        var names = new Stack<string>();
        for (var current = symbol; current is not null; current = current.ContainingType)
        {
            names.Push(current.MetadataName);
        }

        return (symbol.ContainingNamespace.IsGlobalNamespace ? "" : symbol.ContainingNamespace.ToDisplayString() + ".") + string.Join("+", names);
    }
}

internal sealed record RegistrationOwner(RegistrationType Type, string Catalog, string Id);
internal sealed record RegistrationExport(RegistrationType Target, RegistrationType? KeyType, string? Key);
internal sealed record RegistrationBinding(RegistrationOwner Owner, string Property, RegistrationType Target);
internal sealed record RegistrationAsset(string Id, string Uri, string Wrapper, int Order,
    ValueArray<RegistrationExport> Exports, ValueArray<RegistrationOwner> Owners,
    ValueArray<RegistrationBinding> Bindings, ulong CompiledContractFingerprint, ValueArray<string> Guards, ValueArray<string> ExplicitIncludes, ValueArray<string> ExternalDynamicKeys)
{
    internal RegistrationType? DeferredThemeTarget { get; init; }
}
internal sealed record RegistrationControl(RegistrationType Type, string? TokenFactory, string? SemanticFactory,
    ValueArray<string> Triggers, ValueArray<string> Guards, ValueArray<string> Assets,
    ValueArray<RegistrationTrigger> ConditionalTriggers = default);
internal sealed record RegistrationTrigger(RegistrationType Type, string Kind);
internal sealed record RegistrationPackage(string Id, string AssemblyIdentity, string Namespace,
    ValueArray<RegistrationControl> Controls, ValueArray<RegistrationAsset> Assets, ValueArray<string> CompiledGlobalTokenNames,
    RegistrationType? RegistrationBuilderType = null, RegistrationType? VoidType = null);

internal readonly struct ValueArray<T> : IEquatable<ValueArray<T>>, IEnumerable<T>
{
    private readonly ImmutableArray<T> _items;
    internal ValueArray(IEnumerable<T> items) => _items = items.ToImmutableArray();
    internal int Count => _items.IsDefault ? 0 : _items.Length;
    internal T this[int index] => _items[index];
    public bool Equals(ValueArray<T> other) => this.SequenceEqual(other);
    public override bool Equals(object? obj) => obj is ValueArray<T> other && Equals(other);
    public override int GetHashCode()
    {
        var hash = 17;
        foreach (var item in this)
        {
            hash = unchecked(hash * 31 + (item is null ? 0 : item.GetHashCode()));
        }

        return hash;
    }
    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)(_items.IsDefault ? ImmutableArray<T>.Empty : _items)).GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}

internal static class RegistrationNames
{
    internal static string Literal(string value) => SymbolDisplay.FormatLiteral(value, true);
    internal static string Hash(string value)
    {
        var hash = 14695981039346656037UL;
        foreach (var ch in value) { hash ^= ch; hash = unchecked(hash * 1099511628211UL); }
        return hash.ToString("X16", System.Globalization.CultureInfo.InvariantCulture);
    }
    internal static string TypeMapKey(
        string groupIdentity,
        string controlAssemblyIdentity,
        string triggerIdentity,
        Func<string, string>? hash = null) =>
        "v1:" + (hash ?? Hash)(groupIdentity + "\n" + controlAssemblyIdentity + "\n" + triggerIdentity);
    internal static string Proxy(RegistrationControl control) => "ControlFragment_" + Hash(control.Type.AssemblyQualifiedName);
    internal static string AssetFactory(string id) => "CreateResource_" + Hash(id);
    internal static string AssetDescriptor(string id) => "CreateAsset_" + Hash(id);
}
