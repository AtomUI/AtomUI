# IconButton 与 ToggleIconButton

IconButton提供图标按钮行为；ToggleIconButton提供checked与unchecked图标切换。两者为独立公开控件，分别以AtomUI:IconButton、AtomUI:ToggleIconButton注册Global主题身份，不继承Button Own。

IconButton的Icon、IconBrush、IconWidth/Height、LoadingAnimation/Duration及IsMotionEnabled由Common抽象基类声明；ToggleIconButton提供CheckedIcon、UnCheckedIcon、IsChecked、IconBrush、四种状态画刷及IsMotionEnabled。最终值遵循原生属性和Style优先级；主题更新保留实例、checked和输入行为。

主题输入见[Token](token.md)，源码职责和绑定见[实现](implementation.md)，变更见[Changelog](changelog.md)。本次不增加Semantic Part，也不把私有模板部件作为公共定制合同。
