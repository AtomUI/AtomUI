# HyperLinkButton Token

独立identity为`AtomUI:HyperLinkButton`，17个Own字段分为尺寸字体/布局行高、普通与icon-only内边距、图标尺寸/留白和禁用边框。字段不保存loading、visited、导航或焦点状态。

三档ContentFontSize默认分别取自身Effective Global的FontSize/FontSizeLG；对应ContentLineHeight单位DIP。静态默认函数显式依赖有效字体；ContentPadding及IconOnyPadding依赖对应布局行高。显式下游独立保留，缺省使用存在位判断，不使用NaN哨兵。行高是默认尺寸计算指标，不能据此推断所有内容段落属性都由该字段控制。

缺省Padding保持Small横向8-LineWidth，其余横向PaddingContentHorizontal-LineWidth，纵向max((ControlHeight-布局行高)/2-LineWidth,0)。OnlyIconSize和IconMargin各自负责图标布局；IconOny历史拼写不改名。

采用DependencyDefaultsV2及现有Compiler/scope/cache，不创建第二套默认或资源系统。完整状态消费和视觉验证遵守[主题验收规范](../../../../engineering/development/theming/verification.md)，原生字体/实例属性继续由Avalonia处理优先级。

## FontFamily 的精确与继承合同

本控件的类型化Global FontFamily在正规化配置直接包含该字段时进入根渲染；未配置则保留真实InheritanceParent的字体，清除恢复当前祖先。Shared字体配置同样生效，显式配置与缺省不按值相等猜测。FontFamily Setter由ControlTheme MultiBinding提供，控件只暴露内部只读继承源；原生Style/local及资源覆盖保持。接口观察在分离时释放父订阅，重挂刷新，不以逻辑父代替受保护的真实继承父。详细机制见[显式Token资源](../../../../modules/core/configured-token-resources.md)。
