# CT13 S1：Select 默认协议迁移

> 使用 `superpowers:executing-plans`，主 agent 在当前目录执行；承接既有 CT05/CT13 授权，不创建隔离环境、不提交。

**目标：** 将 Select 的 25 个历史 Own 默认声明迁移到 DependencyDefaultsV2 rev1，保留公式、值、类型、key、公开编号及 slot。

**合同：** [Own 有效求值](../../../architecture/systems/theming/own-token-evaluation.md)、[主题验收](../../../engineering/development/theming/verification.md)、[输入目录](inputs.md)。

## 范围与依赖

产品仅修改 `src/AtomUI.Desktop.Controls/Select/SelectToken.cs`。全部默认输入来自 Global；原局部变量不构成 Own 依赖。保持 MultipleItemHeight 的 -2、SM 的 +4、LG 的原值，保持普通/SM 的 Round 与 LG 的 Ceiling、原运算顺序及 PrefixIndent 的零下限。不得为迁移增设 clamp、Own 边或新字段。

Select 的标签及内部输入直接消费这些值，TreeSelect/Cascader 的组合主题也读取 Select 资源；三者均纳入默认兼容采样。OptionPadding、SelectAffixPadding 和 FixedItemMargin 的真实消费另行核定，不用本包默认迁移冒充有效编辑入口。Global 字体/Shared 入口、Semantic route、业务状态和 Custom 退出仍属后续分包。

临时验证位于 `tests/AtomUI.Desktop.Controls.Tests/Temporary/CustomThemeSelectDefaults/`，借用实际 App/Skia 及 internal token。原生驱动在系统临时目录引用当前源码；媒体只通过 evidence.py 登记到仓库外。既有未提交文件全部保留。官方 Avalonia12.1.4 不变。

## 验收步骤

- [x] S1-R：先检查已有 Select Token/Prefix/行为测试；临时 `Descriptor_Uses_V2_Revision_One` 必须因旧协议失败。另以全部25字段验证明暗旧默认/资源key/公开ID/slot、Global输入及显式 Own/0/清除，保存原始文本数据。
- [x] S1-B：原代码下按相同完整轨迹采样 Light/Dark、三尺寸/Custom、Single/Multiple/Tags、空/已选/禁用、Select popup 及实际复用者；保留受限弹层及内容。独立 AA/1DIP/复位，记录真实目标、滚动条及输入到达。
- [x] S1-I：改为 sealed partial + IControlTokenDefaults/V2 rev1，25个静态 `DefaultFor... (in ControlTokenDefaultContext context)`；移除空构造及旧 override。没有消费者产品改动。
- [x] S1-V：同一验证 GREEN，逐字段明暗默认及 metadata 与原文本一致；同轨迹旧新帧精确零差异，AA/检错/扰动复位通过；严格启用恢复的原有Light77/Dark155失败原样保留CT25，不作为完整视觉通过。当前 Mac managed/NativeAOT 实际消费与 NET8 Release 构建；成员及新增产物审查。
- [x] S1-C：更新 Select token/implementation/changelog、CSV及本结果；清理临时测试/驱动/媒体，清理后精确 Select 及受影响 TreeSelect/Cascader 验证，git diff --check。作者独立步骤自审；CT13/CT14/CT25完整家族继续开放。

## 审查关注

1. SM 高度 +4 与 LG Ceiling 不被统一公式改掉：全部默认和不同 Global 输入核对。
2. 相同数值的显式 Own 与0仍具有存在状态：Own 覆盖/清除验证。
3. 公开号与内部 slot 独立：原明暗资源 metadata 对照。
4. 共享消费者不因 Select 协议变化漂移：TreeSelect/Cascader 同轨迹实际画面。
5. 临时验证清理后仍能构建和运行既有直接 evaluator 调用：原 SelectTokenTests 与清理后精确模块验证。

Pre-flight: S1消费CT05已实现的默认协议、CT10/11既有资源机制，不新增运行时接口；无其他本轮产品任务共享写入。
