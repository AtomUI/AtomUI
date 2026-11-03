# CT15 Statistic 限定视觉审查（2026-11-01）

状态：工程、正常Skia旧新/AOT、临时清理及清理后5项精确模块/NET8零警告完成；仅以下12对已获限定视觉批准。

输入4metadata/2Own，四身份Own数量0/2/0/0，沿原共享资源职责，不缩137/274。以下只列12对明确静态变化；四default旧新0。

| 算法 | 状态 | 非零像素 | max通道差 | bbox | 明确原因 |
| --- | --- | ---: | ---: | --- | --- | --- |
| L | global | 14863 | 255 | [100, 260, 583, 461] | 实际Statistic/Timer/CountUp Global字体、原title/heading颜色与spacing生效 |
| L | native-global | 15231 | 255 | [100, 260, 582, 459] | 原生Courier/29/Orange继续优先，剩余标题颜色/spacing随Global生效 |
| L | reattach | 280 | 224 | [193, 369, 220, 388] | 同一已结束倒计时重挂后保持00:00，旧版错误转正计时 |
| D | global | 14861 | 235 | [100, 260, 583, 461] | 实际Statistic/Timer/CountUp Global字体、原title/heading颜色与spacing生效 |
| D | native-global | 15224 | 235 | [100, 260, 582, 459] | 原生Courier/29/Orange继续优先，剩余标题颜色/spacing随Global生效 |
| D | reattach | 279 | 199 | [193, 369, 220, 388] | 同一已结束倒计时重挂后保持00:00，旧版错误转正计时 |
| C | global | 14863 | 255 | [100, 260, 583, 461] | 实际Statistic/Timer/CountUp Global字体、原title/heading颜色与spacing生效 |
| C | native-global | 15231 | 255 | [100, 260, 582, 459] | 原生Courier/29/Orange继续优先，剩余标题颜色/spacing随Global生效 |
| C | reattach | 280 | 224 | [193, 369, 220, 388] | 同一已结束倒计时重挂后保持00:00，旧版错误转正计时 |
| DC | global | 14861 | 235 | [100, 260, 583, 461] | 实际Statistic/Timer/CountUp Global字体、原title/heading颜色与spacing生效 |
| DC | native-global | 15224 | 235 | [100, 260, 582, 459] | 原生Courier/29/Orange继续优先，剩余标题颜色/spacing随Global生效 |
| DC | reattach | 279 | 199 | [193, 369, 220, 388] | 同一已结束倒计时重挂后保持00:00，旧版错误转正计时 |

原窗口720×680、控件宽520、完整Original statistic title with units、USD /数值/ units以及Time /00:00/ left保持。Global为Menlo23、title #123456、heading Red、spacing19/27；原生样本Courier/29/Orange。暗色显式深蓝标题低对比已查看并列明，不改成其他颜色掩盖真实覆盖。97字号在原520宽/窗口边界裁剪、1字号标题难辨，旧新一致，不属于新的默认修正；输入原220宽viewport文字按原裁剪/滚动，完整Text/caret另有断言。

34所选PASS（29临时+5既有）与独立CountUp局部MotionDurationVerySlow一秒/clear 1PASS，合计35个不同验证用例，临时总30；不把34+1冒称一次整批35。首轮7FAIL、增强focus/Global字号3FAIL、结束计时2FAIL均先RED后修。原Own字号0在旧新都抛官方Invalid FontRenderingEmSize，保留正字号边界，不夹值或替换官方依赖。

正常Skia旧新各212帧/140检查，候选全PASS；旧版44FAIL完整保留，无缺帧。128静态116零/12有意变化，其余84持续态72非零不并入本表。候选52主恢复全0、A/A与restore各4全0；旧版主恢复仅最终reattach四对失败。独立1DIP四组1425/1427/1425/1427像素可检出，最大220/198/220/198。完整同坐标RGBA比较，无裁剪、掩码、对齐、容差；contact sheet仅辅助查看。

真实产品TextBox的重模板focus/键盘/caret/原生值、后来用户焦点优先，以及timer退休/重建/方向/事件通过；Headless不能替代OS输入。计数/计时/caret/Skeleton持续态保留正常motion，完整周期相位、平台/Browser、全部自然极值、GC及完整家族/CT15仍开放。CT12/13/14既定批准、CT25/FW1与官方依赖保持，无提交授权。


AOT确切二进制四算法212帧/140检查PASS，128静态cross全0、52主恢复0及独立A/A/restore各4零；46持续态跨运行非零（32实时计时/计数/加载、14输入/caret）保留，不纳入12对。


清理后5PASS/0Skip（416ms）及NET8零警告（18.08s）完成；30临时/641取证/534原会话与4最终文件删除，零新增永久测试。


## 六包限定批准登记

2026-11-01 用户明确回复“仅批准六表现有条目及列明纠正，其余保留”（request_user_input_async / call_08c452ba85d7442ab817b3cf6c7dcce6 / item 0）。仅Flyouts48对及表列两项有效默认纠正、Collapse12对、Descriptions16对、GroupBox40对、Statistic12对、QRCode16对获得限定视觉批准。各表列明的裁剪、低对比、原失败、缺帧、恢复与持续态、平台及完整家族/CT15均保留，不授权提交；GroupBox三Reserved另按call_c8732844a7a841caacc4385b24e13bd3/0既有批准，不扩大。
