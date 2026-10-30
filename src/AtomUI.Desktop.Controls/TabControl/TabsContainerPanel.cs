using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

internal class TabsContainerPanel : Panel
{
    #region 公共属性定义

    public static readonly DirectProperty<TabsContainerPanel, TabScrollViewer?> TabScrollViewerProperty =
        AvaloniaProperty.RegisterDirect<TabsContainerPanel, TabScrollViewer?>(nameof(TabScrollViewer),
            o => o.TabScrollViewer,
            (o, v) => o.TabScrollViewer = v);

    private TabScrollViewer? _tabScrollViewer;

    public TabScrollViewer? TabScrollViewer
    {
        get => _tabScrollViewer;
        set => SetAndRaise(TabScrollViewerProperty, ref _tabScrollViewer, value);
    }

    public static readonly DirectProperty<TabsContainerPanel, IconButton?> AddTabButtonProperty =
        AvaloniaProperty.RegisterDirect<TabsContainerPanel, IconButton?>(nameof(AddTabButton),
            o => o.AddTabButton,
            (o, v) => o.AddTabButton = v);

    private IconButton? _addTabButton;

    public IconButton? AddTabButton
    {
        get => _addTabButton;
        set => SetAndRaise(AddTabButtonProperty, ref _addTabButton, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<TabsContainerPanel, Dock> TabStripPlacementProperty =
        AvaloniaProperty.RegisterDirect<TabsContainerPanel, Dock>(nameof(TabStripPlacement),
            o => o.TabStripPlacement,
            (o, v) => o.TabStripPlacement = v);

    private Dock _tabStripPlacement;

    internal Dock TabStripPlacement
    {
        get => _tabStripPlacement;
        set => SetAndRaise(TabStripPlacementProperty, ref _tabStripPlacement, value);
    }

    #endregion

    static TabsContainerPanel()
    {
        AffectsMeasure<TabsContainerPanel>(TabScrollViewerProperty, AddTabButtonProperty, TabStripPlacementProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        _addTabButton?.Measure(availableSize);
        var addTabButtonSize = _addTabButton?.DesiredSize ?? default;
        if (TabStripPlacement is Dock.Top or Dock.Bottom)
        {
            _tabScrollViewer?.Measure(new Size(
                Math.Max(0, availableSize.Width - addTabButtonSize.Width), availableSize.Height));
            var scrollViewerSize = _tabScrollViewer?.DesiredSize ?? default;
            return new Size(scrollViewerSize.Width + addTabButtonSize.Width,
                Math.Max(scrollViewerSize.Height, addTabButtonSize.Height));
        }

        _tabScrollViewer?.Measure(new Size(availableSize.Width,
            Math.Max(0, availableSize.Height - addTabButtonSize.Height)));
        var verticalScrollViewerSize = _tabScrollViewer?.DesiredSize ?? default;
        return new Size(Math.Max(verticalScrollViewerSize.Width, addTabButtonSize.Width),
            verticalScrollViewerSize.Height + addTabButtonSize.Height);
    }

    protected override Size ArrangeOverride(Size arrangeSize)
    {
        // TODO 暂时不做验证，默认认为两个元素都存在
        // 理论上这里要报错，但是我们是内部使用
        if (_tabScrollViewer is not null && _addTabButton is not null)
        {
            if (TabStripPlacement == Dock.Top || TabStripPlacement == Dock.Bottom)
            {
                var scrollViewerDesiredWidth = _tabScrollViewer.DesiredSize.Width;
                var addTabButtonDesiredWidth = _addTabButton.DesiredSize.Width;
                var totalDesiredWidth        = scrollViewerDesiredWidth + addTabButtonDesiredWidth;

                var btnOffsetY = 0d;
                if (TabStripPlacement == Dock.Top)
                {
                    btnOffsetY = Math.Max(0, arrangeSize.Height - _addTabButton.DesiredSize.Height);
                }

                if (totalDesiredWidth > arrangeSize.Width)
                {
                    var scrollViewerWidth = Math.Max(0, arrangeSize.Width - addTabButtonDesiredWidth);
                    _tabScrollViewer.Arrange(new Rect(new Point(0, 0),
                        new Size(scrollViewerWidth, arrangeSize.Height)));
                    _addTabButton.Arrange(new Rect(new Point(scrollViewerWidth, btnOffsetY),
                        _addTabButton.DesiredSize));
                }
                else
                {
                    _tabScrollViewer.Arrange(new Rect(new Point(0, 0), _tabScrollViewer.DesiredSize));
                    _addTabButton.Arrange(new Rect(new Point(scrollViewerDesiredWidth, btnOffsetY),
                        _addTabButton.DesiredSize));
                }
            }
            else
            {
                var scrollViewerDesiredHeight = _tabScrollViewer.DesiredSize.Height;
                var addTabButtonDesiredHeight = _addTabButton.DesiredSize.Height;
                var totalDesiredHeight        = scrollViewerDesiredHeight + addTabButtonDesiredHeight;
                var btnOffsetX                = 0d;
                if (TabStripPlacement == Dock.Left)
                {
                    btnOffsetX = Math.Max(0, arrangeSize.Width - _addTabButton.DesiredSize.Width);
                }

                if (totalDesiredHeight > arrangeSize.Height)
                {
                    var scrollViewerHeight = Math.Max(0, arrangeSize.Height - addTabButtonDesiredHeight);
                    _tabScrollViewer.Arrange(new Rect(new Point(0, 0),
                        new Size(arrangeSize.Width, scrollViewerHeight)));
                    _addTabButton.Arrange(new Rect(
                        new Point(btnOffsetX, scrollViewerHeight),
                        _addTabButton.DesiredSize));
                }
                else
                {
                    _tabScrollViewer.Arrange(new Rect(new Point(0, 0), _tabScrollViewer.DesiredSize));
                    _addTabButton.Arrange(new Rect(new Point(btnOffsetX, scrollViewerDesiredHeight),
                        _addTabButton.DesiredSize));
                }
            }
        }

        return arrangeSize;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TabScrollViewerProperty)
        {
            var oldScrollViewer = change.GetOldValue<TabScrollViewer?>();
            if (oldScrollViewer is not null)
            {
                Children.Remove(oldScrollViewer);
            }

            if (TabScrollViewer is not null)
            {
                Children.Add(TabScrollViewer);
            }
        }
        else if (change.Property == AddTabButtonProperty)
        {
            var oldAddTabButton = change.GetOldValue<IconButton?>();
            if (oldAddTabButton is not null)
            {
                Children.Remove(oldAddTabButton);
            }

            if (AddTabButton is not null)
            {
                Children.Add(AddTabButton);
            }
        }
    }
}
