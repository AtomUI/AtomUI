# AtomUI.Controls.Shared 模块概览

`AtomUI.Controls.Shared` 是控件共享契约和跨控件基础能力模块。它依赖 `AtomUI.Core` 和 `AtomUI.Generator`，RootNamespace 为 `AtomUI.Controls`。

## 职责

- 提供控件通用接口，例如尺寸、状态、方向、操作系统感知、Header/Content 等共享契约。
- 提供异步加载协调器，统一搜索类和展开类控件的 debounce、超时、取消、去重行为。
- 提供集合视图能力，支撑排序、分组、过滤和虚拟化上下文。
- 提供上传调度、媒体断点、WaveSpirit 等跨控件系统。

## 关键目录

| 目录 | 说明 |
|---|---|
| `AsyncLoad/` | `AsyncSearchLoadCoordinator`、`AsyncExpandLoadCoordinator`、加载结果状态 |
| `Data/ListCollectionViews/` | 列表集合视图、排序、分组、过滤描述 |
| `Net/` | 文件上传调度与传输接口 |
| `MediaQuery/` | 媒体断点和响应式布局支持，规则见 [响应式架构](../../architecture/systems/control-infrastructure/responsive.md) |
| `WaveSpirit/` | 点击波纹/动效扩展 |
| `Converters/`、`Utils/` | 控件共享工具 |
| `Dates/` | `DateUnitMath` 提供日期单位和周序；`DatePanelAlgorithms` 提供共享面板、自然月布局和无分配的逐格状态查询。 |

## 相关专题

- [Control 基础设施架构](../../architecture/systems/control-infrastructure/overview.md)
- [异步加载使用指南](../../guides/control-infrastructure/async-loading.md)
- [过滤使用指南](../../guides/control-infrastructure/filtering.md)

## 图片加载职责

Shared 拥有统一图片系统的完整 engine 与公共数据契约：`ImageLoader.cs`、`ImageLoaderStore.cs`、Source/Request/Result/Error/
Progress、Builder options、两级请求合并、下载/解码调度、encoded/decoded/file cache、HTTP transport、source readers、内容
校验、raster codec，以及 SVG 的安全 XML/CSS/data raster、复杂度和一致性模式验证及集中 security policy version。所有文件位于单层
`src/AtomUI.Controls.Shared/ImageLoading/`；不创建 `Application/`、`Runtime/` 或 `Internal/` 子目录。

该模块只依赖 Core 提供的通用 owned-service 生命周期。它不引用 `AtomUI.Controls`，不包含 Avatar fallback、AsyncImage Theme、
Previewer Current/Cover/Preload 策略，也不引用 `Svg.Controls.Avalonia` 或通过反射发现上层 codec。Controls 使用显式 Builder
调用把统一 `SvgImageCodec` 注册到同一个应用 loader；Shared 只向 codec 交付当前安全策略已经验证的不可变字节和 metadata。
`SvgContentValidator` 明确拥有三层判断：不可关闭的安全边界、应用级有界资源预算，以及默认 `Compatible`/可选 `Strict` 的格式
一致性策略。第一版两种模式只在重复 `id` 上不同；兼容模式仍按元素出现位置建立完整引用图，不能跳过深度、环或复杂度检查。
模式在 Application loader 构建时冻结，不进入 `ImageRequestOptions`，Shared 不公开 per-request 开关或任意 validator delegate。

API、文件布局与引擎规则见：

- [统一图片加载系统](../../architecture/systems/image-loading/overview.md)
- [公共契约](../../architecture/systems/image-loading/public-contracts.md)
- [控件 API](../../architecture/systems/image-loading/control-apis.md)
- [管线、并发与生命周期](../../architecture/systems/image-loading/pipeline-and-lifecycle.md)
- [缓存、HTTP 与内容安全](../../architecture/systems/image-loading/caching-and-security.md)
- [网络 SVG 加载](../../architecture/systems/image-loading/network-svg.md)
- [平台、性能与 AOT](../../architecture/systems/image-loading/platforms-and-aot.md)
- [验证与完成门禁](../../architecture/systems/image-loading/verification.md)

过滤专题中的部分实现位于 `AtomUI.Desktop.Controls`，但统一契约和共享数据结构属于本模块的重要背景。
