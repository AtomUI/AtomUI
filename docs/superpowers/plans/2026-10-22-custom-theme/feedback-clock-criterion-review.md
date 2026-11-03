# CT15 原持续态比较判据限定审查

本表不修改像素容差、不批准非零正确、不转移FW2、不宣布完整CT15或任何家族通过。它只申请明确：下列**原未证明输入同相位**的跨运行/跨版帧，是否应继续作为“必须RGBA0”的验收阻塞，或保留原始差异并改列为“相位输入未配对，不能判定视觉等价”。

| ID | 明确原记录范围 | 已有可判定证据与限制 |
| --- | --- | --- |
| CK1 | Carousel原16旧新持续态、4跨运行动态（light-auto0、light-auto200、dark-auto0、compact-auto0） | [原结果](carousel-consumers-results.md)112静态cross0/44恢复0；[两周期](carousel-cycle-audit.md)正常3000/700ms、页序及AOT38通过，但该周期场景不是原128帧全部历史，不能代替原动态像素 |
| CK2 | Marquee原48旧新/55跨运行持续态 | [原结果](marquee-label-consumers-results.md)112静态cross0/44恢复0；[正常周期](marquee-cycle-audit.md)原220×28/全文/clip/4倍时长两周期与AOT2874通过。live-hover等即便已暂停，两进程进入暂停的offset原点也未匹配，不能因名字相同就视作同相位 |
| CK3 | Spin循环原16真实clock cross及对应16旧新时相 | [循环结果](spin-cycle-results.md)2356受控/就绪/显隐/校准cross0，308端点变更已批准。受控输入不替代真实clock；原16没有同相位完整证明 |
| CK4 | Timeline/Icon P3-C原15真实clock cross及对应15旧新时相 | [端点结果](timeline-endpoint-results.md)736受控/校准cross0、96独立端点批准；真实clock图仍为原差异，不由受控图盖过 |
| CK5 | Drawer最终有效批21旧新/20跨运行定时 | [原结果](drawer-consumers-results.md)164静态cross0/68恢复0，open/close目标0/64/128/320ms；Light candidate实际记录210/224/234/345，AOT579/595/608/617。记录在PNG保存后读取，不能当已证明的曝光相位；原其他失败/错AOT批不在本表 |
| CK6 | Tour最终16旧新/12cross定时 | [原结果](tour-consumers-results.md)原open/move目标0/64/128/256ms，candidate/AOT实际记录不同；180静态中的2micro仍独立FAIL，原72缺帧/Primary重叠不在本表 |
| CK7 | Flyouts最终12旧新/12cross定时 | [原结果](flyouts-consumers-results.md)180静态cross0、原Host/Pointer恢复；独立真实open clock未锁同相位，FY1–4静态归属不包括定时 |
| CK8 | ImagePreviewer原64loading跨版/cross及本轮64loading cross | [当前结果](image-previewer-static-exit-plan.md)每次4时刻正常loading，200业务性质通过；同相位未证明。IP1–31仅静态，不覆盖此项；未决定加载完整周期视觉通过 |

这些范围来自已有最终记录，不将历史坏批、缺帧或新的同类结果自动追加进来。所有原非零数字与失败/未通过登记保留。以上“静态0/业务通过/周期通过”只按其实际原场景与源码范围引用，不作为这些动态比较的替代绿证据。

建议的有限判据区分：

1. 已证明相同输入/相位的比较，继续严格RGBA0；非零仍FAIL。
2. 未证明相同相位的CK1–CK8，仅将该原像素比较列为INPUT_NOT_PAIRED/视觉无法判定，不把它算PASS，也不以它证明产品动效错误；原动态功能/平滑/周期/平台未覆盖保持。
3. 原动作/样式/内容/时钟语义与已有业务断言保持。若还需要确定性动态视觉验收，按[有限采样规则](feedback-clock-input-boundary.md)补实际捕获前后时钟/server状态，计时段外编码；不得挑帧对齐、mask或扩大相位/像素容差。

这只是判据纠正提案，不是新增“所有控件全部周期/任意时相”门槛。批准它仍不能把原未通过算完成，不能关闭CT15；若不批准，上述原状态继续不动。本次仅复用已有已清理文字证据，没有新工程、发布、临时源码、媒体或产品变化，不授权提交。

## 限定裁决登记

2026-11-02，用户选择“1. 同意仅表列CK1–CK8改记相位未配对/视觉无法判定；不算通过，其他缺口与完整CT15保留”（request_user_input_async / call_b13e5e65d50348cc9b75caf64092babd / item 0）。仅本问题/本表范围生效；未列路径、证据不足处、原始失败与完整CT15、不提交约束保持。

## 其余原持续态口径补充（CK9–CK13，已获限定批准）

本补充只复用下列已有已清理原批。不同进程的相同帧名、相同目标时间或相同文字/caret索引，都没有证明相同动画/光标曝光相位；不将其自动按RGBA0等价输入比较，也不据非零认定产品根因。原动态功能、输入投递/焦点/文字断言与原FAIL逐条保留，不改静态或同实例恢复判据，不转FW2。

| ID | 精确原范围 | 原可判定证据与排除 |
| --- | --- | --- |
| CK9 | FloatButton原32定时旧新非零、28定时cross非零 | [原结果](float-button-consumers-results.md)show/scroll实际时间三版不同，Timed在PNG保存后读取actual；[当前28静态核对](float-button-current-remainder-results.md)不在本提案。原6静态cross、32主往返及115旧新中的静态部分均排除 |
| CK10 | Statistic原72旧新、46cross持续态，其中cross32计数/计时/Skeleton、14原生输入/caret | [原结果](statistic-consumers-results.md)128静态cross0、52主恢复0；真实CountUp/Timer值推进和输入断言分别通过。没有成对曝光时钟、显示数值与caret闪烁相位一致证明；不将旧44性质FAIL改为无法判定 |
| CK11 | QRCode正确背景最终批原46旧新、41cross持续态 | [原结果](qr-code-consumers-results.md)176静态cross0；live加载Spin、input-cycle/keyboard/template和key-down/up原生输入等时变画面未证明同相位。8静态恢复、旧44性质FAIL、错误背景480帧与ECI限制均排除 |
| CK12 | Empty原19旧新、10cross原生输入持续态 | [原结果](empty-consumers-results.md)156静态cross0、56主恢复0；原生TextBox保留真实caret和输入，原动态帧没有同曝光相位证明。旧32性质FAIL含原输入未成功、插画重挂丢失，继续作为修复前有效失败保留，不能用相位分类抹去 |
| CK13 | Spin主消费者最终candidate4/old4/aot4的32旧新、32cross持续态（native-glyph/template-only/正常motion采样） | [原结果](spin-consumers-results.md)136静态cross0；原生尺寸/画刷优先已另获明确批准，但旋转/模板glyph采样没有同相位证明。8独立cal旧新静态、旧28性质FAIL、后续循环CK3与308批准各自保持 |

建议与CK1–CK8相同：仅上述原像素比较登记INPUT_NOT_PAIRED／视觉无法判定；原非零及历史FAIL保留，不算PASS、不证明产品动画正确/错误，不转FW2。同相位比较仍严格RGBA0，不扩大像素/时相容差，不关闭任何家族或完整CT15。这里没有新采样、构建、AOT、模型或扩大平台/周期门槛；等待限定裁决后才改索引。

2026-11-02，用户选择“1. 仅CK9–CK13改记输入相位未配对/视觉无法判定；不算PASS、不转FW2”（request_user_input_async / call_2d2076b8577142a8a106ebca90f56a24 / item 0）。仅本补充表的原像素比较生效；非零、静态/性质FAIL、输入/业务缺口、原始坏批和完整CT15保持，不授权提交。这里的输入未配对不声称差异只由相位造成。
