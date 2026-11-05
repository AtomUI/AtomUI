# CT13 S7：选择输入 add-on 的有效 Global 消费

> 执行：按 superpowers:executing-plans 单 agent 完成；用户已授权继续研发，不创建分支/worktree，不提交，不使用子 agent。

**Goal:** 验证并修复 Select、TreeSelect、Cascader 内部 add-on 状态表面对公开 owner 的有效 Global 配置的消费。

**Architecture:** 保留现有模板、状态选择器及 Avalonia 优先级，只修正经实际反例证明错误的资源来源。三个内部 add-on 没有独立 identity，分别使用其公开 owner 已有的 typed TokenResource；不新建属性、Token、缓存或订阅。

**Tech Stack:** 当前 Avalonia、AXAML、.NET10 聚焦测试、Release NET8、当前 Mac managed/NativeAOT。

**Spec:** [输入执行目录](inputs.md)、[消费合同](../../../engineering/development/theming/consumption.md)。

## 全局约束与当前审计

- 2026-10-29 用户确认 CT12 原生采样结束，可以恢复构建测试；不修改 CT12 源码、记录、临时验证及 session-current.json。
- Cascader CHT 的70对限定视觉已在本会话明确批准；不重复实现/取证，不扩大批准范围。
- CT13 目录91条metadata（29 ProductControl/55 InternalTarget/7 AbstractContract），93条Own字段记录；这些数量不是已验收控件数。Cascader/TreeSelect 还需与 CT14 按已有分包协同。
- R/F1/G1/V1/F2/G2、OTP A/B1/B2A/B2B/C/D、CR1–CR4、CB1–CB4、NU1/NU2A/NU2B/NU3、Select S1–S6、SC13默认协议和CHT已有独立结果；不重复这些分包。
- CSV仍有历史 SourceChecked/NeedsDecision 或待审文字，必须以实际结果及批准核对，不能据旧单元格重做。
- 本轮静态候选：三个 add-on 主题 Shared 读取23/23/23共69处；是否存在可见断链需真实实例RED，引用数量本身不是缺陷证明。ComboBox五处Shared、CascaderView其余基础资源和标签身份另包，不混改。
- CheckBoxItemsControl 两处Shared间距已有Group模板绑定；Check/Radio指示器时长走已验收动态Owner机制，不以字符串残留重新迁移。
- CT13其余真实范围还含AutoComplete/Mentions/Form/Upload的4/3/8/13字段（仍V1源码），不能把常见输入控件切片当作完整CT13。

## 审查重点与验收契约

| AC | 来源/条件 | 操作与独立预期 | 判据 |
| --- | --- | --- | --- |
| AC1 owner隔离 | TH02/05；三owner同scope、四variant | 各自配置不同ColorBorder/状态色/背景；目标用自身值，兄弟不串值 | 真实AddOn/Frame属性与像素；先RED |
| AC2 更新/清除 | TH09；同一实例 | 两次颜色、透明0、清除，原选择/焦点/几何保持 | 当前值与独立默认，实例身份保持 |
| AC3 状态 | 原状态机合同 | normal/error/warning、hover/focus/open/disabled及Filled/Outlined | 真输入和公开状态切换；不可用事件伪造成功 |
| AC4 优先级 | 既有root/Part合同 | 根本地画刷/Style及生成专用Style覆盖、撤回 | AXAML专用Style命中，恢复当前资源；无代码节点定制 |
| AC5 默认兼容 | TH12；原主题/相同完整历史 | 明暗、四尺寸/四variant采集前后，AA/1DIP/严格恢复 | RGBA严格0；配置变化另交具体审查 |
| AC6 生命周期/AOT | 资源绑定目标仍为既有Visual | 换scope/重挂/重模板及GC；当前Mac实际消费 | 无全局订阅/新owner缓存；managed/AOT真实运行 |

## Task 1：有效反例与原版轨迹

**Files:** 临时 `tests/AtomUI.Desktop.Controls.Tests/Temporary/CustomThemeSelectAddOn/`。

**Interfaces:** 消费现有ThemeConfigProvider、ControlThemeConfigBuilder.WithToken、三公开owner及真实AddOnDecoratedBox；产出可失败的消费者与完整前证。

- [x] 以各owner ColorBorder、Filled背景、Error/Warning及hover为独立反例；先排除根本地画刷/测试构造覆盖。
- [x] 精确运行 `dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.CustomThemeSelectAddOn`，记录有效失败与正对照。
- [x] 使用项目外登记媒体完成原版同历史、AA及1DIP检错；仅标题扰动；若复用主实例，必须先证明扰动复位严格0再继续两侧相同主轨迹。

## Task 2：最小主题修复

**Files:** `Select/Themes/SelectAddOnDecoratedBoxTheme.axaml`、`TreeSelect/Themes/TreeSelectAddOnDecoratedBoxTheme.axaml`、`Cascader/Themes/CascaderAddOnDecoratedBoxTheme.axaml`（均位于src/AtomUI.Desktop.Controls）。

**Interfaces:** 仅把原SharedTokenResource改成对应Select/TreeSelect/CascaderTokenResource；原Global key、选择器、绑定优先级及节点保持。

- [x] 有效RED之后修复真实断链；不修改跨owner Select Own复用或其他未验证分支。
- [x] Task1原精确filter转绿；补0/清除/owner隔离、根画刷和编译专用Style命中。
- [x] 相同历史默认新旧/恢复严格比较，真实输入保持，记录实际配置像素；不设通用容差。

## Task 3：专项、清理与本包出口

- [x] 当前Mac managed/NativeAOT实跑相同资源/状态消费者，NET8 Release构建。
- [x] 同步三控件implementation/token/changelog；不改API、Own/资源ID或Gallery调用形态。
- [x] 查看关键媒体，记录数值/观察/所有失败；删除本包临时测试/辅助源/工程/媒体/进程，保留CT12文件。
- [x] 清理后精确验证SelectControl/TreeSelectControl/Cascader三个命名空间与git diff --check。
- [x] 作者另步审查；具体配置视觉如有变化请求限定批准，不关闭完整CT13。

## 后续顺序（本包不代替）

1. 三选择及ComboBox剩余Global/公开子Control身份、标签与候选项所有Active Own消费者。
2. 缺失字体链、专用Semantic Part Style逐route及可选/生成节点、优先级和恢复。
3. AutoComplete/Mentions、Form、Upload的默认协议与实际消费；按独立责任拆包，先核对Reserved/Legacy而不虚构功能。
4. 输入、选择、校验、禁用/只读、打开弹层、滚动、换源/回收/重模板/GC的最小充分状态矩阵。
5. 合并既有分包证据、清理、精确模块与家族出口；CT25原始像素/性能/其他平台问题继续独立保留。

2026-10-29本机分包出口通过：[结果](input-addon-owner-results.md)、[限定视觉批准](input-addon-owner-visual-review.md)。后续顺序仍为未完成任务，不随本包勾选关闭。
