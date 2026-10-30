# Button 桌面版架构设计

本文档定义 `AtomUI.Desktop.Controls.Button` 桌面版的最新设计定位、公共契约、状态模型、视觉主题关系和兼容边界。通用控件研发约束见 [控件研发标准](../../../../engineering/development/control-development-guidelines.md)，Semantic Part 公共主题契约见 [Button Semantic Part 契约](semantic-part.md)，内部实现原理见 [Button 桌面版实现原理](implementation.md)，Button Token 的专项设计见 [Button Token 设计](token.md)，设计和契约变化记录见 [Button Changelog](changelog.md)。

## 1. 控件定位

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/General/Button` |
| 控件状态 | Stable |

Button 是 AtomUI 桌面控件体系中的基础动作触发控件，用于承载用户可执行的明确操作。Button 负责把动作语义稳定映射为公共 API、交互状态、主题 Token、模板结构和反馈效果。

Button 不承担复杂内容布局、导航结构管理或业务状态表达职责。复杂按钮形态应通过 Button 家族控件扩展，而不是扩大 Button 本体的职责边界。

Button 家族包括 `DropdownButton`、`SplitButton`、`IconButton` 和 `HyperLinkButton`。这些控件可以拥有不同模板结构，但必须共享 Button 的动作语义、状态解释、尺寸体系和主题资源。

典型使用场景：

- 提交、保存、删除、确认、取消等明确动作。
- 表达主动作、普通动作、弱强调动作、链接式动作和危险动作。
- 在工具栏、表单、对话框、卡片和列表项中提供可点击操作入口。
- 使用图标、加载态、禁用态、尺寸和形状增强动作可识别性。

## 2. 设计语言

Button 的设计语言由三个正交维度组成。

| 维度 | 含义 | 典型表达 |
| --- | --- | --- |
| 动作层级 | 操作在当前界面中的优先级。 | 主动作、普通动作、弱强调动作、链接式动作。 |
| 语义颜色 | 操作的语义属性。 | 默认、主要、危险、业务预设色。 |
| 视觉强度 | 同一语义在视觉上的强调程度。 | 实心、描边、虚线、填充、文本、链接。 |

Button 的当前设计模型以 `Color + Variant` 表达语义颜色和视觉强度。`ButtonType` 是兼容合成模型，同时承载动作层级、语义颜色和视觉强度，应保留为兼容入口。

颜色不应被视为装饰属性。`Danger` 表达破坏性或高风险动作，预设色表达业务分类或语义扩展。视觉强度不应改变语义颜色，只改变同一语义的呈现强度。

Button 的兼容类型映射、`Color + Variant` 状态矩阵、控件 Token 派生以及 normal、hover、pressed、disabled
状态语义应与 Ant Design Button 保持一致。维护时必须把对齐结论固化为 AtomUI 的公开契约、Token 规则和回归测试，
不得在单个 ControlTheme 或 Gallery 示例中增加特殊颜色分支。

## 3. API 与契约模型

Button 的公共 API 是控件最重要的稳定契约。公共属性、事件和方法集中在 `Button.cs`，内部主题变量和实现细节不得替代公共 API。

基础与兼容 API：

- `ButtonType`: `Default`、`Dashed`、`Primary`、`Link`、`Text`。
- `ButtonShape`: `Default`、`Circle`、`Round`。
- `IsDanger`、`IsGhost`、`IsLoading`。
- `SizeType`、`Icon`、`IconPlacement`、`IconWidth`、`IconHeight`。
- `IsMotionEnabled`、`IsWaveSpiritEnabled`。

Root 表面定制 API：

- `Background` / `BorderBrush`（`TemplatedControl` 标准属性）：Button 表面定制的唯一通道，不设平行定制属性。模板 `Frame` 直接 `TemplateBinding` 这两个属性，用户本地值按 Avalonia 优先级高于主题 Style / StyleTrigger——定制期间 hover、pressed、disabled 引起的背景或边框变色保持定制值，语义与内联样式（本地值优先于状态类）一致；清除定制后恢复主题状态机。渐变、图片等非纯色表面同样通过 `Background` 表达。

正交 API：

```csharp
public ButtonColor? Color { get; set; }
public ButtonVariant? Variant { get; set; }
```

`Color` 与 `Variant` 使用 nullable 类型，用于区分“未参与新模型”和“显式设置默认值”。该区分是保持 `ButtonType`、`IsDanger` 与正交 API 兼容优先级的必要条件。

解析优先级：

```text
显式 Color + Variant
> ButtonType / IsDanger 兼容映射
> Variant=Solid 自动补 Color=Primary
> Color=Default, Variant=Outlined
```

旧 API 映射：

| 旧 API | 正交模型 |
| --- | --- |
| `ButtonType=Default` | `Color=Default, Variant=Outlined` |
| `ButtonType=Primary` | `Color=Primary, Variant=Solid` |
| `ButtonType=Dashed` | `Color=Default, Variant=Dashed` |
| `ButtonType=Text` | `Color=Default, Variant=Text` |
| `ButtonType=Link` | internal link color, `Variant=Link` |

`ButtonColor` 不暴露 `Link`。`ButtonType.Link` 是兼容入口，内部映射到链接视觉。

`SizeType` 使用可自定义尺寸模型，支持 `Large`、`Middle`、`Small` 和 `Custom`。`Large`、`Middle`、`Small` 是 Button
预设尺寸档，主题通过对应 ControlHeight Token 设置 `MinHeight` 基线，并由内容、Padding 和其他布局属性决定是否向上
扩展。`Custom` 不设置预设高度基线，表示用户希望基于 Button 现有属性进行实例级尺寸定制，而不是引入 Button 专属的
`CustomHeight`、`CustomPadding` 或尺寸对象。

`IconWidth` 与 `IconHeight` 是 Button 用户图标和 loading 图标共享的公共 Avalonia 尺寸入口。两个属性相互独立，支持非正方形图标；对应的公共属性字段为 `IconWidthProperty` 与 `IconHeightProperty`。Theme 根据 `SizeType` 提供默认值，用户设置在 Button 上的本地值具有更高优先级，并同时投影到 `PART_ButtonIcon` 与 `PART_LoadingIcon`。

`IconPlacement` 控制 `Icon` 相对内容的位置，支持 `Start` 和 `End`。默认值必须是 `Start`，以保持既有 `Icon` 使用方式不变。`End` 只改变用户图标与内容的排列方向，不改变 loading、icon-only、尺寸、颜色、变体或交互状态语义。

```csharp
public enum ButtonIconPlacement
{
    Start,
    End
}

public ButtonIconPlacement IconPlacement { get; set; }
```

Template part 与主题入口：

| 节点 | 职责 |
| --- | --- |
| `PART_WaveSpirit` | 承载点击 wave 反馈。 |
| `ShadowsFrame` | 承载按钮阴影。 |
| `Frame` | 承载主体背景、边框、圆角和尺寸基底；`Background` / `BorderBrush` 直接 `TemplateBinding` owner 属性，是 root 定制的落点。 |
| `PART_RootLayout` | 排列 loading icon、icon 和 content，并根据 `IconPlacement` 调整用户 icon 位置。 |
| `PART_LoadingIcon` | 展示 loading 状态图标，宽高通过 `TemplateBinding` 跟随 `IconWidth`、`IconHeight`。 |
| `PART_ButtonIcon` | 展示用户设置的 icon，位置由 `IconPlacement` 控制，宽高通过 `TemplateBinding` 跟随 `IconWidth`、`IconHeight`。 |
| `PART_ContentPresenter` | 展示用户内容。 |

Button 支持以下 Semantic Part：

| Part | 公共入口 | 数量语义 | 职责摘要 |
| --- | --- | --- | --- |
| `root` | Button 本身 | `Single` | Button 动作、状态与根视觉样式的统一 owner。 |
| `icon` | `.semantic-icon` | `Multiple` | 用户图标与 loading 图标的统一视觉职责。 |
| `content` | `.semantic-content` | `Single` | 用户内容展示与排版区域。 |

完整的 Selector、`ContractType`、存在条件、逐 Part 定制说明、状态矩阵和排除边界见
[Button Semantic Part 契约](semantic-part.md)。`PART_WaveSpirit`、`ShadowsFrame`、`Frame` 和
`PART_RootLayout` 属于 Button Composition Model，不是公开 Semantic Part。

## 4. 行为与状态模型

Button 的交互优先级：

```text
Disabled
> Loading
> Pressed
> PointerOver
> Normal
```

`Disabled` 表示控件不可交互，应屏蔽 hover、pressed、wave 等交互反馈。`Loading` 表示操作正在进行中，应影响 loading icon、透明度、原 icon 展示和 wave 播放条件，但不等价于 API 层面的禁用状态。

Button 继承 Avalonia Button 的基础点击、命令和键盘行为。AtomUI 扩展逻辑不得绕过或破坏基础控件语义。

Button 的 effective state 由 C# 层归一，AXAML 主题只消费已经归一的状态或主题变量。核心 effective state 包括：

- `EffectiveColor`、`EffectiveVariant`。
- `EffectiveIsDanger`、`EffectiveIsGhost`、`EffectiveIsBordered`。
- `EffectiveBorderThickness`、`EffectiveCornerRadius`。
- `WaveSpiritType`。
- icon-only、loading 相关伪类。

## 5. 视觉与主题模型

Button 模板应保持阴影层、主体绘制层、内容层和 wave 层的职责分离。可以移除无明确职责的包装层，但不得合并承担不同视觉职责的节点。

Button 主题采用分层变量模型，避免直接展开 `Color × Variant × State` 的组合样式。

```text
Color Selector
  设置颜色语义变量

Variant Selector
  将颜色语义变量映射到文字、背景、边框、阴影变量

State Selector
  将 Normal / PointerOver / Pressed / Disabled / Loading 状态应用到最终视觉属性
```

用于 AXAML `Setter`、selector、动态资源和主题切换的变量应定义为 internal `StyledProperty`。普通 CLR 属性不适合作为主题变量，`DirectProperty` 仅适用于不参与 Style 系统的内部运行时状态。

ButtonToken 负责提供组件级语义值，Theme 负责把状态映射为视觉属性。Token 的分类、用途、边界和扩展策略见 [Button Token 设计](token.md)。

## 6. 控件家族或集成关系

Button 家族控件应共享一致的动作语义和状态解释。

- `DropdownButton` 继承 Button 的基础动作状态及 `IconWidth`、`IconHeight`，并扩展下拉触发能力；其 `OpenIndicator` 尺寸仍由 DropdownButton 自身主题独立管理。
- `SplitButton` 保持主动作和附加动作的语义区分，但两部分的颜色、尺寸和禁用状态应与 Button 体系一致。它是包含两个图标角色的复合控件，不继承本次 Button 图标尺寸 API。
- `IconButton` 保留 Button 的动作语义，只改变内容呈现密度；其既有 `IconWidth`、`IconHeight` 继续由自身模板解释。
- `HyperLinkButton` 保持链接式动作语义，并复用 Button 尺寸、字体和 icon 相关 Token；其既有 `IconWidth`、`IconHeight` 与 Button 保持同名尺寸语义。
- Button 家族不维护 `BrowserButtonThemes` 或 `Buttons/Themes/Browser/` 形式的平台主题分叉；同一套主题资产必须在
  Native 与 Browser 支持宿主下解释同一 API。

Button 与 CompactSpace、FormItem、Wave 和宿主注册协同。`Color + Variant` 等语义能力不应只覆盖默认 Button，Button 家族控件必须明确继承、覆盖或声明不支持该语义。

## 7. 兼容性不变量

优化或扩展 Button 时必须保持以下不变量：

- 现有五种 `ButtonType` 的默认、hover、pressed、disabled、danger、ghost 渲染不变。
- `IsDanger` 与各 `ButtonType` 的组合行为不变。
- `IsGhost` 与现有按钮类型的组合行为不变。
- loading icon、opacity、原 icon 隐藏逻辑不变。
- icon-only 判断与布局不变。
- 普通状态下的用户 icon，包括非 loading 的 icon-only 场景，继续使用 `IconSizeLG`、`IconSize`、`IconSizeSM` 默认值；不得切换到 `OnlyIconSize*`。
- 只有 `:icononly:loading` 状态的 loading icon 使用 `OnlyIconSizeLG`、`OnlyIconSize`、`OnlyIconSizeSM` 默认值。
- 用户在 Button 上设置的本地 `IconWidth`、`IconHeight` 必须覆盖 Theme 默认值，并同时作用于用户 icon 与 loading icon。
- `IconPlacement` 默认值必须保持 `Start`；`IconPlacement=End` 只允许改变用户 icon 的内容侧位置和间距方向。
- `Shape=Circle`、`Shape=Round` 的尺寸和圆角计算不变。
- `SizeType=Large/Middle/Small` 使用对应 Token 提供 `MinHeight`、字体、内边距、圆角和 icon 尺寸基线；内容或合法
  Semantic Part 布局 Setter 可以使最终高度超过该基线。
- `SizeType=Custom` 未显式设置尺寸相关属性时复用 `Middle` 的字体、内边距、圆角和 icon 默认值，但不继承 Middle
  的 `MinHeight`；用户设置的 `Height`、`MinHeight`、`Padding`、`FontSize`、`CornerRadius`、`IconWidth`、
  `IconHeight` 等现有属性必须按 Avalonia 原生优先级生效。
- CompactSpace 下的有效圆角、有效边框和 z-index 行为不变。
- wave 播放条件不变；播放前必须从 Button 当前最终视觉属性解析 wave brush，依次检查有效实色
  `BorderBrush` 和 `Background`，使 Theme 状态、Semantic root Style 与普通用户 Style 使用同一视觉事实源。
- 透明、纯白或非实色的最终 Brush 不作为 wave 颜色；无有效颜色时清除 Button 写入的 wave brush，使
  `WaveSpiritDecorator` 回到主题默认值。
- root 表面定制语义保持不变：用户在 Button 上设置的本地 `Background` / `BorderBrush` 直接由模板 `Frame`
  渲染并冻结该属性槽在 hover / pressed / disabled 的状态变色，清除后恢复主题状态机；不得重新引入平行
  定制属性或模板内定制覆层。
- 同一 Button 家族主题资产在 Native 与 Browser 支持宿主下保持同一 API 语义，不通过平台专用主题资产复制视觉。

如果实现某项能力时无法保持这些不变量，应先停止实现，说明原因、影响范围、替代方案和迁移方式，并获得授权。

## 8. 专项模型

### 8.1 Color / Variant 模型

多彩按钮属于 Button 当前设计模型的一部分，不应通过扩展 `ButtonType` 枚举表达。

- `ButtonColor` 表达颜色语义。
- `ButtonVariant` 表达视觉强度。
- `ButtonType` 保留为兼容语法糖。
- `IsDanger` 保留为危险语义的旧入口。
- 预设色来源于 AtomUI palette / token 系统，不在 Button 主题中维护私有色表。
- `ButtonType=Text` 是 `Default + Text` 的兼容语法，表达中性低强调动作，不隐式跟随品牌色。
- 品牌色文本动作使用 `Color=Primary + Variant=Text`，其 normal、hover、pressed 状态跟随当前主题主色色阶。
- Danger Text 使用错误语义色阶，与品牌主色保持独立。

### 8.2 Root 表面定制模型

Button 的表面定制由 `TemplatedControl` 标准 `Background` / `BorderBrush` 属性表达，模板 `Frame` 通过
`TemplateBinding` 直接渲染这两个属性。用户本地值按 Avalonia 属性优先级高于主题 Style 与状态 StyleTrigger：
定制期间该属性槽在 hover、pressed、disabled 的状态变色保持定制值（与内联样式语义一致，本地值优先于状态类），`Foreground`
文字色、wave 反馈和布局不受影响；清除定制后恢复主题状态机的状态变色。渐变、图片等非纯色表面同样经由
`Background` 表达，不引入平行定制属性或模板内定制覆层。

### 8.3 Custom 尺寸模型

Button 的尺寸模型由预设档和实例定制组成。预设档 `Large`、`Middle`、`Small` 表达 Button 的三档尺寸；AtomUI 额外通过 `CustomizableSizeType.Custom` 提供实例级自定义入口。

`SizeType=Custom` 的设计契约：

- 未设置本地尺寸属性时，`Custom` 使用 `Middle` 的字体、内边距、圆角、普通 icon 尺寸和 loading icon-only 默认值；
  高度由自然测量决定，不继承 Middle 的预设 `MinHeight`。
- 用户通过 Button 公共属性定制尺寸，例如 `Height`、`MinHeight`、`Width`、`MinWidth`、`Padding`、`FontSize`、`CornerRadius`、`IconWidth` 和 `IconHeight`。
- 主题只能以 Style 默认值或可被 Button 本地属性覆盖的模板绑定提供 Custom 默认值，不得用更高优先级写入覆盖用户本地值。
- Button 不提供 `CustomHeight`、`CustomPadding`、`CustomFontSize`、`CustomIconSize`、`CustomOnlyIconSize` 或 `ButtonSizeMetrics`。
- Custom 模式下 icon 与 loading icon 默认沿用 `Middle` Token；特殊图标尺寸统一通过 `IconWidth`、`IconHeight` 定制，不通过外部样式进入 Button 模板内部。

### 8.4 Icon 位置模型

Button 的 `Icon` 是单一用户图标入口，`IconPlacement` 只描述这个图标相对内容的位置。

- `Start` 表示图标位于内容起始侧，是默认值和兼容行为。
- `End` 表示图标位于内容结束侧，适用于下一步、跳转、查看更多等需要尾随图标的动作。
- `IconPlacement` 不创建第二个图标 slot，也不改变 `Icon` 类型、图标创建方式或 template part 名称。
- 图标和内容之间的间距继续由 Button token 管理；结束侧图标使用起始侧间距的镜像方向。
- icon-only 按钮没有文本内容侧差异，`IconPlacement` 不应改变 icon-only 的尺寸、padding 或居中行为。
- loading icon 仍由 `IsLoading` 状态控制，不作为 `IconPlacement` 的目标；loading 状态下原用户 icon 隐藏逻辑保持不变。
- `DropdownButton`、`SplitButton`、`IconButton`、`HyperLinkButton` 是否暴露同名能力应按各自模板职责单独评估，不由 Button 本体隐式要求。

### 8.5 Icon 尺寸投影模型

Button 以控件自身的 `IconWidth`、`IconHeight` 作为同时控制用户图标和 loading 图标的统一 API owner。Button 与 DropdownButton 模板中的 `PART_ButtonIcon`、`PART_LoadingIcon` 通过 `TemplateBinding` 读取这两个属性，不在模板内部重新定义一套可供外部定位的尺寸入口。Button 本体另外以 `.semantic-icon` 提供受支持的局部视觉覆盖入口，应用不需要也不应依赖两个 `PART_*` 名称。

Theme 默认值矩阵：

| 状态 | Large | Middle | Small | Custom |
| --- | --- | --- | --- | --- |
| 普通用户 icon、普通 loading icon、非 loading 的 icon-only 用户 icon | `IconSizeLG` | `IconSize` | `IconSizeSM` | `IconSize` |
| `:icononly:loading` 的 loading icon | `OnlyIconSizeLG` | `OnlyIconSize` | `OnlyIconSizeSM` | `OnlyIconSize` |

需要让两个图标实现共享同一尺寸和 loading 切换语义时，应用应设置 Button 的 `IconWidth`、`IconHeight`。仅需要局部视觉覆盖时，在 Button owner Style 中嵌套 `ButtonIconStyle`，并以 `x:SetterTargetType="Control"` 提供 Setter 编译类型；不得复制 `/template/ .semantic-icon` route，也不得使用 `Control.semantic-icon`、`:is(Control).semantic-icon`、`PART_ButtonIcon`、`PART_LoadingIcon` 或 internal 类型作为公共 selector。Button 本地尺寸值覆盖所有尺寸档与状态默认值，因此同一实例进入 loading 状态时不会丢失用户指定的宽高。

Gallery 的 Custom 尺寸示例继续优先在 Button selector 上设置 `IconWidth`、`IconHeight`，以证明统一 API owner 的数据流。展示 Semantic Part 定制时则使用 `ButtonIconStyle`，不定位两个内部 template part。

## 9. 文档导航与验证策略

关联文档：

- [Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)
- [Button Semantic Part 契约](semantic-part.md)
- [Button 桌面版实现原理](implementation.md)
- [Button Token 设计](token.md)
- [Button Changelog](changelog.md)

验证策略：

| 改动类型 | 验证要求 |
| --- | --- |
| 文档改动 | `git diff --check`，确认未改实现文件。 |
| C# 状态模型改动 | Button 行为测试，覆盖旧 API 兼容映射以及 `IconWidth`、`IconHeight` 的属性注册、测量失效和本地值优先级。 |
| AXAML 主题改动 | 检查 Button、DropdownButton 模板的 `TemplateBinding`，覆盖三档尺寸、Custom、loading 和 icon-only 状态，并确认 Browser 注册仍使用共享主题资产。 |
| Token / Palette 改动 | Light / Dark 主题检查，确认 Native 与 Browser 支持宿主使用同一共享主题语义。 |
| Button 家族影响 | 覆盖 `DropdownButton`、`SplitButton`、`IconButton`、`HyperLinkButton` 关联场景。 |
| Public API 改动 | 需要授权，并补充 API 兼容测试与文档。 |
