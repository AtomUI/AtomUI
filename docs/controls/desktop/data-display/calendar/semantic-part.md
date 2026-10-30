# Calendar Semantic Part 契约

本文定义 Calendar/LunarCalendar 组合 DateViewer 的公共定制边界。
关联 [架构](overview.md)、[实现](implementation.md)、[公共面板 Part](../../data-entry/date-viewer/semantic-part.md)和
[Semantic 系统规范](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Owner 与语义区域

Calendar 与 LunarCalendar 各自发布产品 descriptor；组合的 DateViewer 有自己的公共面板 owner。
产品 Part 指向职责区域，不要求应用依赖 DatePanel、Cell 或农历内部类。

| Part | ContractType | Cardinality | 责任 |
| --- | --- | --- | --- |
| root | 对应 public owner | Single | 日历根表面。 |
| header | TemplatedControl | Single | 当前有效业务 Header/自定义 Header 区域。 |
| body | Panel | Single | Header 下的正文与 overlay 组合。 |
| content | TemplatedControl | Single | 组合的公共日期面板。 |
| item | TemplatedControl | Multiple | 统一日期/月/周号 Cell。 |
| itemContent | ContentControl | Multiple | Cell 的业务内容槽。 |

所有 Part 的 Since 为 6.2.0；item/itemContent 为运行时生成的 Multiple 部件，声明 CrossNestedOwners 并使用共享面板模板路径。
Calendar/LunarCalendar 主题必须实现同一组产品语义，农历不额外暴露历法计算或 Provider 私有节点。

## 2. 职责与存在条件

Header 代表有效业务区域，HeaderTemplate 不迫使应用定位隐藏默认 Header。
body 承载正文和不命中的业务 overlay；content 指向公共面板。
item 保留禁用/焦点/选择/Automation；itemContent 定制内容，FullCellTemplate 不替换 item 容器。
不同模式的网格数量变化由模型决定，marker 不随状态增删。

## 3. Composition Model

Calendar → 业务 Header + Body → DateViewer + RangeBarPanel。
DateViewer → DatePanel → WeekHeader/CellHost → 统一 Cell → 内容槽。
LunarCalendar 添加 LunarCalendarCellContent 呈现投影，不复制交互树。

## 4. route 与专用 Style

组合 DateViewer 后，产品 route 显式跨入受建模的面板/Cell 模板，按真实祖先与 scope 拓扑校验。
运行时 Cell 使用生成的 `semantic-cell` 常量标记，模板内容槽静态声明 `semantic-cell-content`，复用与 detach 不改变身份。

| Part | SelectorClass | 专用 Style（Calendar / LunarCalendar） | SelectorRoute |
| --- | --- | --- | --- |
| header | semantic-header | CalendarHeaderStyle / LunarCalendarHeaderStyle | `/template/ .semantic-header` |
| body | semantic-body | CalendarBodyStyle / LunarCalendarBodyStyle | `/template/ .semantic-body` |
| content | semantic-content | CalendarContentStyle / LunarCalendarContentStyle | `/template/ .semantic-content` |
| item | semantic-cell | CalendarItemStyle / LunarCalendarItemStyle | `/template/ .semantic-content /template/ .semantic-body > .semantic-content /template/ .semantic-scope-panel > .semantic-scope-cells > .semantic-cell` |
| itemContent | semantic-cell-content | CalendarItemContentStyle / LunarCalendarItemContentStyle | item route 后接 `/template/ .semantic-cell-content` |

Header marker 位于稳定 ContentControl，默认与自定义 Header 共用该有效区域；content 位于 DateViewer，
itemContent 位于 CalendarDateCellTheme 的 PART_CellContent。
应用使用生成的专用 Style，未命中必须修正 route；不以 Loaded/Name 获取节点赋值回退。

## 5. 尺寸与定制边界

根表面与业务内容分别定制，布局 Setter 保持完整 Mini/Fullscreen 尺寸基线与内容滚动。
周号、Header 内部 Select、范围条绘制节点与农历算法不作为 itemContent 私有节点公开。
日期内容模板仍有统一 Cell 的状态防线，样式不能绕过业务约束。
父默认主题不穿透子模板；Semantic 的跨模板 route 仅用于正式产品契约。

## 6. 重建与验证

专用 Style 与 descriptor 的 route 一致，由实际命中测试验证；HeaderTemplate 切换保持 header 有效区域。
验证普通/农历四种布局、ShowWeek、HeaderTemplate、Cell/FullCell、RangeBars 及模板重建。
关键 Setter 必须实际命中，容器池回收保持身份，样式示例不得有代码回退。
还须验证命中区域、裁剪、嵌套滚动和对象释放，不能以 descriptor 存在证明产品体验。
