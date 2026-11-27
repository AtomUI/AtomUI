using System.ComponentModel;
using AtomUI.Theme;
using AtomUI.Theme.DesignTokens;
using Avalonia.Controls;

namespace AtomUI.Controls;

/// <summary>
/// Base for immutable policies that decide bounded control actions from read-only input.
/// </summary>
public abstract class ControlBehavior<TControl> where TControl : Control
{
    /// <summary>
    /// Gets the optional policy selected by the control's committed design language.
    /// </summary>
    public static ControlBehavior<TControl>? GetScoped(TControl control)
    {
        ArgumentNullException.ThrowIfNull(control);
        var snapshot = ThemeScope.ResolveContext(control)?.Snapshot;
        return snapshot?.Registry.FindControlBehavior(typeof(TControl), snapshot.Language.LanguageTokenType)?.Value
            as ControlBehavior<TControl>;
    }
}

/// <summary>
/// Declares a policy for the language token type. The policy's ControlBehavior base identifies its control.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ControlBehaviorContributionAttribute(Type languageTokenType) : Attribute
{
    /// <summary>
    /// Language token type that selects this control behavior contribution.
    /// </summary>
    public Type LanguageTokenType { get; } = languageTokenType;
}

/// <summary>
/// Holds a statically registered policy without creating it until selected.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ControlBehaviorDescriptor
{
    private readonly Lazy<object> _value;

    private ControlBehaviorDescriptor(Type controlType, Type languageTokenType, Type behaviorType, Func<object> create)
    {
        ControlType = controlType;
        LanguageTokenType = languageTokenType;
        BehaviorType = behaviorType;
        _value = new Lazy<object>(create);
    }

    internal Type ControlType { get; }
    internal Type LanguageTokenType { get; }
    internal Type BehaviorType { get; }
    internal object Value => _value.Value;

    /// <summary>
    /// Creates a lazy behavior descriptor binding <typeparamref name="TControl" /> and
    /// <typeparamref name="TLanguageToken" /> to <typeparamref name="TBehavior" />.
    /// </summary>
    public static ControlBehaviorDescriptor Create<TControl, TLanguageToken, TBehavior>()
        where TControl : Control
        where TLanguageToken : LanguageDesignToken
        where TBehavior : ControlBehavior<TControl>, new() =>
        new(typeof(TControl), typeof(TLanguageToken), typeof(TBehavior), static () => new TBehavior());
}
