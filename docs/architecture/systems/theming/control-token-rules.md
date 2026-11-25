# 控件 Token 取值规则设计

控件 Token 取值规则定义一个控件在某种设计语言下如何取值。取值包提供规则，主题编译器在控件求值阶段调用规则，
填充控件自身 Token，并应用该控件的局部 Global Token 调整。语言包、控件包与取值包的职责见[设计语言架构](design-languages.md)。

## 规则契约

规则实现 `IControlTokenRule<TLanguageToken, TToken>`，以 `[ControlTokenRule]` 标记。`TLanguageToken` 为 `LanguageDesignToken`
或其派生类型，`TToken` 为绑定到控件的终端 `AbstractControlDesignToken` 派生类型。规则类必须在取值包内可访问，
须为具有公开无参构造函数的非抽象、非泛型类，且只实现一种 `IControlTokenRule<,>`。
规则类和计算器的命名及公开类型的迁移见[设计语言取值包命名实施标准](../../../engineering/development/value-package-naming.md)。

多个终端规则共用的计算由对应控件家族或分层职责的计算器提供。
终端规则在 `Apply` 中调用 `Calculate`，传入求值上下文、控件可见的全局 Token 和写入目标。
`AutoCompleteTokenCalculator.Calculate` 为 AutoComplete 家族计算候选项和弹层的共享 Token。

接口、标记、上下文与写入目标位于 `AtomUI.Theme.Languages`。`IControlTokenRule.Apply` 返回 `void`，将取值写入主题编译器提供的目标。

- `TokenEvaluationContext<TLanguageToken>` 通过 `Variant` 和 `Language` 提供当前变体与语言 Token。
  全局求值入口 `DesignLanguage<TLanguageToken>.DeriveGlobal` 也使用该上下文。
- `ControlTokenTarget<TToken>.Own` 承载控件自身 Token，规则为终端 Token 的全部属性赋值，包括从抽象层继承的属性。
- `ControlTokenTarget<TToken>.Global` 承载该控件可见的 Global Token。规则可读取这些值，其写入仅限 Map 和 Alias，
  且仅在该控件的取值范围内生效。写入 Seed 会触发求值错误。

编译器在调用 `Apply` 前写入并冻结显式值。读取冻结项得到显式值。规则对已冻结的可写项赋值时，该次赋值会被忽略。
上下文与目标仅在本次求值期间使用，规则在相同输入下产生相同取值，并支持并发调用。
求值顺序与显式值优先级见[单遍管线与冻结语义](design-languages.md#9-单遍管线与冻结语义)。

```csharp
[ControlTokenRule]
internal sealed class AvatarGroupTokenRule
    : IControlTokenRule<AntDesignToken, AvatarGroupToken>
{
    public void Apply(
        in TokenEvaluationContext<AntDesignToken> context,
        ControlTokenTarget<AvatarGroupToken> target)
    {
        target.Own.GroupSpace = target.Global.UniformlyMarginXXS;
        target.Own.GroupOverlapping = target.Global.UniformlyMarginXS;
    }
}
```

## 生成注册与查找

1. 源码生成器依据 `[ControlTokenRule]` 与接口绑定生成 `ControlTokenRuleDescriptor`，通过
   `GeneratedControlTokenRuleRegistration.Register(builder)` 调用 `IThemeManagerBuilder.AddControlTokenRules` 登记。
   注册代码使用静态可达的强类型引用，支持裁剪和 NativeAOT。
2. 注册表以语言 Token 类型和 `ControlTokenIdentity` 为键，每个键只接受一条规则。编译器沿当前语言 Token 类型的继承链
   查找，选用最具体的匹配。登记在 `LanguageDesignToken` 下的是语言无关规则。
3. 应用选择的是 `ControlValuePackage`。一个控件包与语言组合有多个取值包候选时，可通过
   `UseValues(GeneratedControlTokenRuleRegistration.Package)` 选择对应的包。若同一程序集生成多个取值包入口，
   则传入其对应的 `PackageN` 成员。

## 兼容性与验证

取值包、语言包及显式调用生成注册入口的应用应按下表迁移。更新 AtomUI 依赖后，重新生成并编译这些程序集。

| 旧 API | 新 API |
| --- | --- |
| `[ControlDerivation]`、`IControlDerivation<TLanguageToken, TToken>` | `[ControlTokenRule]`、`IControlTokenRule<TLanguageToken, TToken>` |
| `Derive(in DerivationContext<TLanguageToken> context, ControlView<TToken> view)` | `Apply(in TokenEvaluationContext<TLanguageToken> context, ControlTokenTarget<TToken> target)` |
| `view.Token`、`view.Global` | `target.Own`、`target.Global` |
| 共享静态类 `<Language>Abstract<ControlLayer>Derivation.DeriveShared` | 共享计算器的 `Calculate` 方法，类型名按命名实施标准确定 |
| `DeriveGlobal` 的 `DerivationContext<TLanguageToken>` 参数 | `TokenEvaluationContext<TLanguageToken>` |
| `ControlDerivationDescriptor`、`IThemeManagerBuilder.AddControlDerivations` | `ControlTokenRuleDescriptor`、`IThemeManagerBuilder.AddControlTokenRules` |
| `GeneratedControlDerivationRegistration.Register`、`GeneratedControlDerivationRegistration.Package` | `GeneratedControlTokenRuleRegistration.Register`、`GeneratedControlTokenRuleRegistration.Package` |

生成器应依据新接口定位 `Apply`，并检查 Own Token 的完整赋值、赋值前读取与取值覆盖。
验证覆盖显式注册与应用启动注册，以及运行时的语言继承查找、控件局部 Global 写入和显式值冻结。
验证入口见[设计语言架构](design-languages.md#12-注册启动与校验)与[生成器模块](../../../modules/generator/overview.md)。
