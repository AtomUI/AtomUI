# TimePicker Token 设计

本文档定义 TimePicker 相关控件 Token 的专属语义、分类、使用范围和兼容边界。共享输入表面分层见 [输入控件共享架构设计](../input-control-architecture-design.md)，控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。TimePicker 整体架构见 [TimePicker 桌面版架构设计](overview.md)，内部实现原理见 [TimePicker 桌面版实现原理](implementation.md)，设计和契约变化记录见 [TimePicker Changelog](changelog.md)。

## 1. 定位

TimePicker Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- `TimePickerToken`，scope id 为 `TimePicker`，源码位于 `src/AtomUI.Desktop.Controls/TimePicker/TimePickerToken.cs`。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| 尺寸与密度 | 控件高度、宽度、图标尺寸、内容最小尺寸。 | `ItemHeight`、`ItemWidth`、`PeriodHostWidth` |
| 间距与布局 | padding、margin、gap、offset、popup content padding。 | `ItemPadding`、`ButtonsMargin`、`RangePickerArrowMargin`、`HeaderMargin` |
| 颜色与状态视觉 | 文本、背景、边框、hover、selected、active、disabled 视觉。 | 按源码 Token 语义维护 |
| 结构与装饰 | 圆角、阴影、指示器、弹层和装饰线相关变量。 | `RangePickerIndicatorThickness` |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

## 3. 控件专项模型中的 Token 使用

TimePicker 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 默认值从 SharedToken 派生，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。

时间列默认 `ItemWidth=ControlHeightLG × 1.4`，`ItemHeight=ControlHeight-4`，时段列默认同宽；默认高度基线为 28、列宽为 56。`ItemPadding` 将行文本按列宽与行高的差值定位，行两侧间距与圆角由 TimeViewCell 主题表达。候选、悬停、输入有效性和 `IsNeedConfirm` / `IsChangeOnScroll` 不进入 Token。

### 有效 Own 与真实消费

TimePickerToken 使用 DependencyDefaultsV2（Revision1），保留8个字段的名称、类型、资源ID、slot及未配置默认。PeriodHostWidth读取有效ItemWidth；ItemPadding读取有效ItemWidth和ItemHeight，明确下游值优先，清除恢复相应默认。没有新增TimeView identity或外部API。

| 字段 | 默认输入 | 实际区域／边界 |
| --- | --- | --- |
| ItemHeight | 有效Global ControlHeight-4 | 时间行最小高度、列布局行高、header高度；正行高热更新保持逻辑视口／分数偏移，非法值边界仍待单独核定 |
| ItemWidth | 有效Global ControlHeightLG×1.4 | 时／分／秒列宿主宽度 |
| PeriodHostWidth | 有效Own ItemWidth | 12小时制AM/PM列宿主宽度，24小时制该列隐藏 |
| ItemPadding | (有效Own ItemWidth-ItemHeight)/2的left Thickness | 时间行内间距；不承诺所有自定义宽高下文字精确居中 |
| ButtonsMargin | 有效Global UniformlyMarginXS的top Thickness | TimePickerPresenter内PART_ButtonsLayout.Margin，服务单值／范围时间弹层 |
| HeaderMargin | 常量bottom3 | TimeView header；时间弹层中通常隐藏，日期嵌入面板可显示 |
| RangePickerArrowMargin | 有效Global UniformlyMarginXS的horizontal Thickness | RangeTimePicker可选Own覆盖；缺省／清除保留原InfoPickerInput有效Own |
| RangePickerIndicatorThickness | 有效Global LineWidthFocus | RangeTimePicker指示条可选Own厚度，0值有效；回退规则同上 |

时段列此前误读ItemWidth；footer此前借用DatePicker.ButtonsPanelMargin。修复后，TimePicker.PeriodHostWidth和ButtonsMargin实际生效，DatePicker字段不再改变时间footer。DatePicker自己的日期footer继续消费原字段。TimeView还复用于DatePicker／RangeDatePicker带时间弹层，这些时间子区域共享TimePicker角色的Own几何；各公开宿主及公开TimeViewCell的Global身份不因此合并。

## 公开 TimeViewCell 的 Global 消费

Cell没有独立Own字段，字体族／字号在明确配置后取本身份有效Global，缺省保持真实祖先字体，不引入字号预设或自定义字号标志。圆角取本身份BorderRadiusSM。时分秒行几何仍为TimePicker Own角色，不因字体配置而改变循环行高或列宽。

普通／hover文字使用ColorTextSecondary，普通背景ColorTransparent、hover背景ColorBgTextHover；selected文字ColorText及背景ControlItemBgActive。只有Cell直接配置相应Global字段时切换来源，否则保留原ListView角色的有效配色，包含已有ListView Own／control Global和根Global组合。ColorTextDisabled同样须Cell直接明确配置，未配置保留原禁用外观；不能因原Name selector无效就默认改灰。清除恢复旧角色，透明色也视为明确值。

以上最终Setter均在ControlTheme，禁用Foreground通过owner原模板绑定传到真实ContentPresenter。未配置时disabled Setter不激活，避免改变原应用Style优先级。实例本地值及生成的TimePickerPopupItemStyle等专用Style仍按原生优先级覆盖；Semantic Part定制使用AXAML专用Style，不通过代码查找行修改样式。内部TimeView的其他Global及完整日期家族状态仍单独验收。

ButtonsMargin作用在footer内部布局Panel，不等同于生成的popup.footer Style对外层Border设置Margin。应用应按实际区域配置，语义样式仍只用生成专用Style在AXAML声明；不找节点赋样式值。0间距是明确输入，清除回到默认8。

RangePickerArrowMargin与RangePickerIndicatorThickness是RangeTimePicker的可选Own覆盖：明确配置TimePicker字段时优先使用该值，缺省或清除回到原InfoPickerInput的有效Own值，包含已有的控件Global配置。两组默认仍为水平margin8、指示条厚度2，不新增RangeTimePicker Own身份。单值TimePicker和RangeDatePicker不消费此覆盖。0是明确输入；margin保留原Thickness允许的负值语义，不统一禁止。

活动指示条宽度和水平位置跟随实际活动输入框，底边对齐实际框体底边；margin或厚度热更新不能产生旧宽度、旧坐标或底边溢出。几何变化不会提交范围时间，Start/End切换仍使用正常过渡。重套模板的旧部件保留属于另列的生命周期问题，尚未宣称验收通过。

## 4. 控件家族影响

调整 TimePicker Token 时必须评估以下范围：

- `TimePicker`
- `TimePickerPresenter`
- 对应 Gallery ShowCase 的示例和源码片段。
- Light/Dark 主题、Browser/Desktop 主题和 Compact/Form/Popup 集成场景。

## 5. 兼容性要求

- 不删除或重命名已生成的 TokenKind、TokenResource key 和 AXAML 引用。
- 不把实例状态、交互状态或 `EffectiveXxx` 状态写成 Token。
- 不在 Token 中展开颜色、variant 和状态的组合矩阵；组合关系应由 Theme selector 表达。
- Token 默认值变更必须同步评估 Gallery 示例和截图可观察外观。
- 如需引入新 Token，必须同步 Token 类型、生成资源、主题引用和本文档。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| Token 文档 | `git diff --check`，检查相对链接存在。 |
| Token 默认值 | 运行对应控件测试，走查 Light/Dark 和 Browser 主题。 |
| Token 名称或数量 | 检查 generated TokenResource key、AXAML 引用和 token.md。 |
| 主题映射 | 走查 hover、pressed、selected、disabled、loading 等状态视觉。 |

## 根几何与 Effective Global 消费

公开 owner 的 `Padding`、`BorderThickness`、`CornerRadius` 默认值从该 Control 的 Effective Global 读取，再以原生主题优先级投射到实际框体。共享框体继续负责状态色、focus/hover shadow、variant 与 CompactSpace；不能把它描述为跳过 owner 的全局几何入口。字号和其他消费链按各控件当前模板分别定义。本次未增加 Own 字段或改变 Token identity。详见 [共享输入架构的根几何合同](../input-control-architecture-design.md#81-根表面几何与预设默认值)。
