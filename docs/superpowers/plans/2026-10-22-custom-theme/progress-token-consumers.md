# CT-17 P1：Progress 家族 Token 消费分包

沿当前 checkout 与 feature/custom-theme 执行，不创建分支/worktree。已提交审查的34文件快照保持原字节，本包源、临时验证和结果单独记录，不纳入该提交授权。

## P1-A：既有13字段与两项悬空消费

- [x] 先以现有真实Skia/Headless宿主写有效失败验证，核定CircleTextColor与LineBorderRadius在配置／重置、disabled、StrokeLineCap和本地／专用Style优先级下的实际效果。
- [x] ProgressBarToken迁移DependencyDefaultsV2，保留13字段名称、类型、既有资源编号和默认值；10项有视觉语义，三个步骤／动效槽位按原默认0保留，不顺手启用新能力。
- [x] 四种公开控件保持各自identity和现有共享ProgressBar Own scope；本包不另造Circle／Dashboard／Steps的Own字典或复制schema。Global按各owner真实消费另验。
- [x] 圆形文本仅在CircleTextColor明确Own配置时投射，否则明确回退原ColorTextLabel；disabled规则与调用方Foreground仍由原生样式优先级执行。
- [x] 普通line仅在LineBorderRadius明确Own配置且StrokeLineCap=Round时投射，否则保留模板原1000胶囊值；专用Rail／Track Style继续覆盖。配置默认字段100保持原schema含义，不把其未配置值直接替换原模板1000。
- [x] 模板可表达的投射全部AXAML；WhenExplicit未配置会返回Unset，必须显式选择回退，不依赖其自动保留继承值。
- [x] 两主题同实例默认→配置→还原的完整同轨迹旧新、独立A/A和1 DIP检错；目标配置前后预期变化独立审查，未配置默认不得漂移。
- [x] 清理临时源／媒体／专用产物，运行精确Progress类、相关schema检查、NET8及触及文件成员审计，再整理本包结果与正式文档；净新增常驻case默认0。代码和204项／NET8检查、用户Own视觉批准、全部媒体清理已完成；既有34文件快照已获批提交，正式文档继续同步。

## P1-B：其余动态消费与形态

P1-A不代表完整Progress验收。继续逐项覆盖DefaultColor、RemainingColor、CircleMinimumTextFontSize、CircleMinimumIconSize、LineExtraInfoMargin、LineProgressPadding、两档LineInfoIconSize及四种形态的真实消费／失效、字体和用户约束、布局／测量／绘制缓存、Global与专用Style优先级、状态／尺寸／方向及原生显示面。尤其检查圆形min字号／图标只随尺寸更新、普通及步骤Line间距变更后的布局失效；修复前必须有原几何下的有效反例，不预先改产品。

三个旧步骤／动效槽位先保持原导出兼容与0默认，不能从编辑入口简单删除后宣称解决；是否启用原未公开语义及工作台如何标识，归后续明确处置，不冒充已接线。

## P1-B 首个可失败合同（准备）

先在既有Progress行为／语义部件用例中查重，使用Desktop临时宿主验证，保留原360宽line、96圆形及24间距，不以重建控件或关闭动画恢复配置。

1. 同一Circle／Dashboard明确提高CircleMinimumTextFontSize、CircleMinimumIconSize后，现有文字和状态图标须立即更新；清除后恢复，用户本地FontSize及专用生成Icon Style继续优先。当前源码仅Width／Height／EffectiveSizeType变化时计算，先取得RED。
2. 普通line及Steps的LineExtraInfoMargin／LineProgressPadding变化须使真实Measure／Arrange与内部rail、info同步；状态图标两档须与测量缓存相同。零值及原有非零配置均验证，用户MinWidth／MinHeight不被内部留位覆盖。
3. DefaultColor／RemainingColor在四owner的normal／active／completed／exception／disabled及本地画刷、TrailColor清除中按既有原生优先级投射；Shared Global更改及Own清除依赖重算独立验证。
4. 可由ControlTheme表达的字体／模板图标宽高投射放AXAML，代码仅提供尺寸计算值并说明原因；先界定兼容旧默认，再逐项落地，不以重绘或放宽像素门槛绕过恢复问题。

本节只细化既有P1-B范围，尚无实现或通过结论。原生最终显示面、完整动效和家族矩阵仍开放。

## P1-B3 信息适配与状态出口

[结果](progress-circle-fit-results.md)：44项有效旧版失败／候选通过，实际百分比与完整字体适配、激活图标Own／专用Style适配及Exception退出刷新已实现并获视觉批准。132帧中44AA为0、44DIP检出；附带圆弧117／126已用直接Skia定位并独立批准，保留真实计数。Mac managed／NativeAOT各130项通过，清理后42项与NET8、成员审计通过。普通默认及最终重置0，完整形态、Global／性能和原生最终显示面仍开放。
