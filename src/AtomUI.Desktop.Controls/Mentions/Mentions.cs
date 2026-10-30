using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using AtomUI.Controls;
using AtomUI.Controls.AsyncLoad;
using AtomUI.Controls.Utils;
using AtomUI.Data;
using AtomUI.Desktop.Controls.DataLoad;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Icons.AntDesign;
using AtomUI.Input;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

[PseudoClasses(MentionPseudoClass.CandidatePopupOpen)]
public partial class Mentions : TemplatedControl,
                        IMotionAwareControl,
                        IFormItemAware,
                        IInputControlStatusAware,
                        IInputControlStyleVariantAware,
                        ICustomizableSizeTypeAware,
                        IFormItemFeedbackAware
{
    #region 公共属性定义
    public static readonly StyledProperty<PathIcon?> ClearIconProperty =
        AvaloniaProperty.Register<Mentions, PathIcon?>(nameof(ClearIcon));
    
    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<Mentions, string?>(
            nameof(Value),
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);
    
    public static readonly StyledProperty<bool> IsAllowClearProperty =
        AvaloniaProperty.Register<Mentions, bool>(nameof(IsAllowClear));
    
    public static readonly StyledProperty<bool> IsAutoFocusProperty =
        AvaloniaProperty.Register<Mentions, bool>(nameof(IsAutoFocus));
    
    public static readonly StyledProperty<bool> IsAutoSizeProperty =
        AvaloniaProperty.Register<Mentions, bool>(nameof(IsAutoSize));

    public static readonly StyledProperty<string?> DefaultValueProperty =
        AvaloniaProperty.Register<Mentions, string?>(nameof(DefaultValue));

    public static readonly StyledProperty<MentionsPlacementMode> PlacementProperty =
        AvaloniaProperty.Register<Mentions, MentionsPlacementMode>(nameof(Placement), MentionsPlacementMode.Bottom);
    
    public static readonly StyledProperty<object?> EmptyIndicatorProperty =
        AvaloniaProperty.Register<Mentions, object?>(nameof(EmptyIndicator));
    
    public static readonly StyledProperty<IDataTemplate?> EmptyIndicatorTemplateProperty =
        AvaloniaProperty.Register<Mentions, IDataTemplate?>(nameof(EmptyIndicatorTemplate));
    
    public static readonly StyledProperty<bool> IsShowEmptyIndicatorProperty =
        AvaloniaProperty.Register<Mentions, bool>(nameof(IsShowEmptyIndicator), true);
    
    public static readonly StyledProperty<Thickness> EmptyIndicatorPaddingProperty =
        AvaloniaProperty.Register<Mentions, Thickness>(nameof(EmptyIndicatorPadding));
    
    public static readonly StyledProperty<IList<string>?> TriggerPrefixProperty =
        AvaloniaProperty.Register<Mentions, IList<string>?>(nameof(TriggerPrefix));
    
    public static readonly StyledProperty<string?> SplitProperty =
        AvaloniaProperty.Register<Mentions, string?>(nameof(Split));

    public static readonly StyledProperty<bool> ShouldUseOverlayPopupProperty =
        AvaloniaProperty.Register<Mentions, bool>(nameof(ShouldUseOverlayPopup), true);
    
    public static readonly StyledProperty<object?> ContentLeftAddOnProperty =
        AddOnDecoratedBox.ContentLeftAddOnProperty.AddOwner<Mentions>();
    
    public static readonly StyledProperty<IDataTemplate?> ContentLeftAddOnTemplateProperty =
        AddOnDecoratedBox.ContentLeftAddOnTemplateProperty.AddOwner<Mentions>();

    public static readonly StyledProperty<object?> ContentRightAddOnProperty =
        AddOnDecoratedBox.ContentRightAddOnProperty.AddOwner<Mentions>();
    
    public static readonly StyledProperty<IDataTemplate?> ContentRightAddOnTemplateProperty =
        AddOnDecoratedBox.ContentRightAddOnTemplateProperty.AddOwner<Mentions>();
    
    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<Mentions>();

    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<Mentions>();
    
    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        AvaloniaProperty.Register<Mentions, string?>(nameof(PlaceholderText));
    
    public static readonly StyledProperty<IEnumerable<IMentionOption>?> OptionsSourceProperty =
        AvaloniaProperty.Register<Mentions, IEnumerable<IMentionOption>?>(nameof(OptionsSource));
    
    public static readonly StyledProperty<IDataTemplate?> OptionTemplateProperty =
        AvaloniaProperty.Register<Mentions, IDataTemplate?>(nameof(OptionTemplate));
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty = 
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Mentions>();
    
    public static readonly StyledProperty<int> LinesProperty =
        TextArea.LinesProperty.AddOwner<Mentions>();
    
    public static readonly StyledProperty<int> MinLinesProperty =
        TextArea.MinLinesProperty.AddOwner<Mentions>();
    
    public static readonly StyledProperty<int> MaxLinesProperty =
        TextArea.MaxLinesProperty.AddOwner<Mentions>();
    
    public static readonly StyledProperty<IMentionOptionsAsyncLoader?> OptionsAsyncLoaderProperty =
        AvaloniaProperty.Register<Mentions, IMentionOptionsAsyncLoader?>(nameof(OptionsAsyncLoader));
    
    public static readonly DirectProperty<Mentions, bool> IsLoadingProperty =
        AvaloniaProperty.RegisterDirect<Mentions, bool>(
            nameof(IsLoading),
            o => o.IsLoading);
    
    public static readonly StyledProperty<int> DisplayCandidateCountProperty = 
        AvaloniaProperty.Register<Mentions, int>(nameof (DisplayCandidateCount), 10);
    
    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        TextArea.IsReadOnlyProperty.AddOwner<Mentions>();
    
    public static readonly StyledProperty<IValueFilter?> FilterProperty =
        AvaloniaProperty.Register<Mentions, IValueFilter?>(nameof(Filter));
    
    public static readonly StyledProperty<DefaultFilterValueSelector?> FilterValueSelectorProperty =
        AvaloniaProperty.Register<Mentions, DefaultFilterValueSelector?>(
            nameof(FilterValueSelector));
    
    public static readonly StyledProperty<TimeSpan> AsyncLoadDebounceProperty =
        AvaloniaProperty.Register<Mentions, TimeSpan>(
            nameof(AsyncLoadDebounce),
            TimeSpan.Zero,
            validate: IsValidAsyncLoadDebounce);

    public static readonly StyledProperty<TimeSpan> AsyncLoadTimeoutProperty =
        AvaloniaProperty.Register<Mentions, TimeSpan>(
            nameof(AsyncLoadTimeout),
            TimeSpan.FromSeconds(10));
    
    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<Mentions, bool>(
            nameof(IsDropDownOpen));
    
    public static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        Popup.IsPopupPinnedOpenProperty.AddOwner<Mentions>();

    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<Mentions>();
    
    public PathIcon? ClearIcon
    {
        get => GetValue(ClearIconProperty);
        set => SetValue(ClearIconProperty, value);
    }
    
    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    
    public bool IsAllowClear
    {
        get => GetValue(IsAllowClearProperty);
        set => SetValue(IsAllowClearProperty, value);
    }
    
    public bool IsAutoFocus
    {
        get => GetValue(IsAutoFocusProperty);
        set => SetValue(IsAutoFocusProperty, value);
    }
    
    public bool IsAutoSize
    {
        get => GetValue(IsAutoSizeProperty);
        set => SetValue(IsAutoSizeProperty, value);
    }
    
    public MentionsPlacementMode Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }
    
    [DependsOn(nameof(EmptyIndicatorTemplate))]
    public object? EmptyIndicator
    {
        get => GetValue(EmptyIndicatorProperty);
        set => SetValue(EmptyIndicatorProperty, value);
    }

    public IDataTemplate? EmptyIndicatorTemplate
    {
        get => GetValue(EmptyIndicatorTemplateProperty);
        set => SetValue(EmptyIndicatorTemplateProperty, value);
    }
    
    public bool IsShowEmptyIndicator
    {
        get => GetValue(IsShowEmptyIndicatorProperty);
        set => SetValue(IsShowEmptyIndicatorProperty, value);
    }
    
    public Thickness EmptyIndicatorPadding
    {
        get => GetValue(EmptyIndicatorPaddingProperty);
        set => SetValue(EmptyIndicatorPaddingProperty, value);
    }

    public string? DefaultValue
    {
        get => GetValue(DefaultValueProperty);
        set => SetValue(DefaultValueProperty, value);
    }
    
    public IList<string>? TriggerPrefix
    {
        get => GetValue(TriggerPrefixProperty);
        set => SetValue(TriggerPrefixProperty, value);
    }

    public string? Split
    {
        get => GetValue(SplitProperty);
        set => SetValue(SplitProperty, value);
    }

    public bool ShouldUseOverlayPopup
    {
        get => GetValue(ShouldUseOverlayPopupProperty);
        set => SetValue(ShouldUseOverlayPopupProperty, value);
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
    
    public string? PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }
    
    public IEnumerable<IMentionOption>? OptionsSource
    {
        get => GetValue(OptionsSourceProperty);
        set => SetValue(OptionsSourceProperty, value);
    }
    
    [InheritDataTypeFromItems(nameof(OptionsSource))]
    public IDataTemplate? OptionTemplate
    {
        get => GetValue(OptionTemplateProperty);
        set => SetValue(OptionTemplateProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public int Lines
    {
        get => GetValue(LinesProperty);
        set => SetValue(LinesProperty, value);
    }
    
    public int MinLines
    {
        get => GetValue(MinLinesProperty);
        set => SetValue(MinLinesProperty, value);
    }
    
    public int MaxLines
    {
        get => GetValue(MaxLinesProperty);
        set => SetValue(MaxLinesProperty, value);
    }
    
    public IMentionOptionsAsyncLoader? OptionsAsyncLoader
    {
        get => GetValue(OptionsAsyncLoaderProperty);
        set => SetValue(OptionsAsyncLoaderProperty, value);
    }
    
    private bool _isLoading;

    public bool IsLoading
    {
        get => _isLoading;
        internal set => SetAndRaise(IsLoadingProperty, ref _isLoading, value);
    }
    
    public int DisplayCandidateCount
    {
        get => GetValue(DisplayCandidateCountProperty);
        set => SetValue(DisplayCandidateCountProperty, value);
    }
    
    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }
    
    public IValueFilter? Filter
    {
        get => GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }
    
    public DefaultFilterValueSelector? FilterValueSelector
    {
        get => GetValue(FilterValueSelectorProperty);
        set => SetValue(FilterValueSelectorProperty, value);
    }
    
    public TimeSpan AsyncLoadDebounce
    {
        get => GetValue(AsyncLoadDebounceProperty);
        set => SetValue(AsyncLoadDebounceProperty, value);
    }

    public TimeSpan AsyncLoadTimeout
    {
        get => GetValue(AsyncLoadTimeoutProperty);
        set => SetValue(AsyncLoadTimeoutProperty, value);
    }
    
    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }
    
    public bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }

    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    #endregion

    #region 公共事件定义

    public event EventHandler<MentionOptionsLoadedEventArgs>? OptionsLoaded;
    public event EventHandler<MentionsPopulatingEventArgs>? Populating;
    public event EventHandler<MentionsPopulatedEventArgs>? Populated;
    public event EventHandler<CancelEventArgs>? DropDownOpening;
    public event EventHandler? DropDownOpened;
    public event EventHandler<CancelEventArgs>? DropDownClosing;
    public event EventHandler? DropDownClosed;
    public event EventHandler<MentionCandidateTriggeredEventArgs>? CandidateTriggered;
    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<Mentions, string?> FilterValueProperty =
        AvaloniaProperty.RegisterDirect<Mentions, string?>(
            nameof(FilterValue),
            o => o.FilterValue,
            (o, v) => o.FilterValue = v);
    
    internal static readonly DirectProperty<Mentions, double> ItemHeightProperty =
        AvaloniaProperty.RegisterDirect<Mentions, double>(
            nameof(ItemHeight),
            o => o.ItemHeight,
            (o, v) => o.ItemHeight = v);
    
    internal static readonly DirectProperty<Mentions, double> MaxPopupHeightProperty =
        AvaloniaProperty.RegisterDirect<Mentions, double>(
            nameof(MaxPopupHeight),
            o => o.MaxPopupHeight,
            (o, v) => o.MaxPopupHeight = v);
    
    internal static readonly DirectProperty<Mentions, double> MinPopupWidthProperty =
        AvaloniaProperty.RegisterDirect<Mentions, double>(
            nameof(MinPopupWidth),
            o => o.MinPopupWidth,
            (o, v) => o.MinPopupWidth = v);
    
    internal static readonly DirectProperty<Mentions, Thickness> PopupContentPaddingProperty =
        AvaloniaProperty.RegisterDirect<Mentions, Thickness>(nameof(PopupContentPadding),
            o => o.PopupContentPadding,
            (o, v) => o.PopupContentPadding = v);
    
    internal static readonly DirectProperty<Mentions, PlacementMode> PopupPlacementProperty =
        AvaloniaProperty.RegisterDirect<Mentions, PlacementMode>(nameof(PopupPlacement),
            o => o.PopupPlacement,
            (o, v) => o.PopupPlacement = v);
    
    internal static readonly StyledProperty<FormValidateFeedback?> FormFeedbackProperty = 
        AvaloniaProperty.Register<Mentions, FormValidateFeedback?>(nameof(FormFeedback));

    internal static readonly StyledProperty<FormValidateStatus> FormStatusProperty =
        InputControlState.FormStatusProperty.AddOwner<Mentions>();
    
    private string? _filterValue;

    internal string? FilterValue
    {
        get => _filterValue;
        set => SetAndRaise(FilterValueProperty, ref _filterValue, value);
    }
    
    private double _itemHeight;

    internal double ItemHeight
    {
        get => _itemHeight;
        set => SetAndRaise(ItemHeightProperty, ref _itemHeight, value);
    }
    
    private double _maxPopupHeight;

    internal double MaxPopupHeight
    {
        get => _maxPopupHeight;
        set => SetAndRaise(MaxPopupHeightProperty, ref _maxPopupHeight, value);
    }
    
    private double _minPopupWidth;

    internal double MinPopupWidth
    {
        get => _minPopupWidth;
        set => SetAndRaise(MinPopupWidthProperty, ref _minPopupWidth, value);
    }
    
    private Thickness _popupContentPadding;

    internal Thickness PopupContentPadding
    {
        get => _popupContentPadding;
        set => SetAndRaise(PopupContentPaddingProperty, ref _popupContentPadding, value);
    }
    
    private PlacementMode _placementMode;

    internal PlacementMode PopupPlacement
    {
        get => _placementMode;
        set => SetAndRaise(PopupPlacementProperty, ref _placementMode, value);
    }
    
    protected ICandidateList? CandidateList
    {
        get => _candidateList;
        set
        {
            if (_candidateList != null)
            {
                _candidateList.Commit      -= HandleCandidateListComplete;
                _candidateList.Cancel      -= HandleCandidateListCanceled;
                _candidateList.ItemsSource =  null;
            }

            _candidateList = value;

            if (_candidateList != null)
            {
                _candidateList.Commit      += HandleCandidateListComplete;
                _candidateList.Cancel      += HandleCandidateListCanceled;
                _candidateList.ItemsSource =  _view;
            }
        }
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
    #endregion
    
    private static bool IsValidAsyncLoadDebounce(TimeSpan value) => value.TotalMilliseconds >= 0.0;

    private MentionTextArea? _textArea;
    private Popup? _popup;
    private CompositeDisposable? _subscriptionsOnOpen;
    private ICandidateList? _candidateList;
    private DispatcherTimer? _delayTimer;
    private List<IMentionOption>? _items;
    private IList<IMentionOption>? _view;
    private DropDownOpenChangeReason _dropDownOpenChangeReason;
    private long _populateRequestId;
    private IDisposable? _collectionChangeSubscription;
    private readonly AsyncSearchLoadCoordinator<string?, MentionOptionsLoadResult> _asyncLoadCoordinator = new();
    private IDisposable? _deactivationSubscription;
    private IDisposable? _popupPinnedOpenBinding;

    private enum DropDownOpenChangeReason
    {
        None,
        InternalSync
    }
    
    static Mentions()
    {
        FocusableProperty.OverrideDefaultValue<Mentions>(true);
        LinesProperty.OverrideDefaultValue<Mentions>(1);
        
        IsDropDownOpenProperty.Changed.AddClassHandler<Mentions>((mentions,e) => mentions.HandleIsDropDownOpenChanged(e));
        AsyncLoadDebounceProperty.Changed.AddClassHandler<Mentions>((mentions,e) => mentions.HandleAsyncLoadDebounceChanged(e));
        PlacementProperty.Changed.AddClassHandler<Mentions>((mentions,e) => mentions.ConfigurePopupPlacement());
        OptionsSourceProperty.Changed.AddClassHandler<Mentions>((mentions,e) => mentions.HandleItemsSourceChanged((IEnumerable?)e.NewValue));
        FilterValueProperty.Changed.AddClassHandler<Mentions>((mentions,e) => mentions.HandleFilterValueChanged());
        ValueProperty.Changed.AddClassHandler<Mentions>((mentions, args) => mentions.HandleValueChanged());
    }
    
    public Mentions()
    {
    }

    #region 实现 FormItem 接口

    private EventHandler? _formValueChanged;

    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    void IFormItemAware.SetFormValue(object? value) => NotifySetFormValue(value?.ToString());

    object? IFormItemAware.GetFormValue() => NotifyGetFormValue();
    void IFormItemAware.ClearFormValue() => NotifyClearFormValue();
    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status) => NotifyValidateStatus(status);
    void IFormItemFeedbackAware.SetFeedbackControl(FormValidateFeedback? value) => NotifySetFeedBackControl(value);
    private void HandleValueChanged()
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void NotifySetFormValue(string? value)
    {
        SetCurrentValue(ValueProperty, value);
    }

    protected virtual string? NotifyGetFormValue()
    {
        return Value;
    }

    protected virtual void NotifyClearFormValue()
    {
        SetCurrentValue(ValueProperty, null);
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

    #region 生命周期

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (Value == null)
        {
            SetCurrentValue(ValueProperty, DefaultValue);
        }

        if (TriggerPrefix == null)
        {
            SetCurrentValue(TriggerPrefixProperty, ["@"]);
        }

        if (ClearIcon == null)
        {
            SetCurrentValue(ClearIconProperty, new CloseCircleFilled());
        }
        ConfigurePopupPlacement();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_textArea != null)
        {
            _textArea.CandidateOpenRequest  -= HandleCandidateOpenRequest;
            _textArea.CandidateCloseRequest -= HandleCandidateCloseRequest;
            _textArea.Owner                 =  null;
        }

        if (_popup != null)
        {
            _popupPinnedOpenBinding?.Dispose();
            _popupPinnedOpenBinding = null;
            _popup.Opened -= HandlePopupOpened;
            _popup.Closed -= HandlePopupClosed;
        }
        
        _textArea     = e.NameScope.Find<MentionTextArea>("PART_TextArea");
        _popup        = e.NameScope.Find<Popup>("PART_Popup");
        CandidateList = e.NameScope.Find<ICandidateList>("PART_CandidateList");

        if (_textArea != null)
        {
            _textArea.CandidateOpenRequest  += HandleCandidateOpenRequest;
            _textArea.CandidateCloseRequest += HandleCandidateCloseRequest;
            _textArea.Owner                 =  this;
        }

        if (_popup != null)
        {
            _popupPinnedOpenBinding = BindUtils.RelayBind(
                this,
                IsPopupPinnedOpenProperty,
                _popup,
                Popup.IsPopupPinnedOpenProperty);
            _popup.Opened              += HandlePopupOpened;
            _popup.Closed              += HandlePopupClosed;
            if (!IsPopupPinnedOpen)
            {
                _popup.OverlayInputPassThroughElement = _textArea;
            }

        }
        
        ConfigurePopupPlacement();
        if (IsDropDownOpen)
        {
            OpenDropDown();
            if (_view == null && !IsLoading)
            {
                SchedulePopulateDropDown();
            }
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (_collectionChangeSubscription == null && OptionsSource is INotifyCollectionChanged notifyCollectionChanged)
        {
            _collectionChangeSubscription = notifyCollectionChanged.WeakSubscribe(ItemsCollectionChanged);
        }
        _items = OptionsSource == null ? null : BuildItemsCache(OptionsSource);

        _deactivationSubscription =
            TopLevelDeactivation.Subscribe(TopLevel.GetTopLevel(this), HandleWindowDeactivated);
        if (IsPopupPinnedOpen && !IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, true);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        CancelPendingPopulate();
        _subscriptionsOnOpen?.Dispose();
        _subscriptionsOnOpen = null;
        _collectionChangeSubscription?.Dispose();
        _collectionChangeSubscription = null;
        _deactivationSubscription?.Dispose();
        _deactivationSubscription = null;
        _popup?.CloseForLifecycle();
        if (IsDropDownOpen)
        {
            SetDropDownOpenFromInternalSync(false);
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (IsAutoFocus)
        {
            Dispatcher.Post(() => _textArea?.Focus());
        }

        UpdatePseudoClasses();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DisplayCandidateCountProperty ||
            change.Property == ItemHeightProperty)
        {
            ConfigureMaxPopupHeight();
        }
        else if (change.Property == IsPopupPinnedOpenProperty)
        {
            if (change.GetNewValue<bool>() && !IsDropDownOpen)
            {
                SetCurrentValue(IsDropDownOpenProperty, true);
            }
        }
    }


    #endregion

    #region 属性变更处理

    private void HandleIsDropDownOpenChanged(AvaloniaPropertyChangedEventArgs e)
    {
        bool oldValue = (bool)e.OldValue!;
        bool newValue = (bool)e.NewValue!;

        if (_dropDownOpenChangeReason == DropDownOpenChangeReason.InternalSync)
        {
            UpdatePseudoClasses();
            return;
        }

        if (!newValue && IsPopupPinnedOpen)
        {
            SetDropDownOpenFromInternalSync(oldValue);
            return;
        }

        if (newValue)
        {
            if (OpeningDropDown(oldValue))
            {
                SchedulePopulateDropDown();
            }
        }
        else
        {
            ClosingDropDown(oldValue);
        }

        UpdatePseudoClasses();
    }

    private void HandleAsyncLoadDebounceChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var newValue = (TimeSpan)e.NewValue!;

        // Always clean up the old timer first
        if (_delayTimer != null)
        {
            _delayTimer.Stop();
            _delayTimer.Tick -= PopulateDropDown;
            _delayTimer      =  null;
        }

        // Create a new timer with the new delay value if needed
        if (newValue > TimeSpan.Zero)
        {
            _delayTimer           =  new DispatcherTimer();
            _delayTimer.Interval  =  newValue;
            _delayTimer.Tick      += PopulateDropDown;
        }
    }

    private void HandleItemsSourceChanged(IEnumerable? newValue)
    {
        // Remove handler for oldValue.CollectionChanged (if present)
        _collectionChangeSubscription?.Dispose();
        _collectionChangeSubscription = null;

        // Add handler for newValue.CollectionChanged (if possible)
        if (newValue is INotifyCollectionChanged newValueINotifyCollectionChanged)
        {
            _collectionChangeSubscription = newValueINotifyCollectionChanged.WeakSubscribe(ItemsCollectionChanged);
        }

        // Store a local cached copy of the data
        _items = newValue == null ? null : BuildItemsCache(newValue);

        // Clear and set the view on the selection adapter
        ClearView();
        if (IsDropDownOpen)
        {
            RefreshView();
        }
    }

    private void HandleFilterValueChanged()
    {
        if (IsDropDownOpen)
        {
            RefreshView();
        }
    }

    #endregion

    #region 布局和状态同步

    private void ConfigurePopupPlacement()
    {
        if (Placement == MentionsPlacementMode.Bottom)
        {
            PopupPlacement = PlacementMode.BottomEdgeAlignedLeft;
        }
        else
        {
            PopupPlacement = PlacementMode.TopEdgeAlignedLeft;
        }
    }

    protected virtual void ConfigureMaxPopupHeight()
    {
        MaxPopupHeight = ItemHeight * DisplayCandidateCount + PopupContentPadding.Top + PopupContentPadding.Bottom;
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(MentionPseudoClass.CandidatePopupOpen, IsDropDownOpen);
    }

    #endregion

    #region 下拉状态管理

    private void SetDropDownOpenFromInternalSync(bool value)
    {
        if (IsDropDownOpen == value)
        {
            UpdatePseudoClasses();
            return;
        }

        _dropDownOpenChangeReason = DropDownOpenChangeReason.InternalSync;
        try
        {
            SetCurrentValue(IsDropDownOpenProperty, value);
        }
        finally
        {
            _dropDownOpenChangeReason = DropDownOpenChangeReason.None;
        }
        UpdatePseudoClasses();
    }

    private void CancelPendingPopulate()
    {
        _delayTimer?.Stop();
        Interlocked.Increment(ref _populateRequestId);
        _asyncLoadCoordinator.Cancel();
        IsLoading = false;
    }

    private bool OpeningDropDown(bool oldValue)
    {
        var args = new CancelEventArgs();

        // Opening
        NotifyDropDownOpening(args);

        if (args.Cancel)
        {
            SetDropDownOpenFromInternalSync(oldValue);
            UpdatePseudoClasses();
            return false;
        }

        OpenDropDown();

        UpdatePseudoClasses();
        return true;
    }

    private void OpenDropDown()
    {
        if (_popup != null && !_popup.IsOpen)
        {
            _popup.IsOpen = true;
        }
    }

    private void ClosingDropDown(bool oldValue)
    {
        var args = new CancelEventArgs();
        NotifyDropDownClosing(args);

        if (args.Cancel)
        {
            SetDropDownOpenFromInternalSync(oldValue);
        }
        else
        {
            CloseDropDown();
        }

        UpdatePseudoClasses();
    }

    private void CloseDropDown()
    {
        CancelPendingPopulate();
        if (_popup?.IsOpen == true)
        {
            _popup.IsOpen = false;
        }
    }

    private void HandlePopupOpened(object? sender, EventArgs e)
    {
        _subscriptionsOnOpen?.Dispose();
        _subscriptionsOnOpen = new CompositeDisposable(2);
        this.GetObservable(IsVisibleProperty).Subscribe(HandleIsVisibleChanged).DisposeWith(_subscriptionsOnOpen);
        this.GetObservable(IsEnabledProperty).Subscribe(HandleIsEnabledChanged).DisposeWith(_subscriptionsOnOpen);
        this.SubscribeAncestorIsVisible(HandleIsVisibleChanged, _subscriptionsOnOpen);
        NotifyDropDownOpened(EventArgs.Empty);
        _textArea?.Focus();
    }

    private void HandlePopupClosed(object? sender, EventArgs e)
    {
        _subscriptionsOnOpen?.Dispose();
        _subscriptionsOnOpen = null;
        CancelPendingPopulate();
        if (IsDropDownOpen)
        {
            SetDropDownOpenFromInternalSync(false);
        }

        NotifyDropDownClosed(EventArgs.Empty);
    }

    private void HandleIsVisibleChanged(bool isVisible)
    {
        if (!isVisible && IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
        }
    }

    private void HandleIsEnabledChanged(bool isEnabled)
    {
        if (!isEnabled && IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
        }
    }

    private void HandleWindowDeactivated(object? sender, EventArgs e)
    {
        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    protected virtual void NotifyDropDownOpening(CancelEventArgs eventArgs)
    {
        DropDownOpening?.Invoke(this, eventArgs);
    }

    protected virtual void NotifyDropDownOpened(EventArgs eventArgs)
    {
        DropDownOpened?.Invoke(this, eventArgs);
    }

    protected virtual void NotifyDropDownClosing(CancelEventArgs eventArgs)
    {
        DropDownClosing?.Invoke(this, eventArgs);
    }

    protected virtual void NotifyDropDownClosed(EventArgs eventArgs)
    {
        DropDownClosed?.Invoke(this, eventArgs);
    }

    #endregion

    #region 候选交互

    private void HandleCandidateOpenRequest(object? sender, ShowMentionCandidateRequestEventArgs eventArgs)
    {
        CandidateTriggered?.Invoke(this, new MentionCandidateTriggeredEventArgs(eventArgs.TriggerChar));
        if (_popup != null && _textArea != null)
        {
            var triggerBounds       = _textArea.GetTriggerBounds(eventArgs.TriggerBounds, this);
            _popup.HorizontalOffset = triggerBounds.X;
            if (Placement == MentionsPlacementMode.Bottom)
            {
                _popup.VerticalOffset = Math.Min(-(DesiredSize.Height - triggerBounds.Y) + triggerBounds.Height, 0);
            }
            else
            {
                _popup.VerticalOffset = triggerBounds.Y;
            }
        }

        SetCurrentValue(IsDropDownOpenProperty, true);
    }

    private void HandleCandidateCloseRequest(object? sender, EventArgs eventArgs)
    {
        FilterValue = null;
        SetCurrentValue(IsDropDownOpenProperty, false);
        _textArea!.Focus();
    }

    private void HandleCandidateListComplete(object? sender, RoutedEventArgs e)
    {
        if (CandidateList!.SelectedItem is IMentionOption option)
        {
            InsertCandidateOption(option);
        }
        SetCurrentValue(IsDropDownOpenProperty, false);
        _textArea!.Focus();
    }

    private void HandleCandidateListCanceled(object? sender, RoutedEventArgs e)
    {
        FilterValue = null;
        SetCurrentValue(IsDropDownOpenProperty, false);
        _textArea!.Focus();
    }

    private void InsertCandidateOption(IMentionOption option)
    {
        Debug.Assert(_textArea != null);
        var value = option.Value?.ToString() ?? option.Header?.ToString() ?? string.Empty;
        _textArea?.InsertMentionOption(value, Split);
    }

    #endregion

    #region 键盘交互

    protected override void OnKeyDown(KeyEventArgs e)
    {
        _ = e ?? throw new ArgumentNullException(nameof(e));
        base.OnKeyDown(e);
        if (e.Handled || !IsEnabled)
        {
            return;
        }

        // The drop down is open, pass along the key event arguments to the
        // selection adapter. If it isn't handled by the adapter's logic,
        // then we handle some simple navigation scenarios for controlling
        // the drop down.
        if (IsDropDownOpen)
        {
            if (CandidateList != null)
            {
                CandidateList.HandleKeyDown(e);
                if (e.Handled)
                {
                    return;
                }
            }

            if (e.Key == Key.Escape)
            {
                HandleCandidateListCanceled(this, new RoutedEventArgs());
                e.Handled = true;
            }
        }
        else
        {
            // The drop down is not open, the Down key will toggle it open.
            // Ignore key buttons, if they are used for XY focus.
            if (e.Key == Key.Down
                && !this.IsAllowedXYNavigationMode(e.KeyDeviceType))
            {
                SetCurrentValue(IsDropDownOpenProperty, true);
                e.Handled = true;
            }
        }

        // Standard drop down navigation
        switch (e.Key)
        {
            case Key.F4:
                SetCurrentValue(IsDropDownOpenProperty, !IsDropDownOpen);
                e.Handled = true;
                break;

            case Key.Enter:
                if (IsDropDownOpen)
                {
                    HandleCandidateListComplete(this, new RoutedEventArgs());
                    e.Handled = true;
                }
                break;

            default:
                break;
        }
    }

    #endregion

    #region 异步加载

    private void SchedulePopulateDropDown()
    {
        if (_delayTimer != null)
        {
            _delayTimer.Stop();
            _delayTimer.Start();
        }
        else
        {
            PopulateDropDown(this, EventArgs.Empty);
        }
    }

    private void PopulateDropDown(object? sender, EventArgs e)
    {
        _delayTimer?.Stop();

        if (TryPopulateAsync(FilterValue))
        {
            return;
        }

        // The Populated event enables advanced, custom filtering. The
        // client needs to directly update the ItemsSource collection or
        // call the Populate method on the control to continue the
        // display process if Cancel is set to true.
        var populating = new MentionsPopulatingEventArgs(FilterValue);
        NotifyPopulating(populating);
        if (!populating.Cancel)
        {
            PopulateComplete();
        }
    }

    private bool TryPopulateAsync(string? searchText)
    {
        var loader = OptionsAsyncLoader;
        if (loader == null)
        {
            return false;
        }

        var requestId = Interlocked.Increment(ref _populateRequestId);
        _asyncLoadCoordinator.Timeout = AsyncLoadTimeout;
        IsLoading = true;
        _ = PopulateAsync(searchText, loader, requestId);
        return true;
    }

    private async Task PopulateAsync(string? filterValue, IMentionOptionsAsyncLoader loader, long requestId)
    {
        var outcome = await _asyncLoadCoordinator.LoadAsync(
            filterValue,
            (ctx, token) => loader.LoadAsync(ctx, token));

        if (outcome.IsSkipped)
        {
            return;
        }

        await Dispatcher.InvokeAsync(() => CompleteAsyncPopulate(filterValue, loader, requestId, outcome));
    }

    private void CompleteAsyncPopulate(
        string? filterValue,
        IMentionOptionsAsyncLoader loader,
        long requestId,
        AsyncLoadOutcome<MentionOptionsLoadResult> outcome)
    {
        if (Interlocked.Read(ref _populateRequestId) != requestId ||
            !ReferenceEquals(OptionsAsyncLoader, loader))
        {
            return;
        }

        IsLoading = false;

        if (outcome.IsSuccess && outcome.Result != null)
        {
            var result = outcome.Result;
            OptionsLoaded?.Invoke(this, new MentionOptionsLoadedEventArgs(filterValue, result));

            if (result.IsSuccess)
            {
                SetCurrentValue(OptionsSourceProperty, result.Data);
                PopulateComplete();
            }
            return;
        }

        var statusCode = outcome.Status switch
        {
            AsyncLoadStatus.TimedOut  => RpcStatusCode.Timeout,
            AsyncLoadStatus.Cancelled => RpcStatusCode.Cancelled,
            _                         => RpcStatusCode.Unknown
        };

        OptionsLoaded?.Invoke(this, new MentionOptionsLoadedEventArgs(filterValue, new MentionOptionsLoadResult()
        {
            UserFriendlyMessage = outcome.Error?.Message,
            StatusCode          = statusCode
        }));
    }

    public void PopulateComplete()
    {
        // Apply the search filter
        RefreshView();

        // Fire the Populated event containing the read-only view data.
        var populated = new MentionsPopulatedEventArgs(_view);
        NotifyPopulated(populated);
    }

    protected virtual void NotifyPopulating(MentionsPopulatingEventArgs e)
    {
        IsLoading = true;
        Populating?.Invoke(this, e);
    }

    protected virtual void NotifyPopulated(MentionsPopulatedEventArgs e)
    {
        IsLoading = false;
        Populated?.Invoke(this, e);
    }

    #endregion

    #region 数据视图

    private void ItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_items == null)
        {
            _items = OptionsSource == null ? null : BuildItemsCache(OptionsSource);
            RefreshView();
            return;
        }

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Remove when e.OldItems != null:
                for (var index = 0; index < e.OldItems.Count; index++)
                {
                    _items.RemoveAt(e.OldStartingIndex);
                }
                break;
            case NotifyCollectionChangedAction.Add when e.NewItems != null:
                for (var index = 0; index < e.NewItems.Count; index++)
                {
                    _items.Insert(e.NewStartingIndex + index, (IMentionOption)e.NewItems[index]!);
                }
                break;
            case NotifyCollectionChangedAction.Replace when e.NewItems != null:
                for (var index = 0; index < e.NewItems.Count; index++)
                {
                    _items[e.NewStartingIndex + index] = (IMentionOption)e.NewItems[index]!;
                }
                break;
            case NotifyCollectionChangedAction.Move when e.OldItems != null:
                var movedItems = e.OldItems.Cast<IMentionOption>().ToList();
                for (var index = 0; index < movedItems.Count; index++)
                {
                    _items.RemoveAt(e.OldStartingIndex);
                }
                for (var index = 0; index < movedItems.Count; index++)
                {
                    _items.Insert(e.NewStartingIndex + index, movedItems[index]);
                }
                break;
            case NotifyCollectionChangedAction.Reset:
                _items = OptionsSource == null ? null : BuildItemsCache(OptionsSource);
                break;
        }

        // Refresh the observable collection used in the selection adapter.
        RefreshView();
    }

    private static List<IMentionOption> BuildItemsCache(IEnumerable source)
    {
        var items = source switch
        {
            ICollection collection => new List<IMentionOption>(collection.Count),
            IReadOnlyCollection<IMentionOption> collection => new List<IMentionOption>(collection.Count),
            _ => new List<IMentionOption>()
        };
        foreach (var item in source)
        {
            items.Add((IMentionOption)item!);
        }
        return items;
    }

    private void ClearView()
    {
        _view = null;
        if (_candidateList != null)
        {
            _candidateList.ItemsSource = null;
            _candidateList.SelectedItem = null;
        }
    }

    private void RefreshView()
    {
        if (_items == null)
        {
            ClearView();
            return;
        }

        // Determine if any filtering mode is on
        var filter = Filter ?? ValueFilterFactory.BuildFilter(ValueFilterMode.Contains);
        Debug.Assert(filter != null);
        var items = _items;

        // cache properties
        var newViewItems = new Collection<IMentionOption>();

        foreach (var item in items)
        {
            bool inResults = string.IsNullOrWhiteSpace(FilterValue) || filter.Filter(GetValueByOption(item), FilterValue);

            if (inResults)
            {
                newViewItems.Add(item);
            }
        }

        _view = newViewItems;

        if (_candidateList != null)
        {
            if (_candidateList.ItemsSource != _view)
            {
                _candidateList.ItemsSource = _view;
            }
            _candidateList.SelectedItem = _view.Count > 0 ? _view[0] : null;
        }
    }

    private string? GetValueByOption(IMentionOption option)
    {
        string? value = null;
        if (FilterValueSelector != null)
        {
            value = FilterValueSelector(option)?.ToString();
        }
        else
        {
            value = option.Header?.ToString() ?? option.Value?.ToString() ?? option.Key;
        }
        return value;
    }

    #endregion

    #region 文本区域指针协作

    internal void NotifyTextAreaPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if(!e.Handled && e.Source is Visual source)
        {
            if (_popup?.IsInsidePopup(source) == true)
            {
                e.Handled = true;
                return;
            }
        }

        if (IsDropDownOpen)
        {
            if (e.Source is Control sourceControl)
            {
                var textArea = sourceControl.FindAncestorOfType<MentionTextArea>();
                if (textArea != null)
                {
                    return;
                }
            }
            else
            {
                IsDropDownOpen = false;
                e.Handled      = true;
            }
        }
        else
        {
            PseudoClasses.Set(StdPseudoClass.Pressed, true);
        }
    }

    internal void NotifyTextAreaPointerReleased(PointerReleasedEventArgs e)
    {
        if (!e.Handled && e.Source is Visual source)
        {
            if (_popup?.IsInsidePopup(source) == true || PseudoClasses.Contains(StdPseudoClass.Pressed))
            {
                e.Handled = true;
            }
        }

        PseudoClasses.Set(StdPseudoClass.Pressed, false);
        base.OnPointerReleased(e);
    }

    #endregion
}
