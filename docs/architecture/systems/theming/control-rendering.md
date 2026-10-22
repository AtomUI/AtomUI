# Control 主题输入与渲染合同

本文拥有主题值进入Control、内部视觉对象和最终渲染的跨模块设计合同。配置与求值分别见[运行时](runtime.md)和[Own求值](own-token-evaluation.md)，公共Part由[Semantic Part](semantic-parts.md)拥有，作者执行规则见[消费规范](../../../engineering/development/theming/consumption.md)。

本文定义目标，不表示每个现有Control已经完成接线；实现边界见[验证文档](verification.md)，单个Control的实际模板与公开API仍由其控件文档拥有。

## 1. 输入分工

| 输入 | 职责 | 不变量 |
| --- | --- | --- |
| Global/Effective Global | 品牌、通用排版、尺寸、描边、状态和motion | 响应Control覆盖的消费使用其Effective Global；刻意全局共享有明确语义 |
| Own | 控件独有稳定设计值 | Active值有实际消费者及适用分支，依赖与覆盖一致 |
| 公共实例属性/Theme Variables | 用户设置及当前实例状态的有效视觉输入 | 默认派生不无条件重写用户公共属性 |
| 专用Part Style | 稳定视觉区域和条件Setter | route命中真实节点，原生优先级和绘制均有效 |
| ControlTheme/允许的Part Theme | 结构和完整视觉 | 行为、identity与Part兼容责任明确 |

控件代码与Render是上述输入的消费者，不保存第二份主题，不为每实例复制Token字典或订阅全局主题事件。

## 2. owner与组合边界

公开Control按exact注册identity消费；internal frame、presenter、几何和非Visual对象由实际公开owner提供输入。公开子Control继续拥有自身identity；父owner只负责声明的组合布局、边框拼接与状态投射。

不能机械把Shared替换为内部Part identity，也不能因父主题包含Button就让Button自动消费父Control全部Token。需要父语义控制的组合属性显式投射，子Control自身可配置合同保持。

模板可等价表达的关系放ControlTheme；C#仅处理AXAML不可表达的运行时关系，并说明原因、取值/状态/优先级、失效及释放owner。Transitions放style setter，不在模板内容构建期创建。

## 3. root与实际视觉节点

Background、Foreground、Padding、BorderThickness、CornerRadius及排版按实际职责投射到真实frame、文字、图标或绘制输入。root只是外壳时，公开setter不应成为无视觉效果的入口。

投射表属于单Control文档，应标明公开输入、真实目标、适用状态、BindingPriority与派生关系。输入框体还需覆盖addon、compact、校验、禁用、加载和不同variant；替代节点承担相同公开职责时遵守相同合同。

不要求所有Control具有全部框体属性。透明命中面、内容色谱、数学角度等保留其非品牌语义；形态是否开放由公开合同确定。

## 4. 状态、尺寸与优先级

主题默认、状态规则、实例属性和专用Style使用Avalonia原生优先级。代码不得在Measure/Arrange或状态计算中无条件写公共FontSize/CornerRadius等来压制定制；必要coercion有范围和行为合同。

shape/variant/state切回时清理上一分支占有的值或内部缓存。自适应默认使用内部有效计算结果，用户显式输入保持。S/M/L具备完整映射；Custom按[Control研发规范](../../../engineering/development/control-development-guidelines.md)退出接管维度的预设分支并保留可用基础默认。

状态和尺寸覆盖必须同时考虑父级Min/Max、方向、裁剪和命中几何。不能通过去掉高度限制、滚动条、内容或嵌套条件使测试通过。

## 5. 自绘与缓存

Render/Draw/VisualBrush必须读取约定的brush、pen、线宽、形态、文本和动效输入。公开Setter改变节点属性但Render忽略它，仍属于消费断链。

每项几何、文本度量、pen和brush缓存明确真实依赖及invalidate路径：

- 影响自然尺寸的输入触发Measure；仅影响排列的触发Arrange；绘制输入触发Render，并使相关缓存失效。
- VisualBrush中的对象不能假定拥有正常逻辑树字体继承；完整FontFamily/Size/Weight/Style按owner合同投射。
- 同次命令式计算捕获一个snapshot，不能ApplyTemplate或首次Open后永久固化本应动态的主题值。
- 线宽占用、内容留白、绘制与命中采用一致的几何合同；像素对齐不成为隐藏最小值或固定笔宽的理由。

## 6. 更新与资源生命周期

Token/Part Style更新作用于已有实例，覆盖状态变化、re-template、detach/attach、容器回收、owner迁移及已打开弹层。普通配方切换保持输入、选中、焦点和滚动；结构模板替换单独定义内部状态保存责任。

非Visual对象使用[scoped资源host](../../../engineering/development/scoped-resource-host.md)；binding、资源事件、动画、timer、style与跨根lease由实际owner对称释放。不能用静态值规避泄漏，也不能靠永久Application订阅获得更新。

Native/Overlay的Token context、排版和品牌样式各有接线责任，只桥接其中一项不代表全部完成。持续视觉工作遵守有效可见性与motion/wave合同。

## 7. 验证合同

每个Active Own及适用分支、每条Own依赖、每条Part route及替代节点有证据。代表Global必须具有检错意义，完整Global配置空间不因当前未消费而收窄。

从有效值、目标属性及来源、实际几何/绘制到交互逐层验证；覆盖A→B→A、显式实例值、专用Style、尺寸/状态和回收/跨根。最终遵守[主题验收规范](../../../engineering/development/theming/verification.md)，不能只以资源值、编译或截图判定通过。
