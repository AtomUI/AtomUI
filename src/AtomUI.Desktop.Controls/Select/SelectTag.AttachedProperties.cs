using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

// Concrete inheritable style inputs. Each public terminal supplies its own values in AXAML.
internal partial class SelectTag
{
    #region 公共属性定义

    public static readonly AttachedProperty<IBrush?> TagBackgroundProperty =
        AvaloniaProperty.RegisterAttached<SelectTag, StyledElement, IBrush?>("TagBackground", inherits: true);

    public static IBrush? GetTagBackground(StyledElement element) => element.GetValue(TagBackgroundProperty);
    public static void SetTagBackground(StyledElement element, IBrush? value) => element.SetValue(TagBackgroundProperty, value);

    public static readonly AttachedProperty<double> TagHeightProperty =
        AvaloniaProperty.RegisterAttached<SelectTag, StyledElement, double>("TagHeight", inherits: true);

    public static double GetTagHeight(StyledElement element) => element.GetValue(TagHeightProperty);
    public static void SetTagHeight(StyledElement element, double value) => element.SetValue(TagHeightProperty, value);

    public static readonly AttachedProperty<IBrush?> DisabledSelectorBackgroundProperty =
        AvaloniaProperty.RegisterAttached<SelectTag, StyledElement, IBrush?>("DisabledSelectorBackground", inherits: true);

    public static IBrush? GetDisabledSelectorBackground(StyledElement element) => element.GetValue(DisabledSelectorBackgroundProperty);
    public static void SetDisabledSelectorBackground(StyledElement element, IBrush? value) => element.SetValue(DisabledSelectorBackgroundProperty, value);

    public static readonly AttachedProperty<IBrush?> DisabledTagForegroundProperty =
        AvaloniaProperty.RegisterAttached<SelectTag, StyledElement, IBrush?>("DisabledTagForeground", inherits: true);

    public static IBrush? GetDisabledTagForeground(StyledElement element) => element.GetValue(DisabledTagForegroundProperty);
    public static void SetDisabledTagForeground(StyledElement element, IBrush? value) => element.SetValue(DisabledTagForegroundProperty, value);

    public static readonly AttachedProperty<double> OptionFontSizeProperty =
        AvaloniaProperty.RegisterAttached<SelectTag, StyledElement, double>("OptionFontSize", inherits: true);

    public static double GetOptionFontSize(StyledElement element) => element.GetValue(OptionFontSizeProperty);
    public static void SetOptionFontSize(StyledElement element, double value) => element.SetValue(OptionFontSizeProperty, value);

    public static readonly AttachedProperty<Thickness> MultipleSelectionPaddingProperty =
        AvaloniaProperty.RegisterAttached<SelectTag, StyledElement, Thickness>("MultipleSelectionPadding", inherits: true);

    public static Thickness GetMultipleSelectionPadding(StyledElement element) => element.GetValue(MultipleSelectionPaddingProperty);
    public static void SetMultipleSelectionPadding(StyledElement element, Thickness value) => element.SetValue(MultipleSelectionPaddingProperty, value);

    public static readonly AttachedProperty<Thickness> CompactMultipleSelectionPaddingProperty =
        AvaloniaProperty.RegisterAttached<SelectTag, StyledElement, Thickness>("CompactMultipleSelectionPadding", inherits: true);

    public static Thickness GetCompactMultipleSelectionPadding(StyledElement element) => element.GetValue(CompactMultipleSelectionPaddingProperty);
    public static void SetCompactMultipleSelectionPadding(StyledElement element, Thickness value) => element.SetValue(CompactMultipleSelectionPaddingProperty, value);

    public static readonly AttachedProperty<Thickness> MultipleSelectionPrefixIndentProperty =
        AvaloniaProperty.RegisterAttached<SelectTag, StyledElement, Thickness>("MultipleSelectionPrefixIndent", inherits: true);

    public static Thickness GetMultipleSelectionPrefixIndent(StyledElement element) => element.GetValue(MultipleSelectionPrefixIndentProperty);
    public static void SetMultipleSelectionPrefixIndent(StyledElement element, Thickness value) => element.SetValue(MultipleSelectionPrefixIndentProperty, value);

    public static readonly AttachedProperty<Thickness> SingleSelectionPaddingProperty =
        AvaloniaProperty.RegisterAttached<SelectTag, StyledElement, Thickness>("SingleSelectionPadding", inherits: true);

    public static Thickness GetSingleSelectionPadding(StyledElement element) => element.GetValue(SingleSelectionPaddingProperty);
    public static void SetSingleSelectionPadding(StyledElement element, Thickness value) => element.SetValue(SingleSelectionPaddingProperty, value);

    #endregion
}
