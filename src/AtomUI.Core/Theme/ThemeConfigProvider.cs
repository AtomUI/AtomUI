using System.Diagnostics;
using AtomUI.Theme.Configuration;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;

namespace AtomUI.Theme;

public class ThemeConfigProvider : ThemeVariantScope
{
    private static readonly ThemeConfig s_defaultConfig = new ThemeConfigBuilder().Build();

    #region 公共属性定义

    public static readonly StyledProperty<ThemeConfig?> ConfigProperty =
        AvaloniaProperty.Register<ThemeConfigProvider, ThemeConfig?>(nameof(Config));

    public ThemeConfig? Config
    {
        get => GetValue(ConfigProperty);
        set => SetValue(ConfigProperty, value);
    }

    public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;
    public event EventHandler<ThemeChangeFailedEventArgs>? ThemeChangeFailed;

    #endregion

    private ThemeScopeRegistration? _registration;
    private ThemeContext? _context;

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
        DispatchResult(result.WithPublishDiagnostics(publishDiagnostics));
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        var registration = _registration;
        var context = _context;
        _registration = null;
        _context      = null;
        ReleaseRegistration(registration, context);
        base.OnDetachedFromLogicalTree(e);
    }

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
        _registration = null;
        _context = null;
        ReleaseRegistration(registration, context);
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
        ThemeContext? context)
    {
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
