# 弹层动效性能与原生验证闭环

用户授权：解决上一轮披露的新增分配、探针提前关闭及性能证据不可比。沿用 feature/motion 当前目录，保留124个暂存路径，不创建分支/worktree，不commit。采用systematic-debugging、TDD、executing-plans与性能/资源生命周期技能；按项目规则只保留文字记录、临时测试验证后删除。

正式功能合同沿用 [popup-motion](../../architecture/systems/motion/popup-motion.md)，上一轮证据见 [记录](../progress/2026-11-03-popup-motion-unification.md)。

完成条件：

- [x] 恢复原始原生probe：Gallery collapsed NavMenu、API open/close、3预热/10测量/450ms；取得提前关闭的直接触发链，原失败与根因修复同条件验证，不丢弃失败或增加重试掩盖。
- [x] 保留hover/失活关闭/焦点/布局/宿主与原时长合同；根据实际根因修复产品或探针，并用真实输入验证hover进入、离开和意外输入处理。
- [x] 定量拆解请求准备分配并收敛；Core动态禁用、Delay/离场、替代和任务/定义长期存活的GC验证通过；不增加每帧Token查询、布局或分配。
- [x] 相同二进制身份、固定tick/明确预热的A/A和前后成本对比；真实原生场景稳定完成，区分合成回调与显示呈现，不以回调频率宣称FPS。
- [x] 临时源码/媒体/原始数据清理，清理后精确模块验收；一次本轮整体审阅，更新正式验证文档并暂存，不提交。

排除范围：不改公开API/动效规格，不做全量回归，不增加Windows/Linux/全部组合的验收门槛，不修与上述触发链无关的告警。
