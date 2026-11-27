# PopupConfirm Semantic Part 改造 · 真机视觉验收步骤

> 状态：**通过**（用户回传截图驱动，两处问题已修复并经用户确认；2026-09-11 用户确认「解决了」）。
>
> ## 验收结论记录（文字证据）
>
> | 日期 | 证据 | 结论 | 修复记录 |
> | --- | --- | --- | --- |
> | 2026-09-11 | 用户截图：Placement 示例被压缩在半宽列内，5×5 网格拥挤换行 | 确认为缺陷（示例条目未独占整行，与 InfoFlyout 的 Placement 展示不一致） | 给 Placement 示例加 `IsOccupyEntireRow="True"`，示例布局变为 Basic/Locale 并排 → Placement 整行 → Customize icon/StyleClass 并排 |
> | 2026-09-11 | 用户截图（上游对照）：Function Style 示例标题仍为深色，上游为白色 | 确认为缺陷（`PopupConfirmContainerTheme.axaml` 在 `TextBlock#PART_Title` 静态设置 `SharedToken ColorTextHeading`，优先级高于 `popup.root` 的 `Foreground` 继承，仅靠根前景色无法改标题） | 在 Function 分支补 `PopupConfirmPopupTitleStyle` 显式 `Foreground=White`；新增运行时回归 `Title_Semantic_Style_Overrides_The_Default_Heading_Foreground` 锁定该机制 |
> | 2026-09-11 | 用户回话「解决了」（附截图：深色弹层标题与描述均为白色） | 上述两处修复的真机渲染经用户确认通过 | 无 |

## 背景

本次 PopupConfirm 语义部件改造对外开放 9 个 Semantic Part：`root`（隐式）+ 8 个 `popup.*`
（`popup.root` / `popup.container` / `popup.content` / `popup.arrow` / `popup.icon` / `popup.title` /
`popup.description` / `popup.actions`），与上游 Popconfirm 的语义 DOM 对齐。涉及两处可能影响渲染的变更：

1. `PopupConfirmContainerTheme.axaml` 新增 4 处 inert marker（`semantic-popup-icon` / `semantic-popup-title` /
   `semantic-popup-description` / `semantic-popup-actions`），均为静态 `Classes.semantic-*="True"`，仅提供
   样式命中点，不改默认视觉。
2. Gallery 页面顶部 Tabs 由 `GalleryStickyTabsHost` 换成 `GalleryShowCaseHost`，新增 Semantic Parts 页签与
   StyleClass 示例；弹层由 `PopupConfirmFlyout` 代码创建、跨视觉根，需在打开时把弹层根注册进
   `SemanticPartPreview.AdditionalRoots`。

四个弹层框体件（`popup.root` / `popup.container` / `popup.content` / `popup.arrow`）复用 `Flyout` /
`FlyoutPresenter` 的共享代码注入路径；四个确认体件为 `PopupConfirmContainer` 自身主题的静态 marker。

## 操作路径与判定标准

准备：运行 AtomUIGallery（`controlgallery/AtomUIGallery.Desktop`），进入
**Feedback（反馈）** 分类 → **PopupConfirm** 页面。

### 步骤 1：Semantic Parts 页签

| # | 操作 | 预期现象 | 截图 |
| --- | --- | --- | --- |
| 1.1 | 点击页面顶部 "Semantic Parts" 页签 | 页面切换到语义预览：出现一个触发按钮，确认弹层钉住常开、向上/向下不遮挡 Header 与 Tabs；右侧部件列表可滚动到底 | 全景 |
| 1.2 | 在预览右侧部件列表中逐个选中 9 个 Part（root / popup.root / popup.container / popup.content / popup.arrow / popup.icon / popup.title / popup.description / popup.actions） | 每个 Part 都有对应高亮框；四个框体件在跨视觉根的 `FlyoutPresenter` 上高亮，四个确认体件落在 `PopupConfirmContainer` 子树；高亮四边完整可见 | 逐部件或分组 |
| 1.3 | 取消选中、切回 Examples 再切回 Semantic Parts，并切换 Light/Dark 主题 | 高亮清除；重新进入预览正常重建；主题切换下弹层与高亮不残留 | 分组 |

### 步骤 2：Custom Semantic Part styling 示例

| # | 操作 | 预期现象 | 截图 |
| --- | --- | --- | --- |
| 2.1 | 滚动到 "Custom Semantic Part styling" 示例（SourceKey `popupconfirm-semantic-part`），卡片 Tag 显示当前版本号 | 出现 Object Style 与 Function Style 两个触发按钮 | 全景 |
| 2.2 | 点击 **Object Style** | 弹层为浅灰底（`#EEEEEE`）、内边距 16；标题与描述为深色（`#262626`） | 弹层展开 |
| 2.3 | 点击 **Function Style** | 弹层为半透明深蓝底（`#CC35477D`）、圆角 4、内边距 12、按钮间距 12；**标题与描述均为白色**，与上游一致 | 弹层展开 |
| 2.4 | 对照上游 Popconfirm `style-class` 示例 | 两个分支的底色、标题/描述颜色、内边距、圆角、按钮间距一致 | 上游对照 |

### 步骤 3：Examples 回归抽查

| # | 操作 | 预期现象 | 截图 |
| --- | --- | --- | --- |
| 3.1 | 走查 Basic usage、Locale text、Customize icon 三个示例 | 标题、描述、状态图标颜色（Info 蓝 / Warning 黄 / Error 红）、确认/取消按钮视觉与改造前一致 | 全景 |
| 3.2 | 走查 Placement 示例 | 示例独占一整行，12 个按钮的 5×5 网格完整展开、无挤压或换行错位 | 全景 |
| 3.3 | 逐个点击 Placement 的 12 个按钮 | 弹层按对应 placement 展开，箭头指向锚点，无遮挡或越界 | 分组 |

## 判定标准

- 步骤 1：9 个 Part 均可解析高亮，尤其跨视觉根的 `popup.*` → marker 注入与 `>>` 后代路由正确。
- 步骤 2：语义样式命中（灰底/深色标题；深蓝底/白色标题与描述）→ 生成 `PopupConfirmPopupContainerStyle` /
  `PopupConfirmPopupTitleStyle` 等跨视觉根命中目标；标题颜色必须由 `PopupConfirmPopupTitleStyle` 显式给出，
  否则被 `PopupConfirmContainerTheme` 的静态 `ColorTextHeading` 压过。
- 步骤 3：`PopupConfirmContainerTheme` 新增的 inert marker 未改变既有示例的默认视觉；Placement 整行布局生效。

## 证据要求

按上表回传截图或录屏。图像不入库：验收完成后仓库只保留本文字步骤与结论，不保存截图。
