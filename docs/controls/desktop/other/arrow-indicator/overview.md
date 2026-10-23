# ArrowIndicator 设计

`AtomUI.Controls.ArrowIndicator`是共享自绘箭头primitive，作为Tooltip、Popover及PopupConfirm等弹层的箭头部件使用，不负责打开状态、定位、shadow、输入或内容。它没有独立Token identity/注册片段，设计值由组合owner的Token、模板绑定和专用Semantic Part Style提供。

`ArrowSize`控制路径尺寸（DIP），由组合模板提供有效值；自身元数据为0，不提供独立产品尺寸预设。`TipRadius`控制尖端圆弧半径，默认1.5 DIP，0表示尖角，仅接受非负有限值。半径参与真实路径及自然尺寸计算，不声明任意半径下固定高度或固定宽高比。默认半径保持原有形状。

填充由`FilledColor`控制，描边由`StrokeBrush/StrokeThickness`控制。上述输入遵循Avalonia原生StyledProperty优先级，不在布局/绘制中改写调用方值。尺寸和半径更新既有实例的几何、测量及绘制，颜色和描边更新绘制。

组合控件的语义箭头通过其生成专用Style定制，例如[Tooltip arrow](../../data-display/tooltip/semantic-part.md)中的`ToolTipArrowStyle`。owner的打开/关闭、箭头可见性和位置合同仍由对应控件拥有；设置半径不承担弹层状态或输入职责。

实现、坐标与缓存见[实现原理](implementation.md)，变化见[记录](changelog.md)。
