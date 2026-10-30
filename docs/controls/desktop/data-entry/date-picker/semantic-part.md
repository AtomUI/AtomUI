# DatePicker Semantic Part 契约

本文记录当前 DatePicker 家族的定制边界与真实 descriptor。
关联 [架构](overview.md)、[实现](implementation.md)、[公共面板 Part](../date-viewer/semantic-part.md)和
[系统规范](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Owner 与区域

DatePicker/RangeDatePicker 拥有输入和 Popup 产品语义，共享面板的独立 owner 不替代 Picker。

| Part | 责任 |
| --- | --- |
| root | 产品输入表面。 |
| prefix / suffix | 输入前后附属内容。 |
| input / secondaryInput | 单值或范围起点输入；secondaryInput 仅范围终点。 |
| clear | 清除操作。 |
| popup.root / popup.container | 弹层表面与 Presenter 组合区域。 |
| popup.header | 日期导航；单值一个，双面板范围两个。 |
| popup.body / popup.content | 日期正文与实际 Panel。 |
| popup.cell | 统一 Cell，目标 ContractType 为 TemplatedControl，Multiple。 |
| popup.footer | Today/Now/Confirm。 |

区域的 cardinality、Since、类型与 route 由现有 AXAML/声明产生，不复制旧 Button/UniformGrid 契约。
单/双面板须明确实际节点数量，不为保留旧节点制造 wrapper。

## 2. 存在条件

输入归输入控件，日期归公共面板，Footer 归 Presenter。
周列、时间、确认开关改变可见性/内容，不把 marker 当状态开关；未打开 Popup 不提前创建占位 Cell。
适用且已应用的模板才具有对应目标。

## 3. Composition Model

Picker → 输入/Popup → Presenter → DateViewer/RangeDateViewer + TimeView + Footer。
面板 → DatePanel → DateViewerCell → 内容槽。真实模板结构从落地 AXAML 抽取，不能根据目标表生成伪 XML。

## 4. Style 与 route

Marker 静态声明或创建时用生成常量加入；回收不改变身份。
跨 Popup/嵌套 owner 依据真实 logical/templated-parent 链声明完整 route，应用使用生成专用 Style。
禁止 Name/Loaded 获取 Cell 后赋值的定制回退；未命中必须修 route 并补失败命中证据。

## 5. 定制边界

输入定制保持尺寸档位、预留宽度、校验和清除命中。
日期定制保持连续范围、整周行、禁用与焦点；Footer 不能改变确认。
TimeView 内部、Calendar 范围条和用户内容子树不经 Picker cell Part 公开。
旧名称/类型/route 不约束重建，新定制的输出仍须满足产品视觉与使用要求。

## 6. 验证

覆盖新 descriptor、真实 marker、所有面板/时间/Footer、重开和重套模板。
断言关键 Setter 命中、回收后身份稳定、Gallery 无代码回退，不能只查 class 存在。
额外完成 [产品逐步等价验收](../date-viewer/shared-panel-design.md#10-重建边界与等价验收)。
