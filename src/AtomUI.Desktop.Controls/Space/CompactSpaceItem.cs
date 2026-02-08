using System.Reactive.Disposables;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

internal class CompactSpaceItem : Decorator, ICompactSpaceAware
{
    internal static readonly StyledProperty<SpaceItemPosition?> CompactSpaceItemPositionProperty = 
        CompactSpaceAwareControlProperty.CompactSpaceItemPositionProperty.AddOwner<CompactSpaceItem>();
    
    internal static readonly StyledProperty<Orientation> CompactSpaceOrientationProperty = 
        CompactSpaceAwareControlProperty.CompactSpaceOrientationProperty.AddOwner<CompactSpaceItem>();
    
    internal static readonly StyledProperty<bool> IsUsedInCompactSpaceProperty = 
        CompactSpaceAwareControlProperty.IsUsedInCompactSpaceProperty.AddOwner<CompactSpaceItem>();
    
    public static readonly StyledProperty<int> PositionIndexProperty = 
        AvaloniaProperty.Register<CompactSpaceItem, int>(nameof(PositionIndex));
    
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
    
    internal int PositionIndex
    {
        get => GetValue(PositionIndexProperty);
        set => SetValue(PositionIndexProperty, value);
    }
    
    private CompositeDisposable? _disposables;

    static CompactSpaceItem()
    {
        AffectsMeasure<CompactSpaceItem>(CompactSpaceItemPositionProperty, CompactSpaceOrientationProperty, IsUsedInCompactSpaceProperty);
        ClipToBoundsProperty.OverrideDefaultValue<CompactSpaceItem>(false);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ChildProperty)
        {
            if (change.OldValue != null)
            {
                _disposables?.Dispose();
            }

            if (change.NewValue != null && change.NewValue is ICompactSpaceAware && Child != null)
            {
                _disposables = new CompositeDisposable();
                _disposables.Add(BindUtils.RelayBind(this, CompactSpaceItemPositionProperty, Child, CompactSpaceItemPositionProperty));
                _disposables.Add(BindUtils.RelayBind(this, CompactSpaceOrientationProperty, Child, CompactSpaceOrientationProperty));
                _disposables.Add(BindUtils.RelayBind(this, IsUsedInCompactSpaceProperty, Child, IsUsedInCompactSpaceProperty));
            }
        }
        if (change.Property == ChildProperty ||
            change.Property == CompactSpace.ItemSizeProperty ||
            change.Property == CompactSpaceOrientationProperty ||
            change.Property == IsUsedInCompactSpaceProperty ||
            change.Property == CompactSpaceItemPositionProperty)
        {
            ConfigureItemSize(CompactSpace.GetItemSize(this), IsUsedInCompactSpace, CompactSpaceOrientation);
        }
    }
    
    void ICompactSpaceAware.NotifyPositionChange(SpaceItemPosition? position)
    {
        IsUsedInCompactSpace     = position != null;
        CompactSpaceItemPosition = position;
    }

    bool ICompactSpaceAware.IsAlwaysActiveZIndex()
    {
        if (Child is ICompactSpaceAware compactSpaceAware)
        {
            return compactSpaceAware.IsAlwaysActiveZIndex();
        }

        return false;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var size = base.MeasureOverride(availableSize);

        if (CompactSpaceItemPosition == null ||
            CompactSpaceItemPosition == SpaceItemPosition.First ||
            (CompactSpaceItemPosition.Value.HasFlag(SpaceItemPosition.First) && CompactSpaceItemPosition.Value.HasFlag(SpaceItemPosition.Last)))
        {
            return size;
        }
        var borderThickness = (this as ICompactSpaceAware).GetBorderThickness();
        var delta           = borderThickness * PositionIndex;
        if (CompactSpaceOrientation == Orientation.Horizontal)
        {
            RenderTransform = new TranslateTransform(-delta, 0);
        }
        else
        {
            RenderTransform = new TranslateTransform(0, -delta);
        }
        return size;
    }

    void ICompactSpaceAware.NotifyOrientationChange(Orientation orientation)
    {
        CompactSpaceOrientation = orientation;
    }

    double ICompactSpaceAware.GetBorderThickness()
    {
        if (Child is ICompactSpaceAware compactSpaceAware)
        {
            return compactSpaceAware.GetBorderThickness();
        }

        return 0.0;
    }
    
    private void ConfigureItemSize(CompactSpaceSize size, bool isUsedInCompactSpace, Orientation compactSpaceOrientation) 
    {
        if (!isUsedInCompactSpace)
        {
            ClearValue(HorizontalAlignmentProperty);
            ClearValue(VerticalAlignmentProperty);
        }
        else
        {
            if (compactSpaceOrientation == Orientation.Horizontal)
            {
                if (size.IsStar)
                {
                    ClearValue(WidthProperty);
                    SetCurrentValue(HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
                }
                else if (size.IsAbsolute)
                {
                    SetCurrentValue(HorizontalAlignmentProperty, HorizontalAlignment.Left);
                    SetCurrentValue(WidthProperty, size.Value);
                }
                else
                {
                    ClearValue(WidthProperty);
                    ClearValue(HorizontalAlignmentProperty);
                }

                if (Child != null)
                {
                    Child.SetCurrentValue(HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
                }
            }
            else
            {
                if (size.IsStar)
                {
                    ClearValue(HeightProperty);
                    SetCurrentValue(VerticalAlignmentProperty, HorizontalAlignment.Stretch);
                }
                else if (size.IsAbsolute)
                {
                    SetCurrentValue(VerticalAlignmentProperty, VerticalAlignment.Top);
                    SetCurrentValue(HeightProperty, size.Value);
                }
                else
                {
                    ClearValue(HeightProperty);
                    ClearValue(VerticalAlignmentProperty);
                }
                
                if (Child != null)
                {
                    Child.SetCurrentValue(VerticalAlignmentProperty, VerticalAlignment.Stretch);
                }
            }
        }
    }
}