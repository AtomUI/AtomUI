# CT13 AM4：复合输入剩余表面 Global

> 在已授权原目录/分支中使用 executing-plans、TDD 与实际绘制验证；不委派、新建分支/worktree或提交。AM2/AM3视觉仍独立待批。

**Goal:** 证明并接通 Auto 家族 placeholder/popup 背景及 Mention popup 背景/loading margin 四处剩余 Shared 入口。

**Architecture:** Auto共享主题用已有OwnerTokenResource解析三公开owner；Mentions用typed资源。四Shared来源更换保留key/selector；实际发现私有输入的主题覆盖占位色投射，需要保留父标准API的声明式Local绑定。Spin自身绘制与动画不变，父拥有其布局margin。

**Tech Stack:** 当前 AXAML、NET10 临时专项、Skia 正常动画、当前 Mac managed/AOT、NET8。

## 顺序与验收

1. 原版9个真实消费者RED（1s）：三Auto placeholder、四popup、Mentions加载Margin及额外发现的Mentions placeholder。首次Spin查错树的无效FAIL保留，改查popup.Child后取得Margin24→预期1/2/3/4的有效RED；Mention placeholder不能当正对照。同scope隔离、热更新/透明/非均匀margin/清除与专用Style优先级分别核对。
2. 四资源来源更换及实际占位色投射，不加属性、Own、key、状态或代码绑定。同历史默认、明暗、AA/1DIP复位、实际输入保持；配置视觉单独审查。原暗局部主题混合白表面问题保留。
3. 当前 Mac managed/AOT 实跑、NET8；删除临时源/原生工程/媒体后精确 AutoComplete/Mention/直接消费者，常驻测试净增0，CT12六临时文件保留。
4. 作者另步自审，具体视觉交用户限定批准；不关闭完整 CT13。

2026-10-30工程、专项与清理出口完成，见[AM4结果](input-autocomplete-mentions-surfaces-results.md)。8配置/2加载布局及自然12对1/5点边缘见[待批准限定表](input-autocomplete-mentions-surfaces-visual-review.md)；不把本包通过代替完整CT13。

## 只读后续审计

Form8/Upload13仍V1，均Global-only或常量，必须保持精确公式。Form.VerticalLabelPadding没有生产直接消费，先核对历史/文档/布局再分类，不直接接入改变默认。另7+13有typed源码入口，运行、Part与状态尚待核验；V2默认分包不能替代完整控件。

真实发现：三Auto的root值正确但private/leaf仍原默认#3f000000，TemplateBinding未越过内部输入主题；用已验证CompiledBinding Priority=LocalValue投射各自公开PlaceholderForeground。Mentions无公开PlaceholderForeground，原模板未投射父拥有颜色；普通父Setter也未越过private TextArea主题。跨两层/template/的默认ControlTheme selector被Avalonia原生拒绝，已删除。最小实现是在MentionTextArea属性上声明单typed资源输入的Local MultiBinding；内部无状态brush转换只承接该优先级，DynamicResource在模板内会自动降低至Template且没有Priority属性。未修改Core或公共TextArea主题/公开API。配置、foreign身份、Local与专用placeholder Style恢复需验证。其余body颜色、selection/caret的共享身份另做实际审计，不以placeholder闭环代表全部内部Global完成。
