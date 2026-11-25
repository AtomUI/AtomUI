# Ellipsis Changelog

本文记录 `AtomUI.Controls` 中 Ellipsis 共享抽象的变更。控件的产品外观由消费方定义。

## 2026-11-14

- API
  - 新增 `AtomUI.Controls.Commons.AbstractEllipsis` 和 `AtomUI.Controls.EllipsisDirection`，提供 `Start`、`End`、`Middle` 三种截断位置。默认显示一行并从末尾截断。消费方可提供展开与收起内容，并通过双向 `IsExpanded`、`ExpandedChanged` 和 `ContentTapped` 接入交互。
- Theme
  - 共享层提供文字属性与行高、操作前景的扩展点，ControlTheme 和产品 Token 由消费方定义。
- Desktop 影响
  - 本次新增类型位于 `AtomUI.Controls`，Desktop 的控件、主题和 Token 注册沿用 `f11459de0745fe02c46e134d94f39b9e35817706` 基线。
- Tests
  - `AbstractEllipsisTests` 的 11 个用例通过，覆盖默认值、三个截断方向、文本元素边界、换行后可保留的文本、操作区宽度、布局缓存与点击绑定。
