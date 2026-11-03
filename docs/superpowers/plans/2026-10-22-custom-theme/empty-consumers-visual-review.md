# CT15 Empty 限定视觉审查（2026-11-01）

状态：工程、正常 Skia 旧新/AOT、临时清理和清理后13项精确模块/NET8零警告完成；仅以下20对已获限定视觉批准。

范围为2metadata/6Own，不缩137/274。仅下面20对明确静态差异拟申请限定审查。四默认旧新0；三档Own/0/clear、原生字体、三专用Style与clear、显隐/RTL/重模板均有实际轨迹，不因旧新0就省略合同检查。

| 算法 | 状态 | 非零像素 | max通道差 | bbox | 明确原因 |
| --- | --- | ---: | ---: | --- | --- |
| L | custom-global | 8336 | 237 | [223, 189, 497, 370] | 自定义橙色 SVG 保持原色，仅文字随 Empty Global 生效 |
| L | detached-custom | 20000 | 233 | [260, 199, 460, 299] | 离树切换 Source 后重挂，原缺图恢复为原橙色 SVG |
| L | global-simple | 14395 | 255 | [223, 190, 497, 370] | 预设 Simple 的 Empty Global 五色/文字生效 |
| L | global | 11078 | 255 | [223, 189, 497, 370] | 预设 Default 的 Empty Global 五色/文字生效 |
| L | reattach | 5578 | 38 | [314, 199, 420, 299] | 完整历史最终重挂，旧版缺图恢复为最初 Default 插画 |
| D | custom-global | 8331 | 230 | [223, 189, 497, 370] | 自定义橙色 SVG 保持原色，仅文字随 Empty Global 生效 |
| D | detached-custom | 20000 | 230 | [260, 199, 460, 299] | 离树切换 Source 后重挂，原缺图恢复为原橙色 SVG |
| D | global-simple | 14388 | 235 | [223, 190, 497, 370] | 预设 Simple 的 Empty Global 五色/文字生效 |
| D | global | 11087 | 235 | [223, 189, 497, 370] | 预设 Default 的 Empty Global 五色/文字生效 |
| D | reattach | 5590 | 51 | [314, 199, 420, 299] | 完整历史最终重挂，旧版缺图恢复为最初 Default 插画 |
| C | custom-global | 8011 | 237 | [223, 201, 497, 358] | 自定义橙色 SVG 保持原色，仅文字随 Empty Global 生效 |
| C | detached-custom | 15400 | 233 | [272, 211, 447, 299] | 离树切换 Source 后重挂，原缺图恢复为原橙色 SVG |
| C | global-simple | 12253 | 255 | [223, 202, 497, 358] | 预设 Simple 的 Empty Global 五色/文字生效 |
| C | global | 9588 | 255 | [223, 201, 497, 358] | 预设 Default 的 Empty Global 五色/文字生效 |
| C | reattach | 4251 | 38 | [320, 211, 413, 298] | 完整历史最终重挂，旧版缺图恢复为最初 Default 插画 |
| DC | custom-global | 8006 | 230 | [223, 201, 497, 358] | 自定义橙色 SVG 保持原色，仅文字随 Empty Global 生效 |
| DC | detached-custom | 15400 | 230 | [272, 211, 447, 299] | 离树切换 Source 后重挂，原缺图恢复为原橙色 SVG |
| DC | global-simple | 12262 | 235 | [223, 202, 497, 358] | 预设 Simple 的 Empty Global 五色/文字生效 |
| DC | global | 9588 | 235 | [223, 201, 497, 358] | 预设 Default 的 Empty Global 五色/文字生效 |
| DC | reattach | 4259 | 51 | [320, 211, 413, 298] | 完整历史最终重挂，旧版缺图恢复为最初 Default 插画 |

窗口720×520、控件宽420、完整Original description/Original footer文本与原图内容保持。Global使用Menlo23、描述#123456和Red/Lime/Blue/Orange/White测试palette；预设多色按原builder公式生成，不统一染成前景色。自定义Source仍为80×40橙色rectangle，原文/固有比例保持，显式Global只影响文字。暗色测试描述深蓝、原Footer黑色及专用Style Purple的低对比已查看并列明，不借扩大宽度/改色绕过；未宣称暗色完整可读性出口。

完整帧同坐标RGBA比较，无裁剪/掩码/对齐/容差。156静态旧新136零/20非零；24输入持续态中19非零，均不纳入本20对静态批准。旧版各24检查8失败，共32原始失败，含Global、离树/最终重挂及输入焦点/键盘，完整账本保留。真实输入用产品TextBox，不停止caret，不把Headless当OS输入证明。

当前候选主恢复56对、独立A/A及restore各4对全0，+1DIP四组2194/2187/2105/2091像素可检出；旧版主恢复仅最终reattach四对失败，其他恢复同。AOT同180帧/96检查PASS，156静态cross全0、56主恢复与独立A/A/restore各4全0。24输入持续态中10跨运行非零保留，未列入20对。清理后13PASS/0Skip（573ms）与NET8零警告（18.85s）完成。

完整平台/Browser、图片路径与异常资源覆盖、所有极值、GC释放证明、家族出口及完整CT15继续开放。CT12/13/14既定结论和CT25/FW1原始问题/官方依赖/像素容差不改，无提交授权。


## Empty限定批准登记

2026-11-01 用户明确回复“仅批准 Empty 表内20对，其余保留”（request_user_input_async / call_630c3c2e47fe460594f8888610f2e347 / item 0）。仅Empty表内20对Global/跨scope字体颜色获限定批准；原32失败、19旧新/10跨运行输入持续态非零、原裁剪/低对比、平台/完整家族与CT15保留，不授权提交。此前没有引用明确回复的“已批准”摘要不作为批准来源，本条为明确登记。
