using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

internal class DataGridColumnGroupHeader : TemplatedControl
{
    #region 公共属性定义

    public static readonly StyledProperty<object?> HeaderProperty =
        HeaderedContentControl.HeaderProperty.AddOwner<DataGridColumnGroupHeader>();
    
    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.Register<DataGridColumnGroupHeader, IDataTemplate?>(nameof(HeaderTemplate));
    
    public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
        ContentPresenter.HorizontalContentAlignmentProperty.AddOwner<DataGridColumnGroupHeader>();
    
    public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
        ContentPresenter.VerticalContentAlignmentProperty.AddOwner<DataGridColumnGroupHeader>();
    
    public static readonly StyledProperty<bool> IsFrozenProperty =
        AvaloniaProperty.Register<DataGridColumnGroupHeader, bool>(nameof(IsFrozen));
    
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
    
    public IDataTemplate? HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }
    
    public HorizontalAlignment HorizontalContentAlignment
    {
        get => GetValue(HorizontalContentAlignmentProperty);
        set => SetValue(HorizontalContentAlignmentProperty, value);
    }
    
    public VerticalAlignment VerticalContentAlignment
    {
        get => GetValue(VerticalContentAlignmentProperty);
        set => SetValue(VerticalContentAlignmentProperty, value);
    }
    
    public bool IsFrozen
    {
        get => GetValue(IsFrozenProperty);
        set => SetValue(IsFrozenProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<DataGridColumnGroupHeader>();
    
    internal CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    #endregion
    
    internal DataGridColumnGroupItem? OwningGroupItem { get; set; }

    private IDisposable? _owningGridBinding;

    static DataGridColumnGroupHeader()
    {
        PointerPressedEvent.AddClassHandler<DataGridColumnGroupHeader>((x, e) => x.NotifyHeaderPointerPressed(e));
        PointerReleasedEvent.AddClassHandler<DataGridColumnGroupHeader>((x, e) => x.NotifyHeaderPointerReleased(e));
    }

    // 绑定订阅表格的属性变更；分组项离开表格时由其 OwningGrid 变化成对释放
    internal void BindOwningGridProperties(DataGrid owningGrid)
    {
        _owningGridBinding?.Dispose();
        _owningGridBinding = Bind(SizeTypeProperty, owningGrid[!DataGrid.SizeTypeProperty]);
    }

    internal void ReleaseOwningGridBinding()
    {
        _owningGridBinding?.Dispose();
        _owningGridBinding = null;
    }

    private void NotifyHeaderPointerPressed(PointerPressedEventArgs e)
    {
        OwningGroupItem?.NotifyHeaderPointerPressed(e);
    }

    private void NotifyHeaderPointerReleased(PointerReleasedEventArgs e)
    {
        OwningGroupItem?.NotifyHeaderPointerReleased(e);
    }
}
