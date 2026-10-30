# AtomUI 控件文档规范

本文档定义 AtomUI 控件级研发文档的结构、内容边界、写作规则和审查标准，适用于 `AtomUI.Controls`、`AtomUI.Desktop.Controls`、DataGrid、ColorPicker 以及其他控件包。

控件文档用于描述控件的最新设计状态、公共契约、交互模型、状态模型、主题架构、Token 边界、内部实现原理和维护规则。它不替代面向最终用户的站点文档、API reference、正式发布 `CHANGELOG.md` 或代码注释。

## 文档目录结构

每个控件必须使用独立目录承载文档：

```text
docs/controls/<platform>/<category>/<control>/
├── overview.md
├── implementation.md
├── semantic-part.md         # 条件必需，控件公开 Semantic Part 时提供
├── <topic>-design.md        # 可选，复杂专项设计
├── token.md
└── changelog.md
```

要求：

- `overview.md` 必须存在，用于描述控件设计定位、公共契约、状态模型、视觉主题关系和维护入口。
- `implementation.md` 必须存在，用于描述特定控件的内部实现原理、源码职责边界、关键状态流和维护规则，以提升控件可维护性。
- `semantic-part.md` 在控件公开 Semantic Part 时必须存在，是完整 Part 表、逐 Part 说明、Selector 用法、定制边界、兼容性和验证要求的唯一正式来源。
- `changelog.md` 必须存在，用于记录控件级设计、API、主题契约、Token 和实现结构变化。
- `token.md` 仅当控件存在专属 Token 或复杂主题变量关系时存在。
- 内容过多时可以继续拆分专题文档，但 `overview.md` 只保留设计与契约主线，`implementation.md` 只保留实现原理主线，并链接专题文档。
- 同一主题跨越 Public API、平台/状态策略、内部架构、Template、算法和验证边界时，使用本文定义的
  `<topic>-design.md` 控件专项设计文档，不把 Issue 分析、方案比较或实施计划塞入控件主文档。
- 分类入口文档必须链接控件目录中的主要文档。

### Mobile Control 文档

`docs/controls/mobile/` 及 `AtomUI.Mobile.Controls` 相关文档同时遵循
[AtomUI Mobile 文档规范](mobile-documentation-guidelines.md)。同名 Desktop/Mobile Control 分别拥有独立的文档根和
Gallery 证据 ownership；iOS、Android、Release 与发布状态必须分开表达。

在 Mobile Control 的 Public API、Theme、Contract/Headless 测试和 Gallery API/Token/ShowCase 证据同时存在前，不创建
空的单 Control 文档目录。具体状态词、分类清单和同步门禁由 Mobile 专项规范维护，本文不重复。

## 写作原则

控件文档必须采用严谨、稳定、工程化的语言。

必须遵守：

- 描述最新设计状态，不写历史过程。
- 不使用“未来”“后续”“演进方向”等路线图表述。
- 不堆砌属性清单；属性必须服务于 API 模型、状态模型或兼容边界。
- 不重复全局规范；通用规则应链接全局规范文档。
- 不记录临时讨论、实现流水账或无长期维护价值的细节。
- API、主题契约、Token、template part、伪类等必须表述为稳定契约。
- `overview.md` 不展开具体代码实现，具体实现原理进入 `implementation.md`。
- `implementation.md` 不重复公共 API 清单、Token 全局规则或用户文档内容，只解释当前控件内部实现为什么这样组织、状态如何流动、维护时必须守住哪些边界。
- `semantic-part.md` 只描述对应用公开的主题契约，不把 `PART_*`、内部节点、视觉层级或实现类型提升为公共 Part。

历史变化统一进入控件级 `changelog.md`。

## 控件专项设计文档

控件专项设计文档用于描述一个控件内部具有独立语义、跨越公共契约与实现职责、且无法在
`overview.md` 或 `implementation.md` 中简洁说明的稳定设计主题。

典型主题包括：

- 跨平台窗口布局、原生能力投影或平台自适应策略。
- 复杂选择、展开、过滤、校验、异步加载或状态同步模型。
- 跨多个主题、presenter、popup host 或内部协作控件的组合机制。
- 具有明确公式、边界条件和生命周期约束的布局或数据算法。
- 同时影响 Public API、模板契约、内部职责和验证矩阵的专项能力。

专项设计文档不是独立的控件总览，也不替代实现文档。它只展开一个稳定设计主题，并由控件的
`overview.md` 和 `implementation.md` 提供导航。

### 创建条件

满足以下任一条件时可以创建专项设计文档：

- 同一主题同时涉及公共 API、内部架构、模板或平台适配，放入单一主文档会破坏其内容边界。
- 主题包含需要独立维护的术语、状态、区域、角色或数据模型。
- 主题包含跨平台、跨宿主、跨模式或跨窗口状态的策略矩阵。
- 主题包含维护者必须共同遵守的算法、公式、退化规则或时序不变量。
- 主题需要独立的兼容性、定制边界和分层验证要求。

以下情况不创建专项设计文档：

- 仅罗列少量属性、事件或方法。
- 仅记录一个私有方法或局部实现技巧。
- 仅记录 Issue 调查、截图分析、复现过程或根因取证。
- 仅比较候选方案、记录会议结论或实现计划。
- 仅记录版本变化、迁移过程、commit、PR 或发布说明。
- 内容能够在 `overview.md` 或 `implementation.md` 的一个短小章节内完整表达。

### 文件位置与命名

专项设计文档与对应控件的主文档放在同一目录：

```text
docs/controls/<platform>/<category>/<control>/
├── overview.md
├── implementation.md
├── <topic>-design.md
├── token.md
└── changelog.md
```

命名规则：

- 文件名使用稳定的英文 kebab-case 主题名，以 `-design.md` 结尾。
- 标题使用 `# <Control> <专项主题>设计`。
- 主题名表达长期设计能力，不使用 Issue 编号、版本号、日期、分支名或修复动作。
- 同一主题只维护一份专项设计文档，不按平台或实现阶段复制多份。

示例：

```text
window-title-bar/title-alignment-design.md
tree/selection-model-design.md
image-previewer/loading-pipeline-design.md
```

带日期的调查、迁移或方案记录继续放在对应的 specs、迁移记录或发布记录目录，不放入控件专项设计文档。

### 写作边界

专项设计文档只描述最终采用的设计状态，使用确定、稳定、可验证的工程语言。

必须遵守：

- 直接说明控件如何设计，不以 Issue、缺陷、截图、实现任务或讨论过程作为叙事入口。
- 描述采用的模型、契约、职责和算法，不保留候选方案比较或“不采用方案”章节。
- API 使用稳定语义描述，不使用“准备新增”“待实现”“当前尚不支持”等实施状态措辞。
- 文件结构表达稳定 ownership，不写成目标文件清单或任务拆分清单。
- 平台差异使用能力、metrics 和策略表达，不把当前开发机或本地路径当作设计事实。
- 算法必须定义输入、输出、坐标系、边界条件和退化规则。
- 兼容性必须描述不可破坏的契约和定制边界，不写一次性变更摘要。
- 验证要求必须对应设计不变量，不写与专项主题无关的通用测试清单。

不得包含：

- Issue 编号、截图像素取证、复现步骤和根因调查流水。
- 设计状态、实现状态、人员分工、排期、todo 或实施 checklist。
- commit、PR、tag、本地仓库路径或外部项目源码行号。
- 版本历史、迁移历史和 release note；这些内容进入 `changelog.md` 或专项历史记录。
- 与控件专项主题无关的全局工程规范副本。

### 标准结构

专项设计文档按以下通用结构组织。允许根据主题合并相邻章节，但不能混淆内容职责。

```text
# <Control> <专项主题>设计

1. 设计定位
2. 设计原则
3. 专项模型与 Public API
4. 变体、平台或状态策略
5. 架构、文件结构与职责
6. Template、组合与集成契约
7. 核心算法、数据流与生命周期
8. 资源、性能与 AOT 边界
9. 兼容性与定制边界
10. 验证要求
```

章节适用规则：

| 章节 | 要求 | 说明 |
| --- | --- | --- |
| 设计定位 | 必须 | 说明专项能力解决什么设计职责，以及覆盖哪些控件、宿主或状态。 |
| 设计原则 | 必须 | 定义后续 API、策略和算法共同遵守的不变量。 |
| 专项模型与 Public API | 必须 | 先定义术语、区域、状态或角色模型，再说明相关公共契约；没有 Public API 时明确内部契约边界。 |
| 变体、平台或状态策略 | 条件必须 | 存在 `Auto`、平台、CSD、主题变体、模式或状态差异时必须提供映射或策略矩阵。 |
| 架构、文件结构与职责 | 必须 | 说明稳定模块结构、owner、输入输出和明确不负责的内容。 |
| Template、组合与集成契约 | 条件必须 | 涉及 ControlTheme、Template Part、internal presenter、host 或组合关系时必须说明。 |
| 核心算法、数据流与生命周期 | 条件必须 | 存在公式、状态同步、异步流程或资源获取释放时必须说明输入、流程、边界和退化。 |
| 资源、性能与 AOT 边界 | 必须评估 | 无专项影响时可以简短说明不引入额外资源、分配、反射或动态发现。 |
| 兼容性与定制边界 | 必须 | 说明稳定 API、默认值、模板、渲染、平台语义和外部定制责任。 |
| 验证要求 | 必须 | 按纯逻辑、控件行为、主题契约、平台实机、Gallery/demo 和 AOT 风险分层。 |

### 各章节内容要求

#### 设计定位与原则

设计定位从控件能力出发，说明设计是什么、适用在哪里、与相邻职责如何分界。设计原则只记录能够约束后续
API、架构和算法的稳定规则，不写项目目标、实施收益或讨论结论。

#### 专项模型与 Public API

先定义主题自己的术语和模型，例如：

- Leading、Title、Trailing 区域。
- selected、current、checked、expanded 状态 owner。
- loading pipeline、request、result 和 cancellation 状态。
- visible frame、content bounds、overlay bounds 等坐标语义。

Public API 按语义说明属性、枚举、事件和默认值。可以使用短代码片段准确表达契约，但不复制完整 API 表，
不把源码成员顺序改写成文档结构。

#### 变体、平台或状态策略

存在自动模式或变体时，文档必须明确：

- `Auto` 如何解析。
- 显式值是否跨平台保持同义。
- CSD、平台、主题、宿主和 WindowState 如何影响输入或策略。
- 哪些差异允许进入平台层，哪些共享语义不得复制。
- Unknown、unsupported 或能力缺失时如何回退。

映射和组合较多时使用矩阵，不用散落的条件句描述同一状态空间。

#### 架构、文件结构与职责

文件结构只列出该专项的稳定源码边界。每个类型、Panel、Strategy、presenter、handler 或 host 必须有单一
职责，并说明主要输入、输出和 owner。若明确不引入某类辅助对象，应以职责和数据规模解释边界，不写成对
历史方案的评价。

#### Template、组合与集成契约

涉及主题时，说明：

- 语义区域与真实 Template Part 或主题节点的对应关系。
- public control、internal control、presenter、host 和 item container 的组合结构。
- 哪些 part、ControlTheme key、Role、伪类和命中测试语义稳定。
- 默认模板、派生模板和特殊宿主如何复用同一模型。
- 外部替换模板时由 AtomUI 和应用各自承担什么责任。

#### 核心算法、数据流与生命周期

算法设计至少定义：

- 输入、输出、单位和坐标系。
- 状态或数据的 owner 与单向流向。
- 主流程顺序。
- 空值、零尺寸、重叠、取消、异常或能力缺失时的退化规则。
- 触发重新计算、重新测量、重新订阅或释放的条件。

公式应集中在一个共享职责中；平台、模板或派生控件不能复制同一算法。

#### 资源、性能与 AOT

说明专项设计是否引入：

- DynamicResource、binding、subscription、timer、cache 或异步任务。
- measure/arrange、render、pointer move 等热路径分配。
- 反射、动态发现、运行时注册或 trimming 风险。
- owner/container/template reapply 对应的释放路径。

不涉及这些能力时，也应写明使用纯值、静态注册或无状态对象等边界。

#### 兼容性、定制与验证

兼容性描述 API 默认值、显式语义、模板 part、主题 key、渲染结果和外部定制责任。验证要求必须直接证明
这些设计不变量，并按风险选择纯函数测试、headless 控件测试、主题结构测试、Gallery/demo、平台实机或
NativeAOT publish。

### 与控件主文档的关系

- `overview.md` 保留专项能力的设计摘要、Public API 入口和兼容性主线，并链接专项设计文档。
- `implementation.md` 保留源码索引、核心 owner、生命周期和维护入口，并链接专项设计文档。
- 专项设计文档在首段链接回 `overview.md` 和 `implementation.md`。
- `token.md` 只记录 Token 语义；专项设计引用 Token 时链接 `token.md`，不复制 Token 表。
- `changelog.md` 只记录专项设计、API、主题或实现结构的变化，不解释当前设计。
- 专项设计中的公共契约和语义结构不能只存在于专题文件；`overview.md` 与 `implementation.md` 必须保留
  足以支撑导航的摘要。

### 审查清单

- 文档是否从控件设计本身开始，而不是从 Issue、截图或修复过程开始。
- 是否只描述采用的最终设计，没有候选方案比较和实施状态。
- 设计定位、原则、专项模型、职责和兼容边界是否互相一致。
- Public API、默认值和显式语义是否明确。
- 平台、CSD、模式、宿主或状态差异是否使用完整矩阵表达。
- 文件结构是否表达稳定 ownership，而不是任务拆分。
- Template Part、Role、ControlTheme key、组合节点和外部定制责任是否明确。
- 算法是否定义输入、输出、坐标系、边界和退化规则。
- 生命周期是否存在明确的获取、失效、取消和释放路径。
- 性能和 AOT 是否经过明确评估。
- 验证要求是否能够证明每项设计不变量。
- `overview.md`、`implementation.md` 和专项设计文档是否互相链接。
- 是否没有重复全局规范、版本历史或无长期维护价值的实现细节。

### 验证要求

专项设计文档改动至少执行：

```bash
git diff --check
```

同时检查：

- 文件名和标题符合专项设计命名规则。
- 所有相对链接存在。
- `overview.md` 和 `implementation.md` 已添加专项设计入口。
- 文档不包含 Issue 叙事、实现状态、历史过程或任务清单。
- 章节与专项复杂度匹配，没有为了套模板制造无意义内容。
- Public API、Template、算法和平台矩阵与同一变更中的源码设计保持一致。

## `overview.md` 结构

`overview.md` 必须按以下结构书写：

```text
# <Control> <平台>架构设计

1. 控件定位
2. 设计语言
3. API 与契约模型
4. 行为与状态模型
5. 视觉与主题模型
6. 控件家族或集成关系
7. 兼容性不变量
8. 专项模型
9. 文档导航与验证策略
```

章节要求：

- 控件定位：定义控件职责边界，说明控件是什么、不是什么。
- 设计语言：描述控件表达的产品语义和视觉语义。
- API 与契约模型：说明公共属性、事件、方法、template part、伪类和主题入口的契约边界，不机械罗列成员。
- 行为与状态模型：说明 hover、pressed、disabled、loading、focus、keyboard、motion 等交互语义，以及 public API 如何归一为 effective state。
- 视觉与主题模型：说明模板、Theme、Token、SharedToken 的总体关系和不可破坏的视觉契约，不展开具体 AXAML 结构。
- 控件家族或集成关系：说明与派生控件、组合控件、Form、Compact、Browser theme 等关系。
- 兼容性不变量：列出优化和扩展时必须保持不变的 API、行为和渲染。
- 专项模型：记录控件特有模型，例如 Button 的 `Color / Variant`。
- 文档导航与验证策略：链接 `implementation.md`、`semantic-part.md`（存在时）、`token.md`、`changelog.md`，并按 Public API、状态、AXAML、Token、文档分层列出验证要求。

无对应内容的章节不能删除，应写明“不适用”及原因。

md
# <Control>

## 概述
## 包与命名空间
## 何时使用
## 公共 API
## 事件与命令
## 使用示例
## 状态模型
## 主题与 Design Token
## AOT 与裁剪注意事项
## 源码索引
## 相关文档
```

内容来源要求：

- “概述”“何时使用”“状态模型”主要来自 `overview.md`。
- “公共 API”“事件与命令”来自 `overview.md` 语义摘要和源码 public surface。
- “使用示例”来自 Gallery ShowCase 和源码片段 catalog。
- “主题与 Design Token”来自 `token.md`、Token 类型或生成数据。
- “AOT 与裁剪注意事项”来自 `implementation.md` 和全局 AOT 规范。
- “源码索引”来自 `implementation.md` 的源码文件结构。

生成的 `controls/<control>/semantic-cn.md` 必须使用以下结构：

```md
# <Control> 语义结构

## `semantic-part.md` 结构

控件公开 Semantic Part 时必须创建 `semantic-part.md`。该文件是公共 Part 契约的唯一完整来源；`overview.md` 只保留
owner、Part 名称和职责摘要，`implementation.md` 只保留 descriptor、marker 与真实模板或运行时节点的映射。

标准结构如下。多 owner 控件可以按 owner 分组逐 Part 说明，但不能省略任何职责：

```text
# <Control> Semantic Part 契约

1. Semantic Parts
2. Part 说明，或按 public owner 分组的 Parts 说明
3. Selector 用法
4. 状态与数量语义
5. 定制边界
6. 兼容性与验证
```

首段必须链接回 `overview.md` 和 `implementation.md`，并链接
[AtomUI Semantic Part 系统设计](../../architecture/systems/theming/semantic-parts.md)。

`Semantic Parts` 表格必须按 public owner 和控件语义区域组织：
## Abstract AXAML Structure
## Composition Model
## Template Parts
## Pseudo Classes
## State Flow
## Theme and Token Boundaries
## Customization Boundaries
```

AtomUI 的 semantic 文档描述 AXAML、ControlTemplate、运行时组合结构、Template Part、伪类、状态流和 Token 语义，不描述 Web DOM。

`Semantic Parts` 正式文档统一采用按 public owner 和 Part 分组的“字段 / 值”两列表格，不再新增横向宽表。
每个 Part 必须单独成组，表格必须完整保留 `Owner`、`Part`、`Selector`、`SelectorRoute`、`ContractType`、`Cardinality`、
`Customization`、`CrossVisualRoot`、`RuntimeCreated`、`AtomUI 节点`、`职责`、`相关 API`、`相关 Token` 和 `稳定性`；
不能为了缩短表格而省略类型、数量或运行时边界。

```md
### 1.1 `Button`

#### `content`

| 字段 | 值 |
| --- | --- |
| Owner | `Button` |
| Part | `content` |
| Selector | `.semantic-content` |
| SelectorRoute | `/template/ .semantic-content` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | 内容展示区域 |
| 职责 | 用户内容区域 |
| 相关 API | `Content` |
| 相关 Token | 组件 Token / SharedToken |
| 稳定性 | stable since 6.2.0 |
```

要求：

- `Part` 是语义名，不一定等于 Template Part 名。
- `Owner` 必须是实际声明 descriptor 的 public 控件；同一控件目录存在多个 public owner 时逐项区分。
- `Selector` 必须与生成式 Semantic Part descriptor 一致；`root` 不添加 `.semantic-root`。
- `SelectorRoute` 必须与 descriptor 一致；root 写“不适用”。静态根模板 Part 通常为
  `/template/ .semantic-*`，`RuntimeCreated=true` 必须记录完整 owner-relative route。
- `ContractType` 是 marker 节点必须兼容的最低稳定 public 类型，也是 Semantic Style 的
  `x:SetterTargetType`；它不作为 `.semantic-*` selector 的类型前缀。
- 用户示例必须把 Part 身份写成 `.semantic-*`。包含 Setter 时显式写
  `x:SetterTargetType="<ContractType>"`，不得生成 `Control.semantic-*` 或 `:is(Control).semantic-*` 形式。
- `Cardinality` 只能是 `Single`、`Optional` 或 `Multiple`。
- `Single` 表示每个适用模板恰好一个，`Optional` 表示零或一个，`Multiple` 表示一个或多个；表格必须与生成
  descriptor 和所有内置模板一致。
- `AtomUI 节点` 可以是 Template Part、主题节点、控件类或抽象区域。
- `Customization` 使用 `Root`、`Selector` 或 `SelectorAndTheme`。
- 只记录进入生成式 descriptor 的稳定语义区域；用户不应依赖的内部节点只进入 Composition Model 或
  Customization Boundaries，不能标记为公开 Semantic Part。
- 表格、Control 声明、AXAML marker 和生成 descriptor 不一致时必须验证失败，不能只修改文档消除差异。
- `Since`、`CrossVisualRoot` 或 `RuntimeCreated` 影响使用和验证边界时，必须在稳定性或实现说明中明确记录；
  `CrossVisualRoot` 不自动推导 Theme 属性，`RuntimeCreated` 不免除运行时 selector 测试。

Part 表之后必须逐 Part 说明：

- Part 在哪些模板、状态或 owner 生命周期中存在。
- 适合定制的属性，以及布局型 Setter 需要验证的 owner 尺寸、shape、裁剪和 Measure/Arrange 边界。
- 伪类、属性状态、替代实现与 cardinality 的关系。
- 哪些内部节点、Name、`PART_*`、用户内容子树或子 ControlTemplate 明确不属于该 Part。
- Selector 示例使用 owner 与 descriptor 的完整 `SelectorRoute`，并以 `x:SetterTargetType` 提供类型上下文。复杂 Part 可以
  包含多个 `/template/` 和 `>`；不得用普通 logical descendant 缩写 route。
- 删除、改名、类型收窄、cardinality 变化和模板替换时的兼容性与验证要求。

`Abstract AXAML Structure` 必须遵守：

- 只能由生成器从真实 `ControlTheme` / `ControlTemplate` 抽取。
- 生成器通过 `implementation.md` 的源码索引定位 `*Theme.axaml`，再读取模板节点。
- 不允许根据 `Semantic Parts` 表、控件分类或控件名称生成伪 XML。
- 不允许输出 `SemanticPart` 这类并不存在于 AXAML 模板中的占位节点。
- 控件没有可定位的 ControlTheme 模板时，该章节只说明未生成结构，并链接 Template Parts、主题文件和源码索引。
- 如果需要稳定抽象结构，必须先在控件源文档和源码索引中明确对应 ControlTheme / ControlTemplate 来源，而不是手工修改生成产物。

`Composition Model` 必须描述 public 控件与内部协作对象之间的运行时组合关系。它的第一信息源是控件源码目录下的 `Themes/` 文件夹：

- 优先阅读同一控件家族的独立 `*Theme.axaml` 叶子，并使用生成的 ControlTheme asset manifest 校验 owner、引用的 Control identity 和 Semantic Part Theme 关系。
- 从多个 `ControlTheme` 中识别 public control、internal control、item container、adorner、presenter、popup host、motion actor、template part 和跨主题组合关系；不得依赖只用于聚合的 `*Themes.axaml`。
- `implementation.md`、源码索引、C# 创建逻辑、`CreateContainerForItemOverride()`、`OnApplyTemplate` 和 `PART_` 只作为解释与校验补充。
- 不允许仅凭控件名称、控件分类或通用模式发明内部协作节点。

`Composition Model` 解决以下场景：

- public 控件本身 `ControlTheme` 很薄，但行为由内部容器、presenter、adorner、popup host、motion actor 或 item container 完成。
- 控件结构分散在多个主题文件、C# 创建逻辑、item container 生成、adorner 挂载或数据对象中。
- Agent 只阅读 `Semantic Parts` 和 `Abstract AXAML Structure` 时，会低估控件结构复杂度，进而误导用户直接依赖内部类型或错误理解状态流。

`Composition Model` 至少包含：

````md
## Composition Model

### 控件角色图

```text
<PublicControl>
  -> <InternalControlOrPresenter> (<theme source>)
     -> <ElementType>#<TemplatePartName> (<stability>)
        -> <ChildElementType>#<ChildName> (<stability>)
```

### 协作节点

| 节点 | 类型 | 来源 | 生命周期 owner | 影响的 public API | 稳定性 | Agent 使用边界 |
| --- | --- | --- | --- | --- | --- | --- |
| ... | ... | ... | ... | ... | public / template-stable / internal-observable / private | ... |
````

`控件角色图` 必须是模板层级树，不是节点清单。生成器应按真实 `ControlTemplate` XML 父子关系输出结构：`ControlTheme` target 是角色入口，模板视觉节点以 `ElementType#Name` 表示；没有 `Name` 的节点只输出 `ElementType`。同一个 `ControlTheme` 中的 template part、presenter、motion actor、adorner indicator 和布局节点不得被打平成同级节点。存在多个模板变体时可以输出多个根树，但必须保持每个变体内部的父子关系。

稳定性要求：

- `public` 表示用户可直接使用和依赖的 public 控件、数据对象或 API。
- `template-stable` 表示稳定 template part、ControlTheme key 或主题节点，可用于主题维护但不能随意改名。
- `internal-observable` 表示内部协作对象，影响用户可观察行为；Agent 可以用它理解结构，但不能指导用户直接依赖。
- `private` 表示私有实现细节，默认不作为公开文档内容；如必须出现，只能用于说明不可依赖边界。

不同控件族的组合结构重点：

| 控件族 | 代表控件 | 文档重点 |
| --- | --- | --- |
| Shell / Overlay | Drawer、Modal、Notification、Message、PopupConfirm | public control、container、mask、surface、motion、close/action 区。 |
| Adorner / Decorator | Badge、Watermark | decorated target、adorner、indicator、motion actor、offset、placement。 |
| Items / Container | Breadcrumb、Steps、Menu、ListView、TabControl | owner control、item data、item container、separator、trigger、selection/navigation flow。 |
| Popup Input | Select、Cascader、DatePicker、TimePicker、ComboBox、AutoComplete | input shell、popup host、panel、item presenter、selection/value sync。 |
| Presenter / Family | ProgressBar、Skeleton、Card、Descriptions | public API 到内部 presenter、visual unit 或家族子控件的映射。 |

没有额外运行时组合层的控件也应明确说明：该控件主要由 public 控件和 ControlTheme 模板直接表达，没有额外内部协作对象。生成器不得因缺少复杂结构而发明组合节点。

## `implementation.md` 结构

`implementation.md` 描述特定控件的内部实现原理，用于帮助维护者理解源码结构、状态流转、生命周期和关键算法。它不替代代码注释，不记录临时实现过程，也不作为用户 API 文档。

`implementation.md` 必须按以下结构书写：

```text
# <Control> <平台>实现原理

1. 实现定位
2. 源码文件结构
3. 核心类职责
4. 状态与数据流
5. 组合结构模型
6. 生命周期与模板接入
7. 交互与事件处理
8. 内部算法与关键流程
9. 资源、性能与 AOT 边界
10. 维护不变量
11. 测试与验证
```

章节要求：

- 实现定位：说明本文档覆盖的内部实现范围，以及哪些细节仍应直接阅读源码。
- 源码文件结构：说明控件相关 `.cs`、`.axaml`、Token、helper、handler、内部 view 的职责边界。
- 核心类职责：描述主要类、内部接口、handler、presenter、decorator、数据节点等协作关系。
- 状态与数据流：描述 public API、internal state、effective state、ItemsSource、Children、selection、checked、expanded、filter 等状态如何流动。
- 组合结构模型：优先基于控件 `Themes/` 文件夹描述 public 控件、内部协作控件、item container、adorner、popup host、motion actor、presenter、数据对象和 template part 的运行时关系。
- 生命周期与模板接入：描述构造、初始化、加载、卸载、template part 获取、事件订阅释放、container 准备和资源绑定规则。
- 交互与事件处理：描述 pointer、keyboard、focus、context menu、drag/drop、motion、popup 等内部事件路径。
- 内部算法与关键流程：描述维护者必须理解的遍历、布局、状态回放、同步、缓存、渲染等关键流程，不逐行复述代码。
- 资源、性能与 AOT 边界：描述动态资源、事件订阅、binding、缓存、异步任务、反射或生成器相关约束。
- 维护不变量：列出重构、优化和修 bug 时不能破坏的内部行为顺序、状态一致性、生命周期释放和主题契约。
- 测试与验证：列出对应测试、Gallery 走查点、AOT 或发布验证要求。

要求：

- 只写当前实现原理，不写历史过程；历史变化进入 `changelog.md`。
- 不把私有方法逐个改写成说明书；只记录维护者必须知道的稳定结构、关键路径和不变量。
- 不重复 `overview.md` 的设计语言、API 模型和公共契约；必要时只引用它们作为实现入口。
- 不重复 `token.md` 的 Token 分类和语义；实现中使用 Token 的路径可以链接 `token.md`。
- 当控件实现较简单时，也应保留该文档，并用简短章节说明实现没有额外内部模型，避免维护者误判文档缺失。

`implementation.md` 的“组合结构模型”章节应使用稳定的角色图与协作节点表。维护要求：

- 组合节点必须优先来自控件 `Themes/` 文件夹中的 `ControlTheme`、template part、内部主题控件和主题聚合关系。
- C# 源码中的动态创建、item container 生成、adorner 挂载和 popup host 管理用于补充主题文件无法表达的运行时关系。
- 不把 internal 类型写成用户可直接使用的 API。
- 对于 internal 但用户可观察的结构，使用 `internal-observable` 并说明 Agent 只能用于理解行为和维护边界。
- 对于 template part 或 ControlTheme key，使用 `template-stable` 并说明变更需要同步主题、实现和文档。
- 对于简单控件，明确说明没有额外组合层，不允许为了补齐章节发明内部节点。

## `token.md` 结构

`token.md` 只记录控件专属 Token 内容，不重复全局 Token 系统规则。通用 Token 分层、命名、计算、Theme Variables、预设色规则统一链接 [AtomUI 控件 Token 设计规范](../development/control-token-guidelines.md)。

`token.md` 必须按以下结构书写：

```text
# <Control> Token 设计

1. 定位
2. Token 分类
3. 控件专项模型中的 Token 使用
4. 控件家族影响
5. 兼容性要求
6. 验证策略
```

要求：

- Token 分类必须按控件语义组织，而不是按代码顺序机械罗列。
- Token 文档必须说明哪些主题和控件家族引用这些 Token。
- 不允许把实例状态、交互状态或 `EffectiveXxx` 状态写成 Token。
- 不允许在控件 Token 中展开颜色、variant、状态的组合 Token。
- 没有专属 Token 的控件不需要创建 `token.md`，但 `overview.md` 中仍应说明其 Theme 是否直接使用 SharedToken。

如果控件没有专属 `token.md`，`overview.md` 必须明确说明：

- 该控件没有专属 Token；或
- 该控件复用家族 Token；或
- 该控件直接使用 SharedToken / 主题资源。

## `changelog.md` 结构

每个控件必须维护控件级 changelog。

格式：

```md
# <Control> Changelog

本文档记录 <Control> 控件级设计、API、主题契约、Token 和实现结构的变化。
它不替代仓库根目录 CHANGELOG.md，也不作为正式版本发布说明。

## YYYY-MM-DD

- Docs
  - ...
- API
  - ...
- Theme
  - ...
- Token
  - ...
```

记录范围：

- API 模型变化。
- 主题契约变化。
- Token 分类、语义或边界变化。
- template part、伪类、状态模型变化。
- 控件家族协同规则变化。
- 文档结构、实现文档边界和重要设计决策变化。

不记录：

- 临时讨论。
- 纯格式化。
- 无长期维护价值的实现细节。
- 正式 release changelog 条目。

## 链接规则

控件目录内文档必须互相链接：

- `overview.md` 链接 `implementation.md`、`token.md` 和 `changelog.md`。没有 `token.md` 时只链接 `implementation.md` 和 `changelog.md`。
- `implementation.md` 链接 `overview.md`、`changelog.md`，并在涉及控件专属 Token 时链接 `token.md`。
- 存在 `semantic-part.md` 时，`overview.md` 和 `implementation.md` 必须链接它；`semantic-part.md` 必须在首段链接回
  `overview.md` 和 `implementation.md`。
- 存在 `<topic>-design.md` 时，`overview.md` 和 `implementation.md` 必须链接该专项设计文档；专项设计文档
  必须在首段链接回 `overview.md` 和 `implementation.md`。
- `token.md` 链接 `overview.md`、`implementation.md`、`changelog.md` 和 [AtomUI 控件 Token 设计规范](../development/control-token-guidelines.md)。
- `changelog.md` 不解释当前设计，只记录变化。

分类入口文档必须链接控件目录：

```md
- [Button 桌面版架构设计](button/overview.md)
- [Button 桌面版实现原理](button/implementation.md)
- [Button Token 设计](button/token.md)
- [Button Changelog](button/changelog.md)
```

分类入口应保持控件列表完整、顺序稳定，并按控件目录链接主要维护文档。

## 审查标准

控件文档 review 时按以下标准检查：

- 是否描述最新状态，而不是历史过程。
- 是否明确公共 API、事件、方法和兼容边界。
- 是否有清晰的状态模型。
- 是否说明模板职责和不可破坏的 template part。
- 是否有独立 `implementation.md` 描述控件内部实现原理、源码职责边界、生命周期和维护不变量。
- `overview.md` 是否避免承载过重实现细节。
- 是否把通用规则放到全局规范，而不是控件文档重复。
- 是否维护控件级 `changelog.md`。
- 是否包含分层验证策略。
- 是否存在“未来”“后续”“演进方向”等不适合架构文档的表述。
- 控件公开 Semantic Part 时，是否存在 `semantic-part.md`，并且 `overview.md` 只保留支持摘要、`implementation.md` 只保留节点映射。
- `semantic-part.md` 是否完整列出每个 public owner 的 Part，并逐 Part 说明职责、存在条件、状态/cardinality、定制和排除边界。
- `implementation.md` 是否提供稳定源码索引、状态流、生命周期和 AOT 边界。
- 存在专项设计文档时，是否遵循本文“控件专项设计文档”章节，并且只描述最终设计，没有混入 Issue 调查、
  候选方案或实现状态。
- 结构复杂控件的 `implementation.md` 是否基于 `Themes/` 文件夹提供 Composition Model，并明确 internal 协作对象的稳定性和 Agent 使用边界。
- `token.md` 是否能解释 Token 语义；没有 `token.md` 的控件是否在 `overview.md` 明确说明原因。
- Template Part、伪类、主题资源和 Token 是否描述完整。
- 控件文档、源码 public surface、Token 类型和 Gallery ShowCase 示例是否对应。

## 验证要求

控件文档改动至少验证：

```bash
git diff --check
```

同时检查：

- 新增文档没有尾随空白。
- 所有相对链接存在。
- 分类入口文档已更新。
- 只改文档时，不应误改控件实现代码或主题文件。
