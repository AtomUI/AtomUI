using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Theme.Styling;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

public class CardGridItem : ContentControl
{
    public static readonly StyledProperty<BoxShadows> BoxShadowProperty = 
        Border.BoxShadowProperty.AddOwner<CardGridItem>();
    
    public static readonly StyledProperty<int> RowProperty = 
        AvaloniaProperty.Register<CardGridItem, int>(nameof (Row));
    
    public static readonly StyledProperty<int> ColumnProperty = 
        AvaloniaProperty.Register<CardGridItem, int>(nameof (Column));
    
    public static readonly StyledProperty<int> RowSpanProperty = 
        AvaloniaProperty.Register<CardGridItem, int>(nameof (RowSpan));
    
    public static readonly StyledProperty<int> ColumnSpanProperty = 
        AvaloniaProperty.Register<CardGridItem, int>(nameof (ColumnSpan));
    
    public static readonly StyledProperty<bool> IsHoverableProperty = 
        AvaloniaProperty.Register<CardGridItem, bool>(nameof (IsHoverable), true);
    
    public BoxShadows BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
    }
    
    public int Row
    {
        get => GetValue(RowProperty);
        set => SetValue(RowProperty, value);
    }
    
    public int Column
    {
        get => GetValue(ColumnProperty);
        set => SetValue(ColumnProperty, value);
    }
    
    public int RowSpan
    {
        get => GetValue(RowSpanProperty);
        set => SetValue(RowSpanProperty, value);
    }
    
    public int ColumnSpan
    {
        get => GetValue(ColumnSpanProperty);
        set => SetValue(ColumnSpanProperty, value);
    }
    
    public bool IsHoverable
    {
        get => GetValue(IsHoverableProperty);
        set => SetValue(IsHoverableProperty, value);
    }
    
    #region 内部属性定义

    public static readonly StyledProperty<SizeType> SizeTypeProperty = 
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<CardGridItem>();
    
    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<CardGridItem>();
    
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion
    
    private void ConfigureTransitions(bool force)
    {
        if (IsMotionEnabled)
        {
            if (force || Transitions == null)
            {
                Transitions =
                [
                    TransitionUtils.CreateTransition<BoxShadowsTransition>(BoxShadowProperty, SharedTokenKey.MotionDurationFast)
                ];
            }
        }
        else
        {
            Transitions = null;
        }
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (IsLoaded)
        {
            if (change.Property == IsMotionEnabledProperty)
            {
                ConfigureTransitions(false);
            }
        }

        if (change.Property == RowProperty)
        {
            Grid.SetRow(this, Row);
        }
        else if (change.Property == ColumnProperty)
        {
            Grid.SetColumn(this, Column);
        }
        else if (change.Property == ColumnSpanProperty)
        {
            Grid.SetColumnSpan(this, ColumnSpan);
        }
        else if (change.Property == RowSpanProperty)
        {
            Grid.SetRowSpan(this, RowSpan);
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureTransitions(false);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        Transitions = null;
    }
}