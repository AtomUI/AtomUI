# CT-10 / CT-12 字体族的可选精确消费

接续[按钮消费](button-consumers-results.md)，保留祖先字体、原生属性优先级和未配置默认像素；不无条件给每个根设置Global FontFamily。先用Button、Dropdown、Hyper、Split、Option和OptionGroup真实实例证明配置未消费，并保留原继承正对照。

- [x] 实际失败/通过：祖先字体A→控件Global字体B→清除回A、祖先实时变化、显式配置回默认/同值与缺省区分、本地属性/Style优先级、兄弟identity和同实例状态。
- [x] 现有WhenExplicit扩展到已注册控件的类型化Global key（共享及Control正规化配置中直接字段存在）；Own/alias保持。普通Shared key不冒充控件字段，wrong owner/slot仍拒绝，缺省返回found+Unset阻止外层误回退。文档明确仅为输入存在性，不推断算法来源或保证Unset单独保留原生继承。
- [x] Core增加内部强类型继承字体源：各已触及文本owner只暴露受保护真实InheritanceParent的内部接口getter，编译Self observable负责观察父字体；挂载/重挂刷新，分离停止父订阅，原生绑定释放时移除事件。不使用反射、逻辑父猜测、全局强表或控件属性写入。
- [x] 最终FontFamily仍由ControlTheme MultiBinding选择：真实继承字体、WhenExplicit存在位、普通资源值。保留原生本地/Style和资源查找。已有Button FontWeight机制不在本包无必要重写。
- [x] 明暗默认零未解释差异、可见字体变化/同实例恢复、scope移动/重模板/长寿命父移除GC，实际当前Mac NativeAOT及NET8。清理临时源/媒体后focused、正式模块文档、审计和总表同步。

该存在位合同只表示正规化配置中的直接字段；不把所有算法输入或颜色变化都推断成显式字体配置。其他未触及owner继续随家族处理，本包不宣称全控件字体验收完成。

实际失败/数值、AOT、临时清理与最终聚焦出口见[结果](buttons-font-family-results.md)。
