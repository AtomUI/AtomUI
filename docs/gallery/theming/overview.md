# Gallery 主题工具设计

本目录定义AtomUIGallery主题工具的产品模型与使用闭环。主题运行时属于Core，生成与资产属于Generator/主题包，GalleryBase保持产品中立；本目录不建立独立主题引擎。

| 文档 | 所有权 |
| --- | --- |
| 本文 | 产品边界、模块职责与阅读入口 |
| [主题工作台](workbench.md) | 草稿/预览/应用/保存、配置诊断、品牌比较、完整往返及受限生成入口 |

工作台属于目标设计。当前Shell切换、主题Catalog、局部示例与Part Preview继续按既有实现工作；完整编辑/品牌导出/AI的实现证据不能由设计文档替代。

- [品牌配方与scope](../../architecture/systems/theming/brand-recipes.md)：配方、资产、样式资格、跨根与激活失败语义。
- [Control渲染合同](../../architecture/systems/theming/control-rendering.md)：工具所编辑输入的真实消费要求。
- [GalleryBase主题与本地化](../../modules/toolkits-gallery-base/theming-localization.md)：Shell当前状态、注册和产品中立边界。
- [Semantic Part Preview](../authoring/semantic-part-preview.md)：目标查找、高亮及生命周期；不作为应用样式回退机制。
- [主题验收规范](../../engineering/development/theming/verification.md)：工具与Control共同遵守的证据和验证成本要求。
