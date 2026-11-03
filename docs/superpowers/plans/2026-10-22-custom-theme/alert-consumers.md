# CT15 Alert 消费计划

沿既定目录/分支单agent执行，不创建隔离、不提交。范围为Alert 1 metadata/10 Own，不等于Card或整个CT15完成。P3-A限定批准保持；P3-B的Part退出与周期端点原始未通过继续，不重开CT12/13/14，不改依赖或扩大容差。

## 审计与合同

Alert负责Type四状态、Message/Description、ExtraAction、CloseIcon和两种标题。IsShowIcon/IsClosable/Marquee切换已有静态节点；CloseRequest只通知，不隐藏或移除Alert。主要状态owner为Alert，close的Click在re-template先解除旧订阅；资源由原生Visual宿主管理，新增字体只复用Core只读继承桥与AXAML Setter，不新增运行期旗标或C#样式绑定。

T23：CloseIconSize和MessageWithDescriptionMargin资源存在但无消费者。旧关闭图标读Shared.IconSizeSM，旧标题Margin为0，描述另有DescriptionLabelMargin。二者采用明确Own才启用的OptionalOverride，未配置保持原实际几何；资源默认、名称/type/key/slot保持。明确0有效，清除回旧默认。MessageWithDescriptionMargin只作用有描述的Label/Marquee标题，专用Title/Close Style仍优先。

其余8 Own保持原消费者；ExtraElementMargin实际是close左Margin，不扩成额外action布局的新语义。10字段迁V2 Revision1，各字段只读取原Global/常量，无Own依赖边。真实Control Global取Alert owner，模板内图标/Label/Marquee/Close的组合输入不误用子公开identity。字体族、正文root字号和前景使用既有可选继承机制；标题继续原FontSize/FontSizeLG角色，原生本地root值和专用Part值保留原有优先级，不创造SizeType。

旧源码完整重放另外确认：icon专用Style的Tag命中，但FillBrush被模板属性资源的Local绑定覆盖。四个图标绑定明确使用Template优先级；这是原有属性优先级合同缺陷，不是SelectorRoute未命中。原/新完整驱动均756检查，旧272失败、新0失败；其中32项是该图标颜色对照。

公共API/事件/Part/伪类/模板key不变；不拆文件，不新增产品helper，复用已有可选double存在位选择。成员布局与初始化顺序需审查。Gallery原六类专用Style与API/Token自动表形态保持，必要语义更新落正式token/implementation/changelog。

## 执行与验收

- [x] 临时失败验证位于Desktop Temporary/CT15Alert：两个Own实际consumer、四状态Control Global、根字体/清除/原生优先级；已有8 Own作正对照。迁移前后四算法40默认/type/key/slot逐项比较。
- [x] Theme先行接线、只读继承桥和10默认V2；0/清除/兄弟隔离、短标题Marquee两分支、六专用Part、真实close/action输入、disable、重模板/重挂与GC。
- [x] 真实Skia旧新默认/已有定制/配置/清除、A/A、1DIP、正常帧循环；长Marquee持续路径单列，不冻结循环当静态画面。当前Mac AOT运行，其他平台/NSWindow边界分别保留。
- [x] 删除临时源/媒体/外部工程，精确Alert模块最终验收、NET8、成员/作者另步自审及正式文档/CSV/总表同步；新配置生效的具体视觉改变提交限定审查，不询问是否继续。

34项（25临时+9既有）已通过，包含10默认的有效Control Global输入、37真实Glyph及自然高度、动态字体/Marquee/关闭与回收。原固定Height180的八根矩阵保持，再补自然高度场景，不通过移除原高度消除任何差异。原始额外4对禁用/恢复的max1/2像素仍需定位，不自动使用其他包批准。

工程/清理/清理后9项及NET8零、成员2/1/0完成，见[结果](alert-consumers-results.md)。只有28对有意配置/Style变化进入[限定审查](alert-consumers-visual-review.md)，原始额外未通过及完整CT15保持，不将本清单解释成整族或全CT15已获批准。
