namespace AtomUI.Controls.Commons;

/// <summary>
/// 指示器的动画模式。
/// </summary>
public enum IndicatorAnimationMode
{
    /// <summary>基类驱动内置旋转与圆点透明度动画。</summary>
    Rotate,

    /// <summary>由子类通过 StartCustomAnimation / StopCustomAnimation 提供动画，基类仍负责门控下的启停。</summary>
    Custom
}
