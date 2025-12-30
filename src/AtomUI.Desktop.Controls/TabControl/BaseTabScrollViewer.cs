using System.Globalization;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.Themes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Converters;
using Avalonia.Controls.Diagnostics;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input.Raw;
using Avalonia.Media;
using Colors = Avalonia.Media.Colors;
using GradientStop = Avalonia.Media.GradientStop;

namespace AtomUI.Desktop.Controls;

internal abstract class BaseTabScrollViewer : ScrollViewer
{
    private const int EdgeIndicatorZIndex = 1000;

    #region 内部属性定义

    internal static readonly DirectProperty<BaseTabScrollViewer, Dock> TabStripPlacementProperty =
        AvaloniaProperty.RegisterDirect<BaseTabScrollViewer, Dock>(nameof(TabStripPlacement),
            o => o.TabStripPlacement,
            (o, v) => o.TabStripPlacement = v);

    internal static readonly DirectProperty<BaseTabScrollViewer, IBrush?> EdgeShadowStartColorProperty =
        AvaloniaProperty.RegisterDirect<BaseTabScrollViewer, IBrush?>(nameof(EdgeShadowStartColor),
            o => o.EdgeShadowStartColor,
            (o, v) => o.EdgeShadowStartColor = v);

    internal static readonly DirectProperty<BaseTabScrollViewer, double> MenuEdgeThicknessProperty =
        AvaloniaProperty.RegisterDirect<BaseTabScrollViewer, double>(nameof(MenuEdgeThickness),
            o => o.MenuEdgeThickness,
            (o, v) => o.MenuEdgeThickness = v);

    private Dock _tabStripPlacement;

    internal Dock TabStripPlacement
    {
        get => _tabStripPlacement;
        set => SetAndRaise(TabStripPlacementProperty, ref _tabStripPlacement, value);
    }

    private IBrush? _edgeShadowStartColor;

    internal IBrush? EdgeShadowStartColor
    {
        get => _edgeShadowStartColor;
        set => SetAndRaise(EdgeShadowStartColorProperty, ref _edgeShadowStartColor, value);
    }

    private double _menuEdgeThickness;

    internal double MenuEdgeThickness
    {
        get => _menuEdgeThickness;
        set => SetAndRaise(MenuEdgeThicknessProperty, ref _menuEdgeThickness, value);
    }
    
    #endregion

    private protected IconButton? MenuIndicator;
    private protected Border? StartEdgeIndicator;
    private protected Border? EndEdgeIndicator;
    private protected MenuFlyout? MenuFlyout;

    static BaseTabScrollViewer()
    {
        AffectsMeasure<BaseTabScrollViewer>(TabStripPlacementProperty);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == VerticalScrollBarVisibilityProperty ||
            change.Property == OffsetProperty ||
            change.Property == ExtentProperty ||
            change.Property == ViewportProperty)
        {
            SetupIndicatorsVisibility();
        }
    }

    private void SetupEdgeIndicatorSize()
    {
        if (Presenter is not null)
        {
            if (TabStripPlacement == Dock.Top || TabStripPlacement == Dock.Bottom)
            {
                if (StartEdgeIndicator is not null)
                {
                    StartEdgeIndicator.Height     = Presenter.DesiredSize.Height;
                    StartEdgeIndicator.Width      = _menuEdgeThickness;
                    StartEdgeIndicator.ZIndex     = EdgeIndicatorZIndex;
                    StartEdgeIndicator.Background = BuildEdgeIndicatorBrush(TabStripPlacement, true);
                }

                if (EndEdgeIndicator is not null)
                {
                    EndEdgeIndicator.Height     = Presenter.DesiredSize.Height;
                    EndEdgeIndicator.Width      = _menuEdgeThickness;
                    EndEdgeIndicator.Margin     = new Thickness(0, 0, _menuEdgeThickness, 0);
                    EndEdgeIndicator.ZIndex     = EdgeIndicatorZIndex;
                    EndEdgeIndicator.Background = BuildEdgeIndicatorBrush(TabStripPlacement, false);
                }
            }
            else
            {
                if (StartEdgeIndicator is not null)
                {
                    StartEdgeIndicator.Width      = Presenter.DesiredSize.Width;
                    StartEdgeIndicator.Height     = _menuEdgeThickness;
                    StartEdgeIndicator.ZIndex     = EdgeIndicatorZIndex;
                    StartEdgeIndicator.Background = BuildEdgeIndicatorBrush(TabStripPlacement, true);
                }

                if (EndEdgeIndicator is not null)
                {
                    EndEdgeIndicator.Width      = Presenter.DesiredSize.Width;
                    EndEdgeIndicator.Height     = _menuEdgeThickness;
                    EndEdgeIndicator.Margin     = new Thickness(0, 0, 0, _menuEdgeThickness);
                    EndEdgeIndicator.ZIndex     = EdgeIndicatorZIndex;
                    EndEdgeIndicator.Background = BuildEdgeIndicatorBrush(TabStripPlacement, false);
                }
            }
        }
    }

    private LinearGradientBrush BuildEdgeIndicatorBrush(Dock placement, bool isStart)
    {
        var linearGradientBrush = new LinearGradientBrush
        {
            GradientStops =
            {
                new GradientStop((_edgeShadowStartColor as ISolidColorBrush)!.Color, 0),
                new GradientStop(Colors.Transparent, 1)
            }
        };
        if (placement == Dock.Top || placement == Dock.Bottom)
        {
            if (isStart)
            {
                linearGradientBrush.StartPoint = new RelativePoint(0, 0.5, RelativeUnit.Relative);
                linearGradientBrush.EndPoint   = new RelativePoint(1, 0.5, RelativeUnit.Relative);
            }
            else
            {
                linearGradientBrush.StartPoint = new RelativePoint(1, 0.5, RelativeUnit.Relative);
                linearGradientBrush.EndPoint   = new RelativePoint(0, 0.5, RelativeUnit.Relative);
            }
        }
        else
        {
            if (isStart)
            {
                linearGradientBrush.StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative);
                linearGradientBrush.EndPoint   = new RelativePoint(0.5, 1, RelativeUnit.Relative);
            }
            else
            {
                linearGradientBrush.StartPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative);
                linearGradientBrush.EndPoint   = new RelativePoint(0.5, 0, RelativeUnit.Relative);
            }
        }

        return linearGradientBrush;
    }

    protected override bool RegisterContentPresenter(ContentPresenter presenter)
    {
        if (presenter is TabScrollContentPresenter tabStripScrollContentPresenter)
        {
            tabStripScrollContentPresenter.TabStripPlacement = TabStripPlacement;
        }

        return base.RegisterContentPresenter(presenter);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        MenuIndicator      = e.NameScope.Find<IconButton>(TabScrollViewerThemeConstants.ScrollMenuIndicatorPart);
        StartEdgeIndicator = e.NameScope.Find<Border>(TabScrollViewerThemeConstants.ScrollStartEdgeIndicatorPart);
        EndEdgeIndicator   = e.NameScope.Find<Border>(TabScrollViewerThemeConstants.ScrollEndEdgeIndicatorPart);

        SetupIndicatorsVisibility();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var size = base.MeasureOverride(availableSize);
        SetupEdgeIndicatorSize();
        return size;
    }

    private void SetupIndicatorsVisibility()
    {
        var args = new List<object?>();

        object? scrollUpVisibility;
        object? scrollDownVisibility;
        if (TabStripPlacement == Dock.Top || TabStripPlacement == Dock.Bottom)
        {
            args.Add(ScrollBarVisibility.Auto);
            args.Add(Offset.X);
            args.Add(Extent.Width);
            args.Add(Viewport.Width);
            scrollUpVisibility =
                MenuScrollingVisibilityConverter.Instance.Convert(args, typeof(bool), 0d, CultureInfo.CurrentCulture);
            scrollDownVisibility =
                MenuScrollingVisibilityConverter.Instance.Convert(args, typeof(bool), 100d, CultureInfo.CurrentCulture);
        }
        else
        {
            args.Add(ScrollBarVisibility.Auto);
            args.Add(Offset.Y);
            args.Add(Extent.Height);
            args.Add(Viewport.Height);
            scrollUpVisibility =
                MenuScrollingVisibilityConverter.Instance.Convert(args, typeof(bool), 0d, CultureInfo.CurrentCulture);
            scrollDownVisibility =
                MenuScrollingVisibilityConverter.Instance.Convert(args, typeof(bool), 100d, CultureInfo.CurrentCulture);
        }

        if (StartEdgeIndicator is not null &&
            scrollUpVisibility is not null &&
            scrollUpVisibility != AvaloniaProperty.UnsetValue)
        {
            StartEdgeIndicator.IsVisible = (bool)scrollUpVisibility;
        }

        if (EndEdgeIndicator is not null &&
            scrollDownVisibility is not null &&
            scrollDownVisibility != AvaloniaProperty.UnsetValue)
        {
            EndEdgeIndicator.IsVisible = (bool)scrollDownVisibility;
        }

        if (MenuIndicator is not null)
        {
            var startEdgeVisible = StartEdgeIndicator?.IsVisible ?? false;
            var endEdgeVisible   = EndEdgeIndicator?.IsVisible ?? false;
            MenuIndicator.IsVisible = startEdgeVisible || endEdgeVisible;
        }
    }
    
    protected bool ClickHideFlyoutPredicate(IPopupHostProvider hostProvider, RawPointerEventArgs args)
    {
        if (hostProvider.PopupHost is OverlayPopupHost overlayPopupHost && args.Root is Control root)
        {
            var offset = overlayPopupHost.TranslatePoint(default, root);
            if (offset.HasValue)
            {
                var bounds = new Rect(offset.Value, overlayPopupHost.Bounds.Size);
                return !bounds.Contains(args.Position);
            }
        }
                
        return false;
    }
}