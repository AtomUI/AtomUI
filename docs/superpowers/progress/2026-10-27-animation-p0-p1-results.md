# 动效优化 P0 P1 交付记录

本轮按已批准的[方案B](../specs/2026-10-27-animation-system-optimization-design.md)完成P0与P1，用户已授权提交。
研发位置保持当前 `release/6.0`，未创建分支/worktree。未实施P2–P7。

## 实现

- P0：在既有GalleryPerformance工具增加显式 `--native-motion` 模式，使用真实GalleryApplication、Workspace、页面和控件。
  原生场景覆盖sidebar、submenu、progress、statistic、beam、popup及beam-offscreen；提供独立进程聚合与比较脚本。
- 观测公开属性、实际Bounds、布局pass耗时、已有合成batch的Processed/Rendered，以及每动作进程CPU、UI/总分配与GC。
  不请求额外RAF/commit，不把SceneInvalidated当FPS；记录宿主、程序集、产品差异、DPI、主题和场景形态。
- 采样拒绝没有跨时刻中间值或缺少绘制批次的有限动画；比较拒绝宿主、实际目标、观测属性和动画合同不同的工作负载。
- P1：NavMenu根宽度使用原生Animation驱动既有内部布局值，移除16ms延时循环、手算进度和逐帧Dispatcher宽度投递。
  保留CubicEaseOut、MotionDurationMid、真实宽度变化、Width/MinWidth coercion、基础绑定、选择与展开路径。
- 新请求取消前捕获实际Bounds；Mode切换解除旧覆盖。保留一次完成后的Dispatcher收尾，等待框架绑定/final fill退出，只有当前CTS可清理。
- 首tick前取消Forward动画可能写入默认0，已用失败证据确认；取消期间coercion保持当前呈现宽度，旧绑定退出后才释放所有权。
  使用现有CTS的真实取消状态，没有新增ignore/suppress标志或第二套业务状态。

产品改动入口：[NavMenu.cs](../../../src/AtomUI.Desktop.Controls/NavMenu/NavMenu.cs)。
工具入口：[NativeMotionProbe](../../../tools/performances/AtomUI.GalleryPerformance/NativeMotionProbe.cs)、
[场景](../../../tools/performances/AtomUI.GalleryPerformance/NativeMotionScenarios.cs)、
[观测](../../../tools/performances/AtomUI.GalleryPerformance/NativeMotionObservation.cs)、
[聚合脚本](../../../tools/performances/run_native_motion.py)，命令见[性能工具说明](../../../tools/performances/README.md)。

## 性能证据

环境：macOS26.6.2、M1 Max、60Hz、1300×900 DIP、2×缩放、Release/net10.0、Avalonia12.1.4、Native窗口。
API驱动不包含原生pointer延迟，Rendered是batch完成而非显示器present。CPU/分配包含同版本探针开销。

侧栏基线和最终版本各3个独立进程，每方向5次预热、30次正式动作，共180次正式动作/版本。
另对全部7个场景各做3进程、1次预热、5次正式循环的探索性基线与对照，每组180个动作。
工具反馈修正后重新校验了全部原始序列和场景身份，没有缺失batch或终态失败；最终版本再完成全场景12动作smoke。

| 指标 | 基线 | 最终版本 | 计算 | 改善 | 结论 |
| --- | ---: | ---: | --- | ---: | --- |
| 每次宽度更新额外延时任务 | 1次 | 0次 | `(1-0)/1` | 100%移除 | 原生时钟直接驱动，无独立延时帧链 |
| 每次宽度更新额外Dispatcher投递 | 1次 | 0次 | `(1-0)/1` | 100%移除 | 保留的单次收尾不属于逐帧投递 |
| 收缩宽度更新间隔中位 | 17.083ms | 16.661ms | `(17.083-16.661)/17.083` | 2.47% | 更新节奏对齐60Hz，不能据此称整体FPS提高 |
| 展开宽度更新间隔中位 | 17.080ms | 16.659ms | `(17.080-16.659)/17.080` | 2.46% | 同上 |
| 每次展开布局pass中位 | 29次 | 17次 | `(29-17)/29` | 41.38%减少 | pass合并增加，不代表布局总耗时同比减少 |
| 收缩Rendered间隔中位 | 16.643ms | 16.630ms | 差异约0.013ms | 不声明 | 渲染节奏差异处于噪声量级 |

最终Rendered P95：收缩19.877ms、展开18.061ms；P99为23.950/25.542ms。
超过2个60Hz预算的间隔分别为3/1081（0.28%）、6/1216（0.49%），达到本方案本机受控样本目标。
收缩布局pass仍为13次。单pass和总布局时间受合批及调度影响，不从pass数量推导耗时提升。

### CPU与分配的结论边界

以下完整列出同版本复测，避免只选有利一轮：

| 每动作进程CPU中位 | 收缩 | 展开 |
| --- | ---: | ---: |
| 原驱动基线 | 90.810ms | 109.979ms |
| 原生驱动首轮 | 105.642ms | 126.238ms |
| 同版本原生驱动复测 | 91.739ms | 107.585ms |
| 补齐取消边界后的最终版本 | 72.977ms | 94.661ms |

CPU没有形成稳定的可归因提升或回退，本次不报告CPU加速百分比。
UI分配中位由每次收缩2,420,824B至2,408,472B、每次展开4,495,744B至4,468,676B，差异不足1%且含探针成本，
不据此声称显著内存或页面加载收益。其它场景为探索性观测，未修改其产品实现，不把其波动归为P1收益。

真实NavMenu Popup默认OpenMotion/CloseMotion为空，工具将其明确标为静态开合；离屏Beam用viewport几何断言，零更新是合法结果。
不将静态场景的零动画通知误报为动画丢失，也不把直接终值跳变当作动画成功。

## 正确性和验证

- 原驱动在手动动画时钟不前进时仍按墙钟变化，时钟/反转/Mode用例取得修改前失败证据。
- 10个临时时钟/生命周期用例通过：曲线中间值及相邻内容位置、首tick前反转、旧完成隔离、Width/MinWidth运行中绑定变化、
  模式/动效/折叠目标变化、detach及reattach后再播放。首tick前零宽通知先失败、修正后通过。
- 原生Gallery确切PID与window ID的截图已查看：中间收缩、48DIP紧凑态、280DIP展开态均保留图标与完整正文；
  没有文字缩放或移除内容/滚动约束。静态截图不承担FPS或Windows体验证明。
- 临时反例证明旧采样会接受直接跳变、不同host/合同/属性；新校验全部拒绝。编译实际观测源的5个数据用例通过，
  覆盖终值、两个端点、同步三值、真实中间值及无绘制批次。
- 现有宽度动效测试改为异步等待，并推进真正Headless渲染tick；原中间/终态/绑定断言保持，新增首tick前反转断言。
  不新增常驻Fact/Theory，临时验证结束后删除。

NativeAOT：PublishToLocal脚本通过osx-arm64原生产物校验；临时AOT安全驱动在真实原生程序中完成3轮宽度开合，
观察72次有效宽度通知、端点48/280，通过。普通RID与非RID产物已重新构建并确认不含临时验收类型。
发布有ReactiveUI/ReactiveUI.Avalonia程序集级IL2104/IL3053警告及Homebrew OpenSSL/Brotli最低macOS版本链接警告；
本次验证当前macOS宿主，不宣称全库零trim警告或macOS12兼容已验证。

主要执行命令：

```bash
dotnet build tools/performances/AtomUI.GalleryPerformance/AtomUI.GalleryPerformance.csproj -c Release --framework net10.0 --no-restore
python3 tools/performances/run_native_motion.py --binary .artifacts/bin/Release/net10.0/AtomUI.GalleryPerformance --output-dir /tmp/atomui-animation-p0-p1/final-sidebar --scenario sidebar --processes 3 --warmup 5 --iterations 30 --sample-ms 450 --refresh-rate 60 --compare /tmp/atomui-animation-p0-p1/baseline-sidebar/summary.json
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~NavMenuRenderClockTests|FullyQualifiedName~NavMenuInlineCollapsedTests|FullyQualifiedName~NavMenuInlineMotionTests'
```

取证目录和临时测试默认清理，上面的原始数据路径用于标识本次实际命令，不是长期保留的输入。
稳定可复用入口是性能工具的原生模式和聚合脚本。

## 审查处置和残余风险

- 独立审阅未发现确定的产品回退；采样中间值校验与工作负载身份缺口均以反例红绿修正。
- Windows原生交互和显示present仍未验证，不能宣称开发者Windows环境的30FPS反馈已解决。
- CPU不作性能提升结论；实际present、原生pointer延迟和其它后端需要独立证据。
- Width/MinWidth验证以实际绑定恢复与最终布局为准。Avalonia的新绑定输入经过coercion，GetBaseValue不必是未coerce的原始输入；
  本轮不重构原有自动宽度/cache或引入动态目标弹簧。
- 精确NavMenu.cs结构审计为1文件/1控件，HEAD与当前均31项既有声明顺序问题，无新增；目录级检查器还遇到其它类型重复IsReadOnly的问题。
  本轮按P0/P1范围保留这些既有项，避免将整文件重排混入动效改造。风险为声明结构债务继续存在。
- 原有Headless默认LineEdit路径在本次修改前即出现route未找到超时；本轮验证新增原生入口，不将该旧路径作为成功证据。

临时测试与取证媒体已在判断后删除，发布验收注入已退出普通产物；清理后模块验证结果如下。

## 清理后的最终验证

- `python3 scripts/verification/test.py run --scope iterate --path src/AtomUI.Desktop.Controls/NavMenu/NavMenu.cs --path tests/AtomUI.Desktop.Controls.Tests/NavMenu/NavMenuInlineCollapsedTests.cs`：307项通过、零失败，20个测试类、1个测试项目，`iteration-passed`。未运行全量回归。
- 10个临时用例及专用helper删除，现有1个动效测试保留新增的首tick前反转断言，常驻测试数量净新增0。
- 普通RID与非RID Gallery/性能工具Release构建通过，零警告、零错误；两个普通Gallery程序集均不含临时AOT验收类型。
- 最终原生3进程侧栏对照与全场景12动作smoke通过；原生窗口按确切PID/window ID捕获并完成视觉审查。
- 媒体清理审计返回`clean`、`findings=[]`、`removed=true`、`audit_error=null`；没有取证二进制进入工作区或暂存区。
- 独立审阅指出的采样验证和场景身份问题已修正，反例与实际源码编译用例通过；未留下已知的本次产品回退。
- 用户已授权提交P0/P1；未创建分支/worktree。Windows/present及CPU收益结论仍遵循上面的边界。
