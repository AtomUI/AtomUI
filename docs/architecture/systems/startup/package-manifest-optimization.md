# 启动清单去冗余方案

紧凑清单和镜像的直接文本表示已实现，压缩表示仍受独立放行条件约束。这是[控件注册生成与启动清单优化架构](../../foundations/control-registration-optimization.md)的清单与镜像协议。
启动清单是构建期依赖探测、裁剪及运行时包提交的共享输入。验收目标是减少重复存储和常驻对象，并保持裁剪选择、包注册、
资源查询、校验和运行时追加的既有行为。裁剪契约及放行依据见[AOT 与裁剪架构](../../foundations/aot-and-trimming.md)。
整个生成注册文件的体积分布、源码优化和各裁剪后端的风险见[控件注册生成物体积评估](../../foundations/generated-registration-size-review.md)。

## 1. 基线与范围

同一编解码和校验路径适用于所有使用启动清单契约的控件库，包括跨端共用包、桌面及可选控件包、移动端、第三方和商业控件库，
也覆盖空包、仅资产包、无控件 Token 的包，以及源码、传递引用和预编译 NuGet 消费。包名、控件名、资源规模和仓库归属均不参与算法分支。

`AtomUI.Desktop.Controls` 是体积样本。其二进制为 **456,763 B**，Base64 为 **609,020 字符**，两份独立 UTF-16 字符数据约
**2.32 MiB**。同一轮还读取了 Core、Shared、Common、Extras、Gallery、GalleryBase、ColorPicker 和 DataGrid 的生成清单。
九个样本的原文长度为 **5,552–609,020 字符**。将精确原文用 gzip 压缩再转为 Base64，各载荷为原长度的 **19.8%–50.5%**。
这组数据只比较已有产物的表示大小。内存、耗时及仓库外应用对重建控件包的实际消费仍须实测。

| 当前表示 | 消费方 | 本方案处理 |
| --- | --- | --- |
| 控件包的程序集清单特性 | 应用生成器读取预编译引用，校验契约并生成索引 | 保留读取入口，统一采用紧凑清单编码 |
| `PackageMetadata.Definition` 的包清单 | 运行时首次读取时解码，供独立启动和追加包使用 | 作为运行时完整数据来源，保留惰性缓存 |
| 应用启动镜像中的完整包清单副本 | 启动时与实际加载的包逐字符比对 | 改存无损校验载荷；应用自己的索引照常生成 |
| 清单中的重复名称、类型、程序集和资源键 | 包、控件、主题与资源校验及查询 | 通过包内字符串表共享文本，记录仍逐项保留 |

来源见[清单生成](../../../../src/AtomUI.Generator/Registration/StartupManifestWriter.cs)、[编解码与依赖校验](../../../../src/AtomUI.Toolchain/Common/Startup/StartupManifest.cs)、[应用镜像生成](../../../../src/AtomUI.Generator/Startup/StartupImageWriter.cs)和[运行时索引](../../../../src/AtomUI.Core/Registration/StartupPackageIndex.cs)。

## 2. 必须保留的契约

1. 保留每个包的 ID、程序集身份、控件身份和类型、所有 TypeMap 键、Token Schema 及指纹、Semantic PART、资源 URI、导出键、owner 及绑定。字符串相等只用于共享文本，不能合并字段、资源或控件记录。
2. 保留字段顺序、列表顺序和存在性。比如 `GlobalResourceTokens` 与资源键列表的 `null` 和空集合继续分别表示。保持基于部分程序集或类型名同名时的消歧规则。
3. 应用镜像继续保留控件 slot 顺序、资源 owner 索引、本地化和取值包注册入口。对已引用但未启用的可选包，保留原有资源归属诊断。
4. 提交前保持应用生成时的清单与实际加载包清单的完整相等判定，包括内容、长度和顺序。错误格式、不支持版本、缺失包、正文不符及既有契约错误都要明确失败。失败时保持原提交边界，候选注册和资源不进入已提交视图。
5. 无启动镜像、手写注册和运行时追加包继续走现有完整契约校验。追加仍保留已有 slot、资源、快照和主题。保留 `PackageMetadata.Definition.Register(...)` 与框架已有的裁剪、平台防护和启动线程边界。
6. 构建期用同一份完整的逻辑清单探测依赖。保持控件与资源的精确身份、每条保留条件、依赖边、选择顺序和诊断。应用引用、禁用和实际使用的边界继续独立。裁剪失败保持显式失败，禁止退回全量注册。

## 3. 应用镜像的无损校验证据

### 3.1 镜像成员

镜像由生成的静态 C# 构造，以下是完整逻辑结构及写入顺序。包定义是运行时模型的来源，镜像只用校验证据核对它，并提供应用侧索引。

| 顺序 | 成员与类型 | 构造规则 |
| --- | --- | --- |
| 1 | `ImageFormatId: UInt32` | 固定为 `0x314D4941`，独立于包清单格式标识 |
| 2 | `Stamp: UInt64` | 由完整清单正文与主题、语言及依赖输入的既定有序序列计算。使用现有[字段摘要算法](../../../../src/AtomUI.Generator/ThemeDefinitions/ThemeBuildContracts.cs#L263)的前 16 个十六进制字符。替换同一正文的传输表示不改变该值 |
| 3 | `PackageProofs: IReadOnlyList<PackageProof>` | 每个已链接包一条，按 `PackageId` 的 Ordinal 顺序生成。包 ID 重复时构建失败 |
| 4 | `ControlOrder: IReadOnlyList<string>` | 取 `HasTokenDescriptor=true` 的控件，按 `(Catalog, Id)` 的 Ordinal 顺序写入 `Catalog:Id`。运行时与已启用包相交后分配 slot |
| 5 | `ResourceOwners: IReadOnlyList<(string Key, string PackageId)>` | 由主题资产导出和带 Token 或语义描述符的控件类型键、身份键生成。按键的 Ordinal 顺序写入。同键同包归并，归属不同包时报错 |
| 6 | `VerifiedLanguages: IReadOnlyList<string>` | 经本地化链接验证的 BCP 47 标签，保持生成顺序 |
| 7 | `LanguageModules: IReadOnlyList<(string ModuleId, string EncodedData)>` | 经本地化链接校验的模块 ID 与原编码内容，沿用模块链接顺序 |
| 8 | `ControlValuePackages: IReadOnlyList<ControlValuePackage>` | 保留静态接线。元素包含 `Id`、`ControlAssembly`、具体 `LanguageTokenType` 与静态注册入口，顺序沿用应用生成器 |

资源键沿用 `s:<原字符串键>`、`t:<程序集简单名称>|<完整类型元数据名>` 和 `i:<Catalog>:<Id>`。运行时字符串 miss 的直接查询索引仍以去掉 `s:` 的原键查询。构建与提交继续校验资源归属。查看[镜像生成](../../../../src/AtomUI.Generator/Startup/StartupImageWriter.cs#L85)及[运行时索引](../../../../src/AtomUI.Core/Registration/StartupPackageIndex.cs#L213)。

### 3.2 包校验记录与读写算法

`PackageProof` 固定为 `(PackageId: string, ManifestFormatId: UInt32, OriginalAsciiLength: UInt32, Encoding: Byte, Payload: string)`。
长度单位是清单 Base64 原文的 ASCII 字节数。表示标签为 `0`（直接保存原文）或 `1`（保存单个 gzip member 的规范 Base64）。
压缩输入始终是清单的 **Base64 原文**，包括填充。每包按最终文本长度选择较小结果，等长选直接文本。标签 `1` 的 `Payload.Length`
必须小于 `OriginalAsciiLength`。gzip 写入使用一个无可选字段的 member，固定前十字节为 `1F 8B 08 00 00 00 00 00 00 FF`。
随后为 DEFLATE 数据和 RFC 1952 的小端 CRC32、`ISIZE`，压缩级别固定为 6。运行时验证整个帧。

1. 镜像构造先校验自身格式，随后按 `PackageId` 建查找表。运行时只为已启用包查证明；未启用包保留 owner 诊断。
2. 核对包 ID、清单格式和原文长度。直接文本做 Ordinal 比较。压缩载荷先验证标准 Base64 边界，再在固定大小工作缓冲中流式解压，按 ASCII 字节与实际包清单完整比对。
3. 压缩读取器必须报告压缩输入消耗量，并验证结束标记、CRC32、`ISIZE`、输入及输出长度。拒绝截断、额外 member 和尾随字节。仅凭流的 `Position` 或摘要不能作为接受依据。
4. 比对和逻辑校验成功后，包定义的解码数据才可交给启动索引。完整解压原文无需另行物化或缓存。损坏与不一致在提交前失败。

压缩标签只有在可验证单帧边界、实测内存收益和通过启动门槛后才启用。直接文本仍是同一镜像结构的一种载荷。无镜像和运行时追加继续走完整的包契约校验。

## 4. 包清单的二进制结构

### 4.1 基础编码与字段顺序

二进制封装为标准 Base64，使用填充且不插入空白。`ControlPackageManifest` 写入格式标记、`PackageId` 和 Base64，
`PackageMetadata.Definition` 使用同一份编码结果。外层标记和包 ID 分别与清单中的对应字段核对。

`U32`、`U64` 和有符号 `I32` 使用小端固定宽度。`Bool` 是单字节 `00` 或 `01`。`Var` 用最短的无符号 LEB128 编码，范围为
`UInt32`，最多五字节。以下每行的字段均按列出的顺序存储：

| 记录 | 依序写入 |
| --- | --- |
| 根 | `Magic: bytes[4]`（`53 49 55 41`）、`FormatId: U32`（`3`）、`SharedTexts: List<RawText>`、`Package`、文件结束 |
| `Package` | `Id: Text`、`AssemblyName: Text`、`SchemaFingerprint: U64`、`GlobalResourceTokens: Optional<List<Text>>`、`Controls: List<Control>`、`Assets: List<Asset>` |
| `Control` | `Catalog: Text`、`Id: Text`、`TypeName: Text`、`AssemblyName: Text`、`HasSemantic: Bool`、`HasTokenDescriptor: Bool`、`Keys: List<Text>`、`Assets: List<Text>`、`OwnTokens: List<OwnToken>`、`SemanticThemes: List<SemanticTheme>` |
| `OwnToken` | `Name: Text`、`ValueType: Text`、`KeyType: Text`、`KeyMember: Text`、`KeyAssembly: Text` |
| `Asset` | `Id: Text`、`Uri: Text`、`Phase: I32`、`Order: I32`、`ResourceKeys: Optional<List<Text>>`、`Owners: List<Text>`、`SemanticOwners: List<Text>`、`Exports: List<Text>`、`RequiredOwners: List<Owner>`、`SemanticBindings: List<SemanticBinding>` |
| `Owner` | `Identity: Text`、`AssemblyName: Text`、`TypeName: Text` |
| `SemanticTheme` | `Property: Text`、`TargetAssembly: Text`、`TargetType: Text` |
| `SemanticBinding` | `Owner`、`SemanticTheme` |
| `List<T>` | `Count: U32`，再连续写入 `Count` 个元素。空列表为零长度 |
| `Optional<T>` | `Present: Bool`；为 `00` 时终止，为 `01` 时继续写入 `T`。因此缺省资源键与已知空资源键有不同编码 |
| `RawText` | `ByteLength: Var`，再跟随同样长度的严格 UTF-8 字节 |
| `Text` | `Tag: Var`。最低位为 `0` 时，右移一位得到零基共享表索引，不再跟字节。最低位为 `1` 时，右移一位得到内联 UTF-8 字节数，后跟正文 |

文本必须非空且不能全是空白，保持 Ordinal 比较，UTF-8 严格解码，不做大小写或 Unicode 归一化。控件 `Identity` 由
`Catalog + ":" + Id` 派生。清单里原本是简单程序集名的字段继续写简单名，源模型中缺省的控件程序集与 Token 键程序集在写入前解析为包程序集。
完整 CLR 身份继续由 TypeMap 和后端绑定契约提供。资产 URI 必须为绝对地址，`Phase` 为 `0`（Shared）或 `1`（Control），
`Order` 沿用有符号整数和现有排序。现有校验继续核对 `Owners` 与 `RequiredOwners.Identity`，以及 `SemanticOwners`
与 `SemanticBindings.Owner.Identity` 的去重投影。各字段保留独立记录与顺序。参见[共享清单模型](../../../../src/AtomUI.Toolchain/Common/Startup/StartupManifest.cs#L7)与[字段归属](../../../../src/AtomUI.Generator/Registration/StartupManifestWriter.cs#L27)。

### 4.2 与实际裁剪边的对应关系

| 关联 | 必须核对的事实 |
| --- | --- |
| `Control.Keys[]` ↔ 官方 TypeMap | 每个键对应同一 Group 中的具体条件和候选代理。每条边可用 `(Group 完整身份, Key, 条件 CLR 类型, 代理 CLR 类型)` 对照产物。清单按现有条件顺序保留键，链接器仍由真实类型引用决定可达性 |
| `Control.Keys[]` ↔ .NET 8 条件记录 | 每个键关联其候选片段，对照组、片段 ID、条件类型与注册 thunk。保持现有条件记录协议和方法体约束 |
| `Asset.RequiredOwners` / `SemanticBindings` ↔ 资产工厂 | 按完整控件及目标类型核对 owner、语义属性和工厂引用。保留多目标资产的各有效依赖边和原去重身份 `Asset.Id` |
| `Package.Id` ↔ `PackageProof.PackageId` ↔ `Definition.Id` | 三方相等。证明检查正文，清单检查逻辑结构，实际资源与类型保留检查仍在各自后端和包索引 |

### 4.3 写入算法、读取边界与示例

写入端先按固定字段顺序统计文本的 UTF-8 字节长度、出现次数和首次出现位置。再按首次出现顺序考虑每个值。设字节数为 `L`，
出现次数为 `N`，下一可用表索引为 `i`，`varSize(x)` 是 `x` 的最短 LEB128 字节数。

```text
inlineBytes = N * (varSize((L << 1) | 1) + L)
sharedBytes = varSize(L) + L + N * varSize(i << 1)
当 sharedBytes < inlineBytes 时入表；等长时内联。
```

算法只比较字节成本，所有包使用同一规则。比如 `Button` 的 UTF-8 长度为 6，出现 3 次且表索引为 0，内联共 21 B，
入表为 7 B 正文和 3 B 引用，共 10 B。写入端做顺序遍历与按内容查表，无须对全表排序。索引宽度跨越边界时按实际编码长度计算。
该选择保证每个入表值相对内联有正收益。读取端每个共享表值解码一次，随后按索引复用同一个字符串引用。内联项照常解码。

逻辑列表各自最多 65,536 项。展开预算 `Lmax` 为 4 MiB，计算口径为 magic 与格式标记、定宽字段、逻辑列表计数，以及每一处文本按
`RawText` 原样展开的尺寸。每处有效文本至少占 2 B，每个索引最多 5 B，所以二进制输入上限可取
`Lmax + 4 + 5 * floor(Lmax / 2)`。Base64 上限由该二进制上限计算。共享表项数受实际剩余字节与展开预算约束，
不沿用逻辑记录列表的项数上限，也不把它缩窄为 16 位。

解码前校验入口长度和标准 Base64。预检验证格式、最短变长整数、旗标、计数、UTF-8、索引范围、唯一且有消费者的表项、展开预算与文件结束。
在边界确立后分配记录数组，再执行逻辑身份与依赖校验。未知标记、空白身份、截断、溢出和尾随数据均为错误。
包定义缓存解码后的不可变数据，查表不持有程序集间共用的字符串池。

可审计的最小编码示例是包 `P`、程序集 `A`、指纹为 0、`GlobalResourceTokens` 缺省、控件和资产列表为空。两个文本各出现一次，表为空。
下列 33 B 依次为头、零表项、两个内联文本、指纹、缺省标记、空控件和空资产：

```text
53 49 55 41  03 00 00 00  00 00 00 00
03 50  03 41
00 00 00 00 00 00 00 00
00
00 00 00 00  00 00 00 00
```

### 4.4 为什么选择这个结构

对九份现有生成清单按上述字节规则离线建模。候选实际生成的紧凑清单与模型的体积一致，逐包展开后逻辑记录流与冻结基线逐字节相同。
下表描述编码大小，启动分配和实际裁剪产物见[实施与验收记录](../../foundations/control-registration-optimization.md#6-实施与验收记录)。

| 样本 | 现有二进制 | 全部文本入表 | 按实际收益入表 |
| --- | ---: | ---: | ---: |
| Gallery | 4,166 B | 4,547 B | 4,158 B |
| Controls.Shared | 4,184 B | 4,556 B | 4,167 B |
| Common | 10,463 B | 8,488 B | 7,884 B |
| Core | 4,162 B | 4,545 B | 4,156 B |
| ColorPicker | 23,283 B | 12,631 B | 11,810 B |
| DataGrid | 28,720 B | 14,833 B | 13,984 B |
| Extras | 4,200 B | 4,564 B | 4,175 B |
| Desktop Controls | 456,763 B | 186,932 B | 180,330 B |
| GalleryBase | 21,229 B | 12,904 B | 12,159 B |
| 九份合计 | 557,170 B | 254,000 B | 242,823 B |

全部文本入表会让低重复样本变大，因为唯一值多出一个引用。按收益入表避免该成本，九份样本均缩小，合计少 **56.4%**。
Desktop Controls 的 **14,115** 次文本引用中有 **1,996** 个值值得共享，模型中需解码的文本从 **14,115** 处降到 **4,420** 处。
同一样本的紧凑清单 Base64 为 **240,440 字符**。对这一原文执行 gzip 再转 Base64 得 **95,968 字符**，这是镜像校验载荷的额外候选收益。
实际使用压缩要把解压 CPU、声明的内存边界和产物收益一起复测。压缩单独使用仍会在读取逻辑字段时重复解码相同名称，字符串表直接消除这部分重复。

## 5. 实施顺序和验证

先切换紧凑清单，再验证紧凑镜像。实施顺序和共享验收以[总体架构的阶段门槛](../../foundations/control-registration-optimization.md#4-实施边界与阶段门槛)为准。

| 验收项 | 输入及观察 |
| --- | --- |
| 通用性、逻辑等价与确定性 | 同一实现覆盖 Common、Desktop、可选包、Mobile、预编译第三方和商业包，另测空包、仅资产包、无 Token 控件及传递引用。逐字段核对控件、Token、资源、owner、顺序和同名类型消歧。用现有构建入口复验重复生成 |
| 损坏和错误契约 | 复核错包、单字节内容变更、已构建应用加载不同的包、损坏清单和不符格式。另测索引边界、截断、超额展开和压缩流尾部。失败不得发布部分资源 |
| 启用、查找与追加 | 覆盖有镜像启动、无镜像或手写注册、可选包未启用后追加及追加失败。验证原 slot、主题资源、未启用诊断、回滚和已发布对象保持既有行为 |
| 依赖探测与实际裁剪 | 逐条比对生成的控件、触发条件、TypeMap 键、资产工厂、引用边和诊断。对官方 TypeMap、Browser 精细裁剪和 .NET 8 条件注册，在现有真实发布入口比对保留与排除集合。保留直接使用、传递依赖及共享或语义资源，排除未使用控件和未启用包的控件工厂。验证选中工厂可运行、具有诊断的缺失路径仍能报错 |
| 平台与包消费 | 使用既有验证入口覆盖 Desktop JIT 与 macOS arm64 NativeAOT 真实启动，覆盖 Browser 裁剪后的启动和资源访问，并验证 .NET 8 预编译包经其受支持消费路径的产物。Mobile 与商业包接入复用同一编解码和验证准则，并以其真实包消费产物核对。缺少任何适用发布证据时，保持对应范围待验 |
| 内存、体积与启动 | 同一 Gallery 配置对比生成文件、清单传输、程序集和实际发布体积。用堆快照记录各阶段的 Base64、临时缓冲区和清单对象，观察提交后和追加后。采用[启动测量与验收线](verification.md)交错采样，确认减少常驻内存且启动门槛通过 |

优先复用既有验证。临时探针按项目规范清理，再执行最终受影响模块验证。逐字段对比、实际保留集合和真实启动分别出证据。
编译成功无法代替裁剪结果。基线与候选使用相同的应用入口和链接器配置，且不得通过增补裁剪根、全量回退或跳过校验取得通过。
详细原始数据与阶段验收集中登记，离线大小估算不计入启动或内存验收。
