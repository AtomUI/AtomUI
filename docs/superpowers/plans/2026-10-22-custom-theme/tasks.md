# 精确主题优化与重构任务目录

本文的进度表是CT任务状态的唯一总入口；全局约束与阶段出口见[路线图](overview.md)。下表是任务包，CT-00–CT-02的详细步骤见[首批执行计划](readiness.md)；其余包须在前置出口通过后细化文件、接口和验证，不作为现成产品实现计划。

## 0. 执行进度

2026-10-24新增授权：完成CT-10全部子任务，并继续完成CT-11至CT-26；沿当前分支和worktree、不自动提交、不用subagent。CT-24后由用户明确暂缓，本轮先不做；其余工作继续。

2026-10-25用户要求完成全部进行中任务，当前执行集合为CT-11–CT-19及CT-25；不把分包通过、视觉批准或提交当整项完成。

更新日期：2026-10-25。工作位置为`feature/custom-theme`及现有worktree，源码审计基线`85407138f`；阶段提交`79f2fc00e`记录截至CT-08A的成果，本轮已持续推进至CT-19颜色选择及CT-25平台分包，机制及各分包清理后聚焦证据见下表，本轮修改继续交用户审查，不自动提交，整体重构尚未完成。CT-00/CT-01完成的是目录与源码分类，不表示控件或主题机制已经修复。**已完成CT-04/CT-05及CT-06机制和DataGrid17项兼容接入；Own V2持续按家族迁移，DataGrid78字段和ColorPicker24字段的后续默认计划已有独立结果；完整控件家族验收仍为0项**，不能把准备工作换算成视觉验收完成率。

| ID | 状态 | 已完成与证据 | 下一步／未完成门槛 |
| --- | --- | --- | --- |
| CT-00 | 已完成（目录） | [已提交目录](control-inventory.csv)721条；本轮按实际模板新增内部信息面板1条，当前722个主键无重复；原720条双向基线见[目录范围](baseline.md#目录范围) | 305个owner待决保留给对应家族，不伪造identity |
| CT-01 | 已完成（源码分类） | [字段记录](token-contract-review.csv)95个Own终端／1195字段，默认依赖及消费者源码；90组ID映射校验通过 | 123未决字段、124合同决策由对应家族处理；无Approved字段，不代表实际消费已验收 |
| CT-02 | 已完成（代表基线） | [执行清单](readiness.md#task-ct-02建立代表失败与host性能基线)、[结果](baseline.md)：12个目标反例失败／1正对照通过；Skia A/A及1DIP检错；实际Window/Popup Host；启动/热更新和释放样本 | 独立审查Critical/阻断Important均0；原生最终显示面无输入闪动仍归CT-12/CT-25专项，不能声明无闪烁 |
| CT-03 | 已完成（接口合同） | [正式具体接口](../../../architecture/systems/theming/own-token-evaluation.md#9-ct-03具体构建与运行接口)及[实施计划](own-defaults.md)已核对旧路径／版本／跨包／格式边界 | API已由CT-04/05实现；尚未发布，不代表95个产品Token已迁移 |
| CT-04 | 已完成（实现与专项） | [CT-04A/B清单](own-defaults.md#task-ct-04a可用的版本化声明与分派合同)：版本分派／生成计划；20个临时生成用例、75项聚焦对照；.NET8构建、跨包macOS26/arm64 NativeAOT运行通过 | 已随CT-05清理并通过机制审查；没有迁移产品控件或批准全平台 |
| CT-05 | 已完成（实现与专项） | [实现与验证](own-defaults-results.md)：有效Own／存在位、算法／appearance、缓存、惰性／重试及scope；清理后Core47／Generator63通过，跨包AOT实际Compiler通过 | 机制最终审查Critical/Important/Minor均0；进入CT-06，不代表产品控件已迁移 |
| CT-06 | 已完成（机制与17项兼容） | [结果](aliases-results.md)：实际消费者4/4；产品Skia37对差异0/1DIP检错8649像素；清理后Core58/Generator64/DataGrid22通过，NET8构建和当前Mac动态AOT通过；作者自审完成 | 全部临时源/媒体已清理；完整DataGrid家族、compiled literal别名NativeAOT及原生显示面仍按CT-18/CT-25验收 |
| CT-07 | 已完成（最小原型） | [结果](qualification-results.md)：9/9PASS，Card编译专用Style、重置、优先级、资源拒绝及10代frame清理；清理后Card11/Core21/Generator1通过，作者自审完成 | 临时源已清理；产品品牌装配/跨根事务、性能与最终视觉仍属CT-08/CT-25 |
| CT-08 | 已完成（机制） | [A结果](style-assets-results.md)；[B结果](style-hosts-results.md)：Context和Application/Provider/Window lease接线、退役；临时Core19+补充1/Desktop3通过，清理后Core50/Desktop16、NET8零警告，代表Skia前后0像素差异，作者自审完成 | [C结果](style-transactions-results.md)：统一协调/继承重置/NoOp、完整stamp、版本1表现结果和有界清理；临时Core17/Desktop3通过，清理后Core58/Desktop17、NET8零警告，默认前后/品牌恢复0像素差异；作者自审通过。公开注册/manifest属CT-09，多平台/性能/全控件仍待后续验收 |
| CT-09 | 已完成（机制） | [计划](brand-manifests.md)、[结果](brand-manifests-results.md)：显式规范登记/能力、版本1JSON与完整/TokenOnly导出、选择身份及同runtime事务；临时30/30通过并清理 | 清理后Core115/Desktop17、NET8零警告；默认HEAD79前后0px、10次完整往返0px、真实两类Popup、公开仅登记0px；作者自审完成。工作台/AI/全控件及平台仍待CT-22–CT-26 |
| CT-10 | 已完成（共享消费） | [S1–S5计划](shared-consumers.md)、[总结果](shared-consumers-final-results.md)与[104项实际消费者](shared-consumers-catalog.md)：10根owner排版/前景、25既有身份、内部/公开子Control边界闭环；前序箭头/两类Popup/框体/图标/启动分包合并验收。本轮59+2临时验证已清理、净增常驻0；明暗26同轨迹旧新默认/恢复0、Mac AOT/JIT36帧0；最终Desktop960及Combo13/Core3、NET8零警告，作者自审与媒体清理完成 | CT-10无未决子任务；完整家族Own/业务状态/持续动效/平台专项继续CT-11–CT-26，不计作这些任务已通过 |
| CT-11 | 进行中 | [动效消费计划](motion-consumers.md)：[M1结果](motion-wave-results.md)：曲线/范围/形状与缓存修复，默认固定时点0px；[M2结果](motion-transitions-results.md)：四模板动态时长、作用域/优先级/GC，临时10通过已清理，默认明暗终态0px、清理后Desktop76/NET8零警告，当前Mac NativeAOT运行通过；[M3结果](motion-exact-results.md)：精确时长/曲线、Splash周期/比例/停用、真实动画与合成时钟、可变easing隔离通过；清理后Core45/Controls3/Generator63/Desktop76及NET8零警告，当前Mac NativeAOT实际值/恢复通过；[扩展结果](motion-consumers-expanded-results.md)：其余83组/74文件接入、370资产实际物化，18项旧新RED→GREEN及30默认/30AOT对照0px；临时源/媒体已清理，最终Desktop2184/Core27/Grid39/Gallery22与NET8通过。全owner状态出口随家族回填 | [83路径](motion-transition-catalog.md)的逐owner业务状态、连续动效随家族完成后总验收 |
| CT-12 | 进行中 | [家族计划](buttons.md)；[首包结果](buttons-defaults-results.md)：四终端91字段、字重和shape已验收，48实例明暗五状态默认0px、110默认值相同；清理后Core54/Desktop92及NET8零警告，当前Mac NativeAOT实际依赖/字重/shape通过。[Option19结果](option-buttons-results.md)：第五终端19字段、两identity与动态时长通过；明暗默认五状态0px，清理后Desktop30/NET8零警告、当前Mac AOT通过。[消费分包结果](button-consumers-results.md)：六Global身份/三动态集合、Dropdown23新增Own隔离、Hyper绑定优先级和Toggle四画刷通过；明暗五状态0px、清理后Buttons193与NET8零警告，当前Mac AOT通过。[原生稳定性结果](buttons-native-stability-results.md)：当前Mac实际显示面40帧、五阶段内均0px，1DIP检错29220、B变化253549及返回0。[六owner字体族结果](buttons-font-family-results.md)：可选Global/真实继承/优先级与GC、24实例明暗四状态0px及恢复0、当前Mac AOT通过，清理后Core30/Desktop223、NET8零警告。[其余消费分包](buttons-remaining-consumers-results.md)：显式默认禁用描边/实际Flyout owner间距通过；默认明暗/展开0px、原生API同阶段配置及恢复0px；原有9/5RGB1边缘单列CT25，清理后361项与NET8零警告。五个历史字段已明确Reserved/Legacy、保留兼容且非有效编辑入口；持续动效和完整输入继续推进。Button依赖失败及B10离屏稳定性对照；S2新旧A/B均复现Overlay owner边缘恢复4px（RGB1），新旧同阶段0px；Option往返2px/RGB1也旧新同阶段0，均须专项解释/处理 | 等所需机制；逐按钮identity、实际视觉与原生闪动专项 |
| CT-13 | 进行中（分包） | [输入目录](inputs.md)；[R结果](input-resize-results.md)：ResizeHandleSize命中/可见绘制及owner线宽已接通；默认指示线恢复可见经用户批准（明暗88px），统一阶段配置清除0px；真实拖拽/滚动、Mac AOT通过，临时源/媒体清理后395项与NET8零警告。[F1结果](input-font-results.md)：LineEdit三档Own/Global字号及字体族、原生继承/优先级已接通，明暗四状态及清除0px、Mac AOT通过；[G1记录](input-global-results.md)：58处资源投射、四owner真实消费与当前Mac AOT通过，清理后Input136/NET8零警告；默认状态文字颜色已获用户批准、其他边缘/往返像素差异归CT25未决。[V1结果](input-defaults-results.md)：16字段迁移V2、96值相等，同轨迹新旧默认/配置/恢复0px，Mac AOT通过；清理后Core33/Generator1/Input-OTP198及NET8零警告。[F2记录](input-font-remaining-results.md)：三owner可选字体族、Search全尺寸Global字号及Embedded隔离通过；新旧四默认状态0、原生API新旧六阶段0，字体清除合成边缘现象原来即存在且继续CT25；临时源/媒体清理后Core26/Desktop195及NET8零警告。[G2记录](input-frame-owner-results.md)：共享frame及clear/reveal的95处资源接入真实owner，内容路线/同scope迁移和GC通过；26控件明暗六默认阶段0，真实点击48+12/主题。Mac NativeAOT运行通过且与JIT四图0，临时源/媒体清理后Core21/Desktop525及NET8零警告；配置往返/性能出口仍开放。[G3-A结果](input-otp-global-results.md)：OTP45处有效Global消费、17项专项及真实输入通过；96格明暗六默认阶段/清除0，Mac AOT运行及JIT对照四图0；临时源/媒体清理后OTP65及NET8零警告。[B1/B2A/B2B](inputs.md)已按各自结果完成；[C排版结果](input-otp-typography-results.md)已接通可选根字体与分隔符字号，20默认/22AOT画面0px、临时11已清理、最终OTP66/Core1与NET8通过。[D颜色结果](input-otp-color-results.md)完成用户批准的普通／禁用前景修复：73项、批准原型四图0、38AOT-JIT0，临时清理后OTP66及NET8零警告；其余输入选择继续，不能以分包代表家族完成 | 逐输入／选择控件及状态整改，保留焦点与输入 ；用户2026-10-24明确允许默认分隔符ColorText／ColorTextDisabled颜色修复，按[颜色结果](input-otp-color-results.md)完成独立消费验证，不再等待批准 |
| CT-14 | 进行中 | [执行计划](container-consumers.md)：85项metadata/258字段范围；T37及补查共14项保留分类完成，工作台规则仍待CT23；[列表结果](container-list-results.md)：两终端30字段V2/60默认及ID相同、禁用Own/Global与五继承主题实际接通，临时10清理、最终262项/NET8零警告；当前Mac AOT/JIT28帧0，旧版公开API及同源RGBA对照解释原生微差 | 继续N2其他Global/字体与实际业务Host、N3其余字段/Custom及N4完整家族出口 |
| CT-15 | 进行中 | [执行计划](feedback-consumers.md)：137项metadata/274字段；[Timeline P1结果](timeline-consumers-results.md)：Style/实时颜色、明确末项最小高度及Own依赖已接通，临时14清理、默认12/AOT18帧0，最终Timeline52/NET8零警告；[P2排版与Own](timeline-typography-results.md)：20项临时已清理，默认20/AOT32帧0，Padding测量及extent修复，最终Core13/Desktop118/NET8零警告 | Timeline P3 Compact/RTL/完整状态/极值及平台；其余反馈展示逐Control、Part、弹层及形态验收 |
| CT-16 | 进行中 | [执行计划](date-time-consumers.md)、[D1结果](date-time-width-results.md)：TimePicker8历史字段V2，Period列／footer修复；[D2-A结果](date-time-geometry-results.md)：行高变化保留逻辑视口，旧新18兼容帧0／Mac AOT24帧0；[D2-B结果](date-time-cell-results.md)：公开Cell字体／状态颜色及专用AXAML Style、既有配色与继承优先级已验证，21临时清理，最终Core13/Desktop133及NET8零警告 ；[D3-A范围结果](date-time-range-results.md)：两字段可选Own覆盖及动态指示条宽／坐标／底边修复，17临时清理，明暗30帧／Mac AOT-JIT30帧通过，最终133项／NET8零警告 ；[D4-A Global结果](date-viewer-global-results.md)：26处基础资源接通实际DateViewer／Range owner，14临时清理、最终337项／NET8通过，明色原生资源同值17图0；暗色10／24 RGB1及AOT一图10像素归CT25未闭合；[D3-B Own结果](date-time-own-results.md)：四终端43历史字段V2，86默认／ID／slot相同，实际行距／农历高度／业务条偏移更新，14临时清理，351项与Mac AOT-JIT30帧通过 | B的AOT38帧34为0，4帧触发框3像素另有旧版整帧对应0；选中行残留由旧版支持的背景色往返复现，外框及D1范围圆角具体归因继续CT25；D2内部TimeView Global／非法行高、四日期终端剩余逐字段消费、Global与完整状态继续；重套模板旧部件保留已复现并交CT25，完整家族仍0 |
| CT-17 | 进行中（分包） | [绘制计划](drawing-consumers.md)：P0普通线形边界／动态字体与格式修复已获批，第三轮后续34文件经批准提交推送；[P1-A结果](progress-token-results.md)：13历史字段V2／26默认及ID／slot相同，两项明确Own接线获批，明暗各784像素、默认／重置0；当前Mac managed218／NativeAOT212检查及清理后204项、NET8零警告通过。测试宿主region单变量使原七控件42对A/A为0，不消费历史7像素许可。Skeleton端点helper已补查；[Steps边界结果](steps-progress-rounding-results.md)：真实字体末端1 DIP修复、明238／暗239像素已获批准，临时源和24张媒体清理，完整家族仍未验收 | [P1-B结果](progress-circle-results.md)的22项／144帧与真实Mac JIT／AOT各96项已执行，圆形尺寸／结果状态视觉已获批，临时源及244个媒体文件清理，204项聚焦、NET8及6文件成员审计通过；[P1-B3](progress-circle-fit-results.md)的44项反例、132帧及Mac managed／AOT各130项已通过，视觉已批准且临时取证清理，清理后42项与NET8通过；其余Own、Global及状态矩阵仍开放；测试宿主改动仅限Headless/Skia，原生最终显示面及完整家族保持开放 |
| CT-18 | 进行中（G1默认计划完成） | [G1结果](data-grid-default-results.md)：78字段／17别名／61默认函数，14项功能GREEN，312条默认值／类型／slot／ID相同；Mac managed／NativeAOT各37项，216帧旧新108对0px、72组AA0、72组1DIP全部检出；全部临时源／工程／媒体清理，34项精确检查、NET8零警告及成员审计通过 | [计划](data-grid-consumers.md)的G2其余实际消费者、虚拟化／冻结／裁剪和G3完整家族开放；[静止指针hover结果](data-grid-hover-results.md)已通过限定视觉审查及清理门禁；第七轮真实Windows Grid309全部通过（含W14）；完整G2／G3继续 |
| CT-19 | 进行中（分包） | [B08文字结果](gradient-text-results.md)、[C1依赖结果](color-picker-defaults-results.md)、[C2输入结果](color-picker-input-sizing-results.md)、[C3历史formatter结果](gradient-legacy-formatter-results.md)：动态前景／恢复、四条Own依赖、两项输入Own及格式／Alpha／尺寸恢复已取得前后与Mac managed／AOT证据；所示视觉变化及逐场景限定合成差异已获用户明确批准，专用媒体已清理，清理后20项及NET8通过；[C4绘制结果](color-picker-drawing-results.md)：空值斜线／选择环有效颜色与线宽接线、0／恢复／owner隔离及GC通过，初始与补充视觉和限定历史3像素已获批，媒体清理；[C5位图顺序及挂载](color-slider-bitmap-order.md)：旧请求覆盖真实RED/GREEN，清理后Desktop23／Core17／NET8通过，重挂六对Alpha变化已获批准并随第十批提交；[C6输入重挂结果](color-picker-input-lifecycle-results.md)：权威输入回放、无变化保留草稿／校验／光标及原生模板格式绑定完成，20临时／Mac managed与AOT各308／清理后23及NET8通过，324帧及59对限定差异已获批准，临时源／工程／媒体清理，已随获批第十一批提交0c6d8445679e，C7色谱请求顺序继续 | 其余Own／Global／Part／状态与完整平台／原生最终像素继续；本行不表示完整家族验收 |
| CT-20 | 未开始 | 尺寸／Custom候选已挂各家族 | 随家族逐属性区分基础值和预设，不批量替换 |
| CT-21 | 未开始 | GalleryBase／Gallery目录已纳入 | 工具壳自身Token、预览owner隔离及视觉对照 |
| CT-22 | 未开始 | — | 等品牌与控件机制；三品牌及边界配方真实渲染 |
| CT-23 | 未开始 | — | 手动工作台编辑／预览／应用／保存／往返闭环 |
| CT-24 | 未开始（用户暂缓） | 用户2026-10-24明确“这个先不做”，本轮不接AI服务 | 后续需要另行恢复授权；不再将实际endpoint／模型信息列作本轮阻塞 |
| CT-25 | 进行中（分包） | [资源编号兼容结果](token-resource-ordinals-results.md)：96范围1478ID冻结、公开ID/slot分离、独立旧producer JIT及当前Mac NativeAOT实际读取通过；明暗默认五状态及同焦点恢复0px，临时源/媒体清理后Core36/Generator74/Desktop128与NET8零警告。[基线](baseline.md)记录macOS Host、性能／释放及未验证边界；S2记录持Child时原生closed host/anchor缓存，新旧一致，完整释放图GC通过；[第七轮](platform-seventh-followups.md)：失败TRX完整摘要、真实Skia命中与保留原滚动触发的测试输入适配完成，清理后Mac／本机Linux arm64 Gallery各412、W14模块各9通过；第七轮真实Windows Grid309通过，Desktop另11项测试合同见[第八轮](platform-eighth-followups.md)；Mac选测6290全通过，Linux包消费超时同SHA复验中 | [启动热查询结果](startup-token-lookup-results.md)：真实Startup Global/Own/scoped Own及语义标量查询降为0B，追加slot/身份校验通过；17消费者明暗七阶段RGBA0、Mac AOT实跑及JIT四图0，清理后Core64/Desktop525/Grid1与NET8通过。G2完整热切换性能仍开放；随机制／家族补真实trim/AOT／各Host平台和性能对照；[有限切换原生结果](buttons-native-stability-results.md)已补，持续动效/输入/全部平台待验 ；[Picker模板终版](picker-template-results.md)已修复旧部件保留／有效模板替换会话，34项临时及45相邻通过、Mac managed／AOT各851检查通过，临时源与媒体已清理、所示视觉及限定2像素已获批准；完整日期家族及平台最终像素仍待验收 ；用户允许使用github.com/AtomUI/AtomUI的Actions验证其他平台，明确禁止发布到nuget.org；[多平台结果](platform-validation-results.md)：AtomUI凭证可用、默认关闭的三OS／两Browser模式入口已准备；当前Mac Browser两模式各23断言、原生AOT25断言通过，临时16验证已清理，最终34项精确契约／tooling iterate通过。cfb1d6668已获批提交推送并运行首轮Actions：3原生、2 Browser通过，3选测失败；[后续修复](platform-followups.md)20文件经批准分两次提交为938f6e3c9、1fe3358ec并推送；[第二轮CI](https://github.com/AtomUI/AtomUI/actions/runs/40061147545)已结束：3原生／2 Browser通过，3选测失败；Mac Desktop4,128与Generator465通过后有2项包元数据断言失败，Linux Desktop9失败，Windows选择路径失败；[第二轮后续包](platform-second-followups.md)12文件获批提交847c441c1并推送；[第三轮CI](https://github.com/AtomUI/AtomUI/actions/runs/40068141876)五个运行时通过，三个选测失败（Windows旧清单测试的Win32 shell入口、Mac Desktop6项、Linux Desktop12项）；[第三轮后续包](platform-third-followups.md)34文件获批分两次提交fc9896fb6／e9bc9e707并推送；[第四轮CI](https://github.com/AtomUI/AtomUI/actions/runs/40089581608)已结束，5运行时通过，3选测失败；Mac／Linux Desktop各4128通过，Windows 6个POSIX stub Skip使证据不完整，Gallery Mac debounce1／Linux点击3失败，见[第四轮后续](platform-fourth-followups.md)，整体仍开放；[第五轮跟进](platform-fifth-followups.md)：29文件批准后提交975fc8e34／57ae294d4并推送；本轮3 NativeAOT／2 Browser及Mac选测6272项通过，Windows DataGrid2失败／Linux Desktop隔离1失败，整轮failure；[第六轮候选](platform-sixth-followups.md)已完成W15提交顺序及W13所属dispatcher机制修复，清理后基础15／桌面35／Grid73与NET8通过；W14补充失败阶段后随获批30文件提交ef09198b9运行[第六轮CI](https://github.com/AtomUI/AtomUI/actions/runs/40149666278)，三原生／两Browser通过，Windows W14失败、Linux Gallery19高亮失败、Mac取编译器源码遇网络失败并已对同SHA重试；最新处置见[第七轮执行记录](platform-seventh-followups.md) |
| CT-26 | 未开始 | — | 汇总逐Control／90组子项、兼容与视觉证据，清理并最终审查 ；按用户决定，本轮排除暂缓的CT24 |

2026-10-25最新平台出口：[第十批](platform-tenth-followups.md)已按批准提交abd62d849／9e0855940并推送，[Actions40163904747](https://github.com/AtomUI/AtomUI/actions/runs/40163904747)八个job均结束。三NativeAOT／两Browser通过，三OS前八测试项目各5664项通过；Windows import及Grid原失败通过，后续仍因build-host.log清理占用失败，Linux实际包消费600秒总限失败，Mac包消费通过后Gallery的Tour命中1项失败。未运行到的后续项目不计通过；[第十一轮取证](platform-eleventh-followups.md)继续定位，不宣称平台最终通过。

CT19追加[C6输入重挂结果](color-picker-input-lifecycle-results.md)：离树权威颜色／格式变化回放，无变化保留草稿／校验／光标，格式选择改用原生模板绑定；临时20项与Mac managed／NativeAOT各308检查通过，清理后Desktop23、NET8 Release及成员审计通过。324帧及独立直接渲染证据、59对精确限定已获用户批准，临时源码／工程／媒体已清理；仅C6切片完成，已随获批第十一批提交0c6d8445679e，完整CT19／原生像素验收继续。

CT25追加[第十一批](platform-eleventh-followups.md)：消费工程私有NuGet路径／feed与共享source分离，full先构建全部源码、其余消费者真实编译并复用源产物；缺产物正对照明确失败，清理后完整Official四图约136秒通过。Windows仅增加占用PID只读取证和原消费／清理双异常保留，Tour仅追加失败几何；其原始CI根因尚未关闭。清理后Tour2项及C6既有23项／NET8证据通过，四份临时源已删除。13文件获批提交0c6d8445679e并推送，[第十一轮CI](https://github.com/AtomUI/AtomUI/actions/runs/40168600469)运行中；C6已包含在该提交，完整CT19／CT25仍开放，后续C7不在本批授权范围。

第十一轮最终七个job成功，仅Windows选测因三个.NET Host持有build-host.log清理失败；Mac／Linux各6290项通过、Windows5664加本地化4项通过，三OS各27个子进程阶段完整配对。[第十二批](platform-twelfth-followups.md)在日志所属ILC8构建子进程关闭持久服务器及节点复用；调用边界两项RED→GREEN、实际托管编译／help／身份复用和清理、8项工具检查及已提交产品基线的完整Official约141秒通过。五文件获批提交b8325380f671并推送，[实际第十二轮CI](https://github.com/AtomUI/AtomUI/actions/runs/40172623312)的三NativeAOT／两Browser、Mac／Linux选测各6405项通过；Windows本地化集成5项、实际Official及清理通过，原日志占用本轮未复现。Windows随后因Linker反例加载后的Empty.dll删除失败，5993项中5992通过，后续Gallery412项未运行；[第十三轮候选](platform-thirteenth-followups.md)继续定位与验证，CT25仍开放。C7不在本批提交范围。

第十三批4文件获用户批准，提交f24a9aba7710并推送；[三OS／两Browser第十三轮CI](https://github.com/AtomUI/AtomUI/actions/runs/40175124825)运行中。坏IL反例改从同一输出读取内存镜像，保留真实JIT异常及receipt拒绝；Mac精确13项与最终Linker模块89项通过，Windows确认仍待CI。本批不含C7，不代表完整CT25验收。

CT19追加[C7色谱结果](color-spectrum-lifecycle-results.md)：异步与fast统一请求顺序、归一范围／退化轴及共享helper完成，受控26项、有效亮暗514项和Mac managed／NativeAOT各184项通过。140帧与28组1DIP、独立直接Skia及公开输入对照完成；所示修复与41对限定合成差异获用户批准，全部临时驱动／工程／媒体已清理，最终Release23项、NET8及成员审计通过。C7切片待独立Git审查，C8的公开Components默认与实际模板不一致、既有键盘可达性继续；完整CT19未关闭。

最新执行：C7已随获批第十四批分为纯布局3240285cc与修复3e7efb341并推送。[第十三轮](platform-thirteenth-followups.md)Windows原Linker删除失败未复现，6404／6405通过，仅Gallery Tour卡片命中失败；Mac／Linux各6405通过。[第十四轮](platform-fourteenth-followups.md)Windows／Linux各6405全通过、Mac6404／6405，仅Gallery Timeline卡片命中失败；两轮的三平台NativeAOT及两Browser均通过。Tour／Timeline间歇性命中问题仍按实际几何定位，不能因另一次通过而关闭CT25。

CT19 [C8短设计](color-spectrum-components.md)的SV公开默认值与六模式接线已获用户明确批准；[结果](color-spectrum-components-results.md)记录14项有效RED→GREEN、扩展22项通过并删除、清理后ColorPicker27项及NET8 Release零警告通过。192帧包含有效亮暗候选494项、64组1DIP全检出；真实Mac managed／NativeAOT各310项通过。独立直接Skia32组A/A、12组恢复与6组新旧默认／SV／恢复均0；[30对自然合成限定差异](color-spectrum-components-noise-review.md)（2–23像素／通道最大2）已提交用户单独审查，不继承C1–C7批准。媒体／外部工程待审查后清理，Git未授权；既有键盘及完整家族仍开放。CT25 [第十五批调查](platform-fifteenth-followups.md)已建立排队滚动更新的两项有效RED→GREEN，原用例与临时探针5项通过，探针删除后原三个精确用例3秒通过；仅修改两个Gallery测试的实际帧准备，未改产品输入／布局，实际平台确认待下一轮获批CI。

状态规则：`未开始`→`规划中`→`进行中`→`最终审查中`→`已完成`。有真实外部障碍才标`受阻`并写明条件；依赖尚未满足不伪报外部阻塞。每项开始、验证、清理和审查后在此表更新状态和证据链接，详细步骤在其子计划勾选，实际数值写入结果记录。中途问题或状态问答不会撤销继续执行授权；处理后回到当前任务，不以一条总结代替继续工作。提交须单独遵守用户的明确提交指令。用户已明确要求后续所有工作由主agent独立完成，不再使用subagent；后续最终审查由作者另起审查步骤执行，如实标明自审边界。

CT-13追加[G3-B1结果](input-otp-geometry-results.md)：三处OTP几何与专用Cell/Separator Style命中接通；明暗六默认阶段/清除RGBA0，实际输入与NativeAOT Headless/Skia通过、JIT/AOT十八图0。临时测试/工程/媒体清理后OTP66与NET8零警告通过。B2/C与整个家族保持进行中。

CT-13追加[G3-B2A结果](input-otp-caret-results.md)：caret显隐及偏移全部移至AXAML、字号更新/恢复有效；明暗六非闪烁阶段新旧RGBA0、最终NativeAOT与JIT十二图0。临时验证/媒体清理后OTP66、NET8零警告和成员审计通过。B2B时间线、C排版继续。

CT-13追加[G3-B2B结果](input-otp-blink-results.md)：独立周期/缓动Own与原生每光标Style接通，默认1s线性往返、装饰motion=false及隐藏暂停相位保持；明暗16张静态新旧RGBA0，最终NativeAOT/JIT十八图0，实际时间线/输入/GC通过。7项临时验证、工程及媒体已清理，最终OTP66、NET8零警告、成员审计和24个文档链接通过。C根字体及separator排版继续，完整家族验收仍为0。

当前增量目录为721项（CT00基线720之外新增内部caret叶节点），Own字段1221项（CT01基线1195之外累计26项），V2十三个终端历史175字段（新字段26另计）；资源ID1480／96范围，B2B只追加两个新ID，原1478编号不变。这些是目录／机制计数，不换算为完整控件验收进度。

本次阶段提交前补验：Core启动/lease/资源29项、Generator资源键55项、Controls相关消费4项、Desktop按钮/输入/OTP530项，合计618项聚焦检查通过；NET8 Release零警告/错误。此为本次提交检查，不等同全量回归或CT26最终验收。

CT-16追加[D4-C页头结果](date-viewer-header-results.md)：范围Header以Style级编译绑定跟随实际Panel，专用Header Style可覆盖及恢复；Content默认2302／2060像素对齐差异已获批准，Compact不变。原边缘命中2失败→候选11项功能／输入／生命周期通过；Mac实际NativeAOT72项通过。5份临时源、专用发布与媒体已清理，最终日期337项及NET8零警告通过。原生最终显示面、既有暗色RGB1和保留owner时旧模板部件仍归CT25，完整日期家族保持进行中。

## 1. 基础合同与机制

| ID | 依赖 | 工作和边界 | 独立交付/验收 |
| --- | --- | --- | --- |
| CT-00 | 无 | 对照产品public Control、生成注册、Token、Semantic、主题导出和Gallery建立完整主目录；内部目标/抽象层/无Own控件单独分类 | 精确程序集/metadata name/identity/owner/平台/源路径及文档入口；候选不冒充完整Control数 |
| CT-01 | CT-00 | 核对每个终端Own语义及默认依赖，区分Active/Derived/Legacy/Reserved/Deprecated；明确历史字段、别名和未公开能力 | 分类记录及逐Control文档修订范围；无消费者不能标Active已交付，不为字段补造不存在功能 |
| CT-02 | CT-00/CT-01 | 复现代表性owner、Own依赖、框体、优先级、自绘、别名和动态问题；建立视觉兼容比较矩阵及同版本A/A对照，冻结真实Host、原始UX和采样条件 | 可重建失败配方、可重复视觉基线及每个Control的前后比较安排；未复现/未取证项不填通过 |
| CT-03 | CT-01/CT-02 | 明确新版Own声明、可访问静态默认函数、存在状态、依赖/重定义、descriptor/evaluator修订与旧路径迁移的具体API | 正式文档中的版本及公开签名审定；旧whole evaluator、slot与schema边界保持；作为CT-04/CT-05唯一接口输入 |
| CT-04 | CT-03 | Generator读取同/跨程序集元数据，验证依赖图并生成扁平静态求值顺序；声明层移动不改终端schema | 循环/未知输入/类型/重定义/版本混合诊断，确定输出、跨程序集和实际AOT消费 |
| CT-05 | CT-04 | Compiler按显式存在状态保留用户值，其余默认读取有效上游；缓存纳入evaluator修订，保持独立惰性槽 | 上游覆盖/显式下游/恢复/0/null/明暗/算法均成立；先用Button、Skeleton、ColorPicker证明再迁移 |
| CT-06 | CT-01/CT-02 | 别名在每个配置层先规范化，再按层合并；DataGrid17项规范字段与旧Table入口保持兼容 | 同层不同值冲突、同值归一、跨层正常覆盖；公开字段到真实消费者，导出及internal历史字段有版本决策 |
| CT-07 | CT-01/CT-02 | 以最小原型证明owner资格工厂、嵌套继承/重置、selector优先级和候选资源隔离 | 父padding/重置子foreground反例通过；实例/状态/专用Style覆盖符合原生规则；没有运行期反射或找节点改值兜底 |
| CT-08 | CT-07 | ThemeManager协调准备、接线、预激活、再校验/Token提交、不可回滚Publish与退役；版本化表现结果和跨根lease | 已打开/新建Native及Overlay一致；重入/stamp/激活/恢复/退役故障独立注入；普通观察者warning与表现失效区分 |
| CT-09 | CT-08 | 独立版本化品牌组合manifest及注册资产引用/修订/能力要求；复用Token编译与现有格式边界 | Token-only与完整配方范围明确，往返等价；未知资产/Control/版本有诊断，XML v1/ABI v1不静默变化 |

主要文件边界：

- CT-00/CT-01：现有审计CSV、控件公开合同、主题导出；参考[注册构建器](../../../../src/AtomUI.Generator/Registration/RegistrationModelBuilder.cs)、[Token定义遍历](../../../../src/AtomUI.Generator/DesignToken/ControlTokenPropertyWalker.cs)和[Semantic Part设计](../../../architecture/systems/theming/semantic-parts.md)。不为目录清点新增产品运行时注册路径。
- CT-03–CT-05：[ControlTokenDescriptor](../../../../src/AtomUI.Core/Theme/Schema/ControlTokenDescriptor.cs)、[ThemeCompiler](../../../../src/AtomUI.Core/Theme/Compilation/ThemeCompiler.cs)、[配置规范化](../../../../src/AtomUI.Core/Theme/Configuration/ThemeConfigNormalizer.cs)、[继承模型](../../../../src/AtomUI.Generator/DesignToken/ControlTokenInheritanceModel.cs)、[校验器](../../../../src/AtomUI.Generator/DesignToken/ControlTokenCalculationValidator.cs)、[schema生成](../../../../src/AtomUI.Generator/ThemeSchema/GeneratedThemeSchemaWriter.cs)及[生成入口](../../../../src/AtomUI.Generator/TokenResourceKeyGenerator.cs)。
- CT-06：[DataGridToken](../../../../src/AtomUI.Desktop.Controls.DataGrid/DataGridToken.cs)、实际模板/消费者及同一规范化边界；不能只改默认值测试。
- CT-07–CT-09：[ThemeManager](../../../../src/AtomUI.Core/Theme/ThemeManager.cs)、[事务](../../../../src/AtomUI.Core/Theme/ThemeTransaction.cs)、[Scope](../../../../src/AtomUI.Core/Theme/ThemeScope.cs)、[Provider](../../../../src/AtomUI.Core/Theme/ThemeConfigProvider.cs)、[专用Style生成](../../../../src/AtomUI.Generator/SemanticParts/SemanticPartManifestWriter.cs)及现有资源资产/格式所有者。是否新增文件、属性或结果类型由各子计划明确。

## 2. 共享消费与逐控件迁移

| ID | 依赖 | 家族/主要职责 | 独立交付/验收 |
| --- | --- | --- | --- |
| CT-10 | CT-02 | shared/Effective Global选择、InputControlFrame/AddOnDecoratedBox、root/frame、Icon、箭头、排版、Splash/TextSelectionHandle及跨根字体 | 正确公开owner和public子Control身份；既有实例/已打开弹层更新；无本地值压制或资源泄漏 |
| CT-11 | CT-02 | 动态Transition、motion/wave输入、WaveSpirit实际easing/时长/范围 | owner变化/主题切换更新且可释放，Transitions保留style setter；自绘实际消费公开输入 |
| CT-12 | CT-05/CT-10/CT-11 | Button、HyperLinkButton、DropdownButton、SplitButton、OptionButton及实际按钮派生项 | 行高/padding依赖、字重、阴影、shape恢复、三档公式与字段兼容；各identity独立 |
| CT-13 | CT-10/CT-11；有Own依赖者加CT-05 | LineEdit/TextArea/NumericUpDown/OTP、Select/ComboBox、CheckBox/Radio等输入选择 | 字体、frame、option padding、禁用态、resize/下划线几何、public属性和Part有效；保留输入与焦点 ；用户2026-10-24明确允许默认分隔符ColorText／ColorTextDisabled颜色修复，按[颜色结果](input-otp-color-results.md)完成独立消费验证，不再等待批准 |
| CT-14 | CT-10/CT-11 | ListBox/ListView、Tree、Menu/NavMenu、TabControl/TabStrip、Cascader/TreeSelect/Transfer、Pagination/Expander | 容器owner/回收、方向与selected/open状态、组标题、密度；T37未公开能力按CT-01结论处置 |
| CT-15 | CT-10/CT-11 | Alert、Card、Message/Notification/Dialog、ToolTip/PopupConfirm、Timeline、Avatar、Tag/Badge、FloatButton、ImagePreviewer/Carousel及Result | 实际区域/关闭图标/弹层尺寸/字体/阴影/形态入口有效；稳定Part；插画/命名色保留内容语义 |
| CT-16 | CT-10/CT-11 | DatePicker、TimePicker、Calendar及范围条/内部Presenter/公开子Button ；[D3-A范围结果](date-time-range-results.md)：两字段可选Own覆盖及动态指示条宽／坐标／底边修复，17临时清理，明暗30帧／Mac AOT-JIT30帧通过，最终133项／NET8零警告 ；[D4-A Global结果](date-viewer-global-results.md)：26处基础资源接通实际DateViewer／Range owner，14临时清理、最终337项／NET8通过，明色原生资源同值17图0；暗色10／24 RGB1及AOT一图10像素归CT25未闭合；[D3-B Own结果](date-time-own-results.md)：四终端43历史字段V2，86默认／ID／slot相同，实际行距／农历高度／业务条偏移更新，14临时清理，351项与Mac AOT-JIT30帧通过 | Own取正确owner，头部与范围标签可定制，字体/描边/密度不破坏日期状态、焦点和弹层 |
| CT-17 | CT-05/CT-10/CT-11；不含Own依赖者不等待CT-05 | Skeleton、Progress各形态、Rate、Slider、Switch、Separator、Watermark及相关绘制目标 | 标题/渐变/线宽/字体/几何实际消费；极值、细线及动态缓存；固定几何与开放形态明确 |
| CT-18 | CT-06/CT-10/CT-11 | DataGrid表头/单元格/过滤器/选择列/拖拽/展开图形及虚拟化 | 全部规范字段/别名、内部非Visual对象、自绘线条及Custom；原有滚动、裁剪、冻结和回收保持 |
| CT-19 | CT-05/CT-10/CT-11 | ColorPicker/GradientColorPicker、ColorBlock/Spectrum/SliderThumb/输入区域 | 尺寸依赖、HEX/HSV/RGB布局、文字/disabled更新、空值标记/线宽/选择环、scope与释放 |
| CT-20 | CT-01/CT-02；逐家族协调 | 58处Custom/Middle候选逐属性分离基础默认与预设；扩展至CT-00发现的其他尺寸入口 | 每处有实际职责及结论，Custom不接管其未拥有维度；保留三档、方向、Min/Max和输入几何 |

CT-12–CT-19的边界由CT-00精确Control目录补齐：表中示例不能排除未列出的Control，也不能以某个共享修复替代每个实际模板、状态和Part的验证。跨家族同名字段不自动继承配置。

全部控件任务共同遵守[视觉一致性硬门槛](../../../engineering/development/theming/verification.md#视觉一致性硬门槛)：各Control修改前采样，修改后同环境/配置/状态比较；正常和适用交互状态、明暗、尺寸/密度及弹层均有结论。未解释或未经批准的差异为零才可完成，不能只证明Token或Part已生效。共享修复涉及的消费者逐项纳入，CT-21工具壳也不豁免。

CT-10重点路径：[InputControlFrame](../../../../src/AtomUI.Desktop.Controls/Primitives/InputControlFrame.cs)、[框体主题](../../../../src/AtomUI.Desktop.Controls/Primitives/AddOnDecoratedBox/Themes/InputControlFrameTheme.axaml)、[ArrowIndicator](../../../../src/AtomUI.Controls/Primitives/ArrowDecoratedBox/ArrowIndicator.cs)、Core/Icon及实际跨根host。CT-11参考[WaveSpiritDecorator](../../../../src/AtomUI.Controls/Primitives/WaveSpiritDecorator.cs)和[主题](../../../../src/AtomUI.Desktop.Controls/Primitives/Themes/WaveSpiritDecoratorTheme.axaml)。

各家族子计划必须列出具体C#/AXAML、测试、控件overview/implementation/token/semantic-part/changelog及Gallery示例，说明哪些原合同保持、哪些需要版本决策。CT-20的结论随对应家族提交，不另起一次无差别批量修改。

## 3. 品牌、工具和交付

| ID | 依赖 | 工作和边界 | 独立交付/验收 |
| --- | --- | --- | --- |
| CT-21 | CT-10/CT-11 | GalleryBase中立工具壳、SemanticPartPreview、元数据/页头、代码查看器 | 工具自己的排版/密度Token，不污染被预览控件owner；代码字体/语法主题及合理断点单独说明 |
| CT-22 | CT-08/CT-09；完整交付加CT-12–CT-20 | Ant默认、shadcn、Bootstrap及零圆角/粗线/大字/紧凑/关闭动效配方 | 配方多维进入真实渲染；状态/尺寸/嵌套/弹层一致；缺Host能力有诊断 |
| CT-23 | CT-09/CT-21；完整交付加CT-22 | 复用workspace目录/主题菜单，完成草稿/有效值/预览/应用/保存、diff/恢复、完整导入导出 | 同一runtime，兄弟预览隔离，已有实例状态保持，Token与表现结果不混淆；取消/过期不覆盖新草稿 |
| CT-24 | 未开始（用户暂缓） | 用户2026-10-24明确“这个先不做”，本轮不接AI服务 | 后续需要另行恢复授权；不再将实际endpoint／模型信息列作本轮阻塞 |
| CT-25 | 机制及各家族逐批接入；最终等CT-04–CT-23适用任务 | 注册/资产/跨程序集、真实trim/NativeAOT、Browser与各OS、性能/生命周期对照 | 每个Host/平台分别有证据；无全量实例化、反射旁路、无界缓存或旧Visual保留；无证据平台不填通过 ；[Picker模板终版](picker-template-results.md)已修复旧部件保留／有效模板替换会话，34项临时及45相邻通过、Mac managed／AOT各851检查通过，临时源与媒体已清理、所示视觉及限定2像素已获批准；完整日期家族及平台最终像素仍待验收 ；用户允许使用github.com/AtomUI/AtomUI的Actions验证其他平台，明确禁止发布到nuget.org；[多平台方案](platform-validation.md)已检查验证流程且AtomUI凭证可用；cfb1d6668已获批提交推送并运行首轮Actions：3原生、2 Browser通过，3选测失败；[后续修复](platform-followups.md)20文件经批准分两次提交为938f6e3c9、1fe3358ec并推送；[第二轮CI](https://github.com/AtomUI/AtomUI/actions/runs/40061147545)已结束：3原生／2 Browser通过，3选测失败；Mac Desktop4,128与Generator465通过后有2项包元数据断言失败，Linux Desktop9失败，Windows选择路径失败；[第二轮后续包](platform-second-followups.md)12文件获批提交847c441c1并推送；[第三轮CI](https://github.com/AtomUI/AtomUI/actions/runs/40068141876)五个运行时通过，三个选测失败（Windows旧清单测试的Win32 shell入口、Mac Desktop6项、Linux Desktop12项）；[第三轮后续包](platform-third-followups.md)34文件获批分两次提交fc9896fb6／e9bc9e707并推送；[第四轮CI](https://github.com/AtomUI/AtomUI/actions/runs/40089581608)已结束，5运行时通过，3选测失败；Mac／Linux Desktop各4128通过，Windows 6个POSIX stub Skip使证据不完整，Gallery Mac debounce1／Linux点击3失败，见[第四轮后续](platform-fourth-followups.md)，整体仍开放；[第五轮跟进](platform-fifth-followups.md)：29文件批准后提交975fc8e34／57ae294d4并推送；本轮3 NativeAOT／2 Browser及Mac选测6272项通过，Windows DataGrid2失败／Linux Desktop隔离1失败，整轮failure；[第六轮候选](platform-sixth-followups.md)已完成W15提交顺序及W13所属dispatcher机制修复，清理后基础15／桌面35／Grid73与NET8通过；W14补充失败阶段后随获批30文件提交ef09198b9运行[第六轮CI](https://github.com/AtomUI/AtomUI/actions/runs/40149666278)，三原生／两Browser通过，Windows W14失败、Linux Gallery19高亮失败、Mac取编译器源码遇网络失败并已对同SHA重试；最新处置见[第七轮执行记录](platform-seventh-followups.md) |
| CT-26 | CT-12–CT-23/CT-25；AI交付另核CT-24 | 汇总逐Control/问题处置及视觉兼容矩阵，清理临时验证/废弃接线，同步正式文档与迁移资料 | 全目录有结论，90组每个子项闭合或有范围/兼容决策；无未解释/未经批准视觉差异，最终focused验证和审查完成后再申请提交 ；按用户决定，本轮排除暂缓的CT24 |

CT-21/CT-23使用现有[GalleryBase](../../../../src/AtomUI.Toolkits.GalleryBase)及[CustomizeTheme](../../../../controlgallery/AtomUIGallery/ShowCases/General/CustomizeTheme)；产品品牌及生成服务属于Gallery，稳定中立能力才下沉GalleryBase。CT-22/CT-24不得另外建立ThemeManager、Token字典或Style优先级引擎。

## 4. 审计追踪与新增发现

[映射表](problem-task-map.csv)为每个已有审计ID给出唯一主任务。协作任务描述共享修复、同组其他Control或最终证据义务；不代表主任务做完就能跳过协作方验收。TH/EVT及原始位置继续读原问题矩阵。

目录或运行验证产生的新问题先补审计事实与负责任务，再安排整改；不能只追90组而漏掉未被静态候选命中的控件。兼容保留不等于删除字段，范围外必须说明语义或平台原因。

本轮新增机制事实（不冒充原M0源码清单的已修控件）：

| 发现 | 责任与处置 | 当前证据/状态 |
| --- | --- | --- |
| 直接移除原生Styles后OTP模板separator仍保留已移除专用Style的紫色，旧源／候选相同 | CT-25核查原生Remove仅递归逻辑子树与模板子树清理；CT-08托管lease已有桥接，不能声称修复了外部直接Remove | [颜色结果](input-otp-color-results.md)保留失败；class资格停用后移除的相同轨迹通过 |
| RangeDateViewer Content页头按PanelWidth固定宽度，而日期Content列更宽，标题与面板不齐 | CT-16 D4-C已核定并修复；保留Content／ShowWeek／原宽高触发条件 | 用户批准默认对齐差异；[结果](date-viewer-header-results.md)记录11项功能／输入／生命周期、实际Mac NativeAOT72项、清理后日期337项及NET8通过 |
| D4-A暗色配置恢复24个字形RGB1、同值旧资源启用10个及AOT禁用10个 | CT-25精确视觉出口继续核查，不能以容差或同源类型推定通过 | [D4-A结果](date-viewer-global-results.md)保留全图计数／范围和未闭合结论，功能及最终337项通过 |
| TimelineItem自用Padding未声明测量失效，Own清除后DesiredHeight19而原39、后一项上移20 | CT-15在真实Measure owner补PaddingProperty依赖 | [P2结果](timeline-typography-results.md)：有效反例2FAIL、单变量866→0、修复后四次恢复0；临时清理后Core13/Desktop118与NET8通过 |
| inactive Style.Resources仍进入资源搜索，theme-only字典可能被HasResources遗漏 | CT-08准备期检查完整资源结构 | [CT-07反例](qualification-results.md)、[A实现](style-assets-results.md)已验证，B已接入实际宿主 |
| 原生Remove与Add对逻辑/继承子树覆盖不同，模板frame可残留 | CT-08 lease退役先失活，再清frame及验证释放 | B已实测Card10代及PopupRoot/Overlay旧Source清理；完整品牌协调、性能和原生OS仍待验 |
| 后赋资格root不能自动证明子规则Nesting，独立子selector可绕过资格 | CT-08A重新执行原生Nesting校验并在失败时不标记图 | A内部准备已实现/验证；不使用私有selector反射或找节点改值 |
| 旧OfType根selector不匹配真实Window StyleKey；更近Window复制同Context资产会抬高优先级 | CT-08B独立根资格及实际styling祖先去重 | 两项有效RED后通过，应用trigger11不再被复制资产24覆盖 |
| 候选追加尾部会改变应用Style覆盖；保留图Remove已修改后抛错会丢失当前组 | CT-08B相邻插入、原索引恢复及聚合清理错误 | 有效RED后通过；普通应用对象/顺序保持，原生部分owner错误逐项清理/可重试 |
| 工厂/初次Mount回调重入与空host可绕过阶段；完成句柄保留旧图 | CT-08B先登记pending/ready门槛、显式阶段及完成后释放 | 失败/通过及GC证据见B结果；Manager级重入和对外结果仍属C |
| Token准备提前创建表现属性通知；外层cleanup尚不能报告完整品牌状态 | CT-08B惰性创建；CT-08C统一协调和版本1结果 | 外层失败责任已接管，恢复/退役故障和重试通过；完整品牌公开入口仍属CT-09 |

任务拆分以独立可审查的合同为准：一项默认依赖、一条真实消费链或一个状态/形态合同可以独立交付；不按文件数量或任意固定工期拆分。暂不估算全项目日期，先以CT-00–CT-02确定真实规模。

2026-10-25最新：C8的30对限定差异已获用户接受，全部临时驱动／媒体清理并通过审计；获批第十五批15文件原样提交393d718b3、推送后运行[Actions40188740837](https://github.com/AtomUI/AtomUI/actions/runs/40188740837)。C8本机切片出口完成，三OS／两Browser实际结果仍待本轮；键盘审计及完整CT19／CT25继续，不计作家族完成。

第十五轮最终：获批393d718b3的[Actions40188740837](https://github.com/AtomUI/AtomUI/actions/runs/40188740837)8个job全成功；Windows／Linux／Mac选测各6405／6405、Gallery各412／412，三NativeAOT及两Browser通过，三平台本地化各27组子进程阶段完整配对。该轮不含后续C9及透明棋盘生命周期研发；CT19／CT25完整出口继续。用户已批准[C9色谱键盘短设计](color-spectrum-keyboard.md)，自然焦点和方向反例正在分阶段验证。

CT19追加[C9与棋盘格结果](color-spectrum-keyboard-results.md)：已批准的自然焦点／默认adorner与VS方向修复、两类可见棋盘离树更新已实现。原焦点8项失败与四组有效VS Up反例、两类slider／ColorBlock重挂反例均转GREEN，24项扩展检查通过并删除；清理后原ColorPicker27项、NET8 Release零警告及3文件成员审计通过。正式168帧、56组1DIP全检出；四组默认／四组最终reset为0，独立直接Skia56组A/A／16恢复／16旧新兼容为0，32对状态表一致。[31对自然限定差异](color-spectrum-keyboard-noise-review.md)及棋盘重挂视觉已送单独审查，媒体尚在仓库外等回复。真实Mac managed388；NativeAOT首轮383／388的四事件缺失未证明原因，trace与原二进制复跑各388通过不抹去首轮；滑轨键盘、原生最终像素及完整家族继续，Git本批未授权。

2026-10-25用户明确允许C9棋盘格重挂视觉修复及表内31对限定差异；457张PNG和manifest共458文件已清理，Git媒体审计clean，C9临时源／外部驱动已删除。既有原27项、NET8 Release及成员审计证据保持。用户同时批准[C10单分量滑轨短设计](color-slider-keyboard.md)，按[实施计划](color-slider-keyboard-plan.md)继续；两项授权均不包含Git，C9首轮AOT输入缺失及完整家族／原生最终显示面仍开放。

CT19 [C10单分量键盘结果](color-slider-keyboard-results.md)验证中：具体ColorSlider单一入口复用既有步长／边界及颜色链，不改共享多stop；空值与双stop正常数据分别原8键盘RED／候选26 GREEN，独立原Alpha舍入2项保留。真实Mac managed／NativeAOT各八窗口5688项、1248次目标按键均送达。自然384帧的128组1DIP全部检出、八新旧默认0、216对公共树记录一致；有限72对1–20像素／单通道≤2的合成差异正在独立直接Skia核对，尚未申请扩大任何既有批准。临时C#及媒体待完成判断后清理；C9首轮AOT缺失原因与完整CT19／CT25仍开放，没有新Git授权。

2026-10-25 C10最新：用户已接受[72对精确限定自然差异](color-slider-keyboard-noise-review.md)；[本机切片结果](color-slider-keyboard-results.md)完成。空值／正常双stop数据各26项、Mac managed／NativeAOT各5688项／各1248次目标按键通过；640帧的128组1DIP全检出，直接128AA／32恢复／40兼容0，416对公开树记录一致。两份临时C#、外部驱动及645张PNG／manifest共646文件已清理；最终原ColorPicker27项、NET8 Release零警告、四控件完整成员及媒体审计／文档／作者另步自审通过。当前等待新的具体Git审查，不能沿用第十五批授权；完整CT19／CT25及C9首轮AOT原因仍开放。
