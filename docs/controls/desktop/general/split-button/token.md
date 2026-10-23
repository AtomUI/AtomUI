# SplitButton Token

SplitButton以`AtomUI:SplitButton`拥有独立Global及一个Own：GutterToFlyout，单位DIP，默认取自身Effective Global的UniformlyMarginXXS。DependencyDefaultsV2静态默认函数直接声明该输入；显式值保持，缺省重新派生，不保存实例打开状态。

组合的公开Button子控件保留自身identity，父控件负责声明的尺寸、边框拼接与打开状态投射。SplitButton的Global/Own不自动复制为Button配置，Button配置对实际Button子节点的作用也不等同于SplitButton Own。

整体设计及组合责任见[概览](overview.md)、[实现](implementation.md)，Part入口见[Semantic Part](semantic-part.md)。更改间距、对子节点的投射或尺寸入口必须同时检查打开菜单、分隔线、明暗和Custom，保留原生焦点与输入。

组合外框、描边、圆角、分隔色及motion/wave开关由SplitButton自身Global消费；实际Button子控件的独立Token不被父Own复制。Shared资源不能替代这些父级输入。

## FontFamily 的精确与继承合同

本控件的类型化Global FontFamily在正规化配置直接包含该字段时进入根渲染；未配置则保留真实InheritanceParent的字体，清除恢复当前祖先。Shared字体配置同样生效，显式配置与缺省不按值相等猜测。FontFamily Setter由ControlTheme MultiBinding提供，控件只暴露内部只读继承源；原生Style/local及资源覆盖保持。接口观察在分离时释放父订阅，重挂刷新，不以逻辑父代替受保护的真实继承父。详细机制见[显式Token资源](../../../../modules/core/configured-token-resources.md)。

公开Button子控件保持独立配置；未显式配置子字体时继承组合根，配置Button字体时按子身份消费。
