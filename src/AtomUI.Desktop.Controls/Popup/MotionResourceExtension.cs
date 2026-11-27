using AtomUI.Animations;
using AtomUI.Theme;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.XamlIl.Runtime;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Provides a component's scoped motion as the value of a normal AXAML Setter.
/// </summary>
public sealed class MotionResourceExtension(MotionSlot slot) : MarkupExtension
{
    /// <summary>
    /// Component motion slot resolved from the active scope or design language.
    /// </summary>
    public MotionSlot Slot { get; } = slot;

    /// <inheritdoc />
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var priority = serviceProvider.GetService(typeof(IAvaloniaXamlIlControlTemplateProvider)) is null
            ? BindingPriority.LocalValue : BindingPriority.Template;
        var accessorPath = new CompiledBindingPathBuilder().Self()
            .Property(s_accessor, (source, _) => new Accessor(source, Slot)).Build();
        return new CompiledBinding(accessorPath) { Mode = BindingMode.OneWay, Priority = priority };
    }

    private static readonly ClrPropertyInfo s_accessor = new("MotionResource", null, null, typeof(AbstractMotion));

    private sealed class Accessor(WeakReference<object?> source, MotionSlot slot) : IPropertyAccessor
    {
        private readonly object _key = slot;
        private StyledElement? _target;
        private Action<object?>? _listener;
        private object? _value = AvaloniaProperty.UnsetValue;
        private AbstractMotion? _languageValue;
        private (DesignLanguageDescriptor Language, DesignVariant Variant, ThemeSchemaRegistry Registry)? _selection;
        private bool _resolved;

        public Type PropertyType => typeof(AbstractMotion);
        public object? Value => _value;
        public bool SetValue(object? value, BindingPriority priority) => false;

        public void Subscribe(Action<object?> listener)
        {
            _listener = listener;
            if (source.TryGetTarget(out var value) && value is StyledElement target)
            {
                _target = target;
                target.ResourcesChanged += OnResourcesChanged;
                target.ActualThemeVariantChanged += OnThemeVariantChanged;
                _value = Resolve(target);
            }
            listener(_value);
        }

        private object? Resolve(StyledElement target)
        {
            if (ScopedMotionSelection.TryResolveResource(target, _key, out var value))
            {
                return value;
            }
            var snapshot = ThemeScope.ResolveContext(target)?.Snapshot;
            var selection = snapshot is not null ? (snapshot.Language, snapshot.Variant, snapshot.Registry)
                : ((DesignLanguageDescriptor, DesignVariant, ThemeSchemaRegistry)?)null;
            if (!_resolved || _selection != selection)
            {
                _languageValue = ScopedMotionSelection.TryResolve(snapshot, slot, out var motion) ? motion :
                    PopupMotionDefaults.CreateForSlot(slot);
                _selection = selection;
                _resolved = true;
            }
            return _languageValue;
        }

        private void OnResourcesChanged(object? sender, ResourcesChangedEventArgs args) => Refresh();
        private void OnThemeVariantChanged(object? sender, EventArgs args) => Refresh();

        private void Refresh()
        {
            if (_target is not { } target)
            {
                return;
            }
            var value = Resolve(target);
            if (!ReferenceEquals(value, _value))
            {
                _value = value;
                _listener?.Invoke(value);
            }
        }

        public void Unsubscribe()
        {
            if (_target is { } target)
            {
                target.ResourcesChanged -= OnResourcesChanged;
                target.ActualThemeVariantChanged -= OnThemeVariantChanged;
            }
            _target = null;
            _listener = null;
            _value = AvaloniaProperty.UnsetValue;
            _languageValue = null;
            _selection = null;
            _resolved = false;
        }

        public void Dispose() => Unsubscribe();
    }
}
