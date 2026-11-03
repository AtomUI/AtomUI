# CT15 Tour 严格退出核对

复用7metadata/9Own与44对静态批准，不重做Own/Part/定位修复。当前补两条资源退出：公开关闭后移除Tour、打开时直接移除Tour。保留800×600 AtomUI Window/真实目标、完整标题/描述与120高900extent滚动内容、正常动效；目标与窗口持续存活，只对已退休Tour/step/description创建NoInlining弱引用。打开时确认真实popup，关闭/移除后等正常clock消退再GC。不以保留step（合法父链）作为产品泄漏判据。

另补同Tour关闭/重开后目标仍移动同步的正对照；若无有效失败只记录补证，不改产品或旧原版缺72帧/GC未覆盖范围。临时源码在Temporary/CT15TourRetirement，清理后只精确Tour及相邻placement模块，不全量。


因果实验：原关闭后移除/打开时移除2FAIL；ClearValue三属性四用例全FAIL，官方12.1.4 ValueStore.ClearValue只重求有效值而不退役binding，不能把它当解绑。改为公开BindingOperations.GetBindingExpressionBase(...).Dispose三条后4项2PASS/2FAIL：仅dispose诊断两路径PASS，原路径仍FAIL，17.3432s。官方12.1.4 IndexerBindingExpression以readonly _source持有Tour，窗口存活TourLayer的本地binding形成持有链。诊断不是Semantic Style定制，不进入产品。

修复：共享mask是跨视觉根运行时节点，ControlTheme无法等价声明当前owner关系。用现有CompositeDisposable持三typed observable binding，先逻辑归属再接线；DetachMaskLayer释放句柄，跨Tour转交先让原owner退出。保持同Local优先/原值/几何/颜色/motion/Part route，未新增Token/API或容差。原binding与当前不可保留，验证中。


修复后4临时+21既有Tour共25PASS/0Skip/15.8015s；额外共享层交接用例初轮4PASS/1FAIL，Green期望实际Red，源码ConfigureDefaultValues原本把root MaskColor填入Step，本轮不能假定已有Step值随root改变。改成公开Step.MaskColor输入后5临时PASS/0Skip/20.5281s。包含关闭改值重开、目标移动30、第二Tour接管为Blue、第一Step改Yellow不干扰当前Blue、再回第一正确Yellow；原step初始值合同未改，不把判据错误当新产品修复。

修复只解除不可见/退出mask持有的运行时关系，保持打开时三Local投射有效值/优先级；没有新有意视觉变化，不扩大原44对限定批准。当前正常motion/真实Skia检查与原完整静态证据分别记录；本轮未作旧新整帧兼容新声明。AOT/临时清理/清理后精确模块与NET8仍在执行。


同源当前Mac Headless/Skia NativeAOT20检查0FAIL/exit0，包含两条无诊断回收和共享层重开/交接。AOT46245192字节SHA256 275abf59ca867281696b8200745ac1b995b133dbf1adff754dcf81aa5cb3022b；零IL/CS warning，五既有Homebrew最低OS linker warning。5临时case/1源6866字节、311会话757466582字节（含驱动/发布/失败日志）已删除；本轮不生成媒体，不以属性/GC检查冒称新增整帧对照。清理后精确Tour/placement与NET8继续；正式implementation/changelog已同步，原44对批准与旧72缺帧/18非零/14cross保留，完整CT15未关闭。


最终清理后门禁：实际重建Tour21PASS/0Skip/5.6661s；首filter误写Popup而实际namespace为Popups，未把未匹配项计入，随后相同已重建程序集精确placement8PASS/0Skip/2.9760s。合计29不同既有用例。Desktop NET8 Release最终源码0警告0错误17.61s；成员1源/1控件0问题与git diff --check通过。作者另步自审确认：mask绑定只在当前逻辑owner获取/释放，所有新句柄有匹配退出，转交先释放旧owner；无新注册/API/Token/依赖/容差。无独立审查或新视觉批准声明。最终helper/marker记录后删除，其他原失败及完整CT15保留。


## 自审补验：交接通知顺序

作者复读mask退出增量时发现：AttachMaskLayer复用DetachMaskLayer后，又保留了原显式RaiseCrossRootsChanged；前一次还未清previousOwner._layer，形成[1,0]两次通知。当前单独临时用例有效FAIL（5.7505s），预期原完成交接后一次[0]，实际[1,0]；这是本轮重用退出方法引入的通知回归，不能拿前面的颜色/GC绿忽略。

只调整先清旧owner._layer，再调用统一DetachMaskLayer（其中解除binding/逻辑父并通知一次），删除额外Raise。没有新API/字段/Token或样式值，保持先前持有链修复与原一次完成状态通知。1临时+21Tour正在验证；不把旧AOT20冒称本次通知顺序源的重新发布结果。


通知顺序补验已闭环：1有效RED原[1,0]→修复后1临时+21Tour共22PASS/0Skip/7.6984s；2390字节单临时源已删除。清理后真实重建Tour/placement29PASS/0Skip/5.7930s，最终源码Desktop NET8 Release0警告0错误17.41s，成员1/1/0与diff check通过。原AOT20明确保留为此前mask绑定增量证据，本次仅既有调用顺序/重复通知修正，没有新增AOT敏感路径，不冒称本次重新发布。最终会话/marker随读取清理。作者自审补验发现并纠正本轮回归，不影响其他既定批准/原失败、未关闭完整CT15。
