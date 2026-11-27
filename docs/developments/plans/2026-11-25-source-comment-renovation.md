# 注释全面改造实施计划

2026-11-25 · 已确认执行，分支与 worktree 见决策 D5。基线和批次表已按 M0 产出的检查工具重算

依据 [AtomUI 生态注释书写规范](../../engineering/development/code-comment-guidelines.md)（v1.0，提交 `7d8602556`）。基线由检查工具 `scripts/verification/comments.py` 在 `release/6.0` `8222c246b` 上统计，与 `scripts/verification/comments-baseline.json` 一致。

## 1. 目标与完成条件

目标是让全仓源码注释符合规范 v1.0，并建立防回退门禁。改造只改注释，不改运行行为、公开签名和机器读取的标记。

以下条件全部满足才算完成：

1. 产品程序集对外可见的声明全部有英文 `<summary>` 或可解析的 `<inheritdoc />`。对外可见指声明和它所在的整条类型链都是 public 或 protected，与编译器 CS1591 的范围相同。豁免项见第 3 节。
2. 每条 `[SemanticPart]`、每个手写 Token、每种设计语言的入口和关键取值规则都有注释。
3. 机械违规清零：单行块标签、中文注释、引用仓库 `docs/`、注释写在特性之后、`param` 名与签名不一致。
4. internal 成员中承担跨模块契约的部分已甄别并补齐。
5. 注释检查接入 `test.py audit`，基线入库。pre-commit 已经是这条命令的 `--staged` 入口。
6. 每个批次的差异经工具证明只含注释，并通过所涉模块的聚焦验证。

## 2. 基线

待补声明的口径与编译器的 CS1591 相同：声明和它所在的整条类型链都是 public 或 protected 才计数，无参构造按决策 D1 豁免。各列含义见第 5 节开头的说明。

| 范围 | 待补声明 | 已有待复核 | 单行块 | 中文行 | Semantic Part 缺 | Token 缺 | 参数标签 | 引用文档 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| M1 Core、Controls.Shared | 1,944 | 520 | 600 | 777 | 0 | 0 | 23 | 1 |
| M2 AtomUI.Controls | 2,569 | 243 | 164 | 396 | 0 | 0 | 0 | 0 |
| M3 AtomUI.Desktop.Controls | 6,291 | 1,540 | 970 | 1,053 | 481 | 0 | 6 | 8 |
| M4 DataGrid、ColorPicker、Extras | 1,065 | 419 | 341 | 148 | 18 | 0 | 5 | 2 |
| M5 DesignLanguages 三套 | 261 | 256 | 675 | 99 | 0 | 18 | 20 | 14 |
| M6 GalleryBase、Native、Localization、Generator、Toolchain | 1,162 | 49 | 132 | 449 | 0 | 45 | 7 | 0 |
| 合计 | 13,292 | 3,027 | 2,882 | 2,922 | 499 | 63 | 61 | 25 |

- 待补声明比规划初稿的 17,873 条少 4,581 条。初稿用正则估算，把内部类型里的 public 成员、`private protected` 成员和无参构造都算了进去。Native 整个程序集没有对外可见的类型，待补数是 0。
- 注释写在特性列表之后的违规目前为 0。
- `tests/` 有 163 行单行块和 57 行中文注释，`controlgallery/` 有 38 行单行块和 163 行中文注释。这两处只做机械整改，归 M7。
- 已确认引用仓库文档的注释 25 行：`PopupUtils.cs` 第 666 行、`DwmVsyncRenderTimer.cs` 第 15 行、`Drawer.SemanticParts.cs` 第 8 行这类写法，随所在批次改写为自包含说明。
- 与规范冲突的文档示例已在 M0-1 修正。

## 3. 范围与默认决策

纳入范围：

- 第 2 节的 13,292 条待补声明，以及 3,027 条已有注释的复核。
- Semantic Part、Token、设计语言、AXAML 和实现注释。
- internal 成员中的跨模块契约。
- `tests/`、`controlgallery/`、脚本的机械规则。

不纳入范围：

- 956 个已标记的自动生成文件，以及 `GeneratedIcons` 下 844 个图标类文件。
- 任何行为、签名和可见性改动。
- 旧工程的 tsv/xlf 导出和 ATOMUIAGENT 分析器。新规范没有要求，外部包依赖当初就是合入卡点。
- 诊断器读取的 AXAML 标记。

| ID | 决策 | 默认 | 状态 |
| --- | --- | --- | --- |
| D1 | 无参公共构造是否必须写注释 | 不强制。「Initializes a new instance」没有信息量，违反规范「每句话都应有用」。产品范围内共 310 条，由工具规则 R6 豁免 | 待确认 |
| D2 | 图标类 | `GeneratedIcons` 下 844 个文件按生成代码处理；`AtomUI.Icons.AntDesign` 根目录的手写文件纳入 N-04 | 默认执行 |
| D3 | internal 成员 | 只写承担跨模块契约的成员。各批内顺手甄别，M8-1 统一清点 | 默认执行 |
| D4 | tests、Gallery、脚本 | 只套机械规则，不做 XML 覆盖 | 默认执行 |
| D5 | 工作位置 | 分支 `feature/comment`，worktree `/Users/chinboy/Projects/dotnet/AtomUIV6-comment`。主工作区保持在 `release/6.0`。里程碑内串行 | 已授权，2026-11-25 |
| D6 | 旧提交 `72579ba30` | 授权后打本地 tag 并当素材库；不授权则不依赖 | 待授权 |
| D7 | 原生互操作与诊断描述符 | Native 没有对外可见的声明，它的注释归 internal 跨模块契约，随 M8-1 处理。Generator 的诊断 ID 和描述符按规范写契约。不借机改可见性 | 默认执行 |
| D8 | 是否随 NuGet 包发布 XML 文档 | 当前没有项目生成 XML 文档，包里也没有，注释只有读源码的人看得到。建议收口时打开，见 M8-6。它会改变打包产物，默认不做 | 待确认 |

## 4. 执行方式

### 4.1 位置与节奏

- 在 worktree `/Users/chinboy/Projects/dotnet/AtomUIV6-comment`（分支 `feature/comment`）里做，主工作区保持在 `release/6.0`。
- 里程碑内按表内顺序串行。同一工作区共用构建输出，并行写入需要另行安排并让验证排队。
- 每批一个 commit，得到负责人明确指令后才提交。提交前按规范第 8 节审查暂存注释，并通过 `test.py audit --staged`。

### 4.2 每批的固定动作

1. 取清单。运行 `python3 scripts/verification/comments.py list --path <批次目录>`，得到待补声明、缺注释的 Token 和 Semantic Part、单行块和中文注释的位置。
2. 核实并书写。逐条读声明、用法、模板或测试后写英文契约。样式属性写清设置位置、生效条件、特殊值和回退。`Try*`、事件、异步方法和可重写成员按规范第 3 节的表。
3. 同批顺手项。展开单行块标签；中文改英文；删除过期注释和注释掉的旧代码；引用 `docs/` 的内容改写为自包含说明；TODO 和 FIXME 补全适用条件与移除条件，失效的删除。
4. 自检。用注释审查技能对照规范第 7 节逐项核对，再用 `comments.py diff-check --base HEAD` 证明差异只含注释。
5. 验证。构建所涉项目，并临时打开 XML 文档生成，确认批内文件没有 CS1570–CS1574、CS1587、CS1591 类诊断。按[按改动影响选择验证](../../engineering/workflows/affected-verification.md)对改动路径跑聚焦验证。
6. 交付审查并更新台账，得到提交指令后提交。

Semantic Part 的专用 Style 名由 `list` 按生成器的同一规则算出（控件类型名 + Path 转 PascalCase + `Style`），不手写。

### 4.3 验收点

| 验收点 | 判据 | 取证 |
| --- | --- | --- |
| A1 覆盖 | 批内对外可见声明的缺注释数为 0，豁免项除外 | `comments.py list --path` 没有条目 |
| A2 机械规则 | 批内 R1–R5、R7、R8 的违规数为 0 | `comments.py check --path` 退出 0 |
| A3 XML 有效 | XML 良构，`cref` 可解析，`param` 名与签名一致 | 打开文档生成的构建输出，批内文件 0 诊断 |
| A4 行为不变 | 去掉注释后新旧源码逐行一致 | `comments.py diff-check` 退出 0 |
| A5 模块验证 | 改动路径的聚焦验证通过 | `test.py run --scope iterate --path ...` 的结果 |
| A6 审查 | 校准批逐条审读，其余批随机抽 15 条对照实现 | 台账记录抽检条目与结论 |

注释改造不引入行为变化，不新增临时测试。A4 的差异证明就是行为证据。一个程序集的待补数清零后，`check` 对它改为零容忍，不再按基线比较。没有 `--path` 的 `check` 比较基线，任一范围的任一规则高于基线就失败；暂存文件新增的违规由 `test.py audit --staged` 拦截。

### 4.4 校准批

每个里程碑的第一批是校准批：C-01、L-01、D-01、G-01、T-01、N-01。负责人逐条审读校准批，确认措辞和详细程度后，后续批次按抽检审查。校准中定下的口径写回附录的措辞模板。

## 5. 任务总表

批次表的列含义，除「文件」和「TODO」外都由检查工具统计：

- 待补声明：没有 XML 注释的对外可见声明数，口径同 CS1591，不含无参构造。
- 已有待复核：已有 XML 注释、需要核对准确性和排版的对外可见声明数。
- 单行块：块级 XML 标签与正文写在同一行的注释行数。
- 中文行：含中文的注释行数，含 `//`、`///` 和 AXAML。
- Token 缺：没有 XML 注释的 Token 定义数。
- Semantic Part 缺：上方没有注释的 `[SemanticPart]` 数。
- 参数标签：`<param>`、`<typeparam>` 与签名不一致，或只覆盖了部分参数的声明数。
- 引用文档：引用仓库 `docs/` 路径或 `.md` 文件的注释行数。
- TODO：注释中出现的 TODO 和 FIXME 数。

契约批次的共同项：需求来源是规范第 3、4、5、6 节；前置是 M0-2 和 M0-3 产出的工具与基线；验收点是 A1–A6。

### 5.0 M0 规范对齐与门禁

| ID | 交付 | 前置 | 验收点与判据 | 边界与待确认 |
| --- | --- | --- | --- | --- |
| M0-1 | 修正与规范冲突的示例：5 份正式文档里 12 处单行 Token 示例改为逐行展开；`.github/copilot-instructions.md` 的中文旧 Token 示例改为英文并指向规范 | — | 正式文档里不再有与规范冲突的示例；差异只含文档 | `docs/developments/specs` 是历史记录，不改。范围比初稿的 2 份文档大，全仓检索又找到 4 份 |
| M0-2 | 注释检查工具 `scripts/verification/comments.py`，含 `list`、`check`、`baseline`、`diff-check`，实现下表的规则和豁免清单 | — | R6 清单与编译器 CS1591 清单逐条比对，差异都有具名原因；整仓 `check` 在 10 秒内完成；工具自测进入 `test.py self-test` | 正则实现的已知差异记录在本节末尾 |
| M0-3 | 基线文件 `scripts/verification/comments-baseline.json` | M0-2 | 未改动的树上 `check` 退出 0；各里程碑合计与第 2 节一致 | — |
| M0-4 | 注释检查接入 `test.py audit`，`--staged` 同时覆盖 pre-commit | M0-2、M0-3 | 暂存一个无注释的新 public 成员时 `audit --staged` 退出 2 并给出位置；只暂存注释整改时通过；完整 `audit` 在当前树上通过 | 不改 hook 配置；对既有违规只判断「不许增加」 |
| M0-5 | 措辞模板定稿 | C-01 的审读结论 | 模板覆盖注册字段、附加属性访问器、`Try*`、事件、可重写成员、Semantic Part，并写回本计划附录 | 模板不进入规范正文 |
| M0-6 | 旧提交保护与素材提取 | 负责人授权（D6） | 本地 tag 指向 `72579ba30`；脚本输出「类型 + 成员 + 参数」的匹配表 | 未授权则跳过。素材只作草稿，逐条对照当前代码后使用 |

M0-2 实现的规则：

| 规则 | 内容 | 规范条款 |
| --- | --- | --- |
| R1 | 块级 XML 标签与正文写在同一行 | 第 2 节 |
| R2 | 注释含中文，覆盖 `.cs` 的 `//` 和 `///`、`.axaml` 的 `<!-- -->`，机器标记除外 | 第 1 节第 5 条 |
| R3 | 注释引用仓库 `docs/` 路径或仓库内的 `.md` 文件 | 第 1 节第 3 条 |
| R4 | `[SemanticPart]` 上方没有注释，并给出按生成器规则推算的专用 Style 名 | 第 5.3 节 |
| R5 | 没有注释的 Token 定义，不论可见性 | 第 5.1 节 |
| R6 | 对外可见的声明没有 XML 注释，范围同编译器的 CS1591 | 第 3 节 |
| R7 | XML 注释写在特性列表之后 | 第 2 节 |
| R8 | `<param>`、`<typeparam>` 的名称与签名不一致，或同类参数只覆盖了一部分 | 第 2 节 |

豁免清单：自动生成文件、`GeneratedIcons`、无参公共构造（D1）。

M0-2 的校准结论（2026-11-25，`8222c246b`）：临时打开 XML 文档生成和 CS1591 构建取得编译器清单，与工具的 R6 清单按「文件 + 成员名」比对。

| 项目 | 编译器 CS1591 | 工具 R6 | 差异的构成 |
| --- | --- | --- | --- |
| Core | 1,314 | 1,252 | 无参构造 5；未注释的 record 的主构造与位置参数 55；已注释类的主构造 2；报在主文件 1 |
| Controls.Shared | 726 | 692 | 无参构造 5；record 的主构造与位置参数 28；已注释类的主构造 1 |
| Controls | 2,610 | 2,569 | 无参构造 30；record 的主构造与位置参数 12；已注释类的主构造 1；报在主文件 2 |
| Desktop.Controls | 6,455 | 6,291 | 无参构造 123；record 的主构造与位置参数 41；报在主文件 5 |

没有无法解释的差异。下面四条编译器行为决定了工具的口径，由一个独立探针工程实测确认：

1. 文档注释与声明之间隔着空行，注释仍然关联到声明。
2. 分部类型只要有一个部分带注释，整个类型算已注释。工具把缺注释的类型报在以类型名命名的文件上，编译器报在编译顺序里第一个缺注释的部分。
3. 注释写在特性列表之后不关联：编译器报 CS1587，成员照报 CS1591。
4. `record` 的主构造和位置参数随类型注释。普通类的主构造不随：类有注释时，编译器仍单独报它的主构造。

第 4 条影响 M8-3 和 M8-6：用 CS1591 复核或打开文档生成时，带主构造的普通类会留下无法用注释消除的警告，需要逐处决定用 `#pragma` 还是调整写法。

### 5.1 M1 Core 与 Controls.Shared

共 9 批，待补 1,944 条。Theme 放在里程碑末尾，因为它改动最频繁、中文最多。

| ID | 范围 | 文件 | 待补声明 | 已有待复核 | 单行块 | 中文行 | Token 缺 | Semantic Part 缺 | 参数标签 | 引用文档 | TODO |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| C-01 | Animations（校准批） | 38 | 235 | 2 | 12 | 4 | 0 | 0 | 0 | 0 | 0 |
| C-02 | Core 根目录、Utils、Media、Collections 及其余 | 51 | 252 | 43 | 133 | 70 | 0 | 0 | 1 | 1 | 2 |
| C-03 | Core 的 Controls、Data、Registration、Exceptions、Input、Localization、Reflection、Startup | 65 | 251 | 20 | 45 | 49 | 0 | 0 | 0 | 0 | 0 |
| S-01 | Shared 的 Data、Dates、EventArgs、Converters | 44 | 208 | 3 | 180 | 1 | 0 | 0 | 0 | 0 | 30 |
| S-02 | Shared 的 ImageLoading、AsyncLoad、MediaQuery | 72 | 297 | 2 | 0 | 8 | 0 | 0 | 0 | 0 | 0 |
| S-03 | Shared 根目录、Net、Utils、WaveSpirit 及其余 | 51 | 187 | 34 | 13 | 47 | 0 | 0 | 0 | 0 | 0 |
| C-04 | Core 的 Theme 根目录、Theme/Definitions、Theme/Schema | 86 | 279 | 73 | 117 | 53 | 0 | 0 | 22 | 0 | 0 |
| C-05 | Core 的 Theme/Compilation、Configuration、Languages、SemanticParts、Resources 及其余 | 68 | 134 | 67 | 92 | 61 | 0 | 0 | 0 | 0 | 0 |
| C-06 | Core 的 Theme/DesignTokens、Theme/Derivation | 34 | 101 | 276 | 8 | 484 | 0 | 0 | 0 | 0 | 1 |

- C-01 是整个工程的第一个契约批，用来验证第 4.2 节的流程跑得通。
- C-06 以翻译和复核为主：484 行中文，276 条已有注释逐条核对。
- Core 的 Theme 目录有 992 条 internal 声明，是跨模块契约最集中的地方。M8-1 的甄别方法先在 C-04 上试一次。
- S-01 含 30 处 TODO，本批内处理。

### 5.2 M2 AtomUI.Controls

共 8 批，待补 2,569 条。抽象基类多，`<inheritdoc />` 与「差异处写清」的判断多。类型说明写在 `partial` 主声明上。

| ID | 范围 | 文件 | 待补声明 | 已有待复核 | 单行块 | 中文行 | Token 缺 | Semantic Part 缺 | 参数标签 | 引用文档 | TODO |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| L-01 | Buttons、FloatButton、OptionButtonGroup（校准批） | 22 | 343 | 2 | 5 | 30 | 0 | 0 | 0 | 0 | 0 |
| L-02 | Input、CheckBox、RadioButton、Switch、Slider、Segmented | 34 | 315 | 24 | 11 | 52 | 0 | 0 | 0 | 0 | 0 |
| L-03 | Grid、FlexPanel、Space、List、Separator、ScrollViewer、Embedding、ItemsControl | 38 | 272 | 85 | 29 | 16 | 0 | 0 | 0 | 0 | 0 |
| L-04 | ProgressBar、Skeleton、Spin、Splash、Result、Empty | 42 | 411 | 41 | 53 | 94 | 0 | 0 | 0 | 0 | 0 |
| L-05 | Tag、Badge、Avatar、Card、Carousel | 30 | 251 | 21 | 24 | 44 | 0 | 0 | 0 | 0 | 0 |
| L-06 | Timeline、Rate、Watermark、QRCode | 22 | 202 | 2 | 0 | 27 | 0 | 0 | 0 | 0 | 0 |
| L-07 | Form、Cascader、Collapse、TabControl | 38 | 418 | 8 | 0 | 14 | 0 | 0 | 0 | 0 | 0 |
| L-08 | Primitives、ImageLoading、ImagePreview、Icon、Ellipsis、MarqueeLabel、Motion、Feedback、MediaQuery、Localization、Steps、Upload、Select、根目录及其余 | 65 | 357 | 60 | 42 | 119 | 0 | 0 | 0 | 0 | 1 |

### 5.3 M3 AtomUI.Desktop.Controls

共 25 批，待补 6,291 条，是主体。每批是一个或一组控件目录的全部文件，含 `*Token.cs`、`*.SemanticParts.cs` 和 `Themes/` 下的 AXAML。

| ID | 范围 | 文件 | 待补声明 | 已有待复核 | 单行块 | 中文行 | Token 缺 | Semantic Part 缺 | 参数标签 | 引用文档 | TODO |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| D-01 | Buttons、ButtonSpinner、SplitButton、DropdownButton、FloatButton（校准批） | 65 | 315 | 119 | 105 | 37 | 0 | 24 | 0 | 0 | 0 |
| D-02 | Primitives、根目录及其余 | 63 | 181 | 16 | 24 | 21 | 0 | 0 | 0 | 1 | 0 |
| D-03 | Space、Splitter、SplitView、Expander、GroupBox、Separator、AdornerLayer | 46 | 212 | 77 | 66 | 23 | 0 | 10 | 0 | 0 | 0 |
| D-04 | Menu、Flyouts | 47 | 287 | 74 | 43 | 95 | 0 | 14 | 0 | 0 | 0 |
| D-05 | NavMenu、Breadcrumb | 59 | 240 | 92 | 45 | 69 | 0 | 12 | 0 | 0 | 1 |
| D-06 | TabControl | 56 | 264 | 52 | 36 | 29 | 0 | 17 | 0 | 0 | 2 |
| D-07 | Pagination、Steps | 39 | 151 | 111 | 66 | 14 | 0 | 11 | 0 | 0 | 0 |
| D-08 | Popup、PopupConfirm、Tooltip、Tour | 50 | 336 | 39 | 30 | 127 | 0 | 20 | 6 | 1 | 0 |
| D-09 | Input、OtpLineEdit、Mentions、NumericUpDown | 75 | 361 | 29 | 23 | 32 | 0 | 28 | 0 | 0 | 0 |
| D-10 | Select | 39 | 208 | 40 | 23 | 35 | 0 | 12 | 0 | 0 | 0 |
| D-11 | AutoComplete、ComboBox | 45 | 277 | 27 | 15 | 28 | 0 | 35 | 0 | 1 | 0 |
| D-12 | Cascader、TreeSelect | 49 | 276 | 27 | 13 | 19 | 0 | 24 | 0 | 0 | 0 |
| D-13 | Transfer、ListBox | 48 | 287 | 22 | 21 | 0 | 0 | 49 | 0 | 0 | 0 |
| D-14 | DatePicker、TimePicker | 42 | 185 | 25 | 28 | 15 | 0 | 44 | 0 | 0 | 0 |
| D-15 | DateViewer | 30 | 115 | 30 | 33 | 0 | 0 | 11 | 0 | 0 | 0 |
| D-16 | Slider、Rate、Switch、Segmented、CheckBox、RadioButton、OptionButtonGroup、Skeleton、ProgressBar、Spin、Result、Empty、QRCode、Badge、Avatar、Tag、Statistic、Timeline | 181 | 320 | 229 | 144 | 76 | 0 | 75 | 0 | 0 | 0 |
| D-17 | Form、TextBlock、MarqueeLabel、ScrollViewer | 39 | 281 | 42 | 11 | 12 | 0 | 5 | 0 | 0 | 0 |
| D-18 | Upload | 64 | 228 | 13 | 12 | 1 | 0 | 2 | 0 | 0 | 0 |
| D-19 | TreeView | 47 | 298 | 20 | 13 | 12 | 0 | 6 | 0 | 0 | 0 |
| D-20 | ListView、Masonry、Card、Collapse | 57 | 333 | 96 | 39 | 60 | 0 | 17 | 0 | 0 | 1 |
| D-21 | Calendar、Carousel、Descriptions | 75 | 274 | 83 | 59 | 88 | 0 | 15 | 0 | 0 | 0 |
| D-22 | Dialog、MessageBox | 55 | 319 | 60 | 12 | 61 | 0 | 14 | 0 | 3 | 0 |
| D-23 | Drawer、Message、Notifications、Alert、BorderBeam | 51 | 263 | 151 | 59 | 84 | 0 | 22 | 0 | 1 | 0 |
| D-24 | ImagePreviewer | 36 | 127 | 17 | 11 | 63 | 0 | 14 | 0 | 1 | 0 |
| D-25 | Window、WindowTitleBar | 64 | 153 | 49 | 39 | 52 | 0 | 0 | 0 | 0 | 1 |

- D-01 是控件类批次的校准批，用来定三类写法：样式属性契约、Token 说明展开、Semantic Part 注释。
- 481 条 Semantic Part 缺注释全部在本里程碑。
- D-16 由 18 个小控件目录合并而成，文件多但单个控件都小。

### 5.4 M4 DataGrid、ColorPicker、Extras

共 6 批，待补 1,065 条。

| ID | 范围 | 文件 | 待补声明 | 已有待复核 | 单行块 | 中文行 | Token 缺 | Semantic Part 缺 | 参数标签 | 引用文档 | TODO |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| G-01 | DataGrid 根目录、Utils、Cell、Row、Themes、Localization（校准批） | 70 | 268 | 258 | 202 | 100 | 0 | 10 | 1 | 1 | 30 |
| G-02 | DataGrid 的 Column、EventArgs | 61 | 188 | 118 | 76 | 31 | 0 | 0 | 4 | 1 | 13 |
| G-03 | DataGrid 的 Data 其余目录、Data/Selection | 12 | 234 | 9 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| G-04 | DataGrid 的 Data/Query、Data/Virtualization | 11 | 156 | 0 | 0 | 7 | 0 | 0 | 0 | 0 | 0 |
| G-05 | ColorPicker 根目录、Utils、Localization、Themes | 57 | 153 | 33 | 55 | 7 | 0 | 8 | 0 | 0 | 0 |
| G-06 | ColorSlider、ColorView、Extras 全部 | 19 | 66 | 1 | 8 | 3 | 0 | 0 | 0 | 0 | 1 |

DataGrid 的 43 处 TODO 集中在 G-01 和 G-02，两批内处理。

### 5.5 M5 DesignLanguages

共 3 批，待补 261 条，另有 256 条已有注释待复核、675 行单行块。三种语言结构平行，按 Ant Design、PrimeNG、Shadcn 的顺序做。

| ID | 范围 | 文件 | 待补声明 | 已有待复核 | 单行块 | 中文行 | Token 缺 | Semantic Part 缺 | 参数标签 | 引用文档 | TODO |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| T-01 | AntDesign 的 5 个项目（校准批） | 164 | 55 | 45 | 157 | 33 | 0 | 0 | 0 | 0 | 2 |
| T-02 | PrimeNG 的 5 个项目 | 204 | 112 | 85 | 259 | 33 | 1 | 0 | 20 | 14 | 2 |
| T-03 | Shadcn 的 5 个项目 | 173 | 94 | 126 | 259 | 33 | 17 | 0 | 0 | 0 | 2 |

- 按规范第 5.2 节写清每种语言的入口和关键取值规则。语言特有的映射、刻意保留的差异和有条件的回退，在对应分支或赋值组旁写理由。
- 现有注释里对外部来源的引用（如上游预设名、规范站点）是取值依据，不属于规范禁止的仓库 `docs/` 引用，保留。
- T-03 完成后做一次三语言对照，确认同名 Token 和规则在三种语言里的说明口径一致。

### 5.6 M6 其他程序集

共 6 批，待补 1,162 条。各批写入范围互不重叠。

| ID | 范围 | 文件 | 待补声明 | 已有待复核 | 单行块 | 中文行 | Token 缺 | Semantic Part 缺 | 参数标签 | 引用文档 | TODO |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| N-01 | GalleryBase 的 Controls、TokenRules、SourceCode（校准批） | 76 | 294 | 13 | 13 | 142 | 45 | 0 | 0 | 0 | 0 |
| N-02 | GalleryBase 其余目录 | 25 | 208 | 10 | 5 | 0 | 0 | 0 | 0 | 0 | 0 |
| N-03 | Native 全部 | 39 | 0 | 0 | 56 | 242 | 0 | 0 | 4 | 0 | 0 |
| N-04 | Localization 全部，Icons.Shared，Icons.AntDesign 根目录手写文件，Fonts 两个包 | 52 | 191 | 1 | 3 | 24 | 0 | 0 | 0 | 0 | 0 |
| N-05 | Generator 全部 | 106 | 27 | 7 | 27 | 41 | 0 | 0 | 3 | 0 | 0 |
| N-06 | Toolchain 全部，含 build/ 下编入它的源码 | 92 | 442 | 18 | 28 | 0 | 0 | 0 | 0 | 0 | 0 |

- N-01 的 45 个 Token 全部没有注释，按规范第 5.1 节逐个写。
- N-03 没有对外可见的声明，待补数为 0。本批只有 242 行中文和 56 行单行块；它的注释归 M8-1。
- N-05 含诊断 ID 和诊断描述符的 public 字段，逐条写清诊断含义和处理方式。
- `AtomUI.Toolkits.GalleryBase.Generator` 嵌入的示例源码并入 N-05，按生成代码的源声明处理。

### 5.7 M7 tests、Gallery、脚本与 AXAML

只套机械规则，前置是 M0-2。只依赖 M0，可以在 M1 之后的任何时点执行，默认排在 M6 之后。

| ID | 交付 | 验收点与判据 |
| --- | --- | --- |
| M7-1 | `tests/` 机械整改：163 行单行块、57 行中文。测试注释只在需要解释输入、顺序或边界断言时保留 | A2 在 `tests/` 为 0；A4 |
| M7-2 | `controlgallery/` 机械整改：38 行单行块、163 行中文，含 AXAML 注释 | A2 在 `controlgallery/` 为 0；A4 |
| M7-3 | 契约批次没有覆盖到的 `src/` AXAML 注释。机器标记逐处比对不变 | A2；机器标记数量和内容与基线一致 |
| M7-4 | `scripts/`、`build/` 下 `.py`、`.sh`、`.ps1`、`.props`、`.targets` 的注释。先把 R2 扩展到这些文件类型并量出现状 | 现状数字记入台账；A2 为 0 |

### 5.8 M8 收口

| ID | 交付 | 前置 | 验收点与判据 |
| --- | --- | --- | --- |
| M8-1 | internal 跨模块契约清点与补齐 | M1–M6 | 清单有生成方法和证据；清单内成员缺注释数为 0 |
| M8-2 | TODO 和 FIXME 收尾：各批未处理的剩余项补全适用条件和移除条件，或删除 | M1–M6 | 剩余项均写明条件，或已删除 |
| M8-3 | 全仓复核：`check` 各规则为 0；逐程序集打开 CS1591 构建复核 | M1–M7 | 基线清零；CS1591 诊断数为 0，无参构造与已注释的 record 除外；带主构造的普通类留下的警告逐处处置并记录 |
| M8-4 | 文档收尾：changelog；[测试价值与生命周期](../../engineering/development/test-value-and-lifecycle.md)的本地提交检查段补注释检查入口；AGENTS.md 的提交审查条款指向工具 | M8-3 | 文档与实际命令一致 |
| M8-5 | 清理临时产物，台账收口 | 全部 | 没有残留的取证构建输出和临时脚本；台账状态为完成 |
| M8-6 | 打开产品项目的 XML 文档生成，让注释随 NuGet 包发布 | M8-3，以及 D8 的授权 | 包内含 `.xml`；打包布局检查通过或已登记；使用者的 IntelliSense 能看到 summary |

M8-1 建议的清点方法：临时移除某个程序集的 `InternalsVisibleTo` 声明后编译它的消费方，收集 CS0122 和 CS0246 诊断，得到被其他程序集实际使用的 internal 成员清单。方法先在 Core 到 Controls.Shared 这一对上验证可行性，可行再推广；不可行则退回逐目录人工甄别。

M8-6 会改变打包产物，不属于「只改注释」的范围，默认不做。

## 6. 依赖与顺序

```text
M0 → M1 → M2 → M3 → M4 → M5 → M6 → M7 → M8
```

- M0 产出工具和基线，所有后续批次都消费它。M0-6 可选，不在关键路径上。
- M1 到 M3 按 `cref` 的依赖方向排：Controls 和 Desktop.Controls 的注释引用 Core 和 Shared 的类型。
- M4 的 DataGrid 和 ColorPicker 建立在 Controls 和 Desktop.Controls 的基类之上。
- M5 的设计语言规则引用各控件的 Token，放在控件注释之后。
- M6 的程序集互相独立，只依赖 M0。
- M7 只依赖 M0，可以提前。
- M8-1 到 M8-3 需要前面的里程碑完成。

## 7. 风险与应对

| 风险 | 应对 |
| --- | --- |
| 主线并行开发与批次冲突 | 批次限定在一个控件目录或一个程序集内，只动注释。冲突时以对方代码为准重写注释 |
| 注释与实现不符 | 每条对照声明、用法和模板后书写；校准批逐条审读；A3 保证引用可解析 |
| 检查工具误判 | M0-2 用编译器清单校准，差异逐条记录；工具只做「不许增加」的棘轮 |
| Semantic Part 注释写错 Style 名 | 名字由 `list` 按生成器的规则算出，不手写 |
| 主线 API 持续变化造成返工 | 改动最频繁的 Theme 和设计语言排在各自里程碑末尾；每个里程碑结束重算基线 |
| 旧提交被回收 | M0-6 需要授权；不授权则不依赖素材库 |
| 工作量超预期 | 以里程碑为停点，按重算结果调整后续批次 |

## 8. 工作量

契约主体是 13,292 条待补加 3,027 条复核，共 57 批，平均每批约 230 条。按旧工程的实际节奏（每天 600–1,000 条，含评审）粗估：

- 契约主体：15–22 个工作日。
- M0：1–2 天。
- M7：1–2 天。
- M8：2–3 天，取决于 M8-1 甄别出的 internal 数量。

这是粗估。每个里程碑结束时用工具重算，偏差超过两成就调整后续批次。

## 9. 台账

进度记录放在 `docs/developments/progress/2026-11-25-source-comment-renovation.md`，M0-1 开工时创建，本文件的任务表原样登记为初始状态。

- 状态取值：未开始、进行中、待审、完成、阻塞。
- 每批一行。证据列写命令与结果，或提交号；没有证据不得标完成。
- 偏离本计划的决定写进台账的裁定记录。上下文丢失后以台账为准继续。

## 附录：措辞模板（2026-11-27 定稿）

负责人于 2026-11-27 审查通过全部改造，模板以已经落库的注释为准。下面的示例逐字取自源码，括号里是出处；写新注释时照这个措辞和详细程度写。示例给的是写法，某条声明写什么仍以它自己的代码为准，不照搬句意。本模板不进入规范正文。

通用的排版习惯：块级标签各占一行，`<param>` 写成开头、正文、结尾三行；空值写 `<see langword="null" />`；引用继承来的成员写「声明类型.成员」，简单名编译器解析不了。

### 注册字段

注册字段只标识对应的属性，一句话，完整说明写在 CLR 包装属性上。路由事件字段同理，只写「… routed event.」。

出处：`src/AtomUI.Core/Theme/ThemeConfigProvider.cs`。

```csharp
    /// <summary>
    /// Identifies the <see cref="Config" /> property.
    /// </summary>
    public static readonly StyledProperty<ThemeConfig?> ConfigProperty =
        AvaloniaProperty.Register<ThemeConfigProvider, ThemeConfig?>(nameof(Config));

    /// <summary>
    /// The theme configuration of this scope: the axes it fixes for its content and the Token values it sets.
    /// Axes left unset are inherited from the parent scope. <see langword="null" /> uses the default configuration,
    /// the one built by an empty <c>ThemeConfigBuilder</c>.
    /// </summary>
    /// <remarks>
    /// The value is read when the scope attaches. A change while the scope is attached makes the theme manager
    /// replace the scope's configuration at once; a change before that takes effect when the scope attaches.
    /// </remarks>
    public ThemeConfig? Config
```

### 附加属性访问器

摘要写「Reads …」或「Sets … for an element」；`<param name="element">` 写承载值的元素；取值方法的 `<returns>` 写清未设置时返回什么。

出处：`src/AtomUI.Desktop.Controls/Tooltip/OverflowTip.cs`。

```csharp
    /// <summary>
    /// Reads whether the overflow tip is switched on for an element.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <returns>
    /// The value on the element, or <see langword="false" /> when the property is not set on it.
    /// </returns>
    public static bool GetIsEnabled(Control element)

    /// <summary>
    /// Switches the overflow tip for an element on or off.
    /// </summary>
    /// <param name="element">
    /// The element whose text may be clipped.
    /// </param>
    /// <param name="value">
    /// Whether the element is watched for clipped text.
    /// </param>
    public static void SetIsEnabled(Control element, bool value)
```

### `Try*` 方法

摘要写它尝试做什么；`<returns>` 先写成功条件，再写失败条件，失败条件列全；`out` 参数写失败时的值。

出处：`src/AtomUI.Controls.Shared/ImageLoading/ImageSource.cs`。

```csharp
    /// <summary>
    /// Tries to convert the text of a source into an <see cref="ImageSource" />.
    /// </summary>
    /// <param name="source">
    /// The text to convert; <see cref="Parse" /> lists the accepted forms.
    /// </param>
    /// <param name="result">
    /// The converted source, or <see langword="null" /> when the method returns <see langword="false" />.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the text was converted; <see langword="false" /> when it is blank, has an
    /// unknown form or scheme, or is a URI that the constructor of the matching source rejects.
    /// </returns>
    public static bool TryParse(string? source, out ImageSource? result)
```

### 事件

摘要用「Raised …」，写清触发原因和时机，涉及状态变化时写明在提交之前还是之后；处理器抛出异常时发生什么放在 `<remarks>`。

出处：`src/AtomUI.Core/Theme/ThemeConfigProvider.cs`。

```csharp
    /// <summary>
    /// Raised once a theme transition of this scope was committed and published.
    /// </summary>
    /// <remarks>
    /// An exception thrown by a handler is logged and does not reach the code that ran the transition.
    /// </remarks>
    public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;
```

### 可重写成员

摘要写它何时被调用，以及重写时对调用 `base` 的要求。必须调用时写明不调用的后果；基类实现为空时写明无需调用。

出处：`src/AtomUI.Core/Controls/Icon/Icon.cs`、`src/AtomUI.Core/Controls/Icon/IconProvider.cs`。

```csharp
    /// <summary>
    /// Fills <see cref="DrawBrushes" /> and <see cref="DrawPens" /> from the current brush and stroke properties.
    /// An override must call the base member, otherwise <see cref="FindIconBrush" /> returns <see langword="null" />
    /// until a brush property changes.
    /// </summary>
    protected override void OnInitialized()

    /// <summary>
    /// Called by the <see cref="Kind" /> setter when the new value differs from the current one.
    /// </summary>
    /// <remarks>
    /// The base method does nothing, so an override need not call it.
    /// </remarks>
    protected virtual void NotifyKindChanged()
```

### Semantic Part

用普通 `//` 注释，放在特性上方。先写这块区域是什么，再写存在条件（「Present only when …」），最后一句写生成的专用 Style 类名作用在哪个节点上。类名由检查工具按生成器的规则给出，不手写。

出处：`src/AtomUI.Desktop.Controls/Alert/Alert.SemanticParts.cs`。

```csharp
// The close button at the trailing end of the alert. Present only when IsClosable is true.
// AlertCloseStyle applies to the button.
[SemanticPart(
    "close",
    SelectorClass = "semantic-close",
    ContractType = typeof(IconButton),
    Since = "6.2.0")]
```
