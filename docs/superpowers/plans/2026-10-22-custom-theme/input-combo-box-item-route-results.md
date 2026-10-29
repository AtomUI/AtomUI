# CT13 CB3：ComboBox popup.listItem 专用 Style 路由结果

## 原因与单一修复

CB2优先级检查用真实编译AXAML声明ComboBoxPopupListItemStyle，Foreground与独立Opacity Setter同时不命中。marker存在；完整树记录证明实际条目直接逻辑父为ComboBox、TemplatedParent为空，视觉父为VirtualizingStackPanel，PopupFrame在视觉祖先链。原route从PopupFrame继续查逻辑后代，拓扑不匹配。

唯一产品修复为ComboBox.SemanticParts.cs中该SelectorRoute改为 `> .semantic-popup-list-item`。复用生成器已有Child组合器，与ListBox容器模式一致；不改生成器、marker注入、ContractType、Multiple、RuntimeCreated、CrossVisualRoot或优先级。自动/显式容器均沿同一逻辑父匹配，范围不扩大到其他owner。未用查找节点后写Foreground/Opacity、Loaded事件或动态样式回退。

## 有效前后

编译AXAML确实使用生成的专用Style类、x:SetterTargetType=ComboBoxItem，以及Foreground=#119977、Opacity=.73；测试核对类和关键Setter。原四场景（明暗×显式/生成容器）4FAIL、22秒，实际Opacity1；候选4PASS、22秒，实际项及文本Foreground绿色、Opacity .73。Own更新时Style优先，撤回到当前Own/普通值且Opacity回到1，同一容器、选择及打开状态保持。

完整视觉数据与精确批准集中于[审查记录](input-combo-box-item-route-visual-review.md)。28对原→候选仅激活/更新时八对变化；其余20对、AA和1DIP复位全部0，四场景检错均有效。用户单独接受这八对，未建立通用容差、未沿用CB2视觉范围；无新的默认外观差异。

## NativeAOT与警告处置

当前Mac产品NSWindow、明暗四窗口，managed与NativeAOT各54项、exit0；加载同一编译AXAML专用Style，实际容器/文本、Own变化、撤回、同容器与正常关闭通过。首个URI加载夹具触发IL2026，未接受该入口；改成x:Class和Load(this)预编译入口后重新运行managed54及实际AOT54，最终IL2026/IL3050为0，没有警告抑制或动态加载回退。

ILC实际使用官方Avalonia12.1.4/net10.0/Avalonia.Base.dll；本机五条OpenSSL/Brotli较deployment12更新的链接警告保留，不外推最低OS。检查证明当前Mac的编译Style命中，不冒充所有Host、原生最终像素或原生输入验收。

## 清理和最终模块

- 两份临时C#/AXAML及目录、原/候选source快照、媒体映射删除，常驻测试净增0。
- 外部工程与产物334文件、793,208,343 bytes删除；56PNG和manifest共57文件清理，媒体audit clean。
- NET8 Release零警告/错误、24.89秒；单语义partial源文件语法/成员审计1文件/0自带注册control/0问题，源码属性及selector另步手工审查。
- 清理后原ComboBox69PASS、0FAIL、0Skip、2秒；原descriptor断言仅同步真实正确route，未删弱契约。
- 按用户单agent约束完成作者另步自审，没有独立评审、Git提交、新Actions或NuGet发布。

```bash
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.CustomThemeComboBoxDisabledPriority
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~AtomUI.Desktop.Controls.Tests.ComboBox
dotnet build src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj --framework net8.0 --configuration Release --no-restore
```

CB2的专用Style优先级门槛随此闭环。整个ComboBox/CT13、其他Global/业务状态、其他Host与CT25继续，不把本机切片写成家族完成。
