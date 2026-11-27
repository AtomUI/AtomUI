# Collapse 展开时的标题圆角

在 `bugfix/shadcn` 的原生 macOS Gallery 中复现。Shadcn Luma 浅色的 Default Size 示例展开后，标题仍绘制 26 DIP 的两个下圆角，导致紧贴分隔线的两侧露出白底。

主题根据内容视口的实际可见性过滤标题下圆角。展开及收起动画中标题与内容相接，内容隐藏后标题恢复外框下圆角。容器计算上下角时按 Avalonia 的顺时针顺序传递半径。

| 验收项 | 修复前 | 修复后 |
| --- | --- | --- |
| Default Size 展开标题底部两侧 | 白色 `#FFFFFF` | 与标题底色一致，为 `#F5F5F5` |
| Default Size 收起及中间动画 | 标题始终有下圆角 | 收起过程中内容可见时底角为 0，结束后恢复 26 DIP |
| Small、Large、多面板末项 | 展开时仍有白缺口 | 展开时两侧与标题同色，收起后绘制下圆角 |
| 自定义不对称圆角 | 左右下角顺序颠倒 | 展开正文和收起标题均按外框左右顺序绘制 |
| Ant Design 默认尺寸 | 展开标题两侧露白 | 两侧与标题底色一致，为 `#FAFAFA` |

脚本通过 Avalonia 路由输入点击真实 Gallery 的标题，使用原生窗口编号截图。两版同状态复拍和恢复时，标题采样区域均无像素差，能检出 1 DIP 偏移。已查看截图，默认尺寸的标题、正文与外框布局坐标一致，收起后的标题区域和展开标题分隔线中央区域逐像素一致。

临时驱动与截图已清理，工作区取证文件审计通过。清理后在 `net10.0` 上以 `FullyQualifiedName~AtomUI.Desktop.Controls.Tests.CollapseControl` 过滤运行 Desktop Controls 测试，50 项通过。以 `FullyQualifiedName~AtomUIGallery.Tests.ShowCases.CollapseShowCasePageTests` 过滤运行 Gallery 测试，3 项通过。
