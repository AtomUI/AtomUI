# ArrowIndicator 实现原理

实现位于`src/AtomUI.Controls/Primitives/ArrowDecoratedBox/ArrowIndicator.cs`，路径计算复用`AtomUI.Core/Media/CommonShapeBuilder.BuildArrow(size,radius)`。公开API和组合职责见[设计](overview.md)。

路径在首次ApplyTemplate时创建，使用ArrowSize/TipRadius生成局部坐标和原有平移矩阵；Measure读取其bounds，Render读取相同几何。已构建几何依赖尺寸或尖端半径变化时重建，同时使Measure与Render失效。重复ApplyTemplate且输入不变复用几何；模板前改变输入由首次构建读取最终值。绘制前不创建第二套主题状态，也不订阅全局主题事件。

Render使用FilledColor及缓存pen；pen的brush/线宽由StrokeBrush/StrokeThickness决定，绘制时通过PenUtils更新。描边不改写现行自然尺寸约定，方向变换/定位由组合模板和ArrowDecoratedBox布局处理。缓存均为实例持有的几何/pen，没有资源绑定或跨owner事件，随实例释放。

父owner提供的Token和模板绑定保持原有分工。TipRadius作为参数化primitive的原生形态输入，由实际owner的专用Semantic Part Style声明；不为它生成假的独立Own schema，控件代码不读取Application或复制Token字典。AXAML可直接设置该属性，不需要代码查找部件后赋值。

维护时同时核对自然尺寸、实际路径/颜色/pen、同实例A→B→A及真实组合部件。默认值的同输入前后像素保持；改变尺寸/半径应真实改变几何，恢复后回到原形状。平台、AOT与最终显示面证据按实际执行范围报告。
