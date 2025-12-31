using AtomUI;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Data;

namespace AtomUI.Desktop.Controls;

internal static class SplitterPanelPseudoClass
{
    public const string Collapsed = ":collapsed";
    public const string NotResizable = ":not-resizable";
}

[PseudoClasses(SplitterPanelPseudoClass.Collapsed, SplitterPanelPseudoClass.NotResizable)]
public class SplitterPanel : ContentControl
{
    #region 公共属性定义

    public static readonly StyledProperty<Dimension?> SizeProperty =
        AvaloniaProperty.Register<SplitterPanel, Dimension?>(nameof(Size),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<Dimension?> DefaultSizeProperty =
        AvaloniaProperty.Register<SplitterPanel, Dimension?>(nameof(DefaultSize));

    public static readonly StyledProperty<Dimension?> MinProperty =
        AvaloniaProperty.Register<SplitterPanel, Dimension?>(nameof(Min));

    public static readonly StyledProperty<Dimension?> MaxProperty =
        AvaloniaProperty.Register<SplitterPanel, Dimension?>(nameof(Max));

    public static readonly StyledProperty<bool> IsResizableProperty =
        AvaloniaProperty.Register<SplitterPanel, bool>(nameof(IsResizable), true);

    public static readonly StyledProperty<SplitterPanelCollapsible?> CollapsibleProperty =
        AvaloniaProperty.Register<SplitterPanel, SplitterPanelCollapsible?>(nameof(Collapsible));

    public static readonly StyledProperty<bool> IsCollapsedProperty =
        AvaloniaProperty.Register<SplitterPanel, bool>(nameof(IsCollapsed),
            defaultValue: false,
            inherits: false,
            defaultBindingMode: BindingMode.TwoWay);

    public Dimension? Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public Dimension? DefaultSize
    {
        get => GetValue(DefaultSizeProperty);
        set => SetValue(DefaultSizeProperty, value);
    }

    public Dimension? Min
    {
        get => GetValue(MinProperty);
        set => SetValue(MinProperty, value);
    }

    public Dimension? Max
    {
        get => GetValue(MaxProperty);
        set => SetValue(MaxProperty, value);
    }

    public bool IsResizable
    {
        get => GetValue(IsResizableProperty);
        set => SetValue(IsResizableProperty, value);
    }

    public SplitterPanelCollapsible? Collapsible
    {
        get => GetValue(CollapsibleProperty);
        set => SetValue(CollapsibleProperty, value);
    }

    public bool IsCollapsed
    {
        get => GetValue(IsCollapsedProperty);
        set => SetValue(IsCollapsedProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal double EffectiveSize { get; set; }
    internal double? LastNonCollapsedSize { get; set; }
    internal int? LastCollapsedIntoIndex { get; set; }
    internal Splitter? Owner { get; set; }

    internal bool IsCollapsible => Collapsible?.IsEnabled == true;

    internal DimensionUnitType PreferredSizeUnit
    {
        get
        {
            if (Size.HasValue)
            {
                return Size.Value.UnitType;
            }

            if (DefaultSize.HasValue)
            {
                return DefaultSize.Value.UnitType;
            }

            return DimensionUnitType.Pixel;
        }
    }

    #endregion

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsResizableProperty || change.Property == IsCollapsedProperty)
        {
            UpdatePseudoClasses();
        }
    }

    internal void UpdatePseudoClasses()
    {
        PseudoClasses.Set(SplitterPanelPseudoClass.Collapsed, IsCollapsed);
        PseudoClasses.Set(SplitterPanelPseudoClass.NotResizable, !IsResizable);
    }
}
