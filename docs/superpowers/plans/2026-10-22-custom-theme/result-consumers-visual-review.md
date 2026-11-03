# CT15 Result 限定视觉审查（2026-10-31）

状态：工程、临时清理、清理后9项/NET8验收完成，表内60对及列明原生优先修正已获用户限定批准；仅表内60对非零变化，不关闭完整Result/CT15，不授权提交。

900×850、Result宽680自然布局、原Header/SubHeader/Content/Extra按钮，四算法正常motion。静态750ms+12等RGBA帧、6s上限，同坐标完整比较、无mask/crop/align/容差。表16行64格中4格为0（原样），仅申请其余60格。数值为变化像素/max通道差。

| 明确有意变化 | Light | Dark | Compact | DarkCompact |
| --- | --- | --- | --- | --- |
| Own画刷/尺寸在重配主题后持续生效 | 0（原样） | 5149/max151 | 5149/max41 | 5149/max151 |
| 清Own恢复默认glyph画刷 | 0（原样） | 3115/max148 | 3115/max168 | 3115/max148 |
| Global字体/语义色/行高真实进入目标 | 37175/max255 | 40332/max255 | 41454/max255 | 41495/max255 |
| 原生Courier17优先，保留Global语义 | 26720/max255 | 29871/max255 | 30557/max255 | 30593/max255 |
| 清原生回Global | 37175/max255 | 40332/max255 | 41454/max255 | 41495/max255 |
| 清Global后仍维持默认glyph画刷 | 0（原样） | 3115/max148 | 3115/max168 | 3115/max148 |
| 状态循环Info不遗留基础白灰画刷 | 0（原样） | 3115/max148 | 3115/max168 | 3115/max148 |
| 新自定义glyph无原生值时保持Result尺寸/色 | 55080/max255 | 55113/max235 | 57066/max255 | 57083/max236 |
| 原生glyph27×31/Orange优先（明确合同修正） | 55509/max255 | 55537/max235 | 57188/max255 | 57224/max235 |
| 清Icon后新默认glyph72与状态色恢复 | 56341/max255 | 56374/max235 | 58283/max255 | 58300/max236 |
| 原可选内容隐藏后图标保持正确 | 3147/max175 | 3147/max150 | 3147/max175 | 3147/max150 |
| 原内容恢复时不遗留小灰glyph/布局收缩 | 56341/max255 | 56374/max235 | 58283/max255 | 58300/max236 |
| RTL时保持正确图标/原内容布局 | 56460/max255 | 56493/max235 | 58389/max255 | 58406/max236 |
| 清RTL恢复原glyph尺寸/布局 | 56341/max255 | 56374/max235 | 58283/max255 | 58300/max236 |
| 脱离期间Success状态重挂后正确回放 | 56347/max255 | 56380/max235 | 58294/max255 | 58311/max236 |
| 原Extra真实输入后保持正确回放状态 | 56347/max255 | 56380/max235 | 58294/max255 | 58311/max236 |

## 具体合同与兼容修正

五专用Style只定制公开六target：icon两容器Opacity.75/Margin9,7，title Font37/Orange，subTitle Font23/Purple，extra Padding11,7，body Padding17,13/AliceBlue。root仍用公开属性，内部glyph及SVG图元不是Part；descriptor/route/ContractType/数量不改。带Part阶段title37会覆盖Own41，Own41独立无Style验证已通过，不混报。

Own配方15字段：Header41/Sub19/Icon93，Extra1,2,3,4，Image311×355，四语义色#123456/#345678/#56789A/#789ABC，Frame7,9、Content11,13、ContentMargin3,5,7,9、HeaderMargin2,4,6,8、ImageMargin5,7,9,11；zero全部尺寸/间距0，四zero旧新0。Global：Menlo23、Heading#FA8C16、Description#9254DE、FillAlter#D9F7BE、相对行高1.7/1.8；Native字体Courier17。60默认/type/key/slot保持，原403/404/500插画来源/固有颜色不统一染色。

旧C#CurrentValue会在新Child/重配主题时被Icon基础Style覆盖成14灰或白色。实际树/优先诊断与真实ControlTheme失败试验后，复用逐glyph typed relay/Template优先级持续输入原StatusIconSize/Brush，替换/模板/detach释放、attach恢复。新无Native glyph按原Result角色72/状态色；显式Native Width27/Height31/Orange现优先（旧被72和状态色覆盖），这是本表明确请求的原生优先兼容修正，不是纯重构。调用方绑定不得被替换或释放，另有临时验证。

原三版384帧及18项中间恢复失败账本完整保留为pre-glyph批，修复后所有版本按新增NativeGlyph步骤完整重跑。未通过调整原视口、重建单侧、隐藏内容或替换基线消差。

## 结果与保留边界

- 最终旧新132对72零/60非零；本表60全部解释、全部JIT/AOT零。四无定制默认、四zero及三异常插画当前配置旧新均零。
- 最终JIT/AOT132对全部精确0；managed/AOT各72检查0FAIL，旧72中8FAIL为重挂状态与Nativeglyph优先。
- 候选/AOT各32中间/终场恢复全部0；旧32中18仍失败。原pre-glyph三版同现的18非零没有删改判定。
- 三版四组独立A/A/1DIP恢复全0，检错Light3653/max251、Dark1485/max255、Compact3621/max251、DarkCompact1485/max255。
- 当前强持完整模板Presenter的GC探针判据不足已INCONCLUSIVE处置，不计泄漏修复或通过、不归FW1。原长期retemplate退出合同保留。完整资源/真实OS/Browser/自然极值及完整Result/CT15出口仍未关闭。
- CT12/13/14批准与CT25/FW1既定边界保持；官方依赖不替换，不设通用容差。

见[结果](result-consumers-results.md)及[作者自审](result-consumers-review.md)。


限定批准登记：2026-10-31，用户明确回复“批准表内60对及列明原生优先修正，其余边界与完整CT15保留”（request_user_input_async / call_99a149043b344680ba0a26e4e74cb239 / item 0）。仅Result表内60对与自定义glyph27×31/Orange原生优先修正获批准；原pre-glyph384帧/18失败文本、GC判据不足、平台/极值/完整Result与CT15继续保留，不授权提交。
