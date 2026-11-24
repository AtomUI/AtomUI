# 设计语言取值包命名实施标准

设计语言归属由命名空间表达。取值规则和计算器的类型名表达控件、家族或分层职责，源文件名与主类型名一致。
`public` 与 `internal` 表达调用范围。两种可见性使用同一命名规则。

## 适用范围

适用于 `src/DesignLanguages/AntDesign`、`src/DesignLanguages/PrimeNG` 和 `src/DesignLanguages/Shadcn` 下的控件取值包。
每种语言覆盖公共控件、桌面控件、ColorPicker 和 DataGrid 四类取值包，共 12 个项目。

| 对象 | 命名 |
| --- | --- |
| 绑定到终端控件 Token 的取值规则 | `<Control>TokenRule`，例如 `DateViewerTokenRule`、`RangeDateViewerTokenRule` |
| 控件或家族使用的取值计算器 | `<ControlOrFamily>TokenCalculator`，例如 `InputTokenCalculator`、`SkeletonTokenCalculator` |
| 同名主类型的源文件 | `<类型名>.cs`，放在对应控件或家族目录 |

本项迁移以取值类型和对应文件为变更对象，沿用现有命名空间、程序集、访问级别和方法签名。语言包的 `AntDesignLanguage`、
`PrimeNGToken`、`UseShadcn()` 等语言身份与接入契约由[设计语言架构](../../architecture/systems/theming/design-languages.md)维护。

## 统一命名与重名处理

| 迁移前类型 | 迁移后类型 | 所属命名空间 |
| --- | --- | --- |
| `AntDesignDateViewerTokenRule` | `DateViewerTokenRule` | `AtomUI.Desktop.Controls.Themes.AntDesign` |
| `ShadcnInputTokenCalculator` | `InputTokenCalculator` | `AtomUI.Desktop.Controls.Themes.Shadcn` |
| `AntDesignSkeletonTokenCalculator` | `SkeletonTokenCalculator` | `AtomUI.Controls.Themes.AntDesign` |

- **同一命名空间**。去前缀前先核对完整类型名和目标文件路径。发生重名时，用控件、家族或分层职责区分类型。
  三种语言的 TreeView 各有一组已知冲突。返回展示参数的类型使用 `TreePresentationTokenCalculator`，写入
  `AbstractTreePresentationToken` 的类型使用 `TreePresentationLayerTokenCalculator`。两种类型按职责命名，各自现有的方法与调用顺序照常适用。
- **不同语言命名空间**。同一个控件可在三套取值包中使用相同短名。调用方同时引用不同语言的同名类型时，通过完整类型名或
  C# 类型别名指出所用语言，例如 `using AntDesignSkeletonCalculator = AtomUI.Controls.Themes.AntDesign.SkeletonTokenCalculator;`。

## 实施步骤

1. **建立一次迁移清单**。按程序集记录每个规则和计算器的当前完整类型名、目标完整类型名、文件路径和可见性。
   检查同一命名空间的类型冲突、目录内的文件名冲突，以及跨程序集的引用多义性。规则清点以 `[ControlTokenRule]` 与
   `IControlTokenRule<TLanguageToken, TToken>` 为依据。
2. **按符号迁移类型与文件**。从类型名开头去除所属语言标识，同步更新声明、构造引用、`using`、类型别名、
   文档示例和测试里引用的完整名称。对跨包调用的计算器一并更新消费方，例如桌面 Skeleton 对公共取值包的调用。
3. **同步公开 API 迁移说明**。公开类型改名会影响源码引用和已编译程序集。按[变更日志规范](../contributing/changelog-guidelines.md#breaking-changes)
   记录旧名、新名和受影响的包，说明调用方需要修改引用并重新编译。注册代码随项目构建重新生成。

## 验收标准

- **命名**。对照清单核对三种语言下的类型、文件和引用。目标完整类型名在其可见范围内唯一，跨语言引用可以明确解析。
- **注册**。比对改名前后生成并进入注册表的「语言 Token 类型 + `ControlTokenIdentity`」集合，逐项核对其取值包来源。
  类型名可随迁移更新，每个键对应的终端 Token 类型、注册数量、语言继承查找及 `UseValues` 的包选择须一致。
- **求值**。对照改名前后的规则方法体与共享计算调用，核对赋值顺序和冻结语义。用现有主题契约测试验证各语言的取值，
  对跨包共享和有控件特有取值的代表场景执行定向验证。
- **构建**。构建 12 个取值包及受影响的消费方，运行生成器注册和主题契约相关的定向测试。
  发布专项遵循[按改动影响选择验证](../workflows/affected-verification.md#专项义务)。
