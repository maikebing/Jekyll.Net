---
title: "AI Translation Workflow"
description: "How locales, provider support, cache, incremental translation, and glossary support work in JekyllNet."
permalink: /en/blog/ai-translation/
lang: "en"
nav_key: "blog"
---
# AI Translation Workflow

JekyllNet is no longer limited to manually maintained `zh` and `en` content. If `_config.yml` includes an `ai` section, the build can help create and maintain more locale variants.

## Provider support

The current provider model supports:

- OpenAI
- DeepSeek
- Ollama
- OpenAI-compatible third-party providers through a custom `base_url`

## How the multilingual flow works

The intended pattern is:

- define locales in `_config.yml`
- keep source-language pages in a predictable mirrored path structure
- let the builder generate `translation_links`
- use AI translation when you want to scale past hand-maintained copies

## Cache, incremental reuse, and glossary

The translation workflow now includes three operational pieces that matter in practice:

- cache under `.jekyllnet/translation-cache.json`
- incremental translation so unchanged content is not re-requested
- glossary support for product names, repository names, and domain terms
---
