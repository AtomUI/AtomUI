# SDD ledger — plan: docs/superpowers/plans/2026-11-17-delete-bootstrap-material-themes.md

执行方式：Native（本会话内联执行）。无 worktree、无 commit（改动停工作区交审）。

Pre-flight: 任务为严格顺序链 1→2→3→4→5→6，唯一跨任务命名契约为 Task 4 的 fixture（Id=`GalleryFixedBrandFixture` / DesignLanguage=`GalleryFixed`），各任务内一致，无冲突。

Task 1: complete（目录已删：src/AtomUI.Themes.Bootstrap、src/AtomUI.Themes.Material；slnx 删 2 行；NuGetPackageProjects.ps1 删 2 条；验证: pwsh 加载清单 → Count=23（原25）, Matches=0，与 Expected 一致。无 commit（按计划约束）。）

Task 2: complete（AtomUIGallery.csproj 删 2 ProjectReference；ThemeManagerBuilderExtensions 删 2 using + 注册链 2 调用；GalleryThemeMenu 删 2 字典项；WorkspaceWindowLangResourceKind 删 2 枚举成员；4 个 xlf 各删 2 unit；ThemePresetsViewModel.IconFor 删 2 分支；验证: dotnet build AtomUIGallery.Desktop → Build succeeded, 0 Warning, 0 Error，与 Expected 一致。）

Task 3: complete（Themes.Tests.csproj 删 2 ProjectReference；删 BootstrapThemePackageTests.cs、MaterialThemePackageTests.cs；AvaloniaTestApp.cs 删 2 using + 注册链 2 调用；验证: dotnet test AtomUI.Themes.Tests --framework net10.0 → Passed 323/323, Failed 0，与 Expected 一致。）

Task 4: complete（新建 Fixtures/GalleryFixedBrandFixture.theme.xml（DesignLanguage=GalleryFixed, BrandColor=Fixed, ColorPrimary=#7C3AED）+ csproj 注册；ThemePresetsShowCaseTests 五处：品牌支持测试重定向 fixture、Package_Profiles 删 Bootstrap/Material 两块、Local_Preview 删家族循环（含 check/radio 禁用前景断言，计划已记录的取舍）、Swatches_Disabled 重定向 fixture、删除 Own_Seed_Swatch 整测（计划已记录的取舍）；WorkspaceWindowLayoutTests/GalleryBaseFoundationTests 的 Execute("Bootstrap")→Execute("GalleryFixed")；FinalVisualTemporaryTests 删 Material 理论行；验证: 聚焦三件套 → Passed 55/55, Failed 0；Rendering.Tests 编译 → 0 Warning 0 Error，与 Expected 一致。）

Task 5: complete（活文档 15 处 + 计划外 2 处：CHANGELOG.md L9/L22、CHANGELOG.zh-CN.md L11/L24 的 Unreleased 段 Bootstrap 焦点阴影句一并修剪；迁移文档 EN L214-219 与 zh L201-204 删 Bootstrap 具体值句；Shadcn README、card/token.md、themes 模块与架构文档、Gallery 预设页文档（Material 示例换 PrimeNG noir #020617/#FAFAFA）全部更新；验证: grep 活文档包名/Use 调用 → 0 命中，宽松词扫描仅剩「应用引导 bootstrap」无关概念 2 处。）

Task 6: complete（残留引用清零：代码/配置/活文档两组 grep 均 0 命中；最终聚焦回归: Themes.Tests 323/323 + Gallery.Tests 465/465 全绿；git status 足迹 = 14 删 + 43 改 + 1 新 fixture（.xml）+ superpowers 文字记录，无杂物；git diff --check 干净。真机视觉验收步骤已写入交付说明，标注待视觉验收。）

Ruling (Task 5): CHANGELOG Unreleased 段第 22/24 行「Bootstrap 与 PrimeNG 的输入框焦点阴影有变化」为计划遗漏点，执行中发现并一并修剪（同属 Unreleased 活文档，符合"彻底删除"指令）——代价：若用户想保留 Bootstrap 历史对照句需恢复，可能性低（该包从未发版）。

Final review: 独立子代理 fresh-context 审查（审查重点 = 计划 Review Focus 五项 + 测试改造自洽性）。Critical 0；Important 2（preset-theme-packages.md L7「四个预设包」失实、GalleryBaseFoundationTests.cs L431 注释家族列举过时）；Minor 3；Ruling 与宽松词命中均获支持。
Final: fixed 两个 Important — preset-theme-packages.md「四个→两个」、测试注释删 Bootstrap/Material 列举；修复后 GalleryBaseFoundationTests 聚焦回归 29/29 GREEN。
Final: minor (deferred): Own_Seed_Swatch 删除后「Fixed 主题暗色 profile 自带不同种子」组合形态无同形覆盖（单一能力均有覆盖，计划已声明取舍）。
Final: minor (deferred): Material 类型化覆盖下 check/radio 禁用 mark/dot 前景断言随主题删除（ColorTextDisabled 产品逻辑在 Desktop.Controls.Tests 另有覆盖）。
Final: minor (deferred): Primary_Choose_Follows_Declared_Brand_Support 重定向后断言丢失 message 参数（单条断言无影响）。

任务收尾状态：改动停留工作区，未 commit（等待用户明确指令）；真机视觉验收待用户回传。
