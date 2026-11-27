using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml.XamlIl.Runtime;
using Avalonia.Styling;
using AtomUI.Theme.Compilation;

namespace AtomUI.Theme;

internal partial class ThemeManager
{
    private const string LanguageThemeKeyPrefix = "AtomUI.Theme.";

    private static readonly ClrPropertyInfo s_managedScopedThemeProperty =
        new("ManagedScopedControlTheme", null, null, typeof(ControlTheme));

    private Styles? _managedScopedThemeStyles;
    private HashSet<Type>? _managedScopedThemeTypes;

    internal void InitializeScopedControlThemes(Application application)
    {
        ScopedControlTheme.Initialize();
        if (ScopedControlTheme.IsEnabled)
        {
            return;
        }

        _managedScopedThemeStyles = new Styles();
        _managedScopedThemeTypes = [];
        application.Styles.Insert(0, _managedScopedThemeStyles);
        SyncManagedScopedThemeStyles();
    }

    internal void DisposeScopedControlThemes(Application? application)
    {
        var styles = _managedScopedThemeStyles;
        _managedScopedThemeStyles = null;
        _managedScopedThemeTypes = null;
        if (styles is not null && application is not null)
        {
            DisposeBoundary(() => application.Styles.Remove(styles));
        }
    }

    internal void SyncManagedScopedThemeStyles()
    {
        if (ScopedControlTheme.IsEnabled ||
            _managedScopedThemeStyles is not { } styles ||
            _managedScopedThemeTypes is not { } registeredTypes)
        {
            return;
        }

        var exportedTargetNames = new HashSet<string>(StringComparer.Ordinal);
        if (_startupResourceProvider is { } startup)
        {
            CollectExportedLanguageTargetNames(startup.KnownNames, exportedTargetNames);
        }
        if (_staticResourceProvider is { } staticResources)
        {
            CollectExportedLanguageTargetNames(staticResources.KnownNames, exportedTargetNames);
        }
        if (exportedTargetNames.Count == 0)
        {
            return;
        }

        var candidateTypes = new HashSet<Type>();
        var registry = CurrentSnapshot?.Registry ?? _startupRegistry;
        if (registry is not null)
        {
            foreach (var control in registry.Controls)
            {
                if (control.ControlType.FullName is { } fullName && exportedTargetNames.Contains(fullName))
                {
                    candidateTypes.Add(control.ControlType);
                }
            }
        }
        foreach (var semantic in SemanticParts.Controls)
        {
            if (semantic.ControlType.FullName is { } fullName && exportedTargetNames.Contains(fullName))
            {
                candidateTypes.Add(semantic.ControlType);
            }
        }
        if (_staticResourceProvider is { } manualProvider)
        {
            foreach (var knownType in manualProvider.KnownTypes)
            {
                if (knownType.FullName is { } fullName && exportedTargetNames.Contains(fullName))
                {
                    candidateTypes.Add(knownType);
                }
            }
        }

        foreach (var targetType in candidateTypes.OrderBy(static type => type.FullName, StringComparer.Ordinal))
        {
            if (!registeredTypes.Add(targetType))
            {
                continue;
            }

            var style = new Style(selector => selector.OfType(targetType));
            style.Setters.Add(new Setter(
                StyledElement.ThemeProperty,
                CreateManagedScopedThemeBinding(this, targetType)));
            styles.Add(style);
        }
    }

    internal bool HasScopedControlTheme(Type styleKey) =>
        _startupResourceProvider?.HasTypedThemeExport(styleKey) == true ||
        _staticResourceProvider?.HasKnownKey(styleKey) == true;

    internal bool UsesScopedControlTheme(StyledElement element)
    {
        if (ScopedControlTheme.IsEnabled)
        {
            return ScopedControlTheme.HasCachedImplicitTheme(element);
        }

        if (_managedScopedThemeTypes?.Contains(element.StyleKey) != true)
        {
            return false;
        }

        if (element.Theme is null)
        {
            return true;
        }

        return ThemeScope.ResolveContext(element) is { } context &&
               TryGetLanguageTheme(GetLanguageThemeKey(context.Snapshot.Language.Id, element.StyleKey), out var activeTheme) &&
               ReferenceEquals(element.Theme, activeTheme);
    }

    internal bool ChangesScopedControlTheme(Type styleKey, ThemeSnapshot previous, ThemeSnapshot next)
    {
        if (previous.Language.Id == next.Language.Id)
        {
            return false;
        }

        return HasLanguageThemeExport(GetLanguageThemeKey(previous.Language.Id, styleKey)) ||
               HasLanguageThemeExport(GetLanguageThemeKey(next.Language.Id, styleKey));
    }

    private bool HasLanguageThemeExport(string languageKey) =>
        _startupResourceProvider?.HasNamedExport(languageKey) == true ||
        _staticResourceProvider?.HasKnownKey(languageKey) == true;

    private bool TryGetLanguageTheme(string languageKey, [NotNullWhen(true)] out ControlTheme? theme)
    {
        if (_startupResourceProvider is { } startup &&
            startup.HasNamedExport(languageKey) &&
            startup.TryGetResource(languageKey, null, out var startupExported) &&
            startupExported is ControlTheme startupTheme)
        {
            theme = startupTheme;
            return true;
        }

        if (_staticResourceProvider is { } staticResources &&
            staticResources.HasKnownKey(languageKey) &&
            staticResources.TryGetResource(languageKey, null, out var staticExported) &&
            staticExported is ControlTheme staticTheme)
        {
            theme = staticTheme;
            return true;
        }

        theme = null;
        return false;
    }

    private static string GetLanguageThemeKey(string language, Type type) =>
        $"{LanguageThemeKeyPrefix}{language}.{type.FullName}";

    private static void CollectExportedLanguageTargetNames(IEnumerable<string> keys, HashSet<string> targetNames)
    {
        foreach (var key in keys)
        {
            if (!key.StartsWith(LanguageThemeKeyPrefix, StringComparison.Ordinal))
            {
                continue;
            }

            var languageSeparator = key.IndexOf('.', LanguageThemeKeyPrefix.Length);
            if (languageSeparator > LanguageThemeKeyPrefix.Length && languageSeparator + 1 < key.Length)
            {
                targetNames.Add(key[(languageSeparator + 1)..]);
            }
        }
    }

    internal object? ResolveScopedControlTheme(StyledElement element, object key, Application application)
    {
        // Only the framework dictionary can substitute the language asset. The implicit lookup
        // uses Avalonia's unqualified resource variant, even when the control's actual variant differs.
        IResourceHost? host = element;
        while (host is not null && !ReferenceEquals(host, application))
        {
            if (host.TryGetResource(key, null, out var value))
            {
                return value;
            }
            host = (host as IStyleHost)?.StylingParent as IResourceHost;
        }

        if (host is null)
        {
            return null;
        }

        if (application.Resources.TryGetResource(key, null, out var resource))
        {
            return resource;
        }

        var resources = application.Styles.Resources;
        if (resources.TryGetValue(key, out resource) ||
            resources.ThemeDictionaries.TryGetValue(ThemeVariant.Default, out var variant) &&
            variant.TryGetResource(key, null, out resource))
        {
            return resource;
        }

        var merged = resources.MergedDictionaries;
        for (var index = merged.Count - 1; index >= 0; --index)
        {
            var provider = merged[index];
            if (ReferenceEquals(provider, Resources) && key is Type target &&
                ThemeScope.ResolveContext(element) is { } context &&
                TryGetLanguageTheme(GetLanguageThemeKey(context.Snapshot.Language.Id, target), out var theme))
            {
                NotifyControlThemeResolved(key, theme);
                return theme;
            }
            if (provider.TryGetResource(key, null, out resource))
            {
                return resource;
            }
        }

        var styles = application.Styles;
        for (var index = styles.Count - 1; index >= 0; --index)
        {
            if (styles[index].TryGetResource(key, null, out resource))
            {
                return resource;
            }
        }
        return null;
    }

    private object ResolveManagedScopedControlTheme(StyledElement element, Type targetType)
    {
        if (_application is not { } application)
        {
            return AvaloniaProperty.UnsetValue;
        }

        IResourceHost? host = element;
        while (host is not null && !ReferenceEquals(host, application))
        {
            if (host.TryGetResource(targetType, null, out var hostResource))
            {
                return hostResource as ControlTheme ?? AvaloniaProperty.UnsetValue;
            }
            host = (host as IStyleHost)?.StylingParent as IResourceHost;
        }

        if (host is null)
        {
            return AvaloniaProperty.UnsetValue;
        }

        if (application.Resources.TryGetResource(targetType, null, out var appResource))
        {
            return appResource as ControlTheme ?? AvaloniaProperty.UnsetValue;
        }

        var resources = application.Styles.Resources;
        if (resources.TryGetValue(targetType, out var styleResource) ||
            resources.ThemeDictionaries.TryGetValue(ThemeVariant.Default, out var variant) &&
            variant.TryGetResource(targetType, null, out styleResource))
        {
            return styleResource as ControlTheme ?? AvaloniaProperty.UnsetValue;
        }

        var merged = resources.MergedDictionaries;
        for (var index = merged.Count - 1; index >= 0; --index)
        {
            var provider = merged[index];
            if (ReferenceEquals(provider, Resources))
            {
                if (ThemeScope.ResolveContext(element) is { } context &&
                    TryGetLanguageTheme(GetLanguageThemeKey(context.Snapshot.Language.Id, targetType), out var theme))
                {
                    NotifyControlThemeResolved(targetType, theme);
                    return theme;
                }
                return AvaloniaProperty.UnsetValue;
            }
            if (provider.TryGetResource(targetType, null, out var mergedResource))
            {
                return mergedResource as ControlTheme ?? AvaloniaProperty.UnsetValue;
            }
        }

        return AvaloniaProperty.UnsetValue;
    }

    private static CompiledBinding CreateManagedScopedThemeBinding(ThemeManager manager, Type targetType)
    {
        var path = new CompiledBindingPathBuilder()
            .Self()
            .Property(s_managedScopedThemeProperty, (source, _) => new ManagedScopedThemeAccessor(source, manager, targetType))
            .Build();
        return new CompiledBinding(path)
        {
            Mode = BindingMode.OneWay,
            Priority = BindingPriority.Style
        };
    }

    private sealed class ManagedScopedThemeAccessor(WeakReference<object?> source, ThemeManager manager, Type targetType) : IPropertyAccessor
    {
        private StyledElement? _target;
        private Action<object?>? _listener;
        private object? _value = AvaloniaProperty.UnsetValue;

        public Type PropertyType => typeof(ControlTheme);
        public object? Value => _value;
        public bool SetValue(object? value, BindingPriority priority) => false;

        public void Subscribe(Action<object?> listener)
        {
            _listener = listener;
            if (source.TryGetTarget(out var candidate) && candidate is StyledElement target)
            {
                _target = target;
                target.ResourcesChanged += OnResourcesChanged;
                target.PropertyChanged += OnPropertyChanged;
                _value = manager.ResolveManagedScopedControlTheme(target, targetType);
            }
            listener(_value);
        }

        private void OnResourcesChanged(object? sender, ResourcesChangedEventArgs args)
        {
            if (_target is { } target &&
                ThemeContext.IsPublishing &&
                ThemeScope.ResolveContext(target) is { } context &&
                context.LastPublish.Previous is { } previous &&
                previous.Language.Id == context.Snapshot.Language.Id &&
                ReferenceEquals(previous.Registry, context.Snapshot.Registry))
            {
                return;
            }
            Refresh();
        }

        private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
        {
            if (args.Property == ThemeScope.ContextProperty)
            {
                Refresh();
            }
        }

        private void Refresh()
        {
            if (_target is not { } target)
            {
                return;
            }

            var resolved = manager.ResolveManagedScopedControlTheme(target, targetType);
            if (!ReferenceEquals(resolved, _value))
            {
                _value = resolved;
                _listener?.Invoke(resolved);
            }
        }

        public void Unsubscribe()
        {
            if (_target is { } target)
            {
                target.ResourcesChanged -= OnResourcesChanged;
                target.PropertyChanged -= OnPropertyChanged;
            }
            _target = null;
            _listener = null;
            _value = AvaloniaProperty.UnsetValue;
        }

        public void Dispose() => Unsubscribe();
    }
}
