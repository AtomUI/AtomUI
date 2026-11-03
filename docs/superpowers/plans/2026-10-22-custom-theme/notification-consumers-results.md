# CT15 Notification 实际消费者结果（2026-10-31）

本包3 metadata／17 Own，沿feature/custom-theme指定原目录单agent，无分支/worktree/提交。保留既有CT11/用户源码，CT12/13/14关闭结论和CT25/FW1、官方Avalonia12.1.4边界不变。本文为当前工程切片，完整Notification/CT15不关闭。

## 工程与有效合同

- 17 Own全部实际Active，迁DependencyDefaultsV2 Revision1，无Own依赖边。保留原公式/常量、两stop immutable线性渐变与底槽；四算法68默认/type/key/slot相同，完整Desktop生成key源SHA仍`7ff07970abd14f9c303a692f73387fcee2f246053a88a67cf3cd9baa3d784d57`。
- Card/manager Global按真实owner；独立公开IconPresenter/close IconButton的通知角色输入显式Card source。正文FontSize/ColorText角色归Card root，Native字号/前景进入真实字符串正文；标题保留AtomUI SelectableTextBlock的FontSizeLG/ColorTextHeading角色，字符串正文仍Avalonia SelectableTextBlock，用户Control内容原样保留。FontFamily复用Core有限真实原生继承。主题应用后root有效字号12→14、Foreground Black→明色#e0000000/暗色#d8ffffff，原实际默认保持，非metadata默认重注册。
- 有限进度总时长归Card，remaining仍经Expiration投射；换模板/进度隐藏/重挂仅释放视觉part，不把remaining当新总量。Expiration=null结束该有限周期，下一有限值取得新总量。detach移除part并清TemplatedParent，保留仍有效layout；Template变更清layout，attach按当前条件重建，仍跨双列有限宽贴底，无永久占位。
- 已挂载卡片动态IsShowProgress变化通知manager现有scheduler.Refresh调整需求；不创建逐项timer、不在Show初始化期间刷新/触发额外回调。仍由共享scheduler按deadline/可见进度与Stack事实调度，不改调度器算法或时长快照。
- Card.BindOwner持有运行期Position/Motion两项类型化订阅，后者Template优先级保留Native覆盖；ReleaseOwner先退役motion/快照，再Dispose并清manager/回调。稳定presenter状态迁AXAML TemplateBinding；C#只处理私有稳定集合/事件。既有默认图标数据工厂、关闭/hover/Stack/motion/snapshot机制和矩阵数值容差保持。
- 保留默认384宽、实际关闭尺寸、完整标题/描述/Actions、可选择内容、关闭/输入/裁剪合同；没有SizeType，不创造三档/Custom，Native/Part增量定制有效。

## 临时验证与自审

初始14中9FAIL/5PASS包含类型夹具错误：标题为AtomUI而字符串正文为Avalonia SelectableTextBlock。纠正后14为**8有效FAIL/6PASS，3.4098s**，旧close假设与17 Own已通过，不为假设修改关闭事件。动态启用进度新增反例后15为**9FAIL/6PASS，3.9663s**。

反例覆盖三种进度生命周期、动态启用后remaining停止刷新、manager状态未释放、Native动效清除及两owner Global/原生字体。修复首轮44中43PASS/1FAIL为close子Control画刷正在过渡，正常Dispatcher循环350ms后44PASS。最终**55PASS/0FAIL/0Skip，5.6626s**（22临时+29原Notification+4 manager padding）。

专用八Part与manager list编译AXAML、关键Setter值/真实目标、无Name/Loaded/Unloaded或code-behind定制回退；close左右中及Enter实际输入、一次关闭；长寿命manager/provider下已关闭card/progress/icon三WeakReference全部释放。17字段正值/合法0/清除、两位置边缘默认及真实progress、Default无图标/四状态/自定义图标、原快照与motion测试复用。

13文件/3Control初始35成员问题，纯移动后0，逐成员token快照完全相同，注册/字段初始化顺序未变。最终13/3/0在采样前完成，功能增量无后置源码差异；旧版只回放本目录16源，持锁finally恢复，16/16候选文本精确核对。

作者另步复读六产品文件完整增量、五正式文档与Owner/Part/默认、有限总量/remaining归属、attach/template/close/Dispose、实际输入/GC和无反射/AOT路径。单agent作者自审，非独立评审；无搁置Critical/Important或代码minor，像素未通过不以minor豁免。

Ruling：初始总时长属于有限显示生命周期，视觉part退役不得重置；否则换模板会让40%回100%。Ruling：复用scheduler.Refresh处理已挂载的需求变化，避免Show初始化中的回调重入和新timer。Ruling：保留有效的非binding默认图标数据工厂，不重复Message实现。Ruling：标题与正文两种真实SelectableTextBlock分别验证，不能改类型规避合同；Native角色归root的有效值独立审查。

## 真实Skia与当前Mac NativeAOT

1280×900、scale1，3×2六卡，默认384宽、原MaxHeight380约束保留；五Type及一自定义Icon/Control内容，五条完整可换行描述、六个Action与关闭面、正常Motion。19主阶段/四算法，before与candidate操作历史完全相同；至少700ms+12相同帧，上限5s。独立检错实例不污染业务基线，不改动内容、约束、指针/焦点路径，不裁剪/遮罩/对齐或调容差。

managed/AOT各**432主检查0失败**，旧版同历史**432检查116FAIL**。独立A/A四配方全0，1DIP检测95641/95392/95909/95654像素（max233/233/219/219），恢复四配方全0。所有五个业务默认恢复阶段四配方共20对精确0。旧新**38/76零**，另36对有意、2额外；JIT/AOT **76/76全0**。原记录表格式`changedPixels/maxChannelDelta`：

| 阶段 | Default | Compact | Dark | Dark+Compact |
| --- | --- | --- | --- | --- |
| default | 0/0 | 0/0 | 0/0 | 0/0 |
| aa | 0/0 | 0/0 | 0/0 | 0/0 |
| own | 0/0 | 0/0 | 0/0 | 0/0 |
| own-restore | 0/0 | 0/0 | 0/0 | 0/0 |
| global | 84952/255 | 84136/255 | 103967/230 | 98289/230 |
| global-restore | 0/0 | 0/0 | 4/16 | 4/16 |
| native | 42707/255 | 41747/255 | 47397/220 | 45852/220 |
| native-restore | 0/0 | 0/0 | 0/0 | 0/0 |
| parts | 0/0 | 0/0 | 0/0 | 0/0 |
| parts-restore | 0/0 | 0/0 | 0/0 | 0/0 |
| remaining | 0/0 | 0/0 | 0/0 | 0/0 |
| retemplate | 4404/228 | 4404/228 | 4404/180 | 4404/180 |
| progress-hide | 0/0 | 0/0 | 0/0 | 0/0 |
| progress-show | 4404/228 | 4404/228 | 4404/180 | 4404/180 |
| reattach | 4416/233 | 4416/233 | 4416/189 | 4416/189 |
| rtl | 4416/233 | 4416/233 | 4416/189 | 4416/189 |
| rtl-restore | 4416/233 | 4416/233 | 4416/189 | 4416/189 |
| selected | 4416/233 | 4416/233 | 4416/189 | 4416/189 |
| close | 3680/233 | 3680/233 | 3680/189 | 3680/189 |

进度最初总10秒，公开remaining输入4秒；候选换模板、隐藏/恢复及重挂后仍4/10，旧版回满或重挂丢part。四配方retemplate/progress-show各4404像素（max228/228/180/180）；reattach/RTL/选择4416（max233/233/189/189），close剩余五卡3680。原六卡内容/命中面和窗口约束仍在。

真实manager有限5秒、初始不显示进度，120ms后动态启用：managed/AOT各四配方24帧全部24 distinct，各**12检查0失败**，remaining持续下降、part唯一。未注入时钟或把暂停状态当持续动画，未比较不同相位的逐帧零差异。另有独立校准16帧按像素检查，未将其程序0结构断言误报为“零用例通过”。

已实际查看默认、旧/新retemplate、reattach、Dark Global、AOT专用Style、真实有限进度第6/18帧及暗色恢复差异：正文/操作/关闭完整，Global/Native实际进入目标，progress40%保持并持续递减。两张Dark/Dark+Compact的global-restore额外各4像素/max16，位于条端/边缘，原因未证实；不自动归入CT25/FW1或通用容差。

NativeAOT复用仓库MacOSHomebrewNativeAot.targets，发布无IL警告，保留五条本机OpenSSL/Brotli部署版本警告。所有构建/采样先fcntl非阻塞独占`.artifacts/verification/run.lock`，占用则拒绝，未并行采共享产物。Skia和AOT不代替NSWindow最终显示/其他Host/平台。

```bash
CT15_NOTIFICATION_METADATA_DIR=<external-directory> dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~Temporary.CT15Notification|FullyQualifiedName~Tests.Notifications.|FullyQualifiedName~FeedbackManagerPaddingTests'
dotnet build <external-driver>/CT15NotificationProbe.csproj -c Release
dotnet <external-driver>/bin/Release/net10.0/CT15NotificationProbe.dll <external-evidence>/candidate
dotnet <external-driver>/bin/Release/net10.0/CT15NotificationProbe.dll <external-evidence>/candidate-calibration calibration
dotnet publish <external-driver>/CT15NotificationProbe.csproj -c Release -r osx-arm64 --self-contained -p:PublishAot=true -o <external-driver>/publish
<external-driver>/publish/CT15NotificationProbe <external-evidence>/aot
<external-driver>/publish/CT15NotificationProbe <external-evidence>/timed-aot timed
```

## 限定出口与清理

仅[36对有意变化及root两有效值归位](notification-consumers-visual-review.md)申请独立审查，两张global-restore4/max16不申请通过。共享通用Stack几何/真实Host/六位置及自然高度/任意极值尚不由固定MaxHeight380样例证明，留与Message P2继续；完整Notification/CT15不关闭，不扩大CT12/13/14及CT25/FW1。未自动提交。

22临时验证所在3源文件/20500 bytes已删除，434文件外部工程/1105438755 bytes及598取证文件已删除；evidence cleanup审计clean，仓内无CT15Notification/NotificationPartsView专用残留，永久测试净增0，全部本包采样进程退出。

清理后按最终源码重编译并精确运行：**33PASS/0FAIL/0Skip，3.8731s**；Desktop NET8 Release **0 warning/0 error，17.23s**；成员**13文件/3控件/0问题**。正式五文档和本包计划/结果/审查表链接有效，git diff --check通过。未自行跑全量/affected-wide回归。

```bash
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~Tests.Notifications.|FullyQualifiedName~FeedbackManagerPaddingTests'
dotnet build src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj -c Release -f net8.0 --no-restore
python3 <atomui-control-optimization>/scripts/check-member-layout.py --path src/AtomUI.Desktop.Controls/Notifications
git diff --check
```

最终产品输入SHA256：


- `src/AtomUI.Desktop.Controls/Notifications/NotificationCard.cs`：`ad3c4ddfb9914977eaea3703a7c428b09b2908ae93e0c0432d88e92a827e1f2a`
- `src/AtomUI.Desktop.Controls/Notifications/NotificationCardToken.cs`：`5f67da496c758fd398d0de051b26d6d0b93d95cf15dd0b31781fd7f2b6f5cadc`
- `src/AtomUI.Desktop.Controls/Notifications/NotificationProgressBar.cs`：`b7aa0efc9faef761f532c7bda166ec22ead16858b819c1d52ff21bda47c047a9`
- `src/AtomUI.Desktop.Controls/Notifications/WindowNotificationManager.cs`：`fc1ac054109e60b9eb34a2537de23459609895781c381085ec60158adc470e2e`
- `src/AtomUI.Desktop.Controls/Notifications/Themes/WindowNotificationManagerTheme.axaml`：`a90c9760348cb23936d10f3dc398b90dfb1501b29b0a05d242d98f54e31e94f4`
- `src/AtomUI.Desktop.Controls/Notifications/Themes/NotificationCardTheme.axaml`：`90530a66ef2971ac6b512cda19c75d1334d46df655afa811171f922cae2d5b64`


## 2026-10-31 限定批准登记

2026-10-31，用户明确回复“批准表内36对及root两项有效值归位，其余未通过与完整CT15保留”（request_user_input_async / call_0421214e34e1401e90c4b4e835ef3c4e / item 0）。仅本表36对和列明两项有效值/属性默认纠正获限定批准；其他原始非零、P2及完整家族/CT15保留，不授权Git提交。


2026-11-01 P2原生宿主补证：native NSWindow960×720/scale2，managed/AOT完整各147及固定激活各148检查，六位置/变高gap16、原NoWrap全文/显式换行、Cocoa click/Space、五卡片hover/gap/leave、1500ms真实悬停剩余计时、四算法RTL通过。应用内合成Cocoa经Avalonia.Native，不冒充外部物理/其他OS输入。完整OS阴影画布随激活变化，跨运行像素不通过；正常入场初Frame与稳定绘制3497/max2差异定位，初始frame重绘触发、Window重绘无效，纯Border/淡入不复现，不归FW1或扩大容差。248取证/447原会话(411driver)/两AOT已清理，audit clean；清理后精确feedback padding/panel/motion与GroupBox70PASS/0Skip、1s，NET8零警告零错误21.10s。见feedback-native-p2.md；无反馈运行代码改变，GroupBox仅三Reserved XML说明同步。原失败、完整P2/CT15及CT25/FW1保留，不提交。


2026-11-01 Notification恢复定位：四算法/六卡/1280×900/384宽MaxHeight380及4/10公开输入保持，colour/global八A-B-A。所有数值/gradient stops/几何/原引用/命中通过；managed/AOT各128检查124PASS/4严格恢复FAIL，40原帧cross全0，各配方两完整背景重绘参考彼此0。原初始/局部恢复仍非零，旧两暗图4/max16不倒写；新两像素位置为卡片外缘/阴影，非进度输入丢失。不在原FW2批准表，未改归属。4临时/1源10908字节、174取证、427会话1107631045字节已清理，audit clean；清理后精确29PASS/0Skip/980ms，产品未变复用NET8零警告21.10s；见notification-restore-audit.md，完整家族/CT15保留，无提交。


最终清理后NotificationCardTheme/NotificationSemanticPart精确29PASS/0Skip/4.4735s，实际重建；本轮无生产源码变化，复用相同最终源码NET8零警告零错误17.41s。313取证经官方audit/cleanup clean后删除，外部438会话1374918333字节、全部历史恢复/原驱动/发布文件与marker删除；最终helper记录后清理。作者另步自审确认旧新两对与候选自身恢复/后续新ABA4FAIL不混报，没有以新绿倒写原账本。仅两对旧残留消失的限定接受待用户裁决，完整CT15继续。


## Notification 原两对恢复限定接受登记

用户明确回复“仅接受表列两对恢复差异，其余保留”（request_user_input_async / call_fdb5ce1da6f04f16b707b0e5d6b02d29 / item 0）。仅notification-original-visual-review.md列出的原Dark/Dark+Compact旧新global-restore各4/max16，作为旧版清除残留消失、候选回到共同默认的两对差异获限定接受。不宣称渲染根因已定，不把历史FAIL改写成当时PASS；后续新ABA4FAIL、平台/其他边界与完整CT15保留，不授权提交。


当前续作清理后Notification精确29PASS/0Skip/4.3151s，实际重建测试程序集；无生产源码变化，复用相同最终源码NET8零警告零错误17.41s。4临时理论/单源11785字节、460取证、779外部会话2174450601字节和marker已删除，audit clean；最后helper记录后清理。作者另步自审确认原正常4FAIL、no-motion3FAIL、独立模型7FAIL均保留，不把独立复现或跨运行0当像素PASS。18路径有限调查提案待用户裁决，未自动纳入FW2；原两个旧新差异的接受、其他家族/平台/完整CT15保持各自边界。


## Notification 新 ABA 18路径有限调查批准登记

用户明确回复“仅批准表列18路径归CT25/FW2调查，其余保留”（request_user_input_async / call_3c0dd46bb06948bfa8d56cec286a3aff / item 0）。仅notification-aba-visual-review.md所列8真实ABA、3真实初始静态重绘、4纯入场、3纯静态交CT25/FW2继续调查；非零仍FAIL，不增容差、不关产品动效、不加产品重绘补丁。两暗色独立入场各差1像素的限制保持，不认定原全部框架根因；其他边界、FW1/官方依赖与完整CT15均保留，不授权提交。


## 2026-11-02 单厚度与固定Part预算核对

真实standalone NotificationCard/原10秒完整正文，通过生成NotificationCardProgressStyle声明MinHeight16/Tag命中，只热改Own NotificationProgressHeight3→7；同一进度对象、16高Bounds及总量/remaining不变，真实新覆盖像素为Blue。临时单例1PASS/2s，没有取得产品反例，因此不增加AffectsRender补丁或类推Dot缺陷。3临时源5103字节已删除，无媒体/AOT/源码增量；清理后真实重建精确Notifications29PASS/0Skip，临时会话已删除。现有渲染失败与完整CT15不改写。
