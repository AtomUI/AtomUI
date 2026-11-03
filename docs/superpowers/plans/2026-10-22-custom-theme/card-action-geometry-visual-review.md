# CT15 Card Action 安排几何限定视觉审查

状态：工程、集中managed/AOT及清理后验收完成，表列48对已获用户限定批准。详细证据见[结果](card-action-geometry-results.md)。

仅请求表列在子项右对齐或重模板后再次右对齐时，分隔线跟随实际子项位置的48对修正。原Card宽400/高240、原完整标题正文/两个Action及三档/RTL保持；专用CardActionsStyle声明蓝色1宽边线。未改布局、描边公式、默认或点击区域。

| ID | 算法 | 尺寸 | RTL | 阶段 | 旧新像素 | 最大通道差 | 跨运行 |
| --- | --- | --- | --- | --- | ---: | ---: | ---: |
| A01 | compact | large | False | right | 46 | 120 | 0 |
| A02 | compact | large | False | template-right | 46 | 120 | 0 |
| A03 | compact | large | True | right | 46 | 119 | 0 |
| A04 | compact | large | True | template-right | 46 | 119 | 0 |
| A05 | compact | middle | False | right | 46 | 120 | 0 |
| A06 | compact | middle | False | template-right | 46 | 120 | 0 |
| A07 | compact | middle | True | right | 46 | 119 | 0 |
| A08 | compact | middle | True | template-right | 46 | 119 | 0 |
| A09 | compact | small | False | right | 46 | 120 | 0 |
| A10 | compact | small | False | template-right | 46 | 120 | 0 |
| A11 | compact | small | True | right | 46 | 119 | 0 |
| A12 | compact | small | True | template-right | 46 | 119 | 0 |
| A13 | dark | large | False | right | 42 | 95 | 0 |
| A14 | dark | large | False | template-right | 42 | 95 | 0 |
| A15 | dark | large | True | right | 44 | 94 | 0 |
| A16 | dark | large | True | template-right | 44 | 94 | 0 |
| A17 | dark | middle | False | right | 42 | 95 | 0 |
| A18 | dark | middle | False | template-right | 42 | 95 | 0 |
| A19 | dark | middle | True | right | 44 | 94 | 0 |
| A20 | dark | middle | True | template-right | 44 | 94 | 0 |
| A21 | dark | small | False | right | 42 | 95 | 0 |
| A22 | dark | small | False | template-right | 42 | 95 | 0 |
| A23 | dark | small | True | right | 44 | 94 | 0 |
| A24 | dark | small | True | template-right | 44 | 94 | 0 |
| A25 | darkcompact | large | False | right | 40 | 95 | 0 |
| A26 | darkcompact | large | False | template-right | 40 | 95 | 0 |
| A27 | darkcompact | large | True | right | 41 | 94 | 0 |
| A28 | darkcompact | large | True | template-right | 41 | 94 | 0 |
| A29 | darkcompact | middle | False | right | 40 | 95 | 0 |
| A30 | darkcompact | middle | False | template-right | 40 | 95 | 0 |
| A31 | darkcompact | middle | True | right | 41 | 94 | 0 |
| A32 | darkcompact | middle | True | template-right | 41 | 94 | 0 |
| A33 | darkcompact | small | False | right | 40 | 95 | 0 |
| A34 | darkcompact | small | False | template-right | 40 | 95 | 0 |
| A35 | darkcompact | small | True | right | 41 | 94 | 0 |
| A36 | darkcompact | small | True | template-right | 41 | 94 | 0 |
| A37 | light | large | False | right | 45 | 120 | 0 |
| A38 | light | large | False | template-right | 45 | 120 | 0 |
| A39 | light | large | True | right | 45 | 119 | 0 |
| A40 | light | large | True | template-right | 45 | 119 | 0 |
| A41 | light | middle | False | right | 45 | 120 | 0 |
| A42 | light | middle | False | template-right | 45 | 120 | 0 |
| A43 | light | middle | True | right | 45 | 119 | 0 |
| A44 | light | middle | True | template-right | 45 | 119 | 0 |
| A45 | light | small | False | right | 45 | 120 | 0 |
| A46 | light | small | False | template-right | 45 | 120 | 0 |
| A47 | light | small | True | right | 45 | 119 | 0 |
| A48 | light | small | True | template-right | 45 | 119 | 0 |

不申请批准24张left-restore的旧新14–22非零、候选/AOT自身9–18/max1–24及原Card/FW2/平台/完整CT15边界。原Card32对及两默认纠正批准不扩大；本表也不授权提交。

- 原有效1RED/独立重录1PASS；最终8临时与原Card20共28PASS，含三档/LTR-RTL/重模板/命中与活Window回收。
- managed/AOT各240检查全PASS、120跨运行全0；24独立无配置默认及24初始left旧新全0；旧240中24严格绘制FAIL。
- 8临时/3源已删除，361取证audit/cleanup clean后删除；清理后真实重建Card20PASS/0Skip、990ms，最终源码Desktop NET8零警告零错误20.57s；成员2源/2控件/0问题。
- 原/新/AOT完整图已查看；仅缺失分隔线恢复到实际子项边界，原正文、按钮、外框和尺寸保持。AOT25838112字节、SHA256 `5df12919e8123c831615bad7b2356966d13183bff78d6f21fdfd90dd1f3be62a`，0 IL/CS警告，五条既有Homebrew ld警告单列。

## 限定批准登记

2026-11-02，用户对本表48对限定问题回复“批准”（request_user_input_async / call_66f0fe492c0b43bbab20a0728c2968ef / item 0）。仅right/template-right分隔线跟随实际子项边界的48对修正获批准；24张往返旧新非零、候选自身9–18/max1–24、原Card/FW2/其他边界及完整CT15保持，不授权提交。
