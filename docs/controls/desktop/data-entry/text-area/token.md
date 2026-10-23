# TextArea Token

TextAreaToken 的 identity 为 `AtomUI:TextArea`。Own 字段只表达多行操作区和 resize 的稳定视觉差异。

| 字段 | 职责 |
| --- | --- |
| ResizeIndicatorLineColor | resize 指示线颜色，默认来自有效 ColorTextDescription。 |
| ResizeHandleSize | resize 实际命中/布局与指示线比例尺寸，默认来自有效 SizeXS；0使区域为0并停止绘制。 |
| RightAddOnPadding / LG / SM | 对应尺寸的右侧操作区 inset，依赖水平 padding 与 LineWidth。 |
| LeftInnerAddOnMargin | 内部左侧内容与编辑区域间距，默认来自有效 UniformlyMarginXXS。 |

通用边宽、圆角和根内容 inset 使用 owner 的 Effective Global/公开属性，不新增重复 Own 字段。Lines、IsAutoSize、IsResizable 与当前拖拽高度属于实例状态。

ResizeHandleSize通过TextArea ControlTheme同时设置既有末层Handle的Width/Height；绘制按实际Bounds缩放原8单位几何（两条线使用7/4单位，内缩2单位），保持颜色的独立Own语义。指示线描边使用TextArea有效Global LineWidth，0不绘制。IsResizable=false由原生可见性排除Handle，既有文本、高度约束、拖拽与滚动算法保持。

指示线现在由末层Handle绘制，避免原父装饰框绘制被模板背景覆盖。原尺寸下，恢复默认可见指示线是2026-10-23用户明确批准的视觉例外；它不授权其他默认布局/颜色差异。Handle主题允许原参考几何端点绘制到自身Bounds之外，命中仍以原8×8或配置尺寸确定。

TextArea自身主题的selection/caret、placeholder、Filled/Borderless状态文字、行高、间距和EnableMotion使用本身份typed Global；不绕过owner读取Shared资源。状态文字的实际目标是ContentLayout继承链下的文本节点，验证不得把内部滚动容器Foreground当成最终文字颜色。

## 默认求值协议

本Own定义使用DependencyDefaultsV2、Revision1，默认工厂以有效Global为输入，各字段没有Own之间的依赖；显式Own覆盖优先。名称、类型、公共资源编号及既有单位/范围保持，不因迁移另建别名。Global变化时重新计算未显式覆盖的默认值，清除Own后恢复对应公式。
