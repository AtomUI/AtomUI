# 首批目录、合同与运行基线记录

本文件记录CT-00–CT-02的实际取证；不表示产品重构或逐控件视觉验收已经完成。生产C#/AXAML未改动，源码基线为`85407138f`、分支`feature/custom-theme`。

## 目录范围

[Control目录](control-inventory.csv)与9个当前构建程序集、实际注册manifest及12个外部Avalonia主题目标双向核对，共720个程序集＋metadata name主键：255个ProductControl、62个AbstractContract、263个InternalTarget、140个OtherBoundary。Gallery页面和非Visual资源host单独分类，不计入255个产品Control。

元数据probe先因目录缺失产生有效失败，生成记录后1/1通过、无skip；删除一个无Own的已纳入Control也能被差集检出。初始缺失编译引用属于探针环境错误，不算该失败基线。实际编译引用取MSBuild Compile的ReferencePathWithRefAssemblies，不能只用ResolveReferences。

305项保留NeedsDecision：内部组合owner、资源host及两个开放泛型owner须继续核定；不能凭基类或目录伪造identity。两个公开泛型为ReactiveWindow`1及GalleryReactiveUserControl`1。除12个外部依赖目标外，元数据项均有当前源码位置。61个ProductControl未匹配到控件overview导航，文档完整性尚未通过。

## 兼容与合同分类

[字段审查记录](token-contract-review.csv)与编译manifest的95个Own终端、1,195个扁平字段逐键匹配。当前分类为Active 965、Derived 90、Legacy 17、Undecided 123；1,071项SourceChecked、124项NeedsDecision，**没有Approved项**。

分类是迁移提案。本次补查通过Roslyn语法展开局部变量、条件守卫及同类型helper，核对全部字段与1,882处C#/AXAML引用候选，并记录实际Style/ControlTheme selector和目标属性。外部函数的纯度、运行时分支命中、字段范围及真实像素仍须家族核定。缺少扫描直接引用不等于无效字段，不删除schema，也不凭名称发明范围。

| 代表合同 | 已有事实及后续边界 |
| --- | --- |
| Button | ContentFontSize→ContentLineHeight→ContentPadding是可修改依赖链；显式padding须保持。FontWeight等8项未决，归CT-12 |
| LineEdit/Select | 现有输入与内部框体投射须分别核对；LineEdit 1项、Select 3项字段未决，归CT-13 |
| Skeleton | 不能以原整体默认evaluator证明覆盖后派生正确；15字段中的渐变helper已确认读取两个Own端点；TitleHeight及可空brush效果仍须家族核定，归CT-17 |
| Timeline | 专用Semantic Style必须在Arrange后仍生效；11字段中1项未决，归CT-15 |
| DataGrid | 找到25项默认复制关系，其中17项旧入口有实际typed消费者，只有这17项列Legacy；其余8项不伪称有效别名，归CT-06/CT-18继续决策 |
| ColorPicker | 保持同一已激活渐变stop检查前景动态更新；具体终端identity按目录核定，不凭控件名推断token名，归CT-19 |

## 代表反例B01–B08

环境：macOS 26.6.2 / osx-arm64、.NET SDK 10.0.300 / runtime 10.0.8、Avalonia 12.1.4、Debug。测试使用现有Headless环境和真实ThemeConfig/模板；不是Skia或原生绘制通过证据。

| 场景 | 输入/目标 | 实际结果 |
| --- | --- | --- |
| B01 | 根LineWidth=1，Button identity覆盖4 | Button BorderThickness=1，FAIL；后续兄弟断言未执行，不宣称兄弟隔离已通过 |
| B02 | ControlHeight=50、LineWidth=2、ContentLineHeight=10，无padding覆盖 | Padding纵向12，期望18，FAIL |
| B02正对照 | 再显式ContentPadding=3,7 | 纵向7，PASS |
| B03 | LineEdit Padding=11,13、BorderThickness=3、CornerRadius=0 | 实际frame分别0、1、6，三个FAIL |
| B04 | DataGrid HeaderBg=#123456、CellPadding=3,5 | 实际表头#fafafa、cell padding=16,16，两个FAIL |
| B05 | AXAML生成的TimelineItemIconStyle：border=3、radius=0 | Arrange后border=3通过，radius=5.5失败；不宣称两个Setter均无效 |
| B06 | 同一ArrowIndicator ArrowSize 8→16 | DesiredSize.Width仍8，FAIL；失败后的恢复断言未执行 |
| B07 | 已打开Popup owner font size14→20，连同Family/Weight/Style改变 | 两种overlay请求均保持size14/Normal/Normal，两个FAIL；两者实际TopLevel均为AtomUI Window，不能据请求false宣称测过原生PopupRoot |
| B08 | 保持ActivatedStopIndex=1，Foreground Blue→Red | 目标文字仍Blue，FAIL；初始Blue和index不变对照通过 |

精确运行`CustomThemeBaseline.PreciseThemeBaselineTests`得到11例中10 FAIL/1 PASS；`CustomThemeBaseline.DataGridThemeBaselineTests`得到2 FAIL。合计13例、12个目标合同失败、1个正对照通过，零skip。探针中的节点基数、窗口overlay宿主、编译设置错误均已与产品问题分开，不列为产品缺陷证据。

B03/B05/B06/B08的失败属性与几何已记录，逐项真实像素、原生Host、恢复和释放仍未闭合；这些是后续任务的原始反例，当前没有修复为green。

## 脚本化视觉基线 B09

按用户要求使用自驱动场景，不使用Computer Use或桌面坐标输入。复用`AtomUI.Desktop.Controls.Rendering.Tests`的Skia合成器、`UseHeadlessDrawing=false`；实际场景使用AtomUI Window，ClientSize=640×380 DIP，缩放1×/2×，en-US，固定文本和数据、无鼠标/焦点输入。

每场景包含Primary、S/M/L及disabled Button、只读LineEdit、已勾选CheckBox/RadioButton/ToggleSwitch和Timeline/TimelineItem；字体为当前AtomUI Alibaba Sans复合字体链。Light/Dark使用各自算法，Compact使用Default+Compact；既有custom配置为ColorPrimary=#722ed1、BorderRadius=2、FontSize=16。未关闭产品动效。

脚本自动Show→等待真实布局/既有Transition时长及连续稳定帧→A/A→仅把场景容器左padding增加1 DIP→还原同一实例→重建相同场景。稳定等待有3秒上限，逐帧推进；不会设置像素比例容差。初版仅等待连续相等帧，漏过使用真实时钟的标题颜色过渡，已由还原比较检出；修正采样条件后保留原断言重跑。该过程不是产品改动。

| profile | A/A 1×/2× | 1 DIP差异像素 1×/2× | 还原与重建1×/2× |
| --- | --- | --- | --- |
| Light | 0 / 0 | 991 / 3153 | 全部0 |
| Dark | 0 / 0 | 1029 / 3390 | 全部0 |
| Compact | 0 / 0 | 959 / 3041 | 全部0 |
| custom | 0 / 0 | 1075 / 3264 | 全部0 |

正对照实际Button左边界34→35 DIP，尺寸保持；差异图集中在该Button文字、边缘和阴影，未影响其余控件。已打开并查看关键原图及差异图，明暗文字继承正常、控件内容及布局完整。最终精确filter `CustomThemeBaseline.ScriptedThemeVisualBaselineTests`执行8/8 PASS、零skip，18秒；构建有一条已有ButtonFocusVisualRenderingTests的xUnit1051警告，未修改该文件。

这是**同版本流程检错与代表静态场景**，不是重构前后全控件验收。[视觉安排](visual-coverage.csv)为720个目录对象分别记录owner、任务、宿主、维度/状态候选和剩余工作；候选需逐家族确认适用性，不能当作完整场景。原生后端、Native/Overlay、输入路由、动效过程、其他OS和Browser均未由B09证明。

## 性能采样

临时`ThemePerformanceBaselineTests`精确运行1/1 PASS。现有HeadlessDrawing、Debug、初始化测试App内首次构造，场景11个Button、164个visual；首次显示426.3795ms、当前线程分配7,589,664 bytes。每项5次预热、30次采样，断言实际字体/brush及scope隔离；popup使用已经打开的Overlay。

| 场景 | 中位数ms | p95 ms | 当前线程分配中位数bytes |
| --- | --- | --- | --- |
| root等价请求NoOp | 0.0138 | 0.0176 | 2648 |
| 局部Global字号 | 18.475 | 25.8738 | 2017104 |
| 局部Own字号 | 19.71 | 22.8783 | 1970296 |
| 嵌套scope字号 | 1.20855 | 1.4838 | 164696 |
| 已打开Overlay主色 | 0.6131 | 0.6998 | 64240 |

不是进程冷启动，不是Skia/原生绘制耗时，不是全线程分配或泄漏证明。真实冷启动、原生性能、WeakReference/释放及改动后对照仍待CT-02/CT-25补齐，不宣称性能无回退。

## 原生Host补证

外部临时自驱动`NativeReadiness`引用当前worktree项目，最终构建0错误；PID 1657、macOS/Avalonia版本同上，实际`Avalonia.Native.WindowImpl`，ClientSize 720×660 DIP，缩放2×。进程自行退出，exit=1来自以下6个目标合同失败，没有harness异常。截图使用原生树的`RenderTargetBitmap`即时绘制，**不是OS/合成器最终显示面截图**；不能据此证明原生阴影、自绘或不闪烁。

| 场景 | 原生Host实际观察 |
| --- | --- |
| B03 | root padding=11,13、border=3、radius=0；真实可见`PART_ContentFrame`为11,4、1、6，bounds=280×32。此前Headless的内部EffectiveFramePadding=0与该节点不是同一观测量，不混用 |
| B05 | 可见icon bounds=11×11，border=3、radius=5.5；另一个0×0节点radius=0。只宣称可见节点被Arrange覆盖 |
| B06 | 同一ArrowIndicator输入8→16→8，DesiredSize始终8×4；更改未更新几何，恢复仍为8×4 |
| B08 | 通过公开Focus触发到达默认首stop，不改选择；初始目标文字Blue，Foreground改Red后仍Blue，bounds=96.5×17 |
| B07 Overlay | 实际TopLevel=AtomUI Window / WindowImpl；已打开后owner size14→20并改Family/Weight/Style，文字仍14/Normal/Normal/原复合字体 |
| B07 Native | 实际TopLevel=Avalonia PopupRoot / PopupImpl；同样保留旧字体。此次已证明确实创建原生popup，不再只凭请求布尔值判断Host |

窗口激活后同版本A/A的1440×1320 RGBA精确差异为0。已查看主场景、箭头、渐变与Native popup关键帧；未看到箭头变大，渐变目标文字仍蓝色。即时绘制中的渐变色谱空白及阴影形态不能据此列为产品Bug，须在实际合成器路径另行核对。后续的scene-b/scene-return之前已改变字号等状态，**不能与最初scene-a作同状态返回比较**。

这一个成功完成的Debug进程Main入口到首个原生帧通知1507.9114ms，父进程launch到通知2499.39075ms；不是冷磁盘/首次GPU初始化统计。性能场景为局部scope中的11个按钮，另有owner按钮及其他反例控件。每项5次预热＋30次采样；更新类指标包含等到下一帧回调的调度时间，不等于纯Compiler耗时或GPU完成时间。

| 原生采样 | 中位数ms | p95 ms |
| --- | --- | --- |
| root等价NoOp | 0.0142 | 0.0272 |
| 局部Global字号 | 16.63245 | 17.5314 |
| 局部Own字号 | 16.65065 | 17.3562 |
| 嵌套scope字号 | 16.65205 | 17.4591 |
| 已打开Overlay主色 | 16.6544 | 16.7755 |

记录了各次进程总分配差值，包含其他线程工作和采样开销；不能与此前Headless当前线程分配混比。关闭并拆除owner、popup、label、root、window后，两种实际Host的这5个WeakReference均在最多5轮帧推进/GC内失活；该单场景不代表全库无泄漏，也不是产品修改后的对照。

前两次原生尝试的观测节点选择、首次窗口激活和性能popup父级错误均已单列为探针问题，不计产品失败。原生补证只补齐上述Host事实和基线样本，既有6个合同失败没有修复。

## 按钮连续闪动补查 B10

用户在可见原生验证窗口观察到按钮一直闪。核查驱动后确认：原生性能脚本的Global、Own、嵌套字号场景各连续交替配置35次（5次预热＋30次测量），另外连续改弹层主色。该安排会主动制造快速画面变化；程序已退出，不再在可见窗口重放此循环。之前仅取稳定终态没有覆盖连续闪动，不能用A/A零差异否定用户观察。

随后使用现有Headless Skia真实合成器离屏验证，保留同一12个Button、原字号scope结构和开启的产品动效，ClientSize=520×620 DIP、1×、相同字体/默认主题。精确filter：

~~~bash
dotnet test tests/AtomUI.Desktop.Controls.Rendering.Tests/AtomUI.Desktop.Controls.Rendering.Tests.csproj \
  --framework net10.0 --no-restore --filter 'FullyQualifiedName~ButtonIdleStabilityProbe'
~~~

| 阶段 | 实际采样 | 像素/字号观察 |
| --- | --- | --- |
| 初始A不改配置 | 2013.0564ms / 69帧 | 每帧相对稳定首帧0差异，字号变化0次 |
| 同实例公开API交替14/16 | 35次 | 35/35次画面变化，每次11,717像素；有效字号与请求相等 |
| 停止于B | 2008.1474ms / 71帧 | 每帧0差异，字号变化0次 |
| 恢复A后继续观察 | 2003.3097ms / 69帧 | 初始A与返回A精确差异0，继续每帧0差异、字号变化0次；12个实例保持 |

临时用例1/1 PASS、零skip；已查看初始/停止画面和差异图，变化集中于provider内11个按钮文字及外框，owner不变。以上证明此离屏场景的变化由驱动触发、停止后不自行振荡，**不证明macOS原生显示面无闪烁**；该用户观察的原生边界仍待专项复核，不能报告“产品闪烁已修复”。两次编译缺少namespace属于临时探针错误，不算产品RED。

正式验收规范已加入持续静态观测、驱动操作记录，以及高频性能采样默认离屏的要求。未修改产品动画、ControlTheme或控件实现。

## 当前门禁与处置

CT-00元数据集合与逐键字段记录已建立；owner未决及字段待决全部保留。CT-01源码级依赖/消费者分支已补查，123项未决字段与124项决策继续作为各家族开工门槛；CT-02已补原生Host、单场景释放与启动/热更新样本。M0代表基线已完成独立审查并允许下一阶段规划；原生最终显示面及无驱动闪动尚未完成CT-12/CT-25专项复核。原90组审计问题不因本次取证自动改为已修复。

用户要求先完善脚本化视觉验收skill，已升级现有技能并独立前向验证：同一Button/Frame主色A→B→A，A/B差异4,144像素、返回A零差异，1 DIP正对照检出704像素；脚本和实际视觉审查均通过。该结果只覆盖相应单Button/Skia场景，不补齐上述原生后端等缺口。

此前首轮临时probe/测试/AXAML/helper已删除；两个实际取证目录分别清理148和12个文件，均removed=true、findings为空、audit_error=null，临时媒体未进入项目。Gallery已核实的PID 43514已停止；只读元数据及文本过程记录留在本计划工作目录。首轮清理后精确既有检查：Desktop ButtonThemeScopeTests 6/6、Rendering ButtonFocus 1/1、DataGridTokenAlignment 1/1、Generator完整schema 1/1通过，零skip。

本次原生补证与B10的媒体目录另分别清理12和6个文件，均removed=true、findings为空、audit_error=null。原生进程1657已核实退出；外部原生Demo、Roslyn合同probe及B10临时测试源码均已删除。清理后重新构建并精确运行既有Rendering ButtonFocus 1/1 PASS、零skip（570ms），仅有原文件已有xUnit1051警告。720个目录/视觉主键、1,195字段与编译manifest逐键及路径/任务校验通过，git diff --check通过；生产src/controlgallery/tests无最终差异，没有运行全量或提交。

临时验证不建立永久失败测试或截图库。后续对应任务改码前依据相同配方重新采前证，改后保持同一有效断言和原始基线比较；未覆盖的产品及平台门槛继续保持未完成，不能用M0交接结论替代。
