using System.Collections.Specialized;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

using ControlList = Avalonia.Controls.Controls;

/// <summary>
/// Lays out the avatars in <see cref="Children" /> in one row, each one overlapping the previous one.
/// When <see cref="MaxDisplayCount" /> hides avatars, a "+N" avatar at the end opens them in a flyout.
/// The size, shape and motion settings of the group are passed down to every avatar in it.
/// </summary>
public class AvatarGroup : TemplatedControl, IMotionAwareControl
{
    #region 公共属性定义
    
    /// <summary>
    /// Identifies the <see cref="FoldInfoAvatarBackground" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> FoldInfoAvatarBackgroundProperty = 
        AvaloniaProperty.Register<AvatarGroup, IBrush?>(nameof (FoldInfoAvatarBackground));
    
    /// <summary>
    /// Identifies the <see cref="FoldInfoAvatarForeground" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> FoldInfoAvatarForegroundProperty = 
        AvaloniaProperty.Register<AvatarGroup, IBrush?>(nameof (FoldInfoAvatarForeground));
    
    /// <summary>
    /// Identifies the <see cref="FoldAvatarFlyoutTriggerType" /> property.
    /// </summary>
    public static readonly StyledProperty<FlyoutTriggerType> FoldAvatarFlyoutTriggerTypeProperty =
        AvaloniaProperty.Register<AvatarGroup, FlyoutTriggerType>(nameof(FoldAvatarFlyoutTriggerType), FlyoutTriggerType.Hover);
    
    /// <summary>
    /// Identifies the <see cref="MaxDisplayCount" /> property.
    /// </summary>
    public static readonly StyledProperty<int?> MaxDisplayCountProperty =
        AvaloniaProperty.Register<AvatarGroup, int?>(nameof(MaxDisplayCount));
    
    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        Avatar.SizeTypeProperty.AddOwner<AvatarGroup>();
    
    /// <summary>
    /// Identifies the <see cref="Size" /> property.
    /// </summary>
    public static readonly StyledProperty<double> SizeProperty =
        Avatar.SizeProperty.AddOwner<AvatarGroup>();
    
    /// <summary>
    /// Identifies the <see cref="Shape" /> property.
    /// </summary>
    public static readonly StyledProperty<AvatarShape> ShapeProperty =
        Avatar.ShapeProperty.AddOwner<AvatarGroup>();

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AvatarGroup>();
    
    /// <summary>
    /// Background of the "+N" avatar that stands in for the avatars hidden by <see cref="MaxDisplayCount" />.
    /// The default theme sets it from a design token, which applies again when the value is cleared.
    /// <see langword="null" /> leaves the "+N" avatar without a background.
    /// </summary>
    public IBrush? FoldInfoAvatarBackground
    {
        get => GetValue(FoldInfoAvatarBackgroundProperty);
        set => SetValue(FoldInfoAvatarBackgroundProperty, value);
    }
    
    /// <summary>
    /// Foreground of the "+N" avatar that stands in for the avatars hidden by <see cref="MaxDisplayCount" />.
    /// The default theme sets it from a design token, which applies again when the value is cleared.
    /// <see langword="null" /> leaves the "+N" avatar without a foreground.
    /// </summary>
    public IBrush? FoldInfoAvatarForeground
    {
        get => GetValue(FoldInfoAvatarForegroundProperty);
        set => SetValue(FoldInfoAvatarForegroundProperty, value);
    }
    
    /// <summary>
    /// Interaction that opens the flyout of folded avatars; <see cref="FlyoutTriggerType.Hover" /> by default.
    /// With <see cref="FlyoutTriggerType.Click" /> the "+N" avatar shows a hand cursor, otherwise an arrow.
    /// </summary>
    public FlyoutTriggerType FoldAvatarFlyoutTriggerType
    {
        get => GetValue(FoldAvatarFlyoutTriggerTypeProperty);
        set => SetValue(FoldAvatarFlyoutTriggerTypeProperty, value);
    }
    
    /// <summary>
    /// Maximum number of avatars shown in the row.
    /// When the group holds more, the rest move into a flyout and a "+N" avatar closes the row.
    /// A value of 0 or less hides every avatar. <see langword="null" />, the default, shows all avatars.
    /// </summary>
    public int? MaxDisplayCount
    {
        get => GetValue(MaxDisplayCountProperty);
        set => SetValue(MaxDisplayCountProperty, value);
    }
    
    /// <summary>
    /// Size preset of every avatar in the group, including the "+N" avatar: the value is bound to each avatar, so it
    /// must be set on the group and not on an individual avatar.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    /// <summary>
    /// Width and height, in DIPs, of every avatar in the group, including the "+N" avatar.
    /// Each avatar uses them while its size type is <see cref="CustomizableSizeType.Custom" />.
    /// <see cref="double.NaN" />, the default, leaves the size to <see cref="SizeType" />.
    /// </summary>
    public double Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }
    
    /// <summary>
    /// Shape of every avatar in the group, including the "+N" avatar: the value is bound to each avatar.
    /// The default is a circle.
    /// </summary>
    public AvatarShape Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    /// <summary>
    /// Whether the group and every avatar in it take part in motion.
    /// The value is bound to each avatar, so the avatars follow the group rather than their own theme.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// The avatars to lay out, from left to right.
    /// Only items of type <see cref="Avatar" /> are sized and styled by the group.
    /// Do not clear the collection: a reset action makes the group throw a <see cref="NotSupportedException" />.
    /// </summary>
    [Content] public ControlList Children { get; } = new();

    #endregion
    
    #region 内部属性定义
    
    internal static readonly StyledProperty<double> GroupSpaceProperty = 
        AvaloniaProperty.Register<AvatarGroup, double>(nameof (GroupSpace));
    
    internal static readonly StyledProperty<double> GroupOverlappingProperty = 
        AvaloniaProperty.Register<AvatarGroup, double>(nameof (GroupOverlapping));

    internal static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        FlyoutHost.IsPopupPinnedOpenProperty.AddOwner<AvatarGroup>();
    
    internal double GroupSpace
    {
        get => GetValue(GroupSpaceProperty);
        set => SetValue(GroupSpaceProperty, value);
    }
    
    internal double GroupOverlapping
    {
        get => GetValue(GroupOverlappingProperty);
        set => SetValue(GroupOverlappingProperty, value);
    }

    internal bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }
    #endregion
    
    private Avatar? _foldCountAvatar;
    private FlyoutHost? _foldCountFlyout;
    private StackPanel? _foldCountStackPanel;
    private IDisposable? _foldCountFlyoutPinBinding;
    private IDisposable? _motionBinding;

    static AvatarGroup()
    {
        AffectsMeasure<AvatarGroup>(GroupOverlappingProperty, GroupSpaceProperty);
    }
    
    public AvatarGroup()
    {
        Children.CollectionChanged += ChildrenChanged;
    }

    /// <summary>
    /// Called when <see cref="Children" /> changes: rebuilds the shown avatars and requests a new measure.
    /// A reset action throws a <see cref="NotSupportedException" />. Call the base method from an override to keep
    /// the row in sync.
    /// </summary>
    protected virtual void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            throw new NotSupportedException();
        }

        RebuildChildrenPresentation();
        InvalidateMeasureOnChildrenChanged();
    }

    /// <summary>
    /// Called when the group is attached to a visual tree. Binds the group's motion style and rebuilds the shown
    /// avatars so that they receive the group's values. Call the base method from an override.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _motionBinding?.Dispose();
        _motionBinding = this.ConfigureMotionBindingStyle();
        RebuildChildrenPresentation();
    }

    /// <summary>
    /// Called when the group is detached from a visual tree. Disposes the motion style binding and releases the
    /// flyout with the hidden avatars. Call the base method from an override.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _motionBinding?.Dispose();
        _motionBinding = null;
        ReleaseFoldInfo();
    }

    /// <summary>
    /// Invalidates the measure of the group. It is called from <see cref="ChildrenChanged" /> and when
    /// <see cref="MaxDisplayCount" /> changes, each time after the shown avatars were rebuilt. Call the base method
    /// from an override so that the group is measured again.
    /// </summary>
    private protected virtual void InvalidateMeasureOnChildrenChanged()
    {
        InvalidateMeasure();
    }

    /// <summary>
    /// Measures to the largest avatar, then widens the result for all shown avatars with the overlaps subtracted.
    /// Every avatar is assumed to have the same size; an empty group measures zero wide.
    /// </summary>
    protected override Size MeasureOverride(Size availableSize)
    {
        var size = base.MeasureOverride(availableSize);
        // In theory all children have the same size, so the measured width stands for every one of them.
        var count      = LogicalChildren.Count;
        if (count == 0)
        {
            return size.WithWidth(0);
        }
        var totalWidth = count * size.Width - (count - 1) * GroupOverlapping;
        return size.WithWidth(totalWidth);
    }

    /// <summary>
    /// Arranges the shown avatars in one row from left to right, each one shifted left by the group's overlapping
    /// width so that it covers the previous one.
    /// </summary>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var offsetX = 0.0;
        for (var i = 0; i < LogicalChildren.Count; i++)
        {
            var child = LogicalChildren[i];
            if (child is Control avatar)
            {
                var childSize = avatar.DesiredSize;
                avatar.Arrange(new Rect(offsetX, 0, childSize.Width, childSize.Height));
                offsetX += childSize.Width - GroupOverlapping;
            }
        }
        return finalSize;
    }

    /// <summary>
    /// Called when a property of the group changes. Updates the cursor of the "+N" avatar when
    /// <see cref="FoldAvatarFlyoutTriggerType" /> changes, and rebuilds the shown avatars when
    /// <see cref="MaxDisplayCount" /> changes. Call the base method from an override.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == FoldAvatarFlyoutTriggerTypeProperty)
        {
            ConfigureFoldAvatarCursor();
        }
        else if (change.Property == MaxDisplayCountProperty)
        {
            RebuildChildrenPresentation();
            InvalidateMeasureOnChildrenChanged();
        }
    }

    private void RebuildChildrenPresentation()
    {
        ClearPresentedChildren();
        ConfigureChildren();
        UpdateChildrenZIndex();

        var visibleCount = GetVisibleChildrenCount();
        for (var i = 0; i < visibleCount; ++i)
        {
            LogicalChildren.Add(Children[i]);
            VisualChildren.Add(Children[i]);
        }

        ConfigureFoldInfo(visibleCount);
    }

    private void ClearPresentedChildren()
    {
        _foldCountStackPanel?.Children.Clear();
        LogicalChildren.Clear();
        VisualChildren.Clear();
    }

    private void ConfigureChildren()
    {
        foreach (var child in Children)
        {
            if (child is Avatar avatar)
            {
                ConfigureAvatar(avatar);
            }
        }
    }

    private void UpdateChildrenZIndex()
    {
        for (var i = 0; i < Children.Count; i++)
        {
            Children[i].ZIndex = i + 1;
        }
    }

    private int GetVisibleChildrenCount()
    {
        if (!MaxDisplayCount.HasValue || Children.Count <= MaxDisplayCount.Value)
        {
            return Children.Count;
        }

        return Math.Max(0, MaxDisplayCount.Value);
    }

    private void ConfigureFoldInfo(int visibleCount)
    {
        if (!ShouldFold(visibleCount))
        {
            ReleaseFoldInfo();
            return;
        }

        var foldCountFlyout = GetFoldCountFlyout();
        var foldCountAvatar = GetFoldCountAvatar();
        foldCountAvatar.Text = $"+{Children.Count - visibleCount}";
        LogicalChildren.Add(foldCountFlyout);
        VisualChildren.Add(foldCountFlyout);

        for (var i = visibleCount; i < Children.Count; ++i)
        {
            _foldCountStackPanel!.Children.Add(Children[i]);
        }
    }

    private bool ShouldFold(int visibleCount)
    {
        return MaxDisplayCount.HasValue && Children.Count > visibleCount;
    }

    private void BindAvatarProperties(Avatar avatar)
    {
        avatar[!BorderThicknessProperty] = this[!BorderThicknessProperty];
        avatar[!ShapeProperty]           = this[!ShapeProperty];
        avatar[!SizeProperty]            = this[!SizeProperty];
        avatar[!SizeTypeProperty]        = this[!SizeTypeProperty];
    }

    private void ConfigureAvatar(Avatar avatar)
    {
        BindAvatarProperties(avatar);
        avatar[!IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
    }

    private Avatar GetFoldCountAvatar()
    {
        if (_foldCountAvatar == null)
        {
            _foldCountAvatar = new Avatar();

            BindAvatarProperties(_foldCountAvatar);
            _foldCountAvatar[!ForegroundProperty] = this[!FoldInfoAvatarForegroundProperty];
            _foldCountAvatar[!BackgroundProperty] = this[!FoldInfoAvatarBackgroundProperty];
        }

        return _foldCountAvatar;
    }

    private FlyoutHost GetFoldCountFlyout()
    {
        if (_foldCountFlyout == null)
        {
            var foldCountAvatar = GetFoldCountAvatar();
            _foldCountStackPanel             = new StackPanel();
            _foldCountStackPanel.Orientation = Orientation.Horizontal;

            _foldCountFlyout                       = new FlyoutHost();
            _foldCountFlyout.ZIndex                = Int32.MaxValue;
            _foldCountFlyout.Content               = foldCountAvatar;
            _foldCountFlyout.ShouldUseOverlayPopup = true;

            _foldCountStackPanel[!StackPanel.SpacingProperty] = this[!GroupSpaceProperty];
            _foldCountFlyout[!FlyoutHost.TriggerProperty]     = this[!FoldAvatarFlyoutTriggerTypeProperty];

            _foldCountFlyout.Flyout = new Flyout
            {
                Content = _foldCountStackPanel
            };
            _foldCountFlyoutPinBinding = BindUtils.RelayBind(
                this,
                IsPopupPinnedOpenProperty,
                _foldCountFlyout,
                FlyoutHost.IsPopupPinnedOpenProperty);
            ConfigureFoldAvatarCursor();
        }

        return _foldCountFlyout;
    }

    private void ReleaseFoldInfo()
    {
        _foldCountStackPanel?.Children.Clear();
        if (_foldCountFlyout is { } foldCountFlyout)
        {
            _foldCountFlyoutPinBinding?.Dispose();
            _foldCountFlyoutPinBinding = null;
            foldCountFlyout.SetCurrentValue(FlyoutHost.IsPopupPinnedOpenProperty, false);
            foldCountFlyout.Flyout?.CloseForLifecycle();
            LogicalChildren.Remove(foldCountFlyout);
            VisualChildren.Remove(foldCountFlyout);
            foldCountFlyout.Flyout  = null;
            foldCountFlyout.Content = null;
        }

        _foldCountAvatar     = null;
        _foldCountFlyout     = null;
        _foldCountStackPanel = null;
    }

    private void ConfigureFoldAvatarCursor()
    {
        if (_foldCountAvatar == null)
        {
            return;
        }

        if (FoldAvatarFlyoutTriggerType == FlyoutTriggerType.Click)
        {
            _foldCountAvatar.Cursor = new Cursor(StandardCursorType.Hand);
        }
        else
        {
            _foldCountAvatar.Cursor = new Cursor(StandardCursorType.Arrow);
        }
    }
}
