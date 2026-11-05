# CT19 ColorPicker有限家族出口

2026-11-04接续；按tasks.md及C1–C10既有记录执行，主agent沿现有checkout/feature/custom-theme，不创建分支/worktree、不用subagent、不自动提交。CT12–CT18不重开。

## 有限完成条件与执行顺序

1. [x] 恢复22 metadata/24 Own的实际状态、既有批准和有效证据；保留原始失败与平台边界。
2. [x] 逐24 Own和22 metadata闭合消费者、实际owner/Global/字体；四个公开root与内部绘制/输入/色板/滑轨职责准确，不按目录猜identity。
3. [x] 专用AXAML Semantic Style在纯色/渐变trigger与真实popup命中、优先级与撤回；有限Custom、状态/HEX-HSV-RGB/Alpha/确认草稿/相关布局与挂载、缓存/释放出口。
4. [x] 单一参数驱动的必要明暗旧新、A/A、1DIP检错、同实例恢复及当前Mac NativeAOT；实际窗口/布局、渲染与显示面分别记账。仅必要有限场景，不全组合/全平台扩门槛。
5. [x] 源码稳定后合批构建/采样，清理临时源码/driver/媒体/专用产物，再精确模块/NET8/成员及diff检查；正式文档、两CSV/tasks和作者另步自审一致；新视觉需具体材料后限定审查。

## 复用、排除与恢复结论

C1 24字段DependencyDefaultsV2/rev1、四依赖边已完成，不重新迁移。C2 Alpha/按钮Own宽度已接线，未明确时Alpha历史48、输入字体11/通道48/容器262保留；CSV两项NeedsDecision是陈旧状态。HandlerSizeSM经SliderSize/ThumbSize依赖消费，不作为废弃槽。C3 legacy formatter、C4斜线/选择环、C5 slider请求顺序/重挂、C6输入草稿与重挂、C7 spectrum请求/范围、C8六模式/SV默认、C9焦点/棋盘与C10单分量键盘已有实现、具体批准、清理和有效验证，分别按权威结果复用。既有自然合成差异接受仅限原表，不扩到新场景/当前原生最终显示。

C9首轮NativeAOT383/388的四事件缺失历史保留（后续388不倒写首轮）；CT25/FW1/FW2既定边界保留。无新CI/推送/NuGet发布，完整CT20不纳入本包。纯色/渐变原数据、默认/空值/双stop、六分量、Alpha、正常motion、输入/确认草稿及框架焦点树保留，不通过删内容/裁剪/交互或重建一侧消差。

真实剩余集中于：四个公开root和asset-only内部主题的Global与字体来源、专用Style/真实popup目标、有限Custom与全家族实际消费/生命周期/最终出口。旧总表仍说C7待做，实际C7–C10均已批准/验证，不重复解决。

## 环境与改动保留

开始HEAD35f9cc8f5，分支feature/custom-theme，350个既有工作区/未跟踪改动及index/debug完整记录于本包仓库外标记会话；75个ColorPicker源/现有测试原字节已备份。仅两处原有Theme.IsMotionEnabled接线改动属于此前切片，完整保留。共享`.artifacts/verification/run.lock`非阻塞检查可用，后续构建/采样持锁，compat/Avalonia.Skia复用已批准构建，不改官方缓存/native。

Ruling：沿既有C1–C10合同执行当前消费者缺陷迭代，不另造功能短设计；只有确证需要新公开合同/默认语义时才补限定设计与材料。代价是必须由实际RED及真实树/来源证明改动，而不能把静态Shared命中直接判为缺陷。

## 实际反例与工程检查

临时精确过滤`FullyQualifiedName~Temporary.CT19Exit`，Release/net10.0，逐批持共享锁。四root的BorderThickness/背景/文字、View Padding实际4例RED；随后View字体、注册Collapse内色板圆角/间距、Custom根样式3例RED；实际checker颜色与trigger ColorBlock边框2例RED。相应最小修复后14项通过，扩展Own与同实例撤回/隔离/离树重挂/草稿/格式/Alpha后23项通过。新增纯色根Foreground不投射到实际文字的有效RED，AXAML模板绑定修复后连同四root释放检查共25项PASS（20s，命令43.25s）。专用AXAML八类Style在实际popup和触发目标命中、移除class撤回；未改Semantic route，没有查找节点后设置语义样式回退。

Own的23直接消费者及HandlerSizeSM两依赖已核对；两类View配置Own后检查实际spectrum/slider/track/thumb/preview/input/palette/mark/shadow/间距，撤回保持实例且恢复原值；trigger双形态覆盖Padding/文字Margin/棋盘/禁用opacity。触摸大环及明暗环分支沿既有源码和C1/C4证据复用，不声称当前普通指针采样覆盖触摸。原依赖公式Preview=2×Track+UniformlyMarginSM，当前默认28；初版测试误把MarginSM当8（实际12）已更正，不计产品RED。无确认模式时IsInputValid不随每次无效文本更新，初版测试改为既有确认草稿场景；两类View离树后“GG”/光标1不丢失。最初popup空值导致不可见目标未取值，恢复原纯色Red/双stop数据后通过，无route缺陷。准备编译错误均不计反例。

Global/字体映射由ControlTheme声明，四root实现只读内部IThemeInheritanceSource继承父桥；不新增公开API/Token/key/slot，固定字号11、通道48、谱图与圆形计算语义保留。Palette沿实际View锚点跳过登记Collapse，避免误用Collapse或一般共享资源。Custom只释放根CornerRadius/FontSize的preset selector覆盖，base仍采用原Middle回退；内部ColorBlock的Custom计算不改。

两基类另步纯成员整理：布局前后268个成员（含本包两个只读桥）语法逐项完全一致，61个静态字段相对顺序不变，初始化值不变；2文件/2控件/0问题。初版外部重排脚本将UTF16索引当Python索引造成临时重复前缀，检查识别后按真实class声明修正，失败准备日志保留，没有用于有效产物。

## 有限采样合同

同一仓库外参数驱动，1200×820、正常motion、纯色ARGB(155,50,100,180)和同色/Orange两个stop、同三项预设组。Headless Skia预检72帧/258断言/6原始按键通过；该预检早于Foreground及真实stop点击补充，仅作准备，不计最终包。最终轨迹使用真实gradient stop指针命中、原始平台按键，明暗各main/独立A-A/独立1DIP三个实例；base→Global→根Foreground→Own→clear→popup专用Style→已开popup资源更新→Custom→disabled→RGBA→HSVA→keyboard→同实例恢复。两侧的焦点、指针、状态、扰动历史完全相同，probe仅将场景左边距增加1 DIP。另做真实独立PopupRoot的Style/作用域更新及关闭检查，不将主窗口截图当独立弹层显示面证据。

每次采样先覆盖700ms正常动效，再分别确认至少三张相同即时RTB及Headless compositor帧；真实Mac NSWindow断言client/scale/active，并单独抓取窗口最终显示。RTB、Headless compositor、窗口布局及屏幕图分账，所有比较零容差、整图、不裁剪/遮罩；新变化、A-A、恢复与跨运行差异分别登记。旧版只回放本包11个源文件的进入时原字节，既有两处motion接线也保留，finally恢复候选并逐字节核对，不替换DLL绕过StartupImage身份。

### 准备失败、修正与有效边界

首次恢复使用`view.Value=原输入`，但单独View的编辑HSV通过ValueChanged通知、外部Value未变化，赋同值没有恢复编辑状态，造成29175像素差异。保留该原失败与完整13步轨迹；只把恢复改为同一路由Right→Left，模型/RTB/compositor均恢复0，不改产品同步语义。最初观察把差异误归渐变滑轨，实际差异范围x273–533属于纯色面板，后由两View/slider/track实际值定位，未据误读修产品。

独立PopupRoot检查两次中止（Headless准备与首Mac预检），后补实际类型记录显示最外层是Avalonia.Controls.TopLevelHost，Popup已开、requested=false、实际Overlay=false；本地参考框架源码确认该类型是PresentationSource的外层视觉root，真实PopupRoot应由TopLevel.GetTopLevel读取。它们不是产品反例，也不计最终PASS。原生预检另一版记录同样类型断言失败；修正观测不改Popup/平台代码，不把主窗口图片称作独立弹层的最终显示。

Own预检280谱图/260预设区在两个完整View并排时超出1200视口，原截图保留失败准备。最终采样参数改为250/230（仍覆盖加宽，Alpha60/按钮19/handler20/预设色30保留），并逐帧检查四root实际边界完整在固定1200×820内；不缩放、裁剪或删内容。800×600单View工程检查仍使用280/260，原始有效目标消费证据不替换。

补充Global别名发现渐变MarginToAnchor错误读取ColorPicker身份：配置GradientColorPicker.UniformlyMarginXXS=9时实际4，精确RED成立；仅改AXAML为OwnerTokenResource后连同26临时和20既有检查共46PASS（22s，命令36.42s）。最终轨迹增加实际gradient popup的独立owner/9DIP属性消费，仍不改变共享Popup的定位算法。

首个原生预检14阶段完成，NSWindow/scale2/active/原始输入/最终显示捕获均实际执行，但结尾独立Root类型断言失败，整批不算最终验收。补观察的第二预检明确是TopLevelHost类型，而不是宿主失效；最终使用TopLevel.GetTopLevel确认真实PopupRoot。首个AOT发布85.57s成功，只有5条既有Homebrew最低版本链接警告，无新增trim/AOT诊断；它早于下面四处布局补漏，不能充作最终源码验收。

最终消费者扫描发现两类View各两处调色区/确认页脚SpacingXS仍是Shared；实际两个StackPanel的配置7、观测8，两个View分别形成有效RED。仅四处AXAML改为Owner；采样driver用真实IsNeedConfirm打开确认区，在已开popup中配置SpacingXS13并断言实际页脚值。此补漏属于既定Global消费者出口，源码范围合计13文件，旧版字节回放也随之扩为13；不触及共享Popup/布局算法或既有确认业务。源码和driver稳定后再合批对应构建/发布/采样。

四处Spacing修复后合批精确过滤`Temporary.CT19Exit|ColorPicker`：28临时＋27既有＝55PASS/0Skip，23s（命令37.54s）；当前产品源码稳定，成员审计2/2/0因两个C#文件字节不变复用。13个源文件的实际修改不含Token定义/Core/共享Popup/native依赖，文档同步和CSV仅CT19的22/24行。ControlTheme实际Shared消费扫描只余既有MotionDuration值工厂，不把它们当作控件owner实例资源重绑。

最终13文件对应AOT发布82.89s通过，5条原有链接警告；未改官方缓存/native。完整Headless旧709检查/新763检查，各12原始按键、84帧成功捕获。两侧各28 RTB和28 compositor A/A为0、各56组1DIP均检出，清Own恢复0；完整恢复每侧每面6对均550像素/max96，严格FAIL保留。定位范围仅gradient trigger色块24×24：驱动Brush漏了既有C10水平Start/End端点，构造默认是对角线，原GradientColorPicker在挂树Value更新时固定转为水平（原源码明确保留该业务），打开gradient popup后与初始对角线不同。不是本包回归或噪声，不增加容差/改产品，也不把后续正确数据当作修复该旧语义；补回原水平基准后重放，原失败批继续单列。驱动完整数据恢复不足是本次准备责任，不重开C1–C10、CT12–CT18或扩展方向编辑能力。

### 画面复核发现的主题准备错误

补齐水平基准的AOT发布85.07s；Headless旧709/84、原生新1195/84及旧1135/84实际执行，但画面复核发现标为dark的窗口仍是明色，light/dark基准RTB整图0。原因是驱动误将Provider.RequestedThemeVariant当作AtomUI主题切换；ThemeConfigProvider.SetCommittedVariant会按实际ThemeContext覆盖它。该责任属于本次驱动准备，全部“暗色”标签及重复明色结果不计暗色覆盖，已向用户明确更正。原生窗口、实际输入、布局和像素捕获确实执行，不能把它们与正确暗色混称；其中light的独立历史仍有效。

正确切换复用现有ThemeManager.ApplyThemeAsync＋ThemeConfigBuilder.WithAlgorithms(Default/Dark)，与既有Window暗色初始上下文测试一致；逐帧同时断言Window和Provider的实际ThemeContext.Appearance。只补同一完整轨迹的真实dark，明色有效产物/结果复用；AOT按修正后的参数驱动重建。C9原始缺口、此前FAIL、未配对或标签错误帧都不改写为通过。

Custom媒体使用带class的应用Style，该优先级在旧版已能生效，旧新CornerRadius13/Font21和像素均相同；最初比较器误预计该场景非零的12对预测FAIL保留，按实际一致模型归为兼容场景，不改像素容差。无class普通应用Style的旧6/14→新13/21由精确原始反例证明；不把class媒体宣称为该缺陷的原生前后复现，也不扩大成所有Custom组合验收。

焦点协调请求`request_user_input_async/call_8ab33c1995644dbc85cd27efce77b329/item0`用户回复“现在可以连续保持焦点”，仅是环境协调，不是功能视觉或Git批准。发现主题准备错误后已通知释放焦点，不要求用户为无效暗色继续保持前台。

真实Appearance修正后的AOT发布86.84s通过，产品13文件指纹保持；driver SHA256 `f1de13f6976f86397934633c549899ff98dedf0803a808198a15636d45566300`，AOT `bf95d173d75908735d269c99680180035a4740050aa90211bf621edf0a23bb04`。正确暗色在首批窗口双重Appearance断言为Dark，已查看实际深色窗口/文字/棋盘/选择环；不再以标签代替主题状态。有效明色从原Headless旧版、原生旧/新各42阶段复用，共252张PNG逐字节复制到明确登记的最终角色，来源/目标/SHA在仓库外receipt保留；不复用任何错误dark标签。新版Headless明色因此前漏渐变端点未形成该正确数据完整批，不从错误数据补齐，重新完整采样。

产品作者另步源码自审（主agent自行执行，无subagent/独立评审）：只读继承父桥与AXAML消费接线，无新可变字段、缓存、定时器、公开API、Token/slot/key或Semantic Part；FontFamily观察由现有基础设施拥有，4root关闭GC已验证。既有代码绑定只余Popup只读flip回传及运行时Content presenter，均有ControlTheme不可等价表达的具体注释；无可迁移模板绑定遗留。13文件Global消费扫描无实例Shared残留，MotionDuration值工厂保留。两个C#文件268成员语法/61静态字段顺序不变，2文件2控件成员0问题；C1–C10绘制/输入/键盘/缓存/生命周期批准和有效证据按原权威记录复用。视觉及最终清理门禁尚待，不提前写完整出口通过。

## 24 Own与22 metadata逐项消费结论

24项均为Active，零新Reserved/别名；V2/rev1、名称/类型/默认/key/slot/编辑入口保持，分类并非新迁移。

| Own（合计24） | 实际目标与核对 |
| --- | --- |
| ColorPickerWidth、ColorSpectrumHeight | 两View中的实际Spectrum MinWidth/MinHeight；配置280/180→撤回234/160，同实例。媒体视口使用250/180的有限加宽值。 |
| ColorPickerInsetShadow、ColorBlockInnerShadows | Spectrum/两类slider轨面，以及preview/色板项内部Border；实际Spread3/2命中、撤回，默认不重算或截断。 |
| ColorPickerHandlerSizeLG、ColorPickerHandlerSize、ColorPickerHandlerSizeSM、ColorPickerHandlerLightColor、ColorPickerHandlerDarkColor | 实际选择环大小与明暗笔画；普通环20→16及Orange/Purple角色命中，LG触摸分支沿C1/C4和未改分支复用，当前媒体不冒称触摸。SM通过SliderSize/ThumbSize依赖实际20→24→12、22→26→14。 |
| ColorPickerSliderSize、ColorPickerSliderTrackSize、ColorPickerSliderThumbSize、ColorPickerPreviewSize | 两类slider/实际Track/Thumb/Preview；18/10/22/36明确值命中；Track10/12依赖Preview32/36，默认28；独立明确值优先。 |
| ColorPickerAlphaInputWidth、ColorPickerInputNumberHandleWidth | 实际Alpha NumericUpDown Width60、SpinnerHandleWidth19；撤回48/16，资源默认44和明确44/0合同复用C2，7个数值输入字段消费按钮宽度；字体11/通道48不改。 |
| ColorPickerPresetColorSize、ColorPickerPresetPanelWidth、CheckedMarkSize、ColorPickerPresetColorGroupPadding | 实际色板Frame30、Panel260、mark17、Collapse ItemContentPadding2,6；默认/依赖及实际选中沿C1，当前撤回和实际节点命中核对。 |
| TriggerPadding、TriggerTextMargin、SliderContainerMargin、TransparentBgSize、ColorBlockDisabledOpacity | 两触发器Padding5→3/description Margin9,0,6,0→原默认，实际SliderContainer Margin0,0,14,0；slider/ColorBlock棋盘tile6及禁用opacity0.25→1；C5/C9缓存/离树更新复用，当前原实例配置/撤回与四root关闭释放通过。 |

22 metadata＝4公开根＋2公共抽象合同＋16内部目标。公开根分别是ColorPicker、GradientColorPicker、ColorPickerView、GradientColorPickerView，四Global身份独立；共享Own只在ColorPicker身份。AbstractColorPicker/AbstractColorPickerView只提供继承合同。

内部ColorBlock由两触发器和两View实际拥有；ColorPickerPopupRootFrame由两触发器模板拥有，Native与overlay都保持直接Child/箭头mask转发。两类slider、单滑轨Track/Thumb/按钮、Spectrum、Input、palette group/item及ColorPickerCollapse由实际View拥有；GradientColorSlider/Track/Thumb只在GradientView中。Collapse自身沿既有Collapse主题，色板绘制/间距通过View锚点避免误取该登记primitive。内部对象不新建公开identity/Own或Semantic Part。完整22行只更新所属类型/状态/关键链接，不推断其他家族合同。

## 限定视觉审查范围（V1–V3已批准）

| ID | 具体变化与边界 | 工程证据 |
| --- | --- | --- |
| V1 | 四公开根及实际内部目标消费有效Global：背景/边框/状态/圆角/棋盘/间距；色板锚定View，调色区与确认页脚SpacingXS，渐变锚点UniformlyMarginXXS。默认常量与共享Popup定位算法保持。 | 实际字段/节点RED→GREEN、配置/撤回/已开popup/两host/状态/目标布局；24 Own沿C1/C2复用不再迁移。 |
| V2 | 有效字体与真实继承父消费；纯色description通过TemplateBinding响应根Foreground，保留原生本地/Style优先级和固定数字字号11。 | 字体/Foreground有效RED→GREEN，四identity隔离、继承/本地清除、两View带无效草稿重挂及四root GC。 |
| V3 | Custom普通应用Style可覆盖根圆角/字号，未设置用Middle回退；高度/内部色块原计算、固定几何/业务语义保持。 | 无class普通Style原6/14→新13/21反例；Native classStyle几何为兼容正例，不冒称该普通Style前后复现；完整CT20不纳入。 |

八个生成专用AXAML Part Style实际目标命中和撤回是既有契约验证，不新增部件/route或代码找节点定制。批准在全部必要验证、媒体审阅、清理及精确出口后申请；此前C1–C10及CT12–CT18不重开，没有Git授权。

## 正确主题的当前证据与清理进度

正确旧Headless dark-only：440断言、6原始键、42完整阶段通过；其明色42阶段从有效原历史复用。新版正确明暗：933断言、12原始键、84完整阶段通过，模型逐帧包含实际Appearance。当前有效材料的660对严格比较全部符合预期：其中新旧Headless两面168对、原生有效明色两面84对；A/A、1DIP和恢复分别计数，完整后汇总，不把尚缺暗色原生或AOT配对算作通过。

9个Temporary源码文件/30091字节已删除（28项临时检查，常驻测试净增0），只保留仓库外文字备份至审查清理；清理后ColorPicker精确27门禁正在执行。NET8 Release精确模块构建36.70s，0警告/0错误，产品源码指纹不变。详细截图、驱动和专用产物尚用于剩余原生配对，不提前报全部清理。

清理后精确`FullyQualifiedName~ColorPicker`实际27PASS/0Skip，测试2s（进程46.08s）；NET8结果及2/2/0成员结果复用。已审阅并记录的错误准备/重复1118张PNG（135500523字节）提前清理，保留504张最终角色有效图和明确来源哈希；252张复用明色文件SHA仍一致。未删有效未审阅原生/AOT材料，最终标记目录审计清理和全部driver/备份/专用产物删除仍待。

真实dark的当前Mac managed两版已完成：新683检查/6键/42阶段（86.82s），旧653检查/6键/42阶段（83.98s）；各阶段保留1200×820 client、scale2、actual Appearance=Dark、active及四root边界断言。独立PopupRoot真实类型、专用Style、已开资源更新和关闭检查同时通过，主窗口屏幕捕获不冒称独立弹层的屏幕取证。每版再与已验证的明色42阶段配对，共84正确主题阶段、12原始键；不把旧重复明色1195/1135总数充成两主题断言数。

## 最终有效像素、模型与原生结论

最终修正驱动的Mac NativeAOT完整明暗1365断言/12原始键/84阶段通过（175.71s），实际Runtime=NativeAOT；Window/Provider Appearance、1200×820 client/scale2、active、真实stop命中、键送达和四root完整边界均断言。独立PopupRoot的真实类型、专用Style、当前资源更新及关闭同样通过；它的屏幕像素没有从主窗口图推定。18个有效主NSWindow覆盖旧/新managed和当前AOT的两主题×3独立角色，前期错误/重复批不并入。

840/840张最终有效图完整存在，无缺帧或未配对图，Headless RTB/compositor与原生RTB/display分面比较，不跨后端混比。JIT/AOT84阶段的实际几何/模型/scale逐项一致，active均true。产品13源文件全程冻结指纹一致。

| 零容差对照 | RTB | Headless compositor | Mac最终显示 | 总计 |
| --- | ---: | ---: | ---: | ---: |
| 独立A/A（全部0） | 140 | 56 | 84 | 280 |
| 1 DIP正对照（全部检出） | 140 | 56 | 84 | 280 |
| 原实例base→clear及base→恢复（全部0） | 60 | 24 | 36 | 120 |
| old/new（按有限行为预期） | 168 | 84 | 84 | 336 |
| JIT/AOT（全部0） | 84 | — | 84 | 168 |
| 合计 | 592 | 220 | 372 | 1184 |

old/new的336对中，10个兼容阶段×两主题×3角色×两面×两后端共240对全部0；Global/Foreground/popup-live/gradient-popup四个修复阶段共96对均有对应实际变化。Custom带class Style属于兼容阶段，普通Style反例结论按V3工程证据，不扩称原生前后复现。全部1184对严格预期符合，像素通用容差为0；没有新增S1/N1例外或合成原因未定项需要接受。前期预测FAIL、准备错误和历史C9首轮383/388继续保留，未改写为通过或新移交CT25。

已实际查看正确深色旧新Global、AOT深色default/Global、明色gradient popup和带确认操作区的popup-live：字体、边框、棋盘、圆角、palette间距及popup View变化与V1/V2实际目标一致；完整色谱、输入、两stop、预设组和原控件尺寸/交互未被删除或裁剪。紫色/橙色为明确验证输入，默认/撤回帧保持旧新一致；不是推荐配色。V3 Native class几何只作兼容正例。正常motion/真实时钟及独立A/A、完整恢复历史保留，没有强制重绘、关闭交互或重建一侧消差。

主角色各修复阶段的完整old/new变化像素/max通道（其余角色的同类变化与全部严格结果纳入1184）：

| 阶段 | Headless RTB Light | Headless RTB Dark | Mac display Light | Mac display Dark |
| --- | ---: | ---: | ---: | ---: |
| global | 127452/255 | 128870/255 | 501120/255 | 507211/255 |
| foreground | 127452/255 | 128870/255 | 501120/255 | 507216/255 |
| popup-live | 164059/255 | 157704/255 | 647307/255 | 613805/255 |
| gradient-popup | 156454/255 | 150685/235 | 624511/255 | 591691/235 |

全部帧/模型/角色完整性核对后媒体audit clean/findings空。审查所需结论与必要数值已集中到本记录，随后清理标记目录及专用driver/源码备份/managed/AOT产物；本轮V1–V3仍等待用户明确限定批准，前台协调不等于该批准。

## 全部临时产物清理与出口复核

媒体标记目录最终cleanup成功，删除860个文件，audit clean/findings空；此前已删除1118张错误/重复PNG。9临时源码已清。独立会话中的driver、旧/新managed与AOT发布、75个原字节备份、13个候选备份、临时测试文字备份、脚本/receipt/编译中间文件等共1387文件/3089633063字节全部删除，目录和定位指针均不存在；无仓库内专用CT19Driver产物，未删除共享构建目录或用户产物。

清理前最后核对：350项原有改动中345项未触及者SHA逐字节相同；另5项仅本包授权修改（两处原有motion主题文件、两CSV、tasks）。两处原有Owner motion接线完整保留；暂存补丁字节及7个intent-to-add条目一致，仍在原checkout/feature/custom-theme。13个产品源文件均与最终AOT/像素验收冻结指纹一致，无新产品变更，不重跑未变化的NET8与成员审计。全部专用产物清理后再执行明确ColorPicker过滤的模块门禁；不运行全量/change回归，不创建分支/worktree、不委派或提交。

全部专用产物清理后最终精确模块门禁27PASS/0Skip，测试1s（进程11.36s）；git diff --check通过，5份正式文档本地链接无失效，CSV恰为22 metadata/24 Active Own，临时源码和会话定位指针不存在。NET8 36.70s零警告错误、2文件2控件成员0问题因源码一致复用。原有345项未触及内容、5项授权更新与7个intent-to-add/staged字节保留已在清理前核对；当前只等待V1–V3限定批准，没有Git授权，不自动开展其他CT。

## 批准登记与圆角后续

2026-11-04，唯一功能视觉批准请求为`request_user_input_async / call_a43050be209243b3ad7bde20af592aa0 / item 0`。用户原话：**“批准 V1–V3 限定范围”**。仅覆盖上表实际Global/字体/Foreground消费、色板View归属、确认区间距、渐变锚点间距及有限Custom普通Style优先级；Custom普通Style的原生前后复现证据边界、C9首轮383/388历史缺口和CT25/FW按原记录保留，不新增噪声豁免，不包含Git提交。55工程检查、全部清理后27PASS/NET8零/2文件2控件成员0、840有效图/1184严格比较及Mac AOT1365证据不变，按有效结果复用。

V1–V3批准原包完成。用户随后报告：**“colorpicker 弹出popup 的设置背景色的时候圆角和背景色的圆角不一样，背景会尖一些 反正有问题”**。新增限定rounding后续仍开放：恢复专用popup.root Style设置背景的原场景，对齐实际背景/边框/阴影轮廓，保留箭头、位置、尺寸、内容和两宿主；不重开此前V1–V3或其他CT。取得实际反例后只修其根因，再按临时验证生命周期和受影响范围验收。当前不能把原包批准扩称该新问题已关闭，没有Git授权。

## R1：用户截图的背景尖角与白底外溢

用户两张截图为首要证据：第一张background-only显示紫色外层方形底板、白色面板和白箭头；第二张明确红框指出原完整验收popup-live底角，紫色圆角外露出白底。用户要求“你自己分析”，并要求重跑原测试后截图，不能把第一次简化展示当作完整触发轨迹。已重放1200×820四root、相同ARGB155/50/100/180、水平双stop和三色组，原default→Global→Foreground→Own→clear→popup-style→popup-live等原历史；用户捕获的popup-live与原记录匹配。交互重放为方便截图增加5秒停留/F8，未作为正式像素验收；用户关窗后RawKey入口PlatformImpl为空的重放异常保留，不计产品反例或PASS。用户附件属于用户文件，不复制进仓库、不删除。

实际4个有效RED：两picker的背景设置#722ED1后，实际ArrowDecoratedBox.ContentDecorator仍为White；两picker的popup-live设置Radius13后，实际可见面和ShadowsAwareContainer仍为6。原因是原专用Style目标为外层包装Border，真正背景/边框/阴影面位于内层Arrow模板，外层Background方形直绘和内层白底/圆角互不对应。Border.Render为sealed，不能靠覆写忽略背景或延迟刷新隐藏它。

限定工程路线：生成专用Style类/五Part名称/Border合同/Single数量保持，popup.root路由修正为`/template/ .semantic-popup-host > .semantic-popup-surface /template/ .semantic-popup-root`，CrossNestedOwners=true。静态marker位于家族私有ColorPickerPopupSurfaceTheme的实际Border；外层Frame只保持直接Child/箭头-mask协议。私有theme继承原ArrowDecoratedBox全部默认，只为静态marker和实际背景到箭头的CompiledBinding拥有模板；不改共享Arrow/Popup或其他CT。为保留原root Padding附加语义，基线Arrow padding由ContentPresenter.Margin承载，实际public Border.Padding默认0、明确7叠加，保持内容总inset/尺寸；不丢失原8 DIP基线。原始selector route的直接使用需要更新，已有生成专用Style调用不变；该主题契约修正将如实作为本次限定审查范围，不沿用V1–V3批准。

第一种嵌套Style增加marker被现有生成器静态marker门禁拒绝，第二种命名资源BasedOn未有词法来源被注册门禁拒绝；均为准备错误，未修改/放松Generator或诊断。采用既有typed ControlTheme/BasedOn形态后4个原反例全部通过，连同既有27合计31PASS（10s）。实际源变更仅家族两descriptor、两owner theme、新typed theme及既有契约测试；未改Core/共享primitive/native依赖，无反射Binding或代码查找节点设置Semantic样式。

候选/原版managed合批构建及Mac AOT发布85.49s成功（仅既有5条Homebrew链接警告），只在原目录安全字节回放4个原文件，finally恢复候选；同一参数驱动保留原完整历史，增补背景单设/撤回，并选取9阶段捕获。Headless旧/新各108图，AA/DIP/恢复及old-new共276项严格预期全部符合：默认和撤回兼容0、两侧独立A/A0、1DIP全检出。实际查看候选popup-live背景/箭头紫色，底角不再有白底外溢；几何模型visible radius与shadow radius同为13，原版均6。原生和当前AOT显示面仍在完成，不提前写R1已关闭。

## R2：用户红框中的输入框高度

用户第三张截图红框指向独立GradientView的格式/HEX/Alpha行，随后明确说明“输入框高度都不对，不统一”。不把最初对边框/圆角的推测当作根因。实际4项RED：两View×InputAli/Menlo字体的HEX均Bounds.Height22、所在行Height24，VerticalAlignment=Top。只在ColorPickerInput自身ControlTheme将PART_HexValueInput设为VerticalAlignment.Stretch，未改LineEdit/NumericUpDown共享主题；4项原反例及后续HSV/RGB/Alpha行均通过，连同R1动态撤回/两宿主/箭头/非对称圆角的8项，共12临时PASS（36s）。不改变固定FontSize11、通道48、Alpha兼容宽度或步进，不把统一行高改成固定字号或修改业务输入行为。

R1首候选原生采样在最后5阶段失焦，49阶段保留，末组完整历史重放27.81s补齐缺10张；重放前缀8张与首轮已采帧严格0，不扩大容差。随后AOT轮在用户发图同期失焦断言中止，内部焦点变化原因未证明；保留原始失败和已采帧，不计完整AOT通过。R2源码稳定后合批发布与必要采样，避免每项重复发布；完整原历史保持，仅增加原RGB/HSV阶段的取帧。R1/R2均未完成清理和最终出口前不写Fixed。

合并R1/R2发布84.12s通过，原版与候选Headless各132图，原完整历史11阶段的332项严格比较全符合：两侧A/A共88为0、1DIP共88检出、同实例恢复24为0、old/new132符合修复预期（RGB/HSV兼容0，其余包含HEX行高或popup表面变化）。原版合并原生轮77.14s因失焦中止；同一前台条件连续失败后停止重复采样，转入精确模块和NET8检查，再申请明确前台时间。中止时已经写出的文件若未完成active后的模型receipt登记，不纳入比较，不能以“文件存在”充成有效配对。截图/切应用是同期操作，未假定已经证明内部原因。

### S1：当前原生标题栏具体差异（已获限定接受，严格FAIL保留）

合并最终比较896项，854符合、42真实FAIL；全部42对仅关闭/最小化圆点颜色不同（已查看原图诊断对照），每对1328像素/max68，整数像素bbox [30,26,98,54]。控件和popup区域之外未发现其他变化；内部原生着色原因尚未证明，不归因于用户或擅自修Window/平台依赖。JIT/AOT即时RTB66对均0，最终显示66对中32对0、34对如下；另6对old/new兼容、1对AA、1对恢复同处该标题栏。比较器保持整图零容差，以下仍是FAIL，不构成未来帧/其他平台豁免，也不转移CT25/FW归属。

| ID | 类型 | 原帧 | 对照帧 | 像素/max | bbox |
| --- | --- | --- | --- | --- | --- |
| S1-01 | AA | `newmacfinal-native-dark-main-base-display.png` | `newmacfinal-native-dark-aa-base-display.png` | 1328/68 | [30,26,98,54] |
| S1-02 | restore | `newmacfinal-native-dark-main-base-display.png` | `newmacfinal-native-dark-main-restored-display.png` | 1328/68 | [30,26,98,54] |
| S1-03 | old-new | `oldmacfinal-native-dark-main-rgba-display.png` | `newmacfinal-native-dark-main-rgba-display.png` | 1328/68 | [30,26,98,54] |
| S1-04 | old-new | `oldmacfinal-native-dark-main-hsva-display.png` | `newmacfinal-native-dark-main-hsva-display.png` | 1328/68 | [30,26,98,54] |
| S1-05 | old-new | `oldmacfinal-native-dark-aa-rgba-display.png` | `newmacfinal-native-dark-aa-rgba-display.png` | 1328/68 | [30,26,98,54] |
| S1-06 | old-new | `oldmacfinal-native-dark-aa-hsva-display.png` | `newmacfinal-native-dark-aa-hsva-display.png` | 1328/68 | [30,26,98,54] |
| S1-07 | old-new | `oldmacfinal-native-dark-probe-rgba-display.png` | `newmacfinal-native-dark-probe-rgba-display.png` | 1328/68 | [30,26,98,54] |
| S1-08 | old-new | `oldmacfinal-native-dark-probe-hsva-display.png` | `newmacfinal-native-dark-probe-hsva-display.png` | 1328/68 | [30,26,98,54] |
| S1-09 | JIT-AOT | `newmacfinal-native-light-main-base-display.png` | `aotfinal-native-light-main-base-display.png` | 1328/68 | [30,26,98,54] |
| S1-10 | JIT-AOT | `newmacfinal-native-light-main-popup-style-display.png` | `aotfinal-native-light-main-popup-style-display.png` | 1328/68 | [30,26,98,54] |
| S1-11 | JIT-AOT | `newmacfinal-native-light-main-popup-live-display.png` | `aotfinal-native-light-main-popup-live-display.png` | 1328/68 | [30,26,98,54] |
| S1-12 | JIT-AOT | `newmacfinal-native-light-main-gradient-popup-display.png` | `aotfinal-native-light-main-gradient-popup-display.png` | 1328/68 | [30,26,98,54] |
| S1-13 | JIT-AOT | `newmacfinal-native-light-main-restored-display.png` | `aotfinal-native-light-main-restored-display.png` | 1328/68 | [30,26,98,54] |
| S1-14 | JIT-AOT | `newmacfinal-native-light-main-rgba-display.png` | `aotfinal-native-light-main-rgba-display.png` | 1328/68 | [30,26,98,54] |
| S1-15 | JIT-AOT | `newmacfinal-native-light-main-hsva-display.png` | `aotfinal-native-light-main-hsva-display.png` | 1328/68 | [30,26,98,54] |
| S1-16 | JIT-AOT | `newmacfinal-native-light-main-background-only-display.png` | `aotfinal-native-light-main-background-only-display.png` | 1328/68 | [30,26,98,54] |
| S1-17 | JIT-AOT | `newmacfinal-native-light-main-background-cleared-display.png` | `aotfinal-native-light-main-background-cleared-display.png` | 1328/68 | [30,26,98,54] |
| S1-18 | JIT-AOT | `newmacfinal-native-light-main-native-popup-root-display.png` | `aotfinal-native-light-main-native-popup-root-display.png` | 1328/68 | [30,26,98,54] |
| S1-19 | JIT-AOT | `newmacfinal-native-light-main-native-popup-live-display.png` | `aotfinal-native-light-main-native-popup-live-display.png` | 1328/68 | [30,26,98,54] |
| S1-20 | JIT-AOT | `newmacfinal-native-light-aa-base-display.png` | `aotfinal-native-light-aa-base-display.png` | 1328/68 | [30,26,98,54] |
| S1-21 | JIT-AOT | `newmacfinal-native-light-aa-popup-style-display.png` | `aotfinal-native-light-aa-popup-style-display.png` | 1328/68 | [30,26,98,54] |
| S1-22 | JIT-AOT | `newmacfinal-native-light-aa-popup-live-display.png` | `aotfinal-native-light-aa-popup-live-display.png` | 1328/68 | [30,26,98,54] |
| S1-23 | JIT-AOT | `newmacfinal-native-light-aa-gradient-popup-display.png` | `aotfinal-native-light-aa-gradient-popup-display.png` | 1328/68 | [30,26,98,54] |
| S1-24 | JIT-AOT | `newmacfinal-native-light-aa-restored-display.png` | `aotfinal-native-light-aa-restored-display.png` | 1328/68 | [30,26,98,54] |
| S1-25 | JIT-AOT | `newmacfinal-native-light-aa-rgba-display.png` | `aotfinal-native-light-aa-rgba-display.png` | 1328/68 | [30,26,98,54] |
| S1-26 | JIT-AOT | `newmacfinal-native-light-aa-hsva-display.png` | `aotfinal-native-light-aa-hsva-display.png` | 1328/68 | [30,26,98,54] |
| S1-27 | JIT-AOT | `newmacfinal-native-light-aa-background-only-display.png` | `aotfinal-native-light-aa-background-only-display.png` | 1328/68 | [30,26,98,54] |
| S1-28 | JIT-AOT | `newmacfinal-native-light-aa-background-cleared-display.png` | `aotfinal-native-light-aa-background-cleared-display.png` | 1328/68 | [30,26,98,54] |
| S1-29 | JIT-AOT | `newmacfinal-native-light-aa-native-popup-root-display.png` | `aotfinal-native-light-aa-native-popup-root-display.png` | 1328/68 | [30,26,98,54] |
| S1-30 | JIT-AOT | `newmacfinal-native-light-aa-native-popup-live-display.png` | `aotfinal-native-light-aa-native-popup-live-display.png` | 1328/68 | [30,26,98,54] |
| S1-31 | JIT-AOT | `newmacfinal-native-light-probe-base-display.png` | `aotfinal-native-light-probe-base-display.png` | 1328/68 | [30,26,98,54] |
| S1-32 | JIT-AOT | `newmacfinal-native-light-probe-popup-style-display.png` | `aotfinal-native-light-probe-popup-style-display.png` | 1328/68 | [30,26,98,54] |
| S1-33 | JIT-AOT | `newmacfinal-native-light-probe-popup-live-display.png` | `aotfinal-native-light-probe-popup-live-display.png` | 1328/68 | [30,26,98,54] |
| S1-34 | JIT-AOT | `newmacfinal-native-light-probe-gradient-popup-display.png` | `aotfinal-native-light-probe-gradient-popup-display.png` | 1328/68 | [30,26,98,54] |
| S1-35 | JIT-AOT | `newmacfinal-native-light-probe-restored-display.png` | `aotfinal-native-light-probe-restored-display.png` | 1328/68 | [30,26,98,54] |
| S1-36 | JIT-AOT | `newmacfinal-native-light-probe-rgba-display.png` | `aotfinal-native-light-probe-rgba-display.png` | 1328/68 | [30,26,98,54] |
| S1-37 | JIT-AOT | `newmacfinal-native-light-probe-hsva-display.png` | `aotfinal-native-light-probe-hsva-display.png` | 1328/68 | [30,26,98,54] |
| S1-38 | JIT-AOT | `newmacfinal-native-light-probe-background-only-display.png` | `aotfinal-native-light-probe-background-only-display.png` | 1328/68 | [30,26,98,54] |
| S1-39 | JIT-AOT | `newmacfinal-native-light-probe-background-cleared-display.png` | `aotfinal-native-light-probe-background-cleared-display.png` | 1328/68 | [30,26,98,54] |
| S1-40 | JIT-AOT | `newmacfinal-native-light-probe-native-popup-root-display.png` | `aotfinal-native-light-probe-native-popup-root-display.png` | 1328/68 | [30,26,98,54] |
| S1-41 | JIT-AOT | `newmacfinal-native-light-probe-native-popup-live-display.png` | `aotfinal-native-light-probe-native-popup-live-display.png` | 1328/68 | [30,26,98,54] |
| S1-42 | JIT-AOT | `newmacfinal-native-dark-main-base-display.png` | `aotfinal-native-dark-main-base-display.png` | 1328/68 | [30,26,98,54] |

### R1/R2最终工程与配对出口（已获限定批准）

最终12临时＋27既有＝39PASS（37s）；4临时源码/9305字节已删除，清理后原27PASS/0Skip（2s），NET8 Release23.35s零警告错误，3文件/0控件成员0问题。新私有主题继承默认且只在ColorPicker显式应用，未改共享Arrow/Popup、官方缓存或native依赖；实际普通/渐变、两host、箭头开关、非对称/0角、边框2及Style撤回/重开由有限检查覆盖。全部源码稳定后合批发布84.12s，只有原5条Homebrew链接警告。

最终正确主题5个数据角色共660张必需图：Headless原/新各132、Mac原/新/AOT各132。失焦中止源数据完整保留；候选最后暗色补采85.16s，已有暗色base的RTB0、最终显示1328标题栏差异，补缺64图，不冒称原失败批完整。完成receipt同时检查实际Appearance/1200×820/scale2/active；未完成receipt的中止图被排除。原生原版169.14s完整、AOT167.52s完整，候选合并有效明色/补采暗色66阶段。所有原完整操作历史保持，未关闭正常motion或以新场景替代用户原popup-live/输入行。

896严格比较＝220 AA、220检错、60恢复、264旧新、132 JIT/AOT：854符合预期、42标题栏FAIL已逐对固定在S1表。即时RTB的JIT/AOT66对全部0；最终显示仅表列34对同一系统按钮差异，控件/弹层区域变化未被容差掩盖。标题栏原因未证明，保留实际模型一致范围，不改Window、平台、配置或比较器来消差。已查看AOT明色background-only/popup-live和候选暗色原生图：背景/箭头一致、紫色底角无白底露出，HEX/Alpha绘制边界统一24 DIP，HSV/RGB原样；实际默认content inset通过继承margin保留，原模板route改动按公共主题契约说明。

作者另步自审（主agent、无subagent/独立评审）：R1唯一绘制面/箭头编译绑定/静态marker与正确route；R2只HEX Stretch，无新运行时字段/订阅/缓存/字体或宽度变更。新typed theme代表家族专用语义绘制面，避免修改共享primitive或放松生成器诊断；既有契约测试只同步实际静态marker和route，无净增常驻测试。此前V1–V3、C9历史与CT25/FW边界保持；R1/R2及S1的限定审查不等于Git授权。

### R1/R2清理与最终限定审查条件

全部取证先审阅后清理：标记目录删除1262文件，audit clean/findings空；用户原始附件未删除。独立临时会话的driver、原/新managed、AOT、原字节/测试文字备份、脚本与专用中间文件1243个/3078538775字节全部删除，目录及指针不存在；无仓库内专用CT19PopupDemo产物，未删共享构建或用户文件。

跟进开始366项原有内容中359项未触及者逐字节一致，7项只本包正式文档/总表更新；暂存补丁和7个intent-to-add条目保持。全部专用产物清理后再次精确ColorPicker27PASS/0Skip（测试2s，进程13.12s），git diff --check及本地链接检查通过，NET8/成员审计因源码一致复用。源代码/实际消费与两CSV、任务表、自审记录同步；没有分支/worktree/subagent/commit或额外全量回归。

R1/R2审查只覆盖用户两类截图：实际Border背景/边框/圆角/阴影及箭头同色，家族私有主题和跨模板route修正，以及HEX行内22→24 DIP伸展。生成Style名称、Border类型与数量保持；直接使用旧route的应用需按正式文档更新，不能把它隐作无契约变化。S1只申请表列42对当前系统标题栏按钮差异的明确接受，原因未定、FAIL和零通用容差保留，不扩到未来帧/其他区域/平台或CT25/FW。两者均不含Git授权，之前V1–V3不重复申请。

## 最终批准登记与CT19结论

2026-11-04，后续唯一批准请求为`request_user_input_async / call_e14da701279a469eb502ef0dcaec8c70`：

- item 0，用户原话：**“批准 R1/R2 限定范围”**。仅批准上述实际 popup Border/背景/箭头/可见圆角/阴影及跨模板 route 修正、家族私有主题、HEX 行内 22→24 DIP 伸展。生成 Style 名称和 Border 合同保持，直接使用旧 selector route 的迁移边界按正式文档保留；不重开此前 V1–V3，不包含 Git 提交。
- item 1，用户原话：**“仅接受表列 S1 具体差异”**。仅接受 S1-01–S1-42 这42对当前 Mac 关闭/最小化按钮颜色差异，每对1328像素/max68、bbox固定[30,26,98,54]。原生着色原因尚未证明，整帧比较器继续保留42项真实FAIL和零通用容差；不扩大未来帧、区域或平台，不移交CT25/FW，不包含提交。

原包 V1–V3 批准与本次 R1/R2/S1 已分别集中登记，前台环境协调不作为功能或像素批准。CT19 的22 metadata/24 Active Own及既定有限家族出口现已完成，后续两个用户截图问题按批准范围关闭。39项工程、全部清理后27PASS/0Skip、NET8零警告错误、成员3文件/0控件/0问题与当前Mac NativeAOT有效证据复用；合并660图/896比较按854符合预期、42具体接受但仍FAIL记账，不改写为896像素全PASS。

全部临时源码、媒体、driver及专用产物已清理，常驻测试净增0；旧route迁移、V3普通Style原生前后证据限制、C9首轮历史缺口和CT25/FW原有边界继续保留，不从限定批准推出通用平台结论。当前仍在原目录feature/custom-theme，保留全部既有改动和暂存区，没有创建分支/worktree、使用subagent、提交或自动开展其他CT。
