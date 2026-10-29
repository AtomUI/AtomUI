# 内容展开与收起动效设计

本文定义 NavMenu Inline、Collapse、Expander 与 TreeView 系列共用的内容几何和接入合同。
播放身份、结果与属性覆盖见 [Motion 系统](../motion/overview.md)，实际布局能力见
[目标与布局](../motion/targets-and-layout.md)。Core 拥有播放，Controls 拥有 ContentViewport，具体控件拥有业务状态。

## 1. 不变量

- 内容保持自然尺寸排版，动画改变占位、真实视口和透明度，不缩放文字或图标。
- 同一手风琴切换并发交换空间，不等新项展开完成才关闭旧项。
- 反向从实际 Bounds 与透明度接续，首 tick 前和最后 tick 后均保持确定帧。
- 尺寸约束、标题、边框、padding、可见滚动条、嵌套滚动与命中合同保持原有所有权。
- 只有当前请求可以提交稳定状态；旧执行不影响新模板或新 owner。

## 2. 模型与所有权

| 职责 | Owner |
| --- | --- |
| IsExpanded / IsSelected / IsSubMenuOpen | 具体控件、selection 或菜单状态 |
| Motion 定义、时钟、替代、透明度与结果 | Core Motion |
| IMotionLayout / IMotionLayoutLease | Core 的公开扩展能力 |
| 自然内容、占位、实际视口和四方向排列 | Controls.ContentViewport 及其布局租约 |
| Opened / Closed、命令、键盘、路径和物理宿主 | 具体业务控件 |

ContentViewport 继承 ContentControl，保留 ContentTemplate 和延迟物化，只实现布局能力。
它不拥有动画任务、取消源、帧时钟或业务事件，普通视觉 Motion 不需要该原语。
租约的 SetProgress 接受同一次播放的进度，Dispose 只解除自身接管；旧租约不能清除后建立的新租约。

租约建立、进度变化和释放只失效本节点的测量，期望占位变化由框架沿父链传播，并在第一个尺寸不变的祖先处停止。
不得逐帧失效全部视觉祖先的排列：Avalonia 的 InvalidateArrange 会同时重录祖先绘制指令，
合成端把该祖先整棵子树标记为重绘区域，窗口外框等绘制型祖先会使每帧退化为整窗重绘。

框架存在一个排列缺口：祖先因约束变化被重新测量时，Measure 不清除 IsArrangeValid，且祖先测量期间不转发
ChildDesiredSizeChanged；若随后排列矩形相同，祖先跳过排列，子项保留旧槽位（侧栏往返后子菜单 DesiredSize 正确、实际高度接近 0）。
租约存续及释放后的首次排列中，视口在自身 ArrangeCore 后比较父链 DesiredSize 快照，只对尺寸确实变化的祖先失效排列。
该集合与框架正常传播集合一致，不扩大重绘范围；祖先只进入框架 Arrange 队列，不同步刷新布局或另建调度时钟。

## 3. 计算流程

单位为 DIP。水平主轴用于 Left / Right，垂直主轴用于 Top / Bottom。

1. 准备时捕获当前 Bounds 主轴可见长度及扣除 Margin 的期望占位。首次展开以零长度起步，反向使用当前租约呈现值。
2. Measure 以有效宿主约束测量完整 Presenter，取得自然内容尺寸。动画主轴不作为缩小文字排版的测量约束。
3. 期望占位按进度插值；ArrangeCore 单独约束实际分配矩形。即使固定父布局分配更大空间，真实 Bounds 和命中视口也受进度约束。
4. Arrange 使用完整内容尺寸，交叉轴遵守父约束。Bottom / Right 起点为零，Top / Left 以可见长度减完整长度确定尾边锚定。
5. 启用布局舍入时先按实际缩放取得视口像素尺寸，再用该结果计算锚点，不独立舍入相邻边。

实际内容长度取自然内容与有效宿主分配的适用值，Margin 留在原 owner。
Min / Max、固定尺寸与内容边框不会通过删除应用属性来实现收起；实际视口仍单独承担绘制和交互裁剪。

## 4. 动态内容与反向

内容、字体、padding、边框、真实宽度约束或嵌套内容变化时，自然尺寸随布局失效更新。
同一次播放的进度不重启。设新终点为 T、当前呈现长度为 V、当前进度为 p（p<1），重新计算虚拟起点：

```text
S = (V - T * p) / (1 - p)
L(p) = max(0, S + (T - S) * p)
```

因此目标变化当帧保持 V，仍在原截止时间到达新终点；占位与实际视口分别重定向。
新的反向意图是新请求，从当前实际长度开始使用新请求时长。透明度与尺寸共享同一时钟基准。

## 5. 控件接入

| 消费者 | 目标与状态 |
| --- | --- |
| Expander | PART_ContentViewport；主题绑定 IsExpanded、IsMotionEnabled、MotionDuration，方向 selector 提供 LayoutDirection |
| CollapseItem | PART_ContentViewport；主题绑定 IsSelected、动效开关和有效时长，父 selection 同步协调互斥 |
| NavMenu Inline | PART_ChildItemsViewport；主题声明开合，item 持有排队版本和事件核对，不持有执行器 |
| TreeView / TreeSelect / Transfer | PART_ChildrenViewport；owner 捕获自定义定义和请求参数，反向不丢弃，完成时核对业务和模板身份 |

ExpandMotion / CollapseMotion 使用该能力，普通 Control 缺少能力时在写目标前拒绝布局轨迹。
初始模板、重新挂载、关闭动效或方向切换由可见性 owner 按当前业务状态稳定投影。
正常展开保留终态布局覆盖；收起隐藏、Reset、下一次成功请求和 detach 释放覆盖及租约，恢复最新底层来源。

## 6. 输入、滚动与验证

绘制裁剪和命中使用相同实际视口，不只修改 DesiredSize 或透明度。
验证保留原有限高、内容规模、标题、滚动条、嵌套 ScrollViewer 与边界滚动链；不通过移除约束规避问题。

定向验证覆盖四方向与不同缩放、固定父约束、动态内容、同高空间交换、首尾帧、快速反向、模板替换和卸载重挂载。
现有 ContentExpansionMotion / Boundary / Input 与控件行为测试承担长期合同；Headless 不替代原生视觉和帧预算验收。

控件实现入口：[Expander](../../../controls/desktop/data-display/expander/implementation.md)、
[Collapse](../../../controls/desktop/data-display/collapse/implementation.md)、
[NavMenu](../../../controls/desktop/navigation/nav-menu/implementation.md)、
[TreeView](../../../controls/desktop/data-display/tree-view/implementation.md)。
