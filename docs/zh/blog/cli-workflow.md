---
title: "CLI 与开发工作流"
description: "述 build、watch、serve、打包与 CI 于 JekyllNet 中之衔接。"
permalink: /zh/blog/cli-workflow/
lang: "zh-CN"
nav_key: "blog"
---
# CLI 与开发工作流

吾今已有较完备之命令行工作流。盖静态生成器若仅能 build，而不能便于迭代、预览、打包与自动化，则其用未广。

## 核心三令

```powershell
dotnet run --project .\JekyllNet.Cli -- build --source .\sample-site
dotnet run --project .\JekyllNet.Cli -- watch --source .\docs
dotnet run --project .\JekyllNet.Cli -- serve --source .\docs --port 5055
```

- `build`：定式生成之步
- `watch`：适于文稿与模板反复修订
- `serve`：适于稳定本地预览

## 打包与分发

仓库今亦已具：

- `dotnet tool` 打包元数据
- GitHub Actions 示例
- winget 模板
- README 中之安装与升级说明

## 一条实用之例行

1. 修站点时，常开 `watch`。
2. 欲得稳定预览地址时，再开 `serve`。
3. 提交前，行 `dotnet test .\JekyllNet.slnx`。
---
