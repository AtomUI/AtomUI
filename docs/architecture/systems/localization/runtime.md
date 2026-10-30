# AtomUI 多语言运行时架构

本文定义本地化运行时的状态所有权、启动构建、查询、回退和语言切换语义。运行时位于
`AtomUI.Localization`，不依赖 ThemeManager 的内部事务或语言字段。

源码不使用 `Runtime/` 作为兜底目录，也不把 `Runtime` 作为内部类型的通用后缀。应用生命周期拥有的服务组合
为 `LocalizationHost`，当前语言的原子状态持有者为 `LanguageContext`，一次完整发布为
`LanguageRevision`。物理目录遵循概览文档定义的 `Catalog/`、`Resources/`、`Services/` 加根目录核心模型的
紧凑分组。

## 核心组件

```text
LanguageManager
├── LanguageCatalogRegistry
├── IReadOnlyDictionary<LanguageTag, LanguageSnapshot>
├── LanguageContext
├── LanguageResourceProvider
├── LanguageState
└── Localizer
```

| 组件 | 职责 | 不负责 |
|---|---|---|
| `LanguageCatalogRegistry` | 冻结 Catalog schema、slot、翻译来源和语言元数据 | 当前语言、资源通知 |
| `LanguageSnapshot` | 保存某一语言索引，按目录缓存完成回退后的资源值 | 解析 XLIFF、动态修改 |
| `LanguageContext` | 原子持有并发布当前 `LanguageRevision` | 构建 Snapshot、发布资源通知 |
| `LanguageRevision` | 把同一次提交的 Snapshot 和 State 绑定为不可分割值 | 维护支持语言集合 |
| `LanguageManager` | 验证并提交全局语言变化，发布状态和事件 | 主题事务、下载语言包 |
| `LanguageResourceProvider` | 把当前 Snapshot 投影为 Avalonia 动态资源 | 保存第二份翻译状态 |
| `Localizer` | 为 C# 调用提供强类型查询和格式化 | 字符串 Catalog 路径解析 |
| `LocalizationHost` | 持有 Manager、Localizer、Provider 并管理释放；Registry/Snapshots 委托当前 Manager | 执行查询或语言切换算法 |

## 启动构建

`UseAtomUI()` 按以下顺序构建本地化运行时：

1. 创建根 `IAtomUIBuilder`，并接入应用生成的 Language Module bootstrap。
2. 执行用户配置；`UseXxxControls()` 等扩展注册各自生成的 Language Module。
3. `ILocalizationBuilder.AddModuleData(moduleId, data)` 登记生成的纯数据模块；手工 Catalog/Bundle descriptor 注册仍受支持。
4. 规范化 `defaultLanguage`、`supportedLanguages` 与 `LanguageDefinition`；支持语言按首次出现顺序去重，显式
   `LanguageDefinition` 仍要求标签唯一，并验证 Culture 与方向元数据。
5. 构建并冻结 `LanguageCatalogRegistry`，解析来源优先级、Catalog 契约和 slot。
6. 仅为当前语言建立 `LanguageSnapshot` 索引，目录值行在首次查询时物化。生成数据的支持语言覆盖由应用链接器校验；手工目录或含手工 Bundle 的混合目录，其覆盖与格式仍在 Build 时校验。
7. 创建初始 `LanguageRevision` 并交给唯一 `LanguageContext`，再初始化 `LanguageManager` 和稳定
   `LanguageResourceProvider`。
8. 将 Provider 挂载到 Application 资源链，并注册 `ILanguageManager`、`ILocalizer` 的同一运行时实例。
9. 由 `LocalizationHost` 持有构建完成的本地化服务和生命周期，在完整初始资源可用后再让应用进入首帧，
   不在首帧后补做默认语言切换。

`Build()` 后冻结全部注册，不开放运行期动态追加 Catalog/Bundle 或下载语言包。

## Catalog registry

Registry 为每个 Catalog 分配在该 registry revision 内稳定的 slot，并记录：

- 生成式 Catalog ID、enum CLR 类型和稳定 Key 到 slot 的映射。
- 稳定 unit Key、enum member 到 Catalog 内部 slot 的生成式映射。
- 每个语言的 Translation Bundle、来源优先级和源文本指纹。
- `en-US` 完整源文本。
- 用于 Avalonia enum resource key 查询的生成式映射。

Registry 以已知 `Type` 的程序集简单名和元数据名定位数据 Catalog，首次命中后缓存 Type 到 slot。生成数据携带编译期 enum 数值到排序 slot 的映射；数值只是当前构建的数据分派索引，稳定 unit identity 仍是 Key。手工 descriptor 保留调用方的强类型 resolver。Registry 不得通过 `GetFields()`、`Enum.GetNames()`、
`Enum.GetName()`、`ToString()`、Attribute 反射或 `Activator.CreateInstance()` 发现 Catalog 或构造 Key。

## LanguageSnapshot

Snapshot 是某个请求语言的不可变查询视图；其目录行按需建立，一旦发布便不再修改：

```text
LanguageSnapshot
├── RequestedLanguage
├── Resolved values by catalog slot and unit slot
├── Resolved source language per value
└── Revision-independent immutable storage
```

每个目录第一次查询时按照来源优先级和 BCP 47 候选链解码该语言需要的值，并解析 CompositeFormat；完成的行在锁内原子发布。后续 `ILocalizer.Get()` 和 Avalonia 资源查询只执行 slot 定位与数组读取，不重复回退解析，也不合并 ResourceDictionary。

Snapshot 不暴露可变 Dictionary，不缓存调用方格式化结果。启动只创建当前语言的索引；首次切换创建目标语言索引，后续切换复用缓存。切换仍一次发布完整 LanguageRevision，资源通知先于 LanguageChanged，NoOp 不分配新快照或递增 revision。Manager 释放时清空快照缓存及事件订阅；既有 Localizer 读取视图的生命周期保持不变。

## LanguageState

```csharp
public sealed record LanguageState(
    LanguageTag CurrentLanguage,
    CultureInfo FormattingCulture,
    LanguageTextDirection TextDirection,
    long Revision);
```

- `CurrentLanguage` 是当前请求并已提交的文本语言。
- `FormattingCulture` 用于 `ILocalizer.Format()` 以及 AtomUI 控件的数字、日期和时间显示。
- `TextDirection` 是 LTR 或 RTL，供根视觉宿主投影 `FlowDirection`。
- `Revision` 每次成功切换递增，`NoOp` 不递增。

语言、格式化 Culture 和文字方向相关但不等价。应用可以为私有语言标签提供自定义 Culture 和方向。

## 回退算法

每个 Catalog、每个 unit 独立使用同一确定链：

```text
exact tag -> script/parent candidates -> neutral language -> en-US
```

示例：

```text
zh-Hant-HK -> zh-Hant -> zh -> en-US
zh-CN      -> zh-Hans -> zh -> en-US
zh-TW      -> zh-Hant -> zh -> en-US
fr-CA      -> fr -> en-US
```

候选链由 BCP 47 结构和固定 CLDR likely-subtags 数据生成，不用字符串 `Split('-')` 临时猜测。`zh-CN` 不把
`zh-TW` 作为候选，反向同理。只有显式提供的中性 `zh` Translation Bundle 才能被两个脚本族共同使用。

`en-US` 是最终安全值，但对于非英语支持语言，构建时直接落到 `en-US` 不计为翻译覆盖；这一规则防止应用
声明支持日语却悄悄显示整页英文。

## 语言切换事务

静态语言切换使用小型、独立的同步提交，不复用 ThemeTransaction：

1. 验证 UI 线程访问、Manager 未释放且目标属于支持语言；失败分别抛出文档规定的类型化异常。
2. 目标与当前语言相同则返回 `NoOp`。
3. 读取目标 `LanguageDefinition`；复用缓存中的目标 Snapshot，首次切换到该语言时先构建其索引。目录值行继续按首次查询物化。
4. 创建包含目标 Snapshot 与新 `LanguageState` 的 `LanguageRevision`，并在提交点由 `LanguageContext` 原子发布。
5. `LanguageResourceProvider` 发布一次 hosted resources changed 通知。
6. 根文字方向投影与 Snapshot 属于同一提交，不暴露文本已变而方向未变的状态。
7. 发布一次 `LanguageChanged`，事件参数包含旧/新 State 和结果 revision。

提交前失败不改变旧状态。提交后的资源通知可以触发目录首次解码与 `CompositeFormat` 物化；格式和覆盖合法性
由应用链接期校验或手工贡献的启动检查确定。事件订阅方抛出异常不得回滚已提交语言，也不得阻止其他订阅方按既定
事件策略收到通知。

## Stable LanguageResourceProvider

Provider 在 Application 初始化时只挂载一次。它始终从 `LanguageManager` 的当前 Snapshot 读取，不为每种语言
维护一份可变 Avalonia `ResourceDictionary`，切换时也不从 MergedDictionaries 删除或添加字典。

应用级物理挂载由 Core 的 ApplicationScope 负责，LocalizationHost 只拥有本地化服务组合。Provider 与主题资源的
共同宿主、应用覆盖和释放责任见[应用资源宿主设计](../theming/application-resource-host.md)；
该宿主迁移的实现证据与验收见[资源宿主验证](../theming/application-resource-host-verification.md)。
共同宿主不把语言状态交给 ThemeManager，也不让 ChangeLanguage 进入主题事务。

Generator 生成的 `XxxLangResourceExtension` 继续基于 enum key 创建动态资源引用。Provider 负责把 exact enum
类型和值映射到 Catalog/unit slot。语言通知只使已有动态资源重新查询，不替换 Markup Extension 或资源 key。

非 StyledElement 的 XAML 目标如果只能在 ProvideValue 时取得静态值，应通过 `ILocalizer` 读取当前 Snapshot；
这类对象不会假装获得自身不存在的 Avalonia 动态资源生命周期。

## 线程模型

- `ChangeLanguage()`、Provider 挂载和资源通知只在 UI 线程发生。
- Snapshot 与 State 使用不可变对象和原子引用发布。`ILocalizer.Get()` 可从后台线程读取；捕获完整 revision 无锁，首次目录物化受锁保护。
- 单次 `Get()` 或 `Format()` 必须只观察一个 Snapshot；切换不能让一次格式化混用两个语言 revision。
- 事件在 UI 线程发布。后台服务需要响应语言变化时自行调度耗时工作，不阻塞提交路径。

## Culture 策略

AtomUI 默认不修改以下全局状态：

```csharp
CultureInfo.CurrentCulture
CultureInfo.CurrentUICulture
CultureInfo.DefaultThreadCurrentCulture
CultureInfo.DefaultThreadCurrentUICulture
```

AtomUI 控件必须读取 `LanguageState.FormattingCulture`，不能依赖调用线程碰巧具有相同 Culture。应用确实需要
同步进程 Culture 时，应在应用层订阅已提交的 `LanguageChanged` 并采用显式策略；该行为不进入 Manager 默认语义。

## 生命周期

`LanguageManager` 与 Application 生命周期一致，由 LocalizationHost 释放其内部状态和公开事件。Core 的
ApplicationScope 负责从实际挂载目标移除语言 Provider 和根 FlowDirection Style，再释放本地化服务；不能要求
LanguageManager 自行管理应用主题资源集合。Snapshot 和生成表不持有 Visual、ViewModel、DI Scope 或翻译包文件句柄。
