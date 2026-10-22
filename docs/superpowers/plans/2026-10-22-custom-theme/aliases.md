# Own Token别名与DataGrid兼容实施计划

> **For agentic workers:** 使用superpowers:executing-plans由主agent按顺序实施；用户明确要求全部工作不使用subagent，包括最终审查。总状态只更新[tasks.md](tasks.md#0-执行进度)。

**Goal:** 完成CT-06：每层配置先归一别名，保留历史键，规范名与旧名到同一有效值及真实DataGrid消费者。

**Architecture:** 显式属性声明由Generator验证并生成不可变descriptor别名表；动态配置及定义在各自绑定边界使用同一规范语义，合并只处理规范字段。Compiler求值后将规范值投影到历史字段，再冻结现有snapshot与资源键。

**Tech Stack:** 现有Core schema/Normalizer/Compiler、Roslyn、XML v1及compiled ABI v1、DataGrid、既有Skia脚本化宿主。

**Spec:** [Own设计§6](../../../architecture/systems/theming/own-token-evaluation.md#6-别名规范化与兼容)。

## Global Constraints

- 使用当前feature/custom-theme及worktree；不创建分支/隔离副本，不提交。
- 所有实现、验证、文档和审查由主agent完成，不使用subagent。
- 全部Own字段、原slot、resource enum、typed key与internal历史字段继续导出；只显式定义17项DataGrid关系，不推断所有Table字段。
- XML v1和compiled ABI v1保留真实字段名称、字面值及绑定声明，不增加别名语法或修改布局；当前schema的别名语义在每个独立定义层绑定时生效。编译定义继续延迟到所选Control绑定，不为别名提前计算所有Control。
- WholeV1仍先默认后显式；V2仍先显式后默认；两者最后才投影历史字段。默认内部副本不作为显式配置参与冲突。
- 动态配置冲突返回diagnostic且不得发布；XML绑定冲突返回源位置diagnostic；compiled绑定冲突在当前Control准备失败并保留重试语义。
- 临时验证默认删除；最终精确filter验证，禁止自行full/change。真实视觉基线在改动DataGrid声明之前采集。

## Review Focus

| 输入/故障 | 预期与验证步骤 |
| --- | --- |
| 旧名/规范名同层不同值或不同文本同值 | typed canonical formatter比较；不同值拒绝，同值一个规范slot；A |
| 父层旧名/子层规范名及反向、Inherit=false | 先每层归一再合并；高层覆盖和重置；A |
| 旧Table*字段/键、默认副本及V2默认依赖 | slot/key保留，最终投影；别名不独立求默认且不允许用它作依赖；A/B |
| 未查询Control及跨包声明 | 表构建不执行用户Token构造/静态初始化；终端接口使旧Generator不能静默忽略；B |
| 默认视觉和已支持旧配置 | 同条件A/A与前后比较，1DIP对照可检错；canonical新增能力另验，原UX不削减；C |

## Task CT-06A：运行schema与每层配置规范化

**Files:** 新增`src/AtomUI.Core/Theme/Schema/ControlTokenAlias.cs`、`Configuration/ControlTokenAliasNormalizer.cs`；修改`ControlTokenDescriptor.cs`、`ThemeConfigNormalizer.cs`、`Definitions/ThemeDefinitionBinder.cs`、`Definitions/Compiled/CompiledThemeValueWriter.cs`、`Compilation/ThemeCompiler.cs`。验证放`tests/AtomUI.Core.Tests/Temporary/TokenAliases/`。

**Interfaces:** `ControlTokenAlias(string name, string canonicalName)`只读名称对。descriptor保留旧构造，新增WholeV1六参重载(最后`IEnumerable<ControlTokenAlias> aliases`)及V2七参重载(最后revision、aliases)；公开只读`OwnTokenAliases`，内部`GetCanonicalOwnToken(TokenDescriptor)`与`ApplyAliases(AbstractControlDesignToken)`。拒绝未知/自身/重复/链/环/类型不相同及非Own目标；Bind保留表。

- [x] **Step 1：临时RED。** 真实schema测试别名表验证及Bind；Normalizer同层7/7.0合一、7/8冲突、未知不当别名；父旧7子规范9及反向均9；reset回默认。新API不存在先记合同缺失，接口可编译后必须执行数值断言。
- [x] **Step 2：实现表与动态层规范化。** 一对一直接规范目标，多别名可指同一规范字段；同值比较使用规范descriptor格式化值；保留原名/路径用于诊断ATMTHM4008。无alias的既有配置避免新增映射分配。
- [x] **Step 3：真实Compiler验证。** V1/V2最终canonical及alias slot值相同，0/null保留，旧字段消费者继续取原key。未配置默认值保持；Bind/等价配置缓存和未查询构造数保持。先运行失败再接投影。
- [x] **Step 4：定义层RED→GREEN。** XML binder使用ATMTHM3004及原位置返回冲突；compiled writer在ControlThemeDefinition之前归一自己的raw声明列表，同层冲突失败。各层独立归一，不在merge之后修补；保持compiled按Control惰性。
- [x] **Step 5：GREEN与记录。** 精确filter Temporary.TokenAliases、ThemeConfigNormalizerTests、ThemeConfigMergerTests、DenseThemeCompilerTests、ThemeDefinitionBinderTests及对应compiled既有用例。同步正式合同并记录实际数值，不宣布DataGrid已修复。

## Task CT-06B：声明与生成

**Files:** 新增Core `DesignTokens/ControlTokenAliasAttribute.cs`、`IControlTokenAliases.cs`；新增Generator `DesignToken/ControlTokenAliasModel.cs`、`ControlTokenAliasesWriter.cs`；修改同目录Walker/TokenInfo/DefaultModel、TokenResourceKeyGenerator、GeneratedThemeSchemaWriter、集中diagnostics及正式generator文档。临时验证在Generator所属Temporary/TokenAliases。

**Interfaces:** `[ControlTokenAlias(nameof(CanonicalField))]`只用于历史属性。终端partial类型实现生成专用`IControlTokenAliases`静态入口以拒绝不认识别名的旧Generator；descriptor factory直接引用中立生成表，不能为读取表触发Token静态初始化。全部字段继续按名称分配slot。负例集中新诊断ATOMUIGEN044。

- [x] **Step 1：真实Emit RED。** 在小Token两字段上声明别名，生成descriptor实际构造并观察表和数值；声明顺序不改slot；跨包继承仍保留映射；去掉生成器导致接口缺实现编译失败。
- [x] **Step 2：负例与最小实现。** 缺目标、非Own目标、自身/链/环、类型/nullability不符、重复/无效声明、终端缺partial或接口均诊断；V2别名不需要默认声明，拒绝别名独立默认和作为Own输入。中立生成表不触发用户静态构造器。
- [x] **Step 3：GREEN。** 运行临时及既有schema/resource/default generator精确用例；保留XML/compiled原字段而不扩展格式，实测当前schema按层归一。同步正式诊断/生成文档。

## Task CT-06C：17项DataGrid与真实视觉

**Files:** `src/AtomUI.Desktop.Controls.DataGrid/DataGridToken.cs`、真实消费者及必要DataGrid正式token/implementation/changelog文档；临时渲染场景使用所属现有fixture，独立脚本记录文本。

- [x] **Step 1：改动前取证。** 读取control优化/文档及scripted-visual skill；默认明暗/密度和已支持Table配置在相同Skia环境截图，A/A为0且1DIP对照被检出；保留原尺寸/滚动/冻结条件。
- [x] **Step 2：声明17对映射。** 按源码已确认名单添加Attribute及生成接口，不删除其余8个未决内部副本或改成V2；默认计算公式与AXAML不动。
- [x] **Step 3：真实消费。** HeaderBg/HeaderColor/CellPadding等canonical入口与旧入口分别更新真实Header/Cell及适用状态、filter/footer/row等目标；同层冲突拒绝、跨层覆盖。17对都有实际字段/消费者结论，不能只比较Token对象。
- [x] **Step 4：视觉兼容。** 默认及已支持旧配置前后同条件像素/布局/交互比较；新canonical输入达到目标结果。未知差异为0才通过，渲染Host边界与原生最终显示面区别记录。
- [x] **Step 5：清理与最终验证。** 删除临时测试、驱动和媒体，保留文本结果；最终只验证修改模块及diff检查。按独立自审步骤核对合同、消费者与保留边界，更新总表再进入CT-07。不自行提交。
