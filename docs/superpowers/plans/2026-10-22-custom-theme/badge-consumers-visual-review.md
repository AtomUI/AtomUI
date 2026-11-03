# CT15 Badge 限定视觉审查（2026-10-31）

状态：**表内48对已获用户限定批准；临时清理与清理后32项/NET8验收已完成**。只申请本表48对有意旧新变化，不代表Count GC、其他非零或完整Badge/CT15通过，不授权提交。见[结果](badge-consumers-results.md)、[作者自审](badge-consumers-review.md)。

1000×1050/scale1，四算法实际内容面、正常motion、750ms+12等帧/6s上限，同坐标完整RGBA，无mask/crop/align或通用容差。每行4对，共12行48对；数值为变化像素/max通道差。

| 明确有意行为 | Light | Dark | Compact | DarkCompact |
| --- | --- | --- | --- | --- |
| 仅用户四专用Style：standalone direct owner修复后真实尺寸/布局命中 | 29841/max231 | 30008/max236 | 29622/max231 | 29775/max236 |
| 20Own改值在standalone/target热更新，四Style覆盖仍保持 | 14752/max255 | 14681/max255 | 14752/max255 | 14681/max255 |
| Small Own高度/字号/明确角进入实际目标 | 13627/max255 | 13556/max255 | 13627/max255 | 13556/max255 |
| Small默认角跟随有效Own高度27 | 30181/max231 | 30378/max236 | 29962/max231 | 30145/max236 |
| Default默认角跟随有效Own高度42 | 17594/max225 | 17859/max235 | 17432/max231 | 17659/max235 |
| 明确角0优先；Dot大小/阴影/间距0实际生效 | 17985/max225 | 17983/max235 | 17859/max231 | 17818/max235 |
| 三个public owner字体/颜色/角Global进入真实视觉 | 81917/max255 | 82215/max255 | 80863/max255 | 81115/max255 |
| Native Courier高于Global Menlo，原目标内容保留 | 81878/max255 | 82178/max255 | 80826/max255 | 81078/max255 |
| 清Native返回Global Menlo | 81917/max255 | 82215/max255 | 80863/max255 | 81115/max255 |
| 实例preset/hex颜色可移除层生效，原映射不变 | 29841/max231 | 30008/max236 | 29622/max231 | 29775/max236 |
| 清实例颜色回原Token，未保留旧Local/null | 32534/max231 | 32701/max236 | 32118/max231 | 32271/max236 |
| 同实例业务往返最终恢复；仅请求已说明的Part/颜色修复差异 | 32498/max231 | 32665/max236 | 32082/max231 | 32235/max236 |

## 限定范围

“default”场景含用户四专用Style，不能叫无定制默认。独立plain在attach前去用户class、保留全部原内容/target/standard marker，四对旧新均0。80默认/type/key/slot保持；Ribbon immutable矩阵与原fold语义单独验证。只修复实际消费者/字体优先/资源发布/Part direct owner/颜色clear，不替换依赖、不扩大任何像素容差。

本表同时包含原来未命中的standalone用户Style布局生效：Count53×31/Margin5、Dot19×19/Margin3，Ribbon Margin7/Opacity.75和content Font23/Orange/Padding13。目标文本保持；public owner字体Global使用正常附加属性继承，目标显式Native字体仍优先，继承字体的目标可能随owner显式字体配置变化。没有对DecoratedTarget节点直接写样式或改变Ribbon fold/目标180×90预算。

## 全部排除项

- 旧新164对仅8对0，156对非零；只本表48对请求，其余108非零整帧未批准。8零为plain四对及祖先隐藏四对。
- managed/AOT164对全部精确0，不能由此豁免同实例恢复失败。
- 三版独立A/A0，1DIP全检出；恢复Light27/Dark19/Compact24/DarkCompact17像素、max1，三版均未通过。候选/AOT检错15270/15389/15275/15336，旧15184/15275/15189/15222（max255/233/255/233）。主实例未经历检错扰动。
- 候选/AOT各48项主恢复32零；shown/end/rtl-clear各约4.6–4.8k/max233–255，reattach各8或18/max1均未通过。Dot standalone在显隐后阶段缺少点、后续重建恢复，原失败帧保留，不以最终business0覆盖。旧版其他恢复差异另列结果，不改期待或重建一侧消差。
- Count保留detached Adorner时仍经Border composition visual官方parent链保留owner，临时GC57项中该1项FAIL，其余56PASS；没有“已解决”的结论，不并入/扩大CT25/FW1或以小guard绕过。
- Processing现有源仅颜色，无独立循环pulse；真实相隔帧0不证明新动效已实现。show/hide过程时相、真实OS窗口/输入、Browser、未测极值及完整Badge/CT15保留。

原裸Button缺Theme的4 pointer失败批与颜色恢复失败批均单列保留；统一同尺寸/文本产品Button后原区域三点命中/实际click已通过，Badge hit-test/事件未更改。最后全版本相同历史重放，不挑旧批更小差异帧。


限定批准登记：2026-10-31，用户明确回复“批准表内48对，其余未通过与完整CT15保留”（request_user_input_async / call_d223a18435b5484787082d74b8ee393e / item 0）。仅Badge表内48对有意变化获批准；Count composition GC1FAIL、显隐/重挂/校准恢复非零、其余108对及完整Badge/CT15继续保留，不授权提交。
