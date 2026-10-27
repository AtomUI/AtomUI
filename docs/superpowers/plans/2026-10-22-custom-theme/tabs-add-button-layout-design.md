# CT14：卡片Tab添加按钮尺寸恢复短设计（设计已批准）

本片由TabControl V2依赖的原场景比较发现，独立于34字段迁移；正文保留批准前的原始取证与设计，当前实现及验收见[本机结果](container-tabs-defaults-results.md)。它不是把5320／5259像素默认差异归为噪声的申请。

## 实际证据与合同

CardTabControl与CardTabStrip，Top／Bottom，Width310、Height190、12个item、SelectedIndex1、添加按钮开启。两个独立scope只改变前史：先CardPadding16,8或16,16，最后都设CardSize56／CardPadding3,4,5,6。同版本四项临时检查均FAIL：最终所有item的Bounds／DesiredSize、ItemsPresenter的Bounds／DesiredSize均相同，presenter期望646×29，但添加按钮Height分别残留35／51，Bounds与DesiredSize也不同。原始尺寸、overflow、内容及按钮全部保留。

现有两类控件在owner ArrangeOverride及placement／CardSize回调中读取presenter.DesiredSize再写按钮Width／Height；item padding导致presenter重新测量后，owner未必再次执行该同步点。最终输入相同却依赖前史，证明是尺寸同步所有权缺陷，不能靠延迟、强制重绘、移除height限制或隐藏添加按钮规避。

保留UX：四placement、viewport／extent、双向overflow／shadow、内容／标题／extra内容、12item、拖拽／关闭／增加事件、选择、尺寸与现有根圆角；添加按钮继续可见可点，不新增公开API、Token、Part或订阅。

## 推荐实现

- 将添加按钮尺寸／对齐关系移入两个现有ControlTheme的placement Style Setter：Top／Bottom Width跟随实际CardSize、Height跟随同模板ItemsPresenter.DesiredSize.Height；Left／Right继续CardSize正方形及原Right／Left对齐。
- 同模板NameScope可表达presenter绑定，优先使用编译／模板绑定，不在代码新增订阅或relay；删除C#重复写入及仅用于此关系的presenter字段。图标点击处理和现有圆角状态保留。
- placement Style退出时回到该方向的声明值，避免从垂直切回水平残留旧的local对齐。需要保留的容器LocalValue绑定按实际优先级／Content树边界核定；能等价移入主题者同步迁移，不能者写明具体原因。
- 代码LocalValue回写去除后，专用Add部件Style的尺寸Setter可以按正常Style优先级生效；此可观察优先级修复包含本设计，验证只使用生成专用Style的AXAML，不查节点回写尺寸充当定制。

其他方案：继续在Arrange中加刷新／延时不能保证对子树测量的响应；新增DesiredSize订阅可以工作但需额外生命周期且违反可表达关系的AXAML优先约束，因此不采用。

## 实施与验收

API等级L2：改变动态padding／方向往返后的按钮尺寸恢复及Style优先级，需批准后实施。先复用四项有效RED，再测四方向／三尺寸、CardSize0／明确下游、主题切换／重挂／模板替换与声明式专用Style覆盖和恢复；保留actual viewport／shadow与输入命中，不改成固定截图布局。

实际明暗完整历史作自然A/A、1DIP、新旧／恢复，预期可见变化另交视觉审查；输入／AddTabRequest、当前Mac managed／NativeAOT、临时清理后的Tab精确283项／NET8及完整成员审查分别执行。设计不批准其他Tab字体依赖、噪声、Git、其他Host或完整家族；不创建分支／worktree，不发布NuGet。

用户已明确批准推荐短设计；正文保留实施前证据，最终视觉和Git另审。

实施核对：DesiredSize路径没有所需通知，水平绑定实际采用同模板ItemsPresenter.Bounds.Height；编译路径放在模板内Style Setter，保留NameScope以支持高优先级Style暂停后的恢复。原29断言、三尺寸/四方向、重挂/模板替换及GC均验证，所示最终布局与N01–N16精确自然差异于2026-10-27单独获批；直接原生Styles.Remove仍是未闭合失败，Git没有随视觉批准自动授权。
