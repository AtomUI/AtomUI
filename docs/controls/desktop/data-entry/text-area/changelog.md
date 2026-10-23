# TextArea Changelog

## 2026-10-23

- 内部输入框体及实际存在的clear/reveal按钮消费公开owner的有效Global，支持内容子树和重挂载，保留原状态与标准属性优先级。

- 显式控件Global FontFamily接入根字体，保留未配置继承、清除恢复和原生属性优先级。

- ResizeHandleSize实际控制拖拽命中与绘制尺寸，颜色和owner LineWidth进入真实可见指示线，支持0/更新/清除。
- 指示线从被不透明模板覆盖的父绘制迁到既有末层Handle；恢复默认可见指示线已获用户明确批准，其他默认合同保持。

- 建立独立设计、实现、Own Token 和 Semantic Part 文档入口。
- 根 padding 进入原有内层内容布局，边宽/圆角接通实际 frame；保留默认表面、滚动和 resize 空间。
