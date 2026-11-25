# TextBlock Changelog

## 2026-11-10

- Theme
  - 将链接与选区资源改读对应有效全局 Token；HyperLinkTextBlock 保持透明默认背景，用户显式 Background 绘制在已有 PART_Text 上；不再声明默认 ColorBgContainer 消费，避免遮挡 DataGrid 行底色。
