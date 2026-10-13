# AtomUI Agent Guide

This file gives AI coding agents the stable entry points for working in AtomUI. Keep this file small. Put concrete, detailed rules in focused documents under `docs/`, then link them here.

## Project Profile

AtomUI is an Ant Design 6 style component system for Avalonia/.NET desktop applications. It contains runtime libraries, optional desktop control packages, source generators, icon/font packages, tests, and the AtomUI Gallery sample application.

```text
AtomUI/
├── src/
│   ├── AtomUI.Native             # Native platform window capabilities
│   ├── AtomUI.Core               # Cross-cutting runtime infrastructure
│   │   ├── Theme                 # Theme manager, token system, palette, styling
│   │   ├── Language              # Localization providers and language resources
│   │   ├── Animations            # Transitions and animation helpers
│   │   ├── MotionScene           # Motion orchestration primitives
│   │   ├── Controls/Icon         # Icon base types and rendering contracts
│   │   ├── Data                  # Resource and binding support infrastructure
│   │   ├── Media                 # Text/media helpers
│   │   ├── Reflection            # Explicit compatibility boundary for reflection helpers
│   │   └── Utils                 # Shared low-level utilities
│   ├── AtomUI.Controls.Shared    # Shared contracts and data infrastructure
│   ├── AtomUI.Controls           # Cross-platform base controls for mobile and desktop
│   ├── AtomUI.Desktop.Controls   # Desktop control library
│   │   └── DatePicker            # Representative localized control folder
│   │       ├── DatePicker.cs     # Control implementation
│   │       ├── DatePickerToken.cs
│   │       ├── CalendarView/     # Internal view primitives for the control
│   │       ├── Localization/
│   │       │   ├── en_US.cs
│   │       │   ├── zh_CN.cs
│   │       │   └── zh_TW.cs
│   │       └── Themes/
│   │           ├── DatePickerTheme.axaml
│   │           ├── DatePickerPresenterTheme.axaml
│   │           ├── RangeDatePickerTheme.axaml
│   │           └── DatePickerThemes.axaml
│   ├── AtomUI.Desktop.Controls.DataGrid
│   ├── AtomUI.Desktop.Controls.ColorPicker
│   ├── AtomUI.Desktop.Controls.Extras # Stable supplemental controls beyond Ant Design
│   ├── AtomUI.Toolkits.GalleryBase # Product-neutral Gallery base controls and themes
│   ├── AtomUI.Icons.*            # Icon infrastructure and Ant Design icon package
│   ├── AtomUI.Fonts.*            # Font packages
│   ├── AtomUI.Toolchain          # Build tasks, shared build models, ILLink/ILC registration backends
│   └── AtomUI.Generator          # Roslyn source generators
├── controlgallery/              # AtomUIGallery and desktop host
├── tests/                       # Unit and regression tests
├── docs/                        # Architecture, engineering, Gallery, and control docs
├── build/                       # Build props and supporting scripts
└── resources/                   # Images and static resources
```

Important source boundaries:

- `AtomUI.Core`: theme, token, language, resources, animation, platform infrastructure.
- `AtomUI.Controls.Shared`: cross-control data contracts, collection views, shared state.
- `AtomUI.Controls`: common controls and primitives.
- `AtomUI.Desktop.Controls`: main desktop control package.
- `AtomUI.Desktop.Controls.DataGrid`, `AtomUI.Desktop.Controls.ColorPicker`, and `AtomUI.Desktop.Controls.Extras`: optional desktop packages.
- `AtomUI.Toolkits.GalleryBase`: product-neutral Gallery base controls, models, themes, and runtime helpers.
- `AtomUI.Generator`: Roslyn source generators referenced as analyzers.
- `AtomUI.Toolchain`: one build-tool project with isolated Tasks/ILLink8/ILLink10 profiles and the ILC8 adapter recipe; never an application runtime dependency.
- `AtomUIGallery`: Gallery shell, showcases, API tables, token tables, and NativeAOT publish target.

Approved Mobile target boundary: `AtomUI.Mobile.Controls` is a product package parallel to `AtomUI.Desktop.Controls`, with iOS-first
implementation and iOS/Android architecture from Foundation. The current solution has no Mobile Controls project, source, platform
validation, release, or publication evidence.

## Instruction Structure

Do not let `AGENTS.md` become a rule dump. When adding durable guidance:

1. Put narrow, concrete constraints in the most relevant document under `docs/`.
2. Link that document from this file only when it is a common entry point.
3. Prefer a new focused document over expanding this file when the rule applies to one subsystem or workflow.
4. Keep this file limited to project context, routing, and non-negotiable collaboration expectations.

## Required Reading

Read the relevant document before touching the corresponding area:

- Overall architecture: [docs/architecture/overview.md](docs/architecture/overview.md)
- Module boundaries: [docs/architecture/foundations/dependency-graph.md](docs/architecture/foundations/dependency-graph.md)
- Build-tool ownership and profiles: [docs/modules/toolchain/overview.md](docs/modules/toolchain/overview.md)
- Mobile target architecture: [docs/architecture/systems/mobile/overview.md](docs/architecture/systems/mobile/overview.md)
- Localization target architecture: [docs/architecture/systems/localization/overview.md](docs/architecture/systems/localization/overview.md)
- AI collaboration and bug-fix discipline: [docs/engineering/contributing/agent-guidelines.md](docs/engineering/contributing/agent-guidelines.md)
- Test value, temporary-first lifecycle, and mandatory feature/bug-fix acceptance: [docs/engineering/development/test-value-and-lifecycle.md](docs/engineering/development/test-value-and-lifecycle.md)
- Reference project source lookup: [docs/engineering/contributing/reference-project-source-guidelines.md](docs/engineering/contributing/reference-project-source-guidelines.md)
- AOT and trimming architecture: [docs/architecture/foundations/aot-and-trimming.md](docs/architecture/foundations/aot-and-trimming.md)
- TypeMap registration pipeline: [docs/architecture/foundations/aot-typemap-registration.md](docs/architecture/foundations/aot-typemap-registration.md)
- Control, Token and resource registration contracts: [docs/architecture/foundations/control-registration-contracts.md](docs/architecture/foundations/control-registration-contracts.md)
- Browser fine trimming backend: [docs/architecture/foundations/aot-browser-linking.md](docs/architecture/foundations/aot-browser-linking.md)
- Control registration generator: [docs/modules/generator/control-registration.md](docs/modules/generator/control-registration.md)
- AOT programming, dynamic data, source generators: [docs/engineering/development/aot-programming-guidelines.md](docs/engineering/development/aot-programming-guidelines.md)
- Compiler diagnostics: [docs/engineering/development/compiler-diagnostics-guidelines.md](docs/engineering/development/compiler-diagnostics-guidelines.md)
- Code style: [docs/engineering/development/code-style-guidelines.md](docs/engineering/development/code-style-guidelines.md)
- Documentation structure and file naming: [docs/engineering/contributing/documentation-structure-guidelines.md](docs/engineering/contributing/documentation-structure-guidelines.md)
- Mobile documentation contract: [docs/engineering/contributing/mobile-documentation-guidelines.md](docs/engineering/contributing/mobile-documentation-guidelines.md)
- Changelog and release notes: [docs/engineering/contributing/changelog-guidelines.md](docs/engineering/contributing/changelog-guidelines.md)
- Apple iOS development environment: [docs/engineering/platforms/apple-ios/overview.md](docs/engineering/platforms/apple-ios/overview.md)
- Android engineering boundary (Host and commands unvalidated): [docs/engineering/platforms/android/overview.md](docs/engineering/platforms/android/overview.md)
- Gallery NativeAOT release flow: [docs/engineering/workflows/gallery-aot-release-workflow.md](docs/engineering/workflows/gallery-aot-release-workflow.md)
- Gallery page structure: [docs/gallery/authoring/gallery-showcase-design-pattern.md](docs/gallery/authoring/gallery-showcase-design-pattern.md)
- Gallery organization: [docs/gallery/authoring/organization.md](docs/gallery/authoring/organization.md)
- Mobile Gallery host boundary: [docs/gallery/platforms/mobile-gallery.md](docs/gallery/platforms/mobile-gallery.md)
- Resource lifecycle case study: [docs/engineering/case-studies/avalonia-dynamic-resource-memory-leak-case-study.md](docs/engineering/case-studies/avalonia-dynamic-resource-memory-leak-case-study.md)
- Non-Visual AvaloniaObject scoped resource host generator: [docs/modules/generator/scoped-resource-host-generator.md](docs/modules/generator/scoped-resource-host-generator.md)

## Agent Behavior

Detailed AI collaboration rules live in [docs/engineering/contributing/agent-guidelines.md](docs/engineering/contributing/agent-guidelines.md). In this file, keep only the short behavioral contract:

- Understand the affected module before changing code.
- Keep changes scoped to the user request and the ownership boundary.
- When the user asks to reference another project's source, follow the local-first lookup order in [Reference Project Source Lookup](docs/engineering/contributing/reference-project-source-guidelines.md) before using GitHub or relying on memory.
- Prefer root-cause fixes over trigger-point patches.
- Every new feature and bug fix must satisfy [test value and lifecycle](docs/engineering/development/test-value-and-lifecycle.md): new tests are temporary by default, must be written in the prescribed temporary location, and must be deleted after validation. The global suite defaults to zero net-new permanent tests; exceptions require proof of independent value, global-run necessity and reasonable cost. Without before/after evidence, test disposition, cleanup and final module verification, the work is incomplete. TDD does not require permanently retaining validation code.
- For hover, pointer, hit-testing, wheel, scrolling, clipping, or overlay bugs, preserve the original UX contract and follow the [UI input and scrolling bug discipline](docs/engineering/contributing/agent-guidelines.md#ui-输入与滚动-bug).
- Treat AOT compatibility as a first-class design constraint for new features and bug fixes.
- Follow the code style rule that C# control-flow bodies must always use `{}`; even a single-line body may not omit braces.
- Verify with tests or publish checks that match the risk of the change.
- Follow the mandatory Superpowers workflow before any creation, change, implementation, or bug fix: invoke `superpowers:using-superpowers`, then route through `superpowers:brainstorming` (new features/components), `superpowers:systematic-debugging` (bugs), `superpowers:test-driven-development` (implementation), `superpowers:writing-plans`/`superpowers:executing-plans` (multi-step work), and `superpowers:verification-before-completion` (before claiming done). Never skip a skill that could apply; only an explicit user instruction may narrow it.

## 主题绑定优先强约束（本项目生效）

1. **ControlTheme 优先**：Avalonia 控件的模板/样式类绑定（含 RenderTransform、Transitions、模板部件属性、伪类驱动的 Setter 等），只要同时满足以下全部条件，必须优先放在 AXAML ControlTheme 中声明，禁止先写成代码绑定：
   - ControlTheme 能表达该绑定（选择器 / Setter / 模板绑定可达目标元素）；
   - 逻辑效果与代码绑定完全一致（触发时机、状态条件、取值、动画行为）；
   - 性能无可测量差异。
2. **代码绑定是兜底**：仅当 ControlTheme 无法完成时（如目标元素在 Content 子树跨不进 `/template/` 链、依赖 internal 成员而 XAML 编译绑定不可见、目标值为运行时计算值、AOT 约束禁止反射绑定等），才允许放到控件代码中绑定，且必须在代码注释中写明 ControlTheme 不可行的具体原因。
3. **迁移义务**：发现既有的代码绑定实际上可以等价迁移到 ControlTheme 时，在触及该文件的改动中一并迁移，不留“下次再说”。
4. **声明位置边界**：`Transitions` 必须声明在 ControlTheme 的 style setter 中，禁止直接写在 ControlTemplate 内容里——模板构建期元素尚未挂载 clock，会抛 NullReferenceException。

## Semantic PART 专用 Style 强约束（本项目生效）

1. **专用 Style 是唯一定制入口**：Semantic PART 改造中，Gallery 示例、文档示例与测试对语义部件的样式定制，必须使用源生成器产出的专用 Semantic Part Style 类（如 `FlyoutHostPopupRootStyle`、`TreeSelectPopupListStyle`）在 AXAML 声明式应用；禁止在 code-behind 获取目标节点后直接设置属性（Background / Foreground / Padding / CornerRadius 等）作为定制手段。
2. **专用 Style 不生效等于契约缺陷，不是限制**：生成的专用 Style 未命中目标节点时，说明 `SelectorRoute`（`/template/`、`>`、`>>` 组合器）与目标节点的真实树拓扑（模板内 / 逻辑后代 / 跨视觉根代码创建节点）不匹配，必须修正 route 并补可失败的命中测试；禁止把“生成 Style 无法命中”当作限制写进文档，或改用代码回退绕过专用 Style。
3. **测试锁定专用 Style 形态**：新增或改写 Semantic PART 演示时，页面测试 / 快照必须断言专用 Style 类与关键 Setter 值的存在，并断言不存在以 Name / Loaded / Unloaded 事件处理器为特征的代码回退。

详细规则见 [Semantic Part 系统设计](docs/architecture/systems/theming/semantic-parts.md)。

## 提交审查强约束（本项目生效）

1. **未经明确指令不得提交**：没有用户的明确要求，完成一个任务或模块的开发之后，必须先交用户审查；未通过审查前，禁止自行创建 git commit，同样禁止 amend、rebase 等改写已有提交的动作。
2. **验证照常、提交信息可先备好**：构建、测试、运行验证不受本条限制照常执行；提交信息可以预先准备好（如用 commit-msg 技能生成草稿），但执行提交必须等用户明确指令。
3. **例外仅限用户授权**：仅当用户明确要求提交（如“提交”、"commit"、“创建 commit”）或事先授权（如“这个任务做完直接提交”）时才可提交，且只提交授权范围内的工作，不顺手捎带未审查的改动。

## Common Commands

Use [affected verification](docs/engineering/workflows/affected-verification.md) as the default. The following sequence is a mandatory hard constraint for bug fixes and ordinary code changes:

1. For a bug, reproduce with an existing or temporary failing test; follow the documented manual-evidence exception only when automation is not feasible. Validate the fix, decide test disposition, and delete temporary tests and their dedicated artifacts before final verification.
2. Verify only the modules you changed in the final cleaned working tree, through `run --scope iterate --path ...` or an exact `dotnet test --filter`; every iterate run must name its module paths explicitly.
3. Do not run local `change` scope, affected-wide regression, or full regression on your own initiative. Focused tests are the execution gate; bug-fix completion additionally requires the test lifecycle and acceptance checks above.
4. Run `scripts/run-full-regression.sh` only when the user explicitly asks for it or during a release (`version-release`).
5. When a full run does happen and fails, let it finish and collect the complete failure list. Repair every failure with focused tests, then run one final full confirmation. Never rerun the full suite after each individual failure.

The verification CLI enforces scope restrictions, not test-value judgments or the complete bug-fix acceptance checklist. Local `change` scope requires an explicit branch base, local `full` scope is rejected, and focused iterate scope requires `--path`. CI may continue to use `--base`/`--tests-only` for selected or baseline jobs.

```bash
python3 scripts/verification/test.py plan
python3 scripts/verification/test.py run --scope iterate --path src/AtomUI.Desktop.Controls/Popup
python3 scripts/verification/test.py audit
# Only on explicit user request or during a release:
scripts/run-full-regression.sh
```

For CI or an explicitly requested committed-branch comparison, pass `--scope change --base <target-branch>`. Direct project commands below are for reproduction and module-focused diagnosis:

```bash
dotnet test tests/AtomUI.Controls.Shared.Tests/AtomUI.Controls.Shared.Tests.csproj --framework net10.0 --no-restore
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore
dotnet test tests/AtomUI.Desktop.Controls.DataGrid.Tests/AtomUI.Desktop.Controls.DataGrid.Tests.csproj --framework net10.0 --no-restore
dotnet test tests/AtomUIGallery.Tests/AtomUIGallery.Tests.csproj --framework net10.0 --no-restore
git diff --check
```

For NativeAOT Gallery validation:

```bash
pwsh -NoLogo -NoProfile -File controlgallery/AtomUIGallery.Desktop/scripts/PublishToLocal.ps1 -publishRootPath /tmp/atomui-gallery-aot-run -runtime osx-arm64 -buildType Release -publishAot true
```
