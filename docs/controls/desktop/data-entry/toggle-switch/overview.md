# ToggleSwitch 桌面版架构设计

本文档定义 `AtomUI.Desktop.Controls.ToggleSwitch` 桌面版开关控件的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，内部实现原理见 [ToggleSwitch 桌面版实现原理](implementation.md)，Semantic Part 契约见 [ToggleSwitch Semantic Part 契约](semantic-part.md)，Token 专项设计见 [ToggleSwitch Token 设计](token.md)，设计和契约变化记录见 [ToggleSwitch Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/DataEntry/ToggleSwitch` |
| 控件状态 | Stable |

ToggleSwitch 是 AtomUI 桌面数据录入体系中的布尔状态切换控件，用于表达立即生效的开/关选择。它以 Avalonia `ToggleButton` 的选中状态为基础，接入 AtomUI 的尺寸、加载态、文字或图标内容、WaveSpirit、Form 和 Token 体系。

ToggleSwitch 的职责是编辑单个 `bool?` 状态。它不承担多选、单选组、延迟提交、异步命令调度、复杂权限判断或表单校验消息展示职责。

桌面 public 类型 `ToggleSwitch` 继承 `AtomUI.Controls.AbstractToggleSwitch`。`ToggleSwitch` 本体只注册桌面 `ToggleSwitchToken` scope，公共 API、布局、交互和 Form 集成由 `AbstractToggleSwitch` 承载。

## 2. 设计语言

ToggleSwitch 的设计语言来自胶囊轨道、滑动把手和可选内容的组合。

| 维度 | 含义 | 典型表达 |
| --- | --- | --- |
| 状态语义 | 当前值是启用或关闭。 | `IsChecked=True/False/null`。 |
| 输入密度 | 控件在表单、设置面板和工具栏中的尺寸等级。 | `SizeType=Middle/Small/Custom/Large`。 |
| 内容语义 | 开关内部可显示文字或图标。 | `OnContent`、`OffContent`、对应模板。 |
| 操作反馈 | 控件可处于加载、禁用、hover、pressed 和 checked 状态。 | `IsLoading`、`:disabled`、`:pointerover`、`:pressed`、`IsChecked`。 |
| 动效语义 | 切换时把手、内容和背景平滑过渡。 | `IsMotionEnabled`、`IsWaveSpiritEnabled`。 |

`IsLoading=true` 表示开关动作正在处理。此时控件保持当前 `IsChecked` 状态，禁止继续切换，并在把手内显示加载指示。

## 3. API 与契约模型

ToggleSwitch 的公共契约由 Avalonia `ToggleButton` 状态和 AtomUI 扩展属性组成。

| API | 类型 | 语义 |
| --- | --- | --- |
| `IsChecked` | `bool?` | 当前开关值，继承自 `ToggleButton`，支持三态数据绑定。 |
| `GrooveBackground` | `IBrush?` | 轨道背景，通常由主题 selector 根据 checked、hover、disabled 和 loading 状态设置。 |
| `OnContent` / `OffContent` | `object?` | checked / unchecked 状态下显示的内容。 |
| `OnContentTemplate` / `OffContentTemplate` | `IDataTemplate?` | checked / unchecked 内容模板。 |
| `SizeType` | `CustomizableSizeType` | 尺寸密度，支持 `Middle`、`Small`、`Large` 和 `Custom`。 |
| `IsLoading` | `bool` | 是否显示加载态并禁止切换。 |
| `IsMotionEnabled` | `bool` | 是否启用把手、内容偏移、背景和透明度动效。 |
| `IsWaveSpiritEnabled` | `bool` | checked 状态变化时是否播放 wave spirit。 |

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_WaveSpirit` | `WaveSpiritDecorator` | checked 变化时播放胶囊波纹。 |
| `PART_MainContainer` | `Canvas` 的内部派生类型 | 承载内容 presenter，传播内容尺寸变化并提供裁剪边界；把手位于画布之外。 |
| `PART_OnContentPresenter` | `ContentPresenter` | checked 状态下内容承载。 |
| `PART_OffContentPresenter` | `ContentPresenter` | unchecked 状态下内容承载。 |
| `PART_SwitchKnob` | `SwitchKnob` | 把手、加载指示和把手动画承载。 |

公共接口契约：

| 接口 | 语义 |
| --- | --- |
| `ICustomizableSizeTypeAware` | 接入支持 `Custom` 的尺寸模型。 |
| `ICustomHitTest` | 只在有效轨道区域命中，loading 和 disabled 时不可命中。 |
| `IWaveSpiritAwareControl` | 接入 WaveSpirit 开关。 |
| `IFormItemAware` | Form 以 `IsChecked` 作为表单值。 |

## 4. 行为与状态模型

首帧状态契约：把手、内容偏移和轨道外观直接呈现当前 `IsChecked`；后续有效切换保留主题过渡。loading 旋转属于
过程反馈，保持独立语义。根控件与把手的基线提交和主题就绪条件见
[状态动效初始化设计](../../../../architecture/systems/control-infrastructure/state-motion-initialization.md)，源码接入差异及证据见
[状态动效初始化验证](../../../../architecture/systems/control-infrastructure/state-motion-initialization-verification.md)。

ToggleSwitch 的交互优先级：

```text
Disabled
> Loading
> Pressed
> PointerOver
> Checked / Unchecked
> Normal
```

`Disabled` 表示控件不可交互，主题降低整体透明度，把手保持当前状态。`Loading` 表示业务处理中，控件命中测试返回 false，pointer press/release 不进入基类切换逻辑，并把 cursor 设置为默认箭头。

checked 状态流：

```text
Pointer / keyboard / binding / Form.SetValue
      ↓
IsChecked
      ↓
CalculateElementsOffset
      ↓
KnobRect + content offset + GrooveBackground selector
```

按下状态会临时拉伸把手宽度，并同步调整当前可见内容偏移。释放后重新计算标准把手和内容位置。

Form 集成以 `IsChecked` 作为值。`ClearFormValue()` 会把 `IsChecked` 设置为 `null`，因此绑定方必须明确是否允许三态。

## 5. 视觉与主题模型

ToggleSwitch 使用控件本体绘制轨道，模板负责内容和把手。

| 主题 | 职责 |
| --- | --- |
| `ToggleSwitchTheme.axaml` | 主模板、状态 selector、SizeType 分支、轨道背景、WaveSpirit 和 SwitchKnob token 传递。 |
| `SwitchKnobTheme.axaml` | 把手加载透明度、加载动画周期和把手宽度动效。 |

视觉状态主要由主题 selector 表达：

- `IsChecked=True` 使用 `SwitchColor`，hover 使用 `ColorPrimaryHover`。
- `IsChecked=False` 使用 SharedToken 的文本弱化色，hover 使用更强弱化色。
- `IsLoading=True` 和 disabled 使用 `SwitchDisabledOpacity`。
- `SizeType=Small` 使用 small token 组。
- `SizeType=Middle`、`SizeType=Custom` 和 `SizeType=Large` 共享普通 token 组。

`Custom` 不是独立 Token 组；未显式覆盖时与 `Middle` 保持同一视觉基线。

## 6. 控件家族或集成关系

ToggleSwitch 属于 Data Entry 控件，常用于设置项、表单布尔项和即时状态切换。它与 Checkbox、Radio、Segmented、Select 等状态选择控件共享 Form 和 disabled 语义，但表达的是单个即时开关，不表达集合选择或互斥组。

集成关系：

- Avalonia `ToggleButton`：提供 checked、pressed、keyboard 和点击切换基础行为。
- `SwitchKnob`：承载把手绘制、加载指示和加载动画。
- `WaveSpiritDecorator`：承载 checked 变化时的胶囊 wave。
- `PathIcon` / `Icon`：作为 `OnContent` 或 `OffContent` 时由控件同步图标尺寸和前景色。
- `IFormItemAware`：允许 Form 读取、设置、清空开关值。

## 7. 兼容性不变量

维护 ToggleSwitch 时必须保持以下不变量：

- 不修改 `ToggleButton.IsChecked` 的 checked、unchecked、null 和绑定语义。
- `IsLoading=true` 时不得触发新的切换。
- loading 状态必须在 detach 时停止动画并释放 cancellation token。
- `PART_SwitchKnob`、`PART_OnContentPresenter`、`PART_OffContentPresenter`、`PART_WaveSpirit` 和 `PART_MainContainer` 名称不变。
- `SizeType=Custom` 默认与 Middle 分支一致，不新增未授权的专属 Token 组。
- `OnContent` / `OffContent` 为 `PathIcon` 或 `Icon` 时，图标尺寸和前景色必须随控件状态更新。
- Form value 始终是 `IsChecked`，不把 loading、content 或视觉状态作为表单值。
- Token 名称、语义和 AXAML resource key 不擅自重命名、删除或迁移。

## 8. 专项模型

### 8.1 内容模型

`OnContent` 和 `OffContent` 可以是文本、图标或任意模板化对象。默认字符串按实际可见字形垂直居中，字体行盒的下伸预留空间不参与标签视觉中心；自定义模板与控件内容保持自身布局。内容位置由控件测量当前两个 presenter 的最大宽度后计算，确保开/关切换时轨道宽度能容纳较宽的一侧。

图标内容使用 `BindUtils.RelayBind` 同步 `IconSize`、`Foreground`、`Icon.FillBrush` 和 `Icon.StrokeBrush`。这些绑定由 on/off 两组 disposable 管理，在内容替换时释放。

### 8.2 加载模型

`IsLoading` 控制两个层次：

```text
ToggleSwitch.IsLoading
  → CursorProperty local value
  → SwitchKnob.NotifyStartLoading / NotifyStopLoading
  → SwitchKnob loading animation + disabled knob
```

加载动画在 `SwitchKnob` attach 时启动，detach 时取消并释放 token。停止加载后恢复把手 enabled 状态。

### 8.3 尺寸模型

ToggleSwitch 当前主题提供普通和小号两组尺寸。`Middle`、`Large` 和 `Custom` 共用普通尺寸，`Small` 使用 small token。控件测量宽度时取 `TrackMinWidth` 与内容宽度加内部边距的较大值。

## 9. 文档导航与验证策略

关联文档：

- [ToggleSwitch 桌面版实现原理](implementation.md)
- [ToggleSwitch Semantic Part 契约](semantic-part.md)
- [ToggleSwitch Token 设计](token.md)
- [ToggleSwitch Changelog](changelog.md)

`content` 与上游 Switch `content` 语义对齐，由模板中的两个 `ContentPresenter` 节点承载并以 `Multiple` 基数公开；
`indicator` 对应上游 `indicator` 语义，由 `SwitchKnob` 节点承载并以 `Single` 基数公开。完整契约见
[ToggleSwitch Semantic Part 契约](semantic-part.md)。

验证策略：

| 层次 | 验证内容 |
| --- | --- |
| 文档 | `overview.md`、`implementation.md`、`token.md`、`changelog.md` 链接有效，Data Entry 分类入口包含 ToggleSwitch。 |
| API | `IsChecked`、`SizeType`、`IsLoading`、`OnContent`、`OffContent`、Form value 和 WaveSpirit 开关。 |
| 状态 | checked、unchecked、null、hover、pressed、disabled、loading、motion on/off。 |
| AXAML/Theme | template part、SizeType 分支、content presenter、SwitchKnob、WaveSpirit 和 loading opacity。 |
| Token | `ToggleSwitchToken`、生成的 TokenKind、Token 类型、生成数据和主题引用一致。 |
| Gallery | 走查基础、禁用、文字与图标、两种尺寸和加载示例。 |
