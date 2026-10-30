# Button 桌面版实现原理

本文档描述 Button 桌面版的内部实现组织、状态归一、模板接入和维护边界。公共设计与 API 契约见
[Button 桌面版架构设计](overview.md)，Button 支持的 Part、Selector 与逐 Part 定制边界见
[Button Semantic Part 契约](semantic-part.md)，Semantic Part 的系统级契约见
[Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)，Token 语义见
[Button Token 设计](token.md)，变化记录见 [Button Changelog](changelog.md)。

## 1. 实现定位

Button 的实现目标是把兼容 API、正交 `Color + Variant` API、图标尺寸、loading、shape、compact 和 wave 归一为模板可消费的 stable state。实现文档只描述维护者必须理解的内部结构，不逐个复述私有方法。

Button.cs 保留公共属性、事件和接口实现入口；内部 helper 可以按职责拆分，但拆分不能改变 public API、模板契约、伪类或渲染结果。

## 2. 源码文件结构

主要源码：

- `src/AtomUI.Desktop.Controls/Buttons/Button.cs`：Button public API、Avalonia 属性注册、effective state、伪类同步、CompactSpace / Form / Wave 接口实现。
- `src/AtomUI.Desktop.Controls/GeneratedFiles/.../ButtonSemanticParts.g.cs`：生成的 Part 名称与 selector class 常量，仅用于构建检查、测试和 runtime-created 节点场景。
- `src/AtomUI.Desktop.Controls/GeneratedFiles/.../GeneratedSemanticPartManifest.g.cs`：Button `root/icon/content` 静态 descriptor。
- `src/AtomUI.Desktop.Controls/Buttons/ButtonToken.cs`：Button 控件 Token 定义与派生。
- `src/AtomUI.Desktop.Controls/Buttons/Themes/ButtonTheme.axaml`：Button 跨平台模板、状态 selector 和主题变量映射。
- `src/AtomUI.Desktop.Controls/Buttons/Themes/DropdownButtonBaseTheme.axaml`、`DropdownButtonTheme.axaml`：DropdownButton 对 Button 图标尺寸和状态语义的跨平台投影。
- `src/AtomUI.Desktop.Controls/Buttons/Themes/IconButtonTheme.axaml`：IconButton 默认主题叶子。
- `src/AtomUI.Desktop.Controls/Buttons/Themes/ButtonTheme.cs`：主题资源注册辅助。
- `src/AtomUI.Controls/Buttons/ButtonPseudoClass.cs`：共享 Button 伪类定义。
- `src/AtomUI.Controls/Buttons/Converters/ButtonIconVisibleConverter.cs`：icon 可见性转换辅助。
- `src/AtomUI.Desktop.Controls/DropdownButton/DropdownButton.cs`、`src/AtomUI.Desktop.Controls/SplitButton/SplitButton.cs`、`src/AtomUI.Desktop.Controls/Buttons/IconButton.cs`、`src/AtomUI.Desktop.Controls/Buttons/HyperLinkButton.cs`：Button 家族控件。

## 3. 核心类职责

`Button` 是状态归一中心。它同时承担 Avalonia Button 基类扩展、AtomUI 主题状态同步、CompactSpace 圆角和边框协同、Form 语义转发、Wave 几何和颜色感知。

`ButtonToken` 只提供组件级语义值，不保存实例状态。主题变量由 Button internal `StyledProperty` 承载，使 AXAML
selector 和动态资源可以消费状态结果。`Button.cs` 是颜色状态矩阵的唯一计算所有者；Button 家族 ControlTheme
不得按平台、`ButtonType`、Danger 或 Ghost 复制一套颜色计算。

Button 家族控件复用 Button 的动作语义。派生控件可以替换模板或增加行为入口，但不得重新解释 `ButtonType`、`Color`、`Variant`、`IsDanger`、`IsGhost`、loading 和 disabled 语义。

Button 通过 `[SemanticPart]` 声明 `icon` 和 `content`；生成器加入隐式 `root`。该声明是公开主题契约，不参与
Button 状态计算，也不要求运行时查询 descriptor。公共 Part 含义和支持用法由 `semantic-part.md` 维护；本文只维护声明与
真实模板节点、状态流和生命周期之间的实现映射。

Button 的尺寸基线必须先于 Semantic 布局覆盖确定：`Large`、`Middle`、`Small` 和 `Custom` 的完整映射、图标尺寸、内容
Padding、MinHeight、状态替代节点和 Shape 几何共同构成 Button 的尺寸/状态基线。`.semantic-content` 或 `.semantic-icon`
只能作为该基线之上的增量覆盖，不能把一个档位的局部属性叠加到另一个档位的 MinHeight 或几何约束上。

## 4. 状态与数据流

Button 状态流：

```text
ButtonType / IsDanger / IsGhost / IsLoading / Shape / SizeType / Icon
IconWidth / IconHeight / IconPlacement
Color? / Variant?
      ↓
Normalize semantic state
      ↓
EffectiveColor / EffectiveVariant / EffectiveIsDanger / EffectiveIsGhost
EffectiveBorderThickness / EffectiveCornerRadius / WaveSpiritType
      ↓
Pseudo-classes + internal StyledProperty theme variables
      ↓
Shared ControlTheme visual projection
```

`Color` 与 `Variant` 为 nullable，是兼容优先级的关键。归一逻辑必须能区分“用户未设置正交 API”和“用户显式设置默认值”。

Root 表面定制不进入归一流程：`Background` / `BorderBrush` 是 `TemplatedControl` 标准属性，由模板 `Frame` 直接
`TemplateBinding` 渲染；用户本地值按 Avalonia 属性优先级冻结该属性槽的状态变色，清除后回到主题状态机。归一层与
主题变量不感知、不保存这两个属性的用户值。

兼容 API 与正交 API 使用同一条颜色路径。`ButtonType=Text` 归一为 `Default + Text`，保持中性文字语义；需要品牌色
Text 时使用 `Primary + Text`。两者都由 `ConfigureVariantThemeVariables()` 生成最终 normal、hover、pressed 主题变量，
ControlTheme 只绑定这些变量。

尺寸状态由 `SizeType` 与 Button 现有布局属性共同决定。`Large`、`Middle`、`Small` 使用预设 ControlHeight Token 设置
`MinHeight`，同时设置对应字体、Padding、圆角和 icon 默认值；内容和合法的 Semantic Part 布局 Setter 可以使最终
高度超过该基线。`Custom` 复用 `Middle` 的字体、Padding、圆角和 icon 默认值，但不设置预设 `MinHeight`，并允许用户
通过本地 `Height`、`MinHeight`、`Padding`、`FontSize`、`CornerRadius`、`IconWidth`、`IconHeight` 等属性定制。
`IconWidth` 与 `IconHeight` 是独立的 Avalonia StyledProperty，允许非正方形尺寸；二者变化必须触发 Button 重新测量。
实现不得为 Custom 增加 Button 专属 `Custom*` 尺寸属性或尺寸聚合对象。

## 5. 生命周期与模板接入

Button 在静态构造中注册属性、伪类和主题关联，在实例构造中完成需要的状态订阅。模板应用时读取稳定 template part，并把状态同步到视觉节点。

共享 Button ControlTheme 的三个 Button ControlTemplate 都为 `PART_ButtonIcon` 与 `PART_LoadingIcon` 添加
`Classes.semantic-icon="True"`，并为 `PART_ContentPresenter` 添加 `Classes.semantic-content="True"`。这些 marker 是
静态 AXAML，不在 `OnApplyTemplate` 中查找、补写或同步。该写法属于 AtomUI 模板作者约定；应用仍通过
Button owner Style 中的 `ButtonIconStyle` 和 `ButtonContentStyle` 消费 Part。

两个图标节点继续通过 `TemplateBinding` 绑定 `IconWidth`、`IconHeight`。统一尺寸数据流使用 Button 自身属性；局部
视觉覆盖使用 `ButtonIconStyle`，包含 Setter 时由 `x:SetterTargetType="Control"` 提供编译类型。应用不得复制
`/template/ .semantic-icon` route，也不得依赖
`Control.semantic-icon`、`:is(Control).semantic-icon`、两个 `PART_*` 名称或 internal 实现类型。

维护顺序应遵守控件代码规范：

1. 字段与常量。
2. `StyledProperty` / `DirectProperty` / RoutedEvent 及其支持字段。
3. CLR wrapper。
4. 构造函数与静态构造函数。
5. 公共方法与接口显式实现。
6. protected 生命周期方法。
7. private 状态同步和计算方法。

显示接口实现属于公共能力的低一级入口，应靠近公共方法区域，不应被散落到文件末尾的纯 private 实现区。

## 6. 交互与事件处理

Button 交互沿 Avalonia Button 基类处理点击、命令、键盘和 enabled 状态。AtomUI 只在状态同步层补充 loading、wave、icon-only、shape 和 compact。

Wave 播放条件必须同时考虑 `IsWaveSpiritEnabled`、`IsMotionEnabled`、disabled、loading 和 Button 语义状态。释放触发
Wave 时，Button 读取当时已经合并 Theme、状态 selector、Semantic root Style 和用户 Style 的最终视觉属性，按
`BorderBrush -> Background` 顺序选择有效实色 Brush。透明、纯白和非实色 Brush 不参与取色；没有有效 Brush 时清除
本地 `WaveBrush`，由 `WaveSpiritDecorator` 使用主题默认值。用户定制的 root `Background` / `BorderBrush` 属于最终
视觉属性，同样参与该取色顺序。

Loading 状态影响 loading icon、原 icon 可见性和交互反馈，但不应通过直接禁用控件来模拟。

## 7. 内部算法与关键流程

关键流程：

- API 归一：显式 `Color + Variant` 优先，其次兼容 API 映射，再回退到默认语义。
- 颜色解析：`EffectiveColor` 选择 Default、Primary、Danger 或 preset palette；`EffectiveVariant` 再把该颜色组映射为
  Solid、Outlined、Dashed、Filled、Text 或 Link 的最终主题变量。
- 视觉投影：Button 家族 ControlTheme 只消费 `VariantText*`、`VariantBackground*`、`VariantBorder*` 和
  `VariantShadow`，不重复解释 `ButtonType` 或语义色阶。
- Wave 取色：播放前从 Button 当前最终 `BorderBrush`、`Background` 选择有效实色，而不是再次读取 `Variant*`
  内部变量；这样 Theme 状态和外部 root Style 不会形成两套颜色事实源。
- 尺寸归一：`Large`、`Middle`、`Small` 把 ControlHeight Token 映射为 `MinHeight` 基线；`Custom` 不设置预设
  `MinHeight`，其余尺寸指标以 `Middle` Token 作为 Style 默认值，Button 本地尺寸属性保持更高优先级。
- 图标尺寸：SizeType selector 把 `IconWidth`、`IconHeight` 默认映射到 `IconSizeLG`、`IconSize`、`IconSizeSM`；Custom 使用 `IconSize`。只有 `:icononly:loading` selector 把这两个默认值切换到对应 `OnlyIconSize*`，非 loading 的 icon-only 用户图标仍使用普通 `IconSize*`。
- 伪类同步：当 public API、content、icon、loading、shape、enabled 或 compact 状态变化时同步模板可见状态。
- 有效边框：由 Button 类型、variant、enabled、bordered 状态和 compact 状态共同决定。
- 有效圆角：由 `CornerRadius`、`Shape`、`SizeType` 和 CompactSpace 位置共同决定。
- Shape 测量：`MeasureOverride` 先取得内容期望尺寸，再通过 `LayoutHelper.ApplyLayoutConstraints` 合并 owner 的
  `Width`、`Height`、Min/Max 约束，最后根据受约束高度计算 Circle 正方形边界或 Round 胶囊最小宽度。不得用未应用
  `MinHeight` 的内容高度派生 Shape 几何，否则预设高度基线会在 MeasureCore 末尾单独抬高高度，造成 Circle 椭圆或
  icon-only Button 宽高不一致。
- Root 表面：`Background` / `BorderBrush` 由模板 `Frame` 直接渲染，无归一、无中间层；用户本地值冻结该属性槽的状态变色，清除后恢复主题状态机。
- Wave 几何：Button 暴露 wave 所需边框和圆角，使 wave 与最终按钮边界一致。

这些流程必须保持 C# 层归一、AXAML 层消费的分工。不得把 API 优先级判断下沉到大量 AXAML selector 组合中。

维护尺寸时应优先让预设档通过 `MinHeight` 建立基线，让模板内部尺寸节点通过 `TemplateBinding` 跟随 Button 属性。
不得用模板内部固定高度阻断用户在 Button 上设置的本地 `Height`，也不得用 root 固定 `Height` 掩盖 content/icon Padding
导致的实际测量差异。公开的局部模板 selector 只能依赖 `.semantic-icon` 或 `.semantic-content`，不得依赖
`PART_ButtonIcon`、`PART_LoadingIcon` 或 `PART_ContentPresenter`。

### 7.1 Semantic Part 布局排查

Button 的 Semantic Part 布局问题按以下顺序判断：

1. 检查 `.semantic-content` 或 `.semantic-icon` 目标节点的有效属性值，先确认 Setter 已经命中。
2. 如果目标值正确但视觉被裁剪或没有推动 root 增长，检查 Button 与中间模板节点的 `Height`、`MinHeight`、
   `MaxHeight`、Padding、Margin 和裁剪；这属于跨节点布局约束，不是 Semantic Style 优先级失败。
3. 改变预设高度模型后，检查 `MeasureOverride` 是否仍使用未应用 owner Min/Max 的内容尺寸派生 Circle/Round 几何。
4. `icon` 是 `Multiple` Part，必须同时检查用户 icon 与 loading icon，并覆盖 icon-only、loading 和图标位置切换。
5. 共享 Button ControlTheme 的全部 ControlTemplate 必须得到相同结论，不能以单一默认模板作为完成依据。

用于定位问题的高对比颜色、额外 Padding 或强制 icon 尺寸只属于显式诊断输入。确认根因后，默认 Gallery 样例和
ControlTheme 不保留这些诊断 Setter；长期示例只展示公共定制契约，不承担回归补丁职责。

## 8. 资源、性能与 AOT 边界

Button 主题变量使用 Avalonia 属性和动态资源，不使用反射读取模板状态。Token 资源由 ButtonToken scope 提供，并跟随主题切换。

三个 semantic marker 是 Button 默认实例固定承担的 class 存储成本。静态 class property 在模板初始化阶段执行一次
`Classes.Set`，不创建 Binding 或持久 listener；Button 内置主题也不使用 `.semantic-*` 编写默认样式，因此默认路径
不创建 Semantic Style class activator。应用声明 Semantic Style 后，Avalonia 会在 Button 模板的候选节点上保留 class
listener；同一 Part 的 Setter 应合并在一个 Style 中，并在批量 Button 场景验证 listener 数量和 detach 释放。

`IconWidth`、`IconHeight` 使用 Avalonia 属性优先级完成 Theme 默认值与 LocalValue 的覆盖，不增加订阅、运行时 part 遍历或状态变化时的视觉对象创建。两个模板 part 共享同一对属性，因此 loading 切换只改变可见性和默认状态映射，不引入尺寸同步副本。

Root 表面定制复用 `Frame` 的既有 `TemplateBinding`，不增加模板节点、控件实例、订阅或命中测试区域；未定制路径与默认渲染完全一致。

Button 实现不得引入运行时反射、动态代码生成或非 AOT 友好的资源查找路径。

## 9. 维护不变量

内部重构必须保持以下不变量：

- public API、Avalonia 属性字段和 CLR wrapper 名称不变。
- StyledProperty / DirectProperty / RoutedEvent 与支持字段的定义顺序符合控件代码规范。
- `Button.cs` 保留公共属性、事件、方法和接口入口；实现拆分只承载内部逻辑。
- `Color + Variant` 优先级和旧 API 映射结果不变。
- `ButtonType=Text` 保持 `Default + Text` 中性语义；`Primary + Text` 跟随当前主题 `ColorPrimary` 色阶。
- Button 家族主题必须共享 C# 计算出的最终颜色变量，不得按平台或兼容 API 复制颜色矩阵。
- `SizeType=Large/Middle/Small` 只通过 `MinHeight` 建立预设高度基线，不以主题 `Height` 封死自然测量。
- `SizeType=Custom` 不引入 Button 专属 `Custom*` 尺寸属性，不设置预设 `MinHeight`；未设置本地尺寸属性时复用
  `Middle` 的非高度指标，设置本地属性时由 Avalonia 属性优先级自然覆盖。
- 主题不得以高于本地值的优先级写入 Custom 默认尺寸。
- Circle/Round 的几何计算必须基于已经应用 Button Width/Height/Min/Max 约束的测量结果。
- `IconWidthProperty`、`IconHeightProperty` 及其 CLR wrapper 是 Button 公共契约，属性变化必须参与 measure invalidation。
- `PART_ButtonIcon`、`PART_LoadingIcon` 的默认 Width 和 Height 通过 `TemplateBinding IconWidth/IconHeight` 投影；
  外部局部覆盖只依赖 `.semantic-icon`，Setter 类型通过 `x:SetterTargetType="Control"` 提供，不得依赖类型前缀或
  `PART_*` 名称。
- 共享 Button ControlTheme 的每个 Button ControlTemplate 都必须具有两个 `.semantic-icon` marker 和一个
  `.semantic-content` marker；AtomUI 模板使用静态 `Classes.semantic-*="True"`，Button root 不添加
  `.semantic-root`。
- 普通用户 icon 和非 loading 的 icon-only 用户 icon 保持 `IconSize*` 默认值；只有 icon-only loading 默认使用 `OnlyIconSize*`。
- DropdownButton 继承同一图标尺寸属性与投影规则，`OpenIndicator` 继续由独立的 DropdownButton 主题尺寸控制；SplitButton 不纳入这一属性继承范围。
- Root 表面定制不得重新引入平行定制属性或模板内定制覆层；`Background` / `BorderBrush` 的本地值语义（冻结该属性槽状态变色、清除恢复状态机）保持不变。
- wave brush 不从内部模板节点反推；Button root 的最终 hover、pressed 或外部样式结果是
  合法取色输入。
- CompactSpace 圆角和边框折叠行为不变。
- 同一 Button 家族主题资产必须在 Native 与 Browser 支持宿主下保持同一 API 语义；不得维护
  `Buttons/Themes/Browser/` 或 `BrowserButtonThemes.axaml` 形式的平台主题分叉。

## 10. 测试与验证

验证范围：

- API 归一：覆盖 `ButtonType`、`IsDanger`、`Color`、`Variant`、`IsGhost` 的组合。
- 尺寸契约：覆盖 `SizeType=Large/Middle/Small/Custom`；验证预设档使用 `MinHeight` 基线、Custom 不继承预设
  `MinHeight`，并验证本地 `Height`、`MinHeight`、`Padding`、`FontSize`、`IconWidth`、`IconHeight` 的有效值。
- 布局型 Semantic Setter：分别覆盖 `.semantic-content` Padding 和 `.semantic-icon` Width/Height，确认 Setter 命中、
  owner 自然测量增长、内容不裁剪，并确认移除临时示例样式后默认 Button 视觉保持不变。
- 状态同步：覆盖 disabled、loading、hover、pressed、icon-only、circle、round；预设 `MinHeight` 与 icon-only、Circle、
  Round 组合必须验证最终 Bounds、宽高关系、垂直居中和 loading 替代节点。
- 图标投影：覆盖 Button、DropdownButton 模板的两个 part，并验证 Browser 注册使用同一套共享主题资产；验证普通 icon-only 仍使用 `IconSize*`、只有 icon-only loading 使用 `OnlyIconSize*`，并验证非正方形本地尺寸同时作用于用户 icon 和 loading icon。
- Wave：覆盖危险态、预设色、Semantic root Style、运行时最终 Brush 更新、root `Background` 定制取色，以及 disabled /
  loading 播放条件；验证取色发生在每次播放前，不缓存初始化颜色。
- Root 表面：验证本地 `Background`（含渐变）与 `BorderBrush` 由 `Frame` 渲染，并在 pointerover、pressed、disabled
  下保持定制值；清除后状态变色恢复；`Button.cs` 与 Button 家族主题不再包含 `CustomBackground`。
- Theme：检查 default、primary、dashed、text、link、solid、outlined、filled、danger 视觉；Text
  必须覆盖 Default、Primary、Danger 的 normal、hover、pressed 以及主题 Token 动态刷新。
- 家族控件：检查 DropdownButton 继承图标尺寸 API 且不影响 `OpenIndicator`，SplitButton 保持复合控件边界，IconButton、HyperLinkButton 保持既有同名 API 语义。
- Semantic Part：验证 registry 中只有 `root/icon/content`，共享 Button 模板实现相同 marker 数量与 ContractType，并用
  Avalonia 12 原生 class-only template selector 与 `x:SetterTargetType` 证明 icon/content marker 可编译并命中；批量
  Button 验证 class listener 结构预算和 detach 释放。
- 文档改动：运行 `git diff --check`。
