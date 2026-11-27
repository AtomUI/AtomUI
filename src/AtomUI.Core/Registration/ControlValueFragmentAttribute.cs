using System.ComponentModel;
using System.Reflection;
using AtomUI.Theme.Schema;

namespace AtomUI.Registration;

/// <summary>
/// Declares a generated value map for the application's TypeMap assembly targets.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class ControlValueMapAttribute(Type group) : Attribute
{
    /// <summary>
    /// TypeMap group type containing the generated value map entries.
    /// </summary>
    public Type Group { get; } = group;
}

/// <summary>
/// Provides a value package or rule selected by generated TypeMap metadata.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public abstract class ControlValueFragmentAttribute<T> : Attribute where T : class
{
    /// <summary>
    /// Registration payload carried by this fragment attribute.
    /// </summary>
    public abstract T Value { get; }

    /// <summary>
    /// Resolves the registration payload mapped under <paramref name="key" />, or <see langword="null" /> when absent.
    /// </summary>
    public static T? Resolve(IReadOnlyDictionary<string, Type> map, string key)
    {
        if (!map.TryGetValue(key, out var proxy))
        {
            return null;
        }
        var fragment = proxy.GetCustomAttribute<ControlValueFragmentAttribute<T>>(inherit: false);
        if (fragment is null || fragment.GetType() != proxy)
        {
            throw new InvalidOperationException($"Invalid control value registration proxy '{proxy}'.");
        }
        return fragment.Value;
    }
}

/// <summary>
/// Runtime helper for resolving generated control token rule descriptors from TypeMap entries.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ControlValueRegistration
{
    /// <summary>
    /// Resolves every registered control token rule descriptor present in <paramref name="map" /> for
    /// <paramref name="keys" />.
    /// </summary>
    public static IReadOnlyList<ControlTokenRuleDescriptor> ResolveRules(
        IReadOnlyDictionary<string, Type> map, IReadOnlyList<string> keys)
    {
        var rules = new List<ControlTokenRuleDescriptor>();
        foreach (var key in keys)
        {
            if (ControlValueFragmentAttribute<ControlTokenRuleDescriptor>.Resolve(map, key) is { } rule)
            {
                rules.Add(rule);
            }
        }
        return rules;
    }
}
