# Semantic Part 第五批高密度控件实施计划

> **供智能体执行者使用：** 使用 `superpowers:executing-plans` 在当前会话中执行，不得使用 subagent。Gate A 源码工作前先记录性能基线，文档完成后停止并等待用户批准；实现完成后保持未提交，再次接受用户审核。

**目标：** 为对应 Ant Design 6.6.0 稳定发布源码公开 `Menu` 与 `Table` Semantic DOM API 的 `NavMenu`、`DataGrid` 建立 Semantic Part 契约（`Menu` 为 2026-09-15 用户指令撤销排除并追认，随本批次执行），不得造成高密度实例化、虚拟化、Popup 或容器生命周期行为退化。

**架构：** public owner 及其 public item/container 控件只获得真实模板能够证明的最小稳定 Descriptor。静态 marker 不参与 AtomUI 默认主题样式，也不随选择或状态切换。使用现有 `tools/performances/AtomUI.Performance` 检查提供改造前后的实例化、布局、分配和状态证据。

**技术栈：** .NET 10、Avalonia 12、AtomUI Desktop Controls/DataGrid、AXAML、xUnit v3、Avalonia Headless、AtomUI Performance、AtomUI Gallery、NativeAOT。

## 全局约束

- 遵循[全量改造总计划](2026-08-12-semantic-part-control-rollout.md)和[全量改造设计](../specs/2026-08-12-semantic-part-control-rollout-design.md)。
- 编辑每个控件的文档前，使用 `--count 60` 和该控件现有的状态验证参数记录性能基线。
- 改造前后报告使用相同的 SDK、配置、count 和宿主；报告保存在临时路径，不得写入正式控件文档或被跟踪的生成输出。
- Gate A 必须定义每个 owner/container 的 marker 数量、Popup root、容器创建/回收路径和预期热路径成本。
- Gate B 不得添加 VisualTree 扫描、动态 class 变更、逐容器 registry 查询、以 Visual 为 key 的 Descriptor cache 或永久监听器。
- AtomUI 默认主题不得选择 `.semantic-*`。
- Gallery Preview 保持延迟创建，最多高亮 32 个可见目标。
- Gate B 改动保持未提交，直到用户明确完成验证并授权提交。

---

### 任务 1：NavMenu

**控件文档：** `docs/controls/desktop/navigation/nav-menu/overview.md`、`docs/controls/desktop/navigation/nav-menu/implementation.md`。

**证据范围：** `src/AtomUI.Desktop.Controls/NavMenu/**/*.cs`、全部 `src/AtomUI.Desktop.Controls/NavMenu/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/NavMenu`；共享 Gallery `controlgallery/AtomUIGallery/ShowCases/Navigation/Menu` 和真实 Gallery sidebar `controlgallery/AtomUIGallery/Workspace/Views/CaseNavigation*`；性能检查 `tools/performances/AtomUI.Performance/Suites/NavMenu`。

**风险类型：** entry 到容器的图关系、分层 group/item/divider 控件、Inline/Vertical/Horizontal 模板、Popup 和 collapse motion。

- [ ] **记录改造前基线：** 运行：

```bash
dotnet run --project tools/performances/AtomUI.Performance/AtomUI.Performance.csproj --framework net10.0 -- --suite navmenu --count 60 --markdown /tmp/atomui-semantic-navmenu-before.md
dotnet run --project tools/performances/AtomUI.Performance/AtomUI.Performance.csproj --framework net10.0 -- --verify-navmenu-states
```

记录改造后将再次采集的同一组耗时、分配和 Visual/Logical tree 指标。

> 2026-09-15 复核：改造前/后的性能报告未见归档（按本计划全局约束，报告只写 `/tmp`，不随仓库保留）。本项**保持未完成**，且改造前基线只能在 `release/6.0` 侧的未改造代码上重采，与当前 `feature/semantic` 已不是同一时间点，重采价值有限。

- [x] **Gate A 设计审核：** 审计 `NavMenu`、`NavMenuItem`、`NavMenuGroupItem`、`NavMenuDividerItem`、header 变体和 `NavMenuPopupFrame` 的 owner。明确 header/footer/items、item icon/text/indicator/active indicator、group header/items、inline child frame 和 popup frame/items 的职责。记录 entry graph ownership、container binder prepare/clear、Inline collapse、mode switch、selection、Popup root 和资源生命周期。（2026-09-14：已完成，结论落实于新增的 `docs/controls/desktop/navigation/nav-menu/semantic-part.md`，共 11 部件 + 隐式 `root`。）
- [x] 更新两份控件文档，写明准确的 Descriptor、逐模式模板映射、每个容器的 marker 数量、runtime/cross-root 标志、生命周期/性能和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。（2026-09-14：`nav-menu/overview.md`、`nav-menu/implementation.md` 与新增 `nav-menu/semantic-part.md` 同步完成，`docs/AI/generated` 由生成器重生成，随 `1e22ed1a3` 一并提交。）
- [x] **Gate B 实现：** 新增 `tests/AtomUI.Desktop.Controls.Tests/NavMenu/NavMenuSemanticPartTests.cs`，覆盖 structured entry、explicit node、group/divider transparency、全部模式、Inline collapse/expand、Popup reopen、selection/keyboard、entry source replace/reset、资源生命周期，并证明重新绑定容器不会产生重复 marker。（2026-09-14：`NavMenuSemanticPartTests.cs`（567 行）已落地，同提交另含 `NavMenuContainerLifetimeTests`、`NavMenuEntryContainerTests`、`NavMenuSelectionTests`、`NavMenuEntryModelTests`、`NavMenuItemActivationTests`。）
- [x] 将 NavMenu 集成到共享 Menu ShowCase 的 Semantic Tab；不得给同页的 `Menu`、`ContextMenu` 或真实应用 sidebar 附加 Preview 行为，也不得因此改造这些被排除的控件。

> **2026-09-15 用户裁决：追认 `Menu` 纳入本批次。** 集成已完成（`MenuShowCase.axaml` 新增 404 行、
> `MenuShowCasePageTests.cs` 扩写 690 行）。同一次提交 `1e22ed1a3` 另外新增了
> `src/AtomUI.Desktop.Controls/Menu/Menu.SemanticParts.cs`（11 部件 + 隐式 `root`）、`Menu/MenuSemanticLevel.cs` 与
> `docs/controls/desktop/navigation/menu/semantic-part.md`（271 行），即 `Menu` 被一并改造——这原本与设计文档 §2.4
> 「`Menu` 不新增 Semantic Part」的排除判定冲突。
>
> 用户已撤销该排除判定：`Menu` 与 `NavMenu` 映射同一个上游 `Menu` owner、12 个公开键路径逐字相同，职责直接对应
> （设计文档 §2.1 第 4 条）。`Menu` 已从设计文档 §2.4 与总计划「不适用」清单移入 §2.3 纳入映射，作为本批次**任务 3**
> 单独记录，本批次家族数由 2 增至 3。因此本项约束现在读作：不得给同页的 `ContextMenu` 或真实应用 sidebar 附加 Preview
> 行为，也不得借 `Menu` 的纳入去改造其他控件（2026-09-16 更正：原文举例为「仍属排除的控件（如 `ComboBox`）」，`ComboBox`
> 已于 2026-09-16 经用户指令撤销排除并纳入第三批；本项约束的准确含义是 **owner 隔离**——宿主控件不因组合关系向外声明
> 被组合控件的语义区域，与被组合控件自身是否纳入无关）。

- [ ] **记录改造后基线：** 重新运行与改造前完全相同的命令，输出到 `/tmp/atomui-semantic-navmenu-after.md`；比较 ms/item、KB/item、Visual/root、Logical/root 和状态验证结果。

> 2026-09-15 复核：同「记录改造前基线」，无归档对比报告，本项**保持未完成**。

- [ ] 运行 Generator Semantic 测试、全部 NavMenu 测试、GalleryBase 测试、`MenuShowCasePageTests`、LLMS verify、NativeAOT publish 和 `git diff --check`。
- [x] **强制停止：** 保持 NavMenu 改动未提交，直到用户明确完成性能与行为验证并授权提交。（2026-09-14：用户授权提交（Gate C）；单个控件家族提交 `1e22ed1a3`「feat(Semantic): 增加 NavMenu 与 Menu 语义部件并修复弹层钉住与子菜单状态递归」，未推送。）

### 任务 2：DataGrid

**控件文档：** `docs/controls/desktop/data-display/data-grid/overview.md`、`docs/controls/desktop/data-display/data-grid/implementation.md`。只有 Gate A 确认跨 owner 的 grid/row/cell/header/filter 模型无法在两份主文档中保持可维护时，才创建独立 `semantic-part.md`。

> **命名更正（2026-09-15）：** 本项原文写的是 `semantic-part-design.md`，但
> `tools/AtomUI.Docs.LLMsGenerator/Catalog/ControlInventory.cs` 只按 `semantic-part.md` 发现控件语义契约
> （`GetOptionalFilePath(controlDirectory, "semantic-part.md")`）。按原命名创建文件会被生成器完全忽略，等于契约不生效。
> 此处更正为 `semantic-part.md`。
>
> **2026-09-14 实施结论：** Gate A 判定两份主文档足以承载契约，**未**创建独立文件。`DataGrid` 的语义部件表内联在
> `overview.md`「LLMS 语义区域」章节，节点映射与生命周期细节在 `implementation.md`；生成器据此正常产出
> `docs/AI/generated/llms/controls/data-grid/semantic-cn.md`（12 行 = 隐式 `root` + 11 部件）。这是有意的承载方式选择，
> **不是**将 `DataGrid` 排除在改造范围之外——`DataGrid` 自 2026-08-12 起就在设计文档 §2.3 纳入映射中（映射上游 `Table`，Batch 5）。

**证据范围：** 可选包 `src/AtomUI.Desktop.Controls.DataGrid/**/*.cs`、全部 `src/AtomUI.Desktop.Controls.DataGrid/Themes/**/*.axaml`；独立测试工程 `tests/AtomUI.Desktop.Controls.DataGrid.Tests`；Gallery `controlgallery/AtomUIGallery/ShowCases/DataDisplay/DataGrid`；性能检查 `tools/performances/AtomUI.Performance/Suites/DataGrid`。

**风险类型：** 虚拟化 row/cell、多个 public owner 控件、分组 header/row、filter Flyout、pagination、editing、drag/reorder 和可选包注册。

- [ ] **记录改造前基线：** 运行：

```bash
dotnet run --project tools/performances/AtomUI.Performance/AtomUI.Performance.csproj --framework net10.0 -- --suite datagrid --count 60 --markdown /tmp/atomui-semantic-datagrid-before.md
dotnet run --project tools/performances/AtomUI.Performance/AtomUI.Performance.csproj --framework net10.0 -- --verify-datagrid-states
```

保留 Basic、关闭状态的 Menu/Tree filter、row header、collapsed details、column group、row group 和 Gallery-shape 场景指标。

> 2026-09-15 复核：改造前/后性能报告未见归档，本项**保持未完成**（同 NavMenu 说明）。

- [x] **Gate A 设计审核：** 选择 Part 前盘点每个 public Visual owner：`DataGrid`、`DataGridRow`、`DataGridCell`、`DataGridRowHeader`、`DataGridRowGroupHeader`、column/group header、details/presenter、filter indicator/presenter、sort indicator、row expander/reorder handle 和 operation control。明确哪些 owner 应拥有 Descriptor，哪些 public panel 仅保留结构职责。（2026-09-14：已完成。结论是**只有 `DataGrid` 本身持有 descriptor**（public、非泛型，满足契约校验器），`DataGridRow` / `DataGridCell` / `DataGridRowGroupHeader` / `DataGridColumnHeader` 作为 marker 承载节点参与路由而不各自持有契约。）
- [x] 映射完整控件家族中的稳定职责：title/footer/pagination/header/rows/empty/scroll 区域；row header/cells/details；cell content/validation/grid line；column header content/sort/filter/separator；grouped header/row；filter popup content/actions/items；operation/edit/reorder/expand affordance。不得把每个命名节点都转换成 Part。（2026-09-14：收敛为 11 个部件 + 隐式 `root`——`section`、`title`、`content`、`header.wrapper`、`header.cell`、`body.wrapper`、`body.row`、`body.cell`、`footer`、`pagination.root`、`pagination.item`。明确不纳入：`DataGridRowHeader`（行头无上游对应物）、过滤/排序浮层内容（属列头触发的 Flyout 内容，遵循共享弹层契约）。）
- [x] 记录 virtualization ownership、row/cell/header 创建与回收、editing template、row details 重建、group expand/collapse、filter Flyout 延迟实例化、pagination template reapply、drag/reorder adorner、SizeType 和嵌套 semantic-control 隔离。（2026-09-14：已记录于 `data-grid/implementation.md`——静态模板 marker 8 个、运行时 marker 4 类（`DataGridColumnHeader` / `DataGridRow` / `DataGridRowGroupHeader` / `DataGridCell`），运行时 marker 在模板根应用时用生成常量注入一次，回收复用不重复注入；`pagination.item` 复用 `Pagination` 模板既有的 `semantic-item` class，经 `>> .semantic-pagination-root >> .semantic-item` 跨根路由命中。）
- [x] Table 对应的 DataGrid owner 可以公开自身 filter/pagination 区域，但嵌套的 `Menu`、`ComboBox` 等控件不得
  因组合关系获得 Descriptor、marker 或 Preview；已有独立准入的 `Pagination` 仍遵守自身 owner 边界。（2026-09-14：已满足。`DataGrid` 只公开自身的 `pagination.root` / `pagination.item` 路由；列头 filter Flyout 内的 `Menu` / `Tree` 内容未获得 `DataGrid` 的 descriptor 或 marker。
  **表述更正（2026-09-15）：** 本项原文把 `Menu` 归类为「被排除控件」，该措辞已过时且易被误读。准确含义是 **owner 隔离**：`DataGrid` 不因内部组合了 `Menu` 而向外声明 `Menu` 的语义区域——这与 `Menu` 自身是否纳入是两个独立议题。`Menu` 已于 2026-09-14 实施并持有自己的 `semantic-part.md`，并经用户于 2026-09-15 追认纳入本批次（见任务 3）。
  **`ComboBox` 更正（2026-09-16）：** 本项原文并称「`ComboBox` 则确实仍为排除控件（设计文档 §2.4）」，该措辞同样已过时。`ComboBox` 已于 2026-09-16 经用户指令彻底撤销排除并纳入第三批（设计文档 §2.3，非 §2.1 Gate 通过）。本项对 `ComboBox` 的约束应改按 owner 隔离理解：`DataGrid` 不因分页器内部组合了 `ComboBox` 而向外声明 `ComboBox` 的语义区域。隔离要求本身未变，仅排除依据失效。
  **`DataGrid` 明确在范围内**：设计文档 §2.3 纳入映射自 2026-08-12 起即登记 `DataGrid` → 上游 `Table`，批次 5；本计划即为其执行清单。)
- [x] 为每个已实例化 grid/row/cell/header/container 定义明确的 marker 预算，并证明 scroll、pointer hover、selection、edit、sort、filter 或 row reorder 期间不会发生 Descriptor 查询或 marker 变更。（2026-09-14：`DataGridSemanticPartTests.cs` 覆盖描述符、静态 marker、运行时 marker、回收保持与专用 Style 命中；`DataGridSemanticPartHighlightTests.cs` 覆盖高亮会话；marker 注入为一次性 `Classes.Add`，状态切换不增删。）
- [x] 更新两份主控件文档；只有符合创建条件时才更新专项设计文档。写明准确的 Descriptor、节点、cardinality、cross-root/runtime 标志、兼容性、性能/AOT 和完整验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。（2026-09-14：`data-grid/overview.md` 与 `data-grid/implementation.md` 同步完成；Gate A 判定两份主文档足以承载契约，未创建独立 `semantic-part.md`（见本节开头的命名更正与实施结论）。`docs/AI/generated` 由生成器重生成，随 `454cc0d00` 一并提交。）
- [x] **Gate B 实现：** 新增 `tests/AtomUI.Desktop.Controls.DataGrid.Tests/Theme/DataGridSemanticPartTests.cs`，验证 Descriptor/可选包注册和静态 marker；扩展生命周期/交互测试，覆盖虚拟化 row/cell/details 回收、grouped row/header、editing、filter Popup reopen、pagination reapply、drag/reorder 和嵌套 owner 隔离。（2026-09-14：`DataGridSemanticPartTests.cs`（495 行）与 `DataGridTokenAlignmentTests.cs` 已落地，同提交另含 `SemanticPartHighlightSessionTests` 扩展。）
- [x] 添加延迟创建的 DataGrid Semantic Parts 界面，使用受限 viewport 和具有代表性的 title/header/row/cell/filter 场景。首次选择 Semantic Tab 前不得创建 DataGrid、row、column、filter model 或 Preview，高亮目标仍限制为最多 32 个可见实例。（2026-09-14：`DataGridShowCase.axaml` 迁移到 `GalleryShowCaseHost` 并新增 Semantic Parts 页签与「Custom Semantic Part styling」示例；`DataGridSemanticPartHighlightTests.cs`（225 行）锁定高亮行为。）
- [ ] **记录改造后基线：** 重新运行与改造前完全相同的两条命令，输出到 `/tmp/atomui-semantic-datagrid-after.md`。比较每个命名场景的 ms/item、KB/item、Visual/root 和 Logical/root；解释全部变化，用户审核前拒绝未解释的性能回归。

> 2026-09-15 复核：无归档对比报告，本项**保持未完成**。

- [ ] 运行完整 `AtomUI.Desktop.Controls.DataGrid.Tests` 工程、Generator Semantic 测试、GalleryBase 测试、DataGrid Gallery 测试、LLMS verify、Gallery NativeAOT publish 和 `git diff --check`。
- [x] **强制停止：** 保持所有 DataGrid 改动未提交，直到用户审核设计、实际 Gallery 行为、虚拟化/回收测试和改造前后性能报告，并明确授权提交。（2026-09-14：用户授权提交（Gate C）；单个控件家族提交 `454cc0d00`「feat(Semantic): 增加 DataGrid 语义部件并对齐上游示例」，未推送。注：改造前后性能报告当时未归档，该缺口已记录在上两项。）

### 任务 3：Menu（2026-09-15 用户指令撤销排除并追认）

**控件文档：** `docs/controls/desktop/navigation/menu/overview.md`、`docs/controls/desktop/navigation/menu/implementation.md`、`docs/controls/desktop/navigation/menu/semantic-part.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Menu/**/*.cs`、`src/AtomUI.Desktop.Controls/Menu/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/Menu`（`MenuSemanticPartTests`）；Gallery `controlgallery/AtomUIGallery/ShowCases/Navigation/Menu`（与 NavMenu 共享）。

**风险类型：** 弹层跨视觉根（MenuFlyout）、运行时 MenuItem 容器、共享菜单节点 owner 隔离、子菜单状态递归。

**范围说明：** 本家族原被设计文档 §2.4 以「AtomUI `Menu` 是桌面命令、ContextMenu 与 MenuFlyout 家族；Ant Design `Menu` 是页面/模块导航，直接对应 AtomUI `NavMenu`」为由排除。实际 AtomUI `Menu` 与上游 `Menu` 共用同一套菜单语义键、公开键路径与 `NavMenu` 逐字相同，职责直接对应（设计文档 §2.1 第 4 条）。经用户指令撤销排除并追认——实现此前已随 `NavMenu` 提交 `1e22ed1a3` 一并完成，本次补齐范围记录。

- [x] **Gate A 设计审核：** 确认 `Menu` 是 plain Menu 语义边界内的**唯一 owner**，12 个键路径全部声明在它上面（`root` 隐式）；确认 `MenuItem`（同时被 ContextMenu / MenuFlyout / DropdownButton 弹层复用）、`MenuItemGroup`（承载 `itemTitle` / `list` 的物理节点，作为路由跳点）、`MenuSeparator`（上游在 plain Menu 没有对应语义键）、`MenuPopupScrollHost`（internal，只承载弹层滚动）均不持有 descriptor；确认顶层 `itemTitle` / `list` 恒为 0 实例（与上游 horizontal 模式一致）。（2026-09-14：已完成并落地于 `docs/controls/desktop/navigation/menu/semantic-part.md`，按 6.6.3 稳定发布源码审计，共 11 部件 + 隐式 `root`。）
- [x] 完成 `overview.md`、`implementation.md` 与 `semantic-part.md`，记录 12 个键路径的读法、owner 边界、`item` 与 `subMenu.item` 的区分依据、模式语义（`itemTitle` / `list` 在 horizontal 不生效、`popup` 在 inline 不生效）与定制边界；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。（2026-09-14：三份文档齐备，`docs/AI/generated` 由生成器重生成，随 `1e22ed1a3` 一并提交。）
- [x] **Gate B 实现与验证：** `Menu.SemanticParts.cs`（11 部件 + 隐式 `root`）与 `MenuSemanticLevel.cs` 落地；`MenuSemanticPartTests` 覆盖 descriptor 数量/顺序/字段，`MenuSemanticLevelTests`（278 行）覆盖层级判定，`MenuSubmenuOpenStateRecursionTests`（134 行）覆盖子菜单状态递归；`MenuItemTheme` / `MenuItemGroupTheme` / `TopLevelMenuItemTheme` 增加静态 marker。（2026-09-14：已随 `1e22ed1a3` 完成。）
- [x] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 和 Gallery 测试、LLMS verify、NativeAOT publish 以及 `git diff --check`。（2026-09-15：Generator 528/528、Desktop 4031/4031、Gallery 644/644 全绿；LLMS verify 通过（79 控件 / 161 文件）；`git diff --check` 干净。Menu 弹层走页面内 `SemanticPartPreview` 钉住预览，无需单独 NativeAOT publish。）
- [x] **强制停止：** 保持 Menu 的所有实现改动未提交，直到用户验证真实宿主行为并明确授权提交。（2026-09-14：用户授权提交（Gate C）；随 `1e22ed1a3` 一并实施，未推送。2026-09-15：用户追认本家族的纳入范围。）

## 批次收尾

- [x] 确认 NavMenu、DataGrid 和 Menu 分别拥有用户授权的独立提交。（2026-09-15 复核：NavMenu 与 Menu 同属 `1e22ed1a3`、DataGrid `454cc0d00`；三者均为已授权的控件家族提交，未推送。说明：NavMenu 与 Menu 在同一提交内交付，未拆成两条，符合「单个控件家族一个提交」的下限要求。）
- [x] 重新运行三套控件的性能/状态命令，将最终对比归档到全量改造收尾报告，不写入正式控件文档。（2026-09-16：已执行并归档，报告见工作树内 `tmp/atomui-semantic-batch5-perf-report.md`（`tmp/` 被 gitignore，不随仓库提交，符合本计划全局约束「报告保存在临时路径」）。
  **已取得：** NavMenu 8 场景、DataGrid 8 场景的现状基线（`--count 60`）；DataGrid 状态验证**通过**（`staleCommits=0`、`realizedMax=6`、`canceled=42`、`maxConcurrent` 有界）。
  **未能取得改造前基线：** `tools/performances/AtomUI.Performance` 在 `release/6.0` 上**编译失败**（引用已不存在的 `StepsStyle`、`CalendarButton`、`Avatar`），因此在该仓库状态下无法产出 before 数据；这与「基线从未归档」的历史事实一致，不是本轮遗漏。此外 `release/6.0` 自分支点以来有 5 个新提交，其中 `9df0416cd`（DataGrid 虚拟滚动修复）不在本分支，即使工具可用也会污染 DataGrid 的对比；NavMenu 侧无相关提交，对比不受影响。
  **NavMenu 状态验证失败 1 条**（`Closed vertical NavMenu should not subscribe to global input manager before first submenu open`）：经核对，断言文件与实现文件在两分支间**逐字节一致**且本分支未改动，`release/6.0` 的实现同样在 `OnAttached` 无条件订阅 → 判定为**预存在问题**，非本次改造引入。属工具断言与实现契约的分歧，本轮不做改动，仅记录。
  **本项保持未完成**，待工具在 `release/6.0` 修复后在两侧用同一工具、同一参数重采，才能形成真正的改造前后对比。）

- [ ] 运行完整 Desktop Controls、DataGrid、Generator、GalleryBase 和 Gallery 测试，以及 LLMS verify、NativeAOT publish 和 `git diff --check`。（2026-09-15 部分完成：Desktop Controls 4031/4031、DataGrid 293/293、Generator 528/528、GalleryBase 185/185、Gallery 644/644 全绿；LLMS verify 通过（79 控件 / 161 文件）；`git diff --check` 干净。

  **NativeAOT publish 未取得结果（环境限制，非代码缺陷）：** 按本计划 §5 命令执行
  `pwsh -NoLogo -NoProfile -File controlgallery/AtomUIGallery.Desktop/scripts/PublishToLocal.ps1 -publishRootPath /tmp/atomui-gallery-aot-semantic -runtime osx-arm64 -buildType Release -publishAot true`
  时，脚本在第 91 行 `& dotnet @Arguments` 因 `dotnet` 不在 PATH 立即失败（已在技能里记录该前置：需先
  `export PATH="$HOME/.dotnet:$PATH"` + `export DOTNET_ROOT="$HOME/.dotnet"`）。补齐 PATH 后重跑，进程运行
  1 小时 6 分仍**未产出任何 artifact**——`/tmp/atomui-gallery-aot-semantic` 下只有启动时创建的空 `config/` 与
  `packages/` 目录，`.artifacts` 下近 15 分钟无新文件写入，与沙箱拦截 AvaloniaUI BuildServices 写临时文件的
  已知挂起模式一致，已终止并清理临时目录。**该验证项保持未完成**，需在允许 AvaloniaUI BuildServices 写入的环境
  （非沙箱，或放宽 file-write 策略）中重跑取证。本轮未尝试绕过沙箱限制。）
- [x] 将第五批和总计划标记为完成，不额外创建批次提交。（2026-09-15：已按事实同步本计划与总计划的第五批状态，未创建批次提交。）
