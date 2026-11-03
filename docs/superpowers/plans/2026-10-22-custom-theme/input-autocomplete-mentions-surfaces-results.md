# CT13 AM4：复合输入表面 Global 结果

状态更新：最后五表已于2026-10-30获用户明确限定批准，CT13既定范围已完成；下文原始执行/失败记录保留，CT25/FW1继续独立。见[批准汇总](input-final-visual-review.md)。


2026-10-30，现有目录/feature/custom-theme，单agent，无新分支/worktree或提交。工程专项已通过，限定配置视觉及自然边缘待用户审查；AM2/AM3仍各自待批，完整CT13开放。

## 实际改动与反例

原版9个实际消费者FAIL、1s：三Auto placeholder、四popup背景、Mentions加载margin与额外发现的Mentions placeholder。首次Spin从root视觉树查找失败属于夹具错误，修正为popup.Child后取得实际margin24与预期1/2/3/4的有效RED，原日志保留。

四处Shared改为公开owner有效Global：Auto共享主题的placeholder/popup背景用OwnerTokenResource，Mentions背景与加载布局margin用typed资源。root资源修正后5项通过，但4占位色仍失败；诊断确认三Auto的root值#213243正确，private/leaf却保持#3f000000。三具体模板增加声明式Local编译绑定，投射既有公开PlaceholderForeground；Mentions没有该公开API，用单typed输入的Local MultiBinding投射到既有私有属性。内部无状态brush转换仅承接这一声明的优先级；模板内DynamicResource自动降至Template且无可设Priority，不能用普通父Setter等价替代。跨两层/template/的默认ControlTheme selector原型被Avalonia拒绝，已经删除，没有新增Semantic route或代码绑定。

最终只改6个源：共享Auto主题、三具体Auto模板、Mentions主题与既有内部CompoundInputTypographyBindings。没有新Own/Global/API/旗标/key或状态机。Spin保持自身绘制/动画，父拥有布局margin；专用Placeholder与PopupRoot Style仍唯一Part定制入口。

## 验证

- 9原反例转绿；最终15项PASS、0FAIL/Skip、1s，覆盖四尺寸、透明0、foreign公共输入隔离、标准Auto Local优先级/撤回、配置更新/清除、重模板/重挂及两个不同provider往返。
- 8个预编译专用Style类、关键Setter属性/值和实际命中/撤回通过；没有Name/Loaded代码定制回退。12个root/placeholder/frame弱对象回收通过。
- 当前Mac真实NSWindow、Overlay及Native PopupRoot句柄，managed/AOT各834检查PASS；包含四owner/四尺寸、明暗、资源/透明/Local/清除/重模板，以及真实公共异步加载流程的IsLoading、持续Spin、非均匀margin与清除仍保持请求。
- NET8 Release 0警告/错误、18.58s；AOT无IL2026/IL3050或抑制，保留5条本机OpenSSL/Brotli deployment链接警告。成员门禁1辅助文件/0Control/0问题。
- 作者另步自审：资源身份、模板投射优先级、动态资源降级、专用Style撤回、detach与AOT。遵守用户不使用子agent，无外部评审agent，不把自审当用户视觉批准。

## 绘制与原始失败

650×500完整Skia窗口，四owner各中号、原宽300/两条候选/占位文本保持，明暗匹配表面，正常动效。各静态阶段等待至少650ms与连续3稳帧；loading保持原动画，独立记录几何，不声称相位同步或RGBA相等。原版/候选自然及直接绘制共200PNG。

自然48静态对：28对0，8配置对变化8899或21139像素；其余12对出现1/5个像素、最大通道差1的弹层圆角/阴影边缘，严格0仍FAIL。清除同版检查中，原版8场景均0，候选4场景有1/5点，不能称恢复全通过或先验归为旧版同场景已有。原始坐标/颜色/全部计数保留。

单变量直接绘制A/B：撤回且仅撤回AM4六源改动，跑原轨迹，finally逐字节恢复六源并记录hash；重建当前源后同轨迹采候选。40对非配置画面全部RGBA0，8配置对8897～21141点；两版AA/扰动复位均0，1DIP有效检出。资源值/实际bounds一致，支持继续按CT25合成路径调查，但不能用直接0替代自然失败。具体限定范围见[审查表](input-autocomplete-mentions-surfaces-visual-review.md)。

两主题加载margin24→1/2/3/4，指示器本体43×38保持、顶部24→2，弹层120×90→120×48；自然横坐标36、原生35.5按各自观测保留。动态整帧变化17159/14955像素含未同步动画相位，仅作观察，不能作为margin单因像素证明或通用批准。

## 仍未完成的真实任务

独立只读审计另外取得四root正文、caret、selection背景/前景共16个有效Global消费者RED（1s），它们仍读复用公共输入身份；不是AM4通过项，不能以占位色包关闭。下一步需按[接续审计](input-autocomplete-mentions-remaining-audit.md)完成这些内部消费者、Form/Upload V2与兼容分类及实际消费者，再合并Part/状态/家族出口。

7个临时验证/审计/驱动文件（28008 bytes）、2份原主题副本、项目外原生工程427文件（1216159023 bytes）、200PNG+manifest共201媒体全部清理，媒体audit clean；常驻测试净增0。六源临时A/B后逐字节恢复已用hash核对。清理后精确AutoComplete/Mention/CandidateListBehavior、TextInputLifecycle及两个直接wrapper消费者共52PASS、0FAIL/Skip、2s，git diff --check通过。CT12六个临时文件及其代码/进度保持；当前相对会话起点仅14个已列CT13 tracked源变化，另新增内部helper，无其他tracked源码/测试/Gallery变化。

完整CT13/其他平台/CT25保持开放；本包只完成工程与取证，不把待批准的自然边缘写成视觉通过。
