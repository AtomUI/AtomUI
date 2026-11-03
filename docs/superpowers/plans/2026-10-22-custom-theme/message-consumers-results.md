# CT15 Message P1 实际消费者结果（2026-10-31）

范围2 metadata／6 Own，本包为Message工程切片，不关闭完整Message/CT15。沿指定原目录feature/custom-theme单agent执行，无分支/worktree/提交。既有CT11源码纳入旧版回放，CT12/13/14关闭结论和CT25/FW1原始边界、官方Avalonia12.1.4保持。

## 工程及合同

- 六Own迁DependencyDefaultsV2 Revision1，五Active：ContentBg/ContentPadding/MessageIconSize/MessageIconMargin/MessageTopMargin。四算法24默认/type/key/slot逐条相同，整个Desktop生成TokenResourceConst SHA仍为`7ff07970abd14f9c303a692f73387fcee2f246053a88a67cf3cd9baa3d784d57`。
- CardHeight原未计算、默认double0，无生产消费者，按T30/T22保留Reserved资源/type/key/slot/默认/配置往返，不制造固定高度或MinHeight。1/900配置不约束原自适应高度，合法几何0/透明/清除保持。
- Card与manager Global按真实owner读取。IconPresenter为独立公开identity，状态色必须显式MessageCard source；manager背板圆角/阴影归manager，背景仍用Card.ContentBg。沿用TokenTransitions的既有owner时长机制，不替换SharedTokenResourceValue或复做CT11。
- 正文原有FontSize/ColorText角色默认移到Card root，由原SelectableTextBlock继承；原生root FontSize/Foreground进入实际正文，FontFamily复用真实原生继承。LineHeight/选择色仍独立。四配方应用主题后root有效FontSize原12→14，Foreground原Black→明色#e0000000/暗色#d8ffffff；原默认正文/实际高度38或Compact30保持。这是有效值归位，并未改Avalonia属性metadata默认。
- 五种默认图标由既有类型Style内Template逐卡实例化，修复原类型切换仍显示旧glyph；用户Icon优先，null仍恢复当前类型默认。OnPropertyChanged处理显式null变化，OnApplyTemplate另覆盖初始Local null未触发变化事件的路径。不新增默认图标旗标、反射、C#样式绑定或视觉节点。
- 创建于模板外的卡片用BindOwner持有Position/Motion两项类型化订阅，后者以Template优先级提供默认；ReleaseOwner先停motion协调，再释放订阅和回调。关闭/manager Dispose两出口一致。稳定presenter的Position/Stack/Threshold/Motion移至AXAML TemplateBinding，C#只接私有稳定集合及事件，换模板/Dispose解绑旧目标。
- 保留完整文本、五Type、图标、可选择正文、真实键盘/指针、关闭一次、Stack及原计时/布局/内容模型快照合同。没有SizeType，不创造S/M/L/Custom；原生尺寸/Part为定制入口。

## 有效临时验证

初始12项为**6FAIL/6PASS，3.6170s**：动态默认glyph、Card/manager Global、原生正文字号、Native动效清除和关闭后manager投射残留六条真实反例。专用Style、五Own及四算法默认已通过。中间29项28PASS/1FAIL定位到IconPresenter独立identity，补明确Card source后通过。

实际manager路径曾暴露本轮回归：Show初始化Icon=null，Local初值与默认null相同不触发PropertyChanged，遮住迁入主题的默认图标。原版无此缺陷，不归因于驱动；补直接/manager两项候选**2FAIL，3.2053s**，恢复原OnApplyTemplate的null清除语义后完整**51PASS/0FAIL/0Skip，4.1844s**（22临时+29既有）。首轮失效候选236检查4FAIL仅缺图标/类型默认，已实际查看默认帧，未计最终兼容。

覆盖五色/类型、逐卡图标隔离、自定义Icon/null/重模板、五Own/合法0/清除/Reserved、两owner与原生字体、编译专用Card三Part及manager listContent、关键Setter/无节点设置回退、关闭与状态订阅释放。长寿命manager/provider不保留已关闭card/icon两WeakReference。原Message Semantic/manager padding/Message Stack动画检查复用，不重写共享协调器或调度器。

## Skia／NativeAOT与检测器

920×700、scale1，四条原始Information/Success/Warning/Error文本/图标、完整Shadow和Padding，零时长队列；四算法，20主阶段。正常Motion、真实DispatcherFrame泵，至少700ms+12相同帧（上限5s），不关闭动效、删除内容/边界或裁剪/对齐/遮罩，不设容差。物理pointer hover、点击、焦点、Ctrl+A/Meta+A、关闭一次均经实际输入或原公开关闭API。

managed/AOT各**236主检查0失败**；原版相同完整业务历史为**236检查72FAIL**。独立检错实例各4结构检查0失败，A/A四配方0，1DIP变化10604/9689/10549/9665像素（max224/224/212/212）；检错恢复2/38/33/25（max1/29/12/23）仍FAIL。最初把检错放在主实例的88帧因此无效，原42/88零及非零保留为失效候选记录；随后将人工检错移到独立进程/实例，主实例保留同样完整Own/Global/Native/Part/Type/RTL/Stack/选择/关闭业务往返，重采80主帧。没有靠复位业务步骤消差。

新主基线旧新**35/80零**，包括四默认、四A/A、四Part、四RTL、四折叠、四关闭；其余45非零不自动放行。JIT/AOT **41/80零，39非零**，额外差异全部保留。表格式`changedPixels/maxChannelDelta`：

### 旧新80对

| 阶段 | Default | Compact | Dark | Dark+Compact |
| --- | --- | --- | --- | --- |
| default | 0/0 | 0/0 | 0/0 | 0/0 |
| aa | 0/0 | 0/0 | 0/0 | 0/0 |
| own | 3/11 | 4/11 | 1/11 | 13/21 |
| own-restore | 0/0 | 0/0 | 2/12 | 4/1 |
| global | 38342/255 | 34740/255 | 42199/255 | 37079/255 |
| global-restore | 14/29 | 22/15 | 24/33 | 42/33 |
| native | 32804/255 | 32425/255 | 32587/255 | 32208/255 |
| native-restore | 2/15 | 6/15 | 2/12 | 8/1 |
| parts | 0/0 | 0/0 | 0/0 | 0/0 |
| parts-restore | 2/1 | 10/44 | 14/33 | 25/35 |
| type-change | 50/178 | 58/178 | 62/189 | 73/189 |
| type-restore | 2/1 | 10/44 | 14/33 | 25/35 |
| rtl | 0/0 | 0/0 | 0/0 | 0/0 |
| rtl-restore | 0/0 | 0/0 | 0/0 | 0/0 |
| collapsed | 0/0 | 0/0 | 0/0 | 0/0 |
| hover-expanded | 11/11 | 11/23 | 0/0 | 4/7 |
| hover-collapse | 0/0 | 0/0 | 0/0 | 0/0 |
| stack-restore | 6/11 | 1/1 | 12/39 | 7/21 |
| selected | 6/11 | 1/1 | 12/39 | 7/21 |
| close | 0/0 | 0/0 | 0/0 | 0/0 |

### JIT/AOT80对

| 阶段 | Default | Compact | Dark | Dark+Compact |
| --- | --- | --- | --- | --- |
| default | 22/1 | 0/0 | 0/0 | 0/0 |
| aa | 22/1 | 0/0 | 0/0 | 0/0 |
| own | 3/11 | 4/11 | 0/0 | 10/21 |
| own-restore | 0/0 | 0/0 | 2/12 | 0/0 |
| global | 0/0 | 0/0 | 0/0 | 0/0 |
| global-restore | 1/1 | 9/15 | 11/26 | 6/33 |
| native | 0/0 | 3/11 | 2/12 | 7/23 |
| native-restore | 0/0 | 2/15 | 0/0 | 2/12 |
| parts | 0/0 | 0/0 | 0/0 | 0/0 |
| parts-restore | 2/1 | 5/15 | 14/33 | 18/23 |
| type-change | 2/1 | 5/15 | 14/33 | 18/23 |
| type-restore | 2/1 | 5/15 | 14/33 | 18/23 |
| rtl | 0/0 | 0/0 | 0/0 | 0/0 |
| rtl-restore | 0/0 | 0/0 | 0/0 | 0/0 |
| collapsed | 0/0 | 0/0 | 0/0 | 0/0 |
| hover-expanded | 6/11 | 11/11 | 7/13 | 13/24 |
| hover-collapse | 0/0 | 0/0 | 0/0 | 0/0 |
| stack-restore | 5/11 | 11/11 | 31/28 | 9/21 |
| selected | 5/11 | 11/11 | 31/28 | 9/21 |
| close | 0/0 | 0/0 | 0/0 | 0/0 |

### 同候选主业务恢复到最初默认

| 阶段 | Default | Compact | Dark | Dark+Compact |
| --- | --- | --- | --- | --- |
| aa | 0/0 | 0/0 | 0/0 | 0/0 |
| own-restore | 27/29 | 36/15 | 26/23 | 31/12 |
| global-restore | 31/29 | 42/15 | 33/33 | 53/33 |
| native-restore | 29/29 | 39/15 | 26/23 | 32/12 |
| parts-restore | 36/29 | 37/29 | 42/33 | 44/23 |
| type-restore | 36/29 | 37/29 | 42/33 | 44/23 |
| rtl-restore | 2/1 | 38/29 | 33/12 | 25/23 |
| stack-restore | 16/11 | 51/29 | 43/12 | 42/23 |

正常Loading另用新实例，各配方24实际帧：managed distinct24/24/24/23，AOT24/24/24/24，各12检查0失败，正文和Type保持。未把不同相位的Spin做逐帧零差异声明。已实际打开原缺glyph帧、修复默认、专用Style、Dark Global、Native、Loading第12/18帧及放大差异：原内容/交互面仍在，原生Navy/Arial19、Menlo23状态色和专用Style均落于实际目标，Spin持续；微差主要可见于极细glyph边缘，原因未证实，不以“噪声”批准。

外部探针先有目录别名/当前KeyPress签名/partial startup类编译错误，不计产品RED。首次NativeAOT因探针未导入Homebrew搜索路径而缺ssl；复用仓库MacOSHomebrewNativeAot.targets后发布/实际运行成功，无IL警告，保留五条本机OpenSSL/Brotli部署版本警告。未替换官方依赖。全部构建/采样先非阻塞独占`.artifacts/verification/run.lock`，占用即拒绝；一次因污染检错历史终止本包自有发布子进程前已验证12源全部恢复。

## 复放命令与作者另步自审

```bash
CT15_MESSAGE_METADATA_DIR=<external-directory> dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~Temporary.CT15Message|FullyQualifiedName~MessageSemanticPartTests|FullyQualifiedName~FeedbackManagerPaddingTests|FullyQualifiedName~MessageStackAnimationTests'
dotnet build <external-driver>/CT15MessageProbe.csproj -c Release
dotnet <external-driver>/bin/Release/net10.0/CT15MessageProbe.dll <external-evidence>/candidate
dotnet <external-driver>/bin/Release/net10.0/CT15MessageProbe.dll <external-evidence>/candidate-calibration calibration
dotnet publish <external-driver>/CT15MessageProbe.csproj -c Release -r osx-arm64 --self-contained -p:PublishAot=true -o <external-driver>/publish
<external-driver>/publish/CT15MessageProbe <external-evidence>/aot
<external-driver>/publish/CT15MessageProbe <external-evidence>/loading-aot loading
```

旧版只回放本目录12源，持锁并在finally恢复；12/12候选文本精确核对。成员初始27问题，纯排版token快照相同；最终补接口后13私有方法移动，10文件/2Control/0问题，移动前后逐成员/类型header/import tokens完全相同，注册/字段初始化次序不变。渲染/AOT证据对应采样快照；最终文本仅上述私有方法位置变化，清理后模块/NET8按最终文本重新编译，未把它说成另一次最终文本AOT采样。

作者另步复读五产品文件完整增量、五正式文档及API/默认/owner/优先级、类型glyph、null初始化、动态状态归属和释放、无反射/AOT、原输入/布局/内容合同。遵守禁用子agent要求，仅作者自审，非独立评审；无未解决Critical/Important代码问题，未通过像素不作为minor豁免。

Ruling：CardHeight无实际目标，保留Reserved优于凭名字造高度。Ruling：正文默认角色归root可保留实际默认并让Native值生效，root有效值变化独立审查；不把manager字号复制到Card角色。Ruling：默认glyph使用既有Style的Template，不新增C#旗标；保留原null重置且已补候选回归反例。Ruling：人工检错与主业务实例分开，保留所有原始FAIL，不替换依赖/调容差。

## 限定出口与清理

仅[8对Global/原生正文及root有效值归位](message-consumers-visual-review.md)申请独立审查。其余37旧新非零含4对类型glyph有意修复与33微差，整帧仍未通过；39 JIT/AOT、检错/业务恢复非零均保留。NativeAOT Skia不等于NSWindow最终显示或其他Host/平台；Stack原16/8/背板通用几何、真实host作用域/六位置、任意极值不裁剪及完整Message仍需P2。Timeline/Alert/Card未通过及CT25/FW1边界不变，不关闭完整CT15。

22项临时验证所在3源文件/14891 bytes、434文件外部工程/1068797113 bytes及1163取证文件已删除；evidence cleanup审计clean，永久测试净增0，仓内查找无CT15Message/MessagePartsView专用残留。全部本包采样进程退出。

清理后按最终文本重编译并精确运行：**29PASS/0FAIL/0Skip，3.8150s**；Desktop NET8 Release **0 warning/0 error，17.77s**。最终成员10文件/2控件/0问题，方法纯移动前后tokens完全相同。正式五文档及本包计划/结果/限定表链接有效，git diff --check通过。未自行跑全量/affected-wide回归。

```bash
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~MessageSemanticPartTests|FullyQualifiedName~FeedbackManagerPaddingTests|FullyQualifiedName~MessageStackAnimationTests'
dotnet build src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj -c Release -f net8.0 --no-restore
python3 <atomui-control-optimization>/scripts/check-member-layout.py --path src/AtomUI.Desktop.Controls/Message
git diff --check
```

最终产品源SHA256：


- `src/AtomUI.Desktop.Controls/Message/MessageCardToken.cs`：`d851281c7783936e0f7c0e2250065f9b4b6f41721fcedc3cccb22f1b386a0fce`
- `src/AtomUI.Desktop.Controls/Message/WindowMessageManager.cs`：`9eb3d7e35ff993a3ee5389a074a3bc8f2426dd72963ef6a208a423089dde1439`
- `src/AtomUI.Desktop.Controls/Message/MessageCard.cs`：`4e1ef68ff6563f3aa18969e0d625cf76c891e1a997484fb8d7c3cbde75e40895`
- `src/AtomUI.Desktop.Controls/Message/Themes/MessageCardTheme.axaml`：`2cad088c0cfb6eccdd3d1b744c90a85b62b843909676cc3470b6f4317f56dcb9`
- `src/AtomUI.Desktop.Controls/Message/Themes/WindowMessageManagerTheme.axaml`：`4c5ce9532aaeb9e7ab48652b6dc339a1e1d472196bd961a37a8f8d8cbcbd364c`


## 2026-10-31 限定批准登记

2026-10-31，用户明确回复“批准表内8对及root两项有效值归位，其余未通过与完整CT15保留”（request_user_input_async / call_8fffdadfe1c0457483143eea833656e6 / item 0）。仅本表8对和列明两项有效值/属性默认纠正获限定批准；其他原始非零、P2及完整家族/CT15保留，不授权Git提交。


2026-11-01 P2原生宿主补证：native NSWindow960×720/scale2，managed/AOT完整各147及固定激活各148检查，六位置/变高gap16、原NoWrap全文/显式换行、Cocoa click/Space、五卡片hover/gap/leave、1500ms真实悬停剩余计时、四算法RTL通过。应用内合成Cocoa经Avalonia.Native，不冒充外部物理/其他OS输入。完整OS阴影画布随激活变化，跨运行像素不通过；正常入场初Frame与稳定绘制3497/max2差异定位，初始frame重绘触发、Window重绘无效，纯Border/淡入不复现，不归FW1或扩大容差。248取证/447原会话(411driver)/两AOT已清理，audit clean；清理后精确feedback padding/panel/motion与GroupBox70PASS/0Skip、1s，NET8零警告零错误21.10s。见feedback-native-p2.md；无反馈运行代码改变，GroupBox仅三Reserved XML说明同步。原失败、完整P2/CT15及CT25/FW1保留，不提交。


2026-11-01 Message恢复定位：四配方×entry/shift/Own/Global的16独立场景，实际值/几何/四card身份恢复、AA0；managed/AOT各224检查220PASS/4严格零恢复FAIL，76帧66cross零/10非零，16完整背景重绘cross全0且每配方四条历史的重绘图彼此0。原正常初始/局部恢复仍非零，不把重绘当修复；新Message路径尚不在已批准FW2范围，原P1与native3497/max2均保留。4临时case/1源9518字节、331取证、427会话1068266516字节已清理，audit clean；最终精确Message Semantic/managerPadding17PASS/0Skip/1s，产品未变复用NET8零警告21.10s。见message-restore-audit.md，完整Message/CT15未关闭，不提交。


清理结果：当前10临时case/2源14466字节已删除；官方evidence cleanup clean/findings=[]、removed=true/550文件。外部111会话文件569551767字节（含独立工程/原失败与坏模型日志）及marker删除。清理后实际重建MessageSemanticPartTests13PASS/0Skip/3.7454s；首namespace误写Feedback而Padding实际为FeedbackStack，未把未匹配项计入，随后同一重建程序集精确Padding4PASS/0Skip/3.1545s，合计17不同既有用例。未改产品源码，复用相同最终源码NET8零警告零错误16.76s。最终helper/marker随读取清理，无新常驻测试、无全量或提交。

作者另步自审：纯模型默认图0只用于确认图元等价，独立裁剪错误已纠正并保留反证；不将未复现产品历史的模型解释为框架修复。原真实恢复仍FAIL，当前无新视觉批准或FW2归属请求，完整Message/CT15保持开放。


最终清理后精确MessageSemanticPart13+FeedbackManagerPadding4共17PASS/0Skip/4.5267s，实际重建；本轮无产品源码变化，复用相同最终源码NET8零警告零错误17.41s。1191取证与795会话2176410897字节已清理audit clean，最后helper/marker记录后删除；没有新增永久测试、全量或Git操作。作者另步自审，非独立审查；原动效/内容/滚动/选择/关闭合同未改，独立模型与真实主批限制明确分账。仅34条有限调查路径提案待用户裁决，完整CT15及其他排除项保持开放。


## Message 原历史34路径限定调查批准登记

用户明确回复“仅批准表列34路径归CT25/FW2调查，其余保留”（request_user_input_async / call_8107eb29ed024cfbb7dd7fd40dec2aed / item 0）。仅message-original-visual-review.md表列28当前业务恢复、4独立检错路径及2无AtomUI队列重排反例交CT25/FW2继续调查。模型未精确复现原全部像素的限制保持，不宣称完整框架根因；非零仍FAIL、不增容差、不关产品动效、不改FW1。原native3497、其他36跨运行非零、旧失败批、其他家族与完整CT15均保留，不授权提交。
