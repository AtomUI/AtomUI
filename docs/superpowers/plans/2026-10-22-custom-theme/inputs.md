# CT-13 输入与选择主题执行目录

2026-10-28 [S6结果](input-select-option-metrics-results.md)：公开字体与Own最小行高进入真实候选项，均匀普通行预算保留三行；12专项/managed1442/AOT1442/NET8零/清理后Select71通过。4临时源、426工程文件和241媒体清理、audit clean。[36对配置视觉](input-select-option-metrics-visual-review.md)已获用户限定批准；24对默认/恢复0。分组、可变模板、其他配置及完整家族仍开放。

2026-10-28 [S5源码分类](input-select-legacy-fields.md)：SelectAffixPadding/FixedItemMargin按Legacy兼容保留，默认/类型/key/ID/slot/配置读取不变，不虚构内置布局；文档改为真实SingleModePadding三档。C#仅XML注释、去除注释后源码严格相等，常驻测试净增0，成员1/0/0及原Token1项通过，无视觉变化或Git，不关闭家族。

2026-10-28 [S4结果](input-select-handle-results.md)：三手柄父组合IconSize/反馈SpacingXS接通，6RED/6正对照→13PASS；原颜色和真实hover清除保持，编译专用Suffix Style命中/当前值恢复通过。managed/AOT各4202、NET8零19.88s、成员1/1/0；临时5源/426文件工程/201媒体清理后三模块148PASS。[26对配置视觉](input-select-handle-visual-review.md)已独立批准，默认/恢复24对均0；完整家族及CT25开放，无Git。

2026-10-28 [S3结果](input-select-internal-owner-results.md)：三公开owner共用标签间距/计数文字的7处资源接通实际owner，6有效间距RED及3纠正后的颜色RED→10PASS；managed/AOT各290、NET8零18.12s，临时3源/422工程文件/113媒体清理后三模块148PASS。[14对配置视觉](input-select-internal-owner-visual-review.md)已独立批准，非修复14对两后端均0；完整家族、其他尺寸/Global/Part仍开放。

2026-10-28 [S2结果](input-select-option-padding-results.md)：Select Own.OptionPadding进入真实Frame，匹配继承尺寸触发优先级；原4RED→6PASS，原生固定弹层managed/AOT各2044、NET8零19.92s，清理后Select71PASS。[20对配置/0视觉](input-select-option-padding-visual-review.md)已独立批准，默认/清除等24对均0。原生普通弹层窗口失活后的关闭原始记录保留，不冒充原生像素/输入通过。下一入口为[S3三owner内部排版](input-select-internal-owner-audit.md)。

2026-10-28接续：[NU3结果](input-numeric-divider-results.md)的父厚度对应边投射已通过5专项、managed/AOT各590与NET8零警告；临时清理后Numeric49项通过。[10对显式厚度视觉](input-numeric-divider-visual-review.md)已独立批准，16非修复对均0，完整家族仍开放。下一入口为Select候选项OptionPadding实际消费者核对；S1默认协议迁移不代表该Own字段已消费。

在已授权精确主题全局计划内推进，主agent独立，无分支/worktree/提交。依赖CT10已验收共享机制与CT11资源/动效机制；各家族最终出口仍独立记录。CT12五历史字段兼容分类与loading开关分包已完成，完整家族、原生显示和跨Host出口按[九owner核对](buttons-exit-audit.md)保留。

## 来源事实与拆包

- R：TextArea.ResizeHandleSize存在Own字段，交互Handle主题Width/Height写死8；装饰框Render另写死inset2/side7/次线差3/stroke1。该字段没有实际命中入口。先同步实际命中区域与绘制尺寸，保留原默认像素及拖拽/滚动/高度合同。
- F：LineEdit自身主题未使用InputFontSize三字段；SelectFilterTextBox借用LineEdit Own，Small还读中号。这分别属于CT13公开输入与CT14容器owner，不能把配置另一个identity当成正确接入。LineEdit旧默认字号依赖原生继承；接入可选字体须先测实际默认/父字体，不能为了正确字段名把默认外观改掉。
- G：TextBox/LineEdit/TextArea仍有Shared字号/selection/caret/placeholder/error/motion等读取，须逐字段投射到实际公开owner。内部frame无独立Global identity，沿公开owner/模板关系消费，代码创建跨树对象另列理由与释放。
- V：TextBox3、LineEdit3、TextArea6及Otp4字段的V2默认迁移独立处理，保持公式/单位/默认资源值。平坦字段迁移不能编写只复述attribute的无价值测试。

## R Resize执行合同

- [x] 先实际失败验证Own尺寸不改变Handle与绘制；默认8、四尺寸、明暗、resizable开关、原始高度约束和滚动不改变。命中区域与几何先观察。
- [x] ControlTheme把Own ResizeHandleSize送到实际Handle；默认8。绘制原2/7/3坐标作为8单位glyph比例，随尺寸缩放；描边用同owner LineWidth而非写死1。0停止绘制且命中区域为0，不取消IsResizable本身；颜色继续原Own。
- [x] 真实输入验证自定义尺寸左/中/右命中、拖拽行为与height、默认滚动/裁剪/clear/count保持；实际Skia明暗仅已批准的默认指示线差异、配置实际glyph改变/恢复同阶段一致。禁止用隐藏内容或关动画避开视觉差异。
- [x] 当前Mac NativeAOT实际尺寸/颜色/描边配置和清除；临时源/驱动/媒体清理后聚焦Input/Resize/Semantic测试、NET8、成员及文档检查；准确记录未验平台。

F/G/V和Select/ComboBox/CheckBox/Radio/OTP等后续分包在完成各自源码/失败前证后细化，不能用R代表整个CT13完成。

## R 默认指示线例外（用户已批准）

实际Skia截图证明原装饰框Render发生在不透明模板背景之前，Own颜色配置后可见Magenta像素仍0；单独DrawingGroup/AOT数据断言通过不能代替可见消费。将原8单位几何/颜色放到上层临时原型后，12控件样本明暗各88像素变化。用户明确回复“允许修复默认指示线（推荐）”，批准仅默认指示线恢复可见这一视觉差异；其余布局、文本、边框、交互仍保持原合同。

最终采用既有末层ResizeHandle自身Render，不新增wrapper/Adorner/伪类/公开Part或状态旗标。Bounds统一命中与绘制尺寸，Foreground消费原Own颜色，内部描边输入消费TextArea有效LineWidth；删除父装饰框失效的绘制输入/状态/Setter。字体F与其余出口保持未完成。

R实现和专项已通过，清理及最终模块证据见[input-resize-results.md](input-resize-results.md)。F/G/V仍开放。

## F1 LineEdit精确字体合同

只处理LineEdit公开owner，SelectFilterTextBox借用身份另留CT14。未配置时四尺寸均保留真实InheritanceParent的字号/字体族；显式Own InputFontSize/SM/LG，或相应有效Global字号，才选Own最终资源。Custom跟随中号；IsCustomFontSize=true退出字号主题。LocalValue/普通Style优先级不变；父字体动态变化、重挂载、清除恢复和GC必须验证。字体族复用已验收可选Global机制。ControlTheme选择最终属性，protected继承父只能经内部只读桥+AOT编译observable读取；不得代码写最终FontSize。先真实失败，再实现、Skia新旧和往返、当前Mac AOT、清理后Core/输入focused。

## G1 四个公开文本输入owner的Global消费

F1清理验收后执行。逐项将TextBox/LineEdit/TextArea/SearchEdit自身主题的Shared资源投射为各自typed Global：selection/caret/placeholder/error/warning/motion/spacing，TextBox既有字体与行高、TextArea既有行高。保持原Setter条件和优先级，默认资源值不变，不混入内部frame跨owner路由或增加新Own。先每个owner真实失败的动态配置与其他owner隔离；实际模板部件以及root属性、0/清除、LocalValue、Form/validation和disabled链分别核验。已有源码约束如锁旧Shared形态须按新owner合同更新，不能删除契约。实际Skia四owner四尺寸明暗多状态旧新0及配置/恢复、当前Mac AOT，临时清理后输入模块focused。未触及Select/ComboBox/OTP借用身份不据此关闭。

### G1状态文字发现（用户已批准颜色例外）

TextArea Filled/Borderless文字实际经ContentLayout继承链，原测试查ScrollViewer造成两项假失败；实际文本节点已消费typed颜色。LineEdit/SearchEdit六加六选择器却使用原生ScrollViewer精确类型，实际AtomUI派生类型不匹配；实际文字节点4项RED后，仅修正类型，10项专项PASS。恢复默认错误/警告文字颜色涉及默认外观例外，用户已明确回复“允许修复默认错误/警告文字颜色（推荐）”，只批准该颜色恢复；边缘及往返差异继续独立核验。额外抗锯齿差异仍逐项A/B，不使用通用容差。

## V1 四个输入Own默认求值迁移

TextBox3、LineEdit3、TextArea6、OtpLineEdit4共16字段，逐一迁移DependencyDefaultsV2 Revision1。保留全部名称/类型/公共资源ID和精确公式（TextBox中小档Round、大档Ceiling及运算顺序），不顺手改默认/范围。先真实注册后的V2协议失败与旧明暗/控件Global及显式Own快照，再迁移；新旧16字段格式值、实际四owner四尺寸明暗/多状态默认0、当前Mac AOT独立消费、原输入交互和资源清除；临时清理后Core/Generator/输入及OTP精确filter、NET8。V1不代表这些控件全部消费或CT13全家族通过。

## F2 剩余公开文本输入字体

V1清理后执行。TextBox/TextArea/SearchEdit的有效Global FontFamily仍没有自身可选消费，复用真实InheritanceParent桥和根Setter，未配置保留继承/原优先级。SearchEdit原字号全部继承，新增只在对应Global字号显式配置时启用的根Setter；IsCustomFontSize退出，Custom用中号。Core有限字体转换器同时支持“Own+Global三元组”和“Global二元组”，避免不存在Own时伪造字段或重复订阅。先3owner字体族和Search四尺寸真实失败，旧12输入LineEdit路径必须验证不回归；同轨迹明暗像素/配置恢复和当前Mac AOT，清理后Core/Input/按钮字体既有filter及NET8。LineEdit/Search默认状态颜色的已批准例外不被F2扩大。

F2补充隔离合同：EmbeddedTextBox仅为模板资产，BasedOn复用TextBoxTheme时必须维持组合父的排版继承，不能获得公开TextBox identity的可选字体；实际模板实例隔离测试与既有LineEdit12输入选择器均须通过。

## G2 共享输入表面与内部操作按钮

F2清理后执行。InputControlFrameTheme仍直接读取Shared的边框、背景、hover/focus/status/disabled、add-on预设padding与EnableMotion；InputClearIconButton/RevealButton的图标尺寸与状态颜色也绕过公开owner。复用已有OwnerTokenResource，按实际模板链解析最近注册的公开owner，内部资产不新增identity/Own/优先级引擎。保留根显式画刷中继、模板绑定、EffectiveStatus、四variant、CompactSpace、输入几何和动效；无owner的独立资产仍Shared回退。先实际四公开文本owner框体/clear/reveal失败，补动态清除/隔离/用户优先级；再覆盖直接或间接复用框体的选择/数值/日期/时间/OTP默认像素。专用Style仍唯一Part定制入口，不代码找节点设置属性绕过。当前未生效的配置修复与默认外观分别对照，合成边缘未决项不得忽略。实际当前Mac AOT、临时清理后输入及直接消费者focused，不扩大full/change。

## G3 OTP后续执行分包（待G2清理出口后实施）

- A：OtpLineEditTheme与CellTheme直接读取Shared的高度、字号、颜色、选区、placeholder、spacing/motion；投射到OtpLineEdit真实公开owner。Own四字段已V2，保留名称/公式/优先级，不能用这次资源迁移冒充Own全字段消费验收。DataTemplate中的cell与文本分别核对实际模板/逻辑路线，不借用TextBox/LineEdit身份。先真实六cell与实际文本RED，原四variant/四尺寸/默认字体/输入、选择和清除保持。
- B：OTP underlined底边确认写死1，caret宽1、separator左margin4；共享frame的下划线几何单独核实，不根据同类样式推断。逐项区分Global LineWidth、owner空间值及布局语义，以实际默认数值确定等价替换；不因整数常见就机械代换。Cell的caret可见性/RenderTransform目前代码写值；触及C#时按ControlTheme优先迁移可等价项，运行时计算的字符偏移须保留具体兜底理由。完整caret时间线及EnableMotion职责另核，不把输入反馈与装饰动效混称；不得通过关闭blink避免原始输入验收。
- C：根字体与separator字体实测、内部原生输入的继承、root标准属性/CellBorderBrush/CellWidth和专用Semantic Style优先级逐一验证。公开API、模板真实效果及文档固定margin说法不一致处以源码和实际几何修正。先明确控制维度，保留Own/Global/实例的职责，不新增重复的尺寸分支。

各分包继续实际Skia/同轨迹、输入及当前Mac AOT；临时验证和媒体清理后聚焦输入/OTP。G2及旧输入的往返合成边缘和热切换分配仍归CT25开放，不随OTP推进取消。

G2性能闭环的真实Startup索引候选经当前源码复核，5381fb6c9已经采用metadata tuple成功查询。本轮[真实App验证](input-startup-lookup-results.md)确认两类identity、两类Global及Own标量各10000次均0分配，伪造owner拒绝；没有重复改产品或新建缓存。整场景26%热分配增长及往返合成出口仍开放，继续按真实同工作负载核查，不能以局部查询0分配关闭。

### G3-B1 几何资源与声明式部件验证

先处理OTP caret宽1、underline底边1、separator左margin4：caret消费有效LineWidth；underline从有效BorderThickness保留Bottom；separator从有效MarginXXS保留Left，默认分别1/1/4。两种有向厚度转换用有限AXAML MultiBinding转换器，不新增重复Own字段，不改其余边或CornerRadius。先0/粗线/非均匀alias与间距实际RED；再以编译AXAML专用Cell/Separator Style验证命中与优先级，若route不符真实DataTemplate树则修正契约并保留可失败命中证据，禁止代码属性回退。此包不触及Cell C#；caret状态/偏移代码迁移与时间线归B2，根字体归C。原四variant/四尺寸、真实输入、明暗默认/往返、AOT、清理后OTP及NET8逐项验证。

实现、实际新旧像素、命中及专项结果见[G3-B1结果](input-otp-geometry-results.md)。B2/C继续开放，不能以本几何分包代表完整OTP家族验收。

### G3-B2A 光标状态与字号同步

先将可等价表达的caret IsVisible从Cell代码移到ControlTheme，合同仍为IsActive且有效启用，其余cell不可见；不改变原三关键帧、1s周期、默认启动/焦点行为。字符右移仍是FontSize×0.30的既有几何计算，不替换为会改变默认位置的字形度量。原代码未观察FontSize，先用公开owner的Global字号热更新复现已显示字符caret偏移停留旧值。编译AXAML原型已证明Template内的同一TranslateTransform.X双输入绑定可更新和清除，故最终将偏移一并迁移，删除Cell代码中的caret字段与RenderTransform维护；不以无法直接TemplateBinding作为代码兜底理由。Cursor目标为运行时Content子树，按真实可达性核定代码保留理由，不能只因方便保留模板可表达的绑定。

公开API、Own、资源ID、模板部件、伪类和blink时间线保持；Member审计先记录，再按现行规范整理所触及Cell成员。临时验证覆盖已填字符/空格、启用/禁用、根焦点移动、模板重建、scope字号更新/清除与GC；脚本实际输入和默认几何/非闪烁画面对照。原blink曲线不变不等于完整时间线验收，B2B仍需单独定义duration/可见性/motion职责并验证，不把暂停动画当通过。清理后聚焦OTP与NET8，再推进根排版C。

B2A已按最终AXAML版本完成专项、清理及聚焦检查，见[光标状态与字号结果](input-otp-caret-results.md)。B2B/C保持开放。

### G3-B2B 输入光标时间线合同

本地Ant Design 6.6.6的OTP使用原生Input，仅主题化caretColor/布局，没有自建blink周期；本地Avalonia TextBox默认CaretBlinkInterval为500ms，独立于装饰Transition。AtomUI的既有OTP则为1s完整线性往返、关键帧1→0→1，且IsMotionEnabled只影响输入表面Transition。保留这一输入反馈合同，不借主题重构改变为原生开关闪烁，也不把装饰motion开关重新解释为光标周期；明确配置独立周期才能改变它。

- 追加Own `CaretBlinkDuration`（完整周期，默认1s，非正值停止blink并保留静态可见反馈）及`CaretBlinkEasing`（默认Linear）。沿V2、修订2，保留四个既有Own名称/类型/公开ID，新增ID只追加，现有ID与slot分离继续验证。没有跨owner或Global装饰duration的隐式倍数。
- 三个关键帧仍在AXAML中定义。Style.Animations是共享只读列表、Animation sealed、IAnimation的Apply及IClock不可由客户端实现，不能把每实例动态资源绑定到共享非Visual Animation。两个Own配置拒绝的反例已复现，旧四ID正对照通过。最小原型发现RunAsync拒绝Infinite；改用caret独占的原生Style及Animation副本，已实测隐藏暂停、恢复相位及移除Style后恢复Opacity。内部Rectangle派生caret承接typed Own Setter，不新增时钟或定时器，不改Shape绘制、布局和命中。
- 动态duration/easing只在有效值改变时更新，普通颜色/字体热更新不重启blink。可变easing复制，等价参数不误重启。native PlaybackBehavior显式OnlyIfVisible，保持原样式动画的隐藏暂停/恢复相位；blur、detach和模板退役取消并释放。IsActive与现有主题显隐保持唯一输入真源，不在Cell重建caret字段。
- AC先验证1s默认周期及曲线、2s配置、easing、非正值静态可见、清除恢复；明暗、装饰motion=false仍保留既有输入反馈节奏。相同实例A→B→A、正常输入/只读/禁用/隐藏暂停和重挂载、实际动画/clock释放与旧Own literal兼容均单列。
- 默认几何/非闪烁全RGBA0；动态比较必须记录真实相位/周期和曲线，不用关闭动画或同相位挑帧掩盖新旧差异。JIT/当前Mac NativeAOT实际消费及mutable隔离，临时源/媒体清理后OTP/Core受影响精确filter、NET8、成员审计和正式文档；其他原生显示/平台不推断通过。若最小原型不能保留默认时间线，先留证诊断，不擅自批准差异。

B2B已验收，见[时间线结果](input-otp-blink-results.md)：7临时+66既有检查通过，明暗16张静态新旧RGBA0，当前Mac AOT实跑与JIT十八图0；真实周期/缓动/隐藏相位、motion=false、负/零值、清除及GC通过。临时验证清理后OTP66、NET8零警告、成员审计及正式文档链接通过。继续C排版，完整家族与CT25出口仍开放。

### G3-C 根与分隔符排版

保留未配置时根/分隔符对真实父字体与字号的继承，cell继续既有Large16/Middle14/Small12/Custom14的自身预设，不能为统一字号改变默认separator。明确配置OtpLineEdit有效Global FontFamily及对应三档FontSize时，根/分隔符进入该输入；cell继续已有typed Global消费，同一实例清除分别恢复原继承/预设。复用ControlTypographyBindings的8输入可选字号向量（无IsCustomFontSize API），AXAML完成最终Setter，根仅提供内部只读继承源。不添加Own字段、旗标或资源ID。根FontSize与cell独立预设的既有职责不扩成新的跨优先级中继；需要改cell字体的应用使用专用Cell Style。

临时验证先证明Global字体族与separator字号不消费；同时锁定未配置父字号、各尺寸cell、typeface、Style/local覆盖、清除/换父及GC。真实Skia明暗四尺寸/四variant和输入状态前后与往返比较，正常光标时间线保持；当前Mac NativeAOT实跑，临时源/驱动/媒体清理后OTP/Core精确filter、NET8与成员审计。若发现独立公共子Control身份或实际typeface丢失，先取得失败再定最小接线。

G3-C字体专项完成，前后、AOT、清理及最终聚焦结果见[根与分隔符排版](input-otp-typography-results.md)。旧默认暗色separator前景缺陷于2026-10-24获用户批准，按[颜色计划](input-otp-color.md)执行独立分包；完整输入选择家族/CT25仍开放。

### G3-D 已批准的分隔符颜色

[颜色结果](input-otp-color-results.md)：普通／禁用有效Global在根ControlTheme声明，默认与批准原型四图完全一致；7临时RED→GREEN、96cell实际输入／几何／优先级及38帧Mac AOT-JIT通过。原生直接Style移除与栅格边缘分别记录CT25，不能计作完整家族完成。

## CheckBox / RadioButton 接续结果

[CR1默认依赖](input-check-radio-defaults-results.md)及RadioBgColor Legacy分类完成；[CR3现有Global入口](input-check-radio-global-results.md)切片完成。只回填已验证的默认依赖、已有颜色/开关/间距与owner消费；字体、其余Global和完整家族/CT25仍开放。

[CR4字体结果](input-check-radio-typography-results.md)：四owner的可选FontFamily/FontSize进入真实字形，原生默认与父继承保持、当前Mac433/433及GC通过；清理和原51项验证完成。其余输入选择、全家族及CT25继续。

## ComboBox 接续

[CB1默认结果](input-combo-box-defaults-results.md)完成十一项Global-only V2迁移，默认/既有配置/弹层前后0像素。ItemDisabledColor消费缺口及HandleHoverColor真实hover命中核对单独继续，不能用默认资源存在关闭完整家族。

[CB2禁用色](input-combo-box-disabled-results.md)与[CB3专用Style路由](input-combo-box-item-route-results.md)完成本机切片，精确视觉分别批准；HandleHoverColor已确认原有正对照，未重复修改。其他Global/状态/完整家族及CT25继续。

## Select接续

[S1默认协议结果](input-select-defaults-results.md)完成25字段本机V2迁移；原/候选77/155严格恢复FAIL保持CT25开放。其余消费者/字体/Part与完整家族继续。

[CB4显示区文字结果](input-combo-box-display-colors-results.md)的具体新视觉已独立批准，全部临时清理及69/8项最终验证完成；仍不代表完整ComboBox。下一入口按[NumericUpDown/ButtonSpinner核对](input-numeric-spinner-audit.md)建立真实Own边反例。

[NU1结果](input-numeric-spinner-defaults-results.md)的18背景变化与N01-N14已独立批准，临时清理及65/1项最终验证完成；NU2继续Shared实际owner，不混合7/14图标职责。

[NU2-A结果](input-numeric-inline-icons-results.md)的四对配置glyph与四对自然差异已独立批准，临时清理和原49项完成；Numeric token.md按实际无独立Own定义纠正，后续分隔/其他Global/家族出口继续。

[NU2-B结果](input-spinner-stroke-results.md)18描边变化及6默认hover差异已独立批准；清理后65/1通过。NU3继续两条Numeric inline硬编码分隔边，按父边框对应边投射，不改既有内部Handle取Left算法。

2026-10-28 CHT [Cascader高度结果](input-cascader-dropdown-height-results.md)：已按用户选择合同合并Own上限与较小名义页数预算；6有效RED→最终15PASS24s，原生实际Overlay/Native managed及AOT各1044，156内容不减为144。NET8零27.02s、成员1辅助文件/0Control/0问题；6临时源、426工程文件（1,220,202,448 bytes）、313媒体已清理且audit clean，精确Cascader51PASS3s。两后端各78对/70变化，[限定审查](input-cascader-dropdown-height-visual-review.md)包括默认320→180、自然列宽193→186（单变量证明确由实现标签范围导致），当前等待明确视觉确认。其余8对0，原整Panel复位3像素保留CT25；完整家族/其他模式/列/平台开放，不提前Git提交。
