# AtomUI 版本发布准备规范

本文档定义 AtomUI 版本发布准备中「破坏性变更」的判定口径、强制审计步骤、发布就绪清单和发布后缺陷处理策略。
`version-release` skill 负责执行流程，本文档是它的规则来源。

## 适用范围

用户要求发布或准备 AtomUI 版本，或评审发布就绪时适用。正式 changelog 摘要仍由 `CHANGELOG.md` 与
`CHANGELOG.zh-CN.md` 维护；本文档只定义判定口径与门禁，不重复变更条目。

## 破坏性变更的定义

判定依据是**消费方可见的契约是否发生变化**，而不是「是否删除了 C# `public` 成员」。以下任一表面改变即视为破坏性变更：

| 表面 | 典型变化 |
| --- | --- |
| C# 公共 API | 类型、成员、签名、枚举值、默认行为 |
| MSBuild 契约 | `build/**` 属性、Target、`UsingTask` 注册方式、任务参数、Item/Property 名 |
| 包契约 | 目标框架、产物布局（`tools/`、`buildTransitive/`）、包 ID、交付文件 |
| 构建前提 | 所需 SDK/Runtime 版本、`global.json` |
| 运行时契约 | 设计 Token、主题键、资源键、AOT/trimming 行为 |

**构建工具与打包契约的变更同样属于破坏性变更，即使控件 API 完全不动。** 仓库已有先例：

- `docs/releases/6.1.4-api-changes.md`：构建任务路径覆盖 `AtomUILocalizationBuildTasksAssembly` 更名为
  `AtomUIBuildTasksAssembly`。
- v6.1.9：构建任务由 `netstandard2.0` 进程内加载改为 `net10.0` 单次进程执行，新增 .NET 10 SDK 构建要求，
  移除影子副本属性与目录布局。

## 强制审计步骤

1. 枚举破坏性候选项：

   ```bash
   git log <previous-tag>..HEAD --format='%h %s' | grep -E '!:'
   ```

   每个命中项都必须单独给出结论。带 `!` 的提交是仓库在提示存在破坏性变更，不得无证据忽略。

2. 按上表逐面比对 diff：C# 公共表面、`build/**`、包布局、TFM/SDK、运行时契约。

3. 证据规则：判定「非破坏性」必须**正面说明**该变更触及哪些表面、为什么消费方不受影响。
   只返回空结果的否定式检查（例如 `grep '^-.*public '`）**不构成证据**——它看不见 MSBuild、打包、
   TFM、SDK 变化，不得作为结论。

4. 先例核对：只要存在破坏性候选项，或改动触及 `build/`、打包或 TFM，先读最近一份
   `docs/releases/*-api-changes.md`，并在全部历史迁移文档中检索同类：

   ```bash
   grep -rln -iE 'build|packaging|msbuild|netstandard|net10|sdk|tools/' docs/releases/*-api-changes.md
   ```

5. 产出结论表并在发布提交前报告用户：

   | 候选项 | 是否破坏性 | 证据 |
   | --- | --- | --- |
   | `<commit / 变更>` | 是 / 否 | 触及的表面 + 为何消费方受影响或不受影响 |

## 机械化门禁

手工审计会被遗漏；发布流程必须叠加两道可执行门禁，覆盖不同表面。

| 门禁 | 实现 | 覆盖表面 | 是否受 `!` 标记影响 |
| --- | --- | --- | --- |
| 包 API 校验 | `EnablePackageValidation` + `PackageValidationBaselineVersion`（`build/PackageValidation.props`，ApiCompat IL 级比对上一版） | `lib/` 公共 API、跨 TFM 一致性 | 否 |
| 包布局校验 | `scripts/verification/verify-package-layout.ps1` | `tools/`、`buildTransitive/`、`build/`、`lib/` TFM 集合 | 否 |

两者都在 `scripts/BuildNuGetPackages.ps1` 的 `-PackageValidationBaselineVersion` 下运行；发布 workflow 通过同名输入传入上一版本号。

### 关键性质

- **两道门禁都不依赖提交的 `!` 标记**，观测的是产物事实，因此能拦住"漏打 `!`"的情况。6.1.9 的
  `tools/netstandard2.0 → tools/net10.0` 属于包布局门禁的覆盖范围；ApiCompat 看不到它（它只比对 `lib/` 程序集）。
- 基线包由发布脚本对每个发布包项目的 build 步骤隐式 restore 拉取（该 build 带 Release 配置与校验属性），随后
  `pack --no-build` 复用同一 restore。不要在脚本中另加独立 `dotnet restore`：不带 Release 配置的 restore 会用
  Debug 目标框架覆盖 assets 文件，导致 `pack` 报 `NETSDK1005`。前置构建工具不是产品发布包，不施加产品 API 基线属性；TypeMap 目标工具清单与迁移状态见
  [构建与打包](../../architecture/foundations/build-and-packaging.md)。
- AtomUI 开启 `AvaloniaAccessUnstablePrivateApis` 后，`build/PackageValidation.props` 必须在 SDK 完成每个 TFM 的
  `PackageValidationReferencePath` 收集后追加 Avalonia 真实拆分程序集。任何 `Could not resolve reference` 都说明
  ApiCompat 引用图不完整；即使命令退出码为 0，也不能把该次运行作为包 API 门禁通过证据。
- 断言"无破坏"必须有正面证据；只返回空结果的否定式检查不构成证据。

### 有意破坏的处理

- **包 API 破坏**：不得用 ApiCompat suppression 让兼容性门禁通过。若公共 API 删除、成员删除或枚举值变化是产品要求，
  发布必须调整版本/基线策略，使该发布不再声明与被比较基线 API 兼容；否则包验证失败是正确结果。
- **包布局破坏**：在 `scripts/verification/package-layout-allowlist.json` 中按**基线版本 + 包 ID** 记录条目与
  `reason`。条目必须被当前比对实际命中，否则门禁失败，防止清单腐化。
- 两类确认都必须同时补 `docs/releases/<version>-api-changes.md` 迁移说明。

## 迁移文档门禁

- 审计发现任意破坏性变更时，**必须**创建 `docs/releases/<version>-api-changes.md` 与
  `docs/releases/<version>-api-changes.zh-CN.md`，写入 `docs/releases/overview.md`，并在 changelog 的
  `Breaking Changes` 组链接该文件。
- `Breaking Changes` 组中每一条都必须对应迁移文档中的一个章节。无法对应时，要么补章节，要么记录用户确认的豁免理由。
- 审计未发现破坏性变更时不创建这些文件。

## 发布就绪清单

- [ ] 破坏性审计完成，结论表已报告用户
- [ ] `build/Versions.props` 的 `AtomUIVersion` 是唯一版本来源，无散落硬编码
- [ ] 中英文 README 四处版本一致，Release Notes 段落已按本版本重写
- [ ] 中英文 CHANGELOG 同版本、同日期、同信息量，`Breaking Changes` 组在最前
- [ ] 破坏性变更的迁移文档与 `overview.md` 链接齐备
- [ ] 包 API 校验与包布局校验以上一版本为基线通过（有意破坏已写入抑制文件或布局清单）
- [ ] 全量回归测试通过（`version-release` skill 要求）
- [ ] `git diff --check` 干净

## 发布后缺陷处理

- 标签一旦推送，默认**不移动**。发布后再发现缺陷（例如缺失迁移文档），默认在发布分支上追加提交做前向修复，
  并说明影响范围与用户可见后果。
- 只有在用户明确指示时才用 `git tag -f` 重打已推送标签，且不得静默执行；重打会改写已发布引用，需说明风险。
- 授权顺序：准备并验证 → 提交 → 打标签 → 推送/触发发布。禁止在破坏性审计完成前请求标签或推送授权。

## 反模式

- 用 `grep '^-.*public '` 的空结果宣告「无破坏性变更」。
- 把构建工具或打包契约变更当作「非公共 API」而不写迁移文档。
- 看到 `!` 标记的提交仍按普通 fix 处理。
- 在破坏性审计完成前提交、打标签或推送。
- 发现已推送标签有缺陷时直接 `git tag -f` 重打而不询问。
