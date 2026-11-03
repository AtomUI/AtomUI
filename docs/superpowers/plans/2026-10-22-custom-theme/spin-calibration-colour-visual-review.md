# CT15 Spin 独立校准前景限定审查

状态：工程、实际绘制、临时清理和清理后Rendering 77项验收完成；SC1–SC8已获用户限定批准，不先记完整CT15完成。

## 具体变化

原主题Tip直接读取Shared ColorPrimary；当前Tip读取实际Spin owner Global。独立校准显式选择ControlAlgorithmMode.Global，在Dark和Dark+Compact下，Shared为`#FF1668DC`，实际owner Global为`#FF165BBE`。本次只请求这八个状态的前景归位；字号12、Alibaba Sans、完整文案、几何、正常IsSpinning及18×18调用方自定义内容保持。

| 条目 | 配方 | 原四操作 | 本次Shared角色→owner Global角色 | 最大通道差 | bbox（原位；shift向右1DIP） |
| --- | --- | --- | ---: | ---: | --- |
| SC1–SC4 | Dark | cal-a、cal-aa、cal-shift、cal-restore | 各1030像素 | 42 | [126,169,295,181]；shift [127,169,296,181] |
| SC5–SC8 | Dark+Compact | cal-a、cal-aa、cal-shift、cal-restore | 各1030像素 | 42 | [126,166,295,178]；shift [127,166,296,178] |

800×600原窗口、420×240实际Spin、原完整`Independent calibration original`、背景`#141414`保持。Tip Bounds分别为`0,26,169,15`与`0,21,169,15`，两个角色下逐字相同。已查看两角色当前完整图，暗色低对比属于这次具体审查内容，不宣称所有极值可读。

## 证据与限制

采用后的完整原Spin驱动172帧/124性质通过、四独立cal恢复RGBA0，见[采用权威结果](feedback-region-sdk-adoption-results.md)。本次仅用原独立cal真实控件做单变量A/B：一侧通过生成的SpinDescriptionStyle声明原SharedTokenResource；另一侧沿现有owner Global模板。没有获取部件后赋外观、没有新图元模型、生产源码没有变化。两临时参数用例通过，四个角色自身a→restore均RGBA0。

Rendering宿主继承字号14，与原standalone Application的12不同，首批两项前置失败在截图前被检出；调用方scope保留原12后执行。本A/B不是整版旧源码重放。原pre-hot-style八条1002/max42、最终hot-style八条仅有总数、旧媒体已清理等限制继续保留；不能把本次1030写回旧批数值，也不声称整个历史像素集合已逐点重建。本次不新增AOT发布，复用未变消费者及共享渲染实现的已有AOT范围。

只审查SC1–SC8的明确前景角色变化；原46主静态、308循环、P3-C96等批准不扩大。旧偶发原生输入、未配对clock、GPU/其他平台、原失败/缺帧和完整CT15仍按各自有限边界处理；不增容差、不改FW1、不授权提交。

临时两用例通过14s；3源5876字节已删除，16截图/两份值记录及清单共19项经官方audit/cleanup清理，findings=[]。清理后五个具名Rendering类77PASS/0FAIL/0Skip、44s（命令58.08s），既有xUnit1051保留。生产源未变，483/7及NET8证据按相同源码复用，无新AOT或全量。

## 限定批准登记

2026-11-03，用户回复“同意 批准”（request_user_input_async / call_0c599be87c994f54ad3f7fd27654bf94 / item 0）。仅批准SC1–SC8的明确Global前景角色归位，1030与原1002旧批、非整版旧源码重放的限制保持；其他边界、完整CT15及不提交约束不变。
