# Tour Semantic Part 契约

本文档定义 Tour 控件公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。控件整体设计见
[Tour 桌面版架构设计](overview.md)，descriptor、marker 与宿主/模板节点的映射及维护不变量见
[Tour 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

`Tour` 公开 13 个 Semantic Part：`root` 由生成器隐式加入，不要求 `.semantic-root`；其余 12 个为 `popup.*`
弹层部件。除 `root` 外每个 Part 生成 public 强类型 Semantic Style，命名规则为
`AtomUI.Theme.Styling.Tour<PartPathPascalCase>Style`（如 `TourPopupMaskStyle`），用户在外层普通 `Style`
中按 owner 作用域嵌套使用。

部件名单对齐上游 Tour 的语义部件结构（`_semantic.tsx`）：`root`、`mask`、`section`、`cover`、`close`（上游 6.4.0
起）、`header`、`title`、`description`、`footer`、`actions`、`indicators`、`indicator`。上游 Tour 面板整体渲染在
`getPopupContainer=false` 的内联容器中，部件名不带弹层前缀；AtomUI 的引导卡片承载在 `Popup` 的独立视觉根中，
因此除 `root`（owner 本体）外统一以 `popup.` 前缀命名：上游 `mask` 对应 `popup.mask`，上游 `section` 对应
`popup.section`，其余同理。上游 `panel` 包裹节点与箭头不是语义部件，AtomUI 同样不发布（箭头由共享
`ArrowDecoratedBox` 的 `semantic-arrow` marker 承担，但不进入 Tour 契约）。

全部 `popup.*` 部件声明 `CrossVisualRoot=true`：卡片内容在 `Popup` overlay 宿主中（与 owner 同 TopLevel，
样式经逻辑树级联命中）；遮罩在 TopLevel `VisualLayerManager` 的共享 `TourLayer` 中，经逻辑父挂载归属当前打开的
Tour。所有 marker 使用静态 `Classes.semantic-*="True"` 声明（`popup.mask` 与 `popup.indicator` 为运行时代码注入
marker 类，无 TemplatedParent），内置主题不使用 `.semantic-*` selector 实现默认视觉。

### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `Tour` |
| Part | `root` |
| Selector | 不适用（root 无 `.semantic-root`） |
| SelectorRoute | 不适用 |
| ContractType | `Tour` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `Tour` 控件根（隐式；服务型控件，静止态无视觉） |
| 职责 | 引导流程 owner：承载步骤集合、受控开关状态与目标锚定 |
| 相关 API | `IsOpen`、`CurrentIndex`、`Steps`、`StepsSource`、`ShowTour()` |
| 相关 Token | 无独立 Token（流程状态不落 Token） |
| 稳定性 | stable since 6.2.0 |

### `popup.root`

| 字段 | 值 |
| --- | --- |
| Owner | `Tour` |
| Part | `popup.root` |
| Selector | `.semantic-popup-root` |
| SelectorRoute | `/template/ .semantic-popup-root` |
| ContractType | `ArrowDecoratedBox` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `TourTheme.axaml` 模板内 `PART_ArrowDecorator`（静态 marker） |
| 职责 | 引导卡片容器根：承载卡片内容与方向箭头（对齐上游 `.ant-tour` 面板根的容器职责） |
| 相关 API | `Placement`、`IsArrowVisible`、`StyleType` |
| 相关 Token | `TourBorderRadius`、`ColorBgElevated` |
| 稳定性 | stable since 6.2.0 |

### `popup.mask`

| 字段 | 值 |
| --- | --- |
| Owner | `Tour` |
| Part | `popup.mask` |
| Selector | `.semantic-popup-mask` |
| SelectorRoute | `>> .semantic-popup-mask` |
| ContractType | `Control` |
| Cardinality | `Optional` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| CrossNestedOwners | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | TopLevel `VisualLayerManager` 中的共享 `TourLayer`（internal；marker 类由 `Tour` 代码经生成常量 `TourSemanticParts.PopupMaskClass` 注入） |
| 职责 | 遮罩层：覆盖目标区域以外的整屏、镂空高亮当前步骤目标并阻挡交互（对齐上游 mask 的全屏覆盖与指针事件语义） |
| 相关 API | `IsShowMask`、`MaskColor`、`GapRadius`、`GapOffsetX/Y` |
| 相关 Token | 遮罩色默认 `ColorBgMask`（经 `MaskColor` 中继） |
| 稳定性 | stable since 6.2.0 |

遮罩是跨根部件中的特殊形态：物理节点是 VLM 级共享单例（`TourLayer.GetTourLayer` 创建并 `AddLayer`），归属规则
"谁打开谁拥有，关闭即释放"。`Tour.ShowTour()` 挂载时执行三步：`AddLayer`（VLM 内部把层逻辑挂到自身）→
`SetParent(null)`（解除 VLM 归属）→ `SetParent(tourOwner)`（挂到当前 Tour）；`HideTour()` 与生命周期关闭在归属
自己时执行 `SetParent(null)` 释放，下一个打开的 Tour 可重新挂载。owner 作用域 Semantic Style 经 `>>`（沿逻辑树
匹配的 Descendant 选择器）跨视觉根命中遮罩；`Tour` 实现 `ISemanticPartCrossRootProvider`，在遮罩可见时经
`GetCrossRoots()` 上报该层，挂载/释放触发 `CrossRootsChanged` 供语义高亮会话跟随刷新。

`ContractType` 为 `Control`：`TourLayer` 是 internal 类型，不进入公共契约。遮罩颜色定制走 `MaskColor` API
（含镂空几何的绘制由 `TourLayer.Render` 承担，样式通道只承诺 `Visual` 层属性如 `Opacity`、`IsHitTestVisible`）。

### `popup.section`

| 字段 | 值 |
| --- | --- |
| Owner | `Tour` |
| Part | `popup.section` |
| Selector | `.semantic-container`（共享 ArrowDecoratedBox marker） |
| SelectorRoute | `/template/ .semantic-popup-root /template/ .semantic-container` |
| ContractType | `Border` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| CrossNestedOwners | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | 共享 `ArrowDecoratedBoxTheme.axaml` 模板内 `PART_ContentDecorator` Border |
| 职责 | 卡片主要内容区域：圆角、背景、边框与内边距（对齐上游 `.ant-tour` 内 `section` 的卡片样式职责） |
| 相关 API | `StyleType`（Primary 经 popup.root 背景表达） |
| 相关 Token | `TourBorderRadius`、`ColorBgElevated` |
| 稳定性 | stable since 6.2.0 |

marker 声明在共享 `ArrowDecoratedBoxTheme`（与 ToolTip、DatePicker 等共享容器复用同一 `semantic-container`
marker），路由经 owner 模板的 `.semantic-popup-root` 锚点加第二段 `/template/` 进入嵌套模板；生成器按
`CrossNestedOwners` 跨主题资产校验 marker 存在性。

### `popup.cover`

| 字段 | 值 |
| --- | --- |
| Owner | `Tour` |
| Part | `popup.cover` |
| Selector | `.semantic-popup-cover` |
| SelectorRoute | `/template/ .semantic-popup-root >> .semantic-popup-cover` |
| ContractType | `ContentPresenter` |
| Cardinality | `Optional` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| CrossNestedOwners | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `TourStepTheme.axaml` 模板内 `CoverPresenter`（marker 静态声明，节点由 ItemsControl 按步骤物化） |
| 职责 | 卡片封面区域：承载步骤封面图片等内容（对齐上游 `cover`） |
| 相关 API | `TourStep.Cover`、`CoverTemplate` |
| 相关 Token | 无独立 Token |
| 稳定性 | stable since 6.2.0 |

### `popup.close`

| 字段 | 值 |
| --- | --- |
| Owner | `Tour` |
| Part | `popup.close` |
| Selector | `.semantic-popup-close` |
| SelectorRoute | `/template/ .semantic-popup-root >> .semantic-popup-close` |
| ContractType | `Button`（Avalonia `Button` 契约，实际节点为 `DialogCaptionButton`） |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| CrossNestedOwners | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `TourStepTheme.axaml` 模板内 `CloseButton`（`DialogCaptionButton`，marker 静态声明） |
| 职责 | 关闭按钮：结束引导流程（对齐上游 `close`，上游自 6.4.0 发布） |
| 相关 API | `CloseIcon` |
| 相关 Token | `CloseBtnSize`、`IconSize` |
| 稳定性 | stable since 6.2.0 |

### `popup.header`

| 字段 | 值 |
| --- | --- |
| Owner | `Tour` |
| Part | `popup.header` |
| Selector | `.semantic-popup-header` |
| SelectorRoute | `/template/ .semantic-popup-root >> .semantic-popup-header` |
| ContractType | `Border` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| CrossNestedOwners | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `TourStepTheme.axaml` 模板内 header 包裹 Border（组合标题与关闭按钮） |
| 职责 | 卡片头部区域：组合标题与关闭按钮的头部容器（对齐上游 `header`） |
| 相关 API | 无独立 API（内容经 `Title`/`CloseIcon` 进入） |
| 相关 Token | 无独立 Token |
| 稳定性 | stable since 6.2.0 |

### `popup.title`

| 字段 | 值 |
| --- | --- |
| Owner | `Tour` |
| Part | `popup.title` |
| Selector | `.semantic-popup-title` |
| SelectorRoute | `/template/ .semantic-popup-root >> .semantic-popup-title` |
| ContractType | `ContentPresenter` |
| Cardinality | `Optional` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| CrossNestedOwners | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `TourStepTheme.axaml` 模板内 `Title` ContentPresenter |
| 职责 | 引导步骤标题文字（对齐上游 `title`） |
| 相关 API | `TourStep.Title`、`TitleTemplate` |
| 相关 Token | `HeaderColor`、`FontWeightStrong` |
| 稳定性 | stable since 6.2.0 |

### `popup.description`

| 字段 | 值 |
| --- | --- |
| Owner | `Tour` |
| Part | `popup.description` |
| Selector | `.semantic-popup-description` |
| SelectorRoute | `/template/ .semantic-popup-root >> .semantic-popup-description` |
| ContractType | `ContentPresenter` |
| Cardinality | `Optional` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| CrossNestedOwners | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `TourStepTheme.axaml` 模板内 `DescriptionPresenter` |
| 职责 | 引导步骤描述文字（对齐上游 `description`） |
| 相关 API | `TourStep.Description`、`DescriptionTemplate` |
| 相关 Token | 无独立 Token（沿用文本 Token） |
| 稳定性 | stable since 6.2.0 |

### `popup.footer`

| 字段 | 值 |
| --- | --- |
| Owner | `Tour` |
| Part | `popup.footer` |
| Selector | `.semantic-popup-footer` |
| SelectorRoute | `/template/ .semantic-popup-root >> .semantic-popup-footer` |
| ContractType | `Border` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| CrossNestedOwners | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `TourStepsViewTheme.axaml` 模板内 `FooterFrame` Border |
| 职责 | 卡片底部操作区域：组合指示器与操作按钮组（对齐上游 `footer`） |
| 相关 API | 无独立 API（内容经 `Indicator`/`CustomActions` 进入） |
| 相关 Token | 无独立 Token |
| 稳定性 | stable since 6.2.0 |

### `popup.actions`

| 字段 | 值 |
| --- | --- |
| Owner | `Tour` |
| Part | `popup.actions` |
| Selector | `.semantic-popup-actions` |
| SelectorRoute | `/template/ .semantic-popup-root >> .semantic-popup-actions` |
| ContractType | `StackPanel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| CrossNestedOwners | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `TourStepsViewTheme.axaml` 模板内 `ActionsLayout` StackPanel |
| 职责 | 操作按钮组容器：承载上一步/下一步/完成按钮（对齐上游 `actions`） |
| 相关 API | `CustomActions`、步骤导航事件 |
| 相关 Token | `PrimaryPrevBtnBg`、`PrimaryNextBtnHoverBg` |
| 稳定性 | stable since 6.2.0 |

### `popup.indicators`

| 字段 | 值 |
| --- | --- |
| Owner | `Tour` |
| Part | `popup.indicators` |
| Selector | `.semantic-popup-indicators` |
| SelectorRoute | `/template/ .semantic-popup-root >> .semantic-popup-indicators` |
| ContractType | `ContentPresenter` |
| Cardinality | `Optional` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| CrossNestedOwners | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `TourStepsViewTheme.axaml` 模板内 `IndicatorPresenter` ContentPresenter |
| 职责 | 指示器组容器：承载当前 `Indicator` 实例（对齐上游 `indicators`） |
| 相关 API | `Indicator` |
| 相关 Token | 无独立 Token |
| 稳定性 | stable since 6.2.0 |

### `popup.indicator`

| 字段 | 值 |
| --- | --- |
| Owner | `Tour` |
| Part | `popup.indicator` |
| Selector | `.semantic-popup-indicator` |
| SelectorRoute | `/template/ .semantic-popup-root >> .semantic-popup-indicator` |
| ContractType | `Ellipse` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| CrossNestedOwners | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `DefaultTourIndicator` 代码按 `StepCount` 物化的 `Ellipse` 圆点（marker 类与 `active` 状态类由代码注入） |
| 职责 | 单个步骤指示器圆点，含激活态（对齐上游 `indicator`） |
| 相关 API | `IndicatorSize`、`IndicatorColor`、`IndicatorActiveColor`、`ItemSpacing` |
| 相关 Token | `IndicatorSize`、`ColorFill`、`ColorPrimary` |
| 稳定性 | stable since 6.2.0 |

圆点由 `DefaultTourIndicator` 代码物化并只挂 `semantic-popup-indicator` marker 类与 `active` 激活态类；
视觉（尺寸/颜色/间距）全部由 `DefaultTourIndicatorTheme` 表达——圆点无 TemplatedParent，`/template/` 选择器链
无法命中，主题样式声明在模板内 `DotsLayout` 元素作用域（`StackPanel.Styles`）沿逻辑树流到圆点，尺寸/颜色经
`RelativeSource AncestorType` 绑定到指示器实例。激活态经 `.active` 类选择器表达。布局契约保持旧自绘版公式
`N*size + (N+1)*spacing`（`MeasureOverride` 覆写承担，左右各留一份间距）。

`TextTourIndicator` 不物化圆点，使用该指示器时 `popup.indicator` 实例数为 0（`Multiple` 允许 0..N）。

## 2. 职责与存在条件

- `root` 是流程 owner：静止态无视觉，只承载状态；语义预览中描边 owner 本体。
- `popup.root` 是卡片容器：`Popup` overlay 宿主打开时存在；`IsPopupPinnedOpen=true`（public，见 §4）时钉住常开。
- `popup.mask` 仅遮罩可见时存在（`IsShowMask` 为真且引导打开）；`Optional`，关闭释放逻辑父后不残留。
- `popup.section` 随 `popup.root` 存在：共享 `ArrowDecoratedBox` 模板的 `PART_ContentDecorator`。
- `popup.cover` 仅当前步骤设置 `Cover` 时有内容（presenter 恒在，未设置时 `Optional` 语义按实例数 0 处理）。
- `popup.close`/`popup.header`/`popup.title`/`popup.description` 由 `TourStepsView` 的 ItemsControl 按步骤物化，
  每个打开的步骤模板各存在一份（当前步骤可见，非当前步骤 `IsVisible=false` 但 marker 保持）。
- `popup.footer`/`popup.actions`/`popup.indicators` 随 `TourStepsView` 模板存在（单份）。
- `popup.indicator` 由 `DefaultTourIndicator` 按 `StepCount` 物化（0..N）。

## 3. 数量语义

`root`、`popup.root`、`popup.section`、`popup.close`、`popup.header`、`popup.footer`、`popup.actions` 为
`Single`；`popup.mask`、`popup.cover`、`popup.title`、`popup.description`、`popup.indicators` 为 `Optional`；
`popup.indicator` 为 `Multiple`（随 `StepCount` 增减）。步骤切换只改变可见性与激活态类，不增删 marker；
打开、关闭与重开不维护跨打开的 marker 状态。

## 4. Selector 用法

生成的 Semantic Style 类型位于命名空间 `AtomUI.Theme.Styling`（AXAML 命名空间 `https://atomui.net`）。`root`
不生成 Style 类型，owner 级 Setter 写在外层普通 Style 上。推荐写法与 Gallery Tour「自定义语义结构的样式」示例一致
（对齐上游 `style-class.tsx` 的 object/function styles 示例）：

```xml
<atom:Tour Classes="semantic-object-style-demo"
           IsOpen="{Binding TourOpened, Mode=TwoWay}"
           MaskColor="#4D000000"
           IsArrowVisible="False">
    <atom:Tour.Styles>
        <Style Selector="atom|Tour.semantic-object-style-demo">
            <atom:TourPopupRootStyle x:SetterTargetType="atom:ArrowDecoratedBox">
                <Setter Property="CornerRadius" Value="4" />
            </atom:TourPopupRootStyle>
            <atom:TourPopupSectionStyle x:SetterTargetType="Border">
                <Setter Property="CornerRadius" Value="8" />
                <Setter Property="BorderBrush" Value="#4096ff" />
                <Setter Property="BorderThickness" Value="2" />
                <Setter Property="BoxShadow" Value="0 4px 12px rgba(0,0,0,0.15)" />
            </atom:TourPopupSectionStyle>
            <atom:TourPopupCoverStyle x:SetterTargetType="ContentPresenter">
                <Setter Property="CornerRadius" Value="12,12,0,0" />
            </atom:TourPopupCoverStyle>
            <!-- 遮罩颜色走 MaskColor API；专用 Style 只承诺 Visual 层属性 -->
            <atom:TourPopupMaskStyle x:SetterTargetType="Control">
                <Setter Property="Opacity" Value="0.5" />
            </atom:TourPopupMaskStyle>
        </Style>
    </atom:Tour.Styles>
    <!-- steps... -->
</atom:Tour>
```

`popup.*` 生成 Style 由 `Nesting()` 沿 owner 逻辑树命中：卡片部件经 `Popup` overlay 宿主（与 owner 同
TopLevel）；遮罩经共享 `TourLayer` 的逻辑父挂载命中。无需在用户 Style 中复写 `>>` route。

`IsPopupPinnedOpen` 自 6.0 起 public（此前 internal）：钉住弹层常开，用于语义预览与设计检查场景。Gallery
语义预览对齐上游 `_semantic.tsx`（默认打开、居中锚点按钮、首步带封面），并在舞台作用域用
`TourPopupMaskStyle` 设置 `IsHitTestVisible=False`——遮罩全屏覆盖且默认参与命中测试，预览舞台需放行指针事件
才能 hover 部件卡；这同时演示了上游 mask 语义中的"指针事件"定制维度。

不得使用以下写法：

- `.semantic-root`、`PART_*`、Name selector、internal 类型（`TourLayer`、`TourStepsView` 内部件）作为应用主题契约。
- 把 `ContractType`（如 `Control.semantic-popup-mask`）写入 Part 身份 selector；应使用生成 Style 的
  `x:SetterTargetType` 提供类型上下文。
- 直接复制 `/template/ .semantic-popup-root` route；锚点类只用于生成 Style 的 owner-relative 路由。
- 绕过 `TourPopupIndicatorStyle` 在 code-behind 获取圆点后直接设置视觉属性。

## 5. 定制边界

以下区域不属于 Tour Semantic Part：

- **箭头**：共享 `ArrowDecoratedBox` 的 `semantic-arrow` marker 存在，但不发布为 Tour 部件（上游未将 arrow 列为
  semantics；上游 `arrow=false` 由 `IsArrowVisible` API 承担）。
- **操作按钮个体**：`popup.actions` 只承诺按钮组容器本体，上一步/下一步/完成按钮不单独发布（上游同样只发布
  actions 容器）。
- **指示器文本**：`TextTourIndicator` 的文本呈现不是语义部件；`popup.indicator` 只对 `DefaultTourIndicator` 物化。
- **遮罩镂空几何**：目标区域镂空、圆角与偏移由 `GapRadius`/`GapOffsetX/Y` 与 `TourLayer.Render` 承担，
  `popup.mask` 样式通道只承诺 `Visual` 层属性。
- `PART_*` 名称、internal 类型、`.semantic-*` 路由标记与模板层级。

默认主题不消费 `.semantic-*` selector；静态 marker 只提供应用样式命中点。Semantic Style 服从 Avalonia 原生
属性优先级。

## 6. 兼容性与验证

删除或重命名 Part、修改 selector class / route、收窄 `ContractType`、改变 cardinality，或让任一内置模板变体
缺少 marker，均属于公共主题契约变更。`IsPopupPinnedOpen` 由 internal 转 public 是兼容放宽，不破坏既有用法。

与上游 Tour 的对照差异（有意保持）：

- 上游语义预览把 Tour 内联渲染在 600px 高、`overflow:hidden` 的舞台容器中且 mask 不渲染；AtomUI 语义预览使用
  `Popup` overlay 宿主钉住常开并渲染遮罩，遮罩经 `TourPopupMaskStyle` 放行指针事件保证部件卡可 hover。
- 上游部件名不带 `popup.` 前缀（内联容器）；AtomUI 因卡片在 Popup 视觉根中而加前缀，映射关系见 §1。
- 上游 `close` 自 6.4.0 发布；AtomUI 随语义部件整体自 6.0 发布。
- 上游遮罩色经 `styles.mask.backgroundColor` 定制；AtomUI 遮罩颜色走 `MaskColor` API（含镂空绘制的绘制路径），
  样式通道只承诺 `Visual` 层属性。

验证至少覆盖（实现测试矩阵见 [实现原理](implementation.md)）：

- owner descriptor 只包含本节 13 个 Part，字段值与本节一致；内置模板不消费 `.semantic-*` selector。
- 四个主题文件的静态 marker 清单（TourTheme/TourStepTheme/TourStepsViewTheme/DefaultTourIndicatorTheme）。
- 生成 Semantic Style 命中全部 `popup.*` 目标（含代码物化的圆点与遮罩）。
- 遮罩逻辑父挂载/释放：打开归属当前 Tour、关闭释放、第二个 Tour 打开后重新归属；`GetCrossRoots()` 打开时
  上报 TourLayer、关闭为空。
- 圆点物化数量与激活态、布局公式 `N*size + (N+1)*spacing`、主题样式真正落到圆点（尺寸 > 0）。
- Gallery 语义预览列出全部 13 个 Part 并全部参与高亮；style-class 示例锁定专用 Style 形态（无 code-behind
  回退）。NativeAOT publish；descriptor 与生成 Style 使用编译期生成数据，不依赖运行时反射或 VisualTree 扫描。
