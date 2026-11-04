# Popup Motion 默认配置简化

用户已同意移除 DefaultMotions，保留 OpenMotion/CloseMotion，并要求不得引入资源泄漏。沿用当前 feature/motion 工作目录，不创建分支/worktree，不提交。

## 有限完成条件

1. 删除 DefaultMotions、PopupMotionPair 与几何属性回传绑定；组件主题直接给两个公开属性提供默认值。
2. 主题默认按样式实例创建独立定义，避免共享可变配置。Avalonia 的普通 Setter 值会共享；使用 ITemplate 的按实例构建路径，工厂只持有静态无捕获函数。
3. 默认定义实现实际的弹层准备能力：只在播放前接受值类型几何，生成对应内置 Motion；方向、原点、反向衔接和 native 阴影缓冲沿用既有规格。无默认模式属性、全局定义注册表或 owner 缓存。普通用户 Motion 完全不经过该能力。
4. 不增控件/窗口/资源订阅、计时器或 CTS；定义与工厂不引用 owner/target。运行对象沿用已有释放路径。
5. 验证覆盖实例隔离、Style/Binding/null/ClearValue、实际方向/原点、取消/替换/模板重建/卸载及长期持有定义时的 GC；核对分配和布局、原生 click/hover 冒烟及 AOT。
6. 删除本轮临时测试及专用产物后运行受影响模块检查；更新正式文档并仅暂存本轮授权改动。

## 审计和实现顺序

- 现有责任：族基类拥有两项配置，Popup 拥有实际几何和播放生命周期，Core 拥有帧驱动。
- 冗余：组件 owner 的 DefaultMotions、Popup 的反向发布、几何通知和默认值绑定链。
- 保留：公开 API、属性优先级、显式 null、曲线、位置/限高/滚动、关闭状态机、Token 动态响应和既有释放修复。
- 新增内部构件仅为 ITemplate 默认创建器和可执行几何准备契约；六个私有默认定义（Slide/Zoom 与 NavMenu 两种运行形态）沿用既有 Slide/Zoom Build，不复制曲线或建立播放引擎。
- 执行：临时 RED → 默认创建/准备改造 → 接线迁移与 GREEN → 生命周期/性能/原生/AOT → 文档/清理/精确模块检查 → 一次整体独立审阅 → 暂存。

## 排除范围与既有证据

不修改 Issue529 临时测试和用户 ComboBox 格式改动；不扩大为全量回归。不宣称 Windows 原始场景实机通过、显示 FPS 提升或 issue 已可关闭。此前已验证且本轮未触及的 Core 播放及平台阴影能力复用既有记录，本轮只重验默认配置路径的实际依赖。

权威本轮记录：[执行与验收](../progress/2026-11-04-popup-motion-theme-defaults.md)。


## 审查补充：静态默认值类型

用户提供 Rider 类型报错：静态成员声明 PopupMotionTemplate，目标属性要求 AbstractMotion。修正限定为4个源文件及相应文档：
去掉包装类，静态成员声明并返回真实 AbstractMotion，六个私有默认定义实现 ITemplate 复制配置；C# Flyout 路径显式使用同一复制入口。
保留实例隔离、null/覆盖合同与无反向持有；不改 AXAML 动效选择、几何、Core 帧管线或交互。先临时类型断言 RED→GREEN，
再验证实际主题/非视觉实例和回收、相关控件模块及 AOT，清理后暂存，不 commit。不因该诊断重跑全量或上一轮语言包完整消费链。


## 全组件默认值复审

用户要求全面检查其他控件的 OpenMotion/CloseMotion 默认类型、内存与性能，暂存但不commit。有限完成条件：

1. 核对源码全部15个属性声明入口、15份主题30个默认Setter、14个默认原型及普通Flyout/MenuFlyout/DataGridTreeFilter的C#装配；基于每条实际引用检查声明类型与运行值可赋给真实目标属性。
2. 实例矩阵覆盖选择器、四种Picker、三种AutoComplete、两种ColorPicker、按钮、菜单、Tooltip、FlyoutHost/PopupConfirm、非视觉Flyout及其继承路径。检查双方向、独立副本、局部null/Clear/Binding和默认读取缓存。
3. 按共享默认实现选择实际owner回收检查，强持有定义/原型、保持父窗口存活，覆盖换模板/卸载与非视觉托管替换。新增运行时代码时必须先有对应失败证据。
4. 核对默认创建、绑定及播放的获取/释放路径；重复读取不得重复分配，逐帧无默认重建/资源查询/布局。没有源码变更则复用已匹配的AOT、net8/net10及固定clock证据；不重复昂贵发布，不宣称FPS。
5. 删除本轮临时验证，再做精确模块检查、一次独立复审，更新原权威记录并只暂存本轮内容。

排除其他任务的Empty代码/文档及用户ComboBox格式；不创建分支/worktree、不全量回归、不更改TreeView的null回退或普通Popup的空默认。当前审计没有发现新类型不匹配，不为制造代码差异添加封装或抑制诊断。详细证据继续集中在原执行记录。
