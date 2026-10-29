# S2：Select候选项OptionPadding限定视觉审查（已批准）

仅将既有Own.OptionPadding接到真实候选项Frame内缩；默认公式、候选项数据/选择/输入事件及原弹层约束保持。默认与清除零差异，原继承12,0与有效Own12,5在既有行高和垂直居中下绘制相同。

需审查的是Own=9,4,17,6及0的具体文字内缩变化，包括配置后的真实点击选中。Skia540×400、四尺寸/明暗、8项内容、3行viewport256×96及extent256×256保持，44根/弹层记录和132项Bounds/文本/选中记录相同；A/A=0、1 DIP检错与复位通过。自然与直接绘制每对计数完全相同，没有通用容差。

## 每对完整帧

下表每格为“像素 / 最大通道”，自然与直接绘制共用相同精确计数。

| 阶段 | Light | Dark |
| --- | ---: | ---: |
| Large-normal | 0 / 0 | 0 / 0 |
| Large-configured | 4150 / 165 | 4138 / 145 |
| Large-zero | 4181 / 165 | 4181 / 145 |
| Large-clear | 0 / 0 | 0 / 0 |
| Middle-normal | 0 / 0 | 0 / 0 |
| Middle-configured | 4150 / 165 | 4138 / 145 |
| Middle-zero | 4181 / 165 | 4181 / 145 |
| Middle-clear | 0 / 0 | 0 / 0 |
| Small-normal | 0 / 0 | 0 / 0 |
| Small-configured | 4150 / 165 | 4138 / 145 |
| Small-zero | 4181 / 165 | 4181 / 145 |
| Small-clear | 0 / 0 | 0 / 0 |
| Custom-normal | 0 / 0 | 0 / 0 |
| Custom-configured | 4150 / 165 | 4138 / 145 |
| Custom-zero | 4181 / 165 | 4181 / 145 |
| Custom-clear | 0 / 0 | 0 / 0 |
| aa | 0 / 0 | 0 / 0 |
| nudge | 0 / 0 | 0 / 0 |
| reset | 0 / 0 | 0 / 0 |
| ready-click | 4150 / 165 | 4138 / 145 |
| selected | 4206 / 224 | 4205 / 198 |
| selected-clear | 0 / 0 | 0 / 0 |

申请范围仅上述20对配置/0/配置选中变化；其余24对均0。原4FAIL→候选6PASS；最后专用类/两Setter形态及命中/资格撤回1PASS。实际NSWindow公开固定弹层managed/arm64 NativeAOT各2044项；普通弹层的真实点击/像素由上述Skia轨迹证明，不把原生资源检查当像素或输入证据。NET8零警告/错误19.92s；四临时源、426文件工程、177媒体清理且audit clean，清理后Select71PASS、0Skip、3s。无Git或通用容差，完整家族/原生最终显示面/CT25仍开放，原生普通弹层的原始关闭失败及焦点因果范围保留在[结果](input-select-option-padding-results.md)。

2026-10-28用户独立批准call_ecb156e340b04c4283629d0797214f27:0，仅上述20对具体视觉变化，不含原生像素/输入、Git或通用容差。
