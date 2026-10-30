# 验证门槛、实施分期与迁移

入口：[总体方案](overview.md)。本文件定义批准设计后如何证明交付，不是已经执行的实施计划。

## 1. 每个场景的证据组成

| 证据 | 要求 |
| --- | --- |
| 失败基线 | 同一触发条件在缺后端/旧实现下出现可解释失败；不能以 fixture 配置错误代替 |
| 引擎证据 | Group 请求、实际条件节点/事件、片段选择及传递依赖来自本次引擎 |
| 产物证据 | 托管程序集和资源检查；NativeAOT 用结构化元数据/符号/资源证据组合，不依赖单次 strings 空结果 |
| 运行证据 | 实际目标运行时执行成功，模板、Token、Semantic、主题与语言行为正确 |
| 工具身份 | SDK、host、engine、runtime/reference/native assets 和后端内容 hash |
| 成本 | 发布耗时、增量复用、主程序与 payload 体积，在固定环境中对照 |

注册数量相同不代表类型集合相同；类型集合相同不代表资源或排序正确。
对所有重要用例同时保存“应该保留”和“应该删除”的断言，避免扩大根集合后测试仍然全绿。

## 2. 语义矩阵

下表默认同时覆盖 net8/net10、managed trim/NativeAOT；引擎固有差异单独标注并解释。

| ID | 场景 | 核心断言 |
| --- | --- | --- |
| S01 | 直接实例化 Control | 对应片段、主题与必要 Token 存在 |
| S02 | TokenResourceExtension-only / typed Identity-only | 正确 owner 保留，无全包扩张 |
| S03 | own enum 装箱资源键 / 裸整数常量 | 前者符合条件契约；后者不伪装成资源使用 |
| S04 | Semantic Style-only | 正确 owner、selector route 与部件样式可用 |
| S05 | typeof / 反射声明 / type check | 按实际引擎语义选择，不遗漏晚到达证据 |
| S06 | 仅静态方法/字段、仅签名元数据 | 类型存在时也不能误选不相关映射 |
| S07 | 片段 A 的 factory 引入 B | 同一正式闭包继续选择 B |
| S08 | 有根循环、无根循环 | 有根各注册一次；无根全部删除 |
| S09 | 多条件同片段、多 Group、空集合、同 FragmentId 不同 proxy | OR、去重、请求门控、冲突诊断与顺序正确 |
| S10 | 类型被使用但对应包入口未请求 | 不启动该 Group/provider/initializer |
| S11 | 同名跨程序集、nested/closed generic、类型转发 | 身份无歧义，正确解析定义程序集 |
| S12 | 泛型约束调用、去虚拟化、未装箱值类型 | 不将 ILLink 较宽结果冻结为 ILC 根 |
| S13 | cctor、generic cctor、跨程序集/helper/cctor 链 | 不被预初始化吞掉；副作用及异常顺序保持 |
| S14 | 平台/特性开关裁掉的路径 | 片段 guard 前不创建 descriptor/factory，未用平台工厂消失 |
| S15 | scanner on/off × 优化/Size/Speed/非优化、调试发布 | 包含非优化+scanner 与优化+noscan；不靠切换配置绕开 |
| S16 | 未识别元数据、重复 key、错身份、缺方法/工具 | 明确失败且没有成功回执 |
| S17 | 多目标资源字典、显式 include、命名主题 | 不可分割资产正确闭包，独立未用资产删除 |
| S18 | 抛异常片段、注册失败、重复/重入、跨包冻结 | full/selected/net10 对照现有事务/异常/资源清理契约 |
| S19 | net10 应用消费 net8-only 新格式包 | 前置桥接后仍由官方引擎精细选择；无静默 full collector |

Token/Style 原型使用的是替代条件类型，不能替代 S02–S04 的真实生成 API 验收。
泛型/动态路径不能简单跳过或标记 flaky；无法满足的必需行会阻止完整支持声明。

## 3. 真实产品矩阵

| 场景 | 运行与删除检查 |
| --- | --- |
| Minimal Button/Window | 窗口/标题栏/按钮模板正常，未用 DatePicker/DataGrid 等及其独立 factory 缺失 |
| DateViewer/DatePicker | Calendar、范围、嵌套 Popup、时间面板和必要语言资源可用；未用同目录资产不自动加入 |
| Token-only / Style-only | 无直接 Control 构造时资源和语义部件仍正确；专用 Style 在 AXAML 命中 |
| DataGrid/ColorPicker/Extras 可选包 | 启用时闭包正确，未启用包无 provider/initializer 副作用 |
| 应用级主题替换 | 默认 type-key 替换仍生效，局部 ResourceScope 不串用 |
| 动态主题/语言、注册冻结 | 运行时修改保持原有行为，不靠 late registration 修复漏项 |
| Host DynamicResource | 合法宿主输入可用，未使用私有同名资源不改变结果 |
| Complex 与 Gallery | 覆盖真实资源组合和启动；Gallery 不能代替 Minimal 的未用项检查 |

资源保留循环与资源构造 include 循环不同：前者由引擎收敛，后者仍应在普通资源编译中报错。
普通/full 与 selected 对照必须使用相同控件配置、资源输入、字体、全球化和功能开关。

## 4. 消费、构建和平台矩阵

- source、直接/传递 ProjectReference、预编译 adapter DLL、独立第三方控件包、干净 NuGet cache；包含 net10 应用消费 net8-only 包及混合 ABI 图。
- 原包正常重建、新旧格式混用的明确拒绝、同身份不同内容/多份后端冲突。
- 库 Debug/Release TFM、消费应用 Debug/Release、normal/trim/NativeAOT 不同组合。
- 已声明的 Windows/macOS/Linux host-target/RID；Browser net10 两条现有路径需防回归。
- 第一次冷恢复、离线锁定缓存、no-op、改候选/AXAML/引用 DLL/feature/工具/native helper 后重新计算。
- 并行 net8/net10、并行 RID、取消/失败重试、目录迁移、同名项目路径不同，均不能复用错误产物。
- 包矩阵、nuspec 依赖组、程序集 TFM、工具白名单、无工具泄漏至运行时输出。

平台缺口按具体单元报告，不能把本机成功写成跨平台完成，也不静默从总体承诺中删去失败行。

## 5. 体积与性能

沿用[当前体积判定规范](../../../architecture/foundations/aot-and-trimming.md#81-体积评判标准)：固定应用、环境、字体、配置、
runtime/RID 和功能后，记录 selected/full 与相应框架基线、可归因增量和历史棘轮。
net8 与 net10 可有不同框架基线；不能直接用两个绝对文件大小判定后端能力相同。

即使总体更小，只要未用控制片段被错误保留仍算失败；即使总体变大，也必须区分 BCL/原生依赖变化和后端根集合错误。
新后端分别记录冷构建、热构建和 no-op 耗时，不用缓存命中掩盖首次发布成本。

## 6. 里程碑及退出条件

| 阶段 | 交付 | 必须通过才进入下一阶段 |
| --- | --- | --- |
| P0 方案与基线 | 本方案、现有 net10 生成/行为基线、失败的 net8 publish 证据 | 用户 Review 确认目标与工具链维护范围 |
| P1 后端机制完整性 | 定版后端、native 无 scanner 路径、预初始化、准确条件通知、宿主能力 | S05–S16 全部关键机制；官方 net8 runtime/CoreLib 不变；完整模式可解释 |
| P2 真实生成 ABI | package/fragment/condition 记录、真实 builder 参数、跨包/泛型/去重、net10 前置桥接 | S01–S13、S19 与 source/预编译/NuGet 消费 |
| P3 产品资源闭包 | 当前注册、主题、Token、Semantic、AXAML 与生命周期接入 | 全部真实产品场景及删除断言 |
| P4 工具与增量交付 | 工具白名单、能力清单、锁/快照/回执、宿主/RID 包装 | 冷恢复、并发、servicing、失败完整性与平台矩阵 |
| P5 发布准备 | 正确双 TFM 包、迁移说明、最终验证证据 | 所有适用矩阵关闭，再执行正式 release 流程 |

原型结果只关闭 P1 的部分可行性问题，不等于 P1 整体通过。
每阶段应产生独立可审查的小改动；阶段间不自行提交、打标签或发布，遵循仓库授权规则。

## 7. 当前工作树与迁移

当前双 TFM 草稿中的配置修正、工具目标隔离和普通 net8 编译兼容可作为输入候选。
其中“缺 TypeMap 时直接全量注册”的生成分支不能作为最终裁剪实现；应由本方案的受控 full/selected 接线替代。
当前依赖 csproj 字符串识别产品包的临时门禁应迁移为显式 PackageRole。

实施期顺序：冻结 net10 输出/行为 → 新 ABI 与后端独立接入 → 真实 net8 消费验证 → 旧临时分支清理 → 全部模式/包验证。
未验收中间状态不能作为公共 NuGet 版本发布。已发布 6.2.2/6.2.3 的内容和迁移记录保持事实，不移动标签或覆盖包。
修复版本号在发布准备时确定，并同步 README、Changelog、包布局/API 审计和完整迁移说明。

完成后将已验收契约分别归并进现有架构/生成器/构建/ABI 正式所有者文档；本目录保留为带日期的设计与决策记录。
当前实现文档不得依赖未批准方案才能成立，也不把原型的具体 SDK/tag 写成长期不变规则。

## 8. 测试生命周期

沿用[测试价值与生命周期](../../../engineering/development/test-value-and-lifecycle.md)。具体排障/原型测试默认临时，验证后清理。
通用后端协议、实际引擎条件闭包、跨框架 ABI/构建完整性具有长期保障需求，应优先复用现有 fixture/专项入口。

新增永久测试必须证明独立检错和执行成本；重型真实编译/发布矩阵放入有明确触发条件的专项验证，不能每次控件局部修改都运行。
普通修复使用明确模块路径或精确过滤器；全量回归仍只在明确请求或 release 时执行。清理后的最终工作树重新验证相应模块。

## 9. 风险与处置

| 风险 | 控制与停止条件 |
| --- | --- |
| 无 scanner 原生表接线未证明 | P1 独立完成；失败则重回架构评审，不删验收行 |
| 内部编译器 ABI servicing 变化 | 定版补丁、来源/hash、差异审查与矩阵，不按版本大小放行 |
| metadata 自我 rooting | 字符串记录、Mark 前隔离、全未用/无根循环反例 |
| 预初始化吞请求 | 两阶段解释视图屏障，直接/间接/generic/cross-assembly cctor 证据 |
| 阶段后引入未知依赖 | 标记完成断言、原生产物/程序集重读、严格残留检查 |
| net10 消费 net8-only 包绕过条件选择 | P2 前置 ABI 桥接；独立冷 NuGet 与 Browser/NativeAOT 正反例，未验证不得声明支持 |
| 冷恢复或工具包膨胀 | 初版内聚交付、记录包体积，拆包必须另有冷恢复证明 |
| 共享输出误复用 | bin/obj/中间物/最终产物均按输入域隔离，回执核对实际消费 |
| 原型成功被扩大为产品承诺 | 独立记录机制、产品、平台和发布四类完成状态 |

## 10. Review 需要确认的决策

- 采用共同事实模型与 net8 定版引擎适配，维护成本进入正常发布职责。
- CoreLib/官方目标运行时不修改为设计约束；无法达成时返回评审，不悄悄扩大工具链 fork。
- 无 scanner 等完整模式属于硬门槛，不能用全量注册或优化配置替代。
- 接受初版托管工具内聚分发及可量化下载成本，后续拆包另审。
- 将 net10 消费 net8-only 包的前置 ABI 桥接纳入必需范围，保留官方引擎的选择所有权。
- 按 P1→P5 的证据门槛推进，先批准架构，再形成逐项实施计划。
