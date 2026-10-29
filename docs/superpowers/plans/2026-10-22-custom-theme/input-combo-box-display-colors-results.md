# CT13 CB4：ComboBox 显示区颜色候选结果

状态：实现、本机专项、独立限定视觉批准、临时清理与精确模块验证已完成；[具体批准范围](input-combo-box-display-colors-visual-review.md)不扩大为完整家族验收。

## 最终改动

产品仅修改ComboBoxTheme.axaml：根PlaceholderForeground默认读取ComboBox有效Global.ColorTextPlaceholder，普通占位用TemplateBinding消费公开画刷并移除固定Opacity=.3；Filled/Borderless的两类错误/警告文字规则加:not(:disabled)。可编辑输入的既有公开占位转发保持。没有新字段/身份、事件/代码回退、生成器改动或根禁用Foreground Setter。

普通禁用文字原本已由G2共享frame正确消费ColorTextDisabled；根Foreground getter继续原合同。最初把根getter当作最终禁用文字的两项期望不成立，已从实际消费者记录纠正，原始日志保留。初始多余根Setter已删除，最终260条根记录中的geometry/文本/选择/状态和根Foreground getter前后完全一致。

## 有效失败与通过

- 最初5FAIL/2PASS包含两项无效的根getter消费假设；不把它们计为修复证明。
- 修正为实际字形/原根getter合同后，在原产品主题下6FAIL/4PASS、800ms：两种模式的有效Global占位、普通公开画刷、两种variant的禁用错误文字、编译专用Style撤回。
- 原产品另外两项Warning真实文字2FAIL、662ms。先确认这些失败，再保留候选四条状态范围修复。
- 最终12项临时合同加完整帧采样共13PASS、0FAIL、0Skip、9秒。专用ComboBoxPlaceholderStyle以预编译AXAML声明，断言专用类、Foreground=#663399与Opacity=.8两个Setter，实际命中/覆盖/撤回到公开画刷；无Name/Loaded事件属性回退。此项验证的是普通显示区专用Part，没有据此宣布全部状态替代节点验收。

两侧真实Skia/640×570/Light-Dark各10实例，同完整轨迹26帧；精确像素表与颜色职责见视觉审查。AA、1DIP复位、启用恢复、配置清除均0，所有1DIP检错有效。默认占位及明确颜色修复有预期旧新差异，已获用户本次独立批准，不沿用CB2/CB3范围。

## 当前Mac与NET8

最终候选的实际NSWindow、managed与Mach-O arm64 NativeAOT各108项、exit0，使用同一预编译专用Style和公开ControlTokenAccessor。实际默认/作用域占位、公开画刷覆盖/清除、专用Style及撤回、禁用校验文字、原根getter与选择保持通过。原生资源/API路径验证没有替代原生最终像素或真实键盘验收。

ILC输入明确为官方Avalonia12.1.4/lib/net10.0/Avalonia.Base.dll；无IL2026/IL3050或启动图抑制。五条本机OpenSSL/Brotli部署版本链接警告照实保留，不推断最低OS。NET8 Release 0警告/0错误、18.47秒；AXAML拓扑/优先级/新增产物及最终diff由作者另起步骤自审，C#产品控制文件未新增或改写。

## 清理和最终模块

四份临时C#/AXAML和其目录、原/候选源码快照、媒体映射均删除，常驻测试净增0；原生工程430文件、1,207,910,367 bytes删除；133PNG及manifest共134文件清理，media audit clean。没有本次进程或媒体遗留。

清理后原ComboBox69PASS、0Skip、2秒；Gallery ComboBoxSemanticPartHighlightTests及ComboBoxShowCasePageTests精确8PASS、0Skip、5秒。git diff --check通过，所有原有未提交改动保留，没有提交、推送、新Actions或NuGet发布。

```bash
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.CustomThemeComboBoxDisplayColors
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~AtomUI.Desktop.Controls.Tests.ComboBox
dotnet test tests/AtomUIGallery.Tests/AtomUIGallery.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~ComboBoxSemanticPartHighlightTests|FullyQualifiedName~ComboBoxShowCasePageTests'
dotnet build src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj --framework net8.0 --configuration Release --no-restore
```

## 明确未关闭

根Local Foreground在禁用时可保留getter，但原有frame状态会覆盖最终文字，原→新均存在；这是独立待修的可见优先级缺口，不描述成已支持。可编辑替代节点的专用Part、其他Global/状态/完整家族与CT25继续。仅作者自审，无独立评审。

后续NumericUpDown/ButtonSpinner入口已完成[只读源码核对](input-numeric-spinner-audit.md)，没有把候选Shared引用直接判为Bug或实施新消费者。
