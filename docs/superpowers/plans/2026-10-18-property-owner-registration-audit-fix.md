# Property and Routed Event Owner Registration Fixes Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 修复全局扫描发现的 45 个 Avalonia property owner 注册错误和 6 个 routed event owner 注册错误，恢复每个属性/事件与其声明控件的注册边界。

**Architecture:** 以声明字段所在的实际控件类型作为 owner。普通 `Register`/`RegisterDirect` 使用当前声明类型；共享属性先在真实基础控件或共享宿主上注册，再通过 `AddOwner<具体控件>()` 扩展；routed event 使用实际声明并发出事件的控件作为 owner。共享属性 helper 注册到 `StyledElement` 的既有设计不改。

**Tech Stack:** C#、Avalonia 12.1.4、xUnit v3、AvaloniaPropertyRegistry、AtomUI Desktop Controls tests。

**Spec:** 用户请求“全局扫描发现的 owner 类 bug 全部修复”；扫描结果记录在本次会话，具体文件和字段见任务清单。

## Global Constraints

- 保留当前 Avalonia 12.1.4 升级改动、`release/6.0` 上已有提交及其未提交修改，不回退、不重排、不提交新的无关改动。
- 共享 property helper（`InputControlStatusProperty`、`SizeTypeControlProperty`、`ScrollAwareControlProperty`、`MotionAwareControlProperty`、`CompactSpaceAwareControlProperty` 等）继续注册到 `StyledElement`，再由具体控件 `AddOwner`。
- 只修复 owner 注册边界；不改变默认值、绑定模式、属性名称、事件名称、模板结构或业务行为。
- 新增审计测试属于临时验证，验证完成后删除，不进入最终测试集合。
- 遵守 C# 控制流必须使用 `{}`、AOT 约束和 `git diff --check` 要求。
- 未经用户再次明确要求不创建 git commit；本次最终只保留工作区修改。

## Review Focus

- 直接注册到无关类型的 styled/direct property 必须在声明控件上可精确查到；由任务 1 的 owner registry audit 覆盖。
- 派生控件使用的属性不能只注册在兄弟类型或错误的祖先类型；由 ColorPicker、DataGrid、Select、Pagination、Statistic 组的审计行覆盖。
- 嵌套控件字段必须注册在实际使用该字段的嵌套控件上；由 `DragPreview.BackgroundProperty` 覆盖。
- `TreeView` 和 `TreeViewItem` 的同名过滤状态必须保持为两个独立 owner；由 TreeView filter 审计行覆盖。
- routed event 的 owner 必须是实际声明并 `RaiseEvent` 的控件；由 6 个事件 owner 审计行覆盖。

### Task 1: 创建临时 owner 契约测试并确认 RED

**Files:**
- Create: `tests/AtomUI.Desktop.Controls.Tests/Temporary/OwnerRegistrationAudit/OwnerRegistrationContractTests.cs`

**Interfaces:**
- Consumes: 各相关程序集中的 property/event static fields 与 `AvaloniaPropertyRegistry`。
- Produces: 可一次性检查 45 个 property owner 注册和 6 个 routed event owner 的测试。

**Steps:**

- [x] 1. 在测试项目临时目录写入 owner 清单，记录声明类型、字段名、注册方式和期望 owner。
- [x] 2. 使用 `AvaloniaPropertyRegistry` 的精确 `_registered` 字典反射检查 `AddOwner` 是否真正注册到了声明类型；对 `Register`/`RegisterDirect` 检查 `OwnerType`；对 routed event 检查 `OwnerType`。
- [x] 3. 运行 `dotnet test ... --filter FullyQualifiedName~OwnerRegistrationContractTests`，确认当前代码按预期失败，并记录失败数量。

### Task 2: 修复 31 个直接未注册到声明类型的 property

**Files:**
- Modify: `src/AtomUI.Controls/Rate/RateCharacter.cs:15`
- Modify: `src/AtomUI.Controls/RadioButton/RadioIndicator.cs:19`
- Modify: `src/AtomUI.Controls/Primitives/ArrowDecoratedBox/ArrowIndicator.cs:15`
- Modify: `src/AtomUI.Desktop.Controls.ColorPicker/ColorPickerPaletteGroup.cs:12`
- Modify: `src/AtomUI.Desktop.Controls.ColorPicker/ColorSlider/AbstractColorSlider.cs:48`
- Modify: `src/AtomUI.Desktop.Controls.DataGrid/Column/DataGridAbstractTextColumn.cs:18-48`
- Modify: `src/AtomUI.Desktop.Controls/TreeSelect/TreeSelect.cs:67`
- Modify: `src/AtomUI.Desktop.Controls/FloatButton/FloatButtonItemsControl.cs:31`
- Modify: `src/AtomUI.Desktop.Controls/TreeView/DragPreviewAdorner.cs:52`
- Modify: `src/AtomUI.Desktop.Controls/TreeView/TreeView.Filter.cs:10`
- Modify: `src/AtomUI.Desktop.Controls/Transfer/TransferItemDecorator.cs:66-69`
- Modify: `src/AtomUI.Desktop.Controls/Primitives/InfoPickerInput/RangeInfoPickerInput.cs:49`
- Modify: `src/AtomUI.Desktop.Controls/Dialog/ButtonBox/DialogCaptionButton.cs:18-30`
- Modify: `src/AtomUI.Desktop.Controls/Tour/TourStepsView.cs:60`
- Modify: `src/AtomUI.Desktop.Controls/TabControl/BaseTabControl.cs:90-102`
- Modify: `src/AtomUI.Desktop.Controls/Select/SelectTagAwareTextBox.cs:43`
- Modify: `src/AtomUI.Desktop.Controls/TextBlock/HyperLinkTextBlock.cs:52-58`

**Steps:**

- [x] 1. 将每个 `Register`/`RegisterDirect` 的 owner 改为声明控件；将每个错误 `AddOwner<T>()` 改为声明控件。
- [x] 2. 对 `ColorPickerPaletteGroup.PaletteGroupProperty` 保留 `AbstractColorPickerView` 上已有的 `AddOwner` 关系，只把原始注册 owner 改回 `ColorPickerPaletteGroup`。
- [x] 3. 对 `TreeView.Filter.cs` 将 `DirectProperty<TreeViewItem, bool>` 和 getter/setter 参数改为 `DirectProperty<TreeView, bool>`，保持 `TreeViewItem` 自己的同名属性不变。
- [x] 4. 对 `DragPreviewAdorner.cs` 将属性字段及包装器所属 owner 统一为 `DragPreview`，不改变绑定 owner 的生命周期逻辑。
- [x] 5. 运行 owner 临时测试，确认这 31 个条目不再失败。

### Task 3: 修复 14 个祖先/公共类型污染的 property

**Files:**
- Modify: `src/AtomUI.Desktop.Controls.ColorPicker/ColorView/ColorPickerView.cs:12`
- Modify: `src/AtomUI.Desktop.Controls/Pagination/SimplePagination.cs:15`
- Modify: `src/AtomUI.Desktop.Controls/Buttons/Button.cs:234`
- Modify: `src/AtomUI.Desktop.Controls/AutoComplete/CompactSpaceAwareAutoComplete.cs:11-17`
- Modify: `src/AtomUI.Desktop.Controls/Primitives/InfoPickerInput/RangeInfoPickerInput.cs:17-34`
- Modify: `src/AtomUI.Desktop.Controls/Select/Select.cs:96-99`
- Modify: `src/AtomUI.Desktop.Controls/Statistic/TimerStatistic.cs:13-19`
- Modify: `src/AtomUI.Desktop.Controls/Statistic/Statistic.cs:10`

**Steps:**

- [x] 1. 将 `ColorPickerView.ValueProperty` 注册到 `ColorPickerView`，避免与 `GradientColorPickerView.ValueProperty` 共享同名 base registration。
- [x] 2. 将 `SimplePagination.IsReadOnlyProperty`、Button 的 `RightExtraContentTemplateProperty` 注册到自身。
- [x] 3. 将 `CompactSpaceAwareAutoComplete` 的三个 `AddOwner` 改为 `CompactSpaceAwareAutoComplete`，让 AutoComplete 派生类继续通过继承使用这些属性。
- [x] 4. 将 RangeInfoPickerInput 的两个属性注册到 `RangeInfoPickerInput`，并保持 `SecondaryTextProperty` 同样使用自身 owner。
- [x] 5. 将 Select 的 Filter 两个属性、TimerStatistic 的三个属性、Statistic 的 Value 属性注册到各自声明类型。
- [x] 6. 运行 owner 临时测试，确认 45 个 property 条目全部通过，并运行受影响控件现有测试。

### Task 4: 修复 6 个 routed event owner

**Files:**
- Modify: `src/AtomUI.Desktop.Controls.ColorPicker/ColorBlock.cs:52`
- Modify: `src/AtomUI.Desktop.Controls.ColorPicker/ColorSlider/GradientColorSlider.cs:63`
- Modify: `src/AtomUI.Desktop.Controls/Form/FormItem.cs:26`
- Modify: `src/AtomUI.Core/MotionScene/BaseMotionActor.cs:45-48`
- Modify: `src/AtomUI.Desktop.Controls/Select/SelectHandle.cs:164`

**Steps:**

- [x] 1. 将每个 `RoutedEvent.Register<...>` owner 改为实际声明并发出事件的控件类型。
- [x] 2. 保留事件名、路由策略、事件参数和现有 class handler 不变。
- [x] 3. 运行 owner 临时测试，确认 6 个 routed event 全部通过。

### Task 5: 回归验证、结构检查和清理

**Files:**
- Delete: `tests/AtomUI.Desktop.Controls.Tests/Temporary/OwnerRegistrationAudit/OwnerRegistrationContractTests.cs`
- Delete: `tests/AtomUI.Desktop.Controls.Tests/Temporary/OwnerRegistrationAudit/`（若为空）

**Steps:**

- [x] 1. 删除临时审计测试及其专用目录。
- [x] 2. 运行受影响测试项目的精确筛选：Core MotionScene、Controls Rate/Radio/Arrow、Desktop ColorPicker、DataGrid、TreeView/TreeSelect、FloatButton、Pagination、Button、AutoComplete、Transfer、InfoPickerInput、Dialog、Tour、TabControl、Select、Statistic、HyperLinkTextBlock。
- [x] 3. 运行 `python3 .agents/skills/atomui-control-optimization/scripts/check-member-layout.py` 覆盖触及的控件文件。
- [x] 4. 运行 `git diff --check`，检查最终 diff 只包含 owner 修复和临时测试清理。
- [x] 5. 核对当前 Avalonia 升级未提交 diff 的 SHA256 与修复前一致，确认没有污染用户已有工作。

## Verification Commands

```bash
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj \
  --framework net10.0 --no-restore \
  --filter FullyQualifiedName~OwnerRegistrationContractTests \
  --nologo /p:BuildInParallel=false /p:UseSharedCompilation=false

python3 .agents/skills/atomui-control-optimization/scripts/check-member-layout.py \
  --path src/AtomUI.Controls/Rate/RateCharacter.cs \
  --path src/AtomUI.Controls/RadioButton/RadioIndicator.cs \
  --path src/AtomUI.Controls/Primitives/ArrowDecoratedBox/ArrowIndicator.cs \
  --path src/AtomUI.Desktop.Controls.ColorPicker/ColorPickerPaletteGroup.cs \
  --path src/AtomUI.Desktop.Controls.ColorPicker/ColorView/ColorPickerView.cs \
  --path src/AtomUI.Desktop.Controls.ColorPicker/ColorSlider/AbstractColorSlider.cs \
  --path src/AtomUI.Desktop.Controls/TreeView/TreeView.Filter.cs \
  --path src/AtomUI.Desktop.Controls/TreeView/DragPreviewAdorner.cs

git diff --check
```

不运行全量回归，不创建提交，不修改文档或无关行为。最终报告会列出每组修复、RED/GREEN 证据、定向测试数量、临时测试清理结果和残余风险。

## 执行记录

- 原始 HEAD：`5fe77abc0760fe375580cef00c60f3a3b3a00a45`，`release/6.0`，现有未提交 Avalonia 升级文件共 9 个。逐文件 SHA256 保存在系统临时目录，用于最后核对；不修改这些文件。
- RED：51 个逐字段契约测试全部因预期 owner/注册表不匹配失败（45 property、6 event；395 ms）。
- Task 2：31 个 property 修复后为 31 passed / 20 failed，未修复的 14 property 与 6 event 继续失败（485 ms）。
- Task 3：45 个 property 全部通过，仅剩 6 event 失败（340 ms）。
- Task 4：6 event 修复后，51 个契约测试与 9 个真实行为检查合计 60/60 通过（447 ms）。行为检查覆盖 untyped DirectProperty 调用、独立的 TreeView/TreeViewItem 状态、Statistic/TimerStatistic 同名属性、ColorPicker 双视图 Value 隔离、palette 的共享绑定、数值/文字列继承、CaptionButton 同名属性、旧 owner 泄漏、实例 handler 和事件冒泡。
- 为排除“仅检查类型名”的弱验证，将同一份行为检查对照修复前生产源码执行，执行后在 finally 中逐字节恢复修复内容；结果另行记录。
- 成员布局检查 baseline：29 文件/29 控件，共 195 条存量声明结构提示。它们不是 owner 错误；此次不进行无关重排，以保持最小修复与静态初始化顺序。最终与 HEAD 的逐项基线比较，要求零新增；不将现有结构检查描述为无警告通过。
- Ruling：保留 195 条既有成员布局提示，仅验证本次零新增，避免将 BaseTabControl/FormItem 等大文件的成员排序重构混入 owner 修复；若需治理，另行审查初始化顺序并安排独立任务。
- 测试映射复核：Controls 测试项目没有 Rate/Radio/Arrow 的专属既有类，相关行为由实际引用其生产程序集的 Desktop Rate/Radio/Tooltip/PopupConfirm 测试验证，不用无关 Controls 测试冒充覆盖。
- 增加精确 Gallery Form 页面消费验证和 Desktop `RecurringVisualWorkTests` 的 TimerStatistic 验证；这两项属于已修改 HyperLinkTextBlock/TimerStatistic 的直接消费者检查。

- 真实行为 A/B：原始生产源码上 8/9 失败、1/9 通过（245 ms），包括 TreeView untyped `SetValue` 的“不在 TreeView 上注册”异常；恢复修复内容后同一测试集合为 60/60 通过（466 ms）。
- 临时契约/行为测试两个文件及其专用目录已删除，永久测试净新增 0。后续最终模块验证会重新编译，避免使用含临时测试的 DLL。
- 完整成员布局比较：29 文件/29 控件，195 条存量提示与 HEAD 完全一致，零新增；仅比较 rule/member/message，未掩盖现有提示。

## 最终验证与交付

清理后的生产工作树通过以下验证（全部重新构建，未使用 `--no-build`）：

| 验证范围 | 结果 | 测试运行耗时 |
| --- | --- | --- |
| Desktop 实际影响控件与直接消费者 | 1200/1200 通过 | 37 s |
| Core MotionScene | 1/1 通过 | 403 ms |
| DataGrid 列、数据 accessor、列头与语义主题 | 27/27 通过 | 1 s |
| Gallery Form Showcase，包含 HyperLinkTextBlock 消费 | 6/6 通过 | 3 s |
| Gallery Desktop Debug/net10.0 构建 | 0 warning / 0 error | 11.03 s |

既有测试合计 1234/1234；临时 60 个检查不计入永久测试数量。

实际命令：

```bash
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~.Rate.|FullyQualifiedName~.RadioButton.|FullyQualifiedName~.ColorPicker.|FullyQualifiedName~.TreeView.|FullyQualifiedName~.TreeSelect.|FullyQualifiedName~.FloatButton.|FullyQualifiedName~.Pagination.|FullyQualifiedName~.Buttons.|FullyQualifiedName~.AutoComplete.|FullyQualifiedName~.Transfer.|FullyQualifiedName~.DatePicker.|FullyQualifiedName~.TimePicker.|FullyQualifiedName~.Dialog.|FullyQualifiedName~.Tour.|FullyQualifiedName~.TabControl.|FullyQualifiedName~.Select.|FullyQualifiedName~.Statistic.|FullyQualifiedName~.Form.|FullyQualifiedName~.SelectionSemantics.|FullyQualifiedName~.Motion.|FullyQualifiedName~.FeedbackStack.|FullyQualifiedName~ToolTipSemanticPartTests|FullyQualifiedName~ToolTipArrowDecoratedBoxContractTests|FullyQualifiedName~PopupConfirmSemanticPartTests|FullyQualifiedName~RecurringVisualWorkTests' --nologo /p:BuildInParallel=false /p:UseSharedCompilation=false
dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~.MotionScene. --nologo /p:BuildInParallel=false /p:UseSharedCompilation=false
dotnet test tests/AtomUI.Desktop.Controls.DataGrid.Tests/AtomUI.Desktop.Controls.DataGrid.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~.DataMemberAccessors.|FullyQualifiedName~DataGridColumnHeaderLayoutTests|FullyQualifiedName~DataGridColumnFilterBindingTests|FullyQualifiedName~DataGridSemanticPartTests --nologo /p:BuildInParallel=false /p:UseSharedCompilation=false
dotnet test tests/AtomUIGallery.Tests/AtomUIGallery.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~FormShowCasePageTests --nologo /p:BuildInParallel=false /p:UseSharedCompilation=false
dotnet build controlgallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj -c Debug --framework net10.0 --no-restore --nologo -v:minimal /p:BuildInParallel=false /p:UseSharedCompilation=false
```

- 源码复核：29 文件的变化全部仅是 owner 泛型替换，两个 internal DirectProperty 同步修正声明泛型；未改变默认值、名称、绑定模式、路由策略、模板或执行逻辑。
- Roslyn 全局语法扫描：4135 个 C# 文件，4323 个真实注册调用（不把字符串中的测试样本当成注册代码）；已列出的 45 property / 6 event 错误全部消除。剩余差异是共享 StyledElement helper 和单独设计的 attached-property owner/host，不做无依据的统一替换。
- `RegisterAttached` 另有独立 owner/host 契约，本次 51 处修复不扩大为全仓 attached-property 架构重构或资源宿主更改。
- 原有 9 个 Avalonia 升级文件均与开始前 SHA256 完全一致；HEAD 仍为 `5fe77abc0760fe375580cef00c60f3a3b3a00a45`，index 未写入。
- `git diff --check` 通过，临时 owner 审计测试、行为测试、专用宿主/辅助源码已删除，未增加永久测试。
- 结构审查仍有 195 条经基线证明的存量布局提示，本次零新增；没有将布局审查声称为无问题。
- 未运行全量回归、真实 NativeAOT 发布、net8.0 Release 或跨平台图形验收；不将本次 focused 证据描述为这些专项已通过。
- 未创建提交或推送，全部 owner 修复留在工作区供用户审查。
