using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

public class AbstractSelect : TemplatedControl,
                              IMotionAwareControl,
                              ISizeTypeAware
{
    #region 公共属性定义
    public static readonly StyledProperty<bool> IsAllowClearProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(IsAllowClear));
    
    public static readonly StyledProperty<bool> IsAutoClearSearchValueProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(IsAutoClearSearchValue));
    
    public static readonly StyledProperty<bool> IsDefaultOpenProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(IsDefaultOpen));
    
    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(IsDropDownOpen));
    
    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        AvaloniaProperty.Register<AbstractSelect, string?>(nameof(PlaceholderText));
    
    public static readonly StyledProperty<IBrush?> PlaceholderForegroundProperty =
        AvaloniaProperty.Register<AbstractSelect, IBrush?>(nameof(PlaceholderForeground));
    
    public static readonly StyledProperty<bool> IsPopupMatchSelectWidthProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(IsPopupMatchSelectWidth), true);
    
    public static readonly StyledProperty<bool> IsSearchEnabledProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(IsSearchEnabled));
    
    public static readonly StyledProperty<int> DisplayPageSizeProperty = 
        AvaloniaProperty.Register<AbstractSelect, int>(nameof (DisplayPageSize), 10);
    
    public static readonly StyledProperty<int> MaxCountProperty =
        AvaloniaProperty.Register<AbstractSelect, int>(nameof(MaxCount), int.MaxValue);
    
    public static readonly StyledProperty<bool> IsShowMaxCountIndicatorProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(IsShowMaxCountIndicator));
    
    public static readonly StyledProperty<int?> MaxTagCountProperty =
        AvaloniaProperty.Register<AbstractSelect, int?>(nameof(MaxTagCount));
    
    public static readonly StyledProperty<bool?> IsResponsiveMaxTagCountProperty =
        AvaloniaProperty.Register<AbstractSelect, bool?>(nameof(IsResponsiveMaxTagCount));
    
    public static readonly StyledProperty<string?> MaxTagPlaceholderProperty =
        AvaloniaProperty.Register<AbstractSelect, string?>(nameof(MaxTagPlaceholder));
    
    public static readonly StyledProperty<SelectMode> ModeProperty =
        AvaloniaProperty.Register<AbstractSelect, SelectMode>(nameof(Mode));
    
    public static readonly StyledProperty<object?> LeftAddOnProperty =
        AddOnDecoratedBox.LeftAddOnProperty.AddOwner<AbstractSelect>();
    
    public static readonly StyledProperty<IDataTemplate?> LeftAddOnTemplateProperty =
        AddOnDecoratedBox.LeftAddOnTemplateProperty.AddOwner<AbstractSelect>();

    public static readonly StyledProperty<object?> RightAddOnProperty =
        AddOnDecoratedBox.RightAddOnProperty.AddOwner<AbstractSelect>();
    
    public static readonly StyledProperty<IDataTemplate?> RightAddOnTemplateProperty =
        AddOnDecoratedBox.RightAddOnTemplateProperty.AddOwner<AbstractSelect>();
    
    public static readonly StyledProperty<object?> ContentLeftAddOnProperty =
        AddOnDecoratedBox.ContentLeftAddOnProperty.AddOwner<AbstractSelect>();
    
    public static readonly StyledProperty<IDataTemplate?> ContentLeftAddOnTemplateProperty =
        AddOnDecoratedBox.ContentLeftAddOnTemplateProperty.AddOwner<AbstractSelect>();

    public static readonly StyledProperty<object?> ContentRightAddOnProperty =
        AddOnDecoratedBox.ContentRightAddOnProperty.AddOwner<AbstractSelect>();
    
    public static readonly StyledProperty<IDataTemplate?> ContentRightAddOnTemplateProperty =
        AddOnDecoratedBox.ContentRightAddOnTemplateProperty.AddOwner<AbstractSelect>();
    
    public static readonly StyledProperty<AddOnDecoratedVariant> StyleVariantProperty =
        AddOnDecoratedBox.StyleVariantProperty.AddOwner<AbstractSelect>();

    public static readonly StyledProperty<AddOnDecoratedStatus> StatusProperty =
        AddOnDecoratedBox.StatusProperty.AddOwner<AbstractSelect>();
    
    public static readonly StyledProperty<SelectPopupPlacement> PlacementProperty =
        AvaloniaProperty.Register<AbstractSelect, SelectPopupPlacement>(nameof(Placement));
    
    public static readonly StyledProperty<object?> SearchValueProperty =
        AvaloniaProperty.Register<AbstractSelect, object?>(nameof(SearchValue));
    
    public static readonly StyledProperty<PathIcon?> SuffixIconProperty =
        AvaloniaProperty.Register<AbstractSelect, PathIcon?>(nameof(SuffixIcon));
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractSelect>();
    
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractSelect>();
    
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
    
    public int DisplayPageSize
    {
        get => GetValue(DisplayPageSizeProperty);
        set => SetValue(DisplayPageSizeProperty, value);
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
    
    public SelectPopupPlacement Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }
    
    public object? SearchValue
    {
        get => GetValue(SearchValueProperty);
        set => SetValue(SearchValueProperty, value);
    }
    
    public PathIcon? SuffixIcon
    {
        get => GetValue(SuffixIconProperty);
        set => SetValue(SuffixIconProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    #endregion
}