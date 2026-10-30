# TypeMap 深入验证与落地边界

> 过程记录：本轮结论已同步到 [正式 AOT 架构](../../architecture/foundations/aot-and-trimming.md)。本文保留方案比较、实验或迁移过程，不再作为平行的长期规范；事实以正式文档为准。


> 2026-09-27。用户已排除显式控件族选择方案，只研究保持一行入口和自动裁剪的 TypeMap 路线。
> 本文记录一次实际源码实验，不是正式产品实现或发布批准。
> 后续新增 [完整替换设计](2026-09-27-aot-typemap-replacement-design.md) 与已完成小型浏览器裁剪/AOT 实验的
> [浏览器编译后端](2026-09-27-aot-typemap-browser-backend-design.md)。下文“保留旧浏览器路径”是本轮早期结论，已被后续方案取代；BCL Mono API 本身仍未实现。

## 1. 结论

.NET 10 NativeAOT 上，方案已从玩具控件验证推进到真实 AtomUI：一行 `UseDesktopControls()`、真实 ThemeManager 初始化、
Button/NumericUpDown/DatePicker 模板、Button Semantic Part 描述和暗色主题切换均在实验原型中通过。

单纯给现有 ControlMap 加 TypeMap 不够。正式设计至少要补四个边界：

1. 控件、Token 扩展/枚举、内部控件等不同使用入口都能保留同一个注册片段。
2. Semantic Part 改成逐控件工厂，由 leaf 一起收集；不能调用全量 manifest 再过滤。
3. 固定资源覆盖/挂载顺序，不能依赖 TypeMap 键的字母顺序。
4. 区分 .NET 10 CoreCLR/NativeAOT 与 .NET 8/9、Mono/WASM，不能宣称统一删除全部旧管线。

## 2. 实验范围与来源

- 源码基线：`0d39ebb5443a6291e4db5941e50ff9fb193dcc9d`。
- 隔离工作区：`/Users/chinboy/.codex/worktrees/aot-typemap-probe/AtomUIV6`。
- 真正启动/模板实验：`/private/tmp/atomui-typemap-real-host`。
- 独立平台语义实验：`/tmp/atomui-typemap-platform-probe-72anbf9a`。
- 环境：SDK 10.0.300、runtime/ILC/ILLink 10.0.8、Avalonia 12.1.2、macOS arm64。

主工作区只更新设计文档。实验代码留在隔离工作区，未提交、未合并、未修改主工作区的产品代码。

实验把 Desktop 入口的 generated 分支直接改为包内 TypeMap 注册；通过 `AtomUILinkedPublish=false` 禁用旧 linked 分析输入。
原仓库的工具项目引用尚未拆除，因此构建仍可能编译 linked generator 项目；不能把机制验证描述成完整工具链删除。
应用端临时手写一个 TypeMapAssemblyTarget；正式产品需要由普通生成器依据 Package 标记自动生成，用户不写这个 Attribute。

## 3. 实际失败与修正

### 3.1 只按 Control 映射，注册选择有效，但真实启动失败

首先复用现有 Minimal fixture。普通 full registrar 的 Desktop 快照有 200 个控件、326 个资产 descriptor。
Control-only TypeMap 原型降到 32 个 Desktop 控件、50 个资产 descriptor；NativeAOT 程序能够输出快照。
ILC map 有 Buttons 代理，没有 DatePicker、NumericUpDown 和 TreeView 的代理。

这只能证明选择发生了。旧 FixtureHost 只创建 builder 和序列化元数据，不执行 ThemeManager 初始化。

真正使用 `Application.UseAtomUI` 初始化并准备 Button/NumericUpDown/DatePicker 时，原型退出 134：

```text
ThemeSchemaException:
TimePicker/Themes/TimeViewCellTheme.axaml references unregistered identity 'AtomUI:ListView'.
```

原因可以精确定位：TimeViewCellTheme 使用 `ListViewTokenResource`，没有创建 ListView；生成的 Token 扩展只返回字符串身份或
装箱后的枚举值。Control-only TypeMap 无法把这个入口关联到 ListView 注册片段。

### 3.2 用 Token 类型作为额外 trimTarget，同一启动场景通过

原型只增加这一变量：为每个有 descriptor 的 owner，生成 TokenResourceExtension、TokenKey 枚举，以及有 Own Token 时的
TokenKind 枚举到同一 Unit 代理的条件映射，同时把这些键加入纯字符串候选清单。

```csharp
[assembly: TypeMap<DesktopMap>(
    "ListView/control", typeof(ListViewRegistration), typeof(ListView))]
[assembly: TypeMap<DesktopMap>(
    "ListView/token-extension", typeof(ListViewRegistration), typeof(ListViewTokenResourceExtension))]
[assembly: TypeMap<DesktopMap>(
    "ListView/own-token", typeof(ListViewRegistration), typeof(ListViewTokenKind))]
```

同一份真实宿主随后通过启动校验和三个控件的模板创建，注册表未在实例化后改变。
不需要手写 `DatePicker → TimePicker → ListView` 依赖链，也不需要分析应用 AXAML。

这些条件由现有 ControlThemeInfo 的命名、HasDescriptor、HasOwnToken 等事实产生；候选键只保存字符串。
多个条件命中同一代理时，按代理 Type 去重，先去重再读取 Attribute。

这不等于任意字符串都是自动可分析的：仅使用 `ButtonTokens.Identity` 静态字段、运行时字符串 key/类型名仍需明确契约。
不能用空 `typeof`、未读取的静态字段或依赖优化器巧合的代码声称已经补齐 owner 引用。

### 3.3 现有 leaf 缺少 Semantic Part，原型确认后补逐控件工厂

Token 条件补齐后，运行时输出 `SEMANTIC_BUTTON=False`。
这不是 TypeMap 特有问题：当前 AotTrimControlPackageRegistrationBuilder 使用四参数 ControlPackageRegistration 构造函数，
该重载把 SemanticControls 设置为空；full registrar 则提交完整语义集合。

原型增加：

- builder 的逐项 Semantic descriptor 收集与平台过滤，提交时使用包含语义集合的构造函数。
- schema leaf 在添加每个 Control descriptor 后调用对应的语义 hook。
- SemanticPartManifestWriter 通过 partial 方法实现该 hook，输出该控件的 descriptor。
- 没有语义声明的控件，partial hook 自动消除；无需一个 generator 读取另一个 generator 的输出。

生产版本应使用完整元数据名的稳定哈希生成 hook 名称，并让 full/leaf 共用同一个 descriptor factory。
原型使用简单名称，属于实验局限，不应直接作为生产实现提交。

加入逐控件语义工厂后，最终 NativeAOT 输出：

```text
STARTUP PASS
SEMANTIC_BUTTON=root,content,icon
TEMPLATES PASS: Button,NumericUpDown,DatePicker
STARTUP_REGISTRY_STABLE=True
DARK_THEME_SWITCH=Committed REGISTRY_STABLE=True
HOST PASS
```

最终 registry 共 61 个控件，包含 Common 与被保留的 Desktop 控件，不能与前面的 Desktop-only 32 直接比较。
最终 ILC map 中 DatePicker、NumericUpDown、ListView 注册片段存在，TreeView 注册片段缺失；
Desktop 全量 `GeneratedSemanticPartManifest.GetDescriptors` 方法也缺失。

因此这次 Semantic Part 补全没有通过全量 manifest 把所有控件重新保留下来。

## 4. 收敛后的组件边界

```text
Package 源码/主题/语义声明
    └─ 普通 Generator
        ├─ 单项 descriptor 与 leaf fragment
        ├─ Control/Token/内部类型 → Unit 代理的 TypeMap
        ├─ 纯字符串候选键
        └─ Package Group 标记

应用普通编译
    └─ 小型引导生成：引用程序集标记 → TypeMapAssemblyTarget

ILLink / ILC
    └─ 代码可达性 + 条件映射闭包

UseDesktopControls()
    └─ Package Core → TypeMap 查询/去重 → 收集片段
       → 资源校验和挂载 → 注册表冻结
```

AtomUI 负责框架业务事实；官方 linker 负责代码可达性。无应用 usage Sidecar、无自定义应用依赖图、无多轮 linker。

普通 Package 仍整包作为一个 Unit；大型 Desktop 保留控件族粒度。一个 Unit 的内容范围应由产品能力决定，不能随意把 helper
目录作为独立功能。迁移不等于承诺单个 CLR 类型的最小体积。

## 5. 必须补齐的类型保留条件

| 使用入口 | 处理 |
| --- | --- |
| new Control / typeof(Control) | 官方 TypeMap 条件，已测 |
| 模板创建内部 Control | 以实际内部类型归属 Unit；正式模型必须补齐 internal 类型枚举 |
| AXAML TokenResource 扩展 | 扩展类型映射到 owner Unit，真实 AtomUI 已测 |
| Token 枚举装箱到资源 key | 枚举类型映射到 owner Unit，随真实 Token 路径已测 |
| 生成的 Semantic Style | 验证生成 selector 已包含 owner 的真实类型引用；不能仅假设 |
| 纯静态附加属性访问 | 实际实现若已有 owner 的 typeof/运行时泛型类型引用可触发；静态调用本身不保证 |
| 纯字符串 Identity / 动态主题 | 明确强类型 owner 或动态 root；不承诺自动推断任意字符串 |
| UseAllDesktopControls | 直接 full registrar，保留现有显式全量语义 |

现有 ControlThemeModelBuilder 主要枚举 public 非泛型控件，并通过 Token 等补入部分类型，不是完整 internal Control 清单。
正式迁移必须验证 internal-only 跨 Unit 使用，不能只依赖本次 DatePicker 路径恰好被公开 owner 覆盖。

## 6. 资源顺序不能交给 TypeMap 查询顺序

当前 full loader 与 leaf builder 的资源添加顺序已经不完全相同：full 部分按 asset path 排序，leaf 受 Unit planner 顺序影响。
ControlPackageRegistration 排序 descriptor 不会重排已经挂载的实际资源；Avalonia 的 merged dictionary 后加入者有更高优先级。

正式改造需要：

1. 资源收集项携带稳定 AssetId、阶段与必要优先级。
2. full 和 TypeMap 共用资源挂载排序。
3. 保留 deferred wrappers，收集期间不提前解析依赖另一份字典的命名资源。
4. 加重复 key、跨字典 BasedOn、用户覆盖优先级回归。

不能以此次三个模板能创建，推断所有资源覆盖顺序都已等价。
现有代码也没有完整的通用 `StaticResource 字符串 → owner factory` 索引；需要局部显式资源所有权，不能声称通用解析器已经存在。

## 7. 官方平台边界

| 平台 | 事实 | 本方案策略 |
| --- | --- | --- |
| .NET 10 NativeAOT | ILC 构建条件映射 | 本次真实 AtomUI 已测 |
| .NET 10 trimmed CoreCLR | ILLink 保留所选 Attribute，CoreCLR 创建懒映射 | 简单跨程序集/AXAML 已测，完整 AtomUI 仍需对应回归 |
| .NET 10 未裁剪 CoreCLR | 全部声明条目；运行时处理元数据 | 可继续使用已有 full registrar |
| .NET 10 Mono/WASM | TypeMapping 公开 API 直接 NotSupportedException，裁剪不会生成另一份实现 | 不能启用本路径；若要求自动精细裁剪，暂保留旧路径 |
| .NET 8/9 | 无官方 TypeMap 能力 | 不能通过普通 polyfill 获得 ILC 条件映射 |

初始支持基线固定为已验证的 runtime/ILC/ILLink 10.0.8。早期 10.0.0–10.0.3 的 ILLink 跨程序集实现存在缺陷；
10.0.4 起包含修复。未来升级同样需要验证实际工具链，不能只判断 TFM 字符串。

不采用 Mono 元数据 adapter 作为正式方案：虽然实验能读取 ILLink 留下的 Attribute 参数，但官方不保证 Attribute 存在，
ILLink 注释明确要求经公开 TypeMap API 访问。NativeAOT 实验中这些 Attribute 已经被删除。

因此“一行入口 + 自动精细裁剪”可以保留；“所有现有目标一次删除旧管线”目前不成立。
本轮不把不支持的平台偷偷降级为全量后仍称为自动精细裁剪，也不重新引入用户已否决的显式选择方案。

## 8. 官方语义的独立实测

相同小程序、工具链 10.0.8：

| trimTarget 使用 | trimmed CoreCLR | NativeAOT |
| --- | --- | --- |
| new、可观察 typeof、new Derived 的基类 | 保留 | 保留 |
| 仅静态方法、仅静态字段 | 不保留 | 不保留 |
| 泛型方法中 typeof(T) | 保留 | 保留 |
| new Gen<int> | 保留 Gen<int>，并保守保留同定义 Gen<string> | 仅保留 Gen<int> |

不承诺两个后端的体积完全相同。
TypeMapping 的调用点必须是具体闭合 Group；通用 `ReadMap<TGroup>()` 内部调用会触发 IL2124。
公共注册帮助逻辑应接收已经取得的 map，而非内部再查询泛型 Group。

TypeMapAssemblyTarget 从入口程序集递归指向包；扫描目标不等于自动启用包。只有实际查询 Group 并满足保留条件才激活代理。
正式引导生成器只读 Package 元数据标记，不扫描应用控制流；普通 NuGet 冷消费和二进制传递引用仍需补实际验收。

## 9. 诊断和兼容性不能遗漏

- 当前 ATOMUILINK008 的跨程序集“使用包却未调用入口”检查依赖旧 usage 分析；删除分析后不会自动由 TypeMap 重现。
  正式设计必须选择轻量诊断或明确的启动/控件主题错误，不可同时宣称完全删除分析且所有诊断无变化。
- 不允许因为 TypeMap 发现某个类型而隐式执行可选包 initializer。
- 动态 root 仍应表达“保留控件及其注册依赖”，而不是只保留一个字符串键。
- 保留 Package Core 的执行次数与顺序；本次没有验证 ColorPicker、DataGrid、Extras 多包组合。
- 缺少语义代理、重复身份、不可解析资源 owner 必须定位为生成/契约错误，禁止运行时缺资源后重新 full 注册。

## 10. 原型文件与复现

隔离工作区改动：

- `GeneratedThemeSchemaWriter.TypeMapProbe.cs`：条件映射、候选键、代理和包内查询。
- `GeneratedThemeSchemaWriter.cs`：调用实验生成器、逐项语义 partial hook。
- `SemanticPartManifestWriter.cs`：实验的逐控件语义工厂。
- `AotTrimControlPackageRegistrationBuilder.cs`：语义收集与提交。
- `ThemeManagerBuilderExtensions.cs`：Desktop 实验入口。
- `Minimal` fixture：包 Group import 和本机 NativeAOT 链接配置。

真实宿主位于临时目录，引用隔离工作区源码。命令：

```bash
cd /private/tmp/atomui-typemap-real-host
dotnet publish Host.csproj -c Release -r osx-arm64 \
  -p:AtomUILinkedPublish=false -o out
./out/AtomUI.LinkedRegistration.Fixtures.TwoUnits
```

证据文件：

- `/tmp/atomui-typemap-real-baseline.json`：原 full 注册快照。
- `/tmp/atomui-typemap-real-native/result.json`：早期 Control-only 选择快照。
- `run.log`：未补 Token 条件时的 ListView 缺失失败。
- `run-token.log`：Token 条件补齐后的启动、模板通过及语义缺失。
- `run-semantic.log`：最终启动、模板、语义、主题切换通过。
- `obj/Release/net10.0/osx-arm64/native/*.map.xml`：最终 native 可达节点。

发布有本机 Homebrew 库最低 macOS 版本的链接警告；不能表述为完全无警告。
正式迁移前仍需完整的 targeted regression、NuGet/二进制消费、DataGrid/ColorPicker、资源覆盖、scoped token、动态 root 和平台验证。
本次不是体积基准：没有同口径重跑旧机制与 full 的三个 GUI 发布，不给出节省比例或性能改善结论。

## 11. 官方来源

- [TypeMap 设计与保留规则](https://github.com/dotnet/runtime/blob/v10.0.8/docs/design/features/typemap.md)
- [10.0.8 的 Mono 实现边界](https://github.com/dotnet/runtime/blob/v10.0.8/src/libraries/System.Private.CoreLib/src/System/Runtime/InteropServices/TypeMapping.cs)
- [跨程序集 Attribute 处理修复](https://github.com/dotnet/runtime/pull/123558)
- [ILLink TypeMapHandler](https://github.com/dotnet/runtime/blob/v10.0.8/src/tools/illink/src/linker/Linker/TypeMapHandler.cs)
- [ILLink 禁止直接消费 TypeMap Attributes 的说明](https://github.com/dotnet/runtime/blob/v10.0.8/src/tools/illink/src/linker/Linker.Steps/MarkStep.cs#L1735)
