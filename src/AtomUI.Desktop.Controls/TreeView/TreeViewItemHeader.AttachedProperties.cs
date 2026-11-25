using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

internal partial class TreeViewItemHeader
{
    #region 公共属性定义

    public static readonly AttachedProperty<double> HeaderHeightProperty =
        AvaloniaProperty.RegisterAttached<TreeViewItemHeader, StyledElement, double>("HeaderHeight", inherits: true);
    public static double GetHeaderHeight(StyledElement element) => element.GetValue(HeaderHeightProperty);
    public static void SetHeaderHeight(StyledElement element, double value) => element.SetValue(HeaderHeightProperty, value);

    public static readonly AttachedProperty<IBrush?> NodeHoverBackgroundProperty =
        AvaloniaProperty.RegisterAttached<TreeViewItemHeader, StyledElement, IBrush?>("NodeHoverBackground", inherits: true);
    public static IBrush? GetNodeHoverBackground(StyledElement element) => element.GetValue(NodeHoverBackgroundProperty);
    public static void SetNodeHoverBackground(StyledElement element, IBrush? value) => element.SetValue(NodeHoverBackgroundProperty, value);

    public static readonly AttachedProperty<IBrush?> NodeSelectedBackgroundProperty =
        AvaloniaProperty.RegisterAttached<TreeViewItemHeader, StyledElement, IBrush?>("NodeSelectedBackground", inherits: true);
    public static IBrush? GetNodeSelectedBackground(StyledElement element) => element.GetValue(NodeSelectedBackgroundProperty);
    public static void SetNodeSelectedBackground(StyledElement element, IBrush? value) => element.SetValue(NodeSelectedBackgroundProperty, value);

    public static readonly AttachedProperty<IBrush?> NodeSelectedForegroundProperty =
        AvaloniaProperty.RegisterAttached<TreeViewItemHeader, StyledElement, IBrush?>("NodeSelectedForeground", inherits: true);
    public static IBrush? GetNodeSelectedForeground(StyledElement element) => element.GetValue(NodeSelectedForegroundProperty);
    public static void SetNodeSelectedForeground(StyledElement element, IBrush? value) => element.SetValue(NodeSelectedForegroundProperty, value);

    public static readonly AttachedProperty<Thickness> ItemMarginProperty =
        AvaloniaProperty.RegisterAttached<TreeViewItemHeader, StyledElement, Thickness>("ItemMargin", inherits: true);
    public static Thickness GetItemMargin(StyledElement element) => element.GetValue(ItemMarginProperty);
    public static void SetItemMargin(StyledElement element, Thickness value) => element.SetValue(ItemMarginProperty, value);

    public static readonly AttachedProperty<Thickness> ItemHeaderPaddingProperty =
        AvaloniaProperty.RegisterAttached<TreeViewItemHeader, StyledElement, Thickness>("ItemHeaderPadding", inherits: true);
    public static Thickness GetItemHeaderPadding(StyledElement element) => element.GetValue(ItemHeaderPaddingProperty);
    public static void SetItemHeaderPadding(StyledElement element, Thickness value) => element.SetValue(ItemHeaderPaddingProperty, value);

    public static readonly AttachedProperty<Thickness> ItemHeaderMarginProperty =
        AvaloniaProperty.RegisterAttached<TreeViewItemHeader, StyledElement, Thickness>("ItemHeaderMargin", inherits: true);
    public static Thickness GetItemHeaderMargin(StyledElement element) => element.GetValue(ItemHeaderMarginProperty);
    public static void SetItemHeaderMargin(StyledElement element, Thickness value) => element.SetValue(ItemHeaderMarginProperty, value);

    public static readonly AttachedProperty<Thickness> NodeSwitcherMarginProperty =
        AvaloniaProperty.RegisterAttached<TreeViewItemHeader, StyledElement, Thickness>("NodeSwitcherMargin", inherits: true);
    public static Thickness GetNodeSwitcherMargin(StyledElement element) => element.GetValue(NodeSwitcherMarginProperty);
    public static void SetNodeSwitcherMargin(StyledElement element, Thickness value) => element.SetValue(NodeSwitcherMarginProperty, value);

    public static readonly AttachedProperty<Thickness> NodeIconMarginProperty =
        AvaloniaProperty.RegisterAttached<TreeViewItemHeader, StyledElement, Thickness>("NodeIconMargin", inherits: true);
    public static Thickness GetNodeIconMargin(StyledElement element) => element.GetValue(NodeIconMarginProperty);
    public static void SetNodeIconMargin(StyledElement element, Thickness value) => element.SetValue(NodeIconMarginProperty, value);

    public static readonly AttachedProperty<IBrush?> FilterHighlightBrushProperty =
        AvaloniaProperty.RegisterAttached<TreeViewItemHeader, StyledElement, IBrush?>("FilterHighlightBrush", inherits: true);
    public static IBrush? GetFilterHighlightBrush(StyledElement element) => element.GetValue(FilterHighlightBrushProperty);
    public static void SetFilterHighlightBrush(StyledElement element, IBrush? value) => element.SetValue(FilterHighlightBrushProperty, value);

    #endregion
}
