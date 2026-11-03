# CT15 Avatar / AvatarGroup 消费者计划（2026-10-31）

从[Avatar/Tag/Badge范围核对](avatar-tag-badge-consumers.md)先执行Avatar 3metadata/11Own。源码归属Controls的Avatar/AbstractAvatar/Token/Theme和Desktop的AvatarGroup；不新建分支/worktree/提交、不使用agent。前包ToolTip/PopupConfirm已获68对及根前景限定批准，其余边界保持。

已读四正式Avatar文档与实际源码。当前Avatar/AvatarGroup库存没有已发布Semantic Part manifest；原文档将Frame/Icon/Image列为internal-observable，唯一稳定模板合同是PART_TextPresenter。Ruling：本包验证公开root Style/原生属性和现有稳定模板合同，不假称存在生成Part类，也不借取节点赋值制造定制；不为凑数量新增公开语义API。后续Tag/Badge沿已有生成的专用Style验收。

11默认V2R1与44条四算法默认/type/key/slot应保持。TextFontSize三字段当前实际用于EffectiveIconSize，文字通过原生FontSize/继承与Viewbox自然尺寸呈现；不得仅据名字把原默认文字强行改为该数值。先验证明确Own现有消费者、Global字体/方形圆角、原生文字与尺寸、Custom及恢复、Circle半径随实际尺寸、内容切换与图片租约。字体或尺寸变化不得绕开原Viewbox缩小/居中、Gap、原图片优先及加载/fallback状态机。

Group组合尺寸/形状/Border投射保留原合同，先测试移除后旧绑定是否退役、保留child不反向保留已移除group、fold/pin/重挂/集合状态；运行期集合跨入Content子树，不能等价用模板路径时写明typed代码绑定原因与持有/释放。图片管线复用已存在模块证据，不重写loader。

- [ ] Temporary/CT15Avatar有效反例与44默认导出；逐Own/Global/Native/三档/Custom/图片/组合状态。
- [ ] 根因小修与默认迁移，完整成员/注册/初始化指纹；不改闭合家族或CT25/FW1。
- [ ] 实际Skia、AOT及相同完整历史、独立A/A/1DIP；保留完整内容/原滚动裁剪和overflow popup合同，逐状态资源退役。
- [ ] 全部临时源/外部材料/媒体清理后Controls Avatar与Desktop AvatarGroup精确模块、NET8、四正式文档/CSV/总表/作者另步自审；限定视觉工程完成后再审查，完整CT15不因此关闭。

首轮10项5有效FAIL/5PASS：连续Custom尺寸仍48而非72；原生宽64时Circle半径仍16而非32；明确Global字号23实际文字仍14；移除child后仍由group40更新为72；保留已移除child时旧group未回收。四算法44默认/type/key/slot与既有Native文本/Viewbox合同通过。

Ruling：尺寸/圆角/Gap的模板输入优先移到ControlTheme，用有限类型化converter表达计算，代码只保留Size↔SizeType意图及图片状态机。原默认文字随原生继承（本机14），TextFontSize三Own继续用于现有Icon尺寸，不按名字强改默认文字。Group保留既有组合投射优先级，以membership disposable退役，不新增原始Local值恢复引擎；remove/detach/fold退役释放对应源，运行集合不处于ControlTemplate路径，代码说明原因。

首轮修复后10PASS，四配方44默认记录和原文字getter均相同。扩至12项（11Own实际消费者、Group运行中Custom连续变化与恢复）均PASS，不把猜测当新bug。再补构造时Group.Size=72取得有效RED：子项实际32。根因为先投射Size再投射preset SizeType，把子项刚进入的Custom覆盖；仅将SizeType relay排在Size之前，复用子项既有Custom归一规则，不新增Group状态机。11默认V2R1保持原Math.Round和颜色/间距公式，三TextFontSize仍实际Icon角色，未强改文字默认。首次XAML把CompiledBinding当对象元素的编译错误已改为CompiledBindingExtension，仅计实现错误，不计产品RED。

当前修复13临时+13既有AvatarGroup共26PASS/0Skip（2s），V2四算法44默认/type/key/slot及实际文字默认逐条相同。Controls独立Avatar文字/图片模块正在匹配验证，未跑全量；图片状态、GC/极值、成员整理、Skia/AOT及清理后出口继续，不据26关闭本包。

补充borrowed图片：Custom48→80后保持同一原IImage，圆形裁剪半径40，Source清空恢复原Text，重新绑定仍Loaded且不dispose调用方图片；与既有Group13项联合当前27PASS/0Skip（14临时+13既有，3s）。Controls独立Avatar模块7PASS（348ms）。四C#文件/两Control成员73问题纯整理后0，75成员及注册/初始化快照完全相同。后续仍需四算法Skia/NativeAOT、图片/组范围的清理后验收、正式文档/CSV及限定视觉，当前不申请完成。

## 本轮限定验收合同

实际来源为上述有效RED、公开尺寸/文字/图片合同及membership释放风险。固定900×700的Desktop Window，四算法、正常motion、真实Skia；独立进程A/A与1DIP检错，与主业务历史分离。所有版本走完全相同动作，按同坐标完整帧零容差比较，不删完整文字、不取消Viewbox/Gap/图片裁剪或fold限制。

| AC | 前置/动作 | 独立预期与读数 |
| --- | --- | --- |
| A1 Own | 三档Icon、文字、图片及4成员/2可见Group；11Own改值、合法0、清空 | 47/61/29三档、Icon23/31/17，Group19/11及原角色颜色；0确实进入目标，清空恢复原配方 |
| A2 Global/Native | Avatar明确Menlo/23/方形9、Group自身Global BorderThickness3；原生17/Courier/Orange及Circle宽64，再清空 | 字体与真实Text一致、Circle32；原生优先、clear回到对应配置，不把Avatar owner误当Group owner |
| A3 Custom | 同一Icon/Image48→72、Group72→96→NaN，Square Custom48 | 每次真实box/icon/clip同步；NaN恢复原preset，原图片仍同一borrowed对象 |
| A4 组合/模板 | RTL往返、fold0/全显/恢复、原实例卸载重挂、Text替换模板恢复 | 内容不丢、原fold计数/布局复原、退役binding不继续投射；仅验证所执行API，不声称OS输入 |
| A5 内容与极值 | Source清空/重绑、完整Unicode长文Size0→160→NaN | Image>Text>Icon优先保持，borrowed不被dispose，文字完整且Viewbox仅缩小；业务恢复逐帧比较 |
| A6 校准 | 每配方独立实例两静帧、容器TranslateX1、ClearValue恢复 | A/A0、1DIP必须非零、恢复0；发现非零如实保留，不扩大容差 |

Group Global边框补验首次因驱动误用string代替ControlTokenIdentity编译失败，不计产品RED；已更正后再执行。当前临时宿主仅Headless/Skia与后续macOS NativeAOT进程，不代替真实OS窗口、键鼠、平台弹层或Browser。

Group自身Global边框有效RED实际1而非3，模板单入口Shared→Owner后，包含原生5/清空回3/配置清空回1在内所选模块29PASS/0Skip（3s）。此处身份用ControlTokenIdentity，驱动早先string误用的编译错误不计产品RED。主渲染驱动编译时补齐AtomUI/Desktop命名空间并使ProbeApp partial，消除startup警告，最终零警告零错误；这些仅为临时驱动修正。

候选首轮四算法主轨迹76检查0FAIL、116主帧；36项同实例业务恢复均精确0。独立新进程4检查0FAIL；4配方A/A及扰动恢复均0，1DIP分别1607/999/1362/832像素、max235全检出。后续补独立overflow打开/11Own热改/清空/关闭/正在打开时退役，证明真实弹层中的GroupSpace与子Avatar消费，不以关闭态属性值替代。旧新/AOT尚在执行，当前不宣称视觉通过。

实际overflow热Own首轮4FAIL（普通32、Compact28，期望47），未隐藏或放宽。临时单用例复现确认子项context与Group为同一对象、snapshot ContainerSize已47、spacing19，而真实child.Width32，属于资源发布通知缺口。修复限定AvatarGroup运行时fold Content根：复用既有ThemeContextResourceBridge，把context发布桥接到该StackPanel资源；跟随Group Context变化，fold退役/detach解除并dispose。没有修改共享Flyout实现、Scope引擎或把bridge铺到所有Avatar；单头像模板仍纯声明式。首轮307取证文件保留prebridge前缀，最终版本重新走完整轨迹，并补保留退役Content时旧Group/Scope弱引用回收验证。

资源桥修复后所选31PASS/0Skip（5s）：17临时、13既有AvatarGroup及1既有Dialog内AvatarGroup出口。保留退役fold Content的旧Group/Provider两弱引用均回收；popup配置快照47和真实宽47一致，清空回32。四算法44默认/type/key/slot及原文字getter与修改前逐条相同。外部驱动最终Release构建零警告零错误，开始候选完整主轨迹/独立校准/overflow，随后同完整历史旧源回放与NativeAOT；旧源回放以finally逐字恢复候选8源和新增converter，期间不触碰其余未提交内容。

主候选76/0FAIL、overflow20/0FAIL、校准4/0FAIL；同源历史旧版主76/28FAIL、overflow20/4FAIL、校准4/0FAIL。查看画面确认旧新均为浅Window承载暗scope，暗色内容对比不完整。未将其宣称完整Dark视觉；在AOT启动前明确停止该批、待old finally完整恢复，保存light-host前缀证据与数值。驱动补承载Border，背景只读当前配方Global.ColorBgContainer，不改产品/目标部件/原尺寸内容/裁剪；后续所有版本重新执行同样历史。此是取证宿主修正，不算产品修复，不以仅延迟/重建一侧消差。

承载面修正使用公开资源查找SharedTokenKind.ColorBgContainer（支持Color/IBrush），不引用internal ThemeScope、不依赖反射。两次驱动编译时先误用了internal上下文入口及enum命名空间，未执行成功也不计产品RED；已改为公开API，第三次构建零警告/零错误（10.97s）。四算法相同完整历史重放已启动，原浅宿主614文件保留，最终只依据修正后的批次申请限定审查。

修正承载面后的旧新136对：85精确0、48对明确有意变化（Own动态Circle半径、Global字体/Square/Group边框、连续Custom/Group尺寸、完整长文0→160、打开overflow热Own），另3对closed微差Light80/max3、Compact53/max12、DarkCompact44/max13未通过且不列批准。默认4对、两版各36业务恢复均0。独立两版4配方A/A/恢复均0，1DIP分别1607/1645/1362/1401像素，max235/210/235/210，全检出。已查看普通/暗/Compact的Global、Native、Custom长文与真实overflow关键帧，原完整内容/图像/裁剪与Box保持，未拿数值代替视觉查看。NativeAOT publish完成，五条既有Homebrew库macOS26-v-target12链接警告、无IL警告；AOT进程取证继续。


2026-10-31 CT15 Avatar当前切片：[结果](avatar-consumers-results.md)、[48对限定视觉已批准](avatar-consumers-visual-review.md)、[作者另步自审](avatar-consumers-review.md)。3metadata/11Own全Active V2R1、44默认/type/key/slot与自然文字14保持；连续Custom/Circle、字体/owner/Group投射、membership退出及真实overflow热资源发布修复，39匹配验证、managed/AOT各100检查，136跨运行134零；两版主业务恢复各36零。closed/往返微差全部保留。18临时/1843取证/411driver/551原会话清理，最终Controls7/Desktop14及NET8零、成员5-2-0。仅48有意变化已获用户限定批准；完整Avatar/CT15、OS/Browser/Popup过程时相保留。下一[Tag](tag-consumers.md)已有4有效RED/5PASS、36默认导出，尚未改产品；NeedsDecision8及137/274不变，无Git。


限定批准登记：2026-10-31，用户明确回复“批准表内48对，其余未通过与完整CT15保留”（request_user_input_async / call_7b292d7ef454420fb8fc0603ac3e3f4c / item 0）。仅本表48对有意变化获批准；closed/往返微差、OS/Browser/Popup过程时相及完整Avatar/CT15继续保留，不授权提交。
