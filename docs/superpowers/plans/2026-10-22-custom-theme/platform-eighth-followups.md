# CT25 第七轮CI：Windows后续测试合同

> **For agentic workers:** 使用superpowers:executing-plans；沿当前feature/custom-theme与checkout，用户不使用subagent，不自动提交。

**Goal:** 处理40155448673的Windows完整11项失败，保留原布局／弹层／文化合同，并以实际Actions复验。

**Architecture:** 先核定测试观测量和有效平台组合，复用实际产品主题、输入与绘制；只有产品反例成立时才改产品。临时反例证明因果，清理后精确执行三个原测试模块。

**Tech Stack:** Avalonia12.1.4／Headless-Skia、现有xUnit、原Windows Actions TRX摘要、本机Mac诊断。

**Spec:** [任务](tasks.md)、[前轮审查](platform-seventh-followups.md)、[UI输入与滚动纪律](../../../engineering/contributing/agent-guidelines.md)。

## 已取得的实际平台证据

第七轮三NativeAOT及两Browser成功。Windows前六项目共1071项通过，其中Grid309全部通过，原W14不再失败；Desktop4128项中4117通过、11失败。新failed_test_results完整保存11项名称／消息／栈，complete=true。

失败分组：InputSuffixVerticalCentering两项、PopupMarginPlacement的csd=false八项、DatePanelAlgorithm星期标签一项。Mac／Linux当前结果另行回填；本机模拟trait不是原生Windows。

## Review Focus

- 几何断言必须在同一真实坐标系，不能比较各节点局部矩形中心后放宽容差。
- 原clear／reveal、suffix内容、尺寸与垂直居中合同保持；用较高的同类文案暴露坐标错误。
- Popup仍保持120×60内容、原margin、480×360窗口、CSD阴影、贴边及原flip断言。
- Windows源码ConfigureCsdStatus始终true，主题无Windows/csd=false模板；测试强制该无效组合不能冒充产品支持。非CSD模拟应选明确支持的主题平台trait，保留同一AtomUIWindow和几何，不声称测试了原生Linux。
- 星期算法消费提供的Culture.ShortestDayNames；Windows/NLS与ICU的en-US值不同。两字母场景应明确提供两字母文化输入，另保留周前缀空串与顺序断言，不改生产本地化。

### Task 1: 三组失败的有效反例与最小修正

- [x] 临时几何探针调用原AbsCenter，保留真实LineEdit／SearchEdit、文本与高兄弟内容，读取Transform和真实命中几何。旧局部中心反例RED，实际变换后的中心符合原0.5 DIP合同；不改变产品对齐。
- [x] Popup只改变OS主题trait，Windows／csd=false原组合无Overlay；同内容／窗口／margin在受支持的非CSD主题下正常创建并保持原贴边／flip断言。不添加产品Windows非CSD功能、不跳过case。
- [x] 星期给显式ShortestDayNames输入，另证明一字母输入也被原算法如实保留；现行en-US系统数据差异按原Windows日志记录。
- [x] 修正原测试观测／输入，保留全部原断言与case数量。删除临时源／TRX／专用产物后，三个模块精确选测；与C4暂未提交产品改动隔离核对，避免未提交实现影响本批CI结论。
- [ ] 保存具体提交快照供用户批准后，推送现有分支并运行实际Windows／Linux／Mac及两Browser验证。不发布NuGet。

当前只有源码、已安装Avalonia XML与Windows原栈的诊断，尚未声称这11项已修复。输入坐标API说明明确TransformedBounds.Bounds位于local coordinate space，原AbsCenter没有应用Transform。

## 验证、处置与审查

临时探针在原版有效RED：19项中8通过／11失败，3.8293s；两类输入的局部中心误差各6 DIP而实际变换中心符合0.5，Windows非CSD组合八项在Open找不到Overlay，两字母期望遇显式一字母数据失败。三组平台／文化正对照均通过。首个探针因不存在GetVisualRoot扩展编译失败，仅记准备失败；改为实际祖先检查后才计功能RED。

候选同轨迹19/19，3.4743s。AbsCenter应用TransformedBounds.Transform后再读取Y，不放宽容差；Windows非CSD headless case明确选Linux非CSD主题，仍是同一AtomUIWindow和原几何，不能称原生Linux或新增Windows非CSD支持；两字母顺序case提供明确Culture.ShortestDayNames，一字母正对照证明生产算法未被强制归一。

删除临时六项探针、两个环境注入和三份旧测试源备份后，暂时恢复四份C4产品文件为已提交基线，三个原模块精确filter共42/42，3.7842s；finally恢复C4工作内容，证明本批验证不依赖未批准C4产品实现。零新增永久测试，既有case数和原关键断言保持。保存文本日志／SHA，不保留临时测试源或专用媒体。

本批为作者自审，无子agent独立审查。源diff只涉及三个既有测试的坐标／有效主题／文化输入；不更改产品对齐、窗口平台支持或星期本地化。实际Windows最终结果仍须获批提交后Actions确认，不能把本机trait反例当Windows通过。

第七轮最终Mac选测6290项全通过；Linux前八项目5664项全通过（含Desktop4128／Grid309），其后OfficialPtBr_Package_Graph_Is_Consumable在600000ms总体超时，同项目另4项通过，完整摘要complete=true。没有证据据此改产品或延长超时，已对同SHA5c7c4a2cc的原Linux job请求一次复验；保留首轮失败，不把重试申请记通过。三NativeAOT和两Browser已通过。

提交计划：仅三个测试及本轮平台状态文档，在现有feature/custom-theme创建一个提交，推送并运行三OS／两Browser原验证；不包含C4临时测试、未完成绘制改动或产物，不发布NuGet。须用户审查明确批准后才执行。
