# AtomUI 全局主题研发规范

> 本规范定义主题研发与验收要求。现行运行协议与目标机制按其版本边界执行；设计文档更新不表示源码迁移或功能验收已经完成。

## 1. 定位与范围

统一开发者、AI agent、控件作者、主题包作者及Gallery维护者的设计与验收依据，使主题配置能够沿确定路径到达最终渲染，并保持控件交互、动态更新、性能、资源生命周期及AOT合同。

规范覆盖Core主题运行时、全局与Control Token、生成器、ControlTheme、Semantic Part、自绘、共享primitive、普通与跨根弹层、可选控件包、主题包、配置协议和Gallery工具。没有Own Token的公开Control也在范围内；内部视觉对象归属实际owner。Mobile按其产品边界遵守相同原则，但不能把尚无源码和Host证据的平台写成已实现。

成功标准是“每项公开定制合同有准确语义、实际消费者、有效更新和可核对证据”。不以Token数量、字面量消除数量、测试数量或某一张截图作为完成标准。

本目录拥有跨模块主题研发的强制要求、消费规范和验收门禁。系统运行语义继续由[主题系统架构](../../../architecture/systems/theming/runtime.md)拥有，Own定义继承由[继承架构](../../../architecture/systems/theming/control-design-token-inheritance.md)拥有，语义部件由[Semantic Part系统](../../../architecture/systems/theming/semantic-parts.md)拥有；本目录不建立第二套运行时合同。

有效Own、完整品牌与渲染终点分别见[Own求值设计](../../../architecture/systems/theming/own-token-evaluation.md)、[品牌配方设计](../../../architecture/systems/theming/brand-recipes.md)和[Control渲染设计](../../../architecture/systems/theming/control-rendering.md)；源码符合程度由[实现与验证边界](../../../architecture/systems/theming/verification.md)维护。

## 2. 专题所有权与阅读顺序

| 文档 | 所有权 |
| --- | --- |
| 本文 | 定位、适用范围、规范用语、规则索引和事实边界 |
| [基础设施与Token合同](infrastructure.md) | TH-01/02/03/04/08/11：统一运行时、owner、求值、可配置契约、完整品牌和工程边界 |
| [控件消费规范](consumption.md) | TH-05/06/07/09/10：真实渲染、声明式样式、Part、更新/释放与尺寸形态 |
| [验收与兼容性规范](verification.md) | TH-12：兼容性与例外；逐控件交付内容、执行门禁及证据要求 |

Control Token的具体声明规则继续见[Control Token规范](../control-token-guidelines.md)；Control模板和Custom尺寸见[Control研发规范](../control-development-guidelines.md)；资源作用域宿主见[Scoped Resource Host](../scoped-resource-host.md)。主题包、格式与ABI由对应的[主题架构入口](../../../architecture/systems/theming/overview.md)导航。

## 3. 规范用语与事实边界

- **必须/禁止**：可阻止相关改动通过评审的合同；每项必须能对应检查方式或运行证据。
- **应**：默认实践，偏离时说明具体原因及等价保障。
- **可以**：在声明边界内的可选能力，不暗示所有控件都必须提供。
- 条款ID用于稳定引用与审查，不作为新的编译器诊断编号，也不建立运行期规则解释器。

条款来源分为：**现行合同整合**、**消费/质量要求**、**需版本化的机制演进**。现行合同按既有版本执行，新机制按其正式设计及兼容边界接入，不能用文档替代源码验证。设计基线、源码符合、运行验证和平台发布分别记录。

出现不一致时必须指出冲突，并按用户已批准的设计/兼容性决策处理：不能因旧源码存在缺陷就下调合同，也不能因新方案写了目标就假定当前runtime已经支持。修改现行语义时同步调整它的正式所有者、版本/迁移说明及验证；不得让两份正式文档长期表达不同规则。

## 4. 强制条款索引

| ID | 主题 | 规则正文 |
| --- | --- | --- |
| TH-01 | 统一运行时与唯一有效值来源 | [专题合同](infrastructure.md) |
| TH-02 | 精确owner与正确作用域 | [专题合同](infrastructure.md) |
| TH-03 | Token分层、覆盖与派生语义 | [专题合同](infrastructure.md) |
| TH-04 | 严格且最小的可配置契约 | [专题合同](infrastructure.md) |
| TH-05 | 视觉输入必须到达真实渲染 | [专题合同](consumption.md) |
| TH-06 | 声明式模板与原生样式优先级 | [专题合同](consumption.md) |
| TH-07 | Semantic Part是稳定公共定制合同 | [专题合同](consumption.md) |
| TH-08 | 完整品牌表达与工具同源 | [专题合同](infrastructure.md) |
| TH-09 | 确定的动态更新与生命周期 | [专题合同](consumption.md) |
| TH-10 | 值边界、尺寸、形态与可访问性 | [专题合同](consumption.md) |
| TH-11 | AOT、资产、性能与错误语义 | [专题合同](infrastructure.md) |
| TH-12 | 兼容性、例外与验收证据 | [专题合同](verification.md) |

本规范集只定义研发与验收要求，不宣称所有检查已自动化。新增与修改路径按适用条款执行；存量按控件和机制核对，未验证项不得填为通过。方案比较、排期与迁移进度不进入本规范集。
