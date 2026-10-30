# Tag 桌面版实现原理

本文档描述 Tag 家族桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见 [Tag 桌面版架构设计](overview.md)，CheckableTag 选择与组合机制见 [CheckableTag 与 CheckableTagGroup 选择模型设计](checkable-tag-design.md)，变化记录见 [Tag Changelog](changelog.md)。涉及控件 Token 的实现应同时阅读 [Tag Token 设计](token.md)。

## 1. 实现定位

本文档覆盖 Tag、CheckableTag 和 CheckableTagGroup 的控件实现、主题接入、状态同步和 Gallery 可见维护边界。具体属性注册、默认值、绘制细节和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构和不变量。

## 2. 源码文件结构

主要源码文件：

- `src/AtomUI.Controls/Tag/AbstractTag.cs`
- `src/AtomUI.Controls/Tag/TagEnums.cs`
- `src/AtomUI.Controls/Tag/TagPseudoClass.cs`
- `src/AtomUI.Desktop.Controls/Tag/Tag.cs`
- `src/AtomUI.Desktop.Controls/Tag/TagToken.cs`
- `src/AtomUI.Desktop.Controls/Tag/Themes/TagTheme.axaml`
- `src/AtomUI.Desktop.Controls/Tag/Themes/TagTheme.cs`

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。

## 3. 核心类职责

- `AbstractTag`：跨平台或共享基类，承载公共 API、状态归一和模板生命周期。
- `Tag`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `TagTheme`：ControlTheme 类型入口，连接主题资源和控件类型。
- `TagToken`：控件 Token scope，负责从全局 token 派生控件语义变量。
- `AbstractCheckableTag` / `CheckableTag`：以 ToggleButton 作为输入状态 owner，增加 Icon、动效和 Form 投影。
- `AbstractCheckableTagGroup` / `CheckableTagGroup`：拥有 Options、模式、公开选择值、Default 初始化、事件和集合生命周期。
- internal checkable items control：持有 SelectionModel、生成 CheckableTag 容器并同步 IsChecked；其 SelectedItem(s) 不对外暴露。

核心协作规则：

- 控件实例是 public API 和运行时状态 owner。
- Template part 是视觉协作对象，生命周期必须受 `OnApplyTemplate` 或模板加载流程管理。
- 数据对象、选项对象、任务对象或节点对象只保存业务数据，不应反向持有不可释放的视觉对象。
- 弹层、窗口、计时器、异步 loader 和全局管理器必须有明确关闭、解绑或释放路径。

## 4. 状态与数据流

Tag 的状态流遵循下面路径：

```text
Public API / ItemsSource / Command / Event
  -> 控件实例状态
  -> internal state / effective state / pseudo-class
  -> template part property / AXAML selector
  -> renderer / popup / adorner / Gallery observable behavior
```

源码中的状态入口按以下语义维护：

- 内容与数据：`CloseIcon`、`Icon`、`Text`。
- 交互与状态：`IsClosable`、`Variant`。
- 视觉与布局：`TagColor`、颜色分类伪类和三种 Variant 视觉输出。

维护要求：

- 外部设置的 Avalonia 属性必须在模板应用前后保持一致。
- 集合、选择、展开、过滤、分页、上传任务或异步 loader 必须能处理 reset、replace 和 clear。
- 伪类和 internal state 必须从单一 owner 推导，避免双向同步导致循环更新。
- overview.md 的 API 契约说明应与源码实际状态流一致。

CheckableTag 与 Group 使用独立状态流：

```text
CheckableTag input
  -> ToggleButton.IsChecked
  -> checked pseudo-class / ControlTheme / Form

Options + IsMultiple + CheckedItem(s)
  -> Group normalized values
  -> internal option wrappers + SelectionModel
  -> CheckableTag.IsChecked
```

Group 是业务值 owner。internal wrapper、SelectionModel 和容器状态只能作为实现投影，不能反向成为第二套公开状态。

## 5. 生命周期与模板接入

生命周期规则：

- 构造阶段只注册必要状态，不依赖 template part。
- 模板应用时获取 part、建立事件订阅和绑定，并先释放旧 part 订阅。
- 控件卸载、弹层关闭、窗口关闭、集合替换或 container recycle 时释放事件订阅和资源宿主。
- DynamicResource、TokenResourceBinder 或 C# binding 必须有明确 owner 和释放点。
- Browser 和 Desktop 宿主下的主题加载顺序不得影响 public API 语义。

稳定 template part 接入点：

- `PART_CloseButton`：承载用户触发入口、导航或关闭动作。
- `PART_CheckableTagItems`：CheckableTagGroup 的内部选择与容器宿主，part 类型为 `SelectingItemsControl`；默认 internal 派生类型封装受保护的 SelectionModel、`SelectedItems`、`SelectionMode` 和 CheckableTag 容器映射。

Group 模板应用时先解除旧内部 host 的选择事件，再连接新 host 并根据公开值恢复选项、模板和选择状态。Group 不为普通 `ListBox` 提供选择 fallback。容器重新准备或回收时必须覆盖 Content、ContentTemplate、IsChecked 和 IsMotionEnabled，不能保留旧 option 状态。

### 5.1 Semantic Part 处置

Batch 2 Gate A 审计结论：Tag 家族以两个独立 owner 公开 Semantic Part，与上游稳定 Semantic DOM 对齐：

- `Tag` 公开 `root`、`icon`、`content`、`close`，对齐上游 `TagSemanticType`（`classNames` / `styles` 均为
  `{ root?, icon?, content?, close? }`）。上游证据：`root` 消费于 `.ant-tag` 根节点，`icon` 消费于图标节点，
  `content` 消费于文本 span（仅图标与文本并存时上游才包 span），`close` 消费于 `.ant-tag-close-icon`。
- `CheckableTagGroup` 公开 `root`、`item`，对齐上游 `CheckableTagGroupSemanticType`（`classNames` / `styles`
  均为 `{ root?, item? }`）。上游证据：`root` 消费于 `.ant-tag-checkable-group`，`item` 消费于每个
  `.ant-tag-checkable-group-item` 选项容器。
- 上游 `CheckableTag` 没有独立 Semantic DOM Props（不消费 `useMergeSemantic`），AtomUI 同样不为其声明
  descriptor，容器职责由 `item` Part 表达。

AtomUI 对应审计与映射：

- Tag 的 `icon`、`content`、`close` 是静态模板节点 Part：marker `.semantic-icon` / `.semantic-content` /
  `.semantic-close` 声明在 `TagTheme.axaml` 模板内的 `IconPresenter#IconPresenter`、`TextBlock#TagTextLabel`、
  `IconButton#PART_CloseButton` 节点上（TemplatedParent 均为 Tag owner）。SelectorRoute 均为单跳
  `/template/ .semantic-*`，`ContractType` 分别为公开 `IconPresenter`、`TextBlock`、`IconButton`，
  `Cardinality` 均为 `Single`，`RuntimeCreated=false`（生成器按 Tag 主题资产静态校验 marker 与节点类型）。
- Tag 节点可见性由数据驱动：`icon` 随 `Icon` 非空折叠、`close` 随 `IsClosable` 折叠，`content` 常驻；折叠不
  移除 marker，因此 descriptor 的 Cardinality 与主题静态校验不受数据状态影响。
- Tag 颜色算法（`TagColor` × `Variant` 的 Default/Preset/Status/Custom 计算与颜色伪类）是行为状态，不是
  Part；它以 `BindingPriority.Template` 写回 `Background` / `Foreground` / `BorderBrush`，低于用户 selector
  Style 的 `StyleTrigger` 与本地值优先级，因此用户 root 样式 setter 统一覆盖全部颜色分支，移除后状态机
  恢复——覆盖颜色是受支持的定制入口，颜色算法本身不发布为 Part。
- Group 的 `item` 是运行时容器 Part：Group 不是 ItemsControl，选项容器 `CheckableTag` 的逻辑父级是 internal
  的 `CheckableTagItemsControl`，因此 route 必须先 `/template/` 进入 Group 模板命中
  `.semantic-scope-items` 跳点（静态声明在 `CheckableTagGroupTheme.axaml` 的
  `CheckableTagItemsControl#PART_CheckableTagItems` 节点上），再 `>` 一步到达容器（`Descriptions` 同款
  `/template/ .semantic-scope-items > .semantic-scope-item` 链模式）。`.semantic-item` marker 在
  `CheckableTagItemsControl` 容器创建路径用生成常量一次性添加，prepare 幂等补齐；Part 声明
  `RuntimeCreated=true`、`ContractType` 为公开 `CheckableTag`、`Cardinality` 为 `Multiple`。
- 排除范围：`CheckableTag` 不持有独立 descriptor；`AbstractTag` / `AbstractCheckableTag` /
  `AbstractCheckableTagGroup` 基类与 internal `CheckableTagItemsControl` 不声明；`FocusVisual`、
  `WrapPanel`、`PART_ItemsPresenter`、颜色伪类与用户 `ItemTemplate` 子树均不发布为 Part；
  `.semantic-scope-items` 只是 route 跳点，不是 Part。

重新评估触发条件：上游为 `CheckableTag` 公开独立 Semantic DOM Props 时，重新评估为其声明 descriptor；
上游调整 Tag 或 Group 语义键时重新核对契约。

## 6. 交互与事件处理

Tag 的交互事件应从输入源收敛到控件级语义事件：

- Pointer、keyboard、focus 和 command 事件不应绕过 Avalonia 基础控件语义。
- 没有弹层职责的路径不应引入额外 popup 或全局输入捕获。
- 非集合控件不应通过隐藏集合状态模拟业务数据。
- 输入类路径必须保持 Form、validation、clear、placeholder 和键盘行为一致。

稳定事件路径包括 Tag 的 `Closed` 和 CheckableTagGroup 的 `CheckedChanged`。CheckableTag 直接复用 ToggleButton 的 `Click`、`Checked`、`Unchecked` 与 `IsChecked` TwoWay binding，不增加重复状态事件。

## 7. 内部算法与关键流程

维护者需要重点关注以下流程：

Tag 的颜色算法由单一视觉状态入口负责：

```text
TagColor + Variant + ThemeSnapshot
  -> ColorCategory
  -> ColorInfo / StatusInfo / CustomColor
  -> Foreground + Background + BorderBrush
  -> color pseudo-classes + template properties
```

颜色类别包括 `Default`、`Preset`、`Status` 和 `Custom`。`TagColor` 只改变颜色类别，`Variant` 只改变视觉形态。
颜色解析不得通过副作用写回 Variant 或边框状态。

预设颜色读取调色板序列的第 1、3、6、7 号色；状态颜色读取 Success、Info、Warning、Error 的语义 Token；
`info` 和 `processing` 共享 Info Token；`default` 使用 Tag 基础 Token。

自定义颜色按以下规则计算：Filled 使用亮度为 0.95 的 HSL 背景和原色文字，Solid 使用原色背景和浅色文字，
Outlined 使用亮度为 0.95 的 HSL 背景、原色边框和原色文字。

Default 颜色由 `TagTheme.axaml` 的 Variant selector 提供；Preset、Status 和 Custom 颜色由控件根据当前主题
计算。计算结果以 `BindingPriority.Template` 应用，并由 `CompositeDisposable` 管理旧值，保证颜色切换和模板
重套用不会叠加旧的属性值。用户本地 Brush 值优先于控件模板计算值。

所有 Variant 使用相同的边框厚度。Filled 和默认/自定义 Solid 通过透明 BorderBrush 表达；Preset/Status Solid
使用与背景相同的主色作为 BorderBrush，因此不会产生额外可见边界；所有 Variant 都不通过零厚度改变控件测量尺寸。

CheckableTagGroup 的选择同步遵循：

```text
external CheckedItem(s)
  -> value snapshot
  -> wrapper selection
  -> child IsChecked

child IsCheckedChanged
  -> internal selection
  -> new public value snapshot via SetCurrentValue
  -> CheckedChanged + Form ValueChanged
```

同步过程必须使用事务保护或暂时抑制内部回调，防止公开值、SelectionModel 和 IsChecked 形成循环更新。Options 与 CheckedItems 的集合替换和原地变化、Default 初始化、模式转换及失效值处理统一遵循选择模型专项文档，不能在 Theme 或容器类中再次解释。

实现文档不逐行解释私有方法。若某个私有算法成为稳定维护入口，应在本节补充算法不变量，而不是把代码复述为说明书。

## 8. 资源、性能与 AOT 边界

资源和 AOT 约束：

- 不通过运行时反射扫描 public API、Token 或 Gallery 示例数据。
- 不把可静态声明的模板结构迁移到 C# 动态创建。
- 异步加载、上传、弹层和窗口生命周期必须能取消或释放。
- 缓存对象必须与控件、窗口、弹层或数据 owner 生命周期一致。
- Source generator 生成文件不手工编辑；需要修改时改输入源或 generator。
- CheckableTagGroup 的 option 解析和属性同步使用静态类型、AvaloniaProperty 和强类型 getter，不使用 ReflectionBinding、字符串 path 或运行时类型扫描。

性能边界：

- 控件应优先复用 Avalonia 原生虚拟化、模板绑定和资源系统。
- 避免为每次状态变化创建不必要的视觉对象、订阅或动画对象。
- 大集合控件必须保证 container recycle 后不会泄漏旧 item 状态。
- Group 只在 Options、模式或公开选择值变化时执行 O(N) 归一和同步；单项交互不能通过遍历 VisualTree 查找业务值。
- 外部 CheckedItems 必须复制为内部快照，不能直接作为 internal SelectedItems 持有或原地修改。

## 9. 维护不变量

维护 Tag 时不得破坏：

- Public API、`Variant=Filled` 默认值、事件顺序和 Gallery 可观察行为。
- `TagColor × Variant` 视觉矩阵、颜色清除后的状态恢复和 Light/Dark 主题响应。
- Template part 名称、ControlTheme key、伪类和资源 key。
- `IsBordered`、`bordered` 和 `color="xxx-inverse"` 不得重新成为 Tag 的兼容入口。
- CheckableTag 不继承 TagColor、Variant、IsClosable、CloseIcon 或 Closed；其二态选择由 ToggleButton.IsChecked 唯一表达。
- CheckableTagGroup 默认可取消单选；CheckedItem(s) 始终表达 option Value，internal wrapper 和 SelectionModel 不得泄漏。
- PART_CheckableTagItems 重应用、Options/CheckedItems 替换和 detach 必须释放旧订阅，容器回收必须清除旧状态。
- Semantic Part 边界：`Tag` 只发布 `root` / `icon` / `content` / `close`，`CheckableTagGroup` 只发布
  `root` / `item`；Tag 三节点 marker 静态常驻、折叠节点 marker 不随数据状态增删；`item` 的 marker 在容器创建与
  prepare 路径一次性幂等建立、不随 checked 切换、Options 集合变化或单选/多选模式转换增删，默认主题不消费
  `.semantic-*` selector；颜色算法与 `FocusVisual` 不属于 Part。
- 旧 template part、事件订阅、Popup/Flyout/Window host 和 collection view 的释放路径。
- Light/Dark、Browser/Desktop 和不同 SizeType 下的主题一致性。
- 控件文档、源码 public surface、Token 类型或生成数据与源码契约的一致性。

## 10. 测试与验证

推荐验证：

- 纯文档改动运行 `git diff --check` 并检查相对链接。
- 控件 API 或行为变更运行对应 `tests/AtomUI.Desktop.Controls.Tests` 或专用包测试。
- Tag 行为测试覆盖 Default、Preset、Status、Custom 四类颜色与 Filled、Solid、Outlined 三种 Variant 的完整矩阵。
- Tag 行为测试覆盖 `info/processing` 映射、`default`、颜色清除、Variant 动态切换和本地 Brush 优先级。
- Tag 主题测试覆盖默认 Solid 文字对比度、Light/Dark 调色板更新、透明边框保持尺寸和 Icon/CloseIcon 颜色。
- CheckableTag 测试覆盖 pointer、keyboard、command、disabled、IsChecked TwoWay、Icon、Form 和二态 null 归一。
- CheckableTagGroup 测试覆盖 option 归一、可取消单选、多选顺序、Default、模式切换、集合通知、事件、Form、模板重应用、detach 和容器回收。
- Tag Semantic Part 测试：Tag 各形态（默认 / 图标 / 可关闭）marker 稳定、折叠节点 marker 保留、`IsClosable` 切换不增删 marker、Group item marker 随容器创建/prepare 就位、checked 切换与模式转换不增删 marker、owner-scoped Semantic Style 命中对应最低 public 类型（进入 Gate B 落地）。
- DataGrid 相关变更运行 `tests/AtomUI.Desktop.Controls.DataGrid.Tests`。
- Gallery 示例或源码片段变更运行 `tests/AtomUIGallery.Tests`。
- AOT、生成器或动态数据路径变更按 Gallery NativeAOT 发布流程验证。
