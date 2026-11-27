using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Shared base of the Card controls: header, extra, cover, content and an actions collection, plus an on-demand
/// loading indicator. While <see cref="IsLoading" /> is true the indicator created by
/// <see cref="CreateLoadingIndicator" /> is inserted into the template part <see cref="LoadingHostPart" /> and the
/// content is hidden; once loading ends the indicator is removed and dropped.
/// </summary>
public abstract class AbstractCard : HeaderedContentControl
{
    /// <summary>
    /// Panel of the control template the loading indicator is added to.
    /// </summary>
    internal const string LoadingHostPart = "PART_LoadingHost";

    private const string HeaderlessPseudoClass = ":headerless";

    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Extra" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ExtraProperty =
        AvaloniaProperty.Register<AbstractCard, object?>(nameof(Extra));

    /// <summary>
    /// Identifies the <see cref="ExtraTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ExtraTemplateProperty =
        AvaloniaProperty.Register<AbstractCard, IDataTemplate?>(nameof(ExtraTemplate));

    /// <summary>
    /// Identifies the <see cref="Cover" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> CoverProperty =
        AvaloniaProperty.Register<AbstractCard, object?>(nameof(Cover));

    /// <summary>
    /// Identifies the <see cref="CoverTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> CoverTemplateProperty =
        AvaloniaProperty.Register<AbstractCard, IDataTemplate?>(nameof(CoverTemplate));

    /// <summary>
    /// Identifies the <see cref="IsLoading" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<AbstractCard, bool>(nameof(IsLoading));

    /// <summary>
    /// Content shown at the right end of the header area, next to the title.
    /// Setting it counts as header content, so the header area is shown even without a title.
    /// </summary>
    public object? Extra
    {
        get => GetValue(ExtraProperty);
        set => SetValue(ExtraProperty, value);
    }

    /// <summary>
    /// Template used to present <see cref="Extra" />. A value counts as header content on its own, so the header
    /// area is shown even while <see cref="Extra" /> is <see langword="null" />.
    /// </summary>
    public IDataTemplate? ExtraTemplate
    {
        get => GetValue(ExtraTemplateProperty);
        set => SetValue(ExtraTemplateProperty, value);
    }

    /// <summary>
    /// Content shown in the cover area, which spans the width of the card between the header area and the body.
    /// </summary>
    public object? Cover
    {
        get => GetValue(CoverProperty);
        set => SetValue(CoverProperty, value);
    }

    /// <summary>
    /// Template used to present <see cref="Cover" /> in the cover area.
    /// </summary>
    public IDataTemplate? CoverTemplate
    {
        get => GetValue(CoverTemplateProperty);
        set => SetValue(CoverTemplateProperty, value);
    }

    /// <summary>
    /// Shows a loading indicator in place of the body while <see langword="true" />. The header area, the cover
    /// and the actions stay visible.
    /// </summary>
    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    /// <summary>
    /// The controls shown in the actions row at the bottom of the card, one column per item.
    /// The row is shown only while the collection has an item. <c>Card</c> copies the items into the row when
    /// the template is applied and applies later changes item by item, so remove items instead of clearing the
    /// collection.
    /// </summary>
    public Avalonia.Controls.Controls Actions { get; } = new();

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<AbstractCard, bool> IsActionsPanelVisibleProperty =
        AvaloniaProperty.RegisterDirect<AbstractCard, bool>(
            nameof(IsActionsPanelVisible),
            o => o.IsActionsPanelVisible,
            (o, v) => o.IsActionsPanelVisible = v);

    internal static readonly DirectProperty<AbstractCard, bool> IsContentVisibleProperty =
        AvaloniaProperty.RegisterDirect<AbstractCard, bool>(
            nameof(IsContentVisible),
            o => o.IsContentVisible);

    private bool _actionsPanelVisible;

    /// <summary>
    /// Whether the actions panel of the template is visible. The card updates it whenever <see cref="Actions" />
    /// changes; the panel is visible while the collection has an item. The theme binds the <c>IsVisible</c> of the
    /// actions panel to it.
    /// </summary>
    internal bool IsActionsPanelVisible
    {
        get => _actionsPanelVisible;
        set => SetAndRaise(IsActionsPanelVisibleProperty, ref _actionsPanelVisible, value);
    }

    /// <summary>
    /// Whether the content is shown; the opposite of <see cref="IsLoading" />.
    /// </summary>
    internal bool IsContentVisible => !IsLoading;

    #endregion

    private Panel? _loadingHost;
    private Control? _loadingIndicator;

    /// <summary>
    /// The loading indicator created on demand; null while not loading.
    /// </summary>
    internal Control? LoadingIndicator => _loadingIndicator;

    /// <summary>
    /// The template part the loading indicator is added to; null when the template has none.
    /// </summary>
    protected Panel? LoadingHost => _loadingHost;

    protected AbstractCard()
    {
        Actions.CollectionChanged += HandleActionsChanged;
    }

    /// <summary>
    /// Whether the header area has anything to show; drives the <c>:headerless</c> pseudo-class. By default any of
    /// <see cref="HeaderedContentControl.Header" />, <see cref="HeaderedContentControl.HeaderTemplate" />,
    /// <see cref="Extra" /> and <see cref="ExtraTemplate" /> counts.
    /// </summary>
    protected virtual bool HasHeaderContent()
    {
        return Header is not null || HeaderTemplate is not null || Extra is not null || ExtraTemplate is not null;
    }

    /// <summary>
    /// Called after <see cref="Actions" /> changed, before <c>IsActionsPanelVisible</c> is refreshed.
    /// </summary>
    protected virtual void OnActionsChanged(NotifyCollectionChangedEventArgs e)
    {
    }

    /// <summary>
    /// Creates the indicator shown while <see cref="IsLoading" /> is true.
    /// </summary>
    protected abstract Control CreateLoadingIndicator();

    /// <summary>
    /// Puts the indicator into the template; adds it to <see cref="LoadingHost" /> by default.
    /// </summary>
    protected virtual void AttachLoadingIndicator(Control indicator)
    {
        _loadingHost?.Children.Add(indicator);
    }

    /// <summary>
    /// Takes the indicator out of the template; removes it from <see cref="LoadingHost" /> by default.
    /// </summary>
    protected virtual void DetachLoadingIndicator(Control indicator)
    {
        _loadingHost?.Children.Remove(indicator);
    }

    /// <summary>
    /// Updates the card after a property value changed: the <c>:headerless</c> pseudo-class when
    /// <see cref="HeaderedContentControl.Header" />, <see cref="HeaderedContentControl.HeaderTemplate" />,
    /// <see cref="Extra" /> or <see cref="ExtraTemplate" /> changed, and the loading indicator when
    /// <see cref="IsLoading" /> changed. Overrides must call the base member first.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == HeaderProperty ||
            change.Property == HeaderTemplateProperty ||
            change.Property == ExtraProperty ||
            change.Property == ExtraTemplateProperty)
        {
            UpdateHeaderlessPseudoClass();
        }
        else if (change.Property == IsLoadingProperty)
        {
            RaisePropertyChanged(IsContentVisibleProperty, !change.GetOldValue<bool>(), !change.GetNewValue<bool>());
            UpdateLoadingIndicator();
        }
    }

    /// <summary>
    /// Binds the applied template: re-evaluates the <c>:headerless</c> pseudo-class, drops the loading indicator
    /// of the previous template and stores the panel named <see cref="LoadingHostPart" /> as
    /// <see cref="LoadingHost" />. A new indicator is shown while <see cref="IsLoading" /> is true. Overrides must
    /// call the base member first.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdateHeaderlessPseudoClass();
        RemoveLoadingIndicator();
        _loadingHost = e.NameScope.Find<Panel>(LoadingHostPart);
        UpdateLoadingIndicator();
    }

    private void UpdateHeaderlessPseudoClass()
    {
        PseudoClasses.Set(HeaderlessPseudoClass, !HasHeaderContent());
    }

    private void UpdateLoadingIndicator()
    {
        // Clearing the template does not call OnApplyTemplate, but it does detach the old parts from this control;
        // a host that left the template must not receive a new indicator.
        if (_loadingHost is not null && !ReferenceEquals(_loadingHost.TemplatedParent, this))
        {
            RemoveLoadingIndicator();
            _loadingHost = null;
        }

        var shouldExist = IsLoading && _loadingHost is not null;
        if (shouldExist == (_loadingIndicator is not null))
        {
            return;
        }
        if (shouldExist)
        {
            var indicator = CreateLoadingIndicator();
            _loadingIndicator = indicator;
            AttachLoadingIndicator(indicator);
        }
        else
        {
            RemoveLoadingIndicator();
        }
    }

    private void RemoveLoadingIndicator()
    {
        if (_loadingIndicator is not { } indicator)
        {
            return;
        }
        _loadingIndicator = null;
        DetachLoadingIndicator(indicator);
    }

    private void HandleActionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnActionsChanged(e);
        SetCurrentValue(IsActionsPanelVisibleProperty, Actions.Count > 0);
    }
}
