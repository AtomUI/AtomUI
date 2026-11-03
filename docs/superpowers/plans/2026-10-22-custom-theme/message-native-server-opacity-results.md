# CT15 Message 原生3497的server状态单变量核对

本次只检验尚未覆盖的因素：原记录证明client/effective Opacity1，但实际server Opacity/OwnTransform/CombinedTransform是否仍有正常入场残留。不是新淡入/Border模型，不更改Message或官方依赖，不自动扩张FW1/FW2。

复用本会话已校验原驱动复原器，33718初Program及33748/33816/33997/34009/34076/34091/34188/34231/34274/34288/34302/34370/34505/34519/34559/34730/34849/34954共18次文本修订。完整25926字符，SHA256与原`39f5f57c144fd390d1a8e69b901b0fab4dcc68cf75ca3cec9ff2e5e8d20b7631`一致。解释字符串变换，不执行历史产品/Git命令。

当前新Core下原真实native基线51检查PASS/exit0：960×720/scale2、Dark+Compact、BottomCenter、原Independent native calibration全文、226.5×30/点367,686/三段阴影、正常motion与no-motion原对照、激活和16次原捕获顺序保持。normal A/A及Window初重绘0；初Frame重录及后续移动/Window/Frame恢复均3497/max2、原bbox(739,1334)–(1183,1369)。no-motion同6比较全0。没有因新Core批准就声称这个旧失败已修。

本地精确官方12.1.4源码显示实际server visual.Opacity参与Render opacity stack；internal Compositor.PostServerJob(Action,postTarget:true)可在render线程提交后读取。只在原screencapture完成后读frame及全部视觉祖先的server状态，记录client/server精确double和真实矩阵；不在UI线程直接读渲染状态，不设置属性、绘图、重绘或关闭动效。外部临时反射仅JIT诊断，不进入产品/不冒充新AOT。读取调度可能推进一次commit，必须与无探针原基线比较，不先声称观测无扰动。

首次外部工程继承中央包管理却重复声明Avalonia.Desktop版本导致NU1008，删除重复Version后沿用仓库官方版本；不是产品RED。有效原基线构建0警告/0错误6.23s。当前只读副批执行中，后续证据/清理与精确Message模块仍必需；3497继续FAIL，完整CT15未关闭。

实际render线程副批51PASS/exit0，原16张native截图与基线全部RGBA0，首次Frame重录3497/max2仍同原值。frame及15级相关视觉链共240条server记录均Opacity精确1；client同为1。OwnTransform/Combined字段虽打印identity，不把这些缓存属性误当已证明的实际world绘制矩阵或全部绘制状态。

为确认server读取非固定占位，原全部历史结束后仅对调用方原Window.Content公开Control设置Opacity0.5，再ClearValue恢复1并同法读server；不设置Message语义Part，不作为定制示例，不影响之前原帧基线。该正对照执行中，本分支只排除观察时server透明度残留，不声称3497根因已明或FW2归属已获授权。无产品改动、新AOT或模型。

正对照51PASS/exit0，原caller Border client/server均0.5，ClearValue后均1，读取有效。原240条server记录精确1，缩小并排除观察时server透明度残留因素；矩阵缓存字段不作world绘制证明。原3497继续FAIL。

新线索仅为实际差异bbox位于Frame顶部上方阴影区。下一只在原Capture完成后读取同一Border的实际Bounds/背景/边框/圆角/BoxShadow、真实_borderRenderHelper缓存和client保留DrawList的opcode/资源，比较首次与Frame重录；不再建阴影模型、不更改shadow或motion。如果这些记录也一致，仅排除本层记录值差异，不继续堆同类实验或自动转FW2。此末次字段读取仍沿原51性质/16捕获，独立after-history caller正对照不作为主场景基线。

末次commands51PASS/exit0。normal-motion八阶段的Frame Bounds/背景/边框/圆角/BoxShadow、实际缓存、资源和opcode原始字节**全部逐字相同**；normal与no-motion记录也相同。原3497的第一次重录/后续恢复仍严格非零，no-motion对应恢复仍0。原16图与观察/正对照/命令三个副批的48逐图比较全部RGBA0，未因记录改变原呈现；三批新增server/cmd数据仅JIT，不冒充新AOT。

实际原native整图已查看；原Frame上方阴影区域的3497/max2继续FAIL。此次缩小了server透明度和本层绘制命令值差异，但仍没有原全部渲染状态、server缓存/clip/图层和独立框架反例的完整因果证明。停止此分支，不把四批重复计为更多不同功能用例，不新发AOT、不改产品/阴影/动效/依赖/容差。仅准备当前原四条正常入场后严格非零比较的有限独立调查提案，须用户裁决；未自动扩大FW1/FW2或关闭完整CT15。72媒体/日志与外部工程后续清理，精确Message模块仍必需。

Cleanup:officialaudit clean/findings=[],removed73(72evidence/logs+manifest);external148files/616313617bytes,all native processes ended,drivers/marker removed.No project temporary sources/productchanges;matchingNET8zero18.78s reused;focusedMessage gate follows.No full regression or commit.

最终清理后精确MessageSemanticPart/FeedbackManagerPadding：17PASS/0FAIL/0Skip、1s；无产品源码变化，匹配NET8零警告18.78s复用。73取证/148会话已全部清理；NM1–NM4有限调查待裁决，3497仍FAIL，无新AOT/全量/提交。
