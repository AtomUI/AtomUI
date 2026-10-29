# CT13 / CT15 NU2-B：内部描边owner候选结果

状态：实现、本机专项及临时清理完成，[新厚度变化与六对自然hover差异](input-spinner-stroke-visual-review.md)已独立批准；不关闭完整家族或CT25。

## 根因与最小改动

ButtonSpinnerDecoratedBoxTheme与ButtonSpinnerHandleTheme的SpinnerBorderThickness默认直接读取Shared.BorderThickness，两公开owner的有效别名未到达内部布局/绘制输入。原四目标（独立/数值×两内部资产）4FAIL、1s；Standalone目标3与Numeric目标5均实际1。

唯一产品改动为两Setter改用已有OwnerTokenResource.BorderThickness，实际树解析正确：独立ButtonSpinner读取自身、内部NumericUpDownSpinner沿树归NumericUpDown，公开子IconButton的9不抢占内部资产。没有新字段、identity、converter、C#绑定、事件或绘制补丁。

既有完整Thickness输入及绘制取Left为线宽的算法保持；内部描边与根实例BorderThickness外框职责不混为同一属性。测试直接给BorderThickness别名，不把Disabled算法下LineWidth单值当作别名自动重推。Numeric inline两条硬编码分隔边未混入本包。

## 功能、输入与像素

候选四目标/0/清除1及完整帧5PASS、0Skip、14s；两种实际owner不串值。四尺寸、左右固定/浮动、明暗8实例40阶段，3/5厚描边下实际中心点击增减并回到12，原数值格式与内容保持。320根尺寸/值/文本记录前后相同，没有去掉高度、内容或关闭动效。

自然与独立直接绘制分开保留：直接绘制22非修复阶段旧新0，18配置阶段改变对应描边；自然默认hover另外六对Light14/19/23(max10)、Dark6/22/27(max9)。默认普通/AA/1DIP复位、清除、禁用及最终恢复旧新均0。所有精确计数见视觉审查，不用直接绘制覆盖自然差异，不借NU2-A或NU1批准建立通用容差。

已查看厚度3/5前后完整帧及0描边候选，原背景/内容/按钮命中保持。自然六对小差异已获本次独立范围批准，CT25绘制历史根因保持开放。

## 实际Mac / 构建 / 清理

产品NSWindow、managed/arm64 NativeAOT各196项、exit0，覆盖两个真实内部资产的均匀/非均匀/0/清除与数值保持。外部驱动通过已知AvaloniaProperty注册名的有限FindRegistered/GetValue只读观察，没有CLR成员反射扫描，也没有将内部属性变成公开API。它不冒充原生最终像素或原生点击。

官方Avalonia12.1.4；最终无IL2026/IL3050或启动图抑制，五条本机OpenSSL/Brotli部署版本提示保留。NET8 Release0警告/0错误、19.48s；两个AXAML的owner/默认/优先级/树拓扑及新增产物由作者另步自审，未更改产品C#控制文件，无独立评审。

两临时C#及目录、原/候选source快照、媒体映射删除；外部工程422文件、1,117,558,899 bytes删除；160PNG与manifest共161文件清理，audit clean。常驻测试净增0，无本次进程遗留。

清理后原ButtonSpinner/Numeric精确65PASS、0Skip、2秒；Gallery ButtonSpinnerShowCasePageTests精确1PASS、0Skip、15ms。git diff --check通过。无Git操作、新Actions或NuGet发布，全部原有未提交工作保留。

```bash
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.CustomThemeSpinnerStrokeOwner
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~AtomUI.Desktop.Controls.Tests.ButtonSpinnerTests|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.NumericUpDown'
dotnet build src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj --framework net8.0 --configuration Release --no-restore
```
