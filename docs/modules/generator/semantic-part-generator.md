# Semantic Part Generator 实现

Semantic Part Generator 为 AtomUI Control 的公开视觉区域生成静态 descriptor、名称常量、public Semantic Style 类型、
XML namespace 映射、包级注册和构建期诊断。
公共语义、Selector、Popup、Theme 和兼容性契约由
[AtomUI Semantic Part 系统设计](../../architecture/systems/theming/semantic-parts.md) 定义；本文档只定义生成器职责。

## 1. 设计定位

生成器把 Control 源码中的显式 Semantic Part 声明与构建系统提供的 `Themes/**/*.axaml` 组合为确定性输出。它不从
节点名称、控件分类或目录名称推测公共 Part，也不在运行时扫描 AXAML 或程序集。

生成器负责：

- 读取 public Control 上的 Semantic Part 声明。
- 验证 Part 名称、selector class、selector route、ContractType、cardinality 和 customization。
- 分析内置 AXAML 模板中的 `.semantic-*` marker。
- 关联已有 Semantic Part Theme 资产元数据。
- 生成 `ControlSemanticDescriptor`、Part 常量、public Semantic Style 类型、canonical XML namespace 映射和包级注册。
- 向文档和 Gallery 工具提供稳定静态输入。

生成器保持 `SelectorClass`、`SelectorRoute` 与 `ContractType` 正交：class 表达 `.semantic-*` Part 身份，route 表达从 public
owner 到目标的完整静态路径，type 验证 marker 节点类型并供文档工具读取为 `x:SetterTargetType`。生成器不得把
`ContractType` 拼接成类型限定 Semantic selector。

生成器不负责：

- 发明未声明的 Part。
- 修改 AXAML 或自动为模板节点添加 class。
- 运行时查找 VisualTree 节点。
- 验证应用或第三方包未作为当前 compilation 输入的自定义 ControlTheme。
- 把 Semantic Part 转换为 Token identity。

### 1.1 源码归属

| 文件 | 职责 |
| --- | --- |
| `SemanticParts/SemanticPartDeclaration.cs` | 从 Roslyn `AttributeData` 构造声明模型，按 Control symbol 合并 partial 声明并按 path 排序。 |
| `SemanticParts/SemanticPartModelBuilder.cs` | 按 Control identity 确定性合并声明，并编排契约验证、模板验证和最终输出模型。 |
| `SemanticParts/SemanticPartContractValidator.cs` | 校验 Control/Part 声明、唯一性、`ContractType` 和可选强类型 Theme 属性及资产契约。 |
| `SemanticParts/SemanticPartTemplateValidator.cs` | 解析适用模板继承链，并校验静态 marker、节点职责冲突、cardinality 和 marker 类型兼容性。 |
| `SemanticParts/SemanticPartTypeResolver.cs` | 在单次生成构建内预计算 XML namespace 映射，并缓存 TargetType、marker type 和 assignability 所需类型解析。 |
| `SemanticParts/SemanticPartManifestWriter.cs` | 生成 package manifest、per-Control 名称/class/route 常量、public Semantic Style 类型和 canonical XML namespace 映射。 |
| `ThemeAssets/ThemeAssetInfo.cs` | 解析 Theme AdditionalFile 的通用资产信息，并把 Semantic AXAML 结构交给专用解析器。 |
| `ThemeAssets/ThemeAssetSemanticInfo.cs` | 保存 ControlTheme、模板 variant 和 marker 的有序构建期语义模型。 |
| `ThemeAssets/SemanticThemeAssetParser.cs` | 从 `XElement` 结构提取 TargetType、typed BasedOn、模板 variant、节点 identity 与两种 marker 输入。 |
| `Registration/ControlPackageRegistrationWriter.cs` | 把生成的 Semantic descriptor 传入包级注册。 |
| `TokenResourceKeyGenerator.cs` | 组合 Attribute、Compilation、Theme AdditionalFiles 和 catalog 输入。 |

验证职责保持单向：`SemanticPartModelBuilder` 先调用 `SemanticPartContractValidator` 产生完整合法的 Part 集合，再调用
`SemanticPartTemplateValidator` 验证静态模板契约；两个 validator 共享同一个 `SemanticPartTypeResolver`，不得分别扫描
程序集 metadata 或维护相互独立的类型解释规则。

## 2. 输入模型

### 2.1 Control 声明

Control 使用可重复 `SemanticPartAttribute` 声明除 `root` 外的公开 Part。生成器通过固定 metadata name 识别
Attribute，不依赖 Attribute 实例反射。声明必须位于与 owner 同目录的 `<Control>.SemanticParts.cs` partial 文件中；多个
partial 声明按 CLR symbol 合并，只生成一个 descriptor、一份常量文件和一组 Style 类型。泛型 Control 在声明校验阶段拒绝。

推荐组织方式：

```text
Descriptions/
├── Descriptions.cs
└── Descriptions.SemanticParts.cs
```

`<Control>.SemanticParts.cs` 只承载 Attribute 声明和空的 partial class 块，不承载模板节点、Setter、Style 实例或运行时
VisualTree 查找逻辑。

声明字段包括：

```text
Name
Path
SelectorClass
SelectorRoute
ContractType
Cardinality
Customization
ThemePropertyName optional; SelectorAndTheme 时指向 public get/set ControlTheme 属性
CrossVisualRoot
Since
RuntimeCreated
```

`Since` 是具体的三段式发布版本（`major.minor.patch`，例如 `6.2.0`）：生成器按声明契约校验该格式，只写版本线的
`6.0` / `6.2` 会报告 `ATOMUIGEN038` 并阻断 manifest 与 Style 输出。缺失 `Since` 仍为 `ATOMUIGEN031` warning。

静态根模板 Part 未声明 `SelectorRoute` 时生成器规范化为 `/template/ .<SelectorClass>`。`RuntimeCreated=true` 必须显式提供
route，防止生成器、Gallery 或文档回退成无 owner 边界的 logical descendant。

`root` 由生成器为每个具有 Semantic Part 声明的 Control 隐式生成。

`StyleType` 由生成器根据 owner 和 Part path 确定：

```text
CLR namespace: AtomUI.Theme.Styling
Type name:     <ControlName><PartPathPascalCase>Style
```

`root` 不生成 Style type；例如 `Descriptions.label` 生成 `DescriptionsLabelStyle`，`Select.popup.option` 生成
`SelectPopupOptionStyle`。生成类型放在 owner Control 所在程序集，不集中进 `AtomUI.Core`。

### 2.2 AXAML 资产

构建集成继续把 `Themes/**/*.axaml` 作为 `AdditionalFiles` 提供给 Generator。生成器使用结构化 AXAML 分析读取：

- ControlTheme `TargetType`。
- typed `BasedOn` 继承关系（属性语法 `{StaticResource {x:Type ...}}` 与元素语法 `<ControlTheme.BasedOn>` 均支持）。
- ControlTemplate variant。
- AtomUI 自有模板中的静态 `Classes.semantic-*="True"` marker。
- 兼容输入中的字面量 `Classes="semantic-*"` marker。
- marker 所在节点的公开类型 identity。
- Browser 或其他平台主题资产。
- 叶子 Theme 资产与 owner Control 的既有映射。

生成器不得使用正则表达式替代 AXAML 结构分析，也不得依赖只用于聚合的 `*Themes.axaml` 推断模板完整性。

`Classes.semantic-*` 只有静态 `true` 才构成有效 marker。`False`、Binding 或其他动态值不能表示稳定模板契约；生成器
对已声明 Part 报告 `ATOMUIGEN036`，并把该节点排除在 cardinality、类型兼容和节点冲突校验之外。字面量 `Classes`
形式继续用于兼容既有或第三方模板，但 AtomUI 自有模板统一使用 class property 形式。

### 2.3 Semantic Part Theme 资产

现有 Theme asset generator 产生的 `ControlThemeSemanticPartDescriptor` 继续表达：

```text
Theme property name
Theme TargetType
Theme export / SemanticThemeBinding
Referenced Control identity
```

Semantic Part Generator 将该信息关联到 `Customization=SelectorAndTheme` 的 Part。存在同名 Semantic Theme 资产时，
解析其真实 `TargetType`，要求它可赋值给 `ContractType`，并要求平台变体使用同一个目标类型；descriptor 保存该真实类型。
没有对应资产时，Theme target metadata 回退到 `ContractType`。Selector-only Part 不创建
`ControlThemeSemanticPartDescriptor`。

### 2.4 Runtime-created Part

`RuntimeCreated=true` 表示 marker 由 C# 创建路径添加。生成器产生稳定 class 与 route 常量供 Control 使用，但不通过源码
文本搜索证明调用已经发生。声明必须提供合法 `SelectorRoute`；该契约由控件行为测试验证。

Runtime-created Part 仍需在 descriptor 中声明 `ContractType`、cardinality、owner 和跨视觉根信息。

## 3. 输出模型

### 3.1 ControlSemanticDescriptor

每个 Control 生成一个不可变静态 descriptor，内容包括：

```text
Control CLR identity
Control catalog identity
SemanticPartDescriptor[]
```

每个 `SemanticPartDescriptor` 包含：

```text
Name
Path
SelectorClass
SelectorRoute
ContractType CLR identity
StyleType CLR identity; root 为 null
Cardinality
Customization
Theme property metadata optional
CrossVisualRoot
Since
RuntimeCreated
```

Descriptor 不保存运行时 Type 发现逻辑、Style 实例、Setter、ControlTheme 实例或 VisualTree 引用；`StyleType` 以静态
`typeof(<Control><PartPathPascalCase>Style)` 引用写入 descriptor，root 写入 `null`。

### 3.2 Part 常量

生成器为 Control 产生 `<Control>SemanticParts.g.cs`，供 C# runtime-created 节点和测试使用：

```text
Part name constants
Selector class constants
Selector route constants
```

AXAML 仍直接承载 `.semantic-*` class；AtomUI 自有模板使用 `Classes.semantic-*="True"` 静态声明。生成常量不是第二套
命名来源，其值必须与 descriptor 完全一致。

### 3.3 生成 Semantic Style

每个非 root Part 生成一个 public sealed `Style` 子类。生成器必须保证以下固定契约：

```csharp
namespace AtomUI.Theme.Styling;

public sealed class DescriptionsLabelStyle : Style
{
    public DescriptionsLabelStyle()
        : base(static selector => selector
            .Nesting()
            .Template()
            .Class("semantic-scope-items")
            .Child()
            .Class("semantic-scope-item")
            .Template()
            .Class("semantic-label"))
    {
    }
}
```

生成规则：

1. `Nesting()` 把生成 Style 绑定到外层普通 Style 的匹配对象。
2. 外层普通 Style 的 selector 负责 Semantic owner、业务 class 和状态作用域；生成 Style 不重复匹配 owner 类型。
3. `SelectorRoute` 的 `/template/`、`>` 和 class token 按受限语法静态转换为 Avalonia Fluent Selector 调用。
4. 最后一个 class 必须是 Part 自身的 `SelectorClass`；生成器不把 route 降级为普通 descendant。
5. `root` 不生成 Style type；root Setter 由外层普通 Style 或 owner API 承担。
6. 生成 Style 不包含 Setter，不缓存 Style 实例，不解析 AXAML，不持有 Control 或 VisualTree 引用。

`ContractType` 不进入生成 Selector。用户在使用生成 Style 时必须显式声明 `x:SetterTargetType`，例如：

```xml
<Style Selector="atom|Descriptions.semantic-demo">
    <atom:DescriptionsLabelStyle x:SetterTargetType="ContentPresenter">
        <Setter Property="Foreground" Value="#A294F9" />
    </atom:DescriptionsLabelStyle>
</Style>
```

`x:SetterTargetType` 是用户 AXAML 的显式编译期契约，不参与 Selector 匹配、Part identity 或优先级。生成器不得自动
注入、隐藏、lowering 或替换该指令，也不修改 Avalonia 12 源码。

生成类型位于 owner Control 所在程序集。只要该程序集产生至少一个 Semantic Style，生成器必须确保存在以下等价的
canonical XML namespace 映射；已有等价映射时不得重复输出：

```csharp
[assembly: XmlnsDefinition("https://atomui.net", "AtomUI.Theme.Styling")]
```

去重只检查当前输出程序集上的完全等价映射。当前程序集为同一 CLR namespace 声明的其他 XML namespace 属于合法别名，
不能抑制 canonical 映射；引用程序集上的等价映射只服务于该引用程序集，也不能替代当前程序集输出。

用户因此继续使用 `xmlns:atom="https://atomui.net"`，不需要第二套 xmlns。

消费 descriptor 的文档或示例工具必须优先输出 `StyleType` 用法；不得把 `SelectorRoute` 拼成用户主路径，不得输出宽泛
logical descendant、`ContractType.semantic-*` 或 `:is(ContractType).semantic-*`。

### 3.4 包级注册

包级输出包括：

```text
GeneratedSemanticPartManifest.g.cs
<Control>SemanticParts.g.cs
<Control><PartPath>Style.g.cs
Semantic Part XML namespace mapping output
GeneratedControlPackageRegistration.g.cs
```

注册遵循已采纳的 [TypeMap 控件注册模型](control-registration.md)，源码迁移状态见
[AOT 与裁剪架构](../../architecture/foundations/aot-and-trimming.md#1-状态与事实边界)。

每个 Control 生成一个独立 Semantic descriptor factory，完整与选中路径调用同一 factory。控件片段通过共享模型或
确定命名的 partial hook 同时收集 Token、Semantic 与资源；不得调用全量 Semantic manifest 后再过滤。
生成的专用 Semantic Style 类型作为 owner 片段的保留条件，覆盖只使用样式类型的场景。

平台限制在片段引用工厂之前执行，与 Control/Token 使用同一可用性契约。各包先暂存记录，全部包收集后校验 owner 与
Control descriptor 的一致性，再构建冻结 registry。缺失描述或冲突在启动边界失败，不采用无语义描述的兼容分支。
运行时不反射 SemanticPart 声明或扫描 AXAML；TypeMap 的固定注册代理 Attribute 激活只属于受限启动路径。

## 4. 模板分析规则

### 4.1 Marker 完整性

对于 `RuntimeCreated=false` 的 Part，生成器验证每个适用 ControlTemplate variant：

- `Single` 必须存在且只能映射一个有效职责节点。
- `Optional` 可以不存在，但存在时必须类型兼容。
- `Multiple` 必须映射一个或多个节点，所有节点必须表示同一公开职责。
- marker 的节点类型必须可赋值给 `ContractType`。
- 同一节点只能携带一个已声明 Semantic Part marker。

### 4.2 Variant 覆盖

分析范围包括：

- 同一 ControlTheme 中由 selector 分支设置的不同 ControlTemplate。
- 同一 Control 家族的多个叶子 Theme 资产。
- Desktop 与 Browser 主题。
- 派生 Control 自有模板，或通过 typed `BasedOn` 明确复用的基类模板。

typed `BasedOn` 按 Avalonia 12 的应用顺序展开：派生 Theme 携带任何自有 ControlTemplate（含 selector 条件模板）时只
校验自有模板；Theme 自身没有可分析模板（纯继承）时，沿 typed `BasedOn` 递归到基类 Theme。通过资源引用形式
`Template="{StaticResource ...}"` 替换默认模板视为覆盖基类模板，不再校验被覆盖的基类。循环引用使用访问集合
终止，并最终报告没有适用模板。

生成器必须区分“该 variant 不适用”和“适用但漏标”。无法静态确定适用关系时，要求声明方提供明确资产归属，不能
默认为通过。

### 4.3 结构化解析边界

当前静态校验读取模板节点的字面量 `Classes` 和 `Classes.semantic-*` class property marker，不分析或重写
`Style.Selector` 文本。解析使用 XML attribute 结构，不从 AXAML 文本正则匹配。Selector 的 owner scope、`/template/`
边界和状态组合由架构规范、控件主题 review 与运行时 selector 测试保证。

`SelectorRoute` 自身使用受限语法验证，不调用 Avalonia runtime parser：route 必须从 `/template/`、`>` 或（仅限
`CrossNestedOwners=true` 部件的）`>>` 开始，并由“combinator + `.semantic-*` class”成对组成；禁止空格 descendant、
类型、Name、`PART_*`、属性 selector 和其他 token；最后一个 class 必须等于该 Part 的 `SelectorClass`。`>>` 开头的
路由服务于锚点节点位于 owner 模板属性值子树（无 `TemplatedParent` 传播）的场景，运行时由 Descendant 步进从 owner
到达锚点后再进入嵌套控件模板。这项验证只处理公开 route 元数据，不解析任意 AXAML Selector。

`BasedOn` 支持属性语法的 `{StaticResource {x:Type ...}}` 与元素语法的
`<ControlTheme.BasedOn><themes:X TargetType="..."/></ControlTheme.BasedOn>` 两种 typed 形式；元素语法按子元素的
Namespace 解析 `XTargetType` 引用。字符串 key、运行时资源选择或自定义 markup extension 无法静态解析，声明方必须
提供可分析的叶子模板或 typed `BasedOn`。

类型解析支持：

- `using:` XML namespace；
- 当前 compilation 的 `clr-namespace:` XML namespace；
- 通过 `assembly=...` 显式指定引用程序集的 `clr-namespace:` XML namespace；
- compilation 与引用程序集上的 `XmlnsDefinitionAttribute`；
- 当前 compilation 内唯一同名 public Control fallback。

XML namespace metadata 在一次生成构建开始时预计算；TargetType 与 marker type 按结构化引用缓存。模板数量或 marker
数量增加时不得为每次类型检查重复遍历所有引用程序集 attribute。缓存只存在于本次 Generator 构建对象中，不进入运行时
产物，也不形成跨 Compilation 的全局状态。

只用于聚合资源的 `*Themes.axaml` 不产生可分析模板；验证以包含真实 `ControlTemplate` 的叶子 Theme 资产为准。

### 4.4 Popup 与 Overlay

`CrossVisualRoot=true` 不触发 Theme 属性生成。生成器只记录该事实，并要求测试清单覆盖 PopupRoot 与
OverlayPopupHost。

模板内 Popup 的 marker 按正常模板节点分析。Popup 内容由运行时创建时使用 `RuntimeCreated=true`，并由测试证明：

- `TemplatedParent` 或 StyleHost 链连接到正确 owner。
- 生成 Semantic Style 的 owner-scoped Selector 可以命中。
- Popup 关闭和重新打开后 marker 保持一致。

### 4.5 ItemContainer

虚拟化或回收容器通常使用 `RuntimeCreated=true` 和 `Multiple`。生成器验证 descriptor，控件测试验证 prepare、clear、
recycle 和 owner 切换后的实际 marker。

重复容器的 marker 统一由列表 owner 在 `CreateContainerForItemOverride` 中向容器实例注入（先例：`ListBox.semantic-item`、
`CandidateList.PopupListItemClass`），不放入 item 的 ControlTheme 模板——生成器按"携带 marker 的元素类型必须可赋值给
`ContractType`"校验/解析，模板内部节点（如根 Panel）与容器 ContractType 不符会导致运行时永不命中；同时避免为语义
标记复制 item 模板。`RuntimeCreated=true` 的容器部件因此天然跳过模板 marker 校验（无宿主模板可查），由控件测试兜底。

弹层内列表项（如 AutoComplete 的 `popup.listItem`）在此之上再叠加：`CrossVisualRoot=true`（目标位于弹层独立可视根）、
路由以上一级列表键（`popup.list`）的 SelectorClass 为锚点（`>>` descendant 步进），marker 注入与容器生成时机一致。

### 4.6 跨嵌套控件模板部件

`CrossNestedOwners=true` 且路由越过首个模板边界（含 `>>`，或锚点类之后的第二个 `/template/`）的静态部件，
其 marker 位于嵌套控件自己的主题资产中，宿主模板校验不适用。生成器改为：

1. 从路由中首个嵌套边界操作符前的锚点类，在宿主模板中定位嵌套控件节点并解析其类型；
2. 在锚点类型基类链对应的主题资产中收集该部件 `SelectorClass` 的静态 marker——若嵌套控件经 `StyleKeyOverride`
   消费基类主题，只统计最派生主题，避免重复计数；
3. 校验 marker 数量满足 Cardinality（`ATOMUIGEN032`）且元素类型兼容 `ContractType`（`ATOMUIGEN033`）。

未声明 `CrossNestedOwners` 的部件即使路由含 `>>`（如 NumericUpDown prefix，marker 仍在宿主模板内），继续按宿主
模板校验。`RuntimeCreated` 部件不参与本节校验，沿用 4.5 的豁免规则。

## 5. 诊断

当前诊断为：

| ID | Severity | 触发条件 |
| --- | --- | --- |
| `ATOMUIGEN027` | Error | Control/Part 声明形态、名称、path、class、route、cardinality 或 customization 非法。 |
| `ATOMUIGEN028` | Error | 同一 Control 中 Part name、path 或 selector class 重复。 |
| `ATOMUIGEN029` | Error | `ContractType` 不是 public `StyledElement`。 |
| `ATOMUIGEN030` | Error | `SelectorAndTheme` 的 public get/set `ControlTheme` 属性契约非法。 |
| `ATOMUIGEN031` | Warning | Part 未声明 `Since`。 |
| `ATOMUIGEN038` | Error | Part 的 `Since` 不是 `major.minor.patch` 三段式发布版本（如版本线 `6.0` / `6.2`、带 `v` 前缀或预发布后缀）。 |
| `ATOMUIGEN032` | Error | 某个模板的 marker 数量不满足 `Single`、`Optional` 或 `Multiple`。 |
| `ATOMUIGEN033` | Error | marker 节点类型不能赋值给 `ContractType`。 |
| `ATOMUIGEN034` | Error | Control 声明了静态 Part，但没有适用的可分析 ControlTemplate。 |
| `ATOMUIGEN035` | Error | 同一模板节点同时声明了多个 Semantic Part marker。 |
| `ATOMUIGEN036` | Error | 已声明 Part 的 `Classes.semantic-*` marker 不是静态 `true`。 |
| `ATOMUIGEN037` | Error | 生成 Style 的完整 CLR identity 与另一生成候选、当前程序集已有类型或 canonical XML namespace 下可见的引用程序集 public 类型冲突。 |

生成 Style 的完整 CLR identity（程序集、namespace、type name）必须在当前 compilation 和可见 AtomUI Control 包中唯一。
同一 XML namespace 下出现不可区分的 public Style type、同一 owner 内生成名称冲突，或已有用户类型占用生成 identity 时，
生成器必须报告 `ATOMUIGEN037` 并阻断受影响 Control 的 Semantic manifest、常量和 Style 输出；不能静默改名、覆盖或生成
第二个 alias。引用程序集只有同时通过 canonical XML namespace 导出同名 public 类型时才构成 AXAML identity 冲突；
未公开类型或未映射到 canonical XML namespace 的同名 CLR 类型不误报。

同一 Control identity 的包级冲突由 Core `ControlPackageRegistration` 和 `ThemeManagerBuilder` 在启动注册边界拒绝。
控件文档与 descriptor 的一致性由文档 review 和文档验证流程负责，当前 Generator 不解析 Markdown。

Diagnostic ID、默认严重级别、消息格式和帮助链接统一遵循
[编译器诊断规范](../../engineering/development/compiler-diagnostics-guidelines.md)。Runtime-created Part 和
`CrossVisualRoot=true` 的行为覆盖属于测试验证要求，不伪装成 Generator 可以静态证明的诊断。

## 6. 增量生成

生成器输入必须按职责拆分：

```text
Compilation public Control declarations
+ SemanticPartAttribute symbols
+ Theme AdditionalFiles
+ AnalyzerConfig control catalog
+ Existing Theme asset semantic metadata
```

Semantic 声明和 Theme AdditionalFiles 在增量管线中分别收集，再按 compilation 生成确定性 package 输出。实现不承诺
单个 Control 修改时只执行该 Control 的 writer；正确性契约是输入相同则生成文本和诊断顺序相同。

没有 Semantic Part 声明时，model builder 必须在收集 public Control、构造类型解析器和预计算 XML namespace metadata
之前直接返回空模型。零采用路径不得因 Core 提供 Semantic runtime 而承担引用程序集 attribute 扫描成本。

排序规则必须稳定：

1. Control catalog identity。
2. Control identity。
3. Part path。
4. Theme asset `AssetPath` 与资产内模板顺序。

生成代码、diagnostic 顺序和 manifest 顺序不能依赖文件系统枚举顺序。

## 7. AOT 与裁剪

生成器和生成物必须满足：

- 不使用 `Assembly.GetTypes()`。
- 不使用 `Activator.CreateInstance()` 创建 descriptor 或 Theme。
- 不通过 `PropertyInfo` 查找 Theme property。
- 不在运行时解析 selector class、Part path 或 `SelectorRoute` 字符串。
- ContractType identity 由编译期 symbol 产生。
- 生成 Style 使用静态构造和 Fluent Selector；显式生成 Style 类型到 owner 注册片段的条件映射，不假设 selector 必然含 owner Type。
- 包级入口查询发布后端的已选映射并收集单项 descriptor；普通非裁剪路径使用同一事实源的全部片段。
- 第三方 Control 包使用同一 Generator，不提供反射 fallback。

Generator 项目仍以 Analyzer 方式参与构建，不进入应用 Runtime 或发布 payload。

## 8. 文档与 Gallery 集成

Control 文档以生成 descriptor 和真实 ControlTemplate 为事实源维护 Semantic Parts 表，不得根据 descriptor 发明
Abstract AXAML Structure。当前 Semantic Part Generator 不读取 Markdown 文档。

Gallery Semantic Preview 使用 descriptor 展示 Part 名称、StyleType、selector route、ContractType 和 cardinality。代码示例
优先生成 owner 外层 Style 与 `StyleType` 的嵌套用法，不根据 `RuntimeCreated` 猜测 Selector。预览工具可以使用
public VisualTree API 查找已实例化 `.semantic-*` marker；该查找只属于开发工具，不进入 Control 运行时。

## 9. 验证要求

生成器测试至少覆盖：

1. 合法声明、StyleType 命名和确定性输出。
2. 重复名称、非法 class 和 ContractType 错误。
3. Single、Optional、Multiple marker 数量。
4. 多 ControlTemplate 和 Desktop/Browser variant。
5. Selector-only 与 SelectorAndTheme 的差异。
6. 现有 `ControlThemeSemanticPartDescriptor` 关联。
7. Popup、runtime-created Part、显式 route 和 item container 元数据。
8. 旧 Core 引用下保持四参数 package registration 的兼容路径。
9. 包级注册、生成 Style、XML namespace 映射、生成顺序与 NativeAOT 友好输出。
10. 静态 class property marker、字面量兼容 marker，以及 false/dynamic marker 的 `ATOMUIGEN036` 诊断。
11. 静态 Part 默认 route、RuntimeCreated 缺失 route、非法 route token 和 route 末尾 class 不一致的 `ATOMUIGEN027` 诊断。
12. `Nesting()`、多段 `/template/`、`Child()` 的 Fluent Selector 生成结果，以及 owner selector 由外层 Style 提供的约束。
13. `x:SetterTargetType` 显式保留、StyleType identity 冲突和 canonical XML namespace 去重。
