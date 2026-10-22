# CT-07 托管样式资格原型结果

范围：[执行计划](qualification.md)，当前`feature/custom-theme`、基线`85407138f`加本分支未提交修改。由主agent独立实施、自审；没有subagent、提交、分支/worktree创建、Computer Use或全量回归。

## 原型结论

实际项目Avalonia12.1.4/.NET10.0.8、macOS26、现有Desktop Headless per-test宿主。参考源码按本地顺序找到`/Users/chinboy/Projects/ReferenceProjects/Avalonia`，HEAD`8eeda4f6f5`/describe12.1.3；以下结论以实际12.1.4包编译运行确认，不把近邻源码版本当作同版本发布证据。

| 对象 | 实现前/反例 | 原型与实际结果 |
| --- | --- | --- |
| 父padding24、重置子只设红色foreground，基准padding3 | 祖先class+descendant方案1/1失败：子padding24，预期3 | 同一断言改按目标owner资格，子padding3、父24、子红色，PASS |
| 子有效集合、兄弟及公开嵌套Control | 只继承父键不能表示独立集合 | 独立键/继承规则重建；同名不同引用不匹配；同实例A→B→A、移动至兄弟再移回、移除继承规则均PASS，Content/Tag身份保持 |
| 工厂真实类型边界 | 保留任意资产root而忽略输入Control Type时，目标padding3而非24，1/1失败 | 工厂拥有类型root，其他类型不命中；拒绝重用已挂载Style，PASS。原生OfType按StyleKey，不把它当作Token CLR identity解析 |
| 优先级 | PropertyEquals不是透明条件 | GetDiagnostic确认资格StyleTrigger；局部11覆盖品牌24；近host应用class10覆盖；无条件Style9优先级较低；清除局部/条件恢复；同host状态先后及ControlTheme状态frame按原生规则，PASS |
| 候选公共资源 | 未激活candidate padding未应用，但资源查到candidate而非old，1/1失败 | 挂载前拒绝公共资源，old保持；子Style、MergedDictionaries、ThemeDictionaries、未知include拒绝；显式不可变brush引用可激活/恢复，PASS |
| 真实专用Part Style | 不是用找节点设属性模拟 | 编译AXAML的CardHeaderStyle/CardBodyStyle命中产品Card模板：header padding17、LightCyan；body LightYellow。重置子和其嵌套Card保留基准header padding12,0,12,0；近host普通应用专用Style19可覆盖，也按原生规则进入重置子；PASS |
| 退役模板frame | 原生Remove后，子header仍19而非基准12,0,12,0；有效失败 | 本地源码显示Remove遍历逻辑子树、Add失效遍历继承子树；同一原型按原索引重新挂载活动图后原断言PASS。连续10代17↔19切换，旧代Part frames全部清除，同一种资产图的frame数量稳定，host只剩1个活动图，控件/模板header/Content/Tag身份保持 |

这些能力是**最小原型通过**，不是产品ThemeContext/ThemeConfigProvider已接入品牌，也不是Native/Overlay事务已经交付。工厂只创建原生selector/Style并接线资格；Part Setter全部由专用Style在编译AXAML声明。观测模板frame使用临时诊断接口，运行时没有私有API/反射清理、找节点改属性或优先级引擎。

## 执行与问题处置

精确入口（临时源清理前）：

```sh
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.BrandQualification --logger 'console;verbosity=normal'
```

- owner RED1/1失败后，同断言及边界4/4PASS，3.4766s进程总时长。
- resource RED1/1失败：未激活也泄漏；新增检查后PASS。
- expanded初次编译缺临时Immutable namespace，修正；该编译失败不计有效反例。其后7项5PASS/2FAIL：一项观测错误使用Avalonia的BackgroundProperty而非DashedBorder独立BackgroundProperty，已改为真实属性；另一项是`HasResources`遗漏theme-only字典，原型检查改为显式结构检查。
- 工厂类型反例1/1失败后，拥有exact root的工厂通过。
- 原生专用Style移除残留：8项7PASS/1FAIL；同断言增加原生图刷新后8/8PASS，总4.0024s。
- 10代frame检查最初用不同资产图共享同一总frame期望，9与10不等；两种资产分别有1/2个Part规则，改为同图稳定比较，同时逐项断言所有旧代Part Source已消失，未放宽旧frame清除或属性断言。
- 最终临时原型 **9/9PASS、0FAIL、0SKIP，总6.2387s**；单例主逻辑186ms、10代210ms、产品Part18ms，其余1–4ms。并非全量测试结果。

## 交给CT-08的具体输入

1. 内容身份与每context/代的引用激活键分离；继承子重建自己的有效集合，不能共享父资格；稳定Token Context不换identity。
2. 工厂拥有注册类型root；既有生成的Part Style可直接作为子规则，不需要在每个Part上增加资格入口。原生StyleKey与真实Token identity分别处理。
3. 资格规则为StyleTrigger，实例/应用/状态按正式优先级表验收；配方覆盖root属性时要审查ControlTheme状态，不能默认disabled仍能压过品牌Setter。
4. 准备阶段在挂载前验证完整Style图，检查直接/合并/主题资源及未知容器；不得依赖`HasResources`或inactive selector。资产图不对外暴露可变引用。
5. 退役必须先失活再清除frame；活动图原索引重挂是已证明的候选路径。CT-08仍须验证实际root/Provider/Native/Overlay上的失败恢复、stamp、无await时序、性能、交互和释放；本任务没有修复任意应用Style的原生Remove行为。

## 测试处置与验证边界

临时原型不是产品实现，也不进入全局常驻集：全部临时源和AXAML在验收后删除，常驻测试净增0。产品Generator、Control、ThemeContext、Provider及渲染代码未修改，只有正式设计和任务文档更新。

本次没有截图/录屏或媒体产物，没有启动原生Gallery；Headless证据只证明样式/模板属性、原生优先级及实例/frame状态。**没有新增产品视觉一致性、无闪烁、Native/Overlay、NativeAOT、其他OS或性能通过结论**。这些义务由CT-08及对应家族/CT-25继续完成，95个产品Own终端仍为V1。

清理后验证均为精确filter：

- `CardSemanticPartTests`：11/11PASS，0SKIP，总3.1775s。
- `ThemeConfigProviderTests|ThemeScopeGraphTests`：21/21PASS，0SKIP，总2.0132s。
- `Generates_Public_Semantic_Style_Types_With_Owner_And_Fluent_Route`：1/1PASS，731ms用例/总1.6929s。
- 5份本任务文档的60个本地文件链接通过，`git diff --check`通过；清理后测试DLL不含临时namespace，6份临时源/AXAML不存在。
- 作者另起步骤自审完成：本任务范围内无未处置阻断项。这不是独立审查，也不改变上述产品/平台未验证边界。CT-07最小原型完成，进入CT-08装配与事务设计。
