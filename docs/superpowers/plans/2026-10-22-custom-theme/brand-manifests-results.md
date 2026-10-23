# CT-09 显式注册与版本1配方验收

工作位置为既有`feature/custom-theme`，HEAD `79f2fc00e`及未提交CT-08B/C；主agent独立实施和作者自审。CT-09机制验收记录，**不代表CT-10–CT-26或全控件视觉完成**。

## 实现

- 公开不可变`ThemeStyleAssetRegistration`、能力枚举、builder/根builder扩展；规范registry按ID/revision和owner冻结，重复修订不能替换工厂，只针对已启用owner。启动/package登记不执行资产工厂；候选registry与既有package状态一起交换/回滚。
- 原生图准备检查writable styled Setter及编译模板能力，保留资格、资源、owner/nesting及退役规则；不从数据加载代码/AXAML/任意URI。
- `ThemeBrandRecipes`复用同一Manager：Read/Write/CreateConfig/ApplyAsync/ExportCurrent/GetSelection；手工有界JSON，Complete与TokenOnly显式区分，保留nullable算法/Control模式及有序资产引用。完整导出绑定精确schema及基础ContentDigest/ProgramDigest，使用已提交有效输入，不导出机器路径。
- 配方选择进入请求键和有效输入，选择相同/资产相同/Token相同分别处理；纯选择复用图，失败不切换选择。requirements在去重前、编译和scope空配置快捷返回前校验，跨runtime配置重新核对规范资产引用。
- 包候选统一复核活动根层/LastValid scopes的精确要求；拒绝发布将使当前配方失效的schema。不兼容追加先解除或替换相关配方。仅登记表现资产复用原schema/semantic/catalog，既不执行工厂也不改变活动图或使配方失效。
- 已知但未启用Control的metadata异常转为ATMTHM7206数据诊断，不自动启用包。旧XML v1/compiled ABI v1未改变，旧外部builder默认拒绝新协议。

## 失败与通过

临时Core用例在所属项目Temporary目录真实编译执行，最终**30/30 PASS、0SKIP、444ms**。有效失败前证：builder入口缺失1、非法Setter2、模板能力缺失1、选择键1、JSON协议缺失10、完整runtime入口缺失1、scope空配置绕过基础要求1、旧请求NoOp绕过schema1、跨runtime绑定root/scope2、活动配方package候选root/scope2、disabled metadata诊断1、asset-only错误改变schema1。JSON未知成员/重复/版本/算法/能力/kind与限额负例、缺资产/base修订、失败选择保持均真实执行；原生Visual Setter已拒绝作为正对照，不重复发明验证。

旧NoOp反例的包追加现在被更早的候选要求门槛阻止，临时场景改为断言拒绝前状态保持；入口复核仍保留。新增asset-only正向对照证明未放宽要求且合法登记可继续。保留完整导出对象时，已Dispose runtime的工厂捕获对象弱引用被回收；导出仅保留数据。模板撤销恢复到采样前同一默认模板引用；ContentControl本身非空默认模板不被误判成残留。

## 脚本化渲染

外部临时Visual.csproj引用当前源码，用Skia + HeadlessDrawing=false、真实AtomUI Window/Card/主与默认Button/选中CheckBox，480×400、scale1，覆盖实际Transition时长并要求连续3帧相同。定制使用AXAML编译专用CardHeaderStyle Padding17/LightCyan与Button ControlTemplate；探针只读实际模板/值，不找节点改属性。

- 默认同版本A/A **0像素**；1DIP上边距检错 **6316像素**，比较不裁剪/对齐/缩放/遮罩、不使用通用容差。最初固定宽居中场景的左margin1DIP被像素取整抵消，实际确认后改为真实上边距1DIP，不计作产品修复。
- 已提交HEAD79默认 vs 当前默认 **0 RGBA差异像素**。在同一worktree临时反向本任务patch构建基线，finally SHA256核对**56文件全部原样恢复**并删除patch；未建副本/分支/worktree。
- 品牌实际改变**30814像素**；同实例连续10次默认恢复均0，完整manifest导入均0；模板引用及勾选状态保持。公开`Application.GetAtomUI().LoadPackages`仅登记未用资产，追加前后0像素且不执行工厂，配方要求仍有效。
- 嵌套配方/重置scope实际生效；实际物理类型为**PopupRoot与OverlayPopupHost**，打开、局部配方切换、同实例重开均通过。测试Popup需要在owner逻辑树，并完成各自TopLevel布局；补齐测试条件后通过，未修改产品规避。
- 已查看默认/基线/品牌/恢复/导入及1DIP/品牌差异图。品牌只呈现声明的header背景/内边距、编译模板及主色；恢复回到原布局与外观。脚本无桌面点击/识图。

未证明原生OS最终显示面、按钮闪动、多窗口vsync、NativeAOT、其他平台及其他控件；仍归CT-12/CT-25及各家族出口。接口可用与全产品精确主题完成分开记录。

## 清理与最终检查

所有30项临时Core验证源及辅助文件已删除；外部驱动/bin/obj、临时基线patch已删除，媒体audit无发现，cleanup成功删除10文件（含清单），无本次进程。重建Core/Desktop测试程序集均无临时namespace的UTF8/UTF16标记，项目中没有本次取证媒体。

清理后Core精确13组filter **115/115 PASS、0SKIP、685ms**，包括runtime/scope/lease/catalog/startup/配置规范化与不可变/XML reader/来源登记；Desktop CardSemanticPart/WindowThemeContextLease/ThemePopupInheritance **17/17 PASS、0SKIP、879ms**。既有Startup用例顶层枚举Style的假设不适用于代group，替换为真实Window.ApplyStyling后Context/Active实际命中断言，资源优先级断言全部保留；首轮114PASS/1FAIL的原因及修订留档，不冒充产品渲染缺陷。

NET8先用Debug restore时只有net10 target，按实际Release双框架恢复后，Core Release/net8构建**0warning/0error、4.52s**。精确命令：

```sh
dotnet restore src/AtomUI.Core/AtomUI.Core.csproj -p:Configuration=Release
dotnet build src/AtomUI.Core/AtomUI.Core.csproj --configuration Release --framework net8.0 --no-restore
dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~ThemeConfigProviderTests|FullyQualifiedName~ThemeScopeGraphTests|FullyQualifiedName~ThemeContextLeaseTests|FullyQualifiedName~ThemeContextLeaseLifecycleTests|FullyQualifiedName~ThemeManagerTests|FullyQualifiedName~ThemeManagerTransactionTests|FullyQualifiedName~ThemeCatalogReloadTests|FullyQualifiedName~ThemeStartupTests|FullyQualifiedName~ThemeConfigNormalizerTests|FullyQualifiedName~ThemeConfigImmutabilityTests|FullyQualifiedName~ThemeDocumentReaderTests|FullyQualifiedName~ThemeDefinitionResolverRegistrationTests|FullyQualifiedName~StartupContractTests'
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~CardSemanticPartTests|FullyQualifiedName~WindowThemeContextLeaseTests|FullyQualifiedName~ThemePopupInheritanceTests'
```

6份相关正式/计划文档84个本地链接无缺失；Provider成员审计0问题；diff检查通过。作者独立步骤自审，修订NoOp/空配置、外来descriptor、活动配方候选、disabled metadata及仅登记误改schema，Critical/Important无未处置项；这是作者自审，不是独立agent评审。模型、请求/提交/失败、包候选、AOT无反射路径和释放边界逐项核对。无净新增常驻测试、无全量回归、无提交。

CT-09机制出口完成，下一项为[CT-10共享消费](shared-consumers.md)。
