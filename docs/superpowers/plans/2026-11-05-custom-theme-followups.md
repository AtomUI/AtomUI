# Custom-theme 剩余四项重构优化执行计划

> **For agentic workers:** 使用 superpowers:executing-plans 内联逐项执行。用户明确禁止子 agent，按原授权自主推进，不重复申请继续或创建隔离环境。

**Goal:** 完成窗口回收诊断的根因处置、Native hook 多宿主与成本优化、独立 Steps 9 metadata/82 Own 的实际主题消费及正式文档收口。

**Architecture:** 保留现有主题求值/事务/宿主 lease；框架兼容实现集中于 AtomUI.Native，宿主安装与释放由现有生命周期负责。Steps 复用 DependencyDefaultsV2、OwnerTokenResource 与专用 Semantic Style，保留原 public API/默认外观。所有记录只引用实际证据，不将历史标签、已接受差异或平台边界改写为新通过。

**Tech Stack:** C#/.NET8/.NET10、官方 Avalonia12.1.4、当前批准的 Skia 兼容、NativeAOT。

**Spec:** 用户“把这些任务都重构优化”，对应上一轮列出的四项；本文件同时保存本轮有限设计、执行记录和最终结论。

## Global Constraints

- 当前目录 AtomUIV6-custom-theme，分支 feature/custom-theme，开始 HEAD74be4a1a6；不创建分支/worktree/clone/源码隔离副本，不使用子agent。
- 官方 Avalonia12.1.4、官方缓存/native依赖保持；不修改 Avalonia 源码，兼容 hook 最多放 AtomUI.Native。
- CT24继续暂缓，Windows专项继续开放；不要求任意平台、极值或全部组合。
- 本次新增授权恢复独立 Steps 的9/82范围，旧CT17其他50/92的批准直接复用。
- public API、Token名/type/default/resource ID、原始尺寸/滚动/输入/默认外观保持；真实契约修复从根因完成。
- 临时驱动统一放系统会话目录，需内部fixture的临时测试放对应 Temporary/CustomThemeFollowups；真实执行、完成删除，再做精确模块验证。构建/采样使用RunLock。
- 最终按实际改动模块精确filter/iterate验证，不自行change/full；普通构建与AOT按稳定源码合批。新工作交用户审查，不扩大此前已完成的提交/push授权。

## Review Focus

- 保留已分离子对象时关闭Window：不得通过丢弃保留对象、清DataContext/Style、关闭动态资源或漏掉原等待/布局历史伪造回收。
- 原G1全局普通类型Style：完整16根/明暗/四档/140→180→140/清根恢复必须保持；Application/Provider/Popup仅增加真实宿主的同一退役语义。
- 通知顺序、异常、重入、同一宿主多lease、关闭及退役来源隔离；已释放lease不能保留owner。
- 无类型/嵌套选择器：不能因当前激活或树已脱离就跳过残留frame，成本不能靠回到重复逻辑子树遍历隐藏。
- Steps：现有形态/横纵/Small-Middle/RTL/状态/交互及专用Part Style优先级，与明暗/Compact四算法和配置清除恢复一致。

## 开工审查

窄缺陷与共享架构组合任务：Window拥有显示/关闭，Core拥有Context/StyleHost lease，Native只拥有兼容通知和frame退役；hook目前只在Window构造安装。Application用独立IGlobalStyles链通知TopLevel，不能假设它是StyledElement或用单Window代替。现有Core未直接引用Native，扩展前核对模块依赖与生命周期接缝，选择最小明确依赖，禁止反射扫描寻找业务owner。性能热点是重复IStyleHost.StylesRemoved递归，先证明固定私有边界上的逐节点来源退役与AOT可行，再决定实现；不先堆更多类型过滤。

Steps现有StepsToken仍WholeEvaluatorV1，82属性且图标圆角依赖IconContainerSize，需按实际依赖迁移。源码主文件/三个主题、StepsPanel等9节点与现有Steps测试/图库组成有限家族；先核对每字段消费者/默认，再动源码。不按行数机械拆文件，不增加公开属性或为无消费者字段发明功能。

## Task 1：原始Window回收诊断

Files：必要时修改实际保留链所属模块；临时 tests/AtomUI.Desktop.Controls.Tests/Temporary/CustomThemeFollowups/WindowRetentionTests.cs。

- [x] 从原归档21257/21273及后续实际诊断恢复原保留Border/Window/无Owner binding操作体、时钟和完整历史。
- [x] 当前源码精确重放，记录WeakReference对象及真实保留链；失败后用gcroot/对象关系定位单一来源。
- [x] 对实际来源做最小修复，原断言前后验证；保留原子对象与后续重挂/资源更新能力。
- [x] 删除临时源与专用产物，运行实际涉及Window/Theme/生命周期精确filter；根因、前后与去留只记本文件。

## Task 2：多宿主Native hook与成本

Files：src/AtomUI.Native/Compatibility/AvaloniaStyleRemovalHook.cs、实际安装/释放点及必要项目依赖；临时 StyleRemovalTests.cs 和同一参数化原生驱动。

- [x] 以现有原G1操作体建立Application/Provider/Popup的真实RED或覆盖前证；确认无类型选择器的同步退役成本。
- [x] throwaway验证固定ValueStore边界的逐节点来源退役：BeginStyling/RemoveFrames/EndStyling、异常原样传播、NET8与实际AOT。若不可行，基于具体错误改设计，不修改框架。
- [x] 在Native集中完成线性退役与共享、可释放hook；使用现有宿主lease安装/释放，Window原入口兼容，多lease不重复订阅。保证公开通知前完成，不清节点Style或重赋属性。
- [x] 原G1、多宿主、无类型/嵌套、来源/优先级/异常/重入/关闭/回收精确验证；同有效画面的成本前后对照。稳定后集中NET8/NativeAOT。
- [x] 清理后仅跑Native实际消费者/Theme/Window/Buttons精确模块，更新Native/Window/Core正式边界。

## Task 3：独立Steps 9/82

Files：src/AtomUI.Desktop.Controls/Steps/{StepsToken.cs,Steps.cs,StepsItem.cs,StepsItemIndicator.cs,StepsPanel*.cs,Themes/*}实际需要部分；docs/controls/desktop/navigation/steps；临时 StepsThemeTests.cs。

- [x] 核对9节点/82字段的真实owner、依赖、消费者、原默认/状态/专用Part及现有测试；建立失败的Own派生/Global/清除/状态例子与原默认记录。
- [x] 用现有V2默认机制迁移82字段与真实Own依赖，修正实际消费断链；模板可表达的绑定放ControlTheme，保留动态生成节点的明确owner/释放。
- [x] 原形态/方向/尺寸/四算法、实例优先级、专用Style命中与清除、Current/禁用/数据变更/重挂/回收有限矩阵；原生/软件绘制同默认和配置恢复验证，正常动效不关闭作性能替代。
- [x] 临时测试删除、成员审计、精确Steps模块及NET8/实际AOT；按证据更新9/82分类和正式文档，保留其他历史批准范围。

## Task 4：文档与元数据收口

Files：docs/architecture/systems/theming/verification.md、现有tasks/overview/final-exit-review、token-contract-review.csv的23个CT21旧描述及本次Steps行。

- [x] 按当前源码、家族权威出口与精确证据更新正式实现事实；CT21的23个已ApprovedScopeComplete字段删去过期in progress叙述。
- [x] 确认36个NeedsDecision与其他历史标签的真实分类，更新权威指向/历史身份；不将未验收对象批量改PASS。
- [x] 总表只保留当前状态、关键结果与本轮记录链接；旧详细过程留历史，不复制多份。
- [x] 核对724目录/1244字段/90问题的主键和映射、本次Steps范围、相对链接、临时清理和git diff --check；最后另步作者自审。

## 执行记录

开始状态：工作区干净，HEAD74be4a1a6。旧14项hook/362项Window-Buttons与原NET8/AOT证据只认证旧源码，不冒充本次新增作用域或优化后的结果。

Task 4阶段进展：已对照当前已批准出口，修正正式verification里旧“95尚未迁移/编辑器未做/仅原型AOT”的事实叙述；仍如实保留Steps、窗口回收与多宿主扩展在做状态。CT21恰好23个ApprovedScopeComplete字段的过期in progress描述已同步，未改字段名/type/类别/消费者或资源ID。

Task 1原始证据：从08:29会话3516创建源及3574/3606/3626/3654完整变更恢复原四合同，保持750ms真实DispatcherFrame、6次GC、AtomUI.Window、原Button/保留Border、布局及逻辑/视觉脱离断言。无Owner binding的原Live_Detached精确Release重放1FAIL；只读实例字段BFS找到Border._borderVisual→CompositionVisual._parent链→ContentPresenter.Host→Window（2355节点内）。单变量仅清除已分离Border私有缓存后原断言1PASS，确定保留来源。

只读精确12.1.4源码说明：Visual.DetachFromCompositor清CompositionVisual，Border另存_borderVisual并未清；Renderer关闭清待更新集合，因此旧合成父链不能保证再同步。已扫描同类缓存：Controls中的Border为可重建UI缓存，PullToRefresh.RefreshInfoProvider为不同的显式数据/visual引用，不据此扩张其他控件任务。产品修复选Native可释放VisualParent class hook，在实际脱离后清整个脱离子树的Border缓存；不强制绘制、不改属性/逻辑树，不移除用户保留对象。Desktop runtime context.Own与模块释放配对；重新挂载由原CreateCompositionVisual重建缓存。正在按同一原断言验证产品接入。

Task 2低层可行性：固定ValueStore.GetValueStore/BeginStyling/RemoveFrames(IReadOnlyList<IStyle>)/EndStyling的静态限定反射与闭合delegate原型，NET10.0.8与NET8.0.24实际运行成功，osx-arm64 NativeAOT实际运行FRAME_ACCESS_PASS；无CS/IL诊断。第一次AOT缺LIBRARY_PATH的ssl链接准备错误纠正后成功，不计功能RED。此仅低层可行性，不代替G1/多宿主验证。

Task 1产品前后：原无binding Live_Detached在新Native缓存释放hook下1PASS（原操作体未加手动cache清除）；正式前证1FAIL与单变量因果1PASS均保留。重挂/后续绘制及NET8/AOT尚待合批验证，不据单项绿宣称整个任务完成。

Task 2正式扩展接入：在Native采用共享弱键、引用计数lease；Core ThemeStyleHostLease按已有挂载/清理事件取得和释放，同一Window原入口与宿主lease不会重复替换原delegate。Core到Native为明确能力依赖（架构允许，原overview图已有该方向），新增ProjectReference及internal friend，不增加回调服务抽象或升级第三方依赖。Application的原GlobalStylesRemoved订阅target提供真实TopLevel根；Provider/跨根宿主用自身继承scope。源Style图与目标作用域在原回调前快照，直接调用框架ValueStore的Begin/RemoveFrames/End闭合delegate，每节点一次；取消当前TargetType过滤，避免可变selector与重复逻辑树遍历。CWT store adapter仅弱键复用。

当前实际低成本用例：Window通过、Application与Provider有效RED字体23未撤回；产品共享hook后这3项PASS。两项原“Native popup”夹具实际落在Headless Overlay，原根类型断言失败属于准备失败，不能算产品RED或Native证据；已改成明确Overlay测试，独立Popup将由实际NSWindow驱动验证。

Steps前证2FAIL：显式IconContainerSize49时派生圆角仍32；控制级FontSizeLG23时HeaderPresenter仍16。四算法82字段的328默认资源已在旧Steps源码捕获。

绘制前证：10个固定真实Steps（六形态、Small、Vertical、Panel两变体、RTL）1240×1040、四算法、450ms真实时钟，第一批main复用1DIP检错：Light AA/restore0、needle40059；Dark AA0、needle40237、restore40非零。保留该有效旧源码恢复FAIL，不作零容差通过。按视觉skill原规定把检错与主基线分到独立实例，主图四算法每个独立Window做A/A、两版本同历史；这不删除产品触发或把旧restore40改PASS。原32路径已登记，实际生成16；最终记录实际文件与清理。

Task 3迁移进展：82字段全部迁V2R1；唯一原Own依赖IconContainerSize→IconContainerCornerRadius显式声明，原四算法328默认资源逐值比较一致。标题Global字号23已到达真实Header；可选字体MultiBinding仍回退Default，已定位ControlTypographyBindings要求IThemeInheritanceSource，而Steps缺少该只读实际继承父桥，正在补齐。未新增Token或公开API。

用户新增截图指出Filled/Outlined Panel文字没有垂直居中：确认旧190高单元格中section只按自然高度安排在顶端，面板/箭头却填满整格。将其纳入Task3：保留面板尺寸、箭头、文字/描述与padding，只对Panel提供完整内容slot并在ControlTheme声明默认VerticalAlignment=Center；天然高度、Small/Middle、两变体及RTL逐几何/视觉核对。旧截图与旧值作为有效前证，不把该明确请求的变化当默认漂移拒绝处理。

Task 2额外有效RED：真实Headless OverlayPopupHost的Styles移除后模板字体23残留；Overlay内Provider自己的lease已通过。该host不属于Core ThemeStyleHostLease，需由现有Popup.Opened/Closed生命周期取得/释放共享Native hook（含Popup自身Styles），不伪造Core lease或修改Popup外观/交互。独立Popup仍待真实Native驱动确认。

Panel居中有效前证：Filled/Outlined×Small/Middle四项均FAIL，160高wrapper中心80，而文字section中心仅22/约29。修复按AXAML优先：layout只提供Panel完整内容slot，Theme在Panel分支声明section.VerticalAlignment=Center，不写固定像素位移，不改变Frame/Arrow/方向/内容。正在产品验证；后续增加自然高度、RTL与专用Part覆盖。

最新精确10项PASS：Application/Window/Provider/Overlay/Overlay内Provider五宿主均退役恢复；标题Global23、可选字体Menlo及wave开关到达真实owner；Panel两变体×两尺寸居中通过。原几何断言先要求连续坐标中心严格80，而框架实际将奇数高文字组按像素取整到中心80.5；已按框架layout rounding公式锁定期望Top=Round((wrapper.CenterY-section.Height/2)*scale)/scale，不扩任何像素差异容差。原22等前证仍失败，非以放宽消除偏顶。

字体排查中的typed StepsTokenResource(FontFamily)尝试已撤销：FontFamily是Global，不是82 Own枚举；实际缺陷为Steps未实现只读IThemeInheritanceSource，导致已有OptionalFontFamilyConverter的继承源报错。现在使用既有Owner Global入口及真实InheritanceParent桥，Global字体测试通过。新增接口均internal协作，不加public API。

82实际消费者核验：先由旧AXAML映射每字段的真实节点/属性及可达状态，用逐字段独特Own输入检查布局后CLR属性（颜色检查真实brush.Color），不以资源字典变化代替消费。13初始失败中10为夹具选到hover或漏Status/Wait状态；纠正完整状态后稳定剩3有效FAIL：NavItemGutter实际8、DotLineThickness实际1、PanelArrowWidthSM实际56.568，均未到达明确Own37。

根因是template内动态资源属性成为Local优先级，压过后续state Style Setter。将基础ConnectorThickness/IndicatorSpacing/PanelArrow宽及wrapper ArrowWidth迁到既有ControlTheme style Setter。原三个Token声明默认16/3/43.8402与真实历史绘制8/1/56.568不一致；保持82字段默认及未配置像素合同，三个状态入口用已有WhenExplicit，明确Own时接管，清除回到原有效Global/通用箭头宽。不是把未消费字段改称完成或发明新公共参数；兼容默认差异将写正式Token文档。

Steps 82个Own逐字段真实消费者配置现在全部通过（一个参数化临时case内部82项，非82个独立test）；包括颜色brush、尺寸/边距/圆角、进度/点状/Navigation/Inline/Panel状态与Small。三项优先级修复后默认328资源仍需最终再核；其兼容默认入口只认明确Own。四算法完整主图A/A均0；版本比较差异Light/Compact10888px、Dark/DC10876px，全部位于用户明确要求修正的两个Panel单元格，其余区域逐RGBA0。已实际查看Dark before/after，文字组居中、Frame/Arrow大小保持；不将旧needle restore40改写。

Native扩展实际NSWindow重放：原global-types与root-scoped各278检查通过（原280中的额外只读守卫未计入本恢复版本，不凑测试数）；追加Application/Provider、实际Avalonia.Native.PopupImpl独立Popup及其Application全局样式后global扩展287检查通过。无类型Class选择器同完整轨迹278通过。单进程计量显示对无源节点仍Begin/End会触发有效值遍历，故加真实StyleInstance.Source身份预检与弱frame源缓存；后续安静ABBA对比才作最终成本结论。原生驱动恢复中缺UseSkia历史编辑与/var、/private符号路径导致的准备构建失败已纠正，不算产品RED。

成员结构另步整理：Steps目录15文件/9控件及Popup共10控件，75项存量区域/声明顺序问题已归零；纯整理前后Roslyn成员token指纹完全相同。只调整属性区域、internal协作区域、接口实现和私有方法位置，不改变成员访问级别、名称、类型、默认值或方法体。当前15文件/10控件/0问题；后续语义变更须再次按实际受影响文件核对。

Task 2成本收敛：首个直接store版本对所有节点Begin/End导致约1.49MB/37ms，不保留。先按真实旧frame身份预检、弱frame源缓存，再只快照实际有旧source的节点；该完全扫描候选在安静ABBA中typed611036→783752B、8.45745→12.7016ms，class11852284→2052444B、152.2608→60.08015ms，typed回退未忽略。最终仅对“hook在框架挂载前已观察到且Selector对象未变”的Style使用原TargetType必要条件跳过不可能节点；既有/可变/无类型selector仍按真实frame扫，mutable selector保护通过，未回到按当前匹配跳frame。

最终同原16根/正常原尺寸历史/root-scoped或Class单变量、ABBA×3各6进程、每进程明暗2样本、每次撤回16条；48样本、24进程均278检查通过。typed中位before611036B/9.07185ms→after606276B/9.491ms（分配-4760B，时间+0.41915ms，维持原量级不称加速）；class11852284B/151.44695ms→2076052B/60.7821ms（分配-82.48%，时间-59.87%）。Frame源身份/当前selector变更和早期未知图的安全兜底保留。该成本只对应具名退役场景，不扩大旧G2有限成本接受或声称端到端更快。compiled before/after库快照均在专用会话，未创建源码副本或回转工作区。

最新保护：82 Own的配置/清除/再应用到真实消费者全部通过；自然高度/RTL两变体4case通过。共享hook双lease/通知顺序/最后释放、保留已释放lease的owner弱回收、嵌套retire/公开事件首异常、可变selector及其他来源/LocalValue共4case通过。Border脱离子树后改CornerRadius并重挂新Window检查已通过。Steps现有模块203PASS/0Skip（包含专用Section Style退出恢复1case）；Panel隐藏Icon的无关断言已剔除，未把未布局隐藏节点当Part命中失败。原生NET8/最终AOT及清理后模块尚待下段结果。

最终候选成本再核：为避免typed回退，Native记录的是“本hook在原生挂载前亲眼观察的Selector对象与必要类型条件”，Selector对象变更或挂载前未知时全scope真实frame兜底；不是盲信当前Type。最终安静ABBA×3：typed611036B/9.07185ms→606276B/9.491ms，max11.1152→11.8735ms；class11852284B/151.44695ms→2076052B/60.7821ms，max173.9992→73.183ms。每侧12样本，所有24进程仍278完整检查；差额原样披露，不把typed+0.41915ms写为加速。

NET8实际原生最终源码：普通/名称/无类型完整轨迹均通过；扩展Application/Provider/实际Native PopupImpl及82 Steps输入、真实图标依赖/字体/标题、Panel两变体居中/清除后global扩展377PASS，root/class各278PASS。NET8 runtime8.0.24、官方Base12.1.4；不是只build。正在最终NativeAOT发布与运行。

最终NativeAOT：osx-arm64发布成功，无CS/IL诊断；官方Avalonia12.1.4和既定Skia不变。实际Avalonia.Native.WindowImpl原程序中：extended377（含82 Steps输入、依赖/FontFamily/标题、Panel两变体中心/clear、实际Native.PopupImpl的Root与Application来源退役），root278、class278均PASS/exit0。普通JIT的早期287已由额外Steps检查组成377，无虚增独立test计数。原NU1903及Homebrew dylib部署版本链接警告保留，不声明旧macOS兼容或零警告。

绘制末段实际新缺口与处置：同实例配置A→B→A，Light/Compact恢复0，Dark40/DC25非零，位置严格在五个progress ring边缘；Bounds/变换/Percent/line/brush/padding前后完全相同。用实际同一个Indicator.Render直接到独立RenderTargetBitmap，10个暗色ring的A/return逐RGBA全0。仅临时关闭test host RegionDirtyRect选项后仍28/16非零，故未将该开关当修复，原TestApp字节已恢复。上述红均保留。

只读采用Skia源码显示DrawEllipse直接Canvas.DrawOval，未进入已采用的完整geometry primitive重放；本次不改Skia/框架源码。Steps自绘底槽改用与弧线一致的EllipseGeometry→DrawGeometry，Rect/pen/brush不改，按Rect缓存几何而非每帧分配。四算法同实例完整配置/撤回12整图现在恢复全0；原初始默认的Dark40px/max4、DC25px/max2边缘被规范化到稳定重放，其余Light/Compact0，未设容差或标为原失败通过。这40/25具体默认变化仍需最终具体视觉审查；用户明确要求的Panel文字居中另计。

期间整图默认比较还揭示仅WhenExplicit返回Unset并不能重建被原template Local压住的其它状态默认，普通spacing/Small箭头曾出现18k/40k级差异，未保留。最终把三个兼容输入合并为ControlTemplate中的声明式状态/存在位/历史fallback MultiBinding，私有布局/arrow属性保持原始priority与初始时机，删除被取代的dead state setter；新StepsThemeConverters只服务这三个兼容标量，不写控件状态或属性。未配置整图已回到历史显示，除上述40/25规范化与已请求Panel中心外无差异。

包核验：Native/Core/Desktop本地pack均成功、两TFM资产存在，Core.nuspec包含Native6.2.8明确依赖，Avalonia仍12.1.4/NWayland0.11.0。首次Desktop --no-build因准备工具缺失失败，执行标准BuildToolchain复用本地编译器source cache后重pack成功，未修改工具源码/版本。纯NuGet消费者隔离package-cache按source mapping加载本次Core/Native包，NET10.0.8及NET8.0.24实际运行PACKAGE_CONSUMER_PASS；两侧实际加载Core/Native DLL逐SHA等于nupkg对应资产。不往官方/共享package cache写本次包，不发布。

末段兼容默认纠偏：仅在state Setter中WhenExplicit=Unset会让未配置项落到property default，不能替代历史template Local输入；整图抓出default geometry/spacing漂移，未计通过。最终在ControlTemplate以明确Type/SizeType、Own存在位、历史实际fallback组成一个声明式MultiBinding，三个输入由StepsThemeConverters域内标量converter选择；当Own不存在，布局/arrow保持历史初始priority/时机与其他状态。四算法当前初始整图对已保存旧默认：Light/Compact0，Dark40/max4、DC25/max2只属于上文进度环规范化；没有其他几何/颜色默认漂移。82字段资源声明默认不改。原说明“均移到style Setter”由本段实现事实纠正。

本轮当前代码manifest已冻结为25个实际生产输入（以会话实际数量为准）；后续只做最终验证/清理/文档和作者自审，不再叠加功能或平台门槛。最终AOT与清理门禁须对应该manifest；早期source身份仅保留历史，不混作最终证据。

临时测试清理：本轮Temporary/CustomThemeFollowups的6个源/AXAML文件、43217字节已删除；永久净新增0，仅既有StepsConnector消费检查补充合法Kind属性形态，不削弱原未消费断言。229项含82实际A-B-A、328默认及Native/生命周期/居中保护全部通过后删除。外部driver/媒体在最后身份和视觉批准核对后清理；清理后的原模块门禁继续执行。

Task 1/2与Task 3工程最终机制检查229PASS/0Skip（9s test，命令48.43s），包含既有Steps202以及27项临时保护，临时包含82消费者/328默认等子断言，不虚报为独立tests。最新geometry缓存与兼容fallback输入的同一生产manifest在最后NET8/NativeAOT重跑，临时源清理后按实际模块另验证。

## 最终批准与验收

2026-11-05，request_user_input_async / call_96e464d3f15548b4bffdbbde46c619ab / item0，用户原话：“批准这组进度环修复（推荐）”。仅批准1240×1040、五个进度环边缘的默认规范化：Dark40px/max4、Dark-Compact25px/max2；Light/Compact及其它区域不变。Rect/pen/brush不变、Geometry路径统一后的四算法配置撤回整图0成立。不扩大像素容差、任何其它旧FAIL、平台或性能批准。Panel两变体文字居中按用户本轮明确截图要求完成，原箭头/面板尺寸保持。

四项工程结论：窗口回收原诊断由Native脱离缓存hook修复；Application/Provider/Window/Overlay/实际独立Popup退役与无类型性能优化完成；独立Steps9/82的V2/实际消费者与A-B-A、字体/Global/Part/Panel修复完成；正式文档与CT21旧23字段/9节点/82字段及权威入口同步完成。原724/1244/90主键与范围仍在，无新增公共API/Token/resource ID。

最终生产manifest保持：本轮最后229项机制/模块组合PASS0Skip，随后真实NET8和NativeAOT同源码extended377/root278/class278全部PASS。原native失败、低层链接准备问题、错误host/命名空间/隐藏节点断言、未配置回退及40/25红的过程记录保留，不把准备失败算产品RED；进度环旧严格差异仍是具体批准范围而非通用0容差豁免。

删除临时验证源后，精确Core ThemeContextLease/Bridge/ScopeGraph/AssetContract/ManagerTransaction48PASS0Skip（452ms，命令9.38s）；精确Desktop Steps/Popups/Window/Buttons/ThemeContextLease/有效Global620PASS0Skip（10s，命令40.28s），合计668。未运行全量/affected-wide，没有源码/依赖/目标变化就不扩门禁。最后外部会话/媒体清理后以同输入复用产物作一次相同filter的no-build确认，见末段。

作者另步自审：按requesting-code-review标准对照原74be4a1a6与本轮25输入，检查原始触发、owner/订阅/cleanup、ValueStore固定边界、异常/重入、弱缓存、默认回退与几何、Schema/resource ID、真实包/NET8/AOT身份。新产物审计移除一个由旧Small Arrow Setter迁出后留下的空Style（无Setter/Child，不影响行为）；该AXAML清理后重新编译/发布必要目标，并更新manifest。其余没有未处理Critical/Important。遵守禁止子agent要求，为作者自审，未冒充外部独立评审。Visual批准严格保留具体40/25，未借“修复Bug”泛化例外。

最后记录核验：原36 NeedsDecision中6为本轮Steps内部组合节点，按实际owner完成；其余30全部为原CT14已批准的抽象/数据/布局/组合边界，改成InheritedContractAudited并注明不虚构唯一owner/identity或独立像素PASS。未把视觉计划其余PendingScene批量转PASS。724/1244/90主键重复均0、CT21旧in progress字符串0；23行已批准描述与82新真实消费分别记账。
官方evidence cleanup已实际删除77文件，audit clean/findings=[]/audit_error=null；包含所有原始/失败/候选/直接primitive及clip诊断媒体，未删除用户两张原截图。没有输出到仓库或暂存区，详细数值与视觉判断已先保存。最终其余会话删除及必要门禁在末段登记。

最后身份核对：25生产输入manifest不变，官方Base两个SHA与开始一致，参考Avalonia状态干净；所有Markdown相对文件链接缺失0。最后空Style清理后的NET8与NativeAOT extended377/root278/class278再次通过，无CS/IL诊断；本地Desktop包再次按两TFM最终产物重pack成功，Core/Native实际包加载核对保留其相同源码身份。

外部唯一会话将实际删除2864个专有文件/5947798701逻辑字节，含原归档提取/低层原型/驱动、全部新旧二进制profile、成本/绘制/诊断日志、NuGet包与隔离cache、初始/最终manifest。共享正常构建/工具链cache保留，未清理用户截图或其他任务目录。临时测试源码6文件43217字节已删除，official媒体77文件已删除；本轮不留下运行demo/诊断工程/临时测试，永久测试净新增0。

全部专有会话/媒体清理后：Core精确filter，Passed!  - Failed:     0, Passed:    48, Skipped:     0, Total:    48, Duration: 475 ms - AtomUI.Core.Tests.dll (net10.0)，总耗时9.27s，exit0；临时日志已关闭删除。

全部专有会话/媒体清理后：Desktop精确filter，Passed!  - Failed:     0, Passed:   620, Skipped:     0, Total:   620, Duration: 15 s - AtomUI.Desktop.Controls.Tests.dll (net10.0)，总耗时47.31s，exit0；临时日志已关闭删除。

## 最终关闭

全部专有外部会话/媒体/临时测试清理后最后同源码精确确认：Core48/0Skip（475ms，命令9.27s）、Desktop620/0Skip（15s，命令47.31s），合计668PASS/0FAIL/0Skip；git diff --check通过。post-cleanup没有CS/IL诊断，旧NativeAOT包安全/链接部署警告按此前具体身份保留。所有本轮临时目录、会话指针和Temporary路径实际不存在，用户原截图及共享cache保留。

四项任务已完成，无本轮待执行工程或待回复的视觉确认；CT24继续暂缓，Windows专项继续开放，其余旧具体批准/排除/原FAIL不擅自扩大。工作分支feature/custom-theme/HEAD74be4a1a6保持，本轮新代码未commit或push，交用户审查。新增plan仅intent-to-add便于review，不包含实际staged提交内容；没有创建分支/worktree/clone或子agent，没有NuGet发布。
