# Semantic Part Gallery Preview 设计

本文是 Semantic Part Preview 页面模型、目标解析、Adorner 生命周期、性能预算和验证要求的正式所有者。其他架构、
模块和 ShowCase 文档只保留职责摘要与本文链接，不复制完整算法。

Semantic Part Preview 是 Gallery 中面向控件公共视觉契约的交互式检查工具。它读取已经注册的
`ControlSemanticDescriptor`，在独立 Tab 中展示 Part 元数据、真实控件预览和用户侧 AXAML 用法，并在用户主动选择
某个 Part 时高亮当前已实例化的目标节点。

Preview 只属于 `AtomUI.Toolkits.GalleryBase` 和具体产品 Gallery。它不能向 AtomUI Control、ControlTheme、Semantic Part
descriptor 或普通应用运行时注入演示状态。

## 1. 设计目标

- 让 Gallery 用户在真实控件上理解 `root`、`.semantic-*`、`StyleType`、`ContractType`、cardinality 和引入版本。
- 使用现有 `SemanticPartRegistry` 和模板 marker，不重复维护结构元数据。
- 把 Semantic Part 与普通 Examples 分成独立一级 Tab，避免把公共定制契约伪装成普通使用案例。
- 只有用户第一次进入 Semantic Parts Tab 时才创建 Preview 和演示控件。
- 只有用户 Hover 或 Pin 某个 Part 时才查找目标并创建高亮视觉。
- 使用 Avalonia 12 原生 `AdornerLayer` 跟踪布局、Transform 和滚动；高亮覆盖层独立于目标内容的 ancestor clipping。
- 保持 NativeAOT、Browser 和 Desktop 路径不依赖反射、程序集扫描或运行时 AXAML 解析。

## 2. 非目标

Semantic Part Preview 不负责：

- 给 Control 增加 `SemanticClasses`、`SemanticStyles`、调试属性、事件或接口。
- 向目标控件或模板节点注入临时 class、Style、Binding 或 attached behavior。
- 替代 Semantic Part descriptor、Control 文档或应用侧 Selector 用法。
- 扫描 Application、程序集、所有 Window、所有 TopLevel 或任意第三方 AXAML。
- 在 Preview 未激活时缓存 VisualTree 节点、监听布局或跟踪 Popup。
- 恢复 Gallery 的 API 或 Design Token sidecar Tab。

## 3. 依赖与所有权

依赖方向固定为：

```text
具体产品 Gallery
        -> AtomUI.Toolkits.GalleryBase
        -> AtomUI.Desktop.Controls
```

- `AtomUI.Core` 和 Control 包拥有 descriptor、registry 与 `.semantic-*` marker。
- `AtomUI.Toolkits.GalleryBase` 拥有 ShowCase Tab 宿主、目标解析、高亮会话、Adorner 和代码片段构建。
- 具体产品 Gallery 拥有真实演示控件、本地化职责描述和页面接入。
- Control 包不得引用 GalleryBase；GalleryBase 不得把 Preview 状态回写到 Control。

因此普通应用未引用 GalleryBase 时，Semantic Part Preview 不进入其依赖闭包、IL、对象生命周期或运行时初始化链路。

## 4. ShowCase 页面模型

采用 Semantic Part Preview 的标准页面使用以下结构：

```text
GalleryShowCaseHost
  GalleryStickyTabsHost
    Header
      GalleryShowCaseHeader
    StickyContent
      TabStrip
        Examples
        Semantic Parts
    Content
      当前 Tab 内容
```

`GalleryShowCaseHost` 是 GalleryBase 的标准宿主，负责：

- 默认选择 Examples。
- 只有提供 `SemanticPartsContentTemplate` 时才显示 Semantic Parts Tab。
- 延迟创建 Semantic Parts 内容根；内容根可以是单个 `SemanticPartPreview`，也可以是包含多个 Preview 的普通 Control。
- 缓存当前页面生命周期内已经创建的 Tab 内容。
- 枚举该内容根中已经构造的全部 `SemanticPartPreview`，在 Tab 切换和页面 detach 时统一激活、停用和释放。
- detach 时释放 Preview 缓存和宿主持有的 Tab 导航 Visual；reattach 时按当前选择状态重建，禁止已脱离页面通过
  缓存控件继续保留旧合成资源。
- 复用 `GalleryStickyTabsHost` 的滚动上下文和 sticky elevation（吸顶时真实标签条宿主被提升进受控 adorner 层）。

页面不得为 Semantic Part 手写 `ScenarioTabs`、`ScenarioContentHost`、私有 lazy controller 或 Tab 切换 code-behind。
API、Design Token 和 Semantic Parts 不能共用旧 sidecar 场景模型。

未提供 Semantic Parts 内容的页面继续直接展示 Examples，不创建空 TabStrip 或空 sticky 行。

## 5. 真延迟创建

Semantic Parts 必须使用 `IDataTemplate` 或等价显式 factory 保存创建描述。模板不能直接放入 `TabItem.Content`，也不能预先
赋给隐藏 `ContentPresenter`，以免 Measure、模板应用或绑定初始化提前构建内容。

首次进入页面时只允许创建：

- `GalleryShowCaseHost`。
- `GalleryShowCaseHeader`。
- Examples 内容。
- 两个轻量 Tab header；没有 Semantic 内容时只保留 Examples 页面结构。

在用户第一次选择 Semantic Parts 前，以下对象和操作必须不存在：

- `SemanticPartPreview`。
- 用于检查的真实 Control。
- `SemanticPartRegistry` 查询。
- Part presentation item。
- VisualTree 枚举。
- Adorner 和高亮画笔。
- Popup、布局、目标 detach 或主题事件订阅。
- 用户侧 AXAML 代码片段生成。

首次选择 Semantic Parts 后，宿主显式构建模板并缓存结果。切回 Examples 时必须立即停用高亮会话并把 Semantic 内容从
VisualTree 移除；当前页面仍附加时保留已创建对象，避免重复构建。页面 detach 时释放缓存内容及其订阅；如果同一页面在
Semantic Parts 仍选中时重新 attach，宿主重新构建 Preview 和轻量 Tab 导航，不复用已经释放的 Visual 引用。首次 attach
应复用属性初始化阶段已经创建的轻量导航，不能无条件重复构建。

一个页面包含多个公开 Semantic owner 类型时，`SemanticPartsContentTemplate` 使用一个普通布局 Control 作为延迟内容根，并为
每个 owner 类型创建独立 `SemanticPartPreview`。宿主只负责统一生命周期，不合并 descriptor、Part 列表、owner type 或目标作用域。
同一 owner 类型在单个 Preview 内容中出现的多个实例不拆分为多个 Preview，而由该 Preview 的多实例 owner 作用域统一高亮（见
§8.4）。内容根必须在 factory 返回时已经通过普通 visual children 包含至少一个 Preview；不得把 Preview 隐藏在尚未应用的
ControlTemplate 中，再依赖宿主主动应用模板或扫描任意逻辑树。

内容根不得是 `ContentPresenter` 家族（`ContentPresenter` / `ScrollContentPresenter`）：模板刚 Build 出来尚未附加到视觉树时，
呈现器还没有把 `Content` 实现成子节点（`Child` 为 `null`、逻辑子元素为空），视觉树与逻辑树都搜不到 Preview，宿主会判定为
空模板并抛 `InvalidOperationException`。需要滚动时由宿主自身的 `ScrollViewer` 承担；遮罩类弹层控件需要局部层宿主时
（Drawer 先例），`ScrollContentPresenter` 放在 `PreviewContent` 的舞台内部，而不是模板根。

## 6. Preview 模型

`SemanticPartPreview` 是 GalleryBase 控件。它承载一个真实 Preview 内容，并解析一个明确的 owner descriptor 与一组 owner 实例：

- 一个 Preview 永远只对应一个 owner descriptor（一个 owner type）。控件家族拥有多个 public owner 时必须使用多个
  Preview，不能把不同 owner 的 Part 拼接成一个虚拟 descriptor。
- `SemanticOwnerType` 是 descriptor 的 owner type 契约；未显式指定时按 `SemanticOwner` 或 `PreviewContent` 的精确 CLR
  type 推导。owner type 按精确 CLR type 查询 `IThemeManager.SemanticParts`；派生示例需要复用基类契约时必须显式指定
  owner type，不进行程序集或类型层次反射发现。
- `SemanticOwner` 是 Gallery 显式提供的 owner 实例锚点，用于在 `PreviewContent` 本身不是 owner（外层是布局或操作区）时
  确定 owner type 与 Preview 的归属关系。它是 descriptor 解析与类型校验的入口，不把高亮收敛到单一实例。
- 高亮作用域是 Preview 内容范围内的全部 owner 实例，而不是单个 `SemanticOwner`。`PreviewContent` 子树中存在多个满足
  owner type 的实例时，选中某个 Part 后所有实例对应的 Part 同时高亮；单实例内容自然退化为当前行为。

Preview 的 `owner descriptor` 与 `owner 实例作用域` 是两个独立概念：descriptor 决定 Part 元数据与 StyleType，实例作用域
决定一次选择要解析和高亮多少个真实 Control。前者永远单一，后者可以是集合。这套多实例能力是 Preview 的通用机制，不要求
具体 Control 增加任何属性、marker 或模板改动，也不要求 Gallery 为每个实例复制 Preview。

Preview presentation item 的结构字段全部来自 `SemanticPartDescriptor`：

```text
Name
Path
SelectorClass
SelectorRoute
StyleType
ContractType
Cardinality
Customization
CrossVisualRoot
Since
RuntimeCreated
RestHidden
```

具体产品 Gallery 只能按 Part path 提供本地化职责描述和必要的代码片段覆盖，不能覆盖 selector、ContractType、cardinality
或其他结构字段。Preview 必须验证描述 key 不包含 descriptor 中不存在的 Part；缺少描述时使用结构化 fallback 文案，不能
阻止 Preview 工作。

具体产品 Gallery 的标题、描述、演示正文和代码片段必须使用 AtomUI 自身的公共契约与术语。参考场景可以用于校准布局、
状态组合和定制意图，但不得把其他平台的 API 名称或机制描述直接带入 AtomUI，例如 `Semantic DOM`、`classNames`、`styles`
或对象函数式样式入口。Semantic Part 示例统一使用 `Semantic Part`、owner-scoped selector、descriptor 中发布的 Part path，
以及控件实际支持的 AXAML Style/Theme API；本地化资源中的 source 与各语言 target 必须同步表达同一机制。

### 6.1 展示结构

Preview 使用一个完整边框包围预览区和 Part 列表，不能把两栏拆成互不关联的卡片：

- 宽屏左侧是白色真实控件预览区，右侧是固定范围宽度的 Part 列表，中间只保留一条分隔线。
- 窄屏按预览区、Part 列表顺序上下堆叠，分隔线切换到列表顶部。
- 窄屏下 Part 列表高度以 `CompactPaneMaxHeight`（默认 400）为上限并在列表内部滚动，预览区始终钉在上方可见；列表不得随内容无限增高而把预览区挤出页面视口。
- 宽屏下且 Semantic Parts 标签页内容被宿主限高时（`GalleryShowCaseHost` 只对单个 Preview 的模板开启
  `IsContentHeightBounded`，把 `GalleryStickyTabsHost` 的内容宿主 MaxHeight 绑定为页面视口减去页头与吸顶区后的剩余高度），
  Part 列表填满该剩余高度减去检查面板固定上下内边距（20 上边距 + 32 下边距 + 2 边框，共 54）后的空间，并在列表内部滚动；
  列表内容不足该高度时面板仍按内容决定高度并在内容区顶部对齐，不会被页面剩余高度强制拉伸成大面积空白舞台。
  绑定链是：宿主 code-behind 计算的 Content 宿主 MaxHeight → Preview 继承的 MaxHeight →
  `SemanticPartPreview.PaneMaxHeightConverter` 换算后的布局面板 `PaneMaxHeight`。
- 限高钳制保留下限：`GalleryShowCaseHost` 通过 `GalleryStickyTabsHost.ContentMinHeight` 传入单 Preview 的
  `PreviewStageMinHeight + 54`。视口剩余高度不足该下限时，钳制值保持在下限而不是继续收缩，页面 ScrollViewer 的
  extent 超过 viewport，出现页面级垂直滚动条兜底（钉住弹层、详情区等被裁切内容必须可滚动到达）；
  窗口足够高时限高填充行为不变。禁止回到"钳制无下限"的形态——那会把内容挤出可视区且无法滚动。
- 模板堆叠多个 Preview 或宿主未限高时，宽屏 Part 列表以 `PaneMaxHeight`（默认 400）为上限并在列表内部滚动。页面测量链为
  Preview 提供无限高度，若不对列表高度设上限，列表会按全部行高撑高整个面板，把预览区挤出页面视口（短窗口下尤其明显），
  浏览靠后的 Part 就只能滚动页面而不是滚动列表本身；多 Preview 模板保持内容尺寸布局，避免第一个 Preview 占满视口余量后
  把后续 Preview 挤出首屏。
- Part 行使用平铺列表和行分隔线，不使用独立边框、圆角卡片、嵌套卡片或额外的 Semantic Parts 标题。
- 主行只常驻显示 Part 名称、本地化职责描述、Pin 和 Info；selector、SelectorRoute、ContractType、cardinality、
  customization、跨视觉根和运行时创建等技术字段只在用户打开 Info 后显示。
- Info 打开后，下方详情区横跨检查面板完整宽度：selector、SelectorRoute、ContractType、StyleType、cardinality、customization 等技术元数据
  位于左侧固定宽度栏，Styling example 代码区位于右侧并占据主要宽度。
- 紧凑布局中，下方详情区按技术元数据、Styling example 的顺序上下堆叠，两者均占满宽度。
- 代码示例不得挤在固定宽度的 Part 右栏、拆成独立卡片或使用会遮挡目标预览的浮层。
- Pointer 经过的行使用克制的填充色；目标 Adorner 使用在浅色背景和 Primary Control 上都可辨认的高对比描边，
  但不得改变目标 Control 的属性、Style、class 或模板。

Info 元数据视图和代码查看器继续按需创建。未点击 Info 时，右栏元数据区域和全宽代码行都保持零高度，技术元数据视图、
代码文本、语法高亮器及其订阅都不存在。

## 7. 交互状态

Preview 只允许一个有效选择：

```text
effectivePart = pinnedPart ?? hoveredPart
```

- Pointer 进入列表项时设置 `hoveredPart`。
- Pointer 离开时清除 `hoveredPart`。
- Pin 对当前 Part 进行固定或取消固定。
- Pin 存在时，经过其他列表项不改变有效选择。
- Info 按需显示该 Part 的技术元数据并生成用户侧 AXAML，不触发目标查找。
- 用户侧 AXAML 中 AtomUI 控件命名空间统一使用 `atom` 前缀；Avalonia Contract 类型位于其他命名空间时使用
  `contract` 前缀。
- 从 Semantic Parts 切回 Examples 时清除 Hover 并停用当前 `SemanticPartHighlightSession`。
- 返回 Semantic Parts 时，如果仍保留 Pin，可以重新解析一次目标，但不能保留旧 Visual 引用。

列表主行显示名称和本地化职责描述，不附加高亮结果 Tooltip。引入版本保留在技术详情中；未实例化、当前不可见或超过高亮预算只影响
Adorner 结果集，不新增常驻文本、弹层或状态提示。

## 8. 目标解析

目标解析只在 `effectivePart` 变化时执行，并按 `DispatcherPriority.Render` 合并同一帧内连续 Hover 变化。稳定选择期间不重复
扫描 VisualTree。

解析单元不是单一 owner Control，而是 Preview 内容范围内满足 owner type 的全部实例（§8.4）。`SemanticPartHighlightSession`
接收 owner 实例集合，对每个实例执行一次 owner-scoped 查找并合并去重结果；`TotalMatchCount` 为所有实例的匹配总数，
`HighlightedTargetCount` 为合并后实际创建的 Adorner 数。单实例内容集合只有一个元素，行为与单 owner 完全一致。

### 8.1 Root

`root` 直接对应 owner Control，不依赖 `.semantic-root`。多实例作用域下，每个 owner 实例本身各算一个 `root` 目标。

### 8.2 静态模板 Part

静态模板 Part 以 Avalonia 12 公开的 `TemplatedControl.GetTemplateDescendants()` 为 owner 作用域入口。该 API 只返回
`TemplatedParent` 为当前 owner 的模板后代；Preview 在结果上按 descriptor class 过滤，不自行实现跨嵌套模板的全子树
class 搜索。

声明静态 Selector Part 的 owner 必须能够作为 `TemplatedControl` 提供这一作用域。非模板 Control 只能高亮 `root`，或者把
实际节点按 `RuntimeCreated` / `CrossVisualRoot` 契约交给对应的 owner-scoped resolver，不能退化为无边界 class 搜索。

候选节点必须同时满足：

- 节点是已附加的 Visual。
- `Classes` 包含 descriptor 的 `SelectorClass`。
- 节点的 `TemplatedParent` 与 owner 相同。
- 节点 `IsEffectivelyVisible=true` 且 Bounds 非零。
- 所在祖先链没有完全透明节点。`RestHidden=true` 的 Part 例外：静止态透明/隐藏是该 Part 的设计语义（如
  ImagePreviewer `cover` 的悬停遮罩），目标解析跳过 Opacity 过滤，在不可见状态下仍定位并描边；`IsEffectivelyVisible`
  与非零 Bounds 检查保留，closed popup 中的目标依旧不会误命中。`RestHidden` 只是预览定位元数据，不改变控件运行时行为。

不能只按 `.semantic-*` 扫描整个 owner 子树。不同 Control 可以合法复用 `.semantic-icon` 等 class，缺少 owner 边界会误命中
嵌套子 Control 的 Semantic Part。

### 8.3 Runtime-created Part

`RuntimeCreated=true` 时允许在 owner 的局部 Visual 作用域中查找已实例化节点。`SelectorRoute` 是 descriptor 与生成 Style 的静态
契约，不作为 Preview 的 runtime traversal 程序，也不直接生成用户主 selector；Preview 继续通过 registry owner 边界、AdditionalRoots 和 class marker
解析目标。遍历遇到另一个已注册 Semantic Control
时停止进入其内部模板，防止同名 marker 跨 Control owner 泄漏。运行时节点必须遵守 Semantic Part 总架构定义的 parent
和 owner 契约；尚未附加到 VisualTree 的节点不属于可高亮目标。

解析结果不建立长期索引。每次有效选择只保存当前 `SemanticPartHighlightSession` 需要的可见目标，选择结束后释放全部引用。

### 8.4 多实例 owner 作用域

Preview 需要在一个预览内容中展示同一 owner type 的多个实例，并在选中某个 Part 时让所有实例同时高亮各自对应 Part。
这是 Preview 的通用高亮机制，与 `SemanticPart` 的 `Multiple` cardinality 无关：`Multiple` 描述单个 owner 内部同一 Part 的
多个替代节点，多实例作用域描述多个 owner 实例各自贡献的目标集合。两者可以叠加，例如两个实例各有两个 rail，选中 `rail`
时最多高亮四个目标。

多实例作用域的定义与约束：

- 作用域根是 `PreviewContent` 子树；`PreviewContent` 自身满足 owner type 时也纳入作用域。
- 实例集合只按 owner type 可赋值关系从作用域根的 visual descendants 收集，并保持稳定的深度优先顺序去重。
- 收集使用公开 VisualTree API（`GetVisualDescendants` / `IsAssignableFrom`），不进行程序集扫描、反射实例化或全局 TopLevel
  搜索。派生实例（例如 `VerticalSeparator : Separator`）复用基类契约时，通过 owner type 可赋值关系纳入同一作用域。
- 每个实例仍执行 §8.2 / §8.3 的 owner-scoped 查找：静态 Part 只命中 `TemplatedParent == 该实例` 的模板后代，
  `RuntimeCreated` / `CrossVisualRoot` 只在该实例的局部作用域与附加 root 中查找。多实例不会退化为跨实例的无边界 class
  搜索，也不会让一个实例命中另一个实例模板内的同名 Part。
- 不可见、零尺寸或未附加的实例与节点继续被 `IsEligible` 过滤；隐藏实例不产生目标，也不影响其他实例的匹配。
- 合并后的目标顺序稳定：先按实例收集顺序，再按每个实例内部的解析顺序；同一实例内部的顺序不变。
- 集合为空（例如 PreviewContent 尚未实例化任何 owner）时，回落为把 `SemanticOwner` 当作唯一实例，保证至少能解析
  `root`；这不改变 descriptor 解析和类型校验入口。

该机制不要求具体 Control 或 Theme 新增属性、marker 或生命周期代码；Gallery 只需在一个 Preview 的 `PreviewContent` 中
放入多个 owner 实例即可获得多实例高亮。需要分别描述不同 owner type 时仍使用多个 Preview。

## 9. Adorner 高亮

每个可见目标通过 Avalonia 12 公开的 `AdornerLayer.GetAdornerLayer(target)` 获取对应 layer，并在该 layer 中创建一个临时
`SemanticPartAdorner`。不得从 Preview 自身寻找共享 layer，不得把所有目标坐标转换到 Preview 所在 Canvas，也不得跨
VisualRoot 手动换算屏幕坐标。

`SemanticPartAdorner` 必须：

- 只绘制高亮边框，不设置背景或半透明填充，不覆盖或改变目标原有颜色、文字和图形。
- 视觉对齐 antd SemanticPreview 的 `Marker`：金框沿目标边界外侧绘制，主目标使用 `2px` 全不透明金框；其余目标使用同色
  `1px`、85% 不透明描边。
- 主目标不绘制 antd `Marker` 的 `boxShadow: 0 0 0 1px #fff` 白色外环。该外环在浅色舞台上会显出一条突兀的白线，属于
  上游为深色背景做的对比补偿；AtomUI 舞台以浅色为主，按产品决定不采用。外环移除后 layout 外扩量即为笔宽所需的半个
  线宽，主目标外扩 `2px`、次目标外扩 `1px`。
- 描边落在目标 bounds 外沿，细窄目标（如 4px 高的 slider tracks）也能获得清晰可见的金框。
- 描边矩形钳制到 adorner 所在 AdornerLayer（窗口客户区）内：贴边满区目标（如 ImagePreviewer native 预览对话框的
  `popup.root` / `popup.body`）外扩后左、右、下边会越出窗口表面被 OS 裁剪，视觉只剩贴窗的一条边；钳制保留外扩与
  笔宽余量，贴边目标的描边贴窗口边缘完整显示。层不可达或交集退化时按原矩形绘制，不抛错、不隐形。
- 构造时必须设置 `AdornerLayer.SetIsClipEnabled(adorner, false)`。Avalonia `AdornerLayer` 默认会根据 adorned target 的祖先
  裁剪状态合成 clip；如果目标位于 `ClipToBounds=true` 的 Border、Panel、Masonry、ScrollViewer 或其他模板节点中，默认 clip
  会把顶部、左侧或底部的外扩描边截回目标矩形。关闭该 attached property 是 Semantic Preview 的基础设施契约，不能由具体
  Control、Gallery 页面或单个 Part 自行补偿。
- adorner 使用负 Margin 或等价布局外扩，使 marker geometry 及其 Pen 厚度完全落在 adorner 自身 Bounds 内；只扩大 geometry 而
  保留原始 adorner Bounds 不能保证渲染可见。
- adorner 自身不得设置 `Clip`；目标祖先的 `ClipToBounds` 和 `Clip` 不得为了 Preview 被修改。
- 不依赖目标 ControlTemplate。
- `IsHitTestVisible=false`、`Focusable=false`。
- 第一个目标使用主高亮样式，其余目标使用次级样式；多实例作用域下按 §8.4 的合并顺序决定主/次样式，顺序稳定且可复现。
- 不使用 `AdornerLayer.AdornerProperty`，避免覆盖焦点、验证或其他现有 Adorner。

`SemanticPartHighlightSession` 按以下顺序释放每个高亮 Adorner：

1. `AdornerLayer.SetAdornedElement(adorner, null)`。
2. 从对应 `AdornerLayer.Children` 删除 adorner。
3. 退订 Gallery 自己建立的临时事件。
4. 清空 adorner、target 和 layer 引用。

清空 `AdornedElement` 是强制步骤。Avalonia 12 的 Adorner 实现通过该变化释放内部 ancestor property subscription；仅从
Children 删除 Visual 不能作为完整生命周期契约。

### 9.1 高亮渲染不变量

所有高亮创建路径必须经过 `SemanticPartAdorner` 的统一构造路径，由 adorner 自身建立以下不变量：

1. `Focusable=false`、`IsHitTestVisible=false`。
2. `AdornerLayer.GetIsClipEnabled(adorner)==false`，且 `adorner.Clip==null`。
3. 主、副 marker 的几何和 Pen 外接 Bounds 完全位于 adorner 自身布局 Bounds 内。
4. 目标 Control、目标祖先和目标模板不因高亮而改变属性、布局、裁剪、样式或事件。

因此，遇到边缘缺线时，维护者必须先检查 adorner 的 clip 配置和 Bounds；不得通过修改控件 `ClipToBounds`、Preview 对齐、Masonry
坐标或模板 Padding 来修复 Semantic Preview 的渲染问题。该规则适用于所有控件，不是 Masonry 专用约定。

### 9.2 RestHidden 部件的预览显现契约

`RestHidden=true` 的部件静止态透明/隐藏是设计语义（如 ImagePreviewer `cover` 的悬停遮罩）。语义预览激活这类部件时，
除描边外还要把目标本体显现出来，让用户看到部件的真实视觉：

- 高亮会话对每个解析出的目标设置 `AtomUI.Theme.SemanticPartPreviewState.IsPreviewTarget=true`
  （会话释放与刷新时置回 `false`）。该附加属性是预览状态契约，位于 AtomUI.Core——写入方（GalleryBase 会话）与
  读取方（控件 ControlTheme）分属不同程序集，依赖方向要求契约下沉；伪类是 `protected`，外部无法设置。
- 控件 ControlTheme 以属性条件选择器响应并自行决定显现方式。cover 遮罩的显现是把遮罩 Border 的 `Opacity` 提到 1
  （Style 优先级高于 TemplateBinding，状态移除后自动回落到 `MaskOpacity` 绑定）：

```xml
<Style Selector="^ /template/ Border#Mask[(atom|SemanticPartPreviewState.IsPreviewTarget)=True]">
    <Setter Property="Opacity" Value="1" />
</Style>
```

- 拥有 `RestHidden` 部件的控件必须在 ControlTheme 中声明对应显现样式；缺失时该部件只有描边、本体不显形（降级可见，
  不报错）。选择器语法注意：属性条件中的 XML 命名空间前缀用 `|` 分隔（CSS 惯例），不是 `:`。
- 该状态只影响预览观感，不改变控件运行时行为；不用 `.semantic-*` class 表达，内置主题默认视觉依旧不经 `.semantic-*`
  selector 实现。

## 10. Popup 与独立宿主

`CrossVisualRoot=true` 的模板 Popup 使用 owner-scoped 路径：

1. 只从 owner 的 `GetTemplateDescendants()` 结果中枚举 `Popup`。
2. Popup 打开后从公开的 `Popup.Child` 进入实际内容树。
3. 在 Child 所属 VisualRoot 中解析 marker。
4. 对匹配目标调用 `AdornerLayer.GetAdornerLayer(target)` 并添加 Adorner。
5. 只在该 Part 有效选择期间订阅对应 Popup 的 `Opened`、`Closed`。
6. Popup 打开后在 Render 优先级执行一次重新解析；关闭后立即清除对应高亮。

多实例作用域下，每个 owner 实例的模板 Popup 都按同一路径订阅与解析；`Dispose` 时统一退订全部实例的 Popup 事件并清空
所有目标。

这一模型同时覆盖 OverlayPopupHost 和 PopupRoot，不依赖普通视觉树跨根继承，也不扫描所有 TopLevel。

Modal、Message、Notification 等由服务创建且不再能从 owner Popup 到达的独立宿主，不使用隐藏全局搜索。具体 Gallery Demo
可以向 Preview 显式提供 `AdditionalRoots`；该能力属于 Gallery 接入信息，不得要求产品 Control 增加 Preview 接口。

### 10.1 独立窗口宿主（ISemanticPartCrossRootProvider）

控件把部件活体承载在独立 TopLevel（如 ImagePreviewer 的 native 预览对话框窗口）时，模板内不存在 `Popup` 对象，
Popup 订阅路径无从发现宿主。此类控件实现 `AtomUI.Theme.ISemanticPartCrossRootProvider`：

```csharp
public interface ISemanticPartCrossRootProvider
{
    event EventHandler? CrossRootsChanged;      // 宿主集合出现/消失/表面就绪时触发
    IReadOnlyList<Visual> GetCrossRoots();      // 当前存活的跨根宿主视觉根
}
```

- 高亮会话发现 owner 实现该接口即订阅 `CrossRootsChanged`（与 Popup 的 `Opened`/`Closed` 订阅同构，Render 优先级
  合并刷新），并把 `GetCrossRoots()` 结果并入附加根集合；`Dispose` 统一退订。
- **触发位点必须覆盖宿主生命周期全程**：宿主打开、宿主表面模板就绪（模板应用晚于 `Show()` 的同步路径，ImagePreviewer
  在 `RootTemplateApplied` 补抛一次）、宿主关闭回收。漏掉表面就绪会导致首次刷新时目标未附加而拿空结果且不再重试。
- 描边 adorner 按 `AdornerLayer.GetAdornerLayer(target)` 落在宿主窗口自己的 AdornerLayer；`RestHidden` 显现状态
  （`SemanticPartPreviewState.IsPreviewTarget`）直接设在目标节点上——二者天然跨根，无需额外处理。
- 宿主关闭后控件必须把根从 `GetCrossRoots()` 收回（如置空 open state），会话刷新后高亮与显现状态一并释放。
- 契约位于 AtomUI.Core：写入方（GalleryBase 会话）与实现方（控件）依赖方向都指向 Core；不要求产品 Control 增加
  Gallery 专用接口。宿主根的生命周期由控件自己管理，会话不做逻辑树推断或全局 TopLevel 扫描。

带模板 Popup 的控件（AutoComplete 先例）演示 `popup.*` 部件时，演示控件按以下模式钉住弹层常开，使 `popup.root`、
`popup.list`、`popup.listItem` 随时可解析、可高亮：

1. XAML 上设置 `IsDropDownOpen="True"` + `IsPopupPinnedOpen="True"`（钉住后忽略 light-dismiss 关闭请求）。
2. 产品控件在弹层打开前 relay pin，共享 Popup 负责首次打开及运行中 pin 变化的 light-dismiss 注册（见架构文档 9.1）；预览基础设施不得直接改写
   `IsLightDismissEnabled`——Avalonia 仅在打开瞬间读取该属性，打开后修改无效，该路径已被实现并否定。
   注意第 1 条的 `IsDropDownOpen="True"` 是**挂载前**就为 true：若产品控件的模板把弹层写成
   `IsOpen="{TemplateBinding IsDropDownOpen}"`，弹层会在模板充气阶段打开，早于控件的遮罩抑制，于是留下一个
   `IsVisible=true` 的遮罩层挡住整页交互。正确形态是模板**不声明** `IsOpen` 绑定，由控件在抑制遮罩之后以代码补开
   弹层（`AbstractSelect` 一族与 `ComboBox` 均已如此）。因此产品控件在接入 pinned 预览时，除了断言
   `IsLightDismissEnabled=false`，还必须断言树上不存在可见的 `LightDismissOverlayLayer`。
3. `popup.listItem` 等容器部件的 marker 由列表控件容器创建时注入，解析走本节 owner-scoped 路径即可命中，
   Preview 无需额外处理。
4. 高亮框以 marker 元素 Bounds 为准：内联语义（如 placeholder 文字）的 marker 元素必须紧贴内容排布，
   Preview 侧不得通过放大 Adorner 矩形补偿。
5. 首次进入 Semantic Parts 时必须区分 pinned 请求、控件业务 open、`Popup.IsOpen` 和最终视觉可见四层状态；
   物理 Popup 已打开但 motion actor 仍为透明不算成功。
6. `Opened` 与延迟 host / actor ready 的先后顺序由产品控件或共享 Popup 对账。Preview 不直接操纵 actor，也不得把
   `IsMotionEnabled="False"` 当作生命周期修复；至少一个使用共享 host 的验收样例保留默认 motion，覆盖首次物化。

Popup 首次打开竞态、直接 Child wrapper 契约和 pinned light-dismiss 的完整排查记录见
[Semantic Part Popup 首次打开生命周期竞态案例](../../engineering/case-studies/semantic-part-popup-first-open-lifecycle-case-study.md)。

非 Popup 宿主的遮罩类弹层控件（Drawer 先例）演示语义部件时，不走 Popup 钉住模式，改用**内联常开舞台**，
即上游 antd `getContainer={false}` 的等价物：

1. 预览内容根是一个 `ScrollContentPresenter`，内含舞台 Border（限高、裁剪、浅色填充）。`ScrollContentPresenter`
   是 `ScopeAwareAdornerLayer` 层解析的最近宿主：层注入只包裹舞台自身。若缺少该局部宿主，层解析会逃逸到页面级
   滚动容器，`InjectLayer` 重挂整页内容导致 `GalleryShowCaseHost` 脱离视觉树并释放语义预览——这是已踩过的坑，
   不得回退。
2. 演示控件声明式设置 `IsOpen="True"` + `IsPinnedOpen="True"`（钉住后忽略遮罩点击与关闭按钮）+
   `IsMotionEnabled="False"`，`OpenOn` 绑定舞台 Border，遮罩与面板渲染于舞台局部 layer 内。
3. 舞台内容用 `Grid` 让零尺寸 owner 拉伸铺满舞台：`root` 卡片高亮 = 整个内联容器，对齐上游 root 语义
   （resolver 对零尺寸目标不建 Adorner，此布局同时解决该问题）。
4. 跨根根集合由控件自身的 `ISemanticPartCrossRootProvider` 上报，页面无需 code-behind 注册
   `AdditionalRoots`（区别于 DropdownButton 的 Popup 根注册模式）。
5. 舞台 Border 用 `MinHeight` + 默认拉伸，禁止固定 `Height`：限高钳制模式下画布会随宿主收缩，
   固定高度会与视口相抵被裁（遮罩/面板连同高亮框越出可见区）。`PreviewStageMinHeight`
   地板必须覆盖舞台内容的真实期望：视口 = 地板 − 62（面板开销），需 ≥ 舞台 MinHeight + 边距
   （Drawer 先例：MinHeight 320 + 边距 48 → 地板 434）。

## 11. 高密度预算

`Multiple` Part 只处理已实例化且可见的节点。单次 `SemanticPartHighlightSession` 默认最多创建 32 个 Adorner：

- 匹配数量不超过预算时高亮全部目标。
- 超出预算时只高亮按稳定遍历顺序得到的前 32 个目标。
- 列表显示总匹配数和当前高亮数量。
- 不为不可见、零尺寸、完全透明或未附加节点创建 Adorner。

多实例作用域合并后的目标集合共享同一 32 个 Adorner 预算；预算跨实例统一执行，不按实例分别重置。高密度场景应避免在
单个 Preview 中放置大量同类型实例。

该预算保护 DataGrid cell、虚拟化 item 和其他高密度场景，不能通过缓存所有 Visual 引用规避预算。

## 12. 用户侧代码片段

代码片段由 descriptor 按需生成：

- `root` 生成 owner type selector 或实例级 Styles 示例。
- Selector Part 生成 owner scope、`StyleType` 和 `x:SetterTargetType="ContractType"`；生成 Style 内部封装 descriptor
  `SelectorRoute`。
- 不把复杂 route 拼接成用户主 selector，也不根据 `RuntimeCreated` 推断 descendant selector。
- `SelectorAndTheme` 可以补充强类型 Theme 属性入口，但 Selector 仍是基础契约。
- 不生成 `ContractType.semantic-*` 或 `:is(ContractType).semantic-*`。

代码展示复用 GalleryBase 已有源码查看能力，不引入第二套语法高亮器。Snippet 只有在用户打开 Info 时生成并缓存为纯字符串。

## 13. 性能与 AOT

成本模型必须分开描述：

| 状态 | 允许成本 |
|---|---|
| 普通应用未引用 GalleryBase | 无 Preview 增量成本 |
| Gallery 未创建 Semantic Tab 内容 | 只有 Tab header；无 Preview 对象和 registry 查询 |
| Preview 已创建但无有效选择 | descriptor 查询和列表 Visual；无目标扫描、Adorner 或目标事件订阅 |
| Hover/Pin 有效 Part | 一次 owner 实例作用域内查找（见 §8.4）、有限 Adorner 和原生布局跟踪 |
| 离开、取消 Pin、切换 Tab 或 detach | `SemanticPartHighlightSession` 全量释放 |

实现不得使用：

- 反射扫描 Control、属性、程序集或 AXAML。
- `Activator.CreateInstance` 创建 owner 或 Part。
- 全局 VisualTree/TopLevel registry。
- 常驻 `LayoutUpdated`、ScrollChanged、pointer move 或定时器。
- 每帧坐标重算和矩形分配。
- 静态 Visual、Control、Popup、Adorner 或 descriptor-to-instance cache。

多实例 owner 作用域的收集只遍历 `PreviewContent` 子树并只按 owner type 可赋值关系筛选，不使用全局 VisualTree、程序集扫描
或反射实例化。收集结果只在本次有效选择内保留，选择结束后与 `SemanticPartHighlightSession` 一起释放，不建立跨选择的实例
缓存。

Type identity、Part 元数据和 registry 均来自生成式静态 descriptor，满足 NativeAOT 与裁剪边界。

## 14. Button 首个样例

Button 是首个完整验收样例，不拥有 Preview 架构。Button ShowCase 只负责：

- 提供 `SemanticPartsContentTemplate`。
- 在模板中创建一个带图标和内容的真实 Button。
- 为 `root`、`icon`、`content` 提供本地化职责描述。
- 验证 `icon` 的 Multiple/可见替代实现和 `content` 的 Single 契约。

Button、ButtonTheme 和 Button Browser Theme 不因 Gallery Preview 新增任何属性、事件、marker、Style、Binding 或生命周期代码。

## 15. 验证要求

实现至少验证：

1. 页面初始化、Measure、Arrange、滚动、主题和语言切换均不构建 Semantic Parts 内容。
2. 第一次选择 Semantic Parts 时 factory 只执行一次；当前页面内重复切换复用缓存。
3. 未选择 Part 时不枚举 owner VisualTree，不存在 Adorner 和目标事件订阅。
4. Root、Single、Optional、Multiple 和不可见替代节点解析正确。
5. 嵌套 Semantic Control 的同名 class 不被误命中。
6. OverlayPopupHost 与 PopupRoot 使用目标所在 root 的 AdornerLayer。
7. 切回 Examples、Popup 关闭、目标 detach 和页面 detach 后释放全部高亮 Adorner 与订阅。
8. 超过 32 个可见目标时严格执行高亮预算，且不创建额外状态提示。
9. Info 生成的 AXAML 使用 owner selector、生成 `StyleType` 和 `x:SetterTargetType` 正式写法。
10. Preview 使用单外框、相邻双栏、平铺分隔行；主行不显示 selector、ContractType、cardinality 或 customization。
11. Info 打开前不创建技术元数据视图和代码查看器；打开后显示 selector、StyleType、SelectorRoute 等 descriptor 技术字段与样式示例。
12. 复杂 runtime Part 的代码示例使用生成 Style，并验证不会命中嵌套 Semantic owner 的同名 Part。
13. Button Control 和 Theme 的 public surface、模板 marker 与运行时路径没有因 Preview 变化。
14. Preview、演示 Control 和 Popup 在页面释放后可以被 GC。
15. 单 owner 页面和多 owner 家族页面都保持真延迟创建；多 Preview 内容根切换 Tab 时统一激活、停用和释放。
16. 多实例 owner 作用域：`PreviewContent` 含多个同 type 实例时，选中一个 Part 所有可见实例的对应 Part 同时高亮；派生实例
    按基类 owner type 纳入同一作用域；隐藏/未附加实例被过滤；跨实例不泄漏同名 Part；主/次样式顺序稳定；集合为空时回落
    `SemanticOwner` 单实例。
17. Desktop、Browser、裁剪和 NativeAOT 路径不需要反射保留配置。
18. 高亮渲染回归：root、静态模板 Part、runtime-created Part、跨视觉根 Popup、薄尺寸目标和多实例目标都通过统一
    `SemanticPartAdorner` 路径创建；每个 adorner 的 ancestor clipping 已关闭、自己的 Clip 为空、外扩几何不越出 Bounds，且
    被 `ClipToBounds=true` 祖先包裹时四边仍保持完整。
19. Popup 首次物化回归：从 Examples 初始状态第一次选择 Semantic Parts 后，pinned 请求、业务 open、物理 open 和视觉可见
    同时成立，`popup.*` 目标可高亮；切回、detach、reattach 和再次选择不依赖旧 host 或旧 actor。
20. 至少一个共享 Popup host 样例在默认 motion 开启时覆盖首次进入；关闭 motion 的样例不能替代该回归。

## 16. 相关文档

- [Semantic Part 系统](../../architecture/systems/theming/semantic-parts.md)
- [Gallery ShowCase 页面设计](gallery-showcase-design-pattern.md)
- [GalleryBase ShowCase 控件设计](../../modules/toolkits-gallery-base/showcase-controls.md)
- [GalleryBase 架构](../../modules/toolkits-gallery-base/architecture.md)
- [AOT 编程规范](../../engineering/development/aot-programming-guidelines.md)
- [Semantic Part Popup 首次打开生命周期竞态案例](../../engineering/case-studies/semantic-part-popup-first-open-lifecycle-case-study.md)
