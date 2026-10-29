# CT13 CR3：CheckBox / RadioButton 有效 Global 消费结果

## 实现与边界

[计划](input-check-radio-global.md)的实际改动为六个AXAML、38处资源入口。CheckBox、RadioButton的已有文字色、状态色、圆角及动效/水波开关分别读取自身typed Global；CheckBoxGroup、RadioButtonGroup的既有动效和SpacingXS分别读取自身typed Global。内部指示器的BorderThickness/默认开关使用现有OwnerTokenResource，仍归公开控件。

没有新增API、字段、identity、资源编号、优先级引擎、C#订阅或非Visual绑定。内部CheckBoxItemsControl已有Group→内部列表→WrapPanel的模板绑定，实测有效，故保留其独立默认。既有TokenTransitions已经把时长标记转换为owner绑定，实测75ms有效，未重复修改。字体继承、组选择和所有原Setter条件保持。

## 有效前后验证

原8项颜色、开关和组间距反例失败。最初两项线宽断言错误地预期Disabled控件算法会重算BorderThickness，记录保留且不计有效RED；依据ThemeCompiler正式分支，显式启用Global算法后，在原指示器主题上重新取得两项有效失败。候选最终15项通过，加入线宽0/恢复的最终14项消费者检查亦通过，0Skip；另一个用例为完整视觉轨迹。

验证包含四个公开owner、兄弟身份隔离、配置清除、根LocalValue优先级及清除回落、false开关、实际指示器厚度3/0/1、实际WrapPanel间距、既有75ms时长。配置过程订阅ThemeChangeFailed并抛出错误，不能用配置提交失败制造消费反例。

```bash
# 临时阶段，现已清理
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.CustomThemeCheckRadioGlobal
# 清理后最终模块
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~AtomUI.Desktop.Controls.Tests.CheckBox|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.RadioButton'
dotnet build src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj --framework net8.0 --configuration Release --no-restore
```

## 视觉与当前Mac原生消费者

独立自然合成、直接绘制和交错诊断分开记录，完整数据见[精确视觉审查](input-check-radio-global-visual-review.md)。默认/输入原→候选均0；独立自然配置明暗各5015像素，直接配置5020/5009，独立自然恢复各3像素。直接恢复均0，1DIP明暗4725/4700全检出。用户单独批准本次配置生效及三点精确差异（call_d5a1b537ebc04beb801aff762a1dc76d:0），原始同版本失败及计数继续保留CT25，不继承CR1批准或建立容差。

当前Mac实际NSWindow、scale2、410×300，managed与Mach-O arm64 NativeAOT各54项，exit0。公开ControlTokenAccessor与精确身份配置、实际明暗、最终文字/选中色、厚度、动态时长、组间距、四owner动效、清除、相同实例重挂与选择保持通过；未用internal ThemeScope或反射读取证明原生消费。工程准备期的路径别名/using/内部成员误用编译错误均排除出产品RED，最终只用公开API。

ILC实际输入为官方Avalonia12.1.4/lib/net10.0/Avalonia.Base.dll；本地OpenSSL/Brotli产生五条macOS26 dylib与deployment12链接警告，未外推最低OS或其他Host。原生检查证明资源与模板消费者，不等于原始键盘输入或最终原生像素验收。

## 清理与审查

两份临时源码与目录删除，净增常驻测试0；临时主题快照和媒体映射删除。外部原生工程与全部产物419文件、1,375,895,541 bytes删除；108PNG与manifest共109媒体文件清理，audit clean，两个原生执行均exit0。NET8 Release构建0 warning/0 error，18.96秒。清理后原CheckBox/RadioButton模块51PASS、0FAIL、0Skip，测试耗时1秒。

按单agent约束完成作者另步自审：实际owner路由、typed与内部资产边界、已有模板绑定、原生优先级、时长已有消费、算法Disabled/Global职责和无刷新补丁均核对；没有独立评审。C#产品输入未在本包变更，CR1三个产品文件/一个控件的成员审计0问题仍有效。

本包仅完成上述Global入口。字体/其他Global、完整业务状态、持续动效、其他Host、原生最终像素及完整CT13/CT25保持开放；资源绑定沿用已有Control宿主生命周期，本包未新增宿主或订阅机制，未以重挂检查冒充GC证明。无提交、无新Actions或NuGet发布。
