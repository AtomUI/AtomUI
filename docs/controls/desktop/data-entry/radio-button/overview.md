# RadioButton 桌面版架构设计

本文档定义 `RadioButton`、`RadioButtonGroup`、`OptionButton` 和 `OptionButtonGroup` 桌面控件家族的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，内部实现原理见 [RadioButton 桌面版实现原理](implementation.md)，OptionButtonGroup 的方向布局见 [OptionButtonGroup 方向布局设计](option-button-group-orientation-design.md)，相关 Token 见 [RadioButton Token 设计](token.md)，设计和契约变化记录见 [RadioButton Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/DataEntry/RadioButton` |
| 控件状态 | Stable |

RadioButton 控件家族用于在互斥选项集合中选择一个值。`RadioButton` 提供标准单选指示器，`RadioButtonGroup` 管理普通选项组，`OptionButton` 将选项呈现为按钮，`OptionButtonGroup` 管理共享边框的按钮组选项。

RadioButton 不负责多选集合、开关语义或复杂导航菜单。这些职责应由业务层、组合控件或更专用的 AtomUI 控件承担。

主要源码入口：

- `src/AtomUI.Desktop.Controls/RadioButton`
- `src/AtomUI.Controls/RadioButton`
- `src/AtomUI.Desktop.Controls/OptionButtonGroup`
- `src/AtomUI.Controls/OptionButtonGroup`

## 2. 设计语言

RadioButton 的设计语言围绕控件职责、可观察状态和主题契约组织，而不是围绕模板节点组织。

| 维度 | 含义 | RadioButton 中的表达 |
| --- | --- | --- |
| 产品语义 | 控件在界面中承担的稳定职责。 | 普通单选指示器与按钮式单选组共享互斥选择语义。 |
| 内容承载 | 用户数据、展示内容、集合项或操作入口如何进入控件。 | `CheckedItem`、`SelectedItem`、`Items`、`ItemsSource`、`ItemTemplate`。 |
| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | checked/selected、disabled、pointer、motion、ButtonStyle 和方向组合状态。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | RadioButtonToken、OptionButtonToken 与对应 ControlTheme。 |

## 3. API 与契约模型

RadioButton 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。

核心 public surface 按语义分组维护：

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `Content`、`Icon`、`Items`、`ItemsSource`、`ItemTemplate` | 定义选项内容、图标、数据和模板入口。 |
| 选择与集合 | `IsChecked`、`CheckedItem`、`SelectedIndex`、`SelectedItem` | 维护普通单选组和按钮式单选组的当前值。 |
| 交互与状态 | `IsEnabled`、`IsMotionEnabled`、`IsWaveSpiritEnabled`、`ButtonStyle` | 表达可用性、动效和 Outline/Solid 状态视觉。 |
| 视觉与布局 | `Orientation`、`ItemSpacing`、`LineSpacing`、`SizeType`、`CornerRadius`、`BorderThickness` | 控制普通组排列、按钮组方向、尺寸和组合几何。 |

`RadioButtonGroup.CheckedChanged` 通知普通组当前项变化；`OptionButtonGroup.OptionCheckedChanged` 通知按钮组选项进入 checked 状态。两者不互相代理，继承的选择和输入事件继续遵循 Avalonia 事件语义。

主要公开类型与枚举：

- 类型：`AbstractRadioButton`、`AbstractRadioButtonGroup`、`RadioButton`、`RadioButtonGroup`、`RadioButtonGroupCheckedChangedEventArgs`、`RadioButtonOption`、`RadioIndicator`、`AbstractOptionButton`、`AbstractOptionButtonGroup`、`OptionButton`、`OptionButtonGroup`、`OptionButtonData`、`OptionCheckedChangedEventArgs`。
- 枚举：`OptionButtonStyle`、`OptionButtonPositionTrait`。`OptionButtonPositionTrait` 用于组合位置协作，不作为 Group 的方向配置入口。

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_ItemsPresenter` | `ItemsPresenter` | 承载 RadioButtonGroup 或 OptionButtonGroup 的集合容器。 |
| `PART_WaveSpirit` | `WaveSpiritDecorator` | 承载 RadioButton 或 OptionButton 的点击 Wave，使用有效圆角。 |

当前未抽取到控件专属伪类；主题主要依赖 Avalonia 标准伪类、模板绑定和内部 StyledProperty。

## 4. 行为与状态模型

RadioButton 的状态流按以下路径收敛：

```text
Public API / inherited command / item source / user input
  -> 控件实例状态
  -> effective state / pseudo-class / template property
  -> ControlTheme selector / presenter / renderer
  -> Gallery 可观察行为
```

状态维护规则：

- Disabled 或不可交互状态优先屏蔽 pointer、keyboard、motion 和提交类反馈。
- selection/checked/active、input/value、motion、visual option 状态由控件实例或明确的数据 owner 推导，不能在 template part 之间双向竞争。
- `RadioButtonGroup.CheckedItem` 是单选组的外部值 owner，默认 `BindingMode.TwoWay` 并启用 Avalonia data validation；用户选择和 ViewModel 更新必须收敛到同一份当前项状态。
- `OptionButtonGroup` 以 SelectingItemsControl 的选择状态作为按钮组选中 source of truth，用户 checked、键盘导航和外部 `SelectedIndex` / `SelectedItem` 必须收敛到同一选择。
- `OptionButtonGroup.Orientation` 是排列方向、组合圆角、分隔线方向和方向键导航的唯一 owner，默认值为 `Horizontal`。
- 模板重套用时必须把 public API 对应状态回放到新的 part、伪类和主题变量。
- 集合、弹层、异步、动效或窗口相关状态必须能处理 reset、close、cancel、detach 和 owner 释放。

## 5. 视觉与主题模型

RadioButton 的视觉模型由控件模板、ControlTheme、SharedToken 和必要的控件 Token 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `RadioButtonGroupTheme.axaml` | 定义局部操作入口、按钮或 handle 的状态视觉。 |
| `RadioButtonTheme.axaml` | 定义局部操作入口、按钮或 handle 的状态视觉。 |
| `RadioIndicatorTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `OptionButtonGroupTheme.axaml` | 定义按钮组 ItemsPresenter、方向布局入口、尺寸和组级边框资源。 |
| `OptionButtonTheme.axaml` | 定义按钮内容、Outline/Solid、checked/disabled、方向对齐和 Wave 视觉。 |

普通单选控件使用 `RadioButtonToken`，按钮式选项使用 `OptionButtonToken`。Token 只表达组件视觉语义，不承载 checked/selected、Orientation、GroupPositionTrait 或 EffectiveCornerRadius 运行时状态。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- Browser 或平台特化主题必须保持同一 API 的语义一致。

## 6. 控件家族或集成关系

RadioButton 与同分类控件共享尺寸、状态、Token、Gallery 展示和验证规则。组合或派生控件应显式说明哪些 API 被继承、覆盖或不支持。

主要协作类型：

- `AbstractRadioButton`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `AbstractRadioButtonGroup`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `RadioButton`：动作触发类型，负责点击、导航或局部操作状态。
- `RadioButtonGroup`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `RadioButtonGroupManager`：数据、状态或行为协作类型，维护集合同步和事件路径。
- `RadioButtonOption`：集合项、节点或容器类型，承载单项状态和模板协作。
- `RadioButtonToken`：控件 Token scope，负责从全局 token 派生控件语义变量。
- `RadioIndicator`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `AbstractOptionButtonGroup`：按钮组的方向、选择、容器位置和组级渲染 owner。
- `OptionButtonGroup`：桌面 public Group，创建 `OptionButton` 容器并应用桌面主题。
- `AbstractOptionButton`：单项 checked 状态、内容、图标、Wave 和有效圆角 owner。
- `OptionButton`：桌面 public 按钮式单选项。
- `OptionButtonToken`：按钮式选项的字体、Padding、背景、前景和状态颜色语义。

集成关系：

- 与 ThemeManager、SharedToken、ControlTheme、控件文档和 Gallery ShowCase 示例保持一致。
- 涉及 ItemsSource、Popup、Flyout、Window、Form 或 CompactSpace 的路径必须保持生命周期释放和数据状态同步。
- 源码目录中的共享基类和内部协作类型形成维护边界，不能只修改桌面包装类而忽略共享状态 owner。

## 7. 兼容性不变量

维护 RadioButton 时必须保持以下不变量：

- 不擅自新增、删除、重命名或改变 public/protected API、Avalonia 属性、事件和默认值。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级。
- `OptionButtonGroup.Orientation` 默认保持 `Horizontal`；纵向能力不得改变横向尺寸、圆角、边框和选择语义。
- Template part 重新应用、集合替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅和资源宿主。
- 不通过隐藏延迟、强制刷新或吞异常掩盖状态同步问题。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

## 8. 专项模型

### 8.1 选择与当前项模型

RadioButton 的当前项状态必须由单一 owner 推导。public 选择属性、集合项容器和伪类之间只能做单向同步，集合替换、清空和模板重套用时必须回放当前状态。

`RadioButtonGroup.CheckedItem` 作为 Form value 和受控单选值暴露给用户。控件从 `CheckedItem` 同步容器 `IsChecked`，也会在用户勾选容器时更新 `CheckedItem`，默认绑定模式负责把该值回写到 ViewModel。

### 8.2 动效模型

RadioButton 的动效只表达状态变化反馈，不应改变 public API 语义。初始加载、禁用态和卸载路径应能抑制或取消动效，避免保留旧控件实例。

### 8.3 视觉选项模型

RadioButton 的视觉选项通过 public API 归一为 theme variables、伪类或模板绑定。Token 保存组件语义值，不能保存实例运行时状态或业务色值。

### 8.4 OptionButtonGroup 方向模型

`OptionButtonGroup` 使用同一个选择和容器模型支持 Horizontal 与 Vertical。Group 将 Orientation 和 First/Middle/Last/OnlyOne 位置投影给 Item，标准 StackPanel 负责测量排列，Group renderer 负责外边框、共享分隔线和选中边框，Item 根据方向和位置计算 `EffectiveCornerRadius`。

纵向模式默认铺满可用宽度；非 Stretch Alignment 使用最宽 Item 的自然宽度；显式 Width 由调用方接管。完整尺寸矩阵、圆角映射、Custom 边界和验证要求见 [OptionButtonGroup 方向布局设计](option-button-group-orientation-design.md)。

## 9. 文档导航与验证策略

关联文档：

- [RadioButton 桌面版实现原理](implementation.md)
- [RadioButton Semantic Part 契约](semantic-part.md)
- [OptionButtonGroup 方向布局设计](option-button-group-orientation-design.md)
- [RadioButton Token 设计](token.md)
- [RadioButton Changelog](changelog.md)

`icon` 指示圆环语义与上游 Radio `icon` 语义对齐，由模板中的 `RadioIndicator` 节点承载并以 `Single` 基数公开；
`label` 对应上游 `label` 语义，由 `ContentPresenter` 节点承载。`RadioButtonGroup`、`RadioIndicator`、
`OptionButton` 与 `OptionButtonGroup` 不持有独立 Semantic descriptor。完整契约见
[RadioButton Semantic Part 契约](semantic-part.md)。

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档改动 | 运行 `git diff --check`，检查相对链接存在。 |
| Public API | 覆盖属性默认值、事件触发、命令和继承语义。 |
| 状态模型 | 覆盖 checked/selected、Orientation、ButtonStyle、SizeType、disabled、hover、pressed、focus 和 motion。 |
| AXAML/Theme | 检查 template part、伪类、资源 key、Light/Dark 主题和 Browser 主题。 |
| Token | 检查 TokenKind、AXAML token resource、Token 类型、生成数据和 token.md和文档同步。 |
| Gallery | 走查对应 ShowCase 示例和源码片段入口。 |
