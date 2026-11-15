namespace AtomUI.Desktop.Controls;

/// <summary>
/// 预览窗口切换图片时的显示策略。
/// </summary>
public enum ImageSwitchMode
{
    /// <summary>切换后立即清空显示并进入加载态（默认）。</summary>
    Immediate = 0,

    /// <summary>目标图片未加载完成前保持显示上一张已加载图片，加载完成后一次性切换。</summary>
    WaitForLoaded = 1,
}

