using System.Globalization;
using System.Runtime.CompilerServices;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;
using Avalonia.Metadata;

namespace AtomUI.Animations;

// Bind at the Control's style Setter, where native resources have
// an owner. TransitionBase has no resource host and must not share mutable duration.
internal sealed class TokenTransitionsExtension : MarkupExtension
{
    #region 公共属性定义

    public BindingBase? Duration { get; set; }

    public SharedTokenKind? DurationKind { get; set; }

    // Preserve an existing inert default while allowing directly configured duration
    // inputs to opt in. Presence uses the existing normalized Token resource contract.
    public TimeSpan? UnconfiguredDuration { get; set; }

    [Content]
    public IList<TransitionBase> Children { get; } = new List<TransitionBase>();

    #endregion

    #region 内部属性定义

    internal static readonly AttachedProperty<SharedTokenKind?> DurationInputKindProperty =
        AvaloniaProperty.RegisterAttached<TokenTransitionsExtension, TransitionBase, SharedTokenKind?>("DurationInputKind");

    #endregion

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var prototypes = Children.ToArray();
        var factories = prototypes.Select(CreateFactory).ToArray();
        var bindings = new List<BindingBase>();
        var slots = new int[prototypes.Length];
        if (Duration is { } explicitDuration)
        {
            bindings.Add(explicitDuration);
            Array.Fill(slots, 1);
        }
        else
        {
            var kinds = new List<SharedTokenKind>();
            for (var i = 0; i < prototypes.Length; i++)
            {
                var kind = DurationKind ?? prototypes[i].GetValue(DurationInputKindProperty);
                if (kind is null)
                {
                    slots[i] = 0; // A static duration remains authored content.
                    continue;
                }
                var slot = kinds.IndexOf(kind.Value);
                if (slot < 0)
                {
                    slot = kinds.Count;
                    kinds.Add(kind.Value);
                    bindings.Add(OwnerTokenResourceExtension.CreateBinding(kind.Value, UnconfiguredDuration.HasValue));
                }
                slots[i] = slot + 1;
            }
        }
        var result = new MultiBinding
        {
            Mode = BindingMode.OneWay,
            Converter = new OwnerConverter(factories, slots, bindings.Count + 1, UnconfiguredDuration)
        };
        result.Bindings.Add(new CompiledBindingExtension(new CompiledBindingPathBuilder().Self().Build()));
        foreach (var binding in bindings)
        {
            result.Bindings.Add(binding);
        }
        return result;
    }

    private static Func<TransitionBase> CreateFactory(TransitionBase prototype)
    {
        var type = prototype.GetType();
        Func<TransitionBase> create;
        if (type == typeof(SolidColorBrushTransition))
        {
            create = static () => new SolidColorBrushTransition();
        }
        else if (type == typeof(BoxShadowsTransition))
        {
            create = static () => new BoxShadowsTransition();
        }
        else if (type == typeof(DoubleTransition))
        {
            create = static () => new DoubleTransition();
        }
        else if (type == typeof(TransformOperationsTransition))
        {
            create = static () => new TransformOperationsTransition();
        }
        else if (type == typeof(PointTransition))
        {
            create = static () => new PointTransition();
        }
        else if (type == typeof(SizeTransition))
        {
            create = static () => new SizeTransition();
        }
        else if (type == typeof(ThicknessTransition))
        {
            create = static () => new ThicknessTransition();
        }
        else if (type == typeof(RectTransition))
        {
            create = static () => new RectTransition();
        }
        else
        {
            throw new NotSupportedException($"No AOT transition factory is defined for '{type}'.");
        }
        var property = prototype.Property ?? throw new InvalidOperationException("Transition Property is required.");
        var delay = prototype.Delay;
        var duration = prototype.Duration;
        var easing = CopyEasing(prototype.Easing);
        return () =>
        {
            var transition = create();
            transition.Property = property;
            transition.Delay = delay;
            transition.Duration = duration;
            transition.Easing = CopyEasing(easing);
            return transition;
        };
    }

    private static Easing CopyEasing(Easing easing)
    {
        if (TokenValueOperations.TryCopy(easing, out var copy) && copy is Easing result)
        {
            return result;
        }
        throw new NotSupportedException($"No AOT easing copy is defined for '{easing.GetType()}'.");
    }

    private sealed class OwnerConverter(Func<TransitionBase>[] factories, int[] slots, int valueCount,
                                       TimeSpan? unconfiguredDuration) : IMultiValueConverter
    {
        // Weak owner keys keep the style definition from retaining discarded controls.
        // This caches only per-owner native objects; the binding engine owns priority.
        private readonly ConditionalWeakTable<Control, Transitions> _owners = new();

        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count != valueCount || values[0] is not Control owner ||
                slots.Any(slot => slot != 0 && values[slot] is not TimeSpan &&
                                  !(unconfiguredDuration.HasValue && ReferenceEquals(values[slot], AvaloniaProperty.UnsetValue))))
            {
                return AvaloniaProperty.UnsetValue;
            }
            var transitions = _owners.GetValue(owner, _ => CreateTransitions());
            for (var i = 0; i < transitions.Count; i++)
            {
                if (slots[i] != 0)
                {
                    ((TransitionBase)transitions[i]).Duration = values[slots[i]] is TimeSpan duration
                        ? duration : unconfiguredDuration.GetValueOrDefault();
                }
            }
            return transitions;
        }

        private Transitions CreateTransitions()
        {
            var result = new Transitions();
            foreach (var factory in factories)
            {
                result.Add(factory());
            }
            return result;
        }
    }
}
