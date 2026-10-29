# CT13 CB2：禁用项文字色消费结果

唯一颜色产品改动为ComboBoxItemTheme的disabled Foreground资源Shared ColorTextDisabled→ComboBox Own ItemDisabledColor。保留原选择器条件、声明顺序、默认公式、身份和原生优先级，不新增字段/数据状态/代码绑定。初次截断读取误把HandleHoverColor列为缺口，完整源码及真实pointer正对照证明它已有正确模板绑定；没有修改手柄。

## 有效前后与UX

原版读取Own资源已是#d02088，实际禁用项Foreground却为#3f000000，反例失败；已有handle hover指针命中、IconBrush及真实可见箭头StrokeBrush正对照通过。三条目、两行高度约束的原版最终1FAIL2PASS，修复后3PASS、20秒。实际项和TextBlock前景一致，原状态/选择保持；Down/Enter跳过disabled，SelectedIndex=2。

完整明暗同轨迹、正常动效、400×220，默认/handle hover及其定制/恢复、popup打开、AA/1DIP/复位、disabled Own定制/清除、键盘选择/关闭。新旧24对只有disabled-configured的文字变化，明暗各402点，最大通道160/122；其余全部0。原/候选AA、扰动/两类定制恢复均0；1DIP4870/4900全检出。完整图已查看，未裁剪/对齐/遮罩或设置容差。

用户精确视觉批准见[视觉记录](input-combo-box-disabled-visual-review.md)。该批准不覆盖随后发现的专用Style路由行为，后者独立按CB3处理。

## 当前Mac与清理

产品Window实际managed/NativeAOT各42项、exit0：明暗实际variant、实际disabled项/TextBlock、连续两次Own换色与透明0alpha、有效Global默认输入、启用邻项不变、清除、同owner重挂和正常关闭。ILC为官方Avalonia12.1.4/net10.0；保留本机OpenSSL/Brotli五条deployment链接警告，不外推最低OS、其他Host、原生最终像素或原生键盘。

两份临时源码与目录删除，419文件原生工程/产物、1,375,614,294 bytes删除；48PNG+manifest共49媒体文件清理，audit clean。NET8 Release零警告/错误、23.79秒；颜色清理后的原ComboBox69PASS、0FAIL、0Skip、2秒，常驻测试净增0。

精确运行入口：

```bash
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.CustomThemeComboBoxDisabled
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~AtomUI.Desktop.Controls.Tests.ComboBox
dotnet build src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj --framework net8.0 --configuration Release --no-restore
```

## 专用样式验收接续

按计划用真实编译AXAML ComboBoxPopupListItemStyle做覆盖/撤回检查时，Foreground与不冲突Opacity同时未命中，未把它解释成颜色优先级限制。记录marker、实际逻辑/视觉树和selector后确认route与逻辑拓扑不符。该额外临时AXAML/C#属CB3，不能把此前69项绿灯当作路由正确；[CB3结果](input-combo-box-item-route-results.md)已完成四场景及managed/AOT各54项、清理后69项，CB2最后专用Style优先级门槛一并闭环。用户仅主agent，作者另步自审，不独立评审或提交。
