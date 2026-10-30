# Application 资源宿主与外部主题集成方案记录

日期：2026-10-20。关联 [#506](https://github.com/AtomUI/AtomUI/issues/506)、
[#507](https://github.com/AtomUI/AtomUI/issues/507)。状态：方案已由对应模块设计实施；当前验收与发布边界以模块验证页为准。

本文保留两项回归的背景、目标与方案比较。长期设计与验收规则已迁入对应模块文档，由第 4 节链接的正式文档拥有；
本文件不再独立定义资源宿主契约。当前版本的应用接入方式仍以
[开发者集成指南](../../guides/theming/framework-theme-integration.md)为准。

## 1. 问题与根因

| 问题 | 已复现的结果 | 根因 |
| --- | --- | --- |
| #506 外部主题覆盖 AtomUI 基础主题 | App.axaml 加载后，WindowDrawnDecorations 从 AtomUI 主题切换为 Fluent 主题 | ThemeManager 作为 Style 提前加入 Application.Styles；AXAML 随后加入的 FluentTheme 在同键资源查询中优先 |
| #507 应用资源声明使语言资源消失 | Dialog.Ok 的资源查询由成功变为失败，而 GetLocalizer 仍能返回服务 | 语言 Provider 挂在旧 Application.Resources 的合并字典中；AXAML 显式赋值新 ResourceDictionary 后，旧字典离开资源链 |

AtomUI 提前提交内核是有效的启动需求。真正的问题是把框架长期资源绑定到可被 AXAML 替换的字典，或依赖外部主题
在 Styles 集合中的加载先后来决定同键资源归属。修复应改变资源挂载边界，保留现有启动时序。

当前相关实现位于：

- [ApplicationScope](../../../src/AtomUI.Core/ApplicationScope.cs)：语言 Provider、FlowDirection Style 和应用级生命周期。
- [ThemeManager](../../../src/AtomUI.Core/Theme/ThemeManager.cs)：主题资源、根 ThemeContext、系统外观订阅和释放。
- [WindowTheme](../../../src/AtomUI.Desktop.Controls/Window/Themes/WindowTheme.axaml)：消费 WindowDrawnDecorations 类型键。

## 2. 设计目标与不变量

1. `Application.Initialize()` 之前，AtomUI 的主题、语言和服务已经完整可用。App.axaml 与 Prism Shell 不需要延迟消费。
2. App.axaml 可以按 Avalonia 正常方式声明 `Application.Resources` 和基础主题。
3. FluentTheme 在 AXAML 中出现的位置，不得使同键基础资源意外替换 AtomUI 的默认主题。
4. 应用在 `Application.Resources` 中有意定义的同键覆盖仍优先；控件和页面的局部覆盖继续遵循正常作用域规则。
5. 普通 Style Setter、BindingPriority、模板结构、Semantic Part、Token key 和公共 API 不因本次修复改变。
6. 主题和语言服务实例、Provider 与已物化资产继续复用；不复制 Provider，不重建管理器。
7. 保留包启用、OnCommit/OnFirstUse、按键索引、开放资源回退、主题事务、运行时追加包和裁剪语义。
8. 挂载、失败回滚与释放必须对称；一个对象不能被重复挂载到两个 owner。
9. 不增加每控件的字段、订阅或轮询，不增加主题集合的持续重排。
10. 不把正确性或性能退化作为修复代价；未通过相应验收时不得发布。

本次不改变 Dispatcher 初始化、后台 Prepare、动画 Clock、动画时长、Splash 或主窗体交接策略，也不增加 Core 对
Fluent 等外部框架的依赖。

## 3. 方案决策

采用 **Application.Styles.Resources 作为共同的框架资源挂载位置**，分开管理资源提供与全局 Style 匹配。

| 备选方式 | 判断 |
| --- | --- |
| 把 AtomUI 提交推迟到 Initialize 之后 | 无法满足 AXAML 与 Initialize 内构造 Shell 的资源需求 |
| Loaded/Dispatcher 中补挂或重新排序 | 初始消费仍可能出错，并引入二次通知和时序依赖 |
| 持续监听 Styles，强制 AtomUI 始终排最后 | 会干预用户样式、增加重入和重复应用成本 |
| 针对 Window 单独固定模板或改资源键 | 不能解决语言资源丢失，也不能覆盖其他共享基础类型键 |
| 共同框架资源宿主 | 在原生资源层级表达优先级，同时避开 Application.Resources 被 AXAML 替换的路径 |

`Application.Styles` 是应用的稳定集合入口。普通的 Application.Styles 内容声明向该集合加入子项；
普通 Application.Resources 根字典声明不替换这个集合自身的 Resources。

直接替换 `Application.Styles.Resources`、清空整套全局接入状态或手工拆除框架资源，不属于自动恢复范围。
本方案不为这些显式改写增加全局监听与自我修补机制。

## 4. 对应模块的正式设计

| 文档 | 正式责任 |
| --- | --- |
| [应用资源宿主设计](../../architecture/systems/theming/application-resource-host.md) | 共同宿主、框架前缀、覆盖兼容性、所有权、挂载/释放与性能/AOT 边界 |
| [应用资源宿主验证](../../architecture/systems/theming/application-resource-host-verification.md) | 实现与验收证据、修复矩阵及性能采样与门槛 |
| [启动生命周期](../../architecture/systems/startup/lifecycle.md) | Initialize 前提交、自有服务和包钩子的资源消费边界 |
| [本地化运行时](../../architecture/systems/localization/runtime.md) | 独立语言服务、Provider、revision 和 RTL；物理挂载交由 Core |
| [Core 模块](../../modules/core/overview.md) | ApplicationScope 与 ThemeManager 的源码职责 |
| [Localization 模块](../../modules/localization/overview.md) | 本地化服务与应用资源接入的依赖边界 |

正式文档不依赖本记录才能成立，实施以模块契约和验证页为准，不在多个位置维护平行版本。
独立 Style.Resources 同键覆盖的兼容性变化已进入 6.2.7 中英文迁移说明；本记录不单独认证发布状态。

## 5. 实施与交付边界

模块设计文档先于源码修改更新。源头修复已进入 Core 源码；原始 RED→GREEN、生命周期与包事务、实际 NativeAOT、
性能采样、临时验证处置及最终模块结果由模块验证页维护，未覆盖的平台和环境限制也在那里明确记录。

开发者示例已同步为普通 AXAML 接入，兼容性变化进入 6.2.7 迁移说明。设计记录不授权发布或关闭 Issue；
发布按用户本轮明确指令执行。
