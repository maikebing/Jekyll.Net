# Jekyll.Net Roadmap

`Jekyll.Net` 的目标不是做一个“能生成 HTML 就算完成”的静态站点生成器，而是逐步补齐 **Jekyll / GitHub Pages 常用行为**，同时保持 .NET 代码库可维护、可测试、可迭代。

这份路线图不再保留“无限扩张的待办清单”，而是明确区分：

1. 已经完成并可以收口的阶段
2. 还需要继续推进的兼容性缺口
3. 不适合作为主线 gate 的事项
4. 应该废弃的模糊表述

参考基线：

- Front Matter Defaults: https://jekyllrb.com/docs/configuration/front-matter-defaults/
- Front Matter: https://jekyllrb.com/docs/front-matter/
- Variables: https://jekyllrb.com/docs/variables/
- Includes: https://jekyllrb.com/docs/includes/
- Collections: https://jekyllrb.com/docs/collections/
- Rendering Process: https://jekyllrb.com/docs/rendering-process/

## 当前状态

截至 2026-03-25，当前仓库已经稳定具备：

- `build` 命令可生成 `sample-site` 与 `docs`
- `_config.yml`
- YAML Front Matter
- Markdown 转 HTML
- `_layouts` 与嵌套 layout
- `_includes`
- `_data`
- `_posts`
- `collections`
- `tags/categories`
- 一批基础 Liquid 标签与常见 filters
- Sass/SCSS 编译
- 静态资源复制到 `_site`
- `_config.yml defaults` 基础支持
- `drafts / future / unpublished`
- `excerpt_separator`
- static files front matter / defaults
- pagination 基线能力
- site 级 permalink fallback
- `relative_url / absolute_url / markdownify / where / sort / map / compact / jsonify / slugify`
- `docs` 与 `sample-site` golden output / snapshot 回归
- 多语言页面 `translation_links` 自动生成
- AI 翻译基础管线
- OpenAI / DeepSeek / Ollama / 任意 OpenAI-compatible 第三方 provider
- AI 翻译缓存、增量翻译、glossary

验证命令：

`dotnet test .\JekyllNet.slnx`

## Phase 0: 回归基线

状态：`已完成`

- [x] 增加自动化测试项目
- [x] 为 `sample-site` 增加 golden output / snapshot fixture
- [x] 为 `docs` 增加 golden output / snapshot fixture
- [x] 增加面向 Liquid 语义的小型 fixture
- [x] 固定回归命令与文档

结论：

- Phase 0 不再继续扩展。
- 后续只增补必要 fixture，不再把“加更多测试”单独当一个 phase。

## Phase 1: Liquid 语义校正

状态：`部分完成，继续保留`

已完成：

- [x] `if/for` 嵌套块处理更稳定
- [x] `capture`
- [x] `unless`
- [x] `case/when`
- [x] `contains`

剩余核心项：

- [ ] `assign` 作用域按更接近 Liquid/Jekyll 的语义收紧
- [ ] `include` 在条件块、循环块中的渲染时机进一步对齐，避免提前展开

为什么保留：

- 这两项仍然是主题兼容性的高频痛点。
- 它们是“语义正确性”问题，不是边角体验问题。

完成标准：

- `docs` 与 `sample-site` 不再依赖规避性模板写法
- `include` 不会在不应渲染时提前展开
- `assign` 在 fixture 中行为可预测、可回归

## Phase 2: 站点与内容语义

状态：`主体完成，剩余项收缩`

已完成：

- [x] nested `index.md` 默认 permalink 推导
- [x] `drafts`
- [x] `future`
- [x] `unpublished`
- [x] excerpts / `excerpt_separator`
- [x] static files front matter / defaults
- [x] pagination 基线接通
- [x] include / exclude 进入 skip 逻辑
- [x] site 级 permalink fallback
- [x] 一批高价值 `_config.yml` 站点选项已接入真实构建流程

不再保留的旧表述：

- [x] 废弃“补更多 `_config.yml` 站点级选项到构建流程”这种无限开放表述

替代做法：

- 后续只接受“明确、可验证、带 fixture 的 `_config.yml` 选项”进入 backlog
- 不再把“更多配置项”当作 Phase 2 的 gate

Phase 2 剩余只保留两类事项：

- [ ] pagination 继续往 GitHub Pages / Jekyll 细节对齐
- [ ] 发现新的高价值站点语义缺口时，按具体行为逐条补，不再开大口子

完成标准：

- 中小型真实站点可以较低成本迁移
- 内容状态控制、默认 URL、分页和静态资源语义不再是明显断层

## Phase 3: 高价值 Filters

状态：`已完成`

- [x] `relative_url`
- [x] `absolute_url`
- [x] `markdownify`
- [x] `replace_first`
- [x] `where`
- [x] `sort`
- [x] `map`
- [x] `compact`
- [x] `jsonify`
- [x] `slugify`

结论：

- Phase 3 主体关闭。
- 后续如果补 filter，只作为“主题兼容补丁”零散进入，不再维持一个单独大 phase。

## Phase 4: GitHub Pages 兼容边界明确化

状态：`未完成，但不废弃`

这不是“可有可无”的文档工作，而是后续所有兼容投入的边界说明层。它应该继续做，但要做成明确产物，而不是泛泛而谈。

保留项：

- [ ] 整理 GitHub Pages 常用特性的兼容矩阵
- [ ] 明确标记“已兼容 / 部分兼容 / 未兼容”
- [ ] 增加更接近真实 Pages 站点的 fixture
- [ ] 在文档中公开当前兼容边界与已知限制

已收口的重复项：

- [x] `docs` / `sample-site` 固定回归已由 Phase 0 完成，不再在 Phase 4 重复计入

建议产物：

- `docs` 中新增或重写一页兼容矩阵
- 针对 fixture 给出“这个 fixture 代表哪类 Pages 站点”的说明
- 对 README 与 docs 中的兼容声明统一口径

## 已废弃的表述

以下内容不是“功能废弃”，而是“路线图写法废弃”：

- [x] “补更多 `_config.yml` 选项”这种无限扩张项
- [x] 把已经完成的 snapshot 回归在多个 phase 里重复记账
- [x] 把 DevEx、分发、编辑器生态与核心兼容主线混在一个优先级里

## 不进入 Phase 0-4 主线的事项

这些事仍然有价值，但不再阻塞核心兼容闭环：

### DevEx / Distribution

- [ ] `serve/watch`
- [ ] 在 CLI 中显式暴露更多开关
- [ ] `dotnet tool`
- [ ] GitHub Action 构建样例
- [ ] `winget`
- [ ] 安装与升级文档

### 长期探索

- [ ] VS Code 本地预览体验
- [ ] Visual Studio 预览 / 构建期集成
- [ ] `new -t` 模板初始化
- [ ] 模板仓库同步与模板市场
- [ ] 文档编辑器与 AI 翻译工作流
- [ ] `md/pdf/word` 双向转换
- [ ] 图片抽取与 OCR 支持

## 现在真正剩下的主线工作

Phase 0 到 4 不再是“大面积未完成”状态，真正还应继续做的只剩这 4 件事：

1. 做完 `assign` 作用域
2. 做完 `include` 渲染时机
3. 把 pagination 再往 Jekyll / GitHub Pages 细节对齐一层
4. 输出可公开、可回归、可维护的兼容矩阵与边界说明

一句话总结：

主线不需要推倒重来，也不应该继续膨胀。Phase 0 和 Phase 3 可以关闭；Phase 2 收缩；Phase 1 和 Phase 4 保留并聚焦到真正还影响兼容性的少数缺口。
