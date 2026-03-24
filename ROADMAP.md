# Jekyll.Net Roadmap

`Jekyll.Net` 的目标不是做一个“随便能出 HTML 的静态站点生成器”，而是逐步向 **Jekyll / GitHub Pages 常用行为** 靠拢，同时保持 .NET 代码库可维护、可测试、可迭代。

这份路线图分成三部分：

1. 当前已经具备什么
2. 目前还缺什么，为什么缺
3. 下一步按什么顺序补

参考基线主要来自 Jekyll 官方文档中对 front matter、front matter defaults、variables、collections、includes、rendering process 等行为的定义：

- Front Matter Defaults: https://jekyllrb.com/docs/configuration/front-matter-defaults/
- Front Matter: https://jekyllrb.com/docs/front-matter/
- Variables: https://jekyllrb.com/docs/variables/
- Includes: https://jekyllrb.com/docs/includes/
- Collections: https://jekyllrb.com/docs/collections/
- Rendering Process: https://jekyllrb.com/docs/rendering-process/

## Current Status

当前仓库已经覆盖：

- `_config.yml`
- YAML Front Matter
- Markdown 转 HTML
- `_layouts` 与嵌套 layout
- `_includes`
- `_data`
- `_posts`
- `collections`
- `tags/categories`
- 一批基础 Liquid 标签与 filters
- Sass/SCSS 编译
- 静态资源复制到 `_site`
- `_config.yml defaults` 的基础支持
  - 支持 `path`
  - 支持 `type`
  - 支持简单 `*` glob
  - front matter 显式值优先于 defaults

## Gap Analysis

### 1. 渲染引擎语义还不完整

这是当前最核心的缺口，因为很多“页面能不能像 Jekyll 一样工作”最终都卡在这里。

主要问题：

- `assign` 的作用域还不符合 Liquid 预期
  - 例如 header/footer 这种“先 assign，再在后文消费”的模板，目前只能通过改模板绕开。
- include 的展开时机还太早
  - 条件块里的 include 也会先展开，容易把不该渲染的模板碎片带出来。
- `if/for` 的实现仍偏正则驱动
  - 对复杂嵌套块、连续块、块内再嵌套块的稳定性不足。
- 还缺少 `capture`、`unless`、`case/when`、`contains` 等常见控制语法。

结论：

这部分是后面继续补 filter、补站点变量、补复杂模板前的地基。

### 2. 站点建模与 Jekyll 还有一段差距

主要问题：

- nested `index.md` 的默认 permalink 行为还不够像 Jekyll
- `drafts / future / unpublished` 还没做
- excerpts / `excerpt_separator` 还没做
- pagination 还没做
- 静态文件 front matter 与 defaults 还没对齐
- `_config.yml` 的更多配置项还没落实到构建流程

结论：

这部分决定“一个真实项目站点能不能平移过来”。

### 3. Liquid / Jekyll filters 覆盖还偏薄

当前只实现了一小部分常见 filters，距离真实项目模板常用集合还有差距。

优先缺口：

- `relative_url`
- `absolute_url`
- `markdownify`
- `replace_first`
- `where`
- `sort`
- `map`
- `compact`
- `jsonify`
- `slugify`

结论：

这部分决定“模板能不能少改甚至不改”。

### 4. GitHub Pages 兼容层还不够系统

主要问题：

- 还没有一份清晰的 GitHub Pages 兼容矩阵
- 缺少对官方常用插件行为边界的整理
- 缺少更接近真实站点的回归样本

结论：

这部分决定“兼容 GitHub Pages”到底是口号，还是可验证的目标。

### 5. 开发体验和回归验证还需要补

主要问题：

- 目前缺少系统化测试
- 缺少 golden output / snapshot fixture
- 还没有 `serve/watch` 工作流
- 缺少针对 docs/sample-site 的固定回归命令

结论：

这部分决定后续迭代速度和信心。

## Step-by-Step Plan

### Phase 1: 引擎语义校正

目标：先把 Liquid 渲染流程从“能跑一些模板”提升到“能稳定跑文档站模板”。

- [x] 支持 `_config.yml defaults`
- [ ] 重做 `assign` 作用域
- [ ] 修正 include 在条件块中的展开顺序
- [ ] 让 `if/for` 支持更稳定的嵌套块处理
- [ ] 增加 `capture / unless / case / contains`
- [ ] 修正 nested `index.md` 的默认 permalink 推导

建议顺序：

1. `assign` + include 顺序
2. 块语法嵌套稳定性
3. nested index/permalink

### Phase 2: 内容语义补齐

目标：让项目能更像真正的 Jekyll 内容管线。

- [ ] `drafts`
- [ ] `future`
- [ ] `unpublished`
- [ ] excerpts / `excerpt_separator`
- [ ] static files 的 front matter 与 defaults
- [ ] 更完整的 `_config.yml` 站点级选项落地

建议顺序：

1. drafts / future / unpublished
2. excerpt
3. static file metadata

### Phase 3: Liquid / Filters 扩展

目标：减少现有主题和项目模板的迁移成本。

- [ ] `relative_url`
- [ ] `absolute_url`
- [ ] `markdownify`
- [ ] `replace_first`
- [ ] `where`
- [ ] `sort`
- [ ] `map`
- [ ] `compact`
- [ ] `jsonify`
- [ ] `slugify`

建议顺序：

1. URL 相关 filters
2. 集合类 filters
3. 内容类 filters

### Phase 4: GitHub Pages 兼容层

目标：把“兼容 GitHub Pages”从经验判断变成明确边界。

- [ ] 整理 GitHub Pages 常用特性兼容清单
- [ ] 补一批 Pages 风格 fixture site
- [ ] 标记“已兼容 / 部分兼容 / 未兼容”
- [ ] 对 docs、sample-site、最小 blog fixture 做回归

### Phase 5: 工程化与开发体验

目标：让后续完善进入可持续迭代状态。

- [ ] 增加自动化测试项目
- [ ] 增加 golden output 对比
- [ ] 增加 `serve/watch`
- [ ] 增加回归命令和文档

## Immediate Next Milestones

如果按投入产出比排序，下一轮最值得优先做的是：

1. `assign` 作用域与 include 渲染顺序
2. nested `index.md` 默认 permalink
3. `relative_url / absolute_url / markdownify`
4. drafts / future / unpublished
5. golden output 回归测试
6. 加入单元测试， 对大量解析案例进行测试用例。
7. 支持GitHub Pages 的 构建， 比如项目生成一个GitHub Action 供第三方使用。 
8. 支持VS Code 预览 比如打开了http://localhost:4004 然后本地查看生成后的效果。 
9. 可以实施生成， 边写边预览生成的效果。 程序监测到文件变化就生成。 可以配置， 生成间隔 ， 或者立刻生成。 
10. 支持 dotnet tool ， 可以安装为工具。 
11. 支持winget 安装
12. 支持VS2026 预览  编译时构建。 如果可能支持vs浏览器扩展， 可以编写文档， 边预览。 
13. 实现一个文档编辑器， 编辑器里面可以设置AI翻译， 可以从主写语言自动翻译为其他语言。还要能实时预览。 
14. 支持命令行 new  -t 指定模版， 可以自动从模版仓库下载模版。 其他人可以在github上同步模版， gitee上自动同步。 new的时候程序自动从这俩库取， 能访问到谁就从谁哪取模版。 
15. 我们的模版上需要有模版商店。 来展示上面说到的仓库里的案例。 
16. 让我们把md to pdf , md to word  , pdf to md , word to md 都要实现， 包括里面包含的图标也要取出来。 如果pdf是全图片， 则使用ocr进行识别。 
  
