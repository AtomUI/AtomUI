# CT15 原 P3-B 组合恢复限定归属审查（已获有限调查归属批准）

本表AOT、取证清理与最终精确验收已完成，仅下述8路径已获有限调查归属批准，原话与请求ID见末尾登记。原FW2批准只涵盖feedback-partial-redraw-visual-review.md所列3纯框架模型/24当前独立Part路径；本表不自动并入，也不修改既有FW1、CT12/13/14或完整CT15状态。

本次找回并重放原P3-B完整Program/专用AXAML组合及16阶段历史。没有把独立0.65矩阵替代原组合Content0.7，详见[timeline-original-part-history.md](timeline-original-part-history.md)。

| 配方 | 原组合Part退出（像素/max） | 原Content0.7独立退出 | 排除Content的其余原7Part退出 | 根背景黑白再回白 |
| --- | ---: | ---: | ---: | ---: |
| Default | 102/1 | 124/1 | 0 | 0 |
| Compact | 102/1 | 102/1 | 0 | 0 |
| Dark | 82/1 | 98/1 | 0 | 0 |
| Dark+Compact | 82/1 | 82/1 | 0 | 0 |

Compact两组独立Content的像素集合/有符号差与原组合完全相同。普通配方有22/16额外像素，不能将所有阶段合并成同一帧；两列各自保留。原组合bbox为Default(32,48)-(990,612)、Compact(32,48)-(990,602)、Dark(32,51)-(990,612)、Dark+Compact(32,51)-(990,602)。所有统计为完整1320×760同坐标RGBA，零通用容差、无裁剪/缩放/对齐/遮罩。

原同版本A/A、1DIP恢复、RTL恢复、Global+11Own清除均0，四recipe/6根/Height300、原Label/Button/长内容/静态PathIcon pending、真实鼠标/Enter与原11Own有效值保留。原440性质检查不等于像素全部通过。旧474节点按身份核对：旧473可见/内容节点值与几何相同；末根透明滚动条自身Bounds变化，并懒创建9个模板后代，新增9后代的祖先Opacity乘积全0，按透明子树单独记录，不能声称整棵树完全同。背景重绘只作为最后分叉诊断，不进入产品或污染原主批。

拟裁决范围仅表列4个原组合退出路径与4个原值Content独立路径是否交CT25/FW2继续调查；非零仍严格FAIL，不把框架独立opacity反例扩展为通用像素正确，不扩大任何容差。其他家族/原生最终面/原故障账本及完整P3/CT15均保留，不授权提交。当前managed/AOT四分支各1496性质检查，208可比整帧cross全部0；另外96只读观测对图同样0。AOT零IL/CS warning，五既有Homebrew linker warning；46377680字节、SHA256 c62dc04f9d035e40e875a30498295465e76928f4a6faa221604bd6631049d67b。1130取证已由官方cleanup审计clean后删除，外部454会话文件1374532913字节与marker删除，无仓库临时测试/产品修改。最终Timeline52PASS/0Skip/4.5166s，复用相同最终源码NET8零警告零错误17.41s。最终helper/marker读取后清理。


## 原 P3-B 八路径限定归属批准登记

用户明确回复“仅批准表列8路径归CT25/FW2，其余保留”（request_user_input_async / call_cd2233cdfe1848fc889d777f91b52754 / item 0）。仅timeline-original-part-visual-review.md列出的4条原P3-B组合恢复102/102/82/82及4条原值Content0.7恢复124/102/98/82交CT25/FW2继续调查；非零仍FAIL，不增像素容差、不改既有FW1，不把透明滚动条懒节点差别抹去。其他原故障账本、家族/原生平台/完整P3与CT15保留，不授权提交。此前3独立模型/24路径批准范围不自动扩成其他家族。
