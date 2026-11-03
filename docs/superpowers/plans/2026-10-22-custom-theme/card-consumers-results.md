# CT15 Card 实际消费者结果（2026-10-31）

范围7 metadata／22 Own，单agent在指定feature/custom-theme原目录执行，无分支/worktree/提交。保留本包前已有CardTheme.EnableMotion的CT11 owner变更，旧版回放也包含它。CT12/13/14关闭结论、CT25/FW1原始FAIL与官方Avalonia12.1.4保持。本文是工程切片证据，完整Card及CT15未关闭。

## 根因与实施

- 22项默认迁DependencyDefaultsV2 Revision1；四算法88条默认、type、key、slot逐条相同，完整Desktop生成TokenResourceConst SHA256保持`7ff07970abd14f9c303a692f73387fcee2f246053a88a67cf3cd9baa3d784d57`。
- 18 Active：三档Header字体/高度/Padding、三档BodyPadding、HeaderBg、CardShadows、CardGridItemShadows、CardActionsIconSize，以及ActionsBg/ExtraColor两OptionalOverride。ActionsBg旧静态引用实际被Frame.TemplateBinding的空root值遮住；二者明确Own激活，未配置保留真实透明/继承色。局部Global按真实公开owner，内部action panel归Card；五个公开文字根复用Core有限继承桥，角色字号与独立TabControl/Avatar/Skeleton/IconButton责任不合并。
- 四Reserved：ActionsSpacing、TabsMarginBottom、CardHeadPadding、CardPaddingBase。历史/实际源码没有独立消费目标；遵循T22，不创造重复padding、未定义gap或把完整TabControl当旧head-tabs。保留名称、默认、type、key/slot、配置往返；纠正旧文档的虚假有效消费者描述。
- Small普通body的无条件Padding误进入Grid/Tabs，改由实际内容分支提供。根Card.Padding进入外Frame；GridItem三档Padding在root提供默认，原生值可覆盖。actions专用Style的Background/Padding进入实际Frame，自绘分隔线按实际child坐标绘制并响应Padding/集合失效。
- 动态Content和容器状态先于template存在，类型化绑定使用Template优先级并有明确owner释放；自带GridItem跳过generator Clear时按真实逻辑membership释放扩展及状态。Size/disabled/hover原生本地值保留，默认容器跟随Grid.IsHoverable。Grid行列定义是CLR集合，AXAML安装绑定实编译AVLN3000，保留原强类型投射并在代码注明原因。
- 两项可观测属性默认纠正须限定审查：内部actions.Padding原未消费top1→原生0，保持原实际布局；GridItem.root.Padding原0→三档实际Own值，保持原Frame实际Padding，同时使本地值有效。公共API、注册顺序、事件、Part名/route/ControlTheme key及三档SizeType保持；不新造Custom枚举。Reset原NotSupportedException不改。

## 临时验证与有效反例

Temporary/CT15Card共36临时+20原Card=**56 PASS／0 FAIL／0 Skip，7.0151s**。覆盖三档/四内容、五owner根字体与真实Body文字、明确Own/合法0/透明值/清除、Headerless往返、两个阴影和实际hover、原生Padding/disabled/hover/size优先、专用Card六Part与Meta四Part命中/隔离、actions实际Frame及左右中输入、集合移除、Source重模板、动态定义投射和GC。长寿命Grid移出item/text两WeakReference及五文字根各root/text共10 WeakReference全部释放。

初始19项14FAIL/5PASS，其中派生Grid未指定StyleKey的一项无效；补StyleKey后旧源码单项重新取得真实disposable未释放FAIL（3.4312s），候选完整40PASS。原生Card/GridItem.Padding另取得2有效RED；修复后47PASS，再扩展53PASS。最终零值扩展首次7FAIL来自夹具：4项缺导出目录环境变量、3项误将具不同RGB的Colors.Transparent与明确#00000000比较；仅修正夹具后56PASS，不将这些错误计为产品RED。Tabs.ItemsSource重模板旧版已通过，未为假设问题修改代码。

精确命令（临时源按合同现已删除）：

```bash
CT15_CARD_METADATA_DIR=<external-directory> dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Temporary.CT15Card|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Card.'
dotnet build <external-driver>/CT15CardProbe.csproj -c Release --no-restore
dotnet <external-driver>/bin/Release/net10.0/CT15CardProbe.dll <external-evidence>/candidate
dotnet publish <external-driver>/CT15CardProbe.csproj -c Release -r osx-arm64 --self-contained -p:PublishAot=true -o <external-driver>/publish
<external-driver>/publish/CT15CardProbe <external-evidence>/aot
<external-driver>/publish/CT15CardProbe <external-evidence>/loading-aot loading
```

每次构建/采样先以fcntl非阻塞独占`.artifacts/verification/run.lock`，占用则拒绝启动；无并行共享产物采样。原始18源文件文本在finally恢复，回放后18/18与候选快照相同。没有checkout/reset用户改动。

## 真实Skia与Mac NativeAOT

实际源码引用，1260×1380、scale1、12张380×300 Card；三列Small/Middle/Large，四行普通/Meta/Grid/Tabs，保留完整Header/Extra/Cover/Body/Actions、选项与正文。Default/Compact/Dark/Dark+Compact四配方、22阶段，版本不改变操作历史。真实DispatcherFrame泵运行定时器，正常Motion；至少700ms且12连续相同帧（上限5s）取稳态。RegionDirtyRectClipping=True，不重建一侧、裁剪/遮罩/对齐、禁动效或扩大容差。

managed和AOT各**900检查0失败**，**88对JIT/AOT精确RGBA全0**。旧源码同驱动900检查中372FAIL，含实际Own、owner/字体、原生Padding及专用actions；旧/新88对原始非零如下，单位`changedPixels/maxChannelDelta`，不是自动通过：

| 阶段 | Default | Compact | Dark | Dark+Compact |
| --- | --- | --- | --- | --- |
| default | 4514/255 | 4293/255 | 4504/200 | 4281/200 |
| aa | 4514/255 | 4293/255 | 4504/200 | 4281/200 |
| shift | 4514/255 | 4293/255 | 4504/200 | 4281/200 |
| shift-restore | 4514/255 | 4293/255 | 4504/200 | 4281/200 |
| local | 8228/255 | 7918/255 | 8228/200 | 7918/200 |
| local-restore | 4514/255 | 4293/255 | 4504/200 | 4281/200 |
| padding | 85863/255 | 84555/255 | 85658/250 | 84401/250 |
| padding-restore | 4514/255 | 4293/255 | 4504/200 | 4281/200 |
| own | 188901/255 | 166213/255 | 188888/228 | 166199/228 |
| own-restore | 4514/255 | 4293/255 | 4504/200 | 4281/200 |
| reserved | 4514/255 | 4293/255 | 4504/200 | 4281/200 |
| reserved-restore | 4514/255 | 4293/255 | 4504/200 | 4281/200 |
| global | 1346375/255 | 1346257/255 | 1347215/248 | 1347097/248 |
| global-restore | 4630/255 | 4389/255 | 4589/200 | 4347/200 |
| parts | 16662/255 | 41685/255 | 16662/235 | 41643/235 |
| parts-restore | 4514/255 | 4293/255 | 4504/200 | 4281/200 |
| inner-borderless | 4771/255 | 4555/255 | 4761/207 | 4543/207 |
| variant-restore | 4514/255 | 4293/255 | 4504/200 | 4281/200 |
| rtl | 4081/255 | 3864/255 | 4074/200 | 3857/200 |
| rtl-restore | 4514/255 | 4293/255 | 4504/200 | 4281/200 |
| tabs-selected | 4534/255 | 4312/255 | 4524/200 | 4301/200 |
| input | 4534/255 | 4312/255 | 4524/200 | 4301/200 |

默认变化只在Small Grid和Small Tabs：去掉外body重复padding，其他10卡几何保持。1DIP检错每配方35215/35185/34655/34661像素，A/A及1DIP恢复四配方全部0。有意变化32对独立见[限定审查](card-consumers-visual-review.md)；不因局部审查批准全部88对或整个家族。

另用新建的同等12卡正常loading场景采24实际帧/配方，IsActive保持True且Content引用不变。managed distinct19/20/19/19，AOT19/21/19/18；各148检查0失败。最初按2.4s预算又逐帧PNG编码只采到5帧，unique>=8有4FAIL属驱动预算错误；改为固定24帧、原始像素哈希与每6帧存图后通过，保留该记录，不虚构动画产品修复。未做不同时间相位的loading逐帧零差异声明。

已实际打开旧默认、新Own、RTL、Compact专用Style、Dark Global、loading第12/18帧及恢复差异图：布局/原内容仍存在，橙色actions落于真实面，RTL内容顺序和操作分隔线可见，loading持续变化。未用单看数值代替视觉检查。

## 保留的未通过项

- 同候选local/padding/own/global/parts/variant/RTL恢复及Reserved阶段与最初default比：普通/Compact各312/max2，Dark/Dark+Compact各110/max1，主要在外卡圆角边缘；包括loading退出同等默认的相同数值。已查看放大差异，不设容差、不替换依赖。原因尚未证实，不擅自归为已批准框架噪声。
- 旧新global-restore四对4630/4389/4589/4347（max255/255/200/200），除Small修复还含其他位置微差；整对排除出有意变化审查。
- 其余旧新重复阶段52对主要重复Small修复记录，不扩展为新独立批准。上述完整88对和所有恢复非零保持原数值，未将非零改写为默认全兼容。
- 本机UseHeadlessDrawing=False的真实Skia/原生AOT不等于NSWindow最终屏幕或其他Host/平台；任意极值不裁剪、独立子控件完整产品合同及既有Dark黑字未由本包证明。P3周期端点/Part退出、Alert未通过、CT25/FW1边界继续保留。

## 作者另步自审与清理后验收

作者单独复读14产品文件增量和完整Card源/正式五文档，对照原始合同检查owner、显式存在位、优先级、运行关系释放、自绘坐标、无反射/AOT、初始化/注册顺序及全部36临时处置。仅作者自审，遵守禁用子agent要求，非独立评审；无新增Critical/Important或搁置代码minor，像素未通过不以minor豁免。

Ruling：四孤立字段按Reserved保留，当前方案优于凭名称创造布局。Ruling：动态关系/CLR集合保留类型化代码投射，有AVLN3000及模板前生效的具体证据；可达稳定模板输入保留AXAML。Ruling：保持三档和独立公开owner，只修Small特殊内容误加padding。限定审查不授权提交或关闭完整CT15。

36项临时验证所在3源文件/26133 bytes已删除；434文件外部工程/1104837618 bytes及591个取证文件已删除，evidence cleanup审计clean，永久测试净增0。全部采样进程已退出，无本包媒体留在工作树或index。

清理后重新编译并精确运行Card：**20 PASS／0 FAIL／0 Skip，3.2957s**；Desktop NET8 Release **0 warning／0 error，17.49s**；最终Roslyn成员审计**11文件／6控件／0问题**，原注册/字段初始化顺序保持。八篇正式/本包文档链接有效，git diff --check通过。没有自行运行全量/affected-wide回归。

```bash
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Card.'
dotnet build src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj -c Release -f net8.0 --no-restore
python3 <atomui-control-optimization>/scripts/check-member-layout.py --path src/AtomUI.Desktop.Controls/Card
git diff --check
```

本包产品源最终SHA256：


- `src/AtomUI.Desktop.Controls/Card/CardGridContent.cs`：`e078c418707eb1adf65d9eb895ec5bcf613241ccb87d4c3cf5eb8cf74cdf44aa`
- `src/AtomUI.Desktop.Controls/Card/CardTabsContent.cs`：`27043ba550e9500170bfd7419c14c118450a2f1187b39badfd1066a0e72f0a63`
- `src/AtomUI.Desktop.Controls/Card/CardGridItem.cs`：`c151857f71d1119aac57589bbc41220f9ab6b3b0cec955d6f7c134cfd7f7cb2a`
- `src/AtomUI.Desktop.Controls/Card/CardToken.cs`：`89a9eda7e99f1937371b4508c465d1c87a32fdf30bc82abc8c09f1c558a4f12a`
- `src/AtomUI.Desktop.Controls/Card/CardActionPanel.cs`：`2b3f20695cd54c4e2020c77f8c482e50902d5bbb85c834cbd7ed5e7ccded149b`
- `src/AtomUI.Desktop.Controls/Card/Card.cs`：`f536692735a97da01c63f5380d5b824dfb392a1b7d5a0bf8d85afe68f89a48cf`
- `src/AtomUI.Desktop.Controls/Card/CardMetaContent.cs`：`409f300a50b9b2bb5b09e087272e4d0fe702ebf0a098a1894b2ea3d5f6628694`
- `src/AtomUI.Desktop.Controls/Card/Themes/CardTheme.axaml`：`cb6b3faf531fc483a4af54dc6ac099106b70566808a90f8ee01f1401dc8d903a`
- `src/AtomUI.Desktop.Controls/Card/Themes/CardGridContentTheme.axaml`：`3e5c5e4bc1f9d9477b6c1a30e1c4c05ad22e88a6f5c9fa6fcacffc85855bbc74`
- `src/AtomUI.Desktop.Controls/Card/Themes/CardTabsContentTheme.axaml`：`900c2e66d00cdcffc15c0c74a5393b11bcd4ea6f686e6fca27c087eff3ebb6e7`
- `src/AtomUI.Desktop.Controls/Card/Themes/CardMetaContentTheme.axaml`：`96f0025a7210aebe4f989a211110aba6143559125c8477a594aee6338165ee1e`
- `src/AtomUI.Desktop.Controls/Card/Themes/CardActionButtonTheme.axaml`：`5e20e23f4a1d79c91a02bdd27339815678b343d6269520ac953813be040ebeed`
- `src/AtomUI.Desktop.Controls/Card/Themes/CardActionPanelTheme.axaml`：`37e05224845bc2641850bda6f8ebe250492cd4454809c08caf59173e62b5b5e7`
- `src/AtomUI.Desktop.Controls/Card/Themes/CardGridItemTheme.axaml`：`570baa76d66e65bafef9c2e705a61632c635fb51ccffdd209d53d3dd424819f8`


## 2026-10-31 限定批准登记

2026-10-31，用户明确回复“批准表内32对和两项属性默认纠正，其余未通过与完整CT15保留”（request_user_input_async / call_f0de5e7e830f44efaa6cd74e2c0072e0 / item 0）。仅本表32对和列明两项有效值/属性默认纠正获限定批准；其他原始非零、P2及完整家族/CT15保留，不授权Git提交。


清理后精确Card20PASS/0Skip/4.7581s，实际重建测试程序集；本轮无生产源码变化，复用相同最终源码NET8零警告零错误17.41s。官方cleanup910取证、外部790会话2173296792字节与全部恢复/诊断/发布源码已删除，marker删除；最终helper记录后清理。作者另步自审：原88图完整，单变量clear范围限制已纠正、Frame重录因果与独立差分精确对应，严格4FAIL未改为PASS；无永久测试/全量/分支/worktree/提交。

仅card-original-visual-review.md表列40路径提出FW2继续调查，尚待用户明确裁决。原312/110非零仍FAIL、旧新Global完整对比与其他原边界/完整CT15不变，不将当前调查当整个家族关闭。


## Card 40路径限定调查批准登记

用户明确回复“仅批准表列40路径归CT25/FW2调查，其余保留”（request_user_input_async / call_c8333768d41d4ba692fea52d54ee7dea / item 0）。仅card-original-visual-review.md表列原32恢复、4同值Frame重录及4独立图元路径交CT25/FW2继续调查；非零仍FAIL，不增容差、不打产品重绘补丁、不改FW1/官方依赖。原旧新Global非零、其他边界与完整CT15保留，不授权提交。
