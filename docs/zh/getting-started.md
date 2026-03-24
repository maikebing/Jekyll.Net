---
title: "快速开始"
description: "试于本地运行 JekyllNet，构其 sample-site，并观 docs 站点。"
permalink: /zh/getting-started/
lang: "zh-CN"
nav_key: "docs"
---
# 快速开始

欲知吾是否可用，莫若先行一构。今仓库之中，有二处最宜试之：

- `sample-site`，可以观内容站与主题组织之法
- `docs`，可以观吾自家中英双语文档站之生成

## 一、先行一构

```powershell
dotnet run --project .\JekyllNet.Cli -- build --source .\sample-site
```

其默认输出，在 `sample-site\_site`。

若君欲直观当前文档站之产物，可行：

```powershell
dotnet run --project .\JekyllNet.Cli -- build --source .\docs --destination .\artifacts\docs-site
```

## 二、启其本地预览

今 CLI 已具 `watch` 与 `serve` 二途，可用于文稿反复修订与静态预览。

```powershell
dotnet run --project .\JekyllNet.Cli -- watch --source .\docs
dotnet run --project .\JekyllNet.Cli -- serve --source .\docs --port 5055
```

- `watch` 宜于频改频观之时。
- `serve` 宜于求一稳定本地地址以便浏览之时。

## 三、构成之后，当验何物

生成既毕，可先验此数端：

- Markdown 页面是否各归其 permalink 所指目录
- `_posts` 是否依日期或站点级 permalink 规则输出
- `_layouts`、`_includes` 与 Liquid 表达式是否皆落成 HTML
- Sass / SCSS 资源是否已入输出树
- `_config.yml` 之站点配置，是否已见于链接、页脚、统计脚本与多语辅助信息

## 四、可续观者

既见一构功成，则下列诸篇最宜续读：

- [兼容性说明](/zh/compatibility/)
- [特性总览](/zh/blog/feature-overview/)
- [配置指南](/zh/blog/configuration-guide/)
- [CLI 与开发工作流](/zh/blog/cli-workflow/)
