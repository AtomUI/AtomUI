# .NET 8 条件注册后端：可行性结果与接入设计草案

日期：2026-10-05。状态：最小机制原型通过，正式产品接入待评审。

正式评审入口已转到 [AOT 依赖分析架构方案](2026-10-05-aot-dependency-analysis-design/overview.md)。本文保留为原型阶段记录。

## 1. 用户目标

- 产品库 Debug 只构建 net10.0，Release 同时交付 net8.0 和 net10.0。
- .NET 8 必须具备与 .NET 10 同等级的真实依赖分析、条件注册和精细裁剪能力。
- 不接受全包注册、源码使用扫描或较宽的 ILLink 预分析结果代替 NativeAOT 的真实依赖分析。
- 控件 API、业务行为和 .NET 10 当前后端保持原有契约。

“同等级”指条件映射和实际依赖闭包能力；不同运行时/编译器版本的框架代码、优化结果和最终文件大小不要求逐字节相同。

## 2. 已完成的最小原型

外部源码：官方 dotnet/runtime v8.0.27，提交 a6bde67c455f2ac219988c7a66171631090b6f65。
参考基线：官方 .NET 10.0.8 TypeMap。SDK 10.0.300 只作为构建宿主；.NET 8 原型实际使用 8.0.27 的运行时与 NativeAOT 资产。

### NativeAOT

以原始 ILC8 组件源码构建编译器宿主，复用官方匹配版本的 jitinterface、clrjit、objwriter。
仅在编译器侧增加条件注册节点与 ILProvider 适配，没有修改 CoreLib 或运行时库。

执行路径：

1. 候选声明只包含完整程序集身份、条件类型和注册方法，不含应用使用清单。
2. 注册入口真正可达后激活该 Group。
3. 在 ILC8 的真实 scanner 依赖图中加入 `NecessaryTypeSymbol(condition) -> MethodEntrypoint(fragment)` 条件边。
4. 选中片段的 IL 由同一 scanner 继续分析，新依赖可继续激活其他片段，直到闭包稳定。
5. 闭包完成后将注册入口物化成普通 net8 静态调用，交给原有代码生成与原生链接流程。

同源空入口基线运行失败；接入后 12 个场景均成功运行于 .NET 8.0.27。相同 12 个场景在官方 .NET 10 TypeMap 下
得到完全一致的注册选择集合。最终原生符号表验证选中 Register 方法存在，其他片段与未使用工厂标记不存在。

| 场景 | 预期 |
| --- | --- |
| Direct | 直接控件对应片段 |
| Token-only / Style-only | 仅相应条件对应片段 |
| typeof | 对应类型条件片段 |
| Static-only | 静态方法存活，但不误选注册片段 |
| Transitive | A 片段引入 B，闭包包含 A/B |
| Rooted cycle | 有根循环的两个片段各执行一次 |
| Unrooted cycle / unused | 均不保留 |
| Accessor off | 有控件引用但未请求 Group 时不注册 |
| Combined | 合并场景结果正确 |
| Cctor / cctor-helper | 静态构造函数中的直接与传递请求均正确 |

静态构造函数暴露了必须解决的边界：空入口可能被预初始化解释器提前执行并消除。原型先复现失败，再通过
仅对该入口拒绝解释执行的 ILProvider barrier 修正，scanner 和 codegen 各自仍接收正常方法体；未全局关闭预初始化。
错误程序集完整身份和禁用 scanner 均明确失败，没有全量回退。

### 托管裁剪

使用实际 ILLink 8.0.27 的 MarkStep 派生扩展，未修改 linker/runtime 源码。自有字符串声明在 Mark 前解析、隔离；
通过实际实例化、反射可见性、类型检查及相关标记事件激活条目。选中的 factory 通过 MarkMethod 加入官方队列。
Mark 完成后、Sweep 前只物化已经分析过的调用。

真实 net8 self-contained CLR 程序正确执行 9 个选中片段；独立 Cecil 检查确认 8 个未用/无根/未启用类型缺失，
静态调用类型本体保留而对应 factory 被删除。未知 ABI 与缺失 factory 都在发布时失败。

### 证据

精简证据位于 `/tmp/atomui-net8-conditional-backend-evidence-20261005/`：

- `comparison.json`：12 个 .NET 8/.NET 10 场景逐项对照。
- `native/compiler.patch`：编译器侧原型补丁。
- `native/identity.json`、`native/matrix-results.json`、`native/rejection-tests.json`：身份、执行与拒绝检查。
- `illink/backend.patch`、`illink/selection.txt`、`illink/inspection.json`：托管后端与闭包/产物检查。

证据是机制可行性验证，不是完整产品支持承诺。临时可运行工程、测试夹具、源码 checkout 与大型构建产物在复核后删除；
只保留非运行形式的补丁、结果与必要日志。

## 3. 建议正式架构

### 同一事实模型，两个目标后端

继续由现有生成器产生 Control、Token、Semantic Part、资源 factory 与平台可用域事实。应用入口保持 `UseXxxControls()`。
不恢复旧 Unit/Sidecar usage 分析，不让构建工具重新推测 C#/AXAML 使用情况。

- net10.0：保留现有官方 TypeMap 声明、查询与后端。
- net8.0 非裁剪：正常完整注册路径。
- net8.0 trimmed：自有条件元数据 + ILLink8 标记闭包 + 已选调用物化。
- net8.0 NativeAOT：相同条件元数据 + ILC8 scanner/依赖图 + 已选调用物化。

候选元数据不是使用清单。它只陈述“此条件触发此注册片段”；是否触发由对应真实依赖分析器决定。
元数据传输可以采用程序集内构建专用记录或打包资产，格式需独立定版；不得在普通类型参数/静态数组中直接根化全部候选。

### .NET 8 注册 ABI

优先使用普通静态注册 dispatch，避免引入 CoreLib 新 API。生成器为真实注册片段提供强类型调用入口，参数为当前
ControlPackageRegistrationBuilder；后端只写入选中入口。Token、Semantic descriptor 和资源 factory 仍来自同一生成事实。

正式 ABI 必须定义：Package/Group 身份、完整类型和方法签名、多条件到同一片段的去重、泛型及类型转发、平台 guard、
包初始化顺序、异常处理、可识别的入口方法体，以及非裁剪/裁剪入口切换。不能直接把原型的无参 void 约定当作产品 ABI。

### 编译器与工具分发

.NET 8 NativeAOT 使用定版编译器适配，它不是官方 ILC 的稳定插件 API。以单独构建工具包交付，按支持的 host RID
分发，并与对应 .NET 8 compiler/runtime/reference assets 做完整身份校验；不能只判断版本大于某个下限。

工具链升级必须重跑等价性矩阵。未知工具版本、无法解释的元数据、未物化入口或分析证据缺失都应失败，不回退全量注册。
普通 Debug/非裁剪构建不启动额外依赖分析。

## 4. 正式接入前的验证门槛

第一阶段：扩展合成矩阵。覆盖多程序集、多 Group、预编译 consumer DLL、NuGet 传递引用、完整身份冲突、泛型/转发类型、
多条件同片段、动态反射声明、平台/特性开关、静态构造函数、错误输入和增量构建。NET8/NET10 使用同一组产品条件契约对照。

第二阶段：接入真实 AtomUI 生成 ABI。至少覆盖 Button、DateViewer/DatePicker、嵌套 Popup、独立 Token/Style 引用、
AXAML 资源传递闭包、应用级主题替换、主题/语言切换与注册冻结。分别核验运行结果和未使用控件/代理/factory/资源缺失。

第三阶段：发布矩阵。验证 source/ProjectReference/干净 NuGet cache，验证支持的 Windows/macOS/Linux host/RID，
核验 net8 运行时身份、冷构建、无改动复用、元数据/工具变化失效和失败清理。

普通构建、单个 Gallery 发布或“程序能启动”均不能替代以上产品验收。

## 5. 尚未证明的范围

本次 NativeAOT 原型仅验证单程序集、单 Group、无参静态注册方法与 scanner-enabled 发布。没有验证真实 AtomUI
代理 Attribute ABI、AXAML 资源、第三方包、全部泛型/反射组合、非 scanner 编译以及其他 RID。

托管原型覆盖了已列出的事件，但泛型/晚到达事件的完整通知覆盖还需证明；若既有 MarkStep 扩展点不足，需要定版的小范围
linker patch，不能以 IsMarked 近似替代正确条件。

当前 AtomUI 工作树中的 .NET 8 全量注册兼容草稿只证明普通运行，不能作为最终 AOT 方案发布。正式实现应在设计评审后
把该临时路径替换为上面的条件后端，并完成全部适用验收。
