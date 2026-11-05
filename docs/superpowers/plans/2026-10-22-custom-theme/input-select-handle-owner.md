# CT13 S4：三选择控件手柄的父组合尺寸与反馈间距

本包接续已清理的S3，不借其视觉批准；完整家族与CT25仍开放。

## 已核对事实与合同

- SelectHandle是三公开根共用的内部目标；IconLayout仍读Shared.SpacingXS。需要经公开Form/FormItem验证成功反馈与箭头同时存在的真实间距，不能直接给内部节点灌状态或只断言不可见面板的属性。
- OpenIndicator是独立公开IconPresenter，其尺寸在父SelectHandle模板内以Shared.IconSize固定14；实际Icon由IconPresenter以Template优先级转发，显式子值保持优先。不能在公开Presenter目标上机械改OwnerTokenResource，否则会读Presenter自己。
- AddOnDecoratedBox已将实际owner的ColorTextQuaternary投到右侧附加区，SelectHandle箭头经继承的TextElement.Foreground读取；InputClearIconButton也已有真实owner尺寸/颜色。它们先作正对照，不能因看到Shared字样就再改颜色或清除逻辑。
- 若尺寸反例成立，优先考虑在内部SelectHandle声明一个父组合尺寸输入，由ControlTheme读取真实owner，再通过TemplateBinding到实际公开Presenter/图标尺寸。它不构成新Own字段、缓存或身份；不增加包装层、不更改Core/IconPresenter的已核定Template优先级绑定。

## 执行门槛

1. 三真实公开根：父IconSize各23/19/17而箭头仍14的反例；自身Presenter/Icon身份干扰输入、0/清除/根尺寸与原模板树。颜色和真实hover清除尺寸先验证正对照。
2. 公开Form/FormItem成功反馈场景下证明SpacingXS不同值的实际两可见区域间距；保留反馈内容、标签、Root宽高、输入与弹层条件，不修改反馈状态机。
3. 修改前明暗、尺寸/配置/0/恢复、实点清除、专用Suffix Style的编译命中/撤回和关键Setters形态；有意变化按具体完整帧独立审查，不扩用S3批准。
4. GREEN、managed/AOT/NET8、生命周期/成员及正式文档自审；临时源/工程/媒体清理后精确三模块，禁止Git或全量回归。

## 接续事实与实现决策

原9项纠正场景后6FAIL/3PASS、1s：三箭头实际Width=14，期望23/19/17；三公开Form/FormItem成功反馈场景中实际间距仍8，期望18/11/7。默认成功反馈模板实际绘制CheckCircleFilled，先前把SuccessFeedback内容当作“OK”文字的观测错误已纠正；隐藏过滤输入另有清除按钮，观测器已限定真实可见节点。初期namespace/LabelText/非泛型IEnumerable的编译准备失败均保留，不计产品RED。

扩展12项为6RED/6正对照、1s：原箭头颜色、真实hover清除尺寸/颜色已正确；三编译专用Suffix Style的类型/关键Setter、覆盖及退出资格恢复当前值均通过。只修复尺寸与间距，不调整颜色/清除状态机。

API核对：公开/受保护成员、事件、公开接口、模板部件、伪类、Token及资源key不变；拟新增内部类SelectHandle的非空double StyledProperty IndicatorSize，注册默认0由ControlTheme投射实际owner.IconSize（常规14），TemplateBinding到当前模板内实际目标，影响测量。新增内部渲染输入属于同模块主题合同，不形成公开配置identity或Own字段。所有取值/绑定在AXAML，未加C#同步、镜像旗标或缓存。

Ruling：使用一个内部父组合输入，保留现有模板树与Name，不新增包装层，也不改变Core/IconPresenter已核定的Template优先级转发——后者需保留公开子值优先与原生命周期；代价是增加一项内部Avalonia属性，必须验证0/清除、模板/AOT及同实例布局更新。研发修复已在当前任务授权内，新视觉仍以具体范围作最终独立审查，不将内部新增等同于视觉批准。

当前结论：13PASS13s、最终managed/AOT各4202、NET8零19.88s；原文件成员门禁缺分区已按50个完全相同语法指纹整理，1文件/1控件/0问题，原47成员代码不变。临时5源/426文件工程/201媒体清理后精确三模块148PASS6s。具体[26对视觉](input-select-handle-visual-review.md)已独立批准，[完整结果](input-select-handle-results.md)保留计数、原始失败和范围；不关闭完整家族或CT25。
