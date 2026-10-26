# CT14 N3-S：Segmented Own依赖与自动高度边界短设计（设计已批准）

基线6bd607676，当前Segmented产品源码尚未修改；原30项通过。沿CT14 N3／既定Own V2设计迁移14字段，三项自动高度依赖有效TrackPadding。此次另需审查的是自动默认高度的0下限，不将第二十批Expander Git批准扩大到本设计。

## 原版实际证据

产品Window／真实明暗，四尺寸、三个字符串选项、SelectedIndex=1，保留同实例。通过公开ThemeConfigProvider配置，资源与实际SegmentedItem.MinHeight分别读取；原版两个有效理论各14处失败，共28处。资源TrackPadding与根Padding已经更新，三项默认高度及四尺寸消费者仍使用覆盖前的值：

| 输入 | 原版资源 LG／Middle／SM | 按现行依赖公式的期望 |
| --- | --- | --- |
| 默认TrackPadding=2 | 36／28／20 | 36／28／20 |
| Own TrackPadding=2,3,4,5 | 36／28／20 | 32／24／16 |
| Own TrackPadding=0 | 36／28／20 | 40／32／24 |
| 明确三项下游高度均17 | 17／17／17 | 保持17／17／17 |
| Own TrackPadding=24 | 36／28／20，配置可正常使用 | 若只接依赖：-8／-16／-24 |

最后一行负值是由现行公式和有效上游推导的候选风险，尚未执行候选产品。当前框架MinHeight要求非负；本地Avalonia源码8eeda4f6的Layoutable.ValidateMinimumDimension／ValidateMaximumDimension也有同一约束。大padding原本被接受，不能用拒绝该配置或忽略资源失败来完成迁移。

原版reset回到默认，明确下游与选择状态保持；首次探针在Items建立前赋SelectedIndex造成的准备错误已单列，随后按现有测试的公开初始化顺序纠正，不计入28处产品反例。尚无本片最终像素或NativeAOT验收结论。

## 方案

1. 只迁移14项、三个默认使用有效TrackPadding直接相减：修复普通配置，但会给大padding配置产生负MinHeight，不推荐。
2. **推荐**：三项自动默认分别取`Math.Max(Global.ControlHeight* - TrackPadding.Top - TrackPadding.Bottom, 0)`。标准默认保持；大padding继续可用，内容自然测量决定实际高度。明确ItemMinHeightLG／ItemMinHeight／ItemMinHeightSM（含0）继续优先，不强制最终Height。
3. 只在模板消费时钳制：资源表仍含不合法的自动默认高度，形成资源与实际消费者不一致，不推荐。

## 推荐范围与保持项

- 仅改SegmentedToken.cs的14项默认声明，保留字段、类型、顺序、identity／key／枚举数值／slot。11项Global-only默认保留公式，三项默认显式声明TrackPadding依赖并增加0下限。
- 不新增公开签名、Token、Part、Style或伪类；不改ThemeConfig解析／优先级引擎、绑定、模板、选择、键盘／指针、方向、圆角、字体、连续动效及自然内容测量。
- Clamp只作用于自动默认，不重写用户明确的下游。非法明确值继续服从既有校验；本片不新增NaN／Infinity或负padding处理策略，也不发明新值域。
- 验证14默认／类型／key／slot、普通Global／Control Global、有效Own、明确下游与0／撤回、非对称与大padding、四尺寸、两方向、默认／Round及原选择／输入合同。保留现有真实场景，不用固定高度掩盖派生错误。
- 实际明暗同历史做自然A/A、1DIP、新旧及恢复；标准默认和明确下游要求兼容，预期Own布局及极值变化单独视觉审查。Mac managed／NativeAOT、临时清理后原30项／NET8／成员审查分别执行。

本设计是L2自动默认值语义修正，批准前不修改产品；设计批准不替代最终视觉、Git、其他Host或完整家族验收。继续主agent在现有checkout执行，不创建分支／worktree，不发布NuGet。

用户已明确回复“批准Segmented推荐短设计”，授权14项V2、三条有效Own依赖及自动默认0下限；其余设计正文保留实施前证据，不表示视觉或Git批准。
