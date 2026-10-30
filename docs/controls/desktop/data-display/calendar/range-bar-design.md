# Calendar 范围条设计

本文定义共享日期面板上的业务条架构。关联 [架构](overview.md)、[实现](implementation.md)、
[行为](behavior-design.md)、[农历](lunar-calendar-design.md)、[Token](token.md)与
[日期面板设计](../../data-entry/date-viewer/shared-panel-design.md)。

## 1. 定位

RangeBars 是连续日期业务标记，Calendar 拥有其集合、资源宿主和 overlay。
它不是 RangeDateViewer 选择范围，不参与候选端点、确认、焦点或命中。
仅 Fullscreen Month 显示，Year、Mini 和周号不渲染业务条。

## 2. 公共模型

Calendar.RangeBars 为空的可观察集合，可替换并支持增删/移动/Reset。
每条包含 StartDate、EndDate、Label、Background 与 Height；日期首尾包含，颜色/高度是实例视觉参数。
Background 支持普通绑定、DynamicResource 与 TokenResource。
模型不持有生成视觉或最后一个 Cell，非 Visual 资源宿主采用 generator/scoped attachment。

## 3. 组合与 ownership

Calendar Body 叠放 DateViewer 和 RangeBarPanel。overlay 为 IsHitTestVisible=false，位于正文内容上层。
Calendar 拥有集合/条目订阅与 attachment，Panel 拥有计算和绘制，DateViewer 提供日期拓扑和实际几何。
FullCellTemplate 只替换 Cell 内部，不能替换 overlay；业务条不能改变 Cell margin、行列或选择。

## 4. 坐标模型

布局输入为共享面板的可见日期区间、槽位行列、实际 Cell bounds、周号偏移、内容留白及业务条顺序。
坐标统一转换到 overlay 的本地 DIP；不按 Value 重新计算 GridStart，也不复制月份星期算法。
未完成 arrange 或不可用几何不输出错误条段，重新布局时按有效几何重算。

## 5. 分段与 lane

条目区间按日期投影，并裁剪到当前可见日期网格；反向端点按升序视觉区间解释，不改写业务对象。
同一条按周行切段，单日也形成合法条段；周号列不占业务日期宽度。
稳定排序使用原集合顺序作为 tie-breaker，按不重叠区间分配 lane，行边界裁剪保持连续日期语义。
Label 在对应条段的可用区间布局，不扩大网格、不抢占输入。

## 6. 失效与生命周期

集合替换、Add/Remove/Move/Reset 或条目变化失效业务条；拓扑/arrange/尺寸或呈现变化失效几何。
纯 RangeBars 变化不重建 CellModel。重复条目引用采用计数 attachment，最后一次移除释放。
detach、owner 释放或集合替换解除条目事件和 scoped resource host；attach 恢复当前集合。

## 7. 农历、主题与资源

农历顶部偏移来自次级行高与内容布局 metrics，业务条位于公历值/农历内容之后。
不得通过给 Cell 增加外 margin 或关闭内容滚动避让。
条高、间距、圆角、文本 padding 按 Calendar 视觉语义；单条 Background/Height 支持局部动态资源。
overlay 不保存共享日期引擎的业务状态，不增加独立选择 Token。

## 8. 验证

验证单日/跨周/跨月、多个 lane、同起点稳定顺序、集合替换/重复引用/Reset、主题动态更新和 detach。
验证共享几何与实际渲染一致，ShowWeek 不偏移业务日期，农历不重叠，overlay 不命中，嵌套内容滚动保留。
AOT 验证 generated resource host 和 Calendar 消费；独立 DateViewer 不保留业务条资源。
