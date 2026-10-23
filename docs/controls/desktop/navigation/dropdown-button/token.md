# DropdownButton Token

DropdownButton以`AtomUI:DropdownButton`独立求值，23个Own字段由DropdownButtonToken定义；同名字段不读取Button的显式Own配置。完整Global覆盖与[Own默认求值合同](../../../../architecture/systems/theming/own-token-evaluation.md)共用现有运行时。

字体、DIP布局行高、内边距构成三组独立尺寸依赖。布局行高默认由对应有效ContentFontSize计算，ContentPadding及IconOnyPadding读取对应行高；ExtraContentMargin、CirclePadding读取有效Padding。显式上游进入下游默认函数，显式下游独立保留，0与缺省分开。

OnlyIconSize、IconMargin/IconEndMargin、MarginToAnchor及禁用边框负责各自稳定语义。历史IconOny拼写保留，不借迁移重命名。缺省公式保持Small横向8-LineWidth，其余横向PaddingContentHorizontal-LineWidth，纵向max((ControlHeight-布局行高)/2-LineWidth,0)。

Own定义使用DependencyDefaultsV2，静态函数显式声明Own输入，无运行期反射或整体evaluator。状态和尺寸分支是否实际消费字段由[实现文档](implementation.md)及[主题验收规范](../../../../engineering/development/theming/verification.md)约束；不能用默认数值正确替代全部消费验证。

## 独立颜色与阴影

DropdownButton继承Button的变体渲染算法，但有效Token属于DropdownButton identity。除原23个排版/边距/禁用描边字段外，增加23个既有渲染语义的Own字段，终端共46字段；均使用V2静态默认。

| 分组 | Own字段 |
| --- | --- |
| Default | DefaultBg、DefaultColor、DefaultBorderColor、DefaultHoverBg/Color/BorderColor、DefaultActiveBg/Color/BorderColor、DefaultShadow |
| Primary / Danger | PrimaryColor、PrimaryShadow、DangerColor、DangerShadow |
| Text / Link | TextTextColor、TextTextHoverColor、TextTextActiveColor、TextHoverBg、LinkHoverBg |
| Ghost / Solid | DefaultGhostColor、DefaultGhostBorderColor、GhostBg、SolidTextColor |

字段默认公式与对应Button语义一致，输入为**Dropdown有效Global**，例如DefaultBg=ColorBgContainer、PrimaryColor=ColorTextLightSolid，阴影取ControlOutlineWidth和对应语义outline色。渲染reader使用有限静态映射取得Dropdown Own，不借用Button的配置表。

兼容修正：此前Button.DefaultBg等显式值会意外渗透Dropdown；现在两identity独立。应用若确实需要相同效果，应分别配置两个Control；既有字段、类型与未配置默认视觉保持。Global高度、圆角、focus、图标、loading和disabled输入同样从Dropdown身份解析。

## FontFamily 的精确与继承合同

本控件的类型化Global FontFamily在正规化配置直接包含该字段时进入根渲染；未配置则保留真实InheritanceParent的字体，清除恢复当前祖先。Shared字体配置同样生效，显式配置与缺省不按值相等猜测。FontFamily Setter由ControlTheme MultiBinding提供，控件只暴露内部只读继承源；原生Style/local及资源覆盖保持。接口观察在分离时释放父订阅，重挂刷新，不以逻辑父代替受保护的真实继承父。详细机制见[显式Token资源](../../../../modules/core/configured-token-resources.md)。
