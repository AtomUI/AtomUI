# CT-11：动效消费与时序重构计划

> 单 agent 使用 executing-plans 执行，不创建新分支/worktree，不自行提交。临时测试默认删除，只跑明确模块的 focused 验证。

**Goal:** 主题和原生动效输入必须到达实际 Animation、Transition 与合成绘制，保留默认视觉、时序及输入体验。

**Architecture:** 沿当前 Native/Avalonia 动效链修复消费、所有权和缓存，不增加第二套时钟/优先级引擎。先证明具体失败，涉及默认时序的改变先把旧默认转成明确的可定制默认，再移除 consumer 内隐藏偏移。

**Tech Stack:** Avalonia 12.1.4，NET10 focused/Skia，NET8 编译，当前主题上下文/资源生命周期。

**Spec:** [总任务](tasks.md#第二阶段共享消费与控件家族)、[消费规范](../../../engineering/development/theming/consumption.md)、[绘制规范](../../../architecture/systems/theming/control-rendering.md)、[验证规范](../../../engineering/development/theming/verification.md)。

## 约束与边界

- 默认前后零未解释/未经批准视觉差异，动画时序也属于默认合同；不关闭动画、重建实例或隐藏控件制造成功。
- 真实输入/实际输出分开观测，包含已有实例、作用域更新、替换/关闭与释放。
- Transitions 只放 ControlTheme 的 style Setter。代码仅用于声明式不能等价表达的运行时计算/绘制，并写明原因。
- CT-10 S1/S2/框体几何/已验收 S4 包提供所需接口；CT-10 全 owner 字体和 S5 出口仍开放，各家族补回，不虚报已完成。
- 不增加未证明有必要的永久测试、不运行全量回归、不使用 CUA、不使用 subagent。

## M1 Wave 实际输入和缓存

文件：`src/AtomUI.Controls/Primitives/WaveSpiritDecorator.cs`、`src/AtomUI.Controls.Shared/WaveSpirit/RoundRectWavePainter.cs` 及确有失败的相关 painter。正式文档归 Controls 模块及相关控件；不把内部 painter 伪造成公开 Own owner。

- [x] 临时测试实际 Window 内的 standalone WaveSpiritDecorator：提供的 Size/Opacity Easing、WaveRange=12、0/非法 range、运行中切换 WaveType；只读取实际 painter/动画数据，不直接改内部状态制造成功。
- [x] 对比原结果：ConfigureWavePainter 固定 CubicEaseOut、range 截到8；切换 WaveType 未停止已有不同类型的动画。按有效失败修复，保留未提供 easing 的 CubicEaseOut 默认。
- [x] 真实 Paint 后变更 OriginSize/OriginPoint/CornerRadius，再 Paint，原始挖空 geometry 必须匹配新形状。缓存键包括全部几何依赖；不每帧重建不变的源 geometry。
- [x] 公共 Button 的主题 range 通过真实指针驱动到实际波纹；默认帧/时序对照、显式配置变化、取消和恢复。形状、命中、焦点和原输入保留。
- [x] 临时测试/媒体清理、最终模块 focused/NET8、对应文档与结果、自审。

## M2 Transition 的动态资源与归属

文件：`src/AtomUI.Core/Data/SharedTokenResourceValue.cs`、相关原生资源/样式物化边界、实际引用该扩展的 ControlTheme；有前证再确定最小改动文件。

- [x] 取得真实 Transition.Duration 在同实例、已有嵌套/兄弟 scope、主题 A→B→A 时的失败。区分字面值扩展、非 Visual 宿主和 scope 选错，不靠源文件字符串检查宣称消费通过。
- [x] 根据失败设计原生优先级及每个 owner 的明确接线，保持已存在视觉/模板/输入状态；证明没有共享 transition 串值、全局订阅或旧对象保留。
- [x] 逐条实际引用清单与 literal/动态合同，绑定能进 ControlTheme 时直接声明。

M1/M2 分包结果见[Wave](motion-wave-results.md)与[Transition](motion-transitions-results.md)。M2仅四个实际模板，其他 literal 消费者仍由对应家族接入。

## M3 明确波纹默认时序及 Splash 动效

- [x] Wave 的 opacity duration 当前隐含 +50ms。用真实动画取得默认时间线；把兼容默认与精确输入分开，再决定原生/Token 输入合同，不直接删除50ms导致默认缩短。
- [x] 核实 easing/range/duration 的主题可定制入口及外部可达性，不把 internal CLR 属性测试当成用户已能定制。
- [x] SplashProgressBar 的1.2s周期、0.3比例、EnableMotion 和四角合成绘制分别取得实际证据；默认保持，静止/隐藏/关闭释放按正式合同处理。

### M3 实施裁定与验收点

- 兼容默认明确为全局 `WaveSizeMotionDuration`（Alias，MotionDurationSlow）与 `WaveOpacityMotionDuration`（Alias，正 Slow +50ms，否则0）；新增 Seed `WaveSizeEasingCurve` / `WaveOpacityEasingCurve`，默认 CubicEaseOut。消费端不再加隐含50ms，显式800ms必须精确进入实际 Animation。EnableMotion=false 归一化为0。
- Wave 默认主题在实际模板 owner 上消费 Effective Global，复用 M2 原生 context/resource 绑定的公共内部 helper，不复制优先级或订阅引擎。未登记独立 primitive 使用 Shared。各家族独立 owner 的完整验收继续回填 CT-12 等。
- Splash 启动前没有普通 Control Own，保留独立 native asset 合同。新增原生 `CycleDuration`（1.2s）、`SegmentRatio`（0.3）、`IsMotionEnabled`（true）；默认主题用 Shared.EnableMotion 声明绑定。周期0/关闭动效保留居中静态段，不继续请求合成帧；显隐/分离仍清理。固定合成时钟相位取样用于精确默认 A/B，另验真实采样位移。
- AC：默认300/350ms及 Cubic不变；独立设置duration/easing能到达实际painter动画；Control Global与全局分离、A→B→A不换实例；禁用后不播放/不请求帧；Splash原周期/比例与默认固定时点像素不变，修改输入改变实际输出，四角保留；全部临时验证清理后focused/NET8。

M3 实际数值、固定时点/真实时钟、清理和当前Mac NativeAOT结果见[精确动效结果](motion-exact-results.md)。

## M4 出口

2026-10-24继续全部任务授权：剩余83组Token时长集合、74个文件（含ColorPicker/DataGrid/GalleryBase）逐条接入现有TokenTransitions；保留每个prototype的Property/Delay/Easing与静态内容值，不新增反射工厂或动画引擎。先取得真实模板RED、保存当前未提交工作树默认，之后验证同实例切换/身份/优先级/释放、资产物化、Skia和当前Mac AOT，清理后focused/NET8。

IndicatorScrollBarThumb另行处理：原HEAD及当前版本的Border.Transitions实际Duration均为0，作用域800ms仍0。模板中的非Control目标不能直接使用Owner绑定；CT-10机械替换没有解决这条动效链。该集合迁到ControlTheme的style Setter，默认仍为0；只有规范配置直接提供MotionDurationMid时才使用有效时长。复用已有WhenExplicit存在位与owner解析，在TokenTransitions声明可选的UnconfiguredDuration=0；不以数值相等猜配置，不给内部节点造identity。直接改成普通Mid默认200ms会改变既有时序，故不采用。独立未登记primitive保持0，真实公开owner下验证明确配置及清除。另将AvaScrollViewer分隔区的模板内100ms集合移至style Setter，以默认100ms的MotionDurationFast接入；视口、范围和可见滚动条保持。GalleryBase的内部扩展访问经真实物化失败后补齐，与现有产品包使用相同Core内部消费边界。

- [ ] 按实际 transition/motion/wave 消费路径列举公开 owner、值、时序、像素、scope 与释放结果，未验证平台明确挂 CT-25。
- [x] 本次83集合接入的临时验证源/驱动/媒体和专用产物清理；清理后受影响模块及正式文档已同步，见[扩展结果](motion-consumers-expanded-results.md)。
- [x] 本次接入作者自审、tasks/results已更新。第一项逐家族状态出口仍开放，M1完成不能代替 M2/M3 或 CT-12 按钮原生闪动验收。
