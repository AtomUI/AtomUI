# Semantic Part 第三批输入与选择实施计划

> **供智能体执行者使用：** 使用 `superpowers:executing-plans` 在当前会话中执行，不得使用 subagent。即使多个控件共享源码目录或 Gallery 页面，每套正式控件文档仍分别执行独立的 Gate A。每个控件家族在自己的专用 worktree（`feature/semantic-<Control>`）中执行，见[总计划](2026-08-12-semantic-part-control-rollout.md)全局约束。

**目标：** 为 16 个具有 Ant Design 6.6.0 稳定发布源码公开 Semantic DOM 对应 API 的输入与选择控件家族建立 Semantic Part 契约，同时保持 SizeType、原生文本编辑、Popup、候选项容器和可选包行为。（其中 `ComboBox` 为 2026-09-16 用户指令追加，**非 §2.1 Gate 通过**：上游无公开 `ComboBox` owner，准入依据为用户指令 + 其自身即职责完整的 public owner，见任务 16。）

**架构：** 输入控件 owner 只公开自身稳定区域，不侵占嵌套 public 控件的模板职责。Popup Part 继续使用 selector，并受 owner 作用域约束。只有完整记录 owner、每层 wrapper 以及 `ISizeTypeAware` / `ICustomizableSizeTypeAware` 的测量路径后，才能批准可修改布局的 Part。

**技术栈：** .NET 10、Avalonia 12、AtomUI Desktop Controls、AtomUI ColorPicker package、AXAML、xUnit v3、Avalonia Headless、AtomUI Gallery、NativeAOT。

## 全局约束

- 遵循[全量改造总计划](2026-08-12-semantic-part-control-rollout.md)和[全量改造设计](../specs/2026-08-12-semantic-part-control-rollout-design.md)。
- 修改源码前必须通过 Gate A；Gate B 改动保持未提交，直到用户批准。
- 对可修改布局的 Part，必须证明其 Setter 有效值及 Large/Middle/Small/Custom、Min/Max、prefix/suffix、纯图标或 range 变体下的最终 Measure/Arrange 结果。
- Popup 测试覆盖关闭、打开、重新打开、owner 作用域、item 生命周期，以及 close/detach 后的释放。
- 只有所有受影响控件设计都获得批准后，才能修改共享的 Input、Select/TreeView 和 Button 主题。
- ColorPicker 是可选包，必须保持静态注册、trimming 和 NativeAOT 正确性。

---

### 任务 1：AutoComplete

**控件文档：** `docs/controls/desktop/data-entry/auto-complete/overview.md`, `docs/controls/desktop/data-entry/auto-complete/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/AutoComplete/**/*.cs`, `src/AtomUI.Desktop.Controls/AutoComplete/Themes/*Theme.axaml` 以及已批准的共享 Input themes；测试 `tests/AtomUI.Desktop.Controls.Tests/AutoComplete` 和相关 `Input`；Gallery `controlgallery/AtomUIGallery/ShowCases/DataEntry/AutoComplete`.

**风险类型：** ICustomizableSizeTypeAware、派生输入变体、候选项 Popup、异步数据。

- [x] **Gate A 设计审核：** 审计 `AutoComplete`、SearchEdit/TextArea variants、option/candidate popup 与 decorated input owner；确认 input、clear/search controls、popup、option/empty/loading regions，记录 async populate、filter、Popup open-close、容器生命周期 和 SizeType/Custom。
- [x] 更新两份控件文档，写明准确的 Descriptor、节点/owner 映射、布局与 Popup 生命周期、兼容性边界和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/AutoComplete/AutoCompleteSemanticPartTests.cs`，覆盖 line/search/textarea 变体、async option、empty/loading、option 重建、Popup reopen、全部 SizeType 和嵌套 input owner 隔离。
- [x] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 测试、LLMS verify 和 `git diff --check`；对 Popup/可选包/运行时敏感改动执行 NativeAOT 验证。
- [x] **强制停止：** 保持 AutoComplete 的所有实现改动未提交，直到用户验证运行结果并明确授权提交。

### 任务 2：Cascader

**控件文档：** `docs/controls/desktop/data-entry/cascader/overview.md`, `docs/controls/desktop/data-entry/cascader/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Cascader/**/*.cs`, `src/AtomUI.Desktop.Controls/Cascader/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/Cascader`；Gallery `controlgallery/AtomUIGallery/ShowCases/DataEntry/Cascader`.

**风险类型：** Popup、分层运行时容器、异步加载、filter 模式。

- [x] **Gate A 设计审核：** 审计 Cascader input/decorated box、popup CascaderView、level/filter lists 和 items；确认 selector/clear/expand/check/item regions 与 multiple/single、filter/hierarchy variants，记录 async child load、level rebuild、Popup 和容器生命周期。
- [x] 更新两份控件文档，写明准确的 Descriptor、节点/owner 映射、布局与 Popup 生命周期、兼容性边界和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/Cascader/CascaderSemanticPartTests.cs`，覆盖 single/multiple、filter/hierarchy、async loading、level navigation、check/select、Popup reopen、collection reset 和 marker budget。
- [x] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 测试、LLMS verify 和 `git diff --check`；对 Popup/可选包/运行时敏感改动执行 NativeAOT 验证。
- [x] **强制停止：** 保持 Cascader 的所有实现改动未提交，直到用户验证运行结果并明确授权提交。（已按用户授权提交 `fd5cbd97a`；视觉验收已按裁剪范围关闭）

### 任务 3：ColorPicker

**控件文档：** `docs/controls/desktop/data-entry/color-picker/overview.md`, `docs/controls/desktop/data-entry/color-picker/implementation.md`

**证据范围：** 可选包 `src/AtomUI.Desktop.Controls.ColorPicker/**/*.cs`、全部 `Themes/**/*.axaml`；需要创建的测试位于 `tests/AtomUI.Desktop.Controls.Tests/ColorPicker`，现有测试为 `tests/AtomUIGallery.Tests/Controls/ColorPickerCustomSizeTests.cs`；Gallery `controlgallery/AtomUIGallery/ShowCases/DataEntry/ColorPicker`。

**风险类型：** 可选包注册、ICustomizableSizeTypeAware、Popup、多个 public 子控件、rendering 热路径。

- [x] **Gate A 设计审核：** 逐 owner 审计 solid/gradient picker、palette、color view/spectrum、sliders/tracks/thumbs/input/block/collapse；区分 picker root popup regions 与 public child descriptors，记录 gradient stop changes、pointer/render hot paths、SizeType 和 package registration。
- [x] 更新两份控件文档，写明准确的 Descriptor、节点/owner 映射、布局与 Popup 生命周期、兼容性边界和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 创建 `tests/AtomUI.Desktop.Controls.Tests/ColorPicker/ColorPickerSemanticPartTests.cs`，覆盖 solid/gradient、Popup reopen、palette group/item、spectrum/slider/thumb、custom size 和运行时更新；运行可选包注册测试、Gallery 测试和 NativeAOT publish。
- [x] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 测试、LLMS verify 和 `git diff --check`；对 Popup/可选包/运行时敏感改动执行 NativeAOT 验证。
- [x] **强制停止：** 保持 ColorPicker 的所有实现改动未提交，直到用户验证运行结果并明确授权提交。

### 任务 4：DatePicker

**控件文档：** `docs/controls/desktop/data-entry/date-picker/overview.md`, `docs/controls/desktop/data-entry/date-picker/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/DatePicker/**/*.cs`, 全部 `src/AtomUI.Desktop.Controls/DatePicker/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/DatePicker`；Gallery `controlgallery/AtomUIGallery/ShowCases/DataEntry/DatePicker`.

**风险类型：** Popup presenter、range/dual/timed 变体、日历容器、SizeType。

- [x] **Gate A 设计审核：** 审计 DatePicker/RangeDatePicker input regions、public presenters、calendar/day/month/year buttons、dual/timed presenters；确认 prefix/input/clear/suffix/popup/panel/cell/footer职责及 owner，记录 range modes、Popup、calendar rebuild 和 SizeType。
- [x] 更新两份控件文档，写明准确的 Descriptor、节点/owner 映射、布局与 Popup 生命周期、兼容性边界和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/DatePicker/DatePickerSemanticPartTests.cs`，覆盖 date/range/dual/timed、calendar modes、cell rebuild、clear/prefix/suffix、Popup reopen、invalid/disabled 和 所有尺寸。
- [x] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 测试、LLMS verify 和 `git diff --check`；对 Popup/可选包/运行时敏感改动执行 NativeAOT 验证。
- [x] **强制停止：** 保持 DatePicker 的所有实现改动未提交，直到用户验证运行结果并明确授权提交。

### 任务 5：Form

**控件文档：** `docs/controls/desktop/data-entry/form/overview.md`, `docs/controls/desktop/data-entry/form/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Form/*.cs`, `src/AtomUI.Desktop.Controls/Form/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/Form`；Gallery `controlgallery/AtomUIGallery/ShowCases/DataEntry/Form`.

**风险类型：** ICustomizableSizeTypeAware 传递、item 容器、validation feedback、子控件 ownership。

- [x] **Gate A 设计审核：** 审计 `Form`、`FormItem`、decorator、feedback、submit/reset/delete control 的 owner；确认 label/control/help/extra/required/feedback 区域，记录 SizeType 向 `ISizeTypeAware`/`ICustomizableSizeTypeAware` 的传递、validation 更新和 item 生命周期。
- [x] 更新两份控件文档，写明准确的 Descriptor、节点/owner 映射、布局与 Popup 生命周期、兼容性边界和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/Form/FormSemanticPartTests.cs`，覆盖各类 layout、validation 状态、label/help/extra、动态 item、包含 Custom 在内的 SizeType 传递和嵌套 semantic control 隔离。
- [x] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 测试、LLMS verify 和 `git diff --check`；对 Popup/可选包/运行时敏感改动执行 NativeAOT 验证。
- [x] **强制停止：** 保持 Form 的所有实现改动未提交，直到用户验证运行结果并明确授权提交。

### 任务 6：LineEdit

**控件文档：** `docs/controls/desktop/data-entry/line-edit/overview.md`, `docs/controls/desktop/data-entry/line-edit/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Input/TextBox.cs`, `LineEdit.cs`, `TextArea.cs`, helper controls 和 `Themes/{TextBox,LineEdit,TextArea,EmbeddedTextBox,InputClearIconButton,RevealButton,ResizeHandle,TextAreaDecoratedBox}*`；测试 `tests/AtomUI.Desktop.Controls.Tests/Input`；共享 Gallery `controlgallery/AtomUIGallery/ShowCases/DataEntry/LineEdit`.

**风险类型：** ICustomizableSizeTypeAware、原生文本 editor、可选控件、TextArea resize、共享 SearchEdit 主题/页面。

- [x] **Gate A 设计审核：** 审计 TextBox/LineEdit/TextArea owners，确认 text presenter/placeholder/clear/reveal/prefix/suffix/add-on/count/form feedback/resize handle responsibilities；记录 native editor boundary、validation、Custom size natural measurement 和 TextArea auto-size/drag。SearchEdit regions remain separate Gate A。
- [x] 更新两份控件文档，写明准确的 Descriptor、节点/owner 映射、布局与 Popup 生命周期、兼容性边界和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/Input/LineEditSemanticPartTests.cs`，覆盖 TextBox/LineEdit/TextArea variants、clear/reveal/count/add-ons/validation、所有尺寸、Custom padding/height、auto-size/resize 和共享 page lazy Preview。
- [x] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 测试、LLMS verify 和 `git diff --check`；对 Popup/可选包/运行时敏感改动执行 NativeAOT 验证。
- [x] **强制停止：** 保持 LineEdit 的所有实现改动未提交，直到用户验证运行结果并明确授权提交。

### 任务 7：Mentions

**控件文档：** `docs/controls/desktop/data-entry/mentions/overview.md`, `docs/controls/desktop/data-entry/mentions/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Mentions/**/*.cs`, `src/AtomUI.Desktop.Controls/Mentions/Themes/MentionsTheme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/Mentions`；Gallery `controlgallery/AtomUIGallery/ShowCases/DataEntry/Mentions`.

**风险类型：** ICustomizableSizeTypeAware、TextArea composition、候选项 Popup、异步数据。

- [x] **Gate A 设计审核：** 审计 Mentions/MentionTextArea/MentionOption owner，确认 input、clear/count/resize 和 candidate popup/option/empty/loading regions；记录 trigger parsing、async populate、Popup placement/reopen, option lifecycle 和 SizeType。
- [x] 更新两份控件文档，写明准确的 Descriptor、节点/owner 映射、布局与 Popup 生命周期、兼容性边界和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/Mentions/MentionsSemanticPartTests.cs`，覆盖 candidate trigger/populate、option selection、Popup reopen、textarea resize/count、所有尺寸、collection reset 和 nested owner 隔离。
- [x] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 测试、LLMS verify 和 `git diff --check`；对 Popup/可选包/运行时敏感改动执行 NativeAOT 验证。
- [x] **强制停止：** 保持 Mentions 的所有实现改动未提交，直到用户验证运行结果并明确授权提交。

### 任务 8：NumericUpDown

**控件文档：** `docs/controls/desktop/data-entry/numeric-up-down/overview.md`, `docs/controls/desktop/data-entry/numeric-up-down/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/NumericUpDown/*.cs`, `src/AtomUI.Desktop.Controls/NumericUpDown/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/NumericUpDown`；Gallery 目录 `controlgallery/AtomUIGallery/ShowCases/DataEntry/NumberUpDown`.

**风险类型：** ICustomizableSizeTypeAware、嵌入式 editor、spinner button、validation/layout。

- [x] **Gate A 设计审核：** 审计 NumericUpDown、spinner 和 embedded text editor owner，确认 input、increase/decrease handles、prefix/suffix/clear/status regions；记录 parse/format/invalid state、keyboard/wheel、SizeType/Custom 和 repeated spin action。
- [x] 更新两份控件文档，写明准确的 Descriptor、节点/owner 映射、布局与 Popup 生命周期、兼容性边界和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/NumericUpDown/NumericUpDownSemanticPartTests.cs`，覆盖 button/keyboard/wheel、invalid/readonly、prefix/suffix、所有尺寸和 Custom layout Setter；Gallery 测试使用实际的 `NumberUpDown` 目录命名。
- [x] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 测试、LLMS verify 和 `git diff --check`；对 Popup/可选包/运行时敏感改动执行 NativeAOT 验证。
- [x] **强制停止：** 保持 NumericUpDown 的所有实现改动未提交，直到用户验证运行结果并明确授权提交。

### 任务 9：OtpLineEdit

**控件文档：** `docs/controls/desktop/data-entry/otp-line-edit/overview.md`, `docs/controls/desktop/data-entry/otp-line-edit/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/OtpLineEdit/*.cs`、`src/AtomUI.Desktop.Controls/OtpLineEdit/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/OtpLineEdit`；当前没有独立 Gallery 目录，Gate A 必须决定集成到现有 input 页面，或按照 Gallery 组织规范创建标准 ShowCase 路由。

**风险类型：** ICustomizableSizeTypeAware、运行时 cell、隐藏 editor、separator、缺少独立 Gallery。

- [x] **Gate A 设计审核：** 审计 OtpLineEdit、cell、OtpTextBox 和 separator owner，确认 repeated cell/content/mask/separator/focus regions；记录 length changes、paste/focus/completion、runtime cell rebuild 和 SizeType。先按 Gallery organization 明确正式展示 ownership，不创建临时页面。
- [x] 更新两份控件文档，写明准确的 Descriptor、节点/owner 映射、布局与 Popup 生命周期、兼容性边界和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/OtpLineEdit/OtpLineEditSemanticPartTests.cs`，覆盖 length rebuild、text/paste、mask、separator、focus/completion 和所有尺寸；新增经 Gate A 批准的 Gallery route/page 测试并验证延迟创建。
- [x] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 测试、LLMS verify 和 `git diff --check`；对 Popup/可选包/运行时敏感改动执行 NativeAOT 验证。
- [x] **强制停止：** 保持 OtpLineEdit 的所有实现改动未提交，直到用户验证运行结果并明确授权提交。

### 任务 10：SearchEdit

**控件文档：** `docs/controls/desktop/data-entry/search-edit/overview.md`, `docs/controls/desktop/data-entry/search-edit/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Input/SearchEdit*.cs`, `src/AtomUI.Desktop.Controls/Input/Themes/{SearchEdit,SearchEditDecoratedBox,SearchButton}*` 以及已批准的共享 LineEdit themes；测试 `tests/AtomUI.Desktop.Controls.Tests/Input/SearchEdit*`；共享 Gallery `controlgallery/AtomUIGallery/ShowCases/DataEntry/LineEdit`.

**风险类型：** 派生 LineEdit 契约、ICustomizableSizeTypeAware、public Button typed theme、共享源码/页面。

- [x] **Gate A 设计审核：** 审计 SearchEdit 自有搜索按钮/decorated box 与继承 LineEdit regions，确认 `SearchButtonTheme` 是否对应真实 public Button 的 `SelectorAndTheme`，避免重复声明继承 Part 或穿透 Button template；记录 operating/loading、Enter/button 和 Custom size。
- [x] 更新两份控件文档，写明准确的 Descriptor、节点/owner 映射、布局与 Popup 生命周期、兼容性边界和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/Input/SearchEditSemanticPartTests.cs`，覆盖 inherited 与 owned Descriptor、button/Enter、operating/loading、text/icon search button、所有尺寸、typed theme 替换和 owner selector 隔离。
- [x] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 测试、LLMS verify 和 `git diff --check`；对 Popup/可选包/运行时敏感改动执行 NativeAOT 验证。
- [x] **强制停止：** 保持 SearchEdit 的所有实现改动未提交，直到用户验证运行结果并明确授权提交。

### 任务 11：Select

**控件文档：** `docs/controls/desktop/data-entry/select/overview.md`, `docs/controls/desktop/data-entry/select/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Select/**/*.cs`, `src/AtomUI.Desktop.Controls/Select/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/Select`；Gallery `controlgallery/AtomUIGallery/ShowCases/DataEntry/Select`.

**风险类型：** ICustomizableSizeTypeAware、Popup、候选项容器、tag、异步数据、虚拟化。

- [x] **Gate A 设计审核：** 审计 Select root input/decorated box、handle/clear/filter/result tags/max indicator、candidate popup/list/item；明确 single/multiple/tags, selected item vs candidate item owner, async loading, virtualization, Popup 和 SizeType。
- [x] 更新两份控件文档，写明准确的 Descriptor、节点/owner 映射、布局与 Popup 生命周期、兼容性边界和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/Select/SelectSemanticPartTests.cs`，覆盖 single/multiple/tags、filter/clear、max tag/count、async/empty/loading、candidate recycle、Popup reopen 和所有尺寸；记录可见 item 的 marker 数量，并证明不会保留旧 Popup/container。
- [x] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 测试、LLMS verify 和 `git diff --check`；对 Popup/可选包/运行时敏感改动执行 NativeAOT 验证。
- [x] **强制停止：** 保持 Select 的所有实现改动未提交，直到用户验证运行结果并明确授权提交。

### 任务 12：TimePicker

**控件文档：** `docs/controls/desktop/data-entry/time-picker/overview.md`, `docs/controls/desktop/data-entry/time-picker/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/TimePicker/**/*.cs`, `src/AtomUI.Desktop.Controls/TimePicker/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/TimePicker`；Gallery `controlgallery/AtomUIGallery/ShowCases/DataEntry/TimePicker`.

**风险类型：** Popup presenter、range 变体、time cell 容器、SizeType。

- [x] **Gate A 设计审核：** 审计 TimePicker/RangeTimePicker input regions、presenter、TimeView/panels/cells 和 footer actions；确认 input/clear/suffix/popup/column/cell/now/confirm responsibilities, range ownership, Popup 和 SizeType。
- [x] 更新两份控件文档，写明准确的 Descriptor、节点/owner 映射、布局与 Popup 生命周期、兼容性边界和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/TimePicker/TimePickerSemanticPartTests.cs`，覆盖 single/range、12/24h、cell lists、scroll/selection、clear/footer、Popup reopen、disabled 和 sizes。
- [x] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 测试、LLMS verify 和 `git diff --check`；对 Popup/可选包/运行时敏感改动执行 NativeAOT 验证。
- [x] **强制停止：** 保持 TimePicker 的所有实现改动未提交，直到用户验证运行结果并明确授权提交。

### 任务 13：Transfer

**控件文档：** `docs/controls/desktop/data-entry/transfer/overview.md`, `docs/controls/desktop/data-entry/transfer/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Transfer/**/*.cs`, `src/AtomUI.Desktop.Controls/Transfer/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/Transfer`；Gallery `controlgallery/AtomUIGallery/ShowCases/DataEntry/Transfer`.

**风险类型：** ICustomizableSizeTypeAware、多个 view、item 容器、嵌套 Select/Tree/List owner。

- [x] **Gate A 设计审核：** 审计 ListTransfer/TreeTransfer、source/target view、item decorator、action button/dropdown 和嵌套 list/tree owner；确认 header/body/search/list/item/footer/action 职责，不得侵占嵌套控件的 internal Part；记录 selection/move/filter、容器生命周期和 SizeType。
- [x] 更新两份控件文档，写明准确的 Descriptor、节点/owner 映射、布局与 Popup 生命周期、兼容性边界和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/Transfer/TransferSemanticPartTests.cs`，覆盖 list/tree, source-target move, select all/filter, item reset, action dropdown, 所有尺寸 和 nested semantic-owner 隔离。
- [x] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 测试、LLMS verify 和 `git diff --check`；对 Popup/可选包/运行时敏感改动执行 NativeAOT 验证。
- [x] **强制停止：** 保持 Transfer 的所有实现改动未提交，直到用户验证运行结果并明确授权提交。

### 任务 14：TreeSelect

**控件文档：** `docs/controls/desktop/data-entry/tree-select/overview.md`, `docs/controls/desktop/data-entry/tree-select/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/TreeSelect/**/*.cs`, `src/AtomUI.Desktop.Controls/TreeSelect/Themes/*Theme.axaml` 以及已批准的共享 Select/TreeView assets；测试 `tests/AtomUI.Desktop.Controls.Tests/TreeSelect`；Gallery `controlgallery/AtomUIGallery/ShowCases/DataEntry/TreeSelect`.

**风险类型：** 派生自 Select 的 input、TreeView Popup、分层容器、共享主题。

- [x] **Gate A 设计审核：** 审计 TreeSelect 自有 input/decorated box、popup tree 和 tree item overrides；明确继承 Select Part、TreeSelect 自有 regions 与 nested TreeView owner，记录 checked strategies、async hierarchy、Popup 和 SizeType。共享 Select/TreeView 文件须相关 Gate A 批准。
- [x] 更新两份控件文档，写明准确的 Descriptor、节点/owner 映射、布局与 Popup 生命周期、兼容性边界和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/TreeSelect/TreeSelectSemanticPartTests.cs`，覆盖 single/multiple/check strategies、filter、async tree、Popup reopen、container recycle、所有尺寸 和 descriptor inheritance/owner 隔离。
- [x] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 测试、LLMS verify 和 `git diff --check`；对 Popup/可选包/运行时敏感改动执行 NativeAOT 验证。
- [x] **强制停止：** 保持 TreeSelect 的所有实现改动未提交，直到用户验证运行结果并明确授权提交。

### 任务 15：Upload

**控件文档：** `docs/controls/desktop/data-entry/upload/overview.md`, `docs/controls/desktop/data-entry/upload/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/Upload/**/*.cs`, `src/AtomUI.Desktop.Controls/Upload/Themes/*Theme.axaml`；测试 `tests/AtomUI.Desktop.Controls.Tests/Upload`；Gallery `controlgallery/AtomUIGallery/ShowCases/DataEntry/Upload`.

**风险类型：** 运行时 task/item Visual、多个 list 变体、异步生命周期、preview overlay。

- [x] **Gate A 设计审核：** 审计 Upload/trigger/drop zone/default drop area/list/text/picture/picture-card items 和 state contents；确认 trigger/drop/list/item/status/progress/action/preview regions与 public child owners，记录 queue/state transitions、auto remove、item 重建、image preview 和 detach。
- [x] 更新两份控件文档，写明准确的 Descriptor、节点/owner 映射、布局与 Popup 生命周期、兼容性边界和验证矩阵；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。
- [x] **Gate B 实现与验证：** 新增 `tests/AtomUI.Desktop.Controls.Tests/Upload/UploadSemanticPartTests.cs`，覆盖 text/picture/picture-card、pending/uploading/success/error、add/remove/reset/auto-remove、preview open-close 和运行时 marker 稳定性；运行生命周期检查和 NativeAOT 验证。
- [x] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 测试、LLMS verify 和 `git diff --check`；对 Popup/可选包/运行时敏感改动执行 NativeAOT 验证。
- [x] **强制停止：** 保持 Upload 的所有实现改动未提交，直到用户验证运行结果并明确授权提交。

### 任务 16：ComboBox（2026-09-16 追加，原排除判定彻底撤销）

**控件文档：** `docs/controls/desktop/navigation/combo-box/overview.md`, `docs/controls/desktop/navigation/combo-box/implementation.md`

**证据范围：** `src/AtomUI.Desktop.Controls/ComboBox/**/*.cs`、`src/AtomUI.Desktop.Controls/ComboBox/Themes/*Theme.axaml`，以及已批准的共享 `Primitives/AddOnDecoratedBox` 主题；测试 `tests/AtomUI.Desktop.Controls.Tests/ComboBox`；Gallery `controlgallery/AtomUIGallery/ShowCases/Navigation/ComboBox`。

**风险类型：** 直接派生 Avalonia `ComboBox`（无 AtomUI 基类）、ICustomizableSizeTypeAware、模板内 Popup、运行时容器 marker、嵌套 `ComboBoxHandle` owner、非编辑态选中内容与编辑态输入框互斥。

**准入说明（必须先读）：** 本项**不是 §2.1 Gate 通过**。上游 6.6.0 没有公开 `ComboBox` owner，第 1 条不成立；原排除判定中「`Select` 的 internal combobox mode 不能作为公开 owner」继续有效。准入依据是用户指令加上 `ComboBox` 自身就是职责完整、可独立定制的 public owner（直接派生 Avalonia `ComboBox`，自有输入框、下拉 handle、模板内 Popup 与候选容器创建路径）。命名与区域分组参考上游 `Select` 已公开的语义分组，**不得发明 `ComboBox` 实际不存在的键**。

- [x] **Gate A 设计审核：**（2026-09-16：已完成并经用户批准。） 审计 `ComboBox` 自身作为唯一 semantic owner，以及 `ComboBoxHandle`（internal，独立模板）、`ComboBoxItem`（public item container）、`ComboBoxTextBox`（internal）；确认前缀、内容区、占位符、编辑输入框、后缀区、下拉指示器、弹层框体、候选列表区、候选项、空态的职责与存在条件。明确排除 `Pagination` / `DataGrid` 等宿主借组合关系声明 ComboBox 区域，也排除 ComboBox 反向声明嵌套 `AddOnDecoratedBox` 的区域。记录 `SelectedContentPresenter` 与 `PART_EditableTextBox` 的互斥（`IsEditable`）、`IsAllowClear` 为未实现 API 故不发布清除部件、非编辑态 `OverflowTip` 交互、SizeType/Custom、弹层钉住打开与容器生命周期。逐项给出排除理由，避免把每个命名节点都转成 Part。
- [x] 更新两份控件文档以及 `docs/controls/desktop/navigation/combo-box/semantic-part.md`，写明准确的 Descriptor、真实模板/运行时节点、marker 放置位置、owner 边界、排除的 internal wrapper、布局与 Popup 生命周期、兼容性边界和验证矩阵；**必须把 `overview.md` 中现有的 LLMS 占位语义表（`root` / `trigger` / `item` / `popup` / `motion`）替换为与真实 descriptor 逐条一致的契约**，该占位表是生成器 fallback 产物，与 `ComboBox` 实际结构不符；运行 LLMS verify 和 `git diff --check`；随后停止并等待用户批准。（2026-09-16 已完成：`overview.md`、`implementation.md`、新增 `semantic-part.md`、范围文档同步；LLMS verify 通过、`git diff --check` 干净。）
- [x] **Gate B 实现与验证：** 新增 `src/AtomUI.Desktop.Controls/ComboBox/ComboBox.SemanticParts.cs` 并让 `ComboBox` 成为 `partial`；新增 `tests/AtomUI.Desktop.Controls.Tests/ComboBox/ComboBoxSemanticPartTests.cs`，覆盖 Descriptor 全字段与顺序、静态 marker 集合、生成专用 Style 恰好命中一个节点、"非 root 部件不生成 Style" 与 "无 code-behind 回退"、弹层打开/关闭/重新打开的弹层部件命中、候选项容器 prepare/clear/recycle 的 marker 稳定性、`IsEditable` 切换下输入框与选中内容节点的命中、所有 SizeType，以及嵌套 owner 隔离。可用 `Popup` 钉住（`IsPopupPinnedOpen`）取证弹层部件。（2026-09-16 已完成：13 个用例全绿。实现中修正了一处文档缺陷——`popup.empty` 实际只在生效过滤模式下可见，未开启过滤时 `IsEffectiveEmptyVisible` 被强制为 false。**范围追加：** 用户在真机走查中提出「输入框边框需要能做定制颜色 demo」，据此新增第 12 个部件 `frame`（`ComboBoxFrameStyle`，`ContractType=PixelAlignedBorder`，`CrossNestedOwners=true`）——其 marker 落在既有 `PART_ContentFrame` 节点上，**未新增或改造共享主题节点结构**；生成 Selector 以 `Nesting()` 起手，因此 owner 作用域隔离，`Select` / `LineEdit` 复用同一主题不受串味影响（先例：`ToolTip` 的 `container` / `arrow` 由共享 `ArrowDecoratedBoxTheme` 承载）。该部件是本次最常用的定制点：应用可直接覆盖输入框边框颜色、宽度、圆角与背景。契约、overview、implementation、changelog 已同步为 12 部件。**弹层钉住：** 用户指出 ComboBox 下拉框未按 AtomUI 标准方式钉住；核实后确认 `IsPopupPinnedOpen` 原为 internal，而同族 11 个控件（`AbstractSelect`、`AbstractAutoComplete`、`InfoPickerInput`、`Mentions`、`AbstractColorPicker`、`Menu`、`NavMenu`、`DropdownButton`、`SplitButton`、`Tour`、`FlyoutHost`）均为 public，故本次提升为 public 并逐字对齐标准 XML 注释，Gallery 预览改为标准钉住方式。附带发现：`PopupPinnedOpenContractTests` 中 `Direct_Popup_Pinned_Open_Contracts_Are_Internal` 与 `Leaf_Controls_Inherit_The_Contract_From_Their_Semantic_Owner` 两个 Theory 因 `MemberData` 不产出数据而**实际执行 0 个用例**，该 internal 断言从未生效；同一列表中多个类型早已是 public，与断言直接矛盾。本次未改测试，已记录于 changelog 供维护者处理。**遮罩遮挡整页（2026-09-16 已修复）：** 真机走查发现钉住时整页除输入框外不可交互。根因是 Avalonia 只在弹层打开瞬间读取 `IsLightDismissEnabled` 创建 light-dismiss 遮罩且该属性无变更回调，而 ComboBox 模板的 `IsOpen="{TemplateBinding IsDropDownOpen}"` 会在模板充气阶段（早于控件抑制遮罩）打开弹层，留下无法消除的遮罩层。修法采用共享 `AbstractSelect` 的既有标准：模板去掉 `IsOpen` 绑定，开合改由控件代码接管（`OnApplyTemplate` 先抑制再补开、`IsDropDownOpen` 变更经 `OpenPopup`/`ClosePopup`），原由 TwoWay 绑定承担的关闭回写改由 `Popup.Closed` 显式补齐。新增 `ComboBoxPinnedPopupOverlayTests`（6 例）断言遮罩层不存在并覆盖 `InputHitTest` 行为断言与关闭回写；预览高亮同时恢复为真实卡片悬停断言（原先因遮罩只能退化为按 marker 在场性判定）。详见 `docs/engineering/case-studies/semantic-part-popup-first-open-lifecycle-case-study.md` 的补记。）
- [x] 运行 Generator Semantic 测试、目标 Desktop 测试、GalleryBase 测试、目标 Gallery 测试、LLMS verify 和 `git diff --check`；本项含 Popup 与运行时 marker 注入，须执行 NativeAOT 验证（若沙箱阻止 AvaloniaUI BuildServices 写入导致无法取证，如实记录为未取证，不得宣称通过）。（2026-09-16：Generator 528/528、Desktop 4043/4043、GalleryBase 185/185、Gallery 646/646、Docs LLMsGenerator 23/23、LLMS verify 79/161、`git diff --check` 干净；**NativeAOT publish 未执行**——本沙箱此前已确认会阻止 AvaloniaUI BuildServices 写入，故如实标记未取证。）
- [x] 添加延迟创建的 Gallery Semantic Parts 页签与「Semantic Part Style」示例（4 个语言 xlf 同步），首次选择 Semantic Tab 前不得实例化 Semantic 内容；同步（2026-09-16 已完成：ShowCase 迁移到 `GalleryShowCaseHost`，新增 Semantic Parts 预览与「Custom Semantic Part styling」示例，ShowCase 条目 10 → 11，4 语言新增 14 个 key（每语言 xlf 39 → 53 unit），快照、`CatalogMemberOrder.baseline` 与 `GalleryCatalogCoverageTests` 总 unit 数同步。**弹层钉住已按标准方式落地**：`IsPopupPinnedOpen` 提升为 public 后，Gallery 预览直接用它在预览态钉住弹层并逐卡取证 12 个 marker；此前"internal 导致无法钉住、弹层部件只能由控件级测试覆盖"的记录已作废。**Gallery 预览限制已记录**：钉住时遮罩层仅放行输入框区域的指针输入（`OverlayInputPassThroughElement`，与 `Select` / `AutoComplete` 同机制），因此预览断言的判定依据是 marker 在场性而不是卡片悬停命中。**rebase 计数说明（2026-09-16）**：本项最初基于旧 `feature/semantic` 基线（当时总 unit 数 4732），rebase 到 `ea40d5481` 后基线已增至 4749，故新增 14 个 key 后的最终断言值为 4763；该值由测试实际统计 81 个源文件得出，非手工推算。） `tests/AtomUIGallery.Tests` 的 ShowCase 计数与 `Localization/CatalogMemberOrder.baseline` 基线。
- [ ] **强制停止：** 保持 ComboBox 的所有实现改动未提交，直到用户验证运行结果并明确授权提交。真机视觉验收由用户回传截图，未回传前只能标注"待视觉验收"。

## 批次收尾

- [x] 确认 15 个控件家族分别拥有用户授权的独立提交。（2026-09-16：`ComboBox` 为当日追加的第 16 个家族，尚未提交，故本条完成状态仅覆盖原 15 个家族。）
- [ ] 运行完整 Desktop Controls、Generator、GalleryBase 和 Gallery 测试，并执行输入、选择和本地化筛选。
- [ ] 运行 LLMS verify、Gallery NativeAOT publish 和 `git diff --check`。
- [ ] 更新总计划清单，不额外创建批次提交。

> 2026-09-10 复核：15 个家族已全部提交，故单项任务框与第一条收尾项置为已完成。**批次级测试扫描尚未执行**，因此后两条收尾项保持未勾选；每个家族 Gate B 的目标测试、NativeAOT 与 LLMS 证据记录在各家族提交中，不等价于批次收尾的全量重跑结论。
