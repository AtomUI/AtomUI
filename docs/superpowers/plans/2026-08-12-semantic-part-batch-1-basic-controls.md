# Semantic Part 第一批基础控件实施计划

> **供智能体执行者使用：** 使用 `superpowers:executing-plans` 在当前会话中执行，不得使用 subagent。每项任务在文档审核后停止一次，并在未提交实现审核前再次停止。

**目标：** 完成首批 16 个控件家族的 Semantic Part 改造，以代码事实建立契约，并形成后续批次沿用的审核节奏。

**架构：** 每套正式控件文档拥有该控件的最终契约。静态区域使用生成的 Descriptor 和静态 AXAML marker；运行时及 Overlay 区域只复用现有 owner 生命周期。Button 仅作为基础设施参考，不作为命名模板。

**技术栈：** .NET 10、Avalonia 12、AtomUI Desktop Controls、AtomUI Generator、AXAML、xUnit v3、Avalonia Headless、AtomUI Gallery。

## 全局约束

- 遵循[全量改造总计划](2026-08-12-semantic-part-control-rollout.md)和[全量改造设计](../specs/2026-08-12-semantic-part-control-rollout-design.md)。
- 必须先执行 Gate A；用户明确批准前，不得编辑源码、主题、测试、Gallery 或 changelog。
- 每个控件的 Gate A 必须先完成尺寸/状态基线矩阵和外部 default 映射；必须准备一个最小失败回归证明尺寸基线缺失或混用。
- 未通过基线审计，不得在 Gallery 示例中添加布局型 Semantic Setter，也不得用固定 Height/MinHeight/像素偏移绕过问题。
- 只有所有受影响控件的正式设计都获得批准后，才能修改共享源码或主题文件。
- Gate B 以未提交差异结束。只有用户明确完成验证并授权后才能创建提交。
- 所有 Gallery 集成都使用 `GalleryShowCaseHost.SemanticPartsContentTemplate`，首次选择 Semantic Parts Tab 前不得实例化相关内容。
- 每个控件实现必须遵循“完整 SizeType 基线 + Semantic 增量 Setter”顺序；完成一个控件后将尺寸基线、布局回归和实际 Gallery 结果一并交付验收。

---

### 任务 1：Badge

**控件文档：** `docs/controls/desktop/data-display/badge/overview.md`, `docs/controls/desktop/data-display/badge/semantic-part.md`, `docs/controls/desktop/data-display/badge/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Badge/*.cs`, `src/AtomUI.Desktop.Controls/Badge/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/Badge`；Gallery `controlgallery/AtomUIGallery/ShowCases/DataDisplay/Badge`.

**风险类型：** Adorner、运行时创建的 Visual、多个 public 变体。

- [x] **Gate A 设计审核：** 分别审计 `CountBadge`、`DotBadge`、`RibbonBadge` 及其 Adorner owner；确认 badge indicator、count content、ribbon text 等职责属于哪个 public owner，并定义 attached target、adorner attach/detach、无 target 和状态切换时的可达性。
- [x] 更新三份控件文档：`overview.md` 保留支持摘要，`semantic-part.md` 写明准确的 Part/selector/ContractType/cardinality/customization/cross-root/runtime 公共契约、逐 Part 说明和排除边界，`implementation.md` 维护真实节点映射与生命周期；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/Badge/BadgeSemanticPartTests.cs`，覆盖三种 badge、adorner 创建/移除/重新附加、visible/zero 状态与 descriptor；Gallery 测试验证真实 Adorner root 通过显式 additional root 展示且 Tab 未打开时不创建。
- [x] 运行 Generator Semantic Part 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 页面测试、LLMS verify 和 `git diff --check`；存在 Popup/Overlay/运行时宿主风险时增加 NativeAOT publish。
- [x] **强制停止：** 保持 Badge 的所有实现改动未提交，直到用户完成验证并明确要求为该控件家族创建提交。

### 任务 2：Card

**控件文档：** `docs/controls/desktop/data-display/card/overview.md`, `docs/controls/desktop/data-display/card/semantic-part.md`, `docs/controls/desktop/data-display/card/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Card/*.cs`, `src/AtomUI.Desktop.Controls/Card/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/Card`；Gallery `controlgallery/AtomUIGallery/ShowCases/DataDisplay/Card`.

**风险类型：** 复合 public 子控件、可选区域、item 集合。

- [x] **Gate A 设计审核：** 审计 Card header/body/cover/actions/meta/tabs/grid 等真实职责，区分 Card owner region 与 `CardActionButton`、`CardGridItem`、`CardMetaContent` 等 public child descriptor；为可选内容、actions 集合和派生主题确定 cardinality。
- [x] 更新三份控件文档：`overview.md` 保留支持摘要，`semantic-part.md` 写明准确的 Part/selector/ContractType/cardinality/customization/cross-root/runtime 公共契约、逐 Part 说明和排除边界，`implementation.md` 维护真实节点映射与生命周期；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/Card/CardSemanticPartTests.cs`，覆盖 basic/meta/cover/actions/tabs/grid 组合、空区域、动态集合与 marker owner 隔离；Gallery 页面和测试展示至少一个完整复合结构并保持真正延迟创建。
- [x] 运行 Generator Semantic Part 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 页面测试、LLMS verify 和 `git diff --check`；存在 Popup/Overlay/运行时宿主风险时增加 NativeAOT publish。
- [x] **强制停止：** 保持 Card 的所有实现改动未提交，直到用户完成验证并明确要求为该控件家族创建提交。

### 任务 3：Descriptions

**控件文档：** `docs/controls/desktop/data-display/descriptions/overview.md`, `docs/controls/desktop/data-display/descriptions/semantic-part.md`, `docs/controls/desktop/data-display/descriptions/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Descriptions/*.cs`, `src/AtomUI.Desktop.Controls/Descriptions/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/Descriptions`；Gallery `controlgallery/AtomUIGallery/ShowCases/DataDisplay/Descriptions`.

**风险类型：** 运行时布局、重复 cell、响应式变体。

- [x] **Gate A 设计审核：** 审计默认与 bordered 模式下 title/extra/item label/item content 的 owner 和重复数量，确认 `DescriptionDefaultItem`、bordered label/content/cell 是否需要独立 descriptor；记录 responsive column 变化和运行时生成节点。
- [x] 更新三份控件文档：`overview.md` 保留支持摘要，`semantic-part.md` 写明准确的 Part/selector/ContractType/cardinality/customization/cross-root/runtime 公共契约、逐 Part 说明和排除边界，`implementation.md` 维护真实节点映射与生命周期；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/Descriptions/DescriptionsSemanticPartTests.cs`，覆盖默认/bordered、title/extra 缺失、多 item、responsive 重排和动态集合；验证 repeated marker 数量稳定且不跨 item owner 泄漏。
- [x] 运行 Generator Semantic Part 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 页面测试、LLMS verify 和 `git diff --check`；存在 Popup/Overlay/运行时宿主风险时增加 NativeAOT publish。
- [x] **强制停止：** 保持 Descriptions 的所有实现改动未提交，直到用户完成验证并明确要求为该控件家族创建提交。

### 任务 4：Empty

**控件文档：** `docs/controls/desktop/data-display/empty/overview.md`, `docs/controls/desktop/data-display/empty/semantic-part.md`, `docs/controls/desktop/data-display/empty/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Empty/Empty.cs`, `src/AtomUI.Desktop.Controls/Empty/Themes/EmptyTheme.axaml`；在 `tests/AtomUI.Desktop.Controls.Tests/Empty`；Gallery `controlgallery/AtomUIGallery/ShowCases/DataDisplay/Empty`.

**风险类型：** 可选 content、布局。

- [x] **Gate A 设计审核：** 审计 image、description 和 footer/children 等实际模板区域，确认默认图形与用户内容替换时使用同一职责还是替代实现，并记录空内容与自定义内容的 cardinality。
- [x] 更新三份控件文档：`overview.md` 保留支持摘要，`semantic-part.md` 写明准确的 Part/selector/ContractType/cardinality/customization/cross-root/runtime 公共契约、逐 Part 说明和排除边界，`implementation.md` 维护真实节点映射与生命周期；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 创建 `tests/AtomUI.Desktop.Controls.Tests/Empty/EmptySemanticPartTests.cs`，覆盖默认图形、自定义 image、description/footer 缺失和布局 Setter；新增 Gallery page 测试，证明 Preview 只在 Semantic Tab 首次选择后创建。
- [x] 运行 Generator Semantic Part 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 页面测试、LLMS verify 和 `git diff --check`；存在 Popup/Overlay/运行时宿主风险时增加 NativeAOT publish。
- [x] **强制停止：** 保持 Empty 的所有实现改动未提交，直到用户完成验证并明确要求为该控件家族创建提交。

### 任务 5：QRCode

**控件文档：** `docs/controls/desktop/data-display/qr-code/overview.md`, `docs/controls/desktop/data-display/qr-code/semantic-part.md`, `docs/controls/desktop/data-display/qr-code/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/QRCode/QRCode.cs`, `src/AtomUI.Desktop.Controls/QRCode/Themes/QRCodeTheme.axaml`；在 `tests/AtomUI.Desktop.Controls.Tests/QRCode`；Gallery `controlgallery/AtomUIGallery/ShowCases/DataDisplay/QRCode`.

**风险类型：** renderer/content overlay、状态变体、固定 geometry。

- [x] **Gate A 设计审核：** 审计二维码 renderer、center icon、expired/loading/status overlay 等真实职责，区分绘制内容与独立 Visual；明确固定方形几何、icon/status 尺寸和 layout Setter 的兼容边界。
- [x] 更新三份控件文档：`overview.md` 保留支持摘要，`semantic-part.md` 写明准确的 Part/selector/ContractType/cardinality/customization/cross-root/runtime 公共契约、逐 Part 说明和排除边界，`implementation.md` 维护真实节点映射与生命周期；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 创建 `tests/AtomUI.Desktop.Controls.Tests/QRCode/QRCodeSemanticPartTests.cs`，覆盖 normal/loading/expired、icon 有无、descriptor 与实际 Visual 数量；验证 Semantic 样式不会破坏二维码方形测量和纠错内容。
- [x] 运行 Generator Semantic Part 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 页面测试、LLMS verify 和 `git diff --check`；存在 Popup/Overlay/运行时宿主风险时增加 NativeAOT publish。
- [x] **强制停止：** 保持 QRCode 的所有实现改动未提交，直到用户完成验证并明确要求为该控件家族创建提交。

### 任务 6：Statistic

**控件文档：** `docs/controls/desktop/data-display/statistic/overview.md`, `docs/controls/desktop/data-display/statistic/semantic-part.md`, `docs/controls/desktop/data-display/statistic/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Statistic/*.cs`, `src/AtomUI.Desktop.Controls/Statistic/Themes/*Theme.axaml`；在 `tests/AtomUI.Desktop.Controls.Tests/Statistic`；Gallery `controlgallery/AtomUIGallery/ShowCases/DataDisplay/Statistic`.

**风险类型：** 派生主题、格式化 content、timer 生命周期。

- [x] **Gate A 设计审核：** 审计 `Statistic`、`StatisticCountUp`、`TimerStatistic` 的 title/value/prefix/suffix 等职责和 abstract/default theme 继承，确认 count-up/timer 是否只改变数据还是改变 Part 实现，并记录 timer detach 生命周期。
- [x] 更新三份控件文档：`overview.md` 保留支持摘要，`semantic-part.md` 写明准确的 Part/selector/ContractType/cardinality/customization/cross-root/runtime 公共契约、逐 Part 说明和排除边界，`implementation.md` 维护真实节点映射与生命周期；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 创建 `tests/AtomUI.Desktop.Controls.Tests/Statistic/StatisticSemanticPartTests.cs`，覆盖三种 owner、prefix/suffix 缺失、格式更新、timer/count-up 状态与主题继承；Gallery 测试验证 descriptor 与延迟 sample。
- [x] 运行 Generator Semantic Part 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 页面测试、LLMS verify 和 `git diff --check`；存在 Popup/Overlay/运行时宿主风险时增加 NativeAOT publish。
- [x] **强制停止：** 保持 Statistic 的所有实现改动未提交，直到用户完成验证并明确要求为该控件家族创建提交。

### 任务 7：Alert

**控件文档：** `docs/controls/desktop/feedback/alert/overview.md`, `docs/controls/desktop/feedback/alert/semantic-part.md`, `docs/controls/desktop/feedback/alert/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Alert/Alert.cs`, `src/AtomUI.Desktop.Controls/Alert/Themes/AlertTheme.axaml`；在 `tests/AtomUI.Desktop.Controls.Tests/Alert`；Gallery `controlgallery/AtomUIGallery/ShowCases/Feedback/Alert`.

**风险类型：** 可选 icon/action/close、motion 和状态变体。

- [x] **Gate A 设计审核：** 审计 type icon、message、description、action 和 close affordance，确认 closable、banner、with-description 等状态下的替代/可选数量；明确 closing motion 期间 marker 可观察性和关闭后的生命周期。
- [x] 更新三份控件文档：`overview.md` 保留支持摘要，`semantic-part.md` 写明准确的 Part/selector/ContractType/cardinality/customization/cross-root/runtime 公共契约、逐 Part 说明和排除边界，`implementation.md` 维护真实节点映射与生命周期；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 创建 `tests/AtomUI.Desktop.Controls.Tests/Alert/AlertSemanticPartTests.cs`，覆盖四种类型、icon/action/description/close 有无、关闭前后和布局 Setter；Gallery 测试验证状态丰富样例且未打开 Tab 时零 Preview 成本。
- [x] 运行 Generator Semantic Part 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 页面测试、LLMS verify 和 `git diff --check`；存在 Popup/Overlay/运行时宿主风险时增加 NativeAOT publish。
- [x] **强制停止：** 保持 Alert 的所有实现改动未提交，直到用户完成验证并明确要求为该控件家族创建提交。

### 任务 8：ProgressBar

**控件文档：** `docs/controls/desktop/feedback/progress-bar/overview.md`, `docs/controls/desktop/feedback/progress-bar/semantic-part.md`, `docs/controls/desktop/feedback/progress-bar/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/ProgressBar/*.cs`, `src/AtomUI.Desktop.Controls/ProgressBar/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/ProgressBar`；Gallery `controlgallery/AtomUIGallery/ShowCases/Feedback/ProgressBar`.

**风险类型：** 多种 geometry 家族、renderer/layout、派生主题。

- [x] **Gate A 设计审核：** 分别审计 line、steps、circle、dashboard 的 track/indicator/trail/text/status icon 等职责，确认哪些 Part 可跨完全不同模板保持语义，哪些应由具体 public derived owner 声明；记录 fixed geometry 和 progress update 热路径。
- [x] 更新三份控件文档：`overview.md` 保留支持摘要，`semantic-part.md` 写明准确的 Part/selector/ContractType/cardinality/customization/cross-root/runtime 公共契约、逐 Part 说明和排除边界，`implementation.md` 维护真实节点映射与生命周期；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/ProgressBar/ProgressBarSemanticPartTests.cs`，覆盖每种 public progress owner、0/partial/100/exception 状态、marker/cardinality 和布局边界；验证 value 更新不增删 marker、不引入额外监听或分配。
- [x] 运行 Generator Semantic Part 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 页面测试、LLMS verify 和 `git diff --check`；存在 Popup/Overlay/运行时宿主风险时增加 NativeAOT publish。
- [x] **强制停止：** 保持 ProgressBar 的所有实现改动未提交，直到用户完成验证并明确要求为该控件家族创建提交。

### 任务 9：Result

**控件文档：** `docs/controls/desktop/feedback/result/overview.md`, `docs/controls/desktop/feedback/result/semantic-part.md`, `docs/controls/desktop/feedback/result/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Result/Result.cs`, `src/AtomUI.Desktop.Controls/Result/Themes/ResultTheme.axaml`；在 `tests/AtomUI.Desktop.Controls.Tests/Result`；Gallery `controlgallery/AtomUIGallery/ShowCases/Feedback/Result`.

**风险类型：** 可选 semantic icon、content/action 布局。

- [x] **Gate A 设计审核：** 审计 status icon、title、subtitle、extra/action 和 content 区域，确认预设状态图标与用户图标是否共享职责；记录空区域、复杂 content 和 action 集合的 cardinality。
- [x] 更新三份控件文档：`overview.md` 保留支持摘要，`semantic-part.md` 写明准确的 Part/selector/ContractType/cardinality/customization/cross-root/runtime 公共契约、逐 Part 说明和排除边界，`implementation.md` 维护真实节点映射与生命周期；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 创建 `tests/AtomUI.Desktop.Controls.Tests/Result/ResultSemanticPartTests.cs`，覆盖各 status、自定义 icon、title/subtitle/extra 缺失和复杂 content；验证布局 Setter、descriptor 与 Gallery 延迟 Preview。
- [x] 运行 Generator Semantic Part 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 页面测试、LLMS verify 和 `git diff --check`；存在 Popup/Overlay/运行时宿主风险时增加 NativeAOT publish。
- [x] **强制停止：** 保持 Result 的所有实现改动未提交，直到用户完成验证并明确要求为该控件家族创建提交。

### 任务 10：Skeleton

**控件文档：** `docs/controls/desktop/feedback/skeleton/overview.md`, `docs/controls/desktop/feedback/skeleton/semantic-part.md`, `docs/controls/desktop/feedback/skeleton/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Skeleton/*.cs`, `src/AtomUI.Desktop.Controls/Skeleton/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/Skeleton`；Gallery `controlgallery/AtomUIGallery/ShowCases/Feedback/Skeleton`.

**风险类型：** 大型 public 控件家族、重复运行时节点、animation。

- [x] **Gate A 设计审核：** 逐一审计 `Skeleton`、`SkeletonAvatar`、`SkeletonButton`、`SkeletonInput`、`SkeletonImage`、
  `SkeletonNode`、`SkeletonParagraph`、`SkeletonTitle`、`SkeletonLine` 等 public owner，区分容器 region 与各 skeleton element
  自己的 root/region；明确 active animation、paragraph row 数和运行时 child 创建。
- [x] 更新三份控件文档：`overview.md` 保留支持摘要，`semantic-part.md` 写明准确的 Part/selector/ContractType/cardinality/customization/cross-root/runtime 公共契约、逐 Part 说明和排除边界，`implementation.md` 维护真实节点映射与生命周期；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/Skeleton/SkeletonSemanticPartTests.cs`，覆盖全部 public variants、paragraph 多行 cardinality、active/inactive、loading 内容切换和动态 row 数量；验证 marker 静态或一次性创建且 animation 热路径无新增工作。
- [x] 运行 Generator Semantic Part 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 页面测试、LLMS verify 和 `git diff --check`；存在 Popup/Overlay/运行时宿主风险时增加 NativeAOT publish。
- [x] **强制停止：** 保持 Skeleton 的所有实现改动未提交，直到用户完成验证并明确要求为该控件家族创建提交。

### 任务 11：Spin

**控件文档：** `docs/controls/desktop/feedback/spin/overview.md`, `docs/controls/desktop/feedback/spin/semantic-part.md`, `docs/controls/desktop/feedback/spin/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Spin/*.cs`, `src/AtomUI.Desktop.Controls/Spin/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/Spin`；Gallery `controlgallery/AtomUIGallery/ShowCases/Feedback/Spin`.

**风险类型：** Overlay 状态、public indicator 子控件、SizeType。

- [x] **Gate A 设计审核：** 审计 spinner indicator、tip、nested content 与 loading overlay，确认 `Spin` 和 public `SpinIndicator` 的 owner 边界；记录 standalone/nested、spinning 切换、SizeType/Custom 和 overlay visibility。
- [x] 更新三份控件文档：`overview.md` 保留支持摘要，`semantic-part.md` 写明准确的 Part/selector/ContractType/cardinality/customization/cross-root/runtime 公共契约、逐 Part 说明和排除边界，`implementation.md` 维护真实节点映射与生命周期；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/Spin/SpinSemanticPartTests.cs`，覆盖 standalone/nested、indicator/tip 自定义、spinning 切换和所有尺寸；验证 overlay marker 保持、布局 Setter 与 Preview 高亮目标正确。
- [x] 运行 Generator Semantic Part 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 页面测试、LLMS verify 和 `git diff --check`；存在 Popup/Overlay/运行时宿主风险时增加 NativeAOT publish。
- [x] **强制停止：** 保持 Spin 的所有实现改动未提交，直到用户完成验证并明确要求为该控件家族创建提交。

### 任务 12：FloatButton

**控件文档：** `docs/controls/desktop/general/float-button/overview.md`, `docs/controls/desktop/general/float-button/semantic-part.md`, `docs/controls/desktop/general/float-button/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/FloatButton/*.cs`, `src/AtomUI.Desktop.Controls/FloatButton/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/FloatButton`；Gallery `controlgallery/AtomUIGallery/ShowCases/General/FloatButton`.

**风险类型：** Overlay 宿主、public 变体/group、motion、SizeType。

- [x] **Gate A 设计审核：** 审计 `FloatButton`、group、BackTop 及各 host/items control 的 owner，确认 icon/description/badge/tooltip/trigger 区域和 group popup/overlay 的视觉根；记录 group open-close、BackTop visibility 与 SizeType。
- [x] 更新三份控件文档：`overview.md` 保留支持摘要，`semantic-part.md` 写明准确的 Part/selector/ContractType/cardinality/customization/cross-root/runtime 公共契约、逐 Part 说明和排除边界，`implementation.md` 维护真实节点映射与生命周期；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/FloatButton/FloatButtonSemanticPartTests.cs`，覆盖 basic/group/BackTop、overlay open-close-reopen、item 增删、尺寸和 descriptor；Gallery 使用显式 root 展示 overlay，运行 GalleryBase 与 NativeAOT 验证。
- [x] 运行 Generator Semantic Part 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 页面测试、LLMS verify 和 `git diff --check`；存在 Popup/Overlay/运行时宿主风险时增加 NativeAOT publish。
- [x] **强制停止：** 保持 FloatButton 的所有实现改动未提交，直到用户完成验证并明确要求为该控件家族创建提交。

### 任务 13：Separator

**控件文档：** `docs/controls/desktop/general/separator/overview.md`, `docs/controls/desktop/general/separator/semantic-part.md`, `docs/controls/desktop/general/separator/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Separator/Separator.cs`, `src/AtomUI.Desktop.Controls/Separator/Themes/SeparatorTheme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/Separator`；Gallery `controlgallery/AtomUIGallery/ShowCases/General/Separator`.

**风险类型：** orientation/layout、可能重分类为不适用。

- [x] **Gate A 设计审核：** 审计 line 与 optional text/content 的实际模板职责，验证纯分隔线模式是否只有 root；明确 horizontal/vertical、dashed 和 content alignment 下是否存在可长期承诺的非 root Part。
- [x] 更新三份控件文档：`overview.md` 保留支持摘要，`semantic-part.md` 写明准确的 Part/selector/ContractType/cardinality/customization/cross-root/runtime 公共契约、逐 Part 说明和排除边界，`implementation.md` 维护真实节点映射与生命周期；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 若 Gate A 批准至少一个非 root Part，新增 `tests/AtomUI.Desktop.Controls.Tests/Separator/SeparatorSemanticPartTests.cs`，覆盖横纵向、带/不带内容、对齐与布局 Setter；若只有 root 职责，则将 Separator 重分类为不适用，记录事实依据并跳过实现。
- [x] 运行 Generator Semantic Part 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 页面测试、LLMS verify 和 `git diff --check`；存在 Popup/Overlay/运行时宿主风险时增加 NativeAOT publish。
- [x] **强制停止：** 保持 Separator 的所有实现改动未提交，直到用户完成验证并明确要求为该控件家族创建提交。

### 任务 14：CheckBox

**控件文档：** `docs/controls/desktop/data-entry/check-box/overview.md`, `docs/controls/desktop/data-entry/check-box/semantic-part.md`, `docs/controls/desktop/data-entry/check-box/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/CheckBox/*.cs`, `src/AtomUI.Desktop.Controls/CheckBox/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/CheckBox`；Gallery `controlgallery/AtomUIGallery/ShowCases/DataEntry/CheckBox`.

**风险类型：** public indicator/group/container、三态、SizeType。

- [x] **Gate A 设计审核：** 审计 checkbox indicator、label/content、group/items control 的 owner；确认 checked/unchecked/indeterminate 是同一 indicator 状态还是替代实现，并记录 group container prepare/clear 与 SizeType。
- [x] 更新三份控件文档：`overview.md` 保留支持摘要，`semantic-part.md` 写明准确的 Part/selector/ContractType/cardinality/customization/cross-root/runtime 公共契约、逐 Part 说明和排除边界，`implementation.md` 维护真实节点映射与生命周期；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/CheckBox/CheckBoxSemanticPartTests.cs`，覆盖三态、content 有无、group 动态集合、容器生命周期、disabled 和所有尺寸；验证 marker 不随 check 状态增删。
- [x] 运行 Generator Semantic Part 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 页面测试、LLMS verify 和 `git diff --check`；存在 Popup/Overlay/运行时宿主风险时增加 NativeAOT publish。
- [x] **强制停止：** 保持 CheckBox 的所有实现改动未提交，直到用户完成验证并明确要求为该控件家族创建提交。

### 任务 15：RadioButton

**控件文档：** `docs/controls/desktop/data-entry/radio-button/overview.md`, `docs/controls/desktop/data-entry/radio-button/semantic-part.md`, `docs/controls/desktop/data-entry/radio-button/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/RadioButton/*.cs`, `src/AtomUI.Desktop.Controls/RadioButton/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/RadioButton`；Gallery `controlgallery/AtomUIGallery/ShowCases/DataEntry/RadioButton`.

**风险类型：** public indicator/group、selection 容器、SizeType。

- [x] **Gate A 设计审核：** 审计 radio indicator、label/content 与 `RadioButtonGroup` owner，确认 group 是否创建/管理 item container；记录 orientation、selection replacement、disabled 和 SizeType 布局边界。
- [x] 更新三份控件文档：`overview.md` 保留支持摘要，`semantic-part.md` 写明准确的 Part/selector/ContractType/cardinality/customization/cross-root/runtime 公共契约、逐 Part 说明和排除边界，`implementation.md` 维护真实节点映射与生命周期；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/RadioButton/RadioButtonSemanticPartTests.cs`，覆盖 checked/unchecked、group selection、orientation、collection replace/reset 和尺寸；保持既有 OptionButtonGroup 测试不受影响。
- [x] 运行 Generator Semantic Part 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 页面测试、LLMS verify 和 `git diff --check`；存在 Popup/Overlay/运行时宿主风险时增加 NativeAOT publish。
- [x] **强制停止：** 保持 RadioButton 的所有实现改动未提交，直到用户完成验证并明确要求为该控件家族创建提交。

### 任务 16：ToggleSwitch

**控件文档：** `docs/controls/desktop/data-entry/toggle-switch/overview.md`, `docs/controls/desktop/data-entry/toggle-switch/semantic-part.md`, `docs/controls/desktop/data-entry/toggle-switch/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Switch/ToggleSwitch.cs`, `src/AtomUI.Desktop.Controls/Switch/Themes/ToggleSwitchTheme.axaml`, `src/AtomUI.Desktop.Controls/Switch/Themes/SwitchKnobTheme.axaml`；在 `tests/AtomUI.Desktop.Controls.Tests/Switch`；Gallery `controlgallery/AtomUIGallery/ShowCases/DataEntry/ToggleSwitch`.

**风险类型：** public knob 子控件、checked content 变体、SizeType 和 motion。

- [x] **Gate A 设计审核：** 审计 track、public `SwitchKnob`、checked/unchecked content 和 loading indicator 的 owner/替代关系；明确状态 motion、SizeType/Custom 和固定 track geometry 对布局 Setter 的限制。
- [x] 更新三份控件文档：`overview.md` 保留支持摘要，`semantic-part.md` 写明准确的 Part/selector/ContractType/cardinality/customization/cross-root/runtime 公共契约、逐 Part 说明和排除边界，`implementation.md` 维护真实节点映射与生命周期；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 创建 `tests/AtomUI.Desktop.Controls.Tests/Switch/ToggleSwitchSemanticPartTests.cs`，覆盖 checked/unchecked/loading、content/icon variants、所有尺寸和 rapid 状态变化；验证 marker 稳定、motion 不新增 Semantic 工作，Gallery Preview 延迟创建。
- [x] 运行 Generator Semantic Part 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 页面测试、LLMS verify 和 `git diff --check`；存在 Popup/Overlay/运行时宿主风险时增加 NativeAOT publish。
- [x] **强制停止：** 保持 ToggleSwitch 的所有实现改动未提交，直到用户完成验证并明确要求为该控件家族创建提交。

## 批次收尾

- [x] 确认 16 个控件家族分别拥有用户授权的独立提交。
- [x] 运行完整的 `AtomUI.Desktop.Controls.Tests`、`AtomUI.Generator.Tests`、`AtomUI.Toolkits.GalleryBase.Tests` 和 `AtomUIGallery.Tests` 工程。
- [x] 运行 LLMS verify、Gallery NativeAOT publish 和 `git diff --check`。
- [x] 在总计划中将第一批标记为完成，不额外创建批次提交。
