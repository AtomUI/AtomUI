# AtomUI 多设计语言动效承载评估与实施方案

2026-11-27 · 方案评审稿。源码基线为 release/6.0 的 91c061ebd，定向运行复用工作区现有 Debug/net10.0 产物。动效的职责、回退与交接条款由[语言级动效覆盖规范草案](../../architecture/systems/motion/design-language-overrides.md)维护。

## 1. 结论

保留显式有限动效的准备、播放、替代与终态恢复机制。首要缺口在控件和设计语言之间。各类动画已有执行路径，但语言作者缺少按用途提供实现的入口。点击反馈和依赖真实定位的自定义弹层轨迹还缺少公开能力。扩展重点是**动效选择与受控的呈现目标**。重建动画引擎、允许各语言自行挂接业务输入处理器或把可执行工厂放入普通 Token，都会重复已有职责。

本轮审查覆盖下表中的实际消费者。物理弹簧的速度连续交接、无限粒子系统和跨根共享元素转场缺少可用于承诺发布支持的证据，应按实际语言需求单独评估。

## 2. 当前承载能力

| 路径 | 已有机制与证据 | 设计判断 |
| --- | --- | --- |
| 有限轨迹 | 外部可派生 AbstractMotion，通过 MotionBuilder 使用透明度、位移、缩放、旋转、类型化属性、串并联及布局通道。MotionPlayback 在核对挂载、排列和目标能力后接管请求，相容通道可重定向。 | 足以作为多语言有限轨迹的执行层。纯曲线或插值不提供任意物理模拟所需的速度状态。[定义](../../../src/AtomUI.Core/Animations/Motion/AbstractMotion.cs) · [构建器](../../../src/AtomUI.Core/Animations/Motion/MotionBuilder.cs) · [播放端](../../../src/AtomUI.Core/Animations/Motion/Playback/MotionPlayback.cs) |
| 弹层开闭 | 控件已有可为空的 OpenMotion/CloseMotion，Select 从静态 PopupMotionDefaults 取默认值。内置预设能获得真实 placement、翻转、RTL、锚点和内容矩形，准备接口与几何类型为 internal。 | 普通自定义轨迹可替换。若语言需要对齐箭头或避让后的真实锚点，应由 Popup 所属包提供最小只读几何准备契约，沿用现有开闭属性与物理宿主。[弹层默认值与几何](../../../src/AtomUI.Desktop.Controls/Popup/PopupMotion.cs) · [现有开闭契约](../../architecture/systems/motion/popup-motion.md) |
| 属性状态过渡 | TokenTransitions 支持按 owner 和局部作用域解析时长，同一 factory 最多共享 8 组时长向量。部分 Button 过渡把曲线写在 AXAML 中。PrimeNG Nora 已需要颜色立即改变、弹层继续运动。 | 标量调节部分可承载。若只换曲线或通道集合就要复制整份模板，才在已证实的过渡组开放编译定义。缓存键须反映曲线等有效变化。[过渡工厂](../../../src/AtomUI.Core/Theme/Resources/TokenTransitionsFactory.cs) · [Button 主题](../../../src/AtomUI.Desktop.Controls/Buttons/Themes/ButtonTheme.axaml) · [PrimeNG 规则](../../../src/DesignLanguages/PrimeNG/AtomUI.Desktop.Controls.Themes.PrimeNG/Stages/PrimeNGMotion.cs) |
| 激活反馈 | Button 查找具体 PART_WaveSpirit，按压状态退出且变体适用时排队播放。装饰器固定选择三种内部 painter，播放中忽略新的 Play。Switch、CheckBox、Radio 等也直接调用它。Play 无输入来源或位置参数。 | 可调画刷及部分指标。默认模板无法接入外部反馈算法。按实际指针位置扩散、改变内外裁切或选择替代呈现需要新协议。[Button 消费路径](../../../src/AtomUI.Desktop.Controls/Buttons/Button.cs) · [装饰器](../../../src/AtomUI.Controls/Primitives/WaveSpiritDecorator.cs) · [内部绘制接口](../../../src/AtomUI.Controls.Shared/WaveSpirit/IWaveSpiritPainter.cs) |
| 持续反馈 | MotionGate 处理有效可见性、活动源、挂接和开关。Spin 使用合成动画，Skeleton 等也使用 gate。 | 可复用启停机制。替换加载指示器时先用公开模板及专用呈现器。持续执行器的停机与替换须有独立释放路径。[MotionGate](../../../src/AtomUI.Controls/Motion/MotionGate.cs) · [Spin](../../../src/AtomUI.Controls/Spin/AbstractSpinIndicator.cs) |
| 主题交接 | MotionSettle 收束提交窗口内新启动的适用动画。已有历史验证包含特定 macOS 原生及 NativeAOT 场景。启动窗口前已在播放的动画、专用合成循环与自定义时钟各有单独边界。 | 可复用提交收束优化。还须补齐旧装饰取消、持续后端停止及 Popup 的关闭意图核对。收束优化不可用时，业务次数与稳定终态仍须正确。[收束边界](../../architecture/systems/motion/theme-commit-settle.md) · [历史验证](../../architecture/systems/motion/verification.md) |

## 3. Ant 波浪的差距

本地参考源码位于 /Users/chinboy/Projects/ReferenceProjects/ant-design，版本为 6.7.0（c811baa979）。一般波浪的扩张为 400 ms，淡出为 2 s，起始透明度 0.2，外扩 6 CSS px，采用 motionEaseOutCirc。满足 quick 条件的 Checkbox/Radio 另用 motionDurationSlow 与 motionEaseInOut。WaveConfig 支持关闭、定制效果和触发阶段，默认 click。桌面对标要同时核验曲线、几何、触发时机及适用状态。[上游固定版本样式](https://github.com/ant-design/ant-design/blob/c811baa979c82b0170880cb38c9c84d8eb8635fa/components/_util/wave/style.ts) · [官方 WaveConfig](https://ant.design/components/config-provider/#waveconfig)

AtomUI 两个时长在缺省路径上都读取 MotionDurationSlow，传给 painter 时给透明度时长额外增加 50 ms，范围上限为 8。两个 easing 属性变化会触发更新，但实际写入 painter 的是同一个默认 CubicEaseOut。Ant 取值包的 WaveStartOpacity 为 0.4。直接开放现有 painter 仍缺少激活位置、裁切能力和目标所有权。上述差异来自源码，实际像素与呈现时长仍需原生窗口复核。[装饰器](../../../src/AtomUI.Controls/Primitives/WaveSpiritDecorator.cs) · [Ant 取值](../../../src/DesignLanguages/AntDesign/AtomUI.DesignLanguages.AntDesign/TokenEvaluation/AliasTokenRules.cs)

Shadcn、PrimeNG 和外部 Int 语言目前都将 EnableWaveSpirit 默认设为 false。NormalizeFinalBehavior 还会将波浪范围和起始透明度归零。因此“该语言关闭 Ant 波浪”和“该槽位明确关闭所有装饰反馈”需要独立表达。试点用一个合成的内部扩散效果验证正向替换，Int 的实际默认效果以其设计依据为准。[归一化规则](../../../src/AtomUI.Core/Theme/DesignTokens/Definitions/DesignToken.cs) · [Shadcn 语言](../../../src/DesignLanguages/Shadcn/AtomUI.DesignLanguages.Shadcn/TokenEvaluation/LanguageTokenRules.cs) · [PrimeNG 语言](../../../src/DesignLanguages/PrimeNG/AtomUI.DesignLanguages.PrimeNG/PrimeNGLanguage.cs)

## 4. 需要提前验证的三处实现风险

- **弹簧与属性过渡。** 保留 Avalonia 的 SpringEasing。复审实验已将故障定位到参数化字面量进入动态转换器和编译 worker 的共同入口。对象冻结和由已解析值生成代码的路径可复用。修复应集中在解析契约，并用同一个真实主题文件验收编译消费。Ant 状态时长按控件、状态和通道选择，Nora 可统一关闭微过渡，两者的开闭编排仍单独取值。[现有值处理](../../../src/AtomUI.Core/Theme/Schema/TokenValueOperations.cs)
- **编排与结构微动效。** Dialog 的 Zoom、Drawer 的位移和 FeedbackCard 的入退场目前在内部类型里实现，可复用 Build 逻辑，并由 MotionResource 选择对应槽位。Tab 或 Segmented 的指示器移动还涉及运行中布局位置，适合以现有呈现器承载 IndicatorMotionMode。实际方向、RTL、宿主/遮罩和点击原点由控件提供，语言定义只读取准备快照。[Dialog](../../../src/AtomUI.Desktop.Controls/Dialog/DialogMotions.cs) · [Drawer](../../../src/AtomUI.Desktop.Controls/Drawer/DrawerPanelMotion.cs) · [消息卡片](../../../src/AtomUI.Desktop.Controls/Primitives/FeedbackStack/FeedbackCardMotion.cs)
- **反馈尾动画与裁切。** 采用主规范 第 3.3 节 的公开 Playback。宿主在调用外部代码前登记代次与租约，应用仅需现有 IDisposable。先验证相同节点再租时迟到完成不能误删，以及外扩波浪经过祖先视口仍正确裁切。只在活跃反馈上维护运行资源。

## 5. 本轮可执行验证

| 验证 | 实际结果 | 可据此判断的范围 |
| --- | --- | --- |
| 本地 Core 测试程序集选择 MotionExecutionTests、MotionSettleTests、TransitionDurationTests | 19 项通过，0 失败、0 跳过 | 所测组合、提交收束和时长路径 |
| 本地 Controls 测试程序集选择 MotionGateTests | 13 项通过，0 失败、0 跳过 | gate 条件、可见性、挂接释放和活动源行为 |
| 独立非友元程序集通过公开 API 自定义 AbstractMotion，在 Headless 下替换同一已排列目标的四秒轨迹 | 旧请求 Superseded，新请求 Completed、WasAnimated=false。不透明度变为 0.65，Reset 后恢复 0.9 | 有限轨迹外部定义、瞬时替换和底值恢复可用 |
| 同一外部程序集尝试引用 IWaveSpiritPainter 与 IPopupMotion | 两处得到预期的 CS0122 编译诊断 | 两个内部协议均不能作为公开扩展点 |

本次独立实验引用已有程序集，临时驱动和产物已清理。真实 NuGet 登记、语言覆盖热切换、原生帧率和新协议属于待实施验收。原生对照应复用本仓[性能采样入口](../../../tools/performances/README.md)。

### v1.0 复审修订的最小实验

以下使用当前 Debug/net10.0 程序集、Avalonia 12.2.0（5d9e6d1b）和临时非友元驱动。探针已清理。Spring 的诊断直接调用现有 codec、生成器和复制路径，内部调用只用于定位，不能替代发布包消费。

| 项目 | 已得到的证据 | 正式实现还须证明 |
| --- | --- | --- |
| 主题默认值 | 红：内层普通 Style 绑定使语言主题胜过应用普通 Style。绿：仅把框架 Style 放在 Application.Styles 首位，作用域只发布动态资源，应用普通 Style 可胜出。移除后局部资源变化生效；实际设置 Template 优先级输入、触发样式、本地值再逐层清除，均恢复正确来源 | 框架解析器接入真实 ThemeConfigProvider；局部应用 Style、真正编译的 TemplateBinding、用户 StyleKey 资源与 LoadPackages 都需覆盖 |
| 三类回退 | 现有“启用语言缺 Token 规则”定向测试 1/1 通过。CLR 对照中父行为改 A=1，子行为只改 B=2：直接派生默认类得到 (0,2)，显式派生父行为得到 (1,2) | 发布包中分别登记 Token 规则、完整行为对象、可选动效槽；显式 None 与未登记产生不同结果 |
| IME 边界 | Headless 经 Avalonia 的 TextInputMethodClient 注入 pre-edit “pin”。只更换祖先模板，旧 TextBox 即脱离，替代 presenter 的预编辑为空。对照驱动暂存两次替换，旧编辑器保持原串；模拟一次文本上屏“拼”，把已提交值交接给最新模板后结果仍为一个字 | 对照是人工交接。真实事务版本隔离、macOS/Windows 等声称支持平台的原生 IME、焦点、绑定写回次数仍待验收 |
| 尾动画回收 | 同一个 Avalonia 节点在 Panel 中归还再借出。未校验租约的旧完成回调能删除新节点；加借用身份后旧完成无效，最终节点数为 0 | 仅为租约反例与对照。真实公开 Playback、取消时重入、T2 绘制、滚动裁切和外部包均待验 |
| 微状态时长 | 读取固定版本 Ant 6.7.0 的 horizontal Menu 与 Button 样式。Menu 背景/边框取 Slow，普通 Button 取 Mid，Button 的另一些通道取 Slow。将两者压成 200 ms 会改变上游配方 | Ant Menu/Button 真实轨迹与曲线、Nora 零时长状态和正常 Select 打开在相同场景验收 |
| 弹簧定位 | 对 (1,120,12,0.5) 做独立快照复制，改原对象刚度后副本仍为 120。将已解析值经实际 Pack → Write/Read → EmitValue → C# 编译、加载，四个参数和五个采样点吻合。反向从格式化出的 Spring(1,120,12,0.5) 开始，动态转换报 InvalidOperationException，构建 codec 的候选值中也缺少 Easing | 当前字面量端到端仍为红。动态配置和真实编译主题 NuGet 均能吃入同一文本并通过采样，才算通过 |

## 6. 实施次序

1. **锁定行为和视觉基线。** 用真实 Button 和 Checkbox/Radio quick 样例记录轨迹、重复触发、键盘、拖出取消和禁用。保留已有弹层 null、ClearValue、关闭反向及持续 gate 用例。区分恢复 Ant 6.7.0 视觉基线和新增扩展能力，现有对外行为的调整需单独记录兼容性。
2. **完成 Button 激活反馈的外部替换。** 以所在表面的按需装饰宿主替换常驻波浪节点，以两阶段 InteractionEffect 提供默认外轮廓、外部内部扩散以及 None。保留业务 XAML，分别验证波浪外扩、扩散不越界、用户点击次数以及只换模板或只换动效。默认路径与禁用路径分别采样。
3. **接通位置相关弹层轨迹。** 在 Select 或 Tooltip 上用真实 RTL 和碰撞翻转验证外部定义，再覆盖实例/局部作用域、开关反向及宿主更换。Button 与 Popup 的正式 NuGet 独立消费和适用 NativeAOT/裁剪验证合批进行。
4. **按证据推广。** 用 PrimeNG Nora 验证状态色零时长和弹层非零时长共存。用 Spin 验证循环中切语言、不可见停机及重新可见的静态/动态结果。通过后按有需求的控件家族接入。只依赖现有 Token 的控件可继续使用原路径。

发布前按主规范第 6.2 节合并验证 NativeAOT。负例运行只用 Button 的应用并查看无关专属片段的 ILC 闭包；正例分别用 Token-only 和生成 Semantic Style-only 保根，再用另一真实控件场景证明发布后的模板命中与播放终态。三个入口分别归因，不能靠测试代码直接引用目标类型补根。本轮尚未发布这些目标实现的 NativeAOT 样例。

按主规范的最小设计约束，优先复用播放端、曲线和现有 IDisposable。槽位、缓存或池的扩大须有真实消费者、基线成本和正确性收益；先验证单个用途再决定推广。公开 API 评审需要看到外部定义怎样使用可信几何上下文、怎样产生两类反馈、谁持有运行资源和终态，以及默认路径的实测成本。实现沿用控件规范 v1.0 的四类包结构与 LanguageTokenType 继承链，通过[替换验收](../../architecture/systems/motion/design-language-overrides.md#5-合规验收)后再确认公共 ABI 的发布就绪状态。
