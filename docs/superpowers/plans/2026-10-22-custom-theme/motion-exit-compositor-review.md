# CT-11：限定合成帧历史差异审查

本表只请求接受下列实际观测的有限帧差异，继续由CT25保留合成器专项，不请求通用像素容差，也不将产品增加强制重绘作为修复。

用户已明确接受（`call_a4f8b96266604d9caab3c5af6a17a00b:0`：“接受本表8对，保留CT25专项”）。仅覆盖本表8对，原始FAIL与CT25义务保留。

## 原始完整轨迹

基线`4560091de`与候选相同数据、窗口、输入和完整操作历史。每种主题522对，RGBA全通道比较：Light520对0、2对非零；Dark505对0、17对非零，其中12对暗色传播修复已独立批准。其余7对如下。

| ID | 控件/状态 | 像素数 | 最大通道差 | 坐标范围 |
| --- | --- | --- | --- | --- |
| H1 | Light TextBox hover | 1364 | 153 | `(0,40)-(640,72)` |
| H2 | Dark TextBox hover | 1364 | 165 | 同上 |
| R1 | Light RangeDatePicker ready | 6 | 4 | `(3,70)-(6,72)` |
| C1–C4 | Dark ColorPickerView ready/hover/timing-B/timing-A | 每对12 | 14 | `(240,218)-(460,243)` |

独立自然合成重放另有R2：Dark RangeDatePicker ready **1像素、最大22**，位置`(2,70)`；本次请求包含这1对，合计只限8对。

## 单变量定位

- 同一旧源码、完整场景→独立场景：TextBox hover自身变化1364像素；候选同对0。RangeDatePicker ready旧版自身5、候选自身7。不能将完整轨迹差异全部归为候选代码或覆盖原计数。
- 相同两根独立轨迹的新旧8对均0，实际布局及命中一致：TextBox root/frame640×32，root及frame pointer-over为true，center hit为ScrollContentPresenter；RangeDatePicker248×32、内部frame88×22，hit为AddOnDecoratedBoxContentFrame。
- Dark三根补查：即时RenderTargetBitmap12对中ColorPicker与Range均0，TextBox hover仍有一次1364；自然合成12对TextBox与ColorPicker均0，Range出现R2。两种后端分别记录，未混用基线。
- 指针先退出再进入，保留原输入与所有内容：16对只有TextBox hover1364；旧新root/frame pointer-over及IsInnerBoxHover均true。
- **仅增加临时宿主整窗InvalidateVisual**，其他状态和历史保持，16对全部0。此改动仅用于定位合成损伤/重绘历史，未进入产品；不以此替换原始不重绘轨迹的结果。
- 两侧独立校准一致：AA0、1DIP1009、恢复0。没有缩放、裁剪、遮罩、挑帧或相对容差。

原始差异仍为FAIL证据，限定接受不将其改写为零差异；CT25保留其原生/跨平台最终像素和合成器根因义务。批准不覆盖其他帧、其他控制模式、Native像素/输入、未来漂移或Git。审查后的登记媒体已清理，文本数值和定位结论保留在本表及总结果中。
