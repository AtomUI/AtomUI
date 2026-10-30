# Expander 桌面版实现原理

本文档描述 Expander 桌面版的模板接入、展开状态流、触发区域、展开方向、动效取消与归一、自定义 padding 度量和维护边界。公共设计与 API 契约见 [Expander 桌面版架构设计](overview.md)，Semantic Part 契约见 [Expander Semantic Part 契约](semantic-part.md)，Token 语义见 [Expander Token 设计](token.md)，变化记录见 [Expander Changelog](changelog.md)。

## 1. 实现定位

Expander 的实现重点是在 Avalonia `Expander` 基础上稳定扩展 AtomUI 的视觉、触发区域和 motion 行为。实现文档覆盖 `Expander.cs`、`ExpanderPseudoClass.cs`、`ExpanderToken.cs` 和 `ExpanderTheme.axaml` 的职责边界。

本文档不重新解释 Avalonia `Expander` 的基础 Header/Content 机制。共享播放协议见 [Motion 系统](../../../../architecture/systems/motion/overview.md)。

## 2. 源码文件结构

主要源码：

- `src/AtomUI.Desktop.Controls/Expander/Expander.cs`：公共 API、内部 effective state、template part 接入、触发区域、默认图标、边框状态、动效状态机和自定义 padding 度量。
- `src/AtomUI.Desktop.Controls/Expander/ExpanderPseudoClass.cs`：方向、自定义 padding 和展开状态相关伪类常量。
- `src/AtomUI.Desktop.Controls/Expander/ExpanderToken.cs`：Expander 控件 Token。
- `src/AtomUI.Desktop.Controls/Expander/Expander.SemanticParts.cs`：五个 Semantic Part 的声明与生成式 descriptor 输入
  （本次改造新增，Gate B 交付）；`body` 在此声明 `Cardinality=Optional`。
- `src/AtomUI.Desktop.Controls/Expander/Themes/ExpanderTheme.axaml`：控件模板、SizeType 分支、方向分支、图标位置分支、触发分支、Borderless/Ghost 分支和 token 引用；四个静态 `.semantic-*` marker 在此声明。
- `tests/AtomUI.Desktop.Controls.Tests/Expander/ExpanderBehaviorTests.cs`：Expander 行为和布局回归测试。
- `tests/AtomUI.Desktop.Controls.Tests/Expander/ExpanderSemanticPartTests.cs`：Semantic Part descriptor、模板 marker 契约、
  生成 Style 命中与 `body` 呈现语义的回归测试（本次改造新增，Gate B 交付）。
- `src/AtomUI.Core/Animations/Motion`：可复用动效定义、请求身份、框架时钟、属性覆盖与可见性生命周期。
- `src/AtomUI.Controls/Primitives/ContentViewport.cs`：自然内容测量、实际视口、占位与四方向锚定。
- `tests/AtomUI.Desktop.Controls.Tests/Motion`：共用展开机制的帧级几何和边界回归。

## 3. 核心类职责

`Expander` 继承 `Avalonia.Controls.Expander`。它负责把 AtomUI 扩展属性转换为模板可消费的 internal state，并协调 Header 点击、图标点击、结构化 Content 分隔线和 Content motion。

`ExpanderPseudoClass` 是主题状态常量集合。它定义方向伪类和自定义 padding 伪类，供主题 selector 使用。

`ExpanderToken` 提供组件级默认间距、背景和圆角。它不保存实例展开状态、Header 内容、Content 内容或 motion 运行状态。

`ExpanderTheme.axaml` 是模板和视觉状态的唯一来源。稳定 template part 不应在 C# 中动态创建，也不应通过 C# 绑定替代可由 AXAML 表达的关系。

正文字符串转换由 `ExpanderTheme.axaml` 自己的资源字典持有 `StringToTextBlockConverter`。
模板通过同一词法作用域的 `StaticResource` 引用它，将字符串转换为垂直居中、水平拉伸且自动换行的 `TextBlock`；
非字符串内容原样传递，`ContentTemplate` 保持原有处理路径。该转换器不依赖其他控件主题或宿主同名资源，
按片段选择主题时也必须保留相同的内容呈现语义。

## 4. 状态与数据流

展开状态流：

```text
OnPointerPressed / PART_ExpandButton.Click
  → ToggleExpanded()
  → IsExpanded changed
  → UpdateContentVisibility(IsExpanded)
  → motion or stable state
  → PART_ContentViewport.IsVisible
```

边框状态流：

```text
BorderThickness / IsGhostStyle / IsBorderless
  → SetupEffectiveBorderThickness()
  → PART_Frame.BorderThickness

BorderThickness / IsGhostStyle / IsBorderless / ExpandDirection
  → ConfigureContentBorderThickness()
  → PART_ContentViewport / unnamed PixelAlignedBorder.BorderThickness
```

自定义 padding 状态流：

```text
HeaderPadding changed
  → UpdateEffectiveExpandButtonMargin()
  → :custom-header-padding
  → HeaderDecorator.Padding + ExpandButton.Margin + HeaderPresenter.Margin

ContentPadding changed
  → :custom-content-padding
  → ContentPresenter.Padding
```

默认图标状态流：

```text
ExpandIcon == null
  → Clear local/current template value
  → Set RightOutlined at Template priority
```

### 4.1 Semantic Part marker 放置

marker 与 descriptor 声明、生成常量的对应关系：

| marker | 放置方式 | 位置 |
| --- | --- | --- |
| `.semantic-header` | 静态 `Classes.semantic-header="True"` | `ExpanderTheme.axaml` 的 `PixelAlignedBorder#PART_HeaderDecorator` |
| `.semantic-icon` | 静态 `Classes.semantic-icon="True"` | `ExpanderTheme.axaml` 的 `IconButton#PART_ExpandButton` |
| `.semantic-title` | 静态 `Classes.semantic-title="True"` | `ExpanderTheme.axaml` 的 `ContentPresenter#PART_HeaderPresenter` |
| `.semantic-body` | 静态 `Classes.semantic-body="True"` | `ExpanderTheme.axaml` 的 `ContentPresenter#PART_ContentPresenter` |

四个 marker 全部位于 `ExpanderTheme.axaml` 的**唯一** `ControlTemplate` 内，节点由 AXAML 静态声明，`TemplatedParent` 是
Expander owner 本身。因此四个 Part 都声明 `RuntimeCreated=false`、`CrossVisualRoot=false`，且不携带显式 `SelectorRoute`；
生成器按静态根模板 Part 把 route 规范化为 `/template/ .<SelectorClass>`。`root` 由生成器隐式生成，模板不添加
`.semantic-root`。

`header` 位于 `LayoutTransformControl#PART_HeaderLayoutTransform` 内，`body` 位于
`ContentViewport#PART_ContentViewport` 内。Gate B 的 selector 命中测试确认：两者都不跨越第二层模板边界，
`TemplatedParent` 仍是 Expander owner，因此单一 `/template/` 路由已经足够，不需要为它们声明更宽的 logical descendant
路由，也不需要显式 `SelectorRoute`。

cardinality 不是四者一致：`header` / `icon` / `title` 是模板常驻节点，任何状态下都存在于视觉树，声明 `Single`；`body`
声明 `Optional`。原因是 `PART_ContentViewport` 折叠稳定态下 `IsVisible=false`，其内部 `ContentPresenter` 在首次呈现前
不挂接视觉子级（Gate B 实测：actor `visualChildren=0`、`logicalChildren=1`），`body` 节点从视觉树缺席；首次展开后节点
物化并保持存在，再次收起不会解除挂接。这一 0/1 差异来自 Avalonia `ContentPresenter` 的挂接时机，不是容器生命周期或
marker 注入，详见 [semantic-part.md §4](semantic-part.md#4-状态与数量语义)。

Expander 与 Collapse 在 marker 机制上的差异：

- 没有运行时创建的容器或 item，因此不存在 `.semantic-scope-*` 锚点、容器创建/prepare/clear/recycle 路径，也不需要
  `RuntimeCreated=true` 的 C# marker 注入点。
- 没有 Popup、Overlay 或独立宿主，因此不存在跨视觉根 Part。
- `Expander.cs` 在模板接入时只按 NameScope 查找 `PART_HeaderDecorator` 与 `PART_ExpandButton`
  三个协作节点，不创建视觉节点，也不添加 marker；marker 完全由静态 AXAML 提供。

`header` 的承载节点 `PART_HeaderDecorator` 在模板中声明于
`LayoutTransformControl#PART_HeaderLayoutTransform` 之内。该节点是在 Expander 模板内联声明的，其 `TemplatedParent` 仍是
Expander owner，因此路由保持单一 `/template/` 边界，不穿越 `LayoutTransformControl` 自身的 `ControlTemplate`。该结论必须
由 Gate B 的 selector 命中测试验证，不得以源码文本扫描替代。

### 4.2 圆角裁剪（Part 背景不溢出圆角）

`PART_Frame` 同时声明 `ClipToBounds="True"` 与 `ClipContentToCornerRadius="True"`。后者是**必需**的，不是可选优化：

- `PART_Frame` 自己绘制圆角（`CornerRadius` 来自 `ExpanderBorderRadius`），但 `PART_HeaderDecorator` 与
  `PART_ContentPresenter` 是它的子节点，且各自带背景。
- `ClipToBounds` 只做**矩形**裁剪，不做圆角裁剪。缺少 `ClipContentToCornerRadius` 时，子节点的不透明背景会平铺到方形
  边界，覆盖父节点画出的圆弧，两个上角（以及展开态的下角）视觉上变成方角。
- 默认主题 `HeaderBg = ColorFillAlter` 实测 alpha≈2%，缺陷几乎不可见；一旦通过 Semantic Part 或 `IsGhostStyle`
  （header 背景为 `ColorBgContainer`，完全不透明）设置不透明背景就会显形。
- 裁剪由 `PART_Frame` 统一施加于整棵子树，与 `IsExpanded`、`ExpandDirection`、`IsBorderless` / `IsGhostStyle` 无关，
  不增删任何 marker，也不改变 Part 的命中数量。

对照实现：`CollapseItem` 不走这条路径，而是由 `Collapse.ConfigureItemCorners` 按 first/last 给
`HeaderCornerRadius` / `ContentCornerRadius` 赋值并绑定到子节点，使子节点背景跟随容器圆角而不是覆盖它。Expander 是单面板
控件、没有 item 索引语义，因此选择在根节点统一裁剪，而不是复制 Collapse 的逐 corner 计算。

测试限制：headless 测试平台的几何包含性无法表示圆角图形，`DashedBorder.UpdateClip` 会按设计把裁剪降级为**不应用**
（见 `DashedBorderClipContentTests.Clip_Is_Not_Applied_When_The_Platform_Cannot_Hit_Test_The_Rounded_Figure`）。因此自动化
测试只能断言模板契约（`ClipContentToCornerRadius` 为 `true`、`CornerRadius` 非零）与几何前置条件（header 位于角落圆角
区域内），**实际裁剪效果必须在真实 Skia 后端做视觉验证**。

## 5. 生命周期与模板接入

模板接入：

1. 解除旧 `_expandButton.Click` 事件订阅。
2. 从 NameScope 获取 `PART_HeaderDecorator` 和 `PART_ExpandButton`，订阅新按钮。
3. 同步有效外框、Content 分隔线、图标 margin、默认图标和自定义 padding 伪类。
4. 主题把 `IsExpanded`、`IsMotionEnabled`、`MotionDuration` 投影到内容视口的 Motion 附加入口。

首次接入由可见性入口建立稳定状态。脱离视觉树时，Motion 取消旧请求并释放进度、属性覆盖和布局租约；
重新挂载时按最新模板绑定初始化。Expander 不持有内容执行器或内容视口，不维护动画 completion 回调。
模板重套用后旧视口的 continuation 不能修改新模板。

## 6. 交互与事件处理

Header 点击路径：

- `OnPointerPressed` 只处理鼠标左键。
- `CanToggleFromHeaderPointer` 检查 `TriggerType=Header`、鼠标状态和 `PART_HeaderDecorator` 命中。
- 命中后调用 `ToggleExpanded()` 并标记事件 handled。

图标点击路径：

- `PART_ExpandButton.Click` 直接调用 `ToggleExpanded()`。
- `TriggerType=Icon` 下 Header 点击不会触发展开，图标仍可触发。

禁用状态：

- 模板把 `IsEnabled` 传给 `PART_ExpandButton`。
- 主题通过 `:disabled` 同步 Header 和 ContentPresenter 前景。

Expander 不处理 keyboard、focus、drag/drop、popup 或 command 事件。

## 7. 内部算法与关键流程

### 7.1 展开方向转换

主题方向 selector 把 Avalonia `ExpandDirection` 映射为 `Motion.LayoutDirection`：

```text
Left  → Direction.Left
Up    → Direction.Top
Right → Direction.Right
Down  → Direction.Bottom
```

该映射只服务内容视口。Header dock、Header layout transform 和图标旋转也由 AXAML selector 表达。

### 7.2 Content motion 状态机

`PART_ContentViewport` 是实现 `IMotionLayout` 的 `ContentViewport`。主题声明 `ExpandMotion` 与 `CollapseMotion`，
并通过 `atom:Motion.IsVisible` 绑定 `IsExpanded`。单次播放使用非视觉进度源接入 Avalonia 框架时钟，
布局租约以实际 Bounds 捕获当前可见长度，内容保持自然尺寸排版，透明度与尺寸共享时间基准。

- 初始模板、重新挂载或动效关闭：按业务状态同步稳定布局。
- 新请求：按当前可见长度和透明度连续替代旧执行，不通过缩放文字表达展开。
- 正常展开完成：释放运行资源，终态覆盖保持最后一帧；正常收起完成：隐藏视口并释放覆盖。
- 反转、模板替换、方向变化或 detach：旧请求失效，不能覆盖当前可见性。

### 7.3 Motion 资源清理

Motion 拥有单次执行的取消源、原生时钟进度与属性覆盖，ContentViewport 拥有可释放的布局租约。
关闭 `IsMotionEnabled` 或改变 `ExpandDirection` 时，可见性入口取消旧执行，按最新 `IsExpanded` 收敛。
收起稳定态恢复底层透明度来源，隐藏由 `IsVisible=false` 表达，不向应用写入永久的 `Opacity=0`。

清理只释放共用机制拥有的值和资源，不清除用户或模板设置的 `Height`、`Width`、Min/Max 约束、transform 或
transition，也不修改内容自身的布局属性。

### 7.4 Content 结构化分隔线

`ConfigureContentBorderThickness` 取 `BorderThickness.Bottom` 作为 Header 与 Content 分隔线厚度。`IsGhostStyle` 或 `IsBorderless` 下，`SetupEffectiveBorderThickness` 和该方法分别把根边框与分隔线厚度归零，既有背景 selector 保持不变。

分隔线位于 `PART_ContentViewport` 内部的 unnamed `PixelAlignedBorder`，Header 不承担边框。方向映射：

- `Down`：Content 顶边。
- `Up`：Content 底边。
- `Left`：Content 右边。
- `Right`：Content 左边。

边框计算不读取 `IsExpanded`。收起时 motion actor 隐藏，分隔线随 Content 自然消失；展开、收起或快速反向 motion 不修改分隔线 Thickness 或 Brush。

### 7.5 自定义 HeaderPadding 下的图标间距

`UpdateEffectiveExpandButtonMargin` 只在 `HeaderPadding` 非空时计算有效图标 margin：

```text
ExpandIconPosition=Start → Thickness(0, 0, HeaderPadding.Left, 0)
ExpandIconPosition=End   → Thickness(HeaderPadding.Right, 0, 0, 0)
```

默认 HeaderPadding 为空时，`EffectiveExpandButtonMargin` 为默认值，主题继续使用 `LeftExpandButtonHMargin` / `RightExpandButtonHMargin` token。这样默认 SizeType 分支保持 token 驱动，自定义 padding 分支保持与用户显式 spacing 对齐。

### 7.6 共用内容展开接入契约

共用机制的设计所有者为 [内容展开与收起动效设计](../../../../architecture/systems/control-infrastructure/content-expansion.md)。
Expander 提供 `IsExpanded`、`ExpandDirection`、有效 `MotionDuration`、内容宿主及其正常展开约束；Core 内部机制
负责尺寸/透明度插值、自然测量、首尾帧及请求生命周期。执行链与释放边界分别见 7.2、7.3。

| 方向 | 动画尺寸 | Header 所在侧 | 内容锚定边 / 分隔线 |
| --- | --- | --- | --- |
| Down | Height | 顶部 | 顶边 |
| Up | Height | 底部 | 底边 |
| Left | Width | 右侧 | 右边 |
| Right | Width | 左侧 | 左边 |

Header dock、Header 旋转和展开图标继续由方向 selector 表达；内部内容视口按相同方向排列和裁剪。
完整内容测量包含未命名 border 及 `PART_ContentPresenter.Padding`，不包含 Header。横向内容采用正常展开布局的
有效排版宽度，不能把每一帧的视口 Width 当作新的 TextWrapping 宽度。

反转先捕获实际尺寸与透明度，再使旧请求失效并取消；原执行及其清理不能改变新请求的布局值。
运行中改变方向先释放旧轴的临时值，再按新方向和当前 `IsExpanded` 建立稳定布局。关动效、模板替换、detach 和
重新接入同样释放内部尺寸接管，保持模板原有尺寸值、绑定和内容边框的结构所有权。

内容、有效宿主约束、字体或嵌套 Expander 尺寸变化时，自然测量需要失效；动画视口自身变化不重新定义自然尺寸。
输入触发、继承的展开状态语义和主题映射由 Expander 持有，共享层不增加 selection 或手风琴规则。

## 8. 资源、性能与 AOT 边界

资源边界：

- `ExpanderToken` 通过 token generator 注册，主题通过 `ExpanderTokenResource` 消费。
- SharedToken 提供图标大小、动效开关、动效时长、边框厚度和基础颜色。
- 默认图标使用显式 `new RightOutlined()`，不做运行时图标扫描。

生命周期边界：

- `_expandButton.Click` 在模板重套用时先解除旧订阅，再订阅新 part。
- Motion 可见性 owner 随目标存在，单次运行的进度、取消源与任务在结束或取消后释放。
- 属性覆盖与布局租约只由其 owner 释放，旧视口不能清理新请求。

AOT 边界：

- 不新增字符串 path binding、运行时反射扫描或动态类型创建。
- 模板内固定关系优先使用 AXAML `TemplateBinding`、selector 和 TokenResource。
- C# 只处理 AXAML 无法表达的运行时状态流，例如 motion cancellation 和 HeaderPadding 派生 margin。

性能边界：

- Header/Content 默认视觉和未命名 Content 分隔线由静态 AXAML 提供，不在运行时动态构造模板视觉。
- 展开/收起只操作单个 `ContentViewport`，不遍历复杂子树。
- 展开、收起和 motion completion 不重新计算分隔线，也不创建边框 transition 或透明占位。
- 自定义 padding 的 margin 计算是常量时间，不进入渲染热路径。

## 9. 维护不变量

内部重构必须保持以下不变量：

- template reapply 时旧 `_expandButton.Click` 必须解除。
- detach 时必须取消 motion 并清理临时值。
- `TriggerType=Icon` 不能通过 Header 点击切换状态。
- 默认 `ExpandIcon` 为空时必须使用 `RightOutlined`，且不覆盖用户显式图标。
- `IsMotionEnabled=false` 立即释放内部进度和布局接入，保留用户或模板的尺寸与变换。
- 只有当前动画执行器的当前执行可以提交稳定状态或完成通知；旧 actor 的回调不能影响替换后的模板。
- Header/Content 分隔线只能由方向、边框厚度和视觉模式决定，不能依赖 `IsExpanded` 或 motion 时序。
- `ExpandDirection` 的 motion 方向、Header dock、Header transform 和图标旋转必须同步维护。
- 自定义 HeaderPadding 下的图标间距必须跟随 HeaderPadding 对应方向，不回退到默认 SizeType token。
- `:custom-header-padding` 和 `:custom-content-padding` 的伪类语义不能混用。
- Expander 不引入多面板或手风琴状态；这属于 Collapse 的职责。
- Semantic Part 的 marker 放置（`ExpanderTheme.axaml` 的 `semantic-header`、`semantic-icon`、`semantic-title`、
  `semantic-body` 四个静态 marker）属于维护不变量：除 `body` 的“首次呈现前缺席”这一呈现历史差异外，状态切换、方向切换、
  尺寸档切换、自定义 padding 伪类、模板重应用和 detach 都不得增删 marker，默认主题不得消费 `.semantic-*` selector，
  `header` 与 `body` 的单一 `/template/` 路由不得因模板结构调整而退化为宽泛 logical descendant。`body` 的
  `Optional` cardinality 与 `PART_ContentViewport` 的 `IsVisible` 语义绑定：不得把 `semantic-body` 移到 actor 自身
  （会失去 `Padding` / `Background` 的承载节点），也不得为规避 0/1 差异而改变 actor 的可见性模型。
- 圆角裁剪：`PART_Frame` 的 `ClipContentToCornerRadius="True"` 属于维护不变量，不得移除。移除后 header/body 的不透明
  背景会重新覆盖 `PART_Frame` 的圆角（默认外观因 `HeaderBg` alpha≈2% 而不易察觉，但任何不透明 Part 背景都会显形）。
  也不要改用「给子节点各自设圆角」之外的方式绕过——若将来改为 Collapse 式的逐 corner 绑定，必须同步更新 §4.2 与
  对应测试断言。

## 10. 测试与验证

验证范围：

- `ExpanderBehaviorTests.Custom_Header_Padding_Uses_Compact_Expand_Icon_Layout`：自定义 HeaderPadding 下展开图标间距和垂直居中。
- `ExpanderBehaviorTests.Content_Separator_Is_Owned_By_Content_And_Follows_Direction`：Header 分隔线归零、未命名 Content border owner 和四方向 thickness 映射。
- `ExpanderBehaviorTests.Content_Separator_Remains_Adjacent_To_Header_In_Every_Direction`：四方向下 Header 与 Content 的物理相邻关系。
- `ExpanderBehaviorTests.Ghost_And_Borderless_Mode_Update_Frame_And_Content_Separator_At_Runtime`：运行时切换默认、Borderless 和 Ghost 模式时根边框与 Content 分隔线同步归零或恢复。
- `ExpanderBehaviorTests.Icon_Trigger_Does_Not_Toggle_From_Header_Click`：Icon 触发模式下 Header 点击不切换。
- `ExpanderBehaviorTests.Content_Motion_Reversal_Does_Not_Change_Separator_And_Uses_Latest_Expanded_State`：快速反向 motion 使用最新展开状态，且分隔线保持不变。
- `ExpanderBehaviorTests.Template_Reapply_Clears_Old_Content_Motion_Actor_Values` / `Detach_Clears_Active_Content_Motion_Actor_Values`：模板重套用和 detach 的 cancellation 与临时值清理。
- `ExpanderBehaviorTests.Frame_Enables_Content_Clip_To_Corner_Radius`：`PART_Frame` 开启圆角子级裁剪，且 header 位于圆角方形区域内（证明该裁剪承担遮挡职责）；覆盖展开/收起、Ghost、Borderless 组合。
- `ExpanderBehaviorTests.Frame_Corner_Radius_Comes_From_The_Theme_And_Stays_Non_Degenerate`：`PART_Frame.CornerRadius` 来自 `ExpanderBorderRadius`（= `BorderRadiusLG`）且运行期可更新。
- Gallery Expander 示例：Basic、Size、Borderless、Ghost、Custom Padding、Direction、Nested、No Arrow、Icon Position、Trigger。
- Semantic Part：descriptor 的五个 Part 数量、顺序与字段（`header`/`icon`/`title` 为 `Single`、`body` 为 `Optional`、
  `RuntimeCreated=false`、`CrossVisualRoot=false`、无显式 `SelectorRoute`、`SelectorRoute` 由生成器规范化为
  `/template/ .<SelectorClass>`）；四个静态 marker 的存在与节点类型；生成的 `ExpanderHeaderStyle` / `ExpanderIconStyle` /
  `ExpanderTitleStyle` / `ExpanderBodyStyle` 各精确命中一个节点；`body` 从未展开命中 0、展开后命中 1 且
  `TemplatedParent` 为 owner、再次收起保持 1；方向、尺寸档、图标位置、触发模式、视觉模式、自定义 padding 与 disabled
  切换后 `header`/`icon`/`title` 命中数量不变；未声明用户 Semantic Style 时不增加 selector activator。
- 修改主题或动效时运行 Expander 定向测试；共享机制变化时增加该机制、NavMenu、Collapse 及必要直接消费者的定向验证。
- 文档改动运行 `git diff --check`，并检查相对链接存在。

共享契约的中间帧验证需要四方向分别覆盖首个 tick 前、最终 tick 后且 continuation 前、播放中反转、方向改变和
关闭动效。检查内容实际尺寸、文字行布局与局部坐标、标题相邻边及分隔线，不以 transform 非空或最终 `IsVisible`
作为连续动画的充分证据。

Gallery Direction 示例中的固定高度容器、换行文本和原有对齐约束必须保留；同时覆盖嵌套内容、自定义 padding、
Borderless / Ghost 及 1× / 2× 缩放。布局与截图验证不能替代原生宿主下的观感和交互走查。
