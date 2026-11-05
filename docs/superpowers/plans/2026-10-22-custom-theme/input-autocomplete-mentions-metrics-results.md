# CT13 AM2：候选最小行高与 padding 高度依赖结果

状态更新：最后五表已于2026-10-30获用户明确限定批准，CT13既定范围已完成；下文原始执行/失败记录保留，CT25/FW1继续独立。见[批准汇总](input-final-visual-review.md)。


2026-10-30，现有feature/custom-theme，单agent、无分支/worktree/提交。实现、专项与同历史绘制已完成，临时清理/最终模块均完成，[限定视觉](input-autocomplete-mentions-metrics-visual-review.md)待用户审查；完整CT13不关闭。

## 原始失败与根因

AM1原生扩展断言先记录真实PopupFrame.Padding2、MaxHeight96/期望100。两个root的OnPropertyChanged只观察ItemHeight/DisplayCandidateCount，padding初始化及热更新没有触发既有预算公式。OptionHeight仅用于该预算，真实候选行仍从ListBoxItem主题得到32，配置48后显示四行多而非名义三行。

六项有效RED、1s：两个root默认预算100实际96；只热更新padding10的预算116未重算；Own行高48的真实row仍32。保持原标题、宽度、8数据、DisplayCandidateCount3和高度约束，没有减数据、移除overflow或关闭输入。

## 独立结构阶段与检查器精度

修改C#前强制门禁原报61条。工具把setter里所有字段引用都当backing，误将FilterValue的guard `_allowWrite`和计算属性输入认作backing；真实`_filterValue`本已相邻。

MemberLayout.cs只修正识别：优先SetAndRaise的ref目标，再识别简单getter；不把guard/派生输入搬进属性合同。三个临时CLI fixture原8告警含6误报，修正后只有故意错放真实`_value`的region/adjacency两条，规范未放宽。

其余59真实布局项在单独阶段整理：AbstractAutoComplete217、Mentions187，共404个成员；全部非trivia指纹、using、类型header/enum相同，初始化字段序列逐项相同。没有拆文件/改API；纯布局后既有26项通过、仅六个已知指标反例失败。行为阶段后比较仍只有两个OnPropertyChanged的方法体改变，其他402成员相同；最终门禁2文件/2Control/0问题。源码移动不是行为修复的替代证据。

## 最小消费者修复与优先级

两个现有属性回调补PopupContentPaddingProperty，继续唯一ConfigureMaxPopupHeight公式。父主题只对模板内CandidateList的现有ItemContainerTheme提供既有CandidateListItemTheme实例，四尺寸trigger将对应Own.OptionHeight送入MinHeight，保持原行模板、active/selected颜色、代码数据与事件。没有新ControlTheme全局key、属性/Token、C#字段/helper、状态旗标、timer或强制重绘。

既有root→当前Popup的pinning RelayBind保留并补具体原因：它拥有替代/自定义模板实际部件的打开前行为及本地优先级，默认主题Setter不能等价保证所有替代部件；重模板前释放旧绑定、离树关闭并退订外部事件，同owner/part绑定用于重挂。四外观的实际pin-priority反例确认root业务值仍优先，未以主题重构改变此行为。

六反例转绿；扩展12项1s全部通过。四外观实际文本Alpha0保留，row48、viewport144/extent384；左5%/中50%/右90%真实PointerMoved的Source均在row子树，hover与candidate态正确。inner wheel真实offset>0；padding更新到10后预算164、viewport144与offset保持，行高56后viewport168且不重置滚动。8数据、输入值、0与清除保留。本fixture无outer滚动，不虚构chaining验收。

四编译AXAML专用PopupListItem Style锁定生成类型及MinHeight60，覆盖/热更新/撤回到当前Own52都真实命中；code-behind仅加载AXAML，无Name/Loaded/Unloaded节点定制回退。有限真实帧/时钟推进后12个owner/frame/row图回收，无新增非Visual资源宿主/应用订阅。

## 视觉与当前Mac

三AutoComplete外观+Mentions、明/暗局部Provider、650×740、原8数据/三名义页/宽320、非聚焦输入、正常motion。同实例完整历史各29阶段，116张/58对：10对无popup输入本体/AA/检错复位/关闭0；48对为default、Own48、padding10、Global48/间距8、0和默认恢复的popup变化。每对数值及实际观察见[限定表](input-autocomplete-mentions-metrics-visual-review.md)，未设容差。

默认预算96→100、viewport92→96，完整三行32；Own48进入真实行后普通均匀场景三行。大字体/复杂模板或更大的专用行Style可以超过名义最小值，不承诺任意定制都精确三行。0不再保留旧8高度窄条；原数据与业务popup状态保持。原生最终像素/raw输入/其他平台不据此批准。

同一原生预算断言保留，修复后通过。当前Mac managed/NativeAOT各543检查：基础463包含两真实Window/PopupRoot后端、旧字面ID/七项值、非均匀padding/小数/0/清除、实际Frame min/max与页数预算；补充80项覆盖四外观两后端实际行高度/文字及编译专用Style形态/覆盖/撤回。没有通过过滤预算失败取得PASS。

AOT无IL2026/IL3050，五条本机OpenSSL/Brotli目标部署链接警告保留。NET8 Desktop Release含依赖0警告/错误17.52s。联合AM1协议/快照与AM2功能临时18项2s通过；70条原版值/类型/key/ID/slot在联合源码仍相同。AM1隔离22对0仍是Token协议阶段证据，不拿它代替本包新视觉审查。

## 清理与最终验收

全部新增验证默认临时，常驻净增0；保留既有AutoComplete/Mention/CandidateList数据、交互、模板和Semantic测试。正式两控件implementation/token/changelog同步。联合清理已完成：AM1三临时源13,934 bytes、AM2五临时源22,339 bytes、3个checker临时fixture、重排脚本及原版源码备份均删除；共同原生工程427文件（1,216,025,157 bytes）删除。AM1的44PNG/manifest共45文件与AM2的116PNG/manifest共117文件均清理，两个audit clean。CT12六个临时源未触及，无本包进程。清理后精确AutoComplete/Mention/CandidateListBehavior共28PASS、0FAIL/Skip、1s，Generator默认合同1PASS1s；最终成员门禁4文件/2Control/0问题，git diff --check通过。

作者另步自审，未用独立agent；无新API/Own/key/route、无提交/推送。字体、其他Global/Own/状态、完整家族和CT25仍开放。
