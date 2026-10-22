# 自定义主题问题与优化映射

整体目标和工作包见[方案入口](overview.md)，具体运行证据要求见[交付与验证](delivery-and-verification.md)。原始事实由[审计问题清单](../../audits/2026-10-22-theme-customization/findings.md)拥有；本文把问题转换为工程处置，不将静态风险改写为已复现Bug。

## 1. 问题处置原则

P1优先处理影响整条定制链的根因；P2处理局部消费与能力；P3处理工具完整性和待决合同。优先级不等于可以跳过P2/P3的最终处置。合同不明确时先判断Active、Derived、Legacy、Reserved或Deprecated，不能为了消除字段直接发明功能或破坏schema。

源码确认、运行失败、修复完成和验收通过分别记录。保留/范围外必须有语义或平台理由；“当前没有引用”与“没有时间处理”不构成已完成。

## 2. 主要根因、规范及优化

| 问题 | 已掌握的证据 | 规范 | 优化与工作包 | 必需验证 |
| --- | --- | --- | --- | --- |
| owner绕过 | S01：Button颜色状态读Effective Global，尺寸/边框等仍读Shared；输入共享框体、Select、可选包存在同类链路 | TH-02/05 | W2先明确公开owner并修共享消费，W3核对所有家族 | EVT-02，Global与identity覆盖同时存在，兄弟和public子Control不串值 |
| 静态动态值 | S02：SharedTokenResourceValue是普通值读取，已有Transition不自动随scope/主题更新 | TH-06/09 | W2验证可更新与可释放的owner-aware动效输入，Transitions保留style setter位置 | EVT-05/09/10，已创建实例和弹层更新、关闭可释放 |
| 别名断链 | S03：DataGrid先复制17个公开语义字段到Table*，覆盖后消费者继续读取旧别名 | TH-03/04/05 | W1/W3让生产消费接规范值，旧名规范化并诊断冲突；导出字段保留兼容边界 | EVT-03，公开值覆盖到实际表头/单元格，旧名兼容、冲突可诊断 |
| 默认依赖断链 | S05：按钮行高→padding、Skeleton颜色→brush、ColorPicker尺寸→滑块提前派生 | TH-03/04 | W1版本化有效上游求值，W3迁移完整终端链；不再执行一遍旧整体计算 | EVT-01，上游覆盖改变默认下游，下游显式覆盖保持 |
| 公式/尺寸错配 | S06：OptionButton档位和量纲错配；V25：58处Custom/Middle候选 | TH-03/10 | W3修正确公式；W2/W3按属性分离基础默认和三档预设，Custom退出受管维度 | EVT-06，三档与Custom、方向、字体、外层约束和交互 |
| Own没有正确消费者 | T01–T37：关闭图标、选项padding、弹层尺寸等未消费，也包含保留/历史语义项 | TH-04/05/12 | W0分类，W3逐Active Own接到真实节点；保留和弃用有文档及编辑器边界 | EVT-01/03/07/08，逐项证据，不能以少数有效字段代表全部 |
| root没有到达框体 | L02：LineEdit公开框体参数未投射至实际AddOnDecoratedBox | TH-05/06/07 | W2/W3修root/frame映射，必要稳定Part先审定，保留正确brush中继 | EVT-04/07，实例与专用Style同时覆盖，addon/compact/校验态不退化 |
| 本地值压制定制 | L01/03/05：Button、Timeline、CircleProgress在布局中写用户属性或Part外观 | TH-05/06/10 | W2/W3把默认和派生输入放回主题/内部计算，动态形态无残留 | EVT-04/08/09，Round→Default、显式字体/边框/半径仍生效 |
| 缓存和排版更新缺口 | L04/06/07/09/10：默认色固化、箭头geometry、Rate字体、Gradient文字和失配selector | TH-05/06/09 | W2/W3完整属性投射、正确selector和依赖失效；代码例外说明原因并释放 | EVT-05/08/09/10，值、几何及最终像素随已有实例更新 |
| 实际绘制能力不足 | V01–V24及L08/11：颜色、线宽、形态、排版、波纹等存在固定输入或Setter未消费 | TH-05/07/09/10 | W3按Global/Own/公开属性/Part分配稳定语义，自绘实际读取；固定形态单独决策 | EVT-06/07/08，测量/命中/绘制协调，不能只开放无效Setter |
| 完整配方与工具缺口 | G01–G04：现有Gallery主题主要覆盖主色/链接色，缺完整编辑和往返表达 | TH-08/11/12 | W1先验证品牌装配，W4完整配方，W5复用现有工作台设施并补编辑/导出，AI最后接入 | EVT-05/09/11/12，嵌套、弹层、导出等价及故障语义 |
| 工具壳与内容语义 | G05–G11：Gallery密度、插画、命名色、平台与目录字段等 | TH-04/05/10/12 | W3/W5按工具owner、内容资产或平台合同分别处置，不把全部SVG色板Token化 | EVT-06/08/11/12，实际适用输入与清晰支持边界 |

完整逐ID安排见[90组处置矩阵](problem-matrix.csv)。矩阵包含受影响Control或内部目标、原始源码位置、规范、优化和验证；所列内部目标不自动拥有Token identity，实际公开owner仍由W0核定。每行对应一组审计条目，不等于一个Control；同组内所有Control、属性和分支仍须在W0/W3展开验收。

## 3. 四类定制入口的优化规则

| 输入 | 负责什么 | 消费要求 |
| --- | --- | --- |
| Global | 通用品牌、状态色、排版、尺寸、圆角、描边、动效 | 应响应Control覆盖时读其Effective Global；刻意全局共享有依据 |
| Own | 控件独有稳定设计语义 | 每个Active字段有消费者、默认依赖、范围与更新责任 |
| 专用Part Style | 稳定视觉区域的精细Setter与条件样式 | 生成Style命中真实节点且不被默认本地值压制 |
| ControlTheme/允许的Part Theme | 明确开放的结构与完整视觉替换 | 保留行为和Part兼容合同；public子Control仍保留自身identity |

代码和自绘属于消费者，不构成另一套活动主题。模板能等价表达的关系放AXAML；运行计算保留在代码时说明原因、失效和释放边界。

## 4. 两项需要先验证的基础机制

### 4.1 有效Own求值

目标为：显式下游覆盖优先；未覆盖的默认下游基于有效上游、Effective Global及解析完成的Control appearance计算。

采用显式默认函数/依赖元数据和生成式顺序求值的提案，不能从任意C#方法体自动推导。需保持slot/schema规则、跨程序集继承、旧evaluator的独立版本路径、第三方迁移、惰性和缓存修订。实施前以Button、Skeleton、ColorPicker及明暗分支证明合同。

### 4.2 Token与品牌样式共同生效

目标为：框架托管的品牌样式选择与Token属于同一请求，局部重置不残留父品牌规则，已打开Native/Overlay按owner同步；普通应用Styles维持Avalonia原生合同。

正式[品牌设计](../../../architecture/systems/theming/brand-recipes.md)中的owner资格与候选样式准备需要先证明：继承规则重新资格化、选择器优先级、资源不提前泄漏、状态/模板更新、接线及退役异常有明确结果。可恢复接线/预激活放在Token提交前；**现行Token事务CommitCore静默、Publish不可回滚的合同保持有效**。Token与表现结果分别报告，不能把已有Committed改成普通Failed。

这两项原型不过关时，停止相关批量迁移，修订机制规格；不能把问题转移到每个控件的补丁或强制页面重建中。

## 5. 必须制定和同步的规范

[全局规范正文](../../../engineering/development/theming/overview.md)和对应模块的正式设计已经同步，源码符合程度单独记录。需要按以下所有权持续维护：

1. 总纲：范围、规则用语、唯一事实所有者、状态与例外流程。
2. 基础设施/Token：identity、分阶段覆盖、求值版本、分类、别名、范围、scope、缓存、错误、AOT与资产边界。
3. 消费：真实视觉投射、Shared选择、AXAML、原生优先级、Part、自绘、Custom、排版、动效和动态释放。
4. 验收：每个Active Own、派生边、Part route、状态/尺寸、已有实例与跨根、原始UX、平台和测试去留。
5. 专题正式合同：需要改变runtime、继承、Semantic Part或格式时只修改其正式所有者；不得让工程规范和架构长期冲突。
6. 控件与工具资料：overview/implementation/token/semantic-part/changelog、Gallery API/Token/ShowCase与指南同步。AGENTS/skill只做必读路由，不复制完整规范。

## 6. 处置完成条件

每组问题要么有失败→修复→通过及最终模块证据，要么有明确的兼容保留、语义分类或平台范围决策。临时测试按规范清理；长期机制保障按独立价值保留。当前矩阵全部属于计划处置，未填入运行通过。
