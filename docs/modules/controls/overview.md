# AtomUI.Controls 模块概览

`AtomUI.Controls` 是公共控件和通用 Primitives 模块。它依赖 `AtomUI.Core`、`AtomUI.Controls.Shared`、`AtomUI.Fonts.AlibabaSans`、`AtomUI.Icons.AntDesign` 和 `AtomUI.Generator`。

## 职责

- 提供桌面控件包复用的基础控件、抽象控件和 Primitives。
- 承载至少被两个产品终端真实复用的平台无关抽象 Control Token 定义；抽象定义层无 identity，
  Desktop/Mobile 产品包分别拥有 `sealed` 终端 Token。
- 提供公共主题 Provider：`CommonControlThemesProvider` 与 `BrowserCommonControlThemesProvider`。
- 注册公共控件 Token、本地化 Catalog 和内置 Translation Bundle。
- 提供 Icon、ItemsControl、ScrollViewer、Form、Watermark、QRCode、Badge 抽象等公共能力。

## 注册入口

入口位于 `src/AtomUI.Controls/ThemeManagerBuildExtensions.cs`：

```csharp
builder.UseCommonControls();
```

该方法会：

1. 注册源生成器生成的公共控件 Token 类型。
2. 根据 `RuntimePlatform.Features.SupportsNativeWindow` 选择桌面或浏览器公共主题 Provider。
3. 通过生成式模块注册把公共 Catalog 和内置翻译加入 `builder.Localization`。

`UseDesktopControls()` 会先调用 `UseCommonControls()`，所以普通桌面应用无需单独调用本方法。

## 图片加载职责

`AtomUI.Controls` 拥有：

- `AsyncImage` 通用异步显示控件及 Theme。
- `ImageLoadController`，统一 `AsyncImage` 与 Avatar 的 generation、取消、fallback、progress、状态提交和结果租约。
- Avatar 的 `Source`/`FallbackSource`/`RequestOptions` 统一 API，并删除 `Src`、`BitmapSrc` 和 proposed `BitmapUrl`。
- 统一 `SvgImageCodec`、Avalonia SVG owned wrapper，以及在 `UseCommonControls()` 中对该 codec 和 Shared image service 的显式注册。

Controls 不拥有 HTTP、memory/file cache、全局 scheduler 或第二个 loader。`UseCommonControls()` 幂等地补齐
Shared 默认图片注册并添加本包 SVG codec；`UseDesktopControls()` 继续通过 Common 得到同一个 application-scoped loader。
codec 只消费 Shared 已按当前安全策略验证的不可变 SVG 字节，始终使用 stream overload，不自行打开 URL/File；`SvgImage` 创建、
Source/Size 访问和释放遵守 UI dispatcher ownership。
准确 API 和生命周期只在 [统一图片加载系统](../../architecture/systems/image-loading/overview.md)及其
[控件 API](../../architecture/systems/image-loading/control-apis.md)中定义；底层类型见
[公共契约](../../architecture/systems/image-loading/public-contracts.md)。

## Motion 布局与接入职责

[Motion 系统](../../architecture/systems/motion/overview.md)的重构目标中，Controls 拥有可复用的内容视口/布局原语和必要模板接入，
Core 拥有动效定义与播放协议。布局原语只承担自然测量、占位、实际视口和完整内容排列，不成为所有动画必须使用的 Actor。
普通视觉动效优先复用已有模板节点；需要独立变换或裁剪时由 ControlTheme 静态声明，不运行时重组内容树。

`ContentViewport` 已实现公开的 `IMotionLayout`，并接入内容展开消费者；纯视觉动效直接使用实际表面。
旧通用 Actor 类型已删除。第三方/AOT、几何输入与实际呈现的证据统一见
[Motion 验证状态](../../architecture/systems/motion/verification.md)。

## 关键目录

`Primitives/DashedBorder` 与其派生 `PixelAlignedBorder` 统一承载边框绘制、内容圆角裁剪和小数缩放适配。
共享的数值计算、绘制缓存、裁剪所有权、缩放订阅生命周期及视觉兼容约束由
[边框渲染架构](../../architecture/systems/rendering/border-rendering.md)拥有；该文档同时区分共享实现、消费模板与原生平台的验证边界。
具体消费控件的文档继续拥有模板组合与语义部件契约。

| 目录 | 说明 |
|---|---|
| `Primitives/` | 桌面控件复用的基础装饰、动效、视觉层 |
| `Icon/` | 图标控件与图标主题 |
| `ItemsControl/` | ItemsControl 相关反射扩展和主题 |
| `Form/` | 表单验证基础能力 |
| `Grid/`、`FlexPanel/` | 布局基础 |
| `Badge/`、`Buttons/`、`Select/` | 多个桌面控件复用的抽象层 |

具体桌面控件文档应放在 [../../controls/desktop/overview.md](../../controls/desktop/overview.md)，本模块文档只解释公共基础层。
Control Token 定义层的提取门槛、Generator 扁平化和运行时隔离见
[Control Design Token 继承架构](../../architecture/systems/theming/control-design-token-inheritance.md)。该能力不表示仓库已经存在
Mobile 终端或共享 Token 基类。
