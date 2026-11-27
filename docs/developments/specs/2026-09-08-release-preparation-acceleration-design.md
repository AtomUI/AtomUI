# AtomUI 发布准备加速设计

> 状态：设计已在对话中确认，等待书面规范复核后进入实施计划。

## 1. 结论

AtomUI 发布准备采用“修复增量失效、统一完整测试入口、减少重复 MSBuild 进程、保留全部发布门禁”的方案。
优化目标是降低墙钟时间和重复计算，不通过删测试、跳过 Browser smoke、弱化 AOT 验证、缓存跨提交发布产物或并行访问
共享构建输出换取速度。

发布流程继续以同一提交、同一版本和同一包清单为边界：完整测试、Browser Release 验证、适用的 AOT/Trim 与 Desktop
NativeAOT 验证、20 个 NuGet 包完整性校验全部成功后，才允许上传或推送包。

## 2. 已验证的基线与根因

### 2.1 GitHub Actions 基线

`AtomUI NuGet Packages Release` 最近成功运行的 Build + Pack 耗时为：

| Run | 提交时间范围 | Build + Pack |
| --- | --- | ---: |
| #23 | ShadowKey shadow-copy 改造前 | 7 分 08 秒 |
| #24 | 改造后的首次正式发布 | 21 分 55 秒 |
| #25 | 后续稳定复现 | 24 分 23 秒 |
| #26 | v6.1.8 | 24 分 34 秒 |

v6.1.8 整个 NuGet workflow 耗时 25 分 35 秒，其中 Build + Pack 占约 96%。Checkout、SDK setup、本地 feed、artifact
upload 和 nuget.org push 合计约 1 分钟，不是首要优化目标。

### 2.2 增量失效根因

`AtomUI.Build.Tasks.csproj` 的 `_AtomUIWriteBuildTasksShadowKey` 在 `CoreCompile` 后运行，并以 `Overwrite="true"` 写入
`AtomUI.BuildTasks.ShadowKey.props`。`CoreCompile` 即使被跳过，AfterTargets 仍可执行；当前 writer 因此在内容完全相同时
仍刷新 props 时间戳。

`AtomUI.Repository.props` 在评估期导入该 props。导入文件时间戳变化会进入后续项目的增量判断，而
`AtomUI.Generator` 又引用 `AtomUI.Build.Tasks`，大多数产品包把 Generator 作为 analyzer 引用。发布脚本逐个构建 20 个包
时，同一依赖图因 ShadowKey props 被反复改写而持续重新编译。

本地最小 A/B 已验证：

- 未修改时，连续无源码变化构建仍更新 ShadowKey props、Build Tasks DLL 或 Generator DLL，并耗时约 2.22 秒。
- 文件内容 SHA256 不变，只有时间戳变化。
- 只给 `WriteLinesToFile` 增加 `WriteOnlyWhenDifferent="true"` 后，第二次构建耗时约 0.75 秒。
- 第二次构建后 ShadowKey props、Build Tasks DLL 和 Generator DLL 的时间戳全部保持不变。
- 禁止 Build Tasks 导入自身 ShadowKey 不是修复该循环的必要条件，不纳入最小实现。

## 3. 目标

1. 消除 ShadowKey props 的无内容变化重写，恢复 MSBuild 增量构建。
2. 将 NuGet Build + Pack 从 24 分钟级恢复到合理的 8–12 分钟目标区间，并以 GitHub Actions 干跑实测为准。
3. 让 `dotnet test AtomUI.slnx` 成为真正完整的回归入口，覆盖仓库全部 `*.Tests.csproj` 测试项目。
4. 减少发布脚本的独立 `dotnet`/MSBuild 进程数量，同时保留构建前置顺序、串行共享输出和 fail-fast 行为。
5. 生成稳定、可比较的阶段耗时与验收摘要，使后续发布无需人工拼接命令和结果格式。
6. 保持 20 个正式 Package ID、版本、包内容、linked-registration Sidecar 和 Generator/Build Tasks 工具资产一致。

## 4. 非目标

- 不删除、筛选或降低现有完整回归测试覆盖率。
- 不把 Debug Browser build 当作每次正式发布的固定门禁；Browser 构建链发生变化时仍补跑 Debug，正式 smoke 始终使用
  Release。
- 不取消适用范围内的 full AOT/Trim matrix 或 Desktop Gallery NativeAOT 验证。
- 不并行执行会写入同一 `.artifacts`、Build Tasks toolset 或 NuGet 输出目录的任务。
- 不在不同提交、不同版本或不同 workflow run 之间复用待发布 `.nupkg`。
- 不通过跳过 restore、build 或 package completeness 检查来掩盖缺失产物。
- 不先引入 GitHub Actions 二进制缓存；缓存收益必须在增量失效修复后单独测量。
- 不改变任何 AtomUI public API、运行时行为、控制主题、语言资源或包依赖契约。

## 5. 不可破坏的发布质量契约

### 5.1 提交与版本

- `build/Versions.props` 的 `AtomUIVersion` 仍是唯一版本源。
- 发布构建绑定一个解析后的 commit SHA；标签、workflow `TargetRef` 和验收摘要必须指向该 SHA。
- 发布准备不得修改或发布来自其他提交的构建输出。

### 5.2 测试

- 完整回归必须覆盖 `tests/**/*.Tests.csproj` 中所有符合仓库测试命名约定的项目。
- `AtomUI.slnx` 是完整回归的唯一维护入口；新增测试项目未加入 solution 时，架构测试必须失败。
- 发布过程只允许通过构建一次后 `--no-build --no-restore` 复用相同输出，不允许跳过测试本身。
- 测试结果目录继续遵守仓库自动清理与无 dump 持久化契约。

### 5.3 Browser、AOT 与 NativeAOT

- Browser 平台正式验收使用 Release 配置，并验证服务启动、Gallery 首屏渲染、至少一次导航交互和浏览器 console 无错误。
- Browser build infrastructure 或配置发生变化时，同时补跑 Debug 构建。
- AOT、Trim、source generator、Window、主题或模板相关变更继续执行 full AOT/Trim matrix。
- 影响 AOT、Window、主题、模板或 source generator 的发布继续执行真实 Desktop Gallery NativeAOT publish。
- 这些门禁只能通过明确的发布影响分类决定是否适用，不能因耗时自动跳过。

### 5.4 NuGet 包

- `scripts/NuGetPackageProjects.ps1` 继续是正式项目、Package ID、分组和前置项目的唯一清单。
- Build Tasks 和 linked-publish Generator 必须先于产品包构建。
- package build 保留 `--disable-build-servers`、`-m:1` 和 `/nr:false` 的共享输出安全约束。
- pack 前所有产品包必须完成同版本 Release build；pack 使用 `--no-build`，但允许现有 linked-registration Sidecar
  target 对当前包执行必要的内部构建。
- pack 后继续拒绝缺包、多包、错误版本和空输出。
- 优化前后包内容比较忽略 ZIP 容器时间戳，只比较 entry 路径集合与逐 entry 内容哈希。

## 6. 设计

### 6.1 阶段 A：修复 ShadowKey 增量失效

在 `src/AtomUI.Build.Tasks/AtomUI.Build.Tasks.csproj` 的 `WriteLinesToFile` 上增加：

```xml
WriteOnlyWhenDifferent="true"
```

保持 `Overwrite="true"`，使 toolset 内容真实变化时仍原子更新完整 props 内容。ShadowKey 的 SHA256 算法、输出位置、
shadow directory 命名和 staging 行为不变。

回归保护分两层：

1. 架构测试解析项目 XML，要求 `_AtomUIWriteBuildTasksShadowKey` 的 writer 同时包含
   `Overwrite="true"` 和 `WriteOnlyWhenDifferent="true"`。
2. 增量集成验证连续构建 Generator 两次，要求第二次不改变 ShadowKey props、Build Tasks DLL 和 Generator DLL 的
   last-write time，并记录两次耗时。集成验证使用串行、禁用 build server 的现有发布参数。

### 6.2 阶段 B：完整测试入口

将当前遗漏的测试项目加入 `AtomUI.slnx`：

- `AtomUI.Controls.Shared.Tests`
- `AtomUI.Desktop.Controls.Tests`
- `AtomUI.Desktop.Controls.DataGrid.Tests`
- `AtomUI.Generator.Tests`
- `AtomUI.Icons.Shared.Tests`
- `AtomUIGallery.Tests`

新增 solution completeness 测试：递归发现 `tests` 下文件名以 `.Tests.csproj` 结尾的项目，将规范化仓库相对路径与
`AtomUI.slnx` 的 `<Project Path="..." />` 集合比较。缺少任一测试项目时失败；fixture 项目不以 `.Tests.csproj` 命名，
不会被误纳入完整回归入口。

发布准备先对完整 solution restore 一次，再运行一次完整测试：

```bash
dotnet restore AtomUI.slnx --disable-build-servers -m:1 /nr:false
dotnet test AtomUI.slnx --framework net10.0 --no-restore --disable-build-servers -m:1 /p:UseSharedCompilation=false
```

不再先运行不完整 solution 后再手工补跑六个项目。开发阶段的 targeted tests 保持不变。

### 6.3 阶段 C：NuGet 单进程串行编排

`BuildNuGetPackages.ps1` 继续读取 `NuGetPackageProjects.ps1`，但不再为每个项目启动独立 `dotnet build` 和
`dotnet pack` 进程。脚本根据同一清单生成仅存在于 package 输出工作目录的临时 MSBuild traversal project，包含前置项目和
20 个产品包的绝对路径，不在仓库中维护第二份项目清单。

编排分为三个严格顺序的 MSBuild target：

1. `BuildPrerequisites`：按清单顺序串行构建 Build Tasks 和 linked-publish Generator。
2. `BuildPackages`：在同一个 MSBuild 进程中以 `BuildInParallel="false"` 构建所有产品包。
3. `PackPackages`：在同一个 MSBuild 进程中以 `BuildInParallel="false"`、`NoBuild=true` 和统一
   `PackageOutputPath` 对所有产品包执行 Pack。

外层 invocation 继续设置 Release/Debug configuration、`--disable-build-servers`、`-m:1` 和 `/nr:false`。MSBuild 在同一
build session 内复用已完成的 project target 结果，避免 20 个进程重复评估和重复访问依赖图，同时不引入并行写竞争。

若 traversal 无法保证现有 pack 内部 Sidecar build、工具资产内容或 fail-fast 语义完全一致，则阶段 C 停留在
“ShadowKey 修复后的现有逐项目编排”，不以降低质量换取额外速度。阶段 C 只有通过第 7 节的包等价性验收后才保留。

### 6.4 阶段 D：统一发布验收摘要

`BuildNuGetPackages.ps1` 为 prerequisite build、package build、pack 和 artifact verification 分别记录 Stopwatch 耗时，
结束时输出固定表格；检测到 `$GITHUB_STEP_SUMMARY` 时同步写入 GitHub Actions summary。摘要至少包含：

- 解析后的 commit SHA 与 AtomUIVersion。
- 前置项目数、产品包项目数、实际 `.nupkg` 数。
- 各阶段耗时与总耗时。
- 20 个 Package ID/文件名校验结果。
- 是否为 publish dry-run。

本地输出和 GitHub summary 使用相同字段顺序，避免人工重新整理验收格式。摘要只报告事实，不替代测试、Browser 或
AOT 的原始退出码。

### 6.5 Workflow 小项

`release-nuget-packages.yml` 的 local-feed 注册名称当前未被后续命令消费；验证使用的是目录绝对路径。删除
`dotnet nuget add source` 与 `dotnet nuget list source`，只创建目录。该改动约节省 5 秒，并减少对 runner 级 NuGet config
的修改。

本地 feed push、artifact upload 和 nuget.org push 总计约半分钟，保持串行，不为秒级收益增加发布侧复杂度。

## 7. 验收方案

### 7.1 TDD 与定向验证

1. 先添加 ShadowKey writer 属性测试并确认它因缺少 `WriteOnlyWhenDifferent` 失败。
2. 实现最小 writer 修复并确认测试通过。
3. 连续执行两次 Generator Release build，确认第二次三个关键输出时间戳不变。
4. 先添加 solution completeness 测试并确认它列出六个缺失项目。
5. 更新 `AtomUI.slnx` 后确认 completeness 测试通过。
6. 为 package traversal 的顺序、串行参数、单一 manifest 和 artifact completeness 写失败测试，再实现编排。

### 7.2 完整回归

运行更新后的单一完整测试入口，要求全部测试项目通过，且测试数量不低于优化前各入口结果之和。若发现原 solution 与
补充项目存在重复执行，按测试项目集合去重统计，不按控制台总数制造虚假增长。

### 7.3 构建与包等价性

在隔离输出目录分别运行优化前基线编排与优化后编排：

- 两边均产生且只产生同版本 20 个 `.nupkg`。
- Package ID、版本、dependency group、TFM、buildTransitive、tools、analyzer、language assets 和 Sidecar entry 集合一致。
- 对每个 ZIP entry 计算内容哈希；除允许明确记录的生成时间元数据外，不得存在内容差异。
- `dotnet restore` 使用本地 feed 对全部包完成消费验证。

### 7.4 平台与发布验证

- Browser Release build 和真实运行 smoke 通过。
- full AOT/Trim matrix 通过。
- Desktop Gallery NativeAOT publish 通过。
- 使用 `PublishToNuget=false` 触发 GitHub Actions 干跑；不得在优化验证阶段推送包。
- 干跑记录各阶段耗时。Build + Pack 目标为不超过 12 分钟；超过目标但质量验证通过时，保留 P0/P1 的正确性修复，
  重新分析阶段 C，不通过并行或删门禁强行达标。

## 8. 失败处理与回退

- ShadowKey 修复若导致内容变化未更新 key，回退实现并保留失败集成证据；不得固定 key 或绕过 shadow copy。
- 完整 solution 若暴露既有失败，单独记录失败归属；不得把测试项目从 solution 删除以恢复绿色。
- traversal 若产生任何包内容差异、Sidecar 缺失、顺序依赖或文件锁问题，回退到逐项目 build/pack，只保留已验证的
  ShadowKey 与测试入口优化。
- workflow 干跑只允许 `PublishToNuget=false`；发布 secret 不进入性能实验。
- 所有临时 traversal、基线包、对比清单和测试结果写入隔离的 `/tmp` 或 `.artifacts`，验证结束后按仓库清理规则处理。

## 9. 预期结果

- NuGet Build + Pack 从 24 分钟级回落到 8–12 分钟目标区间。
- 完整测试由一个维护入口完成，不再手工补跑六个遗漏项目。
- 发布摘要格式固定，可直接比较相邻 release 的阶段耗时和产物数量。
- 发布质量门禁、20 包完整性、Browser、AOT/Trim 和 NativeAOT 覆盖保持不变。
- 若单进程 traversal 未通过包等价性验证，最终结果仍至少包含已证明安全的 ShadowKey 修复和完整测试入口，且不会以
  发布质量换取速度。
