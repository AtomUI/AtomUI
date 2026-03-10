using System.Collections;
using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

public class Transfer : TemplatedControl,
                        IMotionAwareControl,
                        IInputControlStatusAware,
                        ISizeTypeAware,
                        IControlSharedTokenResourcesHost
{
    #region 公共属性定义
    
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<Transfer, IEnumerable?>(nameof(ItemsSource));
    
    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<Transfer>();
    
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<Transfer>();
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Transfer>();
    
    public static readonly StyledProperty<object?> FooterProperty =
        AvaloniaProperty.Register<Transfer, object?>(nameof(Footer));
    
    public static readonly StyledProperty<IDataTemplate?> FooterTemplateProperty =
        AvaloniaProperty.Register<Transfer, IDataTemplate?>(nameof(FooterTemplate));
    
    public static readonly StyledProperty<PathIcon?> SelectionsIconProperty =
        AvaloniaProperty.Register<Transfer, PathIcon?>(nameof(SelectionsIcon));
    
    public static readonly StyledProperty<PathIcon?> ToSourceTransferIconProperty =
        AvaloniaProperty.Register<Transfer, PathIcon?>(nameof(ToSourceTransferIcon));
    
    public static readonly StyledProperty<PathIcon?> ToTargetTransferIconProperty =
        AvaloniaProperty.Register<Transfer, PathIcon?>(nameof(ToTargetTransferIcon));
    
    public static readonly StyledProperty<bool> IsPaginationEnabledProperty =
        AvaloniaProperty.Register<Transfer, bool>(nameof(IsPaginationEnabled));
    
    public static readonly StyledProperty<bool> IsShowSearchProperty =
        AvaloniaProperty.Register<Transfer, bool>(nameof(IsShowSearch));
    
    public static readonly StyledProperty<bool> IsShowSelectAllProperty =
        AvaloniaProperty.Register<Transfer, bool>(nameof(IsShowSelectAll));
    
    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }
    
    public InputControlStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }
    
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    [DependsOn(nameof(FooterTemplate))]
    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }
    
    public IDataTemplate? FooterTemplate
    {
        get => GetValue(FooterTemplateProperty);
        set => SetValue(FooterTemplateProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public PathIcon? SelectionsIcon
    {
        get => GetValue(SelectionsIconProperty);
        set => SetValue(SelectionsIconProperty, value);
    }
    
    public PathIcon? ToSourceTransferIcon
    {
        get => GetValue(ToSourceTransferIconProperty);
        set => SetValue(ToSourceTransferIconProperty, value);
    }
    
    public PathIcon? ToTargetTransferIcon
    {
        get => GetValue(ToTargetTransferIconProperty);
        set => SetValue(ToTargetTransferIconProperty, value);
    }
    
    public bool IsPaginationEnabled
    {
        get => GetValue(IsPaginationEnabledProperty);
        set => SetValue(IsPaginationEnabledProperty, value);
    }
    
    public bool IsShowSearch
    {
        get => GetValue(IsShowSearchProperty);
        set => SetValue(IsShowSearchProperty, value);
    }
    
    public bool IsShowSelectAll
    {
        get => GetValue(IsShowSelectAllProperty);
        set => SetValue(IsShowSelectAllProperty, value);
    }
    #endregion

    #region 内部属性定义

    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => TransferToken.ID;

    #endregion
}