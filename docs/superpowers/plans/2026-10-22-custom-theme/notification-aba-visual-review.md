# CT15 Notification 新 ABA 初始绘制历史限定调查表（已获有限调查归属批准）

仅准备申请下列18条具体路径交CT25/FW2继续调查：8条当前真实ABA（每条保留restore及随后root-redraw判据）、3条no-motion初始重绘、4条独立入场图元、3条独立静态图元。当前AOT真实ABA、清理与最终门禁均已完成，仅表列18路径已获有限调查归属批准，原话与请求ID见末尾。非零仍FAIL，不关产品动效/强制重绘、不改FW1/依赖/像素容差，不关闭Notification或完整CT15。

本表不是原旧新4/max16两对：那两对已在notification-original-visual-review.md另获限定接受。本表只针对[后续新ABA原场景](notification-restore-audit.md)，仍保留1280×900、六卡/五类型+原PathIcon与caller TextBox、384宽/MaxHeight380、完整描述/Action/4/10。

## 8条真实原ABA

每格`restore像素/max；root-redraw像素/max`。A/A、实际值/geometry/gradient stops/原实例与10/4恢复检查均通过；严格非零失败保持。

| 配方 | colour | global |
| --- | --- | --- |
| Default | 0/0；198/1 | 198/1；198/1 |
| Dark | 0/0；131/2 | 131/2；131/2 |
| Compact | 4/2；202/2 | 202/2；202/2 |
| Dark+Compact | 2/1；133/2 | 133/2；133/2 |

## 独立初始重绘与模型

| 配方 | 真实no-motion初始→重绘 | 纯静态同输入初始→重绘 | 纯入场模型初始→重绘 |
| --- | ---: | ---: | ---: |
| Default | 0（正对照，不计调查路径） | 0（正对照，不计调查路径） | 198/1 |
| Dark | 4/2 | 4/2 | 130/2 |
| Compact | 4/2 | 4/2 | 202/2 |
| Dark+Compact | 6/2 | 6/2 | 132/2 |

真实no-motion与纯静态的3条非零路径，其完整像素集合/每个有符号通道差全部相同。纯入场Default198、Compact202与真实原初始→重绘差分全部相同；两暗色纯130/132与真实131/133各差1个像素，位于原caller TextBox附近（976,520）/（974,512），明确不认定其已精确独立复现。未裁剪/遮罩/删除真实TextBox/调容差消去此差。

独立工程仅官方Avalonia.Headless/Skia12.1.4，断言无AtomUI运行程序集；标准Border/圆角/原Shadow/进度两矩形/梯度/同坐标六动作边框，图元不含业务文字及TextBox模板，不声称完整默认图片同。模型绘图数据来自原只读快照；简化图元与真实业务输入边界明确分开。

正常真实原4理论全部严格FAIL；独立驱动128检查124PASS/4恢复FAIL。关闭入场的副场景只用于因果，4理论1PASS/3FAIL，不作为产品修复。静态独立模型展示初始10→4绘制历史自身可改变，不能简单描述成Token值没恢复；不把找到框架反例改写为像素通过。

排除：原旧批/原生平台/其他家族、进度持续相位及完整CT15均保留；本表不覆盖当前模型尚差的两暗色像素的完整根因解释，也不扩大既有FW2其它表。待真实AOT/清理/精确门禁补齐后再申请有限调查归属。


当前真实managed/AOT128检查124PASS/4严格FAIL、40cross全0；no-motion各128中125PASS/3严格FAIL、40cross全0。纯入场24cross全0，纯静态三非零差分与真实静态逐通道全同，严格图元重绘8判据1PASS/7FAIL/exit2。真实AOT46328112字节SHA256 18de30fdae92e7fc3fe48dfe4a9a2908784cceb870d545b70dbb6a71462ae566，纯AOT7608168字节SHA256 db2beb0248cbd093a6b31dd13a79252385d82db2df45af44892542a053de81a6，均零IL/CS warning、五既有linker warning。


清理：4临时理论/单源11785字节已删除，官方evidence cleanup clean/460文件删除，外部779会话2174450601字节及marker已删除，无生产源变化。最终Notification29PASS/0Skip/4.3151s，复用相同最终源码NET8零警告零错误17.41s；最后helper记录后清理。现仅申请上述18条路径交FW2调查的有限裁决。


## Notification 新 ABA 18路径有限调查批准登记

用户明确回复“仅批准表列18路径归CT25/FW2调查，其余保留”（request_user_input_async / call_3c0dd46bb06948bfa8d56cec286a3aff / item 0）。仅notification-aba-visual-review.md所列8真实ABA、3真实初始静态重绘、4纯入场、3纯静态交CT25/FW2继续调查；非零仍FAIL，不增容差、不关产品动效、不加产品重绘补丁。两暗色独立入场各差1像素的限制保持，不认定原全部框架根因；其他边界、FW1/官方依赖与完整CT15均保留，不授权提交。
