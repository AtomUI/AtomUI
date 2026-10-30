# Control Design Token 继承架构

> 状态：已实施。本文定义 Control Design Token 定义继承的正式架构契约。

本文只适用于 `AbstractControlDesignToken` 下的 Control Own Token。Global DesignToken、Seed/Map/Alias Token、
ThemeConfig 继承和 Control CLR 继承不属于本文范围。

## 设计定位

Control Design Token 继承用于复用平台无关的 Control Own Token 定义和默认计算。Desktop、Mobile 或第三方产品包可以从
共同上游继承同一抽象 Token 层，同时继续拥有独立的 Control identity、配置、snapshot 和资源投影。

继承是生成期的源码定义复用机制，不是运行时 Token identity 继承机制。Generator 将完整继承链扁平化为每个终端
Control 的独立 Own Token schema；运行时只消费终端 descriptor，不保存或遍历 Token 继承关系。

## 核心不变量

- `[ControlDesignToken]` 是抽象定义层和终端 Token 的唯一标记，不增加 `ControlDesignTokenBase`、
  `ControlDesignTokenDefinition` 或泛型 Attribute。
- 标记的 `abstract` 类是无 identity 的非终端复用层；标记的具体类是终端 Token，而且必须 `sealed`。
- 除 `AbstractControlDesignToken` 根类型外，继承链每一层都必须显式标记 `[ControlDesignToken]`。
- 抽象层只贡献 Own Token 属性与默认计算，不生成 identity、descriptor、slot、资源键或注册产物。
- 终端 Token 是链中唯一的 Control identity owner，生成完整且独立的扁平 schema。
- identity、配置、snapshot、资源作用域和运行时值不沿 Token CLR 继承链共享或回退。
- 继承层次不进入 schema fingerprint；只移动属性声明位置不能改变终端 schema revision。
- 运行时不得新增反射、程序集扫描、动态构造、基类 descriptor 查找或 identity fallback。

## 类型模型

### Attribute 契约

现有 Attribute 保持无参数并禁止隐式继承：

```csharp
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ControlDesignTokenAttribute : Attribute
{
}
```

`Inherited = false` 是 opt-in 边界。Generator 必须检查每个类型自身的 Attribute，不能把基类 Attribute 视为派生层声明。

### 抽象定义层

```csharp
[ControlDesignToken]
public abstract class CommonButtonToken : AbstractControlDesignToken
{
    public double ContentHeight { get; set; }
    public Thickness ContentPadding { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        ContentHeight = EffectiveGlobalToken.ControlHeight;
        ContentPadding = EffectiveGlobalToken.InputPadding;
    }
}
```

抽象定义层遵守以下契约：

- 可以形成多层单继承链，也可以在自身程序集没有终端派生类时独立编译；
- 不按类型名称匹配 Control，名称只需以 `Token` 结尾；
- 不拥有 Catalog、Control ID、factory、evaluator、descriptor、slot、资源键或注册片段；
- 必须是非泛型顶级类；
- 对第三方开放时使用 `public abstract`，仅供已知第一方下游包复用时可以使用 `internal abstract` 与正常的
  `InternalsVisibleTo`；
- 使用表达稳定视觉语义的家族名称，不要求 `Base` 或 `Definition` 后缀。

### 终端 Token

```csharp
namespace AtomUI.Desktop.Controls
{
    [ControlDesignToken]
    internal sealed class ButtonToken : CommonButtonToken
    {
        public BoxShadows DefaultShadow { get; set; }

        public override void CalculateTokenValues(bool isDarkMode)
        {
            base.CalculateTokenValues(isDarkMode);
            DefaultShadow = EffectiveGlobalToken.BoxShadows;
        }
    }
}
```

终端 Token 遵守以下契约：

- 必须具体、`sealed`、非泛型且位于顶级；
- 继续按照现有名称和命名空间约定匹配 public Control；
- 可以直接继承 `AbstractControlDesignToken`，也可以继承一层或多层显式标记的抽象 Token；
- 不能继承另一个具体 `[ControlDesignToken]`；
- 是继承链中唯一生成 Control identity、descriptor、factory、evaluator、资源键和注册产物的类型。

合法结构为：

```text
AbstractControlDesignToken
  └── [ControlDesignToken] abstract CommonInteractiveToken
        └── [ControlDesignToken] abstract CommonButtonToken
              ├── Desktop: [ControlDesignToken] sealed ButtonToken
              └── Mobile:  [ControlDesignToken] sealed ButtonToken
```

## 属性扁平化

Generator 从终端沿 `BaseType` 遍历到准确的 `AbstractControlDesignToken`，再按 base-to-derived 顺序读取每一层直接声明的
属性。一个可进入 Own Token schema 的属性必须满足：

- 是实例属性且不是 indexer 或显式接口实现；
- 普通 getter 和 setter 都存在，并可由终端程序集的生成代码访问；`init`-only setter 与 `required` 属性不受支持；
- 未标记 `[NotTokenDefinition]`；
- 不是 `virtual`、`abstract`、`override` 或使用 `new` 隐藏的属性。

不同层声明同名属性、同名不同类型或任何隐藏关系都必须构建失败。继承属性与任一 Global Token 同名时继续使用
Own/Global 名称冲突诊断，不能通过声明层或类型限定符消除歧义。

属性值类型继续服从现有 `TokenValueConverterRegistry` 运行时契约；本次继承改造不新增编译期 converter 白名单，也不把
converter 注册状态并入 `ATOMUIGEN023`。未注册类型会保持现有行为，在主题配置字符串解析时明确失败。

合并后的属性按名称使用 `StringComparer.Ordinal` 排序并分配连续 slot。生成的 getter、setter 和 resource projector
统一转换为终端 Token 类型，不要求运行时引用抽象声明层：

```csharp
static token => ((ButtonToken)token).ContentPadding;
static (token, value) =>
    ((ButtonToken)token).ContentPadding = (Thickness)value!;
```

扁平 schema 只包含终端 identity、Token 名称、值类型、stage、slot 和 resource key。声明层类型、程序集、继承深度和
源码顺序只用于诊断，不进入 schema 或 revision。

## 默认值计算链

`CalculateTokenValues(bool isDarkMode)` 使用普通 C# virtual dispatch。Generator 只为终端生成一个 evaluator，运行时调用
终端实现；各继承层由显式 `base` 调用组成确定的计算链，不生成逐层 evaluator pipeline。

当某层 override 位于复用链的后续层时，方法必须：

1. 使用 block body；
2. 将 `base.CalculateTokenValues(isDarkMode)` 作为第一条可执行语句；
3. 原样转发当前 `isDarkMode` 参数；
4. 整个方法体只调用一次直接基类实现；
5. 在 base 返回后计算当前层属性。

base 调用不能放在条件、循环、异常块、lambda 或局部函数中，也不能在调用前写入 Token。某层没有 override 时正常继承
基类实现。直接继承根类型的终端和直接重写根方法的第一层抽象 Token 不要求调用根类型的空实现；后续层再次 override
时必须调用直接基类。

定义抽象层的程序集负责验证自身源码方法体。下游 Generator 可以从 metadata 验证类型结构和方法签名，但不能重新证明
已编译的方法体；因此发布可复用抽象 Token 的程序集也必须使用兼容版本的 AtomUI Generator。

## Identity 与运行时边界

抽象定义层没有 `ControlTokenIdentity`。每个终端 Token 按匹配的 exact Control 和所属 Catalog 获得独立 identity，继承同一
抽象属性不会共享 descriptor、slot、配置值或 snapshot 存储。

运行时流程保持：

1. 终端 descriptor factory 直接构造终端 Token；
2. `ThemeCompiler` 注入该 identity 的 Effective Global Token；
3. evaluator 通过 virtual dispatch 执行完整默认计算链；
4. 当前终端 identity 的 Own Token override 在默认计算后应用；
5. descriptor 把扁平值投影到当前终端的资源键。

`ControlTokenDescriptor`、`ThemeCompiler`、`ThemeSchemaRegistry`、`ThemeSnapshot`、`ThemeTokenResourceProvider`、
`ControlTokenAccessor` 和 `TokenResourceBinder` 不需要理解 Token 类型继承。未注册 exact Control type 继续明确失败，不能从
Control CLR 基类、Token CLR 基类、`ControlTheme.TargetType` 或 `BasedOn` 推断 identity。

Desktop 与 Mobile 若以相同 Catalog 注册同名 Control，可能产生 identity 冲突。这是产品包组合和 Catalog ownership 问题，
不由 Token 继承解决；需要两个产品包同时注册时必须单独定义 identity 命名空间策略。

## Generator 与诊断

Generator 将标记类型分为两类：

```text
[ControlDesignToken] abstract        → 验证定义层，不生成运行时产物
[ControlDesignToken] sealed concrete → 解析终端并生成完整产物
```

结构或属性模型无效时，不为对应终端生成部分 schema。Generator 可以报告同一阶段的独立错误，但必须抑制由无效模型产生的
级联诊断。跨程序集基类的 Attribute、抽象性、泛型 arity、属性、`[NotTokenDefinition]` 和方法签名变化必须使下游增量输出失效。

继承契约使用构建错误：

| ID | 契约 |
| --- | --- |
| `ATOMUIGEN013` | 继承链包含未标记或具体中间层，或者没有到达 `AbstractControlDesignToken` |
| `ATOMUIGEN020` | 具体终端 `[ControlDesignToken]` 未声明 `sealed` |
| `ATOMUIGEN021` | 抽象层或终端 Token 使用泛型 |
| `ATOMUIGEN022` | 继承属性同名、隐藏或形成重复 schema key |
| `ATOMUIGEN023` | 属性是 indexer、访问器不可访问、`init`-only/`required`，或使用 virtual/abstract/override 等无效形态 |
| `ATOMUIGEN024` | `CalculateTokenValues` base 调用缺失、重复、非首条或参数错误 |
| `ATOMUIGEN025` | 抽象 Token 名称不符合 `*Token` 约定 |
| `ATOMUIGEN026` | Control Token 声明为嵌套类型；位于泛型容器时仍使用 `ATOMUIGEN021` |

终端 Control 名称匹配继续由现有 `ATOMUIGEN012`、`ATOMUIGEN014` 和 `ATOMUIGEN015` 负责；扁平 Own Token 与 Global Token
同名继续由 `ATOMUIGEN019` 负责。metadata 类型没有源码 location 时，诊断消息必须包含完整 metadata type name。

## 包与平台 ownership

共享抽象 Token 必须放在最低且语义正确的共同上游：

- Desktop/Mobile 都使用的 Control 视觉语义放在 `AtomUI.Controls` 对应 Control 或 primitive 附近；
- Desktop-only 或 Mobile-only 语义留在各自产品包；
- Control 产品视觉语义不下沉到 `AtomUI.Core`；
- `AtomUI.Controls.Shared` 不因代码复用接管 Control Theme 语义。

只有至少两个真实终端使用、名称和含义一致、默认计算依赖一致、平台差异能由终端继续表达时才提取共享抽象层。

第三方包可以公开 `public abstract` Token 作为扩展契约。其属性名称、值类型、可访问性和默认计算语义都是公共兼容性
边界；普通 Package 仍按现有包级入口、逐控件片段和静态 descriptor 规则注册终端 Control。

抽象 Token 不拥有 注册片段、factory 或动态 root。终端 factory 对具体类型的静态引用
自然保留完整 CLR 基类链；独立抽象 Token 程序集是普通编译依赖，不通过 runtime scanning 加载。

## 兼容性、性能与 AOT

直接继承 `AbstractControlDesignToken` 的既有终端在增加 `sealed` 后，其 identity、Token 名称和值类型、slot、resource key、
factory、evaluator、AXAML 用法和 schema revision 必须保持不变。

把属性从终端移入抽象层时，只要终端类型、属性名称和值类型、stage、排序、resource key 和 identity 不变，终端 schema revision
必须保持一致。schema fingerprint 不得包含声明层类型、程序集或继承深度。

所有具体 `[ControlDesignToken]` 最终都必须 `sealed`。封闭既有 internal 类型是结构迁移；封闭 public 具体 Token 会阻止第三方
继续继承，属于 Public API breaking change，必须在对应主版本的 breaking API 和 release 文档中说明。公开扩展点应迁移为
专门的 `public abstract` 定义层，不保留非 sealed 终端兼容壳。当前 public 具体类型 `ColorPickerToken` 属于这一迁移边界，
不能作为非 sealed 例外保留。

单个终端的生成复杂度目标为 `O(inheritance depth + flattened property count)`。抽象层不增加 runtime descriptor、snapshot 层、
资源字典、主题订阅或资源查询分支。生成代码继续使用直接 `new TerminalToken()` 和静态强类型 delegate，禁止使用
`PropertyInfo`、`GetProperties`、`BaseType` runtime 遍历、`Activator.CreateInstance`、`MakeGenericType`、runtime Attribute
读取或 Token 层类型数组。

## 验证要求

- 验证直接 sealed 终端保持既有生成输出；
- 验证抽象 Token 可独立编译且不生成 identity 或注册产物；
- 验证同程序集、跨程序集、NuGet metadata reference、多层和 sibling 终端；
- 验证非 sealed、泛型、未标记或具体中间层、错误根类型、属性冲突和非法属性形态均构建失败；
- 验证 base 调用正确、缺失、重复、条件性、非首条、错误参数和无 override 场景；
- 验证 Light/Dark 参数和默认计算顺序贯穿完整继承链；
- 验证 sibling 终端的配置、snapshot、资源和值完全隔离；
- 对比属性移动前后的 Token 名称、类型、slot、resource key 和 schema revision；
- 验证生成源码不包含反射或动态构造，并执行真实 trimmed build、NativeAOT publish 和启动 smoke。

Control 作者的声明规则见 [AtomUI Control Token 设计规范](../../../engineering/development/control-token-guidelines.md)，
Generator 模块职责见 [AtomUI.Generator 模块概览](../../../modules/generator/overview.md)，第三方复用入口见
[第三方 AtomUI Control Package 指南](../../../guides/theming/third-party-control-packages.md)。
