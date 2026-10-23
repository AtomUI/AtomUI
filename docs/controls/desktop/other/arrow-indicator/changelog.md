# ArrowIndicator 变更记录

## 2026-10-23

- ArrowSize变化重建已生成几何并使绘制失效，测量与绘制共同消费当前尺寸。
- 新增TipRadius StyledProperty，默认沿用1.5 DIP，支持尖角及声明式语义部件样式；测量/绘制依赖与几何同步。
- 保持既有ArrowSize、填充/描边API和组合owner/Token身份，不新增独立Token注册。
