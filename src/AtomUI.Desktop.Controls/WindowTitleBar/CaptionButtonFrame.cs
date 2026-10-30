using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// managed caption button 的共享几何外壳：内缩的圆角背景层加上决定布局盒的内容层。
/// 背景层只收视觉、不改变命中面，caption band 与 WindowTitleBar AddOn 按钮通过它共用同一套几何来源。
/// </summary>
internal class CaptionButtonFrame : ContentControl
{
    public static readonly StyledProperty<Thickness> BackgroundInsetProperty =
        AvaloniaProperty.Register<CaptionButtonFrame, Thickness>(nameof(BackgroundInset));

    /// <summary>
    /// 背景层相对布局盒的内缩。它只影响背景视觉，布局盒始终由内容层与 <see cref="Avalonia.Controls.TemplatedControl.Padding"/> 决定。
    /// </summary>
    public Thickness BackgroundInset
    {
        get => GetValue(BackgroundInsetProperty);
        set => SetValue(BackgroundInsetProperty, value);
    }
}
