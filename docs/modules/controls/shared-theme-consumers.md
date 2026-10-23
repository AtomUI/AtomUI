# 共享主题消费与绘制依赖

本文定义通用控件层的实际消费边界。控制级字段取值遵循对应公开 owner 的 Effective Global；没有独立注册身份的内容节点不临时创造 Own identity。状态、字体继承和内容 glyph 必须按真实树拓扑分别处理，不能只检查 Setter 是否存在。

| 路径 | 输入与实际目标 | 默认、优先级和生命周期 |
| --- | --- | --- |
| ArrowIndicator | ArrowSize、TipRadius → 实际 geometry、测量与绘制 | 默认半径 1.5；变化后失效重建；无独立 Own；ToolTip 使用生成的 arrow 专用 Style |
| 输入框体 | 公开owner根几何及有效Global状态颜色 → AddOn/InputControlFrame | owner默认与内部尺寸预设分开；内部OwnerTokenResource沿模板/逻辑内容路线解析，保持根画刷中继与状态；特殊TextArea/数值分段/多选测量职责见控件架构 |
| 文本输入前景 | TextBox/LineEdit/TextArea/SearchEdit 的 ColorText 默认与原生 Foreground → 实际文字 | 根默认读取具体 Control Global；通过框体继承；专用 input Style 可覆盖；状态色仍由原状态样式处理 |
| 跨根字体 | Popup.PlacementTarget 的 FontFamily/Size/Weight/Style → PopupRoot/OverlayPopupHost | ControlTheme 经逻辑 Popup 祖先声明绑定；替换 anchor 自动换源，显式内容属性优先；关闭后的原生宿主缓存另属平台责任 |
| IconPresenter | 当前 owner 的 ColorIconHover → IconBrush → 五个 Icon 画刷槽或 PathIcon.Foreground | 默认从 Control Global 取值；Template 优先级允许显式 child local 值；替换/detach 释放，attach 回放 |
| IconTemplatePresenter | 构建的实际 PathIcon 与宿主 IconBrush/Width/Height | 无独立 fragment，不新增 Own；运行时 content 不在模板内，普通 child Style 不能等价提供 Template 绑定优先级；代码明确说明兜底原因 |
| Rate 字符 | CharacterFontFamily、原生字重/字形、尺寸 → 两张 VisualBrush 的字符对象 | 脱离普通继承树，必须复制已支持字体并在依赖变化后重建；不在每帧构建；不改 SVG 星形内容 |
| SplashProgressBar | 准备阶段全局 BorderRadiusXS、原生四角 CornerRadius、brush → 绘制/合成状态 | 默认 2；确定进度完整四角，合成状态携带完整 CornerRadius；合成循环与停用另按动效合同验收 |
| 原生 TextSelectionHandle | Shared.SelectionBackground、Background、TextSelectHandleSize → native Indicator/PathIcon | Framework asset-only，无 AtomUI Own；保留原资源/Path 名称，提供 ABI 要求的 Border#PART_Indicator；:caret 使用原生伪类，尺寸默认 32×32 |

输入框体的具体角色、根几何和紧凑测量预算见[输入共享架构](../../controls/desktop/data-entry/input-control-architecture-design.md)。图标语义见[Icon](../../controls/desktop/general/icon/overview.md)，字符 glyph 字体见[Rate Token](../../controls/desktop/data-entry/rate/token.md)。

## 内容与预设默认

SVG 坐标、形状比例和 cutout 的白色 fallback 具有内容语义，不能机械替换成面板背景色。宿主仍可通过公开画刷槽和样式定制。

Rate 的字符画刷旧版默认采用平台字形。`CharacterFontFamily` 是明确的内容 Own 字段，默认 `FontFamily.Default`；不从普通 UI 的 Global.FontFamily 自动派生。主题通过该字段或原生根 FontFamily 定制，保留旧默认图像。新增字段应进入后续 Rate Own V2 定义，不把这一消费修复记成整个家族迁移完成。

Splash 在内核提交前加载，只能使用准备好的全局资源与基础元素。420 卡片宽、64 logo、关闭图标尺寸等默认已经位于可替换的 ControlTheme Setter；不能为它们引用尚未提交的普通控件 Own。应用通过 `WithControlTheme`/BasedOn 定制启动样式。详见[启动 Splash](../../architecture/systems/startup/splash.md)。

## 脱树对象与缓存

复制的文字 glyph 不会继承原 owner 的字体，必须显式复制字体并登记绘制、测量和尺寸依赖。持有真实源控件的 VisualBrush 是另一条路径：Tree 拖拽预览、DataGrid header 的 control-content 分支直接引用原可视对象，不用字体快照替代它；DataTemplate 创建分支及拖拽释放继续由对应家族验证。

原生手柄原先依赖未定义的 Fluent `TextControlSelectionHighlightColor`，现在直接消费 AtomUI 的 SelectionBackground。混合 Fluent 应用若曾用该外部 key 定制手柄，应改用 SelectionBackground 或原生 Background。旧模板在当前 Avalonia 缺少必需部件而无法显示，此处属于缺陷修复，没有旧版正常手柄截图可声明零差异。

## 验收范围

分别验证输入值、实际目标、几何/像素、原生优先级和释放，不能用资源存在代替消费。默认零差异和同实例往返需要固定后端、布局与动效采样；拖拽等真实输入引起的合法位置变化必须单独记录。

本文不是全控件验收报告。四类文本输入的原生字体 API 与前景、两类 Popup、上述共享路径有分包证据；其他 owner 的字体、控制级 Global 字段、独立 public 子控件和完整家族状态仍逐项验收。合成动效、NativeAOT、原生显示面及平台矩阵按各自专项记录，未验证部分不能计作通过。

## 波纹消费

WaveSpiritDecorator 将有效原生 easing/range 送入运行时 painter；未提供曲线保持既有 CubicEaseOut。正范围不截断到 8，零或非法范围不构造退化动画；形状切换先取消旧值类型动画。RoundRectWavePainter 的源挖空 geometry 以完整 OriginPoint/OriginSize/CornerRadius 为缓存依赖。默认主题在实际登记模板 owner 上读取 Effective Global；未登记的独立 primitive 回退 Shared。`WaveSizeMotionDuration` 默认跟随 MotionDurationSlow，`WaveOpacityMotionDuration` 默认正 Slow+50ms（Slow为0则0）；显式时长直接进入 Animation，消费端不再加50ms。`WaveSizeEasingCurve` 与 `WaveOpacityEasingCurve` 默认 CubicEaseOut，允许分别配置现有 Easing literal。EnableMotion=false 对两项时长归零。完整家族与原生时间线仍须专项验收。

## 可选字体族消费

Button/Dropdown、Hyper、Split、Option/Group已使用类型化Global FontFamily的直接配置存在位及真实继承回退。Core内部强类型源只读InheritanceParent，ControlTheme保留最终Setter；分离/原生绑定释放清理订阅。合同见[显式Token资源](../core/configured-token-resources.md)，其他owner随家族继续处理，不能将六owner视为全库完成。

四类文本输入的clear及实际存在的reveal使用相同公开owner的图标尺寸/状态色；ContentRightAddOn没有TemplatedParent时不回退到错误的Shared身份。挂载/重挂载、上下文及自身父变化重算路线，原生资源与绑定持有最终优先级，不订阅每个祖先。TextArea没有reveal部件。TextBox/TextArea/SearchEdit及LineEdit的可选字体族和Search/LineEdit有限字号选择已接入真实继承回退；这不代表其他组合owner或完整输入家族通过。
