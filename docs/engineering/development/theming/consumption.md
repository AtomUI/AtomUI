# 控件主题消费规范

> 本文定义消费与验收要求，不表示全部现有Control已经完成接线。

本文的适用范围、规范用语与事实边界见[全局规范入口](overview.md)。

本文拥有主题值到实际节点、布局和绘制的消费要求，交付证据见[验收规范](verification.md)。ControlTheme、尺寸与模板边界同时遵守[Control研发规范](../control-development-guidelines.md)，专用Style和route的系统语义遵守[Semantic Part合同](../../../architecture/systems/theming/semantic-parts.md)。

输入模型、owner/组合、root投射与自绘依赖的系统设计见[Control渲染合同](../../../architecture/systems/theming/control-rendering.md)；本文只拥有作者及评审执行规则。

### TH-05：视觉输入必须到达真实渲染

性质：新增统一消费闭环要求。

- 每个公开主题入口都必须到达实际视觉节点或绘制输入；仅设置root但真实frame不读取，不算支持。
- Background、Foreground、Padding、BorderThickness、CornerRadius与排版等按控件实际职责定义投射，不能机械要求不适用控件具有所有属性。
- 文本、图标、箭头、焦点、校验、禁用、加载、弹层、滚动条和生成容器的适用视觉分支不得漏接。
- Render/Draw/VisualBrush、pen、geometry和文本测量必须消费公开输入，并声明真实失效依赖；用户修改属性后必须更新对应测量/排列/绘制缓存。
- 同一次命令式计算只捕获一个snapshot；禁止首次ApplyTemplate/Open后永久保存本应更新的主题值。

检查：有效Token→目标属性→实际布局/绘制的证据；逐分支修改后观察更新，资源值改变本身不足以通过。

### TH-06：声明式模板与原生样式优先级

性质：现行AXAML优先约束＋新增优先级闭环要求。

- ControlTheme能等价表达的模板、状态与属性关系必须放AXAML；C#绑定仅用于已说明具体原因的不可表达情形，并有释放owner。
- Transitions声明在ControlTheme的style setter中，不放入ControlTemplate内容构建期。
- 框架默认不得通过无条件SetValue、Measure/Arrange赋值或代码找到节点改值压制合法定制。几何派生优先保存在内部计算结果，不能重写公共用户属性。
- shape、variant、size和状态切回时不得遗留上一分支的本地值或缓存。必要的约束/coercion有明确合同和证据。
- 优先级继续由Avalonia执行；规范必须说明主题默认、状态规则、实例属性和专用Style在实际目标上的关系，不创建另一套优先级或Style合并引擎。
- 默认ControlTheme遵守自身模板边界；应用Semantic Style的跨模板route只能使用正式声明和生成合同，不能据此放宽所有生产selector。

检查：默认/状态/实例/专用Style分别生效，A→B→A及re-template无残留；核对实际BindingPriority而非假定赋值一定有效。

### TH-07：Semantic Part是稳定公共定制合同

性质：现行合同整合。

- 公开的是稳定视觉职责及ContractType，不是PART_*、节点Name、内部实现类或偶然层级。
- 除root正常属性/样式入口外，Part定制通过生成的专用Semantic Part Style声明式应用；route必须匹配真实模板、逻辑树或跨根拓扑。
- 专用Style不命中属于合同/实现缺陷，必须修正；禁止用code-behind找节点设置Background/Padding等绕过。
- 状态替代节点、可选Part、多实例容器及Native/Overlay路径必须维持各自声明的cardinality与定制合同。
- 仅在确有完整替换需求的真实public子Control上开放强类型Part Theme；完整模板替换须明确Part及行为兼容责任。

检查：每条独立route及替代节点命中、关键Setter有效、布局Setter效果、专用Style示例无代码回退。

### TH-09：确定的动态更新与生命周期

性质：现行资源/生命周期合同整合＋更新覆盖要求。

- 动态外观与动效不能使用静态读取规避更新；不可变几何、内容数据和静态资产可以静态引用。
- 每个binding、事件、timer、动画、资源host和跨根lease先定义acquire/release对应关系，再接入生命周期；非Visual对象使用scoped资源host。
- detach/attach、owner切换、re-template、容器回收、Popup关闭和功能关闭必须更新或释放对应依赖。
- 热路径不为每个实例复制Token字典或建立全局主题订阅；缓存有容量/释放边界，不持有旧Visual和历史snapshot。
- 持续视觉工作遵守有效可见性、Motion/Wave等最终开关及已有动效合同；主题切换不得新增长期后台视觉工作。

检查：动态值、重挂载与回收验证；必要时对象计数/WeakReference，不能仅凭RSS或截图判定无泄漏。

### TH-10：值边界、尺寸、形态与可访问性

性质：现行Custom尺寸约束＋新增统一视觉边界要求。

- token默认常量、数学几何、色谱数据、透明命中面与用户示例值允许存在；消费端写死的通用设计值必须接入正确主题入口。
- 线宽、间距、阴影、字号、字重、行高、字体、图标、圆角、状态色与动效按实际语义分配Global/Own/Part，不能仅支持颜色。
- 三档预设尺寸有完整基线；Custom按现行合同退出它所接管维度的预设规则，保留稳定回退，不额外复制一套Custom Token。
- 固定圆形、数据图形、原生窗口能力等边界必须明确；是否开放形态变化由公开契约决定，不能强迫每个数学常量变成Token。
- 字体度量、DPI/缩放、极值、无动效及明暗状态下保持内容可读、焦点可见、状态可辨和原始命中/滚动/裁剪合同。对比和平台效果按声明目标及实际证据评估，不虚构全平台像素等同。

检查：零圆角、粗描边、大字体、紧凑尺寸、支持的缩放、明暗和关闭动效；极值不机械穷举无意义组合。
