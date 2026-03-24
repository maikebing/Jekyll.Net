namespace JekyllNet.Tests;

public sealed class SiteBuilderBehaviorTests
{
    [Fact]
    public async Task Build_ExcludesDraftsFutureAndUnpublished_ByDefault()
    {
        var sourceDirectory = CreateContentSiteFixture();
        var outputDirectory = await TestInfrastructure.BuildSiteAsync(sourceDirectory);

        Assert.True(File.Exists(Path.Combine(outputDirectory, "index.html")));
        Assert.True(File.Exists(Path.Combine(outputDirectory, "blog", "index.html")));
        Assert.True(File.Exists(Path.Combine(outputDirectory, "2000", "01", "02", "published", "index.html")));
        Assert.False(File.Exists(Path.Combine(outputDirectory, "2099", "01", "01", "future", "index.html")));
        Assert.False(File.Exists(Path.Combine(outputDirectory, "2000", "01", "03", "unpublished", "index.html")));
        Assert.False(File.Exists(Path.Combine(outputDirectory, "2000", "01", "04", "draft-entry", "index.html")));
    }

    [Fact]
    public async Task Build_IncludesDraftsFutureAndUnpublished_WhenEnabled()
    {
        var sourceDirectory = CreateContentSiteFixture();
        var outputDirectory = await TestInfrastructure.BuildSiteAsync(
            sourceDirectory,
            includeDrafts: true,
            includeFuture: true,
            includeUnpublished: true);

        Assert.True(File.Exists(Path.Combine(outputDirectory, "2099", "01", "01", "future", "index.html")));
        Assert.True(File.Exists(Path.Combine(outputDirectory, "2000", "01", "03", "unpublished", "index.html")));
        Assert.True(File.Exists(Path.Combine(outputDirectory, "2000", "01", "04", "draft-entry", "index.html")));
    }

    [Fact]
    public async Task Build_RespectsExcerptSeparator()
    {
        var sourceDirectory = TestInfrastructure.CreateSiteFixture(new Dictionary<string, string>
        {
            ["_config.yml"] = """
                excerpt_separator: <!--more-->
                show_excerpts: true
                """,
            ["_layouts/default.html"] = """
                {{ content }}
                """,
            ["_posts/2000-01-02-excerpted.md"] = """
                ---
                layout: default
                title: Excerpted
                ---
                Intro paragraph.
                <!--more-->
                Rest of the article.
                """,
            ["index.html"] = """
                ---
                layout: default
                ---
                {{ site.posts | map: "excerpt" | first }}
                """
        });

        var outputDirectory = await TestInfrastructure.BuildSiteAsync(sourceDirectory);
        var output = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "index.html"));

        Assert.Contains("Intro paragraph.", output, StringComparison.Ordinal);
        Assert.DoesNotContain("Rest of the article.", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Build_AppliesFrontMatterAndDefaultsToStaticFiles()
    {
        var sourceDirectory = TestInfrastructure.CreateSiteFixture(new Dictionary<string, string>
        {
            ["_config.yml"] = """
                defaults:
                  - scope:
                      path: assets
                    values:
                      custom_label: from-default
                """,
            ["index.html"] = """
                ---
                ---
                {{ site.static_files | where: "name", "app.js" | map: "custom_label" | first }}
                """,
            ["assets/app.js"] = """
                ---
                title: App Asset
                ---
                console.log("{{ page.title }} {{ page.custom_label }}");
                """
        });

        var outputDirectory = await TestInfrastructure.BuildSiteAsync(sourceDirectory);
        var indexOutput = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "index.html"));
        var scriptOutput = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "assets", "app.js"));

        Assert.Contains("from-default", indexOutput, StringComparison.Ordinal);
        Assert.Contains("console.log(\"App Asset from-default\");", scriptOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("---", scriptOutput, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Build_GeneratesPaginationPages()
    {
        var sourceDirectory = TestInfrastructure.CreateSiteFixture(new Dictionary<string, string>
        {
            ["_config.yml"] = """
                paginate: 2
                paginate_path: /blog/page:num/
                """,
            ["_layouts/default.html"] = """
                {{ content }}
                """,
            ["blog/index.html"] = """
                ---
                layout: default
                permalink: /blog/
                ---
                page={{ paginator.page }}/{{ paginator.total_pages }}
                {% for post in paginator.posts %}
                {{ post.title }}
                {% endfor %}
                next={{ paginator.next_page_path }}
                prev={{ paginator.previous_page_path }}
                """,
            ["_posts/2000-01-01-first.md"] = """
                ---
                layout: default
                title: First
                ---
                First
                """,
            ["_posts/2000-01-02-second.md"] = """
                ---
                layout: default
                title: Second
                ---
                Second
                """,
            ["_posts/2000-01-03-third.md"] = """
                ---
                layout: default
                title: Third
                ---
                Third
                """
        });

        var outputDirectory = await TestInfrastructure.BuildSiteAsync(sourceDirectory);
        var page1 = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "blog", "index.html"));
        var page2 = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "blog", "page2", "index.html"));

        Assert.Contains("page=1/2", page1, StringComparison.Ordinal);
        Assert.Contains("Third", page1, StringComparison.Ordinal);
        Assert.Contains("Second", page1, StringComparison.Ordinal);
        Assert.DoesNotContain("First", page1, StringComparison.Ordinal);
        Assert.Contains("next=/blog/page2/", page1, StringComparison.Ordinal);

        Assert.Contains("page=2/2", page2, StringComparison.Ordinal);
        Assert.Contains("First", page2, StringComparison.Ordinal);
        Assert.Contains("prev=/blog/", page2, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Build_RespectsConfigExcludeAndInclude()
    {
        var sourceDirectory = TestInfrastructure.CreateSiteFixture(new Dictionary<string, string>
        {
            ["_config.yml"] = """
                exclude:
                  - ignored.md
                include:
                  - .well-known
                """,
            ["index.html"] = """
                ---
                ---
                home
                """,
            ["ignored.md"] = """
                ---
                ---
                ignored
                """,
            [".well-known/security.txt"] = "contact: team@example.com"
        });

        var outputDirectory = await TestInfrastructure.BuildSiteAsync(sourceDirectory);

        Assert.True(File.Exists(Path.Combine(outputDirectory, "index.html")));
        Assert.False(File.Exists(Path.Combine(outputDirectory, "ignored", "index.html")));
        Assert.True(File.Exists(Path.Combine(outputDirectory, ".well-known", "security.txt")));
    }

    [Fact]
    public async Task Build_UsesConfigPermalinkPatternForPosts()
    {
        var sourceDirectory = TestInfrastructure.CreateSiteFixture(new Dictionary<string, string>
        {
            ["_config.yml"] = """
                permalink: /blog/:year/:month/:day/:title/
                """,
            ["_layouts/default.html"] = """
                {{ content }}
                """,
            ["_posts/2000-01-02-custom-link.md"] = """
                ---
                layout: default
                title: Custom Link
                ---
                custom link
                """
        });

        var outputDirectory = await TestInfrastructure.BuildSiteAsync(sourceDirectory);

        Assert.True(File.Exists(Path.Combine(outputDirectory, "blog", "2000", "01", "02", "custom-link", "index.html")));
    }

    [Fact]
    public async Task Build_HidesPostExcerptsFromSitePosts_WhenShowExcerptsIsFalse()
    {
        var sourceDirectory = TestInfrastructure.CreateSiteFixture(new Dictionary<string, string>
        {
            ["_config.yml"] = """
                show_excerpts: false
                excerpt_separator: <!--more-->
                """,
            ["_layouts/default.html"] = """
                {{ content }}
                """,
            ["_posts/2000-01-02-excerpted.md"] = """
                ---
                layout: default
                title: Excerpted
                ---
                Intro paragraph.
                <!--more-->
                Rest of the article.
                """,
            ["index.html"] = """
                ---
                layout: default
                ---
                {{ site.posts | map: "excerpt" | first | default: "none" }}
                """
        });

        var outputDirectory = await TestInfrastructure.BuildSiteAsync(sourceDirectory);
        var output = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "index.html"));

        Assert.Contains("none", output, StringComparison.Ordinal);
    }

    private static string CreateContentSiteFixture()
    {
        return TestInfrastructure.CreateSiteFixture(new Dictionary<string, string>
        {
            ["_config.yml"] = """
                title: Feature Test Site
                """,
            ["_layouts/default.html"] = """
                <html>
                <body>
                {{ content }}
                </body>
                </html>
                """,
            ["index.md"] = """
                ---
                layout: default
                title: Home
                ---
                # Home
                """,
            ["blog/index.md"] = """
                ---
                layout: default
                title: Blog
                ---
                # Blog
                """,
            ["_posts/2000-01-02-published.md"] = """
                ---
                layout: default
                title: Published
                ---
                Published
                """,
            ["_posts/2099-01-01-future.md"] = """
                ---
                layout: default
                title: Future
                ---
                Future
                """,
            ["_posts/2000-01-03-unpublished.md"] = """
                ---
                layout: default
                title: Unpublished
                published: false
                ---
                Unpublished
                """,
            ["_drafts/draft-entry.md"] = """
                ---
                layout: default
                title: Draft
                date: 2000-01-04
                ---
                Draft
                """
        });
    }
}
