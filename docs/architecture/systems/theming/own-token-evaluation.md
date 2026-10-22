# Control Own Token 有效值求解设计

本文拥有Own Token依赖默认值、显式覆盖、兼容别名和求值版本的目标设计。Global算法、Control算法策略和appearance的既有语义由[主题运行时](runtime.md)拥有，定义继承由[继承架构](control-design-token-inheritance.md)拥有，生成产物由[Own默认值生成](../../../modules/generator/own-token-defaults.md)拥有。

设计合同与源码符合程度分开维护，当前实现及验收边界见[主题设计实现与验证边界](verification.md)。本文不将新声明或结果协议描述为已经发布的API。

## 1. 求值版本与共同不变量

| 合同 | 默认计算 | 覆盖语义 |
| --- | --- | --- |
| 现行整体evaluator | 调用完整`CalculateTokenValues(bool)`继承链 | 整体计算结束后应用Own覆盖 |
| 依赖默认值合同 | 生成期排列逐Token纯默认计算 | 显式值直接成为有效值；其余默认读取有效上游 |

两种合同拥有明确的descriptor/evaluator版本，不在同一终端定义链混跑。现行路径继续执行其base调用要求；新路径以扁平计划求值，不通过重复调用整体方法模拟依赖更新。

共同保持exact Control identity、完整Global配置空间、按名称分配Own slot、独立惰性Control槽及不可变结果。定义层变化不产生新的运行时identity，内部Part不成为Token owner。

## 2. 求值输入

一次Control计算使用：

- 当前identity规范化合并后的显式Own值及存在状态。
- 已完成该Control算法策略和最终行为规范化的Effective Global。
- 已解析的Control appearance，不再次从应用ThemeVariant推断。
- 已验证的默认计算计划、类型/范围、别名和版本元数据。

计算上下文不暴露Visual、应用服务或活动ResourceDictionary。Own依赖只表达同一终端schema内的默认输入，不形成Global消费白名单。跨owner的数据组合通过公开配置或组合控件合同表达，不增加隐式Own依赖。

## 3. 有效值算法

```text
effective(t) = hasExplicit(t)
    ? explicit(t)
    : default_t(effectiveGlobal, resolvedAppearance,
                effective(declaredOwnInputs))
```

1. 先完成逐配置层的别名规范化、字段合并与值验证，保留“未指定”和显式0、合法null等值的区别。
2. 生成器对依赖图拓扑排序，循环、未知输入、非法跨owner引用和类型不匹配构建失败。
3. 按生成顺序，对有显式值的Token直接赋最终值，其余调用强类型默认函数。
4. 默认函数只读取只读Global/appearance上下文及声明的Own参数，不接收整个可变builder。
5. 冻结并投影最终结果；惰性槽、异常保存与资源查询遵守现行运行时边界。

存在状态使用独立位集或等价表示，不能以NaN、0或null作为统一的缺省哨兵。范围按具体Token定义，允许合法负margin及可空字段；非有限数等非法值明确诊断。

未覆盖下游时，上游修改驱动其默认值；显式设置下游时始终保留。恢复某个字段的继承值必须清除显式存在状态，不是写入某个特殊数值。

## 4. 默认声明与定义继承

默认声明绑定稳定Token名称、强类型静态默认函数及明确的Own参数依赖；无依赖常量允许使用常量默认声明。函数是无订阅、无应用状态和无可观察副作用的纯计算。

新合同的终端类型使用partial承载生成的求值入口。抽象定义层导出可由Roslyn metadata读取的函数签名与依赖事实；终端合并整个定义链并生成一份计划。共享函数必须对终端生成代码可访问，不能依赖反射调用私有基类方法。

默认重定义必须显式声明且匹配同一Token语义；Token属性本身仍不能隐藏、virtual或override。无可访问默认、重复声明、版本混合及未知依赖均拒绝生成。生成期不解析任意C#方法体推导依赖，也不运行第三方默认函数。

拓扑只决定执行顺序，不重排按名称分配的slot。默认函数、依赖或输出语义变化提升独立evaluator revision并使缓存失效；只移动属性声明层不改变终端schema revision。

## 5. 可配置值分类

| 类型 | 规则 |
| --- | --- |
| Active | 有稳定语义、合法范围、默认、适用状态/尺寸、真实直接或间接消费者 |
| 可配置派生值 | 同时遵守有效上游默认和显式下游覆盖规则 |
| 内部计算值 | 从最终Token或实例状态派生，不因存在字段就自动成为配置项 |
| Legacy别名 | 映射到同一规范语义，保留规定的兼容入口，不维护独立过期副本 |
| Reserved/Deprecated | 明确不消费或迁移规则，工具不得展示为当前有效调节项 |

所有Global仍可合法配置；某个Control不消费该Global时允许无视觉效果。Active Own没有消费者属于合同断链，不能用这一Global规则解释。

## 6. 别名规范化与兼容

别名由显式schema元数据声明，不依据命名相似性推断。规范化先在每个配置优先级层内进行，再按现行字段优先级合并：

- 同一层旧名与规范名给出不同显式值时诊断冲突；同值归一。
- 不同层按正常覆盖优先级处理，高层规范名可以覆盖低层旧名，反之亦然。
- 默认计算产生的内部副本不视为用户显式配置，不能与真正覆盖竞争。
- 兼容资源键投影同一有效语义；导出字段即使在C#是internal，也不能未经版本决策直接删除。

DataGrid的HeaderBg、HeaderColor、RowHoverBg、RowSelectedBg、RowSelectedHoverBg、RowExpandedBg、CellPadding及MD/SM、BorderColor、HeaderBorderRadius、FooterBg、FooterColor、CellFontSize及MD/SM、FilterDropdownBg，均按公开语义确定实际消费者；历史Table*入口采用显式兼容关系。本文不把所有Table*都推断成同一类活跃别名。

## 7. 失败、缓存与生命周期

声明/依赖错误在构建期失败，动态配置的未知名称、类型、范围与别名冲突在规范化/绑定边界失败。实际默认计算仍按Control槽首次使用执行；不为提前发现任意第三方错误而构造所有Control。

缓存依赖有效配置、算法及evaluator修订、schema、appearance和当前registry绑定。运行时不建立依赖图、不分配逐Token闭包，不保存抽象继承图或历史Visual。只有编译workspace可变，发布后资源与值遵守深度不可变语义。

## 8. 验证合同

每条依赖边验证上游覆盖、下游显式覆盖和恢复；每个Active Own验证真实消费者及适用分支。机制覆盖0/null、明暗、Control算法、同/跨程序集继承、默认重定义、循环和版本混合；兼容覆盖同层冲突及跨层正常覆盖。

代表消费者包括按钮行高/padding、Skeleton端点色/渐变和ColorPicker尺寸；实际渲染、临时验证及最终模块确认遵守[全局主题验收规范](../../../engineering/development/theming/verification.md)。
