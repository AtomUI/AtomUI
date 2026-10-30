# 文档结构与命名规范

本文是 AtomUI 仓库文档结构与文件命名的现行规范。文档分类的责任划分见
[文档总览](../../overview.md)；本文只定义目录布局、入口文件命名和拆分标准。

本文取代 `docs/superpowers/specs/2026-08-10-documentation-information-architecture-design.md` 中关于入口文件命名的
结论。该 spec 作为当时的设计过程记录保留，不再是命名规则的来源。

## 1. 入口文件命名

**任何文档目录的入口文件统一命名为 `overview.md`。**

这条规则没有例外，适用于：

| 目录层级 | 入口文件 |
|---|---|
| `docs/` | `docs/overview.md` |
| 一级分类目录 | `docs/architecture/overview.md`、`docs/modules/overview.md`、`docs/guides/overview.md` 等 |
| 架构分组目录 | `docs/architecture/foundations/overview.md`、`docs/architecture/systems/overview.md` |
| 跨模块系统目录 | `docs/architecture/systems/theming/overview.md`、`docs/architecture/systems/typography/overview.md` |
| 模块目录 | `docs/modules/core/overview.md`、`docs/modules/fonts/overview.md` |
| Control 平台与分类目录 | `docs/controls/desktop/overview.md`、`docs/controls/desktop/general/overview.md` |
| 单 Control 目录 | `docs/controls/desktop/general/button/overview.md` |
| Guide、Reference 子目录 | `docs/guides/theming/overview.md`、`docs/reference/localization/overview.md` |

不使用 `index.md` 作为文档入口。

## 2. 单 Control 目录文件契约

单 Control 目录的文件名属文档结构契约，不可改动：

```text
overview.md          必需
implementation.md    必需
semantic-part.md     条件必需，控件公开 Semantic Part 时提供
token.md             可选，控件拥有专属 Token 时提供
changelog.md         必需
<topic>-design.md    可选，单个专题设计
```

规则见 [控件文档规范](control-documentation-guidelines.md)。

## 3. 文档拆分标准

一个 `overview.md` 不承载完整系统设计。当一个主题需要展开时，按职责拆成同目录下的多个文件，`overview.md`
只保留定位、边界、总体架构和导航表。

拆分触发条件（满足任一即应拆分）：

- 单文件超过约 400 行。
- 文件同时定义两个以上可独立演进的契约（例如资源注册与 Token 派生）。
- 文件同时服务两类以上读者（例如控件作者与应用开发者）。
- 存在需要被其他文档单独引用的稳定小节。

拆分后的命名按职责，不按章节序号：

```text
docs/architecture/systems/typography/
├── overview.md                    定位、边界、两轴架构、导航
├── font-packages.md               资源层与注册入口
├── font-family-resolution.md      字体族解析、回退、覆盖、时序
├── tokens-and-derivation.md       Token 分级与派生算法
├── consumption.md                 消费契约与文本度量
└── verification.md                AOT、兼容性、验证、已知不一致
```

`overview.md` 的导航表必须列出同目录全部子文档及其所有权，使读者不必逐个打开文件判断归属。

`verification.md` 作为验证要求与已知不一致的独立文件，是跨模块系统目录的推荐结构；本地化与字体子系统均采用
该形态。

## 4. 事实所有权

一个稳定事实只能有一个正式所有者，其他文档通过链接引用，不复制完整规则。跨目录重复同一份规则时，保留层级
更高、读者更广的那一份，其余改为链接。

| 内容 | 所有者 |
|---|---|
| 跨两个以上模块的当前系统架构 | `architecture/` |
| 单个源码项目或发布包的职责与入口 | `modules/` |
| 单个 Control 或紧密 Control 家族 | `controls/` |
| 使用步骤、接入示例、操作流程 | `guides/` |
| 版本化格式、协议、公共 API 定义 | `reference/` |
| 开发规范、工作流、检查清单 | `engineering/` |
| 方案比较、实施计划、阶段进度 | `superpowers/` |

`superpowers/` 不参与正式文档导航和事实所有权判断。正式文档不得依赖 `superpowers/` 才能成立。

## 5. 链接规范

- 目录之间使用相对路径，指向具体文件，不指向目录。
- 指向某目录的入口时写完整文件名 `overview.md`，不依赖目录默认文件解析。

## 6. 变更检查

调整文档结构后必须确认：

1. 所有相对链接可解析，包括 `AGENTS.md` 与仓库根 README。
2. 单 Control 目录的四件套文件名未被破坏。
3. 目录树示意图与实际布局一致。
4. `git diff --check` 通过。
