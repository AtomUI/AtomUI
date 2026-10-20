using AtomUI.Animations;
﻿using System.Collections.Specialized;
using AtomUI.Controls;
using AtomUI.Controls.Utils;
using AtomUI.Data;
using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;

namespace AtomUI.Desktop.Controls;

using AvaloniaComboBox = Avalonia.Controls.ComboBox;
using AvaloniaTextBox = Avalonia.Controls.TextBox;

public partial class ComboBox : AvaloniaComboBox,
                        IMotionAwareControl,
                        ICustomizableSizeTypeAware,
                        IInputControlStatusAware,
                        IInputControlStyleVariantAware,
                        IFormItemAware,
                        IFormItemFeedbackAware
{
    #region 公共属性定义

    public static readonly StyledProperty<object?> LeftAddOnProperty =
        AddOnDecoratedBox.LeftAddOnProperty.AddOwner<ComboBox>();
    
    public static readonly StyledProperty<IDataTemplate?> LeftAddOnTemplateProperty =
        AddOnDecoratedBox.LeftAddOnTemplateProperty.AddOwner<ComboBox>();

    public static readonly StyledProperty<object?> RightAddOnProperty =
        AddOnDecoratedBox.RightAddOnProperty.AddOwner<ComboBox>();
    
    public static readonly StyledProperty<IDataTemplate?> RightAddOnTemplateProperty =
        AddOnDecoratedBox.RightAddOnTemplateProperty.AddOwner<ComboBox>();
    
    public static readonly StyledProperty<object?> ContentLeftAddOnProperty =
        AddOnDecoratedBox.ContentLeftAddOnProperty.AddOwner<ComboBox>();
    
    public static readonly StyledProperty<IDataTemplate?> ContentLeftAddOnTemplateProperty =
        AddOnDecoratedBox.ContentLeftAddOnTemplateProperty.AddOwner<ComboBox>();

    public static readonly StyledProperty<object?> ContentRightAddOnProperty =
        AddOnDecoratedBox.ContentRightAddOnProperty.AddOwner<ComboBox>();
    
    public static readonly StyledProperty<IDataTemplate?> ContentRightAddOnTemplateProperty =
        AddOnDecoratedBox.ContentRightAddOnTemplateProperty.AddOwner<ComboBox>();

    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<ComboBox>();

    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<ComboBox>();

    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<ComboBox>();

    public static readonly StyledProperty<bool> IsAllowClearProperty =
        TextBox.IsAllowClearProperty.AddOwner<ComboBox>();
    
    public static readonly StyledProperty<double> OptionFontSizeProperty =
        AvaloniaProperty.Register<ComboBox, double>(nameof(OptionFontSize));

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ComboBox>();
    
    public static readonly StyledProperty<int> DropDownDisplayPageSizeProperty =
        AvaloniaProperty.Register<ComboBox, int>(nameof (DropDownDisplayPageSize), 10);

    public static readonly StyledProperty<bool> ShouldUseOverlayPopupProperty =
        AvaloniaProperty.Register<ComboBox, bool>(nameof(ShouldUseOverlayPopup), true);

    /// <summary>
    /// 钉住候选弹层：开启后弹层忽略 light-dismiss 关闭请求，保持强制打开。
    /// 典型场景是 Gallery 语义部件预览需要持续高亮 popup.* 部件。
    /// </summary>
    public static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        Popup.IsPopupPinnedOpenProperty.AddOwner<ComboBox>();

    public static readonly StyledProperty<bool> IsFilterEnabledProperty =
        AvaloniaProperty.Register<ComboBox, bool>(nameof(IsFilterEnabled));

    public static readonly StyledProperty<bool> IsShowOverflowTipProperty =
        AvaloniaProperty.Register<ComboBox, bool>(nameof(IsShowOverflowTip), true);

    public static readonly StyledProperty<int> OverflowTipDelayProperty =
        AvaloniaProperty.Register<ComboBox, int>(nameof(OverflowTipDelay), 1200);

    public static readonly StyledProperty<PlacementMode> OverflowTipPlacementProperty =
        AvaloniaProperty.Register<ComboBox, PlacementMode>(nameof(OverflowTipPlacement), PlacementMode.TopEdgeAlignedLeft);

    public static readonly StyledProperty<IValueFilter?> FilterProperty =
        AvaloniaProperty.Register<ComboBox, IValueFilter?>(nameof(Filter));

    public static readonly StyledProperty<object?> FilterValueProperty =
        AvaloniaProperty.Register<ComboBox, object?>(nameof(FilterValue));

    public static readonly StyledProperty<DefaultFilterValueSelector?> FilterValueSelectorProperty =
        AvaloniaProperty.Register<ComboBox, DefaultFilterValueSelector?>(
            nameof(FilterValueSelector));

    public object? LeftAddOn
    {
        get => GetValue(LeftAddOnProperty);
        set => SetValue(LeftAddOnProperty, value);
    }
    
    public IDataTemplate? LeftAddOnTemplate
    {
        get => GetValue(LeftAddOnTemplateProperty);
        set => SetValue(LeftAddOnTemplateProperty, value);
    }

    public object? RightAddOn
    {
        get => GetValue(RightAddOnProperty);
        set => SetValue(RightAddOnProperty, value);
    }
    
    public IDataTemplate? RightAddOnTemplate
    {
        get => GetValue(RightAddOnTemplateProperty);
        set => SetValue(RightAddOnTemplateProperty, value);
    }
    
    public object? ContentLeftAddOn
    {
        get => GetValue(ContentLeftAddOnProperty);
        set => SetValue(ContentLeftAddOnProperty, value);
    }
    
    public IDataTemplate? ContentLeftAddOnTemplate
    {
        get => GetValue(ContentLeftAddOnTemplateProperty);
        set => SetValue(ContentLeftAddOnTemplateProperty, value);
    }

    public object? ContentRightAddOn
    {
        get => GetValue(ContentRightAddOnProperty);
        set => SetValue(ContentRightAddOnProperty, value);
    }
    
    public IDataTemplate? ContentRightAddOnTemplate
    {
        get => GetValue(ContentRightAddOnTemplateProperty);
        set => SetValue(ContentRightAddOnTemplateProperty, value);
    }

    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public InputControlStyleVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }

    public InputControlStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public bool IsAllowClear
    {
        get => GetValue(IsAllowClearProperty);
        set => SetValue(IsAllowClearProperty, value);
    }
    
    public double OptionFontSize
    {
        get => GetValue(OptionFontSizeProperty);
        set => SetValue(OptionFontSizeProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public int DropDownDisplayPageSize
    {
        get => GetValue(DropDownDisplayPageSizeProperty);
        set => SetValue(DropDownDisplayPageSizeProperty, value);
    }

    public bool ShouldUseOverlayPopup
    {
        get => GetValue(ShouldUseOverlayPopupProperty);
        set => SetValue(ShouldUseOverlayPopupProperty, value);
    }

    /// <summary>
    /// 钉住候选弹层：开启后弹层忽略 light-dismiss 关闭请求，保持强制打开。
    /// 典型场景是 Gallery 语义部件预览需要持续高亮 popup.* 部件。
    /// </summary>
    public bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }

    public bool IsFilterEnabled
    {
        get => GetValue(IsFilterEnabledProperty);
        set => SetValue(IsFilterEnabledProperty, value);
    }

    public bool IsShowOverflowTip
    {
        get => GetValue(IsShowOverflowTipProperty);
        set => SetValue(IsShowOverflowTipProperty, value);
    }

    public int OverflowTipDelay
    {
        get => GetValue(OverflowTipDelayProperty);
        set => SetValue(OverflowTipDelayProperty, value);
    }

    public PlacementMode OverflowTipPlacement
    {
        get => GetValue(OverflowTipPlacementProperty);
        set => SetValue(OverflowTipPlacementProperty, value);
    }

    public IValueFilter? Filter
    {
        get => GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }

    public object? FilterValue
    {
        get => GetValue(FilterValueProperty);
        set => SetValue(FilterValueProperty, value);
    }

    public DefaultFilterValueSelector? FilterValueSelector
    {
        get => GetValue(FilterValueSelectorProperty);
        set => SetValue(FilterValueSelectorProperty, value);
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

    #endregion

    private Popup? _popup;
    private IDisposable? _deactivationSubscription;
    private IDisposable? _popupPinnedOpenBinding;
    private ComboBoxHandle? _comboBoxHandle;
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
    private bool _isFrameBorderBrushRelayed;
    private bool _isFrameBackgroundRelayed;
    // Keep popup closing after SelectionChanged callbacks finish updating bound state.
    private bool _selectionFromEventInProgress;

    static ComboBox()
    {
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
        ContainerPrepared += HandleContainerPrepared;

        // 阻止下拉框打开时的 BringIntoView 行为，防止触发页面滚动
        AddHandler(RequestBringIntoViewEvent, (sender, e) =>
        {
            // 如果是下拉框内的元素触发的 BringIntoView，则取消事件
            if (IsDropDownOpen && e.TargetObject != this)
            {
                e.Handled = true;
            }
        }, handledEventsToo: true);
    }

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
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        this.SetPopup(null); // 清空父类，防止鼠标点击的错误处理

        if (_popup != null)
        {
            _popupPinnedOpenBinding?.Dispose();
            _popupPinnedOpenBinding = null;
            _popup.Opened -= HandlePopupOpened;
            _popup.Closed -= HandlePopupClosed;
            _popup.OverlayInputPassThroughElement = null;
        }

        if (_editableTextBox != null)
        {
            _editableTextBox.TemplateApplied -= HandleEditableTextBoxTemplateApplied;
        }
        _editableTextBoxTextSubscription?.Dispose();
        _editableTextBoxTextSubscription = null;
        ClearEditableTextBoxPlaceholderParts();

        _addOnDecoratedBox = e.NameScope.Find<AddOnDecoratedBox>(AddOnDecoratedBox.AddOnDecoratedBoxPart);
        _editableTextBox = e.NameScope.Find<AvaloniaTextBox>("PART_EditableTextBox");
        _popup = e.NameScope.Find<Popup>("PART_Popup");
        RelayRootSurfaceBrush(BorderBrushProperty);
        RelayRootSurfaceBrush(BackgroundProperty);
        if (_popup != null)
        {
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

        UpdatePseudoClasses();
        ConfigureMaxDropdownHeight();
        RefreshFilteredItemVisibility();

        // 模板应用期若业务状态已是打开（例如预览在 AXAML 上声明 IsDropDownOpen="True"），
        // 此时才在抑制遮罩之后补开弹层。弹层不再由模板绑定打开，因此这一步是必需的。
        if (IsDropDownOpen && _popup is { IsOpen: false })
        {
            _popup.IsOpen = true;
        }
    }
    
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

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        ConfigureFormFeedbackSubscription();
    }

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

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        _feedbackStatusSubscription?.Dispose();
        _feedbackStatusSubscription = null;
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var comboBoxItem = new ComboBoxItem();
        ApplySemanticItemMarker(comboBoxItem);
        return comboBoxItem;
    }
    
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
            // NeedsContainerOverride 对显式提供的 ComboBoxItem 返回 false，这类容器不经过
            // CreateContainerForItemOverride，因此在这里兜底注入同一 marker（重复添加是幂等操作）。
            ApplySemanticItemMarker(comboBoxItem);
        }
    }

    private static void ApplySemanticItemMarker(ComboBoxItem comboBoxItem)
    {
        comboBoxItem.Classes.Add(ComboBoxSemanticParts.PopupListItemClass);
    }

    protected override void ClearContainerForItemOverride(Control element)
    {
        if (element is IStateMotionOwner owner)
        {
            owner.StateMotionSession.Reset();
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

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

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
        else if (change.Property == BorderBrushProperty || change.Property == BackgroundProperty)
        {
            RelayRootSurfaceBrush(change.Property);
        }
        else if (change.Property == DropDownDisplayPageSizeProperty ||
                 change.Property == ItemHeightProperty ||
                 change.Property == PopupContentPaddingProperty)
        {
            ConfigureMaxDropdownHeight();
        }
        else if (change.Property == SelectedItemProperty &&
                 change.NewValue != null &&
                 !_selectionFromEventInProgress)
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
                RefreshFilteredItemVisibility();
                OpenPopup();
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
            RefreshFilteredItemVisibility();
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
    }

    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);
        if (e.NewFocusedElement is Visual focusedVisual &&
            _popup?.IsInsidePopup(focusedVisual) == true)
        {
            return;
        }

        FocusEditableTextBox(e.NavigationMethod);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!e.Handled && IsEnabled && IsDropDownOpen && HandleOpenDropDownKeyDown(e))
        {
            return;
        }

        base.OnKeyDown(e);
    }
    
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (!IsDropDownOpen)
        {
            PseudoClasses.Set(StdPseudoClass.Pressed, true);
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (IsDropDownOpen && e.Pointer.Type == PointerType.Mouse &&
            GetContainerFromEventSource(e.Source) is ComboBoxItem comboBoxItem)
        {
            TrySetCandidateFromContainer(comboBoxItem);
        }
    }
    
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

    public override bool UpdateSelectionFromEvent(Control container, RoutedEventArgs eventArgs)
    {
        var previous = _selectionFromEventInProgress;
        _selectionFromEventInProgress = true;
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
            _selectionFromEventInProgress = previous;
        }
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        EffectivePopupWidth = e.NewSize.Width;
    }

    #region 实现 FormItem 接口
    
    private EventHandler? _formValueChanged;
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
    
    protected virtual void NotifyFormValueChanged(object? value)
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void NotifySetFormValue(object? value)
    {
        SelectedItem = value;
    }

    protected virtual object? NotifyGetFormValue()
    {
        return SelectedItem;
    }

    protected virtual void NotifyClearFormValue()
    {
        SelectedItem = null;
    }

    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
        if (FormStatus != status)
        {
            SetCurrentValue(FormStatusProperty, status);
        }
    }
    
    protected virtual void NotifySetFeedBackControl(FormValidateFeedback? value)
    {
        FormFeedback = value;
    }
    #endregion

    private void HandleOpenPopupClicked(object? sender, EventArgs e)
    {
        var shouldOpen = !IsDropDownOpen;
        SetCurrentValue(IsDropDownOpenProperty, shouldOpen);
        if (shouldOpen)
        {
            FocusEditableTextBox(NavigationMethod.Pointer);
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

    private void ConfigureOverlayInputPassThroughElement()
    {
        if (_popup != null)
        {
            _popup.OverlayInputPassThroughElement = _addOnDecoratedBox;
        }
    }

    /// <summary>
    /// 把 owner 的根表面画刷以 LocalValue 中继到输入帧，使应用级根 <c>BorderBrush</c> / <c>Background</c>
    /// 定制压过帧状态机；未设置时把属性交还状态机。ComboBox 派生自 Avalonia 的 ComboBox，不在
    /// <c>AbstractTextInput</c> / <c>AbstractSelect</c> 的继承链上，因此需要与
    /// <c>AbstractSelect.RelayRootSurfaceBrush</c> 保持一致的同名中继，让输入族的根边框定制契约对
    /// ComboBox 同样成立。
    /// </summary>
    private void RelayRootSurfaceBrush(AvaloniaProperty property)
    {
        if (_addOnDecoratedBox is null)
        {
            return;
        }

        var value = GetValue(property);
        if (value is null || ReferenceEquals(value, AvaloniaProperty.UnsetValue))
        {
            // 仅当本控件确实接管过该槽位时才交还，避免清除嵌套 owner 的中继结果。
            if (IsRootSurfaceBrushRelayed(property))
            {
                _addOnDecoratedBox.ClearValue(property);
                SetRootSurfaceBrushRelayed(property, false);
            }

            return;
        }

        _addOnDecoratedBox.SetValue(property, value, BindingPriority.LocalValue);
        SetRootSurfaceBrushRelayed(property, true);
    }

    private bool IsRootSurfaceBrushRelayed(AvaloniaProperty property)
    {
        return ReferenceEquals(property, BorderBrushProperty)
            ? _isFrameBorderBrushRelayed
            : _isFrameBackgroundRelayed;
    }

    private void SetRootSurfaceBrushRelayed(AvaloniaProperty property, bool value)
    {
        if (ReferenceEquals(property, BorderBrushProperty))
        {
            _isFrameBorderBrushRelayed = value;
        }
        else
        {
            _isFrameBackgroundRelayed = value;
        }
    }


    /// <summary>
    /// 按业务状态打开弹层。开合不再由模板 <c>IsOpen</c> 绑定承担，因为模板充气阶段的绑定求值
    /// 会早于遮罩抑制打开弹层，钉住场景因而残留遮罩层。改为跟随 <c>IsDropDownOpen</c> 以代码打开，
    /// 与共享 <c>AbstractSelect</c> 的约定一致。
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
    /// 弹层自行关闭（light-dismiss 外点、Escape）时把状态回写业务属性。此前该回传由模板上的
    /// TwoWay <c>IsOpen</c> 绑定承担，开合改由代码接管后必须显式补齐，否则状态会卡在打开。
    /// </summary>
    private void HandlePopupClosed(object? sender, EventArgs e)
    {
        if (!IsDropDownOpen)
        {
            return;
        }

        if (IsPopupPinnedOpen && _popupLifecycleCloseDepth == 0)
        {
            // 钉住语义：外点/Escape 不改变有效打开状态。共享 Popup 会在钉住时自行抑制物理关闭并
            // 重新收敛，因此这里通常不会到达；若因锚点/TopLevel 失效走到这里，按设计保留“请求
            // 打开”状态而不改写业务状态，等待 reattach 或 pin 变更重新打开。
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
                                         .Subscribe(_ => ConfigureEditableTextBoxPlaceholderVisibility());
        }

        ConfigureEditableTextBoxPlaceholderVisibility();
    }

    private void ClearEditableTextBoxPlaceholderParts()
    {
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
        if (_candidateSelectedIndex == -1 ||
            !IsCandidateItemSelectable(_candidateSelectedIndex))
        {
            return;
        }

        var handled = false;
        if (GetCandidateItemContainer(_candidateSelectedIndex) is { } comboBoxItem)
        {
            handled = UpdateSelectionFromEvent(comboBoxItem, new RoutedEventArgs());
        }

        if (!handled)
        {
            SetCurrentValue(SelectedIndexProperty, _candidateSelectedIndex);
            if (IsDropDownOpen)
            {
                SetCurrentValue(IsDropDownOpenProperty, false);
            }
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
            return comboBoxItem.IsVisible &&
                   comboBoxItem.IsEnabled &&
                   IsCurrentFilterMatched(item);
        }

        return item is not Control control
            ? IsCurrentFilterMatched(item)
            : control.IsVisible && control.IsEnabled && IsCurrentFilterMatched(item);
    }

    private bool IsCurrentFilterMatched(object? item)
    {
        var filterValue    = FilterValue;
        var hasFilterValue = !string.IsNullOrEmpty(filterValue?.ToString());

        if (!IsEffectiveFilterEnabled || !hasFilterValue || Filter == null)
        {
            return true;
        }

        var textBinding     = TextSearch.GetTextBinding(this) ?? DisplayMemberBinding;
        using var evaluator = BindingEvaluator<string?>.TryCreate(textBinding);
        return Filter.Filter(SelectFilterValue(item, evaluator), filterValue);
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
        var filterValue    = FilterValue;
        var hasFilterValue = !string.IsNullOrEmpty(filterValue?.ToString());

        if (!IsEffectiveFilterEnabled || !hasFilterValue || Filter == null)
        {
            ClearFilteredItemVisibility();
            EnsureCandidateItemSelectionIsValid();
            return;
        }

        var textBinding     = TextSearch.GetTextBinding(this) ?? DisplayMemberBinding;
        using var evaluator = BindingEvaluator<string?>.TryCreate(textBinding);
        var hasVisibleMatch = false;

        for (var index = 0; index < Items.Count; index++)
        {
            var item    = Items[index];
            var matched = Filter.Filter(SelectFilterValue(item, evaluator), filterValue);
            var comboBoxItem = GetFilterItemContainer(index);
            if (comboBoxItem != null)
            {
                ApplyFilterState(comboBoxItem, matched);
            }

            hasVisibleMatch |= IsVisibleFilterMatch(item, comboBoxItem, matched);
        }

        IsEffectiveEmptyVisible = !hasVisibleMatch;
        EnsureCandidateItemSelectionIsValid();
    }

    private void ApplyCurrentFilterState(ComboBoxItem comboBoxItem, object? item)
    {
        var filterValue    = FilterValue;
        var hasFilterValue = !string.IsNullOrEmpty(filterValue?.ToString());

        if (!IsEffectiveFilterEnabled || !hasFilterValue || Filter == null)
        {
            ClearFilterState(comboBoxItem);
            return;
        }

        var textBinding     = TextSearch.GetTextBinding(this) ?? DisplayMemberBinding;
        using var evaluator = BindingEvaluator<string?>.TryCreate(textBinding);
        var matched         = Filter.Filter(SelectFilterValue(item, evaluator), filterValue);
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
        var items = new List<ComboBoxItem>(_filterItemVisibilityContext.Keys);
        foreach (var item in items)
        {
            ClearFilterState(item);
        }
        IsEffectiveEmptyVisible = false;
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

    private object? SelectFilterValue(object? item, BindingEvaluator<string?>? textBindingEvaluator)
    {
        var selector = FilterValueSelector;
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

    private readonly record struct FilterVisibilityState(bool IsSet, bool Value);
}
