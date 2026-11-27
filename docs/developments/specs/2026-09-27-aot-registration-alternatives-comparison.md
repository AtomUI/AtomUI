# AtomUI AOT 注册方案比较

> 过程记录：本轮结论已同步到 [正式 AOT 架构](../../architecture/foundations/aot-and-trimming.md)。本文保留方案比较、实验或迁移过程，不再作为平行的长期规范；事实以正式文档为准。


> 2026-09-27 研究与设计结果。只新增方案文档，未修改 AtomUI 产品代码、默认发布行为或公共 API。
> 后续决定：用户已否决方案二，仅深入探索方案一。下文比较保留为历史研究记录，不再建议新增显式选择入口。
> 最新结论见 [TypeMap 深入验证](2026-09-27-aot-typemap-validation.md)。
> 全局复评后的目标架构以 [完整替换设计](2026-09-27-aot-typemap-replacement-design.md) 为准。

## 1. 两份方案

1. [方案一：.NET TypeMap 自动注册](2026-09-27-aot-typemap-registration-design.md)
2. [方案二：显式控件族与包内闭包](2026-09-27-aot-explicit-control-set-design.md)

上一轮“没有直接相关的官方替代能力”的判断不完整。.NET 10 TypeMap 正是条件注册表所需的基础能力；普通
`DynamicDependency` 与 `FeatureSwitchDefinition` 不能替代它的语义。

## 2. 对比

| 维度 | 现有机制 | 方案一 | 方案二 |
| --- | --- | --- | --- |
| 应用写法 | `UseDesktopControls()` | 保持不变 | `UseDesktopControls(c => c.AddButton()...)` |
| 控件使用范围 | AtomUI 分析 C#/AXAML/消费 DLL | 官方 ILLink/ILC 可达性 | 应用选公开控件族 |
| 传递注册依赖 | UnitEdge + 应用 closure/SCC | TypeMap 条件依赖进入官方 linker 闭包 | 包生成阶段预计算并扁平展开 |
| 应用 Sidecar / hash / Plan | 需要 | 新路径不需要 | 新路径不需要 |
| 包内 C# usage 分析 | 需要 | 不需要 | 仍需要 |
| AXAML 资源 ownership | 需要 | 保留局部契约与必要强引用 | 保留 |
| 运行时 | 应用静态计划直接调用 | 字符串键查询 + Attribute 代理 + 去重 | 静态调用 + 去重 |
| 首帧前冻结 | 支持 | 支持 | 支持 |
| .NET 8 | 支持 | 无 TypeMap，需另选兼容路线 | 支持 |
| .NET 10 NativeAOT | 支持 | 核心实验通过 | 原理可用，当前实验选 .NET 8 |
| 用户遗漏风险 | 动态输入需 roots | 动态输入需 roots | 显式集合可能漏选，需要开发期诊断 |
| 额外 linker passes | 无 | 无 | 无 |
| 普通 Debug | full | 可继续 full | 与发布使用同一显式集合 |
| 维护规模 | 最大 | 收敛平台后最小 | 中等，保留包生产期分析 |

## 3. 实验结果

| 实验 | 结果 |
| --- | --- |
| .NET 10 单程序集 NativeAOT，移除映射 | 注册断言失败，证明测试能识别缺失 |
| .NET 10 单程序集 NativeAOT，启用映射 | Button 和仅由主题工厂引用的 Presenter 被注册，Unused 缺失 |
| .NET 10 跨程序集 + Avalonia 12.1.2 真实 AXAML + NativeAOT | 通过，模板内部 Presenter 自动保留 |
| 同场景 trimmed JIT | 通过 |
| 同场景未裁剪 Debug | 全量条目存在，符合预期 |
| .NET 8 跨程序集 + 真实 AXAML + 显式注册 + NativeAOT | 通过，重复 AddButton 幂等 |
| 两种 NativeAOT 的 ILC map | Button/Presenter 注册存在，Unused 控件及注册不存在 |

这是机制验证，不是 AtomUI 完整迁移验证。未运行完整 Gallery、主题切换、平台矩阵，也没有得出体积或发布时间改善百分比。
实验输出和源码位于 `/tmp/atomui-typemap-proof-cqmdilba`，评审结束后可删除。

## 4. 推荐

以保留无参数一行入口、自动裁剪为优先：选择方案一作为 .NET 10 主路线。
它把真正的代码可达性问题交给官方 linker，消除的是分析职责，而非只更换 Manifest 格式。

如果 .NET 8 长期支持及简单、可预测的消费构建优先：方案二可直接落地，但要接受显式控件族选择。
它不要求每个控件拆 NuGet，也不要求用户知道内部依赖。

两个方案应共享一个 leaf fragment / descriptor / resource factory 事实源，不能各生成一套主题注册内容。
如果同时提供，方案一负责无参数自动入口，方案二负责显式 overload；不要额外保留第三套永久自动分析体系。

## 5. 必须显式决定的兼容边界

如果必须同时保留 .NET 8、无参数入口、精细自动裁剪，方案一不能独自满足，旧 .NET 8 分析路径暂时必须存在。
彻底删除旧管线需要以下之一：

- 将精细自动裁剪最低版本收敛到 .NET 10，.NET 8 使用全量兼容路径或显式 overload。
- 将 .NET 8 保持旧机制作为明确过渡，而不是宣称系统已经全面简化。

方案一还需要批准放宽当前“启动阶段禁止字典查询和 Attribute 访问”的内部约束。
这个变化有明确边界：只查询 .NET 已生成的结果，不做运行时依赖推导；注册仍在首次控件实例化前完成。

## 6. 可执行的后续顺序

1. 先评审两份设计中的接口和兼容边界。
2. 在隔离工作区以 Button 和真实跨族资源验证共享 leaf fragment 与两条入口。
3. 增加 DatePicker/DataGrid、Semantic Part、纯字符串资源、循环及多 root fixtures。
4. 使用现有 AOT 验证入口比较同 SDK/RID 的注册快照、主程序体积和发布成本。
5. 证据通过后迁移默认路径，并按支持矩阵退役旧 analyzer、Sidecar 和 Application Plan。

两个临时原型证明了核心机制可执行；它们不会被直接复制成生产实现。
