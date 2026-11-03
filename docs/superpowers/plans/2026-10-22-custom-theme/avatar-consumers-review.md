# CT15 Avatar 作者独立自审（2026-10-31）

本审查遵守用户单agent约束，为实现后另一步作者自审，不冒称独立人员审查。不提交、不重写CT12/13/14、不放宽CT25/FW1、不修改官方依赖。最终执行数据另见[计划记录](avatar-consumers.md)，视觉批准和清理后验收未完成前不判家族通过。

## 代码与合同

- 分包分母3metadata/11Own；AbstractAvatar无独立片段，Avatar与AvatarGroup identity各自注册。V2R1只改默认求值协议，44默认/type/key/slot和实际原文字getter逐条保持。
- 11Own实际角色已逐项核对。TextFontSize三字段仍为Icon尺寸，未按名字改变默认文字。Circle随真实Bounds.Width，Square读Avatar owner Global，Group边框读Group自身Global；明确字体覆盖原继承，Native和声明式root Style须保持优先。
- 可由主题表达的Gap、Custom宽高/Icon输入、Circle半径已移入ControlTheme编译绑定及有限类型化converter。代码不持有新模板目标或复刻文字测量。Transitions未放入模板内容。
- 原图片controller/generation/lease/Reload/borrowed所有权未重写。SizeType↔Size归一沿原逻辑，SizeType relay先于Size修复初始Custom被覆盖；不新增Group尺寸状态机或旧Local值恢复系统。
- membership字典只跟踪实际成员，每项typed relay可dispose；remove/detach/fold均有对应退役。运行集合与Content跨模板边界的具体原因已写代码注释。原型指针、键盘、fold/pin和Flyout关闭入口复用现有实现。
- 打开overflow时snapshot47/真实宽32为有效RED；局部fold StackPanel复用已有ThemeContextResourceBridge。随Context更新、移除资源、dispose有对称路径。未修改共享Flyout/Scope引擎，也未给所有单Avatar添加bridge。保留旧fold内容时Group和Provider均回收。
- 无发布Semantic Part manifest的Avatar不造专用Part API，Frame/Icon/Image仍internal-observable，PART_TextPresenter保持稳定。观测探针只读，root定制沿原公开入口。

## 反证与边界

初轮10项5FAIL/5PASS、Group初始Custom72反例、Group Global厚度3反例及真实overflow热更新反例均保留。驱动命名空间/API编译错误不计产品RED。首轮浅宿主暗scope对比不完整，保存原批结果并修正承载背景后两侧完整重放，不挑帧或只重建一侧。所有像素比较完整同坐标RGBA，独立A/A/1DIP恢复0才有效。

真实OS后端、窗口chrome/输入、Browser、Popup过程时相及未执行的自然极值继续列为未覆盖；Headless Window与macOS NativeAOT进程不代替这些证据。完整Avatar和CT15未关闭。后续Tag的SelectTag继承消费者需复用CT13批准边界，不能借本包改写。

## 待执行出口

- [x] 最终承载面四算法旧新/AOT完整轨迹、零容差差异和关键画面审查。
- [x] 声明式root Style补验、最终成员0问题、初始化/纯移动指纹核对。
- [x] 18临时用例与所属源/媒体/外部驱动清理后精确模块、NET8、文档/CSV/总表同步。
- [ ] 工程/验证完成后只申请具体有意变化的限定视觉审查，不将未批准变化自动视作兼容修复。


## 清理后最终限定验收

Avatar的18临时/3源19202字节、1843取证、411文件外部driver及551文件原会话全部清理后，精确Controls Avatar 7PASS/0Skip（412ms）、Desktop AvatarGroup 13与Dialog内Group 1共14PASS/0Skip（1s）。NET8 Release覆盖Desktop及所引用Controls，0 warning/0 error（00:00:24.37）。未自行跑全量。最终短期日志目录4文件在读数写入本报告后删除；无Avatar临时源、会话/媒体或本次进程残留，Tag进行中临时验证单独归属下一包。

本包18临时+既有14+Controls7共39项匹配验证；managed/AOT各100检查。5源/2Control成员0问题，AbstractAvatar纯移动及初始化指纹保持。四正式文档、CSV 3meta/11Own、总表及作者另步自审同步。仅48对有意变化已获限定视觉批准，所有closed/往返微差、真实OS/Browser/时相及完整Avatar/CT15仍未通过或未验证。无提交授权。


限定批准登记：2026-10-31，用户明确回复“批准表内48对，其余未通过与完整CT15保留”（request_user_input_async / call_7b292d7ef454420fb8fc0603ac3e3f4c / item 0）。仅本表48对有意变化获批准；closed/往返微差、OS/Browser/Popup过程时相及完整Avatar/CT15继续保留，不授权提交。
