# GroupBox 桌面版架构设计

本文档定义 `AtomUI.Desktop.Controls.GroupBox` 桌面版的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，内部实现原理见 [GroupBox 桌面版实现原理](implementation.md)，Semantic Part 契约见 [GroupBox Semantic Part 契约](semantic-part.md)，GroupBox Token 的专项设计见 [GroupBox Token 设计](token.md)，设计和契约变化记录见 [GroupBox Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/DataDisplay/GroupBox` |
| 控件状态 | Stable |

GroupBox 是桌面端数据展示类分组容器，用于在一块有边框的区域中展示一组相关内容，并通过 Header 标题、图标和标题位置表达分组语义。

GroupBox 的职责是组织和标注内容区域，不负责内容项布局、折叠展开、表单校验、数据绑定集合管理或异步加载。需要折叠行为时应使用 Collapse/Expander 类控件；需要卡片信息组织时应使用 Card。

## 2. 设计语言

GroupBox 的视觉语义接近 fieldset：边框提供分组边界，Header 嵌入在边框上方的视觉通道中，内容区域在边框内部保持稳定留白。

Header 可以位于左侧、居中或右侧，但标题本身始终是分组名称，不应承担操作入口或复杂工具栏职责。Header 图标用于强化分组主题，不改变 GroupBox 的交互模型。

## 3. API 与契约模型

GroupBox 继承 `ContentControl`，公共 API 分为 Header API 和继承的容器 API。

Header API：

| API | 类型 | 语义 |
| --- | --- | --- |
| `HeaderTitle` | `string?` | Header 显示的分组标题。 |
| `HeaderTitleColor` | `IBrush?` | Header 标题颜色，默认来自 `ColorText`。 |
| `HeaderIcon` | `PathIcon?` | Header 标题前的图标；为 `null` 时图标节点不可见。 |
| `HeaderTitlePosition` | `GroupBoxTitlePosition` | Header 内容水平位置，支持 `Left`、`Right`、`Center`。 |
| `HeaderFontSize` | `double` | Header 标题字号。 |
| `HeaderFontStyle` | `FontStyle` | Header 标题字体样式。 |
| `HeaderFontWeight` | `FontWeight` | Header 标题字重。 |

继承 API：

- `Content` / `ContentTemplate` 定义分组内容。
- `Background` 定义内容区域背景，允许透明或半透明。
- `BorderBrush` / `BorderThickness` 定义分组边框。
- `CornerRadius` 定义边框圆角。
- `Padding` 定义内容区域内边距，默认由 GroupBox Token 提供。

GroupBox 在未显式设置 `Height` / `MaxHeight` 等外部约束时，会根据模板根节点的测量结果自动确定高度。该高度包含 Header 通道、内容区域 `Padding` 和内容自身 `DesiredSize`。内容容器仍需遵循 Avalonia 布局语义主动汇报期望尺寸，例如使用 `StackPanel`、`Grid` 或显式尺寸；裸 `Panel` / `Canvas` 等不会自然按子元素累加高度的容器不会被 GroupBox 特殊改写。

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_Frame` | `Border` | 承载整体布局根节点，并提供边框 bounds 参考。 |
| `PART_HeaderContainer` | `Panel` | Header 行容器。 |
| `PART_HeaderContent` | `Decorator` | Header 内容实际 bounds，用于标题缺口和对齐计算。 |
| `PART_HeaderIconPresenter` | `IconPresenter` | Header 图标展示。 |
| `PART_HeaderPresenter` | `TextBlock` | Header 标题展示。 |
| `PART_ContentPresenter` | `ContentPresenter` | 内容承载。 |

### 3.5 Semantic Part 契约

`GroupBox` 公开 `root`、`header`、`icon`、`title`、`content` 五个职责区域，完整契约见 [GroupBox Semantic Part 契约](semantic-part.md)：

| Part | Selector | AtomUI 节点 | Cardinality | 定制方式 |
| --- | --- | --- | --- | --- |
| `root` | 控件本身 | `GroupBox` owner（边框与背景由 owner 自绘） | `Single` | owner 选择器 + 公开属性 |
| `header` | `.semantic-header` | `Border#PART_HeaderContent` | `Single` | `GroupBoxHeaderStyle` |
| `icon` | `.semantic-icon` | `IconPresenter#PART_HeaderIconPresenter` | `Single` | `GroupBoxIconStyle` |
| `title` | `.semantic-title` | `TextBlock#PART_HeaderPresenter` | `Single` | `GroupBoxTitleStyle` |
| `content` | `.semantic-content` | `ContentPresenter#PART_ContentPresenter` | `Single` | `GroupBoxContentStyle` |

GroupBox 是独立控件，五个 Part 全部是 `GroupBoxTheme.axaml` 单一模板内的静态节点，`TemplatedParent` 为 GroupBox owner
本身。因此四个非 root Part 都声明 `RuntimeCreated=false`、`CrossVisualRoot=false`，生成 Style 路由是单一的 `/template/`
边界，没有 `.semantic-scope-*` 中间锚点，也没有条件分支模板、容器创建、prepare/clear/recycle 或跨视觉根路径；五个 Part
在任何状态下都恰好命中一个，不存在 `Optional` 或 `Multiple` Part。定制摘要：

- 分组边框与背景（`BorderBrush`、`Background`、`BorderThickness`、`CornerRadius`）由 owner 自绘，模板中没有承载它们的
  节点，因此只能通过 root 的公开属性定制；任何 Part 上的 `Background` / `BorderBrush` Setter 只作用于该 Part 自身，不会
  改变分组边框。见 [semantic-part.md §2.1](semantic-part.md#21-root)。
- Header 区域的背景、内边距与标题/图标样式通过生成的 Semantic Style 定制；缺口是几何排除而非背景遮挡，因此给 `header`
  设置不透明背景不会在标题下方还原出边框短线。
- `header` 的承载节点在本次改造中由 `Decorator` 提升为 `Border`，以获得 `Background` 能力；它仍是 `Decorator` 的子类，
  模板 part 查找、`Bounds` 语义与缺口几何都不变。
- 标题字体/颜色与内容内边距的主路径仍是 owner 的 `HeaderTitleColor`、`HeaderFontSize`、`HeaderFontWeight`、
  `HeaderFontStyle` 与 `Padding`；Semantic Style 用于 class 级批量覆盖以及 owner 未暴露的属性（如 `TextDecorations`、
  图标尺寸与图标颜色）。
- Header 缺口几何、`PART_Frame`、`PART_HeaderContainer` 与 Token 保留值都不属于 Semantic Part，见
  [semantic-part.md §6](semantic-part.md#6-定制边界)。

## 4. 行为与状态模型

GroupBox 自身没有 hover、pressed、loading、selected、expanded 或 checked 状态。它不拦截输入事件，也不为 Header 提供默认点击行为。

GroupBox 的有效状态来自公共属性、模板测量结果和主题资源：

```text
HeaderTitle / HeaderIcon / HeaderTitlePosition
      + Header font properties
      + template measured bounds
        ↓
Header content bounds
        ↓
Frame border bounds + Header gap bounds
        ↓
Background / BorderBrush / BorderThickness / CornerRadius render state
```

`HeaderIcon`、Header 字体、标题位置和 Header 内容变化会影响缺口尺寸或位置。内容尺寸变化会通过模板根 `PART_Frame` 参与 GroupBox 的 measure pass，使自动高度随内容 `DesiredSize` 增长，同时仍尊重父容器可用空间和显式高度约束。`Background`、`BorderBrush`、`BorderThickness`、`CornerRadius` 改变会影响自绘边框和背景。

## 5. 视觉与主题模型

GroupBox 的默认 Theme 位于 `src/AtomUI.Desktop.Controls/GroupBox/Themes/GroupBoxTheme.axaml`。Theme 提供 Header、Frame 和 Content 的可测量结构，并设置默认背景、边框、圆角、标题颜色、标题字号和间距。

标题缺口是 GroupBox 的视觉契约：Header 内容区域必须从上边框中形成缺口。缺口不能依赖用 `Background` 覆盖边框线，因为 `Background` 允许为透明或半透明。

Theme 职责：

- 设置 `Background`、`BorderBrush`、`BorderThickness`、`CornerRadius` 默认值。
- 设置 `HeaderTitleColor`、`HeaderFontSize`。
- 通过 GroupBox Token 设置 Header 容器外边距、Header 内容内边距、Header 图标间距和内容内边距。
- 根据 `HeaderTitlePosition` 设置 `PART_HeaderContent` 的水平对齐。

Theme 不负责动态构建 Header 或绘制边框。Header 缺口属于控件渲染模型。

## 6. 控件家族或集成关系

GroupBox 是独立桌面控件，不存在派生控件家族。

集成关系：

- Desktop Controls 主题注册：通过桌面控件主题 provider 引入 GroupBox Theme。
- Token 系统：通过 `GroupBoxToken.ScopeProvider` 注册控件 Token 资源作用域。
- Gallery：通过 GroupBox ShowCase 展示基础用法、标题位置、标题样式和标题图标。

GroupBox 不实现 `IFormItemAware`、`ICompactSpaceAware` 或 ItemsControl 相关接口。

## 7. 兼容性不变量

维护 GroupBox 时必须保持以下不变量：

- `HeaderTitle`、`HeaderTitleColor`、`HeaderIcon`、`HeaderTitlePosition`、`HeaderFontSize`、`HeaderFontStyle`、`HeaderFontWeight` 的 API 名称、类型和默认语义不变。
- `GroupBoxTitlePosition.Left`、`Right`、`Center` 的名称和含义不变。
- `PART_Frame`、`PART_HeaderContainer`、`PART_HeaderContent`、`PART_HeaderIconPresenter`、`PART_HeaderPresenter`、`PART_ContentPresenter` 的 template part 名称不变。
- `PART_HeaderContent` 保持为 `Decorator` 的子类（当前为 `Border`，用于承载 Semantic Part `header` 的背景能力），缺口几何继续以它的实际 `Bounds` 为准。
- `Background="Transparent"` 时内容区保持透明，同时 Header 标题下方不应出现边框短线；该契约在 Header 设置不透明背景时同样成立，缺口始终是几何排除而非背景遮挡。
- 未设置显式高度时，GroupBox 的 `DesiredSize.Height` 必须包含 Header 通道、内容内边距和内容自身期望高度，避免内容多时被 Header 或边框区域挤压。
- Header 图标为 `null` 时图标节点不可见，不保留额外图标占位宽度。
- Header 内容位置改变只影响 Header 水平对齐，不改变内容区域布局语义。
- Token 名称和语义不擅自重命名或删除。

## 8. 专项模型

### Header 缺口渲染模型

GroupBox 的边框不是普通完整矩形边框。Header 内容嵌入在边框上方，边框顶部需要在 Header 内容区域断开。

缺口语义要求：

- Header 内容区域必须从上边框中形成缺口。
- 缺口宽度至少覆盖 `PART_HeaderContent` 的实际 bounds，包含 Header 内容内边距。
- 圆角、边框厚度和 DPI 半像素对齐不应因为缺口绘制出现断裂、重叠或多余短线。
- 透明背景、半透明背景和父容器复杂背景下都不能依赖背景遮挡边框线。

## 9. 文档导航与验证策略

关联文档：

- [GroupBox 桌面版实现原理](implementation.md)
- [GroupBox Semantic Part 契约](semantic-part.md)
- [GroupBox Token 设计](token.md)
- [GroupBox Changelog](changelog.md)

本次改造同时移除了此前生成器回退路径产出的占位行 `item` 与 `motion`：GroupBox 没有 item 集合、容器生命周期或动效区域，
按 [Semantic Part 全量改造设计 §5.1](../../../../superpowers/specs/2026-08-12-semantic-part-control-rollout-design.md)不得为了
覆盖率虚构这两类区域。五个区域的真实节点映射与 `PART_Frame`、`PART_HeaderContainer` 的排除依据见
[GroupBox Semantic Part 契约](semantic-part.md)。

验证策略：

| 层次 | 验证内容 |
| --- | --- |
| 文档 | `overview.md`、`implementation.md`、`token.md`、`changelog.md` 链接有效。 |
| Public API | API 表和实际 `GroupBox.cs` 属性、枚举值、默认值一致。 |
| AXAML | Template part 名称、Header 对齐 selector、TokenResource 引用不变。 |
| 渲染 | 验证普通背景、透明背景、不同 Header 位置、带图标和无图标场景下标题缺口无多余边框线。 |
| Token | 验证 API 与 Token 契约与 `GroupBoxToken` 属性一致。 |
| Gallery | 运行 GroupBox ShowCase 相关测试，确认示例结构、源码片段和示例快照稳定。 |
