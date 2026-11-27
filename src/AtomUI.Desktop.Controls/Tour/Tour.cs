using System.Collections.Specialized;
using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Generated.AtomUIDesktopControls;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using ItemCollection = AtomUI.Collections.ItemCollection;

/// <summary>
/// The visual style of a <see cref="Tour" />. Set it for the whole tour with <see cref="Tour.StyleType" /> or
/// for one step with <see cref="ITourStepOption.StyleType" />.
/// </summary>
public enum TourStyleType
{
    /// <summary>
    /// The standard look: the step card uses the elevated background color of the theme.
    /// </summary>
    Default,
    /// <summary>
    /// The emphasized look: the step card uses the primary color as its background and light text, and the
    /// indicator and the buttons of the card follow that color.
    /// </summary>
    Primary,
}

/// <summary>
/// A guided tour that leads through a list of steps. Each step points at a target control: a mask dims the
/// surrounding area, leaves the target uncovered and shows a card next to it. The steps are the content of
/// the control or come from <see cref="StepsSource" />, and <see cref="IsOpen" /> controls whether the tour
/// is shown.
/// </summary>
public partial class Tour : TemplatedControl, IMotionAwareControl, ISemanticPartCrossRootProvider
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="StepsSource" /> property.
    /// </summary>
    public static readonly StyledProperty<IEnumerable<ITourStepOption>?> StepsSourceProperty =
        AvaloniaProperty.Register<Tour, IEnumerable<ITourStepOption>?>(nameof(StepsSource));
    
    /// <summary>
    /// Identifies the <see cref="ItemTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<Tour, IDataTemplate?>(nameof(ItemTemplate));

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Tour>();
    
    /// <summary>
    /// Identifies the <see cref="IsArrowVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsArrowVisibleProperty =
        AvaloniaProperty.Register<Tour, bool>(nameof(IsArrowVisible), true);
    
    /// <summary>
    /// Identifies the <see cref="IsPointAtCenter" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPointAtCenterProperty =
        AvaloniaProperty.Register<Tour, bool>(nameof(IsPointAtCenter), false);
    
    /// <summary>
    /// Identifies the <see cref="CloseIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<IIconTemplate?> CloseIconProperty =
        AvaloniaProperty.Register<Tour, IIconTemplate?>(nameof(CloseIcon));
    
    /// <summary>
    /// Identifies the <see cref="IsDisabledInteraction" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDisabledInteractionProperty =
        AvaloniaProperty.Register<Tour, bool>(nameof(IsDisabledInteraction));
    
    /// <summary>
    /// Identifies the <see cref="GapOffsetX" /> property.
    /// </summary>
    public static readonly StyledProperty<double> GapOffsetXProperty =
        AvaloniaProperty.Register<Tour, double>(nameof(GapOffsetX), 6);
    
    /// <summary>
    /// Identifies the <see cref="GapOffsetY" /> property.
    /// </summary>
    public static readonly StyledProperty<double> GapOffsetYProperty =
        AvaloniaProperty.Register<Tour, double>(nameof(GapOffsetY), 6);
    
    /// <summary>
    /// Identifies the <see cref="GapRadius" /> property.
    /// </summary>
    public static readonly StyledProperty<double> GapRadiusProperty =
        AvaloniaProperty.Register<Tour, double>(nameof(GapRadius), 2);
    
    /// <summary>
    /// Identifies the <see cref="Placement" /> property.
    /// </summary>
    public static readonly StyledProperty<TourPlacementMode> PlacementProperty =
        AvaloniaProperty.Register<Tour, TourPlacementMode>(nameof(Placement));
    
    /// <summary>
    /// Identifies the <see cref="IsShowMask" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowMaskProperty =
        AvaloniaProperty.Register<Tour, bool>(nameof(IsShowMask), true);
    
    /// <summary>
    /// Identifies the <see cref="MaskColor" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> MaskColorProperty =
        AvaloniaProperty.Register<Tour, IBrush?>(nameof(MaskColor));
    
    /// <summary>
    /// Identifies the <see cref="StyleType" /> property.
    /// </summary>
    public static readonly StyledProperty<TourStyleType> StyleTypeProperty =
        AvaloniaProperty.Register<Tour, TourStyleType>(nameof(StyleType));
    
    /// <summary>
    /// Identifies the <see cref="IsScrollIntoView" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsScrollIntoViewProperty =
        AvaloniaProperty.Register<Tour, bool>(nameof(IsScrollIntoView), true);
    
    /// <summary>
    /// Identifies the <see cref="IsOpen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<Tour, bool>(
            nameof(IsOpen),
            defaultBindingMode: BindingMode.TwoWay);
    
    /// <summary>
    /// Identifies the <see cref="CurrentIndex" /> property.
    /// </summary>
    public static readonly DirectProperty<Tour, int> CurrentIndexProperty =
        AvaloniaProperty.RegisterDirect<Tour, int>(
            nameof(CurrentIndex),
            o => o.CurrentIndex,
            (o, v) => o.CurrentIndex = v,
            unsetValue: -1,
            defaultBindingMode: BindingMode.TwoWay);
    
    /// <summary>
    /// Identifies the <see cref="StepCount" /> property.
    /// </summary>
    public static readonly DirectProperty<Tour, int> StepCountProperty =
        AvaloniaProperty.RegisterDirect<Tour, int>(nameof(StepCount), o => o.StepCount);
    
    /// <summary>
    /// Identifies the <see cref="Indicator" /> property.
    /// </summary>
    public static readonly StyledProperty<TourIndicator?> IndicatorProperty =
        AvaloniaProperty.Register<Tour, TourIndicator?>(nameof(Indicator));
    
    /// <summary>
    /// The steps of the tour as data. Assigning it closes the tour, replaces the content of <see cref="Steps" />
    /// with the items of the source and returns to the first step; <see langword="null" /> leaves no steps. Items
    /// that are plain data objects are presented with <see cref="ItemTemplate" />.
    /// </summary>
    public IEnumerable<ITourStepOption>? StepsSource
    {
        get => GetValue(StepsSourceProperty);
        set => SetValue(StepsSourceProperty, value);
    }
    
    /// <summary>
    /// Template that builds the content of the step card for a step item that is a plain data object. The data
    /// context is the step item. A <c>TourStep</c> carries its own content and does not use the template.
    /// </summary>
    [InheritDataTypeFromItems(nameof(StepsSource))]
    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }
    
    /// <summary>
    /// Whether changes are animated, such as the mask cutout moving to the target of the next step. The value is
    /// passed on to the step cards and the indicator. The theme sets it from the <c>EnableMotion</c> token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    /// <summary>
    /// Whether the popup shows an arrow that points at the target of the current step. A step overrides it with
    /// <see cref="ITourStepOption.IsArrowVisible" />; a step that has no target shows no arrow.
    /// </summary>
    public bool IsArrowVisible
    {
        get => GetValue(IsArrowVisibleProperty);
        set => SetValue(IsArrowVisibleProperty, value);
    }

    /// <summary>
    /// Default for the <see cref="ITourStepOption.IsPointAtCenter" /> of steps that do not set their own value.
    /// The tour passes the value on to the step cards; nothing in the tour evaluates it.
    /// </summary>
    public bool IsPointAtCenter
    {
        get => GetValue(IsPointAtCenterProperty);
        set => SetValue(IsPointAtCenterProperty, value);
    }
    
    /// <summary>
    /// The icon of the close button in the step card. The theme sets the <c>CloseOutlined</c> icon.
    /// </summary>
    public IIconTemplate? CloseIcon
    {
        get => GetValue(CloseIconProperty);
        set => SetValue(CloseIconProperty, value);
    }
    
    /// <summary>
    /// Not evaluated. Neither the tour nor its themes read this value, so setting it changes no behavior.
    /// </summary>
    public bool IsDisabledInteraction
    {
        get => GetValue(IsDisabledInteractionProperty);
        set => SetValue(IsDisabledInteractionProperty, value);
    }
    
    /// <summary>
    /// The horizontal distance between the target of the current step and the edge of the mask cutout around it,
    /// in device-independent units. For a step placed to the left or right of its target it also sets the gap
    /// between the popup and the target.
    /// </summary>
    public double GapOffsetX
    {
        get => GetValue(GapOffsetXProperty);
        set => SetValue(GapOffsetXProperty, value);
    }
    
    /// <summary>
    /// The vertical distance between the target of the current step and the edge of the mask cutout around it, in
    /// device-independent units. For a step placed above or below its target it also sets the gap between the
    /// popup and the target.
    /// </summary>
    public double GapOffsetY
    {
        get => GetValue(GapOffsetYProperty);
        set => SetValue(GapOffsetYProperty, value);
    }
    
    /// <summary>
    /// The corner radius of the mask cutout around the target of the current step, in device-independent units.
    /// </summary>
    public double GapRadius
    {
        get => GetValue(GapRadiusProperty);
        set => SetValue(GapRadiusProperty, value);
    }
    
    /// <summary>
    /// Where the popup is placed relative to the target of a step. It is the default for every step; a step
    /// overrides it with <see cref="ITourStepOption.Placement" />. A step that has no target is placed with
    /// <see cref="TourPlacementMode.Center" />. The theme sets <see cref="TourPlacementMode.Bottom" />.
    /// </summary>
    public TourPlacementMode Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }
    
    /// <summary>
    /// Whether the mask covers the area around the target of the current step. A step overrides it with
    /// <see cref="ITourStepOption.IsShowMask" />. Without the mask the popup is still shown.
    /// </summary>
    public bool IsShowMask
    {
        get => GetValue(IsShowMaskProperty);
        set => SetValue(IsShowMaskProperty, value);
    }
    
    /// <summary>
    /// The brush that paints the mask. A step overrides it with <see cref="ITourStepOption.MaskColor" />. The
    /// theme sets the <c>ColorBgMask</c> token; a <see langword="null" /> value leaves the mask layer without a
    /// brush.
    /// </summary>
    public IBrush? MaskColor
    {
        get => GetValue(MaskColorProperty);
        set => SetValue(MaskColorProperty, value);
    }
    
    /// <summary>
    /// The visual style of the tour. A step overrides it with <see cref="ITourStepOption.StyleType" />.
    /// </summary>
    public TourStyleType StyleType
    {
        get => GetValue(StyleTypeProperty);
        set => SetValue(StyleTypeProperty, value);
    }
    
    /// <summary>
    /// Default for the <see cref="ITourStepOption.IsScrollIntoView" /> of steps that do not set their own value.
    /// The tour passes the value on to the step cards; nothing in the tour evaluates it.
    /// </summary>
    public bool IsScrollIntoView
    {
        get => GetValue(IsScrollIntoViewProperty);
        set => SetValue(IsScrollIntoViewProperty, value);
    }
    
    /// <summary>
    /// Whether the tour is shown. <see langword="true" /> opens it at <see cref="CurrentIndex" /> and
    /// <see langword="false" /> closes it and returns to the first step. The property is two-way by default, so
    /// a binding is updated when the tour closes itself. Nothing is shown while <see cref="Steps" /> is empty,
    /// and <see langword="false" /> is reverted while <see cref="IsPopupPinnedOpen" /> is
    /// <see langword="true" />.
    /// </summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }
    
    private int _currentIndex;
    /// <summary>
    /// The index of the current step in <see cref="Steps" />, or -1 when there are no steps. While the tour is
    /// open, setting it shows that step, and the navigation of the step card changes it. The tour returns to the
    /// first step when it closes. Any other value outside the steps throws
    /// <see cref="ArgumentOutOfRangeException" /> while the tour is open.
    /// </summary>
    public int CurrentIndex
    {
        get => _currentIndex;
        private set => SetAndRaise(CurrentIndexProperty, ref _currentIndex, value);
    }
    
    private int _stepCount;
    /// <summary>
    /// The number of steps in <see cref="Steps" />. It is read-only and is passed on to the
    /// <see cref="Indicator" />.
    /// </summary>
    public int StepCount
    {
        get => _stepCount;
        private set => SetAndRaise(StepCountProperty, ref _stepCount, value);
    }
    
    /// <summary>
    /// The control that shows the position of the current step. When no indicator is set when the control is
    /// initialized, a <c>DefaultTourIndicator</c> is created. The tour keeps the step count, the active index,
    /// the style and the motion switch of the indicator in sync with its own state.
    /// </summary>
    public TourIndicator? Indicator
    {
        get => GetValue(IndicatorProperty);
        set => SetValue(IndicatorProperty, value);
    }
    
    /// <summary>
    /// The steps of the tour, set as the content of the control or in code. Put <c>TourStep</c> instances here
    /// or any object that implements <see cref="ITourStepOption" />. Assigning <see cref="StepsSource" />
    /// replaces the content of this collection.
    /// </summary>
    [Content]
    public ItemCollection Steps { get; } = new();
    /// <summary>
    /// Controls that the step card shows in its footer in front of the built-in navigation buttons. Each item
    /// must be a <see cref="Control" />. An item that implements <see cref="ITourAction" /> receives this tour
    /// through its <c>NotifyAttached</c> method before it is shown.
    /// </summary>
    public ItemCollection CustomActions { get; } = new();
    
    #endregion

    #region 内部属性定义

    // The popup's Token gap to the target (TourToken.PopupMarginToAnchor), projected by the theme so it follows theme and
    // Control-level changes; the open popup's margin is the gap offset of the step placement plus this value.
    internal static readonly StyledProperty<double> PopupMarginToAnchorProperty =
        AvaloniaProperty.Register<Tour, double>(nameof(PopupMarginToAnchor));
    internal static readonly DirectProperty<Tour, Rect> TargetClipBoundsProperty =
        AvaloniaProperty.RegisterDirect<Tour, Rect>(nameof(TargetClipBounds),
            o => o.TargetClipBounds,
            (o, v) => o.TargetClipBounds = v);
    
    internal static readonly DirectProperty<Tour, bool> IsPopupHorizontalFlippedProperty =
        AvaloniaProperty.RegisterDirect<Tour, bool>(nameof(IsPopupHorizontalFlipped),
            o => o.IsPopupHorizontalFlipped,
            (o, v) => o.IsPopupHorizontalFlipped = v);

    internal static readonly DirectProperty<Tour, bool> IsPopupVerticalFlippedProperty =
        AvaloniaProperty.RegisterDirect<Tour, bool>(nameof(IsPopupVerticalFlipped),
            o => o.IsPopupVerticalFlipped,
            (o, v) => o.IsPopupVerticalFlipped = v);
    
    internal static readonly StyledProperty<ArrowPosition> ArrowPositionProperty =
        ArrowDecoratedBox.ArrowPositionProperty.AddOwner<Tour>();
    
    internal static readonly DirectProperty<Tour, TourStyleType> CurrentStyleTypeProperty =
        AvaloniaProperty.RegisterDirect<Tour, TourStyleType>(nameof(CurrentStyleType),
            o => o.CurrentStyleType,
            (o, v) => o.CurrentStyleType = v);
    
    internal static readonly DirectProperty<Tour, bool> CurrentArrowVisibleProperty =
        AvaloniaProperty.RegisterDirect<Tour, bool>(nameof(CurrentArrowVisible),
            o => o.CurrentArrowVisible,
            (o, v) => o.CurrentArrowVisible = v);
    
    internal static readonly DirectProperty<Tour, IBrush?> CurrentMaskColorProperty =
        AvaloniaProperty.RegisterDirect<Tour, IBrush?>(nameof(CurrentMaskColor),
            o => o.CurrentMaskColor,
            (o, v) => o.CurrentMaskColor = v);

    /// <summary>
    /// Identifies the <see cref="IsPopupPinnedOpen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        Popup.IsPopupPinnedOpenProperty.AddOwner<Tour>();

    internal double PopupMarginToAnchor
    {
        get => GetValue(PopupMarginToAnchorProperty);
        set => SetValue(PopupMarginToAnchorProperty, value);
    }
    
    private Rect _targetClipBounds;

    internal Rect TargetClipBounds
    {
        get => _targetClipBounds;
        set => SetAndRaise(TargetClipBoundsProperty, ref _targetClipBounds, value);
    }
    
    private bool _isPopupHorizontalFlipped;

    internal bool IsPopupHorizontalFlipped
    {
        get => _isPopupHorizontalFlipped;
        private set => SetAndRaise(IsPopupHorizontalFlippedProperty, ref _isPopupHorizontalFlipped, value);
    }

    private bool _isPopupVerticalFlipped;

    internal bool IsPopupVerticalFlipped
    {
        get => _isPopupVerticalFlipped;
        private set => SetAndRaise(IsPopupVerticalFlippedProperty, ref _isPopupVerticalFlipped, value);
    }
    
    internal ArrowPosition ArrowPosition
    {
        get => GetValue(ArrowPositionProperty);
        set => SetValue(ArrowPositionProperty, value);
    }
    
    private TourStyleType _currentStyleType;

    internal TourStyleType CurrentStyleType
    {
        get => _currentStyleType;
        private set => SetAndRaise(CurrentStyleTypeProperty, ref _currentStyleType, value);
    }

    private bool _currentArrowVisible;

    internal bool CurrentArrowVisible
    {
        get => _currentArrowVisible;
        private set => SetAndRaise(CurrentArrowVisibleProperty, ref _currentArrowVisible, value);
    }
    
    private IBrush? _currentMaskColor;

    internal IBrush? CurrentMaskColor
    {
        get => _currentMaskColor;
        private set => SetAndRaise(CurrentMaskColorProperty, ref _currentMaskColor, value);
    }

    /// <summary>
    /// Keeps the tour open so that its popup can be inspected, as Gallery semantic previews do. While it is
    /// <see langword="true" />, the tour opens when the control is loaded or when the property becomes
    /// <see langword="true" />, and neither setting <see cref="IsOpen" /> to <see langword="false" />,
    /// <see cref="HideTour" /> nor the close button closes it. Detaching the control from the visual tree still
    /// closes the tour.
    /// </summary>
    public bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }

    #endregion

    #region ISemanticPartCrossRootProvider 实现

    /// <summary>
    /// Returns the mask layer while it is visible, or an empty list otherwise. The layer is the shared tour layer
    /// of the visual layer manager of the top level, which lies outside the visual tree of this control; an open
    /// tour attaches it as its logical child so that semantic styles reach it. The semantic preview follows this
    /// list to refresh when the tour opens or closes.
    /// </summary>
    public IReadOnlyList<Visual> GetCrossRoots()
    {
        return _layer is { } layer && layer.IsVisible
            ? new Visual[] { layer }
            : Array.Empty<Visual>();
    }

    /// <inheritdoc />
    public event EventHandler? CrossRootsChanged;

    private void RaiseCrossRootsChanged()
    {
        CrossRootsChanged?.Invoke(this, EventArgs.Empty);
    }

    #endregion
    
    private bool _ignorePropertyChanged;
    private bool _isReallyOpened;
    private Popup? _popup;
    private TourLayer? _layer;
    private TourStepsView? _stepsView;
    private CompositeDisposable? _indicatorDisposables;
    private Control? _scrollBlockedTarget;
    private IDisposable? _popupPinnedOpenBinding;

    static Tour()
    {
        StepsSourceProperty.Changed.AddClassHandler<Tour>((tour, e) => tour.HandleStepsSourcePropertyChanged(e));
    }

    public Tour()
    {
        Steps.CollectionChanged         += HandleItemsViewCollectionChanged;
        CustomActions.CollectionChanged += HandleCustomActionsChanged;
    }

    /// <summary>
    /// Creates a <c>DefaultTourIndicator</c> when no <see cref="Indicator" /> is set, and copies the tour level
    /// step options into every step that leaves its own value unset. An override must call the base member first.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (Indicator == null)
        {
            SetCurrentValue(IndicatorProperty, new DefaultTourIndicator());
        }
        ConfigureDefaultValues();
    }

    private void HandleItemsViewCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        StepCount = Steps.Count;
        if (IsInitialized)
        {
            ConfigureDefaultValues();
        }
    }

    private void HandleCustomActionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_stepsView != null)
        {
            _stepsView.CustomActions = PrepareCustomActions();
        }
    }

    private Control[] PrepareCustomActions()
    {
        var actions = new Control[CustomActions.Count];
        var index   = 0;
        foreach (var item in CustomActions)
        {
            var action = (Control)item!;
            if (action is ITourAction tourAction)
            {
                tourAction.NotifyAttached(this);
            }
            actions[index++] = action;
        }

        return actions;
    }

    private void ConfigureDefaultValues()
    {
        foreach (var item in Steps)
        {
            if (item is ITourStepOption stepOption)
            {
                stepOption.IsArrowVisible      ??= IsArrowVisible;
                stepOption.IsPointAtCenter  ??= IsPointAtCenter;
                stepOption.IsShowMask       ??= IsShowMask;
                stepOption.StyleType        ??= StyleType;
                stepOption.Placement        ??= Placement;
                stepOption.IsScrollIntoView ??= IsScrollIntoView;
                // MaskColor is not copied: its default is the Tour's theme value (ColorBgMask), resolved when a step shows
                // so theme and Control-level changes keep reaching the mask.
            }
        }
    }

    /// <summary>
    /// Reacts to property changes while the control is attached to the visual tree: <see cref="IsOpen" /> shows
    /// or hides the tour, the gap properties recalculate the mask cutout, <see cref="CurrentIndex" /> shows that
    /// step while the tour is open, and a new <see cref="Indicator" /> is bound to the state of the tour.
    /// An override must call the base member first.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == IsOpenProperty)
            {
                if (!_ignorePropertyChanged)
                {
                    if (!change.GetNewValue<bool>() && IsPopupPinnedOpen)
                    {
                        using (BeginIgnoringPropertyChanged())
                        {
                            SetCurrentValue(IsOpenProperty, true);
                        }
                    }
                    else if (change.GetNewValue<bool>())
                    {
                        ShowTour();
                    }
                    else
                    {
                        HideTour();
                    }
                }
            }
            else if (change.Property == IsPopupPinnedOpenProperty &&
                     change.GetNewValue<bool>() &&
                     !IsOpen)
            {
                SetCurrentValue(IsOpenProperty, true);
            }
            else if (change.Property == GapRadiusProperty ||
                     change.Property == GapOffsetXProperty ||
                     change.Property == GapOffsetYProperty)
            {
                CalculateTargetClipBounds();
            }
        }
        
        // The popup margin belongs to the open popup only: ShowTour configures it for the current step, so a closed or
        // detached Tour does not reconfigure it when its gap inputs or the Token gap change (borrowed 9c01e0cc7).
        if (_isReallyOpened &&
            (change.Property == GapOffsetXProperty ||
             change.Property == GapOffsetYProperty ||
             change.Property == PopupMarginToAnchorProperty))
        {
            ConfigurePopupOffset(GetCurrentPlacement());
        }
        if (change.Property == MaskColorProperty && _layer != null && _isReallyOpened &&
            CurrentIndex >= 0 && CurrentIndex < Steps.Count && Steps[CurrentIndex] is ITourStepOption maskStep)
        {
            CurrentMaskColor = maskStep.MaskColor ?? MaskColor;
        }
        if (change.Property == IsMotionEnabledProperty && _layer != null && _isReallyOpened)
        {
            _layer.SetCurrentValue(TourLayer.IsMotionEnabledProperty, change.GetNewValue<bool>());
        }
        if (change.Property == CurrentIndexProperty)
        {
            if (!_ignorePropertyChanged)
            {
                HandleCurrentStepChanged();
            }
        }

        if (change.Property == IndicatorProperty)
        {
            if (change.OldValue is TourIndicator)
            {
                _indicatorDisposables?.Dispose();
                _indicatorDisposables = null;
            }

            if (change.NewValue is TourIndicator newIndicator)
            {
                _indicatorDisposables = new CompositeDisposable();
                _indicatorDisposables.Add(BindUtils.RelayBind(this, StepCountProperty, newIndicator, TourIndicator.StepCountProperty));
                _indicatorDisposables.Add(BindUtils.RelayBind(this, CurrentIndexProperty, newIndicator, TourIndicator.ActiveIndexProperty));
                _indicatorDisposables.Add(BindUtils.RelayBind(this, CurrentStyleTypeProperty, newIndicator, TourIndicator.StyleTypeProperty));
                _indicatorDisposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, newIndicator, TourIndicator.IsMotionEnabledProperty));
            }
        }
    }
    
    private void ConfigureArrowPosition(bool isHorizontalFlipped = false, bool isVerticalFlipped = false)
    {
        var requestedPlacement = _popup?.RequestedPlacement;
        if (requestedPlacement == null)
        {
            return;
        }

        ConfigureArrowPosition(requestedPlacement.Value, isHorizontalFlipped, isVerticalFlipped);
    }

    private void ConfigureArrowPosition(
        PlacementMode requestedPlacement,
        bool isHorizontalFlipped = false,
        bool isVerticalFlipped = false)
    {
        var arrowPosition = PopupUtils.CalculateArrowPosition(requestedPlacement, null, null);
        if (arrowPosition.HasValue)
        {
            SetCurrentValue(ArrowPositionProperty,
                ArrowPositionUtils.FlipArrowPosition(arrowPosition.Value, isHorizontalFlipped, isVerticalFlipped));
        }
    }

    private void HandlePositionFlipped(object? sender, PopupFlippedEventArgs args)
    {
        ConfigureArrowPosition(args.HorizontalFlipped, args.VerticalFlipped);
    }

    private void HandleStepsSourcePropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        ResetState();
        if (!Steps.IsReadOnly)
        {
            Steps.Clear();
        }
        Steps.SetItemsSource(change.GetNewValue<IEnumerable<ITourStepOption>?>());
    }
    
    /// <summary>
    /// Closes the tour and returns to the first step: sets <see cref="IsOpen" /> to <see langword="false" /> and
    /// <see cref="CurrentIndex" /> to 0, or to -1 when <see cref="Steps" /> is empty. It is called when
    /// <see cref="StepsSource" /> changes.
    /// </summary>
    public void ResetState()
    {
        SetCurrentValue(IsOpenProperty, false);
        SetCurrentValue(CurrentIndexProperty, Steps.Count > 0 ? 0 : -1);
    }

    /// <summary>
    /// Locates the popup and the steps view of the template, relays the pinned open state and the flipped state
    /// of the popup, and subscribes to the close and navigation requests of the steps view. An override must call
    /// the base member first.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_popup != null)
        {
            _popupPinnedOpenBinding?.Dispose();
            _popupPinnedOpenBinding = null;
            _popup.PositionFlipped -= HandlePositionFlipped;
        }
        _popup = e.NameScope.Find<Popup>("PART_Popup");
        if (_popup != null)
        {
            _popupPinnedOpenBinding = BindUtils.RelayBind(
                this,
                IsPopupPinnedOpenProperty,
                _popup,
                Popup.IsPopupPinnedOpenProperty);
            this[!IsPopupHorizontalFlippedProperty] = _popup[!Popup.IsHorizontalFlippedProperty];
            this[!IsPopupVerticalFlippedProperty] = _popup[!Popup.IsVerticalFlippedProperty];
            _popup.PositionFlipped += HandlePositionFlipped;
        }
        if (_stepsView != null)
        {
            _stepsView.CloseRequest -= HandleCloseRequest;
            _stepsView.NavRequest   -= HandleNavRequest;
        }
        _stepsView = e.NameScope.Find<TourStepsView>("StepsView");
        if (_stepsView != null)
        {
            _stepsView.CloseRequest  += HandleCloseRequest;
            _stepsView.NavRequest    += HandleNavRequest;
            _stepsView.CustomActions = PrepareCustomActions();
        }
    }

    private void HandleCloseRequest(object? sender, EventArgs args)
    {
        HideTour();
    }

    private void HandleNavRequest(object? sender, TourStepNavRequestEventArgs args)
    {
        SetCurrentValue(CurrentIndexProperty, args.Index);
    }

    /// <summary>
    /// Opens the tour at <see cref="CurrentIndex" />: it shows the mask layer, sets <see cref="IsOpen" /> to
    /// <see langword="true" /> and opens the popup. It does nothing when the tour is already open, when
    /// <see cref="Steps" /> is empty or when the template has not been applied. An index outside the steps is
    /// reset to 0.
    /// </summary>
    public void ShowTour()
    {
        if (_isReallyOpened || _popup == null || Steps.Count == 0)
        {
            return;
        }

        PrepareTourLayer();
        using (BeginIgnoringPropertyChanged())
        {
            SetCurrentValue(IsOpenProperty, true);
            if (CurrentIndex < 0 || CurrentIndex >= Steps.Count)
            {
                SetCurrentValue(CurrentIndexProperty, 0);
            }
        }
        HandleCurrentStepChanged();
        _popup.IsOpen = true;
        _isReallyOpened = true;
        _layer?.SetCurrentValue(TourLayer.IsMotionEnabledProperty, IsMotionEnabled);
    }

    private void PrepareTourLayer()
    {
        _layer = TourLayer.GetTourLayer(this);
        if (_layer != null)
        {
            _layer.SetCurrentValue(TourLayer.IsMotionEnabledProperty, false);
            _layer[!TourLayer.BackgroundProperty]               = this[!CurrentMaskColorProperty];
            _layer[!TourLayer.TargetRegionCornerRadiusProperty] = this[!GapRadiusProperty];
            _layer[!TourLayer.TargetRegionProperty]             = this[!TargetClipBoundsProperty];
            AttachMaskLayer(_layer);
        }
    }

    private void AttachMaskLayer(TourLayer layer)
    {
        if (!layer.Classes.Contains(TourSemanticParts.PopupMaskClass))
        {
            layer.Classes.Add(TourSemanticParts.PopupMaskClass);
        }

        if (ReferenceEquals(layer.Parent, this))
        {
            return;
        }

        // VisualLayerManager.AddLayer attaches the shared layer to itself as a logical child. A change from one
        // non-null logical parent to another throws, so the current parent is detached before the layer is attached
        // to this Tour. As a logical child of the Tour, the mask is matched across visual roots by the semantic
        // styles of the owner scope (">>" descendant selectors follow the logical tree).
        if (layer.Parent != null)
        {
            ((ISetLogicalParent)layer).SetParent(null);
        }

        ((ISetLogicalParent)layer).SetParent(this);
        RaiseCrossRootsChanged();
    }

    private void DetachMaskLayer(TourLayer layer)
    {
        if (layer.Parent == null)
        {
            return;
        }

        // The shared layer belongs to the tour that opened it and is released when that tour closes: it is detached
        // from the current Tour so that the next tour that opens can attach it.
        ((ISetLogicalParent)layer).SetParent(null);
        RaiseCrossRootsChanged();
    }
    
    /// <summary>
    /// Closes the tour: it hides the mask layer, closes the popup, sets <see cref="IsOpen" /> to
    /// <see langword="false" /> and returns to the first step. It does nothing while
    /// <see cref="IsPopupPinnedOpen" /> is <see langword="true" /> or when the tour is not open.
    /// </summary>
    public void HideTour()
    {
        if (IsPopupPinnedOpen)
        {
            return;
        }

        if (!_isReallyOpened || _popup == null)
        {
            return;
        }
        BlockTargetScroll(null);
        if (_layer != null)
        {
            _layer.SetCurrentValue(TourLayer.IsMotionEnabledProperty, false);
            _layer.IsVisible = false;
            DetachMaskLayer(_layer);
        }
        _popup.IsOpen = false;
        using (BeginIgnoringPropertyChanged())
        {
            SetCurrentValue(CurrentIndexProperty, 0);
            SetCurrentValue(IsOpenProperty, false);
            _isReallyOpened =  false;
        }
    }

    /// <summary>
    /// Opens the tour when <see cref="IsPopupPinnedOpen" /> or <see cref="IsOpen" /> is
    /// <see langword="true" /> and hides it otherwise; it then returns to the first step when the tour has
    /// steps. An override must call the base member first.
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (IsPopupPinnedOpen || IsOpen)
        {
            ShowTour();
        }
        else
        {
            HideTour();
        }

        if (Steps.Count > 0)
        {
            SetCurrentValue(CurrentIndexProperty, 0);
        }
    }

    private void CloseTourForLifecycle()
    {
        BlockTargetScroll(null);
        if (_layer != null)
        {
            _layer.SetCurrentValue(TourLayer.IsMotionEnabledProperty, false);
            _layer.IsVisible = false;
            DetachMaskLayer(_layer);
        }

        _popup?.CloseForLifecycle();
        using (BeginIgnoringPropertyChanged())
        {
            SetCurrentValue(CurrentIndexProperty, 0);
            SetCurrentValue(IsOpenProperty, false);
            _isReallyOpened = false;
        }
    }

    private void HandleCurrentStepChanged()
    {
        if (CurrentIndex == -1 || _popup == null || !IsOpen || !this.IsAttachedToVisualTree())
        {
            return;
        }

        if (CurrentIndex < 0 || CurrentIndex >= Steps.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(CurrentIndex));
        }
        
        var target = GetCurrentTarget();

        var      step   = Steps[CurrentIndex];
        if (step is ITourStepOption stepOption)
        {
            var stepPlacement = stepOption.Placement ?? Placement;
            var popupPlacement = target == null
                ? GetPopupPlacement(TourPlacementMode.Center)
                : GetPopupPlacement(stepPlacement);

            CurrentStyleType    = stepOption.StyleType ?? StyleType;
            CurrentArrowVisible = stepOption.IsArrowVisible ?? IsArrowVisible;
            if (_layer != null)
            {
                CurrentMaskColor = stepOption.MaskColor ?? MaskColor;
                _layer.IsVisible = stepOption.IsShowMask ?? IsShowMask;
            }

            if (target == null)
            {
                CurrentArrowVisible = false;
            }

            ConfigurePopupOffset(stepPlacement);
            ConfigureArrowPosition(popupPlacement);
            _popup.RequestedPlacement = popupPlacement;
        }

        _popup.PlacementTarget = target;
        BlockTargetScroll(target);
        CalculateTargetClipBounds();
    }

    private Control? GetCurrentTarget()
    {
        var      step   = Steps[CurrentIndex];
        Control? target = null;
        if (step is ITourStepOption stepOption)
        {
            target = stepOption.Target;
        }
        return target;
    }

    private TourPlacementMode GetCurrentPlacement()
    {
        if (CurrentIndex >= 0 &&
            CurrentIndex < Steps.Count &&
            Steps[CurrentIndex] is ITourStepOption stepOption)
        {
            return stepOption.Placement ?? Placement;
        }

        return Placement;
    }

    private void CalculateTargetClipBounds()
    {
        var      step   = Steps[CurrentIndex];
        Control? target = null;
        if (step is ITourStepOption stepOption)
        {
            target = stepOption.Target;
        }
        if (target == null)
        {
            TargetClipBounds = default;
            return;
        }
        // TourLayer draws in its own coordinate system, so the mask cutout (TargetRegion) and the mask geometry
        // must be in the same coordinate space. The layer may be hosted in the shared VisualLayerManager of the
        // TopLevel template (a mask over the whole window) or in a VisualLayerManager embedded in a stage (an
        // in-place mask that matches the upstream behavior with getPopupContainer set to false and covers only the
        // stage area). The cutout is therefore converted relative to the layer itself first. While the layer is not
        // attached to the visual tree, TopLevel coordinates are used instead; the cutout is recalculated with the
        // TargetRegion binding once the layer is attached.
        if (_layer is { } layer &&
            target.TranslatePoint(new Point(0, 0), layer) is { } layerOffset)
        {
            TargetClipBounds = new Rect(layerOffset, target.Bounds.Size)
                .Inflate(new Thickness(GapOffsetX, GapOffsetY));
            return;
        }
        var topLevel = TopLevel.GetTopLevel(target);
        if (topLevel != null)
        {
            var offset       = target.TranslatePoint(new Point(0, 0), topLevel) ?? default;
            var targetBounds = new Rect(offset, target.Bounds.Size);
            TargetClipBounds = targetBounds.Inflate(new Thickness(GapOffsetX, GapOffsetY));
        }
    }

    private PlacementMode GetPopupPlacement(TourPlacementMode placement)
    {
        return placement switch
        {
            TourPlacementMode.Left => PlacementMode.Left,
            TourPlacementMode.LeftTop => PlacementMode.LeftEdgeAlignedTop,
            TourPlacementMode.LeftBottom => PlacementMode.LeftEdgeAlignedBottom,
            TourPlacementMode.Right => PlacementMode.Right,
            TourPlacementMode.RightTop => PlacementMode.RightEdgeAlignedTop,
            TourPlacementMode.RightBottom => PlacementMode.RightEdgeAlignedBottom,
            TourPlacementMode.Top => PlacementMode.Top,
            TourPlacementMode.TopLeft => PlacementMode.TopEdgeAlignedLeft,
            TourPlacementMode.TopRight => PlacementMode.TopEdgeAlignedRight,
            TourPlacementMode.Bottom => PlacementMode.Bottom,
            TourPlacementMode.BottomLeft => PlacementMode.BottomEdgeAlignedLeft,
            TourPlacementMode.BottomRight => PlacementMode.BottomEdgeAlignedRight,
            TourPlacementMode.Center => PlacementMode.Center,
            _ => PlacementMode.Bottom
        };
    }

    private void ConfigurePopupOffset(TourPlacementMode placement)
    {
        if (_popup == null)
        {
            return;
        }
        AdjustForGap(placement);
        _popup.HorizontalOffset = 0;
        _popup.VerticalOffset   = 0;
    }

    private void BlockTargetScroll(Control? target)
    {
        if (_scrollBlockedTarget != null)
        {
            _scrollBlockedTarget.RemoveHandler(PointerWheelChangedEvent, HandleTargetPointerWheelChanged);
            _scrollBlockedTarget = null;
        }
        if (target != null)
        {
            target.AddHandler(PointerWheelChangedEvent, HandleTargetPointerWheelChanged, RoutingStrategies.Bubble);
            _scrollBlockedTarget = target;
        }
    }

    private static void HandleTargetPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        e.Handled = true;
    }

    private void AdjustForGap(TourPlacementMode placement)
    {
        Debug.Assert(_popup != null);
        var value = placement switch
        {
            TourPlacementMode.Left => GapOffsetX,
            TourPlacementMode.LeftTop => GapOffsetX,
            TourPlacementMode.LeftBottom => GapOffsetX,
            TourPlacementMode.Right => GapOffsetX,
            TourPlacementMode.RightTop => GapOffsetX,
            TourPlacementMode.RightBottom => GapOffsetX,
            TourPlacementMode.Top => GapOffsetY,
            TourPlacementMode.TopLeft => GapOffsetY,
            TourPlacementMode.TopRight => GapOffsetY,
            TourPlacementMode.Bottom => GapOffsetY,
            TourPlacementMode.BottomLeft => GapOffsetY,
            TourPlacementMode.BottomRight => GapOffsetY,
            _ => 0.0d
        };
        _popup.SetCurrentValue(Popup.MarginToAnchorProperty, value + PopupMarginToAnchor);
    }
    
     /// <summary>
     /// Closes the tour even while it is pinned open, then unsubscribes the handlers of the collections and of
     /// the steps view and disposes the bindings of the indicator. An override must call the base member.
     /// </summary>
     protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
     {
         CloseTourForLifecycle();
         base.OnDetachedFromVisualTree(e);

         Steps.CollectionChanged -= HandleItemsViewCollectionChanged;
         CustomActions.CollectionChanged -= HandleCustomActionsChanged;
         
         _indicatorDisposables?.Dispose();
         _indicatorDisposables = null;
         
         if (_stepsView != null)
         {
             _stepsView.CloseRequest -= HandleCloseRequest;
             _stepsView.NavRequest -= HandleNavRequest;
         }
     }
     
     private IgnorePropertyChangedScope BeginIgnoringPropertyChanged() => new IgnorePropertyChangedScope(this);
     
     private readonly struct IgnorePropertyChangedScope : IDisposable
     {
         private readonly Tour _owner;
     
         public IgnorePropertyChangedScope(Tour owner)
         {
             _owner                        = owner;
             _owner._ignorePropertyChanged = true;
         }
     
         public void Dispose() => _owner._ignorePropertyChanged = false;
     }
}
