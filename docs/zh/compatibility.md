---
title: 兼容性说明
description: Jekyll.Net 已覆盖、部分覆盖与尚未覆盖的能力边界。
permalink: /zh/compatibility/
lang: zh-CN
nav_key: docs
---
# 兼容性说明

`Jekyll.Net` 的目标不是做一个“带一点 Liquid 语法的任意静态站点生成器”，而是逐步补齐常见的 Jekyll / GitHub Pages 行为，并且保持 .NET 代码库可测试、可维护。

## 兼容矩阵

| 领域 | 状态 | 说明 |
| --- | --- | --- |
| `_config.yml` 读取 | 已完成 | 常用站点配置、defaults、include/exclude、站点级 permalink fallback、页脚和统计配置都已进入构建流程。 |
| Front matter | 已完成 | YAML front matter、defaults、页面变量、静态文件 front matter 与 excerpt 都已支持。 |
| Markdown 与布局 | 已完成 | Markdown 转 HTML、嵌套 layout、includes、collections、posts、tags、categories 都已在正常构建链路中。 |
| Snapshot 回归 | 已完成 | `docs` 与 `sample-site` 都有 golden output / snapshot 回归保护。 |
| Filters | 已完成 | `relative_url`、`absolute_url`、`markdownify`、`where`、`sort`、`map`、`compact`、`jsonify`、`slugify` 等高价值 filter 已补齐。 |
| 发布语义 | 已完成 | `drafts`、`future`、`unpublished` 都已经接入真实构建行为。 |
| Pagination | 部分完成 | 基线分页已可用，支持 `paginate`、`paginate_path`、结构化 `pagination.per_page`、`pagination.path` 和单页禁用。更细的 Jekyll 对齐还在继续。 |
| Liquid 控制语义 | 部分完成 | `if`、`for`、`unless`、`case/when`、`capture`、`contains` 可用。剩余缺口主要集中在 `assign` 作用域和少量 include 时机边角。 |
| GitHub Pages 对齐 | 部分完成 | 已覆盖常见行为，但还没有承诺与某个 GitHub Pages 固定版本 1:1 完全一致。 |
| 插件生态 | 尚未支持 | 第三方 Jekyll 插件兼容仍然不在当前支持边界内。 |

## 已验证的 Fixture

- `sample-site`：一个更接近真实内容站点的 golden regression 基线
- `docs`：项目自己的多语言文档站，也由 snapshot 测试锁定输出
- 聚焦 renderer 的小型 fixture：用于验证 assign、include、嵌套块、filters、pagination 等语义

## 当前边界

现在的 `Jekyll.Net` 已经比较适合：

- 项目文档站
- 中小型内容站
- 带 AI 辅助翻译的多语言文档站
- 主要依赖常见 Liquid 与常见 `_config.yml` 行为的 GitHub Pages 风格主题

但它现在还没有宣称：

- 与 GitHub Pages 固定版本完全等价
- 广泛兼容第三方插件
- 完整覆盖 Liquid 全语法
- 与不同分页插件的所有细节完全一致

## 仍在继续补齐的地方

- `assign` 在更多 Liquid 边角中的作用域对齐
- `include` 在剩余 corner case 中的渲染时机
- 当前分页基线之上的更多细节
- 按功能域持续收紧兼容边界说明
