# TypeMap 注册架构

本文定义 AtomUI 条件映射、生成代理、包引导和注册执行边界。采用状态与源码迁移状态由
[AOT 与裁剪架构](aot-and-trimming.md#1-状态与事实边界)统一说明。
类型与资源事实由 [Control 注册契约](control-registration-contracts.md)定义；隐藏 ABI 的精确格式见
[TypeMap ABI 契约](../../reference/aot/typemap-contract.md)。

## 1. 单一生成事实源

TypeMap 元数据和 accessor 仅在目标 Compilation 提供官方 TypeMap API 时生成。Release 的 .NET 8 产品目标
从同一套片段事实生成 [ConditionalRecord ABI](../../reference/aot/conditional-registration-contract.md)，由受控后端计算
裁剪选择，普通非裁剪运行仍完整收集。.NET 10 的条件映射、候选 key 与链接契约保持不变；消费 net8-only 包时，
发布前的隔离桥接将已验证的候选 ABI 转成官方 TypeMap 输入，实际选择仍由最终 ILLink/ILC 完成。

普通 `AtomUI.Generator` 从 Control、Token、Semantic Part 与包内 AXAML 产生：

- 逐 Control 的单项 descriptor factory 与注册片段。
- 独立主题资产、资源 factory 和必要的真实类型引用。
- 各保留条件对应的 TypeMap 声明。
- 纯字符串候选 key 表和包内具体 Group accessor。
- Package marker 与非裁剪全量调用入口。

生成器不分析应用方法体、消费 AXAML usage 或预编译 DLL 的调用图，不输出应用注册计划。
包内资源归属与字符串资源检查只依据普通主题契约，不把旧跨应用依赖推导移到新的命名下。

普通与裁剪执行复用相同的单项 factory。全量集合、全量 URI dispatch 和全控件静态数组只能由非裁剪分支访问；
不能与 map accessor、Group、候选 key 或共享 helper 放进会共同初始化的全局状态。

## 2. 逐条件映射

一个 Control 片段可以有多个独立保留条件，例如 Control、Token extension、Token enum 和专用 Semantic Style。
每个条件分配唯一 key，多个 key 可以指向同一代理：

```csharp
// 隐藏生成代码，key 格式由生成器定义。
[assembly: TypeMap<DesktopMap>(
    "Button/control", typeof(ButtonRegistration), typeof(Button))]
[assembly: TypeMap<DesktopMap>(
    "Button/token", typeof(ButtonRegistration), typeof(ButtonTokenResourceExtension))]
[assembly: TypeMap<DesktopMap>(
    "Button/style-content", typeof(ButtonRegistration), typeof(ButtonContentStyle))]
```

不得为同一个 key 声明不同 trimTarget 来模拟 OR；不同条件必须使用不同 key。生成器以具体 Group、Control 完整程序集
身份和条件类型完整身份计算版本化短 key，并在生成期检查 hash 碰撞。不能以简单类型名、源码顺序或目录替代完整输入身份。

所有类型、Token、主题和代码引用先交给官方链接器计算闭包。片段不递归调用其他片段；类型之间的保留循环由官方闭包收敛，
不需要 AtomUI 的运行时 visited graph 或应用 SCC planner。

有用内容为空的派生 Control 无需空片段。internal Control 可以只有资源片段；资源字典的不可分割边界不等于包或目录粒度。

## 3. 代理与激活

每个代理是带自身 Attribute 的 sealed 类型，继承隐藏 ABI `ControlRegistrationFragmentAttribute`。
其 `Add(ControlPackageRegistrationBuilder)` 只添加本 Control 的可选 Token/语义 descriptor 与资产记录。
接口以 [ABI 契约](../../reference/aot/typemap-contract.md)为准，不是用户手写扩展点。

运行时对映射已经返回的目标调用已知的 `GetCustomAttribute<ControlRegistrationFragmentAttribute>()`，然后执行 `Add`。
这允许有限的已知 Attribute 读取，不是零反射设计；禁止据此扩展为程序集扫描、动态方法查找或未知构造函数激活。

无 Attribute、代理类型非法、FragmentId 冲突或重复定义不一致时明确失败。代理与 builder 状态只在本次注册上下文存在，
不得建立持有应用 Provider/ThemeManager 的静态缓存。

条件 TypeMap 声明导致的预期 IL2026 仅在对应生成代码处局部处理并说明原因；不能全局关闭 IL2026、IL3050 或其他诊断。

## 4. 候选 key 与具体 accessor

TypeMap 不提供可用的枚举契约。生成器产生纯字符串候选 key 表，运行时逐项 `TryGetValue`，先按返回的 proxy Type 去重，
再读取代理 Attribute。不能使用 `Keys`、`Values`、`Count` 或 `GetEnumerator` 来推导所选条目。

候选表禁止包含 Control Type、代理实例、descriptor、资源实例或工厂 delegate。候选 key 数量决定查询次数，实际保留的
proxy 数量决定激活次数；不能为省去有限查询而引入全量类型根。
短 key 只减少生成字符串和元数据负担，不改变候选查询次数，也不作为资源优先级或运行时诊断文本。

每个包必须生成具体、闭合、非泛型 accessor：

```csharp
[GeneratedTypeMapAccessor(typeof(DesktopMap))]
internal static IReadOnlyDictionary<string, Type> GetMap()
    => TypeMapping.GetOrCreateExternalTypeMapping<DesktopMap>();
```

不能将调用移入 `ReadMap<TGroup>()` 等共享泛型 helper。共享 runtime 接收已经取得的 dictionary；具体 Group 查询保留在
包的生成方法体中，以满足官方 TypeMap intrinsic 的静态可识别要求。
Browser accessor 的构建期转换见 [浏览器链接架构](aot-browser-linking.md)。

## 5. Package marker 与应用引导

包生成隐藏的公共 Group 类型及 assembly marker：

```text
ControlPackageMarkerAttribute(packageId, mapGroup, abiVersion)
```

Group 公共可见仅为跨程序集生成代码引用，不允许用户通过继承或实例化扩展注册机制。Group 不得含有引用全包 Control、
descriptor 或工厂的静态状态。marker 表达包身份与 ABI，不表达应用控件清单。

应用 ordinary generator 只读取已解析程序集引用中的 marker，按完整程序集身份、Package ID、Group 和 ABI 校验并去重，
输出具体的 `TypeMapAssemblyTarget<PackageMap>`。完整类型身份必须包含定义程序集，不能只比较 FullName 或 scope 简称。

普通 adapter DLL 不需要重新以特殊 publish 属性构建，也不需要 usage companion。只要其所需的新格式控件包引用能够解析，
就按同一 marker 规则参与引导。marker 冲突、缺失所需引用或 ABI 不兼容时报告明确错误，不恢复旧消费者 IL 分析。

marker 和 assembly target 不是启用动作。没有调用可选包入口时，不查询该包 map、不创建其 Provider、不运行语言和 initializer。
应用引用一个 NuGet 并不代表要启用它。

## 6. 包入口与执行模式

普通无状态第三方入口只需正常注册 Provider：

```csharp
public static IAtomUIBuilder UseAcmeControls(this IAtomUIBuilder builder)
    => GeneratedControlPackageRegistration.Register(
        builder, static () => new AcmeControlThemesProvider());
```

有生命周期逻辑的入口向生成 helper 提供明确的 provider factory、prepare 和 complete 回调。
生成器不读取入口方法体猜调用顺序；作者不写 TypeMap、入口身份 Attribute、AOT 模式分支或依赖名单。
应用仍通过真实 `UseXxxControls()` 启用包。

一个官方 linker feature switch 决定全量片段调用与 map 查询。非裁剪执行直接添加全部片段；裁剪执行只添加 map 中的片段。
两条执行分支复用单项 factory 和同一个 `ControlPackageRegistrationBuilder`，不保留旧应用 PlanRegistry。
feature switch 必须在发布期消除不可达全量分支；仅运行时 `if` 后过滤全量数组不满足裁剪要求。

包入口只暂存记录与 factory，后续统一跨包提交、失败处理与冻结遵循
[注册生命周期](aot-and-trimming.md#6-注册生命周期)。不能为了简化 map 查询提前创建资源或冻结当前包依赖。

## 7. 平台分支

本节的平台声明来源已接入普通生成器，原项目路径配置已移除；实现与验证证据由
[状态与事实边界](aot-and-trimming.md#1-状态与事实边界)记录。平台可用域比较与片段 guard 复用同一模型。

### 7.1 声明归属

平台能力使用标准 `SupportedOSPlatformAttribute` / `UnsupportedOSPlatformAttribute`，声明放在实际承担限制的对象上：

| 限制属于 | 声明位置 | 生成行为 |
| --- | --- | --- |
| 控件本身 | Control 类型及其包含类型、程序集 | 从导出 TargetType 自动取得类型域，普通主题无需重复声明 |
| 某套主题或资源字典 | 该资产的正常资源 CLR 类型及其包含类型、程序集 | 仅收窄该资产；不反向修改 TargetType 的产品平台能力 |
| 包内全部类型和资源 | 程序集 | 作为本包类型和资产的平台上界，不按项目名或目录推断 |

ResourceDictionary 与 ControlTheme 的 `x:Class` 均绑定到本资产所属程序集的实际资源类型；已有强类型主题复用自己的 CLR 类型。
Styles、UserControl 等其他合法 AXAML 根不因位于 Themes 目录而被当成这两类注册资源。
没有资源 CLR 类型时，资源域来自资产所属程序集，程序集无约束时才是全平台。显式 `x:Class` 无法解析或身份不合法时
必须诊断，不能静默按无声明处理。资源身份使用定义程序集与完整 CLR 名称，不使用简单类名、文件名或目录作为平台依据。

普通跨平台控件无需平台配置；本来不支持 Browser 的控件，其主题自动随类型域排除。对于外部库拥有的目标类型，
或控件可跨平台但本包主题具有额外限制的情形，声明应放在本包的正常资源类上。需要新增资源类时，它必须是实际可加载的
ResourceDictionary/ControlTheme，保持原资源内容与构造语义；不引入标记专用假类型、新 AOT attribute 或路径清单。
这些声明不会自动发现任意方法体中的原生调用，作者仍须准确表达真实产品能力；不能为了裁剪而扩大控件的禁用范围。

### 7.2 可用域与生成分支

复用现有 `PlatformAvailability` 模型。类型域组合 type、containing type 与 assembly 的标准声明，不另建通用基类或
方法体能力推导。设资源自身域为 `R`，各导出 TargetType 的类型域为 `T_i`：

```text
A_i = R ∩ T_i
不可分割资产要求：所有 A_i 相等且非空
所需每个 Token owner 的类型域必须覆盖共同的 A_i
```

单目标资源无额外限制时自然继承目标域；多目标资源不允许静默取不同目标的交集，以免丢失仍可用的主题。
例如一个目标全平台、另一个不支持 Browser：资源无额外声明时必须拆分，或在产品确实只提供桌面版本时，
由资源类明确排除 Browser。显式收窄资源仍不得改变其他资产对同一目标的支持范围。
资产 guard 与默认 type-key 提供者的域覆盖校验消费同一份资源域。Semantic Theme 继续复用同一份 XML/类型/资源索引；
本次声明来源收敛不新增显式 include 的平台覆盖校验或独立语义主题的平台推导。

ControlContract 的 `PlatformAvailability` 是普通产品平台能力，生成器将其写进每个片段的真实平台分支。
先判断平台，再引用或构造该片段的 Token、Semantic、asset factory。即使代理因为静态类型证据可达，不可用平台分支内的
工厂仍必须能被链接器删除。

公共 builder 在构造完 descriptor 后才执行 predicate，不能替代片段内平台 guard。
共同域的 guard 在每条入口到共享 AddAsset 的路径上、descriptor 与 factory 引用之前执行。
资源类型只提供平台事实与真实资源构造，不成为新增的包级 TypeMap 条件或全包静态根；保留既有 theme-class 片段语义。

### 7.3 平台语法与边界

可用域按平台与版本半开区间规范化。
当前支持 browser、windows、linux、macos/osx、ios、maccatalyst、android、tvos、freebsd；browser/linux 不接受版本，
其他平台接受对应 OperatingSystem API 的版本分量（Windows 至四段，其余至三段）。子声明只收窄父域；
重启已排除区间、多区间重新启用、无法表示的 OS/版本以及空域报告 ATOMUIREG006。
IsIOS/IsIOSVersionAtLeast 包含 MacCatalyst，域比较与生成 guard 均保留这个重叠关系，不能把两者当成互斥谓词。
这些边界遵循 [.NET 平台兼容性分析说明](https://learn.microsoft.com/en-us/dotnet/standard/analyzers/platform-compat-analyzer)
与 [CA1416](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1416) 的可用性语义；
这里声明的是生成器支持的有界语法，并不承诺实现分析器的全部属性组合。

### 7.4 首批迁移边界

本次只收敛声明来源，不扩大 Browser 支持范围，也不拆分原生窗口包。原路径排除覆盖的资产按以下归属迁移：

| 对象 | 迁移要求 |
| --- | --- |
| Window、WindowTitleBar、SplitView、OtpLineEdit、OtpLineEditCell、TreeViewFlyoutPresenter | 复用已有类型平台声明，普通主题由 TargetType 自动取得限制 |
| WindowResizer、FullscreenPopoverLayer、CaptionButton、CaptionButtonGroup、WindowsCaptionButton 等原生窗口辅助类型 | 核对实际原生窗口依赖，在类型上补齐真实产品能力；不按同目录批量标注 |
| WindowTitleBarButton、WindowTitleBarToggleButton 的平台专属主题 | 复用现有主题 CLR 类声明资源限制；不据此收窄可复用控件本身 |
| Avalonia 的 AdornerLayer、WindowDrawnDecorations 对应的 AtomUI 主题 | 由本包正常资源类声明限制，不修改外部类型，也不要求外部库配合 |
| OtpTextBox、CaptionButtonFrame 及同目录其他资产 | 按实际控件能力与主题依赖逐项判断；纯资源限制归资源类，不继承目录限制 |

迁移必须先保存原有逐类型/资产的平台域与 Browser 工厂保留基线，再证明新声明得到相同有效域；不能以删除配置为由
静默放开或收窄行为。窗口模板、资源内容、默认主题键、AssetId/URI、覆盖顺序及应用级主题替换保持原契约。

## 8. 验证边界

普通生成器验证输出确定性、key 唯一性、完整类型身份、具体 Group 调用与片段局部平台分支。
发布验证还必须证明：

- Control/Token/Identity/Style 单独使用均能保留正确契约；相同代理多个条件只激活一次。
- 全量数组、全量资源 dispatch 和生成 helper 不把未用 Control 变成根。
- 多 Group、跨程序集及传递引用可用；未启用可选包没有生命周期副作用。
- 资源字典多目标闭包正确，新增无关同目录控件不扩大选择范围。
- 资源 CLR 类型/程序集声明、无资源类的程序集域、不可解析的 `x:Class`、多目标不一致及 Token owner 不覆盖有正反对照。
- 资源移动或重命名并更新正常引用后，平台域不变；同目录新增跨平台资产不会继承平台排除。原路径/AssetId 的正常变化不等于平台能力变化。
- 平台声明迁移前后桌面模板与默认主题替换等价；Browser interpreter/AOT 均删除不可用的 AtomUI 资产工厂。
  不要求 Avalonia 的同名外部目标类型完全消失，它们可能仍被其他合法浏览器路径使用。
- 原型中的已选 map 结果不能替代完整产品的 UI、NuGet、平台与体积矩阵。

产品验收与实现状态由 [AOT 与裁剪架构](aot-and-trimming.md#8-验证与交付门槛)统一拥有。
