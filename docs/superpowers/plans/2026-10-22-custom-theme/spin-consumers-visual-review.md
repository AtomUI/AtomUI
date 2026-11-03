# CT15 Spin 限定视觉审查（工程、清理和最终验收完成，46主静态已获限定批准）

仅下表46对明确主静态变化提请后续限定审查；主批明确配置ControlAlgorithmMode.Global，不将暗色Primary路径变化冒称无配置默认迁移。输入5metadata/7Own，137/274保持。原版→候选的圆点/Tip取真实owner Global，字体、Mask、Spacing进入真实目标。最终三版各172帧，136静态cross全0。

| 算法 | 状态 | 非零像素 | max | bbox |
| --- | --- | ---: | ---: | --- |
| L | global | 100800 | 169 | [40, 30, 460, 270] |
| L | native-font | 100800 | 169 | [40, 30, 460, 270] |
| L | native-font-clear | 100800 | 169 | [40, 30, 460, 270] |
| L | scope-switch | 12208 | 184 | [40, 30, 448, 172] |
| D | custom-clear | 1128 | 43 | [156, 156, 345, 169] |
| D | global | 100800 | 181 | [40, 30, 460, 270] |
| D | global-clear | 1128 | 43 | [156, 156, 345, 169] |
| D | parts-clear | 1128 | 43 | [156, 156, 345, 169] |
| D | own | 1128 | 43 | [156, 178, 345, 191] |
| D | own-small | 1128 | 43 | [156, 178, 345, 191] |
| D | own-large | 1128 | 43 | [156, 178, 345, 191] |
| D | native-size | 1128 | 43 | [156, 178, 345, 191] |
| D | own-clear | 1128 | 43 | [156, 156, 345, 169] |
| D | native-font | 100800 | 180 | [40, 30, 460, 270] |
| D | native-font-clear | 100800 | 181 | [40, 30, 460, 270] |
| D | scope-switch | 12125 | 214 | [40, 30, 448, 172] |
| D | scope-clear | 1128 | 43 | [156, 156, 345, 169] |
| D | retemplate | 1128 | 43 | [156, 156, 345, 169] |
| D | retemplate-clear | 1128 | 43 | [156, 156, 345, 169] |
| D | reattach | 1128 | 43 | [156, 156, 345, 169] |
| D | visible | 1128 | 43 | [156, 156, 345, 169] |
| D | spinning-restore | 1128 | 43 | [156, 156, 345, 169] |
| D | motion-clear | 1128 | 43 | [156, 156, 345, 169] |
| C | global | 100800 | 169 | [40, 30, 460, 270] |
| C | native-font | 100800 | 169 | [40, 30, 460, 270] |
| C | native-font-clear | 100800 | 169 | [40, 30, 460, 270] |
| C | scope-switch | 12135 | 184 | [40, 30, 448, 169] |
| DC | custom-clear | 1128 | 43 | [156, 154, 345, 167] |
| DC | global | 100800 | 181 | [40, 30, 460, 270] |
| DC | global-clear | 1128 | 43 | [156, 154, 345, 167] |
| DC | parts-clear | 1128 | 43 | [156, 154, 345, 167] |
| DC | own | 1128 | 43 | [156, 178, 345, 191] |
| DC | own-small | 1128 | 43 | [156, 178, 345, 191] |
| DC | own-large | 1128 | 43 | [156, 178, 345, 191] |
| DC | native-size | 1128 | 43 | [156, 178, 345, 191] |
| DC | own-clear | 1128 | 43 | [156, 154, 345, 167] |
| DC | native-font | 100800 | 180 | [40, 30, 460, 270] |
| DC | native-font-clear | 100800 | 181 | [40, 30, 460, 270] |
| DC | scope-switch | 12055 | 214 | [40, 30, 448, 169] |
| DC | scope-clear | 1128 | 43 | [156, 154, 345, 167] |
| DC | retemplate | 1128 | 43 | [156, 154, 345, 167] |
| DC | retemplate-clear | 1128 | 43 | [156, 154, 345, 167] |
| DC | reattach | 1128 | 43 | [156, 154, 345, 167] |
| DC | visible | 1128 | 43 | [156, 154, 345, 167] |
| DC | spinning-restore | 1128 | 43 | [156, 154, 345, 167] |
| DC | motion-clear | 1128 | 43 | [156, 154, 345, 167] |

固定800×600、Spin420×240、完整原始Tip/正文/页脚、300×100视口与900extent保持。Global字体23使原长Tip沿420宽裁剪，与正文/页脚局部重叠；原生Courier17仍优先。深蓝显式色与暗底低对比已实际查看，不宣称极值可读。未通过或未审查：32跨运行持续态非零、32旧新持续态/8独立cal旧新非零、OS/Browser、完整周期端点、GC整体与完整家族/CT15。它们不按本表自动通过，不扩CT25/FW1或容差。

自定义PathIcon原生27×31及Orange优先修正另列属性合同：原测试显式IndicatorSize67时旧强写67×67，候选保留27×31；主驱动清Own后的旧值为L/D22×22、C/DC19.5×19.5。对应持续旋转帧仅作状态观察，不把不同相位非零全部批准为预期。仅模板Custom选择由同一ContentTemplate输入控制，持续时相同样保留。

完全无控件配置的默认补证、临时清理、清理后精确模块/NET8待，不提前请求批准。

最终50所选全PASS（34临时+既有16），普通Style热Own/移除/重加/重模板保持。candidate4/AOT4各124检查全PASS，旧124中28FAIL、各172帧完整，无缺帧。独立新实例无Controls配置12静态旧新/跨运行全0，plain往返4全0；主56恢复及A/A4、校准恢复4各全0；1DIP各1210/max233或170。未更改正常motion/官方依赖/容差。首86错误驱动帧和pre-hot-style480帧独立留账，最终使用新完整轨迹，不沿用旧批作为修正后验证。


最终成员8源/2控件0问题，四算法静态最小760ms且连续原始RGBA≥12相同帧。34临时3源18621 bytes已删除；1114取证文件（含86错误驱动原帧、480 pre-hot-style帧、516最终帧、4拼图与TSV/manifest）经官方audit/cleanup清理，findings=[]/removed=true；driver420文件与两publish各15文件、原会话共540文件/1200650917 bytes已删除，源码10源冻结逐字一致。最终精确模块/NET8在独立小会话执行，不保留任何媒体或驱动。


清理后精确验收：Debug/net10真实重建，SpinSemanticPartTests/SpinIndicatorCustomSizeTests/RecurringVisualWorkTests.Spin_共16PASS/0FAIL/0Skip、663ms（命令36.54s）；NET8 Release桌面构建覆盖Common，0警告0错误21.80s（命令22.05s）。均先取得共享产物非阻塞独占锁。最终run.py/cleanup.json/module.log/net8.log四文件会话随后清理，正式五文档相对链接0断链、git diff --check通过。仅表内46主静态和列明原生glyph优先修正待限定批准；其他边界与完整CT15不关闭。


限定批准登记：2026-11-01，用户明确回复“批准表内46对及列明原生优先修正，其余边界保留”（request_user_input_async / call_4176ecaa6e314f4da281e28a8657ee2f / item 0）。仅Spin表内46对主静态与列明glyph尺寸/普通Style优先修正获批准；32持续态cross非零、8独立cal旧新非零、平台/GC/完整周期及完整家族/CT15保留，不授权提交。
