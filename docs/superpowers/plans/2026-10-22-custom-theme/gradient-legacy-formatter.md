# CT19 C3：历史formatter入口的委托兼容

状态：强类型适配、前后运行反例、合格明暗渲染及原生managed／AOT已完成；见[结果](gradient-legacy-formatter-results.md)，清理后20项、NET8及成员审计均通过。B08与C1／C2所示视觉变化已获用户限定批准；本项将单独记录新源码资格，不扩大旧证据。不创建分支、worktree或提交。

## 已确认源码事实

GradientColorPicker.ColorTextFormatterProperty注册为AttachedProperty<Action<LinearGradientBrush?, ColorFormat, Controls>?>，GetColorTextFormatter返回同一Action。公开历史SetPresetColor接受Func<LinearGradientBrush?, ColorFormat, Controls>并直接写入该属性；两者CLR委托形状不兼容。当前调用链没有适配。正确Action路径的既有B08结果不能证明此Func入口可用。

## 最小兼容方案

保持既有公开Func方法的名称和二进制签名、AttachedProperty的Action类型及资源／Part合同；为Func建立强类型Action适配，不使用DynamicInvoke或反射。Func输出表示替换后的Controls，先取得结果再清空原输出，以支持重复调用和复用已归属控件；若返回同一输出集合则不重复添加，空结果清空输出。null历史调用保留清除属性的行为。不添加同名Action重载，避免null调用变成歧义；本切片不以修名引入公共破坏。

## 执行门禁

先用所属测试Temporary目录建立旧入口非空委托的运行反例，分别锁定null清除、正确Action兼容、重复值／格式更新、返回新／复用／空／同一集合、Scope继承和退出后清理。再实现适配并取得前后证据。若所有权／回调顺序与上述推断不同，先修订合同，不用清空内容规避问题。

既有默认与正确Action画面须保持，新增有效Func路径验证预期输出、明暗、选择及关闭／重开；A/A与真实1DIP检错按已验证宿主执行。Native managed／AOT验证强类型委托和两种实际弹层；有视觉变化按主题规范独立审查。必要的收集性反例先推进真实渲染退休，避免把合成器尚持有的输出当作产品泄漏。

更新原有实现／API文档，验证后删除临时代码／工程／备份与专用产物，清理后的精确ColorPicker20项及NET8为最终门禁。此前B08/C2证明以各自源hash保留；涉及B08文件的新改动必须重新资格确认，不能沿用其视觉批准范围。完整CT19其余Own／Global／Part和各平台仍独立验收。
