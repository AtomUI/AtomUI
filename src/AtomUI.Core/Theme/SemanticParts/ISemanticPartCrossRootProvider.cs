using Avalonia;

namespace AtomUI.Theme;

/// <summary>
/// 声明控件把语义部件活体承载在 owner 视觉树之外的宿主中（如独立预览窗口）。
/// Gallery 语义预览据此发现并跟随跨根宿主：宿主集合变化时触发
/// <see cref="CrossRootsChanged"/>，会话重新收集根并刷新高亮；描边 adorner 仍按目标
/// 所在窗口的 AdornerLayer 落点，显现状态（<see cref="SemanticPartPreviewState"/>）直接
/// 设在目标上，二者天然跨根。Avalonia <c>Popup</c> 宿主由语义预览自动订阅，无需实现本接口。
/// </summary>
public interface ISemanticPartCrossRootProvider
{
    /// <summary>跨根宿主集合出现/消失时触发（如预览窗口打开、关闭、表面就绪）。</summary>
    event EventHandler? CrossRootsChanged;

    /// <summary>当前存活的跨根宿主视觉根；无存活宿主时返回空集合。</summary>
    IReadOnlyList<Visual> GetCrossRoots();
}
