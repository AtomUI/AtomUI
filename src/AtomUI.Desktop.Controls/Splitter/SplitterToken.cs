using AtomUI.Theme.TokenSystem;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class SplitterToken : AbstractControlDesignToken
{
    public const string ID = "Splitter";
    
    /// <summary>
    /// 拖拽标识元素大小
    /// Drag and drop the identity element size
    /// </summary>
    public double ResizeSpinnerSize { get; set; }

    /// <summary>
    /// 拖拽标识元素大小
    /// Drag and drop the identity element size
    /// </summary>
    public double SplitBarDraggableSize { get; set; }
    
    /// <summary>
    /// 拖拽元素显示大小
    /// Drag the element display size
    /// </summary>
    public double SplitBarSize { get; set; }
    
    /// <summary>
    /// 拖拽触发区域大小
    /// Drag and drop trigger area size
    /// </summary>
    public double SplitTriggerSize { get; set; }
    
    public SplitterToken()
        : base(ID)
    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        SplitBarSize          = 2;
        SplitTriggerSize      = 6;
        ResizeSpinnerSize     = 20;
        SplitBarDraggableSize = 20;
    }
}