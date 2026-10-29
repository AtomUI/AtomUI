# CT25-FW1：Avalonia 恢复语义源码验证结果

用户批准[主体计划](framework-restore-validation-plan.md)及XamlX固定子模块例外后，使用既有Avalonia参考目录验证了实际12.1.4源码候选。正式AtomUI依赖和NuGet缓存程序集未变，没有提交、推送或发布NuGet。

## 源码与范围

基线为`2d79ba55fae1b1e71c69f93a668080c9029e6a31`，与当前包InformationalVersion相同。只修改两个Base文件：

- EffectiveValue泛型实现：分别保存coerced base及uncoerced base；同优先级时两者复用同一次coercion结果，保持只调用一次callback。
- StyledElement.DetachStyles：按既有Styling/InvalidateStyles的继承作用域清除指定来源Frame；先快照整个作用域再通知，避免父回调摘下子树后漏清孙节点，以及子回调改变列表导致遗漏/越界。无全局缓存或找部件改属性。

最终两文件源码patch SHA256：`74e2a2066a92fd4ae6c97d242936889f8618c32b0df84bb35ad85285267633f0`。这是供审查的源码候选，不是已经发布的依赖版本。

## 有效前后与保护

| 阶段 | 实际结果 |
| --- | --- |
| 原始源码14项 | 11 FAIL / 3 PASS，0Skip |
| 只修coercion | 5 FAIL / 9 PASS；余下均为Style退役失败 |
| 第一轮组合候选 | 原14项通过 |
| 同优先级新增保护 | 原版与两赋值候选都把base留为未coerce的150，期望100；补复用单次结果后Local/Style/Template三项通过 |
| bool/string/TimeSpan/DateTime | 原版4 FAIL，候选4 PASS |
| 父回调摘下整棵子树 | 逐节点快照候选仍保留孙节点Tag=owned；改为通知前完整作用域快照后通过 |
| 最终两个原类加临时检查 | 69 PASS，0 FAIL，0Skip |
| 删除三份临时测试并重建后 | 两个既有精确类47 PASS，0 FAIL，0Skip |

全部保留原始失败和准备错误。新增保护没有改期望或删除触发条件；数组/作用域只在退役调用中临时存在。

## 产物身份与API

net10/net8 Release实际构建均通过。候选与当前对应包的程序集身份、目标框架及16,148条公开/受保护API记录逐条相同，delta=0；包括参数、泛型约束和属性元数据。该结构比较不代替行为验证。

## 实际AtomUI消费者

外部临时消费者只在自身MSBuild运行时资产列表中选择源码Base；实际复制SHA和ILC response的Base路径均核对。最初仅HintPath会回落到原包，身份守卫明确拒绝，未计作通过；之后仅调整消费者SDK资产选择，未改主工程或缓存。

真实Mac NSWindow、scale2、明暗、两类Card、310×190、12item、SelectedIndex1、四方向及返回Top：managed152项、NativeAOT152项均通过。生成专用Add Style的91×37覆盖、直接Styles.Remove恢复56及当前header高度、重加与失活恢复均验证；没有代码回退定制。

原NavMenu作为负向对照，源码候选下managed/AOT仍仅记录终点300，没有48与300之间的中间Bounds，返回非零。框架修正不能代替产品取值算法修复，不计作NavMenu通过。

发布使用现有openssl/brotli本地链接目录；macOS26 dylib与最低链接12警告保留，不宣称macOS12验证。没有采集最终屏幕像素，也不把资源/布局检查称作原生像素或raw输入验收。

## 影响追踪与尚未关闭的门槛

直接coercion/CoerceValue/GetBaseValue搜索为31文件/55引用；GetBaseValue实际用于NavMenu目标宽度、Segmented滑块基准、两类Tab指示条基准。多类型恢复和通知保护验证通用机制；其余控件的实际连续动效、动态Token更新、所有Host与性能仍须随家族完成，不把候选搜索或69项检查当作全控件兼容验收。

2026-10-27查询NuGet公共版本列表，最新列出的稳定和预览版本均仍为12.1.4：[包页面](https://www.nuget.org/packages/Avalonia)。当前没有选定可信的正式修正版依赖；正式接入方式必须另审，不能偷偷替换同版本缓存或把临时二进制诊断发布为产品修复。

## 恢复与清理

- 参考checkout已恢复原detached`8eeda4f6f546165b3f72e63c9f42247abb306905`，工作树干净，原两个文件SHA一致。
- 三份上游临时测试删除，常驻测试净增0；XamlX恢复未初始化，新增212个源码/Git文件、1,069,358 bytes全部删除，没有研发分支refs。
- 外部探针、编译产物和工具共1670文件、1,639,142,548 bytes全部删除；相关进程结束，没有媒体生成。
- 当前NuGet net10/net8 Base原SHA保持不变。完整短日志、API记录、ILC输入和清理receipt在`.superpowers/sdd/framework-restore-validation-plan/`。

本地源码候选验证与恢复完成；正式依赖接入、NavMenu产品实现、各调用方完整兼容及CT14/CT25最终验收仍开放。当前只有文本patch与结果，不存在新的Git或发布授权。

用户后续决定保持官方12.1.4、先推进其余任务（call_a335ae73ab804ea487b074c65af86db9:0）。因此不接入源码构建或替换同版本依赖，FW1临时源码操作及XamlX取得授权均已执行、清理完毕；相关框架恢复问题继续保留，不能把本地候选通过写成正式产品修复。
