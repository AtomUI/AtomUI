# Tag 桌面版架构设计

本文档定义 `Tag`、`CheckableTag` 与 `CheckableTagGroup` 桌面版的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，内部实现原理见 [Tag 桌面版实现原理](implementation.md)，选择模型见 [CheckableTag 与 CheckableTagGroup 选择模型设计](checkable-tag-design.md)，Tag Token 的专项设计见 [Tag Token 设计](token.md)，设计和契约变化记录见 [Tag Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/DataDisplay/Tag` |
| 控件状态 | Stable |

Tag 家族覆盖展示标签与选择标签。`Tag` 用于展示状态、分类、可关闭标记和预设色语义；`CheckableTag` 用标签视觉表达二态选择；`CheckableTagGroup` 在有限选项集合上提供可取消单选和多选。普通 Tag 的颜色模型由 `TagColor` 和 `TagVariant` 两个正交维度组成，按照当前三种变体规范的 `Color × Variant` 语义生成最终视觉。

Tag 不负责复杂筛选器、按钮或徽标计数。这些职责应由业务层、组合控件或更专用的 AtomUI 控件承担。

主要源码入口：

- `src/AtomUI.Controls/Tag`
- `src/AtomUI.Desktop.Controls/Tag`

## 2. 设计语言

Tag 的设计语言围绕控件职责、可观察状态和主题契约组织，而不是围绕模板节点组织。

| 维度 | 含义 | Tag 中的表达 |
| --- | --- | --- |
| 产品语义 | 控件在界面中承担的稳定职责。 | Tag 表达展示语义，CheckableTag 表达二态选择，CheckableTagGroup 表达一组选项的单选或多选。 |
| 内容承载 | 用户数据、展示内容、集合项或操作入口如何进入控件。 | Tag 使用 `Text`/`Icon`；CheckableTag 使用 `Content`/`Icon`；Group 使用 `Options` 和 `ItemTemplate`。 |
| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | Tag 使用颜色分类和 `Variant`；CheckableTag 使用 `IsChecked`；Group 使用 `CheckedItem(s)`。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | Tag Token + ControlTheme。 |

## 3. API 与契约模型

Tag 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。

核心 public surface 按语义分组维护：

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `CloseIcon`、`Icon`、`Text` | 定义控件展示内容、输入数据、模板或业务对象入口。 |
| 交互与状态 | `IsClosable`、`Variant` | 表达关闭能力和 Filled/Solid/Outlined 视觉形态。 |
| 视觉与布局 | `TagColor` | 选择 Default、Preset、Status 或 Custom 颜色类别。 |

稳定事件包括 `Closed`。事件触发顺序属于兼容契约，不能因内部状态重排而改变。

主要公开类型与枚举：

- 类型：`AbstractTag`、`Tag`。
- 枚举：`TagStatus`、`TagVariant`。

`Variant` 的默认值为 `TagVariant.Filled`。Tag 不提供全局 Variant 配置，也不把 Variant 写入 `ThemeConfig`；应用需要统一样式时使用 Avalonia Style，控件实例的本地值优先于样式值。

选择标签的 public surface 按以下边界维护：

| 控件 | 主要契约 | 默认与状态 owner |
| --- | --- | --- |
| `CheckableTag` | `IsChecked`、`IsEnabled`、`Content`、`ContentTemplate`、`Command`、`Icon`、`IsMotionEnabled` | 复用 ToggleButton 二态状态；不继承普通 Tag 的颜色、Variant 或关闭 API。 |
| `CheckableTagGroup` | `Options`、`IsMultiple`、`CheckedItem`、`CheckedItems`、Default 值、`ItemTemplate`、布局属性和 `CheckedChanged` | 默认可取消单选；Group 是公开选择值的唯一 owner。 |
| `ICheckableTagOption` / `CheckableTagOption` | `Value`、`Content` | Value 非空且在同一 Group 中唯一。 |

Group 的 `CheckedItem` 和 `CheckedItems` 默认支持 TwoWay binding；前者只在单选模式生效，后者只在多选模式生效。完整模式转换、Default 初始化和事件契约见选择模型专项文档。

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_CloseButton` | `AbstractIconButton` | 承载用户触发入口、导航或关闭动作。 |

控件专属或内部伪类包括 `CustomColor=:custom-color`、`PresetColor=:preset-color`、`StatusColor=:status-color`、`TagPseudoClass.CustomColor`、`TagPseudoClass.PresetColor`、`TagPseudoClass.StatusColor`。这些伪类属于主题 selector 可观察契约，不能在未同步主题和 Gallery 的情况下重命名或删除。

## 4. 行为与状态模型

Tag 的状态流按以下路径收敛：

```text
TagColor + Variant + ThemeSnapshot
  -> ColorCategory(Default / Preset / Status / Custom)
  -> Foreground / Background / BorderBrush
  -> effective visual state / pseudo-class
  -> ControlTheme selector / presenter / renderer
  -> Gallery 可观察行为
```

状态维护规则：

- `TagColor` 只负责颜色类别，`Variant` 只负责视觉形态；颜色解析不得反向修改 Variant。
- `default`、`null`、空值和无效颜色必须恢复 Default 颜色状态，不能残留之前的 Brush 或伪类。
- `info` 和 `processing` 使用同一组 Info 语义 Token；`default` 使用 Tag 基础 Token。
- 所有 Variant 保持相同边框厚度，Filled 和部分 Solid 通过透明边框表达无可见边界。
- 模板重套用时必须把 public API 对应状态回放到新的 part、伪类和主题变量。

CheckableTag 家族的状态流保持独立：

```text
CheckableTag.IsChecked
  -> ToggleButton input state
  -> checked pseudo-class / ControlTheme

Options + IsMultiple + CheckedItem(s)
  -> Group value normalization
  -> internal SelectionModel
  -> CheckableTag.IsChecked
```

Group 的内部 SelectedItem(s) 只保存归一后的 option wrapper，不是 public state；子项交互必须先回到 Group，再由 Group 通过 `SetCurrentValue` 更新公开值。

## 5. 视觉与主题模型

Tag 的视觉模型由控件模板、ControlTheme、SharedToken 和必要的控件 Token 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `TagTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `CheckableTagTheme.axaml` | 提供二态标签的内容结构以及 checked、focus、disabled 等状态视觉。 |
| `CheckableTagGroupTheme.axaml` | 组合内部选择控件、ItemsPresenter 和 WrapPanel。 |

Tag 使用 `TagToken` 作为控件 Token scope。Token 只表达组件视觉语义，不承载 input/value、visual option 运行时状态。

Tag 的视觉组合由以下矩阵表达：

| 颜色类别 | Filled | Solid | Outlined |
| --- | --- | --- | --- |
| Default | 浅背景、透明边框、默认文字 | `ColorBgSolid` 背景、对比文字 | 默认背景、默认边框、默认文字 |
| Preset | palette 1 背景、palette 7 文字 | palette 6 背景、浅色文字、palette 6 边框 | palette 1 背景、palette 3 边框、palette 7 文字 |
| Status | 状态浅背景、状态主色文字 | 状态主色背景、浅色文字、状态主色边框 | 状态浅背景、状态边框、状态主色文字 |
| Custom | HSL 亮度 0.95 背景、原色文字 | 原色背景、浅色文字 | HSL 亮度 0.95 背景、原色边框和文字 |

主题维护规则：

- `TagVariant`、`Variant=Filled` 默认值、`TagColor` 颜色分类、ControlTheme key、template part 和伪类是稳定契约。
- `IsBordered` 不属于当前契约，不得重新引入；旧 `bordered` 和 `color="xxx-inverse"` 兼容入口不属于当前设计。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 不在控件中增加全局 Tag 配置或 ThemeConfig 组件配置；全局默认样式由 Avalonia Style 承担。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- Browser 或平台特化主题必须保持同一 API 的语义一致。

## 6. 控件家族或集成关系

Tag 与同分类控件共享尺寸、状态、Token、Gallery 展示和验证规则。组合或派生控件应显式说明哪些 API 被继承、覆盖或不支持。

主要协作类型：

- `AbstractTag`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `Tag`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `TagTheme`：ControlTheme 类型入口，连接主题资源和控件类型。
- `TagToken`：控件 Token scope，负责从全局 token 派生控件语义变量。
- `AbstractCheckableTag` / `CheckableTag`：复用 ToggleButton 输入语义并提供标签选择视觉。
- `AbstractCheckableTagGroup` / `CheckableTagGroup`：拥有 Options、模式、公开选择值、Form 和集合生命周期。
- internal checkable items control：拥有 SelectionModel、容器生成和 IsChecked 投影，不进入 public surface。

集成关系：

- 与 ThemeManager、SharedToken、ControlTheme、控件文档和 Gallery ShowCase 示例保持一致。
- 涉及 ItemsSource、Popup、Flyout、Window、Form 或 CompactSpace 的路径必须保持生命周期释放和数据状态同步。
- 源码目录中的共享基类和内部协作类型形成维护边界，不能只修改桌面包装类而忽略共享状态 owner。

## 7. 兼容性不变量

维护 Tag 时必须保持以下不变量：

- 不擅自新增、删除、重命名或改变 public/protected API、Avalonia 属性、事件和默认值。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级。
- Template part 重新应用、集合替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅和资源宿主。
- 不通过隐藏延迟、强制刷新或吞异常掩盖状态同步问题。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- CheckableTagGroup 的公开值始终是 option Value，不能泄漏 internal wrapper、容器或内部 SelectedItem(s)。
- Options/CheckedItems 集合替换、模板重应用和 detach 必须释放旧订阅，容器回收不能残留旧 IsChecked。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

## 8. 专项模型

### 8.1 颜色与 Variant 模型

Tag 的视觉选项通过 `TagColor` 和 `Variant` 归一为颜色类别、Brush、伪类或模板绑定。预设颜色从当前 `ThemeSnapshot.PresetColorPalettes` 读取，状态颜色从 SharedToken 读取，自定义颜色使用 HSL 亮背景算法。Token 保存组件语义值，不能保存实例运行时状态或业务色值，也不能展开完整的颜色组合矩阵。

### 8.2 CheckableTag 选择模型

CheckableTag 使用 ToggleButton 的二态状态；CheckableTagGroup 将 `Options`、`IsMultiple` 和 `CheckedItem(s)` 归一为内部 SelectionModel，并把选择状态单向投影到子项。完整 Public API、模式矩阵、Template、同步算法、Form、生命周期与 AOT 边界见 [CheckableTag 与 CheckableTagGroup 选择模型设计](checkable-tag-design.md)。

## 9. 文档导航与验证策略

关联文档：

- [Tag Semantic Part 契约](semantic-part.md)
- [Tag 桌面版实现原理](implementation.md)
- [CheckableTag 与 CheckableTagGroup 选择模型设计](checkable-tag-design.md)
- [Tag Token 设计](token.md)
- [Tag Changelog](changelog.md)

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档改动 | 运行 `git diff --check`，检查相对链接存在。 |
| Public API | 覆盖属性默认值、事件触发、命令和继承语义。 |
| 状态模型 | 覆盖 `TagColor × Variant`、CheckableTag 二态、Group 单选/多选、Default、集合变化和主题切换。 |
| AXAML/Theme | 检查 template part、伪类、资源 key、Light/Dark 主题和 Browser 主题。 |
| Semantic Part | `Tag` descriptor 只含 `root`/`icon`/`content`/`close`，`CheckableTagGroup` descriptor 只含 `root`/`item`；Tag 三节点 marker 静态常驻、折叠节点 marker 保留；Group item marker 随容器创建/prepare 幂等就位，选择变化与模式转换不增删 marker；owner-scoped Semantic Style 命中最低 public 类型。 |
| Token | 检查 TokenKind、AXAML token resource、Token 类型、生成数据和 token.md和文档同步。 |
| Gallery | 走查对应 ShowCase 示例和源码片段入口。 |
