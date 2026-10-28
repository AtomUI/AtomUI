# Modal / Dialog Semantic Part 契约

本文定义 `Dialog` 与 `MessageBox`（Modal 家族）公开的 Semantic Part：部件清单、marker 落点、双宿主可达性、
数量语义、定制边界与验证要求。控件整体设计见 [Modal 桌面版架构设计](overview.md)，内部状态所有权、宿主实现与
释放规则见 [Modal 桌面版实现原理](implementation.md)，宿主尺寸算法见
[Modal 宿主尺寸与 Resize 设计](host-sizing-design.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

本文的 Part 集合对齐上游 Ant Design `Modal` 的公开 Semantic DOM。上游来源为本地参考源码
`ReferenceProjects/ant-design`（`components/modal/interface.ts` 的 `ModalSemanticType`、
`components/modal/demo/_semantic.tsx` 的语义槽位清单、`components/modal/style/index.ts` 的
`genModalStyle`/`genModalMaskStyle`/`prepareComponentToken`，仓库版本 `6.6.1-104-g3bc029ad8f`，分支 `master`）。

## 1. Semantic Parts

Dialog 家族公开 8 个 Semantic Part，`root` 由生成器隐式加入（不要求 `.semantic-root`）。语义 owner 是 public 的
`Dialog` 控件本身；`MessageBox : Dialog` 复用同一 `DialogSurface` 视觉树，因此单独声明同一组 Part（生成器按 public
Control 生成 descriptor，不继承基类 descriptor，先例：`SimplePagination` 对 `Pagination`）。

`Dialog` **没有 `ControlTemplate`**（`DialogTheme.axaml` 只定义默认属性与 `IsModal` 状态），全部视觉节点位于运行时
创建并附加到 `DialogOverlayLayer` 或原生 `DialogWindow` 的 `DialogSurface` / `OverlayDialogPresenter` /
`OverlayDialogHeader` 模板中。因此全部 Part 声明 `RuntimeCreated=true`、`CrossVisualRoot=true`、
`CrossNestedOwners=true`，路由以 `>>` 从 owner 直接定位 marker 节点（先例：Drawer 的运行时容器部件）。

与上游 Modal 语义 DOM（`_semantic.tsx`，9 个槽位）的映射：

| 上游槽位 | 上游 since | AtomUI 部件 | 说明 |
| --- | --- | --- | --- |
| `root` | 6.0.0 | `root`（隐式，owner 契约） | 上游 `.ant-modal-root` 是最外层容器；AtomUI 遵循系统契约 root=owner 控件，上游 root 对应内部 `DialogOverlayLayer`/presenter 基础设施，不作为部件暴露。 |
| `mask` | 5.13.0 | `mask` | 一一对应；`IsModal=false` 或 Window 宿主时不物化（Optional）。 |
| `container` | 6.0.0 | `container` | 一一对应（`Border#Frame`：背景、圆角、`ClipToBounds`）。上游同一节点的 `boxShadow`/`padding` 在 AtomUI 分属其他节点，见 §5.1。 |
| `wrapper` | 5.13.0 | `wrapper` | 角色对应：上游是动画/滚动包裹层，AtomUI 映射到私有动效表面内的 ContentPresenter。几何边界见 §5.2。 |
| `header` | 5.13.0 | `header` | 一一对应（`Border#HeaderFrame`：背景、内边距）。Overlay 恒存在；Window 宿主不物化（Optional，见 §2）。 |
| `title` | 6.0.0 | `title` | 一一对应（`TextBlock#Title`）。Overlay 恒存在；随 header 在 Window 宿主不物化（Optional）。 |
| `body` | 5.13.0 | `body` | 一一对应（`Border#ContentFrame`：正文内边距与内容承载）。 |
| `footer` | 5.13.0 | `footer` | 一一对应（`Border#FooterFrame`：背景、内边距、外边距）。节点恒存在，`IsFooterVisible=false` 时仅隐藏（Single）。 |
| `close` | 6.4.0 | `close` | 一一对应（`PART_CloseButton`）。Overlay 恒存在，`IsClosable=false` 仅隐藏；随 header 在 Window 宿主不物化（Optional）。 |

Part 明细：

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner / Part | `Dialog` / `MessageBox` / `root` |
| Selector | 不适用（root 无 `.semantic-root`） |
| SelectorRoute | 不适用 |
| Style Type | 不生成（`root` descriptor 的 `StyleType` 为 `null`） |
| ContractType | owner 自身类型 |
| Cardinality | Single |
| RuntimeCreated / CrossVisualRoot | 不适用（owner 本身） |
| AtomUI 节点 | `Dialog` / `MessageBox` 实例（零尺寸状态持有控件，`DialogTheme.axaml` 未定义模板） |
| 职责 | public 打开意图、内容、结果、按钮与事件入口 |
| 相关 API | `IsOpen`、`OpenAsync`、`Content`、`Result`、`BeforeCloseAsync`、`DialogHostType` |
| 相关 Token | `DialogToken`（背景、文字、间距、尺寸） |
| Customization | Root |
| Stability | stable |

#### `mask`

| 字段 | 值 |
| --- | --- |
| Owner / Part | `Dialog` / `mask` |
| Selector | `.semantic-mask` |
| SelectorRoute | `>> .semantic-scope-mask /template/ .semantic-mask` |
| Style Type | `AtomUI.Theme.Styling.DialogMaskStyle` |
| ContractType | `Avalonia.Controls.Border` |
| Cardinality | Optional |
| RuntimeCreated / CrossVisualRoot / CrossNestedOwners | true / true / true |
| AtomUI 节点 | `OverlayDialogMaskTheme.axaml` 的 `Border#Frame`（静态 marker） |
| 职责 | 静态 modal 遮罩：`ColorBgMask` 背景、`CornerRadius` 与指针命中；随 presenter 显示和移除，不执行淡入淡出 |
| 相关 API | `IsModal`、`IsMaskClosable` |
| 相关 Token | `ColorBgMask` |
| Customization | Selector |
| Stability | stable |

#### `wrapper`

| 字段 | 值 |
| --- | --- |
| Owner / Part | `Dialog` / `wrapper` |
| Selector | `.semantic-wrapper` |
| SelectorRoute | `>> .semantic-scope-presenter > .semantic-wrapper` |
| Style Type | `AtomUI.Theme.Styling.DialogWrapperStyle` |
| ContractType | `Avalonia.Controls.Control` |
| Cardinality | Optional |
| RuntimeCreated / CrossVisualRoot / CrossNestedOwners | true / true / true |
| AtomUI 节点 | `OverlayDialogPresenterTheme.axaml` 的 `ContentPresenter.semantic-wrapper`（静态 marker） |
| 职责 | Surface 包裹层；应用样式与私有 Motion 变换隔离 |
| 相关 API | `IsMotionEnabled` |
| 相关 Token | `MotionDurationMid` |
| Customization | Selector |
| Stability | stable（几何语义见 §5.2） |

#### `container`

| 字段 | 值 |
| --- | --- |
| Owner / Part | `Dialog` / `container` |
| Selector | `.semantic-container` |
| SelectorRoute | `>> .semantic-scope-frame-host > .semantic-container` |
| Style Type | `AtomUI.Theme.Styling.DialogContainerStyle` |
| ContractType | `Avalonia.Controls.Border` |
| Cardinality | Single |
| RuntimeCreated / CrossVisualRoot / CrossNestedOwners | true / true / true |
| AtomUI 节点 | `DialogSurfaceTheme.axaml` 的 `Border#Frame`（静态 marker） |
| 职责 | 对话主体框体：`ContentBg` 背景、`BorderRadiusLG` 圆角、子内容裁剪 |
| 相关 API | `DialogHostType`（Overlay 圆角 / Window 方角） |
| 相关 Token | `ContentBg`、`BorderRadiusLG` |
| Customization | Selector |
| Stability | stable（阴影归属见 §5.1） |

#### `header`

| 字段 | 值 |
| --- | --- |
| Owner / Part | `Dialog` / `header` |
| Selector | `.semantic-header` |
| SelectorRoute | `>> .semantic-scope-content-layer > .semantic-scope-header /template/ .semantic-header` |
| Style Type | `AtomUI.Theme.Styling.DialogHeaderStyle` |
| ContractType | `Avalonia.Controls.Border` |
| Cardinality | Optional |
| RuntimeCreated / CrossVisualRoot / CrossNestedOwners | true / true / true |
| AtomUI 节点 | `OverlayDialogHeaderTheme.axaml` 的 `Border#HeaderFrame`（静态 marker） |
| 职责 | 标题区框体：`HeaderBg` 背景与 `HeaderPadding` 内边距 |
| 相关 API | `Title`、`TitleIcon`、`IsClosable`、`IsMaximizable`、`IsDragMovable` |
| 相关 Token | `HeaderBg`、`HeaderPadding`、`HeaderMarginBottom` |
| Customization | Selector |
| Stability | stable（与上游 header 的几何差异见 §5.3） |

#### `title`

| 字段 | 值 |
| --- | --- |
| Owner / Part | `Dialog` / `title` |
| Selector | `.semantic-title` |
| SelectorRoute | `>> .semantic-scope-content-layer > .semantic-scope-header /template/ .semantic-title` |
| Style Type | `AtomUI.Theme.Styling.DialogTitleStyle` |
| ContractType | `Avalonia.Controls.TextBlock` |
| Cardinality | Optional |
| RuntimeCreated / CrossVisualRoot / CrossNestedOwners | true / true / true |
| AtomUI 节点 | `OverlayDialogHeaderTheme.axaml` 的 `TextBlock#Title`（静态 marker） |
| 职责 | 标题文字排版与前景色 |
| 相关 API | `Title` |
| 相关 Token | `HeaderFontSize`、`FontWeightStrong`、`HeaderColor` |
| Customization | Selector |
| Stability | stable |

#### `body`

| 字段 | 值 |
| --- | --- |
| Owner / Part | `Dialog` / `body` |
| Selector | `.semantic-body` |
| SelectorRoute | `>> .semantic-scope-content-layer > .semantic-body` |
| Style Type | `AtomUI.Theme.Styling.DialogBodyStyle` |
| ContractType | `Avalonia.Controls.Border` |
| Cardinality | Single |
| RuntimeCreated / CrossVisualRoot / CrossNestedOwners | true / true / true |
| AtomUI 节点 | `DialogSurfaceTheme.axaml` 的 `Border#ContentFrame`（静态 marker） |
| 职责 | 正文区域：Overlay 使用 `ContentPadding`；Window 宿主使用 `WindowContentPadding`；同时负责内容裁剪与 loading 骨架宿主边界 |
| 相关 API | `Content`、`ContentTemplate`、`IsLoading` |
| 相关 Token | `ContentPadding`、`WindowContentPadding`、`LoadingIndicatorMargin` |
| Customization | Selector |
| Stability | stable |

#### `footer`

| 字段 | 值 |
| --- | --- |
| Owner / Part | `Dialog` / `footer` |
| Selector | `.semantic-footer` |
| SelectorRoute | `>> .semantic-scope-content-layer > .semantic-footer` |
| Style Type | `AtomUI.Theme.Styling.DialogFooterStyle` |
| ContractType | `Avalonia.Controls.Border` |
| Cardinality | Single |
| RuntimeCreated / CrossVisualRoot / CrossNestedOwners | true / true / true |
| AtomUI 节点 | `DialogSurfaceTheme.axaml` 的 `Border#FooterFrame`（静态 marker） |
| 职责 | 底部操作区框体：`FooterBg` 背景、`FooterPadding` 内边距、`FooterMarginTop` 外边距；不承诺内部按钮布局 |
| 相关 API | `IsFooterVisible`、`StandardButtons`、`CustomButtons` |
| 相关 Token | `FooterBg`、`FooterPadding`、`FooterMarginTop` |
| Customization | Selector |
| Stability | stable |

#### `close`

| 字段 | 值 |
| --- | --- |
| Owner / Part | `Dialog` / `close` |
| Selector | `.semantic-close` |
| SelectorRoute | `>> .semantic-scope-content-layer > .semantic-scope-header /template/ .semantic-close` |
| Style Type | `AtomUI.Theme.Styling.DialogCloseStyle` |
| ContractType | `Avalonia.Controls.Button` |
| Cardinality | Optional |
| RuntimeCreated / CrossVisualRoot / CrossNestedOwners | true / true / true |
| AtomUI 节点 | `OverlayDialogHeaderTheme.axaml` 的 `DialogCaptionButton#PART_CloseButton`（静态 marker） |
| 职责 | 标题栏关闭入口的按钮视觉（前景、尺寸、hover/pressed 反馈） |
| 相关 API | `IsClosable` |
| 相关 Token | `CloseBtnSize`、`IconSize`、`HeaderColor` |
| Customization | Selector |
| Stability | stable（与上游绝对定位 close 的差异见 §5.4） |

#### MessageBox

`MessageBox` 是 `Dialog` 的语义专化，复用同一 `DialogSurface`/`OverlayDialogHeader` 模板，因此公开与 `Dialog`
完全相同的 8 个 Part（节点、`SelectorClass`、`SelectorRoute`、`ContractType`、`Cardinality` 一致），但 descriptor 归属
`MessageBox` 自身类型，生成 `MessageBox<Part>Style` 类型（如 `MessageBoxContainerStyle`）。`MessageBox` 的
`MessageBoxContent`（图标 + 内容组合）属于 `body` 的内容，不是独立 Part。

### 1.1 路由精度与 `.semantic-scope-*` 锚点

Dialog body 内嵌 `Skeleton`（加载态常驻），用户内容还常包含 `Card`、`Tooltip`、`Spin` 等语义 owner，它们同样发布
`semantic-header`/`semantic-title`/`semantic-body`/`semantic-footer`/`semantic-container` 等同名 `SelectorClass`。
宽泛 `>>` descendant 会把这些嵌套部件一并命中（系统文档 §3.3 明确禁止公共 Selector 使用宽泛 descendant），
因此本契约的全部路由都用 Dialog 自有的 `.semantic-scope-*` 锚点收窄：

| 锚点 | 节点 | 作用 |
| --- | --- | --- |
| `semantic-scope-presenter` | `OverlayDialogPresenterTheme` 的模板根 `Panel` | 定位 presenter 模板，隔离嵌套 owner 的 `wrapper`。 |
| `semantic-scope-mask` | `OverlayDialogMask#PART_DialogMask` | 定位遮罩控件，隔离嵌套 owner 的 `mask`。 |
| `semantic-scope-frame-host` | `ShadowsAwareContainer#PART_ShadowHost` | 定位框体宿主，隔离嵌套 owner 的 `container`。 |
| `semantic-scope-content-layer` | `DockPanel#PART_SurfaceContentLayer` | 定位 Header/正文/Footer 的直接父，隔离正文内的嵌套 owner。 |
| `semantic-scope-header` | `OverlayDialogHeader#PART_Header` | 定位标题栏控件，隔离嵌套 owner 的 `header`/`title`/`close`。 |

`.semantic-scope-*` 只用于路由，不进入 Part 表；隔离由「锚点唯一 + `>`/`/template/` 精确步骤」共同保证，
不依赖任何“最近 Semantic owner”停止规则。回归测试
`Generated_Semantic_Styles_Hit_Exactly_One_Node_Per_Part` 断言每个 Part 恰好命中一个节点，锁定该精度。

## 2. 职责与存在条件

- 全部非 root Part 在 `DialogSurface` 物化后存在；部件随 Session 打开而物化、随 presenter teardown 而销毁，
  不维护跨打开状态。
- **Overlay 宿主**：`OverlayDialogPresenter` 由 `OpenAsync` 运行时创建，加入 `DialogOverlayLayer`。为让 owner 作用域
  的生成 Semantic Style 命中 presenter 子树，实现阶段必须按 Drawer 先例，在 `layer.Children.Add` **之前**对 presenter
  执行 `((ISetLogicalParent)presenter).SetParent(_dialog)`，并在 teardown 时置空；`Dialog` 必须实现
  `ISemanticPartCrossRootProvider`，`GetCrossRoots()` 返回存活的 presenter/Surface，供 Gallery 语义预览收集跨根。
  该逻辑父**只在 Dialog owner 已附加到逻辑树时设置**（与 `OverlayDialogPresenter` 既有的 inheritance parent 条件一致）：
  owner 未附加时 owner 作用域样式本就无法激活，此时保持 layer 所有权以避免改变 presenter 的 `Parent` 语义。
  因此 Overlay 可达性的前提是 owner 已进入可视/逻辑树（声明式 `IsOpen` 与静态 API 两条路径都满足）。
- **Window 宿主**：`DialogSurface` 是 `DialogWindow.Content`，`DialogWindow` 的逻辑父已指向 `Dialog`
  （`WindowDialogPresenter` 的 `SetParent(_dialog)`），因此 `container`/`body`/`footer` 的 marker 在该宿主完整存在
  （`mask`/`wrapper`/`header`/`title`/`close` 不物化，见上文 Optional 说明）。**owner 实例级 Semantic Style 经该逻辑
  父链在 Window 宿主同样命中**（控件级回归 `Window_Host_Owner_Scoped_Semantic_Styles_Cascade_Via_Logical_Parent`
  实证：owner `Styles` 中的生成 Part Style 命中独立 TopLevel 内的 `container`；Gallery 的样式化窗口 Dialog 演示即该
  模式）。跨根高亮另经 `GetCrossRoots()` 上报的 `HostWindow` 解析（Adorner 落在宿主窗口自己的 AdornerLayer）；
  静态资源仍由 `DialogResourceBridge` 中继（先例：ImagePreviewer 的 native dialog）。
- Window 宿主不物化的部件共五个：`mask`（原生窗口没有遮罩）、`wrapper`（没有 Surface motion actor），以及
  `header`/`title`/`close`——`WindowDialogPresenter` 无条件 `IsHeaderVisible=false`（原生窗口 caption 独占标题栏），
  隐藏的标题栏模板不应用，三个 marker 节点不存在。按系统设计 §8.2（模板变体无法提供 Part 必须声明 Optional），
  五者均为 `Optional`。
- Overlay 宿主中上述部件全部物化：`IsFooterVisible=false`、`IsClosable=false` 只隐藏节点（`IsVisible`），不增删 marker。
  Cardinality 描述节点存在性，不描述可见性。
- `title` 随 `header` 存在；`footer` 在两种宿主都恒存在（`Single`）。
- `Dialog.IsPinnedOpen=true` 时，用户发起的 presenter 关闭请求（标题栏关闭按钮、Overlay 遮罩外点、Escape、
  Footer 按钮）不再进入关闭管道；外部代码直接设置 `IsOpen=false` 仍正常关闭。该开关只是预览用行为门控，
  **不是 Part**，不改变任何 marker、Cardinality 或默认视觉；Window 宿主由原生窗口关闭流程负责，不受其控制。
- 上游 `root` 的 fixed 定位职责由内部 `DialogOverlayLayer` + presenter 承担，不进入公共 Part 表。

## 3. 数量语义

| Part | Cardinality | 说明 |
| --- | --- | --- |
| `root` | Single | owner 控件。 |
| `mask` | Optional | Overlay + `IsModal=true` 时 1 个；Window 或 modeless 为 0。 |
| `wrapper` | Optional | Overlay 时 1 个；Window 为 0。 |
| `container` | Single | 每次展示唯一 Surface 框体。 |
| `header` | Optional | Overlay 时 1 个；Window 宿主标题栏隐藏、模板不应用，为 0。 |
| `title` | Optional | 随 header：Overlay 时 1 个；Window 宿主为 0。 |
| `body` | Single | 每次展示唯一正文区。 |
| `footer` | Single | 节点恒存在；`IsFooterVisible=false` 只隐藏。 |
| `close` | Optional | 随 header：Overlay 恒存在（`IsClosable=false` 只隐藏）；Window 宿主为 0。 |

同一 `Dialog` 实例的多个会话（关闭后重开）不保留 marker 状态；嵌套 Dialog 各自持有独立 presenter 与 Surface，
部件互不串扰。`IsLoading` 只在 `body` 内切换 `Skeleton` 与 `Content`，不增删 Part marker。

## 4. Selector 用法

生成类型命名为 `Dialog<PartPathPascalCase>Style` / `MessageBox<PartPathPascalCase>Style`，位于
`AtomUI.Theme.Styling`（AXAML 前缀 `atom:`），必须作为 owner（或其业务 class）外层普通 `Style` 的嵌套样式使用；
`x:SetterTargetType` 写 Part 的 `ContractType`。`root` 不生成 Style，owner 级 Setter 直接写在外层 Style 上。

```xml
<atom:Dialog.Styles>
    <Style Selector="atom|Dialog.semantic-styles-demo">
        <atom:DialogMaskStyle x:SetterTargetType="Border">
            <Setter Property="Background" Value="#661677ff" />
        </atom:DialogMaskStyle>
        <atom:DialogContainerStyle x:SetterTargetType="Border">
            <Setter Property="CornerRadius" Value="8" />
        </atom:DialogContainerStyle>
        <atom:DialogTitleStyle x:SetterTargetType="atom:TextBlock">
            <Setter Property="Foreground" Value="#1677ff" />
        </atom:DialogTitleStyle>
        <atom:DialogBodyStyle x:SetterTargetType="Border">
            <Setter Property="Padding" Value="32" />
        </atom:DialogBodyStyle>
    </Style>
</atom:Dialog.Styles>
```

规则要点：

- 生成 Style 只能在带 owner 作用域的外层 Style 内使用；禁止把生成 Style 提升为裸全局规则或使用
  `.semantic-*` descendant 直接书写。
- `x:SetterTargetType` 必须是部件 `ContractType`；AtomUI 类型带 `atom:` 前缀，Avalonia 类型不带。
- 禁止把 `ContractType.semantic-*`、`:is(ContractType).semantic-*`、`PART_*`、Name selector 或 internal 类型写进
  Part 身份 selector。
- Window 宿主内 owner 实例级 Semantic Style 同样命中（§2），但机制不是"自动"的——Avalonia `TopLevel` 把
  样式宿主父级固定为 Application（`IStyleHost.StylingParent => _globalStyles`），且窗口模板应用时
  `ContentPresenter` 会改写 surface 的继承父，模板之后才挂载的内容/按钮永远走不到 owner 样式链。
  `DialogWindow` 因此按 `PopupRoot` 的既有范式覆写 `IStyleHost.StylingParent` 交还给逻辑父（owner Dialog）；
  owner 未生根（脱离页面树直接构造 presenter）时退回 Application 保证 ControlTheme 可达
  （回归：`Window_Host_Owner_Instance_Styles_Restyle_Container_Content_And_Footer_Buttons`）。
- `container` 的 `Background`/`CornerRadius` 在 `DialogSurfaceTheme` 中必须以 ControlTheme 嵌套样式声明，
  禁止写成模板局部值——局部值优先级压过任何 Style setter，`DialogContainerStyle` 将永远无法覆盖
  （2026-09-13 真机缺陷：container 始终白色即此因）；`CornerRadius` 经 `TemplatedParent` 绑定保持对
  Surface 运行时变更（Overlay 最大化归零）的跟随。
- **部件内部元素的定制（按钮/正文文字）**：在外层 owner Style 内声明一级嵌套样式即可命中部件内部元素——
  `^ atom|Button` 命中 Footer 按钮、`^ atom|TextBlock` 命中正文文字。三条机制约束（控件级回归
  `Nested_Styles_Inside_Part_Styles_Reach_Footer_Buttons_And_Body_Text` 与上文 Window 宿主回归锁定）：
  1. `DialogButton` 的 `StyleKeyOverride` 是 AtomUI `Button`，Avalonia 类型选择器按 StyleKey 精确匹配——
     必须写 `atom|Button`，写 `atom|DialogButton` 永远不命中；标题栏 caption 按钮的 StyleKey 不同，不会被误伤。
  2. 嵌套样式必须直接挂外层 owner Style；挂在生成 Part Style（如 `DialogFooterStyle`）内部的二级嵌套不激活。
  3. 要演示 footer 按钮样式，Dialog 必须显式声明 `StandardButtons`（默认 `NoButton`，footer 无按钮，
     按钮级样式无目标可命中）。

Gallery 语义预览：对齐上游 `getContainer={false}` 的内联模态——舞台内嵌一个 `ScopeAwareOverlayLayerPanel`，Dialog 同时设置
`OverlayScope`（把 overlay 宿主限定到该作用域，mask 与居中随舞台而非铺满窗口）与 `PlacementTarget`（解析目标），并以
`IsOpen=True` + `IsPinnedOpen=True` 常开钉住；`StandardButtons` 提供真实按钮序列以呈现 `footer`。右侧部件卡因此仍可 hover，
`mask` 也能在舞台内高亮。第三个预览演示**原生 Window 宿主**（先例：ImagePreviewer 的 native dialog）：对话以独立
系统窗口打开（modeless——模态会阻断 Gallery 输入），**由按钮按需触发、不默认打开**；跨根经 `Dialog.GetCrossRoots()`
上报 `HostWindow`，部件高亮落在该窗口自己的 AdornerLayer；`mask`/`wrapper`/`header`/`title`/`close` 在该宿主不物化，
对应卡片无高亮目标。第二个按钮打开**样式化窗口 Dialog**（owner 实例级 Semantic Style 定制 `container`/`body`/`footer`
——该宿主只物化这三个部件）；该 Dialog 必须位于 `PreviewContent` 之外，因为语义预览按 owner 类型多实例解析，
同一 Preview 内容内的第二个 Dialog 实例会被一并高亮。

`OverlayScope` 是可选公共属性（默认 `null`）：指定后 overlay 宿主注入该元素所在作用域，mask、Surface 尺寸与居中都以作用域
为边界；作用域内没有可用 scope layer 时回退到默认 TopLevel 宿主；Window 宿主忽略该属性。作用域宿主没有 Window frame
契约，因此不参与 frame 内缩与 drawn chrome 抑制。默认值不改变任何既有布局或渲染结果。

## 5. 尺寸与几何基线

按系统文档 §7.2 的准入要求，本节记录布局型 Semantic Part 的尺寸/几何归属基线。

| 维度 | 事实 |
| --- | --- |
| 尺寸档 | Dialog 家族不实现 `ISizeTypeAware`，没有 `Large/Middle/Small/Custom` 分支；正文尺寸由 `HostWidth/Height/Min/Max` 描述，结构性下限由 Header/Footer/`DialogToken` viewport 基线解析。 |
| 尺寸属性 owner | owner 根与正文尺寸：`Dialog.HostWidth/Height/Min/Max` + presenter 解析；`container`：`Border#Frame` 的自然测量（受 Surface constraints）；`header`：`OverlayDialogHeader` 的 `Padding`/`Margin`（`HeaderPadding`/`HeaderMarginBottom`）；`body`：`Border#ContentFrame.Padding`（Overlay 为 `ContentPadding`，Window 宿主为 `WindowContentPadding`）；`footer`：`Border#FooterFrame` 的 `Padding`/`Margin`（`FooterPadding`/`FooterMarginTop`）。 |
| Token 映射 | 见各 Part 表“相关 Token”列；无尺寸档分支 Token。 |
| 状态变体 | `IsLoading` 切换 body 内 `Skeleton`/`Content`（不增删 marker）；`DialogHostType` 切换 Overlay 圆角与 Window 方角（`^.window-hosted` 覆盖 `CornerRadius=0`）；`IsResizable` 只影响 `PART_Resizer`（非 Part）。 |
| 模板路径 | Overlay：`OverlayDialogPresenterTheme` + `DialogSurfaceTheme` + `OverlayDialogHeaderTheme` + `OverlayDialogMaskTheme`；Window：同一 `DialogSurfaceTheme`/`OverlayDialogHeaderTheme`，无 mask/wrapper。 |
| 自然测量 | `DialogSurface` 按 Header/Footer/有效按钮计算 structural minimum，presenter 再与 `HostMin/Max` 与宿主容量合成；`container`/`body`/`footer` 的 `Padding` Setter 会改变正文自然测量，但最终尺寸仍受 owner 有效约束裁剪（系统文档 §7.1 的父子不同属性边界）。 |

### 5.1 `container` 的 `boxShadow` 归属

上游 `.ant-modal-container` 在同一节点同时拥有 `backgroundColor`/`borderRadius`/`boxShadow`/`padding`。AtomUI 当前把
`BoxShadow` 放在 `ShadowsAwareContainer#PART_ShadowHost`（`internal class ShadowsAwareContainer : Decorator`，
`ShadowsAwareContainer.cs`），`Background`/`CornerRadius` 在 `Border#Frame`（`container` 节点），`Padding` 在
`Border#ContentFrame`（`body` 节点）。因此通过 `DialogContainerStyle` 设置 `BoxShadow` 会在 `Border#Frame` 上绘制的
是**第二个**阴影，与 `PART_ShadowHost` 的既有阴影（`SharedTokenResource BoxShadows`）并存，且 `PART_ShadowHost`
会为既有阴影预留外扩空间，用户阴影可能被压缩或与之重叠。

处理方式（Gate B 决策项）：把 `BoxShadow` 从 `PART_ShadowHost` 迁移到 `container` 节点（`Border#Frame`），使
`container` 与上游一致地独占背景、圆角与阴影；或保留现状并在本节固定“`container` 不含阴影定制”的差异。
本契约默认记录差异，迁移属于渲染结果变更，需 Gate A 明确批准。

### 5.2 `wrapper` 的几何差异

上游 `wrapper`（`.ant-modal-wrap`）是 `position:fixed; inset:0; overflow:auto` 的**全屏**滚动容器，`root` 与
`.ant-modal` 负责定位。AtomUI 的定位、滚动与宿主容量由 `OverlayDialogLayer`/presenter 承担，`wrapper` 映射到
私有动效表面内的 `ContentPresenter.semantic-wrapper`。其几何由 presenter 和宿主分配，
wrapper 承诺的是包裹与定制边界，不单独承担 CSS fixed / overflow 滚动机制。

### 5.3 `header` 的几何差异

AtomUI `header` 是自绘标题栏，除标题外还承载标题图标、最大化/关闭 caption 按钮与拖动命中，且 `HeaderMarginBottom`
落在 `OverlayDialogHeader` 控件本身（`DialogSurfaceTheme` 的 `Margin` Setter），而背景与内边距落在
`Border#HeaderFrame`（`header` 节点）。`header` Part 因此承诺框体级视觉（背景、内边距、边框），不承诺与上游
`.ant-modal-header` 的高度、位置或 wireframe 分隔线一致；`HeaderMarginBottom` 的调整需经 owner 主题或 `header`
节点的 `Margin`（`Border` 同样具备 `Margin`）。

### 5.4 `close` 的位置差异

上游 `.ant-modal-close` 绝对定位在 container 右上角（`top`/`insetInlineEnd` 由 `modalHeaderHeight - modalCloseBtnSize`
推算，并提升 `zIndex`）。AtomUI 的 `PART_CloseButton` 位于 `OverlayDialogHeader` 右侧 `StackPanel#ButtonGroup`，
位置由 `DockPanel.Dock="Right"`、`ButtonGroupSpacing` 与 `CloseBtnSize` 决定。`close` Part 承诺按钮视觉与尺寸，
不承诺上游的绝对定位几何；若需严格复刻上游位置，需重构 `OverlayDialogHeader` 的标题栏布局（属于渲染结果变更）。

## 6. 定制边界

以下区域**不属于** Semantic Part，不承载稳定定制契约：

- `PART_ShadowHost`（`ShadowsAwareContainer`，internal 类型）：阴影绘制与测量宿主；`container` 的阴影归属见 §5.1。
- `PART_SurfaceContentLayer`（`DialogSurfaceTheme` 内部 `DockPanel`）：关闭时承载前景 opacity 动画的内部协作节点
  （`overview.md` 已声明其非 public Part）。
- `PART_Resizer` / `OverlayDialogResizer` 的 8 个 handle：resize 交互区域，上游 Modal 无语义槽位，不发布。
- `PART_MaximizeButton`、`OverlayDialogHeader` 的 `IconPresenter`（标题图标）：上游 Modal 无对应槽位，不发布。
- `PART_ButtonBox` / `DialogButtonBox`：`DialogButtonBox` 是 public 控件并已有独立 `ControlTheme`，用户可直接对其
  定制；不重复发布为 `footer` 的子 Part。Footer 内按钮个体（`DialogButton`）同样不单独发布。
- `MessageBoxContent` 的图标与内容组合：属于 `body` 内容。
- `DialogOverlayLayer`、`DialogOverlayPresenter`、`DialogWindow`、native window chrome：宿主基础设施与窗口契约。

内置 ControlTheme 不得使用 `.semantic-*` 实现默认视觉；marker 只提供应用样式命中点，使用静态
`Classes.semantic-*="True"` 声明，运行期间不根据状态动态增删。

## 7. 兼容性与验证

### 7.1 与改造前的差异

改造前 `overview.md` 曾以非契约的语义摘要描述 `root`/`host`/`surface`/`content`/`motion`（标注
internal-observable）。本次改造将其替换为上述 8 个公共 Part；`host`/`motion` 不再作为公共语义区域，分别归入宿主
基础设施与 `wrapper` Part。

新增 Part、新增生成 Style 与新增静态 marker 属于**兼容增加**；未修改任何既有 `PART_*` 名称、`DialogToken`、
公共属性或默认渲染结果。删除/重命名 Part、修改 `SelectorClass`/`SelectorRoute`、收窄 `ContractType`、改变
`Cardinality` 或移除任一内置模板变体的 marker 均属于**破坏性**主题契约变更。

### 7.2 验证矩阵

实现阶段（Gate B）至少覆盖：

- descriptor 契约：`Dialog` 与 `MessageBox` 的 Part 名称集合、`SelectorClass`、`SelectorRoute`、`ContractType`、
  `Cardinality`、`RuntimeCreated` 与本文一致；内置主题不消费 `.semantic-*` selector。
- 静态 marker 清单：`DialogSurfaceTheme.axaml`（`container`/`body`/`footer`）、`OverlayDialogHeaderTheme.axaml`
  （`header`/`title`/`close`）、`OverlayDialogMaskTheme.axaml`（`mask`）、`OverlayDialogPresenterTheme.axaml`
  （`wrapper`）的 marker 与节点类型逐项断言（`ATOMUIGEN032`/`033` 清零）。
- Overlay 宿主：打开后生成 Semantic Style 命中全部 8 个 Part；`mask`/`wrapper` 在 Window 宿主与 modeless 下的
  `Optional` 语义；`footer`/`close` 的存在性开关。
- owner 逻辑父不变量：`OverlayDialogPresenter` 的逻辑父指向 `Dialog`，teardown 后置空；`GetCrossRoots()` 上报与
  释放；嵌套/堆叠会话的多实例隔离。
- Window 宿主：`container`/`body`/`footer` marker 存在，`mask`/`wrapper`/`header`/`title`/`close` 不物化（Optional）；
  回归测试固定“owner 实例级样式经逻辑父链命中 Window 宿主”与“跨根高亮落在原生窗口内”的边界（先例：
  ImagePreviewer 的 native dialog）。
- 布局协调：`body`/`footer`/`header` 的 `Padding` Setter 与 owner `HostMin/Max`、structural minimum、裁剪的最终结果
  （系统文档 §7.1）。
- `IsLoading` 骨架状态、`DialogHostType` 切换、关闭重开后的 marker 释放，以及 MessageBox 全部语义类型。
- Gallery 语义预览列出全部 Part；Overlay 宿主内联高亮；Window 宿主仅列出描述。NativeAOT publish 与
  `git diff --check`。

### 7.3 与上游的差异（有意保持，非缺陷）

- 上游 `root` 是 fixed 定位容器，AtomUI `root` 是 owner 控件（系统契约）；上游 root 对应内部 layer/presenter。
- 上游 `wrapper` 全屏，AtomUI `wrapper` 与 Surface 同界（§5.2）。
- 上游 `close` 绝对定位，AtomUI `close` 在标题栏按钮组内（§5.4）。
- 上游 `container` 独占 `boxShadow`，AtomUI 阴影当前在 `PART_ShadowHost`（§5.1，待 Gate A 决策）。
- Window 宿主为原生窗口，无 `mask`/`wrapper`/`header`/`title`/`close`；owner 实例级样式经逻辑父链命中（§2）。
