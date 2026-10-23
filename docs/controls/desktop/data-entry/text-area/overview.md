# TextArea 桌面版设计

TextArea 是独立的多行输入控件，继承 AbstractTextInput。Lines 默认2，IsAutoSize 默认false；IsResizable 控制手动 resize。文本、选择、清除、计数、Form 与验证继续使用共享编辑逻辑。

## 表面与主题

Control identity 为 `AtomUI:TextArea`。根 Padding 作用于内层内容区，外框保持零 padding；右侧操作区、滚动条和 resize 继续拥有其布局空间。边宽、圆角由 owner 输入投射到真实表面；默认值读取本身份 Effective Global。

Lines、继承的 MinLines/MaxLines、显式 Height、自动高度与 resize 不属于 Token 状态。根几何定制不改变文本、焦点、选择或滚动所有权。

参见[实现](implementation.md)、[Token](token.md)、[Semantic Part](semantic-part.md)、[Changelog](changelog.md)及[共享输入架构](../input-control-architecture-design.md)。
