# Segmented Token 设计

本文档定义 `AtomUI.Desktop.Controls.SegmentedToken` 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。Segmented 整体架构见 [Segmented 桌面版架构设计](overview.md)，内部实现原理见 [Segmented 桌面版实现原理](implementation.md)，设计和契约变化记录见 [Segmented Changelog](changelog.md)。

## 1. 定位

SegmentedToken 是 Segmented 的控件级 Token scope，描述分段轨道、选项文本状态、选项背景状态、选中滑块背景和 item 尺寸的主题语义。

SegmentedToken 不承载以下状态：

- `Items`、`ItemsSource`、`ItemTemplate`、`Content` 等数据状态。
- `SelectedIndex`、`SelectedItem`、`:selected`、`:pressed`、`:has-icon` 等实例或伪类状态本身。
- `SelectedThumbPos`、`SelectedThumbSize` 等运行时布局派生状态。
- `Orientation`、`IsExpanding`、可见 item 数量、排列轴和等分宽度等布局状态。
- `Shape` 和 Round 胶囊圆角；它们是实例形状状态和几何覆盖，不是主题尺度。
- `IsMotionEnabled` 或 transition 时长开关；motion 时长来自 SharedToken。

## 2. Token 分类

### 2.1 轨道 Token

- `TrackPadding`
- `TrackBg`

`TrackPadding` 控制根轨道内边距，也是 item 最小高度计算的扣减来源。`TrackBg` 控制根轨道背景色。

### 2.2 item 文本状态 Token

- `ItemColor`
- `ItemHoverColor`
- `ItemSelectedColor`

这些 Token 控制 item 默认、hover 和 selected 状态的文字颜色。`SegmentedItem.Icon` 的 `IconPresenter` 也使用同一组语义同步图标颜色。

### 2.3 item 背景状态 Token

- `ItemHoverBg`
- `ItemActiveBg`
- `ItemSelectedBg`

`ItemHoverBg`、`ItemActiveBg` 控制未选中 item 的 hover 和 pressed 背景遮罩。`ItemSelectedBg` 只用于根控件
render 层的选中滑块背景：选中背景在任何时刻都由滑块独占承载，item 自身不绘制选中背景（含滑块飞行途中）。

### 2.4 item 尺寸 Token

- `ItemMinHeightLG`
- `ItemMinHeight`
- `ItemMinHeightSM`

这些 Token 控制 Large、Middle/Custom、Small 三个主题分支下 item 的最小高度。它们由 SharedToken 的 `ControlHeightLG`、`ControlHeight`、`ControlHeightSM` 扣除轨道上下 padding 计算得到。

### 2.5 内部间距 Token

- `SegmentedItemPadding`
- `SegmentedItemPaddingSM`
- `SegmentedItemContentMargin`

这些字段用于 Segmented item 主题内部：

- `SegmentedItemPadding` 用于 Large、Middle 和 Custom。
- `SegmentedItemPaddingSM` 用于 Small。
- `SegmentedItemContentMargin` 用于 `:has-icon` 时图标和内容之间的间距。

内部间距 Token 不作为公开 Design Token 表展示，但它们已经被 AXAML 主题引用，维护时仍按主题契约处理。

## 3. 控件专项模型中的 Token 使用

SegmentedToken 使用路径：

```text
SharedToken
   ↓
SegmentedToken
   ↓
SegmentedTheme / SegmentedItemTheme
   ↓
track + selected thumb + item states + item size
```

重要映射：

| Token | 主要使用点 |
| --- | --- |
| `TrackPadding` | 根 `Segmented` padding，item 最小高度计算。 |
| `TrackBg` | 根轨道背景。 |
| `ItemColor` | 未选中 item foreground 和 icon brush。 |
| `ItemHoverColor` | 未选中 item hover foreground 和 icon brush。 |
| `ItemHoverBg` | 未选中 item hover 背景。 |
| `ItemActiveBg` | 未选中 item pressed 背景。 |
| `ItemSelectedBg` | 根控件 render 层选中滑块背景；item 自身不绘制选中背景。 |
| `ItemSelectedColor` | 选中 item foreground、icon brush，以及 pressed icon brush。 |
| `ItemMinHeightLG` | Large item 最小高度。 |
| `ItemMinHeight` | Middle 和 Custom item 最小高度。 |
| `ItemMinHeightSM` | Small item 最小高度。 |
| `SegmentedItemPadding` | Large、Middle、Custom item padding。 |
| `SegmentedItemPaddingSM` | Small item padding。 |
| `SegmentedItemContentMargin` | 图标和文本之间的 margin。 |

实例状态派生：

```text
SizeType
  → choose root radius, thumb radius, item min height, item padding, font size, icon size

Shape=Round
  → override root radius + thumb radius + item radius with capsule geometry

Icon != null
  → :has-icon
  → SegmentedItemContentMargin

IsMotionEnabled
  → transitions from SharedToken motion duration
```

`SelectedThumbPos`、`SelectedThumbSize`、排列方向、等分宽度、Shape 和当前选中项是运行时派生或实例状态，不是 Token。

Round 使用足够大的固定 CornerRadius，根据控件实际 Bounds 形成胶囊。该值不进入 SegmentedToken：如果把它设计为普通主题半径，主题覆盖可能破坏 Round 必须始终保持胶囊的形状契约。

## 4. 控件家族影响

SegmentedToken 只直接影响 Segmented 及其 item：

- `SegmentedTheme.axaml`
- `SegmentedItemTheme.axaml`
- Segmented token.md 语义说明

其他数据展示控件、选择控件和输入控件不复用 SegmentedToken。若多个控件需要共享同一语义，应评估是否上升为 SharedToken，而不是跨控件引用 SegmentedToken。

## 5. 兼容性要求

SegmentedToken 属于 Segmented 主题契约。即使 `SegmentedToken` 是 internal 类型，生成的 token kind、AXAML resource 使用点和 token.md 语义说明已经形成稳定依赖。

Token 变更要求：

- 不擅自重命名或删除现有 Token。
- 不把选择状态、hover/pressed 状态、当前 item、滑块坐标或 expanding 等分宽度迁移为 Token。
- 不把 `SizeType`、`Orientation`、`Shape`、`IsExpanding` 或 `IsMotionEnabled` 变成 Token；它们是实例行为或变体属性。
- 修改 `TrackPadding` 时必须同时验证轨道 padding、item 最小高度和选中滑块边界。
- 修改 item 状态色时必须同时验证文字和图标颜色。
- 修改 item 尺寸 Token 时必须验证 Large、Middle、Small、Custom 及图标/文本组合。
- 删除或重命名 token.md 记录的 Token 必须先获得授权，并同步 AXAML、生成 token kind、Gallery 示例和文档。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| 修改轨道 Token | 验证根轨道背景、padding、圆角裁剪和选中滑块边界。 |
| 修改文本状态 Token | 验证默认、hover、selected、disabled 下文字和图标颜色。 |
| 修改背景状态 Token | 验证 hover、pressed item 背景遮罩和根 render 层选中滑块背景。 |
| 修改 item 高度 Token | 验证 Large / Middle / Small / Custom 下高度、文字居中、图标居中和滑块尺寸。 |
| 修改共享圆角映射 | 验证 `Shape=Default` 的各 SizeType 圆角，并确认 `Shape=Round` 仍最终覆盖根、item 和滑块为胶囊。 |
| 修改内部间距 Token | 验证纯文本、纯图标、图标加文本示例的内容间距。 |
| 删除或重命名 Token | 默认不允许；如获授权，需同步 AXAML 引用、生成文件、Token 类型、生成数据和 token.md和控件文档。 |
| 文档改动 | 运行 `git diff --check`，检查文档链接存在。 |
