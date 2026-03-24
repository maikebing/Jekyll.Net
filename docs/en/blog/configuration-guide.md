---
title: "Configuration Guide"
description: "Grouped explanations for the _config.yml options that are already practical in JekyllNet."
permalink: /en/blog/configuration-guide/
lang: "en"
nav_key: "blog"
---
# Configuration Guide

JekyllNet does not need every historical Jekyll option to be useful. What matters first is that the common options behave in a predictable and theme-friendly way.

## Core site settings

| Key | Why it matters |
| --- | --- |
| `title`, `description`, `lang`, `timezone` | Site identity and metadata used across templates. |
| `url`, `baseurl` | Required for correct relative or absolute links in themes and deployments. |
| `markdown` | Keeps the Markdown engine choice explicit. |
| `permalink` | Provides a site-level fallback pattern for generated output. |

## Content defaults and publishing controls

| Key | Current use |
| --- | --- |
| `defaults` | Assign shared front matter values by path scope. |
| `include` / `exclude` | Control which files enter or skip the build. |
| `show_excerpts` | Controls whether excerpts are exposed on site collections. |
| `excerpt_separator` | Defines where excerpts split. |
| `paginate` / `paginate_path` | Baseline top-level pagination settings. |
| `pagination.per_page` / `pagination.path` | Nested pagination configuration, closer to real Jekyll practice. |

## Theme-facing and operational settings

| Key | Current use |
| --- | --- |
| footer metadata | Generates filing, policy, contact, and legal footer blocks when configured. |
| analytics | Generates supported analytics snippets from compact configuration values. |
| locales | Defines language roots and labels for multilingual sites. |
| localized defaults | Lets each locale customize labels, links, and theme text without hardcoding them in templates. |

## AI translation settings

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

This area already supports OpenAI, DeepSeek, Ollama, and OpenAI-compatible third-party providers through a custom `base_url`.
---
