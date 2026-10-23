# HyperLinkButton 桌面版设计

HyperLinkButton属于AtomUI.Desktop.Controls，提供链接外观的按钮动作。它继承Common的AbstractHyperLinkButton及Avalonia Button命令/Click语义；与普通Button保持独立主题identity，不继承另一控件的显式Own配置。

NavigateUri非空时，Click后经当前TopLevel.Launcher异步打开；仅成功才设置IsVisited。无TopLevel时不启动外部动作。IsLoading、IsEnabled、IsDanger、IsGhost及尺寸共同约束可见图标、文字和状态样式，导航行为不进入Token。

Large/Middle/Small与Custom保留各自主题职责；IconWidth/IconHeight属于实例图标入口。根布局、排版和颜色通过原生属性与ControlTheme定制。目前没有独立公开Semantic Part描述，内部PART名称不构成公共专用Style合同。

维护入口：[实现](implementation.md)、[Token](token.md)、[变化记录](changelog.md)。按钮家族共同动作边界见[Button](../button/overview.md)。
