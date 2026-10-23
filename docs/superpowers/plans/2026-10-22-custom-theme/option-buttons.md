# CT-12 OptionButton 分包

延续[按钮计划](buttons.md)，单agent、现有工作树，不提交。首四终端91字段及字重/shape包已验收，不代表本包19字段完成。

- [x] 7项实际RED：Middle/Large显式字体20未进入行高比例；零比例未进入Padding；Small错误读取Large高度平方；ButtonPadding未消费；普通及Outline checked背景未消费。
- [x] 建立16组明暗基线，覆盖两方向、两外观、四尺寸、checked/disabled、非白父背景；真实指针左中右命中、Click/键盘及恢复；19个资源值独立记录。
- [x] V2每字段静态默认，保留比例单位。LG读取LG字体/行高/高度，Small读取SM；不得把比例当DIP。修正LG默认Padding0→7只有在默认像素无差异后才能通过，数值变化单独记录。
- [x] ButtonPadding定义为基础内容留白：普通/Large水平扣描边，Small水平基础值减半再扣描边；纵向基础值加行框居中余量，各边非负。默认16,0保持；显式8,2的Middle结果7,6，有效上游进入ContentPadding，显式下游保留。
- [x] 两背景字段保留既有ColorBgContainer默认读值，采用可选显式渲染，缺省透明保持非白父背景及命中；Outline checked显式背景覆盖普通背景，未配置时保留普通背景。Solid checked及disabled的既有语义色优先，原生属性/Style不被代码重写。
- [x] 两个Transition集合接入现有owner动态时长，相关Shared输入按真实OptionButton/Group identity分别检查；不把Group Global直接借给公开子Control。
- [x] 逐字段消费、scope往返、选择/焦点保留及释放；默认零未解释像素差异。当前平台AOT、NET8、临时清理后focused、正式Radio/Option文档及总表同步。原生闪动仍独立验收。

出口实际数值见[结果](option-buttons-results.md)。临时清理后focused30通过，NET8 Release零警告零错误18.99秒；未把原有边缘栅格现象或完整家族/平台标为通过。
