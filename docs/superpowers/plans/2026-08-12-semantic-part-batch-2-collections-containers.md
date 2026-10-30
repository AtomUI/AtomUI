# Semantic Part 第二批集合与容器实施计划

> **供智能体执行者使用：** 使用 `superpowers:executing-plans` 在当前会话中执行，不得使用 subagent。每个控件在 Gate A 后停止等待用户批准，Gate B 改动保持未提交。

**目标：** 为 19 个集合、容器、布局和导航控件家族建立 Semantic Part 契约，同时保持容器、虚拟化和运行时节点性能。其中 16 个家族具有 Ant Design 6.6.0 公开 Semantic DOM 对应 API，`Expander`（映射上游 `Collapse`）、`TabStrip`（映射上游 `Tabs`）为 2026-09-15 用户指令新增，`GroupBox` 为 2026-09-16 用户指令新增且**没有**上游对应 owner（Part 从 AtomUI 自身模板职责设计）。

**架构：** 父级 owner 只公开自身稳定区域；public item/container 控件在适用时拥有自己的 Descriptor。运行时生成节点只在现有创建路径中使用生成常量添加 marker，并提供 prepare/clear/recycle 证据。

**技术栈：** .NET 10、Avalonia 12、AtomUI Desktop Controls、AXAML、xUnit v3、Avalonia Headless、AtomUI Gallery。

## 全局约束

- 遵循[全量改造总计划](2026-08-12-semantic-part-control-rollout.md)和[全量改造设计](../specs/2026-08-12-semantic-part-control-rollout-design.md)。
- 修改代码前必须通过 Gate A 文档审核；Gate B 改动保持未提交，直到用户批准。
- 不得将任意用户子元素标记为 `.semantic-item`；只有 owner 创建的稳定容器才能形成 item Part 契约。
- 容器 marker 必须为静态声明，或在构造时通过生成常量一次性添加；不得在 prepare、选择或状态变化期间切换。
- 适用时，测试必须证明集合 replace/reset、prepare/clear/recycle、嵌套 semantic owner 隔离，并且旧容器不会被保留。
- 共享源码或 Gallery 页面中如果同时承载被排除控件，只允许修改准入 owner 的路径；不得给 `TabStrip` 或其他
  被排除 owner 添加 Descriptor、marker 或 Semantic Preview。

---

### 任务 1：Calendar

**控件文档：** `docs/controls/desktop/data-display/calendar/overview.md`, `docs/controls/desktop/data-display/calendar/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Calendar/**/*.cs`, `src/AtomUI.Desktop.Controls/Calendar/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/Calendar`；Gallery `controlgallery/AtomUIGallery/ShowCases/DataDisplay/Calendar`.

**风险类型：** 运行时日历 cell、派生 LunarCalendar、导航 header、高密度重复节点。

- [ ] **Gate A 设计审核：** 审计 `Calendar`、`LunarCalendar`、internal CalendarView/Header/Cell 与 range bar 的 owner，确认 header/navigation、cell content、range/status 等职责能否跨 month/year/decade 和 lunar templates 保持；定义 cell builder 创建、mode 切换与可见日期范围的 cardinality。
- [ ] 更新两份控件文档，写明准确的 Descriptor、真实模板/运行时节点、owner 边界、排除的 internal wrapper、生命周期/性能不变量和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [ ] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/Calendar/CalendarSemanticPartTests.cs`，覆盖 Calendar/LunarCalendar、各 view mode、cell rebuild、range、header navigation、集合刷新和 marker owner；记录可见 cell marker 数与 mode 切换前后实例释放。
- [ ] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 测试、LLMS verify 和 `git diff --check`；涉及 Popup/运行时宿主路径时增加 NativeAOT 验证。
- [ ] **强制停止：** 保持 Calendar 的所有实现改动未提交，直到用户明确完成验证并授权提交。

### 任务 2：Collapse

**控件文档：** `docs/controls/desktop/data-display/collapse/overview.md`, `docs/controls/desktop/data-display/collapse/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Collapse/*.cs`, `src/AtomUI.Desktop.Controls/Collapse/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/Collapse`；Gallery `controlgallery/AtomUIGallery/ShowCases/DataDisplay/Collapse`.

**风险类型：** item 容器、展开/收起 motion、accordion 模式。

- [ ] **Gate A 设计审核：** 审计 `Collapse` 与 `CollapseItem` 的 owner，确认 item header/content/expand indicator 与 collection host 的职责；记录 data item 到 container 的创建、accordion selection、expand motion 和 clear/recycle 路径。
- [ ] 更新两份控件文档，写明准确的 Descriptor、真实模板/运行时节点、owner 边界、排除的 internal wrapper、生命周期/性能不变量和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [ ] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/Collapse/CollapseSemanticPartTests.cs`，覆盖 multiple/accordion、item add/remove/reset、header/content templates、expand/collapse/reopen 和 container clear；验证 repeated Part 数量与 owner scope。
- [ ] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 测试、LLMS verify 和 `git diff --check`；涉及 Popup/运行时宿主路径时增加 NativeAOT 验证。
- [ ] **强制停止：** 保持 Collapse 的所有实现改动未提交，直到用户明确完成验证并授权提交。

### 任务 3：ListView / ListBox

**控件文档：** `docs/controls/desktop/data-display/list-view/overview.md`, `docs/controls/desktop/data-display/list-view/implementation.md`, `docs/controls/desktop/data-display/list-box/overview.md`, `docs/controls/desktop/data-display/list-box/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/ListView/*.cs`, `src/AtomUI.Desktop.Controls/ListView/Themes/*Theme.axaml`, `src/AtomUI.Desktop.Controls/ListBox/*.cs`, `src/AtomUI.Desktop.Controls/ListBox/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/ListView`、`tests/AtomUI.Desktop.Controls.Tests/ListBox`；共享 Gallery `controlgallery/AtomUIGallery/ShowCases/DataDisplay/List`.

**风险类型：** 虚拟化、pagination、selection model、共享 Gallery、专用 `GroupHeaderItem` 容器类型与 recycle key 的正确性。

- [ ] **Gate A 设计审核：** 以 Ant Design 6.6.0 新增 `Listy` 组件的 `root` / `item` / `groupHeader` Semantic DOM 为上限，
  审计 `ListView` / `ListViewItem` / `ListBox` / `ListBoxItem` 中职责直接对应的稳定区域；不得据此把 selection、
  pagination、empty/loading、filter 高亮或普通 item content 扩大为公共 Part。同时记录 virtualized 容器生命周期、
  selection model、filter/pagination changes 和 nested content isolation。
- [x] **Gate A 审计结论（ListView，2026-08-17）：** 上游基线为 6.6.0 新增的 `Listy`（`components/listy`，旧 `List` 已
  deprecated）；Semantic DOM 为 `root`（根滚动容器）/ `item`（条目：内间距、分割线、悬浮背景）/ `groupHeader`（分组
  标题：吸顶与背景色），全部 6.6.0 公开，`index.tsx` 经 `useMergeSemantic` 实际消费。AtomUI 映射：`root` →
  `ListView` owner；`item` → 非分组 `ListViewItem` 容器；`groupHeader` → 专用 `GroupHeaderItem` 容器（internal，继承
  `ListViewItem`，构造时 `IsGroupItem=true`）。`.semantic-item` / `.semantic-group-header` 在两类容器构造时用生成
  常量一次性添加，不随状态切换；`CreateContainerForItemOverride` 按 `IGroupListItemData.IsGroupItem` 创建对应容器，
  recycle key 区分两类容器。`virtual` / `height` / `items` / `rowKey` / `group` 均有对应 API；`sticky` 与 `scrollTo`
  属行为功能差距，不在本轮范围。默认视觉对齐（条目分割线、分组标题背景/字重）随 Gate A 一并批准。证据已写入
  ListView overview.md / implementation.md §5.1 与设计 spec 映射表。
- [x] **Gate A 审计结论（ListBox，2026-08-17）：** 同一上游 Listy 基线；ListBox 无分组、无分页（轻量选择列表 +
  过滤 + CandidateList 基座），映射 `root`（`ListBox` owner，`Frame` + `PART_ScrollViewer` 滚动容器）与 `item`
  （`ListBoxItem` 容器，构造时一次性 `.semantic-item`，路由 `> .semantic-item`，`ContractType` 为公开
  `ListBoxItem`）；`groupHeader` 不适用（无分组功能，不虚构 Part）；排除 selection（`SelectedIndicator`）、filter
  高亮（`HighlightableTextBlock`）、empty。条目分割线默认视觉对齐与 ListView 同一决策一并批准。证据已写入
  ListBox overview.md / implementation.md §5.1 与设计 spec 映射表。
- [ ] 更新四份控件文档，写明准确的 Descriptor、真实模板/运行时节点、owner 边界、排除的 internal wrapper、生命周期/性能不变量和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [ ] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/ListView/ListViewSemanticPartTests.cs` 与 `tests/AtomUI.Desktop.Controls.Tests/ListBox/ListBoxSemanticPartTests.cs`，覆盖 `root` / `item` / `groupHeader` 三个 Part、分组开关与两类容器创建/recycle key、virtualized/nonvirtualized、pagination、empty/loading、selection、filter、replace/reset 和 recycle；记录 marker/container 数量与滚动前后 retained instance；Gallery Semantic Preview 对齐 Listy `_semantic.tsx`（Design/Engineering 分组数据），条目分割线与分组标题背景默认视觉基线生效。
- [ ] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 测试、LLMS verify 和 `git diff --check`；涉及 Popup/运行时宿主路径时增加 NativeAOT 验证。
- [ ] **强制停止：** 保持 ListView / ListBox 的所有实现改动未提交，直到用户明确完成验证并授权提交。

### 任务 4：Segmented

**控件文档：** `docs/controls/desktop/data-display/segmented/overview.md`, `docs/controls/desktop/data-display/segmented/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Segmented/*.cs`, `src/AtomUI.Desktop.Controls/Segmented/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/Segmented`；Gallery `controlgallery/AtomUIGallery/ShowCases/DataDisplay/Segmented`.

**风险类型：** item 容器、selected indicator geometry、SizeType。

- [x] **Gate A 设计审核：** 审计 `Segmented` 与 `SegmentedItem` owner、item icon/label 和 selected indicator/track 职责；记录 ItemsSource/容器生命周期、selection motion、block mode 和 SizeType。
- [x] 更新两份控件文档，写明准确的 Descriptor、真实模板/运行时节点、owner 边界、排除的 internal wrapper、生命周期/性能不变量和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。（2026-08-19：新增 `semantic-part.md`，Segmented 单一 owner 公开 `root`/`item`/`icon`/`label`，`SegmentedItem` 不持有 descriptor，选中滑块排除；LLMS verify 与 `git diff --check` 通过，等待批准。）
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/Segmented/SegmentedSemanticPartTests.cs`，覆盖 explicit/generated items、icon/text variants、selection changes、collection reset、block mode、所有尺寸 和 layout Setter。（2026-08-19：单一 owner `Segmented` 发布 `root`/`item`/`icon`/`label`，`SegmentedItem` 无 descriptor；14 个语义测试全绿。）
- [x] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 测试、LLMS verify 和 `git diff --check`；涉及 Popup/运行时宿主路径时增加 NativeAOT 验证。（2026-08-19：Generator 472/472、Segmented 24/24、GalleryBase 115/115、Gallery 486/486、LLMS verify 与 `git diff --check` 通过；全量 Desktop 2800/2802，仅有的 2 个 TabControl 失败在无改动基线上复现，为预存在问题，与 Segmented 无关。Segmented 无 Popup/运行时宿主路径，按计划条件不需要 NativeAOT 验证。）
- [ ] **强制停止：** 保持 Segmented 的所有实现改动未提交，直到用户明确完成验证并授权提交。

### 任务 5：Tag

**控件文档：** `docs/controls/desktop/data-display/tag/overview.md`, `docs/controls/desktop/data-display/tag/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Tag/*.cs`, `src/AtomUI.Desktop.Controls/Tag/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/Tag`；Gallery `controlgallery/AtomUIGallery/ShowCases/DataDisplay/Tag`.

**风险类型：** Tag 变体、close icon、checkable group/容器生命周期。

- [x] **Gate A 设计审核：** 分别审计 `Tag`、`CheckableTag`、group/items control 的 owner，确认 icon/content/close 与 checked indicator 是否共享或分离职责；记录 close lifecycle、group selection、container prepare/clear 和 SizeType/颜色 variants。
- [x] 更新两份控件文档，写明准确的 Descriptor、真实模板/运行时节点、owner 边界、排除的 internal wrapper、生命周期/性能不变量和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。（2026-08-19：新增 `semantic-part.md`，`Tag` owner 公开 `root`/`icon`/`content`/`close`，`CheckableTagGroup` owner 公开 `root`/`item`（Descriptions 同款 scope-items 跳点链），`CheckableTag` 无 descriptor；LLMS verify 与 `git diff --check` 通过，等待批准。用户补充：Gallery 语义展示要**两个** SemanticPartPreview——一个对应 `Tag` owner（root/icon/content/close），一个对应 `CheckableTagGroup` owner（root/item），对齐上游文档两个预览区块。）
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/Tag/TagSemanticPartTests.cs`，覆盖 basic/closable/checkable/group、集合变更、close/checked states、颜色和尺寸；验证 marker 不随状态切换增删。（2026-08-19：`Tag.SemanticParts.cs` + `CheckableTagGroup.SemanticParts.cs` 声明、TagTheme 三个静态 marker、GroupTheme scope-items 跳点、容器创建/prepare 幂等 marker 注入；15 个语义测试全绿，Tag 全量 69/69。TDD 发现并修正 Gate A 颜色边界描述：selector Style 的 `StyleTrigger` 优先级高于颜色状态机的 `Template` 写入，root 样式 setter 统一覆盖全部颜色分支——测试、semantic-part.md、implementation.md 与 changelog 已同步修正。用户复核视觉后又修正 `TagToken.DefaultBg`：antd v6 `defaultBg = colorFillTertiary.onBackground(colorBgContainer)`，AtomUI 原用 `ColorFillQuaternary` 浅一档，已改 `ColorFillTertiary` 并加回归测试。）
- [x] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 测试、LLMS verify 和 `git diff --check`；涉及 Popup/运行时宿主路径时增加 NativeAOT 验证。（2026-08-19 最终验证：Generator 472/472、GalleryBase.Generator 10/10、GalleryBase 115/115、Gallery 489/489、Desktop 2815/2817（仅有的 2 个 TabControl `TabActivationTests` 失败在隔离运行下 14/14 全绿，为预存在的加载顺序抖动，与 Tag 无关）；LLMS generate+verify 通过（78 控件 / 159 文件）；`git diff --check` 干净。Tag 无 Popup/运行时宿主路径，按计划条件不需要 NativeAOT 验证。Gallery 迁移 `GalleryShowCaseHost` + 两个 SemanticPartPreview（TagSemanticPreview / CheckableTagGroupSemanticPreview）+ `tag-semantic-part` 样式示例 + 8 个本地化 key + 快照/页面/高亮测试。）
- [ ] **强制停止：** 保持 Tag 的所有实现改动未提交，直到用户明确完成验证并授权提交。

### 任务 6：Timeline

**控件文档：** `docs/controls/desktop/data-display/timeline/overview.md`, `docs/controls/desktop/data-display/timeline/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Timeline/*.cs`, `src/AtomUI.Desktop.Controls/Timeline/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/Timeline`；Gallery `controlgallery/AtomUIGallery/ShowCases/DataDisplay/Timeline`.

**风险类型：** 重复 item、indicator/tail geometry、orientation 模式。

- [x] **Gate A 设计审核：** 审计 `Timeline` 与 `TimelineItem` owner，确认 item indicator、tail、label/content 的职责与 alternate/right/left/horizontal orientation 一致性；记录 pending item、reverse 和 runtime item generation。（2026-08-19 修订版：上游 `TimelineSemanticType` = `StepsSemanticType` 去掉 `itemSubtitle` 的**全部九个键**，antd master v6.4.5 依赖 `@rc-component/steps ~1.2.2`（npm latest=1.2.2，rc-steps `Step.tsx` DOM：`ol > li > wrapper > [icon, section > [header > [title, rail], content]]`，antd 自身 `semantic.test.tsx` 实锤九个 classNames 映射）。用户指示：Part 必须与上游一致，缺节点的**改造控件本身去支持**。修订结论：单一 owner `Timeline` 公开 9 个 Part。控件结构改造（internal、public API 不变）：新增 `TimelineSectionPanel` 承载原 `TimelineItemPanel` 的 Label/Indicator/Content 方向化 Measure/Arrange，新增 header 包裹节点，`TimelineIndicator` 由自绘 renderer 改为元素组合器——模板 `[Border#PART_Rail, Border#PART_Dot, Border#PART_IconHost [IconPresenter]]`，删除 DrawLine/DrawEllipse 与 Pen 缓存；rail 覆盖整条轴线、`IsFirst`/`IsLast` 裁剪到圆点中心、圆点/图标宿主（`ColorBgContainer` 背景、`:icon-present` 伪类切换）不透明掩膜出与原先一致的缺口，默认主题渲染像素级不变；用户覆盖 Timeline 背景时掩膜与上游 `dotBg` 行为对齐（上游对齐的微行为变化，已写入文档）。与上游两处结构差异已记录：rail 归轴线列（非 header 内，Avalonia Panel 只能排列直接子级）、图标宿主在 Indicator 内。9 个 Part 路由：`item` = `> .semantic-item`（ItemsControl 直接逻辑子级）；`itemWrapper`/`itemSection`/`itemHeader`/`itemTitle`/`itemContent` = `> .semantic-item /template/ .semantic-*`；`itemIcon`/`itemRail` = `> .semantic-item /template/ .semantic-indicator /template/ .semantic-*`（跳点非 Part）。ContractType：item=TimelineItem、itemWrapper/itemSection=Panel、itemHeader=StackPanel、itemTitle=TextBlock、itemContent=ContentPresenter、itemIcon=IconPresenter、itemRail=Border。`TimelineItem` 无独立 descriptor；Pending item 由 `CreatePendingItem` 生成（IsPending=true + LoadingOutlined 旋转），marker 语义与普通容器一致；`IsReverse` 只重算视觉顺序投影。上游 "Timeline Items" 逐项 classNames 区块无 AtomUI 对应物（item 级 Part 已 Multiple），Gallery 建议 1 个 SemanticPartPreview（9 卡片），待用户确认。全量 Generator 静态校验通过。）
- [x] 更新两份控件文档，写明准确的 Descriptor、真实模板/运行时节点、owner 边界、排除的 internal wrapper、生命周期/性能不变量和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。（2026-08-19 修订版：`semantic-part.md` 重写为 9 个 Part 完整契约（含结构改造与视觉保真保证、两处 DOM 差异、rail 掩膜几何、状态数量矩阵、验证清单），`overview.md` §8.4/§9 重写为 9 行 LLMS 表，`implementation.md` §3 职责/§5.1 角色图/§5.2 协作节点/§5.3 Semantic Part 处置/§8 算法/§9 资源/§11 测试全部同步，`changelog.md` 更新 Design/Docs 条目。LLMS verify 与 `git diff --check` 见下；等待批准。）
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/Timeline/TimelineSemanticPartTests.cs`，覆盖 orientations/modes、pending/reverse、自定义 dot、item add/remove/reset 和 重复 marker cardinality；同步重写 TimelineIndicatorTests（自绘几何断言 → rail/dot 元素 Arrange 断言，几何契约不变）。（2026-08-19：TDD 完成——RED 18 个失败全部按预期原因（结构缺失 / descriptor 缺失 / marker 缺失）失败后实现。`TimelineSectionPanel` 新建并承载原 `TimelineItemPanel` 的方向化 Measure/Arrange（label→header StackPanel 查找），`TimelineItemPanel` 改为纯填充 wrapper，`TimelineIndicator` 元素化（`PART_Rail`/`PART_Dot`/`PART_IconHost`/`PART_IconPresenter` + `:icon-present` 伪类，删除 Render/Pen 缓存，BorderThickness/CornerRadius 由 Arrange 计算），两个主题文件重写并加 6+2 个静态 marker 与 `semantic-indicator` 跳点，`Timeline.SemanticParts.cs` 声明 8 个 Part（root 隐式），容器创建/pending/Prepare 三路径幂等注入 `.semantic-item`。Timeline 全部 42 个测试全绿（含 13 个语义测试与 12 个指示器几何测试）；`TimelineThemeTests` 断言随结构更新为 Section，`TimelineItemPanelTests` 重写为 `TimelineSectionPanelTests`（槽位契约移到 header，title 保持自然尺寸，视觉不变）。Gallery：`GalleryShowCaseHost` 迁移 + 1 个 `SemanticPartPreview`（9 卡片，全部带 Label 保证各 Part 高亮资格）+ `timeline-semantic-part` 样式示例（8 个生成 `Timeline*Style`）+ 11 个本地化 key × 4 语言 + 页面/高亮测试 + 快照/目录基线/单测计数更新。TDD 发现并已写入文档：Label 为空时 header/title 自然尺寸为零，marker 保留但不满足高亮资格（与单 item 零尺寸 rail 同规则）。2026-08-19 追加（用户要求示例严格对齐上游 style-class demo）：`itemIcon` 承载从 `IconPresenter` 移到 `PART_Dot`/`PART_IconHost` 双 Border（上游 icon 元素即圆点，`itemIcon.borderColor` 可直接改圆环色）；水平 Label 布局对齐上游同侧模型（Start 轴线在上、Label/Content 居中在下，End 轴线在下）；Gallery 样式示例重写为上游 style-class demo 复刻（水平 #1890ff itemIcon + 垂直 #A294F9 边框盒/itemIcon，root Padding/CornerRadius/Border 语义样式；Frame 模板补绑 BorderThickness，默认 0 不影响默认视觉），示例文案遵循 Gallery 语义文案规范（Custom Semantic Part styling，不出现 dom/classNames——AtomUI 无 DOM 概念，Timeline/Alert/Empty 共 12 个 xlf 一并修正），Timeline 测试增至 44 个。追加（用户指出 antd Semantic DOM 有两个预览区块）：Gallery Semantic Parts 页签复刻上游双预览——`Timeline`（九卡，`_semantic` demo 的 4 项内容）与 `Timeline Items`（`_semantic_items` demo 的 2 项内容 + 九张短描述卡，沿用 FloatButton 多 preview 无节标题模式）；未改动 GalleryBase 公共控件（用户明确要求 showcase 层实现）。追加（用户指出 part 高亮框与上游不一致并给出 8 张对照截图）：垂直间距从 title/content 的 Padding 移到 item 的 Padding（对齐上游 li 的 paddingBottom，AbstractTimelineItem 按 Decorator 模式紧排模板根，末项去间距对齐 li:last-child），section 新增 AxisOverflow TemplateBinding 让指示器延伸到间距区保持 rail 连续；垂直 Label 布局 header/content 铺满列槽、文本朝轴线对齐（空 Label 的 header 保留整槽框、title 折叠为不可见，均与上游一致）——所有 part 的高亮框变为紧凑、逐项、有间隔的框；Timeline 测试增至 50 个。追加（用户指出 itemRail 仍是连续框并给出 antd 对照图）：rail 改为上游分段语义——每段从自身节点底边延伸到下一节点顶边（贯穿 item 边界、紧贴节点，视觉连续）、末项（含单 item）零尺寸，itemRail 高亮框变为逐项分段、节点处有间隔（预览一 3 框/预览二 1 框，与上游一致；垂直段紧贴节点无可见空隙——但初版仍有 3px 缺口，根因是 TemplatedControl/ContentControl 默认 ClipToBounds=true 把溢出 item 底边界的 rail 尾段切掉，TimelineIndicator 与 AbstractTimelineItem 已改为默认 ClipToBounds=false；水平方向 rail 保持贯穿相邻 item 的连续轴线——节点位于 item 中心，单段只能覆盖连接线一半，分段模型会漏掉后半段——首项从自身节点起、末项止于自身节点、中间项全宽贯通由节点掩膜遮盖）。追加修复：共享高亮装饰器在细窄目标上的退化描边——主标记 2px 画笔把 2px 宽 rail 的描边矩形压成零宽导致首段不渲染（首个目标恒为主标记），描边矩形下限改为画笔厚度，常规目标视觉不变。
- [x] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 测试、LLMS verify 和 `git diff --check`；涉及 Popup/运行时宿主路径时增加 NativeAOT 验证。（2026-08-19 最终验证：Generator 472/472、Gallery 491/491、GalleryBase 114/115（仅有的 1 个 `GalleryShowCaseHeaderTests` 失败在隔离运行下 10/10 全绿，为预存在抖动，与 Timeline 无关）、Desktop 2834/2836（仅有的 2 个 TabControl `TabActivationTests` 失败在隔离运行下 14/14 全绿，为预存在抖动，与 Timeline 无关）。Timeline 无 Popup/运行时宿主路径，按计划条件不需要 NativeAOT 验证。）
- [x] **强制停止：** 保持 Timeline 的所有实现改动未提交，直到用户明确完成验证并授权提交。（2026-08-19：所有改动保持未提交。）

### 任务 7：TreeView

**控件文档：** `docs/controls/desktop/data-display/tree-view/overview.md`, `docs/controls/desktop/data-display/tree-view/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/TreeView/**/*.cs`, `src/AtomUI.Desktop.Controls/TreeView/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/TreeView`；Gallery `controlgallery/AtomUIGallery/ShowCases/DataDisplay/TreeView`.

**风险类型：** 分层虚拟化、异步加载、item/header owner、drag adorner。

- [x] **Gate A 设计审核：** 审计 `TreeView`、`TreeViewItem`、`TreeViewItemHeader`、switcher 与 drag preview owner；确认 hierarchy item header/content/indent/check/switcher 的职责，记录 async children load、expand/collapse、filter、state replay、drag adorner 和 container recycle。（2026-08-19：双 owner 递归分解——`TreeView` 公开 root/item，`TreeViewItem` 公开 root/item/itemSwitcher/itemIndicator/itemIcon/itemTitle；itemIndicator 为 AtomUI 扩展，非上游 Semantic DOM 键。）
- [x] 更新两份控件文档，写明准确的 Descriptor、真实模板/运行时节点、owner 边界、排除的 internal wrapper、生命周期/性能不变量和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。（2026-08-19：`semantic-part.md` 新增六 Part 完整契约，overview/implementation/changelog 同步，LLMS verify 与 `git diff --check` 通过。）
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/TreeView/TreeViewSemanticPartTests.cs`，覆盖 generated hierarchy、async load、expand/collapse、check/select/filter、drag preview、replace/reset 和嵌套 owner 隔离；记录 marker 数量、回收行为，并证明不会保留旧节点。（2026-08-20：TDD 完成，7 个语义测试全绿；Gallery `GalleryShowCaseHost` 迁移 + SemanticPartPreview（TreeViewItem owner，含 checkbox/radio indicator、图标与选中态）+ `tree-view-semantic-part` 样式示例 + 本地化 key；修复 Gallery 高亮解析器对 owner 自身与嵌套 owner marker 的匹配（`SemanticPartTargetResolver`）；GalleryBase 119/119、TreeView 48/48、Gallery build 0 警告 0 错误。）
- [x] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 测试、LLMS verify 和 `git diff --check`；涉及 Popup/运行时宿主路径时增加 NativeAOT 验证。（2026-08-20：验证通过，见上；TreeView 无 Popup/运行时宿主路径，按计划条件不需要 NativeAOT 验证。）
- [ ] **强制停止：** 保持 TreeView 的所有实现改动未提交，直到用户明确完成验证并授权提交。（2026-08-20：用户已验收视觉，所有改动保持未提交，等待授权提交。）

### 任务 8：Slider

**控件文档：** `docs/controls/desktop/data-entry/slider/overview.md`, `docs/controls/desktop/data-entry/slider/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Slider/*.cs`, `src/AtomUI.Desktop.Controls/Slider/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/Slider`；Gallery `controlgallery/AtomUIGallery/ShowCases/DataEntry/Slider`.

**风险类型：** Track/thumb public 子控件、range thumb、pointer 热路径、ToolTip。

- [x] **Gate A 设计审核：** 审计 `Slider`、`SliderTrack`、`SliderThumb` owner，确认 rail/filled track/handle/marks/tooltip 的职责以及 single/range 与 horizontal/vertical templates；记录 drag hot path、tooltip Popup 和 multiple thumbs cardinality。（2026-08-20：上游基线为 antd 6.6.x `SliderSemanticType`（`classNames`/`styles` = `{ root?, tracks?, track?, rail?, handle? }`，root 徽标 5.23.0、其余 5.10.0），经 `useMergeSemantic` 传给 `@rc-component/slider 1.1.1` 实际消费：root=`ant-slider`、rail=`ant-slider-rail`、tracks=`ant-slider-tracks`（整体跨度、仅被定制时才渲染的条件节点）、track=每 segment `ant-slider-track`、handle=每 handle `ant-slider-handle`（圆点 `::after` box-shadow）。`_semantic.tsx` demo = `range defaultValue={[20,30,50]}` 五卡预览；`style-class.tsx` demo = 横向（root 宽 300、track 渐变 `#91caff→#1677ff`、handle `#1677ff` + shadow）+ 纵向 reverse（track 渐变 `#722cc0→#722ed1`、handle `#722ed1` + shadow、root hover 环 `#722ed1`）。AtomUI 结构改造（internal、public API 不变）：rail/tracks/track 由 `SliderTrack.Render` 自绘几何元素化为代码创建 `Border` 元素，mark 移入 internal `SliderMarksElement`，绘制顺序 rail→tracks→track→mark→thumb 与历史一致、默认视觉不变；`tracks` 单值模式跨度对齐上游 `Minimum→Value`（此前单值模式不绘制）。结构差异已记录：上游 tracks 条件节点 vs AtomUI 恒渲染（默认透明）；上游 mark 位于 handle 之上 vs AtomUI mark 在 thumb 之下；box-shadow → OutlineBrush/OutlineThickness。五个 Part 全部 RuntimeCreated=true，route 均为单跳 `/template/ .semantic-*`（节点设置外层 Slider 为 TemplatedParent，与既有 thumb 一致）。)
- [x] 更新两份控件文档，写明准确的 Descriptor、真实模板/运行时节点、owner 边界、排除的 internal wrapper、生命周期/性能不变量和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。（2026-08-20：新增 `semantic-part.md`（五 Part 完整契约，含元素化保真保证、三处 DOM 差异、状态数量矩阵、验证清单），overview.md §3/§5/§9 与 implementation.md §1/§2/§3/§4/§5/§6/§8/§10/§11 全部同步，changelog.md 更新 Design/Docs 条目。LLMS verify 与 `git diff --check` 见下；等待批准。）
- [ ] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/Slider/SliderSemanticPartTests.cs`，覆盖 single/range、orientation、marks、tooltip open-close、min/max/value updates 和 drag；验证 marker 静态、thumb cardinality 与 selector owner。
- [ ] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 测试、LLMS verify 和 `git diff --check`；涉及 Popup/运行时宿主路径时增加 NativeAOT 验证。
- [ ] **强制停止：** 保持 Slider 的所有实现改动未提交，直到用户明确完成验证并授权提交。

### 任务 9：Masonry

**控件文档：** `docs/controls/desktop/layout/masonry/overview.md`, `docs/controls/desktop/layout/masonry/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Masonry/*.cs`, `src/AtomUI.Desktop.Controls/Masonry/Themes/MasonryTheme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/Masonry`；Gallery `controlgallery/AtomUIGallery/ShowCases/Layout/Masonry`.

**风险类型：** 仅布局倾向、item 生成、可能重分类为不适用。

- [ ] **Gate A 设计审核：** 审计 Masonry owner 与 `MasonryPanel`，判断是否存在 owner 自有的非 root 视觉职责；不能把任意用户 child 虚构为 `.semantic-item`，除非控件实际拥有稳定生成 container。记录 span/layout changes 的真实 ownership。
- [ ] 更新两份控件文档，写明准确的 Descriptor、真实模板/运行时节点、owner 边界、排除的 internal wrapper、生命周期/性能不变量和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [ ] **Gate B 实现与验证：** 若 Gate A 批准 owner-created 非 root Part，新增 `tests/AtomUI.Desktop.Controls.Tests/Masonry/MasonrySemanticPartTests.cs` 覆盖 add/remove/reset、span changes 与 layout pass；若只有 root 职责，则将 Masonry 重分类为不适用并跳过实现。
- [ ] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 测试、LLMS verify 和 `git diff --check`；涉及 Popup/运行时宿主路径时增加 NativeAOT 验证。
- [ ] **强制停止：** 保持 Masonry 的所有实现改动未提交，直到用户明确完成验证并授权提交。

### 任务 10：Space

**控件文档：** `docs/controls/desktop/layout/space/overview.md`, `docs/controls/desktop/layout/space/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Space/*.cs`, `src/AtomUI.Desktop.Controls/Space/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/Space`；Gallery `controlgallery/AtomUIGallery/ShowCases/Layout/Space`.

**风险类型：** 布局容器、CompactSpace wrapper/add-on、共享 child ownership。

- [ ] **Gate A 设计审核：** 分别审计 `Space` 与 `CompactSpace`，判断普通 Space 是否应重分类为不适用；对 `CompactSpaceItem`、add-on/filler 和 generated wrapper 明确 owner、runtime creation、corner/border coordination，禁止把任意 child 视为 owner Part。
- [ ] 更新两份控件文档，写明准确的 Descriptor、真实模板/运行时节点、owner 边界、排除的 internal wrapper、生命周期/性能不变量和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [ ] **Gate B 实现与验证：** 对 Gate A 批准的 owner 新增 `tests/AtomUI.Desktop.Controls.Tests/Space/SpaceSemanticPartTests.cs`，覆盖 CompactSpace generated wrappers、add/remove/reset、orientation、filler/add-on、child 替换 与布局/圆角协调；任何只有 root 职责的 owner 重分类为不适用，不生成 descriptor。
- [ ] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 测试、LLMS verify 和 `git diff --check`；涉及 Popup/运行时宿主路径时增加 NativeAOT 验证。
- [ ] **强制停止：** 保持 Space 的所有实现改动未提交，直到用户明确完成验证并授权提交。

### 任务 11：Splitter

**控件文档：** `docs/controls/desktop/layout/splitter/overview.md`, `docs/controls/desktop/layout/splitter/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Splitter/*.cs`, `src/AtomUI.Desktop.Controls/Splitter/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/Splitter`；Gallery `controlgallery/AtomUIGallery/ShowCases/Layout/Splitter`.

**风险类型：** 生成的 panel、drag handle/bar、pointer 热路径。

- [ ] **Gate A 设计审核：** 审计 `Splitter`、`SplitterPanel`、`SplitterHandle`、`SplitterDragBar` owner，确认 pane content 与 generated handle/drag bar 的职责；记录 panel add/remove、collapsible state、orientation 和 resize drag lifecycle。
- [ ] 更新两份控件文档，写明准确的 Descriptor、真实模板/运行时节点、owner 边界、排除的 internal wrapper、生命周期/性能不变量和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [ ] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/Splitter/SplitterSemanticPartTests.cs`，覆盖 generated handle、多 pane、collapse、orientation、drag start/cancel/release、集合变更和 marker 数量；验证 drag 热路径不发生 class 变更。
- [ ] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 测试、LLMS verify 和 `git diff --check`；涉及 Popup/运行时宿主路径时增加 NativeAOT 验证。
- [ ] **强制停止：** 保持 Splitter 的所有实现改动未提交，直到用户明确完成验证并授权提交。

### 任务 12：Breadcrumb

**控件文档：** `docs/controls/desktop/navigation/breadcrumb/overview.md`, `docs/controls/desktop/navigation/breadcrumb/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Breadcrumb/*.cs`, `src/AtomUI.Desktop.Controls/Breadcrumb/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/Breadcrumb`；Gallery `controlgallery/AtomUIGallery/ShowCases/Navigation/Breadcrumb`.

**风险类型：** 生成的 item 容器、separator、collapsed menu。

- [ ] **Gate A 设计审核：** 审计 `Breadcrumb` 与 `BreadcrumbItem` owner，确认 item content/icon/separator、collapsed ellipsis/menu 等职责；记录 data items/容器生命周期、maximum display count、navigation 和 Popup/Flyout 如存在。
- [ ] 更新两份控件文档，写明准确的 Descriptor、真实模板/运行时节点、owner 边界、排除的 internal wrapper、生命周期/性能不变量和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [ ] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/Breadcrumb/BreadcrumbSemanticPartTests.cs`，覆盖 explicit/data items、separator templates、collapsed state、add/remove/reset、navigation 和 popup open-close 适用时。
- [ ] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 测试、LLMS verify 和 `git diff --check`；涉及 Popup/运行时宿主路径时增加 NativeAOT 验证。
- [ ] **强制停止：** 保持 Breadcrumb 的所有实现改动未提交，直到用户明确完成验证并授权提交。

### 任务 13：Pagination

**控件文档：** `docs/controls/desktop/navigation/pagination/overview.md`, `docs/controls/desktop/navigation/pagination/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Pagination/*.cs`, `src/AtomUI.Desktop.Controls/Pagination/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/Pagination`；Gallery `controlgallery/AtomUIGallery/ShowCases/Navigation/Pagination`.

**风险类型：** 生成的 nav item、多个 public 变体、内部 ComboBox Popup、SizeType。

- [ ] **Gate A 设计审核：** 审计 `Pagination`、`SimplePagination`、nav/nav item、quick jumper 和 page-size ComboBox item 的 owner，
  确认 previous/next/page/ellipsis/size changer/quick jump regions；记录 page 数量 rebuild、simple/default templates、Popup 和
  SizeType。ComboBox 作为嵌套控件保持独立：`Pagination` 不得获得 ComboBox 的 Descriptor，也不得在 ComboBox 的模板节点上
  打内部 marker；ComboBox 自身已于 2026-09-16 独立撤销排除并持有自己的语义契约，但嵌套 owner 隔离要求不因此改变。
  （2026-09-16 修订：原文依据「ComboBox 是被排除控件」，该依据已失效，隔离要求本身继续有效。）
- [ ] 更新两份控件文档，写明准确的 Descriptor、真实模板/运行时节点、owner 边界、排除的 internal wrapper、生命周期/性能不变量和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [ ] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/Pagination/PaginationSemanticPartTests.cs`，覆盖 default/simple、page 数量与 value 变化、生成的 nav item 生命周期、size changer Popup、quick jumper、所有尺寸和 marker 数量。
- [ ] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 测试、LLMS verify 和 `git diff --check`；涉及 Popup/运行时宿主路径时增加 NativeAOT 验证。
- [ ] **强制停止：** 保持 Pagination 的所有实现改动未提交，直到用户明确完成验证并授权提交。

### 任务 14：Steps

**控件文档：** `docs/controls/desktop/navigation/steps/overview.md`, `docs/controls/desktop/navigation/steps/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Steps/*.cs`, `src/AtomUI.Desktop.Controls/Steps/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/Steps`；Gallery `controlgallery/AtomUIGallery/ShowCases/Navigation/Steps`.

**风险类型：** 生成的 item、indicator public 子控件、navigation 模式、orientation。

- [ ] **Gate A 设计审核：** 审计 `Steps`、`StepsItem`、`StepsItemIndicator`、navigation arrow 和 panel owners；确认 title/subtitle/description/icon/tail/indicator regions 覆盖 default/navigation/inline 和 horizontal/vertical modes。
- [ ] 更新两份控件文档，写明准确的 Descriptor、真实模板/运行时节点、owner 边界、排除的 internal wrapper、生命周期/性能不变量和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [ ] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/Steps/StepsSemanticPartTests.cs`，覆盖 所有模式/orientations、status/current changes、item 集合变更、custom indicator 和 重复 marker/cardinality。
- [ ] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 测试、LLMS verify 和 `git diff --check`；涉及 Popup/运行时宿主路径时增加 NativeAOT 验证。
- [ ] **强制停止：** 保持 Steps 的所有实现改动未提交，直到用户明确完成验证并授权提交。

### 任务 15：TabControl

**控件文档：** `docs/controls/desktop/navigation/tab-control/overview.md`, `docs/controls/desktop/navigation/tab-control/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/TabControl/*.cs`, `src/AtomUI.Desktop.Controls/TabControl/Themes/BaseTabControlTheme.axaml`, `CardTabControlTheme.axaml`, `TabControlTheme.axaml`, `BaseTabItemTheme.axaml`, `CardTabItemTheme.axaml`, `TabItemTheme.axaml` 和共享 scroll/overflow themes；测试 `tests/AtomUI.Desktop.Controls.Tests/TabControl`；Gallery `controlgallery/AtomUIGallery/ShowCases/Navigation/TabControl`.

**风险类型：** 生成的容器、content presenter、overflow Popup、reorder、与被排除 TabStrip 共享源码。

- [ ] **Gate A 设计审核：** 审计 `TabControl`/`CardTabControl`、`TabItem`、scroll viewer/overflow item owners，确认 tab header/icon/close/content/ink/overflow regions；记录 selection、overflow Popup、reorder 和容器生命周期。共享 BaseTab 主题中的改动必须只命中
  `TabControl` / `TabItem` owner，不得给 `TabStrip` 添加 Descriptor、marker 或通过共享模板间接形成 Semantic Part。
- [ ] 更新两份控件文档，写明准确的 Descriptor、真实模板/运行时节点、owner 边界、排除的 internal wrapper、生命周期/性能不变量和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [ ] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/TabControl/TabControlSemanticPartTests.cs`，覆盖 line/card、generated/explicit items、content 切换、overflow popup、close/reorder、collection reset 和 nested owner 隔离。
- [ ] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 测试、LLMS verify 和 `git diff --check`；涉及 Popup/运行时宿主路径时增加 NativeAOT 验证。
- [ ] **强制停止：** 保持 TabControl 的所有实现改动未提交，直到用户明确完成验证并授权提交。

### 任务 16：Expander（2026-09-15 用户指令新增，原排除判定撤销）

**控件文档：** `docs/controls/desktop/data-display/expander/overview.md`、`docs/controls/desktop/data-display/expander/implementation.md`、`docs/controls/desktop/data-display/expander/semantic-part.md`（Gate A 新增）

**证据范围：** `src/AtomUI.Desktop.Controls/Expander/**/*.cs`、`src/AtomUI.Desktop.Controls/Expander/Themes/ExpanderTheme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/Expander`（新增 `ExpanderSemanticPartTests`）；Gallery `controlgallery/AtomUIGallery/ShowCases/DataDisplay/Expander`。

**风险类型：** 单面板控件没有运行时容器与回收路径（与 Collapse 的容器/prepare/clear/recycle 证据要求不同）；四向展开下 Header 位于 `LayoutTransformControl#PART_HeaderLayoutTransform` 内，需验证单一 `/template/` 路由仍成立；content motion 的尺寸动画与固定布局 Setter 存在优先级竞争；与 Collapse 共享 Core 内容展开机制。

- [x] **Gate A 设计审核：** 将 `Expander` 作为单面板折叠容器映射到上游 `Collapse` 已公开的五个 Semantic DOM 键（`root` / `header` / `icon` / `title` / `body`），不新增上游 owner；确认五个 Part 全部是 `ExpanderTheme.axaml` 单一模板内的静态节点（`RuntimeCreated=false`、无显式 `SelectorRoute`、无 `.semantic-scope-*` 锚点、无跨视觉根 Part）；确认 `PART_Frame`、`PART_MainLayout`、`PART_HeaderLayout`、`PART_HeaderLayoutTransform`、`PART_ContentMotionActor` 等模板结构节点、`PART_ContentMotionActor` 内未命名分隔线 border、`PART_AddOnContentPresenter`、动效 actor，以及根表面背景/圆角/内边距都不进入契约。（2026-09-15：准入依据经用户确认为「对齐 Collapse 五部件」——上游 owner 数量不是准入必要条件，第 4 条准入条件检验的是产品职责对应关系。）
- [x] 完成 `overview.md`、`implementation.md` 与新增 `semantic-part.md`，记录五个 Part 的 selector、ContractType、cardinality、静态 marker 位置、尺寸基线与失败回归、定制边界（含根表面 `Background` / `CornerRadius` / `Padding` 不从 owner 属性投影这一真实差异）和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。（2026-09-15：新增 `semantic-part.md`（五 Part 完整契约、模板节点映射、状态数量矩阵、定制边界与验证清单），`overview.md` §3.5 与 `implementation.md` §2/§4.1/§9/§10 同步；LLMS verify 与 `git diff --check` 通过；用户批准后进入 Gate B。）
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/Expander/ExpanderSemanticPartTests.cs`，覆盖 descriptor 五部件的数量/顺序/字段、四个静态 marker 的存在与节点类型、生成的 `ExpanderHeaderStyle` / `ExpanderIconStyle` / `ExpanderTitleStyle` / `ExpanderBodyStyle` 各精确命中一个节点、展开/收起/方向/尺寸档/图标位置/触发模式/视觉模式/自定义 padding/disabled 切换后命中数量不变，并验证 `header` 位于 `PART_HeaderLayoutTransform` 内时仍由单一 `/template/` 路由命中（不得退化为宽泛 logical descendant）；Gallery 页面新增 Semantic Parts Tab 与 SemanticStyles 示例。（2026-09-15：`Expander.SemanticParts.cs` 声明五 Part，`ExpanderTheme.axaml` 增加四个静态 `.semantic-*` marker，`ExpanderSemanticPartTests` 11/11 全绿。TDD 关键发现：`body` 的承载节点是 `PART_ContentMotionActor`（`LayoutAwareMotionActor`）内的 `PART_ContentPresenter`，折叠稳定态下 actor `IsVisible=False`，其 `ContentPresenter` 不挂接视觉子级（实测 `visualChildren=0`、`logicalChildren=1`），节点从视觉树完全缺席；首次展开后物化，再次收起时 Avalonia `ContentPresenter` 不解除挂接故节点保持存在。因此 `body` 声明 `Cardinality=Optional`（0 或 1 个静态 marker），三阶段语义（从未展开 0 / 首次展开 1 / 再次收起仍 1 且 actor 不可见）已同步到 descriptor、`semantic-part.md` §1/§2.5/§4/§7、`overview.md` §3.5、`implementation.md` §4.1/§9/§10 与测试断言；`header` 位于 `LayoutTransformControl#PART_HeaderLayoutTransform` 内仍由单一 `/template/` 路由命中，未退化为宽泛 logical descendant（已由测试证明，非源码推断）。Gallery：`ExpanderShowCase.axaml` 由 `GalleryStickyTabsHost` 迁移为 `GalleryShowCaseHost` + `SemanticPartsContentTemplate`（1 个 `SemanticPartPreview`、5 张 Part 描述卡、预览 `IsExpanded="True"` 保证 Optional 的 `body` 已物化）+ `SourceKey="expander-semantic-part"` 样式示例（`ExpanderHeaderStyle`×2 / `ExpanderTitleStyle`×2 / `ExpanderBodyStyle`×1 / `ExpanderIconStyle`×1，含 `x:SetterTargetType`）+ 7 个本地化 key × 4 语言（文案显式拒绝 React/DOM 专属术语）；`ExpanderShowCasePageTests` 增至 5 个测试，新增页签惰性物化与样式值生效两个运行时测试。同步更新 `CatalogMemberOrder.baseline`（Expander 行插入 7 个成员，位置与 Collapse 基线一致）与 `GalleryCatalogCoverageTests` 单元计数 4725→4732。）
- [x] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 和 Gallery 测试、LLMS verify 以及 `git diff --check`；若 Gallery 语义预览涉及运行时宿主路径则增加 NativeAOT 验证。（2026-09-15：Generator 528/528、Desktop 4025/4025、GalleryBase 185/185、Gallery 644/644；LLMS generate + verify 通过（79 控件 / 161 文件）；`git diff --check` 干净。Expander 无 Popup/运行时宿主路径，按计划条件不需要 NativeAOT 验证。）
- [x] **真机视觉验证与缺陷修复：** 用户验收 Gallery 时发现圆角处背景未被正确裁剪。（2026-09-15：用临时探针测试实测确认根因——`PART_Frame` 只把圆角画在自己身上，而 `PixelAlignedBorder` 的 `ClipToBounds` 只做**矩形**裁剪、`PART_Frame` 未开启 `ClipContentToCornerRadius`，导致紧贴左上角 `(0,0)`、自身 `CornerRadius=0` 且带背景的 `PART_HeaderDecorator` 把圆弧覆盖成方角。触发条件是 header 背景必须不透明：默认主题 `HeaderBg = ColorFillAlter` 实测 alpha≈2% 几乎不可见，`IsGhostStyle`（header 背景为完全不透明的 `ColorBgContainer`）与本次 Gallery 语义示例的不透明 `#f0f0f0` / `#f5efff` 都会显形——因此该缺陷在本次改造之前即已存在，`git diff` 确认本次对 `ExpanderTheme.axaml` 的改动只有 4 行新增 `Classes.semantic-*="True"`，不影响布局与绘制。对照组：`Collapse` 无此问题，`CollapseItemTheme.axaml` 给 header/content 各自绑了由 `Collapse.ConfigureItemCorners`（`Collapse.cs:414`）按 first/last 计算的 `HeaderCornerRadius` / `ContentCornerRadius`，注释明确目的是让子节点背景「follow the container's rounded border instead of covering it」。修复方案 A（用户确认）：`PART_Frame` 增加 `ClipContentToCornerRadius="True"`，由根节点统一把整棵子树裁到圆角内框——一行改动、与展开状态/方向/视觉模式无关、同时覆盖 header 与 body 两侧，与 `ListBoxTheme` / `ListViewTheme` 既有用法一致；未采用方案 B（复制 Collapse 的逐 corner 计算，Expander 是单面板控件、没有 item 索引语义，且折叠态 header 需要四角圆、展开态只需上两角，会引入状态依赖）。回归测试：`ExpanderBehaviorTests` 新增 `Frame_Enables_Content_Clip_To_Corner_Radius`（5 组 Theory 覆盖展开/收起 × Ghost × Borderless，断言 `ClipContentToCornerRadius` 为 `true`、`CornerRadius` 非零、且 header 左上角落在圆角方形区域内即该裁剪确实承担遮挡职责）与 `Frame_Corner_Radius_Comes_From_The_Theme_And_Stays_Non_Degenerate`。测试限制已确认并写入文档：headless 测试平台的几何包含性无法表示圆角图形，`DashedBorder.UpdateClip` 会按设计把裁剪降级为**不应用**（`DashedBorderClipContentTests.Clip_Is_Not_Applied_When_The_Platform_Cannot_Hit_Test_The_Rounded_Figure` 专门断言该行为），因此 `Child.Clip` 与 `Geometry.FillContains` 在测试宿主中均不可用，自动化只能断言模板契约与几何前置条件。文档同步：`semantic-part.md` §2.1（新增裁剪说明段落）、§6（新增「圆角裁剪的可见前提」条目）、§7（新增裁剪验证条目）；`implementation.md` 新增 §4.2「圆角裁剪（Part 背景不溢出圆角）」、§9 维护不变量、§10 验证范围；`overview.md` 模板节点表与验证策略表。复验：Desktop 4030/4031（唯一失败的 `DialogPopupControlFamilyTests.PopupConfirm_In_Dialog_Confirms_And_Closes_Its_Flyout` 隔离复跑 23/23 全绿，为预存在加载顺序抖动，与本次改动无关）、Generator 528/528、GalleryBase 185/185、Gallery 644/644、LLMS generate + verify 通过、`git diff --check` 干净。**真机视觉验收（2026-09-15）：用户在 Gallery 桌面宿主（真实 Skia 后端）确认圆角裁剪已正确生效，视觉验收通过。**）
- [x] **强制停止：** 保持 Expander 的所有实现改动未提交，直到用户验证真实宿主行为并明确授权提交。（2026-09-15：Gate B 与真机视觉验收完成后，用户明确授权提交（Gate C）；已创建单个控件家族提交 `be7b8dc71`「feat(Semantic): 增加 Expander 语义部件并修复圆角裁剪失效」，涵盖 Semantic Part 改造与圆角裁剪修复，未推送。）

### 任务 17：TabStrip（2026-09-15 用户指令撤销排除并追认）

**控件文档：** `docs/controls/desktop/navigation/tab-strip/overview.md`、`docs/controls/desktop/navigation/tab-strip/implementation.md`、`docs/controls/desktop/navigation/tab-strip/semantic-part.md`

**证据范围：** `src/AtomUI.Desktop.Controls/TabControl/TabStrip/**/*.cs`、`src/AtomUI.Desktop.Controls/TabControl/TabStrip/Themes/**/*.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/TabControl`（`TabStripSemanticPartTests`）；Gallery `controlgallery/AtomUIGallery/ShowCases/Navigation/TabStrip`。

**风险类型：** 三个独立 public owner（`TabStrip` / `CardTabStrip` / `TabStripItem`）共享 `ControlTheme` 资源边界；`item` 为运行时创建的容器 marker，`add` / `close` / `icon` / `label` 为模板静态 marker；独立页签条不承载内容页。

**范围说明：** 本家族原被设计文档 §2.4 以「Ant Design 只在 `Tabs` owner 上公开 API，没有独立 `TabStrip` owner」为由排除。该理由检验的是上游 owner 数量，而设计文档 §2.1 第 4 条要求的是产品职责直接对应；`TabStrip` / `CardTabStrip` 是独立 public owner 的页签条、`TabStripItem` 是二者的 item container，与上游 `Tabs` 映射成立。经用户指令撤销排除并追认——实现此前已随 `TabControl` 家族一并完成，本次补齐范围记录。

- [x] **Gate A 设计审核：** 确认为三个独立 public owner 分别建立 descriptor：`TabStrip`（`root` / `item`）、`CardTabStrip`（`root` / `add` / `item`）、`TabStripItem`（`root` / `close` / `icon` / `label`）；确认 `BaseTabStrip`、`TabStripOverflowMenuItem`、`TabStripScrollViewer`、`TabsContainerPanel` 是基类或 internal 协作类型，不持有独立 descriptor；确认独立页签条不公开 `content` Part。（2026-09-14：已完成并落地于 `docs/controls/desktop/navigation/tab-strip/semantic-part.md`。）
- [x] 完成 `overview.md`、`implementation.md` 与 `semantic-part.md`，记录三个 owner 的 Part、selector、ContractType、cardinality、marker 放置位置（运行时容器注入 vs 模板静态声明）、owner 边界与定制风格入口；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。（2026-09-14：三份文档齐备，`docs/AI/generated` 由生成器重生成，随 `TabControl` 家族提交一并提交。）
- [x] **Gate B 实现与验证：** `TabStrip.SemanticParts.cs` / `CardTabStrip.SemanticParts.cs` / `TabStripItem.SemanticParts.cs` 三个 descriptor 文件与 `TabStripSemanticPartTests` 落地，覆盖 descriptor 数量/顺序/字段、`item` 运行时 marker 与 `add` / `close` / `icon` / `label` 静态 marker、生成 Style 各精确命中一个节点、选中/关闭/尺寸档切换后命中数量不变。（2026-09-14：已随 `TabControl` 家族完成。）
- [x] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 和 Gallery 测试、LLMS verify 以及 `git diff --check`。（2026-09-15：Generator 528/528、Desktop 4031/4031、Gallery 644/644 全绿；LLMS verify 通过（79 控件 / 161 文件）；`git diff --check` 干净。TabStrip 语义预览挂载于页面内的 `SemanticPartPreview`，无需 Gallery NativeAOT publish。）
- [x] **强制停止：** 保持 TabStrip 的所有实现改动未提交，直到用户验证真实宿主行为并明确授权提交。（2026-09-14：用户授权提交（Gate C）；已随 `TabControl` 家族提交一并实施，未推送。）

### 任务 18：GroupBox（2026-09-16 用户指令新增，原排除判定撤销）

**控件文档：** `docs/controls/desktop/data-display/group-box/overview.md`、`docs/controls/desktop/data-display/group-box/implementation.md`、`docs/controls/desktop/data-display/group-box/semantic-part.md`（Gate A 新增）

**证据范围：** `src/AtomUI.Desktop.Controls/GroupBox/**/*.cs`、`src/AtomUI.Desktop.Controls/GroupBox/Themes/GroupBoxTheme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/GroupBox`（新增 `GroupBoxSemanticPartTests`）；Gallery `controlgallery/AtomUIGallery/ShowCases/DataDisplay/GroupBox`。

**风险类型：** **无上游 owner**，Part 必须从 AtomUI 自身模板职责设计（不得照抄上游 owner 的键集）；可见边框与背景由 `GroupBox.Render` 自绘、模板中没有承载节点，`root` 因此是边框/背景的唯一入口；`PART_HeaderContent` 需由 `Decorator` 提升为 `Border` 才能提供 `Background`，该提升会牵动主题中两个以节点类型开头的 selector；Header 缺口是几何排除而非背景遮挡，需证明不透明 Header 背景不会还原边框短线。

**范围说明：** 本家族原被设计文档 §2.4 以「Ant Design 没有职责直接对应的公开 Semantic DOM owner」为由排除。经 2026-09-16 用户指令撤销排除：Ant Design 6.6.3 稳定发布源码 `components/` 下确实不存在 fieldset、group 或 group-box 类组件、`GroupBox` 标识零命中，因此本次纳入**不是**一次新的 §2.1 上游准入 Gate 通过，而是用户直接指令下对排除判定的撤销，先例为 `SplitButton` 触发侧的能力补充。Part 参照上游 `Card` 的分区键命名（不借用其 owner 资格），`extra` / `cover` / `actions` 等 Card 专属键不虚构到 GroupBox 模板上。

- [x] **Gate A 设计审核：** 确认为单一 public owner `GroupBox` 建立 descriptor，公开 `root` / `header` / `icon` / `title` / `content` 五个 Part；确认 `header`（`PART_HeaderContent`）、`icon`（`PART_HeaderIconPresenter`）、`title`（`PART_HeaderPresenter`）、`content`（`PART_ContentPresenter`）全部是 `GroupBoxTheme.axaml` 单一模板内的静态节点（`RuntimeCreated=false`、无显式 `SelectorRoute`、无 `.semantic-scope-*` 锚点、无跨视觉根 Part）；确认 `PART_Frame`、`PART_HeaderContainer`、Header 缺口几何、Header 内部 `StackPanel` 排布、GroupBox Token 保留值（`TextPaddingInline` / `OrientationMarginPercent` / `VerticalMarginInline`）以及用户内容子树都不进入契约；确认 `title` 的 `ContractType` 取 `Avalonia.Controls.TextBlock` 基类而非节点派生类型，`header` 取 `Border`。（2026-09-16：Gate A 文档已产出并经用户确认（用户回复「确认」）——`semantic-part.md` 新增五 Part 完整契约；`overview.md` 补 §3.5 与 LLMS 语义表；`implementation.md` 补 `GroupBox.SemanticParts.cs`、§5.1 marker 所有权、§5.2 自绘几何边界；并记录 `PART_HeaderContent` 由 `Decorator` 提升到 `Border` 的理由。准入无上游 owner 这一事实已按日期化范围变更记入设计文档 §2.4 与总计划。）
- [x] 更新两份控件文档，写明准确的 Descriptor、真实模板/运行时节点、owner 边界、排除的 internal wrapper、生命周期/性能不变量和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。（2026-09-16：三份文档齐备；LLMS generate + verify 通过（79 控件 / 161 文件）；`git diff --check` 干净。**注：** verify 首轮报 `forbidden external project name` 与 `stale component path`——原因是生成器会把 `## Semantic Parts` 整节原样抽进产物，而该节内的上游对照文本带出了外部项目名与组件目录路径；已按 `time-picker` 等既有文档的写法改为「上游设计体系」后告警消除。）
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/GroupBox/GroupBoxSemanticPartTests.cs`，覆盖 descriptor 五部件的数量/顺序/字段、四个静态 marker 的存在与节点类型、生成的 `GroupBoxHeaderStyle` / `GroupBoxIconStyle` / `GroupBoxTitleStyle` / `GroupBoxContentStyle` 各精确命中一个节点、`HeaderIcon` 为空、`HeaderTitlePosition` 三档、`Background` 三态与模板重应用后命中数量不变；验证 `PART_HeaderContent` 类型提升后 `Find<Decorator>` 与缺口 `Bounds` 语义不变，且不透明 Header 背景不还原边框短线；验证 Semantic Setter 覆盖 Token/Padding 基线与 `HeaderTitleColor` 投影的优先级关系。（2026-09-16：`GroupBox.SemanticParts.cs` 声明四 Part，`GroupBoxTheme.axaml` 增加四个静态 `.semantic-*` marker 并把 `PART_HeaderContent` 由 `Decorator` 提升为 `Border`、同步两处类型开头 selector，`GroupBox.cs` 加 `partial`。TDD：先写 15 个测试确认 **13 红**（1 个既有行为守卫测试通过），再实现至 **15/15 全绿**。实现期关键发现：`[SemanticPart]` 的 `ContractType = typeof(TextBlock)` 会解析到**同命名空间的派生类型** `AtomUI.Desktop.Controls.TextBlock` 而非 `Avalonia.Controls.TextBlock` 基类，必须完全限定；测试内嵌套 Style 必须用 `Nesting()`（父 Style 提供 owner selector）而非 `Template()` 起始，否则 Avalonia 抛 "Template selector must be preceeded by a selector"。另新增尺寸基线失败回归 `Fixed_Height_On_The_Header_Part_Suppresses_The_Natural_Auto_Height_Baseline`，并**实测其可失败**（把 `Height=1` 换成 `NaN` 后该用例确实失败），证明非空断言。Gallery：`GroupBoxShowCase.axaml` 由 `GalleryStickyTabsHost` 迁移为 `GalleryShowCaseHost` + `SemanticPartsContentTemplate`（1 个 `SemanticPartPreview`、5 张 Part 描述卡）+ `SourceKey="group-box-semantic-part"` 样式示例（`GroupBoxHeaderStyle`×2 / `GroupBoxTitleStyle`×2 / `GroupBoxContentStyle`×1 / `GroupBoxIconStyle`×1，含 `x:SetterTargetType`）+ 7 个本地化 key × 4 语言；同步 `CatalogMemberOrder.baseline`（GroupBox 行插入 7 个成员）与 `GalleryCatalogCoverageTests` 单元计数 4732→4739。）
- [x] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 测试、LLMS verify 和 `git diff --check`；GroupBox 无 Popup/运行时宿主路径，按计划条件不需要 NativeAOT 验证。（2026-09-16：Desktop Controls **4047/4047**、Generator **528/528**、GalleryBase **185/185**、Gallery **647/647**、Docs LLMsGenerator **23/23**；LLMS generate + verify 通过（79 控件 / 161 文件）；`git diff --check` 干净。GroupBox 无 Popup、Overlay、Window 或可选包路径，按计划条件不需要 NativeAOT publish。**边框定制 demo 追加：** 用户要求提供可定制边框的 demo，随后补 `Root_Border_And_Background_Are_Customizable_Through_Owner_Scoped_Style`（实测 owner 作用域 Style 覆盖 ControlTheme 的 `BorderBrush` / `BorderThickness` / `CornerRadius` / `Background`，且覆盖值真正进入 `_cachedBorderThickness` / `_cachedCornerRadius` 与 Render 输出画笔）与 Gallery 示例 `semantic-border` / `semantic-border-plain`；两者均**实测可失败**（移除 demo 的边框 Setter 后 `GroupBox_Semantic_Style_Example_Applies_The_Official_Style_Values` 确实失败），证明非空断言。过程中确认 `GroupBox.Render` 把背景与边框都作为**填充几何**绘制（`DrawGeometry` 的 pen 为 `null`），最初按 pen 断言边框色的写法属测试自身错误，已修正。单元计数随之 4739→4740。）
- [ ] **真机视觉验证：** 用户在 Gallery 桌面宿主（真实 Skia 后端）走查缺口几何、三档标题位置、不透明 Header 背景不还原边框短线、Header 背景的圆角裁剪，以及 Semantic 示例中图标尺寸/颜色与边框定制四项（颜色/粗细/圆角/背景）是否生效。（2026-09-16：步骤已写入 [GroupBox 语义部件真机视觉验收步骤](../specs/2026-09-16-groupbox-semantic-visual-acceptance.md)，覆盖 Semantic Parts 页签、Custom Semantic Part styling 示例（含边框定制步骤 2.6-2.8）与三类 Examples 回归；**待用户回传截图/录屏**，当前状态为「待视觉验收」。）
- [ ] **强制停止：** 保持 GroupBox 的所有实现改动未提交，直到用户验证真实宿主行为并明确授权提交。（2026-09-16：全部实现与文档改动保持未提交，等待视觉验收与提交授权。）

## 批次收尾

- [ ] 确认 19 个控件家族分别拥有用户授权的独立提交。
- [ ] 运行完整 Desktop Controls、Generator、GalleryBase 和 Gallery 测试工程，并执行集合/虚拟化回归筛选。
- [ ] 运行 LLMS verify、NativeAOT publish 和 `git diff --check`。
- [ ] 更新总计划清单，不创建批次提交。
