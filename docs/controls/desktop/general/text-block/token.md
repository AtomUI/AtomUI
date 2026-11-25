# TextBlock Token 设计

`Controls[HyperLinkTextBlock]` 控制 ColorLink / Hover / Active、ColorTextDisabled 和动效。默认背景保持透明以显露父级（如 DataGrid 行）的着色、悬停与选中底色；不消费 ColorBgContainer。显式实例或 Style Background 仍投射到已有 PART_Text 并绘制。`Controls[SelectableTextBlock]` 控制 SelectionBackground / SelectionForeground。普通 TextBlock 和 HighlightableTextBlock 没有独立 Token 身份，保留文字继承及显式高亮设置。

验证应覆盖实际启用状态、局部覆盖、根覆盖和独立注册身份。通用规则见[控件 Token 规范](../../../../engineering/development/control-token-guidelines.md)。
