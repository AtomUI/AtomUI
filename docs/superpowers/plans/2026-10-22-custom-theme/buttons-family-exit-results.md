# CT12 按钮家族总出口结果

最终状态：2026-10-30，用户已明确回复“批准本表限定范围，保留 CT25”（request_user_input_async / call_a2fe796a8df0481aadd7e4393060b87e / item 0）。公开BoxShadow工程、默认原生验证、清理后189项及限定视觉批准已全部完成，CT12在既定批准范围完成并关闭。原始24/max1、两对3968/max2及其他既定历史仍按CT25保留，不设通用容差，不扩大其他平台/背景/恢复模式，无Git授权。 旧阶段V11待选/待采样/待批准内容仅为历史，不作为当前剩余任务。完整收口见[最后结果](buttons-box-shadow-finish-results.md)。


2026-10-30最新：V11公开BoxShadow已实现并通过当前默认原生managed/AOT1236各、清理后189及NET8零警告错误；不再等待合同选择或采样。只剩[最后限定视觉表](buttons-box-shadow-final-visual-review.md)核定，CT25原始失败独立保留；下文早期阶段记录不作为当前未完清单。


2026-10-29，`feature/custom-theme`，HEAD `4560091deea4e56c14e89a70e2f418e72a8afd64`。本轮承接既有分包，未新增产品实现修改、API、主题身份或 Own 字段；仅核对消费出口并修正文档中的三处陈旧表述。原 CT11 工作树输入的 100 路径已记录 SHA256，产品内容保持。

**状态：已完成（既定批准范围；CT25保留）。** 已导出字段/业务及当前未修改产品的原生无输入矩阵通过。原始问题表复核发现V11预设色阴影尚缺稳定几何定制入口，128字段消费矩阵未覆盖该未导出入口。须先完成V11，再核定[混合宿主两对阴影细线](buttons-native-exit-review.md)，不能仅批准两对就关闭CT12。不提交、推送、发布或创建分支/worktree；按用户约束由主agent执行和另步自审。

## 已完成分包的合并依据

- [V2 默认与依赖](buttons-defaults-results.md)、[Option19](option-buttons-results.md)：五终端的默认求值、字体/行高/留白及字重存在位合同。
- [Global 与独立 Own 消费](button-consumers-results.md)、[字体族](buttons-font-family-results.md)、[其余消费](buttons-remaining-consumers-results.md)：独立身份、原生优先级、继承/清除和实际 Flyout Target。
- [持续动效](buttons-continuous-motion-results.md)、[Hyper loading](buttons-hyper-loading-results.md)、[Hyper 状态](buttons-hyper-states-results.md)、[Hyper root](buttons-hyper-root-paint-results.md)：已批准切片不重复实施，不扩大批准范围。
- [CT11 总出口](motion-exit-results.md)关闭共享 motion 合同；其限定视觉批准不作为本轮新差异的通用容差。

## 133 字段与实际消费

| Own 终端 | 有效字段 | 历史兼容 | 本轮实际断言分组 |
| --- | ---: | ---: | --- |
| Button | 45 | 5 | 22 颜色、19 排版/间距、3 阴影、1 实际展开 Flyout 间距 |
| DropdownButton | 46 | 0 | 21 颜色、21 排版/间距、3 阴影、1 实际展开 Flyout 间距 |
| HyperLinkButton | 17 | 0 | 16 排版/间距、1 禁用描边 |
| OptionButton | 19 | 0 | 9 颜色、ButtonPadding、三档字号/比例行高/留白各 3 |
| SplitButton | 1 | 0 | 实际展开 Flyout 的 GutterToFlyout |
| 合计 | 128 | 5 | 128 个独立实际字段检查通过 |

43 条 Button/Dropdown 颜色路径按独立字面量映射到 root Foreground、Background、BorderBrush，使用真实 mouse move/down 进入 hover/pressed，禁用通过原生 IsEnabled。覆盖 Default、Ghost、Text、Link、Solid、Primary、Danger 和 disabled。两次不同显式颜色、透明黑 `#00000000`、清除恢复及另一 identity 的兄弟隔离均通过。颜色不是只从 AXAML 搜索得出：`ButtonThemeTokenReader` 与变体计算先写入 Variant* 属性，再由 ControlTheme 消费。

56 条排版/间距路径覆盖 Button19、Dropdown21、Hyper16：三档 ContentFontSize、ContentPadding、ContentLineHeight、IconOnyPadding、OnlyIconSize；三类 IconMargin、Button/Dropdown 的 IconEndMargin/CirclePadding、Dropdown 三档 ExtraContentMargin，以及 Button 可选 FontWeight。实际读取根字号/留白/图标宽度、PART_ButtonIcon Margin、下拉指示器 Margin；显式值和同实例清除恢复均通过。OnlyIconSize 的 Button/Dropdown 合同限定 icon-only loading，普通用户 icon 沿既有 Global IconSize；Hyper 的 icon-only 合同单列，不擅自统一。

26 条其余路径包括实际 `ShadowsFrame.BoxShadow` 的六字段、Hyper Frame 禁用描边、Option19。阴影断言单项数量、Y=7、Blur=11 和字面颜色；Option 在真实 Group 内进入 checked/hover/pressed/disabled，ButtonPadding 与比例行高通过实际 ContentPadding 依赖验证。几何浮点断言的 `1e-6` 只用于数值计算，不用于像素比较。

三个 Flyout 字段在实际 `ShowAt`、IsOpen=true 后验证 Own27→0→clear 对 MarginToAnchor 的更新。Button 使用实际 Target 的显式 Own，Dropdown 和 Split 保持各自 relay 与默认值；没有把三者合并为同一个 Own 合同。

五个历史字段仍是 Button 的 Reserved ExtraContentItemSpacing/ExtraContentMargin/LG/SM 及 Legacy GroupBorderColor。保留 API、资源 key/ID 和 V2 默认，不产生新视觉区域，不重新归为有效编辑入口。

## 九个公开 owner 的业务与生命周期

额外 28 项通过，和 128 字段合计 **156 PASS、0 Skip**，一次完整临时 filter 总时长 7.0708 秒。

- Button、Hyper、Icon、Toggle、Simple：实际鼠标点击、Space 键、焦点、disabled 点击不增加计数；内容替换后正常点击，Toggle 选择状态正确。Simple 显式透明背景仅为本例的原生完整命中区域，不新增 AtomUI 身份。
- Button、Dropdown、Hyper：同实例 size/shape/variant/danger/loading 往返，字号、Padding、Bounds 和恢复后的 enabled 保持。Loading 的既有合同是视觉状态，不能错误断言它改写根 IsEnabled。
- Button、Dropdown、Hyper、Split 实际子 Button、Option 实际容器：scope 字号17→23、原生条件 Style29、本地31、scope19、清除本地/Style、重模板、detach/reattach、恢复17。Split 标准档和 Option 的字号消费者是子 Button/Option，不能把组合根继承值当作相同消费目标。
- Dropdown/Split：真实指针展开、关闭、换 Flyout 后仅新对象打开；Split 主按钮 Click 恰好一次。
- OptionGroup：ItemsSource 生成容器，实际点击改 SelectedIndex 与 checked；替换数据后内容正确；方向、尺寸和 Outline/Solid 热更新；清理后同一 Option 改挂另一 Group，不遗留旧 owner 状态。
- Icon/Toggle：替换 Icon 后实际子树只保留新内容；内容 Icon 的本地 FillBrush 优先，清除后恢复 owner 画刷。
- 九个 owner：provider 与窗口仍存活，移除子树后经 GC 全部回收。Option 始终放在合法 Group 内，不用裸 Option 绕过父合同。
- 编译 AXAML 中的 ButtonContentStyle/ButtonIconStyle、SplitPrimary/Secondary/ItemStyle、DropdownItemStyle 实际命中；同一 MenuFlyout/Item 从 Split 换到 Dropdown 后改用新 owner 的专用 Style。code-behind 只加载 AXAML，观测代码未直接定制目标。

## 命令、失败处置与验证性质

```bash
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~Temporary.CustomThemeButtonsExit' --logger 'console;verbosity=normal'
```

初始颜色夹具把透明黑误写成 Avalonia 透明白常量；API 适配和业务矩阵也先后修正 PhysicalKey、nullable bool、Flyout 类型、正确产品 Window、StyleTrigger 优先级、组合子消费者和合法 Option 父节点。原始输出均保留在本地文本日志；这些编译或断言错误不算产品 RED。本轮没有确认新的产品缺陷，因此没有以无效失败推动实现修改。

原生临时工程最初的 /var 与 /private/var 规范化、命名空间/生成启动宿主等构建问题也只改验证驱动。JSON 使用 source-generated context；没有动态反射绑定或 AOT 警告抑制。

## 当前 Mac 最后产物原生显示

最终程序已提前构建，用户提供静止时间后立即顺序运行，无夹入构建。全程61.59秒，managed31.35秒、AOT30.24秒；两次均实际NSWindow、RenderScaling2、ClientSize880×700 DIP、1760×1400完整窗口。PID/标题定位ScreenCaptureKit窗口，托管copy-local的Desktop DLL SHA256与当前产物一致。当前官方Avalonia12.1.4、macOS/arm64及NET10；AOT仅保留本机OpenSSL/Brotli最低OS26与目标12的五条既有链接警告，无新增抑制。

40个公开owner实例覆盖九种owner：18个Button/Dropdown、4个Hyper、3个Split、3个Group和9个Option、Icon/Toggle/Simple各1；组合内部子Button另外参与实际模板。四档及三形状、六变体、禁用、图标、选择与组合均可见。正常motion保持开启，不加载持续spin；每阶段先等待1.1秒，超过本场景既有transition时长，再取真实帧。每种主题初始A取3帧，其余阶段各2帧、帧间至少200ms，共每运行26帧、两运行52帧。没有重建主实例、缩放、裁剪、对齐、遮罩或通用像素容差；最终轨迹未触发临时full-damage分支。

两个运行各12阶段、482项断言通过。原生pointer事件0、按下0，12个阶段输入签名均`None|active:True`；窗口激活状态始终相同、实例引用和Toggle选择保持。此为实际无输入轨迹，不依赖关闭控件输入或绕过hover。

| 比较 | Light像素 | Dark像素 | managed/AOT结论 |
| --- | ---: | ---: | --- |
| 每阶段后续帧相对首帧 | 全部0 | 全部0 | 两运行均同 |
| A/A | 0 | 0 | 两运行均同 |
| 1DIP检错 | 77895 | 83124 | 两运行均同，检错有效 |
| 1DIP恢复 | 0 | 0 | 两运行均同，主基线未受扰动污染 |
| 自定义配置B | 381239 | 393559 | 两运行均同，实际Own字号21及配置进入画面 |
| 返回A | 0 | 0 | 两运行均同，同实例恢复 |

另外直接比较managed与AOT的12个同阶段完整画面，全部0。已查看Light默认、Dark自定义与AOT Dark恢复整窗图：字体/颜色/密度和圆角实际变化并恢复，布局可见、组合分隔线保持。结论只覆盖本次当前Mac、有限切换与采样帧；不是所有原生动画帧或35次高频切换的无闪动保证。

### 保留的原始失败

早期局部暗色配白色外宿主的完整轨迹，1DIP恢复及B返回各3968像素、最大通道差2，范围(43,183)–(949,652)。自然路径144行实际绘制属性与几何完全相同，只有初始一次pointer事件、0按下；已查看127倍完整差异图，集中在上边缘阴影细线。[限定审查](buttons-native-exit-review.md)保留两对原始FAIL，不能以统一暗色宿主下0或CT11批准自动豁免。

还保存Light AA一张中间帧13529像素/通道差17、早期AOT全局转暗初态238079像素/通道差199的历史失败。最终受控完整轨迹0不证明这两张旧采样已被修复；它们继续在CT25作全局切换/原生时序和采样就绪调查，不属于两对细线的批准范围。含真实输入或后台捕获超时的尝试也不计静止通过。原始文本数据保持，媒体审查后按规范清理。

## 清理、最终验证与交付

8个临时测试/AXAML文件共46595字节已删除，净新增常驻测试0。现有长期行为、主题、语义Style与焦点渲染合同继续保留。本轮没有产品C#修改，成员布局变更审计不适用；CT11既有成员审计保持。正式文档仅修正Button历史字段用途、Split独立Token导航和图标按钮StateMotionSession名称，没有生成无产品变更的changelog或Gallery示例。

清理后精确模块验证：Desktop162PASS（7.4814秒）、Gallery16PASS（9.5387秒）、焦点渲染1PASS（2.2391秒），合计179、0Skip。Release NET8构建42.88秒，0警告0错误。最初误用Debug/net8的两次命令按项目配置规则失败，已改为正式Release/net8配置；没有改项目目标框架来绕过。

```bash
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Buttons.|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.DropdownButton.|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.RadioButton.OptionButtonGroup|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Theme.ButtonFamilyTokenIsolationTests|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Theme.ButtonThemeCrossPlatformAssetTests'
dotnet test tests/AtomUIGallery.Tests/AtomUIGallery.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~ButtonShowCasePageTests|FullyQualifiedName~SplitButtonSemanticPartHighlightTests|FullyQualifiedName~DropdownButtonSemanticPartHighlightTests|FullyQualifiedName~RadioButtonShowCasePageTests'
dotnet test tests/AtomUI.Desktop.Controls.Rendering.Tests/AtomUI.Desktop.Controls.Rendering.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~ButtonFocusVisualRenderingTests'
dotnet build src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj -c Release -f net8.0 --disable-build-servers -m:1
```

全局测试价值静态audit通过；未运行全量/affected-wide/change回归。179项测试之后仅修改文档与审计CSV，不改变被测产品。76个既有产品改动路径与开工hash相同，无新增产品diff路径；CT11内容保持。

前一取证目录510文件、87825502字节已在文字审查后清理，audit clean；最终52帧目录53文件、9146949字节亦已清理，audit clean，不链接已删除媒体。被遮挡后台窗口的ScreenCaptureKit采样超时，已终止本任务的两个Capture进程，不冒充后台显示通过。所有本轮Native窗口已退出。

首次90秒时间窗被约52秒构建消耗，采样未能在约定时间内完成；这是验证调度问题，不归责于用户或计作产品RED。用户再次提供约60秒静止时间后，两个已构建程序顺序完成最终采样。专用系统临时工程632文件、1103889170字节（含托管/AOT产物和驱动）已全部删除，相关进程0。所有媒体共563文件、96972451字节已清理；没有永久保存截图、视频或其编码副本。

## 明确保留的边界

- SplitButton Custom 固定 Width140 的既有溢出仍归 CT20。
- 官方 Avalonia12.1.4 的长寿命且仍赋给旧 Button 的 Flyout 基类 Opened/Closed 关系，以及旧 Button/Option、Flyout 移动恢复的有限 RGB1 差异仍归 CT25；没有把“新订阅释放”误报为框架对象关系修复。
- 当前 Mac managed/AOT 和原生采样不代表 Windows/Linux/Browser 的最终显示与原生输入。其他 Host、完整热切换性能及已保留专项仍由 CT25 逐项完成。
- 新原生差异仍需定位和明确处置，[两对细线限定审查](buttons-native-exit-review.md)尚未批准，既有批准不能覆盖它。CT11 继续保持完成，本报告不回滚或重复其结论。

最终作者另步自审：逐字段计数128+5、九owner及14元数据行、原生输入/同历史/零容差边界、长期测试去留与清理记录已核对；76个产品源路径与开工hash相同，无新增产品改动。复核原始问题表另发现V11尚未完成，撤回“仅剩两对”的结论。按单agent约束没有委派独立评审；原始全局切换调查仍属CT25，未称已修复。

## 原始问题表复核补充：V11

4项实际消费探针通过（Button/Dropdown各Red、Purple，3.7225秒）：Global ControlOutlineWidth7→11进入实际ShadowsFrame.OffsetY，ColorBgContainer改变相应Alpha颜色；但PrimaryShadow配置为0 19 23 5时仍保持Blur3/Spread0。未配置默认和清除恢复保持。这只证明现有边界，不是统一阴影接口已存在的证据，也不是新接口的产品RED。

V11要求默认/主/危险/预设色阴影均有适当输入。三类Own已消费，预设色几何却固定且无稳定公开BoxShadow/Part入口；不能把字段计数当作解决该要求。候选为新增Button公开BoxShadow属性（Dropdown继承，ControlTheme默认取VariantShadow，133Own及现有Part边界保持），或新增PresetShadow Own（两终端各1，133→135，保持旧Token含义）。新增公开合同当前等待用户选择，尚未实施。

此前原生52帧与179项是本轮尚无产品实现改动时的证据。V11落地后须按实际改动重做必要门禁，不能沿用为修改后证据。

V11现状临时源已删除；清理后精确ButtonBehaviorTests48PASS（3.9796秒），不将4项现状探针或旧原生矩阵当作V11实现完成。
