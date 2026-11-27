# Build 基础设施扁平化设计

> 历史记录：本文涉及的旧控件注册/裁剪方案已被 [TypeMap 正式架构](../../architecture/foundations/aot-and-trimming.md)替代，不再作为维护约束。其余内容保留当时的设计上下文。


> 本文记录 2026-08-14 的 Build 基础设施重组决策。长期事实同步到
> [`docs/architecture/foundations/build-and-packaging.md`](../../architecture/foundations/build-and-packaging.md)；
> 本文保留方案取舍、迁移范围和验收依据。

## 背景

AtomUI 的 `build/` 同时承载仓库级默认值、NuGet build assets、Generator/Localization/Theme/Linked Registration
管线和 macOS NativeAOT 本机链接适配。最初方案进一步按 `repository/`、`nuget/`、`platforms/` 以及功能 owner
拆分目录，并把 Localization targets 拆成四个 fragment。

实际评审后确认，这种目录设计让十几个短小 MSBuild 文件承担了过多的分类层级和转发 Import。交付边界本应由明确的
打包清单表达，不需要通过目录结构重复编码。最终方案改为扁平 `build/`、显式 NuGet 白名单和适度合并。

## 设计决策

1. `build/` 只保存一层 `.props` 和 `.targets` 文件，不建立交付边界子目录。
2. 文件名以 `AtomUI.<Feature>` 或明确的平台用途表达所有权。
3. NuGet build assets 使用显式 item 清单；文件位于 `build/` 不代表自动进入包。
4. Repository 聚合入口允许承载紧密相关的默认值和打包编排，不为几十行逻辑增加转发文件。
5. Localization 保留一个 props 和一个 targets；targets 集中拥有输入、项目引用、导出和打包流程。
6. macOS 文件只表达 Homebrew 原生库搜索路径，不扩张为跨平台 AOT 抽象。
7. 可执行验证脚本继续位于 `scripts/verification/`，不放回 `build/`。

## 目标

- 让开发者能在一个目录中看到完整 Build 资产面。
- 通过稳定文件名而不是深层路径表达职责。
- 通过显式白名单防止 Repository 或平台文件误入 NuGet。
- 保持 Generator、Localization、Theme、Linked Registration 和 AOT/Trim 行为不变。
- 删除只用于转发、聚合或单属性声明的文件。
- 用结构测试、打包检查和真实发布验证迁移正确性。

## 非目标

- 不保留旧路径或转发 Import。
- 不改变公开 API、生成代码协议或控件运行时行为。
- 不把仓库本机工具链适配发布给 NuGet consumer。
- 不为 Windows 和 Linux 创建没有实际配置内容的对称 targets。
- 不借目录迁移重构 Generator 或 Build Tasks 的业务实现。

## 最终目录

```text
build/
├── AtomUI.Generator.props
├── AtomUI.Generator.targets
├── AtomUI.GeneratorConsumer.targets
├── AtomUI.LinkedRegistration.props
├── AtomUI.LinkedRegistration.targets
├── AtomUI.Localization.props
├── AtomUI.Localization.targets
├── AtomUI.Repository.props
├── AtomUI.Repository.targets
├── AtomUI.ThemeAssets.targets
├── MacOSHomebrewNativeAot.targets
├── OutputPaths.props
├── PackageMetadata.props
├── ProjectDefaults.props
└── Versions.props

scripts/
└── verification/
    └── verify-aot-trim-registration.sh
```

`build/` 不允许子目录。新增文件必须说明为什么现有 owner 文件不能容纳该职责，并增加结构白名单与打包边界测试。

## 导入链

顶层只保留两个稳定入口：

```text
Directory.Build.props
└── build/AtomUI.Repository.props
    ├── Versions.props
    ├── ProjectDefaults.props
    ├── PackageMetadata.props
    ├── OutputPaths.props
    └── AtomUI.Generator.props
        ├── AtomUI.LinkedRegistration.props
        └── AtomUI.Localization.props

Directory.Build.targets
└── build/AtomUI.Repository.targets
    └── AtomUI.Generator.targets
        ├── AtomUI.LinkedRegistration.targets
        ├── AtomUI.Localization.targets
        └── AtomUI.ThemeAssets.targets
```

`AtomUI.Repository.props` 在导入 Generator props 前设置源码构建使用的 `$(AtomUIBuildTasksAssembly)` 和第一方
Catalog 统一使用的语言契约默认版本，避免每个项目重复声明。Repository targets 同时拥有第一方库 AOT/Trim 默认值、
`.DotSettings` 与 compiler-generated 源码快照排除，以及产品包 build asset 注入，不再为这些短逻辑保留独立聚合文件。

## 文件职责

| 文件 | 职责 |
|---|---|
| `Versions.props` | `AtomUIVersion` 和 `AvaloniaVersion` |
| `ProjectDefaults.props` | TFM、Configuration、测试项目识别、警告、项目本地旧 `obj/**` 排除和 Avalonia 默认值 |
| `PackageMetadata.props` | NuGet 公共元数据及仓库资产路径 |
| `OutputPaths.props` | NuGet、编译输出和集中式中间产物路径 |
| `AtomUI.Repository.props` | Repository props 聚合、Build Tasks 源码路径、第一方语言契约默认值、NuGet/tool 显式资产清单 |
| `AtomUI.Repository.targets` | AOT/Trim 默认值、生成源码快照排除、产品包资产注入、Generator targets 聚合 |
| `AtomUI.Generator.props/targets` | NuGet 自动导入入口和 feature 编排 |
| `AtomUI.GeneratorConsumer.targets` | 注册型产品包 consumer target 模板 |
| `AtomUI.LinkedRegistration.*` | linked publish 注册构建管线 |
| `AtomUI.Localization.*` | 本地化输入、项目引用、导出和打包管线 |
| `AtomUI.ThemeAssets.targets` | Theme Asset wrapper 构建管线 |
| `MacOSHomebrewNativeAot.targets` | macOS Homebrew OpenSSL/Brotli linker 搜索路径 |

集中式 `BaseIntermediateOutputPath` 不会自动让 SDK 排除项目目录中历史遗留的 `obj/**/*.cs`。因此
`ProjectDefaults.props` 必须把 `$(MSBuildProjectDirectory)/obj/**` 追加到 `DefaultItemExcludes`，避免旧生成的
AssemblyInfo 再次作为源码参与编译。

## NuGet 资产映射

`AtomUI.Repository.props` 明确列出唯一允许进入 `buildTransitive/` 的共享资产：

```text
AtomUI.Generator.props
AtomUI.Generator.targets
AtomUI.LinkedRegistration.props
AtomUI.LinkedRegistration.targets
AtomUI.Localization.props
AtomUI.Localization.targets
AtomUI.ThemeAssets.targets
```

这些文件统一映射到 `buildTransitive/%(Filename)%(Extension)`。禁止使用 `build/**/*.props` 或
`build/**/*.targets` 通配符，因为那会把 Repository 配置和平台适配错误交付给 consumer。

`AtomUI.GeneratorConsumer.targets` 不进入共享清单。注册型产品包将它单独重命名为
`buildTransitive/<PackageId>.targets`，满足 NuGet 自动导入规则。Generator、Build Tasks 及任务依赖通过
`@(AtomUIGeneratorToolAsset)` 进入 `tools/netstandard2.0/`，不得进入 `lib/`、应用输出或 publish 目录。

## Build Tasks 路径

所有 feature target 只使用：

```text
AtomUIBuildTasksAssembly
```

Repository 构建在 `AtomUI.Repository.props` 中把它指向集中输出目录。NuGet consumer 由
`AtomUI.Generator.props` 在属性为空时回退到包内 `../tools/netstandard2.0/AtomUI.Build.Tasks.dll`。不保留
`BuildTasks.props`、Localization/Linked Registration 专用路径属性或兼容别名。

## Localization

`AtomUI.Localization.props` 只定义 module identity 和语言 item 默认 metadata。

`AtomUI.Localization.targets` 按原执行顺序集中保留以下职责：

1. XLIFF 排除、自动发现、AdditionalFiles 和 compiler-visible metadata。
2. 模块源契约与静态语言包项目引用的 provider/consumer target 协议。
3. `AtomUIExportLanguageTemplates` 命令与模板导出 Task。
4. 静态语言包准备、模块语言资产准备、manifest/props/contentFiles 打包。

合并只改变文件所有权，不改变 Target 名称、依赖顺序、执行阶段、Returns 或 Task 参数。

## macOS NativeAOT

`MacOSHomebrewNativeAot.targets` 只在 `PublishAot=true` 且当前 OS 为 macOS 时，根据文件存在性加入：

```text
/opt/homebrew/lib
/opt/homebrew/opt/openssl@3/lib
/usr/local/lib
/usr/local/opt/openssl@3/lib
```

产品工程中只有 Gallery Desktop 导入它；语言包 Consumer fixture 和 AOT/Trim 验证脚本复用它进行仓库验证。该文件
不进入 NuGet，不负责 AtomUI 的通用 AOT/Trim 注册，也不表示 Linux/Windows 缺少实现。Linux 和 Windows NativeAOT
由共享发布属性、.NET NativeAOT 工具链及各自平台 linker 环境完成，不需要内容为空的对应文件。

## 迁移策略

1. 将所有 MSBuild 资产移动到 `build/` 根目录并使用明确名称。
2. 将 Repository props/targets 改为唯一源码构建入口。
3. 用显式 `@(AtomUINuGetBuildAsset)` 替代目录通配符和独立 manifest 文件。
4. 把产品包资产注入合并到 Repository targets。
5. 把 Build Tasks fallback 合并到 Generator props。
6. 把 Localization fragments 合并回 `AtomUI.Localization.targets`。
7. 更新项目、fixture、脚本、测试和正式文档中的全部路径。
8. 删除旧目录和转发文件，不同时维护两套导入链。

## 正确性不变量

- `build/` 只包含批准的十五个 `.props`/`.targets` 文件且没有子目录。
- 所有 Import 解析到唯一文件，不存在循环或旧路径。
- `Directory.Build.props/targets` 只导入 Repository 入口。
- 只有显式白名单中的共享 build assets 进入 Generator 和注册型产品包。
- `MacOSHomebrewNativeAot.targets`、Repository 配置和验证脚本不进入 NuGet。
- 产品包只生成一个匹配自身 PackageId 的 consumer target。
- Generator 和 Build Tasks 不进入应用运行时依赖、输出或 publish 目录。
- Localization target 名称、时序和行为保持等价。
- 集中输出布局不会把项目目录中的旧 `obj/**/*.cs` 重新纳入 Compile。
- 设置 `CompilerGeneratedFilesOutputPath` 的项目由 Repository targets 统一排除生成源码，不在各 `.csproj` 重复规则。
- `tools/` 源码反向 ignore 规则之后重新排除 `bin/` 和 `obj/`，中间态构建产物不能进入版本控制候选。
- Localization build assets 不维护与稳定 Key/source fingerprint 重复的手工版本字段。

## 验证

```bash
dotnet test tests/AtomUI.Build.Tasks.Tests/AtomUI.Build.Tasks.Tests.csproj --framework net10.0 --no-restore
dotnet test tests/AtomUI.Generator.Tests/AtomUI.Generator.Tests.csproj --framework net10.0 --no-restore
dotnet test tests/AtomUI.Localization.IntegrationTests/AtomUI.Localization.IntegrationTests.csproj --framework net10.0 --no-restore
dotnet test tests/AtomUIGallery.Tests/AtomUIGallery.Tests.csproj --framework net10.0 --no-restore
dotnet pack src/AtomUI.Generator/AtomUI.Generator.csproj -c Release --no-restore
dotnet pack src/AtomUI.Controls/AtomUI.Controls.csproj -c Release --no-restore
scripts/verification/verify-aot-trim-registration.sh --quick
scripts/verification/verify-aot-trim-registration.sh --full
dotnet build AtomUI.slnx -c Release --no-restore
git diff --check
```

打包验证必须检查 `.nupkg` ZIP 条目，而不只看命令退出码。NativeAOT 变更必须运行真实 publish；Gallery 发布还需要
启动 smoke，确认主窗口完成 Theme、Localization 和 linked registration 初始化。
