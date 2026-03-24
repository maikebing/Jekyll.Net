---
title: "Getting Started"
description: "Run JekyllNet locally, generate the sample-site output, and preview the docs site."
permalink: /en/getting-started/
lang: "en"
nav_key: "docs"
---
# Getting Started

The fastest way to understand JekyllNet is still to run a real site build locally. The repository now gives you two strong fixtures for that:

- `sample-site` for a compact content and theme example
- `docs` for the project's own bilingual documentation site

## 1. Run a first build

```powershell
dotnet run --project .\JekyllNet.Cli -- build --source .\sample-site
```

By default the generated output goes to `sample-site\_site`.

If you want to inspect the docs site instead:

```powershell
dotnet run --project .\JekyllNet.Cli -- build --source .\docs --destination .\artifacts\docs-site
```

## 2. Open a live preview

The CLI now exposes both `watch` and `serve`, so you can choose between incremental rebuild feedback and a simple static preview server.

```powershell
dotnet run --project .\JekyllNet.Cli -- watch --source .\docs
dotnet run --project .\JekyllNet.Cli -- serve --source .\docs --port 5055
```

Use `watch` while editing content and templates. Use `serve` when you want a predictable local preview endpoint without wiring another server yourself.

## 3. What to inspect in the output

After the build completes, verify a few concrete things:

- Markdown pages become `index.html` output under their permalink paths.
- Posts under `_posts` use date-based or configured permalink patterns.
- `_layouts`, `_includes`, and Liquid expressions resolve into final HTML.
- Sass and SCSS assets are compiled or copied into the output tree.
- Site metadata from `_config.yml` appears in generated URLs, footer metadata, analytics snippets, or localization helpers when configured.

## 4. Good next reads

Once you have seen a build succeed, these pages become much more useful:

- [Compatibility Notes](/en/compatibility/)
- [Feature Overview](/en/blog/feature-overview/)
- [Configuration Guide](/en/blog/configuration-guide/)
- [CLI and Development Workflow](/en/blog/cli-workflow/)
---
