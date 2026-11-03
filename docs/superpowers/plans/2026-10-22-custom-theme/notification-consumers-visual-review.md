# CT15 Notification 限定视觉审查（2026-10-31）

**已获用户限定批准：仅下表36对有意变化和主题应用后root两项有效值归位。** 55验证，清理后33项/NET8零警告/成员13-3-0通过；22临时/434工程/598取证已清理。工程与清理后验收见[结果](notification-consumers-results.md)；不申请完整Notification/CT15关闭，不授权提交。CT12/13/14和CT25/FW1边界不变。

1280×900、scale1，3×2六卡，五Type加一自定义Icon/Control，完整描述/Action/关闭/进度；默认384宽、MaxHeight380保持。正常Motion和Dispatcher，旧新同历史，A/A/独立1DIP恢复与五业务恢复阶段均0；managed/AOT各432+12检查，76对主帧JIT/AOT全0。

## 36对范围

每格一对，格式`changedPixels/maxChannelDelta`：

| 阶段 | Default | Compact | Dark | Dark+Compact |
| --- | --- | --- | --- | --- |
| global | 84952/255 | 84136/255 | 103967/230 | 98289/230 |
| native | 42707/255 | 41747/255 | 47397/220 | 45852/220 |
| retemplate | 4404/228 | 4404/228 | 4404/180 | 4404/180 |
| progress-show | 4404/228 | 4404/228 | 4404/180 | 4404/180 |
| reattach | 4416/233 | 4416/233 | 4416/189 | 4416/189 |
| rtl | 4416/233 | 4416/233 | 4416/189 | 4416/189 |
| rtl-restore | 4416/233 | 4416/233 | 4416/189 | 4416/189 |
| selected | 4416/233 | 4416/233 | 4416/189 | 4416/189 |
| close | 3680/233 | 3680/233 | 3680/189 | 3680/189 |

- Global：Card Menlo、正文19/标题23、正文Purple/标题Green、状态色、close角色色及圆角17进入真实目标；用户Control内容保留。
- Native：root Arial19/Navy进入真实正文，标题仍自身角色；原边框2/Orange、零圆角/Padding11继续有效。
- retemplate/progress-show：初始总10秒、当前remaining4秒；候选继续40%，旧版因视觉part重建错误回100%，只修总量归属。
- reattach：候选重建唯一当前progress并保持40%，旧版丢失；RTL/恢复、真实选择与最后关闭阶段保留同一修复，close图中只剩五个活动进度面。原标题/正文/Action/关闭、窗口约束与命中合同保持。

## root有效值归位

应用主题后四配方root FontSize原12→14；Foreground原Black→明色#e0000000、暗色#d8ffffff，承接原正文角色。默认实际图像四配方精确0，标题FontSizeLG/ColorTextHeading角色及原生/专用Style优先保持。这是ControlTheme有效值变化，不是重注册metadata默认。

## 排除与后续

Dark、Dark+Compact两张global-restore各4/max16不申请批准；不新增阴影/glyph/平台容差。真实Host/六位置、共享Stack通用几何、自然高度及任意极值不由本场景证明，完整Notification/CT15与其他家族未通过继续保留。官方Avalonia12.1.4不替换，其他关闭家族不重开。

所需结论仅为“批准Notification表内36对及root两项有效值归位，其余未通过与完整CT15保留”。不授权Git提交/推送。


## 2026-10-31 限定批准登记

2026-10-31，用户明确回复“批准表内36对及root两项有效值归位，其余未通过与完整CT15保留”（request_user_input_async / call_0421214e34e1401e90c4b4e835ef3c4e / item 0）。仅本表36对和列明两项有效值/属性默认纠正获限定批准；其他原始非零、P2及完整家族/CT15保留，不授权Git提交。
