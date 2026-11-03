# CT15 Message P1 限定视觉审查（2026-10-31）

**已获用户限定批准，仅下表8对Global/Native有意变化，以及主题应用后root两项有效值归位。** 51验证、清理后29项/NET8零警告/成员10-2-0通过，22临时/434工程/1163取证已清理。工程与清理后精确验收见[结果](message-consumers-results.md)。不申请完整Message/CT15通过，不授权提交；不扩大CT12/13/14或CT25/FW1。

920×700、scale1、四条完整Information/Success/Warning/Error消息，正常Motion、原生选择/hover/关闭；Default/Compact/Dark/Dark+Compact。人工1DIP检错独立，主基线不含其污染历史；原始恢复FAIL保留。四默认旧新0，实际字体/前景/表面值和专用Style命中已核对，所有媒体按合同审查后删除。

## 8对有意变化

| 阶段 | Default | Compact | Dark | Dark+Compact |
| --- | --- | --- | --- | --- |
| global | 38342/255 | 34740/255 | 42199/255 | 37079/255 |
| native | 32804/255 | 32425/255 | 32587/255 | 32208/255 |

Global：MessageCard Menlo23、正文#802080、各Type明确状态色（Primary Green、Error Purple、Success Blue、Warning Maroon）、圆角17进入实际Card，manager背板圆角13的目标值另有工程断言。Native：root Arial19/Navy进入实际SelectableTextBlock，原边框3/Orange、零圆角/Padding11继续生效。四消息原内容、测量约束、图标、阴影和完整业务历史保留；自然宽度随真实正文增长，未删文本或关闭输入。

## root有效值归位

原正文字号/ColorText角色默认由root提供：应用主题后root FontSize四配方原12→14；Foreground原Black→明色#e0000000、暗色#d8ffffff。原默认正文和实际高度38/Compact30保持（四默认旧新精确0），Native/专用title Style仍优先。这里是ControlTheme有效值改变，不是Avalonia metadata默认重注册；两项与上表独立审查。

## 排除项

旧新其他37非零（其中类型变化4对50/58/62/73像素含业务glyph修复，但整帧仍未通过）、39 JIT/AOT非零、所有人工检错及业务恢复FAIL均不申请通过。JIT/AOT只有41/80零，不因上表批准改变记录；本表未授予通用glyph/平台/阴影容差。

CardHeight按T30保留Reserved，五实际Own与V2默认并不代表整个Message完成。Stack通用几何/真实Host/六位置、任意极值、NSWindow和其他平台继续P2；完整CT15及其他家族原未通过保持。

所需结论仅为“批准Message表内8对和root两项有效值归位，其他未通过及完整CT15保留”。不授权Git提交/推送。


## 2026-10-31 限定批准登记

2026-10-31，用户明确回复“批准表内8对及root两项有效值归位，其余未通过与完整CT15保留”（request_user_input_async / call_8fffdadfe1c0457483143eea833656e6 / item 0）。仅本表8对和列明两项有效值/属性默认纠正获限定批准；其他原始非零、P2及完整家族/CT15保留，不授权Git提交。
