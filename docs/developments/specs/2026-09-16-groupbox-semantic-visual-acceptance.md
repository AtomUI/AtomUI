# GroupBox Semantic Part 改造 · 真机视觉验收步骤

> 状态：**部分通过**（已在真机核对：Custom Semantic Part styling 示例全项、Semantic Parts 页签的 `root` 高亮；`header` / `icon` / `title` / `content` 高亮与 Examples 回归仍待用户回传截图）。
>
> ## 验收结论记录（文字证据）
>
> | 日期 | 证据 | 结论 | 修复记录 |
> | --- | --- | --- | --- |
> | 2026-09-16 | 用户截图：`semantic-border` 上方的说明文字横向溢出卡片、被右边界截断 | 确认为缺陷（该说明 `TextBlock` 漏写 `TextWrapping="Wrap"`；Avalonia `TextWrapping` 默认 `NoWrap`） | 补 `TextWrapping="Wrap"`，新增 `Semantic_Border_Caption_Declares_Wrapping_So_It_Does_Not_Overflow_The_Card` 锁定（去掉 `Wrap` 时该用例以 `NoWrap` 失败，已实测） |
> | 2026-09-16 | 用户截图：`Custom Semantic Part styling` 示例四项（见步骤 2） | **通过**：caption 正常折行；`semantic-object` 灰底 `#f0f0f0` + 深色标题；`semantic-function` 紫底 `#f5efff` + 紫色图标 + 20×20 图标尺寸；`semantic-border` 紫边框 2px + 12px 圆角 + 淡紫底；`semantic-border-plain` 青边框 2px + 2px 圆角 + 透明底，标题下方无边框短线残留。圆角处边框弧线连续、缺口过渡自然。 | 无（另经探针实测 Header 图标默认尺寸为 16，示例覆盖为 20 属真实视觉变化，非空断言） |
> | 2026-09-16 | 用户截图：Semantic Parts 页签首屏，列表首行显示为 `group` | 判定为**显示假象而非缺陷**：`group` 是 `root` 行说明文字的最后一个词（"…overall appearance of the group"），该行字体为常规字重灰色，与部件名的 SemiBold 明显不同。原因是 `PART_PartsPane` 打开了 `ClipToBounds` 且列表内容略高于可视区，面板处于滚动状态，`root` 行标题与说明前几行被卷出可视区。**未确定滚动由何触发**：headless 复跑切页签后偏移恒为 0，给首行固定按钮聚焦也不滚动（主题已设 `BringIntoViewOnFocusChange="False"`），可能是用户浏览时滚动过，也可能是真实后端上的自动滚动。 | 无需修复（数据本身正确：探针实测 `ITEMS=[root,header,icon,title,content]`、5 行、root 在第一行） |
> | 2026-09-16 | 用户截图：Semantic Parts 页签滚动到顶，`root` 被选中高亮 | **通过**：`root` 行完整显示（粗体部件名 + 完整说明），琥珀色高亮框完整包围 GroupBox owner 整体，四边完整可见，与 `root` = owner 本身的契约一致。 | 无 |
> | 2026-09-16 | 尚无 `header` / `icon` / `title` / `content` 高亮与 Examples 回归（步骤 1.2、步骤 3）的回传截图 | 待视觉验收 | 无 |

## 背景

本次 GroupBox 语义部件改造对外开放 4 个非 root Part（`header` / `icon` / `title` / `content`）与隐式
`root`，均为 `GroupBoxTheme.axaml` 单一模板内的静态 marker。涉及两处**可能影响渲染**的变更，是本轮验收的重点：

1. `PART_HeaderContent` 的承载节点由 `Decorator` 提升为 `Border`，以获得 `Background` 能力。`Border`
   继承 `Decorator`，模板 part 查找路径与基于 `Bounds` 的缺口几何不变；主题中两个以节点类型开头的 selector
   已由 `Decorator#PART_HeaderContent` 同步为 `Border#PART_HeaderContent`。**该同步若遗漏，`HeaderContentPadding`
   与标题三档对齐会静默失效**——自动化已断言，但仍需真机确认缺口与对齐的实际观感。
2. `GroupBoxTheme.axaml` 新增 4 处 inert marker（`semantic-header` / `semantic-icon` / `semantic-title` /
   `semantic-content`），均为静态 `Classes.semantic-*="True"`，只提供样式命中点，不改默认视觉。

Gallery 页面顶部由 `GalleryStickyTabsHost` 换成 `GalleryShowCaseHost`，新增 Semantic Parts 页签与
Semantic Part 样式示例。

## 操作路径与判定标准

准备：运行 AtomUIGallery（`controlgallery/AtomUIGallery.Desktop`），进入
**Data Display（数据展示）** 分类 → **GroupBox** 页面。

示例入口：Semantic 示例卡片 `SourceKey="group-box-semantic-part"`，标题为
`Custom Semantic Part styling`；页签为页面顶部 `Semantic Parts`。

### 步骤 1：Semantic Parts 页签

| # | 操作 | 预期现象 | 截图 |
| --- | --- | --- | --- |
| 1.1 | 点击页面顶部 "Semantic Parts" 页签 | 出现一个 GroupBox 预览（标题 `Title info` + GitHub 图标 + 内容 `Content of group box`），右侧部件列表可滚动到底 | 全景 |
| 1.2 | 在部件列表中逐个选中 5 个 Part（root / header / icon / title / content） | 每个 Part 都有对应高亮框；`header` 高亮覆盖标题与图标整块区域且**上边缘与分组边框缺口对齐**；`icon` / `title` 高亮落在 header 内部各自子区域；高亮四边完整可见 | 逐部件或分组 |
| 1.3 | 取消选中、切回 Examples 再切回 Semantic Parts，并切换 Light/Dark 主题 | 高亮清除；重新进入预览正常重建；主题切换下高亮与预览不残留 | 分组 |
| 1.4 | 观察预览 GroupBox 的边框缺口 | 标题文字两侧边框**断开**成 fieldset 缺口，缺口宽度恰好覆盖 `Title info` 与图标；缺口处**没有**边框短线残留、没有断裂或重叠 | 局部放大 |

### 步骤 2：Custom Semantic Part styling 示例

| # | 操作 | 预期现象 | 截图 |
| --- | --- | --- | --- |
| 2.1 | 滚动到 `Custom Semantic Part styling` 示例，确认卡片 Tag 显示当前 AtomUI 版本（`v6.1.9`） | 出现四个 GroupBox：`semantic-object`、`semantic-function`、`semantic-border`、`semantic-border-plain` | 全景 |
| 2.2 | 观察 **semantic-object**（无图标） | Header 区域为浅灰底 `#f0f0f0`、内边距 16；标题为深色 `#141414`；内容区域内边距 16,12 | 单个控件 |
| 2.3 | 观察 **semantic-function**（带 GitHub 图标） | Header 区域为浅紫底 `#f5efff`；图标为 **20×20** 且为**紫色 `#722ED1`**（默认主题为 `IconSizeLG` + `ColorText`，必须被示例样式覆盖）；标题为深色 `#141414` | 单个控件 + 图标局部放大 |
| 2.4 | 放大观察两个示例 Header | 不透明 Header 背景被边框缺口**正确包围**，标题下方**不出现**被还原的边框短线；Header 背景不溢出分组圆角 | 局部放大 |
| 2.5 | 把窗口切到 Dark 主题后再观察 2.2 / 2.3 | Header 背景与标题颜色仍按示例值生效（示例为显式色值，不随主题变化）；分组边框与圆角正常 | 分组 |
| 2.6 | 观察 **semantic-border** 与 **semantic-border-plain**（这两条演示 root 区域的边框定制） | `semantic-border`：**紫色 `#722ED1` 的 2 像素边框**、**12 像素圆角**、淡紫分组底 `#f9f0ff`、Header 底为白色；`semantic-border-plain`：**青色 `#13c2c2` 的 2 像素边框**、**2 像素近直角圆角**、分组背景透明、标题为青色 `#08979c` | 两个控件同框 |
| 2.7 | 重点核对 2.6 的四项边框视觉 | 边框**颜色、粗细、圆角**三项同时生效，且**圆角处的边框弧线与缺口过渡自然**（这是自绘几何最容易出问题的地方）；圆角变化时缺口位置随之上移/下移，不断裂、不重叠 | 局部放大（圆角处） |
| 2.8 | 观察 `semantic-border-plain` 的透明背景 | 分组内部透出页面底色，**标题下方无边框短线残留**（透明背景下缺口必须仍是几何排除而非背景遮挡） | 局部放大 |

### 步骤 2b：边框定制入口说明

边框定制是 `root` 区域的契约，**不是**某个 Part Style 的能力：分组边框与背景由 `GroupBox.Render` 自绘，模板中没有承载它们的节点。因此示例中对 `semantic-border` / `semantic-border-plain` 使用 owner 作用域的**普通 Setter**（`BorderBrush` / `BorderThickness` / `CornerRadius` / `Background`），而不是 `<atom:GroupBoxXxxStyle>`。走查时若发现这两条示例的边框没有变化，说明该入口未生效，属于缺陷。

### 步骤 3：Examples 回归抽查

| # | 操作 | 预期现象 | 截图 |
| --- | --- | --- | --- |
| 3.1 | 走查 Basic、Auto height 两个示例 | 缺口、边框、圆角、内容内边距与改造前一致；Auto height 示例中内容增多时 GroupBox 高度随之增长，内容不被 Header 或边框挤压 | 全景 |
| 3.2 | 走查 Header title Position 示例（Left / Center / Right） | 三档标题位置正确；**缺口随之左右移动且始终恰好覆盖标题**；标题下方无边框短线 | 全景（三档同框） |
| 3.3 | 走查 Header title style 示例（Italic / Bold / Oblique / 四属性组合） | 字号、字重、字体样式、标题颜色按属性生效；缺口宽度随标题实际宽度变化 | 全景 |
| 3.4 | 走查 Header Icon 示例 | 图标与标题间距正常，缺口覆盖图标 + 标题；**无图标**的示例不保留图标占位宽度 | 全景 + 无图标条目局部 |
| 3.5 | 三档标题位置下切换 Light/Dark | 缺口与对齐在两种主题下均正确 | 分组 |

## 判定标准

- 步骤 1：5 个 Part 均可解析高亮；`header` 的 `ContractType=Border` 提升未破坏缺口几何与主题对齐。
- 步骤 2：生成的 `GroupBoxHeaderStyle` / `GroupBoxIconStyle` / `GroupBoxTitleStyle` / `GroupBoxContentStyle`
  均命中目标；**图标尺寸 20×20 与图标颜色 `#722ED1` 必须生效**（这是 owner API 无法表达、只能由 Part 提供的
  能力，也是本轮改造最核心的验收点）；不透明 Header 背景不得被当作缺口遮挡层。
  另外，**边框定制（步骤 2.6-2.8）必须经 owner 作用域普通 Setter 生效**：`BorderBrush` / `BorderThickness` /
  `CornerRadius` / `Background` 四项都要真正进入 `GroupBox.Render` 的自绘几何，而不只是属性表面被改。
- 步骤 3：新增 inert marker 与节点类型提升未改变既有示例的默认视觉；三档位置与缺口联动正确。

## 证据要求

按上表回传截图或录屏（步骤 3 的 Header title Position 三档请同框截图，便于核对缺口左右移动）。
图像不入库：验收完成后仓库只保留本文字步骤与结论，不保存截图。

## 自动化已覆盖范围（不替代真机验收）

以下已由自动化断言，但仍需真机确认最终观感：

- descriptor 四部件的数量、顺序、`ContractType`、cardinality 与标志位。
- 模板 4 个静态 marker 的存在与节点类型，且不出现 `semantic-scope-*` 锚点。
- 生成的 4 个 Semantic Style 各精确命中一个节点。
- Part Setter 对 `HeaderContentPadding` 基线与 `HeaderTitleColor` / `HeaderFontSize` 投影的优先级覆盖。
- **root 边框定制入口**：owner 作用域 Style 的 `BorderBrush` / `BorderThickness` / `CornerRadius` / `Background`
  覆盖 ControlTheme 默认 Setter，且覆盖后的值真正进入自绘几何（`_cachedBorderThickness` / `_cachedCornerRadius`
  与 Render 输出的填充画笔）——对应步骤 2.6-2.8 的属性与画笔层面，但**圆角处弧线的真实观感仍需真机确认**。
- `PART_HeaderContent` 类型提升后 `Find<Decorator>` 链路与 `header.Padding` 三档对齐仍生效。
- 缺口为几何排除（`CombinedGeometry` + `RectangleGeometry` 按 Header bounds），且与 Header 自身背景无关。
- 图标隐藏时 marker 保留、不占位；状态切换与模板重应用后命中数量不变。
- 固定 `header` 高度会使自动高度基线失效（尺寸基线失败回归）。

**已知自动化局限：** headless 测试平台的几何包含性无法表示圆角图形，`DashedBorder` 会按设计把圆角裁剪降级为
不应用，因此**角处背景是否被正确裁到圆角内、缺口在真实 Skia 后端是否无断裂**只能由真机截图确认。步骤 2.4
与 3.2 专门覆盖这两点。
