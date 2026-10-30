# Steps Changelog

本文档记录 Steps 控件级设计、API、主题契约、Token 和实现结构的变化。
它不替代仓库根目录 CHANGELOG.md，也不作为正式版本发布说明。

## 2026-09-28

- Layout
  - 将水平 Steps 的自然尺寸请求与排列伸展宽度分开，自动尺寸宿主按内容所需宽度布局，固定宽度宿主仍保持伸展；保留窄宽压缩、图标下限和文本换行。
  - 自然宽度按普通布局的非末项等宽单元、末项内容宽度，以及 Navigation、Panel、垂直标题的等宽单元计算；Inline 前导 Offset 单元计入自然宽度，其他类型不消费 Offset。
  - 水平自然尺寸同时预留可见连接线的线段长度与两端间距，避免自动尺寸场景只有图标和标题、连接线宽度为零；线段仍可在窄布局中压缩。
- Tests
  - 覆盖自然尺寸下的连接线实际宽度，以及宽宿主自然测量、不等长 item、隐藏和动态变化，以及包含 Stretch Steps 的 Overlay Dialog 在不同宿主宽度下保持内容尺寸、居中位置、标题和完整遮罩。

## 2026-09-21

- Behavior
  - 垂直 Steps 中同时携带 SubHeader 与 Content 的 item 在 Windows/Linux 高 DPI 缩放下不再出现副标题与描述文字叠印：`StepsItemSectionPanel` 的 heading 同行/换行判定在测量与排列两处统一增加 0.01 容差（同 `StepsPanel` 份额宽度比较的既有容差惯例），消除「标题+副标题恰好占满区段宽度时，布局取整使排列宽度比重新求和小 1 ulp，两处判定翻转、换行副标题被排到内容行上」的缺陷；同行分支下副标题取完整期望宽度，容差缝隙不再引起二次换行。
- Layout
  - Inline hover 背景对齐 antd v6 `.ant-steps-item-wrapper` 契约（`style/inline.ts` + `style/index.ts`）：wrapper 为铺满 item 单元宽度的块，块方向 padding 契约（上 `paddingXS+lineWidth`、下 0）由既有 `InlineItemPadding` token 经主题绑定到 `StepsItemLayoutPanel.Padding` 提供，`ArrangeInlineItemWrapper` 直接消费该 Padding（与 `ArrangeContentItemWrapper` 同一几何契约来源，共用 `ComputeWrappedContentBounds` 内容并集计算），不再使用布局代码内的硬编码常量；背景带 `motionDurationMid` 过渡，不出现"铺满整格高度遮住相邻间距"或"紧贴内容过小"两种偏差。
  - Panel Filled 的非首项 leading notch 裁剪只在 `Type=Panel` 时生效（`StepsPanelItemFrame` 新增 `Type` 属性并由模板 TemplateBinding 接入）；其他类型（含 Inline、可点击 Default、Dot、OutlineDot、Navigation）的 hover/focus 背景不再被裁成带缺口的箭头形状。

## 2026-08-22

- Semantic Part
  - 为 `Steps` 公开 `root`、`item`、`itemWrapper`、`itemIcon`、`itemTitle`、`itemSubtitle`、`itemSection`、`itemContent` 和 `itemRail` 九个 Semantic Part，对齐 Ant Design 6 的 `StepsSemanticType`（since 6.2.0）；新增 `Steps.SemanticParts.cs` descriptor，生成 `StepsItemStyle`、`StepsItemWrapperStyle`、`StepsItemIconStyle`、`StepsItemTitleStyle`、`StepsItemSubtitleStyle`、`StepsItemSectionStyle`、`StepsItemContentStyle` 与 `StepsItemRailStyle`（`AtomUI.Theme.Styling`）。
  - `item` 为运行时标记（`RuntimeCreated`），由 `Steps` 在容器准备时为每个 `StepsItem` 写入 `semantic-item` marker；七个 item 子 Part 为 `StepsItemTheme.axaml` 内的静态标记，路由经 `> .semantic-item /template/ .semantic-item-x` 跨越 ItemsControl 容器边界进入 item 模板。
  - `itemSection` 的 `ContractType` 为 `Panel`（正文分组容器 `StepsItemSectionPanel` 的 public 基类），对应 antd `styles.itemSection` 的内容区布局与对齐定制面。
  - `itemRail` 的 `ContractType` 修正为 `DashedBorder`（Connector 节点 `PixelAlignedBorder` 的 public 基类）；原 `Border` 与节点类型不兼容，owner-scoped 解析永远拒绝 Connector，导致 Gallery 预览中 itemRail 高亮缺失。
  - 新增 `docs/controls/desktop/navigation/steps/semantic-part.md`，overview 与 implementation 同步链接。
- API
  - 新增 `StepsType.Panel`，并新增 `StepsPanelVariant.Filled` / `StepsPanelVariant.Outlined` 与 `Steps.PanelVariant`。
  - `Steps` 新增 `BorderDashArray` / `BorderDashOffset` 根框架属性，补齐 `styles.root` 的虚线边框定制面。
- Behavior
  - 水平 Steps 在可用宽度不足时连续压缩：item 按自然宽度比例收缩，触达 `IconContainerSize` 下限后停止并溢出容器；标题、副标题与描述文本随 item 收缩连续换行，不以裁剪代替换行。宽容器下既有伸展语义（非末 item 等额伸展、末 item 与单 item 内容宽）不变。
- Layout
  - Panel 强制水平等宽布局，忽略垂直 Orientation 请求。
  - 隐藏 Indicator 和普通 Connector，使用 internal `PanelArrow` 在相邻 item 之间绘制可拉伸楔形箭头，并支持 RTL。
- Implementation
  - `StepsPanel` 水平测量改为两遍：第一遍以无限宽度取得每个可见 item 的自然宽度作为 flex basis，第二遍按共享份额算法计算的宽度约束性重测，使文本节点换行并上报换行后的行高；测量与排列共用同一份额计算函数，保证排列宽度等于测量宽度。
  - `StepsPanel` 新增 internal `MinItemWidth` 属性（AffectsMeasure / AffectsArrange）作为 item 收缩下限；收缩时触底 item 冻结退出分配，其余 item 继续按自然宽度比例分摊剩余缺口。
  - `StepsItemLayoutPanel` 水平标题路径的 body 子项改为按排列时的 body 可用宽度（item 宽度 − indicator − spacing）测量，消除「份额落在文本自然宽度邻近区间时文本被裁成单行」的以裁剪代替换行缺陷。
  - `StepsItemLayoutPanel` heading 行的同行/换行决策由测量与排列共享同一判定条件：Header 与 SubHeader 并排放不下时，SubHeader 换到 Header 下方独占一行并保持测量宽度，消除「副标题被安排成 `body 宽度 − Header 宽度` 的剩余宽度而裁成 00:0」的裁剪缺陷；heading 高度随之增高，内容行整体下移。
  - `StepsItemLayoutPanel` 的正文区域（Header / SubHeader / Content）改为由 internal `StepsItemSectionPanel` 分组测量与排列：section 承担 heading 同行/换行决策与垂直标题布局对齐，向 layout panel 上报 heading 高度与标题行右缘；layout panel 以 section 为单一正文单元完成 indicator 侧布局与 Connector 端点定位，几何结果与重构前等价。
- Theme / Token
  - 新增 Panel 状态背景、active 背景、边框厚度、圆角和箭头尺寸 Token。
  - Filled 使用状态面板色，Outlined 使用容器背景、状态边框和浅色 active 背景；Small 使用独立箭头宽度和圆角。
  - `StepsItemTheme.axaml` 在 `ItemWrapper`、`PART_Indicator`、`HeaderPresenter`、`SubHeaderPresenter`、`Section`、`Connector` 和 `ContentPresenter` 上声明七个静态 `semantic-item-*` marker。
  - `StepsItemTheme.axaml` 将 `HeaderPresenter`、`SubHeaderPresenter` 与 `ContentPresenter` 包裹进 internal `StepsItemSectionPanel#Section`，作为 `itemSection` 的模板节点与正文分组布局容器。
  - `StepsTheme.axaml` 根模板在 `PART_ItemsPresenter` 外包裹 TemplateBind 根视觉属性的 `atom:DashedBorder`：`Background` / `BackgroundSizing` / `BorderBrush` / `BorderThickness` / `CornerRadius` / `Padding` 来自 `TemplatedControl`，`StrokeDashArray` / `StrokeDaskOffset` 来自 `BorderDashArray` / `BorderDashOffset`，使 `root` Part 视觉定制（含虚线边框）可渲染，默认值不改变既有外观。
  - `StepsItemIndicator` 删除 `OnSizeChanged` 中的代码层圆角写入（local value 优先级会阻止 Semantic Style）；新增 CornerRadius 类型 Token `IconContainerCornerRadius`，由 `StepsItemIndicatorTheme.axaml` 以 style 优先级设置默认全圆，使 `itemIcon` Part 的圆角可被 Semantic Style 覆盖。
  - `StepsTheme.axaml` 将 `StepsPanel.MinItemWidth` 绑定为 `IconContainerSize` token，作为水平 item 的收缩下限。
- Gallery
  - Showcase 迁移到 `GalleryShowCaseHost`，增加 `StepsSemanticPreview`（九个 Part 描述）以及 `steps-semantic-part` 语义样式示例：默认 Steps 与 Navigation Steps 共享虚线 root 边框，对象式样式统一圆角 item 图标与斜体详情，函数式样式（`Type=Navigation`）覆盖 root 边框色，整体对齐 antd style-class 示例；`StepsSemanticPreview` 的预览 Steps 使用垂直标题布局（`TitlePlacement=Vertical`），图标在上，标题、副标题与内容在图标下方居中堆叠，对齐 antd Semantic DOM 预览的排版。
- Docs
  - overview.md 新增 8.7 弹性压缩与文本换行专项模型：水平布局采用 flex 份额语义（非末项 `1 1 auto`、末项 `0 1 auto`、垂直标题 `1 1 0%`），收缩下限为 `IconContainerSize`；标题、副标题与描述文本在受限宽度下换行，测量与排列共用同一份额算法。
  - implementation.md 同步 8.2 两遍测量与共享份额算法、10 节压缩与换行维护不变量、11 节弹性压缩与文本换行验证清单。
- Tests
  - 增加 Panel 等宽、布局方向、Arrow 外溢、RTL、Filled/Outlined 和模板可见性回归覆盖。

## 2026-08-03

- API
  - 在 `Steps` 根控件定义 nullable `ItemHeaderForeground`、`ItemSubHeaderForeground` 和 `ItemRailBackground` 实例级语义样式属性；默认 `null` 保留当前状态和类型的 Token 视觉。
- Theme
  - 将三项公开语义值投影到 `StepsItem` internal StyledProperty，并仅由 `StepsItemTheme.axaml` 在自身模板边界内消费。
  - 禁止 Gallery 和外部样式依赖 `HeaderPresenter`、`SubHeaderPresenter`、`Connector` 或通过 `/template/` selector 穿透 `StepsItem`。
- Gallery
  - Inline style combination 通过 `Steps` 公开语义 API 表达，不再使用 class 和深层 selector 修改 item 内部节点。
- Docs
  - 同步根 API、容器投影生命周期、模板所有权和实例覆盖相对 StepsToken 的优先级契约。

## 2026-07-17

- API
  - 将 `Steps` 的基类从 `SelectingItemsControl` 改为 `ItemsControl`，删除 Selection、`CurrentContent` 和 `IsFinished` 契约。
  - 将根 API 统一为 `Current`、`Initial`、`Status`、nullable `Percent`、`Type` 和 `TitlePlacement`。
  - 将 `StepsStyle` 与 `StepsItemIndicatorType` 合并为 `StepsType`，将状态枚举统一为 `StepsStatus`。
  - 新增 `StepsType.OutlineDot`，作为与 `Dot` 共享布局的空心点状视觉类型。
  - 删除 `StepsItem.Description` / `DescriptionTemplate`，由 `Content` / `ContentTemplate` 表达步骤详情；`StepsItem.Status` 改为 nullable 显式覆盖。
  - 新增受控导航请求事件 `CurrentChangeRequested`；item 激活不直接修改 `Current`。
- Behavior
  - 采用 Ant Design 的 `Initial + index` 编号、Current 越界、item Status 覆盖和 Connector nextStatus 语义。
  - Wave 改为只响应真实 pointer click；程序化 Current、状态重算和 keyboard 激活不播放 Wave。
- Theme
  - 根、item 和 indicator 各使用一套统一语义模板，删除 Style、Orientation、Indicator 和 TitlePlacement 组合模板。
  - `OutlineDot` 使用透明背景和状态色边框，不播放 Indicator Wave。
  - 使用 internal `StepsPanel` 和 `StepsItemLayoutPanel` 分别承担 item 间和 item 内布局。
  - Navigation 当前项使用唯一 `NavigationActiveIndicator` 节点表达水平底线或垂直右侧线。
- Token
  - 删除冗余 `StepsProgressSize`；Progress 外径改为由 icon size 与 `ProgressFramePadding` 推导。
- Implementation
  - 将 EffectiveStatus 设为状态视觉唯一输入，删除 Current/Selection 双向同步和 Content observable 生命周期。
  - 容器状态改为无旧值依赖的确定性投影，并明确 prepare/index change/clear 的 owner 生命周期。
- Docs
  - 按 Ant Design 6.4.5 和 `@rc-component/steps` 1.2.2 更新架构、实现和 Token 文档。

## 2026-07-06

- API
  - 将 `CurrentStep` 设为默认 `TwoWay` 受控步骤状态。
- Implementation
  - `SelectedIndex` 变化回写 `CurrentStep`，让点击步骤和绑定源保持同步。
- Gallery
  - 在 Switch Step 示例中展示可点击步骤与 `CurrentStep` 绑定，标记为 `v6.0.8`。

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Steps`.
  - Align generated output paths with `controls/steps/index-cn.md` and `controls/steps/semantic-cn.md`.

## 2026-06-22

- Docs
  - 建立 Steps 控件文档目录，补齐 `overview.md`、`implementation.md`、`token.md` 和 `changelog.md`。
  - 记录 Steps 的公共契约、CurrentStep/Selection 状态流、Status 派生、CurrentContent、Default/Navigation/Inline 主题结构、Dot 指示器、进度环、Navigation 箭头槽位和验证策略。
  - 在 Navigation 分类入口中登记 Steps 文档。
