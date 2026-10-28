# Segmented 桌面版架构设计

本文档定义 `AtomUI.Desktop.Controls.Segmented` 桌面版的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，内部实现原理见 [Segmented 桌面版实现原理](implementation.md)，Segmented Token 的专项设计见 [Segmented Token 设计](token.md)，设计和契约变化记录见 [Segmented Changelog](changelog.md)。

## 1. 控件定位

选中滑块首次出现时直接使用当前选中容器的完整矩形，后续有效选择变化保留飞行动画。就绪状态、首次有效几何
与主题过渡的共享契约见 [状态动效初始化设计](../../../../architecture/systems/control-infrastructure/transition-initialization.md)，
源码接入差异与验证边界见 [状态动效初始化验证](../../../../architecture/systems/control-infrastructure/transition-initialization-verification.md)。

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/DataDisplay/Segmented` |
| 控件状态 | Stable |

Segmented 是桌面端数据展示类的紧凑单选控件，用于在少量互斥选项之间切换当前视图、展示模式、时间粒度或状态筛选。它通过分段轨道、选项 item、选中滑块、图标和文本表达“当前只有一个有效选择”的语义。

Segmented 的职责是承载一组有限选项并维护单选状态。它不是完整导航系统、Tab 容器、列表选择器、弹层选择器、表单编辑器或大数据集合控件；复杂页面导航、动态搜索、虚拟化、多选和异步加载应由专用控件或业务层承担。

## 2. 设计语言

Segmented 的设计语言来自 参考设计体系的分段控制器：浅色轨道承载一组互斥选项，选中滑块跟随当前项移动，选项文本和图标在 hover、pressed、selected、disabled 状态下提供即时反馈。

| 维度 | 含义 | 典型表达 |
| --- | --- | --- |
| 单选语义 | 同一组中只有一个选项处于选中态。 | `SelectedIndex`、`SelectedItem`、`:selected`。 |
| 视觉反馈 | 用滑块和 item 状态区分 hover、pressed、selected。 | `SelectedThumbBg`、item 背景和前景色。 |
| 信息密度 | 通过尺寸类型控制高度、字体、图标和内边距。 | `SizeType=Large/Middle/Small/Custom`。 |
| 排列方向 | 同一选择模型支持横向分段和纵向分段。 | `Orientation=Horizontal/Vertical`。 |
| 铺满能力 | 在块级场景中让可见 item 等分父容器宽度。 | `IsExpanding`。 |
| 形状变体 | 默认圆角遵循 SizeType，Round 使用稳定的胶囊几何。 | `Shape=Default/Round`。 |
| 图文组合 | 允许纯文本、纯图标或图标加文本。 | `SegmentedItem.Icon`、`Content`、`:has-icon`。 |
| 动效一致性 | 选中滑块移动和 item 背景变化受全局 motion 控制。 | `IsMotionEnabled`。 |

## 3. API 与契约模型

Segmented 的公共契约由根控件、item 容器、继承的选择 API 和主题契约组成。

根控件 API：

| API | 类型 | 语义 |
| --- | --- | --- |
| `SizeType` | `CustomizableSizeType` | 控制轨道圆角、item 高度、字体、padding 和图标尺寸，支持 `Large`、`Middle`、`Small`、`Custom`。 |
| `Orientation` | `Orientation` | item 排列方向，默认 `Horizontal`；`Vertical` 使用纵向轨道和纵向滑块位移。 |
| `Shape` | `SegmentedShape` | 轨道、item 和选中滑块的形状，默认 `Default`；`Round` 使用胶囊圆角。 |
| `IsExpanding` | `bool` | 是否适配父容器可用宽度；水平模式下可见 item 等分宽度，垂直模式下轨道和 item 填满可用宽度但不扩展高度。 |
| `IsMotionEnabled` | `bool` | 是否启用选中滑块位置/尺寸动画和 item 背景动画，默认来自 `SharedToken.EnableMotion`。 |
| `SelectedIndex` / `SelectedItem` | inherited | 当前选择，来自 `SelectingItemsControl`。 |
| `SelectionChanged` | inherited event | 选择变化事件，来自 `SelectingItemsControl`。 |
| `Items` / `ItemsSource` / `ItemTemplate` | inherited | 选项集合和内容模板入口。 |

item API：

| API | 类型 | 语义 |
| --- | --- | --- |
| `SegmentedItem.Icon` | `PathIcon?` | 选项左侧图标；非空时启用 `:has-icon`。 |
| `SegmentedItem.IsSelected` | `bool` | item 选中状态，接入 Avalonia `ISelectable` 选择模型。 |
| `Content` / `ContentTemplate` | inherited | item 内容和模板。 |

稳定 template part 和主题节点：

| 模板 | 节点 | 职责 |
| --- | --- | --- |
| `SegmentedTheme.axaml` | `Frame` | 根轨道裁剪、圆角和 padding 承载。 |
| `SegmentedTheme.axaml` | `PART_ItemsPresenter` | item presenter 和 `SegmentedStackPanel` 承载。 |
| `SegmentedItemTheme.axaml` | `Frame` | item 背景、圆角和 padding 承载。 |
| `SegmentedItemTheme.axaml` | `IconPresenter` | `SegmentedItem.Icon` 的视觉承载。 |
| `SegmentedItemTheme.axaml` | `Content` | item 内容承载。 |

稳定伪类：

- `:selected`：item 被选中。
- `:pressed`：item 按下态。
- `:has-icon`：item 设置了 `Icon`。
- `:pointerover` 和 `:disabled`：由 Avalonia 标准状态驱动，主题使用这些状态表达 hover 和 disabled 视觉。

`SegmentedShape` 包含 `Default` 和 `Round`。该枚举只表达 Segmented 家族的轨道形状，不复用包含 `Circle` 等无效值的其他控件 Shape 枚举。

`Segmented` 桌面层默认为非 visual 数据项创建 `SegmentedItem` 容器，并把 `SizeType`、`Shape`、`IsMotionEnabled` 传递给容器。`PrepareSegmentedItem(SegmentedItem, object?, int)` 是受保护扩展点，用于派生控件补充容器准备逻辑。

## 4. 行为与状态模型

Segmented 的核心状态流：

```text
Items / ItemsSource
      ↓
SegmentedItem containers
      ↓
ISelectable selected state
      ↓
SelectedItem / SelectedIndex
      ↓
selected thumb position + size
```

选择语义：

- 构造阶段固定 `SelectionMode=Single`。
- 模板应用时，如果没有显式 `SelectedIndex`、`SelectedItem` 或已选中容器，并且 `Items.Count > 0`，控件选择第一个 item。
- 已绑定或已显式设置的选择必须在模板应用后保留。
- 鼠标左键释放在 item 上触发选择，避免按下时立即改变选择造成的交互跳变。
- `SelectionChanged` 后如果控件仍附加在视觉树，会重新计算选中滑块的位置和尺寸。
- `Left` / `Up` 选择前一个有效 item，`Right` / `Down` 选择后一个有效 item，并在首尾之间循环。
- 键盘选择跳过 disabled 和 hidden item，选择完成后焦点进入对应容器。

尺寸语义：

- `Large`、`Middle`、`Small` 进入对应主题分支。
- `Custom` 复用 Middle 作为默认视觉基线；用户可在实例或 item 上显式设置 `Padding`、`MinHeight`、`FontSize`、`CornerRadius` 等 Avalonia 属性形成自定义尺寸。
- 根 `SizeType` 会同步给生成的 `SegmentedItem`，保证 item 的主题分支和根控件一致。

方向与展开布局语义：

| `Orientation` | `IsExpanding` | 布局语义 |
| --- | --- | --- |
| `Horizontal` | `false` | item 按自然宽度横向排列，根控件默认左对齐。 |
| `Horizontal` | `true` | 根控件填满父容器宽度，可见 item 等分可用宽度。 |
| `Vertical` | `false` | item 按自然高度纵向排列，轨道宽度取可见 item 的最大自然宽度。 |
| `Vertical` | `true` | 根轨道和 item 填满父容器宽度，item 仍按自然高度纵向排列，不填满父容器高度。 |

隐藏 item 不参与测量累计、排列、expanding 计数或键盘导航。

Form 语义：

- Segmented 实现 `IFormItemAware`。
- `SetFormValue(value)` 设置 `SelectedItem`。
- `GetFormValue()` 返回 `SelectedItem`。
- `ClearFormValue()` 清空 `SelectedItem`。
- `SelectedItem` 变化会通知 Form value changed。

## 5. 视觉与主题模型

Segmented 的视觉由根主题、item 主题、专属 Token 和 SharedToken 共同决定。

| 主题文件 | 职责 |
| --- | --- |
| `SegmentedTheme.axaml` | 根模板、轨道 padding/background、选中滑块资源、SizeType/Shape 圆角分支、方向/展开对齐和滑块动画。 |
| `SegmentedItemTheme.axaml` | item 模板、图标/内容布局、hover/pressed/selected/disabled 状态、SizeType/Shape 分支和图标尺寸。 |

视觉关系：

```text
SharedToken
   ↓
SegmentedToken
   ↓
SegmentedTheme / SegmentedItemTheme
   ↓
track + selected thumb + item states
```

根控件在 `Render()` 中绘制轨道背景和选中滑块。item 模板绘制每个选项自身的 hover/pressed 背景遮罩、图标、内容和状态颜色。选中背景由选中滑块独占承载：item 的 `:selected` 不绘制背景，选中滑块飞行途中目标项也不会提前点亮；滑块飞行期间非选中 item 的 hover/pressed 背景遮罩被屏蔽，对齐上游 `thumb ~ item::after` 透明规则。无选中项时滑块不绘制。选中滑块位置来自当前选中容器相对根控件的坐标，尺寸来自当前选中容器最终排列后的 `Bounds.Size`。

`Shape=Default` 时，根、item 和滑块圆角继续由 SizeType 对应的 SharedToken 决定。`Shape=Round` 时，Shape 分支在 SizeType 分支之后统一覆盖根、item 和滑块圆角为胶囊几何；该覆盖不新增 Design Token，也不改变模板结构。

## 6. 控件家族或集成关系

Segmented 的共享实现位于 `AtomUI.Controls`，桌面实现位于 `AtomUI.Desktop.Controls`。

集成关系：

- `AbstractSegmented`：共享单选、motion、form、布局和滑块渲染逻辑。
- `Segmented`：桌面公开控件，注册 `SegmentedToken.ScopeProvider`，创建 `SegmentedItem` 容器。
- `AbstractSegmentedItem`：共享 item 选择、图标伪类和 pointer release 选择逻辑。
- `SegmentedItem`：桌面公开 item，注册 `SegmentedToken.ScopeProvider`。
- `SegmentedStackPanel`：内部方向感知 items panel，负责横向/纵向排列、expanding 和可导航容器语义。
- Form：通过 `IFormItemAware` 把 `SelectedItem` 暴露为表单值。
- Gallery：通过 Segmented ShowCase 展示基础、块级、禁用、动态数据、尺寸、垂直布局、胶囊形状、纯图标和图标文本示例。

## 7. 兼容性不变量

维护 Segmented 时必须保持以下不变量：

- `SelectionMode` 保持单选。
- 未提供选择且存在 item 时，模板应用后默认选择第一个 item。
- 已绑定或显式设置的 `SelectedIndex` / `SelectedItem` 不能被默认选择覆盖。
- 鼠标左键释放触发 item 选择的语义不能擅自改为按下触发。
- `Segmented` 必须为非 visual item 创建 `SegmentedItem` 容器。
- `Orientation` 默认值必须保持 `Horizontal`，`Shape` 默认值必须保持 `Default`。
- 容器准备时必须把根 `SizeType`、`Shape` 和 `IsMotionEnabled` 同步给 item。
- 水平 `IsExpanding=true` 时只按可见 `SegmentedItem` 等分宽度；垂直模式不能因此扩展父容器高度。
- 选中滑块必须跟随当前选中容器的最终排列位置和 `Bounds.Size`。
- 四个方向键必须按前后顺序循环选择，并跳过 disabled 和 hidden item。
- `Frame`、`PART_ItemsPresenter`、`IconPresenter`、`Content` 等模板节点名称和职责不能在未授权情况下改变。
- `:selected`、`:pressed`、`:has-icon` 伪类语义不能改变。
- `SegmentedToken` 的名称、语义和资源使用点不能擅自删除或重命名。

如果实现某项能力时无法保持这些不变量，应先停止实现，说明原因、影响范围、替代方案和迁移方式，并获得授权。

## 8. 专项模型

### 8.1 选中滑块模型

选中滑块是根控件 render 层的视觉，不是单独的 visual child。它的 `SelectedThumbPos` 来自选中容器相对根控件的坐标，`SelectedThumbSize` 来自选中容器最终排列后的 `Bounds.Size`。

滑块矩形同步分两条路径：选择变化时（`SelectionChanged`）允许动画飞行，滑块位移与尺寸同时插值，时长取
SharedToken `MotionDurationSlow`（300ms），缓动为 `cubic-bezier(0.645, 0.045, 0.355, 1)`（对齐上游
motionEaseInOut）；布局完成后（`ArrangeOverride`）按最终 Bounds 校准，一律瞬移动画。滑块首次出现、无选中来源
容器、过渡系统尚未就绪或目标矩形无变化时也一律瞬移。无选中项（如 Form `ClearFormValue`）时滑块停止绘制。

item 的文字和图标颜色切换不受滑块飞行约束，立即生效并由 `MotionDurationMid` 柔化过渡。

该模型要求 item 布局完成后再同步滑块尺寸。修改布局、容器创建或选择时序时，必须验证滑块和 item 边界一致。

### 8.2 方向与 Expanding 布局模型

`SegmentedStackPanel` 是 Segmented 专用方向感知 items panel。水平模式沿 X 轴累计宽度，expanding 时按可见 item 数量等分可用宽度；垂直模式沿 Y 轴累计自然高度，并把每个 item 排列为轨道全宽。垂直 `IsExpanding` 只控制父容器宽度适配，不把可用高度分配给 item。

### 8.3 Shape 模型

`SegmentedShape.Default` 保留 SizeType 的圆角映射。`SegmentedShape.Round` 使用足够大的固定圆角形成几何胶囊，并在所有 SizeType 分支之后覆盖根、item 和滑块圆角。Round 是实例形状状态，不是 Token，也不要求新增 Visual、template part 或伪类。

### 8.4 Custom SizeType 模型

`SizeType=Custom` 是 `ICustomizableSizeTypeAware` 的自定义尺寸入口。默认主题下，Custom 使用 Middle 分支作为初始视觉基线；实例上的显式属性值覆盖主题 setter。控件不新增 Segmented 专属 `CustomHeight`、`CustomPadding` 等公开 API。

## 9. 文档导航与验证策略

关联文档：

- [Segmented 桌面版实现原理](implementation.md)
- [Segmented Semantic Part 契约](semantic-part.md)
- [Segmented Token 设计](token.md)
- [Segmented Changelog](changelog.md)

验证策略：

| 层次 | 验证内容 |
| --- | --- |
| Public API | `SizeType`、`Orientation`、`Shape`、`IsExpanding`、`IsMotionEnabled`、`SelectedIndex`、`SelectedItem`、`SelectionChanged`、`SegmentedItem.Icon`、`SegmentedItem.IsSelected`。 |
| 状态行为 | 默认选择、显式选择保留、绑定选择保留、pointer release 选择、四方向键循环选择、Form value、disabled item、hidden item。 |
| 布局与滑块 | 横向/纵向自然布局、水平 expanding、垂直宽度适配、动态方向切换、最终 Bounds 滑块矩形；滑块过渡时长/缓动契约（300ms + `cubic-bezier(0.645, 0.045, 0.355, 1)`）、飞行期间 item 背景遮罩抑制、无选中时滑块隐藏、detach 清理。 |
| AXAML / Template | 根 `Frame`、`PART_ItemsPresenter`、`SegmentedStackPanel`、item `Frame`、`IconPresenter`、`Content` 和 SizeType/Shape 样式分支。 |
| Semantic Part | descriptor 只含 `root`/`item`/`icon`/`label`；`semantic-item` marker 随容器创建/prepare 幂等就位，`semantic-icon`/`semantic-label` marker 位于 item 模板节点；集合重置、图文/纯图标/纯文本选项与选择变化不增删 marker；owner-scoped Semantic Style 命中最低 public 类型；选中滑块不属于任何 Part。 |
| Token | 轨道 padding/background、item 文本/背景状态色、item 最小高度、图标和图文间距。 |
| Gallery | Basic、Block、Disabled、Dynamic、Sizes、Vertical、Round Shape、Icon Only、With Icon 示例，以及 Token 语义和 ShowCase 示例。 |
| 文档 | 运行 `git diff --check`，检查相对链接存在。 |
