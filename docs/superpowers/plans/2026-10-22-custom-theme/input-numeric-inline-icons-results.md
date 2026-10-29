# CT13 NU2-A：NumericUpDown 加减图标候选结果

本机实现/验证及清理完成，[四对图标变化与四对自然差异](input-numeric-inline-icons-visual-review.md)已独立批准；完整家族/CT25保持开放。

## 单一入口修复

仅NumericUpDownSpinnerTheme.axaml的两个加减IconButton各IconWidth/IconHeight，共四处Shared.IconSize替换为已有NumericUpDownTokenResource.IconSize。组合几何由父Numeric控制；公开IconButton的颜色identity保持。没有新Own字段、Control identity、C#绑定、模板拓扑、分隔线或输入算法改动。

OwnerTokenResource会先命中公开目标IconButton自身，不能在这里盲用；明确Numeric typed入口符合父模板赋予尺寸的职责。Input上下箭头仍ButtonSpinnerOwn.HandleIconSize=7，Spinner加减默认14保持。

## 前后与真实输入

四尺寸原4FAIL/1正对照PASS、878ms，实际加减IconWidth14而Numeric有效IconSize22；候选6PASS、0Skip、6秒。除了宽高，实际Icon.Bounds也22；0与清除恢复14、同一实例/值、独立IconButton35、子颜色identity均通过。Input模式7正对照保持。

真实Skia620×510，Light/Dark×4尺寸×2模式，20主帧同完整历史；实际按钮中心点击增减并回到12，原格式/焦点流程保持。160条根尺寸/值/文本记录及320条动作命中宽高/颜色前后相同；不通过去掉高度、内容、控件或关闭motion避免变化。

新旧默认/AA/1DIP复位、禁用及最终恢复均0。四对22/0配置图标变化符合目标；自然清除/点击恢复另有Light11/7(max1)、Dark21/10(max2)，原计数保留。独立实例、同完整轨迹直接绘制：配置四对分别432/272像素，其他16对均0。两backend分开表述，不用直接绘制覆盖自然差异，不设通用容差。

已实际查看配置22和0的完整候选画面。完整精确表及独立批准范围见视觉审查；未沿用NU1任何差异许可。

## 当前Mac与构建

实际产品NSWindow，managed/最终Mach-O arm64 NativeAOT各454项、exit0：4尺寸/2模式/两个主题、实际glyph22/0/18/14和Input7、方形命中区、公开子控件颜色scope、独立IconButton35及既有值保持。该原生专项是资源/模板/API/布局路径，不冒充原生像素或原生点击；真实点击由Headless/Skia验证。

官方Avalonia12.1.4；无IL2026/IL3050或启动图抑制，五条本机OpenSSL/Brotli部署版本提示保留。NET8 Release0警告/0错误、23.53s。产品C#控制文件未改，AXAML默认/优先级/实际owner及新增产物由作者另步自审，无独立评审。

## 清理与范围

两临时C#、原/候选source快照、媒体映射删除，常驻测试净增0；原生工程422文件、1,117,480,480 bytes删除；80PNG和manifest共81文件清理，audit clean；清理后原NumericUpDown精确49PASS、0Skip、2秒。

```bash
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.CustomThemeNumericInlineIcons
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~AtomUI.Desktop.Controls.Tests.NumericUpDown
dotnet build src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj --framework net8.0 --configuration Release --no-restore
```

分隔边/其他Shared入口、全状态/Part/Custom/其他Host及CT25继续，没有Git操作、新Actions或NuGet发布。

文档同步另纠正旧token.md中不存在的NumericUpDownToken继承链和Own字段；当前源码无独立Numeric Own，Global入口及显式ButtonSpinner Own复用保持，没有运行时API新增/删除。
