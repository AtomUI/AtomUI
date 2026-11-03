# CT15 Card 限定视觉审查（2026-10-31）

**已获用户限定批准，仅以下32对有意变化与两项属性默认纠正。** 清理后Card20项、NET8零警告、成员11/6/0通过，临时源/434工程/591取证已清理。工程、清理后精确验收见[结果](card-consumers-results.md)；不申请完整Card/CT15通过，不授权提交。CT12/13/14和CT25/FW1原限定边界保持。

12卡完整固定场景：1260×1380、scale1，三列Small/Middle/Large、四行普通/Meta/Grid/Tabs，380×300；保留标题/Extra、两操作按钮、封面/Meta、两Grid内容和两Tab正文。四配方实际Skia同历史旧新对比，正常Motion/Dispatcher泵；至少700ms+12稳定帧，零通用容差。A/A和1DIP恢复全0，1DIP检错非零；Mac managed/AOT各900+148检查，静态88对JIT/AOT全0。

## 32对范围

每格一对，格式为`changedPixels/maxChannelDelta`。所有阶段均包含Small特殊body修复；其余变化仅本表描述的公开原生/Own/Global/专用Style生效。

| 阶段 | Default | Compact | Dark | Dark+Compact |
| --- | --- | --- | --- | --- |
| default | 4514/255 | 4293/255 | 4504/200 | 4281/200 |
| local | 8228/255 | 7918/255 | 8228/200 | 7918/200 |
| padding | 85863/255 | 84555/255 | 85658/250 | 84401/250 |
| own | 188901/255 | 166213/255 | 188888/228 | 166199/228 |
| global | 1346375/255 | 1346257/255 | 1347215/248 | 1347097/248 |
| parts | 16662/255 | 41685/255 | 16662/235 | 41643/235 |
| inner-borderless | 4771/255 | 4555/255 | 4761/207 | 4543/207 |
| rtl | 4081/255 | 3864/255 | 4074/200 | 3857/200 |

- default：Small Grid/Tabs外body不再额外包一层普通padding；只有这两卡改变，内部item/content Padding仍保留。普通/Meta及Middle/Large默认几何保持。原多余内缩去除会扩展真实可用内容区域。
- local：Native Arial19/Navy、边框2、圆角13；新根文字/继承保持实际配置，角色标题仍各自字号。保留全部原内容和固定尺寸。
- padding：原生Card.Padding11、GridItem.Padding17进入真实Frame；会缩小相应内区，符合显式配置。
- own：三档标题高度51/57/61、字号17/19/21、header Padding11/13/15、body Padding13/17/19、HeaderBg#e8e0f0、明确ActionsBg#d8e8f8/ExtraColor#802080、action icon27真实生效；未配置透明/继承色保持。
- global：每公开owner Menlo19/#802080，Card表面#f8e8d8/边框3/圆角17，Meta标题23/角色色与ActionButton独立Global生效；不把独立子控件全部复制为Card根设置。
- parts：生成CardHeader/Title/Extra/Cover/Body/ActionsStyle及Meta四专用Style真实命中；actions橙色与Padding7,11,13,17进入真实背景/布局，自绘分隔线跟随实际子项坐标。标题/Body/Meta原内容不删除，Native优先级不通过code-behind绕过。
- inner-borderless、rtl：保留对应业务分支和RTL原顺序，只包含已述Small重复padding纠正；未重新实现独立TabControl合同。

## 两项属性默认纠正

内部CardActionPanel.Padding原style值top1没有参与模板布局，改为原生0并投射到Frame，默认实际布局保持；专用Style配置的Padding开始有效。CardGridItem.root.Padding原0改为三档当前Own值，Frame原实际三档Padding保持，同时让原生本地Padding优先。两项可观测属性值变化与上表一同申请限定批准，不能用“默认像素相同”掩盖。

ActionsSpacing/TabsMarginBottom/CardHeadPadding/CardPaddingBase四历史字段保留Reserved，保留名称、默认、type/key/slot及配置往返；无独立生产目标，不凭名称创造gap/第二套padding。18 Active包括两OptionalOverride。分类详见正式[token](../../../controls/desktop/data-display/card/token.md)。

## 排除项

恢复到最初default的312/max2（两明配方）、110/max1（两暗配方），以及旧新global-restore的4630/4389/4589/4347等原始非零均不申请通过。其他52重复旧新记录不扩大为独立批准；完整Card/CT15、Timeline P3端点与Part退出、Alert微差、NSWindow/其他Host及CT25/FW1继续保留。任意极值无裁剪不由固定场景证明。

所需结论仅为“批准Card表内32对和两项属性默认纠正，其他未通过与完整CT15保留”。本表不授予Git提交/推送或依赖/容差变更许可。


## 2026-10-31 限定批准登记

2026-10-31，用户明确回复“批准表内32对和两项属性默认纠正，其余未通过与完整CT15保留”（request_user_input_async / call_f0de5e7e830f44efaa6cd74e2c0072e0 / item 0）。仅本表32对和列明两项有效值/属性默认纠正获限定批准；其他原始非零、P2及完整家族/CT15保留，不授权Git提交。
