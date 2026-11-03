# CT15 DotBadge ShadowColor 限定视觉审查

状态：工程与清理后验收完成，表列24对已获用户限定批准；不是整个Badge或CT15完成报告。详细证据见[结果](badge-shadow-color-results.md)，原Badge48对批准不扩大。

仅阴影颜色变化：旧版有效ShadowColor已更新但仍绘制Green；候选在同一实例/相同Dot24、Shadow4/原标签与target下，Blue、Transparent和clear回默认进入真实绘制。原show/hide motion保持。

| ID | 算法 | 承载 | 阶段 | 旧→新像素 | 最大通道差 | JIT/AOT |
| --- | --- | --- | --- | ---: | ---: | ---: |
| S01 | compact | standalone | blue | 440 | 255 | 0 |
| S02 | compact | standalone | clear | 440 | 255 | 0 |
| S03 | compact | standalone | transparent | 440 | 255 | 0 |
| S04 | compact | target | blue | 439 | 255 | 0 |
| S05 | compact | target | clear | 439 | 255 | 0 |
| S06 | compact | target | transparent | 439 | 255 | 0 |
| S07 | dark | standalone | blue | 439 | 255 | 0 |
| S08 | dark | standalone | clear | 439 | 235 | 0 |
| S09 | dark | standalone | transparent | 439 | 255 | 0 |
| S10 | dark | target | blue | 439 | 255 | 0 |
| S11 | dark | target | clear | 439 | 235 | 0 |
| S12 | dark | target | transparent | 439 | 255 | 0 |
| S13 | darkcompact | standalone | blue | 440 | 255 | 0 |
| S14 | darkcompact | standalone | clear | 440 | 235 | 0 |
| S15 | darkcompact | standalone | transparent | 440 | 255 | 0 |
| S16 | darkcompact | target | blue | 439 | 255 | 0 |
| S17 | darkcompact | target | clear | 439 | 235 | 0 |
| S18 | darkcompact | target | transparent | 439 | 255 | 0 |
| S19 | light | standalone | blue | 439 | 255 | 0 |
| S20 | light | standalone | clear | 439 | 255 | 0 |
| S21 | light | standalone | transparent | 439 | 255 | 0 |
| S22 | light | target | blue | 439 | 255 | 0 |
| S23 | light | target | clear | 439 | 255 | 0 |
| S24 | light | target | transparent | 439 | 255 | 0 |

- 原1有效RED→同例GREEN；四算法×两承载8临时与原32共40PASS。managed/AOT各96检查0FAIL；旧96中24个绘制FAIL。
- 64对跨运行全0；16张无配置默认旧新全0，原始Green与重挂Blue各8对全0。已查看旧Green残留、新Blue及暗Compact target透明图；原目标、全文、阴影尺寸/裁剪保持。
- 两Compact standalone的Green往返在managed/AOT各1像素/max1，旧新这两对也非零；均严格保留，不纳入24对，不归FW1/FW2，不增容差。
- 8临时用例已删除；193取证经官方audit/cleanup clean后删除；清理后真实重建精确Badge32PASS/0Skip、826ms；最终源码NET8 Release零警告零错误23.10s。
- AOT真实身份：23005984字节，SHA256 `4ce903b369d1d9127e66332c168953bce562efc787ce970f76e985ffb8328639`；0 IL/CS警告，五条既有Homebrew最低OS链接警告单列。
- 仅请求接受本表24对颜色热更新修正；其他原失败/恢复、其他家族及完整CT15保留，无提交授权。

## 限定批准登记

2026-11-02，用户对本表24对限定问题回复“批准”（request_user_input_async / call_ffe2edc187f244ad8f0051399c40c1f0 / item 0）。仅表内Blue/透明/清除回默认的24对修正获批准；两Compact独立模式绿色往返各1/max1、原Badge边界、完整CT15、FW1/FW2和不提交约束保持。其他入口引用此登记，不扩大为整个Badge或CT15完成。
