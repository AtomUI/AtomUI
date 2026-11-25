# AdornerLayer 实现原理

`DefaultFocusAdorner` 模板构建 `FocusOutlineAdorner`。绘制部件在原装饰层作用域成组读取画刷、线宽和偏移，离开视觉树时释放资源和形状观察。描边按目标圆角、内外置规则与祖先裁剪范围绘制。附加、移除和层几何由 Avalonia 管理。HeaderedContentControl 的通用模板转发应用设置的标题、内容和根视觉属性。

按[主题生效测试规范](../../../../engineering/development/theme-effect-tests.md)验收受影响的呈现效果、资源作用域和生命周期。
