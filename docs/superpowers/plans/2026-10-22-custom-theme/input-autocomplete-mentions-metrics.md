# CT13 AM2：候选行高与 padding 高度依赖

> 使用superpowers:executing-plans单agent继续已授权CT13；不新建分支/worktree，不提交。AM1两Token迁移已取得2RED、70值/metadata不变及22对同历史0；其原生扩展断言暴露本包缺口，保持失败，不删断言绕过。

**Goal:** 让现有OptionHeight进入真实候选项最小行高；让PopupContentPadding变化实时进入原有行数高度预算。

**Architecture:** 复用CandidateList.ItemContainerTheme与现有CandidateListItemTheme，在父ControlTheme内声明Own行高，不新增全局主题key/属性或C#绑定。两个root现有OnPropertyChanged补齐Padding依赖，保持唯一MaxPopupHeight计算。原C#事件/选择/异步/弹层状态机不变。

**Tech Stack:** 当前Avalonia/AXAML、现有Token资源、.NET10聚焦、NET8、Mac managed/NativeAOT。

**Spec:** [输入目录](inputs.md)、[消费规范](../../../engineering/development/theming/consumption.md)、两控件现有Token文档。

## 源码事实与UX

- AM1原生实际PopupFrame名为PopupFrame、Padding2，MaxHeight96而公式应100。两root只观察ItemHeight/DisplayCandidateCount，未观察PopupContentPadding；初始化与热更新预算会滞后一份padding。
- 两root的ItemHeight仅参与MaxPopupHeight；CandidateListItem仍经ListBoxItemTheme读其自身Global.ControlHeight32。配置OptionHeight48时预算增加而行本身仍32，出现四行多。
- 保留四输入外观、原标题、宽度、八项、DisplayCandidateCount3、原高度约束、scrollbar/viewport/extent、输入/焦点/正常motion；不减少数据、去掉约束或加延时/强制重绘。
- OptionHeight为名义最小行高，普通均匀行按该基准形成名义页数预算；任意大字体/用户逐行样式/复杂模板不能虚称仍精确三行。0保持合法资源/预算语义，不把文字天然高度硬写成0。
- 默认padding2补入完整预算会产生96→100的具体默认变化，必须按实际画面限定审查，不借AM1无差异结论自动批准。
- 候选项定制继续编译生成专用Style，覆盖/撤回恢复当前Own；不代码取节点设置样式。

## 结构与生命周期审计

控制职责：root拥有文本/选择/异步/弹层和预算，CandidateList拥有active candidate及虚拟化，行主题只拥有视觉最小高度。

既有唯一RelayBind为root.IsPopupPinnedOpen到每次模板获取的实际Popup：代码负责任意替代模板部件的行为绑定、本地优先级及打开前初始化；默认ControlTheme Setter无法保证替代/自定义部件同等优先级与时序。替换模板前释放旧绑定，离树关闭popup并释放TopLevel/可见性/数据源订阅，保留同一owner/part绑定以支持重挂。新行高绑定目标为既有Visual行，资源生命周期仍由原生style持有；不新增非Visual资源宿主或应用订阅。

只读成员门禁：两root61条原始告警；其中DirectProperty的setter guard `_allowWrite`被误认作FilterValue backing，实际`_filterValue`已相邻。须精确修正检查器误报并用正/反临时fixture验证，其余布局在独立步骤整理，逐成员非trivia语法指纹与初始化字段序列核对；再做行为修复，不用重排掩盖逻辑变化。

## Task 1：真实失败与原版完整轨迹

**Files:** 临时`tests/AtomUI.Desktop.Controls.Tests/Temporary/CustomThemeAutoMentionMetrics/`，项目外媒体/工具fixture。

- [ ] 两root默认预算应100、padding仅热更新到10时应116；Own行高48应进入真实行、viewport144，先取得有效失败。
- [ ] 三AutoComplete变体+Mentions的明暗、普通/配置/清除轨迹；保持数据/页数/标题，AA/1DIP严格复位。
- [ ] 正高度overflow行左5%/中50%/右90%真实hover与实际hit、inner wheel/offset及原选择；本场景无outer滚动，不虚构chaining结论。

## Task 2：独立结构整理

**Files:** 两root C#；必要的MemberLayout.cs精确DirectProperty backing识别修复。

- [ ] guard/setter/ref与错放真实backing的临时checker反例，修正误报而不放宽规范。
- [ ] 仅移动成员/规范region，初始化字段顺序与所有成员非trivia指纹保持；不拆文件/改API或状态。
- [ ] 成员门禁与聚焦原模块验证，保留已知指标反例；记录纯布局与后续逻辑两阶段证据。

## Task 3：最小消费者修复

**Files:** AbstractAutoComplete.cs、Mentions.cs的Padding通知依赖；AbstractAutoCompleteTheme.axaml、MentionsTheme.axaml。

- [ ] 两个现有if加入PopupContentPaddingProperty，不复制预算值或新增状态字段。
- [ ] 父主题对模板内CandidateList声明现有ItemContainerTheme实例，复用CandidateListItemTheme，四尺寸trigger的MinHeight读取对应AutoComplete/Mentions Own.OptionHeight；不穿透公开子control模板写节点。
- [ ] 原反例转绿、0/清除、8数据/3名义行/可用scrollbar、hover/wheel、专用Style优先级与retemplate/recycle/GC。

## Task 4：共同专项与出口

- [ ] 保留AM1原生高度失败，修复后用同一断言通过，禁止删断言/改容差；当前Mac两真实Popup后端managed/AOT及NET8。
- [ ] 查看并记录默认+配置具体差异，正式文档/CSV/进度同步；清理全部本包与已完成AM1临时源/工具/工程/媒体/进程，常驻净增0。
- [ ] 清理后精确AutoComplete/Mention/直接CandidateList相关模块与Generator受影响检查；作者另步审查、限定视觉批准后只关闭这两个分包，完整CT13继续。
