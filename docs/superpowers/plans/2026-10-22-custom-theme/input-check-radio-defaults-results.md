# CT13 CR1：CheckBox / RadioButton 有效默认依赖结果

## 实现与合同

承接[执行计划](input-check-radio-defaults.md)，CheckBox四字段、RadioButton七字段接入DependencyDefaultsV2 rev1。CheckedMarkSize依赖有效CheckIndicatorSize，DotSize依赖有效RadioSize、DotPadding及Global.LineWidth；保留显式下游、0、清除、三态标记独立字号公式及原颜色/间距默认。默认值、名称、类型、key、公开枚举值及slot的明暗22条记录完全相同。

RadioIndicator在既有唯一派生函数的调用条件中补充RadioSize、DotSizeValue、DotPadding三个输入。实际Render继续使用RadioDotEffectSize；启用选中、禁用选中和未选中三个公式不变。不新增缓存、旗标、订阅、API、绑定或主题选项；算术无法由TemplateBinding表达的原因留在计算函数注释中。

## 前后证据

原版五项反例全部失败：CheckIndicatorSize=30仍得到9.6而非18；RadioSize=30、DotPadding=2时实际绘制输入仍为6；显式DotSize=0/12也不能实时到达Render。仅迁移Token后Check通过，Radio四项仍失败；补齐三项输入失效后全部通过。最终同轨迹截图与功能合计六项通过，未Skip。

使用真实Avalonia Window、公开ThemeConfigProvider/ThemeConfigBuilder配置。现有选择、Semantic Part与组绑定合同另由最终精确模块检查覆盖；没有代码获取语义部件后设置样式。当前Mac实际NSWindow的managed及Mach-O arm64 NativeAOT各51项通过，覆盖有效Own、显式下游/0/清除、未选中及禁用计算、恢复、真实模板替换和重挂。ILC实际引用官方Avalonia12.1.4/net10.0/Avalonia.Base.dll；这类资源/模板消费者检查不等于原生像素或原始键盘输入验收。

精确运行入口：

```bash
# 临时阶段，清理前运行；临时源码现已删除
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.CustomThemeCheckRadio
# 清理后的最终模块
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~AtomUI.Desktop.Controls.Tests.CheckBox|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.RadioButton'
dotnet build src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj --framework net8.0 --configuration Release --no-restore
```

## 视觉与诊断边界

自然/直接完整帧、实际scope、输入历史、原始失败及用户精确批准集中记录在[视觉审查](input-check-radio-visual-review.md)。直接绘制的原版/候选AA、复位及往返均0，1DIP全部检出；配置目标变化1178像素/主题。自然配置原→候选1420/1374，恢复90/76及候选自身3/6已获精确批准；不能把这些原始FAIL改成0。

实际图标Render初始/恢复输入一致；背景surface完整重绘可令候选3/6变0，定位到合成/重绘边界，尚未证明框架中具体故障语句。所有诊断性的Icon埋点、DesiredSize登记和合成器选项均已撤回，正式产品没有刷新补丁，官方依赖保持。该问题归CT25继续，不声明已修复。

## 清理、验证与审查

- 两份临时源码及目录删除，常驻测试净增0；全部调试选项/源码快照清理。
- 外部原生工程及全部产物423文件、1,378,537,051 bytes删除；无遗留本次原生进程。
- 268PNG与manifest共269文件删除，媒体audit clean；不保留媒体链接。
- 清理后CheckBox/RadioButton原51项通过，0FAIL/0Skip，测试耗时1秒。
- NET8 Release构建0 warning/0 error，20.43秒；成员审计明确三个产品文件、一个控件，0问题；diff clean。
- 按单agent约束完成作者另步自审：依赖顺序、显式存在位、所有状态公式、attach路径、主题绑定边界与原有选择行为均核对。没有独立评审或Git提交。

本机有效默认机制切片完成。完整Global/Own/语义部件/业务状态、其他Host、原生最终像素和完整CT13/CT25仍开放；没有以此分包回填整家族完成。

## RadioBgColor 兼容分类

完整搜索产品源码、模板、Gallery和测试，RadioBgColor只有声明和默认函数，内置模板读取的是Global ColorPrimary、ColorBgContainer及ColorBgContainerDisabled。沿已有历史字段分类规则将其归为Legacy/CompatibilityRetained；名称、Color类型、key/ID/slot、V2默认、配置及自定义资源读取能力保留，不新增选中背景行为或Global别名。CT23应显示历史保留边界，不能把它当作有效内置视觉编辑项。

本裁定仅同步文档与字段目录，产品源码没有额外变化；不新增镜像源码或文案的测试，不生成新媒体。前述清理后51项、NET8与成员检查对应的产品输入保持不变。分类已处置，其余Global消费和家族验收继续。
