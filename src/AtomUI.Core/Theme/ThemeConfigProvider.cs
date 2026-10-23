using System.Diagnostics;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Styling;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;

namespace AtomUI.Theme;

public class ThemeConfigProvider : ThemeVariantScope
{
    #region 公共属性定义

    public static readonly StyledProperty<ThemeConfig?> ConfigProperty =
        AvaloniaProperty.Register<ThemeConfigProvider, ThemeConfig?>(nameof(Config));

    public ThemeConfig? Config
    {
        get => GetValue(ConfigProperty);
        set => SetValue(ConfigProperty, value);
    }

    public ThemePresentationResult Presentation { get; private set; } = ThemePresentationResult.Unchanged;

    #endregion

    #region 公共事件定义

    public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;
    public event EventHandler<ThemeChangeFailedEventArgs>? ThemeChangeFailed;
    public event EventHandler<ThemePresentationChangedEventArgs>? ThemePresentationChanged;

    #endregion

    private static readonly ThemeConfig s_defaultConfig = new ThemeConfigBuilder().Build();

    private ThemeScopeRegistration? _registration;
    private ThemeContext? _context;
    private ThemeStyleHostLease? _styleLease;

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
        try
        {
            _styleLease = ThemeStyleHostLease.Attach(this, registration.Context);
        }
        catch
        {
            _registration = null;
            _context = null;
            registration.Dispose();
            throw;
        }
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

    #region 内部协作 API

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
        PublishPresentation(result.Presentation);
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

    internal void PublishPresentation(ThemeContext context, ThemePresentationResult result)
    {
        if (ReferenceEquals(_context, context))
        {
            PublishPresentation(result);
        }
    }

    #endregion

    private void PublishPresentation(ThemePresentationResult result)
    {
        if (ReferenceEquals(Presentation, result))
        {
            return;
        }
        Presentation = result;
        if (result.Status != ThemePresentationStatus.Unchanged)
        {
            var diagnostics = new List<ThemeDiagnostic>();
            ThemeEventDispatcher.Dispatch(ThemePresentationChanged, this,
                new ThemePresentationChangedEventArgs(result), diagnostics, nameof(ThemePresentationChanged));
        }
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

    private void ReleaseRegistration(
        ThemeScopeRegistration? registration,
        ThemeContext? context)
    {
        var styleLease = _styleLease;
        _styleLease = null;
        if (context is not null)
        {
            var presentation = context.Manager.CleanupPresentation(styleLease, "ThemeConfigProvider.StyleLease");
            if (presentation.Status == ThemePresentationStatus.Degraded)
            {
                PublishPresentation(presentation);
            }
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
