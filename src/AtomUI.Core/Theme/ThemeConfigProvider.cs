using System.Diagnostics;
using AtomUI.Data;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Data;
using Avalonia.LogicalTree;

namespace AtomUI.Theme;

/// <summary>
/// A scope of the theme scope graph that gives its content a theme configuration of its own. Once it is
/// attached to the logical tree it is registered with the theme manager under the theme context inherited from
/// an ancestor, and the content reads the theme of this scope: the theme variant, the theme resources and the
/// Token values that <see cref="Config" /> sets.
/// </summary>
/// <remarks>
/// Attaching without an inherited theme context throws an <see cref="InvalidOperationException" />. Detaching
/// releases the registration, so attaching the scope again registers it anew.
/// </remarks>
public class ThemeConfigProvider : ThemeVariantScope
{
    private static readonly ThemeConfig s_defaultConfig = new ThemeConfigBuilder().Build();

    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Config" /> property.
    /// </summary>
    public static readonly StyledProperty<ThemeConfig?> ConfigProperty =
        AvaloniaProperty.Register<ThemeConfigProvider, ThemeConfig?>(nameof(Config));

    /// <summary>
    /// The theme configuration of this scope: the axes it fixes for its content and the Token values it sets.
    /// Axes left unset are inherited from the parent scope. <see langword="null" /> uses the default configuration,
    /// the one built by an empty <c>ThemeConfigBuilder</c>.
    /// </summary>
    /// <remarks>
    /// The value is read when the scope attaches. A change while the scope is attached makes the theme manager
    /// replace the scope's configuration at once; a change before that takes effect when the scope attaches.
    /// </remarks>
    public ThemeConfig? Config
    {
        get => GetValue(ConfigProperty);
        set => SetValue(ConfigProperty, value);
    }

    /// <summary>
    /// Raised once a theme transition of this scope was committed and published.
    /// </summary>
    /// <remarks>
    /// An exception thrown by a handler is logged and does not reach the code that ran the transition.
    /// </remarks>
    public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;
    /// <summary>
    /// Raised when a theme transition of this scope failed, with the diagnostics and the exception that
    /// describe the failure.
    /// </summary>
    /// <remarks>
    /// An exception thrown by a handler is handled as for <see cref="ThemeChanged" />.
    /// </remarks>
    public event EventHandler<ThemeChangeFailedEventArgs>? ThemeChangeFailed;

    #endregion

    private ThemeScopeRegistration? _registration;
    private ThemeContext? _context;
    private IDisposable? _foregroundBinding;
    private IDisposable? _fontSizeBinding;
    private IDisposable? _fontFamilyBinding;

    /// <summary>
    /// Calls the base member first, then registers the scope with the theme manager under the theme context it
    /// inherits from an ancestor, makes the theme resources, theme variant and inherited text defaults (foreground,
    /// font size and font family) of the scope available to its content, and raises <see cref="ThemeChanged" /> or
    /// <see cref="ThemeChangeFailed" /> with the result.
    /// </summary>
    /// <remarks>
    /// An attachment without an inherited theme context throws an
    /// <see cref="InvalidOperationException" />. A scope that is already registered only calls the base member.
    /// </remarks>
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        if (_registration is not null)
        {
            return;
        }

        var parentContext = ThemeScope.ResolveContext(this, e.Root) ??
                            throw new InvalidOperationException(
                                "ThemeConfigProvider requires an inherited ThemeContext.");
        var registration = parentContext.Manager.RegisterScope(
            this,
            parentContext,
            Config ?? s_defaultConfig,
            out var result);
        _registration = registration;
        _context      = registration.Context;
        var publishDiagnostics = new List<ThemeDiagnostic>();
        ThemePublishBoundary.Dispatch(
            () => Resources.MergedDictionaries.Add(registration.Context.ResourceProvider),
            this,
            publishDiagnostics,
            $"ThemeScope[{registration.RegistrationId}].Resources");
        ThemePublishBoundary.Dispatch(
            () => SetValue(ThemeScope.ContextProperty, registration.Context),
            this,
            publishDiagnostics,
            $"ThemeScope[{registration.RegistrationId}].Context");
        publishDiagnostics.AddRange(PublishCommittedContext(
            registration.Context,
            notifyResources: false));
        // ThemeConfigProvider is a non-templated ThemeVariantScope decorator with no ControlTheme, so code
        // bindings at Style priority are the only way to update inherited TextElement foreground, font size and
        // font family for nested scopes while still letting local values take precedence.
        ThemePublishBoundary.Dispatch(
            () => _foregroundBinding = TokenResourceBinder.CreateGlobalTokenBinding(
                this,
                TextElement.ForegroundProperty,
                SharedTokenKind.ColorText,
                BindingPriority.Style),
            this,
            publishDiagnostics,
            $"ThemeScope[{registration.RegistrationId}].Foreground");
        ThemePublishBoundary.Dispatch(
            () => _fontSizeBinding = TokenResourceBinder.CreateGlobalTokenBinding(
                this,
                TextElement.FontSizeProperty,
                SharedTokenKind.FontSize,
                BindingPriority.Style),
            this,
            publishDiagnostics,
            $"ThemeScope[{registration.RegistrationId}].FontSize");
        ThemePublishBoundary.Dispatch(
            () => _fontFamilyBinding = TokenResourceBinder.CreateGlobalTokenBinding(
                this,
                TextElement.FontFamilyProperty,
                SharedTokenKind.FontFamily,
                BindingPriority.Style),
            this,
            publishDiagnostics,
            $"ThemeScope[{registration.RegistrationId}].FontFamily");
        DispatchResult(result.WithPublishDiagnostics(publishDiagnostics));
    }

    /// <summary>
    /// Releases the scope's registration before the base member is called: the inherited text default bindings are
    /// disposed, the theme resources are removed from <c>Resources</c>, the <c>ThemeScope.ContextProperty</c> value
    /// and the theme variant set for the scope are cleared, and the registration is disposed. An exception thrown by
    /// one of these steps is written to the debug trace and does not stop the others.
    /// </summary>
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        var registration = _registration;
        var context = _context;
        var foregroundBinding = _foregroundBinding;
        var fontSizeBinding = _fontSizeBinding;
        var fontFamilyBinding = _fontFamilyBinding;
        _registration      = null;
        _context           = null;
        _foregroundBinding = null;
        _fontSizeBinding   = null;
        _fontFamilyBinding = null;
        ReleaseRegistration(registration, context, foregroundBinding, fontSizeBinding, fontFamilyBinding);
        base.OnDetachedFromLogicalTree(e);
    }

    /// <summary>
    /// Calls the base member first, then hands a change of <see cref="Config" /> to the theme manager while the
    /// scope is registered, which replaces the scope's configuration; <see langword="null" /> is handed over as the
    /// default configuration.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property != ConfigProperty || _registration is null)
        {
            return;
        }

        _registration.Context.Manager.ReplaceScopeConfig(
            this,
            change.GetNewValue<ThemeConfig?>() ?? s_defaultConfig);
    }

    internal IReadOnlyList<ThemeDiagnostic> PublishCommittedContext(
        ThemeContext context,
        bool notifyResources)
    {
        if (!ReferenceEquals(_context, context))
        {
            return Array.Empty<ThemeDiagnostic>();
        }

        var diagnostics = new List<ThemeDiagnostic>();
        SetCommittedVariant(context, diagnostics);
        ThemePublishBoundary.Dispatch(
            () => diagnostics.AddRange(context.Publish(notifyResources)),
            this,
            diagnostics,
            $"ThemeScope[{context.RegistrationId}].ContextPublish");
        return diagnostics.AsReadOnly();
    }

    internal IReadOnlyList<ThemeDiagnostic> SetCommittedVariant(ThemeContext context)
    {
        var diagnostics = new List<ThemeDiagnostic>();
        SetCommittedVariant(context, diagnostics);
        return diagnostics.AsReadOnly();
    }

    private void SetCommittedVariant(
        ThemeContext context,
        List<ThemeDiagnostic> diagnostics)
    {
        if (!ReferenceEquals(_context, context))
        {
            return;
        }

        ThemePublishBoundary.Dispatch(
            () => SetCurrentValue(
                RequestedThemeVariantProperty,
                context.Appearance == ThemeAppearance.Dark
                    ? Avalonia.Styling.ThemeVariant.Dark
                    : Avalonia.Styling.ThemeVariant.Light),
            this,
            diagnostics,
            $"ThemeScope[{context.RegistrationId}].ThemeVariant");
    }

    internal void ReleaseManagerRegistration(ThemeContext context)
    {
        if (!ReferenceEquals(_context, context))
        {
            return;
        }

        var registration = _registration;
        var foregroundBinding = _foregroundBinding;
        var fontSizeBinding = _fontSizeBinding;
        var fontFamilyBinding = _fontFamilyBinding;
        _registration      = null;
        _context           = null;
        _foregroundBinding = null;
        _fontSizeBinding   = null;
        _fontFamilyBinding = null;
        ReleaseRegistration(registration, context, foregroundBinding, fontSizeBinding, fontFamilyBinding);
    }

    internal void DispatchResult(ThemeScopeUpdateResult result)
    {
        var publishDiagnostics = new List<ThemeDiagnostic>();
        if (result.Status == ThemeTransitionStatus.Committed)
        {
            ThemeEventDispatcher.Dispatch(
                ThemeChanged,
                this,
                new ThemeChangedEventArgs(
                    result.Request,
                    result.State,
                    result.PublishDiagnostics),
                publishDiagnostics,
                nameof(ThemeChanged));
            return;
        }

        if (result.Status != ThemeTransitionStatus.Failed)
        {
            return;
        }

        ThemeEventDispatcher.Dispatch(
            ThemeChangeFailed,
            this,
            new ThemeChangeFailedEventArgs(
                result.Request,
                result.Diagnostics,
                result.Exception),
            publishDiagnostics,
            nameof(ThemeChangeFailed));
    }

    private void ReleaseRegistration(
        ThemeScopeRegistration? registration,
        ThemeContext? context,
        IDisposable? foregroundBinding,
        IDisposable? fontSizeBinding,
        IDisposable? fontFamilyBinding)
    {
        if (foregroundBinding is not null)
        {
            CleanupBoundary(foregroundBinding.Dispose);
        }
        if (fontSizeBinding is not null)
        {
            CleanupBoundary(fontSizeBinding.Dispose);
        }
        if (fontFamilyBinding is not null)
        {
            CleanupBoundary(fontFamilyBinding.Dispose);
        }
        if (context is not null)
        {
            CleanupBoundary(() => Resources.MergedDictionaries.Remove(context.ResourceProvider));
        }
        CleanupBoundary(() => ClearValue(ThemeScope.ContextProperty));
        CleanupBoundary(() => ClearValue(RequestedThemeVariantProperty));
        if (registration is not null)
        {
            CleanupBoundary(registration.Dispose);
        }
    }

    private static void CleanupBoundary(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
        }
    }
}
