# Avatar 桌面版实现原理

本文档描述 Avatar 控件家族的源码 ownership、图片加载状态流、模板生命周期和资源释放规则。公共契约见
[Avatar 控件家族架构设计](overview.md)，Token 语义见 [Avatar Token 设计](token.md)。

Popup 接入边界：`AvatarGroup` 负责业务状态和内容准备，内部 `FlyoutHost` 仅作为 relay 适配层，fold-count Flyout Popup 负责实际显示。模板重建或宿主切换时必须先释放旧 relay，再绑定新的 Popup；普通外点、Escape、失焦和业务关闭在 pinned 状态下被拦截，detach、窗口销毁、跨 TopLevel 和无效锚点必须走生命周期关闭并释放 Popup host。完整状态机见 [Popup 钉住打开设计](../../other/popup/popup-pinned-open-design.md)。

## 1. 实现定位

本文覆盖单头像、桌面 AvatarGroup、共享单图 controller、主题接入和图片租约生命周期。具体 StyledProperty 注册、Token 派生值和
AXAML selector 仍以源码为准；本文只记录跨文件协作和维护时不可破坏的实现边界。

## 2. 源码文件结构

`AtomUI.Controls` 拥有单头像实现：

```text
src/AtomUI.Controls/Avatar/
├── AbstractAvatar.cs
├── Avatar.cs
├── AvatarPseudoClass.cs
├── AvatarShape.cs
├── AvatarThemeConverters.cs
├── AvatarToken.cs
└── Themes/AvatarTheme.axaml
```

`AtomUI.Desktop.Controls` 只拥有桌面组合控件：

```text
src/AtomUI.Desktop.Controls/Avatar/
├── AvatarGroup.cs
├── AvatarGroupFoldInfo.cs
└── Themes/AvatarGroupTheme.axaml
```

`Avatar` 和 `AvatarToken` 不在 Desktop 包中保留副本。Desktop 通过对 Controls 的正常项目引用复用类型和 Token resource。

## 3. 核心类职责

- `AbstractAvatar`：Avalonia 属性、图片 controller host、内容优先级、尺寸/形状归一、加载状态和模板生命周期。
- `Avatar`：可实例化的公共控件类型，不建立第二套状态。
- `ImageLoadController`：与 `AsyncImage` 共用的单图片请求 owner，负责 generation、取消、fallback、Reload、progress 和租约提交。
- `AvatarToken`：从全局 Token 派生 Avatar 与 AvatarGroup 共用的视觉值。
- `AvatarGroup`：子 Avatar 排列、重叠宽度、折叠计数和 Flyout 生命周期。

## 4. 状态与数据流

```text
Source / FallbackSource / RequestOptions / Bounds / render scaling
  -> ImageLoadController generation
  -> Application.GetImageLoader().LoadAsync(...)
  -> ImageContentValidator / SvgContentValidator (Application SVG conformance mode)
  -> ImageLoadResult lease
  -> UI dispatcher generation check
  -> LoadedImage + LoadState/Error/Progress
  -> ContentType(Image/Text/Icon) + pseudo-classes
  -> AvatarTheme
```

`AbstractAvatar` 实现 `IImageLoadControllerHost`。它只向 controller 暴露当前 Visual、Normal priority、attach 状态、16 px
量化后的物理解码尺寸和状态提交回调，不直接访问 transport 或 cache。

`SvgContentValidator` 位于 Shared loader 管线中，在 codec 和 Avatar 状态提交之前执行。Application 构建时冻结默认
`Compatible`/可选 `Strict` 模式；Avatar 不把模式写入 `ImageRequestOptions`，也不根据来源类型选择另一条 SVG 校验路径。

Source、FallbackSource 或 RequestOptions 变化调用 `SourceConfigurationChanged()`。Arrange 完成后调用 `RefreshSize()`；只有有效
Bounds 产生新的尺寸 bucket 时才需要新请求。属性快照和状态提交均在 UI 线程边界完成。

## 5. 组合结构模型

### 控件角色图

```text
Avatar
  -> Panel#RootLayout
     -> PixelAlignedBorder#Frame (internal-observable)
     -> IconPresenter#IconPresenter (internal-observable)
     -> Border
        -> Image#ImagePresenter (internal-observable)
     -> Border
        -> Viewbox (internal-observable)
           -> TextBlock#PART_TextPresenter (template-stable)

AvatarGroup
  -> child Avatar collection (public)
  -> fold Avatar + Flyout (internal-observable)
```

### 协作节点

| 节点 | 类型 | 来源 | 生命周期 owner | 影响的 public API | 稳定性 | Agent 使用边界 |
| --- | --- | --- | --- | --- | --- | --- |
| `Avatar` | public control | `AvatarTheme.axaml` | visual tree | 单头像全部 API | public | 用户可直接使用 |
| `ImageLoadController` | internal coordinator | C# | `AbstractAvatar` | Source、fallback、状态、Reload | internal-observable | 只用于理解统一状态机 |
| Text `Viewbox` | layout presenter | `AvatarTheme.axaml` | visual tree | Text、Gap、Size、Font | internal-observable | 负责自然尺寸测量、向下缩放和居中，不作为独立 API 暴露 |
| `PART_TextPresenter` | `TextBlock` | `AvatarTheme.axaml` | template | Text、Font | template-stable | 提供自然排版尺寸；改名需同步实现、主题和文档 |
| `AvatarGroup` | public control | `AvatarGroupTheme.axaml` | visual tree | Children、折叠和尺寸投影 | public | 用户可直接使用 |
| fold Avatar/Flyout | internal visual | C# + Theme | `AvatarGroup` | MaxDisplayCount、trigger、fold colors | internal-observable | 不作为独立 API 暴露 |

内容选择只有三个结果：

1. `LoadedImage != null` 时选择 Image。
2. 否则 `Text != null` 时选择 Text。
3. 否则选择 Icon。

因此 Loading、取消或主来源失败不会主动清空 Text/Icon。Controller 在主来源终态失败后至多请求一次 FallbackSource；主来源
和 fallback 身份相同则不重复请求。fallback 成功时设置 `:loaded` 与 `:fallback`，最终失败时设置 `:failed`。

事件与状态提交顺序必须一致：先保存当前结果和状态，再触发 `ImageOpened`；最终失败先提交错误状态，再触发 `ImageFailed`。
取消、detach 和 generation 过期不属于图片失败事件。

## 6. 生命周期与模板接入

成功的 `ImageLoadResult` 是图片使用租约。Controller 持有当前租约，来源变化、detach、最终失败或 controller dispose 时释放。
替换图片时先接管新租约，再释放旧租约，避免 decoded cache entry 在视觉切换前被销毁。

`Reload()` 不创建新的公开来源状态；它复制当前 options，并只把本次请求的
`CacheRead` 覆盖为 `ImageCacheReadPolicy.RefreshSource`：

- HTTP 允许使用已有 metadata 做条件重验证。
- File、Asset、StorageFile、Bytes 和 Stream 强制重新读取来源。
- Reload 期间可保留旧图片；新结果只有通过 generation 校验后才能替换。

`new BorrowedImageSource(image)` 是 borrowed source。其结果租约只约束消费期，不能 dispose 调用方的 `IImage`。

| 获取/建立 | 对应释放 |
| --- | --- |
| controller attach 与 loader waiter | visual detach 时 controller detach/cancel |
| 当前 `ImageLoadResult` lease | Source 清空、替换终态、detach 或 dispose |
| AvatarGroup motion binding | group detach 时 dispose |
| AvatarGroup fold Flyout/临时视觉 | rebuild、detach 或 owner 释放时清理 |

Reattach 时 controller 使用当前 Source 和当前尺寸重新请求。文字布局不订阅 template part 事件；模板重套用后由 Border、Viewbox
和 TextBlock 的标准 measure/arrange 流程重新建立可用宽度、自然尺寸与缩放关系。AvatarGroup 只管理组合视觉，不取消或复用子
Avatar 的图片请求。

## 7. 交互与事件处理

Avatar 没有独立的 pointer 或 keyboard 状态机；基础交互由 Avalonia 控件和 Theme 处理。图片回调先在 UI dispatcher 校验
generation 和 attach 状态，再提交结果。成功时先保存 lease、状态和内容，再触发 `ImageOpened`；最终失败时先提交 error 状态，
再触发 `ImageFailed`。取消、detach 和过期 generation 不触发失败事件。`Reload()` 是唯一命令式图片入口。

## 8. 内部算法与关键流程

`AvatarTheme.axaml` 通过 internal `ContentType` 选择 Image、Text 或 Icon presenter。唯一稳定 template part 是
`PART_TextPresenter: Avalonia.Controls.TextBlock`。

`AvatarTheme` 用编译绑定和无状态类型化 converter 把 `Gap` 投影为模板内部的水平 Text padding。Border 用该 padding 定义文字可用区域，Viewbox 以
`PART_TextPresenter` 的自然排版尺寸为输入并使用 `StretchDirection=DownOnly`：自然宽度不超过可用宽度时保持原尺寸，超过时
等比缩小到可用区域。缩放和居中由同一个模板布局坐标系负责，不单独调用文字测量工具，也不叠加横向补偿 transform；Text、
Font、Gap 或控件尺寸变化通过 Avalonia measure/arrange 自动重新计算。

`AbstractAvatar` 负责 `Size` 非 `NaN` 时保存原 SizeType 并切换到 Custom；恢复 `NaN` 后回到原 SizeType。模板的 Custom 编译绑定持续观察 Size/Icon/Source，连续修改尺寸时更新真实 box 与 Icon 尺寸。Circle 通过 Bounds.Width 编译绑定把最终宽度的一半写入
CornerRadius。图片请求尺寸来自 Bounds 与 TopLevel render scaling，而不是 Token 名称或逻辑像素直接值。

## 9. 资源、性能与 AOT 边界

- 不进行同步网络或文件 I/O。
- 不使用固定延迟等待布局；Arrange 和尺寸 bucket 是唯一尺寸就绪信号。
- 文字适配使用无状态的 Border/Viewbox 布局，不重复测量字形，不持有 template part 事件订阅。
- 不创建控件私有 `HttpClient`、cache 或 scheduler。
- Theme、codec 和 loader 通过静态注册保留，不使用反射扫描。
- Browser 使用统一 loader 的 Browser 能力矩阵；Avatar 不增加平台分支 API。
- SVG 一致性模式是封闭的应用级 enum；Avatar 不持有 validator delegate，不增加反射或 NativeAOT 动态发现边界。
- 快速 Source 切换只允许当前 generation 回写，旧结果必须及时释放。

## 10. 维护不变量

- `ImageLoadController` 是 Avatar 与 `AsyncImage` 的单图状态机 owner，不在 Avatar 内复制请求协调逻辑。
- 内容优先级始终为 Image、Text、Icon；Loading 和取消不清空可用降级内容。
- 每个已提交 cache image 必须由一个有效 lease 支撑；borrowed image 永不由控件销毁。
- Source、options、尺寸或 attach generation 变化后，旧结果只能释放，不能回写状态。
- `Compatible`/`Strict` 差异只能来自 Shared `SvgContentValidator`；Avatar 不按 Asset/File/HTTP 或单次请求改写该结论。
- Template reapply、detach、group rebuild 和 Application dispose 都有明确的取消、解绑和释放路径。
- 文字的自然尺寸、可用宽度、缩放和居中必须由同一个模板布局路径完成；不得恢复独立文字测量或补偿平移。
- 单头像和 Token 只存在于 `AtomUI.Controls`，Desktop 包只拥有 AvatarGroup 组合能力。

## 11. 测试与验证

维护 Avatar 时至少覆盖：

- Source、FallbackSource、Text、Icon 优先级与状态伪类。
- A -> B 快速切换、Reload、detach/reattach、尺寸 bucket 和 render scaling。
- 主失败/fallback 成功、最终失败、事件顺序和 progress generation。
- owned cache image 与 borrowed image 的释放责任。
- AvatarTheme 独立于 Desktop 类型，AvatarGroup 仍能消费同一 Token。
- 短文本不放大；临界宽度与长文本缩小后保持水平居中，宽度不超过 `Width - 2 * Gap`；运行时 Gap 变化重新布局。
- Gallery 示例只使用 `Source`/`ImageSource`。
- 默认 Application `Compatible` 模式加载含重复 id 的本地 SVG；`Strict` 下同一来源报告 `InvalidImageData`，并继续验证
  FallbackSource、`ImageFailed`、伪类和 generation 语义。两种模式对不安全或超预算 SVG 的结果完全相同。


## 12. CT15 消费与组合绑定

单头像实现既有 `IThemeInheritanceSource`，模板的 optional 字体绑定仅在 Avatar owner 明确指定 Global FontFamily/FontSize 时覆盖原物理继承，原生 Font 属性仍优先。Square 各档圆角消费本 Avatar owner Global；默认文字仍来自原继承，本机14，三 TextFontSize Own 实际供 Icon 尺寸使用。

AvatarGroup 的 BorderThickness 消费自身注册 owner 的 Global；GroupSpace、GroupOverlapping 与 fold 颜色继续复用 Avatar Own。二者的 control identity 不可混同。Children 与 fold Avatar 是运行期集合/Content 对象，不处于 ControlTemplate 路径，故保留类型化 relay：每个 membership 持有自己的 CompositeDisposable，成员退役、Group detach 时释放；fold host/Avatar 的绑定随 fold 退役释放。SizeType 必须先于 Size 投射，避免构造时显式 Size 刚进入 Custom 后被 preset 覆盖。该机制保持原组合投射优先级，不建立旧 Local 值恢复引擎。

本家族未发布生成的 Semantic Part manifest；Frame/Icon/Image 为 internal-observable，PART_TextPresenter 仍为唯一稳定模板 part。定制使用公开 root Style/原生属性，观测内部节点不得变成赋值回退。CT15 验证结果与未覆盖边界见 [执行记录](../../../../superpowers/plans/2026-10-22-custom-theme/avatar-consumers.md)。

fold 的 StackPanel 是跨根弹层 Content，context 值能正确继承并不保证 DynamicResource 收到发布事件。AvatarGroup 仅在该运行期内容根复用 ThemeContextResourceBridge，随 Group Context 变化更新，fold退役/detach时先从资源集合移除并dispose，避免保留旧Scope。单头像不为此增加常驻bridge，共享Flyout不由本包改写。


CT15当前仅[限定表](../../../../superpowers/plans/2026-10-22-custom-theme/avatar-consumers-visual-review.md)48对有意变化已获批准，其他关闭/往返微差及完整Avatar/CT15保留，不授权提交。
