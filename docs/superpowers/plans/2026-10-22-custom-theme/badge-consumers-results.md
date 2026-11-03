# CT15 Badge 当前工程结果（2026-10-31）

状态：13metadata/20Own的限定工程、清理与最终模块完成，48对限定视觉已获批准；Count退出两条已定位路径另有严格补证。完整Badge/CT15仍开放，原失败/恢复非零按本表和后续记录保留。当前CT15 Own未决0，不改变CT12/13/14或CT25/FW1，FW2仅按已批准表列路径。

## 源码与实际消费者

Count11、Dot5、Ribbon4迁DependencyDefaultsV2/revision1，80默认/type/key/slot保持。Count两角通过原Own依赖参数跟随有效高度，显式角0仍优先。其他字段进入真实measure/paint/layout；Ribbon immutable transform按Matrix.Value验证，不用ToString只输出类型名冒充值。

public owner仍无ControlTemplate，字体Setter位于原资源文件的无模板ControlTheme，以TextElement附加属性表达并保持Native Courier高于Global Menlo，clear回Global。内部Adorner颜色/角Global复用既有公共owner代理，Count/Dot跨根资源发布复用ThemeContextResourceBridge，logical attach/context更新建立、detach移除dispose。

standalone Count/Dot保留Panel作为visual挂载级联，Adorner direct logical/style owner与membership为Badge，修复公开Part route失配；target原AdornerLayer不变。四生成专用Style只能AXAML定制，实际观测不赋值回退。descriptor/route/ContractType不变。

原Indexer Local准备投射改按owner持有typed relay CompositeDisposable，detach/Dot模式reset释放，attach恢复；时机/优先级不能等价由setter表达的原因已注释。三族实例颜色以可移除值层覆盖，clear/无效值恢复底层Token，detach释放/当前值重挂恢复。颜色解析仍原preset/Color算法，不改目标内容、实例状态或公开API。

## 有效反证与匹配验证

- 初轮11项5FAIL/6PASS：Count两角依赖、Count Global字体、Dot状态色、Ribbon Global；80默认与Native/原目标内容通过。
- 首修11PASS；Native Courier+Global Menlo补验取得回归，字体移到public owner无模板Theme后12PASS，80默认不变。
- 四专用Style补验Count standalone Width53实际NaN为有效RED，direct logical owner修复后49PASS/0Skip（17临时+32既有）。
- target同Adorner热Own高度42实际20、三族保留detach Adorner时owner不回收为四有效退出/发布反例。bridge与typed句柄后52PASS/1 Count GC FAIL；Dot/Ribbon回收、target hot配置通过。
- Count小guard假设被A/B否证已撤回，临时强链真实指向Border composition visual经官方compositor parent到旧Count owner。该项当前未通过，不归并/扩大或豁免CT25/FW1，不反射修改私有parent、不替换依赖。
- 颜色clear三族RED：Count/Dot原brush变null，Ribbon保留旧#123456。可释放覆盖层修复后最终57所选56PASS/1 Count GC FAIL（25s），含25临时+32既有，原失败未丢弃。
- 成员22C#源/7注册Control 69问题纯整理为0，完整成员/注册/初始化/导入指纹相同。后续颜色功能变化单独记录，最终仍22/7/0，不把功能修改前后冒称纯移动。

临时诊断/驱动错误均分开记在[过程记录](badge-consumers.md)：BoxShadows LINQ误用、BOM offset开括号、缺using/nullable注解、原裸Avalonia Button无产品Theme等不计产品RED。产品Button保持180×90/原文本/Ribbon原位置，三点hit均经目标Button且3次click，未改变Badge事件或hit-test来掩盖症状。

## 实际渲染与当前边界

外部Probe引用原工作树，1000×1050、4算法实际内容面、正常motion、UseHeadlessDrawing=false与UseRegionDirtyRectClipping，真实DispatcherTimer。静态750ms+12等原始像素帧、6s上限；同完整历史旧新/AOT，无mask/crop/align/通用容差。

六主owner覆盖standalone/target、四Style两模式；20Own/依赖/0/clear、Global/Native、完整Count显示/溢出/0、Dot五状态/切模式、Ribbon位置/颜色/目标键鼠、RTL/祖先显隐/重挂/极值。Processing源仅现有状态色，无专门循环pulse；真实间隔帧单列，不凭空新增/关闭动效。show/hide动态过程与静态终态不能互相替代。

第一裸Button主4指针失败批354文件以pre-input-host保留；同Button去Badge仍hit Window.Panel且click0，否证Badge覆盖，改统一产品Button后三点1PASS。第一产品颜色恢复失败三版本1080文件前缀pre-color-clear保留。最终源26快照相同历史全部重跑，不挑旧批零帧消差。

当前最终候选主64检查0FAIL、校准4/0FAIL、plain无定制默认4/0FAIL；旧源主64/16FAIL，失败集中同target热Own、两角依赖、public字体，其余业务保留。AOT发布仅5条既有Homebrew macOS26-v-target12链接警告、无IL警告，最终主/校准/plain均已完成。plain在fixture attach前去用户class但保留全部原内容/目标/标准marker，与含专用Style的default分开。

当前已观察校准A/A0、1DIP可检出，但扰动恢复有17–27/max1非零；显隐shown/end/rtl-clear与重挂等恢复也有非零，明确未通过。颜色修复后原colors-clear大差异消失，仍不能据此覆盖其他失败。最终精确表和具体限定视觉工程待收齐；真实OS窗口/输入、Browser等未测边界保留。

重放入口为会话临时`replay.py candidate/old/aot`，所有构建/采样均以`.artifacts/verification/run.lock`的fcntl非阻塞独占锁守卫，冲突即拒绝；旧26源finally逐字恢复候选，不动其他既有改动。媒体当前仍在仓库外evidence登记目录，未清理，不能报告最终出口完成。

## 清理前检查点（历史；最终处置见末节）

- [ ] 最终三版主/校准/plain与精确RGBA、原失败记录、关键图片审查。
- [ ] 可解释有意变化的具体限定表；Count GC、其他不通过与未覆盖单列。
- [ ] 临时源/图扫描/驱动/媒体清理、清理后精确Badge模块/NET8、五正式文档/CSV/总表与作者自审。
- [ ] 用户限定视觉批准；完整Badge/CT15不因此关闭。


最终三版主/校准/plain收齐：managed/AOT各72检查0FAIL，旧主64/16FAIL、校准4/0FAIL、plain4/0FAIL。164跨运行全0；164旧新8零/156非零，48对仅请求限定有意批准，其余108整帧保留。4无定制plain旧新0。候选/AOT48项主恢复32零，颜色恢复后期business0，显隐/重挂等非零不覆盖；三版独立A/A0、1DIP恢复四对各27/19/24/17 max1未通过。Processing四相隔帧0仅证现有源行为。非零恢复逐项如下：

candidate：light/shown 4750/max255；light/end-restore 4750/max255；light/rtl-clear 4777/max255；light/reattach 8/max1；dark/shown 4731/max233；dark/end-restore 4731/max233；dark/rtl-clear 4750/max233；dark/reattach 18/max1；compact/shown 4673/max255；compact/end-restore 4677/max255；compact/rtl-clear 4697/max255；compact/reattach 8/max1；darkcompact/shown 4655/max235；darkcompact/end-restore 4656/max235；darkcompact/rtl-clear 4672/max235；darkcompact/reattach 18/max1。

old：light/colors-clear 2693/max178；light/count-restore 2693/max178；light/shown 6452/max255；light/end-restore 6452/max255；light/rtl-clear 6479/max255；light/dot-standalone 2657/max178；light/ancestor-restored 2657/max178；light/target-pointer 2657/max178；light/reattach 2657/max178；light/business-restore 2657/max178；dark/colors-clear 2693/max200；dark/count-restore 2693/max200；dark/shown 6433/max237；dark/end-restore 6433/max237；dark/rtl-clear 6452/max237；dark/dot-standalone 2657/max200；dark/ancestor-restored 2657/max200；dark/target-pointer 2657/max200；dark/reattach 2657/max200；dark/business-restore 2657/max200；compact/global-clear 5/max1；compact/colors-clear 2501/max178；compact/count-restore 2496/max178；compact/shown 6220/max255；compact/end-restore 6224/max255；compact/rtl-clear 6244/max255；compact/dot-standalone 2460/max178；compact/ancestor-restored 2460/max178；compact/target-pointer 2460/max178；compact/reattach 2460/max178；compact/business-restore 2460/max178；darkcompact/colors-clear 2496/max200；darkcompact/count-restore 2496/max200；darkcompact/shown 6202/max237；darkcompact/end-restore 6203/max237；darkcompact/rtl-clear 6219/max237；darkcompact/dot-standalone 2460/max200；darkcompact/ancestor-restored 2460/max200；darkcompact/target-pointer 2460/max200；darkcompact/reattach 2460/max200；darkcompact/business-restore 2460/max200。

aot：light/shown 4750/max255；light/end-restore 4750/max255；light/rtl-clear 4777/max255；light/reattach 8/max1；dark/shown 4731/max233；dark/end-restore 4731/max233；dark/rtl-clear 4750/max233；dark/reattach 18/max1；compact/shown 4673/max255；compact/end-restore 4677/max255；compact/rtl-clear 4697/max255；compact/reattach 8/max1；darkcompact/shown 4655/max235；darkcompact/end-restore 4656/max235；darkcompact/rtl-clear 4672/max235；darkcompact/reattach 18/max1。

候选原源26文件最后逐字对照（仅末次一行trailing whitespace清理，不改token/body）与成员22/7/0已核对，尚待临时清理与最终模块/NET8。


## 清理与最终模块验收

已读取最终三版及关键画面、原失败链并保存上述文本结论。取证审计clean/findings=[]，2524文件（73968333字节）均在仓库外登记目录；清理前临时测试3源26658字节，外部driver419文件、publish15文件，原会话总537文件（包含driver/publish，不重复相加）。Count GC失败临时测试不转为永久保障，删除后仍保留FAIL结论，既有模块通过不能覆盖该失败。最终精确Badge32项及NET8结果待下述补记。

最终清理：evidence cleanup返回removed=true/files_removed=2524、audit clean/findings=[]；3临时源、原537文件会话（含driver419/publish15）和marker已删除。清理后精确命令`dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Badge`为32PASS/0FAIL/0Skip、1s；`dotnet build src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj --framework net8.0 -c Release --no-restore`成功、0警告0错误、23.02s。均先取得共享输出非阻塞独占锁，无全量回归。最终另建run.py/tests.log/net8.log三文件目录读取后删除。五正式文档、13metadata/20Own CSV、137/274总表与自审同步；清理后绿不改变已记录Count GC/恢复FAIL。48对有意视觉已获用户限定批准，批准范围见末节。


限定批准登记：2026-10-31，用户明确回复“批准表内48对，其余未通过与完整CT15保留”（request_user_input_async / call_d223a18435b5484787082d74b8ee393e / item 0）。仅Badge表内48对有意变化获批准；Count composition GC1FAIL、显隐/重挂/校准恢复非零、其余108对及完整Badge/CT15继续保留，不授权提交。


后续Count退出：[独立结果](badge-count-retirement-results.md)已分别定位standalone composition成员和layer association时机，两源最小修正、normal motion前失败后通过、managed/AOT各108检查及72静态零变化、清理后32/NET8零完成。原57中GC1FAIL及后续8原版FAIL不抹除；此仅修正这两个限定保留路径，不改原48批准或其他恢复/平台/极值/完整Badge与CT15边界。
