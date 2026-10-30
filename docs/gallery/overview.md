# Gallery 文档

本目录维护 AtomUIGallery 的页面编写、工程组织、平台宿主和本地化约束。当前 Desktop/Browser-bound、产品中立的运行时
基础设施由 [AtomUI.Toolkits.GalleryBase 模块](../modules/toolkits-gallery-base/overview.md) 维护；Mobile Foundation 不依赖该包。

## Authoring

- [ShowCase 页面设计](authoring/gallery-showcase-design-pattern.md)
- [Semantic Part Gallery Preview](authoring/semantic-part-preview.md)：独立 Tab、真延迟创建、descriptor 驱动目标解析、
  Adorner 生命周期和零 Control 增量边界。
- [ShowCase 组织规范](authoring/organization.md)

## Platforms

- [Gallery 平台宿主](platforms/overview.md)
- [Browser Gallery 主题与宿主](platforms/browser-gallery-porting-notes.md)
- [Mobile Gallery](platforms/mobile-gallery.md)：预实现架构；Mobile content、iOS Host、Android Host 和平台验证边界。

## Localization

- [pt-BR 翻译术语表](localization/pt-br-translation-glossary.md)
