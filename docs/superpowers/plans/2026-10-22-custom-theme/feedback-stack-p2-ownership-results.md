# CT15 Message/Notification P2归属与几何核对（2026-11-01）

## 归属裁决

既有message-consumers.md的“16/8有Global语义尚未全部接入”是待核对线索，不是批准的新增映射。正式架构docs/architecture/systems/control-infrastructure/feedback-stack.md第5节明确共享gap16/offset8、背板高度16/缩进8和16的产品几何。CT10 shared-consumers-catalog.md三FeedbackStack类型已核为内部布局/host，无独立Token消费缺口。保留这些既定合同和primary_task，不因常量名字推导Spacing/SpacingXS消费者，不额外添加Own或重复已完成机制。

本裁决不关闭P2：manager的实际Own/Global、原生Padding与六位置、真实host作用域、自然内容/业务、输入/关闭/滚动/裁剪和释放仍按实际证据分项；要新增Global几何映射属于合同演进，不能把未经批准的新默认投射当缺陷修复。

## 精确验证

无新增测试或产品修改。持有共享输出非阻塞独占锁，直接精确运行FeedbackManagerPaddingTests与FeedbackStackPanelTests，共37PASS/0FAIL/0Skip，900ms（命令10.74s）。涵盖manager原生Style/Local Padding、六方位对齐和真实hover命中/空inset排除，以及共享panel阈值、变高卡片、top/bottom镜像、clip/scale/transform目标与重排。该既有测试含motion禁用的终态几何断言，不用它证明正常持续动效、完整像素或原生OS最终面。

无源变化，复用已有正常motion/生命周期及当前最终源码NET8通过证据，不扩大为全量回归；本次没有新的AOT/视觉取证，不借旧结果推定未覆盖host/自然极值通过。临时run.py/module.log会话清理，git diff --check通过。

## 保留项

Message原39 JIT/AOT、检错/业务恢复及其他未通过，Notification暗恢复两张4/max16，真实host/其他平台、完整业务/任意极值与整个P2/CT15仍保留。CT12/13/14、CT25/FW1的既定结论不改，原137metadata/274Own不缩小。
