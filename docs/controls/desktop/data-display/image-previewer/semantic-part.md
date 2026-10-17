# ImagePreviewer Semantic Part 契约

本文档定义 ImagePreviewer 控件家族公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。控件整体设计见
[ImagePreviewer 桌面版架构设计](overview.md)，descriptor、marker 与宿主/模板节点的映射及维护不变量见
[ImagePreviewer 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

ImagePreviewer 家族公开 8 个 Semantic Part，由两个 public owner 分别声明生成式 descriptor：`ImagePreviewer`（单封面入口）与
`ImageGroupPreviewer`（多封面入口）。`root` 由生成器隐式加入，不要求 `.semantic-root`。除 `root` 外每个 Part 生成 public
强类型 Semantic Style，命名规则为 `AtomUI.Theme.Styling.<Control><PartPath>Style`（如 `ImagePreviewerPopupRootStyle`），
用户在外层普通 `Style` 中按 owner 作用域嵌套使用。

8 个 Part 中 `root`/`image`/`cover` 对应 `.ant-image`/`.ant-image-img`/`.ant-image-cover`；`popup.root`/`popup.mask`/
`popup.body`/`popup.footer`/`popup.actions` 对应 `.ant-image-preview` 及其内部 `mask`（半透明遮罩层）、`body`（居中图片区）、
`footer`（底部操作区）、`actions`（footer 内操作按钮组）四个子节点。上游的右上角关闭按钮、左右切换按钮与页码指示不是语义
部件，AtomUI 同样不将其公开为 Part。

`popup.*` 属于独立宿主部件：预览宿主（native `ImagePreviewerDialog` 或 Browser `ImagePreviewerOverlayHost`）由
`OpenDialog()` 运行时创建，并经 logical parent 挂入 owner，宿主 ThemeVariant 经 binding 中继。owner 作用域 Semantic Style
只在 overlay 宿主（与 owner 同 TopLevel 的树内浮层，对齐上游 `.ant-image-preview`）保证命中；native `ImagePreviewerDialog`
是独立 `Window`/TopLevel，Avalonia 样式级联不跨 TopLevel 边界，因此 owner 作用域 Semantic Style 不进入 dialog，dialog 内的
预览视觉经 host 契约（owner 属性/Token 中继与 App 级 `ImageViewer` 主题）定制。`popup.*` 部件随宿主打开而存在、随关闭而销毁，
因此统一声明 `CrossVisualRoot=true`、`CrossNestedOwners=true`、`RuntimeCreated=true`，路由以 `>>` 从 owner 直接定位 marker
节点，不设中间 scope 锚点（overlay 宿主模板根与 viewer 在逻辑树上为兄弟，锚点式路由无法在双宿主间一致命中）。`popup.mask`
仅 Overlay 宿主存在（`Optional`），承担上游浮层"压暗下层页面"的职能；native `ImagePreviewerDialog` 是独立窗口、无下层页面
可压暗。关闭在 overlay 宿主由内嵌关闭按钮（`PART_CloseButton`，非语义部件）承担，native dialog 由 OS 标题栏按钮承担。Gallery Semantic Preview 中 `popup.*` 部件
需示例显式提供宿主 Visual 根作为 `AdditionalRoots` 才能解析；但宿主（`ImagePreviewerDialog` 与 `ImagePreviewerOverlayHost`）
均为 internal、`OpenDialog()` 不返回宿主、产品不暴露任何 Preview 专用 API，Gallery 示例无法取得该根，因此 `popup.*` 部件在
Gallery 仅列出描述、不参与高亮；触发区部件（`root`/`image`/`cover`）在 owner 模板内正常解析。所有 marker 使用静态
`Classes.semantic-*="True"` 声明，内置主题不使用 `.semantic-*` selector 实现默认视觉。

### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `ImagePreviewer` / `ImageGroupPreviewer` |
| Part | `root` |
| Selector | 不适用（root 无 `.semantic-root`） |
| SelectorRoute | 不适用 |
| ContractType | `ImagePreviewer` / `ImageGroupPreviewer` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 控件根（隐式） |
| 职责 | 单封面入口（`ImagePreviewer`）/ 多封面入口（`ImageGroupPreviewer`）与完整预览 owner |
| 相关 API | `ItemsSource`、`CurrentIndex`、`IsOpen`；group 额外 `ItemsPanel` |
| 相关 Token | SharedToken、ImagePreviewerToken |
| 稳定性 | stable since 6.2.0 |

### `image`

| 字段 | 值 |
| --- | --- |
| Owner | `ImagePreviewer` / `ImageGroupPreviewer` |
| Part | `image` |
| Selector | `.semantic-image` |
| SelectorRoute | `ImagePreviewer`：`/template/ .semantic-scope-cover /template/ .semantic-image`；`ImageGroupPreviewer`：`/template/ .semantic-scope-items >> .semantic-image` |
| ContractType | `Control` |
| Cardinality | `ImagePreviewer`：`Single`；`ImageGroupPreviewer`：`Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| CrossNestedOwners | `true` |
| RuntimeCreated | `ImagePreviewer`：`false`；`ImageGroupPreviewer`：`true` |
| AtomUI 节点 | `ImagePreviewerCover` 模板内 `ImagePreviewRenderer`（单封面静态 / 多封面 ItemsControl 项运行时物化） |
| 职责 | 关闭态封面图片元素 / 各封面缩略图元素 |
| 相关 API | `EffectiveCoverImage`、`CoverWidth`、`CoverHeight`；group `ItemsSource`、`ItemsPanel` |
| 相关 Token | Cover 尺寸相关 Token |
| 稳定性 | stable since 6.2.0 |

### `cover`

| 字段 | 值 |
| --- | --- |
| Owner | `ImagePreviewer` / `ImageGroupPreviewer` |
| Part | `cover` |
| Selector | `.semantic-cover` |
| SelectorRoute | `ImagePreviewer`：`/template/ .semantic-scope-cover /template/ .semantic-cover`；`ImageGroupPreviewer`：`/template/ .semantic-scope-items >> .semantic-cover` |
| ContractType | `Border` |
| Cardinality | `ImagePreviewer`：`Single`；`ImageGroupPreviewer`：`Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| CrossNestedOwners | `true` |
| RuntimeCreated | `ImagePreviewer`：`false`；`ImageGroupPreviewer`：`true` |
| AtomUI 节点 | `ImagePreviewerCover` 模板内 `#Mask`（单封面静态 / 多封面 ItemsControl 项运行时物化） |
| 职责 | 封面悬浮提示层：遮罩 + 指示内容。遮罩经负 Margin 铺满整个 owner root（含 padding 环与边框），对齐上游 `genImageCoverStyle` 的 `position:absolute; inset:0` cover 几何 |
| 相关 API | `IsShowCoverMask`、`CoverIndicatorContent(Template)`、owner `Padding` / `BorderThickness`（经中继参与遮罩几何） |
| 相关 Token | `MaskBgColor`、mask 透明度与圆角 Token |
| 稳定性 | stable since 6.2.0 |

### `popup.root`

| 字段 | 值 |
| --- | --- |
| Owner | `ImagePreviewer` / `ImageGroupPreviewer` |
| Part | `popup.root` |
| Selector | `.semantic-popup-root` |
| SelectorRoute | `>> .semantic-popup-root` |
| ContractType | `Panel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| CrossNestedOwners | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | native dialog 内容根包裹 Panel（代码创建并注入 marker）；overlay 宿主模板根 Panel（静态 marker、纯容器，不带背景） |
| 职责 | 预览容器根：承载遮罩层、内容区与关闭按钮的根层（对齐上游 `.ant-image-preview`）；窗口 chrome 不属于契约 |
| 相关 API | `IsOpen`、`OpenDialog()`、`IsDialogModal`、`IsDialogTopmost` |
| 相关 Token | Dialog 背景 Token |
| 稳定性 | stable since 6.2.0 |

### `popup.mask`

| 字段 | 值 |
| --- | --- |
| Owner | `ImagePreviewer` / `ImageGroupPreviewer` |
| Part | `popup.mask` |
| Selector | `.semantic-popup-mask` |
| SelectorRoute | `>> .semantic-popup-mask` |
| ContractType | `Panel` |
| Cardinality | `Optional` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| CrossNestedOwners | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | Overlay 宿主模板根 Panel 内新增的全铺遮罩子元素（静态 marker；半透明黑背景，由原宿主根 Panel 背景迁移而来） |
| 职责 | 预览遮罩层：全铺 `popup.root` 的半透明暗色背景，位于 `popup.body` 之下（对齐上游 `.ant-image-preview-mask`）；仅 Overlay 宿主存在 |
| 相关 API | `IsOpen`（随 overlay 宿主打开出现；点击关闭行为当前未实现，见兼容性与验证） |
| 相关 Token | 遮罩色使用共享 `ColorBgMask`，与上游 `.ant-image-preview-mask` 的 `colorBgMask` 语义一致 |
| 稳定性 | stable since 6.2.0 |

### `popup.body`

| 字段 | 值 |
| --- | --- |
| Owner | `ImagePreviewer` / `ImageGroupPreviewer` |
| Part | `popup.body` |
| Selector | `.semantic-popup-body` |
| SelectorRoute | `>> .semantic-popup-body` |
| ContractType | `Panel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| CrossNestedOwners | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `ImageViewer` 模板内 `PART_ImageViewerScene` Canvas |
| 职责 | 预览内容区：居中承载图片渲染与指针交互（对齐上游 `.ant-image-preview-body`） |
| 相关 API | 基础适配、缩放、拖拽、旋转、翻转与 reset 交互 API |
| 相关 Token | 无独立 Token（沿用 viewer 背景与交互 Token） |
| 稳定性 | stable since 6.2.0 |

### `popup.footer`

| 字段 | 值 |
| --- | --- |
| Owner | `ImagePreviewer` / `ImageGroupPreviewer` |
| Part | `popup.footer` |
| Selector | `.semantic-popup-footer` |
| SelectorRoute | `>> .semantic-popup-footer` |
| ContractType | `Control` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| CrossNestedOwners | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `ImageViewer` 模板内 `ImagePreviewFloatToolbar` 节点 |
| 职责 | 预览页脚：底部居中操作区域，含页码指示与操作组（对齐上游 `.ant-image-preview-footer`） |
| 相关 API | `CurrentIndex`、Count 与 scale/fit 状态投影 |
| 相关 Token | `FloatToolbarPadding`、`NavButtonBgColor` |
| 稳定性 | stable since 6.2.0 |

### `popup.actions`

| 字段 | 值 |
| --- | --- |
| Owner | `ImagePreviewer` / `ImageGroupPreviewer` |
| Part | `popup.actions` |
| Selector | `.semantic-popup-actions` |
| SelectorRoute | `>> .semantic-popup-footer /template/ .semantic-popup-actions` |
| ContractType | `Border` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| CrossNestedOwners | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `ImagePreviewFloatToolbarTheme` 内 `#ActionFrame` |
| 职责 | 预览操作组：footer 内的胶囊形操作按钮组（对齐上游 `.ant-image-preview-actions`） |
| 相关 API | 缩放、翻转、旋转与 reset 命令 |
| 相关 Token | `PreviewOperationSize`、`PreviewOperationColor` |
| 稳定性 | stable since 6.2.0 |

## 2. 职责与存在条件

- `root` / `image` / `cover` 是触发区部件，位于 owner 模板及其封面协作控件的静态节点：`image` / `cover` 的 marker 声明在
  共享 `ImagePreviewerCoverTheme`（单/组共同物化同一封面模板），经 owner 模板的 `.semantic-scope-cover`（单）或
  `.semantic-scope-items`（组）锚点路由，不声明 `CrossVisualRoot`。
- `image` / `cover` 的 owner 差异：`ImagePreviewer` 为静态单封面，`Single`、`RuntimeCreated=false`；`ImageGroupPreviewer`
  的封面由 `PART_CoverItemsControl` 的 DataTemplate 运行时物化，`Multiple`、`RuntimeCreated=true`，路由经
  `.semantic-scope-items` 后以 `>>` 覆盖运行时生成的封面项，容器回收复用时 marker 身份不变。
- `popup.*` 是独立宿主部件：宿主由 `OpenDialog()` 运行时创建并经 `SetParent` 挂入 owner，两个 owner 打开同一宿主，
  物理节点（宿主根、viewer、float toolbar）由两者共享。
  - `popup.root` 两个宿主均存在：native dialog 内容根包裹 Panel（代码用生成的 `ImagePreviewerSemanticParts.PopupRootClass`
    常量注入 marker）；overlay 宿主模板根 Panel（静态 marker、纯容器）。
  - `popup.mask` 仅 overlay 宿主存在：根 Panel 内新增的全铺遮罩子元素（静态 marker、半透明黑背景）；native dialog 无下层
    页面可压暗，不物化该部件（`Optional`）。
  - `popup.body` 两个宿主均存在：viewer 模板内 `PART_ImageViewerScene`，承载居中图片与指针交互。
  - `popup.footer` 两个宿主均存在：viewer 模板内 `ImagePreviewFloatToolbar`，含页码指示与操作组。
  - `popup.actions` 两个宿主均存在：footer 内 `#ActionFrame`，胶囊形操作按钮组。
  - `popup.close` 仅 overlay 宿主存在：`PART_CloseButton`；native dialog 由 OS 标题栏关闭按钮承担（`Optional`）。

## 3. 数量语义

`root`、`popup.root`、`popup.body`、`popup.footer`、`popup.actions` 为 `Single`：静态模板节点或每次打开唯一的宿主容器。
`popup.mask` 与 `popup.close` 为 `Optional`：仅 overlay 宿主物化（每次打开至多一个），native dialog 中为 0。
`image` / `cover` 在 `ImagePreviewer` 为 `Single`（静态单封面），在 `ImageGroupPreviewer` 为 `Multiple`（随 `ItemsSource`
物化的封面项，回收复用时 marker 保持不变）。打开、关闭与重开不维护任何跨打开的 marker 状态；状态变化（`IsOpen`、
`CurrentIndex`、loading/error）只切换可见性或有效视觉值，不增删 marker。

## 4. Selector 用法

生成的 Semantic Style 类型命名为 `ImagePreviewer<PartPathPascalCase>Style` / `ImageGroupPreviewer<PartPathPascalCase>Style`
（如 `ImagePreviewerPopupMaskStyle`、`ImageGroupPreviewerImageStyle`，命名空间 `AtomUI.Theme.Styling`，AXAML 命名空间
`https://atomui.net`）。`root` 不生成 Style 类型，owner 级 Setter 写在外层普通 Style 上。

推荐写法（Gallery ImagePreviewer「自定义语义结构的样式」示例一致，对齐上游 `style-class.tsx` 的 object/function
styles 示例）。上游该示例自定义 `root` 与 `image`：`root`（padding 4 / borderRadius 8 / overflow hidden，右侧再加
常驻 2px `#A594F9` 边框，`transition: all 0.3s ease`）与 `image`（`borderRadius: 4`，右侧 `filter: grayscale(50%)`）。
经对上游示例三张截图逐像素核对：边框在常态与 hover 态均常驻显示，hover 变化的是 `cover` 遮罩（`rgba(0,0,0,0.3)`
淡入 + 眼睛/预览指示），边框本身不随 hover 出现或消失。hover 时"看起来出现一圈粗边框"是上游 cover 的固有几何：
`genImageCoverStyle` 的 cover 以 `position:absolute; inset:0` 铺满整个 `.ant-image` root（含 4px padding 环），30% 黑色
遮罩把白色 padding 环压成约 178 灰（255×0.7），视觉上等价于 hover 出现一圈粗灰框——AtomUI 的 `cover` 部件复现该几何，
遮罩经负 Margin 铺满 owner root（含 padding 环），见下文。AtomUI 中 `root` 即 owner，直接以 owner 属性表达；`image`
part 的 `borderRadius` 经生成 `ImagePreviewerImageStyle` 落到 `ImagePreviewRenderer.CornerRadius`（AddOwner
`Border.CornerRadiusProperty`，内部把 WinUI 关键点圆角几何设到子 `Image` 的 `Clip` 上），`x:SetterTargetType` 指向
internal 渲染器类型；Setter 属性名必须写限定名 `Property="Border.CornerRadius"`——经 public 声明类型 `Border` 的字段
解析，直接写 `CornerRadius` 会解析到 internal 渲染器类型自身的字段，XAML 编译器生成的字段访问不做编译期可见性检查，
运行时抛 `FieldAccessException`。
`filter` 无等价属性，右侧 `grayscale` 仍由 ViewModel 用 SkiaSharp 颜色矩阵去饱和图像源复现；`cover` 与 `popup.mask`
上游未覆盖，保持默认视觉（默认 `cover` 即全铺遮罩 + 0.3 透明度淡入）：

```xml
<StackPanel Orientation="Horizontal" Spacing="16" HorizontalAlignment="Left">
    <StackPanel.Styles>
        <Style Selector="atom|ImagePreviewer.semantic-styles-demo">
            <atom:ImagePreviewerImageStyle x:SetterTargetType="atom:ImagePreviewRenderer">
                <Setter Property="Border.CornerRadius" Value="4" />
            </atom:ImagePreviewerImageStyle>
        </Style>
    </StackPanel.Styles>
    <atom:ImagePreviewer Classes="semantic-styles-demo"
                         Width="160"
                         Padding="4"
                         CornerRadius="8"
                         ClipToBounds="True"
                         ItemsSource="{Binding DefaultImages}" />
    <atom:ImagePreviewer Classes="semantic-styles-demo"
                         Width="160"
                         Padding="4"
                         CornerRadius="8"
                         BorderThickness="2"
                         BorderBrush="#A594F9"
                         ClipToBounds="True"
                         ItemsSource="{Binding GrayscaleImages}" />
</StackPanel>
```

如仍需定制 `cover`/`popup.mask` 等 Part，可继续使用生成 Style（`ImagePreviewerCoverStyle` / `ImagePreviewerPopupMaskStyle`
等，`x:SetterTargetType` 提供类型上下文）；上游示例本身不覆盖这些 Part，故对齐示例不引入额外覆盖。注意 `cover` 生成
Style 只改变视觉（背景、指示前景等），不应覆盖负 Margin 或 `CornerRadius`、也不应设置 `ClipToBounds`——遮罩铺满
root 的几何由 `ImagePreviewerCover` 内部计算（`OwnerPadding` + `OwnerBorderThickness` 的负值），圆角经
`OwnerCornerRadius` 中继跟随 owner `CornerRadius`（对齐上游 root `overflow:hidden + border-radius` 的裁剪视觉），
且控件默认 `ClipToBounds=false`（Avalonia `TemplatedControl` 类级默认为 `true`，已在 cover 静态构造覆盖）；
覆盖 Margin/圆角或重新打开裁剪都会破坏"遮罩 = 整个 root（含 padding 环）"的上游对齐契约。同理，`image` 生成
Style 可以定制 `CornerRadius`（这是上游 `styles.image.borderRadius` 的等价入口），但不应设置 `ClipToBounds`——封面
图片的圆角由 `ImagePreviewRenderer` 以子 `Image` 的 `Clip` 几何施加（与 `DashedBorder.ClipContentToCornerRadius`
同一 WinUI 关键点算法），与 owner 根的裁剪职责互不重叠。

`popup.*` 的生成 Style 由 `Nesting()` 经 owner logical parent 链命中 overlay 宿主节点（与 owner 同 TopLevel），无需在用户
Style 中复写 `>>` route；native `ImagePreviewerDialog` 为独立 TopLevel，owner 作用域样式不级联进去，其预览视觉走 host 契约。
不得使用以下写法：

- `.semantic-root`、`PART_*`、Name selector、internal 类型或视觉祖先顺序作为应用主题契约。
- 把 `ContractType`（如 `Panel.semantic-popup-mask`）写入 Part 身份 selector；应使用生成 Style 的
  `x:SetterTargetType` 提供类型上下文。
- 直接复制 `/template/ .semantic-scope-*` route；scope class 只用于生成 Style 的 owner-relative 路由。
- 依赖 overlay 宿主根 Panel 的视觉子节点顺序（mask 与 ContentPresenter 的先后是实现细节，不进入契约）。

## 5. 定制边界

以下区域不属于 ImagePreviewer Semantic Part：

- **左右切换按钮**：`PART_PreviousButton` / `PART_NextButton` 对应上游 preview switch 按钮，上游未将其作为语义部件，
  AtomUI 同样不发布（多图导航由行为 API 与 viewer 承担）。
- **页码指示**：`IndicatorFrame` 对应上游 preview progress 节点，不发布为 Part（属于 `popup.footer` 内容）。
- **加载与错误呈现**：`PART_LoadingPresenter`、`PART_ErrorPresenter`、`MediaBreakPointIndicator` 是 Template Part，
  不进入 Semantic Part 契约。
- **操作按钮个体**：`popup.actions` 只承诺按钮组容器本体，组内每个按钮（缩放、翻转、旋转、reset）不单独发布。
- **native dialog 窗口 chrome**：标题栏、caption 按钮、窗口边框由 Window 契约承担，不属于任何 Part。
- `PART_*` 名称、internal 类型、`.semantic-scope-*` 路由标记与模板层级。

默认主题不消费 `.semantic-*` selector；静态 marker 只提供应用样式命中点，不改变默认属性优先级或增加状态订阅。Semantic
Style 服从 Avalonia 原生属性优先级。

## 6. 兼容性与验证

删除或重命名 Part、修改 selector class / route、收窄 `ContractType`（含把公共基类承诺收窄为具体实现类型）、改变
cardinality，或让任一内置模板变体缺少 marker，均属于公共主题契约变更。

与上游 Image 控件的对照差异（有意保持）：

- 上游 `popup.mask` / `popup.close` 在单图与 PreviewGroup 预览中恒存在；AtomUI 因 native dialog 是独立窗口（无下层页面
  可压暗、关闭由 OS 标题栏承担），将两者声明为仅 overlay 宿主的 `Optional`。
- 上游遮罩默认点击关闭（`maskClosable=true`）；AtomUI overlay 未实现该行为，关闭仅经 `popup.close`。Part 契约只承诺样式
  命中，不承诺该行为。
- 上游预览浮层与触发图片在同一 DOM，owner 作用域样式天然命中；AtomUI 的 owner 作用域 Semantic Style 只保证命中 overlay
  宿主（与 owner 同 TopLevel 的树内浮层），native `ImagePreviewerDialog` 是独立 TopLevel，样式级联不跨窗口边界，预览视觉经
  host 契约定制。
- AtomUI 操作组的基础适配与 reset 由统一预览变换状态提供；按钮组仍只作为 `popup.actions` 的内部内容，不进入 Semantic Part 契约。

验证至少覆盖（实现测试矩阵见 [实现原理](implementation.md)）：

- owner descriptor 只包含本节 9 个 Part，字段值与本节一致；内置模板不消费 `.semantic-*` selector。
- 六个主题的静态 marker 清单，及 native dialog 运行时注入 `popup.root` marker、overlay 新增 `popup.mask` 遮罩子元素。
- 生成 Semantic Style 在 overlay 宿主命中全部 `popup.*`；`popup.mask` / `popup.close` 在 overlay 命中、在 native dialog
  不存在的 Optional 语义。native dialog 内 marker 完整存在，但 owner 作用域样式不跨 Window/TopLevel 级联（回归测试固定该
  边界，见 [实现原理](implementation.md)）。
- open-close-reopen、owner detach 与 close 后不残留 marker、多个 owner 实例的宿主隔离、source 替换后 marker 与命中关系
  保持不变。
- Gallery 语义预览列出全部 9 个 Part；`root`/`image`/`cover` 在 owner 模板内高亮，`popup.*` 因宿主 internal、无公开访问器而
  仅列出描述不参与高亮（见 §1）。NativeAOT publish；descriptor 与生成 Style 使用编译期生成数据，不依赖运行时反射或
  VisualTree 扫描。
