# CT13 AM1：AutoComplete / Mentions 七项默认协议结果

2026-10-29–30，现有feature/custom-theme，单agent、无新分支/worktree或提交。七项默认迁移专项完成；当前原生联合验证同时包含[AM2消费者修复](input-autocomplete-mentions-metrics.md)，其新视觉另审，不能算整个CT13完成。

## 协议与默认值

实际启动后的Registry描述符原为WholeEvaluatorV1，两项有效RED；原默认、Control Global、显式Own 0和清除两项正对照通过。最初资源探针误用只查当前节点的TryGetResource，改为带Own key的作用域TryFindResource后取得正确前证；该准备失败不算产品RED。

AutoComplete四项、Mentions三项迁移DependencyDefaultsV2、声明修订1及七个静态默认函数；保留属性声明、类型、ID/slot、公式与120/200常量，无Own依赖边。移除旧整表CalculateTokenValues和空构造，不新增字段/消费者/API。

70条明暗五阶段真实Compiler结果（默认、Control Global、小数/非均匀Own、0、清除），包含类型、key名称/数字ID、slot与格式化值，原版/迁移后/AM2联合源码全部严格相同。旧字面IDAutoComplete0–3、Mentions0–2在当前Mac也实际读取相同值。

声明Revision=1与运行EvaluatorRevision不同：后者是生成计划指纹，包含层修订、类型、字段/方法/输入；初期探针把它误断为1导致两项夹具失败，按现有ControlTokenDefaultModel指纹合同修正为V2与非零校验，未改生产算法或像素断言。最终协议/配置4项通过785ms，联合临时专项18项2s通过；Generator.ControlTokenDefaultContractTests既有1项通过1s。

## 隔离的默认绘制前后

仅两Token迁移、尚未AM2改消费者时，三种AutoComplete外观与Mentions的非聚焦输入、八项/三名义页、已打开popup、Global配置/恢复和关闭，明/暗局部Provider、正常motion、650×740同完整历史，共44张/22对新旧RGBA全0。各侧AA/标题1DIP/严格复位有效，作者查看明色普通与暗色配置关键画面。该0差异不表示原消费者缺陷不存在。

原生扩大断言实际发现默认Frame.Padding2、MaxHeight96而设计公式应100；源码未观察padding、候选行高也未消费Own。失败保留并转入AM2，没有删预算断言或放宽容差。AM2修复后同一原生预算断言通过。

## 当前Mac与结构

联合AM1+AM2原生工程引用当前源码，managed/实际Mach-O NativeAOT各543检查：基础463覆盖两真实Window/PopupRoot后端、七项旧字面ID/值、作用域更新、0、清除、padding/min/max与完整名义预算；补充四外观实际行内容/尺寸及编译专用Style覆盖/撤回。原生只证明资源/几何/API，不宣称raw输入或最终像素。

AOT无IL2026/IL3050，五条本机OpenSSL/Brotli部署目标警告保留。NET8 Desktop Release含依赖0警告/错误、17.52s。两Token成员门禁2文件/0Control/0问题。AM2两root另有独立结构与行为证据，不归入这两Token的源码改动。

## 验证与处置

```bash
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~CustomThemeAutoMentionDefaults.DescriptorTests
# AM1_STAGE/SNAPSHOT/PATHS为本包文字快照和项目外媒体映射；before/after完成隔离前后，joint完成联合值复核。
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~CustomThemeAutoMentionDefaults.SnapshotTests
dotnet test tests/AtomUI.Generator.Tests/AtomUI.Generator.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~ControlTokenDefaultContractTests
```

正式两控件implementation/token/changelog同步。联合清理已完成：AM1三临时源13,934 bytes、AM2五临时源22,339 bytes、3个checker临时fixture、重排脚本及原版源码备份均删除；共同原生工程427文件（1,216,025,157 bytes）删除。AM1的44PNG/manifest共45文件与AM2的116PNG/manifest共117文件均清理，两个audit clean。CT12六个临时源未触及，无本包进程。清理后精确AutoComplete/Mention/CandidateListBehavior共28PASS、0FAIL/Skip、1s，Generator默认合同1PASS1s；最终成员门禁4文件/2Control/0问题，git diff --check通过。常驻净增0；作者另步自审，无独立agent。字体、其余Global/Own/Part/状态、完整家族与其他平台继续。
