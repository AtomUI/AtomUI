# TextBox 桌面版实现

源码由 `Input/TextBox.cs`、`Input/Themes/TextBoxTheme.axaml` 和 `Input/TextBoxToken.cs` 组成。AbstractTextInput 拥有编辑扩展与模板订阅；InputControlFrame 拥有表面状态。

TextBox 计算 EffectiveCornerRadius，消费根圆角与 CompactSpace 位置/方向。模板将 Padding、BorderThickness、EffectiveCornerRadius 传入 frame，再由 PixelAlignedBorder 绘制。frame 的 SizeCornerRadius 仅提供低优先级默认值，不能覆盖 owner 的 TemplateBinding。

ContentPadding 三档默认依赖有效高度、字号/行高和线宽。模板与资源依赖由原生 style/template 生命周期管理，不新增运行时反射、程序集扫描或全局主题订阅。模板重建仍由 AbstractTextInput 释放旧编辑部件和反馈/viewport 订阅。

根表面共同边界见[共享输入架构](../input-control-architecture-design.md#81-根表面几何与预设默认值)。

### 根前景与字体消费

根 Foreground 的默认来自当前 Control 的 Effective Global ColorText，通过框体继承链到达真实文本。根 FontFamily/FontSize/FontWeight/FontStyle 的原生实例输入保留；语义 input 的专用 Style 可声明覆写，禁用及错误状态仍由对应状态样式处理。其他组合 owner 的独立子控件合同在各家族分别定义。

## 可选主题字体族

公开owner的有效Global FontFamily仅在显式配置时覆盖原继承值；未配置及清除后跟随真实InheritanceParent，保持普通Style/LocalValue优先级。根属性由ControlTheme的有限MultiBinding选择，内部只读继承观察器在分离后释放父订阅，不通过代码写最终字体。TextBox派生的asset-only EmbeddedTextBox在自身主题恢复真实组合父字体，阻止公开TextBox配置通过BasedOn泄漏到其他组合owner。

内部frame与操作按钮通过[共享输入架构](../input-control-architecture-design.md#内部资产的主题查询路线)消费真实公开owner的Global；Content附加区的逻辑路线与模板路线同属该合同，保留原生样式优先级和状态归一。
