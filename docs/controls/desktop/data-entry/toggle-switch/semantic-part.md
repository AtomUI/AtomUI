# ToggleSwitch Semantic Part 契约

本文档定义 ToggleSwitch 对应用公开的 Semantic Part、选择器、类型约束、数量语义和定制边界。ToggleSwitch 的整体设计见
[ToggleSwitch 桌面版架构设计](overview.md)，真实模板与生命周期见 [ToggleSwitch 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

ToggleSwitch 主控件公开 `root`、`content` 与 `indicator` 三个职责区域，与上游稳定 Semantic DOM
（`root` / `content` / `indicator`）对齐。`indicator` 对应滑动把手区域，由模板中的 `SwitchKnob` 节点承载；`content`
对应开关内部 on/off 内容区域，由模板中的两个 `ContentPresenter`（`PART_OnContentPresenter` 与
`PART_OffContentPresenter`）承载。

`AbstractToggleSwitch`、`SwitchKnob` 与 `WaveSpiritDecorator` 均不持有独立 Semantic descriptor：

- 上游 Switch 只提供一个 owner 的 Semantic DOM（`root` / `content` / `indicator`），`AbstractToggleSwitch` 是跨平台共享
  基类，不是对应用公开的独立 owner，因此不为它声明 descriptor。
- `SwitchKnob` 是 internal 类型，不能作为公共 descriptor owner；其把手职责通过 `ToggleSwitch` 的 `indicator` Part 对外
  公开，并以 `TemplatedControl` 作为最低 ContractType。
- `WaveSpiritDecorator` 是 checked 变化时的视觉反馈 actor，不是用户可定制的公共区域，不公开 Part。

因此本控件的 Semantic Part 只由 `ToggleSwitch` owner 公开。

### 1.1 `ToggleSwitch`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `ToggleSwitch` |
| Part | `root` |
| Selector | ToggleSwitch 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `ToggleSwitch` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | ToggleSwitch owner |
| 职责 | ToggleSwitch root 是开关值、状态、内容与根视觉样式的统一 owner。 |
| 相关 API | `IsChecked`、`GrooveBackground`、`OnContent`、`OffContent`、`OnContentTemplate`、`OffContentTemplate`、`SizeType`、`IsLoading`、`IsMotionEnabled`、`IsWaveSpiritEnabled`、`TrackHeight`、`TrackMinWidth`、`TrackPadding`、`KnobSize` |
| 相关 Token | ToggleSwitchToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `content`

| 字段 | 值 |
| --- | --- |
| Owner | `ToggleSwitch` |
| Part | `content` |
| Selector | `.semantic-content` |
| SelectorRoute | `/template/ .semantic-content` |
| Style Type | `ToggleSwitchContentStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | On/Off 内容 `ContentPresenter` |
| 职责 | 统一表示开关内部 checked / unchecked 内容区域的文本与视觉职责。 |
| 相关 API | `OnContent`、`OffContent`、`OnContentTemplate`、`OffContentTemplate` |
| 相关 Token | `ContentIconSize`、`ContentIconSizeSM`、`ExtraInfoFontSize`、`ExtraInfoFontSizeSM`、`InnerMinMargin`、`InnerMaxMargin` |
| 稳定性 | stable since 6.2.0 |

#### `indicator`

| 字段 | 值 |
| --- | --- |
| Owner | `ToggleSwitch` |
| Part | `indicator` |
| Selector | `.semantic-indicator` |
| SelectorRoute | `/template/ .semantic-indicator` |
| Style Type | `ToggleSwitchIndicatorStyle` |
| ContractType | `TemplatedControl` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 滑动把手 `SwitchKnob` |
| 职责 | 统一表示开关的滑动把手视觉职责，含把手填充、阴影与 loading 指示。 |
| 相关 API | `IsChecked`、`IsLoading` |
| 相关 Token | `HandleBg`、`HandleShadow`、`HandleSize`、`HandleSizeSM`、`SwitchColor`、`OffStateLoadIndicatorColor` |
| 稳定性 | stable since 6.2.0 |

`root` 是隐式 Part，不添加 `.semantic-root`。`ContractType` 只定义 Setter 可以稳定依赖的最低 public 类型，并通过
`x:SetterTargetType` 提供 AXAML 编译期类型上下文；它不参与 `.semantic-*` 的身份匹配。`indicator` 的 `ContractType` 为
`TemplatedControl` 而非 `SwitchKnob`，因为 `SwitchKnob` 是 internal 类型，不能作为公共 Setter 依赖的最低类型。
`content` 的 `Cardinality` 为 `Multiple`：模板中始终存在 on 与 off 两个内容 `ContentPresenter`，二者是同一 content
职责的两个替代节点，不随 `IsChecked` 增删。

## 2. Part 说明

### 2.1 root

`root` 是 ToggleSwitch owner 本身，在 ToggleSwitch 实例的整个生命周期内始终存在，并且每个 ToggleSwitch 恰好一个。

它负责：

- 承载 `IsChecked`、`OnContent`、`OffContent`、`SizeType`、`IsLoading`、`IsMotionEnabled`、`IsWaveSpiritEnabled` 等公共状态。
- 承载 `:checked`、`:unchecked`、`:indeterminate`（`IsChecked` 为 `null`）、`:pointerover`、`:pressed`、`:disabled` 等伪类。
- 提供轨道背景（`GrooveBackground`）、整体前景色（`Foreground`）、对齐、光标和整体透明度等根视觉属性。
- 承载轨道与把手的几何属性 `TrackHeight`、`TrackMinWidth`、`TrackPadding`、`KnobSize`，对应 antd Switch `ComponentToken`
  的 `trackHeight` / `trackMinWidth` / `trackPadding` / `handleSize`，是 root 的 min-width、height 等基础尺寸样式在
  AtomUI 中的公开映射。
- 作为 `content`、`indicator` owner-scoped Selector 的作用域边界。

适合通过 root 定制 ToggleSwitch 整体前景色、透明度、光标、对齐状态与几何尺寸。需要根据 checked、hover、disabled 状态
改变轨道背景时，应在 owner Selector 上组合公开属性或伪类；默认主题已按这些状态设置 `GrooveBackground`，Semantic Style
覆盖时作用于同一属性。几何属性允许负的 `TrackPadding`：此时把手按 `TrackPadding` 偏移出轨道边界，用于复刻 MUI 一类
"把手大于轨道、越界外露"的开关形态（见 [§3 Selector 用法](#3-selector-用法) 的示例）。

root 不表示模板中的 `PART_MainContainer`（`Canvas`）、`PART_WaveSpirit` 或任何内部节点；这些节点的名称、数量和层级不
属于 root 契约。轨道胶囊本身由 owner 的 `Render` 直接绘制，不是独立的模板节点。

### 2.2 content

`content` 表示开关内部 checked / unchecked 内容区域，每个内置模板恰好两个 `ContentPresenter`（on 与 off），cardinality
为 `Multiple`。`OnContent` / `OffContent` 可以为 `null`，此时对应 `ContentPresenter` 通过 `IsVisible` 绑定被隐藏，但节点
仍属于模板稳定结构，Part 身份与数量不变。

它负责：

- 展示 `OnContent` / `OffContent` 与对应 `ContentTemplate` 的最终结果。
- 承载内容前景色、透明度、排版等局部视觉覆盖。
- 在 checked、unchecked、loading、disabled 下保持同一 Part 身份。

适合定制 `Foreground`、`FontSize`、`FontWeight`、`FontStyle`、`Opacity` 等文本与视觉属性。on/off 内容的可见位置由
owner 的 `CalculateElementsOffset` 计算，经 TemplateBinding 传给内部内容画布，由画布统一执行 `Arrange`，因此 `Margin`、对齐等定位型 Setter 不作为
公共定制路径；需要调整内容与把手的间距时，应通过 `InnerMinMargin` / `InnerMaxMargin` 等 Token 或主题分支实现。

content 不公开内容模板生成的用户子树、文本内部 presenter、内容值的具体 CLR 类型，也不承诺 on/off 两个 presenter 之间
的固定层级或顺序。

### 2.3 indicator

`indicator` 表示开关滑动把手区域，每个内置 ToggleSwitch 模板恰好一个，cardinality 为 `Single`。checked 与 unchecked 是
同一把手节点通过 owner 布局切换位置，不是两个替代实现节点，因此不因 `IsChecked` 增删 marker。

它负责：

- 为把手提供一致的局部视觉入口。
- 承载把手的视觉状态，包括 checked/unchecked 两端对齐、按下拉伸和 loading 指示。

`indicator` 的 `ContractType` 是 `TemplatedControl`，把手填充色由标准的 `Background` 属性驱动：主题通过
`HandleBg` Token 设置 `Background`，`.semantic-indicator` 直接定制 `Background` 即可改变把手填充色。把手的阴影同样
可定制：主题从 `HandleShadow` Token 派生 `Effect`，并以低于用户样式的优先级写入，因此 `.semantic-indicator` 设置
`Effect` 可以替换默认阴影。把手的几何尺寸（`KnobSize`）与位置基准（`TrackPadding`）通过 root 的公开几何属性驱动，
见 §2.1。

indicator 不公开 `SwitchKnob` 具体控件类型、其 `Render` 绘制细节、loading arc 的旋转角度与几何、内部 `Name` 或按下拉伸
phase。

## 3. Selector 用法

应用级样式先限定 ToggleSwitch owner，再通过生成的 Semantic Style 进入 Part。生成类型已经封装 owner 类型保护和
`SelectorRoute`，用户不需要复制模板路径：

```xml
<Application.Styles>
    <Style Selector="atom|ToggleSwitch">
        <atom:ToggleSwitchContentStyle x:SetterTargetType="ContentPresenter">
            <Setter Property="FontWeight" Value="SemiBold" />
        </atom:ToggleSwitchContentStyle>

        <atom:ToggleSwitchIndicatorStyle x:SetterTargetType="TemplatedControl">
            <Setter Property="Background" Value="#1976d2" />
        </atom:ToggleSwitchIndicatorStyle>
    </Style>
</Application.Styles>
```

对特定 ToggleSwitch class 或状态定制时，把 class、属性或伪类放在 owner 一侧：

```xml
<Style Selector="atom|ToggleSwitch.semantic-custom:checked">
    <Setter Property="GrooveBackground" Value="#801976d2" />
    <atom:ToggleSwitchContentStyle x:SetterTargetType="ContentPresenter">
        <Setter Property="Foreground" Value="#1677ff" />
    </atom:ToggleSwitchContentStyle>
</Style>
```

几何定制与 antd Switch `style-class` 演示一致：MUI 形态的开关用 14px 高、最小 32px 宽的轨道承载 20px 的圆形把手，
把手经负 `TrackPadding` 越出轨道边界，`.semantic-indicator` 只负责把手自身的填充与阴影：

```xml
<Style Selector="atom|ToggleSwitch.semantic-mui">
    <Setter Property="TrackHeight" Value="14" />
    <Setter Property="TrackMinWidth" Value="32" />
    <Setter Property="TrackPadding" Value="-3" />
    <Setter Property="KnobSize" Value="20,20" />
    <atom:ToggleSwitchIndicatorStyle x:SetterTargetType="TemplatedControl">
        <Setter Property="Background" Value="#1976d2" />
        <Setter Property="Effect">
            <DropShadowEffect OffsetX="0" OffsetY="1" BlurRadius="4" Color="#4D000000" />
        </Setter>
    </atom:ToggleSwitchIndicatorStyle>
</Style>
<Style Selector="atom|ToggleSwitch.semantic-mui:checked">
    <Setter Property="GrooveBackground" Value="#801976d2" />
</Style>
```

把手越出轨道依赖模板把手节点位于内容裁剪 `Canvas` 之外，属于内置模板的结构保证，用户不得通过 `/template/` 重建。

不得把 `ContractType` 写入 Part Selector。以下写法不属于公共契约：

- `TemplatedControl.semantic-indicator` 或 `:is(TemplatedControl).semantic-indicator`。
- `ContentPresenter.semantic-content` 或 `:is(ContentPresenter).semantic-content`。
- 直接复制 `/template/ .semantic-*` route 作为用户主路径；route 只属于 descriptor 与生成 Style 的实现元数据。
- 连续穿过子控件模板的多个 `/template/`。
- 依赖 `PART_*`、internal 类型、Name 或视觉祖先顺序。

## 4. 状态与数量语义

| 状态 | root | content | indicator | 说明 |
| --- | --- | --- | --- | --- |
| unchecked | 1 | 2 | 1 | 把手位于左侧，off 内容可见，marker 数量不变。 |
| checked | 1 | 2 | 1 | 把手位于右侧，on 内容可见，marker 数量不变。 |
| `IsChecked` 为 `null` | 1 | 2 | 1 | 语义上按 unchecked 处理，marker 数量不变。 |
| `OnContent` / `OffContent` 为 `null` | 1 | 2 | 1 | 对应 presenter 隐藏但仍存在，Part 身份与数量不变。 |
| disabled | 1 | 2 | 1 | 整体透明度降低，不增删 marker。 |
| loading | 1 | 2 | 1 | 把手内显示 loading 指示，不增删 marker。 |
| hover / pointerover | 1 | 2 | 1 | 状态只改变轨道背景有效值，不增删 marker。 |
| pressed | 1 | 2 | 1 | 把手宽度临时拉伸，不增删 marker。 |

`indicator` 的 checked / unchecked 两端位置由 owner 在 `Arrange` 阶段计算，loading 指示与把手共享同一 `indicator` Part，
不因 loading 状态额外产生替代节点。`content` 的两个 presenter 始终同时存在于模板，on/off 可见性只通过 owner 计算的内容
偏移与裁剪边界表达，不通过增删 marker 实现。

## 5. 尺寸基线

ToggleSwitch 实现 `ICustomizableSizeTypeAware`，`SizeType` 支持 `Middle`、`Small`、`Large`、`Custom` 四档，但主题只提供
两组尺寸 Token 分支，普通档（`Middle` / `Large` / `Custom`）与小号档（`Small`）：

| 项目 | 内容 |
| --- | --- |
| 普通档尺寸分支 | `SizeType=Middle`、`Large`、`Custom` 共用 `TrackHeight`、`TrackMinWidth`、`HandleSize`、`InnerMinMargin`、`InnerMaxMargin`、`ContentIconSize`、`ExtraInfoFontSize`。 |
| 小号档尺寸分支 | `SizeType=Small` 使用 `TrackHeightSM`、`TrackMinWidthSM`、`HandleSizeSM`、`InnerMinMarginSM`、`InnerMaxMarginSM`、`ContentIconSizeSM`、`ExtraInfoFontSizeSM`。 |
| 尺寸 owner | 轨道高度由 owner 的 `MeasureOverride` 返回；把手尺寸由公开属性 `KnobSize` 写入 `SwitchKnob`；内容边距由 owner 的偏移计算消费。`TrackHeight`、`TrackMinWidth`、`TrackPadding`、`KnobSize` 是 owner 级公开属性，默认值来自两组尺寸 Token 分支，可被 Semantic Style 覆盖。 |
| 高度链 | `TrackHeight` 派生自 SharedToken `FontSize * RelativeLineHeight`，`TrackHeightSM = ControlHeight / 2`；`HandleSize = TrackHeight - TrackPadding * 2`（`TrackPadding` 固定为 2）；`InnerMaxMargin = HandleSize + TrackPadding * 3` 等均由同一高度链推导。 |
| 固定几何 | 轨道高度由 `MeasureOverride` 返回 `TrackHeight`，把手尺寸由 `SwitchKnob.MeasureOverride` 返回 `KnobSize`。显式 `Width` 优先于内容测量宽度（不小于 `TrackMinWidth`），把手与内容偏移随之按显式宽度计算，等价于 antd root 的 `width` 样式。 |
| 外部映射 | Ant Design Switch 只有单一默认尺寸（无 `size` prop），对应 AtomUI `Middle` 档；`Small` 与 `Custom` 是 AtomUI 桌面扩展。 |

`Custom` 不是独立 Token 组，未显式覆盖时与 `Middle` 共用同一普通档视觉基线。

失败回归：轨道高度、把手尺寸与内容边距三者由同一高度链推导。若只覆盖其一（例如单独改变 `TrackHeight`，或让
`Small` 与 `Middle` 分支混用 `HandleSize` / `InnerMaxMargin`），把手将不再填满轨道高度，on/off 内容会与把手重叠或被
`PART_MainContainer` 裁剪。最小复现是把 `SizeType=Small` 的 `HandleSize` 误指向普通 `HandleSize`（或反向），此时小号
开关的把手无法贴边滑动、内容与把手重叠；恢复同一高度链后通过。因此布局型 Semantic Setter 不得绕过 SizeType 分支单点
覆盖尺寸，也不得用固定 `Height` / `MinHeight` / 像素偏移替代完整尺寸映射。

## 6. 定制边界

以下区域明确不属于 ToggleSwitch Semantic Part：

- 轨道胶囊的绘制（owner `Render` 的 `DrawPilledRect`）与 `GrooveBackground` 状态映射。
- 把手几何尺寸之外的内部状态：loading 指示 arc 的旋转角度、几何与 `SwitchKnob` 内部状态；把手几何尺寸（`KnobSize`）与位置基准（`TrackPadding`）经 root 公开属性定制，把手阴影经 `.semantic-indicator` 的 `Effect` 覆盖。
- `WaveSpiritDecorator` 的 checked 变化波纹 actor。
- 用户 `OnContent` / `OffContent` 模板生成的子树。
- `PART_*` 名称、internal 类型、状态转换器和 motion phase。

把手越出轨道边界的渲染依赖内置模板将把手节点置于内容裁剪 `Canvas` 之外；用户不得依赖 `PART_MainContainer` 的
`ClipToBounds` 细节或重建模板来复刻该结构。

Semantic Style 服从 Avalonia 原生属性优先级。Part Setter 命中只证明目标属性已生效；如果最终布局仍被 owner 或中间
节点的固定测量、`Canvas` 显式 `Arrange`、`ClipToBounds` 或 `KnobSize` 限制，应按跨节点布局约束排查，不能把它解释为
Semantic Style 优先级失效。

## 7. 兼容性与验证

删除或重命名 Part、修改 selector class、收窄 `ContractType`、改变 cardinality，或者让任一内置模板缺少 marker，均
属于公共主题契约变更。

验证至少覆盖：

- descriptor 中只有 `root`、`content`、`indicator`，字段值与本文表格一致（`indicator` 为 `Single`，`content` 为 `Multiple`）。
- 内置 ToggleSwitch 模板的 marker 数量与类型一致（`indicator` 一个、`content` 两个、`root` 隐式）。
- checked / unchecked / `IsChecked=null` 切换不增删 marker，`indicator` 保持 `Single`，`content` 保持 `Multiple`。
- `OnContent` / `OffContent` 为 `null` 时两个 `ContentPresenter` 节点仍存在且 marker 身份不变。
- root 状态样式、indicator 局部 `Background`、content 局部前景/字重在 hover / disabled / loading 下保持有效。
- 公开几何属性（`TrackHeight`、`TrackMinWidth`、`TrackPadding`、`KnobSize`）与显式 `Width` 驱动把手与内容几何；负
  `TrackPadding` 下把手越出轨道且不被内容裁剪 `Canvas` 裁剪；`.semantic-indicator` 的 `Effect` 覆盖主题派生阴影。
- `SizeType` 四档与两组尺寸分支的映射一致，`Custom` 默认复用普通档，混用尺寸分支不产生布局错误。
- 默认主题不消费 `.semantic-*`，未声明用户 Semantic Style 时不增加 selector activator。
- Generator 静态输出和 NativeAOT 路径不依赖反射或运行时扫描。
