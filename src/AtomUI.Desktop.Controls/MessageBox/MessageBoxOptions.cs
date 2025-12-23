using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

public record MessageBoxOptions
{
    public DialogHostType HostType { get; init; } = DialogHostType.Overlay;
    public string? Title { get; init; }
    public PathIcon? Icon { get; init; }
    public MessageBoxStyle  Style { get; init; } = MessageBoxStyle.Information;
    public bool IsModal { get; init; } = true;
    public bool IsDragMovable { get; init; }
    public Control? PlacementTarget { get; init; }
    public Dimension? HorizontalOffset { get; init; }
    public Dimension? VerticalOffset { get; init; }
    public bool IsCenterOnStartup { get; init; } = true;
    /// <summary>
    /// 次选项仅 Overlay Host 类型的弹窗有效
    /// </summary>
    public bool IsLightDismissEnabled { get; init; }
    
    public bool IsLoading { get; init; }
    public bool IsConfirmLoading { get; init; }
    
    public double MaxHeight { get; init; } = double.PositiveInfinity;
    public double MaxWidth { get; init; } = double.PositiveInfinity;
    public double MinHeight { get; init; } = 0d;
    public double MinWidth { get; init; } = 0d;
    public double Width { get; init; } = double.NaN;
    public double Height { get; init; } = double.NaN;
}