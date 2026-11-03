# CT15 DotBadge 仅阴影颜色热更新

2026-11-02，原137metadata/274Own范围内的DotBadgeIndicator消费者缺陷；不重新迁移默认，不扩大既有Badge48对批准、CT12/13/14或FW1/FW2。当前单独保留本缺陷证据，矩阵/CSV/总表只引用。

## 有效反例与最小修正

真实DotBadge/ThemeConfigProvider/AtomUI Window，正常motion、原文本；Own固定DotSize24、ShadowSize4。初始ShadowColor=#FF00FF00后只把此Own改为#FF0000FF，同一Indicator和尺寸保持，目标有效ShadowColor已为Blue，但实际阴影中部像素仍Green。临时精确用例1FAIL/2s，失败位于真实蓝色像素判据，不是字段或尺寸问题。

原因：DotBadgeIndicator.OnPropertyChanged已重建颜色对应的BoxShadows，但静态AffectsRender只包含点颜色/阴影尺寸，遗漏BadgeShadowColorProperty。只补这个属性的绘制失效声明；不加入额外重绘、事件/订阅、资源、缓存策略或API，不改变默认、尺寸、裁剪、内容、键盘和原show/hide motion。

同一精确反例修后1PASS/2s。原Release AtomUI.Controls.dll已保留到本会话外部临时目录，SHA256 `0ba8582f779c0dcb2954482cda5e61b1ea0d7f6f2f2180d76f9e003605cc7af8`，仅用于后续相同驱动旧新对照，不改工作区源或共享缓存。

后续8参数条件：四算法×独立/DecoratedTarget，保留原Green→Blue前缀，补透明、清除到默认、再次着色及离树改色/重挂；与Badge原模块合批验证中，不提前记通过。当前临时测试与会话尚未清理；新源码的NET8、必要AOT/绘制与清理后精确模块仍待完成，不据单项绿关闭Badge或CT15。

扩展结果：8临时参数条件与原Badge32共40PASS/0FAIL/0Skip、52s；原相同Green→Blue单项另1PASS。八临时条件已删除，不新增永久测试。一个共享参数驱动负责四算法×两承载模式的旧/managed/AOT与独立无配置默认对照；采用原Release Controls DLL的外部副本作旧行为，不改工作区源/共享缓存。当前驱动构建与后续集中采样进行中，不提前计AOT、清理后门禁或完整CT15通过。


## 集中验证、清理与作者自审

同一个冻结参数驱动重放四算法×standalone/target。旧Release Controls副本96检查72PASS/24绘制FAIL，managed/AOT各96PASS/0FAIL。三个版本各64帧；64跨运行整帧RGBA全0，16张无配置默认旧新0。24对预期修正每对439或440像素，最大差235或255，见[限定表](badge-shadow-color-visual-review.md)；其余Green/重挂Blue各8对0，两Compact standalone Green往返各1/max1仍FAIL，不归FW1/FW2或增容差。

AOT23005984字节，SHA256 `4ce903b369d1d9127e66332c168953bce562efc787ce970f76e985ffb8328639`；实际执行exit0。0 IL/CS警告，五条既有Homebrew链接最低OS警告单列。驱动首构建遗漏Desktop扩展using的CS1061已修正，不计产品RED；未改官方依赖或发布参数掩盖警告。

8临时参数用例/1源已删除；193取证经官方audit/cleanup clean/findings=[]后删除。独占共享产物锁下，清理后真实重建Badge32PASS/0FAIL/0Skip、826ms；最终源码Desktop NET8 Release零警告零错误23.10s。所有正式文档、两CSV相关2metadata/1Own引用同步，分母/分类不变，无新增永久测试或全量。

作者另步自审：本轮只在现有自绘输入声明补BadgeShadowColorProperty；原其他未提交成员整理保留。没有新成员/事件/订阅/资源/字段顺序、API/默认/语义Part变化；ControlTheme不能声明自绘失效元数据，未新增代码绑定或定制回退。Count通过Border.BoxShadow更新、Ribbon已有相关AffectsRender，不类推扩大本修正。原2张恢复微差和完整CT15不关闭。外部会话及marker在最后记录后清理。

本表24对有意修正已获限定批准，原话/请求ID/排除范围集中登记于[视觉审查表](badge-shadow-color-visual-review.md#限定批准登记)；完整Badge/CT15不因此关闭。

正式采用后原路径重放：当前严格结果集中于[采用结果](feedback-region-sdk-adoption-results.md#原恢复合批结果正式采用后)，旧FAIL保留历史，本轮未覆盖不自动转通过；清理与最终精确门禁集中执行。
