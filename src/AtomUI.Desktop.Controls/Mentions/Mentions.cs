using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.Data;
using AtomUI.Desktop.Controls.DataLoad;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
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

[PseudoClasses(MentionPseudoClass.CandidatePopupOpen)]
public class Mentions : TemplatedControl, IControlSharedTokenResourcesHost, IMotionAwareControl
{
    #region 公共属性定义
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
    
    public static readonly DirectProperty<Mentions, IMentionOptionFilter?> OptionFilterProperty =
        AvaloniaProperty.RegisterDirect<Mentions, IMentionOptionFilter?>(
            nameof(OptionFilter),
            o => o.OptionFilter,
            (o, v) => o.OptionFilter = v);
    
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
    
    public static readonly DirectProperty<Mentions, IMentionOptionAsyncLoader?> OptionAsyncLoaderProperty =
        AvaloniaProperty.RegisterDirect<Mentions, IMentionOptionAsyncLoader?>(
            nameof(OptionAsyncLoader),
            o => o.OptionAsyncLoader,
            (o, v) => o.OptionAsyncLoader = v);
    
    public static readonly DirectProperty<Mentions, bool> IsLoadingProperty =
        AvaloniaProperty.RegisterDirect<Mentions, bool>(
            nameof(IsLoading),
            o => o.IsLoading);
    
    public static readonly StyledProperty<int> DisplayCandidateCountProperty = 
        AvaloniaProperty.Register<Mentions, int>(nameof (DisplayCandidateCount), 10);
    
    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        TextArea.IsReadOnlyProperty.AddOwner<Mentions>();
    
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
    
    private IMentionOptionFilter? _optionFilter;
    
    public IMentionOptionFilter? OptionFilter
    {
        get => _optionFilter;
        set => SetAndRaise(OptionFilterProperty, ref _optionFilter, value);
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
    
    private IMentionOptionAsyncLoader? _optionAsyncLoader;
    
    public IMentionOptionAsyncLoader? OptionAsyncLoader
    {
        get => _optionAsyncLoader;
        set => SetAndRaise(OptionAsyncLoaderProperty, ref _optionAsyncLoader, value);
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
    #endregion

    #region 公共事件定义

    public event EventHandler<MentionOptionLoadedEventArgs>? OptionsLoaded;
    public event EventHandler? CandidatePopupClosed;
    public event EventHandler? CandidatePopupOpened;
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
    
    internal static readonly DirectProperty<Mentions, bool> IsPopupOpenProperty =
        AvaloniaProperty.RegisterDirect<Mentions, bool>(nameof(IsPopupOpen),
            o => o.IsPopupOpen,
            (o, v) => o.IsPopupOpen = v);
    
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

    private bool _isPopupOpen;

    internal bool IsPopupOpen
    {
        get => _isPopupOpen;
        set => SetAndRaise(IsPopupOpenProperty, ref _isPopupOpen, value);
    }
    
    private PlacementMode _placementMode;

    internal PlacementMode PopupPlacement
    {
        get => _placementMode;
        set => SetAndRaise(PopupPlacementProperty, ref _placementMode, value);
    }
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => MentionsToken.ID;
    #endregion

    private MentionTextArea? _textArea;
    private Popup? _popup;
    private MentionOptionList? _candidateList;
    private bool _ignorePopupClose;
    private readonly CompositeDisposable _subscriptionsOnOpen = new ();
    private IList<ListFilterDescription>? _filterDescriptions;
    private CancellationTokenSource? _asyncLoadCTS;
    private int _loadTaskCount;

    static Mentions()
    {
        LinesProperty.OverrideDefaultValue<Mentions>(1);
        MentionTextArea.KeyDownEvent.AddClassHandler<Mentions>(
            (x, e) => x.HandleKeyDown(e),
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
    }
    
    public Mentions()
    {
        this.RegisterResources();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == PlacementProperty)
        {
            ConfigurePopupPlacement();
        }
        else if (change.Property == DisplayCandidateCountProperty ||
                 change.Property == ItemHeightProperty)
        {
            ConfigureMaxPopupHeight();
        }
        else if (change.Property == IsPopupOpenProperty)
        {
            UpdatePseudoClasses();
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

        if (_candidateList != null)
        {
            _candidateList.ItemClicked -= HandleMentionOptionClicked;
        }
        
        _textArea      = e.NameScope.Find<MentionTextArea>(MentionsThemeConstants.TextAreaPart);
        _popup         = e.NameScope.Find<Popup>(MentionsThemeConstants.PopupPart);
        _candidateList = e.NameScope.Find<MentionOptionList>(MentionsThemeConstants.CandidateListPart);

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
            _popup.Opened              -= HandlePopupOpened;
            _popup.Closed              -= HandlePopupClosed;
        }
        
        if (_candidateList != null)
        {
            _candidateList.ItemClicked += HandleMentionOptionClicked;
        }
        
        ConfigurePopupPlacement();
    }

    private void HandleMentionOptionClicked(object? sender, MentionOptionItemClickedEventArgs eventArgs)
    {
        IsPopupOpen = false;
        InsertCandidateOption(eventArgs.Option);
    }

    private void InsertCandidateOption(IMentionOption option)
    {
        Debug.Assert(_textArea != null);
        var value = option.Value?.ToString() ?? option.Header;
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
        IsPopupOpen = true;
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

        if (_optionAsyncLoader != null)
        {
            HandleNodeLoadRequest(eventArgs.Predicate);
        }
        
        if (_candidateList != null)
        {
            var filterValue = eventArgs.Predicate;
            if (string.IsNullOrEmpty(filterValue))
            {
                _candidateList.FilterDescriptions?.Clear();
                _filterDescriptions = null;
            }
            else
            {
                var newFilterDescriptions = new List<ListFilterDescription>();
                if (_filterDescriptions != null)
                {
                    var oldFilterDescriptions = _filterDescriptions;
                    foreach (var description in oldFilterDescriptions)
                    {
                        var oldFilterValue = description.FilterConditions.First().ToString();
                        if (oldFilterValue != filterValue)
                        {
                            var newFilterDescription = new ListFilterDescription()
                            {
                                PropertyPath     = description.PropertyPath,
                                Filter           = description.Filter,
                                FilterConditions = [filterValue]
                            };
                            newFilterDescriptions.Add(newFilterDescription);
                        }
                        else
                        {
                            newFilterDescriptions.Add(description);
                        }
                    }
                }
                else
                {
                    newFilterDescriptions.Add(new ListFilterDescription()
                    {
                        PropertyPath     = nameof(IMentionOption.Header),
                        FilterConditions = [filterValue],
                    });
                    newFilterDescriptions.Add(new ListFilterDescription()
                    {
                        PropertyPath     = nameof(IMentionOption.Value),
                        FilterConditions = [filterValue],
                    });
               
                }
                _candidateList.FilterDescriptions?.Clear();
                _candidateList.FilterDescriptions?.AddRange(newFilterDescriptions);
                _candidateList.SelectedIndex = 0;
                _filterDescriptions          = newFilterDescriptions;
            }
        }
        
    }

    private void HandleCandidateCloseRequest(object? sender, EventArgs eventArgs)
    {
        IsPopupOpen        = false;
        _candidateList?.FilterDescriptions?.Clear();
        _filterDescriptions = null;
        if (_optionAsyncLoader != null)
        {
            OptionsSource       = null;
        }
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Value         ??= DefaultValue;
        TriggerPrefix ??= ["@"];
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
    
    private void HandleNodeLoadRequest(string? predicate)
    {
        if (_optionAsyncLoader == null)
        {
            return;
        }
        
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            Debug.Assert(_optionAsyncLoader != null);
            try
            {
                if (_asyncLoadCTS != null)
                {
                    await _asyncLoadCTS.CancelAsync();
                }

                _asyncLoadCTS = new CancellationTokenSource();
                IsLoading     = true;
                _loadTaskCount++;
                var result = await _optionAsyncLoader.LoadAsync(predicate, _asyncLoadCTS.Token);
                _loadTaskCount--;
                IsLoading = false;
                OptionsLoaded?.Invoke(this, new MentionOptionLoadedEventArgs(predicate, result));
                if (result.IsSuccess)
                {
                    SetCurrentValue(OptionsSourceProperty, result.Data);
                    if (_candidateList != null)
                    {
                        _candidateList.SelectedIndex = 0;
                    }
                }
            }
            catch (OperationCanceledException e)
            {
                IsLoading = false;
                OptionsLoaded?.Invoke(this, new MentionOptionLoadedEventArgs(predicate, new MentionOptionLoadResult()
                {
                    IsSuccess = false,
                    UserFriendlyMessage = e.Message,
                    ErrorCode = RpcErrorCode.Cancelled
                }));
                if (_loadTaskCount == 0)
                {
                    --_loadTaskCount;
                }
            }
        });
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
        IsPopupOpen = false;
    }
    
    private void HandleKeyDown(KeyEventArgs e)
    {
        if (e.Handled)
        {
            return;
        }

        if (IsPopupOpen)
        {
            if (e.Key == Key.Down)
            {
                _candidateList?.MoveSelectedBy(1);
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                _candidateList?.MoveSelectedBy(-1);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                if (_candidateList?.SelectedItem is IMentionOption selectedOption)
                {
                    InsertCandidateOption(selectedOption);
                }
                IsPopupOpen = false;
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                IsPopupOpen = false;
                e.Handled   = true;
            }
            else if (e.Key == Key.Tab)
            {
                IsPopupOpen = false;
            }
        }
    }
    
    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(MentionPseudoClass.CandidatePopupOpen, IsPopupOpen);
    }
    
    private void HandlePopupClosed(object? sender, EventArgs e)
    {
        _subscriptionsOnOpen.Clear();
        NotifyPopupClosed();
    }

    private void HandlePopupOpened(object? sender, EventArgs e)
    {
        _subscriptionsOnOpen.Clear();
        this.GetObservable(IsVisibleProperty).Subscribe(HandleIsVisibleChanged).DisposeWith(_subscriptionsOnOpen);
        foreach (var parent in this.GetVisualAncestors().OfType<Control>())
        {
            parent.GetObservable(IsVisibleProperty).Subscribe(HandleIsVisibleChanged).DisposeWith(_subscriptionsOnOpen);
        }
        NotifyPopupOpened();
        _textArea?.Focus();
    }
    
    protected void NotifyPopupClosed()
    {
        CandidatePopupClosed?.Invoke(this, EventArgs.Empty);
    }
    
    protected void NotifyPopupOpened()
    {
        CandidatePopupOpened?.Invoke(this, EventArgs.Empty);
    }
    
    private void HandleIsVisibleChanged(bool isVisible)
    {
        if (!isVisible && IsPopupOpen)
        {
            IsPopupOpen = false;
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
    
        if (IsPopupOpen)
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
                IsPopupOpen = false;
                e.Handled   = true;
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
}