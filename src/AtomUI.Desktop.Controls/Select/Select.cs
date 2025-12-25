using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.Data;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Diagnostics;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public enum SelectMode
{
    Single,
    Multiple,
    Tags
}

public class Select : TemplatedControl,
                      IMotionAwareControl,
                      IControlSharedTokenResourcesHost,
                      ISizeTypeAware
{
    #region 公共属性定义
    public static readonly StyledProperty<IEnumerable<ISelectOption>?> OptionsSourceProperty =
        AvaloniaProperty.Register<Select, IEnumerable<ISelectOption>?>(nameof(OptionsSource));
    
    public static readonly StyledProperty<IDataTemplate?> OptionTemplateProperty =
        AvaloniaProperty.Register<Select, IDataTemplate?>(nameof(OptionTemplate));
    
    public static readonly StyledProperty<bool> IsAllowClearProperty =
        AvaloniaProperty.Register<Select, bool>(nameof(IsAllowClear));
    
    public static readonly StyledProperty<bool> IsAutoClearSearchValueProperty =
        AvaloniaProperty.Register<Select, bool>(nameof(IsAutoClearSearchValue));
    
    public static readonly StyledProperty<bool> IsDefaultActiveFirstOptionProperty =
        AvaloniaProperty.Register<Select, bool>(nameof(IsDefaultActiveFirstOption));
    
    public static readonly StyledProperty<bool> IsDefaultOpenProperty =
        AvaloniaProperty.Register<Select, bool>(nameof(IsDefaultOpen));
    
    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<Select, bool>(nameof(IsDropDownOpen));
    
    public static readonly StyledProperty<bool> IsGroupEnabledProperty =
        List.IsGroupEnabledProperty.AddOwner<Select>();
    
    public static readonly StyledProperty<string> GroupPropertyPathProperty =
        List.GroupPropertyPathProperty.AddOwner<Select>();
    
    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        AvaloniaProperty.Register<Select, string?>(nameof(PlaceholderText));
    
    public static readonly StyledProperty<IBrush?> PlaceholderForegroundProperty =
        AvaloniaProperty.Register<Select, IBrush?>(nameof(PlaceholderForeground));
    
    public static readonly StyledProperty<bool> IsPopupMatchSelectWidthProperty =
        AvaloniaProperty.Register<Select, bool>(nameof(IsPopupMatchSelectWidth), true);
    
    public static readonly StyledProperty<bool> IsSearchEnabledProperty =
        AvaloniaProperty.Register<Select, bool>(nameof(IsSearchEnabled));
    
    public static readonly StyledProperty<bool> IsHideSelectedOptionsProperty =
        AvaloniaProperty.Register<Select, bool>(nameof(IsHideSelectedOptions));
    
    public static readonly StyledProperty<int> DisplayPageSizeProperty = 
        AvaloniaProperty.Register<Select, int>(nameof (DisplayPageSize), 10);
    
    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<Select, bool>(nameof(IsLoading));
    
    public static readonly StyledProperty<int> MaxCountProperty =
        AvaloniaProperty.Register<Select, int>(nameof(MaxCount), int.MaxValue);
    
    public static readonly StyledProperty<bool> IsShowMaxCountIndicatorProperty =
        AvaloniaProperty.Register<Select, bool>(nameof(IsShowMaxCountIndicator));
        
    public static readonly StyledProperty<int?> MaxTagCountProperty =
        AvaloniaProperty.Register<Select, int?>(nameof(MaxTagCount));
    
    public static readonly StyledProperty<bool?> IsResponsiveMaxTagCountProperty =
        AvaloniaProperty.Register<Select, bool?>(nameof(IsResponsiveMaxTagCount));
    
    public static readonly StyledProperty<string?> MaxTagPlaceholderProperty =
        AvaloniaProperty.Register<Select, string?>(nameof(MaxTagPlaceholder));

    public static readonly StyledProperty<SelectMode> ModeProperty =
        AvaloniaProperty.Register<Select, SelectMode>(nameof(Mode));
   
    public static readonly StyledProperty<object?> LeftAddOnProperty =
        AddOnDecoratedBox.LeftAddOnProperty.AddOwner<Select>();
    
    public static readonly StyledProperty<IDataTemplate?> LeftAddOnTemplateProperty =
        AddOnDecoratedBox.LeftAddOnTemplateProperty.AddOwner<Select>();

    public static readonly StyledProperty<object?> RightAddOnProperty =
        AddOnDecoratedBox.RightAddOnProperty.AddOwner<Select>();
    
    public static readonly StyledProperty<IDataTemplate?> RightAddOnTemplateProperty =
        AddOnDecoratedBox.RightAddOnTemplateProperty.AddOwner<Select>();
    
    public static readonly StyledProperty<object?> ContentLeftAddOnProperty =
        AddOnDecoratedBox.ContentLeftAddOnProperty.AddOwner<Select>();
    
    public static readonly StyledProperty<IDataTemplate?> ContentLeftAddOnTemplateProperty =
        AddOnDecoratedBox.ContentLeftAddOnTemplateProperty.AddOwner<Select>();

    public static readonly StyledProperty<object?> ContentRightAddOnProperty =
        AddOnDecoratedBox.ContentRightAddOnProperty.AddOwner<Select>();
    
    public static readonly StyledProperty<IDataTemplate?> ContentRightAddOnTemplateProperty =
        AddOnDecoratedBox.ContentRightAddOnTemplateProperty.AddOwner<Select>();
    
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<Select>();

    public static readonly StyledProperty<AddOnDecoratedVariant> StyleVariantProperty =
        AddOnDecoratedBox.StyleVariantProperty.AddOwner<Select>();

    public static readonly StyledProperty<AddOnDecoratedStatus> StatusProperty =
        AddOnDecoratedBox.StatusProperty.AddOwner<Select>();
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Select>();
    
    public static readonly DirectProperty<Select, IList<ISelectOption>?> SelectedOptionsProperty =
        AvaloniaProperty.RegisterDirect<Select, IList<ISelectOption>?>(
            nameof(SelectedOptions),
            o => o.SelectedOptions,
            (o, v) => o.SelectedOptions = v);
    
    public static readonly StyledProperty<double> OptionFontSizeProperty =
        AvaloniaProperty.Register<Select, double>(nameof(OptionFontSize));
    
    public static readonly StyledProperty<bool> AutoScrollToSelectedOptionsProperty =
        AvaloniaProperty.Register<Select, bool>(
            nameof(AutoScrollToSelectedOptions),
            defaultValue: false);
    
    public static readonly StyledProperty<string> OptionFilterPropProperty =
        AvaloniaProperty.Register<Select, string>(
            nameof(OptionFilterProp), "Value");
    
    public IEnumerable<ISelectOption>? OptionsSource
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
    
    public bool IsAllowClear
    {
        get => GetValue(IsAllowClearProperty);
        set => SetValue(IsAllowClearProperty, value);
    }
    
    public bool IsAutoClearSearchValue
    {
        get => GetValue(IsAutoClearSearchValueProperty);
        set => SetValue(IsAutoClearSearchValueProperty, value);
    }
    
    public bool IsDefaultActiveFirstOption
    {
        get => GetValue(IsDefaultActiveFirstOptionProperty);
        set => SetValue(IsDefaultActiveFirstOptionProperty, value);
    }
    
    public bool IsDefaultOpen
    {
        get => GetValue(IsDefaultOpenProperty);
        set => SetValue(IsDefaultOpenProperty, value);
    }
    
    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }
    
    public bool IsGroupEnabled
    {
        get => GetValue(IsGroupEnabledProperty);
        set => SetValue(IsGroupEnabledProperty, value);
    }
    
    public string GroupPropertyPath
    {
        get => GetValue(GroupPropertyPathProperty);
        set => SetValue(GroupPropertyPathProperty, value);
    }
    
    public string? PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }
    
    public IBrush? PlaceholderForeground
    {
        get => GetValue(PlaceholderForegroundProperty);
        set => SetValue(PlaceholderForegroundProperty, value);
    }

    public bool IsPopupMatchSelectWidth
    {
        get => GetValue(IsPopupMatchSelectWidthProperty);
        set => SetValue(IsPopupMatchSelectWidthProperty, value);
    }
    
    public bool IsSearchEnabled
    {
        get => GetValue(IsSearchEnabledProperty);
        set => SetValue(IsSearchEnabledProperty, value);
    }
    
    public bool IsHideSelectedOptions
    {
        get => GetValue(IsHideSelectedOptionsProperty);
        set => SetValue(IsHideSelectedOptionsProperty, value);
    }
    
    public int DisplayPageSize
    {
        get => GetValue(DisplayPageSizeProperty);
        set => SetValue(DisplayPageSizeProperty, value);
    }

    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public int MaxCount
    {
        get => GetValue(MaxCountProperty);
        set => SetValue(MaxCountProperty, value);
    }
    
    public bool IsShowMaxCountIndicator
    {
        get => GetValue(IsShowMaxCountIndicatorProperty);
        set => SetValue(IsShowMaxCountIndicatorProperty, value);
    }
    
    public int? MaxTagCount
    {
        get => GetValue(MaxTagCountProperty);
        set => SetValue(MaxTagCountProperty, value);
    }
    
    public bool? IsResponsiveMaxTagCount
    {
        get => GetValue(IsResponsiveMaxTagCountProperty);
        set => SetValue(IsResponsiveMaxTagCountProperty, value);
    }
    
    public string? MaxTagPlaceholder
    {
        get => GetValue(MaxTagPlaceholderProperty);
        set => SetValue(MaxTagPlaceholderProperty, value);
    }
    
    public SelectMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }
    
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
    
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
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
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public string OptionFilterProp
    {
        get => GetValue(OptionFilterPropProperty);
        set => SetValue(OptionFilterPropProperty, value);
    }
    
    [Content]
    public AvaloniaList<ISelectOption> Options { get; set; } = new();
    
    private IList<ISelectOption>? _selectedOptions;

    public IList<ISelectOption>? SelectedOptions
    {
        get => _selectedOptions;
        set => SetAndRaise(SelectedOptionsProperty, ref _selectedOptions, value);
    }

    public double OptionFontSize
    {
        get => GetValue(OptionFontSizeProperty);
        set => SetValue(OptionFontSizeProperty, value);
    }
    
    public bool AutoScrollToSelectedOptions
    {
        get => GetValue(AutoScrollToSelectedOptionsProperty);
        set => SetValue(AutoScrollToSelectedOptionsProperty, value);
    }
    #endregion
    
    public IComparer<ISelectOption>? FilterSortFn { get; set; }
    public Func<object, object, bool>? FilterFn { get; set; }
    
    public Func<object, ISelectOption, bool>? DefaultValueCompareFn { get; set; }
    public IList<object>? DefaultValues { get; set; }
    
    #region 公共事件定义
    
    public event EventHandler? DropDownClosed;
    public event EventHandler? DropDownOpened;

    #endregion

    #region 内部属性定义
    
    internal static readonly StyledProperty<double> ItemHeightProperty =
        AvaloniaProperty.Register<Select, double>(nameof(ItemHeight));
    
    internal static readonly StyledProperty<double> MaxPopupHeightProperty =
        AvaloniaProperty.Register<Select, double>(nameof(MaxPopupHeight));
    
    internal static readonly StyledProperty<Thickness> PopupContentPaddingProperty =
        AvaloniaProperty.Register<Select, Thickness>(nameof(PopupContentPadding));
    
    internal static readonly DirectProperty<Select, bool> IsEffectiveShowClearButtonProperty =
        AvaloniaProperty.RegisterDirect<Select, bool>(nameof(IsEffectiveShowClearButton),
            o => o.IsEffectiveShowClearButton,
            (o, v) => o.IsEffectiveShowClearButton = v);
    
    internal static readonly DirectProperty<Select, double> EffectivePopupWidthProperty =
        AvaloniaProperty.RegisterDirect<Select, double>(
            nameof(EffectivePopupWidth),
            o => o.EffectivePopupWidth,
            (o, v) => o.EffectivePopupWidth = v);
    
    internal static readonly DirectProperty<Select, bool> IsPlaceholderTextVisibleProperty =
        AvaloniaProperty.RegisterDirect<Select, bool>(
            nameof(IsPlaceholderTextVisible),
            o => o.IsPlaceholderTextVisible,
            (o, v) => o.IsPlaceholderTextVisible = v);
    
    internal static readonly DirectProperty<Select, bool> IsSelectionEmptyProperty =
        AvaloniaProperty.RegisterDirect<Select, bool>(
            nameof(IsSelectionEmpty),
            o => o.IsSelectionEmpty,
            (o, v) => o.IsSelectionEmpty = v);
    
    internal static readonly DirectProperty<Select, ISelectOption?> SelectedOptionProperty =
        AvaloniaProperty.RegisterDirect<Select, ISelectOption?>(
            nameof(SelectedOption),
            o => o.SelectedOption,
            (o, v) => o.SelectedOption = v);
    
    internal static readonly DirectProperty<Select, int> SelectedCountProperty =
        AvaloniaProperty.RegisterDirect<Select, int>(nameof(SelectedCount),
            o => o.SelectedCount,
            (o, v) => o.SelectedCount = v);
    
    internal static readonly DirectProperty<Select, string?> ActivateFilterValueProperty =
        AvaloniaProperty.RegisterDirect<Select, string?>(nameof(ActivateFilterValue),
            o => o.ActivateFilterValue,
            (o, v) => o.ActivateFilterValue = v);
    
    internal static readonly DirectProperty<Select, bool> IsEffectiveSearchEnabledProperty =
        AvaloniaProperty.RegisterDirect<Select, bool>(nameof(IsEffectiveSearchEnabled),
            o => o.IsEffectiveSearchEnabled,
            (o, v) => o.IsEffectiveSearchEnabled = v);
    
    internal double ItemHeight
    {
        get => GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }
    
    internal double MaxPopupHeight
    {
        get => GetValue(MaxPopupHeightProperty);
        set => SetValue(MaxPopupHeightProperty, value);
    }
    
    internal Thickness PopupContentPadding
    {
        get => GetValue(PopupContentPaddingProperty);
        set => SetValue(PopupContentPaddingProperty, value);
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
    
    private bool _isSelectionEmpty = true;

    internal bool IsSelectionEmpty
    {
        get => _isSelectionEmpty;
        set => SetAndRaise(IsSelectionEmptyProperty, ref _isSelectionEmpty, value);
    }
    
    private ISelectOption? _selectedOption;

    internal ISelectOption? SelectedOption
    {
        get => _selectedOption;
        set => SetAndRaise(SelectedOptionProperty, ref _selectedOption, value);
    }
    
    private int _selectedCount;

    internal int SelectedCount
    {
        get => _selectedCount;
        set => SetAndRaise(SelectedCountProperty, ref _selectedCount, value);
    }
    
    private string? _activateFilterValue;

    internal string? ActivateFilterValue
    {
        get => _activateFilterValue;
        set => SetAndRaise(ActivateFilterValueProperty, ref _activateFilterValue, value);
    }
    
    private bool _isEffectiveSearchEnabled;

    internal bool IsEffectiveSearchEnabled
    {
        get => _isEffectiveSearchEnabled;
        set => SetAndRaise(IsEffectiveSearchEnabledProperty, ref _isEffectiveSearchEnabled, value);
    }
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => SelectToken.ID;

    #endregion
    
    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() => new VirtualizingStackPanel());
    
    private Popup? _popup;
    private SelectOptions? _optionsBox;
    private SelectSearchTextBox? _singleSearchInput;
    private readonly CompositeDisposable _subscriptionsOnOpen = new ();
    private ListFilterDescription? _filterDescription;
    private ListFilterDescription? _filterSelectedDescription;
    private bool _clickInTagCloseButton;
    private ISelectOption? _addNewOption;

    static Select()
    {
        FocusableProperty.OverrideDefaultValue<Select>(true);
        SelectHandle.ClearRequestedEvent.AddClassHandler<Select>((target, args) =>
        {
            target.HandleClearRequest();
        });
        OptionsSourceProperty.Changed.AddClassHandler<Select>((x, e) => x.HandleOptionsSourcePropertyChanged(e));
        SelectSearchTextBox.TextChangedEvent.AddClassHandler<Select>((x, e) => x.HandleSearchInputTextChanged(e));
        SelectSearchTextBox.KeyDownEvent.AddClassHandler<Select>(
            (x, e) => x.HandleSearchInputKeyDown(e),
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        SelectResultOptionsBox.KeyDownEvent.AddClassHandler<Select>(
            (x, e) => x.HandleSearchInputKeyDown(e),
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        SelectTag.ClosedEvent.AddClassHandler<Select>((x, e) => x.HandleTagCloseRequest(e));
    }
    
    public Select()
    {
        this.RegisterResources();
    }

    private void HandleClearRequest()
    {
        Clear();
    }
    
    private bool OptionEqualByValue(object value, ISelectOption selectOption)
    {
        if (DefaultValueCompareFn != null)
        {
            return DefaultValueCompareFn(value, selectOption);
        }
        var strValue = value.ToString();
        var optValue = selectOption.Value?.ToString();
        return strValue == optValue;
    }

    private bool TryHandleDeleteKey(KeyEventArgs e)
    {
        if (Mode == SelectMode.Single || SelectedOptions == null || SelectedOptions.Count == 0)
        {
            return false;
        }

        if (e.Key != Key.Back && e.Key != Key.Delete)
        {
            return false;
        }

        if (e.Source is TextBox textBox && string.IsNullOrWhiteSpace(textBox.Text) == false)
        {
            return false;
        }

        var newSelection = new List<object>();
        foreach (var selectedItem in SelectedOptions)
        {
            newSelection.Add(selectedItem);
        }

        if (newSelection.Count == 0)
        {
            return false;
        }

        var lastIndex   = newSelection.Count - 1;
        var removedItem = newSelection[lastIndex];
        newSelection.RemoveAt(lastIndex);
        SetCurrentValue(SelectedOptionsProperty, newSelection);
        SyncSelection();

        if (Mode == SelectMode.Tags && removedItem is ISelectOption option && option.IsDynamicAdded)
        {
            Options.Remove(option);
            if (ReferenceEquals(_addNewOption, option))
            {
                _addNewOption = null;
            }
        }

        e.Handled = true;
        return true;
    }
    
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Handled)
        {
            return;
        }

        if (TryHandleDeleteKey(e))
        {
            return;
        }

        if ((e.Key == Key.F4 && e.KeyModifiers.HasAllFlags(KeyModifiers.Alt) == false) ||
            ((e.Key == Key.Down || e.Key == Key.Up) && e.KeyModifiers.HasAllFlags(KeyModifiers.Alt)))
        {
            SetCurrentValue(IsDropDownOpenProperty, !IsDropDownOpen);
            e.Handled = true;
        }
        else if (!IsDropDownOpen && (e.Key == Key.Down || e.Key == Key.Up))
        {
            SetCurrentValue(IsDropDownOpenProperty, true);
            e.Handled = true;
        }
        else if (IsDropDownOpen && e.Key == Key.Escape)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
            e.Handled = true;
        }
        else if (!IsDropDownOpen && (e.Key == Key.Enter || e.Key == Key.Space))
        {
            SetCurrentValue(IsDropDownOpenProperty, true);
            e.Handled = true;
        }
        else if (IsDropDownOpen)
        {
            if (e.Key == Key.Down)
            {
                _optionsBox?.MoveActiveBy(1);
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                _optionsBox?.MoveActiveBy(-1);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                _optionsBox?.CommitActiveSelection();
                if (Mode == SelectMode.Single)
                {
                    SetCurrentValue(IsDropDownOpenProperty, false);
                }
                e.Handled = true;
            }
        }
    }

    private void HandleSearchInputKeyDown(KeyEventArgs e)
    {
        if (TryHandleDeleteKey(e))
        {
            return;
        }

        if (e.Handled)
        {
            return;
        }

        if (!IsDropDownOpen)
        {
            if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Enter)
            {
                SetCurrentValue(IsDropDownOpenProperty, true);
                e.Handled = true;
            }
            return;
        }

        if (e.Key == Key.Down)
        {
            _optionsBox?.MoveActiveBy(1);
            e.Handled = true;
        }
        else if (e.Key == Key.Up)
        {
            _optionsBox?.MoveActiveBy(-1);
            e.Handled = true;
        }
        else if (e.Key == Key.Enter)
        {
            _optionsBox?.CommitActiveSelection();
            if (Mode == SelectMode.Single)
            {
                SetCurrentValue(IsDropDownOpenProperty, false);
            }
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
            e.Handled = true;
        }
        else if (e.Key == Key.Tab)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
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
                var parent = sourceControl.FindAncestorOfType<IconButton>();
                var tag    = parent?.FindAncestorOfType<SelectTag>();
                if (tag != null)
                {
                    _clickInTagCloseButton = true;
                }
            }
            else if (!IsHideSelectedOptions)
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
                if (Mode == SelectMode.Single)
                {
                    SetCurrentValue(IsDropDownOpenProperty, false);
                }
                e.Handled = true;
            }
            else if (PseudoClasses.Contains(StdPseudoClass.Pressed))
            {
                var clickInTagCloseButton = false;
                if (e.Source is Control sourceControl)
                {
                    var parent = sourceControl.FindAncestorOfType<IconButton>();
                    var tag    = parent?.FindAncestorOfType<SelectTag>();
                    if (tag != null)
                    {
                        clickInTagCloseButton = true;
                    }
                }

                if (!clickInTagCloseButton)
                {
                    SetCurrentValue(IsDropDownOpenProperty, !IsDropDownOpen);
                }
    
                e.Handled = true;
            }
        }

        PseudoClasses.Set(StdPseudoClass.Pressed, false);
        base.OnPointerReleased(e);
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_popup != null)
        {
            _popup.Opened -= PopupOpened;
            _popup.Closed -= PopupClosed;
        }
        _optionsBox        = e.NameScope.Get<SelectOptions>(SelectThemeConstants.OptionsBoxPart);
        _singleSearchInput = e.NameScope.Get<SelectSearchTextBox>(SelectThemeConstants.SingleSearchInputPart);
        if (_optionsBox != null)
        {
            _optionsBox.Select = this;
            ConfigureOptionsBoxSelectionMode();
        }
        
        _popup                    =  e.NameScope.Get<Popup>(SelectThemeConstants.PopupPart);
        _popup.ClickHidePredicate =  PopupClosePredicate;
        _popup.Opened             += PopupOpened;
        _popup.Closed             += PopupClosed;
        ConfigureMaxDropdownHeight();
        ConfigurePlaceholderVisible();
        ConfigureSelectionIsEmpty();
        UpdatePseudoClasses();
        ConfigureSingleSearchTextBox();
        ConfigureDefaultValues();
        ConfigureEffectiveSearchEnabled();
    }
    
    private bool PopupClosePredicate(IPopupHostProvider hostProvider, RawPointerEventArgs args)
    {
        var popupRoots = new HashSet<PopupRoot>();
        if (_popup?.Host is PopupRoot popupRoot)
        {
            popupRoots.Add(popupRoot);
        }

        if (_clickInTagCloseButton)
        {
            _clickInTagCloseButton = false;
            return false;
        }
        return !popupRoots.Contains(args.Root);
    }

    internal void NotifyLogicalSelectOption(ISelectOption selectOption)
    {
        Debug.Assert(_optionsBox != null);
        var selectedOptions = new List<ISelectOption>();
        if (Mode == SelectMode.Single)
        {
            if (_singleSearchInput != null)
            {
                _singleSearchInput.Width = double.NaN;
            }
        
            selectedOptions.Add(selectOption);
        }
        else
        {
            if (Mode == SelectMode.Tags)
            {
                _addNewOption = null;
            }
            if (SelectedOptions != null)
            {
                foreach (var item in SelectedOptions)
                {
                    selectedOptions.Add(item);
                }
            }
            
            if (!selectedOptions.Contains(selectOption))
            {
                selectedOptions.Add(selectOption);
            }
            else if (selectedOptions.Contains(selectOption))
            {
                selectedOptions.Remove(selectOption);
            }
        }
        SetCurrentValue(SelectedOptionsProperty, selectedOptions);
        SyncSelection();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsDropDownOpenProperty)
        {
            PseudoClasses.Set(SelectPseudoClass.DropdownOpen, change.GetNewValue<bool>());
            ConfigureSingleSearchTextBox();
        }
        else if (change.Property == DisplayPageSizeProperty ||
                 change.Property == ItemHeightProperty)
        {
            ConfigureMaxDropdownHeight();
        }
        else if (change.Property == IsPopupMatchSelectWidthProperty)
        {
            ConfigurePopupMinWith(DesiredSize.Width);
        }
        else if (change.Property == StyleVariantProperty ||
                 change.Property == StatusProperty)
        {
            UpdatePseudoClasses();
        }
        if (change.Property == SelectedOptionsProperty)
        {
            ConfigureSingleSelectedOption();
            ConfigureSelectionIsEmpty();
            ConfigurePlaceholderVisible();
            ConfigureSelectedFilterDescription();
            SetCurrentValue(SelectedCountProperty, SelectedOptions?.Count ?? 0);
            CleanDynamicAddedOptions();
        }
        else if (change.Property == OptionFilterPropProperty)
        {
            HandleOptionFilterPropChanged();
        }
        else if (change.Property == ModeProperty)
        {
            ConfigureSingleSelectedOption();
            ConfigureOptionsBoxSelectionMode();
        }
        else if (change.Property == IsHideSelectedOptionsProperty)
        {
            if (!IsHideSelectedOptions)
            {
                _filterSelectedDescription = null;
            }
        }
        
        if (change.Property == IsSearchEnabledProperty ||
            change.Property == ModeProperty)
        {
            ConfigureEffectiveSearchEnabled();
        }
    }
    
    private void PopupClosed(object? sender, EventArgs e)
    {
        _subscriptionsOnOpen.Clear();
        DropDownClosed?.Invoke(this, EventArgs.Empty);
        if (Mode == SelectMode.Single)
        {
            if (_singleSearchInput != null)
            {
                _singleSearchInput.Clear();
                _singleSearchInput.Width = double.NaN;
            }
        }
    }

    private void PopupOpened(object? sender, EventArgs e)
    {
        _subscriptionsOnOpen.Clear();
        this.GetObservable(IsVisibleProperty).Subscribe(IsVisibleChanged).DisposeWith(_subscriptionsOnOpen);
        foreach (var parent in this.GetVisualAncestors().OfType<Control>())
        {
            parent.GetObservable(IsVisibleProperty).Subscribe(IsVisibleChanged).DisposeWith(_subscriptionsOnOpen);
        }

        DropDownOpened?.Invoke(this, EventArgs.Empty);
        if (Mode == SelectMode.Single)
        {
            _singleSearchInput?.Focus();
        }

        SyncSelection();
    }

    private void SyncSelection()
    {
        if (_optionsBox != null)
        {
            var selectedItems = new List<object>();
            if (Mode == SelectMode.Single)
            {
                if (SelectedOptions != null)
                {
                    foreach (var option in SelectedOptions)
                    {
                        selectedItems.Add(option);
                        break;
                    }
                }
            }
            else
            {
                if (SelectedOptions != null)
                {
                    foreach (var option in SelectedOptions)
                    {
                        selectedItems.Add(option);
                    }
                }
            }
            _optionsBox.SetCurrentValue(SelectOptions.SelectedItemsProperty, selectedItems);
            _optionsBox.EnsureActiveOption();
        }
    }

    private void ConfigureOptionsBoxSelectionMode()
    {
        if (_optionsBox == null)
        {
            return;
        }

        _optionsBox.SetCurrentValue(List.SelectionModeProperty,
            Mode == SelectMode.Single ? SelectionMode.Single : SelectionMode.Multiple);
    }
    
    private void IsVisibleChanged(bool isVisible)
    {
        if (!isVisible && IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
        }
    }
    
    public void Clear()
    {
        SelectedOptions = null;
    }
    
    private void ConfigureMaxDropdownHeight()
    {
        SetCurrentValue(MaxPopupHeightProperty, ItemHeight * DisplayPageSize + PopupContentPadding.Top + PopupContentPadding.Bottom);
    }
    
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        ConfigurePopupMinWith(e.NewSize.Width);
    }

    private void ConfigurePopupMinWith(double selectWidth)
    {
        if (IsPopupMatchSelectWidth)
        {
            SetCurrentValue(EffectivePopupWidthProperty, selectWidth);
        }
        else
        {
            SetCurrentValue(EffectivePopupWidthProperty, double.NaN);
        }
    }
    
    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(StdPseudoClass.Error, Status == AddOnDecoratedStatus.Error);
        PseudoClasses.Set(StdPseudoClass.Warning, Status == AddOnDecoratedStatus.Warning);
        PseudoClasses.Set(AddOnDecoratedBoxPseudoClass.Outline, StyleVariant == AddOnDecoratedVariant.Outline);
        PseudoClasses.Set(AddOnDecoratedBoxPseudoClass.Filled, StyleVariant == AddOnDecoratedVariant.Filled);
        PseudoClasses.Set(AddOnDecoratedBoxPseudoClass.Borderless, StyleVariant == AddOnDecoratedVariant.Borderless);
    }

    private void ConfigurePlaceholderVisible()
    {
        SetCurrentValue(IsPlaceholderTextVisibleProperty, (SelectedOptions == null || SelectedOptions?.Count == 0) && string.IsNullOrEmpty(ActivateFilterValue));
    }

    private void ConfigureSelectionIsEmpty()
    {
        SetCurrentValue(IsSelectionEmptyProperty, SelectedOptions == null || SelectedOptions?.Count == 0);
    }

    private void ConfigureSingleSearchTextBox()
    {
        if (_singleSearchInput != null)
        {
            if (IsDropDownOpen)
            {
                _singleSearchInput.Width = _singleSearchInput.Bounds.Width;
            }
        }
    }

    private void HandleSearchInputTextChanged(TextChangedEventArgs e)
    {
        if (_optionsBox != null && _optionsBox.FilterDescriptions != null)
        {
            if (e.Source is TextBox textBox)
            {
                ActivateFilterValue = textBox.Text?.Trim();
            }

            if (_addNewOption != null)
            {
                var isSelected = SelectedOptions?.Contains(_addNewOption) == true;
                var isCurrentInput = ActivateFilterValue == _addNewOption.Header?.ToString();
                if (!isSelected && !isCurrentInput)
                {
                    Options.Remove(_addNewOption);
                    _addNewOption = null;
                }
            }
            ConfigurePlaceholderVisible();
            if (string.IsNullOrEmpty(ActivateFilterValue))
            {
                _optionsBox.FilterDescriptions.Clear();
                _filterDescription = null;
            }
            else
            {
                if (_filterDescription != null)
                {
                    var oldFilter = _filterDescription;
                    Debug.Assert(oldFilter.FilterConditions.Count == 1);
                    var oldFilterValue = oldFilter.FilterConditions.First().ToString();
                    if (oldFilterValue != ActivateFilterValue)
                    {
                        _filterDescription = new ListFilterDescription()
                        {
                            PropertyPath     = _filterDescription.PropertyPath,
                            Filter           =  _filterDescription.Filter,
                            FilterConditions = [ActivateFilterValue]
                        };
                        _optionsBox.FilterDescriptions.Remove(oldFilter);
                        _optionsBox.FilterDescriptions.Add(_filterDescription);
                    }
                }
                else
                {
                    _filterDescription = new ListFilterDescription()
                    {
                        PropertyPath     = OptionFilterProp,
                        Filter           = FilterFn,
                        FilterConditions = [ActivateFilterValue],
                    };
                    _optionsBox.FilterDescriptions.Add(_filterDescription);
                }
            }

            // Only allow "create from search text" in Tags mode.
            // For Single/Multiple, when data is empty (or filter results are empty),
            // the dropdown should show the empty indicator instead of adding a temporary option.
            if (Mode == SelectMode.Tags &&
                _optionsBox.CollectionView?.Count == 0 &&
                !string.IsNullOrWhiteSpace(ActivateFilterValue))
            {
                _addNewOption = new SelectOption()
                {
                    Header = ActivateFilterValue,
                    Value  = ActivateFilterValue,
                    IsDynamicAdded = true
                };
                Options.Add(_addNewOption);
            }
            SyncSelection();
        }
        e.Handled = true;
    }

    private void HandleTagCloseRequest(RoutedEventArgs e)
    {
        if (Mode == SelectMode.Single)
        {
            return;
        }
        if (e.Source is SelectTag tag && tag.Option != null)
        {
            if (SelectedOptions != null)
            {
                var selectedOptions = new List<ISelectOption>();
                foreach (var selectedItem in SelectedOptions)
                {
                    selectedOptions.Add(selectedItem);
                }
                if (selectedOptions.Contains(tag.Option))
                {
                    selectedOptions.Remove(tag.Option);
                }
                SelectedOptions = selectedOptions;
                SyncSelection();
            }

            if (Mode == SelectMode.Tags)
            {
                if (tag.Option != null && tag.Option.IsDynamicAdded)
                {
                    Options.Remove(tag.Option);
                }
            }
        }
        e.Handled = true;
    }

    private void HandleOptionFilterPropChanged()
    {
        if (_filterDescription != null && _optionsBox!= null && _optionsBox.FilterDescriptions != null)
        {
            var oldFilter = _filterDescription;
            _filterDescription = new ListFilterDescription()
            {
                PropertyPath     = OptionFilterProp,
                Filter           =  oldFilter.Filter,
                FilterConditions = oldFilter.FilterConditions
            };
            _optionsBox.FilterDescriptions.Remove(oldFilter);
            _optionsBox.FilterDescriptions.Add(_filterDescription);
        }
    }
    
    private void HandleOptionsSourcePropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var newItemsSource = (IEnumerable<ISelectOption>?)change.NewValue;
        if (newItemsSource != null)
        {
            Options.Clear();
            Options.AddRange(newItemsSource);
            ConfigureDefaultValues();
            SyncSelection();
        }
    }

    private void ConfigureDefaultValues()
    {
        if (SelectedOptions == null || SelectedOptions.Count == 0)
        {
            if (Mode == SelectMode.Single)
            {
                if (DefaultValues?.Count > 0)
                {
                    var defaultValue = DefaultValues.First();
                    foreach (var option in Options)
                    {
                        if (OptionEqualByValue(defaultValue, option))
                        {
                            SetCurrentValue(SelectedOptionsProperty, new List<ISelectOption>()
                            {
                                option
                            });
                            break;
                        }
                    }
                }
            }
            else if (Mode == SelectMode.Multiple)
            {
                if (DefaultValues?.Count > 0)
                {
                    var selectedOptions = new List<ISelectOption>();
                    foreach (var defaultValue in DefaultValues)
                    {
                        foreach (var option in Options)
                        {
                            if (OptionEqualByValue(defaultValue, option))
                            {
                                selectedOptions.Add(option);
                            }
                        }
                    }
                    SetCurrentValue(SelectedOptionsProperty, selectedOptions);
                }
            }
        }
    }

    private void ConfigureSingleSelectedOption()
    {
        if (Mode == SelectMode.Single)
        {
            if (SelectedOptions?.Count > 0)
            {
                SetCurrentValue(SelectedOptionProperty, SelectedOptions[0]);
            }
            else
            {
                SetCurrentValue(SelectedOptionProperty, null);
            }
        }
        else
        {
            SetCurrentValue(SelectedOptionProperty, null);
        }
    }

    private void ConfigureSelectedFilterDescription()
    {
        if (_optionsBox?.FilterDescriptions != null)
        {
            if (IsHideSelectedOptions)
            {
                var selectedOptions = new HashSet<ISelectOption>();
                if (SelectedOptions?.Count > 0)
                {
                    foreach (var selectedOption in SelectedOptions)
                    {
                        selectedOptions.Add(selectedOption);
                    }
                }
                var oldFilter = _filterSelectedDescription;
                _filterSelectedDescription = new ListFilterDescription()
                {
                    Filter           = SelectFilterFn,
                    FilterConditions = [selectedOptions],
                };
                if (oldFilter != null)
                {
                    _optionsBox.FilterDescriptions.Remove(oldFilter);
                }
                _optionsBox.FilterDescriptions.Add(_filterSelectedDescription);
            }
            else
            {
                if (_filterSelectedDescription != null)
                {
                    _optionsBox.FilterDescriptions.Remove(_filterSelectedDescription);
                }
                _filterSelectedDescription = null;
            }
        }
    }

    private static bool SelectFilterFn(object value, object filterValue)
    {
        if (filterValue is HashSet<ISelectOption> set)
        {
            return !set.Contains(value);
        }
        return true;
    }

    private void ConfigureEffectiveSearchEnabled()
    {
        if (Mode == SelectMode.Tags)
        {
            SetCurrentValue(
                IsEffectiveSearchEnabledProperty, true);
        }
        else
        {
            SetCurrentValue(
                IsEffectiveSearchEnabledProperty,
                IsSearchEnabled);
        }
    }

    private void CleanDynamicAddedOptions()
    {
        if (Mode != SelectMode.Tags)
        {
            return;
        }
        
        var selected = new HashSet<ISelectOption>();
        if (SelectedOptions != null)
        {
            foreach (var opt in SelectedOptions)
            {
                selected.Add(opt);
            }
        }

        var toRemove = new List<ISelectOption>();
        foreach (var option in Options)
        {
            if (option.IsDynamicAdded && !selected.Contains(option))
            {
                toRemove.Add(option);
            }
        }

        foreach (var option in toRemove)
        {
            Options.Remove(option);
            if (ReferenceEquals(_addNewOption, option))
            {
                _addNewOption = null;
            }
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

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
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
}
