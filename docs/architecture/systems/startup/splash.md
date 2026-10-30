# Splash

启动 Splash 在 AtomUI 内核提交之前显示，覆盖提交与主窗体构建的耗时。主窗体首个合成批次完成后，Splash 立即隐藏并随后关闭。生命周期中的
位置见[启动生命周期](lifecycle.md)；接入示例见[启动指南](../../../guides/startup/overview.md#2-启用-splash)。

## 1. 类型

全部位于 `src/AtomUI.Controls/Splash/`，命名空间 `AtomUI.Controls`（`SplashMode` 位于 `AtomUI.Core`，命名空间 `AtomUI`）。

| 类型 | 说明 |
|---|---|
| `Splash : TemplatedControl` | 属性 `Logo`、`BackgroundImage`（`IImage?`）、`Title`、`Message`（`string?`）、`Progress`（`double`，0–1）、`IsIndeterminate`（默认 `true`）、`Status`（`SplashStatus`）。布局完全由 ControlTheme 决定。可选模板部件 `PART_CloseButton`（任意元素）：错误态下点击即请求关闭。伪类 `:error`（`Status == Error`）、`:opaque`（平台不支持透明窗口）。 |
| `SplashWindow : Avalonia.Controls.Window` | Splash 宿主窗口，资源完全自带：全局资源表、默认主题、可选自定义主题；不依赖任何 AtomUI 主题。错误态另支持 Esc 关闭。 |
| `SplashProgressBar : Control` | 无模板，在 `Render` 中绘制轨道与确定进度；属性 `Value`、`IsIndeterminate`、`TrackBrush`、`IndicatorBrush`、`CornerRadius`。 |
| `SplashStatus` | `Loading`、`Error`。 |
| `SplashMode` | `Off`、`Always`、`Auto`。 |
| `SplashDefaultTheme` | 默认主题（x:Class 资源字典，公开）。 |
| `SplashOptionsBuilder` / `SplashBuilderExtensions.UseSplash` | 配置 API（§3）。 |

两处实现约束来自 Avalonia 的行为，修改时必须保留：

- **样式键**：Avalonia 的 `Window` 把 `StyleKeyOverride` 固定为 `typeof(Window)`。`SplashWindow` 必须覆盖为
  `typeof(SplashWindow)`，否则以 `SplashWindow` 为键的 ControlTheme 既查不到，显式指定也因 `TargetType` 不匹配而不生效，
  窗口会退回带标题栏的普通窗口。
- **不确定进度不能做布局元素的属性动画**：`Visual.SynchronizeCompositionProperties` 在每次同步时都会改写布局元素合成
  视觉的 `Offset`、`Opacity` 等属性，而直接赋值会移除该属性上正在运行的合成动画，进度块会停在布局原点。
  `SplashProgressBar` 因此在挂接的 `CompositionCustomVisual` 上由渲染线程按合成时钟绘制不确定进度，不经过布局。

## 2. 显示判定

全部满足才显示：

1. 已调用 `UseSplash`（不调用即不显示）。
2. 模式为 `Always`；或为 `Auto` 且 `RuntimeFeature.IsDynamicCodeCompiled`（JIT，含 ReadyToRun）。NativeAOT 启动足够快，
   `Auto` 不显示。
3. `ApplicationLifetime` 是 `IClassicDesktopStyleApplicationLifetime`。
4. 不是设计模式。
5. 后台 Prepare 成功（Splash 需要全局资源表）。

显示过程：创建 `SplashWindow`，合并资源后 `Show()`，用受控运行循环驱动 Dispatcher 直到 Splash 首个合成批次渲染完成
（上限 5 s）。创建失败或平台不支持受控运行循环时写日志并跳过 Splash，启动继续。Logo、背景图的工厂在此时于 UI 线程
调用（配置回调执行时还无法创建位图）。

## 3. 配置 API

```csharp
// AtomUI.Controls
public static IAtomUIBuilder UseSplash(this IAtomUIBuilder builder,
                                       SplashMode mode = SplashMode.Auto,
                                       Action<SplashOptionsBuilder>? configure = null);

public sealed class SplashOptionsBuilder
{
    public SplashOptionsBuilder WithLogo(Uri source);                  // avares:// 或文件 URI
    public SplashOptionsBuilder WithLogo(Func<IImage> factory);        // 矢量图等自定义来源
    public SplashOptionsBuilder WithBackgroundImage(Uri source);
    public SplashOptionsBuilder WithBackgroundImage(Func<IImage> factory);
    public SplashOptionsBuilder WithTitle(string title);
    public SplashOptionsBuilder WithMessage(string message);
    public SplashOptionsBuilder WithControlTheme<TTheme>() where TTheme : ResourceDictionary, new();
}
```

同一 builder 只能调用一次 `UseSplash`。`AtomUI.Core` 不引用 `AtomUI.Controls`：`UseSplash` 向 builder 登记一个内部的
Splash 呈现器，Core 只通过该接缝显示 Splash、汇报进度、切换错误态与交接。泛型 `new()` 对裁剪与 AOT 安全。

## 4. 默认主题

`src/AtomUI.Controls/Splash/Themes/SplashDefaultTheme.axaml`。

- **启动专用主题**：文件按控件规范位于 `Themes/`，但 `AtomUI.Controls.csproj` 给它设置逻辑资源路径
  `Splash/SplashDefaultTheme.axaml`，使包主题资产生成器不把它登记为提交后的普通主题资产；`SplashWindow`
  在提交前直接实例化它。页面内预览 Splash（例如 Gallery 示例页）需要自行合并该字典。
- **只依赖全局 Token**：Splash 在内核提交之前显示，此时只有 Prepare 产出的全局资源表，没有任何控件主题与控件 Token。
  默认主题只使用 `{atom:SharedTokenResource …}`，不绑定字体（只用系统默认字体）。
- **外观**：卡片宽 420（`ColorBgContainer`、`BorderRadiusLG`、`BoxShadowsSecondary`，内边距 `PaddingLG`），可铺满背景图；
  居中纵列依次为 Logo（64 px）、标题（`FontSizeHeading4`、`ColorText`）、消息（`FontSize`、`ColorTextSecondary`）、
  进度条（指示 `ColorPrimary`、轨道 `ColorFillSecondary`）。错误态消息用 `ColorError`，隐藏进度条，显示右上角关闭图标
  （`ColorIcon`）。`:opaque` 下退化为方角、无阴影。窗口无边框、随内容定尺寸、居中、交接前置顶、请求透明背景。
- **可局部覆盖**：模板只描述结构，尺寸、颜色、字号等外观值都写在 ControlTheme 的样式（`^ /template/ …`）里。模板内的
  本地值优先级高于样式；写在模板里的值无法被派生主题覆盖。修改默认主题时必须保持这一点。
- **独立阴影缓存**：`Card` 保留尺寸、背景、圆角、BoxShadow 定制入口和内容裁剪。其同级的 Canvas 承载独立 `Shadow`，
  通过编译绑定跟随 Card 的实际 Bounds、圆角和阴影；Canvas 不参与内容期望尺寸计算，避免阴影反向撑大卡片。
  Splash 不裁剪外侧阴影，窗口默认留出 48 DIP 透明边距供默认阴影自然衰减。`Shadow` 使用 `BitmapCache`，消息和进度
  更新不在该缓存子树内；轮廓、尺寸、阴影资源或渲染缩放变化时允许缓存重建。
  缓存首次生成有渲染和内存成本，不等于零开销；资源由 Avalonia 合成器管理，在视觉移出窗口或缓存替换时释放。
  阴影层不参与命中，`:opaque` 下隐藏。

## 5. 自定义主题

`WithControlTheme<TTheme>()` 指定一个 x:Class 资源字典，内含 `TargetType` 为 `Splash`（可选 `SplashWindow`）的隐式
ControlTheme。它合并在默认主题之后，同键优先。两种写法：

- **整体替换**：直接定义 `Splash` 的 ControlTheme 与模板。
- **局部覆盖**：先合并 `SplashDefaultTheme`，再用 `BasedOn="{StaticResource {x:Type atom:Splash}}"` 派生，只写需要改的
  样式。示例见[启动指南](../../../guides/startup/overview.md#3-自定义-splash-主题)。

**硬约束**：只能使用 Splash 控件、Avalonia 基础元素（`Border`、`Panel`、`Image`、`TextBlock`、`Path` 等）和全局 Token；
不能放 AtomUI 控件（提交前没有主题）；不能依赖自定义字体；图片要小（在关键路径上解码）；模板直接计入 Splash 首帧。

## 6. 交接、错误态与关闭

- **交接**：主窗体首个合成批次完成后立即隐藏 Splash，取消命中与置顶；窗口关闭投递到下一轮 UI 调度，避免在渲染回调中
  同步销毁合成目标触发 Avalonia 合成器重入。UseMainWindow 与传统宿主均遵循此顺序，避免 Splash 消失后主窗体尚未上屏。
- **进度**：框架只在状态切换时发布进度，Splash 默认显示不确定进度；应用经 `UseMainWindow` 的
  `MainWindowContext.Progress` 汇报时，更新消息并切换为确定进度。
- **错误态**：提交失败或主窗体工厂抛错时，`Status` 切换为 `Error`，消息显示异常的根消息；用户点击关闭部件或按 Esc 后
  关闭窗口，启动按[出错与取消](lifecycle.md#7-出错与取消)继续。交接隐藏失败时恢复 Splash 的可见性、置顶与命中。
  错误态隐藏进度条时也停止其合成帧请求；
  普通页面预览被自身或祖先隐藏、或从 VisualTree 脱离时同样停止，重新可见后按当前进度状态恢复。
- **用户关闭**：交接前用户关闭 Splash 视为取消启动并调用 `Shutdown()`。

## 7. 已知问题

- **交接瞬间的短暂停顿**：所有窗口共用一个渲染线程。Splash 保持可见直到主窗体首帧完成，随后立即隐藏，避免出现长时间空档。
- **macOS 主窗体上屏前的渲染线程阻塞**：AtomUI 的 `WithAtomUIDefaultOptions()` 在 macOS 上 Metal 优先，Metal 在渲染线程
  直接呈现，提交占用 UI 线程时 Splash 动画照常上屏（OpenGL 与 Software 经 IOSurface 呈现，需要主线程设置图层内容，提交
  期间帧送不上屏）。但 Avalonia 的 `Window.Show` 先开始渲染、后让窗口上屏；UI 线程在主窗体上屏前仍忙时，主窗体的
  `-[CAMetalLayer nextDrawable]` 可阻塞渲染线程约 1 s（drawable 池耗尽后的系统超时），Splash 在此期间停顿。这是 Avalonia
  多窗口共用渲染线程的行为；Splash 会保持可见直到主窗体首帧完成，不会留下提前隐藏后的长时间空档。
- **Wayland**：不允许应用定位窗口，Splash 的位置由合成器决定。
