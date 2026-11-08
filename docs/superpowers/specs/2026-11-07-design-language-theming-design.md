# 多设计语言主题架构设计

日期：2026-11-07。状态：设计草案，待用户审阅；产品实现未启动。

基线：`release/6.0`（`ae5ac0a32`，AtomUI 6.2.13，Avalonia 12.1.5）。分支 `feature/custom-theme` 的全部改动不作为参考，本文不复用其设计、术语或实现。

## 1. 目标

让 AtomUI 应用在运行时切换多种设计语言（Ant Design、Material、shadcn、Bootstrap……），效果对齐 ant.design 首页 "Flexible theme customization" 的预设切换器：同一棵控件树，不重建、不丢状态，切换后整体外观变为另一套设计语言；同一应用内允许不同子树使用不同设计语言。

成功标准：

1. Gallery 增加一个与 ant.design 预览区等价的"设计语言预览"页，至少提供 Ant、Material、shadcn、Bootstrap 四套，切换在一次 UI 线程 Publish 内完成，不重挂载子树，输入框文字、选中项、滚动位置、焦点保持。
2. 第三方控件包与应用可以为任一设计语言提供自己的样式片段与 Token 片段，不修改 AtomUI 源码。
3. 不依赖 Avalonia 私有成员、不 hook 框架内部、不修改 Avalonia；仅使用公开且在 12.1.5 源码中核对过的行为。
4. 对 `release/6.0` 的改动，除新增能力外只允许三类：简化代码、提升性能、架构优化；每一处在本文第 9 节登记理由。

## 2. 参考实现：ant.design 的切换器是怎么做的

来源：`ant-design/.dumi/pages/index/components/ThemePreview/previewThemes/*.ts`（master）。14 个预设全部是普通 `ConfigProvider` 配置对象：

| 机制 | 用途 | 使用预设数 |
| --- | --- | --- |
| `theme.token` | 全局 Token（颜色、圆角、字体、控件高度……） | 14/14 |
| `theme.components` | 逐组件 Token | 14/14 |
| `theme.algorithm` | 明暗算法 | 14/14 |
| 逐组件 `classNames` / `styles` | 对语义 DOM 区域注入 CSS（渐变、硬投影、backdrop blur、霓虹阴影、全大写、自定义 ripple） | 7/13 |
| 结构/模板替换、图标替换、`cssVar`、`componentSize` | 无 | 0 |

预览区 JSX 固定，DOM 结构在所有预设下完全一致，切换只改 class 与注入的 CSS。结论：**设计语言 = Token 层 + 语义区域样式层**，两层都必须能按作用域热切换；结构层（模板）不是必需，但作为可选扩展保留。

对应到 AtomUI 已有的原语：

| ant.design | AtomUI release/6.0 已有 | 缺口 |
| --- | --- | --- |
| `token` / `components` / `algorithm` | `ThemeConfig.Tokens` / `Controls` / `Algorithms`，`ThemeConfigProvider` 作用域 | 没有"按设计语言打包并切换"的入口 |
| `classNames` / `styles` | Semantic Part：`.semantic-*` marker + 生成的 `XxxPartStyle` 嵌套样式 | 只能由应用手写进 `Application.Styles`，不能按语言打包、不能按作用域切换 |
| 模板差异 | 每个控件一个 `ControlTheme`，Style 可设置 `Template` | 同上 |

所以本设计不新建第二套样式系统，而是给现有 Token 层和 Semantic Part 层各加一个"按设计语言分组并按作用域激活"的维度。

## 3. 核心概念

### 3.1 三个正交轴

```text
Theme definition（色板，XML v1，可来自用户目录）   DaybreakBlue / 用户主题 …
Design language（形态 + 语义样式 + 语言默认 Token） Ant / Material / Shadcn / Bootstrap …
Algorithms（派生算法）                              Default / Dark / Compact
```

一个作用域的有效外观 = 三者的组合。`Ant` 是内置默认设计语言，等于当前 `release/6.0` 外观，不携带任何样式片段。

### 3.2 设计语言由什么组成

| 组成 | 内容 | 所有者 |
| --- | --- | --- |
| `DesignLanguageDefinition` | `Id`、显示名、语言默认全局 Token（`Tokens`）、暗色追加 Token（`DarkTokens`） | 定义该语言的程序集，一个语言一个 |
| `DesignLanguageFragment` | 面向某个控件包的片段：该语言下这些控件的 Own/Effective Global Token 覆盖，以及一个 `DesignLanguageStyles`（AXAML） | 拥有这些控件的程序集（AtomUI 控件包、第三方控件包、应用） |

一个语言可以有多个片段，来自多个程序集；运行时按 `Id` 聚合。片段引用的 `Id` 没有对应定义时，启动阶段给出诊断并忽略该片段，不阻断启动。

### 3.3 激活模型：一个可继承附加属性

新增 `ThemeScope.DesignLanguageProperty`：`AttachedProperty<string>`，`inherits: true`，与现有 `ThemeScope.ContextProperty` 并列。每个样式片段中的顶层 `Style` 在挂载时被追加限定 `.PropertyEquals(ThemeScope.DesignLanguageProperty, "<Id>")`。

切换语言只改变根或局部作用域上这个属性的值；Avalonia 的 `PropertyEqualsActivator` 订阅目标控件的属性 observable（`PropertyEqualsActivator.Initialize` → `_control.GetObservable(_property)`），继承值变化沿树传播即触发 Setter 激活/退役。这与伪类切换走同一条框架路径，不增删 `Styles`，因此不触碰 Avalonia 移除样式时模板节点 frame 不退役的问题。

### 3.4 Token 层

设计语言的 Token 作为一个新的合并层进入现有 `ThemeConfigMerger`：

```text
library defaults
  → theme definition tokens           （色板）
  → design language Tokens            （语言默认：圆角、高度、字体、线宽、必要时主色）
  → design language DarkTokens        （仅当有效外观为 Dark 时追加）
  → fragments 的 Controls 覆盖        （语言下逐控件 Token）
  → root ThemeRequest.Config          （应用全局覆盖）
  → 局部 ThemeConfigProvider.Config   （作用域覆盖，沿父链继承）
```

决策：语言 Token 覆盖主题定义，用户配置再覆盖语言。理由：选择 shadcn 的用户期待 zinc 色系，这是语言身份的一部分；不想要时在请求里覆盖 `ColorPrimary` 即可。不设计 Token 的"按语言条件"语法，`DarkTokens` 是唯一的条件维度，由规范化阶段按有效外观选择。

`ThemeConfig` 新增可空字段 `DesignLanguage`（`string?`）：根请求 `null` 表示 `Ant`；局部作用域 `null` 表示继承父作用域。它参与请求缓存键与 Snapshot 身份，语言不同的两个作用域不共享控件编译结果。

## 4. 公开 API

全部位于 `AtomUI.Core`，命名空间 `AtomUI.Theme`（定义/片段/注册）与 `AtomUI.Theme.Resources`（AXAML 容器）。

```csharp
public sealed class DesignLanguageDefinition
{
    public DesignLanguageDefinition(string id, string displayName,
        ThemeConfig? tokens = null, ThemeConfig? darkTokens = null);
    public string Id { get; }
    public string DisplayName { get; }
    public ThemeConfig? Tokens { get; }      // 只允许 Tokens；Algorithms/Controls/Inherit 非默认时构造失败
    public ThemeConfig? DarkTokens { get; }
}

public sealed class DesignLanguageFragment
{
    public DesignLanguageFragment(string languageId,
        IReadOnlyDictionary<ControlTokenIdentity, ControlThemeConfig>? controls,
        Func<DesignLanguageStyles>? styles);
    public string LanguageId { get; }
    public IReadOnlyDictionary<ControlTokenIdentity, ControlThemeConfig> Controls { get; }
    public Func<DesignLanguageStyles>? Styles { get; }   // 惰性，首次激活该语言时调用一次
}

// AXAML 根元素。普通 Styles 子类，仅多一个 Language 属性。
public class DesignLanguageStyles : Styles
{
    public string Language { get; set; }
}

public static class DesignLanguageIds
{
    public const string Ant = "Ant";
    public const string Material = "Material";
    public const string Shadcn = "Shadcn";
    public const string Bootstrap = "Bootstrap";
}

public interface IThemeManagerBuilder
{
    void AddDesignLanguage(DesignLanguageDefinition definition);
    void AddDesignLanguageFragment(DesignLanguageFragment fragment);
    // 其余不变
}

public interface IThemeManager
{
    IReadOnlyList<DesignLanguageInfo> AvailableDesignLanguages { get; }   // Id、DisplayName、片段数
    // 其余不变
}

public sealed class ThemeConfig { public string? DesignLanguage { get; } /* 其余不变 */ }
public sealed class ThemeConfigBuilder { public ThemeConfigBuilder WithDesignLanguage(string? id); }
public sealed record ThemeState { public string DesignLanguage { get; } /* 其余不变 */ }
```

`ControlPackageRegistration` 增加 `IReadOnlyList<DesignLanguageFragment> DesignLanguageFragments`，由生成器从约定目录产出（见第 6 节）；手工注册走 `AddDesignLanguageFragment`。

不提供：运行时从文件加载样式、JSON/XML 承载样式、任何形式的配方对象、AI 生成入口。设计语言是代码交付的程序集，与控件包同一生命周期。

## 5. 运行时

### 5.1 ThemeManager 新增状态

- `_designLanguages`：`Id → (Definition, List<Fragment>)`，启动注册期冻结。
- `_mountedLanguageStyles`：`Id → Styles`，已挂载到 `Application.Styles` 的片段样式；只增不减，上界为已注册语言数。
- `RootDesignLanguage`：`IObservable<string>`，根作用域当前语言，供根 TopLevel 样式的 Setter 绑定。

### 5.2 一次切换的事务路径

沿用现有 Capture / Prepare / CommitCore / Publish / Complete，不加阶段：

1. **Capture**：请求携带 `DesignLanguage`。未注册的 Id 在 Normalizer 失败，`Failed` 返回诊断，不进编译。
2. **Prepare**（可后台）：Normalizer 把语言 Tokens/DarkTokens/片段 Controls 合并进有效配置；编译器照常产出 Snapshot。语言不同即有效配置不同，缓存键自然区分。
3. **CommitCore**（UI 线程，无通知）：替换 Snapshot 引用（不变）。
4. **Publish**（UI 线程）按固定顺序：
   1. 对本次事务涉及的每个作用域的目标语言，若 `_mountedLanguageStyles` 没有，则创建并挂载（5.3）。挂载在属性切换之前，保证属性变化时样式已就位，不出现"属性已切、样式未到"的帧。
   2. 根事务：推送 `RootDesignLanguage`；局部事务：`ThemeConfigProvider` 在既有 `PublishCommittedContext` 中 `SetValue(DesignLanguageProperty, effective)`。
   3. 既有 Token 资源通知、`RequestedThemeVariant`、事件。

   Avalonia 在同一次 UI 线程回合内处理样式失效与属性变化，首个可见帧已是完整新状态，与现有"Publish 阶段才可见"的保证一致。
5. **Complete**：`ThemeState.DesignLanguage` 更新。

### 5.3 片段样式的挂载与限定

首次激活语言 `L` 时：

1. 对 `L` 的每个片段调用 `Styles()` 工厂（AXAML 编译的 `DesignLanguageStyles`），校验 `Language == L`。
2. 遍历其顶层 `Style`，把 `Selector` 替换为 `Selector.PropertyEquals(ThemeScope.DesignLanguageProperty, L)`。只处理顶层；嵌套样式通过 `^` 继承父选择器。`Style.Selector` 是公开可写属性，在未挂载前修改是公开用法。
3. 把片段 `Styles` 追加到 `Application.Styles`（ThemeManager 本身是 `Styles` 宿主，追加到其后保持"框架样式先于应用样式"的既有顺序）。

限定追加在选择器链末端，Avalonia 按链从左到右求值并在首个不匹配处短路（`Selector.Match` → `MatchUntilCombinator`），因此 `atom|Button:pointerover` 这类选择器先做类型判断，非 Button 元素既不创建 activator 也不订阅；只有匹配类型的元素才为 `DesignLanguageProperty` 建立一个订阅。成本与"再加一条按钮伪类样式"同阶。

不卸载：语言 `L` 退役只靠属性值不再等于 `L`，所有 Setter 按框架路径退役。已挂载但当前无人使用的语言样式只保留订阅成本，数量受已注册语言数限制。

### 5.4 属性在树上的来源

| TopLevel 类型 | 值来源 |
| --- | --- |
| 无逻辑父级的 Window | 根样式 `Is<TopLevel>().PropertyEquals(StyledElement.ParentProperty, null)`，Setter 绑定 `RootDesignLanguage` |
| 带 owner 的独立 TopLevel（Dialog、Notification host 等） | `ThemeContextLease.Attach` 在设置 `ContextProperty` 与 `RequestedThemeVariant` 时同时设置本地值；Dispose 时同样清除 |
| PopupRoot / OverlayPopupHost | 有逻辑父级，不命中根样式，沿 Popup 的继承链自然取得作用域值 |
| `ThemeConfigProvider` 子树 | Provider 本地值，Publish 时更新 |

现有 `ContextProperty` 根样式使用 `OfType<TopLevel>()`，会把根 Context 以样式值写到 PopupRoot 上并覆盖其从 Popup 继承来的局部 Context。本设计把两个属性共用同一条 `Parent == null` 限定的根样式（见第 9 节的改动登记）。

### 5.5 片段中的资源解析

片段样式的 Setter 使用 `{atom:XxxTokenResource ...}` / `{atom:SharedTokenResource ...}`，即 DynamicResource。绑定在目标控件上实例化，沿目标控件的资源链解析，因此局部 `ThemeConfigProvider` 的 Token Provider 对片段样式同样生效，片段无需感知作用域。

片段的 `Styles.Resources` 允许放只被该片段自身引用的 `ControlTheme`/模板/画刷；禁止放会被应用以 key 查找的公共资源（挂载顺序不保证）。

### 5.6 片段可以做什么

| 需求 | 写法 |
| --- | --- |
| 根属性（圆角、边框、阴影、字重、Padding） | `<Style Selector="atom|Button"><Setter .../></Style>` |
| 语义区域（图标、内容、弹层框体……） | 外层 owner Style + 嵌套生成的 `atom:ButtonIconStyle` 等 |
| 状态 | `<Style Selector="atom|Button:pointerover">`、`^:pressed`、属性选择器 |
| 结构差异 | `<Setter Property="Template">` 直接替换模板；模板内须保留该控件的 Semantic marker |
| 语言专属视觉效果（渐变、硬投影、模糊） | Setter 值为片段资源中的画刷/效果；需要代码时片段程序集可携带转换器与装饰器 |
| 该语言下控件默认 Token | 片段的 `Controls` 覆盖，或语言定义的 `Tokens` |

不允许：在片段里写 `[(ThemeScope.DesignLanguage)=…]`（由运行时追加，重复限定报错）、修改 `Application.Resources`、引用其他语言的样式。

## 6. 作者约定与生成器

### 6.1 目录约定

```text
Buttons/
+-- Button.cs
+-- Button.SemanticParts.cs
+-- ButtonToken.cs
+-- Themes/
|   \-- ButtonTheme.axaml                  Ant 基线，不变
\-- DesignLanguages/
    +-- Material/
    |   \-- ButtonMaterialStyles.axaml      <atom:DesignLanguageStyles Language="Material">
    \-- Shadcn/
        \-- ButtonShadcnStyles.axaml
```

生成器把 `DesignLanguages/<Id>/**/*.axaml` 按 `<Id>` 聚合为一个 `DesignLanguageFragment`（一个程序集一个语言一个片段，内部用一个外层 `DesignLanguageStyles` 以 `StyleInclude` 聚合各文件）。该语言下的控件 Token 覆盖写在同目录的 C# 文件里，用一个静态方法声明：

```csharp
// DesignLanguages/Material/MaterialTokens.cs
internal static class MaterialTokens
{
    [DesignLanguageControlTokens(DesignLanguageIds.Material)]
    internal static IEnumerable<KeyValuePair<ControlTokenIdentity, ControlThemeConfig>> Create() => [ ... ];
}
```

生成器把同一 `Id` 的样式工厂与 Token 方法并入同一片段；一个程序集内同一 `Id` 出现两个方法报错。产物进入 `ControlPackageRegistration.DesignLanguageFragments`，走现有包注册路径，运行时不扫描目录、不反射。

构建期校验（新增诊断号沿用 `ATOMUIGEN` 序列）：

- 根元素必须是 `DesignLanguageStyles` 且 `Language` 与目录名一致。
- 顶层 Style 选择器必须以本程序集或其依赖包中已注册的 Control 类型开头；禁止 `:is(Control)`、裸 class、通配。
- 顶层选择器不得含 `/template/`、`>`、后代组合子；语义区域必须用生成的 Part Style 嵌套表达。
- `Template` Setter 的模板必须包含该控件 descriptor 声明的全部非 Optional Semantic marker。
- 不得出现对 `ThemeScope.DesignLanguage` 的手写限定。

### 6.2 语言定义的位置

| 程序集 | 内容 |
| --- | --- |
| `AtomUI.Core` | 机制；`DesignLanguageIds` 常量；`Ant` 定义（空 Token、无片段） |
| `AtomUI.Desktop.DesignLanguages`（新） | `Material`、`Shadcn`、`Bootstrap` 的定义（全局 Token、DarkTokens）与对 `AtomUI.Desktop.Controls` 控件的片段 |
| `AtomUI.Desktop.Controls.DataGrid` / `ColorPicker` / `Extras` | 各自控件在这些语言下的片段，放在各自的 `DesignLanguages/` 目录；它们引用 `DesignLanguageIds` 常量，不引用新程序集 |

应用通过 `UseDesktopDesignLanguages()` 一次注册内置三套；不调用则只有 `Ant`，零额外开销。第三方包按 6.1 约定或手工 `AddDesignLanguageFragment` 参与。

## 7. Gallery

- 工作台顶栏增加设计语言切换（与明暗、紧凑并列），调用 `ApplyThemeAsync` 并在 `ThemeConfig` 设置 `DesignLanguage`。
- 新增"设计语言预览"展示页：复刻 ant.design 预览区的控件集合（输入、选择、日期、开关、步骤、进度、徽标、按钮组、验证码、卡片、通知、对话框等），页面内一个 `ThemeConfigProvider` 承载预览，切换只改该 Provider 的 `Config`，演示局部作用域能力；同页可同时放两个不同语言的 Provider 做对照。
- 现有 CustomizeTheme 页继续演示 Token 覆盖。

## 8. 性能、内存与 AOT

- 切换成本 = 一次 Token 事务 + 属性变化引发的 Setter 激活/退役；首次激活某语言额外一次 `Styles` 挂载（`InvalidateStyles(recurse)` 一次）。目标：Gallery 预览页（约 60 个控件）切换在 16 ms 内完成 Publish，首次挂载不超过 50 ms；以 Stopwatch 计 Publish 区间，不以截图计时。
- 常驻订阅：每个已挂载语言，每个被其顶层选择器类型命中的元素一个 `PropertyEqualsActivator`；四套语言下相当于每个控件多三条伪类样式。超出接受范围时的优化手段是减少顶层 Style 数量（合并同类型），不是卸载。
- Token 侧：语言 Token 只是合并层；缓存键包含语言，`LazyControlSnapshots` 按控件惰性物化不变，无新缓存。
- AOT：定义与片段都是静态构造 + AXAML 编译产物，无反射、无 `Activator`、无字符串类型名；`DesignLanguageStyles` 是普通 `Styles` 子类。

## 9. 对 release/6.0 的改动登记

| 改动 | 类别 | 理由 |
| --- | --- | --- |
| 根 Context 样式选择器由 `OfType<TopLevel>()` 改为 `Is<TopLevel>().PropertyEquals(ParentProperty, null)`，并与 `DesignLanguage` 共用一条样式 | 架构优化 | 消除根样式覆盖 PopupRoot 继承值的隐患；一个根样式承载两个根值 |
| `ThemeContextLease` 同时管理 `DesignLanguageProperty` | 新能力所需 | 独立 TopLevel 与局部作用域语言一致 |
| `ThemeConfig` 增加 `DesignLanguage`；Normalizer/Merger 增加语言层 | 新能力所需 | 复用既有合并器，不新建配置对象 |
| `ThemeState`/`ThemeInfo` 旁新增 `DesignLanguageInfo` | 新能力所需 | 只读观察 |

不改动：事务结果枚举、算法枚举、Own Token 求值、资源扩展、Semantic Part 生成、启动流程、用户主题目录、编译主题包。不引入别名、兼容层或过渡 API。

## 10. 验证

单元（`AtomUI.Core.Tests`）：

- 合并顺序：主题定义 < 语言 Tokens < DarkTokens（仅 Dark）< 片段 Controls < 根配置 < 局部配置，各层逐对验证。
- 未注册语言 Id → `Failed` 且当前状态不变；片段引用未知 Id → 启动诊断，语言列表不含它。
- 首次激活挂载一次，再次激活不重复挂载；A→B→A 后 `Application.Styles` 中语言样式数量不变。
- 属性传播：根 Window、带 owner 的 Window（lease）、PopupRoot、`ThemeConfigProvider` 子树四种宿主在切换后 `DesignLanguageProperty` 值正确；lease 释放后不残留。
- 选择器限定：顶层 Style 被追加限定且仅一次；嵌套样式未被修改。
- 泄漏：关闭带 lease 的窗口、移除 Provider 后 WeakReference 回收。

集成（`AtomUI.Desktop.Controls.Tests` / Gallery 测试）：

- 每套语言 × 预览页：切换后关键区域（按钮背景/圆角/阴影、输入框边框、开关轨道、步骤条连接线）按已采用的容差策略对比各自基准；不设整帧零差异门槛。
- 状态保持：切换前后 TextBox 文本与光标、ListBox 选中项、ScrollViewer 偏移、焦点元素相同。
- 局部作用域：同页两个 Provider 分别为 Material/Shadcn，各自子树命中各自样式，Popup 跟随所在作用域。
- `Template` Setter 替换模板后 Semantic Part 预览仍能解析全部非 Optional 部件。

性能：第 8 节两项计时在 Gallery headless 宿主下由测试断言。

## 11. 实施顺序

1. **Spike（半天）**：在 `release/6.0` 上用一个临时 Styles 验证三件事并记录结果：`PropertyEquals` 对继承附加属性值变化的实时激活；`Application.Styles` 运行时追加后已挂载控件被重新套用；Style 中 `Template` Setter 切换对已挂载控件实时生效且 Semantic marker 保留。任一不成立即停下修订本设计。
2. Core 机制：属性、定义/片段/注册、合并层、挂载与限定、lease、根样式合并、`ThemeState`。
3. 生成器：目录约定、片段聚合、六条校验。
4. `AtomUI.Desktop.DesignLanguages`：先做 Button、Input 家族、Select、Checkbox/Radio/Switch、Steps、Progress、Tag、Card、Alert、Dialog、Notification 这组预览集合的 Material/Shadcn/Bootstrap 片段。
5. Gallery 切换器与预览页；验证第 10 节。
6. 扩展到其余控件包（DataGrid、ColorPicker、Extras）。

每一步独立提交、独立验收；第 4 步之后即可在 Gallery 看到 ant.design 式切换效果。

## 12. 已决策事项（供审阅时推翻）

| 问题 | 决策 | 替代方案及为何不选 |
| --- | --- | --- |
| 设计语言能否作为数据文件交付 | 不能，只能是程序集 | 样式必须是编译 AXAML；数据化样式需要自建解析与校验，分支已证明成本极高且不 AOT 友好 |
| 语言 Token 与主题定义谁优先 | 语言覆盖主题，用户配置覆盖语言 | 主题覆盖语言会让 shadcn 失去自身色系，需要再引入"语言推荐主题"机制 |
| 语言样式是否卸载 | 不卸载，靠属性退役 | 卸载触碰 Avalonia 移除样式的 frame 残留问题，需要私有 hook |
| 结构替换方式 | Style 中 `Template` Setter | 替换以类型为 key 的 ControlTheme 资源只在挂入树时解析，要求重挂载 |
| 首批语言 | Material、Shadcn、Bootstrap | 与 ant.design 预设对齐，覆盖"仅 Token"与"需要语义样式"两类 |
| 是否支持一个控件同时属于多个语言 | 不支持；属性值唯一 | 多值匹配需要 class 级机制，且无产品需求 |
