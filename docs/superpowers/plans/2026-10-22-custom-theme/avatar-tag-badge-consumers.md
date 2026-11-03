# CT15 Avatar / Tag / Badge 实际消费者计划（2026-10-31）

沿指定原目录feature/custom-theme单agent，不创建分支/worktree、不提交。当前ToolTip/PopupConfirm正在旧新/AOT对照，仅提前只读核对本包；不重复已完成分包或以局部默认迁移关闭CT15。

输入Avatar 3 metadata/11 Own、Tag 8 metadata/9 Own、Badge 13 metadata/20 Own；Badge拆为Count11、Dot5、Ribbon4。真实源横跨AtomUI.Controls（Avatar、abstract Tag/Badge、motion/visibility/adorner）与Desktop（AvatarGroup、Tag/Checkable、三Badge），不能只扫Desktop目录。137/274原分母不变。

## 现有事实

- AvatarToken及实际AvatarTheme位于AtomUI.Controls；Desktop只补AvatarGroup。需要核对图像/文字/图标、文字缩放、三尺寸与Custom/Group重叠和overflow popup、真实owner/Native字体及图片生命周期，不能只迁Group。
- TagCloseIconSize仍NeedsDecision，旧模板close实际取Shared.IconSizeXS，而旧Own默认同为Global.IconSizeXS；先取得明确Own/0/清除的有效RED，核对默认、公开IconButton优先及关闭/键盘/取消事件，不能把未消费字段直接默认为通过。
- CountBadge两CornerRadius旧默认读IndicatorHeight/SM，应以实际Own依赖锁定圆角/明确值优先；其余18字段逐Indicator/Adorner/Text/Ribbon验证。保留原动画/数值格式/overflow/ShowZero/dot、原内容、计数输入极值、裁剪、祖先可见与重挂、color语义和资源退役。
- FloatButton13 metadata/8 Own仅复用CT11动效，不在CT12九owner完整出口；本包不顺带重做或关闭它。ButtonSpinner已获CT13批准复用。CT12/13/14结论、CT25/FW1原问题和官方依赖不动。

## 执行门槛

- [ ] 读各五正式文档/既有批准与实际metadata，将40 Own的默认/type/key/slot按四算法导出，保留名称与公式，先证真实反例。
- [ ] 临时验证放所属tests/Temporary，逐Own/Global/字体、原生请求、生成专用Part、状态/尺寸/Custom、弹层热更新与GC；跨模板可达绑定AXAML，运行期关系解释兜底原因并持有释放。
- [ ] Skia/AOT保留原内容/高度/scroll/clip、真实事件/键盘/焦点、RTL/Compact及代表极值；独立A/A/1DIP、同完整操作历史旧新/跨运行精确RGBA，无新容差。
- [ ] 清理临时源/专用工程/媒体后精确Controls+Desktop相关模块、NET8、完整成员及纯移动/初始化指纹，正式文档/CSV/总表与作者另步自审；构建/采样先取共享锁，不自行全量回归。需要视觉批准时先提供完整工程/验证后的限定表。

此为初始事实与执行顺序，不是Tag字段已裁决或家族已通过。先完成前一包清理/验收，再开始这里的产品修改。


限定批准登记：2026-10-31，用户明确回复“批准表内48对，其余未通过与完整CT15保留”（request_user_input_async / call_7b292d7ef454420fb8fc0603ac3e3f4c / item 0）。仅本表48对有意变化获批准；closed/往返微差、OS/Browser/Popup过程时相及完整Avatar/CT15继续保留，不授权提交。


2026-10-31 CT15 Tag：[结果](tag-consumers-results.md)、[26对限定视觉已批准](tag-consumers-visual-review.md)、[作者另步自审](tag-consumers-review.md)。8metadata/9Own全Active V2R1、36默认/type/key/slot保持；真实字体/owner/关闭glyph、container退役强链修复，84匹配验证、managed/AOT各120，164跨运行159零；原微差及派生输入大值影响保留，不改CT13批准。18临时/1090取证/外部driver与会话清理，最终66/NET8零、成员18-4-0。NeedsDecision余7，137/274不变；仅26有意帧已获限定批准，完整Tag/CT15不关闭。下一[Badge](badge-consumers.md)13metadata/20Own仅准备反例，未改源，无Git。


限定批准登记：2026-10-31，用户明确回复“批准表内26对及列明派生影响，其余未通过与完整CT15保留”（request_user_input_async / call_5cea4f7e59a243dcab027c42807d9848 / item 0）。仅26对与列明SelectTag继承close37/0导致的换行/原高度裁剪影响获批准；恢复、5对跨运行微差、原CT13批准边界、OS/Browser及完整Tag/CT15保留，不授权提交。


2026-10-31 CT15 Badge当前切片：[结果](badge-consumers-results.md)、[48对限定视觉已批准](badge-consumers-visual-review.md)、[作者另步自审](badge-consumers-review.md)。13metadata/20Own全Active V2R1、80默认/type/key/slot保持；真实owner字体/跨根发布/专用Style/实例颜色clear与投射退役修复。57所选56PASS、Count composition GC1FAIL保留；managed/AOT各72检查、164跨运行全零，4无定制默认旧新零，显隐/重挂及校准恢复非零不豁免。25临时用例3源26658字节、2524取证、419driver/15publish（原会话共537文件）已清理，清理后既有Badge32PASS/0Skip及NET8零警告零错误23.02s，成员22-7-0。NeedsDecision7与137/274不变；仅申请48对明确变化，完整Badge/CT15和CT25/FW1边界保留。下一按实际依赖推进ImagePreviewer/Carousel/Result，无Git提交。


限定批准登记：2026-10-31，用户明确回复“批准表内48对，其余未通过与完整CT15保留”（request_user_input_async / call_d223a18435b5484787082d74b8ee393e / item 0）。仅Badge表内48对有意变化获批准；Count composition GC1FAIL、显隐/重挂/校准恢复非零、其余108对及完整Badge/CT15继续保留，不授权提交。
