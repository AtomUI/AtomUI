using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

public record DialogOptions
{
    public string? Title { get; init; }
    public PathIcon? TitleIcon { get; init; }
    /// <summary>
    /// 次选项仅 Overlay Host 类型的弹窗有效
    /// </summary>
    public bool IsLightDismissEnabled { get; init; }
    public bool IsResizable { get; init; }
    public bool IsClosable { get; init; } = true;
    public bool IsMaximizable { get; init; }
    
    public double MaxHeight { get; init; } = double.PositiveInfinity;
    public double MaxWidth { get; init; } = double.PositiveInfinity;
    public double MinHeight { get; init; } = 0d;
    public double MinWidth { get; init; } = 0d;
    public double Width { get; init; } = double.NaN;
    public double Height { get; init; } = double.NaN;

    /// <summary>
    /// 次选项仅 Window Host 类型的弹窗有效
    /// </summary>
    public bool IsMinimizable { get; init; } = true;
    
    public bool IsDragMovable { get; init; }
    public bool IsFooterVisible { get; init; } = true;
    public Control? PlacementTarget { get; init; }
    public Dimension? HorizontalOffset { get; init; }
    public Dimension? VerticalOffset { get; init; }
    public DialogHostType DialogHostType { get; init; } = DialogHostType.Overlay;
    public DialogStandardButtons StandardButtons { get; init; } = DialogStandardButton.NoButton;
    public DialogStandardButton DefaultStandardButton { get; init; }
    public DialogHorizontalAnchor HorizontalStartupLocation { get; init; } = DialogHorizontalAnchor.Custom;
    public DialogVerticalAnchor VerticalStartupLocation { get; init; } = DialogVerticalAnchor.Custom;
}