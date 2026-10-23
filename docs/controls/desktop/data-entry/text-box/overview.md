# TextBox 桌面版设计

`AtomUI.Desktop.Controls.TextBox` 是基于 Avalonia 文本编辑内核的基础输入控件，继承 AbstractTextInput 的文本、选择、清除、Form、状态和 CompactSpace 合同。内部 EmbeddedTextBox 在其基础上提供无 chrome 的组合输入。

## 主题与公开输入

Control identity 为 `AtomUI:TextBox`。Padding 的预设默认来自 ContentPadding 系列 Own Token；边宽、圆角来自本身份的 Effective Global。根几何通过模板进入真实 InputControlFrame，CompactSpace 只派生有效圆角，不改写用户 CornerRadius。

元数据默认值不等同于应用主题后的 getter。实例属性和应用样式按 Avalonia 原生优先级覆盖默认值；清除实例值后恢复主题。TextBox 没有独立 Semantic Part 注册，不提供虚构的专用 Part Style。根属性/普通样式仍是公开定制入口。

共享分层见[输入架构](../input-control-architecture-design.md)，内部实现见[implementation](implementation.md)，字段语义见[token](token.md)，变化见[changelog](changelog.md)。
