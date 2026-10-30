# Splash Semantic Part 改造 · 真机视觉验收步骤

> 状态：**部分通过**（2026-09-16 用户回传第二轮截图：预览舞台裁切缺陷与示例徽标溢出缺陷均已确认修复；
> 逐 Part 悬停高亮、多档窗口尺寸、窗口启动页与 Light/Dark 仍待复验）。
>
> ## 验收结论记录（文字证据）
>
> | 日期 | 证据 | 结论 | 修复记录 |
> | --- | --- | --- | --- |
> | 2026-09-16 | 用户截图：Semantic Parts 页签中，预览卡片区域下方出现一条与卡片同宽的白色残条（第二个实例只剩顶部一条） | 确认为**缺陷**：舞台把「不确定态」与「确定态」两个实例纵向堆叠，合计高约 757；而单预览页签的画布高度在部分视口下被 `GalleryShowCaseHost` 钳制到视口剩余高度（实测 482/358），超出部分被 `PART_PreviewContentHost` 静默裁掉，用户只看到第二个实例的顶部一条 | 把两个实例改为**两列等分 `Grid`**（列宽自适应画布、永不换行到第二行），实例 `Width=NaN` 覆盖主题 `WindowWidth` 由列宽决定；文案改用短标签并加 `Padding=18,16` 留出余量。新增按视口矩阵（12 种窗口尺寸）的参数化回归，实测在 520×380 至 1920×1080 全部尺寸下包含关系成立 |
> | 2026-09-16 | 用户截图：Semantic Parts 页签与「Custom Semantic Part styling」示例 | **裁切缺陷已修复**：两个实例现在左右并排、各自完整可见，画布底部不再有白色残条。复验中发现**第二处缺陷**（见下一行）。 | 无（裁切修复确认） |
> | 2026-09-16 | 复验截图：示例中的徽标看起来比 `SplashLogoStyle` 设定的尺寸偏大 | 确认为**缺陷**（示例自身不真实）：`logo` Part 的 `Width`/`Height` 作用在 presenter 上，而示例的 `LogoTemplate` 内容是固定 48×48 的 `Border`，把 36×36 的 presenter 撑破（实测 `Bounds=-6,-6,48,48`，每边溢出 6 px），Part Setter 对可见尺寸失效 | 示例徽标改为 `Stretch` 填满 presenter（预览舞台与样式示例两处），并新增断言**可见徽标 `Bounds` 等于 Part 设定值**（只断言 presenter 的 `Width`/`Height` 会放过该缺陷；实测反例下该断言失败）。约束与正确写法记入 `semantic-part.md` §2.2 |
> | 2026-09-16 | 用户截图（Semantic Parts 页签）：两个实例左右并排，均完整可见，画布底部无残条 | **通过**（第二处修复复验） | 无 |
> | 2026-09-16 | 用户截图（Examples 页签 · Custom Semantic Part styling）：徽标为有界圆角方块，不再外溢 | **通过**；另经运行时探针复核：presenter 与徽标本体均为 36×36、`Bounds=0,0,36,36`，零溢出；title/subtitle/message/detail 分别为 `#531DAB` / `#722ED1` / `#1D39C4` / `#08979C`，进度条高度 10，root 圆角 16 + padding 28,24 + 底色 `#F9F0FF`；两个实例顶对齐（y 均为 0、高度均 361） | 无 |
> | — | 逐 Part 悬停高亮、多档窗口尺寸、窗口启动页（Window service）、Light/Dark 尚无回传 | 待视觉验收 | 无 |

## 背景

本次 Splash 语义部件改造对外开放 9 个非 root Part（`logo` / `title` / `subtitle` / `content` / `spin` /
`progressBar` / `message` / `detail` / `footer`）与隐式 `root`，全部是 `SplashTheme.axaml` 单一模板内的静态
marker。本轮有**两处可能影响渲染**的变更，是验收重点：

1. `SplashTheme.axaml` 新增 9 处 inert marker（`Classes.semantic-*="True"`），只提供样式命中点，**不改默认视觉**；
   既有 `#PART_*` selector 与 Token 全部保留。因此 Examples 页签的默认观感应与改造前逐像素一致。
2. Gallery 专用启动窗口 `GalleryWindowSplash` 原本用 `/template/ atom|TextBlock#PART_TitleBlock` 这类选择器**穿透
   Splash 模板**定制颜色（违反既有文档的模板边界约定），本次改为生成的专用 Semantic Part Style。**这是本轮唯一
   会改变实现路径的渲染相关改动**：若路由或优先级不一致，四个文本位的前景色与状态色语义会静默走偏。自动化已断言
   可观察颜色（含 Success/Error 状态色），但仍需真机确认窗口观感。

Gallery 页面顶部由 `GalleryStickyTabsHost` 换成 `GalleryShowCaseHost`，新增 Semantic Parts 页签与 Semantic Part
样式示例。

## 操作路径与判定标准

准备：运行 AtomUIGallery（`controlgallery/AtomUIGallery.Desktop`），进入
**左侧导航「其他」（Other）** 分类 → **「Splash 启动页」** 页面。

页面顶部两个页签：**「示例」（Examples）** 与 **「语义部件」（Semantic Parts）**。

示例入口（可直接检索的锚点）：

| 入口 | 定位标识 |
| --- | --- |
| Semantic Parts 页签 | 页面顶部页签「语义部件」；`SemanticPartPreview` 名为 `SplashSemanticPreview` |
| Semantic 样式示例 | `ShowCaseItem` 的 `SourceKey="splash-semantic-part"`，标题「Custom Semantic Part styling」 |
| 窗口启动页示例 | `ShowCaseItem` 标题「窗口服务」（`WindowServiceTitle`），按钮「显示窗口 Splash」（`P2ContentShowWindowSplash`） |

### 步骤 1：语义部件（Semantic Parts）页签

| # | 操作 | 预期现象 | 截图 |
| --- | --- | --- | --- |
| 1.1 | 点击页面顶部「语义部件」页签 | 出现**两个** Splash 预览，**左右并排**（左为不确定态 + 右为确定态），两者完整可见、互不遮挡；右侧部件列表含 10 行（root / logo / title / subtitle / content / spin / progressBar / message / detail / footer） | 全景 |
| 1.2 | 逐个选中 `root` / `logo` / `title` / `subtitle` / `content` / `message` / `detail` / `footer` | 每个 Part 都有对应高亮框，且**两个实例上的同名 Part 同时高亮**（预览层合并同类型实例）；高亮四边完整可见 | 逐部件或分组 |
| 1.3 | 选中 `spin` | 高亮落在**左侧（不确定态）实例**的加载指示器上，且指示器当前可见 | 单个控件 |
| 1.4 | 选中 `progressBar` | 高亮落在**右侧（确定态）实例**的进度条上，且进度条当前可见 | 单个控件 |
| 1.5 | 取消选中、切到「示例」页签再切回「语义部件」，并切换 Light/Dark 主题 | 高亮清除；重新进入预览正常重建；主题切换下高亮与预览不残留 | 分组 |
| 1.6 | 观察两个预览实例的排版 | 标题、副标题、消息、详情、底部文案均正常显示，无溢出、无遮挡；两个实例互不重叠；**画布底部不出现任何被裁断的白色残条** | 局部放大 |

> 步骤 1.3 / 1.4 是本次最特殊的点：`spin` 与 `progressBar` **互斥可见**（前者只在不确定态可见，后者只在确定态
> 可见），而语义预览会跳过不可见目标。因此舞台放置了两个实例分别固定两种状态。若某一项高亮为空，说明该状态固定
> 失效，属缺陷。

### 步骤 2：Custom Semantic Part styling 示例

| # | 操作 | 预期现象 | 截图 |
| --- | --- | --- | --- |
| 2.1 | 滚动到 `Custom Semantic Part styling` 示例，确认卡片 Tag 显示当前 AtomUI 版本（`v6.1.9`） | 出现两个 Splash：`semantic-parts`（确定进度）与 `semantic-parts-root`（Success 状态） | 全景 |
| 2.2 | 观察 **semantic-parts** 的 Logo | 徽标为**实心圆角方块且可见尺寸就是 36×36**（示例覆盖值）；不出现「方块比承载区大、明显外溢」的情况 | 单个控件 + Logo 局部放大 |
| 2.3 | 观察 **semantic-parts** 的文本层级 | 主标题为紫色 `#531DAB` 且字号 **22**；副标题为紫色 `#722ED1`；消息为蓝色 `#1D39C4`；详情为青色 `#08979C` 且字号 **13** | 单个控件 |
| 2.4 | 观察 **semantic-parts** 的进度条 | 确定进度条高度为 **10**（默认 `ProgressBarHeight` 更细/不同），宽度铺满内容区 | 局部放大 |
| 2.5 | 观察 **semantic-parts-root**（Success 状态） | 表面背景为淡紫 `#F9F0FF`、圆角 **16**、内容内边距 **28,24**（明显比默认留白更大） | 单个控件 |
| 2.6 | 把窗口切到 Dark 主题后再观察 2.2–2.5 | 上述示例色值与尺寸仍生效（示例为显式值，不随主题变化） | 分组 |

### 步骤 2b：入口说明（防止误判）

- `logo` / `title` / `subtitle` / `content` / `message` / `detail` / `footer` / `spin` / `progressBar` 的定制走
  **生成的专用 Semantic Part Style**（`<atom:SplashTitleStyle x:SetterTargetType="TextBlock">` 等）。
- `root` 的定制走 **owner 作用域的普通 Setter**（`Background` / `CornerRadius` / `Padding`）——**不存在** `root`
  Part Style。走查时若这两条示例没有变化，说明对应入口未生效，属缺陷。

### 步骤 3：窗口启动页示例（本轮迁移重点，走「示例」页签）

| # | 操作 | 预期现象 | 截图 |
| --- | --- | --- | --- |
| 3.1 | 在「示例」页签点击「显示窗口 Splash」按钮 | 在屏幕中央出现专用启动窗口：深蓝→蓝紫→青的渐变背景、`A` 品牌标识、标题 `AtomUI`、进度与阶段文案、底部版本信息 | 全景（窗口可见时） |
| 3.2 | 观察标题与弱文本颜色 | **标题为纯白**；副标题与详情为浅蓝 `#D6E4FF` | 局部放大 |
| 3.3 | 观察加载阶段的普通消息颜色 | 加载阶段（`:loading`）消息为极浅蓝 `#F5F8FF` | 局部放大 |
| 3.4 | 等待示例推进到 Success 状态 | 消息颜色切换为**主题 SuccessColor**（不再是 `#F5F8FF`），即状态色语义未被普通消息定制覆盖 | 局部放大 |
| 3.5 | 观察窗口关闭 | 淡出后窗口正常关闭，主窗口回到前台并恢复输入；无残留窗口、无卡死 | 录屏 |

> 步骤 3.2–3.4 是本轮唯一改变实现路径的渲染点。迁移前用 `/template/ + PART_*` 选择器，迁移后用生成的专用
> Semantic Part Style；两者必须产生**完全相同的可观察颜色**。若颜色出现差异或 Success 状态色被 `#F5F8FF`
> 覆盖，说明迁移或优先级处理有缺陷。

### 步骤 4：「示例」页签回归抽查

| # | 操作 | 预期现象 | 截图 |
| --- | --- | --- | --- |
| 4.1 | 走查「基础」（Basic）示例 | 不确定态：标识、标题、副标题、消息、详情正常显示，加载指示器可见 | 全景 |
| 4.2 | 走查确定进度（Determinate）示例 | 确定态：进度条可见、不确定指示器隐藏；进度与文案正常 | 全景 |
| 4.3 | 走查「状态」（Status）示例（Success / Error 并排） | 两个 Splash 的消息颜色分别为主题成功色与错误色；底部文案正常 | 两个控件同框 |
| 4.4 | 走查「Logo、内容与页脚」（Composed）示例 | 自定义 Logo 模板、Tag 内容区、Footer 模板均正常排布，无溢出 | 全景 |
| 4.5 | 切换 Light/Dark 主题后重走 4.1–4.4 | 默认观感与改造前一致（marker 为 inert，不应改变任何默认值） | 分组 |

## 判定标准

- 步骤 1：10 个 Part 均可解析高亮；`root` 高亮包围整块启动页表面；两个实例上同名 Part 同时高亮；`spin` /
  `progressBar` 分别在不确定态与确定态实例上高亮。两个实例必须左右并排且**完整落在画布内**。

> **窗口尺寸建议覆盖多档**：该布局契约与视口尺寸相关，实测失效区间是「非 compact 且画布较窄」的中间宽度。
> 走查时请至少覆盖以下窗口尺寸各一次（步骤 1）：约 1920×1080（最大化）、1300×900（默认）、
> 1000×800、900×900。自动化已按 12 档视口断言，但真实 Skia 后端下仍建议至少复验其中 3 档。
- 步骤 2：生成的 9 个 Semantic Part Style 均命中目标；**Logo 可见尺寸 36×36**（注意是徽标本体的可见尺寸，不只是承载区）、标题 `#531DAB`/22、副标题 `#722ED1`、
  消息 `#1D39C4`、详情 `#08979C`/13、进度条高度 10、root 淡紫底 + 16 圆角 + 28,24 内边距必须全部生效**——
  这些是 owner API 无法表达、只能由 Semantic Part 提供的能力，也是本轮改造的核心价值。
- 步骤 3：窗口启动页渐变色、白标题、浅蓝弱文本、加载阶段消息色与 Success 状态色**与迁移前完全一致**；窗口正常
  关闭且无残留。
- 步骤 4：新增 inert marker 未改变既有示例的默认视觉；Light/Dark 下均一致。

## 证据要求

按上表回传截图或录屏。步骤 3.1–3.5 建议录屏（窗口淡出关闭与状态色切换是时序行为，静态截图难以取证）；
步骤 4 的「状态」示例（4.3）请同框截图便于对比状态色。

**图像不入库**：验收判定完成后仓库只保留本文字步骤与结论，不保存截图。

## 自动化已覆盖范围（不替代真机验收）

以下已由自动化断言，但仍需真机确认最终观感：

- descriptor 十部件的数量、顺序、`ContractType`、cardinality 与标志位；Extras 为首个采用 Semantic Part 的包，
  descriptor 已进入包级注册与冻结 registry。
- 模板 9 个静态 marker 的存在与节点类型，且不出现 `semantic-root` 与 `semantic-scope-*`；`SplashWindowTheme.axaml`
  不含任何 semantic marker，`SplashWindow` 不是 Semantic owner。
- 生成的 9 个 Semantic Style 各精确命中一个 owner-scoped 节点（不是 0 个也不是多个）。
- Part Setter 对 `LogoSize` / `ProgressBarHeight` / `Token` 前景投影的优先级覆盖；并单独断言 `logo` 的**可见徽标 `Bounds`** 等于 Part 设定值（防止模板内容自带固定尺寸造成的「presenter 正确但可见尺寸错误」）。
- 状态矩阵（默认 / 确定进度 / Success / Error / `Logo=null` / `Footer=null` / 模板重应用）下 marker 数量恒为 1/部件，
  且 `spin` 与 `progressBar` 的互斥可见性正确。
- 固定 `Width` + `MinHeight` 下内容可自然增长；给 `content` 设固定 `Height` 会破坏该基线（尺寸基线失败回归）。
- Gallery：预览仅在 Semantic Parts 页签物化、10 个说明路径全部解析、两个实例分别固定两种进度状态；专用 Style
  演示的 Setter 值真正落到目标节点；`GalleryWindowSplash` 的四个文本位前景色与 Success/Error 状态色。

**已知自动化局限：** headless 测试断言的是属性值与选择器命中，无法证明真实 Skia 后端下的最终观感——特别是
窗口透明/无装饰组合、渐变背景与阴影遮罩的实际呈现，以及窗口淡出关闭的时序观感。这些只能由真机截图/录屏确认，
即步骤 3 与步骤 4。
