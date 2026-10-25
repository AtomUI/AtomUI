# CT19 C10：单分量滑轨键盘结果（本机切片出口完成）

用户2026-10-25批准[短设计](color-slider-keyboard.md)，沿现有feature/custom-theme及checkout实施。没有新建分支／worktree、委派或Git授权，不发布NuGet。C9视觉批准不覆盖本片新的有限合成差异。

## 原因与最小实现

实际ColorSlider继承RangeBase；既有Focusable及thumb导航不能提供Slider键盘数值行为。原版自然Tab已到slider／thumb，raw Right真实送达一次，Value仍66.66666666666666；两View×LTR／RTL×两焦点入口8项均取得同样有效RED。

只在具体ColorSlider增加OnKeyDown：base后排除Handled／禁用／修饰键，使用继承SmallChange／LargeChange、Minimum／Maximum及FlowDirection计算并钳制，以SetCurrentValue进入既有颜色链。八种标准数值键边界也Handled；RTL只反转水平键。没有改AbstractColorSlider或多stop轨道，没有新增字段、订阅、公开签名、Token或Part。输入数值计算不能由模板Setter等价表达。

## 功能、舍入及正常数据

- 初始10项通过，扩展26项覆盖六模式剩余H／S／V、Alpha、slider／thumb自然焦点、LTR／RTL、四方向、Page／Home／End／钳制、边界同值事件、禁用恢复、修饰键及Handled、显式3.5／12步长与已有绑定、RGBA字节单位、邻接实际文本快捷键、保留／重建模板与实际指针拖动。
- 初始候选到Alpha后暴露过严的未舍入HSV预期。两模板原来明确IsRoundingEnabled=true；独立原源码2项证明Alpha61→62得到H99／S.42／V.67／A.62。本片保留该合同，使用独立字面值断言，不改产品舍入来迎合测试。
- 每键scalar／一次事件严格断言。Hue1度小于1DIP时thumb坐标可能保持原取整；物理方向使用两个水平键再比较，仍逐键断言1度及一次送达／一次事件。
- 初始空值View数据26项通过。实际画面发现空渐变没有stop数据，补充明确#9C7BAA62→Blue双stop数据，保留全部原空值证据；正常数据原8项键盘反例失败、2项舍入表征通过，候选26项全通过。没有覆盖旧预期帧。

## 当前Mac真实输入

临时外部工程使用仓库Directory.Build.props／中央包和产品project references；Avalonia12.1.4、net10.0。前置缺失namespace、中央包导入时机和SDK输出路径问题均留日志，尚未执行的准备失败不计作控件反例；最终build零警告／错误。

managed／NativeAOT各8个NSWindow、1200×820 client／scale2，覆盖明暗×普通／渐变×LTR／RTL。每种六模式分别自然Tab到slider／thumb，检查单分量及Alpha；每次目标键前记录active／focus类型／是否属于当前slider。另有Page／Home／End与边界检查。

| 宿主 | 功能断言 | 目标原始按键 | 缺失送达／非active／非本节点焦点 |
| --- | ---: | ---: | ---: |
| 当前Mac managed | 5688／5688 | 1248 | 0／0／0 |
| 当前Mac NativeAOT | 5688／5688 | 1248 | 0／0／0 |

使用实际窗口平台Input及keyboard device，不是绕过产品直接写Value；此项不是最终屏幕像素验收。源码、驱动、依赖及产物SHA、完整按键trace和所有准备日志保留于本地文字receipt。外部驱动／工程／产物428文件、1,241,928,586 bytes已删除。C9首轮AOT四事件缺失的原因仍未证明，本次成功不能抹去该历史。

## 像素与实际画面

共640帧：空值与双stop两套各96旧／96新自然compositor、各64旧／64新独立即时Skia；不是从其中挑选某轮消差。每套保持相同完整操作历史：default→focus→third-right（Right两次）→third-page（PageUp）→neutral→alpha-focus→alpha-right（Right两次）→reset。每个自然版本另建独立A/A及1DIP实例；正常动效及最低650ms／真实Transition时长后连续三帧相同才捕获。

| 对照 | 数量与实际结果 |
| --- | --- |
| 自然A/A | 128组，114组0；非零逐对列有限表 |
| 自然1DIP | 128组全部检出 |
| 自然新旧默认 | 8组整图0 |
| 自然同版本neutral→reset | 48组非零，逐对列有限表；没有伪报恢复0 |
| 直接Skia A/A | 128组全部0 |
| 直接Skia同版本恢复 | 32组全部0 |
| 直接Skia新旧兼容阶段 | 40组全部0 |
| 公共属性／源位图观察 | 自然216＋直接200，共416对一致 |

[72对限定审查表](color-slider-keyboard-noise-review.md)仅包括自然A/A14对、恢复48对及兼容新旧10对，每对1–20像素、单通道最大2；完整历史、前后帧名、离散点集与各对上限都固定，不设置通用容差。公开树一致及独立直接Skia0只证明所采诊断，不声称已证明每个历史帧的内部缓存原因。正常合成真实失败计数保持。

实际查看了带双stop的暗色Alpha旧／新画面、明色PageUp候选和对应差异图：Alpha两Right使61%→63%，滑块及颜色／HEX同步；PageUp使Hue按当前数值大步长改变，色谱、preview、相关色条及文字同步，没有缺失渐变stop、移除面板、改变尺寸或焦点树来绕过原场景。复位差异图仅有表内两个轨道thumb边界像素；原始RGBA比较未裁剪、平移或遮罩。

自然旧新键盘阶段的完整图变化像素如下，保留包含小合成差异的真实计数；这些是已批准输入行为的结果，不列为环境噪声。

| 数据／主题／View | third-right | third-page | alpha-right |
| --- | ---: | ---: | ---: |
| 空值／light／plain | 35592 | 42732 | 1592 |
| 空值／light／gradient | 35592 | 42732 | 1592 |
| 空值／dark／plain | 35583 | 42750 | 1596 |
| 空值／dark／gradient | 35583 | 42750 | 1596 |
| 双stop／light／plain | 35594 | 42732 | 1592 |
| 双stop／light／gradient | 35592 | 42732 | 1592 |
| 双stop／dark／plain | 35582 | 42748 | 1594 |
| 双stop／dark／gradient | 35583 | 42750 | 1604 |

## 清理、成员与验收边界

两份临时C#均已删除，没有新增永久测试；外部原生工程及产物已删除。最终清理后Release原ColorPicker精确27项全通过，NET8 Release构建零警告／错误；触及的完整ColorBlock／AbstractColorSlider／ColorSlider／ColorSpectrum四文件四控件成员审计零问题。没有本地全量或change scope。

C10候选ColorSlider SHA256 `15645488c85d53e1b4e1ae12363e2a52beab1c8c5bcb8e6ff8972d6a3dc9d4a1`；C9其余四文件身份见[C9结果](color-spectrum-keyboard-results.md)，A/B后均已恢复候选并核对。具体OnKeyDown没有代码绑定；触及文件无待迁移的模板relay binding。四文件成员完整审计通过，不混淆AbstractColorSlider另备的纯成员布局与本次行为。

作者另步自审已完成（主agent独立执行，非独立评审）；16份相关文档的56个本地链接有效，媒体审计clean。72对有限差异已获用户明确批准，仅限审查表范围；645张PNG与manifest共646文件已清理，run目录不存在，媒体审计clean。没有Git授权。完整CT19／CT25、其他平台与原生最终像素不因此关闭；C9首轮AOT未解释事件缺失仍在台账。
