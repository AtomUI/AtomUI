# CT13 CB3：popup.listItem 专用 Style 命中审查

原生成的ComboBoxPopupListItemStyle不能命中实例：Foreground未变，独立Opacity Setter也仍1而非0.73。条目marker存在，实际逻辑父直接为ComboBox，TemplatedParent为空；PopupFrame只在视觉祖先链。唯一产品修复把SelectorRoute从 `/template/ .semantic-popup-root >> .semantic-popup-list-item` 改为 `> .semantic-popup-list-item`，保留ContractType/owner/Multiple/RuntimeCreated/CrossVisualRoot。未改通用生成器、优先级或追加代码设置目标属性。

## 编译AXAML与完整历史

临时AXAML在owner class `cb2-priority` 下声明生成的专用ComboBoxPopupListItemStyle，x:SetterTargetType为ComboBoxItem，Foreground=#119977、Opacity=0.73。测试检查编译后实际专用类和关键Setter值，无Name/Loaded/Unloaded代码回退。

当前Mac、Skia scale1、400×220、实际明暗×显式/自动生成容器四场景。每侧同一个用例独立进程，正常动效：popup默认→AA→1DIP→复位→激活owner class→Own更新→撤回class。Style激活期间前景与文本保持绿色，Opacity .73；Own更新不越过Style，撤回后读取当前Own（显式disabled容器为#2233aa）或原普通值，Opacity恢复1。同一容器、选择和开合保持。

## 精确视觉变化

| 场景 | styled / updated 原→候选（每阶段） | 最大通道 | 范围 |
| --- | ---: | ---: | --- |
| 显式容器Light | 各7182 | 149 | x28–239，y64–116 |
| 显式容器Dark | 各13556 | 201 | x28–239，y64–127 |
| 自动生成Light | 各7006 | 149 | x28–239，y64–116 |
| 自动生成Dark | 各13556 | 201 | x28–239，y64–127 |

其余20对默认、AA、扰动/复位及撤回原→候选全部0；原/候选四场景AA与复位全部0。1DIP显式Light/Dark分别4863/4895，自动4674/4706，各版本均检出。全部RGBA参与，不裁剪、遮罩、对齐或设容差；未添加刷新、延迟或改模板绕过route。

差异是声明的Style前景与整体条目Opacity开始生效，故文字与选中背景的透明混合均在范围内；未匹配owner class的默认图和撤回图完全保持。已查看明暗完整图，不把专用Style失效写成限制。

## 当前门槛与范围

原四场景4FAIL→候选4PASS，各22秒；显式和自动容器均真实命中。Mac managed初轮54项通过；原生夹具的动态URI XAML入口产生IL2026，已拒绝该入口并改为预编译x:Class的Load(this)，静态入口与NativeAOT正在最终验证，不抑制警告。

用户已明确接受上述专用Style开始生效的八对精确变化（call_1ee4d04140054b10905135e9f59f80d3:0），不沿用CB2的402点批准，不包含Git、完整家族或CT25完成。C#/AXAML、334文件原生工程与56PNG+manifest共57文件已清理，audit clean；清理后原ComboBox69项通过。最终预编译x:Class managed/AOT各54项，IL2026/IL3050为0。详见[路由结果](input-combo-box-item-route-results.md)。
