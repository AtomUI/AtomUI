# CT13 CB3：ComboBox popup.listItem 专用样式路由

CB2优先级验收发现真实缺陷：已编译ComboBoxPopupListItemStyle的Foreground及Opacity都不命中。marker存在，但容器的直接逻辑父是ComboBox，TemplatedParent为空；视觉父为VirtualizingStackPanel，PopupFrame只在视觉祖先链。原 `/template/ .semantic-popup-root >> .semantic-popup-list-item` 错把视觉拓扑当逻辑后代。

唯一产品修复为该语义声明的SelectorRoute改为 `> .semantic-popup-list-item`，沿真实逻辑父匹配；沿用既有生成器Child组合器，不改通用引擎、marker、ContractType、Multiple/RuntimeCreated/CrossVisualRoot字段。不添加代码目标属性回退，也不将不命中写成定制限制。

- [x] CB3-R：编译AXAML专用Style与关键Setter真实存在，原不命中（Opacity1而非0.73）失败；记录完整逻辑/视觉树与实际selector。
- [x] CB3-B：原/候选四场景（明暗×显式/生成容器）同历史完整帧，默认/AA/1DIP/复位/激活/更新/撤回；不重建一侧或关闭正常动效。
- [x] CB3-I：仅修正route，保留owner隔离与原生属性优先级；普通/disabled、Own动态变化、撤回到当前Own和真实文本必须通过。
- [x] CB3-V：公开API NativeAOT加载同一编译AXAML专用Style；NET8、原ComboBox模块及原有descriptor断言/文档同步。若style或颜色仍失败，保留原反例，不做回退。
- [x] CB3-C：精确视觉审查、临时AXAML/C#/原生工程/媒体清理，原模块复验与作者另步自审；完整家族/其他Host/CT25继续，无Git授权。

CB3：popup.listItem真实直接逻辑父为ComboBox，原从视觉PopupFrame查逻辑后代不命中；route改> .semantic-popup-list-item，marker/ContractType/跨根/Multiple保持，无代码属性回退。原4FAIL→4PASS22s，真实编译专用Style类/Setter核对、Own更新/撤回及文本通过；预编译x:Class managed/AOT各54，消除夹具URI入口IL2026，无抑制。八对精确Style变化已单独批准，其余20对/AA/复位0。临时C#/AXAML、334文件工程、57媒体清理，原69项/NET8零警告24.89s；完整家族/CT25继续，无Git。
