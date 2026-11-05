# CT13 AM1：AutoComplete / Mentions 默认协议迁移

> 使用superpowers:executing-plans单agent继续已授权CT13；现有目录/分支，不提交、不新建分支/worktree。S7已批准，CB5仅视觉待审，二者不得重复实现。

**Goal:** 将AutoComplete四项与Mentions三项现有Own迁移到DependencyDefaultsV2 rev1，严格保留原默认、类型、ID/slot和既有配置效果。

**Architecture:** 两个internal sealed Token使用现有IControlTokenDefaults和七个静态默认函数，无Own依赖边；不改任何Control/Theme/消费者或新增Own。

**Tech Stack:** 当前Generator/Core、.NET10聚焦验证、NET8 Release、当前Mac managed/NativeAOT。

**Spec:** [Own默认求值](../../../architecture/systems/theming/own-token-evaluation.md)、[输入目录](inputs.md)。

## 固定合同与审计

- AutoComplete：OptionHeight=有效Global.ControlHeight；PopupContentPadding=四边UniformlyPaddingXXS/2；MinPopupWidth=120；MaxPopupWidth=200。
- Mentions：相同前两公式，MinPopupWidth=120。没有Own间依赖，不把同值字段连接起来。
- 原名称/类型/声明顺序/资源ID/slot保留，不改舍入、单位或值域。
- AutoCompleteSearchEdit/TextArea是独立Global身份、当前无独立Own定义；本包只迁移真实两个终端，不伪造其schema。
- 原三个AutoComplete外观与Mentions的private输入、CandidateList、Popup尺寸/行数/原数据与输入保持；字体和Shared入口属后续消费者分包。
- 临时验证默认删除，净增常驻0；不使用全量/change scope。CT12代码/进度/临时源不动。

## Task 1：真实注册与旧值前证

**Files:** 临时`tests/AtomUI.Desktop.Controls.Tests/Temporary/CustomThemeAutoMentionDefaults/`。

**Interfaces:** 真实启动后Snapshot.Registry中的AutoComplete/Mentions描述符、现有资源key与实际Compiler结果。

- [x] 两注册描述符的V2协议断言先有效失败，旧默认/有效Global/Own显式0及清除为正对照。
- [x] 保存明暗7字段的格式化值、类型、key数字/名称/slot，含控件Global与既有Own配置快照；不使用attribute字符串作为协议证据。
- [x] 原输入与候选弹层保持原数据、标题、行数和约束，明暗多状态同历史Skia AA/1DIP/复位及配置/清除前证。

## Task 2：七个默认函数

**Files:** `src/AtomUI.Desktop.Controls/AutoComplete/AutoCompleteToken.cs`、`src/AtomUI.Desktop.Controls/Mentions/MentionsToken.cs`。

**Interfaces:** 声明DependencyDefaultsV2、Revision=1、sealed partial + IControlTokenDefaults；[ControlTokenDefault]静态函数接受`in ControlTokenDefaultContext context`，只读有效Global或返回原常量。

- [x] RED后替换旧整表CalculateTokenValues与空构造，保留所有字段和公式。
- [x] 原协议检查转绿，全部旧/新快照相等；实际消费者配置/0/清除及同实例输入保持。
- [x] 默认和既有配置同轨迹前后严格RGBA0；任何实际差异留证诊断，不更新基线消差。

## Task 3：专项与清理出口

- [x] 当前Mac managed/NativeAOT实际消费、两个Token成员审计、NET8 Release、正式Token/implementation/changelog同步。
- [x] 删除全部临时验证/专用工程/媒体/进程，常驻净增0，保留CT12临时验证。
- [x] 清理后精确AutoComplete与Mentions命名空间、Generator默认协议相关检查及git diff --check；原值无差异无需另批默认外观变化。
- [x] 记录作者另步审查和未覆盖项，继续字体/其他Global/Part/状态及完整CT13。

2026-10-30默认机制分包完成：[结果](input-autocomplete-mentions-defaults-results.md)。原生扩展暴露的AM2预算失败未跳过，已由AM2修复并通过同一断言；AM2具体视觉仍独立待批。
