using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using AtomUI.Controls;
using AtomUI.Controls.Utils;
using AtomUI.Desktop.Controls.DataLoad;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Icons.AntDesign;
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
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public delegate object? MentionsFilterValueSelector(IMentionOption option);

[PseudoClasses(MentionPseudoClass.CandidatePopupOpen)]
public class Mentions : TemplatedControl, IControlSharedTokenResourcesHost, IMotionAwareControl
{
    #region 公共属性定义
    public static readonly StyledProperty<PathIcon?> ClearIconProperty =
        AvaloniaProperty.Register<Mentions, PathIcon?>(nameof(ClearIcon));
    
    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<Mentions, string?>(nameof(Value));
    
    public static readonly StyledProperty<bool> IsAllowClearProperty =
        AvaloniaProperty.Register<Mentions, bool>(nameof(IsAllowClear));
    
    public static readonly StyledProperty<bool> IsAutoFocusProperty =
        AvaloniaProperty.Register<Mentions, bool>(nameof(IsAutoFocus));
    
    public static readonly StyledProperty<bool> IsAutoSizeProperty =
        AvaloniaProperty.Register<Mentions, bool>(nameof(IsAutoSize));

    public static readonly StyledProperty<string?> DefaultValueProperty =
        AvaloniaProperty.Register<Mentions, string?>(nameof(DefaultValue));
    
    public static readonly StyledProperty<IMentionOptionFilter?> OptionFilterProperty =
        AvaloniaProperty.Register<Mentions, IMentionOptionFilter?>(nameof(OptionFilter));
    
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
    
    public static readonly StyledProperty<object?> ContentLeftAddOnProperty =
        AddOnDecoratedBox.ContentLeftAddOnProperty.AddOwner<Mentions>();
    
    public static readonly StyledProperty<IDataTemplate?> ContentLeftAddOnTemplateProperty =
        AddOnDecoratedBox.ContentLeftAddOnTemplateProperty.AddOwner<Mentions>();

    public static readonly StyledProperty<object?> ContentRightAddOnProperty =
        AddOnDecoratedBox.ContentRightAddOnProperty.AddOwner<Mentions>();
    
    public static readonly StyledProperty<IDataTemplate?> ContentRightAddOnTemplateProperty =
        AddOnDecoratedBox.ContentRightAddOnTemplateProperty.AddOwner<Mentions>();
    
    public static readonly StyledProperty<AddOnDecoratedVariant> StyleVariantProperty =
        AddOnDecoratedBox.StyleVariantProperty.AddOwner<Mentions>();

    public static readonly StyledProperty<AddOnDecoratedStatus> StatusProperty =
        AddOnDecoratedBox.StatusProperty.AddOwner<Mentions>();
    
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
    
    public static readonly StyledProperty<MentionsFilterValueSelector?> FilterValueSelectorProperty =
        AvaloniaProperty.Register<Mentions, MentionsFilterValueSelector?>(
            nameof(FilterValueSelector));
    
    public static readonly StyledProperty<TimeSpan> MinimumPopulateDelayProperty =
        AvaloniaProperty.Register<Mentions, TimeSpan>(
            nameof(MinimumPopulateDelay),
            TimeSpan.Zero,
            validate: IsValidMinimumPopulateDelay);
    
    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<Mentions, bool>(
            nameof(IsDropDownOpen));
    
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
    
    public IMentionOptionFilter? OptionFilter
    {
        get => GetValue(OptionFilterProperty);
        set => SetValue(OptionFilterProperty, value);
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
    
    public MentionsFilterValueSelector? FilterValueSelector
    {
        get => GetValue(FilterValueSelectorProperty);
        set => SetValue(FilterValueSelectorProperty, value);
    }
    
    public TimeSpan MinimumPopulateDelay
    {
        get => GetValue(MinimumPopulateDelayProperty);
        set => SetValue(MinimumPopulateDelayProperty, value);
    }
    
    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
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

    internal static readonly DirectProperty<Mentions, string?> OptionFilterValueProperty =
        AvaloniaProperty.RegisterDirect<Mentions, string?>(
            nameof(OptionFilterValue),
            o => o.OptionFilterValue,
            (o, v) => o.OptionFilterValue = v);
    
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
    
    private string? _optionFilterValue;
    
    internal string? OptionFilterValue
    {
        get => _optionFilterValue;
        set => SetAndRaise(OptionFilterValueProperty, ref _optionFilterValue, value);
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
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => MentionsToken.ID;
    #endregion
    
    private static bool IsValidMinimumPopulateDelay(TimeSpan value) => value.TotalMilliseconds >= 0.0;

    private MentionTextArea? _textArea;
    private Popup? _popup;
    private bool _ignorePopupClose;
    private CompositeDisposable? _subscriptionsOnOpen;
    private ICandidateList? _candidateList;
    private DispatcherTimer? _delayTimer;
    private List<IMentionOption>? _items;
    private IList<IMentionOption>? _view;
    private bool _cancelRequested;
    private bool _filterInAction;
    private bool _popupHasOpened;
    private bool _ignorePropertyChange;
    private IDisposable? _collectionChangeSubscription;
    private CancellationTokenSource? _populationCancellationTokenSource;
    
    static Mentions()
    {
        FocusableProperty.OverrideDefaultValue<Mentions>(true);
        LinesProperty.OverrideDefaultValue<Mentions>(1);
        
        IsDropDownOpenProperty.Changed.AddClassHandler<Mentions>((x,e) => x.HandleIsDropDownOpenChanged(e));
        MinimumPopulateDelayProperty.Changed.AddClassHandler<Mentions>((x,e) => x.HandleMinimumPopulateDelayChanged(e));
        PlacementProperty.Changed.AddClassHandler<Mentions>((x,e) => x.ConfigurePopupPlacement());
        OptionsSourceProperty.Changed.AddClassHandler<Mentions>((x,e) => x.HandleItemsSourceChanged((IEnumerable?)e.NewValue));
        OptionFilterValueProperty.Changed.AddClassHandler<Mentions>((x,e) => x.HandleOptionFilterValueChanged());
    }
    
    public Mentions()
    {
        this.RegisterResources();
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
        
        if (!newValue)
        {
            ClosingDropDown(oldValue);
        }

        UpdatePseudoClasses();
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
        _items = newValue == null ? null : new List<IMentionOption>(newValue.Cast<IMentionOption>());
        
        // Clear and set the view on the selection adapter
        ClearView();
    }

    private void HandleOptionFilterValueChanged()
    {
        if (IsDropDownOpen)
        {
            RefreshView();
        }
    }
    
    private void ItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Update the cache
        if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
        {
            for (int index = 0; index < e.OldItems.Count; index++)
            {
                _items!.RemoveAt(e.OldStartingIndex);
            }
        }
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null && _items!.Count >= e.NewStartingIndex)
        {
            for (int index = 0; index < e.NewItems.Count; index++)
            {
                var newItem = e.NewItems[index] as IMentionOption;
                _items.Insert(e.NewStartingIndex + index, newItem!);
            }
        }
        if (e.Action == NotifyCollectionChangedAction.Replace && e.NewItems != null && e.OldItems != null)
        {
            for (int index = 0; index < e.NewItems.Count; index++)
            {
                var newItem = e.NewItems[index] as IMentionOption;
                _items![e.NewStartingIndex] = newItem!;
            }
        }

        // Update the view
        if ((e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace) && e.OldItems != null)
        {
            for (int index = 0; index < e.OldItems.Count; index++)
            {
                var oldItem = e.OldItems[index] as IMentionOption;
                _view!.Remove(oldItem!);
            }
        }

        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            // Significant changes to the underlying data.
            ClearView();
            if (OptionsSource != null)
            {
                _items = new List<IMentionOption>(OptionsSource);
            }
        }

        // Refresh the observable collection used in the selection adapter.
        RefreshView();
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DisplayCandidateCountProperty ||
            change.Property == ItemHeightProperty)
        {
            ConfigureMaxPopupHeight();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_textArea != null)
        {
            _textArea.CandidateOpenRequest  -= HandleCandidateOpenRequest;
            _textArea.CandidateCloseRequest -= HandleCandidateCloseRequest;
        }

        if (_popup != null)
        {
            _popup.Opened -= HandlePopupOpened;
            _popup.Closed -= HandlePopupClosed;
        }
        
        _textArea     = e.NameScope.Find<MentionTextArea>(MentionsThemeConstants.TextAreaPart);
        _popup        = e.NameScope.Find<Popup>(MentionsThemeConstants.PopupPart);
        CandidateList = e.NameScope.Find<ICandidateList>(MentionsThemeConstants.CandidateListPart);

        if (_textArea != null)
        {
            _textArea.CandidateOpenRequest  += HandleCandidateOpenRequest;
            _textArea.CandidateCloseRequest += HandleCandidateCloseRequest;
            _textArea.Owner                 =  this;
        }

        if (_popup != null)
        {
            _popup.ClickHidePredicate  =  PopupClosePredicate;
            _popup.IgnoreFirstDetected =  false;
            _popup.Opened              += HandlePopupOpened;
            _popup.Closed              += HandlePopupClosed;
        }
        
        ConfigurePopupPlacement();
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
        OptionFilterValue = null;
        SetCurrentValue(IsDropDownOpenProperty, false);
        _textArea!.Focus();
    }
    
    private void InsertCandidateOption(IMentionOption option)
    {
        Debug.Assert(_textArea != null);
        var value = option.Value?.ToString() ?? option.Header?.ToString() ?? string.Empty;
        _textArea?.InsertMentionOption(value, Split);
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

    private void HandleCandidateOpenRequest(object? sender, ShowMentionCandidateRequestEventArgs eventArgs)
    {
        CandidateTriggered?.Invoke(this, new MentionCandidateTriggeredEventArgs(eventArgs.TriggerChar));
        if (_popup != null && _textArea != null)
        {
            var textPresenterBounds = _textArea.GetTextPresenterBounds();
            var triggerBounds       = eventArgs.TriggerBounds;
            _popup.HorizontalOffset = triggerBounds.X + textPresenterBounds.X;
            if (Placement == MentionsPlacementMode.Bottom)
            {
                _popup.VerticalOffset = -(textPresenterBounds.Height - triggerBounds.Y - textPresenterBounds.Y) + triggerBounds.Height / 2;
            }
            else
            {
                _popup.VerticalOffset = textPresenterBounds.Y + triggerBounds.Y;
            }
        }

        _ignorePropertyChange = true;
        var oldIsDropDownOpen = IsDropDownOpen;
        SetCurrentValue(IsDropDownOpenProperty, true);
        OpeningDropDown(oldIsDropDownOpen);
        
        if (_delayTimer != null)
        {
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
        
        if (TryPopulateAsync(OptionFilterValue))
        {
            return;
        }
        
        // The Populated event enables advanced, custom filtering. The
        // client needs to directly update the ItemsSource collection or
        // call the Populate method on the control to continue the
        // display process if Cancel is set to true.
        var populating = new MentionsPopulatingEventArgs(OptionFilterValue);
        NotifyPopulating(populating);
        if (!populating.Cancel)
        {
            PopulateComplete();
        }
    }
    
    private bool TryPopulateAsync(string? searchText)
    {
        _populationCancellationTokenSource?.Cancel(false);
        _populationCancellationTokenSource?.Dispose();
        _populationCancellationTokenSource = null;
        
        if (OptionsAsyncLoader == null)
        {
            return false;
        }
        
        _populationCancellationTokenSource = new CancellationTokenSource();
        IsLoading                          = true;
        var task = PopulateAsync(searchText, _populationCancellationTokenSource.Token);
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

            var result = await OptionsAsyncLoader.LoadAsync(filterValue, cancellationToken);
            IsLoading = false;
            OptionsLoaded?.Invoke(this, new MentionOptionsLoadedEventArgs(filterValue, result));
            var resultList = result.Data;

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    if (result.IsSuccess)
                    {
                        SetCurrentValue(OptionsSourceProperty, resultList);
                        PopulateComplete();
                    }
                }
            });
        }
        catch (TaskCanceledException e)
        {
            IsLoading = false;
            OptionsLoaded?.Invoke(this, new MentionOptionsLoadedEventArgs(filterValue, new MentionOptionsLoadResult()
            {
                UserFriendlyMessage = e.Message,
                StatusCode           = RpcStatusCode.Cancelled
            }));
        }
        finally
        {
            _populationCancellationTokenSource?.Dispose();
            _populationCancellationTokenSource = null;
        }
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

    private void HandleCandidateCloseRequest(object? sender, EventArgs eventArgs)
    {
        OptionFilterValue = null;
        SetCurrentValue(IsDropDownOpenProperty, false);
        _textArea!.Focus();
    }

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

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (IsAutoFocus)
        {
            Dispatcher.UIThread.Post(() => _textArea?.Focus());
        }

        UpdatePseudoClasses();
    }

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
    
    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(MentionPseudoClass.CandidatePopupOpen, IsDropDownOpen);
    }
    
    private void HandlePopupClosed(object? sender, EventArgs e)
    {
        _subscriptionsOnOpen?.Dispose();
        _subscriptionsOnOpen = null;
        // Force the drop down dependency property to be false.
        if (IsDropDownOpen)
        {
            _ignorePropertyChange = true;
            SetCurrentValue(IsDropDownOpenProperty, false);
        }

        // Fire the DropDownClosed event
        if (_popupHasOpened)
        {
            NotifyDropDownClosed(EventArgs.Empty);
        }
        NotifyDropDownClosed(EventArgs.Empty);
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
        _textArea?.Focus();
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
                    _ignorePopupClose = true;
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
    
    private void ClearView()
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
                // Exit the fitter when requested if cancellation is requested
                if (_cancelRequested)
                {
                    return;
                }

                bool inResults = string.IsNullOrWhiteSpace(OptionFilterValue) || filter.Filter(GetValueByOption(item), OptionFilterValue);
              
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
            }
            if (_candidateList != null && _view?.Count > 0)
            {
                _candidateList.SelectedItem = _view.First();
            }
        }
        finally
        {
            // indicate that filtering is not ongoing anymore
            _filterInAction  = false;
            _cancelRequested = false;
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
    
    private void OpenDropDown()
    {
        if (_popup != null)
        {
            _popup.IsMotionAwareOpen = true;
        }
        _popupHasOpened = true;
        NotifyDropDownOpened(EventArgs.Empty);
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
}