using AtomUI.Toolkits.GalleryBase.Configuration;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AtomUI.Toolkits.GalleryBase.Controls;

public sealed class ShowCaseZoomOverlayHost : UserControl
{
    public static readonly StyledProperty<object?> PageContentProperty =
        AvaloniaProperty.Register<ShowCaseZoomOverlayHost, object?>(nameof(PageContent));

    private readonly ContentPresenter _contentPresenter;
    private readonly ShowCaseZoomOverlay _overlay;
    private ShowCaseItem? _zoomedItem;
    private object? _zoomedContent;
    private GalleryStickyTabsHost? _suppressedStickyHost;

    public ShowCaseZoomOverlayHost()
    {
        AddHandler(ShowCaseItem.ZoomRequestedEvent, HandleZoomRequested);

        _contentPresenter = new ContentPresenter();
        _overlay = new ShowCaseZoomOverlay();
        _overlay.CloseRequested += HandleOverlayCloseRequested;

        Content = new Grid
        {
            Children =
            {
                _contentPresenter,
                _overlay
            }
        };
    }

    public object? PageContent
    {
        get => GetValue(PageContentProperty);
        set => SetValue(PageContentProperty, value);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        RestoreZoomedItem();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == PageContentProperty)
        {
            _contentPresenter.Content = PageContent;
        }
    }

    private void HandleZoomRequested(object? sender, ShowCaseZoomRequestedEventArgs e)
    {
        if (_zoomedItem is not null ||
            (GalleryBaseConfigurationProvider.Current?.ShowCaseZoom.IsEnabled ?? true) != true)
        {
            return;
        }

        var item = e.Item;
        item.MaterializeDeferredContent();

        _zoomedItem    = item;
        _zoomedContent = item.Content;
        // 捕获内容在卡片内的布局宽度，舞台按该宽度测量，保证放大态布局与卡片内一致
        _overlay.StageMeasureWidth = _zoomedContent is Control staged && staged.Bounds.Width > 0
            ? staged.Bounds.Width
            : double.NaN;
        item.SetCurrentValue(ContentControl.ContentProperty, null);
        item.DetachedFromVisualTree += HandleZoomedItemDetached;

        _overlay.Title        = e.Title;
        _overlay.Description  = e.Description;
        _overlay.DataContext  = item.DataContext;
        // 先恢复逻辑挂载再挂舞台（ImagePreviewer overlay 先例）：内容视觉上迁入
        // overlay，但逻辑上保持对原卡片的挂载——页面级 Styles/Resources 与
        // DataContext 继承沿逻辑树解析，脱离页面树会导致 Grid 展示页等以
        // UserControl.Styles 定制示例的样式整体丢失；面板集合只认领无父级子级，
        // 顺序颠倒会与逻辑挂载冲突。
        if (_zoomedContent is Control stagedContent)
        {
            ((ISetLogicalParent)stagedContent).SetParent(item);
        }

        _overlay.StageContent = _zoomedContent;
        _overlay.IsOpen       = true;

        // 放大 overlay 位于内容树内，钉住的页签宿主却提升在窗口级 adorner 层
        // （渲染于所有内容之上），不挂起会浮在 overlay 之上；关闭后由粘滞机制
        // 自动重新提升。
        _suppressedStickyHost = item.FindAncestorOfType<GalleryStickyTabsHost>();
        if (_suppressedStickyHost is not null)
        {
            _suppressedStickyHost.SetCurrentValue(
                GalleryStickyTabsHost.IsStickyElevationSuppressedProperty, true);
        }

        e.Handled = true;
    }

    private void HandleOverlayCloseRequested(object? sender, EventArgs e)
    {
        RestoreZoomedItem();
    }

    private void HandleZoomedItemDetached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        RestoreZoomedItem();
    }

    private void RestoreZoomedItem()
    {
        if (_zoomedItem is null)
        {
            return;
        }

        var item = _zoomedItem;
        _zoomedItem = null;

        item.DetachedFromVisualTree -= HandleZoomedItemDetached;

        if (_suppressedStickyHost is not null)
        {
            _suppressedStickyHost.SetCurrentValue(
                GalleryStickyTabsHost.IsStickyElevationSuppressedProperty, false);
            _suppressedStickyHost = null;
        }

        _overlay.IsOpen           = false;
        _overlay.StageContent     = null;
        _overlay.Title            = null;
        _overlay.Description      = null;
        _overlay.StageMeasureWidth = double.NaN;
        _overlay.DataContext      = null;

        if (_zoomedContent is Control stagedContent)
        {
            ((ISetLogicalParent)stagedContent).SetParent(null);
        }

        item.SetCurrentValue(ContentControl.ContentProperty, _zoomedContent);
        _zoomedContent = null;
    }
}
