# NavMenu：实际依赖的coercion根因补充

本片是第二十二批提交346732602a之后的只读调查及临时诊断，不在已批准27文件内，也没有新的提交授权。产品、依赖版本、NuGet缓存中的程序集和参考仓库均未修改。

## 实际二进制与源码身份

当前Avalonia12.1.4程序集的InformationalVersion为`12.1.4+2d79ba55fae1b1e71c69f93a668080c9029e6a31`。在已按规范定位的本地参考仓库`/Users/chinboy/Projects/ReferenceProjects/Avalonia`中，该确切commit存在，提交标题为`Version 12.1.4`；直接git show读取，没有切换分支、创建checkout或fetch。

| 目标 | Avalonia.Base.dll SHA256 |
| --- | --- |
| net10.0 | `99bf47f44f8e6b2c91f96f175dcf7efa8ba64267a1acd17faa05266d7210b72e` |
| net8.0 | `ffaf5263f964ea313ed29361c0dc86ed20fa7fda1473aefa687fde3a90622f8d` |

net10文件与最后一次AtomUI测试实际依赖逐字节相同。两个目标的元数据读取均确认以下写入；反射探针先因重载筛选不充分退出的记录保留，成功结论来自之后的只读Cecil元数据检查。

## 两处写入问题

确切版本的``src/Avalonia.Base/PropertyStore/EffectiveValue`1.cs``，六参数`SetAndRaiseCore`分别计算当前coerced值`v`及base coerced值`bv`，但base变化分支却：

- 将`v`写入`_baseValue`，而非刚比较的`bv`。
- 将`baseValue`写入`_uncoercedValue`，覆盖当前优先级输入；对应位置应是`_uncoercedBaseValue`。

对应源码的两行诊断修正补丁已固定，SHA256为`c78ad3bcde7a8ad3de4f562f20669d26675eb8cf1349fed2200a578e671e0dcb`。补丁仅供审查，未应用到参考源码或产品依赖；它不是升级授权。

实际net10/net8 IL均在`IL_0094`加载local3（v）后写`_baseValue`，在`IL_00aa`加载baseValue后于`IL_00ac`写`_uncoercedValue`。因此结论不是从另一个参考版本推断。

## 单变量及组合实验

独立临时控制台仅引用相同Avalonia包，使用普通Border派生类覆盖Width coercion，不使用NavMenu。每次完整历史相同：Local300→Animation360→coercion约束48→80→清除约束→释放Animation。每阶段检查实际Width，失败返回非零。

仅在系统临时目录中的测试副本上改指令，不修改NuGet缓存；每个变体从相同原DLL重新产生：

| 变体 | 清除约束，期望360 | 释放Animation，期望300 | 6项检查 |
| --- | ---: | ---: | --- |
| 原版 | 300 FAIL | 300 PASS | 5PASS/1FAIL，exit1 |
| 仅修正base coerced写入 | 300 FAIL | 300 PASS | 5PASS/1FAIL，exit1 |
| 仅修正raw base字段 | 360 PASS | 360 FAIL | 5PASS/1FAIL，exit1 |
| 两处同时修正 | 360 PASS | 300 PASS | 6PASS/0FAIL，exit0 |

这证明两处写入各自造成一个状态恢复缺陷，不能只改一处。它仅验证该最小反例的因果性，**不是**Avalonia依赖兼容验收或NavMenu修复完成。

## 处置边界

- 元数据检查和A/B工具的外部临时工程已删除，所有变体DLL随工程删除，NuGet原文件SHA保持不变；只保存文本指令、数值和hash。
- 没有升级、降级、发布或替换AtomUI实际使用的包，没有给产品增加反射、私有字段访问或IL重写。
- 原NavMenu镜像候选还有独立的低优先级Style260覆盖Local300问题；框架两处修正不会自动证明该候选正确，仍须修订取值设计并重跑完整15项及后续UX/AOT检查。
- 当前版本为12.1.4，交付目标版本/来源尚未选定；不能凭本地原型宣布某个公开版本已修复。若调整依赖，需先有精确目标源码及完整差异、影响追踪和经审查的升级/修复计划。
- AtomUI中显式coercion、CoerceValue和GetBaseValue候选搜索得到31文件/55引用，只是影响追踪的输入。六参数setter还处理Animation与base输入同时变化，不能把影响闭包限为这些显式调用，默认Transitions和动态Token更新也须核查。
- 保持现有用户Width/MinWidth合同是默认约束；不能为了绕过框架问题，擅自改变属性读取语义或把问题移到MaxWidth。

详细记录在`.superpowers/sdd/nav-menu-initial-motion-audit/dependency-metadata-*`和`coercion-ab-*`，原始失败没有被覆盖。
