# CT12 接手后的最终验证与出口

**状态：CT12已完成并关闭，CT25独立保留。** 2026-10-30，用户已明确回复“批准本表限定范围，保留 CT25”（request_user_input_async / call_a2fe796a8df0481aadd7e4393060b87e / item 0）。公开BoxShadow工程、默认原生验证、清理后189项及限定视觉批准已全部完成，CT12在既定批准范围完成并关闭。原始24/max1、两对3968/max2及其他既定历史仍按CT25保留，不设通用容差，不扩大其他平台/背景/恢复模式，无Git授权。


2026-10-30。本会话按用户明确指令接手CT12，沿feature/custom-theme，不创建分支/worktree、不用agent、不提交。公开BoxShadow已由用户选定并实现，133Own保持；本轮生产代码修改0，CT13/CT11输入保护。

## 真实剩余项及完成情况

原会话没有落盘V11最终原生结果，不能将“采样结束”当通过。沿用有效2RED、20功能及默认18旧新对0记录；当前20功能832ms再次通过。原公开BoxShadow注册、编译根VariantShadow默认、既有ShadowsFrame TemplateBinding三路径正确，显式空值/Style/local/清除/当前主题/模板/重挂与GC均通过，无新Own或Part。

核对本地Avalonia参考源码时按规定路径找到/Users/chinboy/Projects/ReferenceProjects/Avalonia；以包匹配2d79ba55fae1b1e71c69f93a668080c9029e6a31版本读取默认，未改参考源码/依赖或缓存。发现旧原生驱动显式UseRegionDirtyRectClipping=true，与产品默认不一致，最终驱动仅移除该选项后重建两runtime，保留全部窗口/数据/动作。旧宿主结果不冒充当前默认原生证据。

## 当前真实原生

实际NSWindow、RenderScaling2、960×500 DIP、30个Button/Dropdown/Submit/Reset/Dialog实例、每类六role，正常motion；两runtime分别执行Light/Dark的A/A/1DIP/恢复/编译Style/清除，全程原实例。

- managed1236、NativeAOT1236检查，均exit0；每runtime26帧，共52帧。
- 每runtime14阶段内配对均0；AA/检错恢复/清除六对均0；1DIP正对照80382/85581像素。
- managed/AOT十二完整阶段逐RGBA0；配置650547/601191像素，分别max205/111，两后端一致。
- 每阶段实际输入恒定None|active:False，按下0，未强制damage或操作桌面指针。
- managed实际复制的产品DLL与本次源码构建SHA完整相等；AOT当前源码重建，无IL2026/IL3050。五条macOS26 OpenSSL/Brotli对最低链接12提示保留，不宣称其他系统验证。

## Skia与原始失败

本轮3条完整同输入绘制复核PASS8s，实际像素重新计算：自然配置5390/max231、清除24/max1；baseline恢复0，full-damage独立对照恢复0。24坐标和旧失败保留，不把属性相等写成像素0。原明暗33实例/四档/三模板/颜色变体18对默认旧新0证据保持。关键当前原生及原Skia配置图已查看。

旧混合宿主两对3968/max2及其144记录不改写；早期13529/max17与238079/max199采样/全局主题历史继续CT25，未扩大限定范围。见[具体视觉审查](buttons-box-shadow-final-visual-review.md)。

## 家族及交付

既有九owner、128有效字段+5兼容、156业务/消费检查不重做；V11是另行发现并现已接通的公开入口，未假借Own计数关闭问题。14metadata/133Own在最终范围对账。Split Custom宽140属CT20；CT25的框架/其他平台/合成历史仍保留。

接管的六临时源23264 bytes已删除，Temporary当前无文件；外部工程/preimage/驱动441文件1,105,918,557 bytes已删除。新52原生+9Skia帧及manifest共62文件、旧58图及manifest59文件，共121取证文件清理，两audit均clean；保留文字/hash，不链接已删除媒体。常驻测试净增0。

清理后精确Desktop125+47=172项（按钮家族、Option Group、独立身份、Dropdown与Dialog直接继承），Gallery16项、键盘焦点渲染1项，共189PASS/0Skip；NET8 Release0警告/错误21.33s；Button全文件成员1/1/0，diff检查通过。功能和清理后源码核对确认生产/test/Gallery接手后无修改，差异仅六个临时源删除；CT13及CT11既有成果保持。

作者另步复核公开合同、133Own/14metadata、模板可达性与优先级、明确空值/清除、产物/默认渲染配置、真实原生像素及输入、临时清理；未发现剩余工程阻断。按用户约束未委派独立agent，不把作者自审称为独立同行审查。

[最后限定视觉表](buttons-box-shadow-final-visual-review.md)的配置效果、Skia24/max1和旧混合宿主两对3968/max2已获明确限定批准，无剩余CT12开发/清理/审查门槛。早期其他失败仍CT25，表内接受不扩成通用容差。禁止全量回归、自动提交和把旧产物替代本次产物。

```bash
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Buttons.|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.OptionButton'
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --no-build --filter 'FullyQualifiedName~AtomUI.Desktop.Controls.Tests.RadioButton.OptionButtonGroup|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Theme.ButtonFamilyTokenIsolationTests|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Theme.ButtonThemeCrossPlatformAssetTests|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.DropdownButtonControl.|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Dialog.DialogButtonBoxTests'
dotnet test tests/AtomUIGallery.Tests/AtomUIGallery.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~ButtonShowCasePageTests|FullyQualifiedName~DropdownButtonSemanticPartHighlightTests|FullyQualifiedName~SplitButtonSemanticPartHighlightTests'
dotnet test tests/AtomUI.Desktop.Controls.Rendering.Tests/AtomUI.Desktop.Controls.Rendering.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~ButtonFocusVisualRenderingTests'
dotnet build src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj --framework net8.0 --configuration Release --no-restore
```
