# CT-07 托管样式资格原型实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: 使用`superpowers:executing-plans`逐项执行。按用户明确要求，由主agent独立实施和另起步骤自审，不使用subagent；未经明确指令不提交。

**Goal:** 用可失败的原型证明实际owner资格、继承/重置、原生优先级和候选资源隔离，为CT-08提供有证据的装配输入。

**Architecture:** 每个有效样式集合/激活代持有不透明引用资格；scope根通过继承的激活属性传播，规则在目标Control上检查资格，再进入现有生成的专用Part Style。样式资产仍为原生Style；不实现第二套优先级引擎，不改变Token Snapshot。原型是临时验证，不冻结公开品牌API。

**Tech Stack:** .NET10、项目实际Avalonia12.1.4、现有Desktop Headless宿主、编译AXAML、现有Semantic Part生成器。

**Spec:** [品牌配方](../../../architecture/systems/theming/brand-recipes.md)、[渲染合同](../../../architecture/systems/theming/control-rendering.md)。CT-08负责跨根lease/协调事务，CT-09负责独立manifest，本任务不提前实现。

## 全局约束

- 继续使用现有`feature/custom-theme`和worktree，不创建分支/副本。只运行精确filter，无全量。
- 资格是表现激活状态，不运输Token，不替代ThemeContext，不更改公开子Control identity。
- Part定制仅用已生成的专用Style在AXAML写Setter；C#只创建/接线资格和读取观测结果，不找节点改样式。
- 本地Avalonia参考源码`/Users/chinboy/Projects/ReferenceProjects/Avalonia`，HEAD `8eeda4f6f5`（describe 12.1.3）；实际12.1.4包运行验证优先于近邻版本源码推断。
- 资格PropertyEquals会产生activator，实际优先级须明确报告；不能宣称与无条件Style同级。不能为了保持假设写私有API/反射selector或手动SetValue模拟优先级。
- 所有临时源/辅助资产完成后删除，常驻测试净增0。没有修改产品渲染路径时，不把原型验证称为控件视觉验收。

## Review Focus

1. 父padding、重置子只有foreground时，父规则仍被祖先StyleHost枚举；子必须恢复基准padding。
2. 继承子拥有独立集合键；继承规则必须为新键重建，兄弟和公开嵌套Control不被父目标冒充。
3. 资格提升为StyleTrigger；实例值、状态规则、普通/专用应用Style按真实优先级、host距离和顺序工作。
4. 未激活候选的Style.Resources仍可能公开；准备期必须拒绝此类资源，而不是依赖selector。
5. 同实例A→B→A、重挂载及退役恢复后不得残留旧规则/资格；候选规则不得共享可变owner。

## 文件与接口

临时目录`tests/AtomUI.Desktop.Controls.Tests/Temporary/BrandQualification/`：

- `QualificationPrototype.cs`：`ActivationKey`不透明引用，继承`ActiveKeyProperty`；`Create(Type, ActivationKey, Style)`拥有exact类型root再追加PropertyEquals；`ValidateResources(IStyle)`递归拒绝公共/合并/主题资源及未知容器；`RefreshActive(Styles, Style)`按原索引重新挂载活动图以验证原生退役失效。这些名称仅属于原型。
- `QualificationTests.cs`：实际树、原生优先级诊断、可失败反例及继承/重挂载矩阵。
- `QualificationAssets.axaml[.cs]`：编译的Style资产，产品Card的`CardHeaderStyle`/`CardBodyStyle`及应用覆盖，完全声明式Setter；不创建运行期AXAML加载器。

持久交付：本计划、`qualification-results.md`，正式设计`brand-recipes.md`及当前事实`verification.md`；唯一进度入口仍为`tasks.md`。详细文本过程放`.superpowers/sdd/qualification/`，不保留媒体/临时测试。

## Task CT-07A：失败反例与owner工厂

- [x] 写父padding24/重置子foreground红色且基准padding3的断言；先以祖先class方案运行，确认因子padding24失败，保留同一断言。
- [x] 实现最小资格工厂和继承属性，根/继承子/重置子/兄弟各用独立激活键；按有效集合为继承子重建规则；同一断言通过。
- [x] 验证公开嵌套Control、同实例A→B→A、移到兄弟再移回、移除旧规则恢复；不改变Control身份或用户局部值。

Run: `dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.BrandQualification --logger 'console;verbosity=normal'`。

## Task CT-07B：原生优先级与真实专用Style

- [x] 用GetDiagnostic记录普通Style、资格Style、普通状态/应用class规则和实例值的BindingPriority；固定同host先后、近host和ControlTheme状态对照，明确哪些覆盖成立。
- [x] 编译AXAML的CardHeaderStyle/CardBodyStyle实际命中产品模板；父、重置子、公开嵌套Card分别观测，应用专用Style在更近host覆盖；不能用代码写Part属性。
- [x] 有效集合相同值但不同引用的key不误命中；候选inactive及回退保留原实例业务属性。
- [x] 新发现：原生Remove仅逻辑子树清理造成模板frame残留；以同一断言失败证明，再用原生活动图按原顺序重新挂载，验证10代切换旧frame清除、同图frame数量不增长和实例保持。此为CT-08产品lease清理的强制输入，不声称修复任意应用Style移除。

## Task CT-07C：资源、收尾与下一任务输入

- [x] 写候选inactive Style.Resources公共键泄漏的失败断言，证明selector不是资源隔离；实现准备期递归拒绝公共Resources，显式不可变资产引用可用；拒绝发生在挂载前，活动资源不变。
- [x] 完成作者分离自审；将实际可行方案、优先级表和限制更新到正式模块文档，并细化CT-08输入，不声称品牌事务已实现。
- [x] 删除全部本任务临时源/资产/专用产物；如产生媒体，按scripted-visual-acceptance取证/审查/清理；检查无残留进程。
- [x] 清理后精确运行既有Semantic Part和ThemeConfigProvider/Scope合同，`git diff --check`及文档链接检查通过；记录数值后更新唯一进度表。

## 计划自审

本任务覆盖品牌设计§2和§3候选资源边界；§3跨根及§4–§6事务/格式由CT-08/09承担。模板结构替换不在稳定结构原型内。五项Review Focus均映射到上述断言；没有新公开API、产品Control改动或永久测试承诺。用户已选择主agent按任务清单连续执行，本子计划沿用该执行方式，不另行申请创建或提交。
