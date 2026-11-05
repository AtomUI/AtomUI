# CT13 NU3：Numeric inline 分隔边几何

现有NumericUpDownSpinnerTheme的减号段右边和加号段左边写死1。原模板已经将Numeric公开BorderThickness传到内部spinner与frame；本包以同一父几何为输入，分别过滤Right/Left，默认1保持。沿用BorderThicknessFilterConverter，只加两声明式资源与TemplateBinding，不新增Own字段、converter类、C#绑定或状态旗标。

## UX与所有权

分隔线仍在按钮本体之外占位，按钮宽度继续跟随实际高度维持正方形。Numeric根Width/Height/Min/Max、padding/Content、文本/数据、两模式、可见性与输入流程保持。全Thickness非均匀输入按对应边解释：减号右分隔取Right，加号左分隔取Left；Top/Bottom不进入这两条竖边。此为已存在父几何的可见投射，不重定义NU2-B的内部手柄取Left算法，也不将LineWidth-only配置误作别名重推。

- [x] R：全局/本地显式父厚度3与(3,5,7,9)、0/清除，两真实外层PixelAlignedBorder；独立ButtonSpinner/child IconButton干扰输入不抢owner，方形按钮不含分隔占位。
- [x] B：原明暗/四尺寸/固定宽与自定义高，原内容、实际点按/键盘/隐藏步进和Min/Max保持；默认/配置/清除全轨迹AA/1DIP，记录根/按钮/分隔几何，不通过去掉约束消差。
- [x] I：两边Thickness用TemplateBinding BorderThickness与现有Left/Right过滤converter，四边只保留该边，默认1；不改事件或Root模板层级。
- [x] V：前反例及真实布局/交互GREEN，managed/NativeAOT/NET8，像素/几何具体范围独立审查，未获批差异不填通过。
- [x] C：临时源/工程/媒体清理后精确Numeric模块、正式文档/记录与作者另步自审，无Git；完整家族及CT25继续。

Local validation/cleanup done; exact NU3 visual scope approved call_8c6c10577cc94e12b7d37d281865fcfc:0. See [results](input-numeric-divider-results.md). No Git operations.
