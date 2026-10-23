# CT-08B Context 样式宿主与退役实施计划

> 承接已批准的[全局清单](tasks.md)与[品牌架构](../../../architecture/systems/theming/brand-recipes.md)，主 agent 按 executing-plans 独立执行；不创建分支/worktree，不自动提交。

**Goal:** 将 CT-08A 准备组件接到真实 Context、根应用、Provider 和独立窗口 lease，提供后续事务可复用的准备、接线、激活、恢复、提交和退役原语。

**Architecture:** Token Snapshot 保持纯值。每个 Context 独占表现状态、引用激活键和宿主登记；每个宿主/代创建独占原生图。资格用原生 Style Setter 绑定运行期 Context 的可观测激活值，不用反射路径绑定，不在模板节点上写值。应用根入口仅匹配无逻辑父级的 TopLevel，原生/Overlay Popup 沿实际 Popup 的 styling/inheritance parent 继承，不在同一父链重复安装。独立 Window 通过已有 ThemeContextLease 安装同一 Context 的独占图。普通应用 Styles 的对象和相对顺序保持。

**Boundary:** 本子项不公开品牌选择/导入 API，不把未协调的内部原语接进 Token CommitCore 或 Publish。CT-08C 才将全部 root/scope/catalog/package 入口、输入继承/重置、NoOp 身份、host stamp/重入及表现结果统一协调。B 通过不代表 CT-08 完成，不能计入完整控件家族视觉验收。

## 接线合同与审计

- 现状：Provider 拥有 scope 注册/资源释放；Window lease 拥有 bridge/variant/关闭/失败打开；Manager 拥有 Application 样式与释放。沿用这些所有者，不增加第二套根窗口发现器或全局 Visual 缓存。
- 根锚点使用可匹配派生 StyleKey 的 `Is<TopLevel>`，且要求 Parent=null，不能将根 Context 覆盖到有 Popup 逻辑父级的原生弹层。已有 `OfType<TopLevel>` 不能匹配真实 Window StyleKey，需有效反例锁定。
- 宿主挂载先完整创建/验证全部资产，再改原生 Styles；候选保持旧资格。激活可能同步通知，不当作静默提交；Commit 仅切换状态引用。后续协调器必须在同一 UI 调度段完成阶段序列。
- 收集原生 API 已部分修改再抛错的实际状态。失败清理独立尝试，返回聚合异常，不能吞掉表现清理错误。重复 Dispose 安全；外部 Style 不由本机制接管。
- 退役先失活，移除旧组，按保留组原索引重新挂载以清除模板 frame；最终卸载用只匹配宿主的临时无资源 Style 触发原生全继承树失效，再删除它，不残留托管资产、Context 绑定或普通应用样式顺序变化。此原生退役策略须以实际模板诊断和弱引用验证。
- 不改任何控件 Token、默认模板、尺寸或颜色。新增对象均为 internal 生命周期所有者，不冻结配方格式。Provider 公共 API/事件/成员结构保持；代码绑定仅针对运行期选择的非模板 Context，释放由原生 Style frame 和宿主 lease 负责。

## 验收点与任务

- [x] AC-B1：先写临时失败测试，确认现有原生 Window 未取得根 Context/资格以及 Provider 未挂载受托管资产；临时源位于所属测试项目 Temporary/ThemeStyleHosts，头部注明删除要求。
- [x] AC-B2：Context/代与宿主独占；旧资格下挂载候选无效，激活生效，恢复回旧代；Token Snapshot identity 全程不变。空集合切换可清除父代缺失的 Setter，不复制值抵消。
- [x] AC-B3：应用根、Provider、独立 Window；已有/新建 Native 和 Overlay Popup 沿实际 owner 的 Context/资格/样式链正确渲染；兄弟与重置 Context 不串色，公开子 Control identity 保持。
- [x] AC-B4：普通应用 Style/局部值优先级合同与原对象/顺序保持；同一模板节点连续多代退役无旧 Source frame，最终卸载/重挂载正确；GC 不被 Context 的 host 登记反向保留。
- [x] AC-B5：工厂失败、原生 Add/Remove 已修改后抛错、激活/恢复失败可观察，逆序清理与重复释放；跨窗口 owner 替换失败恢复旧 Context 和样式，关闭/Manager Dispose 释放。
- [x] AC-B6：脚本化实际产品 Card AXAML 专用 Part Style 验证；无品牌兼容场景 A/A、1 DIP 检错和前后同坐标零容差 Skia 图比较，查看后保留文字、删除媒体。Headless/Skia/原生结论分别报告，不能外推无闪烁。
- [x] 删除临时测试/驱动与专用产物；清理后精确验证 Core Provider/Scope/Lease/启动及 Card 相关模块，NET8 编译、diff 与链接检查。
- [x] 更新正式品牌/实现边界、B 结果和唯一进度表；作者单独自审，无独立 agent 评审；CT-08 保持进行中并列明 C 的欠项。
