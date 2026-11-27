# 多语言模块迁移方案

本文记录从旧 `ThemeManager + LanguageProvider + C# 翻译类` 架构到独立应用级本地化基础设施的历史映射、
兼容结果和删除边界。迁移已经完成；本文不是当前实现入口，当前契约以同目录其他文档为准。

## 旧实现问题

旧语言系统具有以下结构性限制：

- `ThemeManager` 同时实现主题和 `ILanguageManager`，语言资源被当作主题静态资源的一部分挂载。
- `LanguageCode` 和 `LanguageVariant` 只能表达 `en-US`、`zh-CN`、`zh-TW`。
- 每个语言由一份 C# 常量类表示，翻译不使用标准交换格式。
- `LanguageProviderAttribute`、Provider 构造和资源字段曾依赖运行时 Attribute/字段反射。
- `LanguageProviderPool` 和 `IThemeManagerBuilder.AddLanguageProvider()` 把本地化注册绑在主题/控件包注册上。
- 应用 ViewModel 和服务缺少独立、强类型、DI 中立的 `ILocalizer`。
- 第三方新增语言需要扩展固定 enum/API，无法形成静态 NuGet 翻译协议。

## 迁移后的对应关系

| 旧类型或机制 | 已落地替代 |
|---|---|
| `LanguageCode` | `LanguageTag` + `LanguageTags` |
| `LanguageVariant` | `LanguageState.CurrentLanguage` / `LanguageDefinition` |
| `LanguageProvider` / `ILanguageProvider` | 生成式 `TranslationBundleDescriptor` |
| `LanguageProviderAttribute` | `[LanguageCatalog]` enum + XLIFF |
| 每语言 C# 常量类 | `en-US.xlf`、`zh-CN.xlf`、`zh-TW.xlf` |
| `LanguageProviderPool` | `GeneratedLanguageModuleRegistration` |
| ThemeManager language dictionaries | `LanguageCatalogRegistry` + `LanguageSnapshot` |
| ThemeManager merged dictionary switching | 稳定 `LanguageResourceProvider` Snapshot 交换 |
| `WithDefaultCultureInfo()` | `UseLanguages()` + `LanguageTag.FromCultureInfo()` 边界转换 |
| `IThemeManagerBuilder` 根入口 | `IAtomUIBuilder` |

## 必须保留的兼容面

现有应用和组件 XAML 形态必须保留：

```xml
<TextBlock Text="{atom:DatePickerLangResource Today}" />
<TextBlock Text="{atom:CommonLangResource Ok}" />
```

迁移后仍生成同名：

```text
DatePickerLangResourceKind
DatePickerLangResourceExtension
CommonLangResourceKind
CommonLangResourceExtension
```

AtomUI 公共 Catalog 的 enum namespace、类型名和成员名应尽量保持不变，使 AXAML namespace 与资源调用无需批量
改写。底层基类、descriptor、slot 和资源 Provider 可以完全替换。

## 明确不保留的 API

重构中已经删除：

```text
LanguageCode
LanguageVariant
LanguageVariantTypeConverter
LanguageProvider
ILanguageProvider
LanguageProviderAttribute
LanguageProviderPool
IThemeManagerBuilder.AddLanguageProvider()
IThemeManagerBuilder.WithDefaultLanguageVariant()
```

不创建逐语言兼容扩展：

```csharp
UseAtomUIZhCN();
UseAtomUIZhHans();
UseAtomUIJaJP();
```

也不保留 `UseAtomUILanguages()`；统一入口是根 `IAtomUIBuilder` 上的 `UseLanguages()`。

## Catalog 迁移

每组现有 Provider 按 `LanguageId` 转成一个源代码 Catalog enum：

```text
DatePicker en_US/zh_CN/zh_TW
  -> DatePickerLangResourceKind.cs
  -> en-US.xlf
  -> zh-CN.xlf
  -> zh-TW.xlf
```

第一阶段迁移工具或脚本只负责一次性抽取现有常量、生成候选 XLIFF 和显式数字 ID；结果经过人工审查后进入
正常源码。生成文件不再依赖旧 C# Provider，也没有在运行时提供双轨资源。

ID 分配规则：

- 以迁移时确定的 Catalog 成员清单分配从 1 开始的稳定 ID。
- 分配后立即写入 enum 和 XLIFF `unit id`，不再依赖字母排序或声明顺序。
- 迁移清单记录旧成员名到新 ID，防止分批迁移时不同语言产生不同编号。
- 已发布的 enum 底层数值如果被外部持久化，应在对应 Catalog 迁移前单独评估；不能假设现有隐式值是契约。

## Key identity 收敛

显式数字 ID 同时出现在 enum 和每个 XLIFF 中，要求开发者跨文件维护同一身份，容易产生错配。第二阶段将
Catalog identity 收敛为 enum 成员名：

- enum 删除显式数字值，成员名成为唯一稳定 Key。
- XLIFF `unit id` 直接使用 Key，规范输出删除重复的 `unit name`。
- Generator、Build Tasks、template merge、override 冲突检测、source fingerprint、manifest 和运行时 descriptor
  全部按 Key 工作。
- `LanguageCatalogUnitDescriptor.Id` 和 `Name` 收敛为单一 `Key` 属性，不保留数字兼容入口。
- 运行时继续使用生成式 enum member switch 和 dense slot 数组，不增加反射、enum 名称查询或字符串热路径。

该收敛改变了已发布 Catalog 和 descriptor 的身份契约，因此已有语言包必须从新的权威 `en-US.xlf` 重新导出和打包。
迁移后的所有 AtomUI 包、Gallery 和应用必须整体重新编译，不得混用数字 ID 版本与 Key 版本的程序集或静态语言包。

移除显式值后，enum 底层值从声明顺序自动产生。它不是本地化 identity，但仍会以内联常量形式进入消费程序集，
因此迁移后只能在末尾追加新成员，不能重排已有成员，也不得把底层值持久化或作为跨进程协议。公共 Catalog 的
成员序列需要由契约基线测试保护。

## 最终迁移结果

- `AtomUI.Localization`、`AtomUI.Build.Tasks`、Localization Generator、根 `IAtomUIBuilder` 和静态语言包模板已经落地。
- AtomUI、GalleryBase 与 Gallery Catalog 使用 XLIFF 2.1；Catalog identity 的最终形态是 enum 成员 Key。
- Desktop、DataGrid、ColorPicker、Extras 等包分别注册自身 `GeneratedLanguageModuleRegistration`。
- Gallery 与测试应用使用 `UseLanguages()`、`ILanguageManager` 和 `ILocalizer`。
- 旧 Provider 类型、旧 Generator Walker/Writer、ThemeManager 语言状态和逐语言扩展已从源码删除。
- 模块主包与静态语言包的真实 NuGet 消费由 `AtomUI.Localization.IntegrationTests` 保护。

## 禁止恢复双轨

单个 Catalog 在任何可运行应用中只能由新系统提供。禁止：

- 同时把旧 ResourceDictionary 和新 Provider 挂到 Application。
- 让新 `ILocalizer` 回退查询旧 ThemeManager。
- 用 adapter 把 `LanguageProvider` 包装成 Translation Bundle 作为长期兼容层。
- 在新 Snapshot 缺值时搜索旧 Provider。

这些做法会制造两个语言状态源、重复资源通知和无法删除的过渡架构。

## 完成条件

迁移只有同时满足以下条件才算完成：

1. 应用、AtomUI 控件和第三方示例都可定义并使用自己的 Catalog。
2. 所有官方 Catalog 使用 XLIFF 2.1，三种内置语言覆盖完整。
3. 现有 `{atom:XxxLangResource Key}` XAML 用法保持工作。
4. `ThemeManager` 不再拥有任何语言状态或 Provider。
5. 仓库不存在运行时 XLIFF 解析、语言程序集扫描或旧 LanguageProvider 注册。
6. 一个静态 I18n NuGet 可以通过 PackageReference 为新语言提供完整翻译。
7. 普通测试、Generator/Task 测试、trim/AOT analyzer 和真实 NativeAOT publish 全部通过。
