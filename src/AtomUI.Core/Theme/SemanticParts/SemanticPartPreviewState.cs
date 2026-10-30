using Avalonia;

namespace AtomUI.Theme;

/// <summary>
/// 语义部件预览状态契约。Gallery 语义预览在激活 <c>RestHidden</c> 部件（静止态透明/隐藏
/// 是设计语义的部件，如 ImagePreviewer 的 cover 悬停遮罩）时，对解析出的目标节点设置
/// <see cref="IsPreviewTargetProperty"/>；各控件 ControlTheme 以属性条件选择器响应，
/// 自行决定如何显现目标本体（如把悬停遮罩的 Opacity 提到 1）。
/// </summary>
/// <remarks>
/// 契约位于 AtomUI.Core：写入方（GalleryBase 高亮会话）与读取方（控件 ControlTheme）分属
/// 不同程序集，依赖方向要求共享契约下沉。伪类（PseudoClasses）是 protected，外部无法设置，
/// 故以附加属性表达。该状态只影响预览观感，不改变控件运行时行为。
/// </remarks>
public sealed class SemanticPartPreviewState
{
    private SemanticPartPreviewState()
    {
    }

    /// <summary>标识该节点是当前激活语义部件的预览目标。</summary>
    public static readonly AttachedProperty<bool> IsPreviewTargetProperty =
        AvaloniaProperty.RegisterAttached<SemanticPartPreviewState, Visual, bool>(
            "IsPreviewTarget");

    /// <summary>读取节点的预览目标状态。</summary>
    public static bool GetIsPreviewTarget(Visual visual)
    {
        ArgumentNullException.ThrowIfNull(visual);
        return visual.GetValue(IsPreviewTargetProperty);
    }

    /// <summary>设置节点的预览目标状态。</summary>
    public static void SetIsPreviewTarget(Visual visual, bool value)
    {
        ArgumentNullException.ThrowIfNull(visual);
        visual.SetValue(IsPreviewTargetProperty, value);
    }
}
