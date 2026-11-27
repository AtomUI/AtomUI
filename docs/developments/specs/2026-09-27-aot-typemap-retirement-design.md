# TypeMap 替换：旧机制删除与交付门槛

> 过程记录：本轮结论已同步到 [正式 AOT 架构](../../architecture/foundations/aot-and-trimming.md)。本文保留方案比较、实验或迁移过程，不再作为平行的长期规范；事实以正式文档为准。


> 配套 [总体设计](2026-09-27-aot-typemap-replacement-design.md)。这是待实施的删除清单，不是本轮已删除的文件。

## 1. 切换方式

最终交付不保留 legacy backend、Sidecar fallback 或用户可切回旧 pipeline 的开关。
实现过程中允许在隔离工作区使用受控原型进行对照；生产切换必须在新路径通过矩阵后与旧代码删除一起交付。
不能先删旧实现、再把浏览器或第三方问题留给后续版本。

正式实现基于干净工作区整理；之前 `TypeMapProbe`、手写 App target 和单程序集 custom-step 原型不能直接作为生产补丁。

## 2. 新增或重构的源码边界

| 边界 | 职责 |
| --- | --- |
| `AtomUI.Core/Registration` | 中性 package builder、片段 Attribute、包注册状态、map 收集、共同提交 |
| `AtomUI.Generator/Registration` | ControlContract/ThemeAsset 模型、片段/TypeMap/marker、应用 marker bootstrap |
| `AtomUI.Generator/DesignToken` | owner-bearing identity、资源键与生成 API |
| `AtomUI.Generator/SemanticParts` | 单 Control factory、专用 Style 条件 |
| `AtomUI.Generator/ThemeAssets` | 导出主题/词法资源/显式 include/factory；无应用 usage |
| `AtomUI.TypeMap.Linker`（新构建项目） | 仅 Browser/Mono 的已选映射转换与验证 |
| `build/AtomUI.Registration.targets`（新） | 后端能力/feature switch/增量输入/验证，无旧 linked context |

Common、Desktop、DataGrid、ColorPicker、Extras、GalleryBase 全部使用同一生成事实模型和简化入口。
Common 的 `UseImageLoading`、SvgImageCodec 和语言保持明确的 Package Core 顺序，不能因删除注册项目而丢失。

## 3. 删除项目、分析与协议

整体删除 `src/AtomUI.Generator.LinkedPublish/`。

从 `src/AtomUI.Generator/LinkedRegistration/` 删除：

- ApplicationRegistrationPlanGenerator、LinkedRegistrationUsageGenerator。
- LinkedAxamlUsageInput、LinkedRegistrationManifestCatalog。
- Model/ApplicationPackagePlan、LinkedUsageInfo、RegistrationUnitDependencyAnalyzer。
- Writers/ApplicationRegistrationPlanWriter。
- Manifest/ 中旧 codec、metadata writer 与模型。
- LinkedRegistrationProtocol、ControlPackageRegistrationEntryDiscovery。

最终模型不再需要 RegistrationUnitId/Granularity；也删除其目录推导。稳定类型 metadata name、合法标识符/哈希等通用函数迁到
普通生成辅助代码，仅保留新模型真正使用的部分，不保留整个旧 options 类。

从 `AtomUI.Build.Tasks` 删除：

- `LinkedRegistration/` 的 Sidecar、budget、consumer IL extractor。
- CollectAxamlUsageTask。
- GenerateLinkedRegistrationSidecarTask。
- ResolveLinkedRegistrationSidecarCandidatesTask。
- DiscoverLinkedRegistrationConsumerReferencesTask。
- ValidateAssemblyMetadataMarkerTask（旧 Plan marker 用途）。

从 Core 删除 AotTrimRegistrationPlanRegistry、旧 AotTrimRegistration 模式 ABI、ControlPackageRegistrationEntryAttribute、
AotTrimUnitAttribute。旧 builder 的有用收集能力迁入中性实现，不能保留一个空壳旧 Plan 协议。

删除生成物协议：`AtomUI.Linked.*.v1`、`.LinkedRegistrationV1` 的跨程序集计划 ABI、GeneratedApplicationRegistrationPlan、
其 ModuleInitializer、`.atomui-link.json`、AXAML usage XML、MVID/hash 来源合并与旧 DLL 入口恢复。

## 4. 构建、打包与脚本

删除三个 `build/AtomUI.LinkedRegistration*.props/targets` 文件。
移除旧 properties/items/metadata：

```text
AtomUILinkedPublish
AtomUIRegistrationStrict
AtomUIRegistrationPlanOwner
AtomUIEmitLinkedManifest
AtomUILinkedPublishGeneratorAssembly
AtomUIAxamlUsageOutputPath
AtomUILinkedSidecarOutputPath
AtomUIPackSidecarInnerBuild
AtomUIRegistrationUnitRoot / AtomUIPackageRoot
AtomUIRegistrationGranularity / AtomUIRegistrationUnit
AtomUILinkedContextApplied
AtomUIAxamlUsage / AtomUILinkedSidecar / sidecar candidate/source
AtomUI.AotTrimRegistration.Enabled
```

PackageShared 等旧 item 不机械改名保留：纯辅助资产改用正常显式 include，真正公共包资源由 Provider/core 正常引用。

必须更新：

- `build/AtomUI.Repository.props/.targets` 的项目/工具白名单和包判断。
- `AtomUI.Generator.props/.targets`、`AtomUI.GeneratorConsumer.targets`。
- Generator csproj 的 linked Compile Remove 清单、linked pack build target。
- Build.Tasks csproj 的旧协议 linked source；Process 适配器与 TaskProcessHost 的旧任务分发。
- `AtomUI.slnx`、`scripts/NuGetPackageProjects.ps1`、`scripts/BuildNuGetPackages.ps1`。
- Gallery Desktop `PublishToLocal.ps1` 中强制 AtomUILinkedPublish 参数。
- `scripts/verification/test-policy.json`、AOT 注册验证脚本与包布局检查。
- `build/ProjectDefaults.props` 取消 net8 生产目标，统一新支持矩阵；保留 Browser TFM。

NuGet 删除 linked analyzer、Sidecar 和旧 targets；保留普通 Generator、资源 wrapper 工具、Localization 工具和 buildTransitive
自动接入。加入 Browser linker 工具，确保不进入 lib/、runtime deps、应用输出或发布目录。
不同产品包携带的同版本构建工具必须一致，幂等注入一次。

## 5. 明确保留的有效设施

- GenerateThemeAssetWrappersTask 和 deferred wrappers。
- Build Tasks 进程隔离、协议宿主与 Localization/语言包工具。
- ThemeSchema/Token/Semantic 生成能力和 schema fingerprint 校验。
- AotDataMemberPathAnalyzer、generated data accessors、有效 trimming/AOT annotations。
- 原生平台发布、字体/图标资源和普通 NuGet 分发能力。

“删除旧 AOT 方案”在此精确指旧的控件注册分析体系，不能把其他解决真实 AOT 问题的设施一并删除。

## 6. 测试与隐藏消费者

旧 ApplicationPlan、Usage、Sidecar codec/catalog、GraphPlanner、旧诊断协议测试随实现退役。
原 builder/包入口/build assets/publish fixtures 改写为新行为覆盖，不简单删除测试。

必须处理的隐藏引用：

- Localization IntegrationTests 的 OfficialPtBrLanguagePackTests 显式 restore linked 项目。
- LanguagePackEndToEndTests 注入旧 generator assembly 属性。
- GalleryBasePackagingTests 断言 LinkedPublish 工具。
- Generator BuildLayoutTests 锁定旧 assets、pack target；Generator Tests 的 LinkedPublish alias reference。
- verify-build-task-isolation.ps1 目前使用 CollectAxamlUsageTask 验证宿主，应改用保留任务。
- TaskProcessHostTests 当前借旧 resolver/marker 任务验证 wire，需替换而非取消隔离测试。
- Core/Localization 对旧 fixture 名称的 InternalsVisibleTo。

新 fixture host 必须真实执行 UseAtomUI/ThemeManager 初始化、模板、Token、Semantic Part 与主题切换。
不能沿用只 new AtomUIBuilder 再输出排序快照的方式；Package Core 顺序必须记录原始事件顺序。

## 7. 验收矩阵

| 维度 | 必须覆盖 |
| --- | --- |
| 构建模式 | Debug、非裁剪 Release、trimmed CoreCLR、Desktop NativeAOT、Browser trimmed、Browser AOT |
| 消费方式 | 同程序集、直接/传递 ProjectReference、普通预编译 adapter DLL、干净 NuGet cache |
| 第三方 | 只依赖产品包就自动获得工具；作者与用户不写 TypeMap/Unit/root 列表 |
| 类型入口 | Control-only、Token-only、Identity-only、Semantic Style-only、internal presenter、非泛型基类承载泛型控件 |
| 资源 | named/default theme、多目标资产、局部/显式 include、scope/key 覆盖、host DynamicResource、resource-only |
| 生命周期 | optional package 未启用不运行、重复/递归/失败、多个 Application builder 独立 |
| 稳态 | 注册先于控件实例化、主题切换与 scoped token 不改变 schema、Semantics 与 Token/Theme 一致 |
| 后端 | 关闭/损坏转换器失败，未知 ABI 失败，incremental 输入准确，Browser 不走 full |
| 裁剪 | unused control/proxy/factory 不存在，新增无关同目录控件不改变集合，无全量 manifest 意外 root |

发布体积必须同 SDK、RID、字体和配置，与旧实现/full/Fluent 基线按现有指标比较；不能把不同实验程序大小相减。
现有最小桌面体积、unused 增量门槛仍需验证，任何调整必须有新的同口径数据。

## 8. 机械删除审计

实现收尾检查源码、工程、脚本、正式文档、nupkg 和构建产物，确保不再有活动引用：

```text
Generator.LinkedPublish / AtomUILinkedPublish
GeneratedApplicationRegistrationPlan / AotTrimRegistrationPlanRegistry
AtomUI.Linked.*.v1 / .atomui-link.json
Sidecar candidate / consumer IL extraction / UnitEdge
RegistrationGranularity / AotTrimUnit / registration Unit ownership
旧 linked ProjectReference AdditionalProperties
```

日期化 specs、旧 release note、迁移文档可保留这些词并标注历史。普通源码不能靠 allowlist 隐藏残留的旧执行路径。
不能仅禁用 target，却继续编译和分发旧 analyzer 项目。

## 9. 文档与发布

正式实现时重写 AOT 架构、启动链、构建打包、generator 模块、第三方指南和编译诊断注册表。
删除 Sidecar reference，替换 linked pipeline/unit 粒度入口并更新 AGENTS 与导航链接。
新版本记录 Public API、TFM、构建工具要求、第三方重新构建要求和资源契约变更；不绕过既有破坏性变更审计。

## 10. 实施阶段边界

1. 类型/资产事实模型、owner-bearing identity、资源局部化和共同 leaf factories。
2. 普通包入口、Common 与 optional packages、新 TypeMap/bootstrap。
3. 浏览器正式 linker 后端、工具链/增量/产物门禁。
4. 完整消费与 UI/发布矩阵、同口径体积验证。
5. 同次交付删除旧项目/协议/工具/测试，更新正式文档与迁移说明。

详细实施计划在本设计评审通过后编写。本轮不把探索性原型当作已完成前四阶段。
