# CT25-FW1：Avalonia 恢复语义源码验证计划（主体已批准）

**Goal:** 用实际12.1.4源码验证coercion当前值/base值恢复及原生Style移除的根因修正，为NavMenu和完整家族验收准备依赖方案。

**Source target:** `2d79ba55fae1b1e71c69f93a668080c9029e6a31`加下述两个文件的限定修正；这是本地源码候选，不声称某个公开版本已修复。

**Current dependency:** AtomUI仍为Avalonia12.1.4，由`build/Versions.props`拥有。本阶段不改变版本声明、正式PackageReference或NuGet缓存，不发布NuGet，不创建任何提交。

## 工作位置及恢复

- AtomUI研发目录保持`/Users/chinboy/Projects/dotnet/AtomUIV6-custom-theme`，现有`feature/custom-theme`，不改变该checkout的HEAD。
- 仅使用已有参考目录`/Users/chinboy/Projects/ReferenceProjects/Avalonia`。它目前干净、detached HEAD为`8eeda4f6f546165b3f72e63c9f42247abb306905`，与实际依赖版本存在其他生产差异，不能直接据其构建认证12.1.4。
- 获批后先复核参考目录仍干净，再临时detached checkout已存在的`2d79ba55…`，不创建分支/worktree/clone或另一个源码副本。
- 验证后删除本片临时源码，反向应用本片两个文件的修正并确认源码干净，再恢复原detached `8eeda4f6…`。失败也恢复；若检测到其他任务/用户改动则停止，不reset或覆盖。
- 外部探针、产物和媒体放系统临时目录。参考源码仅留可审查的文本patch与结果，不提交、不推送。

## 限定修正

1. `src/Avalonia.Base/PropertyStore/EffectiveValue`泛型实现的六参数`SetAndRaiseCore`：base分支写`_baseValue = bv`及`_uncoercedBaseValue = baseValue`。两行源码patch已固定为`c78ad3bcde7a8ad3de4f562f20669d26675eb8cf1349fed2200a578e671e0dcb`；实际net10/net8 IL和最小6项A/B证据见[根因补充](nav-menu-framework-coercion-results.md)。
2. `src/Avalonia.Base/StyledElement.cs`的`DetachStyles`：退役遍历与现有StylesAdded/InvalidateStyles使用同一继承子树，清除实际已应用的模板/继承节点Frame。保留按Style来源筛选，不清除无关Frame，不增加全局扫描、重套模板或产品找部件改属性。

第二点先有同条件失败再实现；若真实树或回调反例否定该方式，回到根因调查，不吞异常或删断言。

## 执行步骤与验证

- [ ] 1. 再核对两个仓库状态、当前/目标ref和源码hash，记录临时参考HEAD切换；实际12.1.4为基线，当前8eeda与12.1.4差异不得混入候选。
- [ ] 2. 在参考测试项目`tests/Avalonia.Base.UnitTests/Temporary/AtomUIThemeRestore/`增加临时检查，头部注明验证后删除；先运行并保留有效RED。覆盖Animation360/Local300、coercion48→80→清除→释放Animation；运行中base输入变化、绑定撤销与通知；Style移除的模板/继承节点、无关Style保留、重加与释放。
- [ ] 3. 只实施两个源文件的最小修正，相同测试转GREEN；分别保留单变量与组合结果。没有公开API、类型、属性默认、TFM、原生资产或引用包变化。
- [ ] 4. 运行已有`AvaloniaObjectTests_Coercion`与`Styling.StyledElementTests`两个精确测试类，及本片临时类。使用该源码的MTP runner，先`--help`确认其类过滤参数；不得用零用例、Skip或整个Avalonia全量代替精确验证。
- [ ] 5. 构建`src/Avalonia.Base/Avalonia.Base.csproj -c Release -f net10.0`及`-f net8.0`。使用既有输出结构，记录实际产物；比较与当前包的公开/受保护API及程序集身份，任何不符先调查，不直接替换消费端。
- [ ] 6. 外部临时消费者显式选择候选Base产物，记录实际加载/ILC输入身份；复核原NavMenu和卡片Style移除反例，区分框架修正与NavMenu仍需修改的取值算法。普通JIT与当前Mac NativeAOT分别验证，不把二进制诊断副本当作最终产物。
- [ ] 7. 按影响追踪检查直接coercion/GetBaseValue候选及Animation期间base更新、默认Transition和动态Token更新；31文件/55引用只是候选输入。只跑有明确路径的聚焦检查，不自行全量回归。
- [ ] 8. 清理临时源码/工程/媒体/进程，恢复参考checkout和原文件；记录实际通过数、原失败、API/ABI比较及未验Host。AtomUI产品依赖仍保持原12.1.4。

上游执行入口为现有`tests/Avalonia.Base.UnitTests/Avalonia.Base.UnitTests.csproj`，精确类为`Avalonia.Base.UnitTests.AvaloniaObjectTests_Coercion`、`Avalonia.Base.UnitTests.Styling.StyledElementTests`及新增`Temporary.AtomUIThemeRestore`。预期旧问题RED、修正后GREEN，两个原类保持通过；实际runner参数以本ref的help校验后记账。

## 出口与后续边界

本阶段交付源码patch、相同触发条件的前后证据、兼容验证和正式依赖接入所需条件。正式交付的修正版依赖来源/版本仍须明确并单独审查；不把本地修正测试、旧CI或编译成功写成NavMenu、Style系统或完整CT14/CT25已完成。

执行本计划需要明确批准，因为它会临时切换并修改当前AtomUI目录之外的既有Avalonia参考checkout；原27文件的Git授权已经执行完毕，不覆盖本计划。

## 执行登记与新增前置条件

主体计划已获用户明确批准（问题`call_9adba39d1c044b9da5e1e7ea91aa1fb2:0`）。已在原有参考目录切到精确12.1.4基线，生产源码尚未修改；外部SDK artifacts关闭hardlink，避免改写正式缓存。

基线测试项目构建失败，原因是固定生产源码依赖`external/XamlX`未初始化，缺少`XamlIl`类型；该失败不是有效测试RED。其gitlink为`d7e37ca63dc9b13cdc95ca165938d4904fa0eddf`，来源`https://github.com/kekekeks/XamlX.git`，Build.Tasks及Runtime XAML Loader直接编译其中源码。规定的本地查找路径均未找到XamlX，现有Avalonia仓库也没有缓存的子模块Git对象。

**待批准的唯一范围补充：** 临时取得这个固定子模块源码到既有`/Users/chinboy/Projects/ReferenceProjects/Avalonia/external/XamlX`路径，仅取固定commit、保持detached，不创建研发分支或额外worktree。它属于对当前“不clone/不新增源码检出”限制的一个明确例外，未获批准前不执行。验证结束恢复为未初始化状态并删除本片新增源码/Git对象；其他子模块不在本次补充范围内。

正式依赖、NuGet发布、Git提交/推送及生产API边界保持主体计划原约束；不以包DLL或剔除测试替代缺失的源码依赖。
