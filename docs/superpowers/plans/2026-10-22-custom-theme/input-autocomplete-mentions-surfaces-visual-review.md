# CT13 AM4：占位色、弹层与加载布局限定审查

2026-10-30，用户已明确回复“批准五表限定范围，保留 CT25/FW1”。本表限定视觉已批准；批准来源 request_user_input_async / call_5f004b68efc344048653a3d95ae55694 / item 0。范围与原始计数保持，CT25/FW1保留，无Git授权。本表不批准完整CT13、其他尺寸/字体/平台、Git或通用容差。AM2/AM3亦已获本次汇总限定批准。

四owner中号、300宽、两条原候选与占位文本保持；650×500窗口，明暗匹配表面，正常动效。有效Global配置占位色Light #213243/Dark #f0a0d0，弹层背景Light #e9f8df/Dark #203344；Mentions另设MarginLG=1/2/3/4。

## 8个静态配置场景

| owner | 自然Light / Dark变化像素 | 直接Light / Dark变化像素 |
| --- | --- | --- |
| AutoComplete | 21139 / 21139 | 21141 / 21137 |
| AutoCompleteSearchEdit | 21139 / 21139 | 21141 / 21137 |
| AutoCompleteTextArea | 21139 / 21139 | 21141 / 21137 |
| Mentions | 8899 / 8899 | 8901 / 8897 |

占位色进入真实文字，popup颜色生效；标准Auto PlaceholderForeground Local与专用Style优先，清除恢复当前资源。无默认颜色或原输入几何的主动变更。

## 两个加载布局场景

Light/Dark均为margin24→1/2/3/4，指示器本体43×38保持、顶部24→2，弹层120×90→120×48；两条候选在非loading恢复。原生横坐标35.5与自然36如实保留。动画一直运行，动态整帧17159/14955点含未同步相位，不能当作纯margin差异；本项只审已查看的布局变化，不批准其他相位/动画差异。

## 自然边缘的精确限定

| owner/主题 | 原→候选不同的非配置阶段 | 每对变化像素 / 最大通道差 |
| --- | --- | --- |
| AutoComplete Light | cleared | 5 / 1 |
| SearchEdit Light | cleared | 5 / 1 |
| TextArea Light | initial、AA、1DIP、reset、cleared | 各5 / 1 |
| AutoComplete Dark | initial、AA、1DIP、reset | 各1 / 1 |
| TextArea Dark | cleared | 1 / 1 |

共12对；对应弹层圆角/阴影边缘，原坐标与颜色保留。其余28自然静态对0；直接40非配置对全部0，8配置对如上。AA/扰动复位均0且1DIP检出，原始自然严格0失败保留CT25，不设容差或扩成其他画面允许误差。

15专项、8编译专用Style/12图GC、当前Mac managed/AOT各834、NET8零及清理后精确52项通过，见[结果](input-autocomplete-mentions-surfaces-results.md)。7临时文件、2副本、427工程文件与201媒体均已清理且audit clean。正文/caret/selection后续已由AM5完成并获限定批准，不在本表重复申请范围。

批准只覆盖8配置场景、上述两主题加载布局，以及表列12对自然边缘；不接受未同步动画相位为通过依据，不关闭CT25、其余消费者或完整CT13。

本次批准由[最后五表汇总](input-final-visual-review.md)统一记录；CT13已在既定批准范围完成，原始FAIL与独立CT25/FW1不改写。
