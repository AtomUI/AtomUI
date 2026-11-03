# CT15 Core Icon 几何快照限定视觉审查

状态：工程、AOT、全部临时清理及精确模块/NET8门禁已完成。本次共享规则修正、明确默认影响及20例证已获限定批准，不代表完整CT15通过。详细证据只见[根因与结果](icon-retained-geometry-investigation.md)。

## 本次共同渲染规则修正

DrawingInstruction为每次绘制记录独立几何，并保留原指令矩阵/图标全局矩阵。该规则作用于所有使用此绘制器的指令，包含已关闭家族使用的生成SVG；CT12/13/14历史批准结论不改写。本次需明确批准该共同规则修正及其默认笔画范围影响，不能隐瞒为“默认无变化”，也不等于每张下游图或每个家族已经视觉通过。宽高/布局/画刷角色/笔宽语义保持；调用方几何热更新、重挂与资源退出已经验证。

## 十二个真实图标例证

Orange为显示轮廓而明确设置的原生画刷。下表为旧新合成器完整RGBA变化像素/max；候选合成器与即时输出、旧新即时输出及候选/AOT均为0。

| 图标与尺寸 | 旧新像素/max | 候选与即时 | 候选/AOT |
| --- | --- | --- | --- |
| CloseOutlined-12 | 52/96 | 0 | 0 |
| CloseOutlined-32 | 120/255 | 0 | 0 |
| CloseOutlined-37 | 244/255 | 0 | 0 |
| CloseOutlined-67 | 508/255 | 0 | 0 |
| ExclamationCircleFilled-12 | 53/96 | 0 | 0 |
| ExclamationCircleFilled-32 | 180/224 | 0 | 0 |
| ExclamationCircleFilled-37 | 240/240 | 0 | 0 |
| ExclamationCircleFilled-67 | 627/255 | 0 | 0 |
| UserOutlined-12 | 54/159 | 0 | 0 |
| UserOutlined-32 | 233/255 | 0 | 0 |
| UserOutlined-37 | 297/255 | 0 | 0 |
| UserOutlined-67 | 752/255 | 0 | 0 |

## 八个真实消费者默认例证

| 消费者/算法 | 旧新像素/max | 候选/AOT |
| --- | --- | --- |
| tag/light | 490/251 | 0 |
| tag/dark | 490/212 | 0 |
| tag/compact | 490/251 | 0 |
| tag/darkcompact | 490/212 | 0 |
| avatar/light | 104/55 | 0 |
| avatar/dark | 104/155 | 0 |
| avatar/compact | 104/55 | 0 |
| avatar/darkcompact | 104/155 | 0 |

以上20对是共同规则修正的具体视觉例证。332份布局/状态记录旧新及跨运行均逐字相同。已查看UserOutlined67旧新、真实Avatar默认及完整Compact Tag global-clear图。原生尺寸、完整内容与原裁剪保持。

## 明确排除项

356对候选/AOT中353为0；Dark Avatar closed57/max7、DarkCompact closed44/max13、Compact Tag group-scroll4/max1仍FAIL。Avatar open恢复仍12/max8和12/max7（两Compact为0），既有有限调查归属不是修复。Tag本轮明确配对52项恢复，候选/AOT均46零/6非零：Dark own-clear24/max1、Compact own-clear2/max1/native-clear5/max1、DarkCompact own-clear21/max1/native-clear7/max2/large-restore12/max1。原18非零恢复及全部历史失败账本继续保留。

本20例证不将336张旧新非零逐张批为通过；其中含其他Tag/Avatar状态的共同绘制变化及原恢复差异，不申请通用像素容差或FW2扩张。原Avatar popup模式没有pre-open默认截图，不能拿另一进程main默认替代该基线推断closed恢复。

AOT46808144字节，SHA256 `3bb622bb108f60861ab0185501b62ab5e361b35e09e7f365985acf52c6e0febd`，0 IL/CS警告、5条既有Homebrew linker警告。原图标一致性12FAIL，候选/AOT12PASS；Tag104+4+12、Avatar76+4+20各版通过；原生几何原6/7、候选/AOT7/7；六临时聚焦通过。须补齐最终清理、精确模块和NET8结果后才申请审查。不授权提交。

Final gate:6 temporary cases/8421 bytes removed;2070 evidence files+manifest cleaned (audit clean,removed2071);external822 files/3233947432 bytes,all drivers/runtimes/publish/marker removed. Cleaned source focused Desktop78PASS(2s)+Common Avatar7PASS(373ms),new Desktop NET8 Release0warnings/0errors21.16s. Source freeze unchanged,member2files/1control0issues,scoped doc links and diff check clean. No permanent tests,full regression or commit. Visual shared-rule/default20examples still pending.

## 限定批准登记

2026-11-02，用户选择“1. 批准上述共享规则修正、明确默认影响及20例证；所有列明未通过与完整CT15保留”（request_user_input_async / call_2549959c004349c29787910a8cc10909 / item 0）。仅按本问题和表列范围批准；其余严格FAIL/未列下游、CT12/13/14历史结论、FW1/FW2既有边界、完整CT15与不提交约束保持。
