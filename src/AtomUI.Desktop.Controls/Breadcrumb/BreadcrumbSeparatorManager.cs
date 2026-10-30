using System.Collections.Specialized;
using AtomUI.Data;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia;
using Avalonia.Controls.Presenters;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Owns the Breadcrumb sibling separator lifecycle: maintains N - 1 separator presenters as
/// logical children of the breadcrumb, binds each to its preceding item container and keeps
/// the interleaved visual order in <see cref="BreadcrumbItemsPanel"/>.
/// </summary>
internal sealed class BreadcrumbSeparatorManager : IDisposable
{
    private readonly Breadcrumb _owner;
    private readonly List<SeparatorEntry> _separatorEntries = new();
    private BreadcrumbItemsPanel? _itemsPanel;
    private bool _itemsCollectionSubscribed;
    private DispatcherOperation? _pendingUpdate;

    public BreadcrumbSeparatorManager(Breadcrumb owner)
    {
        _owner = owner;
    }

    /// <summary>
    /// The template was (re)applied: drop the stale panel reference and rebuild separator visuals.
    /// </summary>
    public void OnTemplateApplied()
    {
        _pendingUpdate?.Abort();
        _pendingUpdate = null;
        ClearSeparators();
        _itemsPanel = null;
        Attach();
    }

    /// <summary>
    /// Coalesces collection/container churn into one update after the item generator settles.
    /// </summary>
    public void QueueUpdate()
    {
        if (!_itemsCollectionSubscribed || _pendingUpdate is not null)
        {
            return;
        }

        _pendingUpdate = Dispatcher.UIThread.InvokeAsync(() =>
        {
            _pendingUpdate = null;
            Update();
        }, DispatcherPriority.Loaded);
    }

    /// <summary>
    /// Re-evaluates the separator set against the current item and container state.
    /// </summary>
    public void Update()
    {
        if (!_itemsCollectionSubscribed)
        {
            return;
        }

        var targetCount = Math.Max(0, _owner.ItemCount - 1);
        while (_separatorEntries.Count > targetCount)
        {
            RemoveSeparatorAt(_separatorEntries.Count - 1);
        }

        while (_separatorEntries.Count < targetCount)
        {
            _separatorEntries.Add(CreateSeparator());
        }

        for (var i = 0; i < _separatorEntries.Count; i++)
        {
            BindSeparator(_separatorEntries[i], i);
        }

        GetItemsPanel()?.SyncSeparators(_separatorEntries.Select(static entry => entry.Presenter).ToList());
    }

    public void Dispose()
    {
        _pendingUpdate?.Abort();
        _pendingUpdate = null;
        if (_itemsCollectionSubscribed)
        {
            _owner.Items.CollectionChanged -= HandleItemsChanged;
            _itemsCollectionSubscribed = false;
        }
        ClearSeparators();
        _itemsPanel = null;
    }

    public void Attach()
    {
        if (!_owner.IsAttachedToVisualTree())
        {
            return;
        }
        if (!_itemsCollectionSubscribed)
        {
            _owner.Items.CollectionChanged += HandleItemsChanged;
            _itemsCollectionSubscribed = true;
        }
        Update();
        QueueUpdate();
    }

    private void HandleItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) => QueueUpdate();

    private BreadcrumbItemsPanel? GetItemsPanel()
    {
        if (_itemsPanel is null)
        {
            _itemsPanel = _owner.GetVisualDescendants().OfType<BreadcrumbItemsPanel>().FirstOrDefault();
        }

        return _itemsPanel;
    }

    private void ClearSeparators()
    {
        _itemsPanel?.ClearSeparatorVisuals();
        foreach (var entry in _separatorEntries)
        {
            entry.Dispose();
            _owner.DetachSeparatorLogicalChild(entry.Presenter);
        }

        _separatorEntries.Clear();
    }

    private SeparatorEntry CreateSeparator()
    {
        var presenter = new ContentPresenter
        {
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment   = VerticalAlignment.Center,
            IsHitTestVisible           = false
        };
        presenter.Classes.Add(BreadcrumbSemanticParts.SeparatorClass);

        var entry = new SeparatorEntry(presenter)
        {
            ForegroundTokenBinding = TokenResourceBinder.CreateControlTokenBinding(
                _owner,
                presenter,
                ContentPresenter.ForegroundProperty,
                BreadcrumbTokenKind.SeparatorColor),
            MarginTokenBinding = TokenResourceBinder.CreateControlTokenBinding(
                _owner,
                presenter,
                ContentPresenter.MarginProperty,
                BreadcrumbTokenKind.SeparatorMargin)
        };

        _owner.AttachSeparatorLogicalChild(presenter);
        return entry;
    }

    private void RemoveSeparatorAt(int index)
    {
        var entry = _separatorEntries[index];
        _separatorEntries.RemoveAt(index);
        entry.Dispose();
        _owner.DetachSeparatorLogicalChild(entry.Presenter);
    }

    private void BindSeparator(SeparatorEntry entry, int index)
    {
        var container = _owner.ContainerFromIndex(index) as BreadcrumbItem;
        if (ReferenceEquals(entry.SourceItem, container))
        {
            return;
        }

        entry.ContentBinding?.Dispose();
        entry.TemplateBinding?.Dispose();
        entry.ContentBinding  = null;
        entry.TemplateBinding = null;
        entry.SourceItem      = container;

        if (container is null)
        {
            return;
        }

        entry.ContentBinding = entry.Presenter.Bind(
            ContentPresenter.ContentProperty,
            container.GetObservable(BreadcrumbItem.SeparatorProperty));
        entry.TemplateBinding = entry.Presenter.Bind(
            ContentPresenter.ContentTemplateProperty,
            container.GetObservable(BreadcrumbItem.SeparatorTemplateProperty));
    }

    private sealed class SeparatorEntry : IDisposable
    {
        public SeparatorEntry(ContentPresenter presenter)
        {
            Presenter = presenter;
        }

        public ContentPresenter Presenter { get; }

        public BreadcrumbItem? SourceItem { get; set; }

        public IDisposable? ContentBinding { get; set; }

        public IDisposable? TemplateBinding { get; set; }

        public IDisposable? ForegroundTokenBinding { get; set; }

        public IDisposable? MarginTokenBinding { get; set; }

        public void Dispose()
        {
            ContentBinding?.Dispose();
            TemplateBinding?.Dispose();
            ForegroundTokenBinding?.Dispose();
            MarginTokenBinding?.Dispose();
            ContentBinding         = null;
            TemplateBinding        = null;
            ForegroundTokenBinding = null;
            MarginTokenBinding     = null;
            SourceItem             = null;
        }
    }
}
