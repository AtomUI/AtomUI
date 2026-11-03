# CT15 Alert：Own、局部Global与图标Style限定视觉

2026-10-30，**表内28对已获限定批准，非完整CT15批准**。仅申请以下28对明确配置生效的有意变化；额外禁用/恢复、AOT微差等仍未通过，不申请扩展任何像素容差。工程与原始失败见[结果](alert-consumers-results.md)。

环境为相同1280×840/scale1、八根Alert、四Type×有/无描述、Label/短Marquee、原消息/详情/Action/关闭、正常motion。固定Height180的19阶段保留，另补自然高度；两侧同一完整操作历史。默认、既有原生定制及清除、RTL和真实输入旧新均0；Part对close41/title Margin11的覆盖保持有效，图标颜色变化另列。

## 固定高度的20对有意变化

| 配方 | Own接线 | 全Own置0 | Alert局部Global/字体 | 专用图标Style | 专用Style+Own |
| --- | --- | --- | --- | --- | --- |
| Default | 35064 | 13412 | 819168 | 2174 | 3745 |
| Compact | 33208 | 12388 | 821888 | 2174 | 3745 |
| Dark | 35230 | 13672 | 819168 | 2174 | 3745 |
| Dark+Compact | 33374 | 12648 | 821888 | 2174 | 3745 |

数字为完整RGBA改变像素，不是容差。Own组中其他8字段两版原本都生效；本次新增变化来自CloseIconSize37（旧实际12→37）和有描述Label/Marquee标题Margin bottom13（旧0→13）。全0时旧关闭glyph仍12，新按0消费；不自动改变IsClosable或关闭事件。

局部Global组现到达四状态背景/边框/图标、根圆角17/描边3、兼容glyph19、Menlo23/标题角色29与文字色等原目标；旧模板主要读Shared而忽略Alert独立输入。原生本地值和正文/标题角色合同保持。

专用Style组使用真实生成AlertIconStyle声明FillBrush=Orange：旧版Tag命中但颜色被模板Local绑定压住，新版四状态实际转橙色。Style+Own中专用close41及title Margin11保持优先；不是code-behind定制回退，不改route或Part名。

## 自然高度的8对有意变化

| 配方 | Own接线 | 局部Global/字体 |
| --- | --- | --- |
| Default | 81742 | 486656 |
| Compact | 90058 | 439496 |
| Dark | 81976 | 486656 |
| Dark+Compact | 90304 | 439496 |

无描述标准Own场景的自然高度48→53、Compact44→53，有描述85→98；37×37真实glyph和新增标题留白参与测量，宽580和完整消息/详情/Action保留。改变范围包括相应表面增长与内容自然重排。原固定高度场景未移除，清除后自然高度及画面恢复0。

## 审查排除项

不申请批准：旧新4对disabled/恢复的288/max2、228/max1、185/max1、228/max1；JIT/AOT一对288/max2；相同候选重复的两对228/max1；固定矩阵disabled恢复到初始的281/185/199像素；既有局部Dark黑字、任意极值不裁剪、NSWindow系统最终显示/其他平台，以及P3-B仍保留的Part退出/周期端点。原始记录保持，不由本表或CT12/CT25/FW1既定批准自动豁免。

所需结论仅为“批准Alert表内28对配置/Style生效变化，保留其他未通过与完整CT15”。批准不授权Git提交、推送、依赖替换或扩大通用容差。

## 批准登记

2026-10-30，用户明确回复“批准 Alert 表内28对，其余未通过与完整CT15保留”（request_user_input_async / call_8790c4384212492cba2b1512c1e8b335 / item 0）。仅表内28对有意Own/Global/Style变化已获限定批准；额外4旧新/1AOT微差、P3未通过及完整CT15保留，不授权Git提交。
