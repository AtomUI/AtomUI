# CT15 Avatar 原open恢复限定调查审查

状态：真实场景分层、清理和精确门禁已完成；AR1/AR2已获新增限定调查归属批准。详细证据只有[原历史结果](avatar-original-overflow-results.md)，不复制过程，不申请完整Avatar/CT15通过或提交。

## 仅两条路径

| ID | 原完整历史 | 合成器原恢复 | 差异位置 | 真实Frame直接/Visual | 真实Avatar/整窗即时 | managed/AOT |
| --- | --- | --- | --- | --- | --- | --- |
| AR1 | Light open-default→open-own→open-restore | 12像素/max8 | (59,178)–(73,180)，主场景图标Avatar底边 | 两比较均0 | 两比较均0 | 对应恢复帧跨运行0，AOT同12/max8 |
| AR2 | Dark 同完整历史 | 12像素/max7 | 同坐标、同主场景节点 | 两比较均0 | 两比较均0 | 对应恢复帧跨运行0，AOT同12/max7 |

原900×700、四个Group成员/显示二、原三独立Avatar/8×8借用位图、全文、normal motion、原Own47与Group间距19、配方背景、完整popup动作保持。普通明暗各12个差异点不在popup内。焦点为空、受影响节点无pointer-over；有效颜色/Opacity/Size/Radius/CenterBorder/零描边/无shadow恢复一致。

## 分层证据与限制

- 原分支20检查、实际分层20检查及AOT20检查通过。真实Frame直接、Frame Visual、Avatar Visual、整窗Visual共四种输出，在四算法初值/恢复各自严格0；这是实际消费者输出，不是独立近似控件。
- 即时整窗Own检错检出L25766/D24779/C25150/DC23780像素、max255，恢复全部0；即时输出非空，关键整窗/Avatar图已查看。该正对照是新增managed扩展，未冒称同一AOT重新发布。
- 52张managed/AOT跨运行48零；closed四张L88/max8、D57/max7、C53/max12、DC44/max13仍非零，**不在本提案内**。
- Frame、Avatar、完整背景重绘不能消除AR1/AR2；官方独立圆对照未复现。故本表仅收敛到真实场景保留式呈现结果，**没有证明完整框架根因，也不声称与既有FW2局部重绘同根因**。
- AOT29064272字节，SHA256 `72dce6bdf62f180aa27973c629b3733bf4f74d1f46dbf5570488ed368c22965b`，0 IL/CS警告；五条既有Homebrew最低OS链接警告单列。无生产修改，不替换官方依赖。

## 请求的有限裁决

仅请求把AR1/AR2以独立子记录交CT25/FW2继续调查：保留其原历史、12像素严格FAIL和上述限制，不批准非零正确，不增容差、不打重绘补丁、不改FW1。需要新裁决是因为既有FW2批准没有覆盖Avatar；本表不自动扩大其归属。

其他closed/跨运行、原Avatar48对批准之外的变化、其他家族/平台/完整CT15继续保留。清理与精确门禁完成后才提交本表审查。

收尾：334取证及外部445文件/1179655232字节、驱动/发布/marker已清理，audit clean。精确AvatarGroup13PASS/0Skip、1s；无生产/测试编译输入变化，匹配Common7/最终NET8证据复用，不全量、不提交。

## 限定调查归属批准

2026-11-02，用户选择“1. 仅批准AR1/AR2交CT25/FW2独立记录继续调查；非零仍FAIL，不认定与原FW2同根因”（request_user_input_async / call_62d934e07b11473ea906b50da709c29a / item 0）。仅上表两条原open恢复路径按此归属继续调查；closed四张、其他跨运行、其他家族、完整CT15和不提交约束保留。原先free-text问题的“给出选项让我选择”不是批准，本登记以本次明确选择为准。

## 2026-11-03 实质修复候选补证

Avatar最终原主/popup/校准100性质、152静态已在同一临时软件Skia候选上JIT/AOT和完整server参考全0，原AR1/AR2及当前closed相应差异消除；项目官方依赖未采用，原历史与其他批准边界不改写。完整原因、源码/产物身份和最后验收只见[唯一结果](feedback-region-sdk-results.md)，不重复采样或另提归属选择。
