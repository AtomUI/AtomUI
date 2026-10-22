# CT-08A 内部样式资产与准备边界实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: `superpowers:executing-plans`。用户已要求主agent独立连续执行；不委派、不提交、不创建分支/worktree。

**Goal:** 将CT-07证明的资格与资源约束落为Core内部准备组件，为CT-08后续事务/lease共用，尚不开放品牌选择API。

**Architecture:** 纯资产描述保存类型/修订和静态Style工厂；UI线程创建独占图，完整检查后注入原生资格selector。激活键不引用Control/Context，不把Style加入Snapshot。完整CT-08的context接线、root/scope/catalog协调、跨根lease/结果仍须后续独立细化，本计划只拥有A子项，不能据此关闭CT-08。

**Tech Stack:** 现有Core/Avalonia12.1.4、NET10/NET8、Headless和编译AXAML。

**Spec:** [品牌设计](../../../architecture/systems/theming/brand-recipes.md)、[CT-07结果](qualification-results.md)。

## 全局约束与复核点

- 不改变任何现有ControlTheme、模板、公共控件属性或Token编译路径；没有接入产品host前不宣称品牌应用已支持。
- 不清理任意应用Style，不更改原生优先级。保留资产原root条件，工厂追加exact类型和资格条件，不能静默抹除状态；类型声明一致性仍由注册资产构建合同负责，运行期追加类型条件防止跨owner命中。
- 所有mutable Style节点仅准备一次，拒绝已挂载、重复引用、共享/循环或未知节点。当前profile只接受Style节点，不接受Styles/Include容器。使用弱键标记已准备的图，不持有Visual/Context、注册全局资源事件或引入强引用缓存。
- 任意层的直接/合并/主题公共Resources必须在挂载前拒绝；不使用HasResources作为唯一判断，不执行未知Include。
- 测试先临时，写失败再实现；涉及内部和宿主的探针放所属Temporary目录。无产品消费时不保留原型矩阵进全局集，最终清理后运行精确模块filter。

## 文件与内部接口

- `src/AtomUI.Core/Theme/Styling/ThemeStyleActivation.cs`：无数据引用的sealed引用键。
- `src/AtomUI.Core/Theme/Styling/ThemeStyleQualification.cs`：继承的`ActiveProperty`，类型`ThemeStyleActivation?`；只表达表现激活状态。
- `src/AtomUI.Core/Theme/Styling/ThemeStyleAsset.cs`：`ThemeStyleAsset(string id, string revision, Type controlType, Func<Style> factory)`；只读元数据；`Style Create(ThemeStyleActivation activation)`验证UI访问、调用工厂，检查全图后在原selector后追加exact OfType+PropertyEquals。
- `src/AtomUI.Core/Theme/Styling/ThemeStyleGraph.cs`：`Prepare(Style root, Selector qualifiedSelector)`验证全图，赋值资格后通过原生Children重赋值重新验证Nesting，最后用ConditionalWeakTable弱键标记全部Style；任何不合法时不改host或已准备标记，恢复原root selector。节点父/owner拓扑必须与容器一致，拒绝重复节点。

以上全部internal，不是CT-09品牌manifest/公共ABI。Factory只提供新未挂载图，不获得host参数；工厂自身的任意外部副作用不在框架可恢复范围内。

## Task CT-08A

- [x] 在`tests/AtomUI.Core.Tests/Temporary/ThemeStyleAssets/`写目标合同测试并运行，确认新接口缺失；覆盖参数、资格基准/候选/恢复、资源/未知节点、已有root条件保留、类型约束、挂载/共享/重复图、子规则不得绕过资格，以及弱键不保留Style的GC反证。
- [x] 实现上述4个内部组件，不添加Manager、Provider或Snapshot接线，不偷偷执行资源查找或运行期AXAML。
- [x] 相同临时测试通过；补实际编译AXAML的typed root、嵌套disabled状态与既有生成CardHeaderStyle验证，确认未丢状态或使用代码改Part值；用独立Thread及不同线程ID证明UI线程以外调用Create拒绝。
- [x] 决定测试去留并删除本任务临时源/AXAML，完成Core精确验证、NET8编译和diff/文档检查；若保留共享契约，先写独立价值、全局必要性及成本理由。
- [x] 更新正式品牌实现边界、结果与唯一进度表，作者分离自审；CT-08保持进行中，下一子项才接context/lease与原生frame退役。

Run: `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.ThemeStyleAssets`。

## 自审与后续出口

本子项只处理准备/工厂；不把root/scope/catalog任一入口遗漏后仍宣称协调完成。CT-08后续必须实测现有ThemeContextLease、Native/Overlay实际owner及已打开host，并验证CT-07原生模板frame退役、stamp/重入、CommitCore静默和Publish不可回滚、故障分类/释放/性能/视觉。没有公开API或格式冻结，正式设计的这些出口仍全部有效。

实施中的设计修正：编译器AVLN2000证明selector-free root无法承载原生AXAML的`^:disabled`，即使声明SetterTargetType也不足。改为保留原selector并追加注册类型条件；原有严格拒绝root的方案有失败证据后撤销，不强制作者用代码生成状态Style。原生SetParent的Nesting重验证仍须保留，防止先构造无selector root的代码资产携带未限定子规则。
