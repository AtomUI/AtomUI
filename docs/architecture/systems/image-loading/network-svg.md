# 网络 SVG 加载设计

> 状态：当前架构。本文定义统一图片加载系统对 SVG 的格式、安全、缓存、线程和平台契约；
> [系统概览](overview.md)、[公共契约](public-contracts.md)、[管线与生命周期](pipeline-and-lifecycle.md)、
> [缓存与安全](caching-and-security.md)、[平台与 AOT](platforms-and-aot.md)和[验证门禁](verification.md)必须与本文一致。

## 设计定位

SVG 是统一图片系统支持的静态矢量格式，不是独立 loader，也不允许控件或 SVG renderer 建立第二条网络通道。HTTP/HTTPS
SVG 主文档与 raster 图片共用 `ImageSource`、`HttpImageTransport`、请求合并、缓存、取消、诊断和结果租约；
SVG 专属逻辑只负责受限 XML/CSS 验证和矢量模型构建。

核心不变量：

- `HttpImageTransport` 是 SVG 主文档的唯一网络入口；codec 只接收已经读取完成的不可变字节。
- SVG renderer 不接收源 URL、文件路径或可用于解析相对外部资源的 `BaseUri`。
- 网络、File、Storage、Bytes、Stream 和 Asset SVG 使用同一个静态安全子集；Asset 可信不等于允许隐式 I/O。
- XML 安全和不可绕过的资源上限由 AtomUI 先验证；可恢复的 SVG 一致性问题由应用级一致性模式决定，上游 renderer 的
  secure mode 作为第二道防线。
- 危险内容返回 typed error，不通过删除节点、忽略错误或静默降级制造“成功但内容已改变”的结果。兼容模式只允许本文明确列出的
  可恢复一致性问题，并把通过验证的原始字节原样交给 renderer。
- SVG 结果保持矢量语义；控件请求的 raster decode 尺寸不触发预栅格化，也不产生低分辨率放大。

## 依赖基线

`AtomUI.Controls` 使用以下经过验证的基线：

| Package | Version | 用途 |
| --- | --- | --- |
| `Svg.Controls.Avalonia` | `12.0.0.15` | `SvgSource`、`SvgImage` 与 Avalonia 绘制桥接 |
| `Svg.Model` | `5.2.1` | `SvgParameters` 与 document load options |
| `Svg.Custom` | `5.2.1` | `SvgDocumentLoadOptions`、processing mode 与外部资源策略 |

`AtomUI.Controls.csproj` 对源码直接使用的 package 建立显式引用，不能只依赖传递引用碰巧存在。升级这些依赖时必须重新验证
本文的 processing mode、外部资源策略、DTD 行为、线程归属、Browser 和 NativeAOT 结论。

AtomUI 固定使用：

```csharp
new SvgParameters(
    Entities: null,
    Css: null,
    CurrentColor: null,
    LoadOptions: new SvgDocumentLoadOptions
    {
        ProcessingMode = SvgProcessingMode.SecureStatic,
        ExternalResources = SvgExternalResourcePolicy.SameDocumentAndDataOnly,
        PreserveUnknownElements = false,
        PreferSvg2Href = true
    });
```

不得调用 `SvgSource.Load(string path, ...)` 或其他让上游自行打开 URL/File 的入口，也不得修改
`SvgDocument.DisableDtdProcessing`、`ResolveExternalImages`、`ResolveExternalElements` 等进程级静态状态。

## 公共配置

SVG 一致性模式和资源预算通过 `ImageLoadingOptionsBuilder.Svg` 配置：

```csharp
public enum SvgConformanceMode
{
    Compatible,
    Strict
}

public sealed class SvgImageLoadingOptionsBuilder
{
    public SvgConformanceMode ConformanceMode { get; set; } =
        SvgConformanceMode.Compatible;

    public long MaxDocumentBytes { get; set; } = 4L * 1024 * 1024;
    public long MaxXmlCharacters { get; set; } = 8_000_000;
    public int MaxElementCount { get; set; } = 20_000;
    public int MaxAttributeCount { get; set; } = 100_000;
    public int MaxElementDepth { get; set; } = 256;
    public int MaxReferenceDepth { get; set; } = 64;
    public long MaxPathDataCharacters { get; set; } = 2_000_000;
    public int MaxEmbeddedImageCount { get; set; } = 16;
    public long MaxEmbeddedImageBytes { get; set; } = 8L * 1024 * 1024;
}
```

所有值在 `UseAtomUI()` 构建阶段验证并冻结。SVG 主文档同时受 `MaxResponseBytes` 和 `MaxDocumentBytes` 约束，取更严格者。
`ConformanceMode` 是同一个 Application loader 的全局策略，不进入 `ImageRequestOptions`，控件和单次请求不能覆盖。配置不提供
允许 script、DTD、外部网络/File/Asset、`foreignObject` 或外部字体的开关，也不接受任意 validator delegate。

## 校验分层与一致性模式

SVG 校验分为三个互不替代的层级：

| 层级 | 可配置性 | 契约 |
| --- | --- | --- |
| 安全边界 | 不可关闭 | 安全 XML reader、DTD/entity、script、事件属性、`foreignObject`、`xml:base`、外部资源、危险 CSS、递归 SVG data URI |
| 资源预算 | 应用级有界配置 | 文档字节/字符、元素/属性/深度、path、引用深度、嵌入 raster 数量和成本；必须为有效正值且不能绕过全局图片上限 |
| 格式一致性 | `Compatible` 或 `Strict` | 只决定安全边界内、本文明确列出的可恢复 SVG 规范问题是否接受 |

第一版一致性模式只区分重复 `id`：

| 模式 | 默认值 | 重复 `id` 行为 |
| --- | --- | --- |
| `Compatible` | 是 | 接受重复 `id`；AtomUI 使用元素出现次序建立无歧义的内部引用图，renderer 接收完全相同的原始字节并执行其确定性 id 归一化 |
| `Strict` | 否 | 完成不可关闭的安全与复杂度校验后，若存在重复声明则返回 `InvalidImageData` |

XML 不完整、根元素或 namespace 错误不是一致性放宽项；两种模式均拒绝。公共契约不提供 `Disabled`、per-request 模式或自定义验证器，
因此安全校验、资源预算和 cache 验证结论不能被单个 Avatar、`AsyncImage` 或第三方调用方绕过。

## 支持的静态子集

允许：

- 标准静态 SVG shape、path、group、defs、gradient、pattern、clip、mask、filter 和 text。
- 同文档 `href="#id"`、`xlink:href="#id"` 与 CSS `url(#id)`。
- 内联 style 和 `<style>`；CSS 引用只能指向同文档 id。
- 内联 SVG font；不允许外部 font source。
- `data:image/png`、`data:image/jpeg` 和 `data:image/webp` raster image，且必须通过独立资源预算与 raster header probe。
- `width`、`height`、`viewBox`、`preserveAspectRatio` 和 SVG 2 `href`。
- `Compatible` 模式下允许重复 `id`；重复声明本身不放宽任何 URI、安全或资源预算规则。

拒绝：

- `DOCTYPE`、XML entity declaration、外部 entity 和自定义 entity expansion。
- `<script>`、任意大小写的 `on*` 事件属性和动态交互模式。
- `<foreignObject>`、`xml:base`、XML stylesheet processing instruction。
- HTTP/HTTPS、File、`avares`、相对文件路径等外部 image/use/filter/paint/stylesheet/font 引用。
- CSS `@import`、外部 `@font-face src` 和非 `#id` 的 `url(...)`。
- `data:image/svg+xml`、data font、未知 data MIME 和递归 SVG。
- 超过文档、结构、路径、引用图或嵌入资源预算的内容。
- `Strict` 模式下的重复 `id`。

动画元素不会建立时间轴；`SecureStatic` 只生成静态绘制结果。动画图片与可控制时间轴仍不属于统一图片系统。

## MIME 与格式识别

`ImageContentValidator` 先按受限前缀确认 XML/SVG 候选，再由 `SvgContentValidator` 使用安全 XML reader 验证根节点必须是
`http://www.w3.org/2000/svg` namespace 下的 `svg`。只有完整结构验证成功才形成 `ImageContentFormat.Svg`。

真实内容为 SVG 时允许以下声明：

```text
image/svg+xml
application/xml
text/xml
application/octet-stream
缺失 Content-Type
```

`text/html`、`application/xhtml+xml`、非 SVG XML，以及声明为具体 raster MIME 但实际为 SVG 的响应必须拒绝。HTML/XML/SVG
polyglot 不能因为包含 `<svg>` 子串而通过；格式判断以安全 XML reader 读取到的唯一根元素和 namespace 为准。

## 内容验证组件

Shared 单层 `ImageLoading/` 目录增加：

```text
SvgImageLoadingOptions.cs
SvgImageLoadingOptionsBuilder.cs
SvgContentValidator.cs
SvgContentMetadata.cs
SvgCssReferenceValidator.cs
SvgDataImageValidator.cs
ImageSecurityPolicy.cs
```

职责固定为：

- `SvgContentValidator`：编排不可关闭的安全校验、资源预算和一致性模式；负责安全 XML 读取、根元素、元素/属性/深度/路径字符预算、
  危险元素、URI-bearing attribute、重复 id 和引用图验证。
- `SvgCssReferenceValidator`：按 CSS token 语义识别 string、comment、`url()`、`@import` 和 `@font-face src`；不得使用正则或简单
  `Contains` 作为安全解析器。
- `SvgDataImageValidator`：解析 data URI、严格 base64/percent decoding、MIME allowlist、累计字节和数量限制。
- `SvgContentMetadata`：不可变保存 intrinsic size、viewBox、结构计数、嵌入 raster 估算和引用图信息。
- `ImageSecurityPolicy`：集中提供 `Version = 2`；reader 产生的 raw content 使用 version `0`，只有完整验证后的副本才标记为当前
  version 并允许进入 encoded/file cache，不能继续把版本默认值散落在 record 构造参数中。

XML reader 固定使用：

```csharp
new XmlReaderSettings
{
    DtdProcessing = DtdProcessing.Prohibit,
    XmlResolver = null,
    MaxCharactersInDocument = options.MaxXmlCharacters,
    MaxCharactersFromEntities = 0,
    IgnoreComments = true,
    IgnoreProcessingInstructions = false
};
```

验证循环定期检查 cancellation。处理 instruction 只允许 XML declaration；其他 processing instruction 按不安全 vector 内容拒绝。
验证器只读取同一份不可变 byte array，不重写、清洗或重新序列化文档；验证通过后 codec 处理的必须是完全相同的字节。

### 重复 id 与引用图

重复 `id` 不能通过删除现有唯一性检查就直接支持，因为元素引用图仍必须正确执行深度、环和复杂度预算：

1. parser 为每个元素出现位置分配内部唯一 node identity；该 identity 不等同于文档中的原始 `id` 字符串。
2. 父子元素以权重为 0 的 containment edge 相连，显式本地引用以权重为 1 的 reference edge 相连；这样引用外层元素时不会丢失
   带 `id` 的嵌套子树依赖，而 `MaxReferenceDepth` 仍只统计实际引用跳数。
3. 首次声明表把每个原始 `id` 映射到第一个声明节点；`href="#id"`、`xlink:href="#id"` 和 CSS `url(#id)` 都解析到该节点，
   与固定依赖基线 renderer 的解析语义一致。
4. 后续同名声明仍是独立 source node，其自身属性、子树和出边继续参与元素、属性、path、引用深度与环检测，不能合并进第一个
   节点或跳过。
5. parser 记录第二个同名声明；`Strict` 在不可关闭的安全与复杂度校验完成后返回 `InvalidImageData`，避免重复 id 掩盖
   `UnsafeVectorContent` 或资源预算错误；`Compatible` 保留全部节点，在上述图模型通过后把未修改字节交给 renderer。

因此实现不能继续使用“原始 id 字符串就是唯一 graph node”的 `_ids`/`_references` 假设。依赖升级时必须重新验证 renderer 对重复
id 的首声明解析和确定性归一化行为；若该行为变化，不能在没有同步更新契约、测试和 codec/security version 的情况下沿用兼容结论。

嵌入 raster 复用共享的 header probe，不复制 PNG/JPEG/WebP 尺寸算法。每个嵌入图同时检查单图宽高、像素数和
`width * height * 4` checked 估算，累计 encoded bytes 受 SVG 配置限制，累计 decoded estimate 受全局
`MaxDecodedImageBytes` 限制。

## Codec 与缓存身份

Controls 使用唯一的 `SvgImageCodec`：

```csharp
internal sealed class SvgImageCodec : ImageCodec
{
    internal override string Id => "atomui.svg";
    internal override int Version => 2;
    internal override bool IsDecodeSizeDependent => false;
}
```

codec 只匹配 `ImageContentFormat.Svg` 且 probe 携带当前 security policy 生成的 `SvgContentMetadata`。Asset、HTTP、File、Storage、
Bytes 和 Stream 不再按 source kind 分配不同 SVG codec。

decoded key 包含：

```text
cache partition
+ SHA-256 ImageContentId
+ codec id/version
+ security policy version
+ codec-specific decode options
```

SVG key 不包含目标 decode width/height；raster key 继续包含物理像素尺寸桶。Loader 必须先按 SourceKey 解析或验证
SourceVersion，取得 snapshot 对应的 ContentId 后才能形成 decoded key。不存在“根据来源猜测 codec 并在来源验证前返回 decoded
entry”的 candidate fast path。这样既保留 content-addressed raster/矢量复用，也保证同一 URL 或文件路径被替换后不会显示旧图。

`ConformanceMode` 在 Application loader 构建时冻结，因此同一个 loader 不会用不同模式解释同一份缓存内容，也不需要把模式加入
per-request SourceKey/DecodeKey。persistent content 读取后仍按当前 loader 的完整 SVG 策略重新验证；不能只凭历史 metadata 跳过
一致性检查。未来若引入 per-request 模式，必须同时把策略身份加入 source operation、probe/encoded validation stamp 和 decoded
key/cache，不能只给控件增加一个属性。

source-resolution in-flight 的编码产物必须已经通过 `ImageContentValidator` 并计算 ContentId。
`ImageSecurityPolicy.Version` 固定为 `2`；raw reader output 的 version 为 `0`。encoded memory/file cache 只存储标记为当前
version 的已验证内容；持久缓存读取发现 schema、摘要、ContentId 或 policy version 不匹配时删除并按 miss 处理。策略升级后旧
字节可以重新下载或重新验证，但不能直接进入 codec。

SVG decoded cache 的字节成本按以下 checked 估算：主文档 encoded bytes、嵌入 raster decoded estimate、元素和属性模型成本之和，
并设置最小非零成本。不能按 `viewBox.Width * viewBox.Height * 4` 把纯矢量画布错误当成完整像素缓冲，也不能按对象数量把复杂
path 当成零成本。

## 解码、线程与释放

执行顺序：

```text
normalize request
-> source snapshot lookup
-> validate/refresh SourceVersion
-> encoded content lookup or source read
-> ImageContentValidator + SvgContentValidator
-> SHA-256 ImageContentId
-> SvgImageCodec selection
-> exact decoded cache/in-flight lookup
-> decode scheduler: SvgSource.Load(stream, secure parameters)
-> UI dispatcher: create SvgImage, assign Source, cache Size
-> decoded cache insert
-> UI dispatcher: generation check and control commit
```

`SvgSource.Load` 在 decode scheduler 执行；`SvgImage` 是 AvaloniaObject，只能在 UI 线程创建、设置 `Source`、读取 `Size` 和解除
`Source`。owned wrapper 在创建时把 `Size` 缓存为普通值，后续 `IImage.Size` 不再访问 `SvgImage.Size`。这条规则同时适用于
后台 cache clear、loader dispose 和最后租约释放。

后台释放只向 UI dispatcher 投递一次，不同步等待 UI，也不在 coordinator/cache lock 内投递。UI 线程不得使用 `.Wait()`、
`.Result`、`GetAwaiter().GetResult()` 或同步 dispatcher call 等待 decode；decode worker 只允许异步等待 UI 创建 wrapper。结果在
UI 创建前取消时释放未发布的 `SvgSource.Picture`；创建后取消时按正常 owned wrapper 路径释放。

上游模型构建是同步且没有可靠强制中断 API。AtomUI 在 XML 验证循环中响应取消，并在模型构建前后检查 token；构建期间取消
不会提交结果，返回后立即释放。元素、路径和嵌入资源预算负责限制单次不可中断计算量，不使用 `Thread.Abort` 或无界
`Task.Run` 规避。

## 控件集成

`AsyncImage`、Avatar、Masonry 中的 `AsyncImage` 和 ImagePreviewer 不增加 SVG 专属 public API：

- `AsyncImage.Source` 自动接受网络和本地 SVG，loading/fallback/reload 状态与 raster 一致。
- Avatar 使用同一 `Source` 加载网络头像；首次有效 Arrange 提供尺寸后启动，不依赖旧 `Bounds` 或固定延迟。
- Masonry 只负责布局；SVG 请求期间仍由 `AsyncImage.LoadingContent` 显示 skeleton。
- ImagePreviewer 的 cover、thumbnail 和 full source 均可为 SVG；同一 source 共享同一 decoded vector entry，zoom/rotate/fit 不先
  栅格化成缩略尺寸。
- Source 快速替换、页面切换、Previewer current 切换和 detach 必须取消 waiter、保留 generation 防陈旧提交并释放租约。
- 所有控件继承 Application loader 冻结的 `Svg.ConformanceMode`；`ImageRequestOptions`、Source 类型和 fallback 均不能覆盖该模式。

## Error 契约

| 场景 | Error code |
| --- | --- |
| DTD、script、事件属性、外部资源、危险 CSS、`foreignObject` | `UnsafeVectorContent` |
| `Strict` 模式发现重复 `id` | `InvalidImageData` |
| XML 损坏、根元素/namespace 错误、data raster 损坏 | `InvalidImageData` |
| 元素、属性、字符、深度、path 或引用图超限 | `VectorComplexityLimitExceeded` |
| data image 数量或累计 encoded bytes 超限 | `EmbeddedResourceLimitExceeded` |
| intrinsic/final 尺寸、像素或 decoded estimate 超限 | 现有 dimension/pixel/decoded-byte error |
| 上游模型或 Avalonia wrapper 创建失败 | `DecodeFailed` |

错误消息只包含安全来源显示名和失败类别，不回显完整 SVG、data URI、认证 header、cache partition 或被阻止 URL 的敏感部分。

## 平台与 AOT

Desktop 与 Browser 使用相同的安全子集、codec id/version、cache identity 和错误语义。Browser 的主文档下载仍受 Fetch、CORS 和
mixed-content 约束；SVG renderer 不创建第二次 Fetch。File source 在 Browser 继续按平台能力拒绝，Storage/Bytes/Stream SVG
在已有授权和 source contract 下可用。

所有 reader、validator、codec 和 options 由显式代码构造，不扫描程序集、不反射创建类型、不动态生成 XML/CSS serializer。
`SvgConformanceMode` 是封闭 enum，配置面不接受 delegate、反射类型名或运行时加载的 validator。
Package Core 的真实静态调用必须保留 `SvgImageCodec` 及其直接引用的 SVG dependency 类型。依赖升级后必须重新执行 Desktop
NativeAOT 发布/启动、Browser 裁剪发布/启动和 Browser AOT 发布/启动；精细裁剪契约见 [AOT 架构](../../foundations/aot-and-trimming.md)。

## 验证

自动化测试至少覆盖：

- 合法 DiceBear 风格 SVG、同文档 defs/use、inline CSS 和 data raster。
- 使用 Gallery `AvatarShowCase/AntDesign.svg` 这类重复 `id` 资源验证默认 `Compatible` 成功；同字节在 `Strict` 下返回
  `InvalidImageData`。
- 重复 id 出现在被引用节点、引用发起节点、环和深度边界时，按“内部 occurrence node + 原始 id 首声明解析”计算，不能通过合并或
  跳过重复节点绕过复杂度限制。
- 每种危险 XML/CSS/URI 输入及每个资源预算边界。
- `Compatible` 与 `Strict` 对 DTD、script、外部资源、危险 CSS、递归 SVG 和全部资源预算得到相同拒绝结果。
- HTTP MIME、缓存、重验证、`CacheStorage=None`、取消、source/decode coalescing 和 security policy 升级。
- off-thread decode、UI thread Size/Measure/Draw、后台 cache clear 和 loader dispose。
- Avatar 首次 Window layout、Masonry skeleton、ImagePreviewer cover/full/切换/detach 和 Masonry 到 Previewer 导航无死锁。
- `Empty`、`Result`、Gallery shell 等直接使用 `Svg.Controls.Avalonia` 的既有场景，防止依赖升级造成相邻回归。
- Desktop、Browser、NativeAOT 与 Gallery 实际网络 SVG 场景。

完整命令与完成门禁见[图片加载验证与完成门禁](verification.md)。
