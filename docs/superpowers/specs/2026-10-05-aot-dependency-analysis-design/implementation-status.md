# 实施验收记录

本文件对应本目录已批准设计，记录本次实现的证据与剩余门槛。早期 [原型证据](evidence.md) 仍是当时的历史事实；
它的“尚未证明”列表不代表下面新增验证的当前状态。正式实现入口见
[.NET 8 条件后端](../../../architecture/foundations/aot-net8-conditional-backends.md)。

## 已实现的机制

| 范围 | 验证事实 |
| --- | --- |
| 统一生成 ABI | net8 实际编译的候选记录、摘要和 thunk；full 生命周期与 selected 未物化拒绝；net10 生成文本与原基线一致 |
| ILLink8 | 实际 MarkStep 条件闭包，晚到达泛型/反射、OR、多 Group、无根循环及静态使用反例；重读最终 DLL 并运行 |
| ILC8 | scanner 与无 scanner、优化组合、cctor/泛型 cctor、builder/GC、原生表重定位；本机 arm64 与 x64 执行 |
| 产品资源 | 真实 Minimal 4 项和 Complex 24 项资源/注册检查，对照 full，并检查未用控件与工厂删除 |
| 输入快照 | 实际 SDK 冻结 246 个额外输入、覆盖 8 类；冻结后修改原文件仍消费副本；PDB 邻接及控制任务源码捕获 |
| 托管变换 | R2R、压缩单文件、组合 composite R2R 均 consumed 并运行 selected；最终产物篡改拒绝 |
| net10 混合 ABI | 官方 ILLink/ILC 消费桥接副本；Browser 解释与 31 程序集 AOT 实际执行通过；不增加应用依赖分析器 |
| 缺 ABI 门禁 | 伪造 accessor 声明而无真实 TypeMapping 调用、闭合 Group 不一致均拒绝；纯 net10 不依赖 bridge 工具存在 |

## Review 修复

1. 原生 published 验证曾重新认证已改写的程序；原始篡改探针确实成功。现在 linked 固定首次 hash，published 对比
   调用方保存的回执 hash 和实际副本。代码段篡改、回执改写和用 linked 重试重新认证均拒绝。
2. 固定 mutable 目录可能跨调用混用。现在每次 SDK 调用拥有 GUID 和独立 object、exports、report、native 目录；
   真实并发发布各自运行通过，A 消费 B 的完整产物被拒绝。
3. 失败清理曾接受回执内的任意报告路径，复现能删除外部探针文件。现在只清理本次拥有的路径，外部路径、`..`、
   文件和父目录符号链接均拒绝，外部文件保持存在。
4. 构建资产校验现比较实际依赖和内容，并验证包内 Import 闭包；缺失依赖、工具或被导入 targets 不再放行。

## 临时测试处置

生成器临时验证最后运行 9/9，Build.Tasks 临时验证最后运行 45/45，均无跳过。已删除本任务三个 Temporary 子目录。
不新增全局常驻 xUnit 测试。ILC8 按需专项入口保留，因为真实条件图、原生重定位、calli/GC 与预初始化无法由普通
单元测试替代；其触发范围为编译器/适配器/相关 ABI 变化，不进入普通全局测试。
现有产品 fixture 增加可观察枚举使用：官方 net10 与 net8 的优化无 scanner 均可能消除仅用于 GC.KeepAlive 的装箱，
不能把这种死代码当作正向条件根。

清理后的最终 focused 验证为：Generator 注册/Token/构建契约 226、Build.Tasks 116、Core 注册 24、TypeMap Linker 89，
共 455 项通过，无失败、无跳过。现有构建契约测试同步了 net8 分析能力、工具资产清单及后端进入首轮 restore 图的预期，
没有新增全局测试用例。未运行全量回归。

## 最终本地包与冷消费

Release 与 Debug 各 20 个包均通过实际 DLL TFM、nuspec 依赖、工具内容与本地 Import 闭包校验。
Release 产品库包含 net8.0/net10.0，Debug 产品库仅 net10.0。全新隔离 obj/bin 的 Generator 直接 pack 也通过，
首轮 restore 包含全部工具项目，不再依赖 pack 阶段临时构建未恢复项目。

统一 Release 包集的 net8 managed、net8 NativeAOT 与纯 net10 managed 独立空缓存消费均发布并实际运行通过。
三个程序均报告 selected，执行模板、ThemeManager、注册顺序与冻结检查。managed8 的 49 个发布 DLL 与 consumed
回执逐一 hash 一致；net8/net10 的独立 PE 元数据检查均保留实际使用的 Button，删除未使用的 Calendar。
Native8 独立复核 226 个程序集和 195 个附加输入，最终程序匹配首次 linked 证明及本次 caller GUID。

net10 消费第三方 net8-only 包也完成空包/HTTP 缓存的一次 restore，随后 `publish --no-restore` 与实际执行通过；
42 个最终文件获得 published-output 回执，未用片段/full collector 缺失，3 个原始 net8 DLL 字节不变，工具不进入运行依赖。
第三方测试包遵循作者指南，通过正常 Desktop.Controls 依赖获得工具；消费者仅引用第三方包。最初人为 Core-only
依赖的机制包缺少构建资产，运行失败，这一负例保留为边界，不能据此宣称任意缺工具的第三方包均能自动桥接。

新增引擎与维护者脚本已登记验证策略所有权，精确 iterate 计划无 ownership gap。它们的真实编译/运行仍为按需专项，
不把无关单测或零测试计划算作成功；跨平台等未验收义务继续显示 pending，未手工填入完成回执。

本地 Desktop 包为 8,419,428 bytes，其中受控原生编译器与许可证共 12 项，压缩 916,899 bytes、未压缩 2,414,606 bytes。
这是本次工具分发成本，不是应用部署体积。包沿用工作树 6.2.3 仅供隔离测试，没有上传到任何公共 feed。

精简本次证据位于 `/tmp/atomui-aot-implementation-evidence-20261005/`，仅保存非运行摘要和必要日志；
其中旧原始产物路径是历史定位信息。临时 compiler 源码、发布工程和大体积缓存按验收结束顺序删除，
仓库正常工具缓存保留。系统可清理临时证据，长期结论以本记录和正式架构文档为准。

## 仍需关闭的门槛

- Windows/Linux 原生消费、对应 runtime/helper/RID 运行证据；Windows 维护者编译器构建。
- 混合 ABI 桥接的 R2R/单文件、Browser Webcil，以及当前门禁之外的工具链版本与发布形态。
- 完整产品与平台矩阵中尚未逐项记录的 Gallery、动态资源/生命周期对照、servicing、取消重试和性能成本。
- 正式发布准备、版本号与完整回归。当前未提交、未发布，不移动 6.2.2/6.2.3 标签或覆盖已发布包。

上述缺口继续阻止完整跨平台支持声明。不能用本机通过、打包成功或显式门禁代替缺失的验收行。


## 2026-10-06 单工程收敛验收

原五个顶层工具目录已归入本工程，原 78 个 C# 文件内容逐一保持一致；收敛工程、共享源码归属、任务声明和打包编排，
不改写依赖选择算法。477 项模块定向测试、5 项语言包集成及 7 项验证策略检查通过，未运行全量回归。

20 个 Release 包通过实际框架、依赖、内容和导入闭包检查。普通 net8/net10 冷消费均运行 selected 并删除未用 Calendar；
net8-only 作者包到 net10 的桥接对照保持命名 include、AXAML type-key 资源，删除未用控件、full collector 和候选记录。
最终 Native8 冷消费验证了 421 份冻结输入、调用身份及 linked/published 哈希；独立机制还验证 scanner 开/关的 6 个场景。

收尾修复了冷 profile 恢复、源码 publish 准备次序、独立产品 pack 准备、维护者空缓存依赖源，以及第三方包重复转发
上游语言源的问题。所有临时工程、缓存和编译器源码已清理，无新增常驻测试、提交或发布。
精简证据位于 `/tmp/atomui-toolchain-evidence-20261006/`；本次不扩大原有平台支持门禁。
