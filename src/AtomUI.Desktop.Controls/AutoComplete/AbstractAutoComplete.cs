using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using AtomUI.Controls;
using AtomUI.Controls.Utils;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Input;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Diagnostics;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Metadata;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using AvaloniaTextBox = Avalonia.Controls.TextBox;
using ItemCollection = AtomUI.Collections.ItemCollection;

public enum AutoCompletePlacementMode
{
    Top,
    Bottom
}

public delegate object? AutoCompleteFilterValueSelector(IAutoCompleteOption option);

[PseudoClasses(AutoCompletePseudoClass.CandidatePopupOpen)]
public class AbstractAutoComplete : TemplatedControl, 
                                    IControlSharedTokenResourcesHost, 
                                    ISizeTypeAware,
                                    IMotionAwareControl
{
    #region 公共属性定义
    public static readonly StyledProperty<PathIcon?> ClearIconProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, PathIcon?>(nameof(ClearIcon));
    
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractAutoComplete>();
    
    public static readonly StyledProperty<int> CaretIndexProperty =
        AvaloniaTextBox.CaretIndexProperty.AddOwner<AbstractAutoComplete>(new(
            defaultValue: 0,
            defaultBindingMode: BindingMode.TwoWay));
    
    public static readonly StyledProperty<int> MinimumPrefixLengthProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, int>(
            nameof(MinimumPrefixLength), 1,
            validate: IsValidMinimumPrefixLength);
    
    public static readonly StyledProperty<TimeSpan> MinimumPopulateDelayProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, TimeSpan>(
            nameof(MinimumPopulateDelay),
            TimeSpan.Zero,
            validate: IsValidMinimumPopulateDelay);
    
    public static readonly StyledProperty<int> MaxLengthProperty =
        AvaloniaTextBox.MaxLengthProperty.AddOwner<AbstractAutoComplete>();
    
    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, string?>(nameof(Value));
    
    public static readonly StyledProperty<bool> IsAllowClearProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, bool>(nameof(IsAllowClear));

    public static readonly StyledProperty<bool> IsAutoFocusProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, bool>(nameof(IsAutoFocus));
    
    public static readonly StyledProperty<string?> DefaultValueProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, string?>(nameof(DefaultValue));
    
    public static readonly StyledProperty<int> DisplayCandidateCountProperty = 
        AvaloniaProperty.Register<AbstractAutoComplete, int>(nameof (DisplayCandidateCount), 10);
    
    public static readonly StyledProperty<object?> ContentLeftAddOnProperty =
        AddOnDecoratedBox.ContentLeftAddOnProperty.AddOwner<AbstractAutoComplete>();
    
    public static readonly StyledProperty<IDataTemplate?> ContentLeftAddOnTemplateProperty =
        AddOnDecoratedBox.ContentLeftAddOnTemplateProperty.AddOwner<AbstractAutoComplete>();

    public static readonly StyledProperty<object?> ContentRightAddOnProperty =
        AddOnDecoratedBox.ContentRightAddOnProperty.AddOwner<AbstractAutoComplete>();
    
    public static readonly StyledProperty<IDataTemplate?> ContentRightAddOnTemplateProperty =
        AddOnDecoratedBox.ContentRightAddOnTemplateProperty.AddOwner<AbstractAutoComplete>();
    
    public static readonly StyledProperty<AddOnDecoratedVariant> StyleVariantProperty =
        AddOnDecoratedBox.StyleVariantProperty.AddOwner<AbstractAutoComplete>();

    public static readonly StyledProperty<AddOnDecoratedStatus> StatusProperty =
        AddOnDecoratedBox.StatusProperty.AddOwner<AbstractAutoComplete>();
    
    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, string?>(nameof(PlaceholderText));
    
    public static readonly StyledProperty<IEnumerable<IAutoCompleteOption>?> OptionsSourceProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, IEnumerable<IAutoCompleteOption>?>(nameof(OptionsSource));
    
    public static readonly StyledProperty<IDataTemplate?> OptionTemplateProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, IDataTemplate?>(nameof(OptionTemplate));
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty = 
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractAutoComplete>();
    
    public static readonly DirectProperty<AbstractAutoComplete, bool> IsLoadingProperty =
        AvaloniaProperty.RegisterDirect<AbstractAutoComplete, bool>(
            nameof(IsLoading),
            o => o.IsLoading);
    
    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaTextBox.IsReadOnlyProperty.AddOwner<AbstractAutoComplete>();
    
    public static readonly StyledProperty<double> MaxDropDownHeightProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, double>(
            nameof(MaxDropDownHeight),
            double.PositiveInfinity,
            validate: IsValidMaxDropDownHeight);
    
    public static readonly StyledProperty<IValueFilter?> FilterProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, IValueFilter?>(nameof(Filter));
    
    public static readonly DirectProperty<AbstractAutoComplete, string?> FilterValueProperty =
        AvaloniaProperty.RegisterDirect<AbstractAutoComplete, string?>(
            nameof(FilterValue),
            o => o.FilterValue,
            unsetValue: string.Empty);
    
    public static readonly StyledProperty<ValueFilterMode> FilterModeProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, ValueFilterMode>(
            nameof(FilterMode),
            defaultValue: ValueFilterMode.StartsWith,
            validate: IsValidFilterMode);
    
    public static readonly StyledProperty<AutoCompleteFilterValueSelector?> FilterValueSelectorProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, AutoCompleteFilterValueSelector?>(
            nameof(FilterValueSelector));
    
    public static readonly StyledProperty<ICompleteOptionsAsyncLoader?> OptionsAsyncLoaderProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, ICompleteOptionsAsyncLoader?>(nameof(OptionsAsyncLoader));
    
    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, bool>(
            nameof(IsDropDownOpen));
    
    public static readonly StyledProperty<AutoCompletePlacementMode> PlacementProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, AutoCompletePlacementMode>(nameof(Placement), AutoCompletePlacementMode.Bottom);
    
    public static readonly StyledProperty<bool> IsCompletionEnabledProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, bool>(
            nameof(IsCompletionEnabled));
    
    public static readonly StyledProperty<object?> SelectedItemProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, object?>(
            nameof(SelectedItem),
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);
    
    public static readonly StyledProperty<bool> ClearSelectionOnLostFocusProperty =
        AvaloniaTextBox.ClearSelectionOnLostFocusProperty.AddOwner<AbstractAutoComplete>();
    
    public static readonly StyledProperty<bool> IsPopupMatchSelectWidthProperty =
        AvaloniaProperty.Register<AbstractAutoComplete, bool>(nameof(IsPopupMatchSelectWidth), true);
    
    public PathIcon? ClearIcon
    {
        get => GetValue(ClearIconProperty);
        set => SetValue(ClearIconProperty, value);
    }
    
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    public int CaretIndex
    {
        get => GetValue(CaretIndexProperty);
        set => SetValue(CaretIndexProperty, value);
    }
    
    public TimeSpan MinimumPopulateDelay
    {
        get => GetValue(MinimumPopulateDelayProperty);
        set => SetValue(MinimumPopulateDelayProperty, value);
    }
    
    public int MinimumPrefixLength
    {
        get => GetValue(MinimumPrefixLengthProperty);
        set => SetValue(MinimumPrefixLengthProperty, value);
    }
    
    public int MaxLength
    {
        get => GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
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
    
    public string? DefaultValue
    {
        get => GetValue(DefaultValueProperty);
        set => SetValue(DefaultValueProperty, value);
    }
        
    public int DisplayCandidateCount
    {
        get => GetValue(DisplayCandidateCountProperty);
        set => SetValue(DisplayCandidateCountProperty, value);
    }
    
    public bool IsAutoFocus
    {
        get => GetValue(IsAutoFocusProperty);
        set => SetValue(IsAutoFocusProperty, value);
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
    
    public AddOnDecoratedVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }

    public AddOnDecoratedStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }
    
    public string? PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }
    
    public IEnumerable<IAutoCompleteOption>? OptionsSource
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
    
    public double MaxDropDownHeight
    {
        get => GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }
    
    private bool _isLoading;

    public bool IsLoading
    {
        get => _isLoading;
        internal set => SetAndRaise(IsLoadingProperty, ref _isLoading, value);
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
    
    private string? _filterValue;
    public string? FilterValue
    {
        get => _filterValue;
        private set
        {
            try
            {
                _allowWrite = true;
                SetAndRaise(FilterValueProperty, ref _filterValue, value);
            }
            finally
            {
                _allowWrite = false;
            }
        }
    }
    
    public ValueFilterMode FilterMode
    {
        get => GetValue(FilterModeProperty);
        set => SetValue(FilterModeProperty, value);
    }
    
    public AutoCompleteFilterValueSelector? FilterValueSelector
    {
        get => GetValue(FilterValueSelectorProperty);
        set => SetValue(FilterValueSelectorProperty, value);
    }
    
    public ICompleteOptionsAsyncLoader? OptionsAsyncLoader
    {
        get => GetValue(OptionsAsyncLoaderProperty);
        set => SetValue(OptionsAsyncLoaderProperty, value);
    }
    
    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }
    
    public AutoCompletePlacementMode Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }
    
    public bool IsCompletionEnabled
    {
        get => GetValue(IsCompletionEnabledProperty);
        set => SetValue(IsCompletionEnabledProperty, value);
    }
    
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }
    
    public bool ClearSelectionOnLostFocus
    {
        get => GetValue(ClearSelectionOnLostFocusProperty);
        set => SetValue(ClearSelectionOnLostFocusProperty, value);
    }
    
    public bool IsPopupMatchSelectWidth
    {
        get => GetValue(IsPopupMatchSelectWidthProperty);
        set => SetValue(IsPopupMatchSelectWidthProperty, value);
    }
    
    [Content]
    public ItemCollection Options { get; set; } = new();
    #endregion

    #region 公共事件定义
    
    public static readonly RoutedEvent<CompleteValueChangedEventArgs> ValueChangedEvent =
        RoutedEvent.Register<AbstractAutoComplete, CompleteValueChangedEventArgs>(
            nameof(ValueChanged),
            RoutingStrategies.Bubble);
    
    public static readonly RoutedEvent<SelectionChangedEventArgs> SelectionChangedEvent =
        RoutedEvent.Register<SelectionChangedEventArgs>(
            nameof(SelectionChanged),
            RoutingStrategies.Bubble,
            typeof(AbstractAutoComplete));
    
    public event EventHandler<CompleteValueChangedEventArgs>? ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }
    
    public event EventHandler<SelectionChangedEventArgs> SelectionChanged
    {
        add => AddHandler(SelectionChangedEvent, value);
        remove => RemoveHandler(SelectionChangedEvent, value);
    }
    
    public event EventHandler<CompleteOptionsLoadedEventArgs>? OptionsLoaded;
    public event EventHandler<CompletePopulatingEventArgs>? Populating;
    public event EventHandler<CompletePopulatedEventArgs>? Populated;
    public event EventHandler<CancelEventArgs>? DropDownOpening;
    public event EventHandler? DropDownOpened;
    public event EventHandler<CancelEventArgs>? DropDownClosing;
    public event EventHandler? DropDownClosed;
    
    #endregion

    #region 内部属性定义
    
    internal static readonly DirectProperty<AbstractAutoComplete, double> ItemHeightProperty =
        AvaloniaProperty.RegisterDirect<AbstractAutoComplete, double>(
            nameof(ItemHeight),
            o => o.ItemHeight,
            (o, v) => o.ItemHeight = v);
    
    internal static readonly DirectProperty<AbstractAutoComplete, double> MaxPopupHeightProperty =
        AvaloniaProperty.RegisterDirect<AbstractAutoComplete, double>(
            nameof(MaxPopupHeight),
            o => o.MaxPopupHeight,
            (o, v) => o.MaxPopupHeight = v);
    
    internal static readonly DirectProperty<AbstractAutoComplete, double> MinPopupWidthProperty =
        AvaloniaProperty.RegisterDirect<AbstractAutoComplete, double>(
            nameof(MinPopupWidth),
            o => o.MinPopupWidth,
            (o, v) => o.MinPopupWidth = v);
    
    internal static readonly DirectProperty<AbstractAutoComplete, double> EffectivePopupWidthProperty =
        AvaloniaProperty.RegisterDirect<AbstractAutoComplete, double>(
            nameof(EffectivePopupWidth),
            o => o.EffectivePopupWidth,
            (o, v) => o.EffectivePopupWidth = v);
    
    internal static readonly DirectProperty<AbstractAutoComplete, Thickness> PopupContentPaddingProperty =
        AvaloniaProperty.RegisterDirect<AbstractAutoComplete, Thickness>(nameof(PopupContentPadding),
            o => o.PopupContentPadding,
            (o, v) => o.PopupContentPadding = v);
    
    internal static readonly DirectProperty<AbstractAutoComplete, PlacementMode> PopupPlacementProperty =
        AvaloniaProperty.RegisterDirect<AbstractAutoComplete, PlacementMode>(nameof(PopupPlacement),
            o => o.PopupPlacement,
            (o, v) => o.PopupPlacement = v);
    
    internal static readonly DirectProperty<AbstractAutoComplete, IValueFilter?> EffectiveFilterProperty =
        AvaloniaProperty.RegisterDirect<AbstractAutoComplete, IValueFilter?>(nameof(EffectiveFilter),
            o => o.EffectiveFilter,
            (o, v) => o.EffectiveFilter = v);
    
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
    
    private double _effectivePopupWidth;

    internal double EffectivePopupWidth
    {
        get => _effectivePopupWidth;
        set => SetAndRaise(EffectivePopupWidthProperty, ref _effectivePopupWidth, value);
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
    
    private IValueFilter? _effectiveFilter;

    internal IValueFilter? EffectiveFilter
    {
        get => _effectiveFilter;
        set => SetAndRaise(EffectiveFilterProperty, ref _effectiveFilter, value);
    }
    
    protected AvaloniaTextBox? TextBox
    {
        get => _textBox;
        set
        {
            _textBoxSubscriptions?.Dispose();
            _textBox = value;

            // Attach handlers
            if (_textBox != null)
            {
                _textBoxSubscriptions =
                    _textBox.GetObservable(AvaloniaTextBox.TextProperty)
                            .Skip(1)
                            .Subscribe(_ => HandleTextBoxTextChanged());

                if (Value != null)
                {
                    UpdateValue(Value);
                }
            }
        }
    }
    
    private int TextBoxSelectionStart
    {
        get
        {
            if (TextBox != null)
            {
                return Math.Min(TextBox.SelectionStart, TextBox.SelectionEnd);
            }
            return 0;
        }
    }
    
    private int TextBoxSelectionLength
    {
        get
        {
            if (TextBox != null)
            {
                return Math.Abs(TextBox.SelectionEnd - TextBox.SelectionStart);
            }
            return 0;
        }
    }
    
    protected ICandidateList? CandidateList
    {
        get => _candidateList;
        set
        {
            if (_candidateList != null)
            {
                _candidateList.Commit           -= HandleCandidateListComplete;
                _candidateList.Cancel           -= HandleCandidateListCanceled;
                _candidateList.ItemsSource      =  null;
            }

            _candidateList = value;

            if (_candidateList != null)
            {
                _candidateList.Commit           += HandleCandidateListComplete;
                _candidateList.Cancel           += HandleCandidateListCanceled;
                _candidateList.ItemsSource      =  _view;
            }
        }
    }
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => AutoCompleteToken.ID;

    #endregion
    
    private protected DispatcherTimer? _delayTimer;
    private protected AvaloniaTextBox? _textBox;
    private protected IList<IAutoCompleteOption>? _view;
    private protected ICandidateList? _candidateList;
    private protected Popup? _popup;
    private protected bool _ignorePopupClose;
    private bool _allowWrite;
    private bool _cancelRequested;
    private bool _filterInAction;
    private int _ignoreValuePropertyChange;
    private bool _ignorePropertyChange;
    private bool _popupHasOpened;
    private int _textSelectionStart;
    private CompositeDisposable? _subscriptionsOnOpen;
    private CancellationTokenSource? _populationCancellationTokenSource;
    private IDisposable? _textBoxSubscriptions;
    private bool _userCalledPopulate;
    private bool _ignoreTextSelectionChange;
    private bool _skipSelectedItemTextUpdate;
    private bool _isFocused;
    
    static AbstractAutoComplete()
    {
        IsTabStopProperty.OverrideDefaultValue<AbstractAutoComplete>(false);
        FocusableProperty.OverrideDefaultValue<AbstractAutoComplete>(true);
        
        SelectedItemProperty.Changed.AddClassHandler<AbstractAutoComplete>((x,e) => x.HandleSelectedItemPropertyChanged(e));
        IsDropDownOpenProperty.Changed.AddClassHandler<AbstractAutoComplete>((x,e) => x.HandleIsDropDownOpenChanged(e));
        MinimumPopulateDelayProperty.Changed.AddClassHandler<AbstractAutoComplete>((x,e) => x.HandleMinimumPopulateDelayChanged(e));
        PlacementProperty.Changed.AddClassHandler<AbstractAutoComplete>((x,e) => x.HandlePlacementChanged());
        ValueProperty.Changed.AddClassHandler<AbstractAutoComplete>((x,e) => x.HandleValuePropertyChanged(e));
        OptionsSourceProperty.Changed.AddClassHandler<AbstractAutoComplete>((x,e) => x.HandleItemsSourceChanged(e));
        FilterModeProperty.Changed.AddClassHandler<AbstractAutoComplete>((x,e) => x.HandleFilterModePropertyChanged(e));
        FilterProperty.Changed.AddClassHandler<AbstractAutoComplete>((x,e) => x.HandleFilterPropertyChanged(e));
        FilterValueProperty.Changed.AddClassHandler<AbstractAutoComplete>((x,e) => x.HandleFilterValuePropertyChanged(e));
    }

    public AbstractAutoComplete()
    {
        this.RegisterResources();
        Options.CollectionChanged += HandleOptionsChanged;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        HandlePlacementChanged();
    }

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);
        FocusChanged(HasFocus());
    }
    
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);
        FocusChanged(HasFocus());
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DisplayCandidateCountProperty ||
            change.Property == ItemHeightProperty)
        {
            ConfigureMaxPopupHeight();
        }
        else if (change.Property == IsPopupMatchSelectWidthProperty)
        {
            ConfigurePopupMinWith(DesiredSize.Width);
        }
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        ConfigurePopupMinWith(e.NewSize.Width);
    }

    protected virtual void ConfigureMaxPopupHeight()
    {
        MaxPopupHeight = ItemHeight * DisplayCandidateCount + PopupContentPadding.Top + PopupContentPadding.Bottom;
    }

    protected bool HasFocus() => IsKeyboardFocusWithin;
    
    private void FocusChanged(bool hasFocus)
    {
        // The OnGotFocus & OnLostFocus are asynchronously and cannot
        // reliably tell you that have the focus.  All they do is let you
        // know that the focus changed sometime in the past.  To determine
        // if you currently have the focus you need to do consult the
        // FocusManager (see HasFocus()).
        
        bool wasFocused = _isFocused;
        _isFocused = hasFocus;
        
        if (hasFocus)
        {
        
            if (!wasFocused && TextBox != null && TextBoxSelectionLength <= 0)
            {
                TextBox.Focus();
                TextBox.SelectAll();
            }
        }
        else
        {
            // Check if we still have focus in the parent's focus scope
            if (GetFocusScope() is { } scope &&
                (FocusUtils.GetFocusManager(this)?.GetFocusedElement(scope) is not { } focused ||
                 (focused != this &&
                  (focused is Visual v && !this.IsVisualAncestorOf(v)))))
            {
                SetCurrentValue(IsDropDownOpenProperty, false);
            }
        
            _userCalledPopulate = false;
        
            var textBoxContextMenuIsOpen = TextBox?.ContextFlyout?.IsOpen == true || TextBox?.ContextMenu?.IsOpen == true;
            var contextMenuIsOpen = ContextFlyout?.IsOpen == true || ContextMenu?.IsOpen == true;
        
            if (!textBoxContextMenuIsOpen && !contextMenuIsOpen && ClearSelectionOnLostFocus)
            {
                ClearTextBoxSelection();
            }
        }
        
        _isFocused = hasFocus;
        
        IFocusScope? GetFocusScope()
        {
            IInputElement? c = this;
        
            while (c != null)
            {
                if (c is IFocusScope scope &&
                    c is Visual v &&
                    v.GetVisualRoot() is Visual root &&
                    root.IsVisible)
                {
                    return scope;
                }
        
                c = (c as Visual)?.GetVisualParent<IInputElement>() ??
                    ((c as IHostedVisualTreeRoot)?.Host as IInputElement);
            }
        
            return null;
        }
    }
    
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
    
    private void ClearTextBoxSelection()
    {
        if (TextBox != null)
        {
            int length = TextBox.Text?.Length ?? 0;
            TextBox.SelectionStart = length;
            TextBox.SelectionEnd   = length;
        }
    }
    
    private void HandleMinimumPopulateDelayChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var newValue = (TimeSpan)e.NewValue!;

        // Stop any existing timer
        if (_delayTimer != null)
        {
            _delayTimer.Stop();

            if (newValue == TimeSpan.Zero)
            {
                _delayTimer.Tick -= PopulateDropDown;
                _delayTimer      =  null;
            }
        }

        if (newValue > TimeSpan.Zero)
        {
            // Create or clear a dispatcher timer instance
            if (_delayTimer == null)
            {
                _delayTimer      =  new DispatcherTimer();
                _delayTimer.Tick += PopulateDropDown;
            }

            // Set the new tick interval
            _delayTimer.Interval = newValue;
        }
    }

    private void PopulateDropDown(object? sender, EventArgs e)
    {
        _delayTimer?.Stop();

        // Update the prefix/search text.
        FilterValue = Value;
        
        if (TryPopulateAsync(FilterValue))
        {
            return;
        }
        
        // The Populated event enables advanced, custom filtering. The
        // client needs to directly update the ItemsSource collection or
        // call the Populate method on the control to continue the
        // display process if Cancel is set to true.
        var populating = new CompletePopulatingEventArgs(FilterValue);
        NotifyPopulating(populating);
        if (!populating.Cancel)
        {
            PopulateComplete();
        }
    }
    
    protected virtual void NotifyPopulating(CompletePopulatingEventArgs e)
    {
        IsLoading = true;
        Populating?.Invoke(this, e);
    }

    protected virtual void NotifyPopulated(CompletePopulatedEventArgs e)
    {
        IsLoading = false;
        Populated?.Invoke(this, e);
    }

    private void HandlePlacementChanged()
    {
        if (Placement == AutoCompletePlacementMode.Bottom)
        {
            PopupPlacement = PlacementMode.BottomEdgeAlignedLeft;
        }
        else
        {
            PopupPlacement = PlacementMode.TopEdgeAlignedLeft;
        }
    }

    private void HandleValuePropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        HandleValueUpdated((string?)e.NewValue, false);
    }
    
    private void HandleFilterValuePropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_ignorePropertyChange)
        {
            _ignorePropertyChange = false;
            return;
        }

        // Ensure the property is only written when expected
        if (!_allowWrite)
        {
            // Reset the old value before it was incorrectly written
            _ignorePropertyChange = true;
            SetCurrentValue(FilterValueProperty, e.OldValue);
            throw new InvalidOperationException("Cannot set read-only property FilterValue.");
        }
    }
    
    private void HandleItemsSourceChanged(AvaloniaPropertyChangedEventArgs change)
    {
        // Clear and set the view on the selection adapter
        ClearView();
        Options.SetItemsSource(change.GetNewValue<IEnumerable<IAutoCompleteOption>?>());
        RefreshView();
    }

    private void HandleOptionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshView();
    }
    
    private void HandleValueUpdated(string? newText, bool userInitiated)
    {
        // Only process this event if it is coming from someone outside
        // setting the Text dependency property directly.
        if (_ignoreValuePropertyChange > 0)
        {
            _ignoreValuePropertyChange--;
            return;
        }
        
        newText ??= string.Empty;
        
        // The TextBox.TextChanged event was not firing immediately and
        // was causing an immediate update, even with wrapping. If there is
        // a selection currently, no update should happen.
        if (IsCompletionEnabled && TextBox != null && TextBoxSelectionLength > 0 && TextBoxSelectionStart != (TextBox.Text?.Length ?? 0))
        {
            return;
        }
        
        // Evaluate the conditions needed for completion.
        // 1. Minimum prefix length
        // 2. If a delay timer is in use, use it
        bool minimumLengthReached = newText.Length >= MinimumPrefixLength && MinimumPrefixLength >= 0;
        
        _userCalledPopulate = minimumLengthReached && userInitiated;
        
        // Update the interface and values only as necessary
        UpdateValue(newText, userInitiated);
        
        if (minimumLengthReached)
        {
            _ignoreTextSelectionChange = true;
        
            if (_delayTimer != null)
            {
                _delayTimer.Start();
            }
            else
            {
                PopulateDropDown(this, EventArgs.Empty);
            }
        }
        else
        {
            FilterValue = string.Empty;
            if (SelectedItem != null)
            {
                _skipSelectedItemTextUpdate = true;
            }
        
            SetCurrentValue(SelectedItemProperty, null);
        
            if (IsDropDownOpen)
            {
                SetCurrentValue(IsDropDownOpenProperty, false);
            }
        }
    }

    private void UpdateValue(string? value)
    {
        UpdateValue(value, null);
    }

    private void UpdateValue(string? value, bool? userInitiated)
    {
        bool callTextChanged = false;
        // Update the Text dependency property
        if ((userInitiated ?? true) && Value != value)
        {
            _ignoreValuePropertyChange++;
            SetCurrentValue(ValueProperty, value);
            callTextChanged = true;
        }
        
        // Update the TextBox's Text dependency property
        if ((userInitiated == null || userInitiated == false) && TextBox != null && TextBox.Text != value)
        {
            _ignoreValuePropertyChange++;
            TextBox.Text = value ?? string.Empty;
        
            // Text dependency property value was set, fire event
            if (!callTextChanged && (Value == value || Value == null))
            {
                callTextChanged = true;
            }
        }

        if (callTextChanged)
        {
            NotifyValueChanged(new CompleteValueChangedEventArgs(Value, ValueChangedEvent));
        }
    }
    
    protected void ClearView()
    {
        _view = null;
    }
    
    private void RefreshView()
    {
        // If we have a running filter, trigger a request first
        if (_filterInAction)
        {
            _cancelRequested = true;
        }
        
        // Indicate that filtering is ongoing
        _filterInAction = true;
        
        try
        {
            // Cache the current text value
            var text = Value ?? string.Empty;
        
            // Determine if any filtering mode is on
            bool filtering = EffectiveFilter != null;
        
            var items = Options;
        
            // cache properties
            var newViewItems = new Collection<IAutoCompleteOption>();
        
            // if the mode is objectFiltering and itemFilter is null, we throw an exception
            if (FilterMode == ValueFilterMode.Custom && Filter is null)
            {
                throw new Exception(
                    "ItemFilter property can not be null when FilterMode has value ValueFilterMode.Custom");
            }
        
            foreach (var item in items)
            {
                if (item is IAutoCompleteOption option)
                {
                    // Exit the fitter when requested if cancellation is requested
                    if (_cancelRequested)
                    {
                        return;
                    }
        
                    var inResults = !filtering;

                    if (EffectiveFilter != null)
                    {
                        if (!inResults)
                        {
                            inResults = EffectiveFilter.Filter(GetValueByOption(option), text);
                        }
                    }
              
                    if (inResults)
                    {
                        newViewItems.Add(option);
                    }
                }
            }
            
            _view = newViewItems;
            
            if (_candidateList != null)
            {
                if (_candidateList.ItemsSource != _view)
                {
                    _candidateList.ItemsSource = _view;
                }
            }
        }
        finally
        {
            // indicate that filtering is not ongoing anymore
            _filterInAction  = false;
            _cancelRequested = false;
        }
    }
    
    private static bool IsValidMinimumPrefixLength(int value) => value >= -1;
    private static bool IsValidMinimumPopulateDelay(TimeSpan value) => value.TotalMilliseconds >= 0.0;
    private static bool IsValidMaxDropDownHeight(double value) => value >= 0.0;

    private static bool IsValidFilterMode(ValueFilterMode mode)
    {
        switch (mode)
        {
            case ValueFilterMode.None:
            case ValueFilterMode.StartsWith:
            case ValueFilterMode.StartsWithCaseSensitive:
            case ValueFilterMode.StartsWithOrdinal:
            case ValueFilterMode.StartsWithOrdinalCaseSensitive:
            case ValueFilterMode.Contains:
            case ValueFilterMode.ContainsCaseSensitive:
            case ValueFilterMode.ContainsOrdinal:
            case ValueFilterMode.ContainsOrdinalCaseSensitive:
            case ValueFilterMode.Equals:
            case ValueFilterMode.EqualsCaseSensitive:
            case ValueFilterMode.EqualsOrdinal:
            case ValueFilterMode.EqualsOrdinalCaseSensitive:
            case ValueFilterMode.Custom:
                return true;
            default:
                return false;
        }
    }

    private void HandleFilterModePropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var mode = (ValueFilterMode)e.NewValue!;
        // Sets the filter predicate for the new value
        EffectiveFilter = ValueFilterFactory.BuildFilter(mode) ?? Filter;
    }
    
    private void HandleFilterPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var value = e.NewValue as AutoCompleteFilterPredicate<object>;

        // If null, revert to the "None" predicate
        if (value == null)
        {
            SetCurrentValue(FilterModeProperty, ValueFilterMode.None);
        }
        else
        {
            SetCurrentValue(FilterModeProperty, ValueFilterMode.Custom);
        }
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

    protected virtual void NotifyValueChanged(CompleteValueChangedEventArgs eventArgs)
    {
        RaiseEvent(eventArgs);
    }
    
    private void HandleIsDropDownOpenChanged(AvaloniaPropertyChangedEventArgs e)
    {
        // Ignore the change if requested
        if (_ignorePropertyChange)
        {
            _ignorePropertyChange = false;
            return;
        }

        bool oldValue = (bool)e.OldValue!;
        bool newValue = (bool)e.NewValue!;

        if (newValue)
        {
            HandleValueUpdated(Value, true);
        }
        else
        {
            ClosingDropDown(oldValue);
        }

        UpdatePseudoClasses();
    }
    
    private void HandleSelectedItemPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_ignorePropertyChange)
        {
            _ignorePropertyChange = false;
            return;
        }
    
        // Update the text display
        if (_skipSelectedItemTextUpdate)
        {
            _skipSelectedItemTextUpdate = false;
        }
        else
        {
            HandleSelectedItemChanged(e.NewValue);
        }
    
        // Fire the SelectionChanged event
        var removed = new List<object>();
        if (e.OldValue != null)
        {
            removed.Add(e.OldValue);
        }
    
        var added = new List<object>();
        if (e.NewValue != null)
        {
            added.Add(e.NewValue);
        }
    
        NotifySelectionChanged(new SelectionChangedEventArgs(SelectionChangedEvent, removed, added));
    }
    
    
    protected virtual void NotifySelectionChanged(SelectionChangedEventArgs e)
    {
        RaiseEvent(e);
    }
    
    private void ClosingDropDown(bool oldValue)
    {
        var args = new CancelEventArgs();
        NotifyDropDownClosing(args);

        if (args.Cancel)
        {
            _ignorePropertyChange = true;
            SetCurrentValue(IsDropDownOpenProperty, oldValue);
        }
        else
        {
            CloseDropDown();
        }

        UpdatePseudoClasses();
    }
    
    private void OpenDropDown()
    {
        if (_popup != null)
        {
            _popup.IsMotionAwareOpen = true;
        }
        _popupHasOpened = true;
        NotifyDropDownOpened(EventArgs.Empty);
    }
    
    private void CloseDropDown()
    {
        if (_popupHasOpened)
        {
            if (_popup != null)
            {
                _popup.IsMotionAwareOpen = false;
            }
            NotifyDropDownClosed(EventArgs.Empty);
        }
    }
    
    private void OpeningDropDown(bool oldValue)
    {
        var args = new CancelEventArgs();

        // Opening
        NotifyDropDownOpening(args);

        if (args.Cancel)
        {
            _ignorePropertyChange = true;
            SetCurrentValue(IsDropDownOpenProperty, oldValue);
        }
        else
        {
            OpenDropDown();
        }

        UpdatePseudoClasses();
    }
    
    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(AutoCompletePseudoClass.CandidatePopupOpen, IsDropDownOpen);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        if (_popup != null)
        {
            _popup.Opened -= HandlePopupOpened;
            _popup.Closed -= HandlePopupClosed;
        }

        TextBox       = e.NameScope.Find<AvaloniaTextBox>(AutoCompleteThemeConstants.TextBoxPart);
        _popup        = e.NameScope.Find<Popup>(AutoCompleteThemeConstants.PopupPart);
        CandidateList = e.NameScope.Find<ICandidateList>(AutoCompleteThemeConstants.CandidateListPart);

        if (_popup != null)
        {
            _popup.ClickHidePredicate  =  PopupClosePredicate;
            _popup.IgnoreFirstDetected =  false;
            _popup.Opened              += HandlePopupOpened;
            _popup.Closed              += HandlePopupClosed;
            _popup.CloseAction         =  PopupCloseAction;
        }
        
        // If the drop down property indicates that the popup is open,
        // flip its value to invoke the changed handler.
        if (IsDropDownOpen && _popup != null && !_popup.IsOpen)
        {
            OpeningDropDown(false);
        }
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is Window window)
        {
            window.Deactivated += HandleWindowDeactivated;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is Window window)
        {
            window.Deactivated -= HandleWindowDeactivated;
        }
    }
    
    private void HandleWindowDeactivated(object? sender, EventArgs e)
    {
        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    private void PopupCloseAction(Popup popup)
    {
        popup.MotionAwareClose();
        _ignorePropertyChange = true;
        SetCurrentValue(IsDropDownOpenProperty, false);
        ClosingDropDown(true);
    }
    
    protected bool PopupClosePredicate(IPopupHostProvider hostProvider, RawPointerEventArgs args)
    {
        if (_ignorePopupClose)
        {
            _ignorePopupClose = false;
            return false;
        }
        if (hostProvider.PopupHost is OverlayPopupHost overlayPopupHost && args.Root is Control root)
        {
            var offset = overlayPopupHost.TranslatePoint(default, root);
            if (offset.HasValue)
            {
                var bounds = new Rect(offset.Value, overlayPopupHost.Bounds.Size);
                return !bounds.Contains(args.Position);
            }
        }
        else if (hostProvider.PopupHost is PopupRoot popupRoot)
        {
            var popupRoots = new HashSet<PopupRoot>();
            popupRoots.Add(popupRoot);
            return !popupRoots.Contains(args.Root);
        }
        
        return false;
    }
    
    private void HandlePopupOpened(object? sender, EventArgs e)
    {
        _subscriptionsOnOpen?.Dispose();
        _subscriptionsOnOpen = new CompositeDisposable(2);
        this.GetObservable(IsVisibleProperty).Subscribe(HandleIsVisibleChanged).DisposeWith(_subscriptionsOnOpen);
        this.GetObservable(IsEnabledProperty).Subscribe(HandleIsEnabledChanged).DisposeWith(_subscriptionsOnOpen);
        foreach (var parent in this.GetVisualAncestors().OfType<Control>())
        {
            parent.GetObservable(IsVisibleProperty).Subscribe(HandleIsVisibleChanged).DisposeWith(_subscriptionsOnOpen);
        }
        NotifyDropDownOpened(EventArgs.Empty);
        var selectedItem = TryGetMatch(Value, _view, ValueFilterFactory.BuildFilter(ValueFilterMode.EqualsCaseSensitive));
        CandidateList!.SelectedItem = selectedItem;
    }
    
    private void HandlePopupClosed(object? sender, EventArgs e)
    {
        _subscriptionsOnOpen?.Dispose();
        _subscriptionsOnOpen = null;
        // Force the drop down dependency property to be false.
        if (IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
        }

        // Fire the DropDownClosed event
        if (_popupHasOpened)
        {
            NotifyDropDownClosed(EventArgs.Empty);
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
    
    protected virtual void HandleTextBoxTextChanged()
    {
        //Uses Dispatcher.Post to allow the TextBox selection to update before processing
        Dispatcher.UIThread.Post(() =>
        {
            // Call the central updated text method as a user-initiated action
            HandleValueUpdated(_textBox!.Text, true);
            if (TextBox != null && string.IsNullOrWhiteSpace(TextBox.Text))
            {
                ClearView();
            }
        });
    }
    
    private bool TryPopulateAsync(string? filterValue)
    {
        _populationCancellationTokenSource?.Cancel(false);
        _populationCancellationTokenSource?.Dispose();
        _populationCancellationTokenSource = null;

        if (OptionsAsyncLoader == null)
        {
            return false;
        }

        _populationCancellationTokenSource = new CancellationTokenSource();
        var task = PopulateAsync(filterValue, _populationCancellationTokenSource.Token);
        if (task.Status == TaskStatus.Created)
        {
            task.Start();
        }

        return true;
    }
    
    private async Task PopulateAsync(string? filterValue, CancellationToken cancellationToken)
    {
        try
        {
            if (OptionsAsyncLoader == null)
            {
                return;
            }

            var result     = await OptionsAsyncLoader.LoadAsync(filterValue, cancellationToken);
            var resultList = result.Data;
            OptionsLoaded?.Invoke(this, new CompleteOptionsLoadedEventArgs(filterValue, result));
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            
            SetCurrentValue(OptionsSourceProperty, resultList);
            
            Dispatcher.UIThread.Post(() =>
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    PopulateComplete();
                }
            });
        }
        catch (TaskCanceledException e)
        {
            OptionsLoaded?.Invoke(this, new CompleteOptionsLoadedEventArgs(filterValue, new CompleteOptionsLoadResult()
            {
                UserFriendlyMessage = e.Message,
                StatusCode          = RpcStatusCode.Cancelled
            }));
        }
        finally
        {
            _populationCancellationTokenSource?.Dispose();
            _populationCancellationTokenSource = null;
        }
    }
    
    private void PopulateComplete()
    {
        // Apply the search filter
        RefreshView();
        
        // Fire the Populated event containing the read-only view data.
        var populated = new CompletePopulatedEventArgs(_view);
        NotifyPopulated(populated);
        
        bool isDropDownOpen = _userCalledPopulate && (_view!.Count > 0);
        if (isDropDownOpen != IsDropDownOpen)
        {
            _ignorePropertyChange = true;
            SetCurrentValue(IsDropDownOpenProperty, isDropDownOpen);
        }
        if (IsDropDownOpen)
        {
            OpeningDropDown(false);
        }
        else
        {
            ClosingDropDown(true);
        }

        UpdateTextCompletion(_userCalledPopulate);
    }
    
    private void UpdateTextCompletion(bool userInitiated)
    {
        // By default this method will clear the selected value
        object? newSelectedItem = null;
        string? value           = Value;
        //
        // Text search is StartsWith explicit and only when enabled, in
        // line with WPF's ComboBox lookup. When in use it will associate
        // a Value with the Text if it is found in ItemsSource. This is
        // only valid when there is data and the user initiated the action.
        if (_view?.Count > 0)
        {
            if (IsCompletionEnabled && TextBox != null && userInitiated)
            {
                int currentLength = TextBox.Text?.Length ?? 0;
                int selectionStart = TextBoxSelectionStart;
                if (selectionStart == value?.Length && selectionStart > _textSelectionStart)
                {
                    // When the FilterMode dependency property is set to
                    // either StartsWith or StartsWithCaseSensitive, the
                    // first item in the view is used. This will improve
                    // performance on the lookup. It assumes that the
                    // FilterMode the user has selected is an acceptable
                    // case sensitive matching function for their scenario.
                    var top = FilterMode == ValueFilterMode.StartsWith || FilterMode == ValueFilterMode.StartsWithCaseSensitive
                        ? _view[0]
                        : TryGetMatch(value, _view, ValueFilterFactory.BuildFilter(ValueFilterMode.StartsWith));
                
                    // If the search was successful, update SelectedItem
                    if (top != null)
                    {
                        newSelectedItem = top;
                        var topString = (top.Value ?? top.Header ?? top.Key)?.ToString();
                
                        // Only replace partially when the two words being the same
                        int minLength = Math.Min(topString?.Length ?? 0, Value?.Length ?? 0);
                        if (string.Equals(Value?.Substring(0, minLength), topString?.Substring(0, minLength),  StringComparison.CurrentCultureIgnoreCase))
                        {
                            // Update the text
                            UpdateValue(topString);
                            // Select the text past the user's caret
                            TextBox.SelectionStart = currentLength;
                            TextBox.SelectionEnd = topString?.Length ?? 0;
                        }
                    }
                }
            }
            else
            {
                // Perform an exact string lookup for the text. This is a
                // design change from the original Toolkit release when the
                // IsTextCompletionEnabled property behaved just like the
                // WPF ComboBox's IsTextSearchEnabled property.
                //
                // This change provides the behavior that most people expect
                // to find: a lookup for the value is always performed.
                newSelectedItem = TryGetMatch(value, _view, ValueFilterFactory.BuildFilter(ValueFilterMode.EqualsCaseSensitive));
            }
        }
        
        // Update the selected item property
        
        if (SelectedItem != newSelectedItem)
        {
            _skipSelectedItemTextUpdate = true;
        }
        SetCurrentValue(SelectedItemProperty, newSelectedItem);
        
        // Restore updates for TextSelection
        if (_ignoreTextSelectionChange)
        {
            _ignoreTextSelectionChange = false;
            if (TextBox != null)
            {
                _textSelectionStart = TextBoxSelectionStart;
            }
        }
    }
    
    private IAutoCompleteOption? TryGetMatch(string? filterValue, IList<IAutoCompleteOption>? view, IValueFilter? predicate)
    {
        if (predicate is null)
        {
            return null;
        }

        if (view != null && view.Count > 0)
        {
            foreach (var option in view)
            {
                var value = GetValueByOption(option);
                if (predicate.Filter(value, filterValue))
                {
                    return option;
                }
            }
        }

        return null;
    }

    private string? GetValueByOption(IAutoCompleteOption option)
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
    
    private void HandleSelectedItemChanged(object? newItem)
    {
        string? text = null;

        if (newItem == null)
        {
            text = FilterValue;
        }
        else if (newItem is IAutoCompleteOption option)
        {
            text = option.Value?.ToString() ?? option.Header?.ToString() ?? option.Key;
        }
        // Update the Text property and the TextBox values
        UpdateValue(text);
        
        // Move the caret to the end of the text box
        ClearTextBoxSelection();
    }
    
    private protected virtual void HandleCandidateListComplete(object? sender, RoutedEventArgs e)
    {
        SetCurrentValue(SelectedItemProperty, _candidateList!.SelectedItem);
        SetCurrentValue(IsDropDownOpenProperty, false);
        // Text should not be selected
        ClearTextBoxSelection();
        TextBox!.Focus();
    }
    
    private void HandleCandidateListCanceled(object? sender, RoutedEventArgs e)
    {
        UpdateValue(FilterValue);
        // Completion will update the selected value
        UpdateTextCompletion(false);
        SetCurrentValue(IsDropDownOpenProperty, false);
    }
    
    protected virtual void ConfigurePopupMinWith(double selectWidth)
    {
        if (IsPopupMatchSelectWidth)
        {
            SetCurrentValue(EffectivePopupWidthProperty, selectWidth);
        }
    }
    
    protected override void OnPointerPressed(PointerPressedEventArgs e)
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
            // When a drop-down is open with OverlayDismissEventPassThrough enabled and the control
            // is pressed, close the drop-down
            if (e.Source is Control sourceControl)
            {
                var textBox = sourceControl.FindAncestorOfType<AvaloniaTextBox>();
                if (textBox != null)
                {
                    _ignorePopupClose = true;
                    return;
                }
            }
            else
            {
                SetCurrentValue(IsDropDownOpenProperty, false); 
                e.Handled = true;
            }
        }
        else
        {
            PseudoClasses.Set(StdPseudoClass.Pressed, true);
        }
    }
    
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (!e.Handled && e.Source is Visual source)
        {
            if (_popup?.IsInsidePopup(source) == true)
            {
                e.Handled = true;
            }
            else if (PseudoClasses.Contains(StdPseudoClass.Pressed))
            {
                if (IsDropDownOpen)
                {
                    SetCurrentValue(IsDropDownOpenProperty, false);
                }
                else if (_view?.Count > 0)
                {
                    _ignorePropertyChange = true;
                    SetCurrentValue(IsDropDownOpenProperty, true);
                    OpeningDropDown(false);
                }
       
                e.Handled = true;
            }
        }
    
        PseudoClasses.Set(StdPseudoClass.Pressed, false);
        base.OnPointerReleased(e);
    }
}