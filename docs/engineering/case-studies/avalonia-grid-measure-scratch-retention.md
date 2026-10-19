# Avalonia Grid 测量临时数组保留已关闭 Dialog

调查日期：2026-10-19。AtomUI 继续使用官方 Avalonia 12.1.4，通过内部兼容层隔离 Grid 测量缓存。

## 现象与根因

`Closed_Overlay_Session_Releases_Surface_When_A_Custom_Button_Is_Retained` 在 Dialog 模块验证中失败，
具体断言为 `references.Surface.IsAlive` 仍为 `true`，而 Presenter 已回收。

失败现场的堆转储确认：自定义按钮的逻辑父、视觉父、模板父和继承父均为空；Surface 的事件、按钮与绑定均已释放。
实际保留链为：

```text
旧 Grid._extData.TempDefinitions
  → 同线程共享 DefinitionBase[]
  → 当前 Dialog 的 ColumnDefinition
  → ValueStore.InheritanceAncestor
  → 已 Dispose 的 DialogSurface
```

旧 Grid 与当前 Dialog 内 Grid 的 `TempDefinitions` 指向同一数组。数组中的当前列定义不是旧 Grid 的列定义。

`Grid.TempDefinitions` 通过线程局部弱引用复用数组，同时各 Grid 实例也持有该数组。
测量结束不清理数组；清理只发生在后续 Arrange 的 `finally`。只完成 Measure 后便关闭的控件可能无法进入 Arrange，
导致仍存活的其他 Grid 通过共享数组保留它。现有清理长度还未覆盖完整数组，可能遗漏后半部分的最大值候选。

## 源码依据

最终采用本地参考仓库 `/Users/chinboy/Projects/ReferenceProjects/Avalonia` 的 tag `12.1.4`，
commit `2d79ba55fae1b1e71c69f93a668080c9029e6a31`。检查的文件为 `src/Avalonia.Controls/Grid.cs`，
并核对了实际 NuGet DLL 的实现和失败现场字段。

`git diff 12.1.3..12.1.4 -- src/Avalonia.Controls/Grid.cs` 无差异，因此不能把这个问题归因于本次版本升级新增。
数组只参与 Measure 内的局部计算；Arrange 使用独立的 `DefinitionIndices`，不需要保留此数组内容。

## 最小通用修复

候选补丁见 [avalonia-grid-measure-scratch-retention.patch](avalonia-grid-measure-scratch-retention.patch)：

1. 在 `MeasureOverride` 的 `finally` 清理测量临时数组。
2. 清理整个数组长度，再清空实例字段；保留 Arrange 的清理兜底。

该补丁尚未接入 AtomUI 依赖，也未发布任何第三方包。不能通过只修改 Dialog 的布局、延迟 GC、放宽断言或忽略失败来代替上游修复。

## AtomUI 临时兼容层

实现位于 `src/AtomUI.Core/Reflection/GridMeasureCacheCompatibility.cs`，在 `ApplicationStartup` 的两个挂接入口安装，
早于 Splash 和主题初始化器创建视觉内容。只在 UI 线程读取已知的 `s_tempDefinitionsDataSlot` 字段，
以 `DynamicDependency` 保留精确字段元数据，不扫描类型、不修改方法、不生成动态代码。

线程缓存原本存放 `WeakReference`。兼容层放入静态编译的子类，使其 `Target` 始终返回空且不保存写入值。
Grid 因此为自己分配临时数组，仍在整个 Measure 中拥有该数组；不同 Grid 不再共享引用。
不在任意布局事件中清空数组，避免破坏嵌套测量正在使用的内容。

安装前先保存线程缓存，以独立的普通 Grid 执行 Measure，检查测量后数组是否仍含定义引用，然后在 `finally` 恢复缓存。
探测对象用 `GC.KeepAlive` 保持到弱引用读取完成，避免 Release/NativeAOT 的局部变量生命期导致误判。
当上游在 Measure 结束后正确清空数组时，此探测不会安装兼容层；字段结构不匹配则记录告警，要求重新评估。
升级到已验证修复的版本后，应删除此兼容层及两个启动调用。

代价是取消跨 Grid 和同 Grid 跨 Measure/Arrange 轮次的数组复用。两列 Grid 的 1000 次重测微基准中，
线程分配量从 288000 字节增加到 344000 字节，即每轮增加 56 字节；这不是一次性分配。
本记录不据此宣称耗时无变化。待上游采用测量结束清理补丁后，可恢复其原有缓存复用。

## 验证证据

- 原始 Dialog 模块：264 项中 263 通过、1 项因 Surface 存活失败，完整日志保留了断言与栈。
- 最小 A/B：保留一个只 Measure 的旧 Grid；第二个 Grid 只 Measure 后返回弱引用。未清临时数组时目标仍存活，
  在测量完成后清空同一个数组时可以回收。复现不包含 AtomUI 控件。
- 候选源码来自上述 tag，只应用两行补丁；`Avalonia.Controls` 的 net10.0 源码构建为 0 warning、0 error。
- 补丁运行验证同时检查有限 MaxWidth、完整 Measure/Arrange 尺寸与 Dialog、Grid 模块；尺寸按官方二进制记录的结果做精确 A/B，
  避免假设布局舍入结果。284/284 通过（含 6 项临时验证），之后恢复官方 `Avalonia.Controls.dll`。
- 官方 DLL 上兼容层 A/B：相同 Measure-only 所有权测试在未隔离时失败、隔离后通过；嵌套布局精确快照一致。
- 原 Dialog、Grid 模块及临时兼容层验证共 282/282 通过，包含保留自定义按钮的原始生命周期用例、启动重复调用和测量异常。
- macOS arm64 的真实 NativeAOT 探针通过公开 `ApplicationStartup` 入口完成启动、Grid 回收与布局验证，
  `RuntimeFeature.IsDynamicCodeSupported=false`。探针没有额外反射或保留声明，字段保留仅依赖产品兼容层。
  原生链接出现本机 Homebrew 库构建系统版本高于 macOS 最低目标的告警；运行成功，不代表更早 macOS 已完成验证。
- 清理临时验证代码后，最终源码的 Dialog、Grid 模块 278/278 通过；Core 启动与应用作用域用例 15/15 通过。
- 最终发布全量使用 `scripts/run-full-regression.sh`：15 个项目、6510/6510 通过，耗时 12:14；
  完整 Gallery 的 osx-arm64 NativeAOT 发布及产物检查通过，进程运行约 52 秒无异常日志。
  裸可执行文件无法通过自动化工具的精确路径绑定，因此此次没有确认 Gallery 主窗口视觉结果。

临时复现代码、源码副本与大体积堆转储在验证结束后清理；本记录及补丁保留供依赖修复交付评审。
