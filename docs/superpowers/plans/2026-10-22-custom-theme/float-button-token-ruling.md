# CT15 FloatButton 两标量badge偏移分类提案（已获限定批准）

对象：CircleBadgeOffset、SquareBadgeOffset；仍计入原274Own、primary_task CT15。原提案阶段NeedsDecision5未提前减少；2026-10-31用户已限定批准两项Reserved，当前余3/GroupBox三字段。本文保留提案证据与复核义务，不是永久豁免。

## 证据

- 两字段当前及本地历史a8cdfc0f5保留Math.Sqrt(2)计算的单个double；32四算法默认/type/key/slot已捕获。
- AbstractFloatButtonTheme/FloatButtonHostTheme真实消费者是公开BadgeOffset Point。Circle Dot(-10,4)、Circle Count(-10,-2)、Square Dot(-5,-1)、Square Count(-10,-5)，实际坐标为原DesiredSize.Width加X、Y直接加0基线。
- 单个标量没有既定X/Y映射、Dot/Count差别或与公开Point优先的合同；直接施默认会改旧布局。明确Own37/clear未改变Point的临时事实验证已通过，Native Point13,17实际仍有效。

## 提案：Reserved兼容保留，不强行启用

两字段保留名称/type/key/slot、原默认公式及配置解析，明确当前不消费，不将其标ConsumerVerified或宣称已定制生效。实际定制使用已有公开BadgeOffset Point，继续保留原优先级、形状与模式预算；不新增二维Token或运行时flag。

理由是当前不存在可靠的二维语义映射，猜测(-v,v)等会引入新功能/默认漂移。Reserved不是豁免：正式token文档明确历史无消费者、替代入口、范围与复核条件，并交具体审查。以后只有明确轴向/模式/Native优先与兼容演进方案、有效反证和原外观/真实目标验收齐备，才可版本化启用；不能靠本提案永久忽略违约。

代价：这两旧字段本批继续不产生视觉变化，不能承诺8/8 Active；其余6 Active逐目标验证。若本提案不获批准，仍保持未决，不把默认迁移或本地事实测试当裁决批准。完整FloatButton/CT15也不因本提案关闭。


限定批准登记：2026-10-31，用户明确回复“批准65对及两项Reserved，其余保留”（request_user_input_async / call_e7be1ded2cd04cea8d90b50314dba528 / item 0）。仅FloatButton表内65对明确静态（含列明原窄宽换行/窗口上缘裁剪）与CircleBadgeOffset/SquareBadgeOffset两项Reserved兼容分类获批准；两字段仍无scalar消费者，不改写成Active/ConsumerVerified，不缩分母。NeedsDecision由5降为3，仅余GroupBox三字段。其余115旧新非零、34跨运行非零、恢复/平台/GC/极值及完整FloatButton/CT15保留，CT12/13/14既定批准和CT25/FW1不变，不授权提交。
