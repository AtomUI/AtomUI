# CT13 CB4：ComboBox 显示区文字颜色

承接已授权CT13，使用主agent/当前目录，不创建分支、worktree或提交。实现仍遵循既有Effective Global、原生优先级及AXAML入口；新视觉差异按本包具体范围另审，不沿用CB2/CB3批准。

源码假设：普通占位文字继承根Foreground并固定Opacity=.3，没有消费公开PlaceholderForeground；可编辑占位文字已经模板绑定该属性，但根主题没有相应默认。显示区禁用态没有根ColorTextDisabled入口，但真实G2共享frame本来已消费禁用色；根getter保持原合同，不增设禁用Setter。有效缺陷是Filled/Borderless内部错误/警告文字规则在禁用时仍激活。弹层disabled项已由CB2完成，本包不改。

- [x] CB4-R：临时真实控件反例，分别核查普通/可编辑模式的有效Global占位色、公开PlaceholderForeground覆盖、禁用已选文字及根/文本最终值；已有正常ColorText与局部Foreground作正对照。先记录真实失败，不按源码假设直接修复。
- [x] CB4-B：原代码真实Skia/Light与Dark、空/选择/禁用/恢复、作用域配置/清除及实际可编辑文字，固定背景/尺寸/焦点/轨迹；AA、独立1DIP及复位。公开属性保持正常优先级；本包不改变输入/弹层尺寸、内容或选择实现。
- [x] CB4-I：仅在RED确认后将缺失的占位默认与错误/警告状态范围放入ControlTheme，普通占位通过模板绑定公开PlaceholderForeground，不用代码找节点写属性；保留其他状态/局部值优先级。任何额外发现单独记录，不静默扩大。
- [x] CB4-V：原反例与扩展状态/清除/公开覆盖GREEN；真实编译专用Placeholder Style命中与优先级、Mac managed/AOT与NET8，具体新旧像素及默认变化单独审查。
- [x] CB4-C：批准后清理临时源/工程/媒体，清理后精确ComboBox/Gallery相关验证；更新正式文档与记录，作者另步自审，完整家族及CT25保持开放。

实现与本机专项/清理/最终模块已执行，见[候选结果](input-combo-box-display-colors-results.md)。[新视觉范围](input-combo-box-display-colors-visual-review.md)已单独批准；临时清理后69/8项通过，完整家族保持开放。
