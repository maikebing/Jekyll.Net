---
title: "AI 翻译工作流"
description: "述 JekyllNet 之多语、provider、缓存、增量翻译与 glossary 机制。"
permalink: /zh/blog/ai-translation/
lang: "zh-CN"
nav_key: "blog"
---
# AI 翻译工作流

吾今不复局于手工维持 `zh` 与 `en` 二语。凡 `_config.yml` 中有 `ai` 配置者，构建时即可助其生养更多语种之页。

## 所支持之提供者

今可接：

- OpenAI
- DeepSeek
- Ollama
- 借自定义 `base_url` 所接之 OpenAI 兼容第三方提供者

## 多语流转之法

吾所荐者，大略如下：

- 于 `_config.yml` 中定义 `locales`
- 令源语言页面保有可镜像之稳定路径
- 使构建器自动生成 `translation_links`
- 待语种渐多，手工难继，再接 AI 翻译

## 缓存、增量与术语表

今 AI 翻译已具三事：

- 缓存：结果存于 `.jekyllnet/translation-cache.json`
- 增量：未变内容，不复每次重译
- glossary：品牌名、产品名、专有术语可定其译
---
