# CT14 N3-HM：HorizontalMargin 兼容分类

完整搜索当前产品C#、AXAML、Gallery和测试，`TabControlToken.HorizontalMargin`只有声明、V2默认及资源导出，没有内置消费者。四种Tab的模板没有该资源引用；实际页签与内容间距由公开`TabAndContentGutter`按placement投射到内部`TabStripMargin`，两者类型、默认和方向职责不同，不建立别名。

本片沿既有CT14 N3分类规则将该字段从Undecided归为Legacy/CompatibilityRetained：保留名称、Thickness、key/ID、slot、V2默认和配置往返。源码只改XML注释，排除XML注释后C#文本逐字相同；未改运行语句、默认值、模板、布局或优先级。

正式Token/overview/changelog及一条字段目录同步。CT23目录应展示历史保留，不将该字段作为有效内置视觉编辑入口。该裁定不删除应用自行读取该资源的能力。

不新增镜像实现或文案的测试；既有TabControl精确模块283/283、0Skip通过，单Token完整成员审计1文件/0问题，`git diff --check`通过。没有新视觉行为、媒体或专项驱动，因此不新增截图或NativeAOT矩阵。

该未决字段已经处置；直接Styles.Remove、NavMenu、其他消费者和完整家族出口仍开放。本片未提交，前批Git授权已消耗。
