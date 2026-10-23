# TextArea 桌面版实现

`Input/TextArea.cs` 拥有多行高度与 resize 业务；`TextAreaTheme.axaml` 装配计数区、TextAreaDecoratedBox、文本 presenter 和 resize handle。内部 frame 继续复用 InputControlFrame 状态。

根 Padding 通过 TemplateBinding 进入 TextAreaFramePadding；TextAreaFramePaddingConverter 保留右侧四分之一 inset 与操作区的既有空间分配。ScrollerPadding 继续由右侧内容尺寸决定。外框零 padding 与这些内部 inset 分开，不把根 padding 机械赋给最外层边框。

固定行数测量使用当前完整 typeface、字号、FontFeatures 和 LineHeight，结合文本 viewport 的垂直空间计算滚动区域约束；显式 Height、IsAutoSize 与 resize 分支保持各自职责。模板重建重新获取 scroller/frame/resize 部件，编辑层订阅仍由 AbstractTextInput 管理。

几何绑定和资源查询保持编译期/原生模板路径；无新全局订阅或反射访问。根表面规则见[共享输入架构](../input-control-architecture-design.md#81-根表面几何与预设默认值)。

### 根前景与字体消费

根 Foreground 的默认来自当前 Control 的 Effective Global ColorText，通过框体继承链到达真实文本。根 FontFamily/FontSize/FontWeight/FontStyle 的原生实例输入保留；语义 input 的专用 Style 可声明覆写，禁用及错误状态仍由对应状态样式处理。其他组合 owner 的独立子控件合同在各家族分别定义。

### Resize 绘制与命中

ResizeHandle是既有模板的最后一层，直接持有指示线的轻量Pen并在Render中绘制。ControlTheme把TextArea Own ResizeHandleSize送到Width/Height，把Own ResizeIndicatorLineColor送到Foreground，把有效Global LineWidth送到内部描边属性。尺寸/前景/描边变化使真实绘制更新，无代码资源绑定或额外Adorner。

旧父装饰框的绘制早于不透明模板子树，颜色Token即使求值成功也看不到；现在删除该层的失效绘制属性/Setter与状态，避免双绘制。TemplatedControl原生默认裁剪为true，Handle主题明确关闭自身裁剪以保留原8单位几何的越界端点；不改变输入区域或祖先滚动/裁剪。默认指示线恢复可见已获用户批准，实际像素与其余不变合同由脚本验收记录。

## 可选主题字体族

有效Global FontFamily仅在显式配置时覆盖原继承值；未配置及清除后跟随真实InheritanceParent，保持普通Style/LocalValue优先级。根属性由ControlTheme的有限MultiBinding选择，内部只读继承观察器在分离后释放父订阅，不通过代码写最终字体。

内部frame与操作按钮通过[共享输入架构](../input-control-architecture-design.md#内部资产的主题查询路线)消费真实公开owner的Global；Content附加区的逻辑路线与模板路线同属该合同，保留原生样式优先级和状态归一。
