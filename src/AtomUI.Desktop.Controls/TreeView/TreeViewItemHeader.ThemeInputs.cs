using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

internal partial class TreeViewItemHeader
{
    #region 公共属性定义

    public static readonly AttachedProperty<double> ThemeHeaderHeightProperty =
        AvaloniaProperty.RegisterAttached<TreeViewItemHeader, StyledElement, double>("ThemeHeaderHeight", inherits: true);
    public static double GetThemeHeaderHeight(StyledElement element) => element.GetValue(ThemeHeaderHeightProperty);
    public static void SetThemeHeaderHeight(StyledElement element, double value) => element.SetValue(ThemeHeaderHeightProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeNodeHoverBgProperty =
        AvaloniaProperty.RegisterAttached<TreeViewItemHeader, StyledElement, IBrush?>("ThemeNodeHoverBg", inherits: true);
    public static IBrush? GetThemeNodeHoverBg(StyledElement element) => element.GetValue(ThemeNodeHoverBgProperty);
    public static void SetThemeNodeHoverBg(StyledElement element, IBrush? value) => element.SetValue(ThemeNodeHoverBgProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeNodeSelectedBgProperty =
        AvaloniaProperty.RegisterAttached<TreeViewItemHeader, StyledElement, IBrush?>("ThemeNodeSelectedBg", inherits: true);
    public static IBrush? GetThemeNodeSelectedBg(StyledElement element) => element.GetValue(ThemeNodeSelectedBgProperty);
    public static void SetThemeNodeSelectedBg(StyledElement element, IBrush? value) => element.SetValue(ThemeNodeSelectedBgProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeNodeSelectedColorProperty =
        AvaloniaProperty.RegisterAttached<TreeViewItemHeader, StyledElement, IBrush?>("ThemeNodeSelectedColor", inherits: true);
    public static IBrush? GetThemeNodeSelectedColor(StyledElement element) => element.GetValue(ThemeNodeSelectedColorProperty);
    public static void SetThemeNodeSelectedColor(StyledElement element, IBrush? value) => element.SetValue(ThemeNodeSelectedColorProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeTreeItemMarginProperty =
        AvaloniaProperty.RegisterAttached<TreeViewItemHeader, StyledElement, Thickness>("ThemeTreeItemMargin", inherits: true);
    public static Thickness GetThemeTreeItemMargin(StyledElement element) => element.GetValue(ThemeTreeItemMarginProperty);
    public static void SetThemeTreeItemMargin(StyledElement element, Thickness value) => element.SetValue(ThemeTreeItemMarginProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeTreeItemHeaderPaddingProperty =
        AvaloniaProperty.RegisterAttached<TreeViewItemHeader, StyledElement, Thickness>("ThemeTreeItemHeaderPadding", inherits: true);
    public static Thickness GetThemeTreeItemHeaderPadding(StyledElement element) => element.GetValue(ThemeTreeItemHeaderPaddingProperty);
    public static void SetThemeTreeItemHeaderPadding(StyledElement element, Thickness value) => element.SetValue(ThemeTreeItemHeaderPaddingProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeTreeItemHeaderMarginProperty =
        AvaloniaProperty.RegisterAttached<TreeViewItemHeader, StyledElement, Thickness>("ThemeTreeItemHeaderMargin", inherits: true);
    public static Thickness GetThemeTreeItemHeaderMargin(StyledElement element) => element.GetValue(ThemeTreeItemHeaderMarginProperty);
    public static void SetThemeTreeItemHeaderMargin(StyledElement element, Thickness value) => element.SetValue(ThemeTreeItemHeaderMarginProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeTreeNodeSwitcherMarginProperty =
        AvaloniaProperty.RegisterAttached<TreeViewItemHeader, StyledElement, Thickness>("ThemeTreeNodeSwitcherMargin", inherits: true);
    public static Thickness GetThemeTreeNodeSwitcherMargin(StyledElement element) => element.GetValue(ThemeTreeNodeSwitcherMarginProperty);
    public static void SetThemeTreeNodeSwitcherMargin(StyledElement element, Thickness value) => element.SetValue(ThemeTreeNodeSwitcherMarginProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeTreeNodeIconMarginProperty =
        AvaloniaProperty.RegisterAttached<TreeViewItemHeader, StyledElement, Thickness>("ThemeTreeNodeIconMargin", inherits: true);
    public static Thickness GetThemeTreeNodeIconMargin(StyledElement element) => element.GetValue(ThemeTreeNodeIconMarginProperty);
    public static void SetThemeTreeNodeIconMargin(StyledElement element, Thickness value) => element.SetValue(ThemeTreeNodeIconMarginProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeFilterHighlightColorProperty =
        AvaloniaProperty.RegisterAttached<TreeViewItemHeader, StyledElement, IBrush?>("ThemeFilterHighlightColor", inherits: true);
    public static IBrush? GetThemeFilterHighlightColor(StyledElement element) => element.GetValue(ThemeFilterHighlightColorProperty);
    public static void SetThemeFilterHighlightColor(StyledElement element, IBrush? value) => element.SetValue(ThemeFilterHighlightColorProperty, value);

    public static readonly AttachedProperty<double> ThemeDragIndicatorLineWidthProperty =
        AvaloniaProperty.RegisterAttached<TreeViewItemHeader, StyledElement, double>("ThemeDragIndicatorLineWidth", inherits: true);
    public static double GetThemeDragIndicatorLineWidth(StyledElement element) => element.GetValue(ThemeDragIndicatorLineWidthProperty);
    public static void SetThemeDragIndicatorLineWidth(StyledElement element, double value) => element.SetValue(ThemeDragIndicatorLineWidthProperty, value);

    #endregion
}
