# CT-16 D4-A DateViewer Global 接线结果

本分包按[日期计划](date-time-consumers.md)修改四个基础主题：DateViewerTheme 2处、DateViewerHeaderTheme 2处、DatePanelTheme 1处、DateViewerCellTheme 21处，共26处SharedTokenResource改为OwnerTokenResource。资源名称、所有Own、默认公式／ID／slot、C#和公开API保持；内部Panel／Header／Cell动态读取当前公开DateViewer或RangeDateViewer，公开Button／IconButton保留各自身份。Calendar专用Cell和Lunar内容的投射不在本26处结论中。

有效RED4为14临时用例9FAIL／5PASS（2s）；Shared Global、编译专用Cell Style／AXAML声明形态与两个真实detach GC为正对照。修复后DateViewer／Calendar聚焦240PASS（12s），含14临时和226既有。先前Grid命名空间编译失败、错把Week cell当普通日期及错查Avalonia Border的观察错误不计有效RED。真实普通／选中／禁用、Content表面、周标题、header／footer、radius、motion、清除／兄弟身份／重挂均通过。部件定制只使用生成的DateViewerCellStyle／RangeDateViewerCellStyle编译AXAML；无代码节点样式赋值。

Skia Headless Window为1560×1220、scale1，固定2026-10-15、Range15..18、DisabledDate22，四个Compact／Content公开viewer共288cell，正常motion。17阶段覆盖A/A、1DIP、实际hover、根禁用／恢复、两个身份分别绿／蓝配置、专用Style资格／停用、透明、清除、真实点击20／键盘21与范围两次点击20..21及恢复。旧新完整历史一致：旧版每主题72个预期Global失败，候选两主题0。A/A0，1DIP检错38120明／38078暗且扰动恢复0；选择和业务日期保持。

旧新34张中18个兼容帧严格0，12个配置帧为预期新消费差异；明色禁用3像素与暗色清除／输入／恢复24像素为字形RGB1差异。用原版已支持的公开root Shared枚举资源投射同样10个有效值，重放全部轨迹：明色17张全图0（包括前述3像素），暗色13张全图0；启用10及后三帧24个RGB1字形像素仍待CT25核查。原版自己的暗色禁用往返已有30像素；候选同阶段也是30，旧新该阶段0。不裁剪、遮罩、容差或挑帧消差，**不将未闭合的暗色细节列作精确视觉通过**。

当前Mac NativeAOT实际34阶段全部功能断言通过，与JIT33张全帧0，暗色禁用另有10个RGB1字形像素，Alpha不变；无IL警告，5条Homebrew系统dylib部署目标警告单列。此为当前Mac执行／Skia合成器证据，不代表其他OS或最终原生显示面。颜色、选中、禁用、Content、实际输入和专用Style画面已审查。

一次缩略图误读曾把标题判断为消失；原始标题区域339有效文字像素、125种RGBA与原版相同，文字／布局／6标题均正确。只读链检查及InvalidateVisual／Measure／日期往返的单变量探针未改变画面；没有因此添加任何产品修补。已校验NuGet Avalonia12.1.4与本地tag源码commit均为2d79ba55fae1b1e71c69f93a668080c9029e6a31，未把本地HEAD12.1.3当成该版本依据。

另外观察到旧版RangeDateViewer Content的Header固定PanelWidth，而body实际列更宽，标题与面板不齐。该几何问题归后续D4-C，保留原宽高／ShowWeek／Content条件，不以移除触发条件回避。基础接线、完整日期家族、CT25字形细节与平台出口分开记录；本结果不是完整视觉或家族验收通过。

执行命令为`dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj -c Release --framework net10.0 --filter 'FullyQualifiedName~DateViewer|FullyQualifiedName~Calendar' --no-restore`。外部驱动以`dotnet build Visual.csproj -c Release`及`dotnet AtomUI.Desktop.Controls.Tests.dll before2|after|old-native light|dark <registered-output>`执行；NativeAOT采用`dotnet publish Visual.csproj -c Release -r osx-arm64 --self-contained true -p:PublishAot=true`后运行同参数二进制。源码A/B四文件均由finally逐字节恢复，产品差异已核定仅26处资源引用。

14项临时验证／2文件、429个外部驱动／发布文件（1,369,277,732字节）、两份源字节备份已删除；172个媒体运行文件清理且audit clean。清理后DateViewer／Calendar／DatePicker精确filter337PASS（21s），NET8 Desktop Release含依赖0警告／0错误（18.41s）；新增常驻测试0。作者自审确认产品只改四个AXAML的26处资源入口，既有SharedTokenResourceValue动效输入声明及原生ControlTheme位置保持。CT25的暗色RGB1视觉出口仍未闭合，本轮没有为此修改渲染、放宽阈值或标记完整视觉通过。
