# CT13 CB5：ComboBox 手柄与公开候选项 Global 消费

> 使用 superpowers:executing-plans，单 agent 沿现有目录/分支完成；不提交、不创建分支/worktree。用户已授权继续CT13，S7限定视觉已批准。

**Goal:** 先证明并修复内部手柄的父组合Global和公开ComboBoxItem自身Global圆角的实际消费。

**Architecture:** 内部ComboBoxHandle的几何与pressed组合样式归ComboBox；公开ComboBoxItem保留独立AtomUI:ComboBoxItem身份。仅修改已证实的AXAML资源来源；不混合Own颜色、字体、Semantic路由或容器生命周期改造。

**Tech Stack:** 当前Avalonia、.NET10测试、Release NET8、当前Mac managed/NativeAOT。

**Spec:** [输入目录](inputs.md)、[消费规范](../../../engineering/development/theming/consumption.md)。

## 审计与合同

- 手柄主题两处IconSizeSM和一处ColorPrimaryActive仍为Shared，内部IconButton同时保持自己的公开identity；本包只核对父模板明确拥有的尺寸/pressed投射。
- ComboBoxItem是独立Token identity，无Own；主题CornerRadius仍为Shared.BorderRadiusSM。是否为实际缺口先由真实弹层反例证明。
- ComboBoxItem的SharedTokenResourceValue时长由CT11的TokenTransitions按实际owner求值，不重复迁移。
- 候选字号由ComboBox.OptionFontSize在PrepareContainer明确以本地绑定投射，不能误把独立Global.FontSize无效果判作字体断链。字体族/独立使用的字号合同另行实测。
- 所有API、Own名称/ID、原树与尺寸/三行viewport/滚动/输入保持。生成专用Style是唯一部件定制入口。

## Task 1：真实反例与原版轨迹

**Files:** 临时`tests/AtomUI.Desktop.Controls.Tests/Temporary/CustomThemeComboGlobal/`。

**Interfaces:** 现有ThemeConfigProvider和ComboBox/ComboBoxItem/内部IconButton真实模板。

- [x] 原始Control Global IconSizeSM=23应进入实际IconWidth/Height；ColorPrimaryActive应进入真实MouseDown pressed图标；独立ComboBoxItem BorderRadiusSM=0应进入真实容器与ContentPresenter。
- [x] 精确filter `FullyQualifiedName~Temporary.CustomThemeComboGlobal`：记录有效RED及已有Own hover/父字号投射正对照。
- [x] 明暗四尺寸、八项/三行popup、原指针/选择/禁用及相同配置/清除轨迹，AA/1DIP/严格复位。媒体只存项目外登记目录。

## Task 2：最小修复与优先级

**Files:** `src/AtomUI.Desktop.Controls/ComboBox/Themes/ComboBoxHandleTheme.axaml`、`ComboBoxItemTheme.axaml`。

**Interfaces:** 消费现有ComboBoxTokenResource及ComboBoxItemTokenResource的Global键；不新增接口或注册。

- [x] RED后按真实owner替换资源，保留选择器、原值和既有Own来源。
- [x] 热更新/显式0/透明/清除、父/公开子identity隔离；实际编译AXAML ComboBoxIndicatorStyle与ComboBoxPopupListItemStyle覆盖及撤回，并锁定Style类型/关键Setter和无代码回退。
- [x] 原版/候选默认严格RGBA0，显式配置效果与任何自然边缘差异独立记录，不放宽容差。

## Task 3：本包出口

- [x] 当前Mac managed/NativeAOT真实消费，NET8 Release、ComboBox正式文档与作者另步审查。
- [x] 临时源码/helper/工程/媒体清理，常驻净增0；精确ComboBox命名空间验收与git diff --check。
- [x] 对具体视觉变化申请限定审查；完整CT13和其他家族保持开放。

2026-10-29全部分包步骤通过，限定视觉已获用户批准：[结果](input-combo-box-global-results.md)。完整CT13与后续字体/消费者/家族出口继续。
