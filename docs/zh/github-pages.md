---
title: "站点部署"
description: "以 docs 为 GitHub Pages 源站，或于 CI 中行其同法。"
permalink: /zh/github-pages/
lang: "zh-CN"
nav_key: "docs"
---
# 站点部署

今仓库之 `docs`，已成一套可用于 GitHub Pages 之源站结构。其既为对外文档，亦为吾之 golden regression fixture 之一。

## docs 之分工

其内大略如下：

- `docs/_config.yml`：主站配置、locales、导航文辞与 defaults
- `docs/_layouts`、`docs/_includes`：页面外壳
- `docs/assets`：样式与品牌资源
- `docs/zh`、`docs/en`：中英文镜像内容

## 直用 GitHub Pages

若欲以 GitHub 直接发布此源站，可设为：

`Deploy from a branch` -> `main` -> `/docs`

若欲更正其域，可并设自定义域名 `jekyllnet.help`。今仓库中亦已置 `docs/CNAME` 以备之。

## 关于域名配置

今配置以：

```yml
url: https://jekyllnet.help
baseurl: ""
```

既用独立域名，则 `baseurl` 宜为空。若后日域名有迁，请同步改此二项，以免生成链接失其所归。

## 何时改用 Actions

若君欲令构建可审、可控、可固其版本，则宜用仓库中的 GitHub Actions 示例。此法尤适于：

- 固定 CI 中之 .NET SDK 版本
- 发布生成之产物，而非原始源目录
- 使站点构建与打包发布共归一套流程

更详之命令行与自动化说明，可参 [CLI 与开发工作流](/zh/blog/cli-workflow/)。
---
