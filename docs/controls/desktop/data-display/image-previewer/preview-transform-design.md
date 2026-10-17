# ImagePreviewer 预览变换设计

本文定义 ImagePreviewer 预览图片的基础适配尺寸、用户变换、输入处理、宿主协作和解码尺寸边界。公共定位、来源模型和控件家族关系见
[ImagePreviewer 桌面版架构设计](overview.md)，源码 ownership、生命周期和资源边界见
[ImagePreviewer 桌面版实现原理](implementation.md)。

## 1. 设计定位

ImagePreviewer 的预览面采用与 Ant Design Image 一致的交互语义：图片先在视口内建立可完整显示的基础尺寸，用户变换再作用于该尺寸。
基础适配、缩放、拖拽、旋转、翻转、切图和 reset 使用同一套坐标与边界模型；Desktop native dialog 和 Browser overlay 只提供宿主能力，
不复制变换算法。

该设计覆盖 `ImagePreviewer`、`ImageGroupPreviewer`、`ImagePreviewerDialog`、`ImagePreviewerOverlayHost` 和 `ImageViewer`。图片来源、
编码内容、解码缓存和 loader 并发仍归应用级图片加载系统所有。

## 2. 设计原则

- 基础图片尺寸与用户变换比例是两个独立维度；`scale=1` 表示当前基础适配尺寸。
- 基础适配同时约束宽度和高度，任何宽高比的图片都能在打开时完整显示。
- 所有变换都使用同一个 `PreviewTransformState` 和同一个边界修正算法。
- 宿主只转发属性和操作，不能分别实现缩放、拖拽或位置钳制。
- 缩放以指针位置为中心，拖拽结束、缩放结束、旋转、翻转和视口变化后都执行边界修正。
- 关闭、切换图片和 reset 恢复同一个初始变换状态。
- `minScale`、`maxScale` 约束用户变换比例，不改变基础适配尺寸。
- 显示尺寸可以按视口和缩放级别升级解码，但不能绕过应用级图片安全预算。

## 3. 专项模型与 Public API

### 3.1 坐标和尺寸模型

| 名称 | 单位 | 含义 |
| --- | --- | --- |
| `ImageSize` | 比例尺寸 | 当前 entry 的原始像素宽高；无元数据时回退到 `IImage.Size`，仅用于保持宽高比 |
| `ViewportSize` | logical DIP | `PART_ImageViewerScene` 的可视区域 |
| `BaseImageSize` | logical DIP | 基础适配后的图片布局尺寸 |
| `Scale` | 无量纲 | 用户变换比例，默认 `1` |
| `Translation` | logical DIP | 图片相对视口中心的平移 |
| `RenderScaling` | 无量纲 | TopLevel 的物理像素缩放 |

基础适配目标使用两个边界：

```text
fitScale = min(
    ViewportWidth / ImageWidth,
    ViewportHeight * 0.70 / ImageHeight)

BaseImageSize = ImageSize * max(fitScale, 0)
```

图片布局使用 `BaseImageSize`，`Scale=1` 时完整图片位于视口中心。`0.70` 对齐 Ant Design Image 预览图片的 `max-height: 70%` 语义；
宽度同时受视口宽度约束，因此极宽和极高图片都按较小的比例缩放。

### 3.2 变换状态

```text
PreviewTransformState:
    X       logical DIP
    Y       logical DIP
    Scale   double
    Rotate  degrees
    FlipX   bool
    FlipY   bool
```

初始状态固定为：

```text
X=0, Y=0, Scale=1, Rotate=0, FlipX=false, FlipY=false
```

现有 `ImageMinScale=1`、`ImageMaxScale=50`、`ImageScaleStep=0.5` 和 `IsImageMovable=true` 继续作为公共交互契约。预览新增
`IsWheelZoomEnabled=true`，对应 Ant Design Image 的 `wheel` 能力。`ImageMinScale` 允许小于 `1`，用于把图片缩小到基础适配尺寸以下。

### 3.3 操作语义

| 操作 | 结果 |
| --- | --- |
| Zoom in | `Scale *= 1 + ImageScaleStep`，并限制到 `ImageMaxScale` |
| Zoom out | `Scale /= 1 + ImageScaleStep`，并限制到 `ImageMinScale` |
| Wheel | 根据滚轮幅度计算同一比例，最大单次比例增量为 `ImageScaleStep` |
| Rotate | `Rotate += 90` 或 `Rotate -= 90` |
| Flip X/Y | 切换 `FlipX` 或 `FlipY` |
| Reset | 恢复完整初始状态 |
| Previous/Next | 切换当前项并恢复完整初始状态 |
| Close | 释放宿主并恢复下一次打开的初始状态 |

缩放按钮和变换计算共用同一边界归一化：最小比例必须为有限正数，否则按 `1`；最大比例必须为有限正数且不小于有效最小比例，否则按有效最小比例。缩放按钮在当前比例到达有效边界时禁用。运行时边界改变时立即重新计算按钮状态，并把当前比例限制在新边界内。

## 4. 变体、平台或状态策略

| 场景 | 基础尺寸 owner | 变换 owner | 宿主差异 |
| --- | --- | --- | --- |
| Desktop native dialog | `ImageViewer` | `ImageViewer` | 独立 Window/TopLevel，标题栏提供操作入口 |
| Browser/无 native window | `ImageViewer` | `ImageViewer` | Overlay 提供遮罩、关闭按钮和操作区 |
| 单封面 | `ImagePreviewer` 选择 entry | 打开后的 `ImageViewer` | 关闭态只请求 Thumbnail |
| 多封面 | `ImageGroupPreviewer` 选择 entry | 打开后的 `ImageViewer` | 切图只改变 CurrentIndex |
| Loading | 已提交图片的尺寸 | 保留当前状态 | `:loading:not(:has-image)` 决定 loading presenter |
| 图片失败 | 无可显示 `IImage` 时无基础尺寸 | 清空显示状态 | 错误 presenter 只呈现错误内容 |

Dialog 和 OverlayHost 不拥有 `Scale`、`X/Y`、边界或拖拽状态；它们通过已有 relay bindings 与 `ImageViewer` 连接共享状态。

## 5. 架构、文件结构与职责

| 责任 | owner | 输入 | 输出 |
| --- | --- | --- | --- |
| entry 和 lease | `ImagePreviewEntry` | Source、RequestOptions、解码 bucket | `IImage`、尺寸元数据、加载状态 |
| 基础尺寸 | `ImageViewer` 的布局层 | `ImageSize`、`ViewportSize`、`RenderScaling` | `BaseImageSize` |
| 变换状态 | `ImageViewer` 的变换层 | 操作事件、当前状态 | `PreviewTransformState` |
| 输入归一化 | `ImageViewer` | Pointer、Wheel、Touch、Toolbar | 统一变换操作 |
| 边界修正 | `ImageViewer` | BaseImageSize、ViewportSize、TransformState | 修正后的 `X/Y/Scale` |
| 宿主生命周期 | `AbstractImagePreviewer`、Dialog、OverlayHost | IsOpen、CurrentIndex | host 创建、绑定、释放 |
| 图片绘制 | `ImagePreviewRenderer` | 已提交 `IImage`、BaseImageSize、RenderTransform | 视觉输出 |

变换层可以由 `ImageViewer` 内部稳定职责承载；Dialog 与 OverlayHost 不保留同名的缩放或钳制算法。任何平台新增输入都必须先转换为统一操作，再进入变换层。

## 6. Template、组合与集成契约

`PART_ImageViewerScene` 是视口坐标空间，负责裁剪和承载基础居中布局；`PART_ImageRenderer` 使用 `BaseImageSize` 作为未变换布局尺寸，
再应用 `PreviewTransformState` 生成的 `RenderTransform`。基础尺寸不通过 `RenderTransform` 表达，避免把适配比例和用户缩放混成一个状态。

操作区继续使用现有 `ImagePreviewFloatToolbar` / `ImagePreviewToolbar`。Zoom、Rotate、Flip、Reset、Previous、Next 的事件只表达操作请求，
不直接写图片位置或尺寸。Semantic Part `popup.body` 继续指向 viewer scene，`popup.footer` 和 `popup.actions` 继续指向操作区。

## 7. 核心算法、数据流与生命周期

### 7.1 打开和布局

1. `ImagePreviewEntry` 提交当前 `IImage` 及原始/解码尺寸；显示 tracker 将图片与同一 entry 的原始尺寸一起交给 viewer。
2. `ImageViewer` 测量视口和当前图片尺寸。
3. 按宽高两个约束计算 `fitScale` 与 `BaseImageSize`。
4. 将 `PreviewTransformState` 初始化为 `Scale=1`、零平移、无旋转和无翻转。
5. 居中排列图片，调用 `ClampTransform()`。

视口大小、DPI、当前图片或主题模板改变时重新计算基础尺寸；如果用户已经执行变换，只保留变换状态并重新钳制平移，不能把用户比例写回成新的基础比例。模板首次排列前 scene 尚无 Bounds 时使用 viewer 的有效 Bounds，避免把有效平移按零视口清空。切图/reset 先结束拖拽、释放 pointer capture 并清空捏合累计状态。

### 7.2 指针缩放

滚轮或双击产生以指针位置为中心的缩放请求。给定旧比例 `s`、新比例 `s'`、指针位置 `p` 和当前平移 `t`，先按图片中心坐标换算缩放引起的边缘位移，
再更新平移，使指针下的图片位置保持稳定；随后调用 `ClampTransform()`。

滚轮增量先归一到浏览器的 100 单位刻度：Avalonia 的一个 detent 为 `1`，因此 `WheelDeltaY = PointerWheelEventArgs.Delta.Y * 100`。小数增量保留触控板的连续变化。滚轮比例为：

```text
delta = min(abs(WheelDeltaY / 100), 1)
ratio = 1 + delta * ImageScaleStep
zoom-in  => s' = s * ratio
zoom-out => s' = s / ratio
```

`IsWheelZoomEnabled=false` 时保留滚轮事件给宿主，不改变工具栏、双击和触摸缩放。双击在初始比例时按一个缩放步长放大，已缩放时执行 reset；捏合使用相邻累计 gesture scale 的比值，手势结束后清空累计值。

### 7.3 拖拽和边界修正

拖拽只修改 `X/Y`。结束时以旋转后的包围盒计算实际显示宽高：

- 显示宽高都不超过视口：`X=0,Y=0`；
- 某轴显示尺寸超过视口：该轴的边缘必须覆盖视口对应边界，平移限制在合法区间；
- 旋转角度为 90 或 270 度时交换包围盒宽高；
- `IsImageMovable=false` 时不接受拖拽，缩放和 reset 仍可用。

窗口尺寸、基础尺寸、比例或旋转改变后重复使用同一 `ClampTransform()`，避免拖拽、缩放和 resize 产生不同的边界。

### 7.4 解码尺寸升级

解码目标至少覆盖当前基础图片的物理像素尺寸：

```text
DecodeWidth  >= BaseImageWidth  * Scale * RenderScaling
DecodeHeight >= BaseImageHeight * Scale * RenderScaling
```

目标尺寸按现有 16 px bucket 向上量化，并交给当前 `ImagePreviewEntry` 的 Full 通道。相邻预加载项继续使用基础视口目标，不跟随当前项的放大比例。尺寸和 DPI 来自实际承载 viewer 的 TopLevel；首次布局前才回退到封面所在 TopLevel。升级期间继续显示旧 lease；新结果提交后保持 `PreviewTransformState`，
只替换 renderer 的 `IImage`。解码通知携带 current entry 身份和本次基础布局使用的原始尺寸；owner 只接受与当前 entry 和原始尺寸一致的目标，旧图 reset 或保留帧不能为新项发布尺寸。`ReloadCurrent()` 与 `ReloadItem(CurrentIndex)` 使用当前显示目标。来源验证、DecodeKey、generation、取消和 cache lease 继续遵守统一图片加载系统契约。

解码器最多提供来源原始像素，超过原始分辨率的显示放大由 renderer 插值。来源或实际解码超过应用级单边、像素数或解码内存预算时，加载系统返回明确限制错误；算术上溢只饱和请求 bucket，不绕过预算。失败提交清空旧帧并释放 lease，错误占位与现有 Full 通道失败语义一致。

## 8. 资源、性能与 AOT 边界

- 变换状态使用纯值和现有 Avalonia 属性，不引入反射、动态发现或运行时注册。
- PointerMove、Wheel 和 Clamp 热路径不创建图片、loader、cache 或临时宿主。
- 解码升级通过已有 entry generation、共享 loader 和 lease 完成，不建立 Previewer 私有并发队列。
- `ImageViewer` re-template 时解除旧 scene 和按钮事件，清理拖拽状态并重新应用基础尺寸；detach 时解除实际 TopLevel 的 DPI 订阅。Dialog 和 OverlayHost 在关闭时解除 viewer 通知，open-state cleanup 释放跨根 relay bindings。
- 图片创建、替换和释放遵守现有 `ImageLoadResult` lease 生命周期；变换状态不拥有或销毁 `IImage`。
- 公式使用 `double` 和有限值检查；零尺寸、无限尺寸或无当前图片时退化为不发起缩放和解码请求。

## 9. 兼容性与定制边界

- `ImageMinScale`、`ImageMaxScale`、`ImageScaleStep` 和 `IsImageMovable` 的名称与默认值保持稳定。
- 新增 `IsWheelZoomEnabled` 默认 `true`；关闭只影响滚轮，不影响工具栏、双击或触摸缩放。
- `PART_ImageViewerScene`、`PART_ImageRenderer`、`popup.body`、`popup.footer` 和 `popup.actions` 的语义保持稳定。
- `Scale=1` 的视觉含义固定为基础适配尺寸；应用不能通过模板 Setter 把基础尺寸和用户变换拆成两套不一致的坐标系。
- Semantic Style 只能定制公开 Part 的视觉属性；缩放边界、变换状态和 lease 由控件 owner 管理。
- Desktop 和 Browser 必须保持相同的基础适配、缩放、拖拽、reset、切图和边界语义；窗口标题栏与 overlay 关闭入口属于平台宿主差异。

## 10. 验证要求

- 纯逻辑：基础适配同时覆盖宽高约束，极宽、极高和普通宽高比图片的 `BaseImageSize` 正确。
- 纯逻辑：按钮和滚轮按 `ImageScaleStep` 计算并限制在 `ImageMinScale`/`ImageMaxScale`，运行时边界修改立即更新状态。
- Headless viewer：打开、resize、DPI、缩放、旋转、翻转、reset、切图和关闭后的状态回收。
- Headless 输入：鼠标中心缩放、拖拽 rebound、图片小于视口时回中、旋转后轴向边界和 `IsImageMovable`。
- 宿主一致性：native dialog 与 overlay 对同一操作产生相同 `PreviewTransformState` 和可见图片边界。
- 加载集成：缩放升级使用正确物理像素 bucket，旧 lease 保持可见，旧 generation 不能覆盖新结果，预算超限转为明确错误。
- 主题契约：`PART_ImageViewerScene`、`PART_ImageRenderer` 和 `popup.*` marker 结构不因变换层调整而改变。
- Gallery 与发布：Gallery API/ShowCase 使用稳定属性；Browser managed publish 和 Desktop NativeAOT 保持静态注册与绑定可用。
