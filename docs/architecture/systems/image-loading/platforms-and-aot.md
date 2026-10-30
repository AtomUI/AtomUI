# 图片加载平台、性能与 AOT 边界

> 状态：当前架构。本文区分跨平台不变量和真实平台能力，不把 Desktop 行为虚构成 Browser 已具备能力。

## 平台能力矩阵

| 能力 | Desktop | Browser WebAssembly | 当前处理 |
| --- | --- | --- | --- |
| HTTP/HTTPS | 支持 | 支持，受 Fetch/CORS 限制 | 统一 `HttpImageTransport` 契约、平台 adapter |
| HTTP 逐跳重定向检查 | 支持，关闭自动跳转 | 中间跳通常不可观测 | Browser 依赖 Fetch/mixed-content，校验初始和可见最终 URI |
| File path | 支持 | 不支持任意本机路径 | Browser 返回 `AccessDenied`/`UnsupportedScheme`，不模拟文件系统 |
| `avares` Asset | 支持 | 支持 | 统一 Asset reader |
| `IStorageFile` | 支持 | 支持用户授权得到的 handle | 每次读取仍遵守 stream ownership 与上限 |
| Bytes/Stream | 支持 | 支持 | 无稳定 key 时不持久缓存 |
| borrowed `IImage` | 支持 | 支持 | AtomUI 永不 dispose 原对象 |
| raster codec | 支持 | 支持平台可用格式 | 显式注册并探测 capability |
| 受限静态 SVG | HTTP、Asset、File、Storage、Bytes、Stream | HTTP、Asset、Storage、Bytes、Stream；File 仍受平台限制 | Shared 安全验证，Controls 显式 `SvgImageCodec`；renderer 不发起外部 I/O |
| persistent cache | 可选、默认关闭 | 不可用 | Browser 配置启用时启动失败并给出 diagnostics |
| NativeAOT/trimming | publish/启动必须验证 | 裁剪运行与 AOT 均须 publish、启动和精细保留验证 | 按已采纳 TypeMap 架构验收，不使用动态发现 |

移动产品包尚未落地，不能在本架构中宣称 iOS/Android 已验证。任何 Mobile adapter 必须复用 Shared 的 source/key/cache/
security contract，并在 [Mobile 系统架构](../mobile/overview.md) 的平台验证矩阵中补充真实证据，不能复制一套 loader。

## Codec 边界

internal `ImageCodec` 使用稳定 Id、Version 和 `CanDecode(probe, source)` capability 显式注册。registry 要求每个实际 probe
恰好匹配一个 codec，零匹配返回 `UnsupportedFormat`，多匹配视为实现配置错误；不扫描程序集、不读取 Type 名称字符串、不用
`Activator.CreateInstance`。

`RasterImageCodec` 是 Shared 的 raster 边界：负责 header probe、目标尺寸计算、解码、真实 decoded byte 报告和 owned
image release。具体实现只能使用当前目标框架和 Avalonia/Skia 提供的公开、可裁剪 API，或引入经过单独依赖评审且明确支持
Desktop、Browser 和 NativeAOT 的 codec 库；不得为复用某个内部方法加入反射、动态 delegate 或未验证的私有 API。
当前 baseline 至少覆盖 PNG、JPEG、WebP、BMP 和单帧 GIF；codec capability probe 发现多帧容器时返回
`AnimationNotSupported`。平台无法可靠支持的额外格式不能只在某一后端静默出现，必须进入能力矩阵和跨平台测试。

Controls 的 `SvgImageCodec` 只匹配 Shared 已按当前 `ImageSecurityPolicy` 验证为 `ImageContentFormat.Svg` 的内容，不按 HTTP、File、
Asset、Storage、Bytes 或 Stream 分裂 codec。AtomUI 只调用 `SvgSource.Load(stream, parameters)`；不得把源 URL、文件路径或可解析
外部资源的 `BaseUri` 交给 renderer，也不得调用会自行访问 URL/File 的 path overload。

SVG 依赖基线、`SecureStatic` load options、静态子集与资源预算由[网络 SVG 加载设计](network-svg.md)统一定义。依赖升级必须重新
验证 XML DTD 行为、CSS/资源解析、线程归属、Browser 和 NativeAOT；不得通过修改上游进程级 static 开关实现单次请求安全策略。

动画图片时间轴不属于统一系统。codec 必须识别动画容器并按安全策略拒绝，而不是后台持续解码多帧或静默制造无法控制
的 CPU/内存负担；任何动画支持都必须先具备独立公共状态、时钟、缓存和可见性契约。

## 解码尺寸与 RenderScaling

Control 层把逻辑尺寸转换为物理像素：

```text
targetWidth  = ceil(max(0, arrangedWidth)  * TopLevel.RenderScaling)
targetHeight = ceil(max(0, arrangedHeight) * TopLevel.RenderScaling)
```

然后每个非零轴向上量化到 16 physical-pixel bucket；bucket 不得超过安全上限，codec 最终按保持宽高比的最大边界解码。
RenderScaling、Bounds 或 Stretch 约束使 bucket 改变时才请求新 decoded key，桶内 1 px 布局抖动不重新解码。

- `Auto`：使用有效 arranged width；只有控件自身存在有限 `Height`/`MaxHeight`，或父布局在 Measure 时提供有限高度上限时，才把高度作为解码边界。无界内容布局（例如 Masonry 子项）只按宽度桶解码，避免解码结果的固有高度反向改变布局并形成尺寸反馈环。两个轴都无效时等待真实 Arrange 信号，不启动原图下载后的无界解码。
- `Original`：请求原始尺寸，仍受原始尺寸、像素和 decoded-byte 上限。
- `Explicit`：使用公开 `DecodePixelWidth/Height` 物理像素边界，至少一轴大于零；明确的 0 x 0 配置不发起请求，控件返回
  `InvalidSource` 状态。
- Avatar：按最终头像逻辑尺寸和 scaling 计算，不公开解码尺寸属性。
- Preview cover/thumbnail：按 cover presenter 的物理尺寸；当前完整图按 viewport 与缩放策略所需的最大初始尺寸，交互放大超过
  当前 bucket 时可请求更高分辨率并保留旧租约到替换完成。

尺寸计算在 UI dispatcher 读取 Visual 状态，生成纯值 `ImageLoadRequest` 后，loader 不再持有 Control/TopLevel。跨 TopLevel 移动
或 scaling 变化由 controller 递增 generation 并重新计算。

## 线程与提交边界

- URI 规范化、cache 查询、网络/stream 读取、摘要、raster/SVG 内容校验、`SvgSource.Load(stream, ...)` 和允许后台执行的 raster
  decode 不占用 UI dispatcher。
- codec 明确声明是否需要 dispatcher；scheduler 为该 codec 切换，但仍受 decode 并发预算控制。
- Loader event/diagnostics 不操作 AvaloniaObject；订阅方负责自己的线程切换。
- Control 的 `Image`、`LoadState`、`LoadError`、`LoadProgress`、pseudo-class 和事件提交全部在 UI dispatcher。
- generation 的最终检查必须与属性提交处于同一个 dispatcher work item；只在 await 前检查不足以阻止 stale result。
- 释放租约可从任意线程调用；cache/codec 把实际图像释放调度到其声明的合法线程，不要求调用方猜测。
- `SvgImage` 的创建、`Source` 赋值、`Size` 首次读取和 `Source` 解除只在 UI dispatcher；owned wrapper 缓存纯值 `Size`，后台
  measure/cache clear/dispose 不再访问线程绑定 AvaloniaObject。后台释放只异步 post，不能同步等待 UI。

UI 线程和 cache/coordinator lock 内禁止 `.Wait()`、`.Result`、`GetAwaiter().GetResult()` 或同步 dispatcher call。上游同步 SVG
模型构建在开始/结束检查 cancellation，构建中的最坏工作量由 XML、path、引用和嵌入资源预算限制。

进度回调需要节流，默认每 50 ms 或每新增 64 KiB 至多发布一次，并始终发布阶段切换/最终快照。节流发生在 loader，不用 UI
timer；多个 waiter 对同一下载各自获得快照，取消的 waiter 不再接收进度。

## 默认并发与背压

| 平台 | 下载/读取默认 / hard cap | decode 默认 / hard cap | 原因 |
| --- | --- | --- | --- |
| Desktop | 6 / 32 | `min(4, max(1, ProcessorCount - 1))` / 8 | 利用网络并行且保留 UI/业务 CPU |
| Browser | 4 / 8 | 2 / 4 | 限制 WASM heap 和主线程/worker 压力 |

配置只能设置正整数并受平台 hard cap 约束。并发槽从实际 I/O/decode 开始到 owned resource 释放后才归还；排队请求不打开
stream、不创建 response、不占 decoded buffer。单个大响应通过 streaming byte limit 产生背压，不允许先无限复制到多个
MemoryStream。

Loader snapshot 和 event 只暴露有界 diagnostics：队列长度、active count、cache byte/entry count、hit/miss、取消和错误码计数。
不包含完整 URI、header、partition、byte buffer 或活动 Control 引用。`LoadEvent` 逐订阅者隔离非致命异常：诊断观察者抛出的普通
异常不会改变图片请求结果，也不会阻止后续观察者收到同一事件；`OutOfMemoryException`、`StackOverflowException` 和
`AccessViolationException` 等致命异常仍不会被吞掉。订阅方负责 `-=`；AtomUI 控件不依赖该 diagnostics event，loader dispose
会清空 handler 列表。

## AOT 与裁剪

统一图片加载必须遵守 [AOT 编程规范](../../../engineering/development/aot-programming-guidelines.md)和
[AOT 与裁剪架构](../../foundations/aot-and-trimming.md)：

- source reader、codec、owned service 和 package integration 全部显式代码注册。
- 不使用程序集扫描、`Type.GetType(string)`、反射构造、动态代理或运行时生成序列化 metadata。
- file-cache metadata 使用手写 binary contract 或 source-generated serializer，并带 schema version。
- `ImageSourceConverter` 是直接引用的 converter 类型，不按名称反射查找。
- AXAML 只绑定公开/生成式可保留成员；`AsyncImage` Theme 与 Avatar Theme 的 Source/状态属性进入正常 Control descriptor/asset
  注册，不通过运行时枚举属性。
- `UseCommonControls()` 的 Package Core 以真实静态调用保留 image service factory、raster reader/codec、
  `SvgImageCodec`、安全验证器和直接引用的 SVG dependency 类型；AsyncImage/Avatar 的契约片段由官方 TypeMap 条件选择。
- HTTP transport 的实例在首次网络 source read 时才创建，以避免未使用图片应用的启动对象；网络 reader 与 transport
  类型仍属于 `UseCommonControls()` 的静态能力闭包。这一延迟只承诺减少未使用路径的运行时构造，不承诺链接器删除
  `System.Net.Http` 或缩小 NativeAOT 文件，体积结论必须来自同口径发布测量。
- 未被应用显式注册的自定义 codec/reader 不作为动态 fallback 保留。

Browser 的交付门槛同时包含裁剪运行与 AOT 的真实启动、图片/主题行为和未用类型保留检查。
既往 Gallery 环境失败不构成永久豁免，小型 TypeMap 原型通过也不代表完整图片管线已验收。当前源码与已采纳目标的状态见
[AOT 与裁剪架构](../../foundations/aot-and-trimming.md#1-状态与事实边界)，验收规则见
[AOT 编程规范](../../../engineering/development/aot-programming-guidelines.md)。不能以全量注册绕过 Browser 精细裁剪要求。

当前实现不得复制 `AsyncImageLoader.Avalonia` 的静态 service locator 或通过反射发现 loader。可借鉴的是请求取消、缓存和
attached-control 生命周期经验，但 AtomUI 的正式边界是 Application scoped loader、显式 registry、租约和安全校验。

## Application 销毁所有权

`ApplicationScope` 拥有 image service 的 attach/dispose，不由首个 Window、最后一个 Control 或 Gallery 页面推断应用
结束。受控桌面 lifetime 退出、宿主显式销毁 application scope、测试 teardown 和启动失败必须汇合到同一幂等 dispose 路径。

Browser single-view lifetime 通常与页面进程同寿；仍必须实现 scope dispose 供测试、热重载宿主和显式 teardown 调用。
即使宿主最终由进程回收，也不能以此为理由省略 CTS、response、stream、cache entry 和事件订阅的释放实现。
