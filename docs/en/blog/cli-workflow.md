---
title: "CLI and Development Workflow"
description: "How build, watch, serve, packaging, and CI fit together in JekyllNet."
permalink: /en/blog/cli-workflow/
lang: "en"
nav_key: "blog"
---
# CLI and Development Workflow

JekyllNet now has a fuller command-line story. That matters because a generator becomes much easier to evaluate once local iteration, preview, packaging, and CI all fit together.

## The core commands

```powershell
dotnet run --project .\JekyllNet.Cli -- build --source .\sample-site
dotnet run --project .\JekyllNet.Cli -- watch --source .\docs
dotnet run --project .\JekyllNet.Cli -- serve --source .\docs --port 5055
```

- `build` is the deterministic generation step.
- `watch` is for content or template editing loops.
- `serve` is for local preview with a stable HTTP endpoint.

## Tooling and distribution

The repository now also includes:

- `dotnet tool` packaging metadata for the CLI
- GitHub Actions examples for CI and release artifacts
- winget packaging templates
- README guidance for installation and upgrades

## A practical local routine

1. Run `watch` against the site you are editing.
2. Use `serve` when you want a stable preview URL.
3. Run `dotnet test .\JekyllNet.slnx` before landing changes.
---
