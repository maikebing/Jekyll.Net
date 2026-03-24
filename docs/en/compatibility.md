---
title: Compatibility Notes
description: What Jekyll.Net already implements, what is partial, and what still remains.
permalink: /en/compatibility/
lang: en
nav_key: docs
---
# Compatibility Notes

Jekyll.Net is not trying to be “a random static site generator with some Liquid syntax”. The direction is to close the gap with common Jekyll and GitHub Pages behavior in a way that stays testable and maintainable in .NET.

## Compatibility Matrix

| Area | Status | Notes |
| --- | --- | --- |
| `_config.yml` loading | Done | Common site config, defaults, include/exclude, permalink fallback, footer and analytics options are wired into the build. |
| Front matter | Done | YAML front matter, defaults, page variables, static file front matter, and excerpts are supported. |
| Markdown and layouts | Done | Markdown to HTML, nested layouts, includes, collections, posts, tags, and categories are in the normal build pipeline. |
| Snapshot regression | Done | `docs` and `sample-site` are protected by golden output regression tests. |
| Filters | Done | High-value theme filters such as `relative_url`, `absolute_url`, `markdownify`, `where`, `sort`, `map`, `compact`, `jsonify`, and `slugify` are available. |
| Publishing semantics | Done | `drafts`, `future`, and `unpublished` are all connected to real build behavior. |
| Pagination | Partial | Baseline pagination works, including `paginate`, `paginate_path`, structured `pagination.per_page`, `pagination.path`, and per-page disable. More edge cases still need Jekyll-level alignment. |
| Liquid control flow | Partial | `if`, `for`, `unless`, `case/when`, `capture`, and `contains` work. The remaining compatibility gap is mainly `assign` scope polish and include timing corner cases. |
| GitHub Pages parity | Partial | Common behavior is covered, but this is not yet version-pinned one-to-one parity with a specific GitHub Pages release. |
| Plugin ecosystem | Not yet | Third-party Jekyll plugin compatibility is still outside the supported boundary. |

## Verified Fixtures

- `sample-site`: a practical content and theme-style site used as a golden regression baseline
- `docs`: the project’s own multilingual documentation site, also locked by snapshot tests
- focused renderer fixtures: targeted tests for Liquid semantics such as assign, include, nested blocks, filters, and pagination

## Current Boundary

Jekyll.Net is already a reasonable fit for:

- project documentation sites
- small and medium content sites
- multilingual docs with AI-assisted translation
- GitHub Pages style themes that stay within common Liquid and config behavior

It is not yet claiming:

- full GitHub Pages version parity
- broad plugin compatibility
- complete Liquid language coverage
- perfect pagination parity with every Jekyll plugin variant

## What Is Still Being Closed

- `assign` scope alignment in more Liquid edge cases
- `include` rendering timing in the remaining corner cases
- pagination details beyond the current baseline
- sharper compatibility statements per feature group over time
