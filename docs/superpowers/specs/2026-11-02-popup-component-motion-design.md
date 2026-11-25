# 弹层组件动效对齐设计决策记录

日期：2026-11-02。状态：已按用户要求纳入正式设计文档；产品实现未启动。

## 正式契约与当前实现

- [弹层组件动效架构](../../architecture/systems/motion/popup-motion.md)：配置 owner、主题覆盖、几何/原点、播放时序、宿主和资源边界。
- [内置轨迹与组件规格](../../architecture/systems/motion/built-in-motion.md#弹层组件选择)：组件对应关系、动效族、曲线和时长。
- [源码差距与验证状态](../../architecture/systems/motion/verification.md#弹层组件动效接入状态)：默认接入和平台验收的真实状态。
- 各控件的 overview.md/implementation.md/changelog.md 只维护其语义、接入责任及设计变化，不重复共享算法。

正式文档拥有当前设计事实；本记录仅保留参考基线、方案取舍和本次授权，不作为正式文档的依赖。

## 参考基线与已知前提

- AtomUI：4513cfd2898c66d90e2d137f8ac8fc2d171f5598，6.2.11，Avalonia 12.1.4。
- Ant Design：按本地优先规则使用 /Users/chinboy/Projects/ReferenceProjects/ant-design，6.6.6，211c6ed29b0d2abd18485e293681f8a4e415240b，核对时工作树干净。
- 组件封装之外补读官方 npm 发布源码：@rc-component/menu@1.5.0、trigger@3.10.1、select@1.10.1、picker@1.12.2、dropdown@1.0.3、tooltip@1.5.2。它们是明确选取的辅助版本；Ant Design 参考仓库没有 lockfile，不能声称是所有消费端唯一解析版本。临时源码摘取在核对后删除。
- 用户确认：此前未接入动画是受跨平台 PopupRoot 阴影能力阻塞；现该前置条件已经解决。直接复用现有阴影和 PopupSurface，不重建阴影系统。
- 原始问题入口见 [issue #525](https://github.com/AtomUI/AtomUI/issues/525)。本次源码评估没有重跑 Windows 原始探针，不作为运行修复证据。

## 方案取舍

| 方案 | 结论 |
| --- | --- |
| Popup 元数据统一设置一种动效 | 不能表达组件族、方向、原点和时长差异，改变通用原语 |
| 各控件独立处理创建和翻转 | 重复算法和生命周期责任，容易遗漏 |
| 组件主题选择规格、共享解析几何、原引擎播放 | 采用；保留现有公开属性、用户覆盖和单一宿主状态机 |

独立 builder 审阅补齐两项关键差异：普通 Menu 的 RTL 最终右上原点；Dropdown 内嵌子菜单使用独立样式、固定左上原点。二者已进入正式契约。
审阅同时确认布局后重新取值、唯一配置 owner、默认反向原点与自定义 Motion 边界，最终无重要未决设计问题。设计审阅不代表运行验收。

## 本次授权与排除范围

用户要求：“可以根据这一版方案更新对应控件的正式的设计架构文档。”
本次授权覆盖正式架构、组件规格、控件接入约束、Token 语义与文档导航/设计变更记录的同步。
不修改产品或 Gallery 源码，不创建分支/worktree，不提交，不把文档定稿计作功能完成。

源码变更和运行验收另行执行；原始 11 条入口、临时验证清理、受影响模块、Release/NativeAOT 与实际平台证据仍按正式契约验收。
