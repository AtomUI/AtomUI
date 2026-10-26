# CT14 N3-TR：Transfer 默认函数迁移

延续[容器 N3 计划](container-consumers.md)，在当前feature/custom-theme／现有checkout单agent实施，不创建分支/worktree、提交或推送。本片不纳入第十七批14文件审查范围。该批原始blob已保存为可校验补丁；本片CSV／总状态回填作为后续工作树差异保留，获得旧批Git授权时只应用对应补丁，不捎带本片。

## 审计合同

具体公开owner为ListTransfer、TreeTransfer、TransferListView；AbstractTransfer为抽象状态/模板协调基类，不虚构TransferToken或独立Transfer控件。现有正式Token文档引用不存在TransferToken.cs/TransferListBox，须按真实source纠正。

共11字段：ListTransfer和TreeTransfer各5项，TransferListView1项。ListWidth=180、ListWidthLG=250、ListHeight=200；HeaderHeight=Global.ControlHeightLG；HeaderPadding=(Global.UniformlyPaddingSM, Ceil((Global.ControlHeightLG-Global.LineWidth-Global.FontHeight)/2))；PaginationMargin=(0,Global.UniformlyMarginXXS)。没有Own→Own边，HeaderPadding不擅自改读Own.HeaderHeight。IsStretchView/分页宽度分支、SizeType转发/固定Small按钮、两视图当前身份保持。

L0内部机制迁移：三个Token接入DependencyDefaultsV2/Revision1的11静态函数；字段、类型、名称、ID/资源key/枚举数值/dense slot、默认结果及明确Own/0/清除语义不变。没有公开签名、Part、伪类、Binding、模板、布局、交互、订阅或缓存变化；没有真实消费失败之前不声称修复产品bug。

## 验证步骤

1. 原Transfer精确模块基线；临时测试记录明暗22项真实资源/类型/枚举数值/slot及三个实际descriptor。V2合同先RED，与原消费兼容分开报告。
2. 核对Global、Control专属Global、Own0/零间距/明确下游与清除、同实例重挂、分页宽度分支及实际两装饰器/header/pagination消费者；保持原数据、滚动、过滤及目标key状态。
3. 只迁移11默认函数，临时验证同一合同GREEN并逐项比较基线。默认/配置/恢复、分页/选中状态同历史真实明暗窗口Skia自然A/A与1DIP，新旧差异须定位；所有Semantic定制只能用生成专用Style，不找节点回写样式。
4. 适用当前Mac managed/NativeAOT资源与同实例状态，记录平台/最终像素边界。临时测试、外部工程、媒体和专用工具清理后，只跑Transfer精确模块、NET8 Release、三完整Token文件成员审计、正式文档/链接和作者另步自审。

不以本片关闭Transfer完整状态/Part/交互/平台、N2/N3/N4或CT14。任何超出机制迁移的行为/视觉变化先审查；NavMenu首次展开设计和第十七批Git仍各自待批准。
