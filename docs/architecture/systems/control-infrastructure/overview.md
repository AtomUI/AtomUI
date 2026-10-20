# Control 基础设施

本目录保存由共享契约实现、被多个 Control 家族消费的稳定系统架构。

- [异步加载](async-loading.md)：搜索覆盖、节点去重、timeout、取消和结果投射。
- [过滤](filtering.md)：谓词、过滤值、Selector、有效状态和候选视图边界。
- [响应式](responsive.md)：媒体断点、partial map、求值和 fallback。
- [内容展开与收起动效](content-expansion.md)：NavMenu、Collapse 和 Expander 的共用设计契约，覆盖尺寸、裁剪、方向、请求所有权与验证边界；实现证据见各控件文档。
- [状态动效初始化架构设计](state-motion-initialization.md)：Core 初始化会话、可撤销过渡抑制、模板目标登记、新控件接入、生命周期及性能验收契约；ProgressBar、Spin 等过程反馈保留原有语义。
- [状态动效初始化验证](state-motion-initialization-verification.md)：实际源码与目标架构的差异、专项原型证据、首帧与后续交互、真实页面性能及发布验收边界。
- [Feedback 堆叠基础设施](feedback-stack.md)：Message 与 Notification 共用的堆叠布局、hover 展开、生命周期计时、性能和资源释放契约。
- [使用指南](../../../guides/control-infrastructure/overview.md)
- [AtomUI.Controls.Shared 模块](../../../modules/controls-shared/overview.md)
