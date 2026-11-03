# CT15 P3-A Timeline 动态集合与容器生命周期结果

2026-10-30，现有`feature/custom-theme`、`/Users/chinboy/Projects/dotnet/AtomUIV6-custom-theme`；主agent执行与另步作者自审，无新分支/worktree、子agent、提交或全量回归。保留其他家族未提交改动。本包是P3的状态切片，**不是完整Timeline或CT15完成报告**。P1/P2既有证据保持，完整输入核对见[真实剩余](feedback-family-remainder.md)。

## 根因与实现

1. 移除末项或pending时，逻辑子节点通知早于ItemCount更新及presenter旧容器删除。旧循环读滞后的ItemCount／ContainerFromIndex，把已删除项继续当末项，导致存活末项仍有多余rail及间距。改读当前Items计数，自带TimelineItem直接取源对象，数据项继续取生成容器；后续prepared/index回调仍使用原状态投影，无延时、强制重排或重建页面。
2. PrepareContainer的三条状态绑定没有释放。公开自带容器又不经过生成容器Clear，移出后仍响应旧owner。三条绑定改由Item的真实Parent生命周期获取和Dispose，兼容自带/生成容器、移出和换父。Mode/Reverse是DirectProperty运行状态，ControlTheme不能建立等价Setter；没有新公开API、Token、Part、identity、时钟或代码样式定制。

只触及Common的AbstractTimeline／AbstractTimelineItem及其正式说明；没有重做P1/P2、CT12/13/14，也没有替换官方Avalonia12.1.4。

## 有效前后验证

临时源位于Desktop测试项目`Temporary/CT15TimelineExit/TimelineExitTests.cs`。原有测试先检索复用。

- 夹具先修正枚举using、Window命名空间冲突、nullable告警；这些编译错误不是产品RED。最初RTL观测只取镜像后的局部原点，六项误判；改为变换后矩形物理左边界，六项原本通过，无RTL产品修改。
- 有效RED：12项中**2 FAIL／10 PASS，4.2065s**。分别是移出Item仍接收旧Mode、删除尾项后middle.IsLast=false。
- 候选首轮与原52项合计**64 PASS，4.4212s**。扩展代表Own极值及数据容器后17项通过；最终18临时＋52既有合计**70 PASS，5.5612s**。
- 覆盖两方向三模式的LTR/RTL坐标及复位，pending反复增改/删除、隐藏/标签/Reverse，同实例Compact/Dark+Compact往返，ItemsSource普通数据容器删除/替换/清空再加，长寿命移出Item保留时旧Timeline WeakReference释放，再挂新owner。
- 代表极值为字号1/96、dot64／border0／rail0、间距99、末项120及清除，原root Height180保持。字体恢复原Bounds，Own清除恢复末项实际MinHeight0。不以极值代表11字段的全部分支；P2逐11 Own/0/透明证据继续复用。

## 真实Skia与当前Mac NativeAOT

外部临时自驱动工程引用当前源码，真实Skia、UseHeadlessDrawing=false、normal motion。1320×760、scale1；每配方6根、两方向三模式、每根原Height300、原长内容/标签/Button/静态PathIcon pending保留。Default/Dark/Compact/Dark+Compact各16个静态阶段，A→B→A保留实例；Compact的同实例切换另由临时单测验证。

真实MouseDown/Up一次Click，Focus＋Enter再一次Click；代码赋值只用于场景状态，未代替输入。8个生成专用Semantic Style真实编译及route命中，8个Tag Setter、4个颜色/Opacity Setter源码形态检查通过，code-behind仅Load，无Name/Loaded/Unloaded找节点定制。

旧/新managed和当前Mac osx-arm64 NativeAOT各**276项驱动检查通过**。发布无IL警告；五条已知Homebrew dylib的最低macOS linker警告保留，不证明macOS12支持。这里的NativeAOT使用Headless/Skia，**不是NSWindow系统最终显示面**。

默认pending循环另行运行，未关motion、冻结clock或以静态图标冒充。旧managed明29帧、暗30帧均不同；新managed明暗各30帧均不同；AOT明30帧29个不同、暗30帧30个不同，实际采样约3.2s，移除后容器数正确。此为持续运行与移除证据，不是相同时间原点的全轨迹像素兼容结论。

## 完整画面对照与未通过项

同版本A/A四配方均0；1DIP正对照分别24693／24181／28174／27661像素，复位及RTL复位均0。旧新64对为**50零／14非零**；JIT/AOT64对为**53零／11非零**，没有裁剪、遮罩、对齐、统一容差或覆盖基线。

其中8对为本次正确末项状态的有意修复，见[限定视觉表](timeline-exit-state-visual-review.md)。另6对旧新非零与11对JIT/AOT非零仍保留原始FAIL：主要是共享Global改色后的嵌入Button文字过渡，及真实输入后的Button边缘。旧源码自身也有配置恢复非零；加入至少700ms、12个图像与属性稳定帧仍出现，不归结为过早采样。仅凭这一现象不能认定框架根因或本次已修复。

新managed从初始默认到custom清除分别1410/max13、1296/max5、1512/max89、1512/max137；Part资格退出恢复分别102/max1、102/max1、82/max1、82/max1。所有Timeline记录几何恢复，Button.Foreground存在中间颜色值；原始数值不改写成通过。既有CT25/FW1问题及批准范围保持，本次额外观察不自动归入其已批准容差，不改写CT12关闭结论。


未通过跨运行对照的完整计数（像素/max通道；0表示该对为0，不代表整配方通过）：

| 配方 | 旧新custom | 旧新custom清除 | 旧新input | JIT/AOT custom | JIT/AOT custom清除 | JIT/AOT input |
| --- | --- | --- | --- | --- | --- | --- |
| light | 0 | 0 | 0 | 2514/2 | 1134/2 | 987/1 |
| light-compact | 0 | 0 | 949/1 | 2106/1 | 882/1 | 939/1 |
| dark | 3282/71 | 1512/61 | 0 | 3288/95 | 1512/30 | 0 |
| dark-compact | 3300/56 | 1512/57 | 949/1 | 3282/47 | 1512/48 | 802/1 |

已查看完整默认、Global极值、删除末项旧/新、明暗差异及AOT spinner关键帧。极值下固定宽度的自然换行/裁剪、旧暗色黑字保持原貌；没有宣称任意字号无裁剪或暗色可读性已解决。

## 清理、最终模块验收与自审

18项临时测试所在1源/12311 bytes已删除；外部driver434文件/1088135119 bytes已删除；取证419文件（226 PNG含派生差异、192几何文本及manifest）已删除，evidence cleanup审计clean，无Git媒体发现。专用checkout输出0，永久测试净增0。只保存本报告的必要数值，不保留驱动或源码备份。

清理后以最终源码真实重建，精确Timeline **52 PASS／0 FAIL／0 Skip，1s**；Desktop NET8 Release（覆盖Common）**0 warning／0 error，19.02s**。没有使用no-build冒充清理后验收，没有执行change/affected-wide/full回归。命令：

```bash
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Timeline.'
dotnet build src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj -c Release -f net8.0 --no-restore
python3 .agents/skills/atomui-control-optimization/scripts/check-member-layout.py --path src/AtomUI.Controls/Timeline/AbstractTimeline.cs --path src/AtomUI.Controls/Timeline/AbstractTimelineItem.cs
git diff --check
```

所有构建/运行经checkout的`.artifacts/verification/run.lock` OS锁串行执行，开工确认锁可用，未覆盖其他会话采样。最终HEAD为4560091de；原有status条目无减少。两产品文件SHA256分别为`142cacee2f5e13c9472e893ed61a3ad7975f6c497c0af1bf399653762aee5ff6`（AbstractTimeline）、`efb3d595af3f2ce0dcbd26b06efff9c6f8bcafe23adbdf7b380255d7b3ebc1b2`（AbstractTimelineItem）。

正式implementation/changelog、两CSV、范围核对与tasks已同步。metadata仅12行变化（8 Timeline增量＋4 ButtonSpinner批准复用）；Own仅19行notes变化（11 Timeline＋8 ButtonSpinner），目录键集137/274均保持。P3-A工程与清理门槛及表内8对限定视觉批准已完成；完整P3与CT15仍开放。

作者另步审查范围为本次两文件增量，非独立评审。审查Parent替换/Dispose、生成与自带容器、权威集合时序、AXAML优先、Direct状态例外及原尺寸/输入合同；未发现本包新增的Critical/Important代码问题。成员审计2文件/2控件/0问题；两处region调整的Roslyn成员token快照完全相同，运行期弱引用字段无初始值，移动不改变静态初始化顺序。

Ruling：沿用户指定目录/分支、无委派/提交；执行技能的默认隔离、委派和提交步骤不适用。Ruling：仅提交P3-A两项根因修复供限定审查，不关闭完整P3/Timeline/CT15；误读会导致丢失后续业务/持续轨迹/家族/平台门槛，因此总表与CSV继续标记部分验证。无搁置的代码minor；未通过视觉不作为minor豁免。

## 继续边界

完整P3仍需更完整集合组合、同时间持续动效轨迹、其余极值/字段分支及家族出口；原生最终显示面与其他Host按既定平台责任处理。其余125 metadata／255 Own是扣除Timeline与ButtonSpinner批准复用后的目录分组，不能据此视作全都未经任何共享验证；逐家族工作继续按[计划](feedback-consumers.md)和[范围核对](feedback-family-remainder.md)推进。

## 2026-10-30 限定批准登记

2026-10-30，用户明确回复“批准表内 8 对，其余未通过与完整 CT15 保留”（request_user_input_async / call_2f2081133b664f489dfdebc37e25baf6 / item 0）。仅P3-A表内末项rail/间距8对已获限定批准；其他非零、完整P3/Timeline/CT15及CT25/FW1边界保持，未授权提交。

## P3-B 后续定位

[正常帧推进结果](timeline-frame-pump-results.md)通过单变量证明中间色来自临时驱动遗漏Dispatcher定时器。修正驱动后旧新64对仅8对已批准状态变化，JIT/AOT64对全0；原始FAIL计数保留为历史，不自动清除Part恢复max1或持续动画精确周期端点观察。无产品改动，完整P3与CT15仍开放。
