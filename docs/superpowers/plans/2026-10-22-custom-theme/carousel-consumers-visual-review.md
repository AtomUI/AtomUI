# CT15 Carousel 限定视觉审查（2026-10-31）

状态：工程、临时清理与清理后3项/NET8验收已完成，表内44对已获用户限定批准。仅表内44对有意旧新静态变化，不关闭完整Carousel/CT15、不授权提交。

900×600、原轮播360×160、三原页/标题/背景，四算法正常motion，同完整历史。静态750ms+12等RGBA帧、6s上限；同坐标完整比较，无mask/crop/align/通用容差。每行4对，数值为变化像素/max通道差。

| 明确变化 | Light | Dark | Compact | DarkCompact |
| --- | --- | --- | --- | --- |
| 七Own改值与ArrowOffset37进入目标 | 456/max40 | 456/max21 | 456/max40 | 456/max21 |
| Global Menlo23及组合颜色#FFA940 | 2455/max201 | 2455/max235 | 2455/max201 | 2455/max235 |
| Native Courier17优先且保留组合Global色 | 250/max191 | 250/max235 | 250/max191 | 250/max235 |
| 清Native回Global | 2455/max201 | 2455/max235 | 2455/max201 | 2455/max235 |
| Left方向热更新实际箭头边距 | 168/max40 | 168/max21 | 168/max40 | 168/max21 |
| Right方向热更新实际箭头边距 | 168/max40 | 168/max21 | 168/max40 | 168/max21 |
| 分页offset29同时正确通知两按钮 | 164/max40 | 164/max21 | 164/max40 | 164/max21 |
| 换模板后原页内容保留，旧版为空 | 57449/max201 | 57535/max235 | 57449/max201 | 57535/max235 |
| 同实例重挂保留原页与分页 | 57449/max201 | 57535/max235 | 57449/max201 | 57535/max235 |
| 完整同实例业务往返恢复 | 57449/max201 | 57535/max235 | 57449/max201 | 57535/max235 |
| 退出自动播放保留内容并归零有效进度 | 57449/max201 | 57535/max235 | 57449/max201 | 57535/max235 |

## 配方和合同

Own：IndicatorWidth21、Height7、ActiveWidth39、Gap13、PaginationOffset19、ArrowSize31、ArrowOffset37；zero全部0，clear回原值。ArrowOffset只在显式Own生效，缺值仍原PaginationOffset（默认12），没有强施旧未消费SpacingXS默认。zero与四默认旧新均0。28四算法默认/type/key/slot保持；四方向/Native/clear验证按实际目标。

Global：Carousel Menlo23、ColorBgContainer#FFA940；Native：Courier17。三页应用背景与文本颜色保持原内容。布局热通知修正与虚拟化ownership修复会让原有相同输入出现表内变化；换模板/重挂必须保留caller-owned Content，不通过新建页/关闭虚拟化或放宽视口避开。七Own实际目标、两个公开叶身份及退出验证另见结果。

没有公开Semantic Part，不新增或虚构Part；内部PART协作与Native属性合同保留。原smoke空白57449/max201、复查geometry批均保留前证并在文本记录后清理。

## 不批准的边界

- 旧新128对68零/60非零；本表44静态对有意，其余16持续态不请求批准。
- JIT/AOT128对124零/4非零；全部112静态对精确0。四动态差异：light auto0 70/max2、auto200 6/max141；dark auto0 68/max1；compact auto0 69/max2。不以通用phase/平台容差豁免。
- managed/AOT各48检查通过，旧版48中24FAIL（Own/热通知/空集/内容恢复/进度退出）；这不是把旧版失败改为期待。
- 候选/AOT各44恢复均0；旧版44中16为模板后空白。三版独立A/A/恢复四组均0，1DIP各1182/max201检出。候选/AOT12组持续态间隔均非零，旧版因后半空白12组仅6非零。
- 真实OS/Browser、全部swipe/键盘/自然极值与完整家族出口仍未完成；本次真实Headless鼠标next已命中且结果正确，不能冒称OS输入。CT12/13/14与CT25/FW1原边界不变。

见[结果](carousel-consumers-results.md)与[作者自审](carousel-consumers-review.md)。


限定批准登记：2026-10-31，用户明确回复“批准表内44对，其余未通过与完整CT15保留”（request_user_input_async / call_aa6a1cc8835941ae997eada0a60a4e92 / item 0）。仅Carousel表内44对静态变化获批准；其余16对旧新持续态、4对跨运行时相、平台/输入/极值及完整Carousel/CT15继续保留，不授权提交。
