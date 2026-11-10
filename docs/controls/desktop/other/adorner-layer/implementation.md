# AdornerLayer 实现原理

DefaultFocusAdorner 模板构建 Border，读取 AdornerLayerTokenResource。焦点装饰的附加、移除、逻辑父级与层几何由 Avalonia 管理，主题不新增资源宿主或事件订阅。HeaderedContentControl 的通用模板仅转发应用设置的标题、内容和根视觉属性，无独立 Token 读链。

契约验证：`DisplayFamilyEffectTests` 的对应效果、作用域和资源生命周期用例。
