using AtomUI.Animations;
using System.Collections.Specialized;
using AtomUI.Controls;
using AtomUI.Controls.Utils;
using AtomUI.Data;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Desktop.Controls.Primitives.Themes;
using AtomUI.Generated.AtomUIDesktopControls;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using AvaloniaComboBox = Avalonia.Controls.ComboBox;
using AvaloniaTextBox = Avalonia.Controls.TextBox;

/// <summary>
/// An Ant Design style combo box: a drop-down list of options with an optional editable text box that
/// filters the options, add-ons outside and inside the input frame, and a clear button. It reports its
/// selected item to a form and shows the feedback of that form.
/// </summary>
public partial class ComboBox : AvaloniaComboBox,
                                IMotionAwareControl,
                                ICustomizableSizeTypeAware,
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
        Popup.OpenMotionProperty.AddOwner<ComboBox>();

    /// <summary>
    /// Identifies the <see cref="CloseMotion" /> property.
    /// </summary>
    public static readonly StyledProperty<AbstractMotion?> CloseMotionProperty =
        Popup.CloseMotionProperty.AddOwner<ComboBox>();

    /// <summary>
    /// Identifies the <see cref="LeftAddOn" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> LeftAddOnProperty =
        AddOnDecoratedBox.LeftAddOnProperty.AddOwner<ComboBox>();
    
    /// <summary>
    /// Identifies the <see cref="LeftAddOnTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> LeftAddOnTemplateProperty =
        AddOnDecoratedBox.LeftAddOnTemplateProperty.AddOwner<ComboBox>();

    /// <summary>
    /// Identifies the <see cref="RightAddOn" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> RightAddOnProperty =
        AddOnDecoratedBox.RightAddOnProperty.AddOwner<ComboBox>();
    
    /// <summary>
    /// Identifies the <see cref="RightAddOnTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> RightAddOnTemplateProperty =
        AddOnDecoratedBox.RightAddOnTemplateProperty.AddOwner<ComboBox>();
    
    /// <summary>
    /// Identifies the <see cref="ContentLeftAddOn" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ContentLeftAddOnProperty =
        AddOnDecoratedBox.ContentLeftAddOnProperty.AddOwner<ComboBox>();
    
    /// <summary>
    /// Identifies the <see cref="ContentLeftAddOnTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ContentLeftAddOnTemplateProperty =
        AddOnDecoratedBox.ContentLeftAddOnTemplateProperty.AddOwner<ComboBox>();

    /// <summary>
    /// Identifies the <see cref="ContentRightAddOn" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ContentRightAddOnProperty =
        AddOnDecoratedBox.ContentRightAddOnProperty.AddOwner<ComboBox>();
    
    /// <summary>
    /// Identifies the <see cref="ContentRightAddOnTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ContentRightAddOnTemplateProperty =
        AddOnDecoratedBox.ContentRightAddOnTemplateProperty.AddOwner<ComboBox>();

    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<ComboBox>();

    /// <summary>
    /// Identifies the <see cref="StyleVariant" /> property.
    /// </summary>
    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<ComboBox>();

    /// <summary>
    /// Identifies the <see cref="Status" /> property.
    /// </summary>
    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<ComboBox>();

    /// <summary>
    /// Identifies the <see cref="IsAllowClear" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsAllowClearProperty =
        TextBox.IsAllowClearProperty.AddOwner<ComboBox>();
    
    /// <summary>
    /// Identifies the <see cref="OptionFontSize" /> property.
    /// </summary>
    public static readonly StyledProperty<double> OptionFontSizeProperty =
        AvaloniaProperty.Register<ComboBox, double>(nameof(OptionFontSize));

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ComboBox>();
    
    /// <summary>
    /// Identifies the <see cref="DropDownDisplayPageSize" /> property.
    /// </summary>
    public static readonly StyledProperty<int> DropDownDisplayPageSizeProperty =
        AvaloniaProperty.Register<ComboBox, int>(nameof (DropDownDisplayPageSize), 10);

    /// <summary>
    /// Identifies the <see cref="ShouldUseOverlayPopup" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShouldUseOverlayPopupProperty =
        AvaloniaProperty.Register<ComboBox, bool>(nameof(ShouldUseOverlayPopup), true);

    /// <summary>
    /// Identifies the <see cref="IsPopupPinnedOpen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        Popup.IsPopupPinnedOpenProperty.AddOwner<ComboBox>();

    /// <summary>
    /// Identifies the <see cref="IsFilterEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsFilterEnabledProperty =
        AvaloniaProperty.Register<ComboBox, bool>(nameof(IsFilterEnabled));

    /// <summary>
    /// Identifies the <see cref="IsShowOverflowTip" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowOverflowTipProperty =
        AvaloniaProperty.Register<ComboBox, bool>(nameof(IsShowOverflowTip), true);

    /// <summary>
    /// Identifies the <see cref="OverflowTipDelay" /> property.
    /// </summary>
    public static readonly StyledProperty<int> OverflowTipDelayProperty =
        AvaloniaProperty.Register<ComboBox, int>(nameof(OverflowTipDelay), 1200);

    /// <summary>
    /// Identifies the <see cref="OverflowTipPlacement" /> property.
    /// </summary>
    public static readonly StyledProperty<PlacementMode> OverflowTipPlacementProperty =
        AvaloniaProperty.Register<ComboBox, PlacementMode>(nameof(OverflowTipPlacement), PlacementMode.TopEdgeAlignedLeft);

    /// <summary>
    /// Identifies the <see cref="Filter" /> property.
    /// </summary>
    public static readonly StyledProperty<IValueFilter?> FilterProperty =
        AvaloniaProperty.Register<ComboBox, IValueFilter?>(nameof(Filter));

    /// <summary>
    /// Identifies the <see cref="FilterValue" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> FilterValueProperty =
        AvaloniaProperty.Register<ComboBox, object?>(nameof(FilterValue));

    /// <summary>
    /// Identifies the <see cref="FilterValueSelector" /> property.
    /// </summary>
    public static readonly StyledProperty<DefaultFilterValueSelector?> FilterValueSelectorProperty =
        AvaloniaProperty.Register<ComboBox, DefaultFilterValueSelector?>(
            nameof(FilterValueSelector));

    /// <summary>
    /// Identifies the <see cref="IsShowEmptyIndicator" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowEmptyIndicatorProperty =
        AvaloniaProperty.Register<ComboBox, bool>(nameof(IsShowEmptyIndicator), true);

    /// <summary>
    /// Identifies the <see cref="EmptyIndicator" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> EmptyIndicatorProperty =
        AvaloniaProperty.Register<ComboBox, object?>(nameof(EmptyIndicator));

    /// <summary>
    /// Identifies the <see cref="EmptyIndicatorTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> EmptyIndicatorTemplateProperty =
        AvaloniaProperty.Register<ComboBox, IDataTemplate?>(nameof(EmptyIndicatorTemplate));

    /// <summary>
    /// Identifies the <see cref="EmptyIndicatorPadding" /> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> EmptyIndicatorPaddingProperty =
        AvaloniaProperty.Register<ComboBox, Thickness>(nameof(EmptyIndicatorPadding));

    /// <summary>
    /// Motion played when the drop-down opens. The theme sets it to <c>PopupMotionDefaults.SelectOpen</c>,
    /// the template hands it to the drop-down popup, and clearing a local value falls back to the theme
    /// value.
    /// </summary>
    public AbstractMotion? OpenMotion
    {
        get => GetValue(OpenMotionProperty);
        set => SetValue(OpenMotionProperty, value);
    }

    /// <summary>
    /// Motion played when the drop-down closes. The theme sets it to <c>PopupMotionDefaults.SelectClose</c>,
    /// the template hands it to the drop-down popup, and clearing a local value falls back to the theme
    /// value.
    /// </summary>
    public AbstractMotion? CloseMotion
    {
        get => GetValue(CloseMotionProperty);
        set => SetValue(CloseMotionProperty, value);
    }

    /// <summary>
    /// Content of the add-on attached outside the input frame, on its left. The add-on box is shown only
    /// while this value is not <see langword="null" />. <see cref="ContentLeftAddOn" /> places content
    /// inside the frame.
    /// </summary>
    public object? LeftAddOn
    {
        get => GetValue(LeftAddOnProperty);
        set => SetValue(LeftAddOnProperty, value);
    }
    
    /// <summary>
    /// Template used to build <see cref="LeftAddOn" />.
    /// </summary>
    public IDataTemplate? LeftAddOnTemplate
    {
        get => GetValue(LeftAddOnTemplateProperty);
        set => SetValue(LeftAddOnTemplateProperty, value);
    }

    /// <summary>
    /// Content of the add-on attached outside the input frame, on its right. The add-on box is shown only
    /// while this value is not <see langword="null" />. <see cref="ContentRightAddOn" /> places content
    /// inside the frame.
    /// </summary>
    public object? RightAddOn
    {
        get => GetValue(RightAddOnProperty);
        set => SetValue(RightAddOnProperty, value);
    }
    
    /// <summary>
    /// Template used to build <see cref="RightAddOn" />.
    /// </summary>
    public IDataTemplate? RightAddOnTemplate
    {
        get => GetValue(RightAddOnTemplateProperty);
        set => SetValue(RightAddOnTemplateProperty, value);
    }
    
    /// <summary>
    /// Content of the add-on inside the input frame, to the left of the text. <see cref="LeftAddOn" /> is
    /// the add-on attached outside the frame.
    /// </summary>
    public object? ContentLeftAddOn
    {
        get => GetValue(ContentLeftAddOnProperty);
        set => SetValue(ContentLeftAddOnProperty, value);
    }
    
    /// <summary>
    /// Template used to build <see cref="ContentLeftAddOn" />.
    /// </summary>
    public IDataTemplate? ContentLeftAddOnTemplate
    {
        get => GetValue(ContentLeftAddOnTemplateProperty);
        set => SetValue(ContentLeftAddOnTemplateProperty, value);
    }

    /// <summary>
    /// Content of the add-on inside the input frame, to the right of the text and before the form feedback
    /// icon and the action group with the clear button and the arrow handle.
    /// <see cref="RightAddOn" /> is the add-on attached outside the frame.
    /// </summary>
    public object? ContentRightAddOn
    {
        get => GetValue(ContentRightAddOnProperty);
        set => SetValue(ContentRightAddOnProperty, value);
    }
    
    /// <summary>
    /// Template used to build <see cref="ContentRightAddOn" />.
    /// </summary>
    public IDataTemplate? ContentRightAddOnTemplate
    {
        get => GetValue(ContentRightAddOnTemplateProperty);
        set => SetValue(ContentRightAddOnTemplateProperty, value);
    }

    /// <summary>
    /// Size of the control. The template passes it to the input frame, and every option item container
    /// follows it. <see cref="CustomizableSizeType.Large" /> and <see cref="CustomizableSizeType.Small" />
    /// use the large and small font sizes; <see cref="CustomizableSizeType.Middle" /> and
    /// <see cref="CustomizableSizeType.Custom" /> use the middle font size.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// Visual style of the input frame: outlined, filled or borderless. The template passes it to the
    /// frame. With the filled and borderless styles, the selected text takes the error or warning color
    /// under the <c>:error</c> or <c>:warning</c> state.
    /// </summary>
    public InputControlStyleVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }

    /// <summary>
    /// Status that the input frame shows, such as error or warning. The template passes it to the frame,
    /// and the <c>:warning</c> pseudo-class follows the status resolved from this value and
    /// <c>FormStatus</c>.
    /// </summary>
    public InputControlStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>
    /// Shows the clear button inside the input frame. It appears only while there is a value to clear, the
    /// control is enabled, and the pointer is over the control or the focus is within it. Clicking it
    /// clears the typed text and the selection.
    /// </summary>
    public bool IsAllowClear
    {
        get => GetValue(IsAllowClearProperty);
        set => SetValue(IsAllowClearProperty, value);
    }
    
    /// <summary>
    /// Font size of the option items in the drop-down; it is bound to every item container. The theme
    /// supplies the token value.
    /// </summary>
    public double OptionFontSize
    {
        get => GetValue(OptionFontSizeProperty);
        set => SetValue(OptionFontSizeProperty, value);
    }

    /// <summary>
    /// Switch for the motion of the control. The drop-down popup, the input frame, the arrow handle, the
    /// clear button and the option item containers all receive this value. The theme supplies the token
    /// value.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    /// <summary>
    /// Number of option rows the drop-down shows before it scrolls; the default is 10. The maximum
    /// drop-down height is this number times <c>ItemHeight</c>, plus the top and bottom padding of the
    /// popup content.
    /// </summary>
    public int DropDownDisplayPageSize
    {
        get => GetValue(DropDownDisplayPageSizeProperty);
        set => SetValue(DropDownDisplayPageSizeProperty, value);
    }

    /// <summary>
    /// Value handed to the drop-down popup as <c>ShouldUseOverlayLayer</c>. When
    /// <see langword="true" />, the default, the popup is hosted in the overlay layer of the window.
    /// </summary>
    public bool ShouldUseOverlayPopup
    {
        get => GetValue(ShouldUseOverlayPopupProperty);
        set => SetValue(ShouldUseOverlayPopupProperty, value);
    }

    /// <summary>
    /// Keeps the candidate popup open: while <see langword="true" />, light-dismiss close requests (an
    /// outside click or Escape) are ignored, and clearing <c>IsDropDownOpen</c> re-opens the drop-down.
    /// The drop-down still closes while the control is detached and opens again when it is attached.
    /// Setting it to <see langword="true" /> opens a closed drop-down. The Gallery semantic part preview
    /// uses it to keep the <c>popup.*</c> parts highlighted.
    /// </summary>
    public bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }

    /// <summary>
    /// Hides the option items that do not match the typed text. It takes effect only while
    /// <c>IsEditable</c> is <see langword="true" />: <see cref="FilterValue" /> then follows <c>Text</c>,
    /// and an item stays visible while <see cref="Filter" /> accepts it.
    /// </summary>
    public bool IsFilterEnabled
    {
        get => GetValue(IsFilterEnabledProperty);
        set => SetValue(IsFilterEnabledProperty, value);
    }

    /// <summary>
    /// Shows a tip with the text of the selected item when that text does not fit the selection area of
    /// the input frame. The default is <see langword="true" />.
    /// </summary>
    public bool IsShowOverflowTip
    {
        get => GetValue(IsShowOverflowTipProperty);
        set => SetValue(IsShowOverflowTipProperty, value);
    }

    /// <summary>
    /// Time in milliseconds the pointer must rest on the selection before its overflow tip appears. The
    /// default is 1200.
    /// </summary>
    public int OverflowTipDelay
    {
        get => GetValue(OverflowTipDelayProperty);
        set => SetValue(OverflowTipDelayProperty, value);
    }

    /// <summary>
    /// Position of the overflow tip relative to the input frame. The default is
    /// <see cref="PlacementMode.TopEdgeAlignedLeft" />.
    /// </summary>
    public PlacementMode OverflowTipPlacement
    {
        get => GetValue(OverflowTipPlacementProperty);
        set => SetValue(OverflowTipPlacementProperty, value);
    }

    /// <summary>
    /// Filter that decides whether an option item matches the typed text: it receives the item's filter
    /// value and the current <see cref="FilterValue" />. It is used while <see cref="IsFilterEnabled" />
    /// is in effect and <see cref="FilterValue" /> is not empty. A <see langword="null" /> filter is
    /// replaced by a contains filter when the control is initialized.
    /// </summary>
    public IValueFilter? Filter
    {
        get => GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }

    /// <summary>
    /// Text the filter compares the option items with. While filtering is in effect it follows
    /// <c>Text</c> (an empty text gives <see langword="null" />, which leaves every item visible); a value
    /// set directly is overwritten by the next text change.
    /// </summary>
    public object? FilterValue
    {
        get => GetValue(FilterValueProperty);
        set => SetValue(FilterValueProperty, value);
    }

    /// <summary>
    /// Chooses the value of an item that <see cref="Filter" /> compares with <see cref="FilterValue" />.
    /// When it is <see langword="null" />, the display text of the item is used.
    /// </summary>
    public DefaultFilterValueSelector? FilterValueSelector
    {
        get => GetValue(FilterValueSelectorProperty);
        set => SetValue(FilterValueSelectorProperty, value);
    }

    /// <summary>
    /// Whether to show the empty indicator when the source is empty or filtering has no visible matches.
    /// Defaults to true and does not change filtering or selection.
    /// </summary>
    public bool IsShowEmptyIndicator
    {
        get => GetValue(IsShowEmptyIndicatorProperty);
        set => SetValue(IsShowEmptyIndicatorProperty, value);
    }

    /// <summary>
    /// Custom empty content. The default Empty is shown when both content and template are null.
    /// </summary>
    [DependsOn(nameof(EmptyIndicatorTemplate))]
    public object? EmptyIndicator
    {
        get => GetValue(EmptyIndicatorProperty);
        set => SetValue(EmptyIndicatorProperty, value);
    }

    /// <summary>
    /// Template for empty content, including null content when only a template is specified.
    /// The template must match the supplied content through IDataTemplate.Match.
    /// </summary>
    public IDataTemplate? EmptyIndicatorTemplate
    {
        get => GetValue(EmptyIndicatorTemplateProperty);
        set => SetValue(EmptyIndicatorTemplateProperty, value);
    }

    /// <summary>
    /// Padding around empty content. Defaults to zero; the default theme supplies SharedToken.Padding.
    /// </summary>
    public Thickness EmptyIndicatorPadding
    {
        get => GetValue(EmptyIndicatorPaddingProperty);
        set => SetValue(EmptyIndicatorPaddingProperty, value);
    }

    #endregion

    #region 内部属性定义

    
    internal static readonly DirectProperty<ComboBox, double> EffectivePopupWidthProperty =
        AvaloniaProperty.RegisterDirect<ComboBox, double>(
            nameof(EffectivePopupWidth),
            o => o.EffectivePopupWidth,
            (o, v) => o.EffectivePopupWidth = v);
    
    internal static readonly StyledProperty<double> ItemHeightProperty =
        AvaloniaProperty.Register<ComboBox, double>(nameof(ItemHeight));
    
    internal static readonly StyledProperty<Thickness> PopupContentPaddingProperty =
        AvaloniaProperty.Register<ComboBox, Thickness>(nameof(PopupContentPadding));

    internal static readonly StyledProperty<bool> UsePopupLayoutTokenResourcesProperty =
        AvaloniaProperty.Register<ComboBox, bool>(nameof(UsePopupLayoutTokenResources));
    
    internal static readonly StyledProperty<FormValidateFeedback?> FormFeedbackProperty =
        AvaloniaProperty.Register<ComboBox, FormValidateFeedback?>(nameof(FormFeedback));

    internal static readonly StyledProperty<FormValidateStatus> FormStatusProperty =
        InputControlState.FormStatusProperty.AddOwner<ComboBox>();

    internal static readonly DirectProperty<ComboBox, bool> IsFormFeedbackVisibleProperty =
        AvaloniaProperty.RegisterDirect<ComboBox, bool>(
            nameof(IsFormFeedbackVisible),
            o => o.IsFormFeedbackVisible);

    internal static readonly DirectProperty<ComboBox, bool> IsEffectiveFilterEnabledProperty =
        AvaloniaProperty.RegisterDirect<ComboBox, bool>(
            nameof(IsEffectiveFilterEnabled),
            o => o.IsEffectiveFilterEnabled);

    internal static readonly DirectProperty<ComboBox, bool> IsEffectiveEmptyVisibleProperty =
        AvaloniaProperty.RegisterDirect<ComboBox, bool>(
            nameof(IsEffectiveEmptyVisible),
            o => o.IsEffectiveEmptyVisible);

    internal static readonly DirectProperty<ComboBox, bool> IsDefaultEmptyIndicatorVisibleProperty =
        AvaloniaProperty.RegisterDirect<ComboBox, bool>(
            nameof(IsDefaultEmptyIndicatorVisible),
            o => o.IsDefaultEmptyIndicatorVisible);

    internal static readonly DirectProperty<ComboBox, bool> CanClearValueProperty =
        AvaloniaProperty.RegisterDirect<ComboBox, bool>(
            nameof(CanClearValue),
            o => o.CanClearValue);

    internal static readonly StyledProperty<bool> IsEffectiveShowClearButtonProperty =
        AvaloniaProperty.Register<ComboBox, bool>(nameof(IsEffectiveShowClearButton));

    private double _effectivePopupWidth;

    internal double EffectivePopupWidth
    {
        get => _effectivePopupWidth;
        set => SetAndRaise(EffectivePopupWidthProperty, ref _effectivePopupWidth, value);
    }
    
    internal double ItemHeight
    {
        get => GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }
    
    internal Thickness PopupContentPadding
    {
        get => GetValue(PopupContentPaddingProperty);
        set => SetValue(PopupContentPaddingProperty, value);
    }

    internal bool UsePopupLayoutTokenResources
    {
        get => GetValue(UsePopupLayoutTokenResourcesProperty);
        set => SetValue(UsePopupLayoutTokenResourcesProperty, value);
    }

    internal FormValidateFeedback? FormFeedback
    {
        get => GetValue(FormFeedbackProperty);
        set => SetValue(FormFeedbackProperty, value);
    }

    internal FormValidateStatus FormStatus
    {
        get => GetValue(FormStatusProperty);
        private set => SetCurrentValue(FormStatusProperty, value);
    }

    private bool _isFormFeedbackVisible;

    internal bool IsFormFeedbackVisible
    {
        get => _isFormFeedbackVisible;
        private set => SetAndRaise(IsFormFeedbackVisibleProperty, ref _isFormFeedbackVisible, value);
    }

    private bool _isEffectiveFilterEnabled;

    internal bool IsEffectiveFilterEnabled
    {
        get => _isEffectiveFilterEnabled;
        private set => SetAndRaise(IsEffectiveFilterEnabledProperty, ref _isEffectiveFilterEnabled, value);
    }

    private bool _isEffectiveEmptyVisible;

    internal bool IsEffectiveEmptyVisible
    {
        get => _isEffectiveEmptyVisible;
        private set => SetAndRaise(IsEffectiveEmptyVisibleProperty, ref _isEffectiveEmptyVisible, value);
    }

    private bool _isDefaultEmptyIndicatorVisible;

    internal bool IsDefaultEmptyIndicatorVisible
    {
        get => _isDefaultEmptyIndicatorVisible;
        private set => SetAndRaise(IsDefaultEmptyIndicatorVisibleProperty, ref _isDefaultEmptyIndicatorVisible, value);
    }

    private bool _canClearValue;

    internal bool CanClearValue
    {
        get => _canClearValue;
        private set => SetAndRaise(CanClearValueProperty, ref _canClearValue, value);
    }

    internal bool IsEffectiveShowClearButton
    {
        get => GetValue(IsEffectiveShowClearButtonProperty);
        set => SetValue(IsEffectiveShowClearButtonProperty, value);
    }

    internal bool ShouldDeferFilterProjection =>
        !IsPopupPinnedOpen &&
        (_selectionCommitDepth > 0 ||
         (!IsDropDownOpen && _popup?.IsPlayingCloseMotion == true));

    #endregion

    private const string ContentLeftAddOnPresenterPart = "PART_ContentLeftAddOnPresenter";
    private const string ContentRightAddOnPresenterPart = "PART_ContentRightAddOnPresenter";

    private static readonly CandidatePopupLayoutTokenResourceKeys s_popupLayoutTokenResourceKeys =
        new(ComboBoxTokenKind.ItemHeight,
            ComboBoxTokenKind.PopupContentPadding);
    private OwnerTokenSet? _popupLayoutTokens;
    private IDisposable? _rowHeightStyleValue;
    private IDisposable? _popupPaddingStyleValue;
    private double? _themedRowHeight;
    private Thickness? _themedPopupPadding;
    private bool _updatingPopupLayout;

    private Popup? _popup;
    private IDisposable? _deactivationSubscription;
    private IDisposable? _popupPinnedOpenBinding;
    private ComboBoxHandle? _comboBoxHandle;
    private IconButton? _clearButton;
    private AddOnDecoratedBox? _addOnDecoratedBox;
    private AvaloniaTextBox? _editableTextBox;
    private IDisposable? _editableTextBoxTextSubscription;
    private TextBlock? _editableTextBoxPlaceholder;
    private int _popupLifecycleCloseDepth;
    private TextPresenter? _editableTextBoxPresenter;
    private IDisposable? _editableTextBoxPreeditTextSubscription;
    private string? _editableTextBeforeUserEditKeyDown;
    private IDisposable? _feedbackStatusSubscription;
    private int _candidateSelectedIndex = -1;
    private ComboBoxItem? _candidateSelectedItem;
    private readonly Dictionary<ComboBoxItem, FilterVisibilityState> _filterItemVisibilityContext = new();
    private FilterProjection _presentedFilterProjection;
    // A refresh requested while the leaving view was frozen; Popup.Closed publishes it.
    private bool _hasDeferredFilterRefresh;
    private EventHandler? _formValueChanged;
    // Selection commits include nested display-text updates; their view projection
    // is published after callbacks and the popup's physical closing boundary.
    private int _selectionCommitDepth;

    static ComboBox()
    {
        ItemsPanelProperty.OverrideDefaultValue<ComboBox>(
            new FuncTemplate<Panel?>(() => new ComboBoxVirtualizingStackPanel()));
        TextInputEvent.AddClassHandler<ComboBox>(
            (box, args) => box.HandleEditableTextBoxTextInput(args),
            RoutingStrategies.Tunnel);
        KeyDownEvent.AddClassHandler<ComboBox>(
            (box, args) => box.CaptureEditableTextBoxUserEditKeyDown(args),
            RoutingStrategies.Tunnel);
        KeyDownEvent.AddClassHandler<ComboBox>(
            (box, args) => box.HandleEditableTextBoxUserEditKeyDown(args),
            RoutingStrategies.Bubble,
            handledEventsToo: true);
    }

    public ComboBox()
    {
        SelectionBoxItemProperty.Changed.AddClassHandler<ComboBox>((box, args) => box.NotifyFormValueChanged(args.NewValue));
        Items.CollectionChanged += HandleItemsCollectionChanged;
        ContainerPrepared       += HandleContainerPrepared;

        // Stop BringIntoView requests raised while the drop-down is open; they would scroll the page.
        AddHandler(RequestBringIntoViewEvent, (sender, e) =>
        {
            // Cancel the request when the drop-down is open and its target is not the control itself.
            if (IsDropDownOpen && e.TargetObject != this)
            {
                e.Handled = true;
            }
        }, handledEventsToo: true);
    }

    /// <summary>
    /// Sets a contains filter when <see cref="Filter" /> is <see langword="null" />, then applies the filter
    /// state to the option items. The base method runs first.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (Filter == null)
        {
            SetCurrentValue(FilterProperty, ValueFilterFactory.BuildFilter(ValueFilterMode.Contains));
        }

        ConfigureEffectiveFilterEnabled();
        RefreshFilteredItemVisibility();
    }

    /// <summary>
    /// Detaches the clear button handler of the previous template before the base method runs.
    /// </summary>
    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs change)
    {
        DetachClearButton();
        base.OnTemplateChanged(change);
    }
    
    /// <summary>
    /// Resolves the template parts and connects them: the frame, the editable text box, the drop-down popup
    /// (pin relay, open and close handlers), the arrow handle and the clear button. Handlers and subscriptions
    /// of the previous template are released first. When <c>IsDropDownOpen</c> is already
    /// <see langword="true" />, the popup is opened here, after the base method has run.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        this.SetPopup(null); // Clears the base class popup to stop its pointer click handling.

        if (_popup != null)
        {
            _popupPinnedOpenBinding?.Dispose();
            _popupPinnedOpenBinding               =  null;
            _popup.Opened                         -= HandlePopupOpened;
            _popup.Closed                         -= HandlePopupClosed;
            _popup.OverlayInputPassThroughElement =  null;
        }

        if (_editableTextBox != null)
        {
            _editableTextBox.TemplateApplied -= HandleEditableTextBoxTemplateApplied;
        }
        _editableTextBoxTextSubscription?.Dispose();
        _editableTextBoxTextSubscription = null;
        ClearEditableTextBoxPlaceholderParts();

        _addOnDecoratedBox = e.NameScope.Find<AddOnDecoratedBox>(AddOnDecoratedBox.AddOnDecoratedBoxPart);
        _editableTextBox   = e.NameScope.Find<AvaloniaTextBox>("PART_EditableTextBox");
        _popup             = e.NameScope.Find<Popup>("PART_Popup");
        // Root BorderBrush / Background reach the input frame through the template's HostBorderBrush / HostBackground
        // bindings, which the frame theme projects over its state brushes (antd styles.root).
        if (_popup != null)
        {
            // Popup.IsPopupPinnedOpen is internal and cannot be targeted by compiled AXAML.
            // This relay is disposed and recreated with the template Popup.
            _popupPinnedOpenBinding = BindUtils.RelayBind(
                this,
                IsPopupPinnedOpenProperty,
                _popup,
                Popup.IsPopupPinnedOpenProperty);
            _popup.Opened += HandlePopupOpened;
            _popup.Closed += HandlePopupClosed;
            // The shared Popup owns effective light-dismiss and its registration.
        }
        if (_editableTextBox != null)
        {
            _editableTextBox.TemplateApplied += HandleEditableTextBoxTemplateApplied;
            _editableTextBoxTextSubscription = _editableTextBox.GetObservable(AvaloniaTextBox.TextProperty)
                                                               .Subscribe(_ => ConfigureEditableTextBoxPlaceholderVisibility());
        }
        ConfigureOverlayInputPassThroughElement();
        ConfigureBaseEditableTextBoxFocusBehavior();

        if (_comboBoxHandle != null)
        {
            _comboBoxHandle.HandleClick -= HandleOpenPopupClicked;
        }

        _comboBoxHandle = e.NameScope.Find<ComboBoxHandle>("PART_ComboBoxHandle");

        if (_comboBoxHandle != null)
        {
            _comboBoxHandle.HandleClick += HandleOpenPopupClicked;
        }

        DetachClearButton();
        _clearButton = e.NameScope.Find<IconButton>("PART_ClearButton");
        if (_clearButton is not null)
        {
            _clearButton.Click += HandleClearButtonClicked;
        }

        UpdateCanClearValue();

        UpdatePseudoClasses();
        ConfigureMaxDropdownHeight();
        RefreshFilteredItemVisibility();

        // The business state may already be open when the template is applied, for example when a preview
        // declares IsDropDownOpen="True" in AXAML. The popup is opened only here, after the mask has been
        // suppressed; no template binding opens it any more, so this step is required.
        if (IsDropDownOpen && _popup is { IsOpen: false })
        {
            _popup.IsOpen = true;
        }
    }
    
    /// <summary>
    /// Subscribes to the deactivation of the hosting window, and opens the drop-down when
    /// <see cref="IsPopupPinnedOpen" /> is set while the drop-down is closed. The base method runs first.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _deactivationSubscription =
            TopLevelDeactivation.Subscribe(TopLevel.GetTopLevel(this), HandleWindowDeactivated);
        if (IsPopupPinnedOpen && !IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, true);
        }
    }

    /// <summary>
    /// Refreshes the subscription to the form feedback once the control is in the logical tree.
    /// The base method runs first.
    /// </summary>
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        ConfigureFormFeedbackSubscription();
    }

    /// <summary>
    /// Closes the drop-down (a pinned popup is not re-opened) and runs the base method, then disposes the
    /// window deactivation subscription and clears the candidate selection and the filter state of the item
    /// containers.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _popupLifecycleCloseDepth++;
        try
        {
            _popup?.CloseForLifecycle();
            SetCurrentValue(IsDropDownOpenProperty, false);
        }
        finally
        {
            _popupLifecycleCloseDepth--;
        }

        base.OnDetachedFromVisualTree(e);
        _deactivationSubscription?.Dispose();
        _deactivationSubscription = null;
        ClearCandidateItemSelection();
        ClearFilteredItemVisibility();
    }

    /// <summary>
    /// Disposes the form feedback subscription after the base method ran.
    /// </summary>
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        _feedbackStatusSubscription?.Dispose();
        _feedbackStatusSubscription = null;
    }

    /// <summary>
    /// Returns a new <see cref="ComboBoxItem" /> with the semantic item marker for an item that needs a
    /// container.
    /// </summary>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var comboBoxItem = new ComboBoxItem();
        ApplySemanticItemMarker(comboBoxItem);
        return comboBoxItem;
    }
    
    /// <summary>
    /// Returns <see langword="true" /> for an item that is not a <see cref="ComboBoxItem" />, with
    /// <paramref name="recycleKey" /> set to <c>DefaultRecycleKey</c>. For a <see cref="ComboBoxItem" /> the
    /// result is <see langword="false" /> and the key is <see langword="null" />.
    /// </summary>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        if (item is ComboBoxItem)
        {
            recycleKey = null;
            return false;
        }

        recycleKey = DefaultRecycleKey;
        return true;
    }

    /// <summary>
    /// Prepares a <see cref="ComboBoxItem" /> container after the base method: a non-visual item
    /// becomes the content when the container has none; <c>ItemTemplate</c>, <see cref="SizeType" />,
    /// <see cref="OptionFontSize" />, <see cref="IsMotionEnabled" /> and <c>ItemHeight</c> are bound to the
    /// container; then the current filter state and the semantic item marker are applied.
    /// </summary>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is ComboBoxItem comboBoxItem)
        {
            if (item != null && item is not Visual)
            {
                if (!comboBoxItem.IsSet(ComboBoxItem.ContentProperty))
                {
                    comboBoxItem.SetCurrentValue(ComboBoxItem.ContentProperty, item);
                }
            }
            
            if (ItemTemplate != null)
            {
                comboBoxItem[!ComboBoxItem.ContentTemplateProperty] = this[!ItemTemplateProperty];
            }
            
            comboBoxItem[!ComboBoxItem.SizeTypeProperty]        = this[!SizeTypeProperty];
            comboBoxItem[!ComboBoxItem.FontSizeProperty]        = this[!OptionFontSizeProperty];
            comboBoxItem[!ComboBoxItem.IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
            comboBoxItem[!ComboBoxItem.HeightProperty]          = this[!ItemHeightProperty];

            ApplyCurrentFilterState(comboBoxItem, item);
            // NeedsContainerOverride returns false for an explicitly supplied ComboBoxItem, so that container
            // never goes through CreateContainerForItemOverride. The same marker is injected here as a
            // fallback; adding it twice is idempotent.
            ApplySemanticItemMarker(comboBoxItem);
        }
    }

    /// <summary>
    /// Resets a container for reuse before the base method clears it: the transition initialization state,
    /// the candidate selection reference, and the candidate and filter state of a
    /// <see cref="ComboBoxItem" />.
    /// </summary>
    protected override void ClearContainerForItemOverride(Control element)
    {
        if (element is ITransitionInitializationOwner owner)
        {
            owner.TransitionInitialization.Reset();
        }

        if (element is ComboBoxItem comboBoxItem)
        {
            if (ReferenceEquals(_candidateSelectedItem, comboBoxItem))
            {
                _candidateSelectedItem = null;
            }

            SetCandidateItemSelectedIfChanged(comboBoxItem, false);
            ClearFilterState(comboBoxItem);
        }

        base.ClearContainerForItemOverride(element);
    }

    /// <summary>
    /// Runs the base handler, then keeps the derived state in step with the change: the drop-down popup
    /// follows <c>IsDropDownOpen</c> (a pinned popup is re-opened), the candidate view follows selection,
    /// text and filter changes, the pseudo-classes follow the status and the maximum drop-down height
    /// follows the page size and popup layout. A selection made while the drop-down is open closes it.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        // Avalonia fills Text from SelectedItem inside its base property handler.
        // That nested Text update must not re-filter the view before close is requested.
        var selectionPropertyCommit = change.Property == SelectedItemProperty &&
                                      change.NewValue is not null &&
                                      IsDropDownOpen && !IsPopupPinnedOpen;
        if (selectionPropertyCommit)
        {
            _selectionCommitDepth++;
        }

        try
        {
            base.OnPropertyChanged(change);

            if (change.Property == SelectedIndexProperty ||
                change.Property == TextProperty ||
                change.Property == IsEditableProperty)
            {
                UpdateCanClearValue();
            }

            if (change.Property == IsDropDownOpenProperty &&
                !IsDropDownOpen &&
                IsPopupPinnedOpen &&
                _popupLifecycleCloseDepth == 0)
            {
                SetCurrentValue(IsDropDownOpenProperty, true);
                return;
            }

            if (change.Property == StatusProperty ||
                change.Property == FormStatusProperty ||
                change.Property == DataValidationErrors.HasErrorsProperty ||
                change.Property == DataValidationErrors.ErrorsProperty)
            {
                UpdatePseudoClasses();
            }
            else if (change.Property == UsePopupLayoutTokenResourcesProperty)
            {
                UpdatePopupLayoutTokenSubscription();
            }
            else if (change.Property == DropDownDisplayPageSizeProperty ||
                     change.Property == ItemHeightProperty ||
                     change.Property == PopupContentPaddingProperty)
            {
                if (!_updatingPopupLayout)
                {
                    ConfigureMaxDropdownHeight();
                }
            }
            else if (change.Property == SelectedItemProperty &&
                     change.NewValue != null &&
                     _selectionCommitDepth == (selectionPropertyCommit ? 1 : 0))
            {
                // Close dropdown when an item is selected
                if (IsDropDownOpen)
                {
                    SetCurrentValue(IsDropDownOpenProperty, false);
                }
            }
            else if (change.Property == IsDropDownOpenProperty)
            {
                ClearCandidateItemSelection();
                if (IsDropDownOpen)
                {
                    RefreshStaleFilteredItemVisibility();
                    OpenPopup();
                    MoveFocusToInputForDropDown();
                }
                else
                {
                    ClosePopup();
                }
            }
            else if (change.Property == IsPopupPinnedOpenProperty)
            {
                if (change.GetNewValue<bool>() && !IsDropDownOpen)
                {
                    SetCurrentValue(IsDropDownOpenProperty, true);
                }
            }
            else if (change.Property == FormFeedbackProperty)
            {
                ConfigureFormFeedbackSubscription();
            }
            else if (change.Property == IsEditableProperty ||
                     change.Property == IsFilterEnabledProperty)
            {
                ConfigureEffectiveFilterEnabled();
                ConfigureBaseEditableTextBoxFocusBehavior();
                SyncFilterValueFromText();
                ClearCandidateItemSelection();
                RefreshFilteredItemVisibility();
                ConfigureOverlayInputPassThroughElement();
            }
            else if (change.Property == TextProperty)
            {
                SyncFilterValueFromText();
                ConfigureEditableTextBoxPlaceholderVisibility();
                ClearCandidateItemSelection();
                RefreshStaleFilteredItemVisibility();
            }
            else if (change.Property == FilterValueProperty ||
                     change.Property == FilterProperty ||
                     change.Property == FilterValueSelectorProperty ||
                     change.Property == DisplayMemberBindingProperty ||
                     change.Property == TextSearch.TextBindingProperty)
            {
                ClearCandidateItemSelection();
                RefreshFilteredItemVisibility();
            }
            else if (change.Property == IsShowEmptyIndicatorProperty)
            {
                RefreshFilteredItemVisibility();
            }
            else if (change.Property == EmptyIndicatorProperty ||
                     change.Property == EmptyIndicatorTemplateProperty)
            {
                ConfigureEmptyIndicator();
            }
        }
        finally
        {
            if (selectionPropertyCommit)
            {
                _selectionCommitDepth--;
                RefreshStaleFilteredItemVisibility();
            }
        }
    }

    /// <summary>
    /// Hands a focus that arrives from outside to the editable text box. The base handler is skipped when
    /// the focus goes to the clear button or to add-on content, which are keyboard stops of their own; a
    /// focus inside the drop-down popup is left as it is.
    /// </summary>
    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        // The clear button (Ant Design renders it as a button) and focusable add-on content are keyboard stops of
        // their own. The base editable ComboBox hands any focus inside the control to its input text box, so it must
        // not see these; the control itself is not focused here, so nothing else in the base handler applies.
        if (IsOwnKeyboardStop(e.NewFocusedElement))
        {
            return;
        }

        base.OnGotFocus(e);
        if (e.NewFocusedElement is Visual focusedVisual &&
            _popup?.IsInsidePopup(focusedVisual) == true)
        {
            return;
        }

        FocusEditableTextBox(e.NavigationMethod);
    }

    /// <summary>
    /// Lets the open drop-down handle a key first while the control is enabled; the base handler is
    /// skipped when the key was consumed.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!e.Handled && IsEnabled && IsDropDownOpen && HandleOpenDropDownKeyDown(e))
        {
            return;
        }

        base.OnKeyDown(e);
    }
    
    /// <summary>
    /// Sets the <c>:pressed</c> pseudo-class, which <see cref="OnPointerReleased" /> reads to toggle the
    /// drop-down. It is set only while the drop-down is closed.
    /// </summary>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (!IsDropDownOpen)
        {
            PseudoClasses.Set(StdPseudoClass.Pressed, true);
        }
    }

    /// <summary>
    /// Moves the candidate selection to the option item under the mouse pointer while the drop-down is
    /// open. The base method runs first.
    /// </summary>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (IsDropDownOpen && e.Pointer.Type == PointerType.Mouse &&
            GetContainerFromEventSource(e.Source) is ComboBoxItem comboBoxItem)
        {
            TrySetCandidateFromContainer(comboBoxItem);
        }
    }
    
    /// <summary>
    /// Raises the context request for a right click on the control itself, toggles the drop-down for a
    /// release inside the control but outside the popup, and clears the <c>:pressed</c> pseudo-class at
    /// the end. The base method is not called.
    /// </summary>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (e.Source == this
            && !e.Handled
            && e.InitialPressMouseButton == MouseButton.Right)
        {
            var args = new ContextRequestedEventArgs(e);
            RaiseEvent(args);
            e.Handled = args.Handled;
        }
        if (!e.Handled && e.Source is Visual source)
        {
            // Check if the click is inside the popup
            if (_popup?.IsInsidePopup(source) != true && PseudoClasses.Contains(StdPseudoClass.Pressed))
            {
                var shouldOpen = !IsDropDownOpen;
                SetCurrentValue(IsDropDownOpenProperty, shouldOpen);
                if (shouldOpen)
                {
                    FocusEditableTextBox(NavigationMethod.Pointer);
                }
            }
        }
        PseudoClasses.Set(StdPseudoClass.Pressed, false);
    }

    /// <summary>
    /// Commits the selection of a container through the base method and closes the drop-down when that
    /// succeeded, which is also when <see langword="true" /> is returned. The filtered view is not
    /// published during the commit; a refresh that became due is published afterwards.
    /// </summary>
    public override bool UpdateSelectionFromEvent(Control container, RoutedEventArgs eventArgs)
    {
        _selectionCommitDepth++;
        try
        {
            var handled = base.UpdateSelectionFromEvent(container, eventArgs);
            if (handled && IsDropDownOpen)
            {
                SetCurrentValue(IsDropDownOpenProperty, false);
            }

            return handled;
        }
        finally
        {
            _selectionCommitDepth--;
            RefreshStaleFilteredItemVisibility();
        }
    }

    /// <summary>
    /// Keeps the minimum width of the drop-down popup equal to the new width of the control. The base
    /// method runs first.
    /// </summary>
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        EffectivePopupWidth = e.NewSize.Width;
    }

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
    /// Notifies the form that the value of the control changed. It runs whenever <c>SelectionBoxItem</c>
    /// changes, after the new value is stored; <paramref name="value" /> is that new item.
    /// </summary>
    protected virtual void NotifyFormValueChanged(object? value)
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Applies a value handed over by a form: <paramref name="value" /> becomes <c>SelectedItem</c>.
    /// <see langword="null" /> clears the selection.
    /// </summary>
    protected virtual void NotifySetFormValue(object? value)
    {
        SelectedItem = value;
    }

    /// <summary>
    /// Returns the selected item as the value handed to a form, or <see langword="null" /> when nothing is
    /// selected.
    /// </summary>
    protected virtual object? NotifyGetFormValue()
    {
        return SelectedItem;
    }

    /// <summary>
    /// Clears the selection when a form resets the control.
    /// </summary>
    protected virtual void NotifyClearFormValue()
    {
        SelectedItem = null;
    }

    /// <summary>
    /// Stores a validation status handed over by a form in <c>FormStatus</c>, unless it is the current
    /// status.
    /// </summary>
    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
        if (FormStatus != status)
        {
            SetCurrentValue(FormStatusProperty, status);
        }
    }
    
    /// <summary>
    /// Stores the feedback control handed over by a form in <c>FormFeedback</c>.
    /// <see langword="null" /> removes it.
    /// </summary>
    protected virtual void NotifySetFeedBackControl(FormValidateFeedback? value)
    {
        FormFeedback = value;
    }
    #endregion

    private static void ApplySemanticItemMarker(ComboBoxItem comboBoxItem)
    {
        comboBoxItem.Classes.Add(ComboBoxSemanticParts.PopupListItemClass);
    }

    private void HandleOpenPopupClicked(object? sender, EventArgs e)
    {
        var shouldOpen = !IsDropDownOpen;
        SetCurrentValue(IsDropDownOpenProperty, shouldOpen);
        if (shouldOpen)
        {
            FocusEditableTextBox(NavigationMethod.Pointer);
        }
    }

    private void UpdateCanClearValue()
    {
        CanClearValue = SelectedIndex >= 0 || (IsEditable && !string.IsNullOrEmpty(Text));
    }

    // Keyboard stops inside the control that are not its input: the clear button and add-on content.
    private bool IsOwnKeyboardStop(IInputElement? element)
    {
        if (element is not Visual visual)
        {
            return false;
        }

        if (_clearButton is not null && ReferenceEquals(visual, _clearButton))
        {
            return true;
        }

        for (var current = visual.GetVisualParent(); current is not null && !ReferenceEquals(current, this);
             current = current.GetVisualParent())
        {
            if (current is ContentPresenter
                {
                    Name: AddOnDecoratedBoxThemeConstants.LeftAddOnPresenterPart or
                          AddOnDecoratedBoxThemeConstants.RightAddOnPresenterPart or
                          ContentLeftAddOnPresenterPart or
                          ContentRightAddOnPresenterPart
                })
            {
                return true;
            }
        }

        return false;
    }

    // The open dropdown's keys (Up / Down / Enter / Escape) are handled by the input. When it opens with focus on the
    // clear button or add-on content (Alt+Down / F4 bubble up from there), that button would take Enter and Space
    // first, so focus moves to the input; a keyboard opening keeps the focus visual.
    private void MoveFocusToInputForDropDown()
    {
        if (TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() is not InputElement focused ||
            !IsOwnKeyboardStop(focused))
        {
            return;
        }

        var navigationMethod = focused.Classes.Contains(":focus-visible")
            ? NavigationMethod.Tab
            : NavigationMethod.Unspecified;
        if (IsEditable)
        {
            FocusEditableTextBox(navigationMethod);
        }
        else
        {
            Focus(navigationMethod);
        }
    }

    private void DetachClearButton()
    {
        if (_clearButton is not null)
        {
            _clearButton.Click -= HandleClearButtonClicked;
            _clearButton = null;
        }
    }

    private void HandleClearButtonClicked(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        if (!IsAllowClear || !IsEffectivelyEnabled || !CanClearValue)
        {
            return;
        }

        // Keyboard activation keeps focus visible on the restored target; the button hides once cleared.
        var navigationMethod = _clearButton?.Classes.Contains(":focus-visible") == true
            ? NavigationMethod.Tab
            : NavigationMethod.Pointer;

        // Null does not match an empty-string option in Avalonia's editable text matching.
        // Clear text first, then selection, while preserving application bindings.
        SetCurrentValue(TextProperty, null);
        SetCurrentValue(SelectedIndexProperty, -1);

        if (IsEditable)
        {
            FocusEditableTextBox(navigationMethod);
        }
        else
        {
            Focus(navigationMethod);
        }
    }

    private void HandleWindowDeactivated(object? sender, EventArgs e)
    {
        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    private void ConfigureFormFeedbackSubscription()
    {
        _feedbackStatusSubscription?.Dispose();
        _feedbackStatusSubscription = null;
        if (!((ILogical)this).IsAttachedToLogicalTree)
        {
            IsFormFeedbackVisible = false;
            return;
        }

        if (FormFeedback is { } feedback)
        {
            _feedbackStatusSubscription = feedback.GetObservable(FormValidateFeedback.ValidateStatusProperty)
                                                  .Subscribe(status => IsFormFeedbackVisible = status != FormValidateStatus.Default);
        }
        else
        {
            IsFormFeedbackVisible = false;
        }
    }

    private void UpdatePseudoClasses()
    {
        var effectiveStatus = InputControlState.ResolveEffectiveStatus(this, Status, FormStatus);
        PseudoClasses.Set(StdPseudoClass.Warning,
            effectiveStatus == InputControlStatus.Warning);
    }

    private void ConfigureMaxDropdownHeight()
    {
        SetCurrentValue(MaxDropDownHeightProperty,
            DropDownDisplayPageSize * ItemHeight + PopupContentPadding.Top + PopupContentPadding.Bottom);
    }

    private void UpdatePopupLayoutTokenSubscription()
    {
        _popupLayoutTokens?.Dispose();
        _popupLayoutTokens = UsePopupLayoutTokenResources ? s_popupLayoutTokenResourceKeys.Observe(this, RefreshPopupLayout) : null;
        RefreshPopupLayout();
    }

    private void RefreshPopupLayout()
    {
        var height = CandidatePopupLayoutTokenResourceKeys.ReadLength(_popupLayoutTokens?.GetValue(0), _themedRowHeight);
        var rawPadding = _popupLayoutTokens?.GetValue(1);
        Thickness? padding = rawPadding is null || ReferenceEquals(rawPadding, AvaloniaProperty.UnsetValue)
            ? null
            : CandidatePopupLayoutTokenResourceKeys.ReadPadding(rawPadding, _themedPopupPadding ?? default);
        var previous = (ItemHeight, PopupContentPadding);
        _updatingPopupLayout = true;
        try
        {
            // ItemHeight still feeds the realized item bindings. Both template values keep their theme priority.
            SetPopupLayoutValue(ItemHeightProperty, height, ref _themedRowHeight, ref _rowHeightStyleValue);
            SetPopupLayoutValue(PopupContentPaddingProperty, padding, ref _themedPopupPadding, ref _popupPaddingStyleValue);
        }
        finally
        {
            _updatingPopupLayout = false;
        }
        if (!previous.Equals((ItemHeight, PopupContentPadding)))
        {
            ConfigureMaxDropdownHeight();
        }
    }

    private void SetPopupLayoutValue<T>(
        StyledProperty<T> property, T? value, ref T? previous, ref IDisposable? styleValue)
        where T : struct
    {
        if (Nullable.Equals(previous, value))
        {
            return;
        }
        var oldStyleValue = styleValue;
        styleValue = value is { } resolved ? SetValue(property, resolved, BindingPriority.Style) : null;
        previous = value;
        oldStyleValue?.Dispose();
    }

    private void ConfigureOverlayInputPassThroughElement()
    {
        if (_popup != null)
        {
            _popup.OverlayInputPassThroughElement = _addOnDecoratedBox;
        }
    }

    /// <summary>
    /// Opens the popup from code, following <c>IsDropDownOpen</c>. The template no longer binds <c>IsOpen</c>:
    /// a binding evaluated during template inflation opened the popup before the mask was suppressed, which
    /// left the mask in place for a pinned popup. The shared <c>AbstractSelect</c> follows the same
    /// convention.
    /// </summary>
    private void OpenPopup()
    {
        if (_popup is { IsOpen: false })
        {
            _popup.IsOpen = true;
        }
    }

    private void ClosePopup()
    {
        if (_popup is { IsOpen: true })
        {
            _popup.IsOpen = false;
        }
    }

    /// <summary>
    /// Clears the open state of the control when the popup closed itself (a light-dismiss outside click or
    /// Escape). A TwoWay <c>IsOpen</c> binding in the template used to write this back; now that opening and
    /// closing are handled in code, it must be done explicitly, or the state stays open.
    /// </summary>
    private void HandlePopupClosed(object? sender, EventArgs e)
    {
        // Business open is already false during exit motion. Publish the latest query after the physical
        // host has finished closing, but only when one is pending.
        RefreshStaleFilteredItemVisibility();
        if (!IsDropDownOpen)
        {
            return;
        }

        if (IsPopupPinnedOpen && _popupLifecycleCloseDepth == 0)
        {
            // Pinned semantics: an outside click or Escape does not change the effective open state. The
            // shared Popup suppresses the physical close and converges again while it is pinned, so this
            // branch is normally not reached. If it is reached because the anchor or the TopLevel became
            // invalid, the requested open state is kept by design and the business state is not written;
            // a reattach or a pin change opens the popup again.
            return;
        }

        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    private void ConfigureBaseEditableTextBoxFocusBehavior()
    {
        if (_editableTextBox == null)
        {
            return;
        }

        this.SetInputTextBox(IsEffectiveFilterEnabled ? null : _editableTextBox);
    }

    private void HandleEditableTextBoxTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        ClearEditableTextBoxPlaceholderParts();

        _editableTextBoxPlaceholder = e.NameScope.Find<TextBlock>("Placeholder");
        _editableTextBoxPresenter   = e.NameScope.Find<TextPresenter>("PART_TextPresenter");

        if (_editableTextBoxPresenter is not null)
        {
            _editableTextBoxPreeditTextSubscription =
                _editableTextBoxPresenter.GetObservable(TextPresenter.PreeditTextProperty)
                                         .Subscribe(text =>
                                         {
                                             if (_editableTextBox is { } editor)
                                             {
                                                 AtomUI.Theme.ThemeCompositionProtection.Track(editor, !string.IsNullOrEmpty(text));
                                             }
                                             ConfigureEditableTextBoxPlaceholderVisibility();
                                         });
        }

        ConfigureEditableTextBoxPlaceholderVisibility();
    }

    private void ClearEditableTextBoxPlaceholderParts()
    {
        if (_editableTextBox is { } editor)
        {
            AtomUI.Theme.ThemeCompositionProtection.Track(editor, false);
        }
        _editableTextBoxPreeditTextSubscription?.Dispose();
        _editableTextBoxPreeditTextSubscription = null;
        _editableTextBoxPlaceholder             = null;
        _editableTextBoxPresenter               = null;
    }

    private void ConfigureEditableTextBoxPlaceholderVisibility()
    {
        if (_editableTextBoxPlaceholder is null)
        {
            return;
        }

        _editableTextBoxPlaceholder.SetCurrentValue(Visual.IsVisibleProperty,
            string.IsNullOrEmpty(_editableTextBox?.Text) &&
            string.IsNullOrEmpty(_editableTextBoxPresenter?.PreeditText));
    }

    private void FocusEditableTextBox(NavigationMethod navigationMethod)
    {
        if (!IsEditable || _editableTextBox == null)
        {
            return;
        }

        var wasAlreadyFocused = _editableTextBox.IsFocused;
        if (!wasAlreadyFocused)
        {
            _editableTextBox.Focus(navigationMethod);
        }

        if (wasAlreadyFocused)
        {
            CollapseEditableFilterFullSelection();
        }
        else
        {
            NormalizeEditableFilterInitialFocusSelection();
        }
    }

    private void NormalizeEditableFilterInitialFocusSelection()
    {
        if (!CollapseEditableFilterFullSelection())
        {
            MoveEditableFilterInitialCaretToTextEnd();
        }
    }

    private bool CollapseEditableFilterFullSelection()
    {
        if (!IsEffectiveFilterEnabled || _editableTextBox == null)
        {
            return false;
        }

        var textLength = _editableTextBox.Text?.Length ?? 0;
        if (textLength == 0)
        {
            return false;
        }

        var selectionStart = _editableTextBox.SelectionStart;
        var selectionEnd   = _editableTextBox.SelectionEnd;
        var isFullSelection = (selectionStart == 0 && selectionEnd == textLength) ||
                              (selectionStart == textLength && selectionEnd == 0);
        if (!isFullSelection)
        {
            return false;
        }

        _editableTextBox.SelectionStart = textLength;
        _editableTextBox.SelectionEnd   = textLength;
        _editableTextBox.CaretIndex     = textLength;
        return true;
    }

    private void MoveEditableFilterInitialCaretToTextEnd()
    {
        if (!IsEffectiveFilterEnabled || _editableTextBox == null)
        {
            return;
        }

        var textLength = _editableTextBox.Text?.Length ?? 0;
        if (textLength == 0 ||
            _editableTextBox.CaretIndex != 0 ||
            _editableTextBox.SelectionStart != 0 ||
            _editableTextBox.SelectionEnd != 0)
        {
            return;
        }

        _editableTextBox.SelectionStart = textLength;
        _editableTextBox.SelectionEnd   = textLength;
        _editableTextBox.CaretIndex     = textLength;
    }

    private void HandleItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ClearCandidateItemSelection();
        RefreshFilteredItemVisibility();
    }

    private void HandlePopupOpened(object? sender, EventArgs e)
    {
        RefreshFilteredItemVisibility();

        if (IsEditable)
        {
            FocusEditableTextBox(NavigationMethod.Unspecified);
        }
        else if (TopLevel.GetTopLevel(this)?.FocusManager.GetFocusedElement() is Visual focusedVisual &&
                 _popup?.IsInsidePopup(focusedVisual) == true)
        {
            Focus(NavigationMethod.Unspecified);
        }
    }

    private void HandleEditableTextBoxTextInput(TextInputEventArgs e)
    {
        if (!IsEffectiveFilterEnabled ||
            !ReferenceEquals(e.Source, _editableTextBox) ||
            _editableTextBox?.IsFocused != true ||
            IsDropDownOpen)
        {
            return;
        }

        SetCurrentValue(IsDropDownOpenProperty, true);
    }

    private void CaptureEditableTextBoxUserEditKeyDown(KeyEventArgs e)
    {
        _editableTextBeforeUserEditKeyDown = IsEditableTextBoxUserEditKey(e)
            ? _editableTextBox?.Text ?? string.Empty
            : null;
    }

    private void HandleEditableTextBoxUserEditKeyDown(KeyEventArgs e)
    {
        var textBefore = _editableTextBeforeUserEditKeyDown;
        _editableTextBeforeUserEditKeyDown = null;

        if (textBefore == null ||
            !IsEffectiveFilterEnabled ||
            !ReferenceEquals(e.Source, _editableTextBox) ||
            _editableTextBox?.IsFocused != true)
        {
            return;
        }

        var textAfter = _editableTextBox.Text ?? string.Empty;
        if (textAfter == textBefore)
        {
            return;
        }

        UpdateDropDownFromUserTextEdit(textAfter);
    }

    private bool IsEditableTextBoxUserEditKey(KeyEventArgs e)
    {
        if (!IsEffectiveFilterEnabled ||
            !ReferenceEquals(e.Source, _editableTextBox) ||
            _editableTextBox?.IsFocused != true)
        {
            return false;
        }

        var text            = _editableTextBox.Text ?? string.Empty;
        var selectionLength = Math.Abs(_editableTextBox.SelectionStart - _editableTextBox.SelectionEnd);
        var caretIndex      = _editableTextBox.CaretIndex;

        return e.Key switch
        {
            Key.Back   => selectionLength > 0 || caretIndex > 0,
            Key.Delete => selectionLength > 0 || caretIndex < text.Length,
            Key.X      => HasCommandModifier(e.KeyModifiers) && selectionLength > 0,
            Key.V      => HasCommandModifier(e.KeyModifiers),
            _          => false
        };
    }

    private void UpdateDropDownFromUserTextEdit(string text)
    {
        SetCurrentValue(IsDropDownOpenProperty, !string.IsNullOrEmpty(text));
    }

    private static bool HasCommandModifier(KeyModifiers modifiers)
    {
        return modifiers.HasFlag(KeyModifiers.Control) ||
               modifiers.HasFlag(KeyModifiers.Meta);
    }

    private bool HandleOpenDropDownKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Down:
                SelectNextCandidateItem();
                e.Handled = true;
                return true;

            case Key.Up:
                SelectPreviousCandidateItem();
                e.Handled = true;
                return true;

            case Key.Enter:
                CommitCandidateSelection();
                e.Handled = true;
                return true;

            case Key.Escape:
                ClearCandidateItemSelection();
                SetCurrentValue(IsDropDownOpenProperty, false);
                e.Handled = true;
                return true;

            default:
                return false;
        }
    }

    private void SelectNextCandidateItem()
    {
        SelectCandidateItem(1);
    }

    private void SelectPreviousCandidateItem()
    {
        SelectCandidateItem(-1);
    }

    private void SelectCandidateItem(int delta)
    {
        if (Items.Count == 0)
        {
            ClearCandidateItemSelection();
            return;
        }

        var startIndex = _candidateSelectedIndex;
        if (startIndex == -1)
        {
            startIndex = SelectedIndex != -1
                ? SelectedIndex
                : (delta > 0 ? -1 : Items.Count);
        }

        SetCandidateSelectedIndex(FindNextCandidateIndex(startIndex, delta));
    }

    private int FindNextCandidateIndex(int startIndex, int delta)
    {
        if (Items.Count == 0)
        {
            return -1;
        }

        var index = startIndex;
        for (var scanned = 0; scanned < Items.Count; scanned++)
        {
            index += delta;
            if (index >= Items.Count)
            {
                index = 0;
            }
            else if (index < 0)
            {
                index = Items.Count - 1;
            }

            if (IsCandidateItemSelectable(index))
            {
                return index;
            }
        }

        return -1;
    }

    private void SetCandidateSelectedIndex(int index)
    {
        if (SetCandidateSelectedIndexCore(index) && index != -1)
        {
            ScrollIntoView(index);
        }
    }

    private void SetCandidateSelectedIndexWithoutScroll(int index)
    {
        SetCandidateSelectedIndexCore(index);
    }

    private bool SetCandidateSelectedIndexCore(int index)
    {
        if (_candidateSelectedIndex == index)
        {
            if (index == -1)
            {
                ClearCandidateItemVisualSelection();
            }
            else
            {
                SelectCandidateItemVisual(index);
            }
            return false;
        }

        ClearCandidateItemVisualSelection();
        _candidateSelectedIndex = index;
        SelectCandidateItemVisual(_candidateSelectedIndex);

        return true;
    }

    private bool TrySetCandidateFromContainer(ComboBoxItem comboBoxItem)
    {
        var index = IndexFromContainer(comboBoxItem);
        if (index < 0 || !IsCandidateItemSelectable(index))
        {
            return false;
        }

        SetCandidateSelectedIndexWithoutScroll(index);
        return true;
    }

    private void ClearCandidateItemSelection()
    {
        SetCandidateSelectedIndex(-1);
    }

    private void SetCandidateItemSelected(int index, bool isSelected)
    {
        if (index < 0 || index >= Items.Count)
        {
            return;
        }

        if (GetCandidateItemContainer(index) is { } comboBoxItem)
        {
            SetCandidateItemSelectedIfChanged(comboBoxItem, isSelected);
        }
    }

    private void ClearCandidateItemVisualSelection()
    {
        if (_candidateSelectedItem != null)
        {
            SetCandidateItemSelectedIfChanged(_candidateSelectedItem, false);
            _candidateSelectedItem = null;
            return;
        }

        SetCandidateItemSelected(_candidateSelectedIndex, false);
    }

    private void SelectCandidateItemVisual(int index)
    {
        if (index < 0 || index >= Items.Count)
        {
            _candidateSelectedItem = null;
            return;
        }

        _candidateSelectedItem = GetCandidateItemContainer(index);
        if (_candidateSelectedItem != null)
        {
            SetCandidateItemSelectedIfChanged(_candidateSelectedItem, true);
        }
    }

    private static void SetCandidateItemSelectedIfChanged(ComboBoxItem comboBoxItem, bool isSelected)
    {
        if (comboBoxItem.IsCandidateSelected != isSelected)
        {
            comboBoxItem.SetCurrentValue(ComboBoxItem.IsCandidateSelectedProperty, isSelected);
        }
    }

    private void CommitCandidateSelection()
    {
        var index = _candidateSelectedIndex;
        if (index == -1 || !IsCandidateItemSelectable(index))
        {
            return;
        }

        _selectionCommitDepth++;
        try
        {
            var handled = false;
            if (GetCandidateItemContainer(index) is { } comboBoxItem)
            {
                handled = UpdateSelectionFromEvent(comboBoxItem, new RoutedEventArgs());
            }

            if (!handled)
            {
                SetCurrentValue(SelectedIndexProperty, index);
                if (IsDropDownOpen)
                {
                    SetCurrentValue(IsDropDownOpenProperty, false);
                }
            }
        }
        finally
        {
            _selectionCommitDepth--;
            RefreshStaleFilteredItemVisibility();
        }
    }

    private void EnsureCandidateItemSelectionIsValid()
    {
        if (_candidateSelectedIndex != -1 &&
            !IsCandidateItemSelectable(_candidateSelectedIndex))
        {
            ClearCandidateItemSelection();
        }
    }

    private bool IsCandidateItemSelectable(int index)
    {
        if (index < 0 || index >= Items.Count)
        {
            return false;
        }

        var item         = Items[index];
        var comboBoxItem = GetCandidateItemContainer(index);
        if (comboBoxItem != null)
        {
            // A virtualizing panel hides the ComboBoxItem items it recycles and shows them again when it realizes
            // them (ContainerFromIndex still returns such an item), so visibility only describes a realized
            // container; filtering is checked from the item.
            return (comboBoxItem.IsVisible || !IsRealizedContainer(comboBoxItem)) &&
                   comboBoxItem.IsEnabled &&
                   IsCurrentFilterMatched(item);
        }

        return item is not Control control
            ? IsCurrentFilterMatched(item)
            : control.IsVisible && control.IsEnabled && IsCurrentFilterMatched(item);
    }

    // Only a container the panel has realized has an index; a recycled ComboBoxItem item has none.
    private bool IsRealizedContainer(Control container) => IndexFromContainer(container) >= 0;

    private bool IsCurrentFilterMatched(object? item)
    {
        var filterValue    = FilterValue;
        var hasFilterValue = !string.IsNullOrEmpty(filterValue?.ToString());

        if (!IsEffectiveFilterEnabled || !hasFilterValue || Filter == null)
        {
            return true;
        }

        var       textBinding = TextSearch.GetTextBinding(this) ?? DisplayMemberBinding;
        using var evaluator   = BindingEvaluator<string?>.TryCreate(textBinding);
        return Filter.Filter(SelectFilterValue(item, evaluator, FilterValueSelector), filterValue);
    }

    private ComboBoxItem? GetCandidateItemContainer(int index)
    {
        return GetFilterItemContainer(index);
    }

    private void HandleContainerPrepared(object? sender, ContainerPreparedEventArgs args)
    {
        if (args.Container is ComboBoxItem comboBoxItem)
        {
            ApplyCurrentFilterState(comboBoxItem, Items[args.Index]);
            if (args.Index == _candidateSelectedIndex)
            {
                _candidateSelectedItem = comboBoxItem;
            }

            SetCandidateItemSelectedIfChanged(comboBoxItem, args.Index == _candidateSelectedIndex);
        }
    }

    private void ConfigureEffectiveFilterEnabled()
    {
        IsEffectiveFilterEnabled = IsEditable && IsFilterEnabled;
    }

    private void SyncFilterValueFromText()
    {
        if (!IsEffectiveFilterEnabled)
        {
            return;
        }

        var text = Text;
        SetCurrentValue(FilterValueProperty, string.IsNullOrEmpty(text) ? null : text);
    }

    private void RefreshFilteredItemVisibility()
    {
        if (ShouldDeferFilterProjection)
        {
            _hasDeferredFilterRefresh = true;
            return;
        }

        _hasDeferredFilterRefresh = false;
        var projection = GetCurrentFilterProjection();
        _presentedFilterProjection = projection;
        var hasVisibleMatch = Items.Count > 0;

        if (!projection.IsActive)
        {
            ClearFilteredItemVisibility();
            // Clearing forgets the projection; keep the inactive one as presented so an unchanged query is not stale.
            _presentedFilterProjection = projection;
        }
        else
        {
            using var evaluator = BindingEvaluator<string?>.TryCreate(projection.TextBinding);
            hasVisibleMatch = false;

            for (var index = 0; index < Items.Count; index++)
            {
                var item         = Items[index];
                var matched      = projection.Filter!.Filter(SelectFilterValue(item, evaluator, projection.ValueSelector), projection.Value);
                var comboBoxItem = GetFilterItemContainer(index);
                if (comboBoxItem != null)
                {
                    ApplyFilterState(comboBoxItem, matched);
                }

                hasVisibleMatch |= IsVisibleFilterMatch(item, comboBoxItem, matched);
            }
        }

        IsEffectiveEmptyVisible = IsShowEmptyIndicator && !hasVisibleMatch;
        ConfigureEmptyIndicator();
        EnsureCandidateItemSelectionIsValid();
    }

    // Selection commits, text echoes and popup open / close re-check the view after the query may have changed.
    // Re-running an unchanged query would call the filter for every item again (a pinned popup does not defer and
    // reopens after each selection, so every selection would pay for it several times); only a deferred or changed
    // query is published.
    private void RefreshStaleFilteredItemVisibility()
    {
        if (_hasDeferredFilterRefresh || GetCurrentFilterProjection() != _presentedFilterProjection)
        {
            RefreshFilteredItemVisibility();
        }
    }

    private void ConfigureEmptyIndicator()
    {
        IsDefaultEmptyIndicatorVisible = IsEffectiveEmptyVisible && EmptyIndicator is null && EmptyIndicatorTemplate is null;
    }

    private FilterProjection GetCurrentFilterProjection() => new(
        IsEffectiveFilterEnabled,
        FilterValue,
        Filter,
        FilterValueSelector,
        TextSearch.GetTextBinding(this) ?? DisplayMemberBinding);

    private void ApplyCurrentFilterState(ComboBoxItem comboBoxItem, object? item)
    {
        // Virtualization can prepare containers during a selection commit. They
        // still belong to the presented query, not to its pending replacement.
        var projection = ShouldDeferFilterProjection
            ? _presentedFilterProjection
            : GetCurrentFilterProjection();
        if (!projection.IsActive)
        {
            ClearFilterState(comboBoxItem);
            return;
        }

        using var evaluator = BindingEvaluator<string?>.TryCreate(projection.TextBinding);
        var matched = projection.Filter!.Filter(
            SelectFilterValue(item, evaluator, projection.ValueSelector), projection.Value);
        ApplyFilterState(comboBoxItem, matched);
    }

    private void ApplyFilterState(ComboBoxItem comboBoxItem, bool matched)
    {
        if (!_filterItemVisibilityContext.TryGetValue(comboBoxItem, out var originalVisibility))
        {
            originalVisibility = new FilterVisibilityState(
                comboBoxItem.IsSet(IsVisibleProperty),
                comboBoxItem.IsVisible);
            _filterItemVisibilityContext[comboBoxItem] = originalVisibility;
        }

        comboBoxItem.SetValue(IsVisibleProperty, matched && originalVisibility.Value);
    }

    private void ClearFilteredItemVisibility()
    {
        _presentedFilterProjection = default;
        var items = new List<ComboBoxItem>(_filterItemVisibilityContext.Keys);
        foreach (var item in items)
        {
            ClearFilterState(item);
        }
    }

    private void ClearFilterState(ComboBoxItem comboBoxItem)
    {
        if (_filterItemVisibilityContext.TryGetValue(comboBoxItem, out var originalVisibility))
        {
            if (originalVisibility.IsSet)
            {
                comboBoxItem.SetValue(IsVisibleProperty, originalVisibility.Value);
            }
            else
            {
                comboBoxItem.ClearValue(IsVisibleProperty);
            }
            _filterItemVisibilityContext.Remove(comboBoxItem);
        }
    }

    private ComboBoxItem? GetFilterItemContainer(int index)
    {
        if (ContainerFromIndex(index) is ComboBoxItem comboBoxItem)
        {
            return comboBoxItem;
        }

        return Items[index] as ComboBoxItem;
    }

    private static object? SelectFilterValue(
        object? item,
        BindingEvaluator<string?>? textBindingEvaluator,
        DefaultFilterValueSelector? selector)
    {
        if (selector != null)
        {
            return selector(item);
        }

        if (item is AvaloniaObject avaloniaObject)
        {
            var text = TextSearch.GetText(avaloniaObject);
            if (!string.IsNullOrEmpty(text))
            {
                return text;
            }
        }

        if (textBindingEvaluator != null)
        {
            var text = textBindingEvaluator.Evaluate(item);
            if (!string.IsNullOrEmpty(text))
            {
                return text;
            }
        }

        if (item is ContentControl contentControl)
        {
            return contentControl.Content?.ToString();
        }

        return item?.ToString();
    }

    private bool IsVisibleFilterMatch(object? item, ComboBoxItem? comboBoxItem, bool matched)
    {
        if (!matched)
        {
            return false;
        }

        if (comboBoxItem != null)
        {
            if (_filterItemVisibilityContext.TryGetValue(comboBoxItem, out var originalVisibility))
            {
                return originalVisibility.Value;
            }

            return comboBoxItem.IsVisible;
        }

        return item is not Control control || control.IsVisible;
    }

    private readonly record struct FilterProjection(
        bool IsEnabled,
        object? Value,
        IValueFilter? Filter,
        DefaultFilterValueSelector? ValueSelector,
        BindingBase? TextBinding)
    {
        public bool IsActive => IsEnabled && Filter is not null && !string.IsNullOrEmpty(Value?.ToString());
    }

    private readonly record struct FilterVisibilityState(bool IsSet, bool Value);
}
