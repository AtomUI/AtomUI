# TextBox Changelog

## 2026-10-23

- 内部输入框体及实际存在的clear/reveal按钮消费公开owner的有效Global，支持内容子树和重挂载，保留原状态与标准属性优先级。

- 显式控件Global FontFamily接入根字体，保留未配置继承、清除恢复和原生属性优先级。
- EmbeddedTextBox保留组合父字体，隔离公开TextBox身份配置。

- 为基础文本输入建立独立文档入口，保持与 LineEdit 家族共享逻辑的边界。
- 内部尺寸圆角退回默认层；根/Control Effective Global 的边宽、圆角到达真实 frame，默认渲染保持。
