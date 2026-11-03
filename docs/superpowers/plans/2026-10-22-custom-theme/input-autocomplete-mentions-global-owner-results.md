# CT13 AM5：复合输入实际 Global owner 结果

2026-10-30，现有feature/custom-theme、单agent，无新分支/worktree或提交。最终实现与本机取证在本文件记录；用户已明确批准本表限定视觉并保留CT25，完整CT13继续。

## 最终实现

四私有输入通过internal只读IControlThemeGlobalOwnerSource返回实际TemplatedParent。LineEdit/SearchEdit/TextArea主题的86个Global来源使用既有OwnerTokenResource，SourceControlType明确原所属输入，避免状态色误用模板内公开ScrollViewer身份；SearchButton主题四个组合输入也明确SearchEdit来源。真正公开Button自身键与共享Own不转投。

root Foreground主题默认与声明式Local模板投射使标准Style/Local进入实际正文；原状态selector与较近的Filled/Borderless错误/警告文字优先关系保持。Dark局部provider下64个初始root getter从原外层Light继承#e0000000对齐到原实际正文#d8ffffff，实际默认正文不变。没有新公开API、Own、key、伪类、状态机或代码属性绑定。

最终没有实例Token字典、ResourceProvider、ThemeContext、context/snapshot缓存或新增全局订阅。四控件既有47成员非trivia指纹保持，仅增加4只读接口成员；触及的12个旧结构问题已整理，四文件/两实际注册属性控件的成员门禁0问题。

## 真实失败与架构裁决

- 原16个正文/caret/selection消费者RED1s；标准Foreground另4RED958ms。
- 第一原型通过这些检查，但实例资源provider直接读snapshot。自审补出的4项局部原生Resources覆盖均RED952ms，属于必须修复的问题；该原型已整体撤回，不能以其37项/6101原生/零分配记录证明最终实现。
- Core ThemeTokenResourceProvider的snapshot重载与slot helper逐字节恢复，私有provider源码及模板Resources块已删除。新方案复用既有资源观察者，保留局部资源覆盖/清除。
- 新方案最初未明确源输入时，LineEdit状态色命中模板ScrollViewer身份，两项检查失败；增加SourceControlType后原状态输入恢复正确owner，没有复制状态规则。

## 最终验证范围

39临时检查、0FAIL/Skip、2s：原20反例、四原生资源覆盖、四无条件普通Style/Local/撤回、四尺寸/variant/default/error/warning/disabled、透明、foreign身份、两个provider往返、重模板/重挂、共享Own37与公开子Button独立身份、四编译专用Input Style及16个关键画刷Setter值/实际命中/撤回、真实KeyTextInput/selection/caret及选中字形ShapedTextRun颜色更新、12个root/input/presenter图回收。

最终managed与NativeAOT原生NSWindow各6117检查通过，包括局部Resources覆盖/清除与正常Native资源查询预热后10000次0分配。此为查询检查，不声称页面加载或主题切换速度提升。第一原型额外key装箱已随架构撤回，不把其720KB→0当最终页面性能收益。最终不存在每实例Token字典/provider，较原型每实例1→0，100%移除这一结构开销。

NET8 Release 0警告/错误、20.29s。清理后Core四个精确资源类22PASS、207ms。原生发布的5条本机OpenSSL/Brotli deployment链接警告如实保留，不称AOT零警告；无新增反射/裁剪抑制。

## 绘制

1130×560完整窗口，四root各四尺寸×四variant、原Sample文本/245宽保持，正常motion。静态采样至少650ms与连续3稳帧；选区通过真实focus与selection后只读保持高亮，未关闭正常输入反馈。原版与最终候选的1536对状态观测中字号/Bounds完全相同；原型媒体仅作排除记录。

自然96对：44默认/恢复等RGBA0，48个有效配置/状态/Local/选择场景变化；四个Local清除场景严格0仍FAIL，分别Area Light2(max1)、Dark2(max2)，Mentions Light13(max4)、Dark15(max6)，保留完整原始坐标与颜色。不能声明自然恢复全部通过。

最终直接多行A/B：撤回且仅撤回AM5的14个源文件，采原轨迹，finally逐字节恢复并重建当前源；48对子集的24个非配置画面均0，24配置等变化单列。不用直接0替代自然失败，不扩大容差。具体范围见[限定审查](input-autocomplete-mentions-global-owner-visual-review.md)。

10个临时验证/驱动文件（35113 bytes）、15份源副本、3份含完整源码的成员inventory、439文件原生工程（1381381879 bytes）、480PNG+manifest共481媒体全部清理，媒体audit clean。保留的数值/文本/hash区分被否决原型与最终实现，常驻测试净增0。

清理后Desktop首轮157项中156PASS，一项旧源码契约只因仍要求字面SearchEditTokenResource失败。已将该原断言更新为四个明确SourceControlType=SearchEdit组合输入，同时保留public Button、ButtonTokenResource及禁止LineEdit借用的原约束；不是删除检查或增加常驻用例。最终精确Input/AutoComplete/Mention/CandidateListBehavior共157PASS、0FAIL/Skip、6s，git diff --check通过。

AM2/AM3/AM4待审范围不被本包代批。Form/Upload默认与实际消费者、其他选择控件剩余入口及Semantic Part/状态/完整家族继续。
