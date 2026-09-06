// 技術解説サイト用の静的サイトジェネレータ。
// pages/ 以下のMarkdown記事(frontmatter付き)を読み込み、HTMLと
// AI向けMarkdownの両方を出力ディレクトリに生成するコンソールアプリ。
// HTMLのテンプレート展開には ASP.NET Core の Razor エンジン (RazorLight) を利用する。

using System.Text.Encodings.Web;
using System.Text.Unicode;
using Markdig;
using Microsoft.AspNetCore.Html;
using RazorLight;
using SiteGenerator.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

var options = CliOptions.Parse(args);
var pagesDir = options.PagesDir ?? Path.Combine(FindRepoRoot(), "pages");
var outputDir = options.OutputDir ?? Path.Combine(FindRepoRoot(), "dist");

if (!Directory.Exists(pagesDir))
{
    Console.Error.WriteLine($"入力ディレクトリが見つかりません: {pagesDir}");
    return 1;
}

Console.WriteLine($"入力: {pagesDir}");
Console.WriteLine($"出力: {outputDir}");

if (Directory.Exists(outputDir))
{
    Directory.Delete(outputDir, recursive: true);
}
Directory.CreateDirectory(outputDir);

var markdownPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
var razorEngine = new RazorLightEngineBuilder()
    .UseFileSystemProject(Path.Combine(AppContext.BaseDirectory, "Templates"))
    .UseMemoryCachingProvider()
    .Build();
// 既定のHtmlEncoderは非ASCII文字(日本語)を数値文字参照にエスケープするため、
// 出力がUTF-8である前提でUnicode全域を許可し、素の日本語文字列を出力させる。
razorEngine.Options.PreRenderCallbacks.Add(page =>
{
    if (page is TemplatePage templatePage)
    {
        templatePage.HtmlEncoder = HtmlEncoder.Create(UnicodeRanges.All);
    }
});

var yamlDeserializer = new DeserializerBuilder()
    .WithNamingConvention(CamelCaseNamingConvention.Instance)
    .IgnoreUnmatchedProperties()
    .Build();
var yamlSerializer = new SerializerBuilder()
    .WithNamingConvention(CamelCaseNamingConvention.Instance)
    // note が無い参照で "note: " を出力しないようにする
    .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
    .Build();

var directoryLinks = new List<DirectoryLink>();

foreach (var dir in Directory.GetDirectories(pagesDir).OrderBy(d => d, StringComparer.Ordinal))
{
    var dirKey = Path.GetFileName(dir);
    if (dirKey.StartsWith('.'))
    {
        continue;
    }

    var dirTitle = ReadDirectoryTitle(dir) ?? dirKey;
    Console.WriteLine($"- ディレクトリ: {dirKey} ({dirTitle})");

    var mdFiles = Directory.GetFiles(dir, "*.md", SearchOption.TopDirectoryOnly)
        .Where(f => !Path.GetFileName(f).StartsWith('_'))
        .OrderBy(f => f, StringComparer.Ordinal)
        .ToList();

    var articles = new List<ArticleDocument>();
    foreach (var file in mdFiles)
    {
        var (frontMatter, body) = ParseFrontMatter(File.ReadAllText(file), yamlDeserializer);
        articles.Add(new ArticleDocument
        {
            DirKey = dirKey,
            FileBaseName = Path.GetFileNameWithoutExtension(file),
            Title = frontMatter.Title,
            Category = frontMatter.Category,
            Order = frontMatter.Order,
            Prerequisites = frontMatter.Prerequisites,
            Next = frontMatter.Next,
            RelatedFileNames = frontMatter.Related,
            BodyMarkdown = body,
        });
    }

    var articlesByBaseName = articles.ToDictionary(a => a.FileBaseName);
    var outDir = Path.Combine(outputDir, dirKey);
    Directory.CreateDirectory(outDir);

    foreach (var article in articles)
    {
        var prerequisites = ResolveReferences(article, article.Prerequisites.Select(r => (r.File, r.Note)), "前提記事", articlesByBaseName);
        var next = ResolveReferences(article, article.Next.Select(r => (r.File, r.Note)), "次に読む記事", articlesByBaseName);
        var related = ResolveReferences(article, article.RelatedFileNames.Select(f => (f, (string?)null)), "関連記事", articlesByBaseName);

        // 前提記事は本記事より易しい（orderが小さい）はず。逆転していたら記事側の設定ミスの可能性が高い
        foreach (var p in prerequisites)
        {
            var target = articlesByBaseName[p.FileBaseName];
            if (target.Order >= article.Order)
            {
                Console.WriteLine($"  [警告] {article.FileBaseName} (order {article.Order}): 前提記事 '{p.FileBaseName}' の order ({target.Order}) が同じか大きいです");
            }
        }

        var bodyHtml = Markdown.ToHtml(article.BodyMarkdown, markdownPipeline);
        var viewModel = new ArticleViewModel
        {
            DirTitle = dirTitle,
            Title = article.Title,
            Category = article.Category,
            Order = article.Order,
            BodyHtml = new HtmlString(bodyHtml),
            Prerequisites = prerequisites,
            NextArticles = next,
            RelatedArticles = related,
        };

        var content = await razorEngine.CompileRenderAsync("Article.cshtml", viewModel);
        var html = HtmlShell.Wrap(article.Title, content, depth: 1);
        File.WriteAllText(Path.Combine(outDir, $"{article.FileBaseName}.html"), html);

        var markdownOut = BuildArticleMarkdown(article, prerequisites, next, related, yamlSerializer);
        File.WriteAllText(Path.Combine(outDir, $"{article.FileBaseName}.md"), markdownOut);
    }

    var categories = articles
        .GroupBy(a => a.Category)
        .OrderBy(g => g.Min(a => a.Order))
        .Select(g => new CategoryGroup
        {
            CategoryName = g.Key,
            Articles = g.OrderBy(a => a.Order)
                .ThenBy(a => a.Title, StringComparer.Ordinal)
                .Select(a => new ArticleSummary { Title = a.Title, FileBaseName = a.FileBaseName, Order = a.Order })
                .ToList(),
        })
        .ToList();

    var dirIndexModel = new DirIndexViewModel { DirTitle = dirTitle, Categories = categories };
    var dirContent = await razorEngine.CompileRenderAsync("DirIndex.cshtml", dirIndexModel);
    var dirHtml = HtmlShell.Wrap(dirTitle, dirContent, depth: 1);
    File.WriteAllText(Path.Combine(outDir, "index.html"), dirHtml);
    File.WriteAllText(Path.Combine(outDir, "index.md"), BuildDirIndexMarkdown(dirTitle, categories));

    directoryLinks.Add(new DirectoryLink { Title = dirTitle, DirKey = dirKey });
}

var rootModel = new RootIndexViewModel { Directories = directoryLinks };
var rootContent = await razorEngine.CompileRenderAsync("RootIndex.cshtml", rootModel);
var rootHtml = HtmlShell.Wrap("技術解説サイト", rootContent, depth: 0);
File.WriteAllText(Path.Combine(outputDir, "index.html"), rootHtml);
File.WriteAllText(Path.Combine(outputDir, "index.md"), BuildRootIndexMarkdown(directoryLinks));

var assetsOutDir = Path.Combine(outputDir, "assets");
Directory.CreateDirectory(assetsOutDir);
File.Copy(
    Path.Combine(AppContext.BaseDirectory, "Assets", "style.css"),
    Path.Combine(assetsOutDir, "style.css"),
    overwrite: true);

var repoRoot = FindRepoRoot();
var cnamePath = Path.Combine(repoRoot, "CNAME");
if (File.Exists(cnamePath))
{
    File.Copy(cnamePath, Path.Combine(outputDir, "CNAME"), overwrite: true);
}

Console.WriteLine("完了しました。");
return 0;

static string? ReadDirectoryTitle(string dir)
{
    var metaPath = Path.Combine(dir, "_directory.yml");
    if (!File.Exists(metaPath))
    {
        return null;
    }

    var deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();
    var meta = deserializer.Deserialize<DirectoryMetadata>(File.ReadAllText(metaPath));
    return string.IsNullOrWhiteSpace(meta?.Title) ? null : meta.Title;
}

static (FrontMatter FrontMatter, string Body) ParseFrontMatter(string text, IDeserializer deserializer)
{
    var lines = text.Replace("\r\n", "\n").Split('\n');
    if (lines.Length == 0 || lines[0].Trim() != "---")
    {
        throw new FormatException("frontmatterの開始 '---' が見つかりません");
    }

    var endIndex = -1;
    for (var i = 1; i < lines.Length; i++)
    {
        if (lines[i].Trim() == "---")
        {
            endIndex = i;
            break;
        }
    }

    if (endIndex < 0)
    {
        throw new FormatException("frontmatterの終端 '---' が見つかりません");
    }

    var yaml = string.Join('\n', lines[1..endIndex]);
    var body = string.Join('\n', lines[(endIndex + 1)..]).TrimStart('\n');
    var frontMatter = deserializer.Deserialize<FrontMatter>(yaml) ?? new FrontMatter();
    return (frontMatter, body);
}

/// <summary>
/// frontmatterに書かれた他記事への参照を、同一ディレクトリ内の記事に解決する。
/// 見つからない参照は警告を出してスキップする（ビルドは止めない）。
/// </summary>
static List<ArticleLink> ResolveReferences(
    ArticleDocument article,
    IEnumerable<(string File, string? Note)> references,
    string kind,
    IReadOnlyDictionary<string, ArticleDocument> articlesByBaseName)
{
    var result = new List<ArticleLink>();
    foreach (var (file, note) in references)
    {
        var baseName = Path.GetFileNameWithoutExtension(file);
        if (string.IsNullOrWhiteSpace(baseName))
        {
            Console.WriteLine($"  [警告] {article.FileBaseName}: {kind} にファイル名が空の項目があります");
            continue;
        }

        if (articlesByBaseName.TryGetValue(baseName, out var target))
        {
            result.Add(new ArticleLink { Title = target.Title, FileBaseName = target.FileBaseName, Note = note });
        }
        else
        {
            Console.WriteLine($"  [警告] {article.FileBaseName}: {kind} '{baseName}' が見つかりません");
        }
    }
    return result;
}

static string BuildArticleMarkdown(
    ArticleDocument article,
    IReadOnlyList<ArticleLink> prerequisites,
    IReadOnlyList<ArticleLink> next,
    IReadOnlyList<ArticleLink> related,
    ISerializer yamlSerializer)
{
    // 解決できた参照だけをfrontmatterに書き戻す（欠けているリンクをAI向け出力に残さない）
    var frontMatter = new FrontMatter
    {
        Title = article.Title,
        Category = article.Category,
        Order = article.Order,
        Prerequisites = prerequisites.Select(r => new ArticleReference { File = $"{r.FileBaseName}.md", Note = r.Note }).ToList(),
        Next = next.Select(r => new ArticleReference { File = $"{r.FileBaseName}.md", Note = r.Note }).ToList(),
        Related = related.Select(r => $"{r.FileBaseName}.md").ToList(),
    };
    var yaml = yamlSerializer.Serialize(frontMatter).TrimEnd('\n');

    var sb = new System.Text.StringBuilder();
    sb.Append("---\n").Append(yaml).Append("\n---\n\n");

    if (prerequisites.Count > 0)
    {
        sb.Append("## 前提知識\n\n");
        AppendLinkList(sb, prerequisites);
        sb.Append('\n');
    }

    sb.Append(article.BodyMarkdown.TrimEnd('\n')).Append('\n');

    if (next.Count > 0)
    {
        sb.Append("\n## 次に読む\n\n");
        AppendLinkList(sb, next);
    }

    if (related.Count > 0)
    {
        sb.Append("\n## 関連記事\n\n");
        AppendLinkList(sb, related);
    }

    return sb.ToString();

    static void AppendLinkList(System.Text.StringBuilder sb, IReadOnlyList<ArticleLink> links)
    {
        foreach (var l in links)
        {
            sb.Append($"- [{l.Title}]({l.FileBaseName}.md)");
            if (!string.IsNullOrWhiteSpace(l.Note))
            {
                sb.Append(" … ").Append(l.Note);
            }
            sb.Append('\n');
        }
    }
}

static string BuildDirIndexMarkdown(string dirTitle, IReadOnlyList<CategoryGroup> categories)
{
    var sb = new System.Text.StringBuilder();
    sb.Append("[Top](../index.md)\n\n");
    sb.Append($"# {dirTitle}\n\n");
    foreach (var cat in categories)
    {
        sb.Append($"## {cat.CategoryName}\n\n");
        foreach (var a in cat.Articles)
        {
            sb.Append($"- [{a.Title}]({a.FileBaseName}.md) 難易度: {a.Stars}（{a.Label}）\n");
        }
        sb.Append('\n');
    }
    return sb.ToString();
}

static string BuildRootIndexMarkdown(IReadOnlyList<DirectoryLink> directories)
{
    var sb = new System.Text.StringBuilder();
    sb.Append("# 技術解説サイト\n\n");
    sb.Append("カテゴリ別の目次です。\n\n");
    foreach (var d in directories)
    {
        sb.Append($"- [{d.Title}]({d.DirKey}/index.md)\n");
    }
    return sb.ToString();
}

static string FindRepoRoot()
{
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir is not null)
    {
        if (Directory.Exists(Path.Combine(dir.FullName, "pages")))
        {
            return dir.FullName;
        }
        dir = dir.Parent;
    }

    throw new InvalidOperationException(
        "リポジトリルート（pages ディレクトリを含む場所）が見つかりません。--pages / --out で明示的に指定してください。");
}

sealed class DirectoryMetadata
{
    public string? Title { get; set; }
}

sealed class CliOptions
{
    public string? PagesDir { get; private set; }
    public string? OutputDir { get; private set; }

    public static CliOptions Parse(string[] args)
    {
        var opts = new CliOptions();
        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--pages" when i + 1 < args.Length:
                    opts.PagesDir = Path.GetFullPath(args[++i]);
                    break;
                case "--out" when i + 1 < args.Length:
                    opts.OutputDir = Path.GetFullPath(args[++i]);
                    break;
            }
        }
        return opts;
    }
}

static class HtmlShell
{
    private const string HighlightJsVersion = "11.9.0";

    public static string Wrap(string title, string bodyContent, int depth)
    {
        var prefix = string.Concat(Enumerable.Repeat("../", depth));
        return $$"""
            <!doctype html>
            <html lang="ja">
            <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <title>{{System.Net.WebUtility.HtmlEncode(title)}}</title>
            <link rel="stylesheet" href="{{prefix}}assets/style.css">
            <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/{{HighlightJsVersion}}/styles/github-dark.min.css">
            </head>
            <body>
            {{bodyContent}}
            <script src="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/{{HighlightJsVersion}}/highlight.min.js"></script>
            <script>hljs.highlightAll();</script>
            </body>
            </html>
            """;
    }
}
