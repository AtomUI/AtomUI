# ComboBox 统一空态实施计划

> **For agentic workers:** 使用 `superpowers:executing-plans` 在本会话执行。用户已要求按现有设计开始修复，不重复申请研发授权。

**Goal:** 修复普通空数据与过滤无匹配的空态，接入四个公开属性并同步 Gallery。

**Architecture:** ComboBox 统一计算候选与有效空态，过滤清理只恢复容器。ControlTheme 声明默认与自定义内容的呈现和绑定，保持既有语义外框。

**Tech Stack:** .NET 10、Avalonia 12.1.4、AtomUI 6.2.11、xUnit/Headless。

**Spec:** [ComboBox 架构设计](../../controls/desktop/navigation/combo-box/overview.md)、[实现设计](../../controls/desktop/navigation/combo-box/implementation.md)。

## 全局约束

- 使用当前分支和目录；不创建分支/worktree，不提交或推送。
- 已授权四个空态 StyledProperty 与文档定义的默认值、布局和行为范围。
- 不改共享基类、过滤匹配算法、选择语义、弹层约束或其他控件。
- 临时测试使用所属测试项目的 `Temporary/Issue526/`，媒体与原始日志在系统临时目录，验收后清理。
- 证据、决定和清理结果统一写入[执行记录](../progress/2026-11-02-combobox-empty-state.md)。
- 最终只运行 ComboBox 和实际修改 Gallery 模块的 focused 检查，不运行 change/full。

## 审查重点

- 空集合在非编辑、编辑无过滤、有效过滤三种入口均有反馈。
- 清空过滤、集合 Reset/替换、关闭再打开不能保留过期状态。
- 仅设置模板与清空自定义内容正确切回默认视觉。
- 空态开关不能取消过滤；原本隐藏的容器不能被恢复为可见。
- `popup.empty` 外框、专用 Style 命中、行尺寸与弹层最大高度保持。

## Task 1：候选与主题

**Files:** `ComboBox.cs`、`Themes/ComboBoxTheme.axaml`、现有 `ComboBoxSemanticPartTests.cs`；临时复现在 `tests/AtomUI.Desktop.Controls.Tests/Temporary/Issue526/`。

**Interfaces:** 四个已定义 StyledProperty；内部只读有效状态；现有 `PART_EmptyIndicator` 与 `ComboBoxPopupEmptyStyle`。

- [x] 用当前源码运行临时复现，确认空集合、集合清空及过滤清空的实际失败；验证 API 缺失。
- [x] 注册四个属性与 wrapper；过滤刷新统一写最终空态，清理仅恢复可见性；内容变化单独更新默认视觉。
- [x] 在既有语义 Border 内声明 ContentPresenter 和默认 Empty，使用 TemplateBinding 与默认 Padding setter。
- [x] 精确运行临时用例；扩展现有语义用例而不净增永久测试，检查过滤与键盘旧契约。

## Task 2：Gallery 与文档一致性

**Files:** ComboBox ShowCase、语言枚举与四份 XLF、现有 Gallery 页面测试、ComboBox 五份设计文档。

**Interfaces:** 复用产品空态 API、专用 Style；公开 API 由源码与控件文档维护，Gallery 只维护示例和 Semantic Parts。

- [x] 添加默认、关闭、自定义内容与模板的空态示例，更新 popup.empty 描述和现有页面检查。
- [x] 通过同一临时驱动取得实际 Skia 截图并检查；保持原始空数据与受限弹层条件。
- [x] 同步设计与实现记录，移除文档先行差异，保留实际证据边界。

## Task 3：清理、验证与审查

- [x] 删除临时测试、驱动、截图和专用产物，保留简短文字证据。
- [x] 执行成员布局检查和 `git diff --check`。
- [x] 执行 `python3 scripts/verification/test.py run --scope iterate --path src/AtomUI.Desktop.Controls/ComboBox --path controlgallery/AtomUIGallery/ShowCases/Navigation/ComboBox`，确认实际范围与结果。
- [x] 请求一次仅针对本任务 diff 的独立审查，处理必要发现，按新增变化重验对应范围。
