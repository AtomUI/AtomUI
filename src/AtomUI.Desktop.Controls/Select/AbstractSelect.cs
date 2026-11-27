using AtomUI.Animations;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Base class of controls that show the selected value in an input-like box and let the user pick an
/// option from a drop-down popup: <c>Select</c>, <c>TreeSelect</c> and <c>Cascader</c>. It owns the life
/// cycle of that popup (open and close events, width and height), the integration with a form item and
/// the membership in a compact group. A derived class provides the popup content in
/// <see cref="EnsurePopupContent" />.
/// </summary>
public abstract class AbstractSelect : TemplatedControl,
                                       IMotionAwareControl,
                                       ICustomizableSizeTypeAware,
                                       ICompactSpaceAware,
                                       IInputControlStatusAware,
                                       IInputControlStyleVariantAware,
                                       IFormItemAware,
                                       IFormItemFeedbackAware
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="OpenMotion" /> property.
    /// </summary>
    public static readonly StyledProperty<AbstractMotion?> OpenMotionProperty =
        Popup.OpenMotionProperty.AddOwner<AbstractSelect>();

    /// <summary>
    /// Identifies the <see cref="CloseMotion" /> property.
    /// </summary>
    public static readonly StyledProperty<AbstractMotion?> CloseMotionProperty =
        Popup.CloseMotionProperty.AddOwner<AbstractSelect>();
    /// <summary>
    /// Identifies the <see cref="IsAllowClear" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsAllowClearProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(IsAllowClear));

    /// <summary>
    /// Identifies the <see cref="IsAutoClearSearchValue" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsAutoClearSearchValueProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(IsAutoClearSearchValue));

    /// <summary>
    /// Identifies the <see cref="IsDefaultOpen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDefaultOpenProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(IsDefaultOpen));

    /// <summary>
    /// Identifies the <see cref="IsDropDownOpen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(IsDropDownOpen));

    /// <summary>
    /// Identifies the <see cref="PlaceholderText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        AvaloniaProperty.Register<AbstractSelect, string?>(nameof(PlaceholderText));

    /// <summary>
    /// Identifies the <see cref="PlaceholderForeground" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> PlaceholderForegroundProperty =
        AvaloniaProperty.Register<AbstractSelect, IBrush?>(nameof(PlaceholderForeground));

    /// <summary>
    /// Identifies the <see cref="IsPopupMatchSelectWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPopupMatchSelectWidthProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(IsPopupMatchSelectWidth), true);

    /// <summary>
    /// Identifies the <see cref="ShouldUseOverlayPopup" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShouldUseOverlayPopupProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(ShouldUseOverlayPopup), true);

    /// <summary>
    /// Identifies the <see cref="IsFilterEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsFilterEnabledProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(IsFilterEnabled));

    /// <summary>
    /// Identifies the <see cref="IsShowOverflowTip" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowOverflowTipProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(IsShowOverflowTip), true);

    /// <summary>
    /// Identifies the <see cref="OverflowTipDelay" /> property.
    /// </summary>
    public static readonly StyledProperty<int> OverflowTipDelayProperty =
        AvaloniaProperty.Register<AbstractSelect, int>(nameof(OverflowTipDelay), 1200);

    /// <summary>
    /// Identifies the <see cref="OverflowTipPlacement" /> property.
    /// </summary>
    public static readonly StyledProperty<PlacementMode> OverflowTipPlacementProperty =
        AvaloniaProperty.Register<AbstractSelect, PlacementMode>(nameof(OverflowTipPlacement), PlacementMode.TopEdgeAlignedLeft);

    /// <summary>
    /// Identifies the <see cref="DisplayPageSize" /> property.
    /// </summary>
    public static readonly StyledProperty<int> DisplayPageSizeProperty =
        AvaloniaProperty.Register<AbstractSelect, int>(nameof (DisplayPageSize), 10);

    /// <summary>
    /// Identifies the <see cref="MaxCount" /> property.
    /// </summary>
    public static readonly StyledProperty<int> MaxCountProperty =
        AvaloniaProperty.Register<AbstractSelect, int>(nameof(MaxCount), int.MaxValue);

    /// <summary>
    /// Identifies the <see cref="IsShowMaxCountIndicator" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowMaxCountIndicatorProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(IsShowMaxCountIndicator));

    /// <summary>
    /// Identifies the <see cref="MaxTagCount" /> property.
    /// </summary>
    public static readonly StyledProperty<int?> MaxTagCountProperty =
        AvaloniaProperty.Register<AbstractSelect, int?>(nameof(MaxTagCount));

    /// <summary>
    /// Identifies the <see cref="IsResponsiveTagMode" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsResponsiveTagModeProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(IsResponsiveTagMode));

    /// <summary>
    /// Identifies the <see cref="MaxTagPlaceholder" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> MaxTagPlaceholderProperty =
        AvaloniaProperty.Register<AbstractSelect, string?>(nameof(MaxTagPlaceholder));

    /// <summary>
    /// Identifies the <see cref="LeftAddOn" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> LeftAddOnProperty =
        AddOnDecoratedBox.LeftAddOnProperty.AddOwner<AbstractSelect>();

    /// <summary>
    /// Identifies the <see cref="LeftAddOnTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> LeftAddOnTemplateProperty =
        AddOnDecoratedBox.LeftAddOnTemplateProperty.AddOwner<AbstractSelect>();

    /// <summary>
    /// Identifies the <see cref="RightAddOn" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> RightAddOnProperty =
        AddOnDecoratedBox.RightAddOnProperty.AddOwner<AbstractSelect>();

    /// <summary>
    /// Identifies the <see cref="RightAddOnTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> RightAddOnTemplateProperty =
        AddOnDecoratedBox.RightAddOnTemplateProperty.AddOwner<AbstractSelect>();

    /// <summary>
    /// Identifies the <see cref="ContentLeftAddOn" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ContentLeftAddOnProperty =
        AddOnDecoratedBox.ContentLeftAddOnProperty.AddOwner<AbstractSelect>();

    /// <summary>
    /// Identifies the <see cref="ContentLeftAddOnTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ContentLeftAddOnTemplateProperty =
        AddOnDecoratedBox.ContentLeftAddOnTemplateProperty.AddOwner<AbstractSelect>();

    /// <summary>
    /// Identifies the <see cref="ContentRightAddOn" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ContentRightAddOnProperty =
        AddOnDecoratedBox.ContentRightAddOnProperty.AddOwner<AbstractSelect>();

    /// <summary>
    /// Identifies the <see cref="ContentRightAddOnTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ContentRightAddOnTemplateProperty =
        AddOnDecoratedBox.ContentRightAddOnTemplateProperty.AddOwner<AbstractSelect>();

    /// <summary>
    /// Identifies the <see cref="StyleVariant" /> property.
    /// </summary>
    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<AbstractSelect>();

    /// <summary>
    /// Identifies the <see cref="Status" /> property.
    /// </summary>
    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<AbstractSelect>();

    /// <summary>
    /// Identifies the <see cref="Placement" /> property.
    /// </summary>
    public static readonly StyledProperty<SelectPopupPlacement> PlacementProperty =
        AvaloniaProperty.Register<AbstractSelect, SelectPopupPlacement>(nameof(Placement), SelectPopupPlacement.BottomEdgeAlignedLeft);

    /// <summary>
    /// Identifies the <see cref="FilterValue" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> FilterValueProperty =
        AvaloniaProperty.Register<AbstractSelect, object?>(nameof(FilterValue));

    /// <summary>
    /// Identifies the <see cref="ClearIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> ClearIconProperty =
        AvaloniaProperty.Register<AbstractSelect, PathIcon?>(nameof(ClearIcon));

    /// <summary>
    /// Identifies the <see cref="SuffixIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> SuffixIconProperty =
        AvaloniaProperty.Register<AbstractSelect, PathIcon?>(nameof(SuffixIcon));

    /// <summary>
    /// Identifies the <see cref="SuffixLoadingIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> SuffixLoadingIconProperty =
        AvaloniaProperty.Register<AbstractSelect, PathIcon?>(nameof(SuffixLoadingIcon));

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractSelect>();

    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractSelect>();

    /// <summary>
    /// Identifies the <see cref="EmptyIndicator" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> EmptyIndicatorProperty =
        AvaloniaProperty.Register<AbstractSelect, object?>(nameof(EmptyIndicator));

    /// <summary>
    /// Identifies the <see cref="EmptyIndicatorTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> EmptyIndicatorTemplateProperty =
        AvaloniaProperty.Register<AbstractSelect, IDataTemplate?>(nameof(EmptyIndicatorTemplate));

    /// <summary>
    /// Identifies the <see cref="IsShowEmptyIndicator" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowEmptyIndicatorProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(IsShowEmptyIndicator), true);

    /// <summary>
    /// Identifies the <see cref="EmptyIndicatorPadding" /> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> EmptyIndicatorPaddingProperty =
        AvaloniaProperty.Register<AbstractSelect, Thickness>(nameof(EmptyIndicatorPadding));

    /// <summary>
    /// Identifies the <see cref="IsLoading" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractSelect, bool> IsLoadingProperty =
        AvaloniaProperty.RegisterDirect<AbstractSelect, bool>(
            nameof(IsLoading),
            o => o.IsLoading);

    /// <summary>
    /// Motion played on the drop-down when it opens, or <see langword="null" /> to show it without an entry
    /// animation. The value is passed to the popup; the theme sets <c>PopupMotionDefaults.SelectOpen</c>.
    /// </summary>
    public AbstractMotion? OpenMotion
    {
        get => GetValue(OpenMotionProperty);
        set => SetValue(OpenMotionProperty, value);
    }

    /// <summary>
    /// Motion played on the drop-down before it closes, or <see langword="null" /> to close it without an exit
    /// animation. While it plays, the close is put off. The value is passed to the popup; the theme sets
    /// <c>PopupMotionDefaults.SelectClose</c>.
    /// </summary>
    public AbstractMotion? CloseMotion
    {
        get => GetValue(CloseMotionProperty);
        set => SetValue(CloseMotionProperty, value);
    }

    /// <summary>
    /// Shows a clear button in the suffix handle when <see langword="true" /> and the selection is not empty.
    /// While the pointer hovers or presses the input, the button takes the place of the open indicator.
    /// </summary>
    public bool IsAllowClear
    {
        get => GetValue(IsAllowClearProperty);
        set => SetValue(IsAllowClearProperty, value);
    }

    /// <summary>
    /// Intended to decide whether the search text is cleared after an option is selected. Nothing in this
    /// library reads this property, so setting it has no effect.
    /// </summary>
    public bool IsAutoClearSearchValue
    {
        get => GetValue(IsAutoClearSearchValueProperty);
        set => SetValue(IsAutoClearSearchValueProperty, value);
    }

    /// <summary>
    /// Intended to open the drop-down by default when the control is shown. Nothing in this library reads
    /// this property, so setting it has no effect.
    /// </summary>
    public bool IsDefaultOpen
    {
        get => GetValue(IsDefaultOpenProperty);
        set => SetValue(IsDefaultOpenProperty, value);
    }

    /// <summary>
    /// Whether the drop-down is open. Setting <see langword="true" /> raises <see cref="DropDownOpening" /> and
    /// then opens the popup; setting <see langword="false" /> raises <see cref="DropDownClosing" /> and then
    /// closes it. The control sets it to <see langword="false" /> by itself when the window deactivates, when
    /// the control becomes invisible and after the popup closed by light dismiss. While
    /// <see cref="IsPopupPinnedOpen" /> is <see langword="true" />, a change to <see langword="false" /> is
    /// reverted.
    /// </summary>
    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    /// <summary>
    /// Text shown in the selection area while nothing is selected. In single mode it is also the placeholder
    /// of the filter input. <see langword="null" /> shows no text.
    /// </summary>
    public string? PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    /// <summary>
    /// Foreground of the placeholder text in the selection area and of the placeholder of the filter input.
    /// When the value is not set, the theme uses the <c>ColorTextPlaceholder</c> token.
    /// </summary>
    public IBrush? PlaceholderForeground
    {
        get => GetValue(PlaceholderForegroundProperty);
        set => SetValue(PlaceholderForegroundProperty, value);
    }

    /// <summary>
    /// When <see langword="true" />, the drop-down is at least as wide as the control; the minimum width is
    /// applied from the control width each time the popup opens. When <see langword="false" />, the drop-down
    /// has no minimum width and follows the width of its content. The default is <see langword="true" />.
    /// </summary>
    public bool IsPopupMatchSelectWidth
    {
        get => GetValue(IsPopupMatchSelectWidthProperty);
        set => SetValue(IsPopupMatchSelectWidthProperty, value);
    }

    /// <summary>
    /// When <see langword="true" />, the drop-down is hosted in the overlay layer of the top level, so that
    /// the ancestors of the control cannot clip it. When <see langword="false" />, it stays in the visual tree
    /// of the control. The value is passed to the popup. The default is <see langword="true" />.
    /// </summary>
    public bool ShouldUseOverlayPopup
    {
        get => GetValue(ShouldUseOverlayPopupProperty);
        set => SetValue(ShouldUseOverlayPopupProperty, value);
    }

    /// <summary>
    /// When <see langword="true" />, the user can type a search text that filters the candidate list while
    /// the drop-down is open, and the handle shows the search indicator instead of the open arrow while it is
    /// open. When <see langword="false" />, the input shows a hand cursor. <c>Select</c> passes an effective
    /// value that it derives from this property on to its parts.
    /// </summary>
    public bool IsFilterEnabled
    {
        get => GetValue(IsFilterEnabledProperty);
        set => SetValue(IsFilterEnabledProperty, value);
    }

    /// <summary>
    /// When <see langword="true" />, a trimmed selected tag or a trimmed text in single mode shows a tip with
    /// the full text when the pointer rests on it. The value is passed to the tags and to the filter input.
    /// The default is <see langword="true" />.
    /// </summary>
    public bool IsShowOverflowTip
    {
        get => GetValue(IsShowOverflowTipProperty);
        set => SetValue(IsShowOverflowTipProperty, value);
    }

    /// <summary>
    /// Time in milliseconds that the pointer must rest on a trimmed text before the tip appears. The default
    /// is 1200.
    /// </summary>
    public int OverflowTipDelay
    {
        get => GetValue(OverflowTipDelayProperty);
        set => SetValue(OverflowTipDelayProperty, value);
    }

    /// <summary>
    /// Placement of the tip relative to the trimmed element. The default is
    /// <c>PlacementMode.TopEdgeAlignedLeft</c>.
    /// </summary>
    public PlacementMode OverflowTipPlacement
    {
        get => GetValue(OverflowTipPlacementProperty);
        set => SetValue(OverflowTipPlacementProperty, value);
    }

    /// <summary>
    /// Number of option rows that the drop-down can show. Its maximum height is this number times the row
    /// height plus the vertical padding of the popup. As long as this property is not set explicitly and the
    /// popup layout tokens supply a default height, that default height is used instead; assigning any value,
    /// even the current one, makes the property explicit. The default is 10.
    /// </summary>
    public int DisplayPageSize
    {
        get => GetValue(DisplayPageSizeProperty);
        set
        {
            var becomesExplicit = _defaultPopupHeight.HasValue && !IsSet(DisplayPageSizeProperty) && value == DisplayPageSize;
            SetValue(DisplayPageSizeProperty, value);
            if (becomesExplicit)
            {
                ConfigureMaxDropdownHeight();
            }
        }
    }

    /// <summary>
    /// Maximum number of options that can be selected when several options are allowed. Further options
    /// cannot be selected once the limit is reached. <see cref="int.MaxValue" /> means no limit and is the
    /// default.
    /// </summary>
    public int MaxCount
    {
        get => GetValue(MaxCountProperty);
        set => SetValue(MaxCountProperty, value);
    }

    /// <summary>
    /// When <see langword="true" />, the suffix area shows how many options are selected out of
    /// <see cref="MaxCount" />.
    /// </summary>
    public bool IsShowMaxCountIndicator
    {
        get => GetValue(IsShowMaxCountIndicatorProperty);
        set => SetValue(IsShowMaxCountIndicatorProperty, value);
    }

    /// <summary>
    /// Maximum number of selected tags shown in the selection area; the tags beyond it are replaced by a
    /// summary tag that states how many tags are hidden. In responsive mode it is the maximum number of tags
    /// that the width-aware panel displays. <see langword="null" /> shows every tag.
    /// </summary>
    public int? MaxTagCount
    {
        get => GetValue(MaxTagCountProperty);
        set => SetValue(MaxTagCountProperty, value);
    }

    /// <summary>
    /// When <see langword="true" />, the selected tags are arranged by a panel that measures them against the
    /// available width, so that tags that do not fit are hidden. When <see langword="false" />, the tags are
    /// arranged by the default wrap panel.
    /// </summary>
    public bool IsResponsiveTagMode
    {
        get => GetValue(IsResponsiveTagModeProperty);
        set => SetValue(IsResponsiveTagModeProperty, value);
    }

    /// <summary>
    /// Text meant to stand in for the tags that the responsive tag layout hides because the width is not
    /// sufficient. Nothing in this library reads this property, so setting it has no effect.
    /// </summary>
    public string? MaxTagPlaceholder
    {
        get => GetValue(MaxTagPlaceholderProperty);
        set => SetValue(MaxTagPlaceholderProperty, value);
    }

    /// <summary>
    /// Content of the add-on attached to the left of the input, outside the input border. The value is passed
    /// to the add-on decorated box of the template.
    /// </summary>
    public object? LeftAddOn
    {
        get => GetValue(LeftAddOnProperty);
        set => SetValue(LeftAddOnProperty, value);
    }

    /// <summary>
    /// Template used to present <see cref="LeftAddOn" />. When it is <see langword="null" />, the presenter
    /// falls back to its own data templates.
    /// </summary>
    public IDataTemplate? LeftAddOnTemplate
    {
        get => GetValue(LeftAddOnTemplateProperty);
        set => SetValue(LeftAddOnTemplateProperty, value);
    }

    /// <summary>
    /// Content of the add-on attached to the right of the input, outside the input border. The value is
    /// passed to the add-on decorated box of the template.
    /// </summary>
    public object? RightAddOn
    {
        get => GetValue(RightAddOnProperty);
        set => SetValue(RightAddOnProperty, value);
    }

    /// <summary>
    /// Template used to present <see cref="RightAddOn" />. When it is <see langword="null" />, the presenter
    /// falls back to its own data templates.
    /// </summary>
    public IDataTemplate? RightAddOnTemplate
    {
        get => GetValue(RightAddOnTemplateProperty);
        set => SetValue(RightAddOnTemplateProperty, value);
    }

    /// <summary>
    /// Content shown inside the input, to the left of the selection area, like a prefix. The value is passed
    /// to the add-on decorated box of the template.
    /// </summary>
    public object? ContentLeftAddOn
    {
        get => GetValue(ContentLeftAddOnProperty);
        set => SetValue(ContentLeftAddOnProperty, value);
    }

    /// <summary>
    /// Template used to present <see cref="ContentLeftAddOn" />. When it is <see langword="null" />, the
    /// presenter falls back to its own data templates.
    /// </summary>
    public IDataTemplate? ContentLeftAddOnTemplate
    {
        get => GetValue(ContentLeftAddOnTemplateProperty);
        set => SetValue(ContentLeftAddOnTemplateProperty, value);
    }

    /// <summary>
    /// Content shown inside the input between the selection area and the count indicator and handle, like a
    /// suffix. The value is passed to the add-on decorated box of the template.
    /// </summary>
    public object? ContentRightAddOn
    {
        get => GetValue(ContentRightAddOnProperty);
        set => SetValue(ContentRightAddOnProperty, value);
    }

    /// <summary>
    /// Template used to present <see cref="ContentRightAddOn" />. When it is <see langword="null" />, the
    /// presenter falls back to its own data templates.
    /// </summary>
    public IDataTemplate? ContentRightAddOnTemplate
    {
        get => GetValue(ContentRightAddOnTemplateProperty);
        set => SetValue(ContentRightAddOnTemplateProperty, value);
    }

    /// <summary>
    /// Visual variant of the input: <c>Outlined</c>, <c>Filled</c> or <c>Borderless</c>. The value sets the
    /// matching pseudo-class on the control and is passed to the add-on decorated box of the template.
    /// </summary>
    public InputControlStyleVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }

    /// <summary>
    /// Status that the input shows through its border, such as <c>Error</c> or <c>Warning</c>. A validation
    /// error registered through <c>DataValidationErrors</c> and the status reported by the owning form item
    /// take precedence over this value.
    /// </summary>
    public InputControlStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>
    /// Placement of the drop-down relative to the control. The value is applied to the popup when the control
    /// initializes and whenever it changes. The default is <c>BottomEdgeAlignedLeft</c>.
    /// </summary>
    public SelectPopupPlacement Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    /// <summary>
    /// Current search text used to filter the candidate list, or <see langword="null" /> when there is none.
    /// The control writes it while the user types in the filter input.
    /// </summary>
    public object? FilterValue
    {
        get => GetValue(FilterValueProperty);
        set => SetValue(FilterValueProperty, value);
    }

    /// <summary>
    /// Icon intended for the clear button of the handle. Nothing in this library reads this property; the
    /// clear button does not take its icon from it.
    /// </summary>
    public PathIcon? ClearIcon
    {
        get => GetValue(ClearIconProperty);
        set => SetValue(ClearIconProperty, value);
    }

    /// <summary>
    /// Icon shown in the handle as the open indicator. If it is <see langword="null" /> at initialization, a
    /// down arrow is applied as the current value.
    /// </summary>
    public PathIcon? SuffixIcon
    {
        get => GetValue(SuffixIconProperty);
        set => SetValue(SuffixIconProperty, value);
    }

    /// <summary>
    /// Icon shown in the handle while <see cref="IsLoading" /> is <see langword="true" />. If it is
    /// <see langword="null" /> at initialization, a spinning loading icon is applied as the current value.
    /// </summary>
    public PathIcon? SuffixLoadingIcon
    {
        get => GetValue(SuffixLoadingIconProperty);
        set => SetValue(SuffixLoadingIconProperty, value);
    }

    /// <summary>
    /// When <see langword="true" />, the open and close motion of the drop-down and the animations of the
    /// input box and the handle are played. The value is passed to the popup and to the template parts. When
    /// the value is not set, the theme uses the <c>EnableMotion</c> token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// Size of the control: <c>Small</c>, <c>Middle</c>, <c>Large</c> or <c>Custom</c>. It selects the font
    /// size and the metrics of the selected tags and is passed to the input box. With <c>Custom</c>, the
    /// height comes from <c>Height</c> of the control.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// Content shown in the drop-down when the candidate list is empty. While this and
    /// <see cref="EmptyIndicatorTemplate" /> are both <see langword="null" />, the list shows its default
    /// empty indicator.
    /// </summary>
    [DependsOn(nameof(EmptyIndicatorTemplate))]
    public object? EmptyIndicator
    {
        get => GetValue(EmptyIndicatorProperty);
        set => SetValue(EmptyIndicatorProperty, value);
    }

    /// <summary>
    /// Template used to present <see cref="EmptyIndicator" /> in the drop-down.
    /// </summary>
    public IDataTemplate? EmptyIndicatorTemplate
    {
        get => GetValue(EmptyIndicatorTemplateProperty);
        set => SetValue(EmptyIndicatorTemplateProperty, value);
    }

    /// <summary>
    /// When <see langword="true" />, the drop-down shows an empty indicator when the data source is empty or
    /// no option is visible. When <see langword="false" />, nothing is shown then. The default is
    /// <see langword="true" />.
    /// </summary>
    public bool IsShowEmptyIndicator
    {
        get => GetValue(IsShowEmptyIndicatorProperty);
        set => SetValue(IsShowEmptyIndicatorProperty, value);
    }

    /// <summary>
    /// Padding intended for the empty indicator of the drop-down. Nothing in this library reads this
    /// property, so setting it has no effect.
    /// </summary>
    public Thickness EmptyIndicatorPadding
    {
        get => GetValue(EmptyIndicatorPaddingProperty);
        set => SetValue(EmptyIndicatorPaddingProperty, value);
    }

    private bool _isLoading;

    /// <summary>
    /// When <see langword="true" />, options are being loaded and the handle shows the loading icon in place
    /// of the open indicator. Only a derived class can set it.
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        protected set => SetAndRaise(IsLoadingProperty, ref _isLoading, value);
    }
    #endregion

    #region 公共事件定义

    /// <summary>
    /// Raised after <see cref="IsDropDownOpen" /> changed to <see langword="true" /> and before the popup
    /// opens. Setting <see cref="CancelEventArgs.Cancel" /> keeps the drop-down closed and restores the
    /// previous value of the property.
    /// </summary>
    public event EventHandler<CancelEventArgs>? DropDownOpening;
    /// <summary>
    /// Raised after the drop-down popup opened.
    /// </summary>
    public event EventHandler? DropDownOpened;
    /// <summary>
    /// Raised after <see cref="IsDropDownOpen" /> changed to <see langword="false" />. For a change made
    /// through the property the popup is still open, and setting <see cref="CancelEventArgs.Cancel" />
    /// restores the previous property value and keeps the drop-down open. When the popup closed by light
    /// dismiss, the event is raised by the state write-back after the popup closed; cancelling then only
    /// restores the property value.
    /// </summary>
    public event EventHandler<CancelEventArgs>? DropDownClosing;
    /// <summary>
    /// Raised after the drop-down popup has closed.
    /// </summary>
    public event EventHandler? DropDownClosed;

    #endregion

    #region 内部属性定义


    internal static readonly StyledProperty<CandidatePopupLayoutTokenResourceKeys?> PopupLayoutTokenResourceKeysProperty =
        AvaloniaProperty.Register<AbstractSelect, CandidatePopupLayoutTokenResourceKeys?>(nameof(PopupLayoutTokenResourceKeys));

    internal static readonly DirectProperty<AbstractSelect, double> MaxPopupHeightProperty =
        AvaloniaProperty.RegisterDirect<AbstractSelect, double>(
            nameof(MaxPopupHeight),
            o => o.MaxPopupHeight,
            (o, v) => o.MaxPopupHeight = v);

    internal static readonly DirectProperty<AbstractSelect, Thickness> PopupContentPaddingProperty =
        AvaloniaProperty.RegisterDirect<AbstractSelect, Thickness>(nameof(PopupContentPadding),
            o => o.PopupContentPadding,
            (o, v) => o.PopupContentPadding = v);

    internal static readonly DirectProperty<AbstractSelect, bool> IsEffectiveShowClearButtonProperty =
        AvaloniaProperty.RegisterDirect<AbstractSelect, bool>(nameof(IsEffectiveShowClearButton),
            o => o.IsEffectiveShowClearButton,
            (o, v) => o.IsEffectiveShowClearButton = v);

    internal static readonly DirectProperty<AbstractSelect, double> EffectivePopupWidthProperty =
        AvaloniaProperty.RegisterDirect<AbstractSelect, double>(
            nameof(EffectivePopupWidth),
            o => o.EffectivePopupWidth,
            (o, v) => o.EffectivePopupWidth = v);

    internal static readonly DirectProperty<AbstractSelect, bool> IsPlaceholderTextVisibleProperty =
        AvaloniaProperty.RegisterDirect<AbstractSelect, bool>(
            nameof(IsPlaceholderTextVisible),
            o => o.IsPlaceholderTextVisible,
            (o, v) => o.IsPlaceholderTextVisible = v);

    internal static readonly DirectProperty<AbstractSelect, bool> IsSingleResultVisibleProperty =
        AvaloniaProperty.RegisterDirect<AbstractSelect, bool>(
            nameof(IsSingleResultVisible),
            o => o.IsSingleResultVisible,
            (o, v) => o.IsSingleResultVisible = v);

    internal static readonly DirectProperty<AbstractSelect, bool> IsSelectionEmptyProperty =
        AvaloniaProperty.RegisterDirect<AbstractSelect, bool>(
            nameof(IsSelectionEmpty),
            o => o.IsSelectionEmpty,
            (o, v) => o.IsSelectionEmpty = v);

    internal static readonly DirectProperty<AbstractSelect, int> SelectedCountProperty =
        AvaloniaProperty.RegisterDirect<AbstractSelect, int>(nameof(SelectedCount),
            o => o.SelectedCount,
            (o, v) => o.SelectedCount = v);

    internal static readonly DirectProperty<AbstractSelect, PlacementMode> PopupPlacementProperty =
        AvaloniaProperty.RegisterDirect<AbstractSelect, PlacementMode>(
            nameof(PopupPlacement),
            o => o.PopupPlacement,
            (o, v) => o.PopupPlacement = v);

    internal static readonly StyledProperty<SpaceItemPosition?> CompactSpaceItemPositionProperty =
        CompactSpaceAwareControlProperty.CompactSpaceItemPositionProperty.AddOwner<AbstractSelect>();

    internal static readonly StyledProperty<Orientation> CompactSpaceOrientationProperty =
        CompactSpaceAwareControlProperty.CompactSpaceOrientationProperty.AddOwner<AbstractSelect>();

    internal static readonly StyledProperty<bool> IsUsedInCompactSpaceProperty =
        CompactSpaceAwareControlProperty.IsUsedInCompactSpaceProperty.AddOwner<AbstractSelect>();

    internal static readonly StyledProperty<FormValidateStatus> FormStatusProperty =
        InputControlState.FormStatusProperty.AddOwner<AbstractSelect>();

    internal static readonly StyledProperty<FormValidateFeedback?> FormFeedbackProperty =
        AvaloniaProperty.Register<AbstractSelect, FormValidateFeedback?>(nameof(FormFeedback));

    /// <summary>
    /// Identifies the <see cref="IsPopupPinnedOpen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        Popup.IsPopupPinnedOpenProperty.AddOwner<AbstractSelect>();

    internal CandidatePopupLayoutTokenResourceKeys? PopupLayoutTokenResourceKeys
    {
        get => GetValue(PopupLayoutTokenResourceKeysProperty);
        set => SetValue(PopupLayoutTokenResourceKeysProperty, value);
    }

    private OwnerTokenSet? _popupLayoutTokens;
    private double _itemHeight;
    private double? _defaultPopupHeight;
    private double _maxPopupHeight;

    internal double MaxPopupHeight
    {
        get => _maxPopupHeight;
        set => SetAndRaise(MaxPopupHeightProperty, ref _maxPopupHeight, value);
    }

    private Thickness _popupContentPadding;

    internal Thickness PopupContentPadding
    {
        get => _popupContentPadding;
        set => SetAndRaise(PopupContentPaddingProperty, ref _popupContentPadding, value);
    }

    private bool _isEffectiveShowClearButton;

    internal bool IsEffectiveShowClearButton
    {
        get => _isEffectiveShowClearButton;
        set => SetAndRaise(IsEffectiveShowClearButtonProperty, ref _isEffectiveShowClearButton, value);
    }

    private double _effectivePopupWidth;

    internal double EffectivePopupWidth
    {
        get => _effectivePopupWidth;
        set => SetAndRaise(EffectivePopupWidthProperty, ref _effectivePopupWidth, value);
    }

    private bool _isPlaceholderTextVisible;

    internal bool IsPlaceholderTextVisible
    {
        get => _isPlaceholderTextVisible;
        set => SetAndRaise(IsPlaceholderTextVisibleProperty, ref _isPlaceholderTextVisible, value);
    }

    private bool _isSingleResultVisible;

    internal bool IsSingleResultVisible
    {
        get => _isSingleResultVisible;
        set => SetAndRaise(IsSingleResultVisibleProperty, ref _isSingleResultVisible, value);
    }

    private bool _isSelectionEmpty = true;

    internal bool IsSelectionEmpty
    {
        get => _isSelectionEmpty;
        set => SetAndRaise(IsSelectionEmptyProperty, ref _isSelectionEmpty, value);
    }

    private int _selectedCount;

    internal int SelectedCount
    {
        get => _selectedCount;
        set => SetAndRaise(SelectedCountProperty, ref _selectedCount, value);
    }

    private PlacementMode _popupPlacement = PlacementMode.BottomEdgeAlignedLeft;

    internal PlacementMode PopupPlacement
    {
        get => _popupPlacement;
        set => SetAndRaise(PopupPlacementProperty, ref _popupPlacement, value);
    }

    internal SpaceItemPosition? CompactSpaceItemPosition
    {
        get => GetValue(CompactSpaceItemPositionProperty);
        set => SetValue(CompactSpaceItemPositionProperty, value);
    }

    internal Orientation CompactSpaceOrientation
    {
        get => GetValue(CompactSpaceOrientationProperty);
        set => SetValue(CompactSpaceOrientationProperty, value);
    }

    internal bool IsUsedInCompactSpace
    {
        get => GetValue(IsUsedInCompactSpaceProperty);
        set => SetValue(IsUsedInCompactSpaceProperty, value);
    }

    internal FormValidateStatus FormStatus
    {
        get => GetValue(FormStatusProperty);
        private set => SetCurrentValue(FormStatusProperty, value);
    }

    internal FormValidateFeedback? FormFeedback
    {
        get => GetValue(FormFeedbackProperty);
        set => SetValue(FormFeedbackProperty, value);
    }

    /// <summary>
    /// Keeps the drop-down open while <see langword="true" />: the popup ignores light-dismiss close requests,
    /// and a change of <see cref="IsDropDownOpen" /> to <see langword="false" /> is reverted without raising
    /// <see cref="DropDownClosing" />. Setting it to <see langword="true" /> opens the drop-down when it is
    /// closed. The value is relayed to the popup. It serves previews that must keep the popup on screen.
    /// </summary>
    public bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }
    #endregion

    private protected readonly CompositeDisposable SubscriptionsOnOpen = new ();
    private protected Popup? Popup;
    private protected bool PopupHasOpened;
    private protected bool IgnorePropertyChange;
    private AddOnDecoratedBox? _addOnDecoratedBox;

    private IDisposable? _deactivationSubscription;
    private IDisposable? _popupPinnedOpenBinding;
    private EventHandler? _formValueChanged;

    static AbstractSelect()
    {
        AffectsArrange<AbstractSelect>(CompactSpaceItemPositionProperty, CompactSpaceOrientationProperty);
        IsDropDownOpenProperty.Changed.AddClassHandler<AbstractSelect>((select, args) => select.HandleIsDropDownOpenChanged(args));
    }

    /// <summary>
    /// Applies <see cref="Placement" /> to the popup and, when <see cref="SuffixIcon" /> or
    /// <see cref="SuffixLoadingIcon" /> is <see langword="null" />, sets a default icon as the current value.
    /// An override must call the base member to keep this setup.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        ConfigurePopupPlacement();
        if (SuffixIcon == null)
        {
            SetCurrentValue(SuffixIconProperty, new DownOutlined());
        }

        if (SuffixLoadingIcon == null)
        {
            SetCurrentValue(SuffixLoadingIconProperty, new LoadingOutlined()
            {
                LoadingAnimation = IconAnimation.Spin
            });
        }
    }

    /// <summary>
    /// Subscribes to the deactivation of the top level, so that the drop-down closes when its window
    /// deactivates, and opens the popup again while the drop-down is pinned open. An override must call the
    /// base member to keep both.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _deactivationSubscription =
            TopLevelDeactivation.Subscribe(TopLevel.GetTopLevel(this), HandleWindowDeactivated);
        if (IsPopupPinnedOpen && IsDropDownOpen)
        {
            OpenDropDown();
        }
    }

    /// <summary>
    /// Disposes the subscription to the deactivation of the top level. An override must call the base member
    /// so that the subscription is released.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _deactivationSubscription?.Dispose();
        _deactivationSubscription = null;
    }

    /// <summary>
    /// Takes the popup and the add-on decorated box from the applied template. It detaches the handlers from
    /// the popup of a previous template, relays <see cref="IsPopupPinnedOpen" /> to the new popup, subscribes
    /// to its open and close events and recalculates the maximum drop-down height. When
    /// <see cref="IsDropDownOpen" /> is already <see langword="true" />, the new popup is opened through the
    /// opening sequence. An override must call the base member first.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ConfigureMaxDropdownHeight();
        if (Popup != null)
        {
            _popupPinnedOpenBinding?.Dispose();
            _popupPinnedOpenBinding = null;
            Popup.Opened -= PopupOpened;
            Popup.Closed -= PopupClosed;
            Popup.OverlayInputPassThroughElement = null;
        }

        Popup = e.NameScope.Find<Popup>("PART_Popup");
        _addOnDecoratedBox = e.NameScope.Find<AddOnDecoratedBox>(AddOnDecoratedBox.AddOnDecoratedBoxPart);
        // Root BorderBrush / Background reach the input frame through the template's HostBorderBrush / HostBackground
        // bindings, which the frame theme projects over its state brushes (antd styles.root).

        if (Popup != null)
        {
            _popupPinnedOpenBinding = BindUtils.RelayBind(
                this,
                IsPopupPinnedOpenProperty,
                Popup,
                Popup.IsPopupPinnedOpenProperty);
            Popup.Opened += PopupOpened;
            Popup.Closed += PopupClosed;
        }

        // If the drop down property indicates that the popup is open,
        // flip its value to invoke the changed handler.
        if (IsDropDownOpen && Popup != null && !Popup.IsOpen)
        {
            OpeningDropDown(false);
        }
    }

    /// <summary>
    /// Handles the event that reports a closed popup: it releases the subscriptions of the open state,
    /// writes the closed state back into <see cref="IsDropDownOpen" /> and raises
    /// <see cref="DropDownClosed" />. An override must call the base member to keep this behavior.
    /// </summary>
    protected virtual void PopupClosed(object? sender, EventArgs e)
    {
        SubscriptionsOnOpen.Clear();
        PopupHasOpened = false;
        // The popup also closes without a request from this control, for example by light dismiss. Write the closed
        // state back so that IsDropDownOpen follows the actual popup state. AbstractAutoComplete does the same.
        if (IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
        }
        NotifyPopupClosed();
    }

    /// <summary>
    /// Handles the event that reports an opened popup: it subscribes to the visibility of the control and of
    /// its ancestors so that the drop-down closes when the control becomes invisible, applies the minimum
    /// width and the maximum height of the drop-down and raises <see cref="DropDownOpened" />. An override
    /// must call the base member to keep this behavior.
    /// </summary>
    protected virtual void PopupOpened(object? sender, EventArgs e)
    {
        SubscriptionsOnOpen.Clear();
        this.GetObservable(IsVisibleProperty).Subscribe(IsVisibleChanged).DisposeWith(SubscriptionsOnOpen);
        this.SubscribeAncestorIsVisible(IsVisibleChanged, SubscriptionsOnOpen);

        ConfigurePopupMinWith(Bounds.Width);
        if (_defaultPopupHeight.HasValue)
        {
            ConfigureMaxDropdownHeight();
        }
        NotifyPopupOpened();
    }

    /// <summary>
    /// Reacts to property changes: it updates the pseudo-classes when a status or validation property
    /// changes, reapplies <see cref="Placement" />, renews the subscription to the popup layout tokens,
    /// recalculates the maximum drop-down height, and opens the drop-down when
    /// <see cref="IsPopupPinnedOpen" /> changes to <see langword="true" />. An override must call the base
    /// member first.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == StatusProperty ||
            change.Property == FormStatusProperty ||
            change.Property == DataValidationErrors.HasErrorsProperty ||
            change.Property == DataValidationErrors.ErrorsProperty)
        {
            UpdatePseudoClasses();
        }
        else if (change.Property == PlacementProperty)
        {
            ConfigurePopupPlacement();
        }
        else if (change.Property == PopupLayoutTokenResourceKeysProperty)
        {
            UpdatePopupLayoutTokenSubscription(change.GetOldValue<CandidatePopupLayoutTokenResourceKeys?>()?.ContentPadding is not null);
        }
        else if (change.Property == DisplayPageSizeProperty ||
                 change.Property == PopupContentPaddingProperty)
        {
            ConfigureMaxDropdownHeight();
        }
        else if (change.Property == IsPopupPinnedOpenProperty)
        {
            if (change.GetNewValue<bool>() && !IsDropDownOpen)
            {
                SetCurrentValue(IsDropDownOpenProperty, true);
            }
        }
    }

    /// <summary>
    /// Sets the minimum width of the drop-down from the width of the control. It takes effect only while
    /// <see cref="IsPopupMatchSelectWidth" /> is <see langword="true" />; otherwise the minimum width is
    /// reset to 0. It is called when the popup opens. Derived classes may override it to use another width.
    /// </summary>
    /// <param name="selectWidth">
    /// The width of the control.
    /// </param>
    protected virtual void ConfigurePopupMinWith(double selectWidth)
    {
        if (IsPopupMatchSelectWidth)
        {
            SetCurrentValue(EffectivePopupWidthProperty, selectWidth);
        }
        else
        {
            SetCurrentValue(EffectivePopupWidthProperty, 0.0);
        }
    }

    /// <summary>
    /// Calculates the maximum height of the drop-down. While <see cref="DisplayPageSize" /> is not set
    /// explicitly and the popup layout tokens supply a default height, that default is used. Otherwise the
    /// height is the row height times <see cref="DisplayPageSize" /> plus the vertical padding of the popup.
    /// It is called when the template is applied, when the popup opens and whenever one of these values
    /// changes.
    /// </summary>
    protected virtual void ConfigureMaxDropdownHeight()
    {
        MaxPopupHeight = _defaultPopupHeight is { } maxHeight && !IsSet(DisplayPageSizeProperty)
            ? maxHeight
            : _itemHeight * DisplayPageSize + PopupContentPadding.Top + PopupContentPadding.Bottom;
    }

    private void UpdatePopupLayoutTokenSubscription(bool previouslyThemedPadding)
    {
        _popupLayoutTokens?.Dispose();
        _popupLayoutTokens = null;
        if (PopupLayoutTokenResourceKeys is { } keys)
        {
            _popupLayoutTokens = keys.Observe(this, RefreshPopupLayout);
        }
        if (previouslyThemedPadding && PopupLayoutTokenResourceKeys?.ContentPadding is null)
        {
            PopupContentPadding = default;
        }
        RefreshPopupLayout();
    }

    private void RefreshPopupLayout()
    {
        var previousRowHeight = _itemHeight;
        var previousMaxHeight = _defaultPopupHeight;
        _itemHeight = CandidatePopupLayoutTokenResourceKeys.ReadLength(_popupLayoutTokens?.GetValue(0), _itemHeight) ?? 0;
        _defaultPopupHeight = PopupLayoutTokenResourceKeys?.DefaultMaxHeight is null
            ? null
            : CandidatePopupLayoutTokenResourceKeys.ReadLength(_popupLayoutTokens?.GetValue(2), _defaultPopupHeight);
        if (PopupLayoutTokenResourceKeys?.ContentPadding is not null)
        {
            var padding = CandidatePopupLayoutTokenResourceKeys.ReadPadding(_popupLayoutTokens?.GetValue(1), PopupContentPadding);
            if (padding != PopupContentPadding)
            {
                // The final padding is a template output and recalculates height with the complete new metrics.
                PopupContentPadding = padding;
                return;
            }
        }
        if (!previousRowHeight.Equals(_itemHeight) || previousMaxHeight != _defaultPopupHeight)
        {
            ConfigureMaxDropdownHeight();
        }
    }

    /// <summary>
    /// Updates the pseudo-classes of the control from its current state: drop-down open from
    /// <see cref="IsDropDownOpen" />, warning from the effective status, and outline, filled and borderless
    /// from <see cref="StyleVariant" />. Derived classes call it after they changed one of these states.
    /// </summary>
    protected void UpdatePseudoClasses()
    {
        var effectiveStatus = InputControlState.ResolveEffectiveStatus(this, Status, FormStatus);
        PseudoClasses.Set(SelectPseudoClass.DropdownOpen, IsDropDownOpen);
        PseudoClasses.Set(StdPseudoClass.Warning,
            effectiveStatus == InputControlStatus.Warning);
        PseudoClasses.Set(AddOnDecoratedBoxPseudoClass.Outline, StyleVariant == InputControlStyleVariant.Outlined);
        PseudoClasses.Set(AddOnDecoratedBoxPseudoClass.Filled, StyleVariant == InputControlStyleVariant.Filled);
        PseudoClasses.Set(AddOnDecoratedBoxPseudoClass.Borderless, StyleVariant == InputControlStyleVariant.Borderless);
    }

    private protected void SetDropDownOpenWithoutPropertyHandling(bool isOpen)
    {
        IgnorePropertyChange = true;
        try
        {
            SetCurrentValue(IsDropDownOpenProperty, isOpen);
        }
        finally
        {
            if (IgnorePropertyChange)
            {
                IgnorePropertyChange = false;
            }
        }
    }

    /// <summary>
    /// Runs the closing sequence for a change of <see cref="IsDropDownOpen" /> to <see langword="false" />:
    /// it raises <see cref="DropDownClosing" /> and closes the popup unless a handler canceled the event.
    /// When the event was canceled, <see cref="IsDropDownOpen" /> is restored to the value it had before.
    /// The pseudo-classes are updated afterwards.
    /// </summary>
    /// <param name="oldValue">
    /// The value of <see cref="IsDropDownOpen" /> before the change.
    /// </param>
    protected void ClosingDropDown(bool oldValue)
    {
        var args = new CancelEventArgs();
        NotifyDropDownClosing(args);

        if (args.Cancel)
        {
            SetDropDownOpenWithoutPropertyHandling(oldValue);
        }
        else
        {
            CloseDropDown();
        }

        UpdatePseudoClasses();
    }

    /// <summary>
    /// Closes the popup when this control has opened it. It does nothing while the popup was not opened by
    /// this control, and it raises no event by itself: the closed popup reports back, which synchronizes
    /// <see cref="IsDropDownOpen" /> and raises <see cref="DropDownClosed" />.
    /// </summary>
    protected void CloseDropDown()
    {
        if (PopupHasOpened)
        {
            if (Popup != null)
            {
                Popup.IsOpen = false;
            }
        }
    }

    /// <summary>
    /// Prepares the popup content through <see cref="EnsurePopupContent" />, opens the popup and marks it as
    /// opened by this control. It raises neither <see cref="DropDownOpening" /> nor <see cref="DropDownOpened" />;
    /// <see cref="DropDownOpened" /> is raised once the popup reports that it opened. Setting
    /// <see cref="IsDropDownOpen" /> to <see langword="true" /> runs this method inside the full opening
    /// sequence.
    /// </summary>
    protected void OpenDropDown()
    {
        EnsurePopupContent();
        if (Popup != null)
        {
            Popup.IsOpen = true;
        }
        PopupHasOpened = true;
    }

    /// <summary>
    /// Prepares the content of the popup before it opens, for example by creating the candidate list. It is
    /// called by <see cref="OpenDropDown" /> right before the popup is set to open. The base implementation
    /// does nothing.
    /// </summary>
    protected virtual void EnsurePopupContent()
    {
    }

    /// <summary>
    /// Runs the opening sequence for a change of <see cref="IsDropDownOpen" /> to <see langword="true" />:
    /// it raises <see cref="DropDownOpening" /> and opens the popup through <see cref="OpenDropDown" />
    /// unless a handler canceled the event. When the event was canceled, <see cref="IsDropDownOpen" /> is
    /// restored to the value it had before. The pseudo-classes are updated afterwards.
    /// </summary>
    /// <param name="oldValue">
    /// The value of <see cref="IsDropDownOpen" /> before the change.
    /// </param>
    protected void OpeningDropDown(bool oldValue)
    {
        var args = new CancelEventArgs();

        // Opening
        NotifyDropDownOpening(args);

        if (args.Cancel)
        {
            SetDropDownOpenWithoutPropertyHandling(oldValue);
        }
        else
        {
            OpenDropDown();
        }

        UpdatePseudoClasses();
    }

    /// <summary>
    /// Raises <see cref="DropDownClosed" />. It is called by <see cref="PopupClosed" /> after the closed
    /// state was written back; an override must call the base member for the event to be raised.
    /// </summary>
    protected virtual void NotifyPopupClosed()
    {
        DropDownClosed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises <see cref="DropDownOpened" />. It is called by <see cref="PopupOpened" /> after the width and
    /// height of the drop-down were applied; an override must call the base member for the event to be
    /// raised.
    /// </summary>
    protected virtual void NotifyPopupOpened()
    {
        DropDownOpened?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises <see cref="DropDownOpening" />. It is called by <see cref="OpeningDropDown" /> before the popup
    /// opens; an override must call the base member for the event to be raised.
    /// </summary>
    /// <param name="eventArgs">
    /// The arguments passed to the handlers; they carry the cancel decision back to the caller.
    /// </param>
    protected virtual void NotifyDropDownOpening(CancelEventArgs eventArgs)
    {
        DropDownOpening?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Raises <see cref="DropDownOpened" /> with the given arguments. The drop-down logic of this class
    /// raises the event through <see cref="NotifyPopupOpened" /> instead, so this method is an entry point
    /// for derived classes.
    /// </summary>
    /// <param name="eventArgs">
    /// The arguments passed to the handlers.
    /// </param>
    protected virtual void NotifyDropDownOpened(EventArgs eventArgs)
    {
        DropDownOpened?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Raises <see cref="DropDownClosing" />. It is called by <see cref="ClosingDropDown" /> before the popup
    /// closes; an override must call the base member for the event to be raised.
    /// </summary>
    /// <param name="eventArgs">
    /// The arguments passed to the handlers; they carry the cancel decision back to the caller.
    /// </param>
    protected virtual void NotifyDropDownClosing(CancelEventArgs eventArgs)
    {
        DropDownClosing?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Raises <see cref="DropDownClosed" /> with the given arguments. The drop-down logic of this class
    /// raises the event through <see cref="NotifyPopupClosed" /> instead, so this method is an entry point
    /// for derived classes.
    /// </summary>
    /// <param name="eventArgs">
    /// The arguments passed to the handlers.
    /// </param>
    protected virtual void NotifyDropDownClosed(EventArgs eventArgs)
    {
        DropDownClosed?.Invoke(this, eventArgs);
    }

    #region 实现 CompactSpace 接口

    void ICompactSpaceAware.NotifyPositionChange(SpaceItemPosition? position)
    {
        var isUsedInCompactSpace = position != null;
        if (IsUsedInCompactSpace != isUsedInCompactSpace)
        {
            IsUsedInCompactSpace = isUsedInCompactSpace;
        }

        if (CompactSpaceItemPosition != position)
        {
            CompactSpaceItemPosition = position;
        }
    }

    void ICompactSpaceAware.NotifyOrientationChange(Orientation orientation)
    {
        if (CompactSpaceOrientation != orientation)
        {
            CompactSpaceOrientation = orientation;
        }
    }

    double ICompactSpaceAware.GetBorderThickness()
    {
        return GetBorderThicknessForCompactSpace();
    }

    /// <summary>
    /// Returns the width of the border that this control draws as a member of a compact group; it is the
    /// value reported through <see cref="ICompactSpaceAware.GetBorderThickness" />. It returns 0 when the
    /// control is not used in a compact group, when the template has no add-on decorated box, or for the
    /// borderless style variant. Otherwise it returns the left border width of the inner box.
    /// </summary>
    /// <returns>
    /// The border width, or 0 when the control draws no border.
    /// </returns>
    protected virtual double GetBorderThicknessForCompactSpace()
    {
        if (!IsUsedInCompactSpace)
        {
            return 0.0;
        }

        if (_addOnDecoratedBox == null || _addOnDecoratedBox.StyleVariant == InputControlStyleVariant.Borderless)
        {
            return 0.0;
        }

        // All sides of the border are equally wide, so the left value is enough.
        return _addOnDecoratedBox.InnerBoxBorderThickness.Left;
    }

    #endregion

    #region 实现 FormItem 接口

    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    void IFormItemAware.SetFormValue(object? value) => NotifySetFormValue(value);

    object? IFormItemAware.GetFormValue() => NotifyGetFormValue();
    void IFormItemAware.ClearFormValue() => NotifyClearFormValue();
    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status) => NotifyValidateStatus(status);
    void IFormItemFeedbackAware.SetFeedbackControl(FormValidateFeedback? value) => NotifySetFeedBackControl(value);

    /// <summary>
    /// Raises the event that informs an owning form item that the value of the control changed. A derived
    /// class calls it after its selection changed. An override must call the base member for the event to be
    /// raised.
    /// </summary>
    /// <param name="value">
    /// The new value of the control. The base implementation does not use it.
    /// </param>
    protected virtual void NotifyFormValueChanged(object? value)
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Applies a value that the owning form item hands over to the control. It is called through
    /// <see cref="IFormItemAware.SetFormValue" />. The base implementation does nothing; a derived class
    /// overrides it to apply the value to its selection.
    /// </summary>
    /// <param name="value">
    /// The value to apply. A derived class defines which type it expects.
    /// </param>
    protected virtual void NotifySetFormValue(object? value)
    {
    }

    /// <summary>
    /// Returns the value that the owning form item reads back from the control. It is called through
    /// <see cref="IFormItemAware.GetFormValue" />. The base implementation returns <see langword="null" />;
    /// a derived class overrides it to return its selection.
    /// </summary>
    /// <returns>
    /// The current value of the control for the form, or <see langword="null" /> when the derived class
    /// provides none.
    /// </returns>
    protected virtual object? NotifyGetFormValue()
    {
        return null;
    }

    /// <summary>
    /// Resets the value of the control when the owning form item clears it. It is called through
    /// <see cref="IFormItemAware.ClearFormValue" />. The base implementation does nothing; a derived class
    /// overrides it to clear its selection.
    /// </summary>
    protected virtual void NotifyClearFormValue()
    {
    }

    /// <summary>
    /// Stores the validation status reported by the owning form item in the form status of the control,
    /// which drives the effective status and the warning pseudo-class. It is called through
    /// <see cref="IFormItemAware.NotifyValidateStatus" />. An override must call the base member for the
    /// status to take effect.
    /// </summary>
    /// <param name="status">
    /// The validation status reported by the form item.
    /// </param>
    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
        if (FormStatus != status)
        {
            SetCurrentValue(FormStatusProperty, status);
        }
    }

    /// <summary>
    /// Stores the feedback control of the owning form item in the feedback of the control, which the handle
    /// shows next to its indicator. It is called through
    /// <see cref="IFormItemFeedbackAware.SetFeedbackControl" />. An override must call the base member for the
    /// feedback to be stored. The base implementation ignores a value that did not change.
    /// </summary>
    /// <param name="value">
    /// The feedback control, or <see langword="null" /> to remove it.
    /// </param>
    protected virtual void NotifySetFeedBackControl(FormValidateFeedback? value)
    {
        if (!ReferenceEquals(FormFeedback, value))
        {
            FormFeedback = value;
        }
    }

    #endregion

    private void HandleIsDropDownOpenChanged(AvaloniaPropertyChangedEventArgs e)
    {
        // Ignore the change if requested
        if (IgnorePropertyChange)
        {
            IgnorePropertyChange = false;
            return;
        }

        bool oldValue = (bool)e.OldValue!;
        bool newValue = (bool)e.NewValue!;

        if (!newValue && IsPopupPinnedOpen)
        {
            SetDropDownOpenWithoutPropertyHandling(oldValue);
            UpdatePseudoClasses();
            return;
        }

        if (!newValue)
        {
            ClosingDropDown(oldValue);
        }
        else
        {
            OpeningDropDown(oldValue);
        }

        UpdatePseudoClasses();
    }

    private void HandleWindowDeactivated(object? sender, EventArgs e)
    {
        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    private void IsVisibleChanged(bool isVisible)
    {
        if (!isVisible && IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
        }
    }

    private void ConfigurePopupPlacement()
    {
        if (Placement == SelectPopupPlacement.BottomEdgeAlignedLeft)
        {
            PopupPlacement = PlacementMode.BottomEdgeAlignedLeft;
        }
        else if (Placement == SelectPopupPlacement.BottomEdgeAlignedRight)
        {
            PopupPlacement = PlacementMode.BottomEdgeAlignedRight;
        }
        else if (Placement == SelectPopupPlacement.TopEdgeAlignedLeft)
        {
            PopupPlacement = PlacementMode.TopEdgeAlignedLeft;
        }
        else if (Placement == SelectPopupPlacement.TopEdgeAlignedRight)
        {
            PopupPlacement = PlacementMode.TopEdgeAlignedRight;
        }
    }

}
