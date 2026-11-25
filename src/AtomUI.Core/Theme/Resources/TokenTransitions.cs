using Avalonia;
using Avalonia.Animation;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.XamlIl.Runtime;
using Avalonia.Metadata;

namespace AtomUI.Theme.Resources;

/// <summary>
/// Supplies a <see cref="Transitions"/> value whose transition durations are read from the Design Tokens of the
/// control that receives it, so they follow theme switches, <c>EnableMotion</c>, per-control overrides and local
/// <see cref="ThemeConfigProvider"/> scopes like every other Token read.
/// </summary>
/// <remarks>
/// <para>
/// AXAML usage, in a Setter or as a property element of a template element:
/// <code>
/// &lt;Setter Property="Transitions"&gt;
///     &lt;atom:TokenTransitions&gt;
///         &lt;Transitions&gt;
///             &lt;atom:SolidColorBrushTransition Property="Background" atom:TokenTransitions.Duration="MotionDurationMid" /&gt;
///         &lt;/Transitions&gt;
///     &lt;/atom:TokenTransitions&gt;
/// &lt;/Setter&gt;
/// </code>
/// A transition that carries <see cref="DurationProperty"/> gets its <see cref="TransitionBase.Duration"/> from that
/// global Token; other transitions keep the duration written in the markup.
/// </para>
/// <para>
/// A plain <c>&lt;Transitions&gt;</c> Setter value is one instance shared by every target of the style, and
/// <see cref="TransitionBase.Duration"/> is read from the transition object when the transition starts, so a shared
/// instance cannot carry per-control durations. The content is therefore deferred, like a template, and the extension
/// returns a binding that is instantiated per target: it resolves the target's Effective Global value of each declared
/// Token (through the inherited Token owner, exactly like <see cref="OwnerTokenResourceExtension"/>) and publishes a
/// <see cref="Transitions"/> built for those durations. Targets that resolve the same durations share one built
/// instance, so the default theme allocates the transitions once per Setter rather than once per control.
/// </para>
/// </remarks>
public sealed class TokenTransitions : MarkupExtension
{
    /// <summary>
    /// The global Design Token (one of the <c>MotionDuration*</c> Tokens) that supplies a transition's duration.
    /// </summary>
    public static readonly AttachedProperty<SharedTokenKind?> DurationProperty =
        AvaloniaProperty.RegisterAttached<TokenTransitions, TransitionBase, SharedTokenKind?>("Duration");

    private TokenTransitionsFactory? _factory;

    /// <summary>
    /// Gets the global Design Token that supplies the duration of <paramref name="transition"/>.
    /// </summary>
    public static SharedTokenKind? GetDuration(TransitionBase transition)
    {
        ArgumentNullException.ThrowIfNull(transition);
        return transition.GetValue(DurationProperty);
    }

    /// <summary>
    /// Sets the global Design Token that supplies the duration of <paramref name="transition"/>.
    /// </summary>
    public static void SetDuration(TransitionBase transition, SharedTokenKind? kind)
    {
        ArgumentNullException.ThrowIfNull(transition);
        transition.SetValue(DurationProperty, kind);
    }

    /// <summary>
    /// The deferred <see cref="Transitions"/> content, built once for each distinct set of resolved durations.
    /// </summary>
    [Content]
    [TemplateContent(TemplateResultType = typeof(Transitions))]
    public object? Content { get; set; }

    /// <inheritdoc />
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (Content is null)
        {
            throw new InvalidOperationException("TokenTransitions requires a Transitions content.");
        }

        // Same template-context check as DynamicResourceExtension: values inside a ControlTemplate use
        // Template priority so triggered styles can still override them.
        var priority = serviceProvider.GetService(typeof(IAvaloniaXamlIlControlTemplateProvider)) is not null
            ? BindingPriority.Template
            : BindingPriority.LocalValue;
        _factory ??= new TokenTransitionsFactory(Content);
        return TransitionDurationBinding.Create(_factory, priority);
    }
}
