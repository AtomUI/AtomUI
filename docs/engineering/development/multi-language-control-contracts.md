# AtomUI 多设计语言控件设计与实现规范

**正式规范 v1.0 · 2026-11-27 · 架构复审修订**
适用范围：`AtomUI.Core`、`AtomUI.Controls`、`AtomUI.Desktop.Controls`、`DataGrid`、`ColorPicker`、`Extras`、跨仓 `AtomUI.Mobile.Controls` 共享基元，以及所有第一方（Ant Design、Shadcn、PrimeNG 等）与第三方设计语言包。本文定义多设计语言目标架构与合规要求，现行已落地取值包架构见[设计语言架构](../../architecture/systems/theming/design-languages.md)。

关联规范：[主题系统运行时](../../architecture/systems/theming/runtime.md) · [Motion 动效系统](../../architecture/systems/motion/overview.md) · [视觉层规范](../../architecture/systems/rendering/visual-layers.md) · [Semantic Part 系统](../../architecture/systems/theming/semantic-parts.md) · [Control 注册契约](../../architecture/foundations/control-registration-contracts.md) · [AOT 与裁剪架构](../../architecture/foundations/aot-and-trimming.md) · [控件研发标准规范](control-development-guidelines.md)

---

## 一、总则与架构公理

AtomUI 在多设计语言架构下，将每一个控件严格解耦为 **控件公共契约（Control API）**、**控件行为契约（Behavior）**、**控件主题模板（ControlTheme & Template）** 以及横切后两者的 **多语言动效体系（Motion & Interaction Effects）**，并遵守以下四条架构公理：

1. **统一控件公共契约（One Control, One API）**
   同一控件在所有设计语言下共用同一 CLR 类型及第 2.1 节所列的适用契约。切换语言时，业务 AXAML、绑定和公开操作保持兼容。
2. **默认行为与动效兜底、语言按需覆盖（Default Behavior & Motion with Scoped Override）**
   控件本体唯一掌管公共状态机、数据校验、命令执行、事件触发次数与虚拟化生命周期。AtomUI 提供完整的默认行为与默认动效方案；设计语言可按需覆盖局部行为、瞬态交互反馈（波浪/水波纹/按压缩放/静默）与组件编排轨迹，可选贡献按第 4.2 节继承或采用默认实现。
3. **默认主题模板兜底与独立替换（Default Template with Scoped Override）**
   AtomUI 在控件包内为每个控件提供默认 `ControlTheme`（含 `ControlTemplate`、状态样式与动效绑定）。设计语言可直接复用默认模板并仅提供 Token / 动效方案，也可为存在真实结构差异的控件提供独立编译的语言级 `ControlTheme` 覆盖；应用层以设计语言为整体单元切换，不暴露跨语言半混搭公开配置。
4. **默认组合 100% 复现 Ant Design 体验（100% Ant Design Default Baseline）**
   **AtomUI 默认属性语义 + 默认控件行为 + 默认动效方案 + 默认控件主题模板 + `AntDesign` 设计语言取值包** 的组合，必须追求 **100% 复现锁定基线版本的 Ant Design 交互、动效与视觉体验**。同时保持平权架构：`AntDesign` 在 `AtomUI.Core` 与控件包中不享有硬编码分支或特权通道，其他设计语言通过完全对等的公开契约实现自身体验。

**正交可组合性不变量**：对于任意开放定制的控件，**（默认行为 / 语言行为）×（默认模板 / 语言模板）×（默认动效方案 / 语言动效方案）** 搭配当前语言有效 Token 时，均必须无需修改业务 AXAML 即可协同工作，且公共状态提交、事件触发次数与可访问性契约完全一致。

---

### 最小设计约束

满足公开语义、目标语言的真实设计和性能门槛后，优先保留最小实现。先核对已有属性、Token、模板、呈现器及播放入口；已有入口足够就直接复用。新增公共类型、状态或常驻订阅须说明具体消费场景、已有入口的可观察缺口，以及默认路径的成本。零覆盖时保持直接路径，按需生成和激活。单一控件的实现细节先留在其所属模块；通用层应有已经确认的共享消费者。

## 二、控件公共契约、行为与模板的职责边界

```mermaid
flowchart TD
    App["业务层 AXAML / ViewModel\n绑定同一套公开 Avalonia 属性、命令与事件"]
    API["第一层：控件公共契约（Control API）\n属性注册 · 数据校验 · 状态提交 · 事件路由 · 容器回收"]
    Token["当前作用域 ThemeContext\nThemeSnapshot + MotionScheme + InteractionEffect"]
    Behavior["第二层：控件行为契约（ControlBehavior）\n默认复现 Ant Design · 语言可覆写局部语义虚方法"]
    Template["第三层：控件主题与模板（ControlTheme）\n默认复现 Ant Design · 语言可独立替换视觉结构"]

    App -->|"显式属性 / 绑定 / 监听事件"| API
    Token -->|"Style 优先级默认值 · Token / Motion 资源投影"| API
    Token -->|"TokenResource / TokenTransitions / MotionResource"| Template
    Template -->|"约定 PART_* 输入目标与命中几何"| API
    API -->|"只读语义上下文询问"| Behavior
    Behavior -->|"返回有界语义决策"| API
    API -->|"公开/内部有效状态 · 伪类 · 两阶段交互动效信号"| Template
```

### 2.1 第一层：控件公共契约（Control API）

控件面向应用的公共契约按用途分为以下十类。

| 契约种类 | 范围 |
| --- | --- |
| 类型与接口 | 应用依赖的公开类型、接口及其身份 |
| 属性与绑定 | 配置输入、读写权限、默认值、校验、绑定与变更通知 |
| 事件 | 公开通知、参数、路由、取消及触发时序 |
| 命令 | 命令调用、参数、可执行条件及执行语义 |
| 公开方法 | 应用可调用的同步或异步操作、返回值、完成、取消与失败语义 |
| 内容与组合 | 内容槽、子元素、父子关系及应用提供的内容模板 |
| 数据源与集合 | 数据输入、项目身份、集合所有权及增删、替换与刷新语义 |
| 可观察状态 | 应用或样式可读取的有效状态、公开伪类及状态变化语义 |
| 输入与焦点 | 对外承诺的操作入口、键盘导航、焦点进入与恢复 |
| 无障碍与自动化 | 辅助技术和自动化客户端可读取的角色、名称、状态及可执行操作 |

各设计语言共用同一控件的适用类别及其公开语义。每一类只约束控件已经承诺的能力；分类本身不要求增加成员、接口或运行时层。模板协作部件、交互策略、动效及 Token 的扩展契约分别由对应章节和专项规范定义。

**公共属性与优先级硬约束：**

1. **同一注册字段与语言中性命名**：所有设计语言共享控件类上唯一注册的 `AvaloniaProperty` 与 `RoutedEvent`。公开属性与接口必须使用设计语言中性命名——原带 Ant Design 专名的 `IWaveSpiritAwareControl`、`IsWaveSpiritEnabled` 与全局种子 `EnableWaveSpirit`（见 [IWaveSpiritAware.cs](../../../src/AtomUI.Controls.Shared/WaveSpirit/IWaveSpiritAware.cs)）在本次重构中直接统一升级为 **`IInteractionEffectAwareControl`、`IsInteractionEffectEnabled` 与 `EnableInteractionEffect`**（不保留旧名转发别名，按[变更日志规范](../contributing/changelog-guidelines.md)记录破坏性迁移）。
2. **显式配置跨语言同义**：应用显式指定的交互与动效属性（如 `<atom:SplitButton TriggerType="Hover" />`、`<atom:Select OpenMotion="{x:Null}" />`）在任何设计语言与任何语言模板下语义完全一致。
3. **单一状态提交点**：程序写属性值、UI 自动化操作（`AutomationPeer`）与指针/键盘输入汇聚到控件受控的内部提交入口，严格区分触发源——代码修改选中值绝不能伪造一次用户 `Click` 或触发点击交互动效。
4. **原生属性优先级。** 对同一 StyledProperty，Avalonia 12.2 按 `Animation > LocalValue > StyleTrigger > Template > Style > Inherited` 选择有效值，最终使用注册默认值。普通应用 Style、普通 ControlTheme Setter 与框架普通 Style 都可能处于 Style 级；同级胜负还取决于样式宿主、帧和声明顺序。Semantic Part Style 的实际 selector 决定其优先级。语言默认必须放在应用可覆盖的位置，具体装载规则见第 5.1 节。不得用写入实例 LocalValue 修正优先级。

### 2.2 第二层：两级控件行为体系（Control Behavior）

设计值沿用既有 Token 分工：跨控件共享的值归 Global Token，单控件的稳定设计参数归 Control Own Token，语言私有的派生输入归 Language Token。

存量代码中类似 [ITreeViewInteractionHandler.cs](../../../src/AtomUI.Desktop.Controls/TreeView/ITreeViewInteractionHandler.cs) 仅暴露 `Attach` / `Detach` 并接管原始指针、单选分组与级联勾选的模式（见 [DefaultTreeViewInteractionHandler.cs](../../../src/AtomUI.Desktop.Controls/TreeView/DefaultTreeViewInteractionHandler.cs)），**禁止**作为设计语言行为扩展口开放。采用 **“行为型 Token + 内聚语义 `<Control>Behavior` 基类”** 两级模型：

1. **第一级：行为型 Token（标量和有限选项优先）**
   布尔开关、有限枚举、悬停延时、形变比例等差异声明在 `[ControlDesignToken]` 中（先例见 [ButtonBehavior.cs](../../../src/AtomUI.Desktop.Controls/Buttons/ButtonBehavior.cs) 读取的 `DisabledByOpacity`、`PressMoves`、`UnderlineLinkOnHover`），由 `.Themes.<Lang>` 中的 `[ControlTokenRule]` 写全（受 `ATOMUIGEN050` 编译期完整性保护）。供控件逻辑读取的交互默认值通过主题作用域按 `Style` 优先级投射，确保仅换纯视觉模板不丢失当前语言的交互默认值。
2. **第二级：内聚语义行为类 `<Control>Behavior`（按需处理组合手势与上下文决策）**
   - 当相同显式属性下，合法决策同时取决于当次命中语义区、手势类型、修饰键与节点只读状态的组合时，控件包声明唯一的 `public class <Control>Behavior : ControlBehavior<TControl>`，其 `Default` 静态单例 100% 实现 Ant Design 默认行为。
   - 注册表为每个控件选取**一个完整行为对象**。该对象的方法按 CLR 虚派发。需要沿用父语言已修改的决定时，显式继承其公开行为类型，或由子类显式委托给父语言实现。直接继承控件默认行为即选择其默认方法。Token 类型继承不参与行为对象内部的方法派发。
   - **四不准与故障隔离**：行为实例必须**无状态、不可变、跨实例共享单例**；严禁直接改写控件属性/伪类、严禁执行命令或发路由事件、严禁订阅原始输入或持有控件/模板节点引用、严禁强转内部部件类型。行为方法抛异常或返回未定义枚举值时，控件记录结构化诊断并立即中止当次未提交输入、清理自身捕获与临时按下态，绝不二次重放默认操作。

### 2.3 第三层：控件主题模板与自绘解耦（ControlTheme & Template）

1. **模板协作部件（`PART_*`）公开类型边界**：控件在 `implementation.md` 中明确列出 **必需部件（Required Parts）** 与 **可选部件（Optional Parts）**。语言模板不享有控件包的 `InternalsVisibleTo`，所有约定 `PART_*` 的目标类型、转换器与附加属性必须为 `public` 类型或 `public` 接口。`OnApplyTemplate` 重套模板时必须对称释放旧订阅、旧中继绑定与旧指针捕获。
2. **根控件自绘剥离强约束**：严禁在支持换模板的控件根类上通过 `public sealed override void Render` 硬编码特定设计语言的几何外观（如 [AbstractToggleSwitch.cs](../../../src/AtomUI.Controls/Switch/AbstractToggleSwitch.cs) 的胶囊轨道、[SplitButton.cs](../../../src/AtomUI.Desktop.Controls/SplitButton/SplitButton.cs) 的分隔背景、[TreeViewItem.cs](../../../src/AtomUI.Desktop.Controls/TreeView/TreeViewItem.cs) 的连接线）。特定语言几何外观必须下沉至默认模板内的轻量 `public` 呈现器基元，使语言替换 `ControlTheme` 时天然替换或移除原外观，彻底消除双重绘制。
3. **Semantic Part 跨语言模板同权校验**：正式分发的语言 `ControlTheme` 必须完整履行控件声明的全部公开 `[SemanticPart]`（保留 `.semantic-*` 标记、`ContractType` 兼容性、`Cardinality` 以及 `SelectorRoute` 的 `/template/` 边界跳数与 `.semantic-scope-*` 锚点），允许在锚点间自由重排布局容器。`AtomUI.Generator`（`ATOMUIGEN020-029`、`ATOMUIGEN041`）对语言包中的 `ControlTheme` 执行与主控件包同等的编译期阻断校验。

---

## 三、多设计语言动效与交互反馈体系

各类动效的执行归属、作用域覆盖、反馈尾动画释放与异常恢复见[多设计语言动效与替换契约（草案）](../../architecture/systems/motion/design-language-overrides.md)。

### 3.1 底层播放内核定界：复用统一执行内核，解耦上层规格与反馈模型

现有的底层播放内核（[MotionPlayback.cs](../../../src/AtomUI.Core/Animations/Motion/Playback/MotionPlayback.cs)、[MotionCompositionTransform.cs](../../../src/AtomUI.Core/Animations/Motion/Playback/MotionCompositionTransform.cs) 合成器直写平移/缩放/旋转、[MotionFrameDriver.cs](../../../src/AtomUI.Core/Animations/Motion/Playback/MotionFrameDriver.cs) 呈现时间预测节拍、[IMotionLayout.cs](../../../src/AtomUI.Core/Animations/Motion/IMotionLayout.cs) 自然视口展开、中途打断 `FromCurrent()` 平滑衔接与主题切换单帧收束 `MotionSettle`）**继续负责各语言的显式有限动效**。状态过渡沿用 Avalonia Transitions，持续反馈沿用专用后端及 MotionGate。设计语言复用对应的播放、收束和释放能力，禁止另建全局帧循环。

在此执行内核之上，将多设计语言动效拆解为四个正交可定制层，消除曲线私有、弹层/对话框轨迹硬编码、微状态过渡与编排时长耦合（见 [PrimeNGMotion.cs](../../../src/DesignLanguages/PrimeNG/AtomUI.Desktop.Controls.Themes.PrimeNG/Stages/PrimeNGMotion.cs) 还原补丁）及 [WaveSpiritDecorator.cs](../../../src/AtomUI.Controls/Primitives/WaveSpiritDecorator.cs) 单阶段常驻节点的限制：

```mermaid
flowchart LR
    subgraph Kernel["统一底层播放与收束内核（AtomUI.Core，全语言共用）"]
        MP["MotionPlayback · MotionCompositionTransform\nMotionFrameDriver · IMotionLayout · MotionSettle"]
    end

    L1["第 1 层：动效词汇与双维 TokenTransitions\n编排时长 vs 微状态过渡时长分离\n语义 Easing Token + 现有弹簧曲线"]
    L2["第 2 层：两阶段瞬态交互反馈（InteractionEffect）\n按下会话 + 提交激活 · AdornerLayer 对象池按需挂载\n承载 AntD 波浪 / Material 水波纹 / Fluent 缩放 / Shadcn 静默"]
    L3["第 3 层：组件编排动效方案（MotionScheme）\n公开 PlacementMotionContext + 标准语义槽位\n经 {atom:MotionResource} 实现四级无损替换"]
    L4["第 4 层：控件内部结构与持续微动效\n指示条 Slide/Stretch 模式 · 加载指示器槽位解耦"]

    L1 --> TR["Avalonia Transitions · TokenTransitions"]
    L2 --> MP
    L3 --> MP
    L4 --> ACTIVE["现有指示器呈现与持续反馈后端 · MotionGate"]
```

### 3.2 第一层：动效词汇层与双维 `TokenTransitions`

1. **分离“微状态过渡”与“组件编排动效”时长**：
   - 全局总开关 `EnableMotion` 为 `false`（或跟随无障碍减少动画）时，所有编排时长与微过渡时长统一归零，所有瞬态交互反馈短路跳过。
   - 保留组件编排时长 `MotionDurationFast`（Ant Design 默认 `100ms`）、`MotionDurationMid`（`200ms`）、`MotionDurationSlow`（`300ms`）、`MotionDurationVerySlow`（`400ms`）。
   - **保留组件与通道配方。** `StateTransitionDuration`、`StateTransitionEasing` 是语言可选用的微状态基准。控件取值按组件、状态以及发生变化的属性通道确定时长、缓动及延迟。Ant Design 6.7.0 的横向 Menu 边框/背景用 Slow（300 ms），一般 Button 的普通状态用 Mid（200 ms），Button 的宽度等专用通道可用 Slow。按该通道上游声明匹配曲线。Nora 可对微状态通道统一给零，打开 Select、Dialog 仍分别采用对应编排时长。每个已声明可覆盖的显式通道保持原有优先级。
2. **公开语义缓动曲线 Token 与弹簧物理曲线（`SpringEasing`）**：
   - 在 `DesignToken` 中声明 `MotionEaseOut`、`MotionEaseIn`、`MotionEaseInOut`、`MotionEaseEmphasized`（允许超调，Ant Design 对应 `OutBack (0.12, 0.4, 0.29, 1.46)`）。
   - 复用 Avalonia 12.2 的 `Avalonia.Animation.Easings.SpringEasing`。动效快照按类型与质量、刚度、阻尼、初速度四个参数保存，并隔离可变实例。规范化字面量 `Spring(1,120,12,0.5)` 必须能在动态与编译主题两条路径读回同一曲线。先修通当前解析与编译链，再讨论是否有引入第二种公共类型的需求。无量纲曲线采样不承诺反向播放时保留物理速度。
3. **升级 `TokenTransitions` 与 `AbstractMotion` 的 Token 感知**：
   - 扩展 [TokenTransitions](../../../src/AtomUI.Core/Theme/Resources/TokenTransitions.cs) 的 Easing 投射。缓存使用通道有序时长及冻结后的曲线值身份（类型与参数），保留工厂有界缓存；不同参数不可误共享，相同参数不得因换了对象引用而持续开辟缓存。
   - 在 [AbstractMotion.cs](../../../src/AtomUI.Core/Animations/Motion/AbstractMotion.cs) 上提供可选的 `DurationToken` 与 `EasingToken`（`SharedTokenKind?`），未显式赋值时自动从目标元素主题作用域读取对应档位 Token。

### 3.3 第二层：两阶段瞬态交互反馈系统（`InteractionEffect`）

以公开、语言可插拔、零常驻节点的 **`InteractionEffect` 两阶段协议** 全面替代 `WaveSpiritDecorator`：

```csharp
public abstract class InteractionEffect
{
    public static InteractionEffect None { get; } = new NoneInteractionEffect();

    public virtual IInteractionEffectSession? OnPressStarted(in InteractionEffectStartContext context) => null;
    public virtual void OnActivated(in InteractionEffectActivateContext context) { }
}

public interface IInteractionEffectSession : IDisposable
{
    void Complete(bool activated);
    void Cancel();
}
```

两种回调上下文均向外部实现提供**公开的 `Playback` 入口**及只读操作身份、输入来源、有效圆角、可选局部位置和已解析画刷。`OnActivated` 可以继续返回 void。任何超出回调寿命的绘制、淡出或合成动画都须经此入口取得宿主登记的租约。下列为公共协议的最小签名形态，正式落地时由无友元权限的消费包编译验收。

```csharp
public interface IInteractionEffectPlayback
{
    IDisposable PlayDecoration(Func<Control> createVisual, AbstractMotion motion,
        InteractionEffectClip clip);
    IDisposable PlayOnSurface(AbstractMotion motion);
}

public enum InteractionEffectClip { InsideShape, OutsideShape }
```

`Playback` 绑定到目标、操作与作用域代次。宿主先登记唯一租约，再调用工厂、挂载或启动。若工厂失败或同步重入使代次失效，则撤销部分连接。新启动的自然完成、取消和释放最终归还同一租约一次；租约直接复用 `IDisposable`，重复释放安全。按下会话保留其既有 Cancel 语义。调用者不保留返回值时，宿主仍跟踪到自然结束或主动撤销。按下会话的 `Complete(activated)` 与 `Cancel()` 互斥，其后淡出可以由独立租约继续。切语言、换根或回收容器时，撤销按下会话和全部旧租约。迟到的回调须核对租约身份，不得摘走已经借给下一次操作的节点。

按住保持型效果允许按下阶段在原呈现目标保留终值；同一操作在 Complete 中提交退出动画时，宿主在原目标接续并移交给独立尾租约，旧句柄的迟到 Dispose 不能影响退出请求。装饰节点不参与命中与焦点。`InsideShape` 取目标内轮廓与真实可见祖先裁切链的交集；`OutsideShape` 允许越过目标自身边界，仍按坐标变换与祖先视口、裁切和当前表面求交。滚动或变换期间更新裁切，目标完全滚出时不可见。`IsClipEnabled=false` 仅能放开目标裁切。`PlayOnSurface` 只申请允许的呈现通道，与已有变换冲突时按定义的降级路径收束。池只复用来源及表面能力匹配、已清空引用的节点。

| 设计语言范式 | `InteractionEffect` 实现 | 生命周期与渲染宿主（基于 [LayerLadder](../../architecture/systems/rendering/visual-layers.md)） | 静置视觉树节点数 |
| --- | --- | --- | --- |
| **Ant Design 6（默认）** | `AntDesignWaveEffect.Default` | `OnPressStarted` 返回 `null`；`OnActivated` 从 UI 线程有界池租借绘制节点，按需挂载至目标所在表面的 **`AdornerLayer`（T2 层，`IsClipEnabled=false`，`IsHitTestVisible=false`）**，经 `MotionFrameDriver` 播放外扩光环（支持连点多波叠加），结束后立即卸载归池。 | **0 个**（从所有默认模板移除 `PART_WaveSpirit`） |
| **Material 3 / PrimeNG Ripple** | `InkRippleInteractionEffect` | `OnPressStarted` 租借带 `(TargetSurface.Bounds, EffectiveCornerRadius)` 圆角裁剪的水波纹层，从 `PointerOrigin`（键盘取中心）扩散并在按住期间保持；`Complete(_)` 扩散至对角淡出后卸载归池，支持连点多波并存。 | **0 个** |
| **Fluent 2 / Apple 触感缩放** | `PressScaleInteractionEffect` | `OnPressStarted` 通过 `Playback.PlayOnSurface` 请求宿主驱动 `TargetSurface` 合成层 `Scale`（如 `1.0 -> 0.97`）；`Complete(_)` 经 `SpringEasing` 弹回 `1.0` 并释放通道。 | **0 个** |
| **Shadcn / PrimeNG Nora / 极简** | `InteractionEffect.None` | 空方法内联短路，零分配、零挂载。 | **0 个** |

`IsInteractionEffectEnabled=false` 使反馈短路。启用时按第 2.1 节读取实例上的有效 `InteractionEffect`。语言仅为框架默认层提供定义；缺席沿父语言链解析，显式 `None` 停止回退。应用清除本地来源后重新取当前有效值。

### 3.4 第三层：组件编排动效方案（`MotionScheme`）与 `{atom:MotionResource}`

1. **公开几何感知契约**：将 [PopupMotion.cs](../../../src/AtomUI.Desktop.Controls/Popup/PopupMotion.cs) 的私有准备逻辑升级为公开无状态 `PlacementMotionContext`（含 `Placement`、翻转标志、`FlowDirection`、`ContentBounds`、`AnchorBounds`、对话框点击坐标 `Point? TriggerOrigin`、`SurfaceCornerRadius` 与层级标志）与 `IPlacementAwareMotion`。
2. **标准组件编排语义槽位（`MotionSlot`）与公开入口对齐**：补齐 [DialogMotions.cs](../../../src/AtomUI.Desktop.Controls/Dialog/DialogMotions.cs)、[DrawerPanelMotion.cs](../../../src/AtomUI.Desktop.Controls/Drawer/DrawerPanelMotion.cs) 与 [FeedbackCardMotion.cs](../../../src/AtomUI.Desktop.Controls/Primitives/FeedbackStack/FeedbackCardMotion.cs) 所在组件的公开动效属性，建立全库标准 `MotionSlot` 集合：
   - **浮层与菜单族**：`SelectOpen/Close`、`DropdownOpen/Close`、`SubMenuOpen/Close`、`PopoverOpen/Close`、`TooltipOpen/Close`；
   - **模态与抽屉族**：`DialogOpen/Close`（Ant Design 取点击原点 `0.2↔1` 缩放；Shadcn/PrimeNG 可在各自 `MotionScheme` 中忽略 `TriggerOrigin` 切换为屏幕中心 `0.95↔1` / `0.7↔1` 缩放）、`DrawerOpen/Close`、`OverlayMaskOpen/Close`；
   - **展开与反馈族**：`InlineExpand/Collapse`、`MessageEnter/Leave`、`NotificationEnter/Leave`。
3. **`{atom:MotionResource}` 四级替换闭环**：
   默认 ControlTheme 通过 `<Setter Property="OpenMotion" Value="{atom:MotionResource SelectOpen}" />` 读取当前作用域槽位。语言内部依第 4.2 节逐槽解析，结果作为该 Setter 的默认来源。控件属性按原生优先级求值；本地显式 null 禁用对应方向，ClearValue 后从仍然有效的来源恢复。
4. **AOT 零反向保留保证**：`MotionScheme` 中的轨迹类型仅依赖 `AbstractMotion` 与值类型 `PlacementMotionContext`，**严禁引用 `Select`、`DatePicker`、`Dialog`、`DataGrid` 等具体控件 CLR 类型**，确保启用多语言 `MotionScheme` 在 NativeAOT 下对未使用控件零反向 root。

### 3.5 第四层：结构微动效与持续反馈解耦

1. **活动选中指示器运动模式（`IndicatorMotionMode`）**：为 `TabControl`、`Segmented` 与导航指示器提供可由 Token 驱动的 `None`（瞬切）、`Slide`（同速平滑滑动，Ant Design `Line` 页签与 `Segmented` 默认）与 `Stretch`（首尾异速拉伸滑动）模式。
2. **加载指示器槽位与有效可见性边界**：解除 [ButtonTheme.axaml](../../../src/AtomUI.Desktop.Controls/Buttons/Themes/ButtonTheme.axaml) 等模板对单一图标类的硬编码，通过主题可替换的加载指示器模板/资源提供；所有无限循环动画、骨架屏闪烁与忙碌进度条严格遵守[持续视觉工作生命周期](control-development-guidelines.md#持续视觉工作生命周期)，在自身或任意视觉祖先不可见时立即暂停。

---

## 四、包结构、静态登记、继承回退与 AOT 裁剪

### 4.1 统一四类包结构（不新增第五类包）

延续[设计语言架构 §3](../../architecture/systems/theming/design-languages.md#3-四类包与依赖规则)的四类包划分，**不拆分独立的行为包或模板包**：

| 包类别 | 命名约定 | 多设计语言架构下的完整职责 | 依赖方向 |
| --- | --- | --- | --- |
| 核心包 | `AtomUI.Core` | 全局 Token、语言/行为/动效核心抽象、单遍编译器、`MotionPlayback` 内核、`ThemeContext` 与五阶段事务 | 无上层控件/语言依赖 |
| 控件包 | `AtomUI.Controls`、`AtomUI.Desktop.Controls`、`DataGrid`、`ColorPicker`、`Extras` | 控件公共 API、状态机、`<Control>Behavior` 基类与默认实现、`AntDesignWaveEffect` 与 `AntDesignMotionScheme` 默认实现、Token 与 Semantic Part 声明、默认 `ControlTheme` | `AtomUI.Core` 及上游控件包 |
| 语言包 | `AtomUI.DesignLanguages.<Lang>` | `DesignLanguage<TLanguageToken>`、语言 Token、种子与全局推导（含语言默认动效时长/曲线）、具名主题、`Use<Lang>()` | `AtomUI.Core`（派生语言另引用基语言包） |
| 语言控件主题包 | `<控件包名>.Themes.<Lang>` | **必选**：该控件包全部控件 Token 的 `[ControlTokenRule]`；**可选**：该语言的 `MotionScheme` / `InteractionEffect` 覆盖、少量控件的 `[ControlBehavior]` 子类及语言级 `ControlTheme` 资产 | 对应控件包 + 对应语言包 + 上游控件包同语言 `.Themes.<Lang>` |

- **唯一接入心智模型**：**“启用了哪个控件包、用了哪种设计语言，就安装对应的 `<控件包名>.Themes.<Lang>`”**。
- **`ATOMUIGEN053` 边界演进**：继续严禁控件包引用语言侧、严禁语言包引用控件包、严禁在 `.Themes.<Lang>` 中声明新 `Control` 类型或新 `[ControlDesignToken]`；允许在 `.Themes.<Lang>` 中提供针对目标控件包既有控件的 Token 规则、行为子类、动效覆盖与 `ControlTheme` 资产。

### 4.2 各类登记的查找单位与缺省结果

同一个 `LanguageTokenType` 单继承链只决定查找登记的顺序。不同贡献采用下列终止规则。槽位冲突与协议不兼容在登记冻结或追加预检时诊断，结果不得取决于包加载顺序。

| 贡献 | 查找单位 | 查找到根仍缺失 |
| --- | --- | --- |
| 必需 Token 规则 | 当前控件 Token 类型及语言 Token 类型。派生语言可采用父类型规则，也可命中合法的语言无关规则 | 启用的控件有 Own Token 且无有效规则时，按现有覆盖校验报告缺少的取值包；候选失败，不借用独立 Ant 语言的数据。无 Own Token 的控件沿现有规则处理 |
| ControlBehavior | 按控件沿语言链选首个完整对象 | 采用控件默认对象。父语言行为改 A、子语言登记只改 B 的对象时，显式继承父行为应得到父 A / 子 B；直接继承默认行为应得到默认 A / 子 B |
| 模板、MotionScheme、InteractionEffect | 按模板身份/形态、MotionSlot 或反馈表面分别查找 | 采用控件包对该项声明的缺省实现。动效/反馈的显式 None 是已命中；其他槽位继续独立解析 |

取值全覆盖和摘要校验按[设计语言架构](../../architecture/systems/theming/design-languages.md)执行。有效配置不得因可选覆盖缺席而失败，也不得把已登记的无效贡献当作缺席。

### 4.3 NativeAOT 与精细裁剪硬门槛

1. **条件化 `TypeMap` 片段对齐**：`.Themes.<Lang>` 中针对具体控件 `TControl` 的 Token 规则、`<Control>Behavior` 子类与语言 `ControlTheme` 资产工厂，必须由 `AtomUI.Generator` 按目标 `TControl`（及其 Token/Semantic 触发类型）输出条件化 `TypeMap` 片段。
2. **正、负保留双门槛。** 使用真实 NuGet 消费、相同 SDK/RID/编译选项运行第 6.2 节的最小发布样例。未进入真实可达闭包的控件及其语言片段应消失；仅由 Token、Semantic Style 等合法条件触发的 owner、注册和相应资产必须保留并可用。[注册保留条件](../../architecture/foundations/control-registration-contracts.md)
3. **运行期惰性加载**：未激活语言的 `DeferredControlThemeResourceProvider`、未触发槽位的 `Motion` 实例与未发生点击的 `InteractionEffect` 池化视觉节点在启动与空闲态保持**零构造**。

### 4.4 按实际使用付费

**先从登记信息确定有效来源，再物化胜出的实现及其真实依赖。** 区分发布包保留、程序集加载、描述符登记、资源工厂调用、视觉树创建和持续运行。已启用包的契约索引、必要的兼容校验和控件本身的状态机都可能被使用；它们的成本应单独计量。用最终视觉树中“看不见”某个功能，不能证明它的构造、绑定或帧回调为零。

| 场景 | 必须守住的冷路径边界 |
| --- | --- |
| 只启用 Shadcn，某控件有完整独立的语言模板 | 第一次创建该控件，默认主题工厂 0 次、默认模板构建 0 次、语言模板构建 1 次。显式 BasedOn 默认主题时，允许解析被引用的基础样式；派生模板完全覆盖 Template 时，默认视觉树仍为 0 次。只覆盖 Template 属性的应用仍可消费默认 ControlTheme 的其他 Setter。 |
| 已登记的其他语言、可选控件包和 Token | 注册与完整性校验使用声明信息。枚举语言、查询键是否存在或创建一个按钮，不得执行其他语言或无关控件的取值规则、构造其模板与动效。独立语言不启动 Ant 语言包来取得控件默认实现。已知导出键的 miss 不应物化无关资产；外部未声明键全集的资源提供者仍按原生优先级查询。 |
| 行为覆盖、动效覆盖、显式 None 或 null | 先确定有效策略或动效槽位。选中语言实现或明确关闭时，不创建被遮蔽的默认会话、轨迹副本、订阅或计时器。CLR 基类的通用实现和业务状态机按实际依赖复用。清除显式覆盖后，届时首次需要的缺省实现必须能正确接管。 |
| 可选图标、加载/完成反馈、弹层重内容和虚拟化容器 | 状态从未进入时，不求值可替换的具体图标/装饰工厂，不启动其专用监听、动画或候选数据视图。必要的模板壳、公开部件锚点和可访问性节点维持原契约。用声明式延迟内容或已有宿主承载具体实现；调整既有 Semantic Part 形态前先验证其生成样式、布局和交互。 |
| 实例、局部作用域和热切换 | 只更改 Token 或行为时，已选模板身份不变则不重建视觉树。等价作用域复用有效结果。未被该次变更影响的控件不重挂监听；旧目标、动作租约和已结束会话按生命周期释放。首次使用后是否保留缓存由已有所有权、容量与再次使用成本决定。 |
| 图标、字体、预设与 NativeAOT | 未启用的设计语言及可选包不因默认注册表产生反向保留。实际选中模板引用的字形、字体或共享字典是有效依赖，按资源归属计账。第 6.2 节的正、负可达性用例必须与运行时工厂计数分别通过。 |

每项验收复用同一个最小宿主，测试侧计数即可，不给生产热路径增加计数器。冷进程先测完全覆盖，再测缺省、显式关闭和首次启用；计数器应设在真实工厂、模板构建与活动订阅处。不能为消除一个廉价对象而给每个控件新增一套惰性服务、字典或永久监听。

---

## 五、作用域解析与运行时协调切换

### 5.1 在第一次消费时选定实现

**默认实现与语言实现都是候选资产。** 控件声明所需能力、拥有状态机。构建期记录每个候选的来源、身份和工厂；作用域在主题事务中选定身份；使用方在确实需要时执行选中工厂。应用只启用 Shadcn，且 Button 有独立 Shadcn 主题时，默认 Button 资产没有消费边。显式 BasedOn、用户直接请求基础主题或其他控件使用它时才产生这条边。

**具体实现落点**

1. **构建端生成真实依赖。** 在现有 Control 注册片段和资产导出索引中登记默认与语言候选、公开键、形态及显式依赖。索引项保留已有 AssetId/工厂入口，禁止登记时通过构造 ControlTheme、Behavior、Motion 或 icon 得知其身份。按键只读索引，从选中的片段才进入 GetRegistration 和资源工厂；开放资源提供者保留原有查询语义。TypeMap 的精细裁剪仍按真实引用闭包运行。[资产登记](../../../src/AtomUI.Core/Registration/ControlThemeResourceRegistration.cs) · [启动索引](../../../src/AtomUI.Core/Registration/StartupPackageIndex.cs)
2. **事务只提交有效选择。** 准备时在现有 ThemeContext/快照结构中记录各类贡献的有效条目标识，沿第 4.2 节规则处理缺省、继承与显式关闭。共享父结果的作用域只需记录差异，避免为每个子作用域展开整张控件表。冻结、完整性校验、NoOp 比较均无需逐项执行资产工厂。物化仍由现有按 Application 管理的资源设施负责，部件与播放会话归实际实例。无需新增跨控件服务定位器、每控件的选择字典或第二套资源缓存。
3. **使用方只消费有效条目。** 未提供语言覆盖的类型保留现有原生路径。有覆盖时，冷启动、热切换、重挂载及清除显式值须共用一次来源判定。有效主题身份不变时保留视觉树。切换结果为 None 时不进入效果工厂；清除覆盖后才按当前作用域重新选择。失败的候选按现有事务与诊断规则处理，不能通过先装好一套默认实例掩盖失败。

**模板的根问题在 Avalonia 的首次隐式查找。** Avalonia 12.2.0 的 StyledElement 在逻辑挂载时先执行 ReevaluateImplicitTheme，再执行 ApplyStyling；后者也先处理 ControlTheme，再处理普通 Style。TryFindResource 在每个宿主上调用 TryGetResource(key, variant)，应用级资源提供者的这个入口收不到起始控件。同一 Light 变体下两个语言作用域不能只靠 variant 区分。普通资源查询返回的是值，调用它来判断默认资产“是否存在”可能已经执行工厂。

目标顺序如下。此处是框架与 Avalonia 集成必须满足的语义，具体属性优先级和样式匹配仍由 Avalonia 执行。

~~~text
解析该元素 Theme 属性的有效来源，包括模板来源和适用的应用 Style
  有有效显式主题 → 交还原生来源，结束框架默认选择
  需要隐式主题   → 沿原生资源优先级查找其 StyleKey 或既有形态键
    命中用户资源       → 使用该资源
    命中 AtomUI 资产入口 → 从查找起点取得有效 ThemeContext
                         → 读取已提交的条目标识 → 只物化该条目及显式依赖
首次实际需要视觉内容 → 构建有效 Template
~~~

- **单语言最短路径。** 在框架资源原有优先级位置，让该公开键的延迟出口直接指向选中的语言资产；默认资产保留自己的身份，仍可作为真实依赖被请求。语言模板 BasedOn 默认主题时，基础 Setter 可以来自默认主题；只有最终有效的 Template 构建视觉树。
- **双作用域与应用覆盖。** 查找起点、公开键和当前上下文必须在框架资产原本应命中的位置共同参与选择。不得往每个局部 Provider 添加一个抢占用户同名资源的普通类型键字典。框架基线 Style 如用于响应热切换，仍须位于 Application.Styles 的应用普通样式之前，并且不能充当冷启动的第二次主题选择。显式用户 Theme/Template、StyleKey 资源、命名资源、清除覆盖及 TypeMap 保留语义都要保留；不得用 LocalValue、反射清缓存或卸载再挂载规避。
- **底层接入决策。** 推荐的接入点是原生有效主题选择与隐式资源取值的交界。分两步实现：先由样式系统决定当前控件的 Theme 来源，按需推迟隐式资产的求值；确需框架隐式主题时，将查找起点和键传到既有框架资产入口。该入口读取起点对应的 ThemeContext 并返回选中资产。单独给资源 provider 增加上下文不能改变取值时机，单独调整 Style 顺序也不能给应用级 provider 补出查找起点。冷启动和热更新复用相同选择逻辑，只在有效资产身份改变时按正常优先级失效。未开放语言模板覆盖的类型也可能被应用完整覆盖，该场景同样检查落选工厂。
- **实现边界。** 能在 AtomUI 一侧通过公开机制满足同一合同即可采用；如需改变 Avalonia 的调用顺序或增加可选解析入口，应单独评审依赖变更并在两个项目边界做回归。当前两种原型各自只满足部分条件。不能以全局替换资源遍历、私有字段反射、预写 LocalValue 或逐控件补救替代该接入。完整方案须先通过第 6.2 节联合门禁，再推广到控件家族。

**同一机制在其他来源的落点**

| 现有早建来源 | 所有权与创建时刻 |
| --- | --- |
| 默认 Behavior 在控件构造或 attach 时先创建 | 控件直接执行公共交互流程。需委派的决定读取已选的完整行为对象；默认和覆盖对象都按所选条目取得。旧处理器承担的必要业务职责先归回控件，未选处理器不建立输入订阅。 |
| 静态预设表一次构造所有 Motion，Flyout 先创建默认值再设置 Style | 表中存静态工厂入口，各槽独立取用。把工厂留到有效默认值真实被读取或播放时执行，服从原有公开属性、GetValue/绑定、显式 null 和 ClearValue 语义。避免在 SetValue 的实参里提前调用工厂。 |
| 模板一次建好互斥状态的具体反馈、加载图标、完成图标 | 控件维护状态，模板保留必要语义锚点，以现有模板或内容工厂声明各状态的呈现。具体内容由当前可呈现状态或已有公共契约的真实读取触发。反馈为 None 时不建立波浪指标订阅；触发时由既有租约机制创建。保留第一次显示的布局与图标样式命中。 |
| 查询 miss、打开语言菜单或切换无关 Token 引发物化 | 生成资产以完整键索引排除无关工厂；菜单读描述符，Token 读已选控件的现有惰性槽。运行时只使有效依赖改变的消费者失效，清理或复用沿原有 owner 生命周期执行。 |

### 5.2 五阶段原子事务与进行中交互/动效交接

完整复用[主题系统运行时 §12](../../architecture/systems/theming/runtime.md#12-事务与发布顺序)的 `Capture -> Prepare -> CommitCore -> Publish -> Complete` 事务：

- **统一原子提交**：每个 `ThemeContext` 在 `CommitCore` 一次性原子交换 `(ThemeSnapshot, EffectiveThemeTable, EffectiveBehaviorTable, EffectiveMotionScheme, EffectiveInteractionEffects)` 引用。`NoOp` 相等性判据完整覆盖上述五项身份。
- **切换期动效与反馈收束。** CommitCore 只交换已准备好的引用，不调用用户代码、释放捕获或挂接部件。Publish 交接受影响控件，撤销旧按下会话与所有激活后租约，再接受新输入。MotionSettle 仅用于其支持的提交窗口内新动画，不替代租约注销。失败、NoOp、Superseded、Committed 沿用现有事务语义。
- **已开始的输入。** 模板或策略变更要使旧手势代次失效，取消相关捕获和延时，不能用旧的松开事件在新目标上合成点击。
- **IME 保护单位是编辑宿主的替换边界。** Prepare 识别当前 pre-edit 所依赖的编辑器、原生 IME client、presenter、焦点及其祖先模板链。凡本次替换会拆除其中任一对象的祖先，都必须纳入保护；多个嵌套替换点由覆盖全部拆除路径的最小边界统一交接。优先保持稳定编辑宿主身份。无法保持时，在该边界的有效呈现入口延迟破坏性重套，把旧模板、解析所依赖的 Token/作用域资源、策略和输入目标作为一个完整版本固定。仅冻结最内层 TextBox 或仅推迟 OnApplyTemplate 均不合规。
- **局部例外与追平。** 整窗事务按常规提交，不等待组字。首个可交互帧上，边界外使用新版本，受保护边界内继续使用完整旧版本。连续请求只保留最新待应用版本；原生确认或取消到达且文本更新完成后，在 UI 线程一次追平并释放保留的引用。失焦、卸载和主动关闭按正常平台/业务生命周期收尾，不以丢弃预编辑或伪造提交完成切换。只在有活动组字且确实会拆宿主时保留该边界所需的既有引用，普通控件无逐实例保留表。保护边界无法实现隔离的控件，须先把编辑宿主移出可替换子树再启用此能力。

---

## 六、Ant Design 100% 复现标准、验收门禁与迁移路线

### 6.1 Ant Design 6.7.0 对标与分母冻结口径

1. 每个在 Ant Design 存在对应物的控件在 `implementation.md` 锁定上游参考基线（当前 **Ant Design `6.7.0`**），建立 **API 映射表、交互状态机表、视觉度量表与动效轨迹/曲线对照表**（含 `Wave` 触发条件、弹层原点与曲线控制点）。
2. 验收前先冻结全部适用场景分母（尺寸档、紧凑密度、亮/暗色、各交互伪类、受控/非受控、键盘流、入离场与波浪动效）；未实现、未验与失败项均保留在分母中，仅全部适用项通过证据齐全时方可声明 100% 复现。

### 6.2 必败红灯（Red-Green）验收门禁

开放多语言定制的控件家族与运行时底座必须通过以下可失败场景验证（具体控件临时测试按[测试价值与生命周期规范](test-value-and-lifecycle.md)在验收后清理，框架核心契约测试永久保留）：

| 验收维度 | 注入场景与故障条件 | 必须满足的判定标准 |
| --- | --- | --- |
| **正交替换（行为×模板×动效）** | 同一份业务 AXAML 分别运行默认与语言覆盖的行为、模板、`MotionScheme` 与 `InteractionEffect` 组合 | 业务事件次数、命令执行与受控绑定结果完全一致；显式属性（含 `OpenMotion={x:Null}`、`IsInteractionEffectEnabled=False`）始终优先，清除后回落当前作用域。 |
| **交互反馈零常驻节点与多形态** | 实例化 100 个静置 `Button` / `CheckBox` / `ToggleSwitch`；分别在 Ant Design（外扩波浪）、Material/PrimeNG（指针坐标水波纹，含连点双波）、Fluent（按压缩放）与 Shadcn（无反馈）下触发点击/按住取消 | 静置态视觉树中 **0 个波浪/水波纹装饰节点**；Ant Design 仅在有效激活后挂载 `AdornerLayer` 并在结束后完全回收；水波纹准确从按下局部坐标扩散并在取消时淡出回收。 |
| **状态过渡与编排时长解耦** | 用锁定的 Ant 6.7.0 对照横向 Menu 的背景/边框与普通 Button 状态过渡；切到 Nora 后触发按钮悬停与 Select 开闭 | Ant Menu 为 Slow（300 ms），Button 的普通状态为 Mid（200 ms），曲线/延迟逐通道匹配上游。Nora 微状态为 0 ms，Select 仍按其编排配方播放；显式关闭开启动效只影响对应方向。 |
| **同窗多作用域与内层回退** | 同一窗口内根作用域为 Shadcn（中心缩放 Dialog + 锚点微移弹层 + 自定义模板），局部 `ThemeConfigProvider` 为 Ant Design（点击原点缩放 + 外扩波浪 + 默认模板） | 两个局部区域各自呈现对应语言的模板、行为、弹层轨迹与点击反馈，内层默认模板与默认动效不向上穿透残留外层覆盖。 |
| **交互中切换与 IME** | LineEdit 活动组字时分别替换自身及包含它的复合祖先模板，连续请求 B、C；分别覆盖确认、取消、失焦与卸载 | 边界外按常规提交，边界内旧宿主、焦点、client、pre-edit 文本/光标及读到的呈现版本一致。确认只写回一次、取消不写入临时串，只追平 C；无旧请求重放或引用残留。原生输入法必须实测，Headless 注入只验证树和交接逻辑。 |
| **原生优先级** | 外层框架默认、应用普通 Style、局部语言切换、应用局部/触发 Style、模板输入、显式绑定与 ClearValue | 逐次断言 Theme 对象身份、实际模板部件与绑定是否存活；移除覆盖回到当前有效来源。另验按 StyleKey 覆盖的用户资源、同窗双作用域及追加语言包。 |
| **未使用路径的成本** | 冷启动分别测独立覆盖、无覆盖、显式 BasedOn、应用普通 Style 指定完整主题、效果 None。另测同窗外层 Shadcn/内层默认，在 Window 或 Application 提供用户类型键，再移除用户覆盖 | 先断言源、再断言工厂和树：独立覆盖为默认 0/0、语言 1/1；用户完整主题胜出且无显式依赖时，框架默认与语言均 0/0。移除覆盖后两区首次实际用到的资产分别构建。BasedOn 允许基础主题工厂 1 次，替换 Template 时基础树仍为 0。按 §4.4 另断言未选行为、效果和状态内容无专用运行资源；只变 Token 不重建同一模板。 |
| **三类回退** | 独立语言缺必需 Token 规则；派生语言省略模板/动效；父行为改 A、子行为只改 B | 缺规则须报语言与取值包并拒绝候选；可选槽逐项继承，None 不穿透；分别断言未登记子行为、显式派生父行为、直接派生默认行为的 A 和 B。 |
| **尾动画租约与祖先裁切** | 无友元包在 OnActivated 经 Playback 启动淡出，不持有返回值；切语言、归池、同节点再租、注入旧完成回调；反馈跨滚动视口边缘 | 新效果继续播放，旧回调无效。最终活跃租约、订阅与装饰子节点回到基线。内外反馈都不得绘到祖先不可见区域或影响命中。 |
| **弹簧往返** | 动态主题读取规范化字面量、快照冻结、真实主题包编译后独立加载，同一组参数走完三种路径 | 按动效替换契约的弹簧门禁核对类型、参数、采样、缓存与可变对象隔离。任何一条路径拒绝合法字面量都未通过。 |
| **NativeAOT 正负保留** | 用下述同配置消费样例运行真实发布产物 | 必须同时满足合法引用生效和无关片段裁除。 |
| **无泄漏** | Gallery 反复切语言 50 次，包含编辑、弹层与池化装饰 | 旧窗口和控件可回收，活跃订阅/租约和 AdornerLayer 活跃子项回到基线；允许有界池留下不持有旧目标的空闲节点。 |

**NativeAOT 发布样例的判定。** 使用相同版本、SDK、RID、构建配置与启用的语言包，用真实 NuGet 独立消费。记录包身份、生成片段、最终 ILC 可达性证据和可执行产物的运行结果。托管 DLL 大小或对二进制搜索类型名字不能独立证明保留关系。

- **负例**：最小 Button 应用中运行其默认与语言覆盖行为/模板/反馈。选择确实无任何直接或间接合法引用的对照控件，核对 ILC 闭包内无其专属控件片段、规则与工厂。不可分割的共享资产按真实依赖计入预期。
- **正例**：对一个目标控件建立两份隔离输入，一份只引用其强类型 Token 资源入口，另一份只引用生成的 Semantic Style。两者均不得用显式 `typeof(T)`、`new T()` 或测试反射人为保根。分别运行发布产物，通过公开登记/资源入口断言正确 owner、专用片段及可用资源。只发出裸整数的枚举不构成 Token 引用。
- **实际呈现正例**：在另一个允许创建目标控件的消费场景中，用第三方 NuGet 贡献真实模板、动效和专用部件样式，在 NativeAOT 产物里切换语言，断言有效部件、样式命中与播放终态。此场景证明功能可用；前两份隔离输入单独证明仅靠相应条件即可保留。

#### 启动性能准入

首次装载机制合入前必须通过[启动系统既有验收线](../../architecture/systems/startup/verification.md#2-验收线)。Release JIT Gallery 从启动到**完整 Shell 首个渲染批次**的配对中位差（候选减基线）须 ≤ 0 ms；NativeAOT Gallery 按同机交错采样须不劣于上一正式版本。保留既有完整首帧、内核 Commit 和响应性门槛。工厂调用为零、内存下降或 Splash 提前出现都不能代替完整窗口的耗时证据。

- **设计约束。** 在构建期生成导出键和条件依赖。启动只登记必要元数据、执行既有完整性校验、准备当前使用的语言。不得为了实现回退而在启动期构造所有候选、为所有控件安装主题绑定，或在 UI 线程遍历窗口补套主题。启动阶段与控件首次使用共用同一选择结果。
- **成对比较。** 在实施前固定基线产物和场景，以同机、相同 SDK/RID、窗口树及渲染后端比较基线和候选。复用[原生启动采样器](../../../tools/performances/AtomUI.StartupPerformance/README.md)，各变体至少 10 轮独立进程交错运行，预热不计，保留全部样本。用 Ant-only、三语言已启用但只使用 Ant、Shadcn-only 三种配置分别做同场景前后对照，禁止用不同语言之间的绝对时间判断重构收益。现有 `--all-languages` 仍以 Ant 启动；Shadcn-only 复用同一工具的临时配置，两侧确认实际启用语言。
- **防止延迟转移。** 原生基准记录 `Launch.ToFirstRenderedBatch`、`Launch.ToFirstUiPump` 与 `Mark.MaxUiGap`；Gallery 用 `Launch.ToShellFirstBatch` 验收。Shadcn-only 首屏放一个真实使用的代表控件，两侧保持等价内容，并确认首个完整批次已经呈现目标内容。若改动把工作移到首次输入或首次打开，则只对该操作补测。超过既有验收线，或出现超出同基线噪声且可重复的首次使用退化，应先按 `Phase.*` 和控件首次物化记录定位，修正后再推广。噪声不足以作出判定时记录为待验。

### 6.3 渐进式落地路线

1. **阶段一：核心运行时与动效底座（`AtomUI.Core` + `AtomUI.Controls` + `AtomUI.Generator`）**
   按第 2.1 节迁移中性命名。接通组件状态过渡配方、两维 TokenTransitions 及现有 SpringEasing 的动态/编译值链。按第 3.3 节实现双阶段效果与宿主租约，按第 4.2 节区分规则和可选覆盖，按第 5.1 节接入默认主题层，按第 6.2 节验证 TypeMap 正、负闭包。
2. **阶段二：存量硬编码清债与典型控件试点**
   - 清理按钮、开关、复选/单选框与步骤条模板中的常驻 `PART_WaveSpirit`，接入统一 `InteractionEffect`；
   - 为 `Dialog`、`Drawer`、`Notification`/`Message` 补齐标准动效槽位接入，退役硬编码内部轨迹类，移除 `PrimeNGMotion.Restore` 补丁；
   - 完成 `SplitButton`（双目标交互 + 模板替换）、`ToggleSwitch`（剥离根 `Render` 胶囊自绘）与 `TreeView` / `TabItem`（状态机内聚 + 消除 `Theme` 本地值写入）三组试点验收。
3. **阶段三：多设计语言真实包对照与分批家族认证**
   在 `AntDesign`（100% 6.7.0 基线核验）、`Shadcn`、`PrimeNG` 及独立外部无友元包上完成端到端渲染、动效帧率与 NativeAOT 体积棘轮验收，按控件家族分批开放认证。
