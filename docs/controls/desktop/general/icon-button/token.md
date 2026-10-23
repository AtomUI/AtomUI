# 图标按钮主题合同

IconButton和ToggleIconButton没有独立Own终端；Global属于各自identity。不能通过Button配置间接定制它们。

| 输入 | 消费 |
| --- | --- |
| IconSizeSM | 默认IconWidth/Height |
| BorderRadiusSM | 根圆角 |
| EnableMotion / MotionDurationMid | 动效开关及同实例动态Transition时长 |
| ColorIcon / ColorIconHover / ColorText / ColorTextDisabled | 普通/hover/pressed/disabled图标色 |

ToggleIconButton保留四个原生画刷入口：NormalIconBrush默认ColorIcon；SelectedIconBrush缺省跟随Normal；ActiveIconBrush默认ColorText（pressed）；DisabledIconBrush默认ColorTextDisabled。最终优先级为disabled、pressed、hover、checked、normal；hover仍使用ColorIconHover。IconBrush的原生本地值仍按Avalonia优先级覆盖主题状态Setter。

配置画刷只改变状态的实际颜色，不重建控件或修改IsChecked。字段是原生属性，不另造同名Own、状态Token或第二套优先级引擎。默认checked仍同普通色，必须保留原始视觉。
