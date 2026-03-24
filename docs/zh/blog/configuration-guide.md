---
title: "配置指南"
description: "分组而述 JekyllNet 当前已堪倚赖之 _config.yml 选项。"
permalink: /zh/blog/configuration-guide/
lang: "zh-CN"
nav_key: "blog"
---
# 配置指南

吾不必先收尽一切旧日 Jekyll 配置，方可为用。其所重者，在常用之项先得稳定、可测、可为主题所亲。

## 站点基础之设

| Key | 其用 |
| --- | --- |
| `title`、`description`、`lang`、`timezone` | 站点身份与元数据所系。 |
| `url`、`baseurl` | 关乎相对链接、绝对链接与部署地址。 |
| `markdown` | 明示 Markdown 引擎。 |
| `permalink` | 为全站生成结果立 fallback 规则。 |

## defaults 与发布控制

| Key | 今之用途 |
| --- | --- |
| `defaults` | 按路径范围，批量施共享 front matter。 |
| `include` / `exclude` | 决文件入构与否。 |
| `show_excerpts` | 定 excerpt 是否暴露于站点集合。 |
| `excerpt_separator` | 定摘要之切分处。 |
| `paginate` / `paginate_path` | 基础分页配置。 |
| `pagination.per_page` / `pagination.path` | 更近 Jekyll 实用之嵌套分页配置。 |

## 主题与运维相关之设

| Key | 今之用途 |
| --- | --- |
| footer 元数据 | 生成备案、条款、隐私、联系方式等页脚块。 |
| analytics | 以简要 ID 或字符串生成所支持之统计脚本。 |
| locales | 定多语站点之语种根路径与显示标签。 |
| locale 级 defaults | 令不同语种各有其导航文辞、链接与 UI 文本。 |

## AI 翻译之设

```yml
ai:
  provider: openai
  model: gpt-5-mini
  api_key: ${OPENAI_API_KEY}
  translate:
    targets:
      - fr
      - ja
    front_matter_keys:
      - title
      - description
    glossary: _i18n/glossary.yml
```

今已支持 OpenAI、DeepSeek、Ollama，亦可借自定义 `base_url` 接 OpenAI 兼容之第三方提供者。
---
