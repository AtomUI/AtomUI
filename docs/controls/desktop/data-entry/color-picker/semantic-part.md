# ColorPicker Semantic Part 契约

本文档定义 `ColorPicker` 与 `GradientColorPicker` 控件公开的 Semantic Part、Selector、类型约束、数量语义
和定制边界。控件整体设计见 [ColorPicker 桌面版架构设计](overview.md)，真实模板与 marker 映射见
[ColorPicker 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

`ColorPicker` 与 `GradientColorPicker` 各自是唯一 Semantic owner，公开 5 个 Semantic Part（与上游
ColorPicker 的官方语义 API 逐一对齐：`root` / `body` / `content` / `description` / `popup.root`）。
声明分别位于 `ColorPicker.SemanticParts.cs` 与 `GradientColorPicker.SemanticParts.cs` partial 文件。

触发区部件（`body` / `description`）与弹层根部件（`popup.root`）的 marker 均位于两个 owner 自有的
`Themes/ColorPickerTheme.axaml`、`Themes/GradientColorPickerTheme.axaml` 宿主模板内；`content` 的
marker 位于共享的 `Themes/ColorBlockTheme.axaml`（ColorBlock 自身模板内），因此 `content` 声明
`CrossNestedOwners=true`——这与 Cascader 的 `clear` 部件（marker 在共享 `SelectHandle` 模板内）同构。

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `ColorPicker` / `GradientColorPicker` |
| Part | `root` |
| Selector | owner 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `ColorPicker` / `GradientColorPicker` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | owner 根 |
| 职责 | 触发器容器：边框、圆角、尺寸、状态与布局的组织边界。 |
| 相关 API | `SizeType`、`Status`、`BorderBrush`、`TriggerPadding` 等 owner public API |
| 相关 Token | ColorPickerToken、SharedToken（ColorBorder、BorderRadius*） |
| 稳定性 | stable since 6.2.0 |

#### `body`

| 字段 | 值 |
| --- | --- |
| Owner | `ColorPicker` / `GradientColorPicker` |
| Part | `body` |
| Selector | `.semantic-body` |
| SelectorRoute | `/template/ .semantic-body` |
| Style Type | `ColorPickerBodyStyle` / `GradientColorPickerBodyStyle` |
| ContractType | `Control` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | owner 模板中的 `PART_ColorIndicator`（内部控件 `ColorBlock` 的模板实例节点；公共契约承诺 Avalonia `Control`） |
| 职责 | 触发器内的色块容器，承载底色、空色斜线与棋盘格呈现。 |
| 相关 API | `ColorBlockSize`、`ColorBlockBackground` |
| 相关 Token | ColorPickerHandlerSize*、ColorBlockInnerShadows |
| 稳定性 | stable since 6.2.0 |

#### `content`

| 字段 | 值 |
| --- | --- |
| Owner | `ColorPicker` / `GradientColorPicker` |
| Part | `content` |
| Selector | `.semantic-content` |
| SelectorRoute | `/template/ .semantic-body /template/ .semantic-content` |
| Style Type | `ColorPickerContentStyle` / `GradientColorPickerContentStyle` |
| ContractType | `Border` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `ColorBlockTheme.axaml` 中 `PART_ColorPreview`（`semantic-body` 节点自有模板内） |
| 职责 | 色块颜色元素，呈现实际选择的颜色填充。 |
| 相关 API | —（随 owner 的 `Value` 联动，不单独开放） |
| 相关 Token | ColorBlockInnerShadows |
| 稳定性 | stable since 6.2.0 |

#### `description`

| 字段 | 值 |
| --- | --- |
| Owner | `ColorPicker` / `GradientColorPicker` |
| Part | `description` |
| Selector | `.semantic-description` |
| SelectorRoute | `/template/ .semantic-description` |
| Style Type | `ColorPickerDescriptionStyle` / `GradientColorPickerDescriptionStyle` |
| ContractType | `TextBlock`（GradientColorPicker：`Panel`） |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `ColorPickerTheme.axaml` 中 `PART_ColorText`；`GradientColorPickerTheme.axaml` 中 `PART_ColorTextPanel`（WrapPanel，逐渐变 stop 的文本格） |
| 职责 | 触发器文本区：单色模式显示格式化颜色文本；渐变模式显示逐 stop 文本格。 |
| 相关 API | `IsTextVisible`、`ColorTextFormatter`（attached）、`Format` |
| 相关 Token | TriggerTextMargin、SharedToken（FontSize*、ColorText） |
| 稳定性 | stable since 6.2.0 |

#### `popup.root`

| 字段 | 值 |
| --- | --- |
| Owner | `ColorPicker` / `GradientColorPicker` |
| Part | `popup.root` |
| Selector | `.semantic-popup-root` |
| SelectorRoute | `/template/ .semantic-popup-root` |
| Style Type | `ColorPickerPopupRootStyle` / `GradientColorPickerPopupRootStyle` |
| ContractType | `Border` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `false` |
| AtomUI 节点 | owner 模板 `PART_Popup` 直接子节点 `ColorPickerPopupRootFrame`（`Border` 子类，向内容层 `ArrowDecoratedBox` 转发 `IArrowAwareShadowMaskInfoProvider`），语义边框贴合弹层外沿（对齐上游 popup root） |
| 职责 | 弹层根容器：承载弹层边框、背景类视觉的定制入口；弹层 View 本身由 `CreatePresenter()` 动态创建，不经此 Part 发布。 |
| 相关 API | `IsPopupPinnedOpen`（6.0 公共化）、`Placement`、`IsArrowVisible` |
| 相关 Token | ColorPickerInsetShadow、SharedToken（ColorBgElevated） |
| 稳定性 | stable since 6.2.0 |

### GradientColorPicker 差异

`GradientColorPicker` 与 `ColorPicker` 共享同一套 5 部件语义契约，仅 `description` 的 `ContractType`
不同：触发文本区是 `WrapPanel`（`PART_ColorTextPanel`，内含逐 stop 的文本格），公共契约放宽为
`Panel`。其推荐 selector 为 `atom|GradientColorPicker /template/ Panel.semantic-description`。其余四
部件的 selector、ContractType、Cardinality 与 `ColorPicker` 完全一致。

## 2. 职责与存在条件

- `body` / `description` / `popup.root` 的 marker 在 owner 宿主模板内，默认 route（`/template/
  .<class>`），不声明 `CrossNestedOwners`。
- `content` 的物理节点在 `ColorBlock` 自身模板内，route 以 `body` 的 marker 为锚点经第二个
  `/template/` 跨入其模板；声明 `CrossNestedOwners=true`。弹层内的 `ColorBlock` 实例（清除示例、
  颜色预览）不带 `semantic-body`，selector 链不会误命中。
- `popup.root` 采用静态包裹层而非弹层 View 内节点的原因：弹层 View 由 `CreatePresenter()` 在打开时
  动态创建，其模板节点的 `TemplatedParent` 链断在 View 上，`/template/` route 与 `>>` 视觉步进均无法
  从 owner 可达；owner 模板内的静态 `Border` 在 overlay 弹层（`ShouldUseOverlayPopup=true`）下经
  `TemplatedParent` 传播保持可达。`PART_Popup` 直接子节点是 `ColorPickerPopupRootFrame`（`Border`
  子类，向内容层 `ArrowDecoratedBox` 转发 `IArrowAwareShadowMaskInfoProvider`，共享 Popup 的箭头
  布局与阴影遮罩机制只识别直接 Child，见 §5），语义边框贴合弹层外沿（对齐上游 popup root）；
  该节点默认无背景无边框，零视觉影响。
- 所有部件均为静态模板节点，无 `RuntimeCreated=true` 部件。

## 3. 数量语义

`root`、`body`、`content`、`description`、`popup.root` 均为 `Single`。状态变化（清空颜色、disabled、
`IsTextVisible=false`、弹层开合、单色/渐变文本格数量）只切换可见性或有效视觉值，不增删 marker：
`description` 关闭文本时节点仍在模板中（`IsVisible=false`）；渐变模式逐 stop 的文本格由
`PART_ColorTextPanel` 一个 Part 统一覆盖。

## 4. Selector 用法

生成的 Semantic Style 类型命名为 `<Owner><PartPathPascalCase>Style`，如
`ColorPickerPopupRootStyle`、`GradientColorPickerDescriptionStyle`（命名空间 `AtomUI.Theme.Styling`，
AXAML 命名空间 `https://atomui.net`）。`root` 不生成 Style 类型，owner 级 Setter 写在外层普通 Style 上。

推荐写法（owner 嵌套 Style + 语义 class，与 Gallery
`ShowCases/DataEntry/ColorPicker` 的「自定义语义结构的样式」示例一致）：

```xml
<StackPanel.Styles>
    <Style Selector="atom|ColorPicker.semantic-styles-demo">
        <!-- root：owner 级定制 -->
        <Setter Property="CornerRadius" Value="{atom:SharedTokenResource BorderRadius}" />
        <!-- popup.root：typed part style -->
        <atom:ColorPickerPopupRootStyle x:SetterTargetType="Border">
            <Setter Property="BorderBrush" Value="#FFFFFF" />
            <Setter Property="BorderThickness" Value="1" />
        </atom:ColorPickerPopupRootStyle>
    </Style>
</StackPanel.Styles>
<atom:ColorPicker Classes="semantic-styles-demo" ... />
```

单实例定制也可以直接把 typed part style 挂在 owner 的 `Styles` 上：

```xml
<atom:ColorPicker.Styles>
    <atom:ColorPickerBodyStyle x:SetterTargetType="Control">
        <Setter Property="Opacity" Value="0.8" />
    </atom:ColorPickerBodyStyle>
</atom:ColorPicker.Styles>
```

不得使用以下写法：

- `.semantic-root`、`PART_*`、Name selector、internal 类型（如 `atom:ColorBlock`、`atom:ArrowDecoratedBox`）
  或视觉祖先顺序作为应用主题契约。
- 把 `ContractType` 之外的类型写入 Part 身份 selector（Part 身份只由 `.semantic-*` class 承载）。
- 直接复制 `/template/ .semantic-*` route 字符串；route 只用于生成 Style 的 owner-relative 路由。
- 穿过弹层 View 的内部模板（滑杆、色谱、输入区、预设色板）匹配——它们不属于语义契约（见 §5）。

## 5. 定制边界

以下区域不属于 ColorPicker Semantic Part：

- **弹层内部结构**：色谱（`ColorSpectrum`）、色相/透明度滑杆（`ColorSlider`）、渐变滑杆
  （`GradientColorSlider`）、格式输入区（`ColorPickerInput`）、预设色板（`ColorPickerPaletteGroup`）
  均由各自控件的行为 API 与 Token 承担，不经 owner 发布 Part（上游公开语义 API 同样不覆盖这些区域）。
- **触发器 frame**：`PART_Frame` 的边框、内边距由 owner Root 级 API（`BorderBrush`、`TriggerPadding`、
  `Status`）承担。
- **弹层 Popup 宿主与定位**：弹层的定位、钉住打开、动画由共享 Popup 契约承担；`popup.root` 覆盖弹层根
  包裹 Border 的视觉，不含箭头绘制本身（箭头由 `IsArrowVisible`/`ArrowPosition` API 控制）。
- `PART_*` 名称、internal 类型、模板层级与视觉树顺序。

默认主题不消费 `.semantic-*` selector；静态 marker 只提供应用样式命中点，不改变默认属性优先级或增加状态
订阅。Semantic Style 服从 Avalonia 原生属性优先级。

## 6. 兼容性与验证

删除或重命名 Part、修改 selector class / route、收窄 `ContractType`（含把 `Control` 收窄为具体实现
类型）、改变 cardinality，或让任一内置模板变体缺少 marker，均属于公共主题契约变更。

验证至少覆盖：

- owner descriptor 只包含 §1 的 5 个 Part，字段值与本文一致
  （`tests/AtomUI.Desktop.Controls.Tests/ColorPicker/ColorPickerSemanticPartTests.cs`）。
- `ColorPickerTheme.axaml` / `GradientColorPickerTheme.axaml` 携带 `body` / `description` /
  `popup.root` 的静态 marker（含弹层包裹 Border）；`ColorBlockTheme.axaml` 携带 `content` marker。
- 生成的 `ColorPicker*Style` / `GradientColorPicker*Style` 可编译，并在触发区与 overlay 弹层
  （`IsPopupPinnedOpen` 钉住展开）两条路径下命中目标节点（同上测试文件）。
- 状态变化（清空颜色、disabled、`IsTextVisible`、弹层开合）不改变 marker 身份与数量语义。
- 默认主题不消费 `.semantic-*`，未声明用户 Semantic Style 时不增加 selector activator。
- Gallery Semantic Parts 页签延迟创建 Preview，弹层经 `IsPopupPinnedOpen` 钉住常开，5 个 Part 均可
  解析高亮。
- descriptor、生成 Style 与 NativeAOT 路径使用编译期生成数据，不依赖运行时反射或 VisualTree 扫描。
