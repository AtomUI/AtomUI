# CT13 S6：Select 候选项字体、最小行高及三行预算结果

2026-10-28；当前已有 feature/custom-theme，HEAD 346732602。此为本机分包，不关闭 CT13/CT25 或完整输入家族。

## 契约与根因

公开 OptionFontSize 的实例值应优先于主题默认并进入真实候选文字。Own.OptionHeight 控制候选项最小行高；DisplayPageSize 继续是可视行数。原版四尺寸8项有效失败、1项正对照通过（1s）：实例27实际仍读Own21；Own行高54实际被继承尺寸触发器覆盖为32。完整原版轨迹自然14s、直接16s。

五个产品文件：根主题从 Own 提供 OptionFontSize 默认；列表与候选项通过 AXAML 投射字体；MinHeight匹配继承尺寸触发优先级。列表沿现有布局读取首个已实现、可见、非分组行的 DesiredSize；行Bounds高度变化通知所属列表，更新既有ItemHeight。没有新事件订阅、字段、缓存、额外源枚举或容器创建。代码只计算实际布局度量，ControlTheme不能表达该运行时聚合；字体绑定全部在主题。

初版10项通过不能证明清除正确；新增恢复检查实际失败（viewport72而非96）。只读DesiredSize仍失败，完整失败日志保留。行高度通知后的恢复、重开及10000条源验证通过；不靠延迟、容差或关闭布局掩盖。成员重排与行为差异分别记录，两个文件审计2/2/0问题。

## 实际验证

临时入口：tests/AtomUI.Desktop.Controls.Tests/Temporary/CustomThemeSelectMetrics；均已真实编译和执行。

- 原版精确过滤：`dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.CustomThemeSelectMetrics`；最初8失败/1正对照。
- 最终带 `CT_METRICS_MEDIA`、`CT_METRICS_TEXT`、`CT_METRICS_STAGE=after-direct`、`CT_METRICS_CAPTURE=direct` 的同过滤：12通过、0失败/跳过，本轮26s。含字体四档、行高四档、专用AXAML Style覆盖/撤回、四次自然/清除往返、关闭重开、10000条源的已实现容器数<32及滚到底。
- 原生独立 MetricsNative 工程引用当前产品源码和同AXAML；实际NSWindow、公开 IsPopupPinnedOpen、明暗四尺寸和字体/行高/专用Style/恢复：managed1442、NativeAOT1442检查。AOT本轮再次实际运行返回 PASS1442；不等于原生像素或raw输入。
- `dotnet publish <临时工程>/MetricsNative.csproj -c Release -f net10.0 -r osx-arm64 -p:PublishAot=true -p:SelfContained=true` 成功。链接器报告本机brew库目标系统版本警告，不能说该publish零警告。
- `dotnet build src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj --framework net8.0 --configuration Release --no-restore`：0警告/0错误，23.64s。
- 成员审计：`python3 .agents/skills/atomui-control-optimization/scripts/check-member-layout.py --path src/AtomUI.Desktop.Controls/Select/SelectCandidateList.cs --path src/AtomUI.Desktop.Controls/Select/SelectCandidateListItem.cs`：2文件/2控件/0问题。

## 像素与实际几何

两后端各60对，24对零差异、36对配置变化，计数逐对相同，详见[限定视觉审查](input-select-option-metrics-visual-review.md)。60条根位置/尺寸、条目数、尺寸档位及选择状态记录一致。真实框架鼠标中心点击后选择第二项并关闭弹层；没有据此声称左/右命中、滚轮、链式滚动全部验收。

同版本A/A、1DIP检错、恢复计数如下；每次两侧轨迹一致，没有更新基线或通用容差。

| 后端/阶段 | 主题 | A/A像素/最大差 | 1DIP像素/最大差 | 恢复像素/最大差 |
| --- | --- | --- | --- | --- |
| before | Light | 0/0 | 8651/252 | 0/0 |
| before | Dark | 0/0 | 4998/200 | 0/0 |
| after | Light | 0/0 | 8651/252 | 0/0 |
| after | Dark | 0/0 | 4998/200 | 0/0 |
| before-direct | Light | 0/0 | 8657/252 | 0/0 |
| before-direct | Dark | 0/0 | 5004/200 | 0/0 |
| after-direct | Light | 0/0 | 8657/252 | 0/0 |
| after-direct | Dark | 0/0 | 5004/200 | 0/0 |

## 去留、清理与边界

临时专项默认删除，不晋升常驻；现有Select数据/交互/模板/语义样式保障继续复用，净新增常驻0。四个临时源已删除；原生工程426文件（1,208,040,318 bytes）、媒体241文件已删除，audit clean，无遗留进程。清理后实际 `SelectControl` 命名空间71通过、0失败/跳过、4s；命令为 `dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~AtomUI.Desktop.Controls.Tests.SelectControl.`。此前宽前缀85项混入SelectionSemantics，另一次误写Select.为空匹配；两者不替代这次精确模块结果。最终diff检查通过。无新增公开API、Token、Part或Gallery调用形态；现有Gallery字段不需新增示例。文档同步implementation/token/changelog。

用户已明确批准表内36对限定视觉；分组/可变模板、其他配置、原生像素和输入、完整家族以及跨平台仍开放。作者分开自审，遵守单agent约束；没有新commit/分支/worktree或发布。
