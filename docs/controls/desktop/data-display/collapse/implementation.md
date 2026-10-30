# Collapse 桌面版实现原理

本文档描述 Collapse 桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见 [Collapse 桌面版架构设计](overview.md)，Semantic Part 契约见 [Collapse Semantic Part 契约](semantic-part.md)，变化记录见 [Collapse Changelog](changelog.md)。涉及控件 Token 的实现应同时阅读 [Collapse Token 设计](token.md)。

## 1. 实现定位

本文档覆盖 Collapse 的控件实现、主题接入、状态同步和 Gallery 可见维护边界。具体属性注册、默认值、绘制细节和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构和不变量。

## 2. 源码文件结构

主要源码文件：

- `src/AtomUI.Desktop.Controls/Collapse/Collapse.cs`
- `src/AtomUI.Desktop.Controls/Collapse/Collapse.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/Collapse/CollapseItem.cs`
- `src/AtomUI.Desktop.Controls/Collapse/CollapseToken.cs`
- `src/AtomUI.Desktop.Controls/Collapse/ICollapseItemData.cs`
- `src/AtomUI.Core/Animations/Motion`：动效定义、请求身份、框架时钟、属性覆盖与可见性生命周期。
- `src/AtomUI.Controls/Primitives/ContentViewport.cs`：自然内容测量、实际视口、占位与方向锚定。
- `src/AtomUI.Desktop.Controls/Collapse/Themes/CollapseItemTheme.axaml`
- `src/AtomUI.Desktop.Controls/Collapse/Themes/CollapseTheme.axaml`

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。

## 3. 核心类职责

- `Collapse`：维护 items、Avalonia selection model、selection mode、输入路由、容器生成、模式切换归一和 item 位置投影。
- `CollapseItem`：承载单项 public 内容与 `IsSelected` 投影，处理 header 命中、展开按钮和 content motion 生命周期。
- `CollapseToken`：控件 Token scope，负责从全局 token 派生控件语义变量。

核心协作规则：

- Avalonia selection model 是展开状态的唯一 owner；`CollapseItem.IsSelected` 是容器投影，不存在内部 active-key 镜像。
- Template part 是视觉协作对象，生命周期必须受 `OnApplyTemplate` 或模板加载流程管理。
- 数据对象、选项对象、任务对象或节点对象只保存业务数据，不应反向持有不可释放的视觉对象。
- 弹层、窗口、计时器、异步 loader 和全局管理器必须有明确关闭、解绑或释放路径。

## 4. 状态与数据流

Collapse 的展开状态流遵循下面路径：

```text
Pointer / keyboard / CollapseItem.IsSelected / inherited selection API
  -> SelectingItemsControl selection model
  -> CollapseItem.IsSelected
  -> content visibility + arrow direction + motion target
```

源码中的状态入口按以下语义维护：

- 内容与数据：`AddOnContent`、`AddOnContentTemplate`、`ContentPadding`、`ExpandIcon`、`ExpandIconPosition`、`HeaderPadding`、`IsShowExpandIcon`、`ItemContentPadding`、`ItemHeaderPadding`。
- 选择与集合：`IsSelected`。
- 交互与状态：`IsAccordion`、`IsBorderless`、`IsGhostStyle`、`IsMotionEnabled`。
- 视觉与布局：`SizeType`。
- 其他稳定入口：`TriggerType`。

普通模式将 `SelectionMode` 设为 `Multiple | Toggle`。手风琴模式将其设为 `Single | Toggle`，利用原生 selection 操作完成旧项关闭、目标项打开和当前项收起。模式从普通切换到手风琴时，只保留视觉索引最小的已展开项。

禁止在 `SelectionChanged` 中遍历容器并回写 `IsSelected`，也禁止保存初始 active item 或单独的 active-key 集合。`ItemsSource` replace 会清除被替换索引的选择并让替换容器从收起状态开始，reset 和 clear 会清空 owner selection；这些结果由 selection model 与容器生成生命周期共同处理。

### 4.1 组合结构模型

```text
Collapse
└── PixelAlignedBorder#PART_Frame
    └── ItemsPresenter#PART_ItemsPresenter              ← .semantic-scope-items
        └── ItemsPanel（运行时 StackPanel）              ← .semantic-scope-panel（运行时）
            └── CollapseItem × N                         ← .semantic-scope-item（运行时）
                └── item shell border
                    └── DockPanel#PART_MainLayout
                        ├── PixelAlignedBorder#PART_HeaderDecorator   ← .semantic-header
                        │   ├── IconButton#PART_ExpandButton          ← .semantic-icon
                        │   ├── ContentPresenter#PART_HeaderPresenter ← .semantic-title
                        │   └── ContentPresenter#PART_AddOnContentPresenter
                        └── ContentViewport#PART_ContentViewport
                            └── PixelAlignedBorder#PART_ContentFrame  ← .semantic-body
                                └── ContentPresenter#PART_ContentPresenter
```

边框职责固定为：`PART_Frame` 绘制外框；item shell 绘制非末项底线；`PART_ContentFrame` 在默认 bordered 模式绘制内容顶线。Header 不承担 item 分隔线，动效状态不参与边框计算。root 表面投影把 owner 的 `Background`、`BorderBrush`、`BorderThickness`、`CornerRadius` 与 `Padding` 绑定到 `PART_Frame`；默认主题不设置根背景，root 背景由 Semantic Setter 或应用级样式提供。圆角按 antd collapse 样式规则分发：首项 header 承接容器上圆角、末项 header 与 content 承接下圆角（`HeaderCornerRadius`/`ContentCornerRadius` 由 owner 的 `CornerRadius` 与 item 位置计算），使 header/body 的背景沿容器圆角绘制而非盖住圆角边框。

### 4.2 Semantic Part marker 放置

marker 与 descriptor 声明、生成常量的对应关系：

| marker | 放置方式 | 位置 |
| --- | --- | --- |
| `.semantic-scope-items` | 静态 `Classes.semantic-scope-items="True"` | `CollapseTheme.axaml` 的 `ItemsPresenter#PART_ItemsPresenter`，items host 链的静态锚点标识 |
| `.semantic-scope-panel` | 运行时一次性 `Classes.Add` | `Collapse.DefaultPanel` FuncTemplate 创建的默认 ItemsPanel（`StackPanel`）；items host 链标识，不参与 Part 路由 |
| `.semantic-scope-item` | 运行时幂等 `Classes.Add` | 容器创建路径（`CreateContainerForItemOverride`）与 `PrepareContainerForItemOverride` 的每个 `CollapseItem` 容器；Part 路由必经步骤 |
| `.semantic-header` | 静态 `Classes.semantic-header="True"` | `CollapseItemTheme.axaml` 的 `PixelAlignedBorder#PART_HeaderDecorator` |
| `.semantic-icon` | 静态 `Classes.semantic-icon="True"` | `CollapseItemTheme.axaml` 的 `IconButton#PART_ExpandButton` |
| `.semantic-title` | 静态 `Classes.semantic-title="True"` | `CollapseItemTheme.axaml` 的 `ContentPresenter#PART_HeaderPresenter` |
| `.semantic-body` | 静态 `Classes.semantic-body="True"` | `CollapseItemTheme.axaml` 的 `PixelAlignedBorder#PART_ContentFrame` |

`header`、`icon`、`title`、`body` 的节点位于 `CollapseItem` 自己的模板内，而容器由 `SelectingItemsControl` 的容器生命周期
运行时创建（`TemplatedParent` 为 null），因此四个 Part 声明 `RuntimeCreated=true` 并携带显式 SelectorRoute。`/template/`
只能命中 `TemplatedParent` 非 null 的模板节点，而 Avalonia 的 `>` 步骤沿逻辑树（`LogicalParent`）行走：ItemsControl
生成的容器逻辑父级是 Collapse owner 本身（而非运行时 ItemsPanel），所以路由从 owner 出发经一步 `>` 直达
`.semantic-scope-item` 容器，再以 `/template/` 进入容器模板命中 Part 节点。`.semantic-scope-items` 与
`.semantic-scope-panel` 只标识 items host 链、不参与路由；scope marker 均不发布为 Part；容器复用/回收、items 集合变化
与模板重应用不增删 scope 或 Part marker。用户自定义 `ItemsPanel` 不影响 Part 路由（容器逻辑父级始终是 Collapse
owner）；建议自定义面板根节点声明 `Classes.semantic-scope-panel="True"` 以保持 items host 链标识一致。

## 5. 生命周期与模板接入

生命周期规则：

- 构造阶段只注册必要状态，不依赖 template part。
- 模板应用时获取 part、建立事件订阅和绑定，并先释放旧 part 订阅。
- 控件卸载、弹层关闭、窗口关闭、集合替换或 container recycle 时释放事件订阅和资源宿主。
- DynamicResource、TokenResourceBinder 或 C# binding 必须有明确 owner 和释放点。
- Browser 和 Desktop 宿主下的主题加载顺序不得影响 public API 语义。

稳定 template part 接入点：

- `PART_AddOnContentPresenter`：展示用户内容、文本、图标或模板化数据。
- `PART_ContentViewport`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_ContentPresenter`：展示用户内容、文本、图标或模板化数据。
- `PART_ExpandButton`：承载用户触发入口、导航或关闭动作。
- `PART_Frame`：承载根视觉、边框、背景或尺寸基线。
- `PART_HeaderDecorator`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_HeaderPresenter`：展示用户内容、文本、图标或模板化数据。
- `PART_ItemsPresenter`：展示用户内容、文本、图标或模板化数据。
- `PART_MainLayout`：稳定模板协作入口，重命名前必须同步主题和实现。

## 6. 交互与事件处理

Header 模式下，header 区域和展开图标触发 selection；Icon 模式下只有展开图标触发 selection。Enter/Space 和方向键导航复用 `SelectingItemsControl` 的 selection 入口。Disabled item 不进入 selection 操作。

输入处理器只识别目标容器和合法触发区域，不能直接修改其他 item 的 `IsSelected`。普通/手风琴差异由 selection mode 表达，不在 pointer、keyboard 或 button handler 中复制两套算法。

## 7. 内部算法与关键流程

### 7.1 选择与内容显示

关键流程：

1. `IsAccordion` 变化时先切换 selection mode，再通过 selection model 归一非法多选状态；容器准备期间若新容器携带显式选中值，则在基础 selection 投影后保留已有的较小已选索引。
2. 容器 prepare 和 index change 只投影 owner 属性、padding 和“是否末项”结构状态（含角半径分发），不根据 selection 重算边框。
3. `CollapseItem.IsSelected` 变化只改变内容目标可见性和箭头方向。
4. 新 content motion 开始前取消旧 motion；完成时仅在目标仍与最新 `IsSelected` 一致时应用稳定状态。
5. content 顶边是 content frame 的固定视觉，收起时随 content 一起被裁剪，不需要父控件等待动画完成。

### 7.2 共用内容展开执行链

主题将 `CollapseItem.IsSelected` 绑定到 `PART_ContentViewport` 的 `atom:Motion.IsVisible`，
声明 `ExpandMotion` 和 `CollapseMotion`，并绑定启用状态及有效时长。Core Motion 通过每次执行独立的非视觉进度源
接入 Avalonia 框架时钟，收放共用 `Spline(0.645, 0.045, 0.355, 1)`。
`ContentViewport` 实现公开的 `IMotionLayout` 能力，以布局租约控制占位和实际视口，
完整内容按自然尺寸排版，不缩放文字或图标；`CollapseItem` 不持有执行器或完成回调。
源码所有权与算法见 [内容展开与收起动效设计](../../../../architecture/systems/control-infrastructure/content-expansion.md)。

新请求在旧执行清理前捕获当前 Bounds 和有效透明度，连续替代旧动画。同步起点覆盖首个 tick 前的绘制窗口，
`FillMode.Both` 保持首尾帧；只有当前请求可以提交稳定显示状态。收起后隐藏视口并释放属性覆盖，恢复底层透明度来源；
item 的 selection 与图标状态继续由 `IsSelected` 决定。

### 7.3 接入与生命周期边界

- `Collapse` 通过 selection model 同步产生互斥目标，两项动画由同一动画时钟交换空间；完成回调不重新选择 item。
- `PART_ContentFrame` 内的 padding 与顶部边框包含在自然高度中，收起到零时随内容裁剪；item shell 底线保持结构所有权。
- 内容、字体、padding、宿主约束或嵌套内容尺寸改变通过布局失效更新自然测量；执行中的目标变化从当前帧接续并保持原截止时间。
- 关闭动效、初始模板接入、模板替换、detach / reattach 和容器回收按当前 selection 投影稳定布局。
- 动画执行器释放自己的动画、取消源、内部进度和布局接入；不清除用户或模板的 `Height`、`Width`、transform 或 transition。
- `ColorPickerCollapse` 通过继承消费相同路径，调色板内容与 `IsOpen` 到 `IsSelected` 的投影继续由派生接入层处理。

## 8. 资源、性能与 AOT 边界

资源和 AOT 约束：

- 不通过运行时反射扫描 public API、Token 或 Gallery 示例数据。
- 不把可静态声明的模板结构迁移到 C# 动态创建。
- 异步加载、上传、弹层和窗口生命周期必须能取消或释放。
- 缓存对象必须与控件、窗口、弹层或数据 owner 生命周期一致。
- Source generator 生成文件不手工编辑；需要修改时改输入源或 generator。

性能边界：

- 单项展开不得遍历全部容器重算边框。
- 模式切换只处理当前 selection indexes；item 位置只在集合结构或容器索引变化时更新。
- 边框、背景、padding 和 icon placement 使用 AXAML、selector 和 template binding 表达。
- 不新增运行时反射、字符串 binding、全局事件、timer、active-key 缓存或长期状态对象。
- template reapply 必须解绑旧展开按钮；detach 和新动效开始前必须取消旧 content motion。

## 9. 维护不变量

维护 Collapse 时不得破坏：

- Public API、默认值、事件顺序和 Gallery 可观察行为。
- Template part 名称、ControlTheme key、伪类和资源 key。
- Selection model 是唯一展开状态 owner，不能增加 active-key 镜像或 `SelectionChanged` 回写循环。
- 手风琴模式最多展开一项，并允许点击当前项后全部收起。
- 分隔线只由 item 位置、视觉模式和固定模板结构决定，不能依赖 selection 或 motion 时序。
- 旧 template part、事件订阅和 content motion cancellation 的释放路径。
- Light/Dark、Browser/Desktop 和不同 SizeType 下的主题一致性。
- Semantic Part 的 marker 放置（`CollapseTheme.axaml` 的 `semantic-scope-items` 静态节点、默认 ItemsPanel 的
  `.semantic-scope-panel`、容器创建/prepare 路径的 `.semantic-scope-item`、`CollapseItemTheme.axaml` 的四个静态 Part
  marker）属于维护不变量：状态切换、容器复用/回收、items
  集合变化与模板重应用不得增删 marker，默认主题不得消费 `.semantic-*` selector，root 表面投影不得丢失。
- 控件文档、源码 public surface、Token 类型或生成数据与源码契约的一致性。

## 10. 测试与验证

推荐验证：

- 纯文档改动运行 `git diff --check` 并检查相对链接。
- 状态测试覆盖普通多开、手风琴切换、点击当前项收起、初始多选归一、运行时模式切换和 items reset/replace/clear。
- 输入测试覆盖 Header、Icon、keyboard 和 disabled。
- 视觉测试覆盖默认、Borderless、Ghost 的首项、中间项、末项以及展开/收起/反向动画过程。
- 生命周期测试覆盖 template reapply、旧按钮解绑、detach 和 motion cancellation。
- Semantic Part 测试覆盖 descriptor 的五个 Part 数量、顺序与字段（`header`/`icon`/`title`/`body` 为 `Multiple`、
  `RuntimeCreated=true` 且携带显式 route）；N 个面板时四个 Part 各 N 个节点、空集合为 0、items reset/replace/clear 后
  数量跟随容器；`IsShowExpandIcon=False` 时 `semantic-icon` 节点隐藏但存在；状态与视觉模式切换不改变 marker 数量；root
  表面投影与生成 Style 对 `header`/`body`/`title`/`icon` 的局部 Setter 生效，见
  [semantic-part.md §7](semantic-part.md#7-兼容性与验证)。
- 运行 Collapse 定向测试、Gallery 测试和 `git diff --check`。

共享动效设计的验证要求：

- 在固定尺寸及原有滚动容器内，对等高和不等高内容执行手风琴切换，逐帧检查内容总占位及后续元素位置。
- 覆盖首个 tick 前、最终 tick 后且 continuation 前、播放中反转和连续切换多个面板，验证尺寸、透明度及旧回调隔离。
- 对多行文本、三层嵌套、Borderless / Ghost 和显式 padding 采样 1× / 2× 画面，检查文字局部坐标和结构分隔线。
- 验证关动效、detach / reattach、模板重套用及容器回收后的当前状态，并覆盖 ColorPicker 的直接消费路径。
- `CollapseBehaviorTests` 负责状态、输入、结构边线和生命周期；`ContentExpansionMotionTests` 与
  `ContentExpansionBoundaryTests` 负责共享机制的帧边界、几何和执行边界。不能以 transform 非空或终态可见性替代中间帧验证。
