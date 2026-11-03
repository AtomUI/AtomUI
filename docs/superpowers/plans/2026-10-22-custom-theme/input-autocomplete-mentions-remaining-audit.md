# CT13 AM5/FU 接续只读审计

2026-10-30，AM4工程与限定审查独立处理，不以分包关闭家族。

接续：下列16项原始失败已由[AM5实际owner结果](input-autocomplete-mentions-global-owner-results.md)闭环；原文保留作前证。AM5最终39专项、managed/AOT各6117、清理后Desktop157/Core22通过，具体视觉待审；本审计余项现在继续Form/Upload及家族出口，不重做旧反例或把其视作未修。

当前四root的ColorText、SelectionBackground、SelectionForeground配置经真实注册合法，算法Disabled保持原值。实际TextPresenter正文、实际内部输入CaretBrush、TextPresenter选区背景/文字分别核对，16项均有效RED、1s：正文/caret仍为默认#e0000000，选区仍原primary/白色，未进入预期#c332a1/#123456/#fedcba。临时审计仅记录缺口，不在AM4修复或计作通过，清理时删除。

源码事实：私有输入复用LineEdit/SearchEdit/TextArea主题与StyleKey，其typed Global仍读取另一公开输入身份；OwnerTokenResource本身也支持StyleKey回退，不能只机械换引用便假设已经解析父owner。AM3字体、AM4占位色分别通过父投射解决对应维度，不代表正文/状态/选区/光标全部完成。后续设计应统一核对公开root标准Foreground优先级、普通/错误/警告/禁用状态、真实模板树及私有输入身份，再选择最小声明式方案；不得复制状态机或用代码写部件属性绕过。

Form8/Upload13仍V1源码，公式均只Global/常量，无Own边；八加十三字段的V2机制、精确默认/key/ID/slot、配置清除及实际Mac消费需要单独验证。Form.VerticalLabelPadding生产搜索只有声明和默认，文档已说明当前只用VerticalLabelMargin、保留Padding稳定契约；优先按Legacy/CompatibilityRetained核对资源兼容，不直接增加8 DIP布局改变。另7+13有typed源码入口，但运行状态与专用Style尚未验收。

执行依赖：先统一复合输入内部Global责任并验证既有状态/标准API；再Form/Upload默认协议与兼容字段分类、实际消费者；最后汇合其他选择控件的剩余字体/标签/候选与Semantic route、最小充分状态矩阵和完整CT13出口。已通过分包不得重复；已有CT25像素/性能和本轮原始自然边缘保持开放。
