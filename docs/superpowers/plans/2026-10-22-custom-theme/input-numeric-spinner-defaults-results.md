# CT13 / CT15 NU1：ButtonSpinner 默认候选本机结果

状态：源码、专项、独立精确视觉批准、临时清理及最终模块完成；[批准范围](input-numeric-spinner-visual-review.md)不扩大为完整家族或CT25验收。

## 改动与字段合同

只修改ButtonSpinnerToken.cs：8历史字段接入DependencyDefaultsV2 rev1、8静态默认函数、FilledHandleBg声明HandleBg依赖并接收有效Own参数，继续原ColorUtils.OnBackground算法。删除空ctor/whole evaluator，不修改模板、绘制、输入同步、尺寸、动效或Global机制。所有名称/类型/key-ID-slot和明暗16条默认相同。

ControlWidth经完整产品/模板/Gallery引用搜索只有声明/default，无内置宽度消费，按现有兼容保留策略归Legacy。保留90默认、资源/config读写及编号，不映射实例Width；Native实际0与清除恢复也通过。文档纠正把它当成当前内置宽度入口的表述，没有为历史字段增造行为。

## 失败与消费证据

原首轮4FAIL/2PASS、716ms：旧V1、两主题有效Own上游未进入Filled、透明上游未进入Filled；显式Filled覆盖及实际手柄正对照已通过。新增双透明输入后原5FAIL/3PASS、15s，包含完整原版帧轨迹；候选8PASS、16s，终值硬检查/逐阶段AA扩展后8PASS28s，独立直接绘制版本8PASS29s。

固定有效Global.ColorFillSecondary=#80000000和显式HandleBg=#c86432，独立按byte舍入期望#ff643219；原灰色，候选资源及ButtonSpinnerHandle.Background均命中。透明HandleBg产生#80000000，双透明产生透明；显式Filled=#663399优先。实际上下箭头7、NumericUpDown Spinner加减图标14保持；12实例的44阶段共528根/动作/偏移透明度/变换记录相同。

真实Headless/Skia，900×530、明暗、4尺寸、左右手柄/浮动输入/inline数值、4行实际hover与两次Up/Down到达及数值往返。键盘后两版本都将12显示为12.0，记录真实文本，未将此既有格式行为当成主题恢复失败。

## 像素调查及未闭合项

原自然捕获的默认hover及恢复小像素差异保留。只读强化实际过渡基值检查和逐阶段AA后，原1PASS30s、候选8PASS28s，逐阶段AA均0；跨完整轨迹仍有14对小差异。记录到的显式过渡输入/几何一致，没有修改clock、关闭motion、去掉尺寸或强制重画。

同一候选从头复跑1PASS26s，也出现2–34像素/最大通道11的自然差异，不能把它们单独归因于V2。独立直接绘制、同完整历史，26非依赖变化阶段旧新均0、18阶段只发生对应背景变化，逐阶段AA均0。该结果不替代自然合成器，相关完整绘制历史/增量绘制根因仍归CT25开放；不把不同backend混作同一像素基线。

所有原始/强化/同版复跑/直接绘制计数均保留为文本。18背景变化及14对自然差异见独立视觉审查，不沿用CB4批准，不设通用容差。已实际查看主要背景修复前后完整帧，默认值与动作尺寸另有真实属性与几何记录。

## 当前Mac / NET8 / 审查

实际产品NSWindow、managed/arm64 NativeAOT各246项、exit0；覆盖16明暗默认、8字段显式/0/清除、3种Own alpha输入、8个实际手柄背景、两种模式7/14动作尺寸及已有数值保持。只证明当前Mac资源/模板/API路径，没有宣称原生最终像素或原生键盘通过。

ILC明确使用官方Avalonia12.1.4/net10.0/Avalonia.Base.dll，无IL2026/IL3050或启动图抑制；五条本机OpenSSL/Brotli部署版本链接警告保持，不外推最低OS。NET8 Release0警告/0错误、20.98s；单Token完整成员审计1文件/0自带注册control/0问题，作者另步核对默认运算/Own参数/新产物，没有新helper、运行时字段或订阅。

## 清理与最终模块

两临时C#、原/候选源码快照及媒体映射删除，净新增常驻测试0；外部原生工程422文件、1,117,804,821 bytes删除；564PNG与manifest共565文件清理，audit clean。原有其他改动全部保留，无Git操作。

清理后精确ButtonSpinner/NumericUpDown原65PASS、0Skip、2s；Gallery ButtonSpinnerShowCasePageTests精确1PASS、0Skip、17ms。不运行local change/full或全量回归。

```bash
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.CustomThemeNumericSpinnerDefaults
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~AtomUI.Desktop.Controls.Tests.ButtonSpinnerTests|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.NumericUpDown'
dotnet build src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj --framework net8.0 --configuration Release --no-restore
```

Shared入口、所有Own状态/Part/完整Custom、其他Host和完整CT13/CT15/CT20/CT25保持开放。审查仅作者独立步骤，无subagent、提交、推送、新Actions或NuGet发布。
