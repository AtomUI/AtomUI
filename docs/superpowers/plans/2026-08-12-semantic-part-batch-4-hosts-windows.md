# Semantic Part 第四批 Popup 与独立宿主实施计划

> **供智能体执行者使用：** 使用 `superpowers:executing-plans` 在当前会话中执行，不得使用 subagent。每个控件修改源码前都必须明确 Visual root ownership 并获得批准。

**目标：** 为 10 个具有 Ant Design 6.6.0 稳定发布源码公开 Semantic DOM 对应 API 的 Popup、Overlay 和服务宿主控件家族建立 Semantic Part 契约，并覆盖完整生命周期与多 root 隔离。2026-09-15 用户指令新增任务 11 `SplitButton`（原排除判定撤销），随本计划执行。

**架构：** 每个生产 owner 通过现有 host/session 生命周期公开 selector Part。Gallery 可以提供由示例显式拥有的 additional root，但生产控件不得引入 Preview API、全局 root registry 或运行时搜索。服务型控件使用真实 owner 边界。

**技术栈：** .NET 10、Avalonia 12、AtomUI Desktop Controls、AXAML、xUnit v3、Avalonia Headless、平台宿主、AtomUI Gallery、NativeAOT。

## 全局约束

- 遵循[全量改造总计划](2026-08-12-semantic-part-control-rollout.md)和[全量改造设计](../specs/2026-08-12-semantic-part-control-rollout-design.md)。
- Gate A 必须列明每个 Visual root、owner、创建点、attach/open 状态转换和 close/detach 释放路径。
- 不得向生产控件添加全局 TopLevel registry、服务查找或 Preview 专用属性、事件、接口。
- Popup/Overlay 以及服务所使用的 Window host 测试覆盖打开、关闭、重新打开、owner detach 和多宿主隔离；不得因此给
  被排除的 `Window` / `WindowTitleBar` owner 增加 Semantic Part。
- Gallery additional root 必须是示例显式提供的输入，并且只能在打开 Semantic Parts Tab 后创建。
- Gate B 改动保持未提交，直到用户明确完成验证并授权提交。

---

### 任务 1：ImagePreviewer

**控件文档：** `docs/controls/desktop/data-display/image-previewer/overview.md`, `docs/controls/desktop/data-display/image-previewer/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/ImagePreviewer/**/*.cs`, 全部 `Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/ImagePreviewer`；Gallery `controlgallery/AtomUIGallery/ShowCases/DataDisplay/ImagePreviewer`.

**风险类型：** Overlay/Dialog 宿主、多个 public 子控件、异步图片加载、renderer 和 motion。

- [x] **Gate A 设计审核：** 审计 previewer/group/dialog/overlay host、viewer/renderer、title bar、toolbar/nav/cover/item 的 owner；确认 image viewport、loading/error、title、toolbar/nav/close/mask 区域，明确 service/dialog/overlay Visual root、source 切换和释放路径。
- [x] 更新两份控件文档，写明准确的 Descriptor、cross-root/runtime 标志、owner/session 生命周期、真实节点、兼容性和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/ImagePreviewer/ImagePreviewerSemanticPartTests.cs`，覆盖 single/group、loading/error/success、navigation、toolbar/title、overlay/dialog open-close-reopen、source 替换和 owner 隔离；Gallery 使用显式 additional root，并执行 NativeAOT 验证。
- [x] 运行 Generator Semantic 测试、目标宿主/控件测试、GalleryBase 和 Gallery 测试、LLMS verify、NativeAOT publish 以及 `git diff --check`；当契约依赖原生/窗口行为时执行平台冒烟检查。
- [x] **强制停止：** 保持 ImagePreviewer 的所有实现改动未提交，直到用户验证真实宿主行为并明确授权提交。

### 任务 2：InfoFlyout

**控件文档：** `docs/controls/desktop/data-display/info-flyout/overview.md`, `docs/controls/desktop/data-display/info-flyout/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Flyouts/**/*.cs`, `src/AtomUI.Desktop.Controls/Flyouts/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/Flyouts`；Gallery `controlgallery/AtomUIGallery/ShowCases/DataDisplay/InfoFlyout`.

**风险类型：** PopupFlyoutBase、延迟创建的 Popup、menu/tree presenter、资源生命周期。

- [x] **Gate A 设计审核：** 审计 `FlyoutHost`、Flyout/FlyoutPresenter、Menu/TreeView flyout presenter 的 owner；确认 anchor content 与 popup arrow/surface/content/item 区域，区分 host root 与 Flyout presenter root，记录 Popup 延迟创建、overlay 与 PopupRoot 差异、open-close 和 resource bridge。
- [x] 更新两份控件文档，写明准确的 Descriptor、cross-root/runtime 标志、owner/session 生命周期、真实节点、兼容性和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/Flyouts/FlyoutSemanticPartTests.cs`，覆盖 content/menu/tree variants、arrow placements/flips、overlay/PopupRoot、open-close-reopen、presenter 资源生命周期 和 item containers。
- [x] 运行 Generator Semantic 测试、目标宿主/控件测试、GalleryBase 和 Gallery 测试、LLMS verify、NativeAOT publish 以及 `git diff --check`；当契约依赖原生/窗口行为时执行平台冒烟检查。
- [x] **强制停止：** 保持 InfoFlyout 的所有实现改动未提交，直到用户验证真实宿主行为并明确授权提交。

### 任务 3：ToolTip

**控件文档：** `docs/controls/desktop/data-display/tooltip/overview.md`, `docs/controls/desktop/data-display/tooltip/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Tooltip/*.cs`, `src/AtomUI.Desktop.Controls/Tooltip/Themes/ToolTipTheme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/Tooltip`；Gallery `controlgallery/AtomUIGallery/ShowCases/DataDisplay/Tooltip`.

**风险类型：** Attached service、Popup/overlay 模式、延迟、detached owner。

- [x] **Gate A 设计审核：** 审计 ToolTip instance、attached `ToolTipService` 和 OverflowTip owner，确认 content/arrow/surface regions、service-created tooltip ownership 和 Popup modes；记录 delay timers、anchor detach、overlay vs PopupRoot 和 repeated show/hide。
- [x] 更新两份控件文档，写明准确的 Descriptor、cross-root/runtime 标志、owner/session 生命周期、真实节点、兼容性和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/Tooltip/ToolTipSemanticPartTests.cs`，覆盖 explicit/service/overflow tooltip、placement、overlay/PopupRoot、show-hide-reopen、delay cancellation 和 detach cleanup；Gallery additional root 只存在于示例侧。
- [x] 运行 Generator Semantic 测试、目标宿主/控件测试、GalleryBase 和 Gallery 测试、LLMS verify、NativeAOT publish 以及 `git diff --check`；当契约依赖原生/窗口行为时执行平台冒烟检查。
- [x] **强制停止：** 保持 ToolTip 的所有实现改动未提交，直到用户验证真实宿主行为并明确授权提交。

### 任务 4：Tour

**控件文档：** `docs/controls/desktop/data-display/tour/overview.md`, `docs/controls/desktop/data-display/tour/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Tour/*.cs`, `src/AtomUI.Desktop.Controls/Tour/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/Tour`；Gallery `controlgallery/AtomUIGallery/ShowCases/DataDisplay/Tour`.

**风险类型：** Popup 与 overlay mask、step 容器、target tracking、motion。

- [x] **Gate A 设计审核：** 审计 Tour/TourLayer、step/steps view、indicators 和 Popup owner；确认 mask/spotlight、panel/title/content/actions/indicator/arrow regions，记录 target switch、placement, step rebuild, open-close 和 target detach。
- [x] 更新两份控件文档，写明准确的 Descriptor、cross-root/runtime 标志、owner/session 生命周期、真实节点、兼容性和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/Tour/TourSemanticPartTests.cs`，覆盖 steps navigation、default/text indicator、placement、target change/detach、Popup reopen、overlay mask 和 step collection lifecycle；验证 NativeAOT 和 additional root。
- [x] 运行 Generator Semantic 测试、目标宿主/控件测试、GalleryBase 和 Gallery 测试、LLMS verify、NativeAOT publish 以及 `git diff --check`；当契约依赖原生/窗口行为时执行平台冒烟检查。
- [x] **强制停止：** 保持 Tour 的所有实现改动未提交，直到用户验证真实宿主行为并明确授权提交。

### 任务 5：Drawer

**控件文档：** `docs/controls/desktop/feedback/drawer/overview.md`, `docs/controls/desktop/feedback/drawer/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Drawer/*.cs`, `src/AtomUI.Desktop.Controls/Drawer/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/Drawer`；Gallery `controlgallery/AtomUIGallery/ShowCases/Feedback/Drawer`.

**风险类型：** Window overlay layer、运行时容器、stacked session、motion。

- [x] **Gate A 设计审核：** 审计 Drawer owner、DrawerContainer/InfoContainer 和 overlay layer；确认 mask/panel/header/title/extra/content/footer/close regions，明确 nested/stacked drawers、placement, motion, drawn-titlebar overlay bounds 和 teardown。
- [x] 更新两份控件文档，写明准确的 Descriptor、cross-root/runtime 标志、owner/session 生命周期、真实节点、兼容性和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/Drawer/DrawerSemanticPartTests.cs`，覆盖四种 placement、header/footer/close 变体、stacked/open-close-reopen、owner Window detach，并证明不会保留旧容器；验证 Gallery additional root 和 NativeAOT。
- [x] 运行 Generator Semantic 测试、目标宿主/控件测试、GalleryBase 和 Gallery 测试、LLMS verify、NativeAOT publish 以及 `git diff --check`；当契约依赖原生/窗口行为时执行平台冒烟检查。
- [x] **强制停止：** 保持 Drawer 的所有实现改动未提交，直到用户验证真实宿主行为并明确授权提交。

### 任务 6：DropdownButton

**控件文档：** `docs/controls/desktop/navigation/dropdown-button/overview.md`、`docs/controls/desktop/navigation/dropdown-button/implementation.md`、`docs/controls/desktop/navigation/dropdown-button/semantic-part.md`

**证据范围：** `src/AtomUI.Desktop.Controls/DropdownButton/**/*.cs`、`src/AtomUI.Desktop.Controls/Buttons/Themes/DropdownButton*Theme.axaml`、共享 MenuFlyout / MenuItem marker 注入路径；测试 `tests/AtomUI.Desktop.Controls.Tests/DropdownButton`；Gallery `controlgallery/AtomUIGallery/ShowCases/Navigation/DropdownButton`。

**风险类型：** MenuFlyout 跨视觉根、子菜单嵌套视觉根、运行时 MenuItem 容器、共享菜单节点的 owner 隔离。

- [x] **Gate A 设计审核：** 将 AtomUI `DropdownButton` 作为直接拥有下拉命令弹层的 public owner 映射到上游 `Dropdown`，不映射 deprecated `Dropdown.Button` 的 split-trigger 组合；确认触发侧不发布 Part，上游弹层 `root` 映射为 `popup.root`，菜单项区域由同一 owner 的运行时 marker 承载。
- [x] 完成 `overview.md`、`implementation.md` 与 `semantic-part.md`，记录 `popup.root` / `itemTitle` / `item` / `itemContent` / `itemIcon` 的 route、cardinality、跨根与嵌套 owner 边界。
- [x] **Gate B 实现与验证：** `DropdownButtonSemanticPartTests` 覆盖 descriptor、marker 注入、顶层与嵌套菜单项、owner-scoped Style 命中、打开/关闭/重开和 pinned-open 生命周期；Gallery 页面与高亮测试覆盖全部公开 Part。
- [x] **已授权提交：** `6abaf6100` 已交付 descriptor、控件文档、运行时 marker、测试与 Gallery 示例。

### 任务 7：Message

**控件文档：** `docs/controls/desktop/feedback/message/overview.md`, `docs/controls/desktop/feedback/message/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Message/*.cs`、`src/AtomUI.Desktop.Controls/Message/Themes/*Theme.axaml`；在 `tests/AtomUI.Desktop.Controls.Tests/Message` 下创建测试；Gallery `controlgallery/AtomUIGallery/ShowCases/Feedback/Message`。

**风险类型：** 服务创建的 Window 宿主、运行时 card、queue 和 motion。

- [x] **Gate A 设计审核：** 审计 `Message` service/session、WindowMessageManager 和 public `MessageCard` owner；确认 icon/content/action/status surface 区域，明确 manager host layer、多个 message stack、timeout/manual close、motion 和 Window detach。
- [x] 更新两份控件文档，写明准确的 Descriptor、cross-root/runtime 标志、owner/session 生命周期、真实节点、兼容性和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 创建 `tests/AtomUI.Desktop.Controls.Tests/Message/MessageSemanticPartTests.cs`，覆盖全部类型、custom content/icon、多项 queue、timeout/manual close、host attach/detach 和 card 保留检查；Gallery 只在选择 Tab 后创建显式示例 root，并执行 NativeAOT 验证。
- [x] 运行 Generator Semantic 测试、目标宿主/控件测试、GalleryBase 和 Gallery 测试、LLMS verify、NativeAOT publish 以及 `git diff --check`；当契约依赖原生/窗口行为时执行平台冒烟检查。
- [x] **强制停止：** 保持 Message 的所有实现改动未提交，直到用户验证真实宿主行为并明确授权提交。

> 2026-09-12 状态复核：任务 7 的勾选长期滞后于实际进度——Message 家族已由 `be91097d0`（增加 Message 语义部件、
> 对齐上游示例并统一 Semantic Part 示例标题）与 `ebe687f89`（修复 Message 语义示例对齐并移除高亮白环）交付并经用户
> 授权提交；`docs/controls/desktop/feedback/message/semantic-part.md`、`MessageSemanticPartTests` 与 Gallery 示例均在
> 仓库中，且包含在历次全量测试通过范围内。本条目据此补记为完成。

### 任务 8：Modal / Dialog

**控件文档：** `docs/controls/desktop/feedback/modal/overview.md`、`docs/controls/desktop/feedback/modal/implementation.md`；只有已批准的 Part 模型改变稳定的尺寸契约时，才更新现有 `host-sizing-design.md`。

**证据范围：** `src/AtomUI.Desktop.Controls/Dialog/**/*.cs`、`src/AtomUI.Desktop.Controls/MessageBox/**/*.cs` 和全部相关 Dialog/MessageBox 主题；测试 `tests/AtomUI.Desktop.Controls.Tests/Dialog` 和 `MessageBox`；Gallery `controlgallery/AtomUIGallery/ShowCases/Feedback/Modal`。

**风险类型：** Overlay 和 native Window 宿主、session state machine、多个 public 子控件、resize/motion。

- [x] **Gate A 设计审核：** 审计 Dialog/MessageBox owner、DialogSurface、button box/header/resizer、overlay presenter/mask 和 window presenter；确认 surface/title/icon/content/footer/actions/close/mask/resize regions 和 owner 覆盖 Overlay/Window，记录 session open-close, teardown, host sizing 和 nested dialogs。
- [x] 更新两份控件文档，写明准确的 Descriptor、cross-root/runtime 标志、owner/session 生命周期、真实节点、兼容性和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/Dialog/DialogSemanticPartTests.cs` 并扩展 MessageBox 测试，覆盖 Overlay/Window、modal/modeless、MessageBox 类型、button、resize/motion、嵌套 session、close failure 和 root release；验证 Gallery additional root 和 NativeAOT。
- [x] 运行 Generator Semantic 测试、目标宿主/控件测试、GalleryBase 和 Gallery 测试、LLMS verify、NativeAOT publish 以及 `git diff --check`；当契约依赖原生/窗口行为时执行平台冒烟检查。
- [x] **强制停止：** 保持 Modal / Dialog 的所有实现改动未提交，直到用户验证真实宿主行为并明确授权提交。

> 2026-09-12 任务 8 收尾：实现与验证由 `7271b0a95`（用户 amend 并入 Core using 清理）与 `327346ea1`（示例文案修正）
> 交付。`DialogSemanticPartTests` 14 用例覆盖 descriptor/marker/逻辑父/cross-root/精确单节点命中（Dialog 与
> MessageBox 各自）/`OverlayScope`/钉住门控/Optional 语义；Gallery 新增 `ModalSemanticPartHighlightTests`
> 运行期高亮回归（两个预览 × 9 部件逐张悬停恰好一个 Adorner；`root` 依赖舞台 owner 显式铺满——Dialog 主题默认零尺寸，
> resolver 对零尺寸目标不建 Adorner）。真机视觉验收见
> `docs/superpowers/specs/2026-09-12-dialog-semantic-visual-acceptance.md`（用户回传截图判定 1–4 项通过，
> 悬停高亮由自动化覆盖）。随后补齐第三个**原生 Window 宿主预览**：`GetCrossRoots()` 在该宿主上报 `DialogWindow`
> （`WindowDialogPresenter` 于显示/Opened/teardown 触发 `CrossRootsChanged`），高亮落在原生窗口自己的
> AdornerLayer；运行期诊断证实 `WindowDialogPresenter` 无条件隐藏 Surface 标题栏（原生 caption 独占），
> `header`/`title`/`close` 在该宿主不物化，据此把三者修正为 `Optional`（连同 `mask`/`wrapper` 共五个），
> `footer` 保持 `Single`。窗口预览最终形态：**不默认打开**；触发按钮经用户截图指正后位于 Examples 页签
> 「自定义语义结构的样式」卡片按钮行（Open Modal / Open MessageBox 之后的第三、四个按钮）——「打开窗口 Dialog」
> 用于语义预览部件高亮（两页签内容常驻，切页不关闭），
> 「打开样式化窗口」打开 owner 实例级 Semantic Style 定制的窗口 Dialog（`container`/`body`/`footer`，该宿主只物化
> 这三个部件）。**样式级联实证修正**：`Window_Host_Owner_Scoped_Semantic_Styles_Cascade_Via_Logical_Parent`
> 证明 owner 实例级 Semantic Style 经逻辑父链（`DialogWindow` 挂 owner 逻辑树下）在 Window 宿主同样命中，推翻了
> 早前「只在 Overlay 宿主保证命中」的文档结论。样式化 Dialog 必须位于 `PreviewContent` 之外——语义预览按 owner
> 类型多实例解析，同一 Preview 内容内的第二个 Dialog 实例会被一并高亮（高亮回归实证：root 悬停出现两个 Adorner）。
> 窗口宿主显式 `HostWidth=360/HostHeight=220`（自然测量过小）。Desktop Controls 3737/3737、Gallery 621/621、
> Generator 532/532、LLMS 23/23、
> NativeAOT `osx-arm64` 通过；`git diff --check` 干净。既有间歇性 GC/时序抖动（`Closed_Overlay_Session_Releases_Surface…`、
> `PopupConfirm_In_Dialog_Confirms…`）经基线提交 `f86c5b772` 双跑复现确认为**先于本批存在**，非本次引入。

### 任务 9：Notification

**控件文档：** `docs/controls/desktop/feedback/notification/overview.md`, `docs/controls/desktop/feedback/notification/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Notifications/*.cs`, `src/AtomUI.Desktop.Controls/Notifications/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/Notifications`；Gallery `controlgallery/AtomUIGallery/ShowCases/Feedback/Notification`.

**风险类型：** 服务创建的 Window 宿主、card/progress 运行时节点、queue 和 placement。

- [x] **Gate A 设计审核：** 审计 Notification/session、WindowNotificationManager、NotificationCard/ProgressBar owner；确认 icon/message/description/action/close/progress/surface regions，记录 placements、stack, duration/progress, manual close, replacement 和 Window detach。用户已确认结构对齐（重构模板到上游 notice DOM）并一并引入 `Actions` API。
- [x] 更新两份控件文档（并新增 `semantic-part.md`），写明准确的 Descriptor、cross-root/runtime 标志、owner/session 生命周期、真实节点、兼容性和验证矩阵；运行 LLMS verify 和 `git diff --check`。
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/Notifications/NotificationSemanticPartTests.cs`，覆盖 type/placement、custom content/action、progress/duration、多项 queue、close/detach 和 card 保留检查；Gallery 新增 Semantic Parts 双 owner 预览、Actions 与 Semantic Part styling 示例并完成 NativeAOT 验证。
- [x] 运行 Generator Semantic 测试、目标宿主/控件测试、GalleryBase 和 Gallery 测试、LLMS verify、NativeAOT publish 以及 `git diff --check`。
- [x] **强制停止：** Notification 的实现改动已通过用户授权提交，真机视觉验收通过后不再保持未提交状态。

### 任务 10：PopupConfirm

**控件文档：** `docs/controls/desktop/feedback/popup-confirm/overview.md`, `docs/controls/desktop/feedback/popup-confirm/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/PopupConfirm/*.cs`、`src/AtomUI.Desktop.Controls/PopupConfirm/Themes/*Theme.axaml` 及已批准的 Flyout/Button 共享契约；在 `tests/AtomUI.Desktop.Controls.Tests/PopupConfirm` 下创建测试；Gallery `controlgallery/AtomUIGallery/ShowCases/Feedback/PopupConfirm`。

**风险类型：** 派生自 FlyoutHost 的 Popup、运行时 presenter binding、action Button。

- [x] **Gate A 设计审核：** 审计 PopupConfirm host、PopupConfirmFlyout 和 container owner；确认 icon/title/content/actions/surface/arrow regions，避免穿透 nested Button descriptor，记录 relay binding acquire/release, Popup reopen 和 confirm loading。
- [x] 更新两份控件文档，写明准确的 Descriptor、cross-root/runtime 标志、owner/session 生命周期、真实节点、兼容性和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 创建 `tests/AtomUI.Desktop.Controls.Tests/PopupConfirm/PopupConfirmSemanticPartTests.cs`，覆盖 status/icon/content/cancel visibility、confirm loading、open-close-reopen、presenter binding disposal 和 nested Button owner 隔离；NativeAOT。
- [x] 运行 Generator Semantic 测试、目标宿主/控件测试、GalleryBase 和 Gallery 测试、LLMS verify、NativeAOT publish 以及 `git diff --check`；当契约依赖原生/窗口行为时执行平台冒烟检查。
- [x] **强制停止：** 保持 PopupConfirm 的所有实现改动未提交，直到用户验证真实宿主行为并明确授权提交。

### 任务 11：SplitButton（2026-09-15 用户指令新增，原排除判定撤销）

**控件文档：** `docs/controls/desktop/general/split-button/overview.md`、`docs/controls/desktop/general/split-button/implementation.md`、`docs/controls/desktop/general/split-button/semantic-part.md`（Gate A 新增）

**证据范围：** `src/AtomUI.Desktop.Controls/SplitButton/**/*.cs`、`src/AtomUI.Desktop.Controls/Buttons/Themes/SplitButtonTheme.axaml`、共享 MenuFlyout / MenuItem / MenuItemGroup marker 注入路径（注入点无条件、owner 隔离靠生成 Style 的逻辑祖先链，同 DropdownButton）；测试 `tests/AtomUI.Desktop.Controls.Tests/Buttons`（新增 `SplitButtonSemanticPartTests`）；Gallery `controlgallery/AtomUIGallery/ShowCases/General/SplitButton`。

**风险类型：** MenuFlyout 跨视觉根（弹层锚定模板内 `PART_SecondaryButton`）、运行时 MenuItem 容器、共享菜单节点 owner 隔离、primary 形态接缝分隔线视觉对齐。

- [x] **Gate A 设计审核：** 将 AtomUI `SplitButton` 作为直接拥有下拉命令弹层的 public owner 映射到上游 `Dropdown` 的弹层 Semantic DOM（`root` / `itemTitle` / `item` / `itemContent` / `itemIcon`），不映射 deprecated `Dropdown.Button` 的 split-trigger 组合；上游弹层 `root` 映射为 `popup.root`；触发侧补充发布 `primary` / `secondary`（`PART_PrimaryButton` / `PART_SecondaryButton`，静态 marker）——上游消费者自持触发按钮、AtomUI 触发按钮为模板内部件，属显式能力补充；复核 primary 形态接缝分隔线与 antd `button/style/compact.ts` 的差距（分隔线颜色 `colorPrimaryHover` / `colorErrorHover`、次按钮 hover 时隐藏）。（2026-09-15：已完成并落地为 7 部件契约，见 `general/split-button/semantic-part.md`。）
- [x] 完成 `overview.md`、`implementation.md` 与新增 `semantic-part.md`，记录 `primary` / `secondary` / `popup.root` / `itemTitle` / `item` / `itemContent` / `itemIcon` 的 route、cardinality、跨根与嵌套 owner 边界及触发侧补充发布的理由；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。（2026-09-15：三份文档同步完成，`docs/AI/generated` 由生成器重生成，随 `44cd1494a` 一并提交。）
- [x] **Gate B 实现与验证：** `SplitButtonSemanticPartTests` 覆盖 descriptor（7 部件形态）、触发侧静态 marker（`primary` / `secondary`）与模板变体覆盖、pinned-open 弹层物化（含声明式子菜单）、owner-scoped 生成 Style 命中、light-dismiss 禁用、分隔线颜色与 hover 隐藏回归；Gallery 页面新增 Semantic Parts Tab（`IsPopupPinnedOpen` 钉住 + 弹层根显式注册 `AdditionalRoots`）与 SemanticStyles 示例（专用生成 Style 类）。（2026-09-15：`tests/AtomUI.Desktop.Controls.Tests/Buttons/SplitButtonSemanticPartTests.cs` 已落地，同目录另含 `SplitButtonSeparatorStabilityTests`、`SplitButtonPinnedSubMenuPopupTests`、`SplitButtonLayoutTests`；Gallery 侧含 `SplitButtonShowCasePageTests` 与 `SplitButtonSemanticPartHighlightTests`。）
- [ ] 运行 Generator Semantic 测试、目标控件测试、GalleryBase 和 Gallery 测试、LLMS verify、NativeAOT publish 以及 `git diff --check`。
- [x] **强制停止：** 保持 SplitButton 的所有实现改动未提交，直到用户验证真实宿主行为并明确授权提交。（2026-09-15：用户授权提交（Gate C）；单个控件家族提交 `44cd1494a`「feat(Semantic): 增加 SplitButton 语义部件并修复弹层钉住与子菜单状态递归」，未推送。）

## 批次收尾

> 2026-09-11 Modal / Dialog 任务 8 进展（Gate B 部分完成，未收尾）：Gate A 与控件文档已完成并经用户批准。
> 已交付 `Dialog.SemanticParts.cs` / `MessageBox.SemanticParts.cs`（8 个部件，对齐上游 Modal 语义 DOM）、四个宿主主题的
> 静态 marker、`Dialog` 的 `ISemanticPartCrossRootProvider`、`OverlayDialogPresenter` 的逻辑父与 cross-root 上报、
> 新增 public `Dialog.IsPinnedOpen`（预览钉住），以及 `tests/AtomUI.Desktop.Controls.Tests/Dialog/DialogSemanticPartTests.cs`
>（11 个用例）。Desktop Controls 3732/3732、LLMS verify 79/161 通过，`git diff --check` 干净。
>
> 两处相对 Gate A 文档的实现决策：(1) 全部路由改用 Dialog 自有的 `.semantic-scope-*` 锚点收窄，而非宽泛 `>>`——body 常驻
> `Skeleton`、用户内容常含 `Card`/`Tooltip`/`Spin`，宽泛 descendant 会命中同名部件（已用“每部件恰好命中一个节点”的回归测试
> 锁定）；(2) 新增 `Dialog.IsPinnedOpen` 产品 API：AXAML 无法表达 `BeforeCloseAsync` 否决，原“不新增产品 API”的承诺不成立，
> 已按 Drawer 先例实现并经用户确认。
>
> 2026-09-11 Gallery 与 NativeAOT 补齐：`ModalShowCase` 已从 `GalleryStickyTabsHost` 切到 `GalleryShowCaseHost`，新增语义页签
> （`SemanticPartPreview` + 9 个 `SemanticPartDescription`，顺序对齐上游）与 SemanticStyles 示例（专用生成 Style 类 + 显式
> `x:SetterTargetType`）；本地化补齐 14 个资源键（枚举 + 4 个 xlf 各 80 unit + `CatalogMemberOrder.baseline` +
> `GalleryCatalogCoverageTests` unit 总数 4632→4646），并按仓库既有约束把 `SemanticStylesTitle` 对齐到四语言唯一文案；
> `ModalShowCasePageTests` 扩展为 3 个用例并更新 `ModalShowCaseExamples.snapshot`（count 10）。
>
> 验证：Gallery 619/619、Desktop Controls 3732/3732、Generator 532/532、LLMS 文档测试 23/23、LLMS verify 79/161、
> NativeAOT `osx-arm64` publish 通过（NativeAOT output validation passed，`/tmp/atomui-gallery-aot-dialog`）、
> `git diff --check` 干净。
>
> 2026-09-11 上游严格对齐复核（用户指出预览未严格对齐 antd）：修正三处偏差。(1) `SemanticPartDescription` 顺序改为上游
> `_semantic.tsx` 原序 `root, mask, container, wrapper, header, title, body, footer, close`（原先 wrapper/container 颠倒）；
> (2) 预览从 modeless 浮动浮层改为**舞台内模态**：新增可选公共属性 `Dialog.OverlayScope`（默认 `null` = 既有 TopLevel 宿主，
> 行为与渲染完全不变），舞台内嵌 `ScopeAwareOverlayLayerPanel`，Dialog 同时设置 `OverlayScope` + `PlacementTarget` +
> `IsModal=True` + `StandardButtons="Cancel,Ok"`，使 mask 覆盖舞台、模态居中于舞台、`footer` 有真实按钮呈现；
> (3) `container` 的 boxShadow/padding 归属保持现状并已在文档记录（经用户确认不迁移，避免默认渲染变更）。
>
> `OverlayScope` 实现要点：`DialogOverlayLayer.GetOrCreate(anchor, overlayScope)` 优先解析作用域宿主并在无可用 scope layer 时
> 回退默认宿主；`OverlayDialogPresenter` 仅在**显式** `OverlayScope` 时不解析 owning Window（作用域无 Window frame 契约，
> 不参与 frame 内缩与 drawn chrome 抑制），未指定时保持既有 owner window/base chrome 抑制解析不变。
>
> 新增回归：`OverlayScope_Contains_The_Host_And_Mask_Within_The_Scope`（断言宿主落在舞台内、mask bounds = 舞台尺寸且小于窗口）、
> `Without_OverlayScope_The_Host_Stays_In_The_TopLevel_Overlay_Layer`（默认行为不变）。验证：Desktop Controls 3734/3734、
> Gallery 619/619、Generator 532/532、LLMS 文档测试 23/23、LLMS verify 79/161、NativeAOT `osx-arm64` publish 通过、`git diff --check` 干净。
>
> 2026-09-11 Gallery 交互补齐（用户要求，最终形态）：在「自定义语义结构的样式」之外**新增 MessageBox 语义样式示例**
> （`MessageBoxSemanticStylesTitle`，用生成的 `MessageBox<Part>Style` 类定制，与 Dialog 示例平行），并把 Modal 页面所有示例
> 触发方式从 ToggleSwitch 改为 Button——MessageBox 宿主切换改为 Overlay/Window 两个按钮，两个语义样式示例改为打开按钮。
> 代码后置移除 `StyleCaseHostTypeSwitch` 的 ToggleButton 处理器与全局开关订阅。本地化净增 3 键（4 个 xlf 各 83 unit，
> unit 总数 4646→4649），基线由枚举重建；页面测试改为 `Modal_ShowCase_MessageBox_Semantic_Styling_Uses_Generated_MessageBox_Styles`
> 并把「页面不含 ToggleSwitch」写入布局断言。
>
> 用户复核确认 MessageBox 已具备 Semantic Part 改造（独立 descriptor + `MessageBox<Part>Style`）；本次补齐其缺失的**回归覆盖**
> ——`Generated_Semantic_Styles_Hit_Exactly_One_Part_On_MessageBox` 断言 MessageBox 的 8 个生成 Style 各精确命中一个节点。
> Gallery 620/620、AtomUIGallery 构建通过。
>
> 2026-09-11 语义页签并列 MessageBox 预览（用户要求「在这个下面再新增一个 MessageBox 的 Preview 示例」）：语义页签的
> `DataTemplate` 改为纵向 `StackPanel`，并列两个 `SemanticPartPreview`——`ModalSemanticPreview`（Dialog）与
> `MessageBoxSemanticPreview`（MessageBox，`SemanticOwnerType={x:Type atom:MessageBox}`），两者都使用舞台内模态模式
> （`OverlayScope` + `PlacementTarget` + `IsModal=True` + `IsPinnedOpen=True` + `StandardButtons`）。`GalleryShowCaseHost`
> 本身支持多预览（≥2 时改用内容尺寸布局，不高限钳制）。页面测试改为按预览切片断言，避免两个预览的 Part 路径混淆。
> 验证：Gallery 620/620、Desktop Controls 3735/3735、Generator 532/532、LLMS 23/23、LLMS verify 79/161、
> AtomUIGallery 构建成功、NativeAOT publish 通过、`git diff --check` 干净。
>
> 说明：Desktop 全量首轮出现过一次 `DialogLifecycleTests.Closed_Overlay_Session_Releases_Surface_When_A_Custom_Button_Is_Retained`
> 失败；该用例是既有 `WeakReference` + `GC.Collect` 回收断言、未被本次改动修改，单独连跑 6 次与全量复跑均通过，判定为间歇性
> GC 时序抖动，不是本次改动引入的回归。

> 2026-09-11 范围复核：ImagePreviewer、InfoFlyout、ToolTip、Tour、Drawer、DropdownButton、PopupConfirm、Notification 八个家族的单项任务框已置为已完成（均已按用户授权提交；PopupConfirm 真机视觉验收见 `docs/superpowers/specs/2026-09-11-popupconfirm-semantic-visual-acceptance.md`）。Message、Modal / Dialog 两个家族未开始，本批次仍未收尾。
>
> 2026-09-11 Notification 任务 9：Gate A、文档、Gate B 与全部验证已完成，实现改动经用户授权提交。
>
> 2026-09-11 Notification 视觉对齐补充：修复 `Border#Frame` 的 `BoxShadow` 被中间 `Panel#PART_Layout` 裁剪的问题
> （右/下各只剩 1 逻辑像素，上游为 4），把 `Frame` 提升为 `LayoutAwareMotionActor` 的直接内容、新增 `Border#ContentBox`
> 承担 `Padding`，并让 Gallery 示例的 error 分支补齐上游 `defaultStyles.root` 继承来的 `border: 2px solid` 与
> `borderRadius: 16`。回归测试 `Frame_Sits_Directly_In_The_Motion_Actor_So_BoxShadow_Is_Not_Clipped` 锁定结构约束；
> Desktop Controls 3715/3715、Gallery 615/615 通过。
>
> 2026-09-11 Notification 真机视觉验收：用户按步骤在自己桌面启动本地 Gallery 并回传截图确认，
> `Custom Semantic Part styling` 两张绿色卡片的右侧/底部硬阴影带（`4px 4px 0 #D9F7BE`）与红色 error 卡片的
> 加粗边框（2px `#FFCCC7`）、完整 `#FFCCC7` 硬阴影均已呈现，用户答复「解决了」。视觉验收判定通过，
> 证据为用户回传截图（按约定不入库）。

> 2026-09-12 批次收尾（本批 10 个家族全部完成）：Modal / Dialog 是最后一个家族（`7271b0a95` + `327346ea1`，含
> `ModalSemanticPartHighlightTests` 运行期高亮回归与舞台 owner 铺满修复）；任务 7 Message 补记完成（`be91097d0` +
> `ebe687f89`）。10 个家族的交付提交：ImagePreviewer `f32e2b258`/`08aa3e6ef`、InfoFlyout `e9401ee8b`、ToolTip
> `5fe34ab8a`、Tour `b6ee2e315`、Drawer `e415b81f9`、DropdownButton `6abaf6100`、Message `be91097d0`/`ebe687f89`、
> PopupConfirm `d8857e738`、Notification `f86c5b772`、Modal/Dialog `7271b0a95`/`327346ea1`，均经用户授权提交。
> 收尾验证：Desktop Controls 3736/3736、Generator 532/532、GalleryBase 181/181、Gallery 621/621、Popup/Overlay
> 生命周期筛选（Dialog|MessageBox）通过、LLMS verify 79/161、NativeAOT `osx-arm64` 通过、`git diff --check` 干净。
> 已知非阻塞事项：两条先于本批存在的间歇性测试抖动（`DialogLifecycleTests.Closed_Overlay_Session_Releases_Surface…`
> 与 `DialogPopupControlFamilyTests.PopupConfirm_In_Dialog_Confirms…`），已在基线提交 `f86c5b772` 双跑中复现确认；
> Modal/Dialog 真机悬停高亮未单独截图，由自动化回归覆盖（见
> `docs/superpowers/specs/2026-09-12-dialog-semantic-visual-acceptance.md`）。

- [x] 确认 10 个控件家族分别拥有用户授权的独立提交。
- [x] 运行完整 Desktop Controls、Generator、GalleryBase 和 Gallery 测试，并执行 Popup/Overlay 生命周期筛选。
- [x] 运行 LLMS verify、Gallery NativeAOT publish、适用的平台冒烟检查和 `git diff --check`。
- [x] 更新总计划清单，不创建批次提交。
