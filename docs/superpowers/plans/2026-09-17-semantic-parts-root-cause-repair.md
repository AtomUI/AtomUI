# Semantic Part Root-Cause Repair Implementation Plan

> **Execution:** Use superpowers:executing-plans in this session, task by task. The user explicitly prohibits subagents and commits. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 修复已复现的 Semantic Part 运行时、布局、资源问题，保留正常 UX 与公开主题契约。

**Architecture:** 菜单宿主拥有活动路径，共享 Popup 拥有真实遮罩生命周期，容器准备/释放维护语义作用域。布局默认值留在 ControlTheme，资源与运行时节点建立对称 acquire/release。独立控件采用不相交写集，构建测试串行。

**Tech Stack:** .NET 10、Avalonia 12.1.2、AtomUI、xUnit v3、Avalonia.Headless/Skia、编译 AXAML。

**Contract references:** [Semantic Part 系统设计](../../architecture/systems/theming/semantic-parts.md)；[Popup 钉住打开契约](../../controls/desktop/other/popup/popup-pinned-open-design.md)。原计划引用的独立 spec 不在当前工作树，不据此声称存在额外批准。

## Global Constraints

- 不创建 git commit，不改写已有提交。
- 不升级依赖，不重命名公共属性、事件、Token、Part、生成式 Style 或 route。
- 外层钉住、同级互斥、只维护当前活动链；生命周期关闭始终放行。
- 保留原始尺寸、限高、滚动条、嵌套滚动、滚动链、圆角和实际命中。
- 专用 Style 测试使用 AXAML；不以直接设置部件属性作为定制回退。
- 每项生产代码修改前必须记录对应失败测试；所有 .NET 构建/测试串行。
- 测试通过不自动代表 Gallery、NativeAOT 或性能验收通过。

## 2026-09-18 单代理重做进度

本轮未使用 subagent、未提交 commit。旧方案不作为实现依据；以下是当前工作树的证据，未覆盖项仍保留未完成状态。

- T01：移除向所有兄弟项的 pin 绑定；`MenuPinnedOpenScope` 管理同级活动项，切换前释放旧分支；
  `IsSubMenuOpen` coercion 替代关闭后同步回弹。新增兄弟往返、分组及 Flyout 路径回归。
- T02：共享 Popup 保留 light-dismiss 原始配置，以 coercion 推导有效值；兼容边界取得自身 registration，
  pin/unpin 不关闭重开 host；已验证实际空白点命中、两 Popup 隔离、原本禁用配置及 Opened/Closed 次数。
- T03/T05/T06/T07/T09/T10：本轮此前重做的裁剪所有权、显式 ListBox marker、按容器索引判断末项、
  Breadcrumb Margin、NumericUpDown 依赖刷新及 OTP buffer 释放保留；ListBox/Breadcrumb 新定制测试已改为编译 AXAML 专用 Style。
- T04：`SemanticLevel` 更新当前及已实现后代；prepare/clear 与显式容器 Parent 移除维护作用域。
  编译 AXAML 的 `DropdownButtonItemStyle` 证明迁移分组中的已实现/新增子项实际命中，回迁后旧 Style 退出。
- T08：完全删除 Panel 中对齐属性硬写和 standalone 测试专用 fallback；主题通过继承的 TextAlignment
  表达内容默认值，覆盖 Style 移除和 Mode/方向切换。
- T11：同批更新合并为可取消 DispatcherOperation；视觉树增量维护，detach 取消并释放，reattach 重建。
  原稳定 separator 加一个 item 被重复挂载 3 次，回归修复后为 0 次。
- 当前 Desktop Controls 整个项目：4,451 passed / 0 failed / 0 skipped；最后一轮定向组合：334 passed。
- osx-arm64 Gallery NativeAOT 发布与产物验证通过；保留第三方 ReactiveUI trim/AOT 和本机 dylib 版本警告。
- `test.py plan` 仍因已有 Rendering 测试项目没有 production selection rule 返回 blocked；消费项目改为显式串行验证。
- 消费回归：Controls 23、DataGrid 302、Docs.LLMsGenerator 23、Gallery 682 全部通过；最后补充的专用 Style 结构与真实外点关闭 4 项通过。
- GalleryBase 全项目 184 passed / 1 failed（代码高亮）；Rendering 恢复依赖后 68 passed / 4 failed（吸顶容器空引用）。
  从 HEAD 导出独立临时快照对照：Rendering 同样 4 项失败；代码高亮固定 3 轮交替 A/B 中，HEAD 与当前工作树均 1 failed / 2 passed。
  这些结果证明基线存在相同问题，不代表已修复；没有用重试成功覆盖初次失败记录。
- LLMS 使用生成器同步并验证 79 个控件 / 161 个文件；同时刷新已有 Segmented/TabControl 过期生成片段，未改其生产源码。
- `git diff --check` 通过；验证日志和 TRX 位于 `/tmp/atomui-semantic-repair-tests` 及对应 `/tmp/atomui-semantic-*.log`。
- 尚未完成：Gallery 真机交互矩阵、跨平台 native Popup 实测、所有源回归点的最终逐条验收。
  上一轮全仓库回归中的 Core namespace 约束与 TabControl XLIFF canonical 失败未在本轮修改，不能宣称全仓库绿色。

## Task T01: 菜单钉住活动链（P1）

**Files:** `src/AtomUI.Desktop.Controls/Flyouts/MenuFlyoutPresenter.cs`、`src/AtomUI.Desktop.Controls/Menu/MenuItem.cs`、必要的 `Menu.cs` / `MenuItemGroup.cs`；`tests/AtomUI.Desktop.Controls.Tests/Buttons/` 与 `Menu/`。

**Interfaces:** 消费现有 `IsPopupPinnedOpen` / `IsSubMenuOpen` / 容器生命周期；输出无同步兄弟回弹的当前活动链，不新增 public API。

- [ ] 增加两个兄弟子菜单、嵌套兄弟、叶子和分组测试；显示后先断言同层至多一个展开节点，随后验证关闭请求与导航。
- [ ] 用修复前独立复现确认原栈溢出；运行新测试并记录红灯，不用进程崩溃替代普通测试断言。
- [ ] 将钉住的有效状态限定到活动链，明确导航与 dismiss/生命周期关闭；删除冲突的全兄弟钉住与同步回弹路径。
- [ ] 验证 DropdownButton、SplitButton、普通 Menu/ContextMenu 的既有钉住与 detach/reopen 测试。
- [ ] 更新对应实现说明和 changelog，保留变更供审查。

## Task T02: 运行中 light-dismiss 同步（P1）

**Files:** `src/AtomUI.Desktop.Controls/Popup/Popup.cs`、`PopupReflectionExtensions.cs`（仅必要兼容边界）、`Flyouts/Flyout.cs`、`ComboBox/ComboBox.cs`、`Select/AbstractSelect.cs`、`Primitives/InfoPickerInput/InfoPickerInput.cs`；相应 pinned/overlay tests。

**Interfaces:** 消费已有 Popup pin 和 light-dismiss 配置；共享宿主使实际遮罩与有效值一致；不新增公共配置项。

- [ ] 对正常打开后 pin、预先 pin、取消 pin 增加实际遮罩和空白点 hit-test 断言；记录配置本来为 false 的恢复案例。
- [ ] 红灯必须重现属性 false 但遮罩仍命中的原始场景。
- [ ] 将配置与 effective 状态分开并在共享物理宿主收敛，保证原业务 IsOpen、事件、焦点/选择不被人为重开破坏。
- [ ] 清理触及 owner 中重复或会覆盖调用方配置的 pin 设置；兼容边界变更需单独 AOT 证据。
- [ ] 验证 native/overlay、嵌套 popup、取消 pin、detach、模板替换和关闭动效；同步 Popup 契约文档。

## Task T03: Child 裁剪所有权（P2）

**Files:** `src/AtomUI.Controls/Primitives/DashedBorder.cs`；`tests/AtomUI.Desktop.Controls.Tests/Primitives/DashedBorderClipContentTests.cs`；必要的 `tests/AtomUI.Desktop.Controls.Rendering.Tests/` 回归。

**Interfaces:** 保持 `ClipContentToCornerRadius`，仅改变内部 Child/geometry acquire/release。

- [ ] 测试旧 Child 移除/替换、开关裁剪和后来用户写入的 Clip；添加 100×80 → 300×200 复用真实几何命中回归。
- [ ] 确认旧对象残留 Clip 的红灯。
- [ ] 在 Child 变化和释放时按目标与几何身份归还自己的裁剪，不清除不属于自己的新值。
- [ ] 验证 Expander、ListBox、ListView 原圆角与输入契约不退化，保持无几何 hit-test 后端的既有 fallback。

## Task T04: 菜单语义作用域迁移（P2，T01 后）

**Files:** `Menu/MenuSemanticLevel.cs`、`Menu.cs`、`MenuItem.cs`、`MenuItemGroup.cs`、`Flyouts/MenuFlyoutPresenter.cs`；菜单层级/Dropdown Semantic tests 与 AXAML fixture。

**Interfaces:** 使用现有 `MenuSemanticLevel` 与生成 class 常量，进入和退出 owner 均有明确处理。

- [ ] AXAML 使用 `DropdownButtonItemStyle` 同时设置 Tag 与外观，验证新建/从 Menu 迁移的分支及新添加后代一致。
- [ ] 确认旧 TopLevel/SubMenu 残留导致子项 marker 错误的红灯。
- [ ] prepare 明确新作用域，clear 释放旧作用域；覆盖已生成与延迟生成后代、分组及双向迁移。
- [ ] 重跑 T01 验证层级处理不会重新引入开合递归。

## Task T05: ListBox 显式容器 marker（P2）

**Files:** `ListBox/ListBox.cs`；`tests/AtomUI.Desktop.Controls.Tests/ListBox/` 与编译 AXAML fixture。

- [ ] 相同 `ListBoxItemStyle` 对显式和生成容器设置相同背景，先确认显式容器红灯。
- [ ] 在合适的 prepare/clear 路径维护 owner 的 marker；不要无条件污染 CandidateList、Transfer 等独立派生语义。
- [ ] 验证移除、重新添加、回收与既有 Candidate_List_Containers_Do_Not_Carry_The_ListBox_Item_Marker。

## Task T06: 列表末项按视图索引定位（P2，T05 后）

**Files:** `ListBox/ListBox.cs`、`ListView/ListView.cs`、必要的 `ListView.Selecting.cs`；对应 Semantic/container tests。

- [ ] 增加整数 [1,2,3] 和引用 [same,other,same]；期望 divider [true,true,false]，再覆盖 append/remove/move 与分页边界。
- [ ] 记录引用比较的两个失败结果。
- [ ] 基于当前视图索引/IndexFromContainer 更新 prepare、索引变化和集合刷新，不使用数据项 IndexOf 或 Equals。
- [ ] 保持 borderless、group header、BottomPagination 与虚拟化契约；消费者包括 CandidateList/Transfer。

## Task T07: Breadcrumb Margin（P2）

**Files:** `Breadcrumb/BreadcrumbItemsPanel.cs`；Breadcrumb Semantic tests 与 AXAML fixture。

- [ ] 两个 item 宽40、separator宽10；专用 Style 设置 Margin=10,0，总宽必须110、separator X=50；补非对称/垂直对齐测试。
- [ ] 确认当前总宽130、X=60红灯。
- [ ] Measure 使用包含 Margin 的 DesiredSize，Arrange 传递外部布局槽，不重复相加/偏移。
- [ ] 验证 root padding/border、有限宽度和正常菜单点击区域，不改 Token 伪装修复。

## Task T08: Timeline 对齐主题优先级（P2）

**Files:** `src/AtomUI.Controls/Timeline/TimelineSectionPanel.cs`、`src/AtomUI.Desktop.Controls/Timeline/Themes/TimelineItemTheme.axaml`；Timeline tests/AXAML fixture。

- [ ] 编译 AXAML 专用 header/content Style 设置 Right，title Style 设置 Center，并用 Tag 同时证明 selector 命中。
- [ ] 先验证当前 LocalValue 把用户样式压回 Stretch/Right。
- [ ] 把等价默认对齐放入 ControlTheme，移除布局代码硬写；运行时计算无法声明时只保留必要的内部状态，不接管公开 Part 的 LocalValue。
- [ ] 验证移除 Style 恢复默认，以及方向、Mode、Label/Alternate/Reverse 的切换和原视觉基线。

## Task T09: NumericUpDown suffix 完整派生状态（P2）

**Files:** `NumericUpDown/NumericUpDown.cs`、必要的 `NumericUpDown/Themes/NumericUpDownTheme.axaml`、`ButtonSpinner/ButtonSpinnerDecoratedBox.cs`；NumericUpDown tests。

- [ ] hover 中 Middle → Small：有效右 padding 11→7 后位移立即 -16.5→-10.5，无需移出重入。
- [ ] 验证红灯；加入浮动开关、位置、有效 padding 变化和重模板/reattach。
- [ ] 优先主题消费计算值；确需代码时订阅全部依赖并在 retemplate/detach 释放，Transitions 留在主题 Setter。
- [ ] 验证 clear button、suffix 内容尺寸、只读/禁用和现有动画，不做每动画帧反向回写。

## Task T10: OTP 缓冲区释放（资源正确性）

**Files:** `OtpLineEdit/OtpSeparatorPresenter.cs`；OTP tests/既有资源规范测试。

- [ ] 用实际依赖确认 GlyphRun.Dispose 不释放 ShapedBuffer；增加本控件使用范围/退出路径的资源所有权测试。
- [ ] 确认未释放路径红灯，再使用明确 using 生命周期确保所有返回/异常路径归还缓冲区。
- [ ] 保持 GlyphRun 先于 shaped 释放；验证文本、空文本、无有效墨迹、字体/字号变化后的居中基线。

## Task T11: Breadcrumb 增量 separator 更新（P3，T07 后）

**Files:** `Breadcrumb/Breadcrumb.cs`、`BreadcrumbSeparatorManager.cs`、`BreadcrumbItemsPanel.cs`；Breadcrumb tests。

- [ ] 给稳定 separator 订阅 attach/detach，添加一个 item 不得反复拆挂原节点；记录旧基线3次拆挂。
- [ ] 合并同一批更新，按身份和顺序增量维护 visual child，去掉无变化时全量清空、重新挂载和分配。
- [ ] 验证增删移动、模板重建、owner脱离后的待调度更新、样式/资源持续响应。
- [ ] 报告结构工作量与可测性能；没有收益不保留增加复杂度的纯性能改动。

## Task T12: 整体验收

- [ ] 记录每任务红/绿命令与结果，合并审查公共 API、主题优先级和生命周期 acquire/release。
- [ ] `python3 scripts/verification/test.py plan` 检查映射；迭代使用 `run --scope iterate`，最终执行 `run` 验证直接与消费影响。
- [ ] 独立复现验证原菜单进程不再退出134，所有第一/第二轮失败点均复核。
- [ ] Gallery 实机矩阵：菜单兄弟/嵌套导航，pin切换页面空白点击，裁剪区域输入，列表滚动与边界，Timeline主题切换，Numeric hover切尺寸。
- [ ] 如果触及 Popup 兼容边界，执行本地 osx-arm64 Gallery NativeAOT publish；不得用静态反射说明代替发布证据。
- [ ] 同步受影响控件 implementation/semantic-part/changelog，运行 LLMS verify，必要时用生成器重新生成聚合文档。
- [ ] `git diff --check`；输出完成/未完成/专项风险，所有改动留待用户审查，不提交。
