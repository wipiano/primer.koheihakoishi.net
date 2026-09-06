using Microsoft.AspNetCore.Html;

namespace SiteGenerator.Models;

// Razorテンプレート (Templates/*.cshtml) に渡すビューモデル群。

public sealed class DirectoryLink
{
    public required string Title { get; init; }
    public required string DirKey { get; init; }
}

public sealed class RootIndexViewModel
{
    public required IReadOnlyList<DirectoryLink> Directories { get; init; }
}

public sealed class ArticleSummary
{
    public required string Title { get; init; }
    public required string FileBaseName { get; init; }
    public required int Order { get; init; }
    public string Stars => Difficulty.StarsOf(Order);
    public string Label => Difficulty.LabelOf(Order);
}

public sealed class CategoryGroup
{
    public required string CategoryName { get; init; }
    public required IReadOnlyList<ArticleSummary> Articles { get; init; }
}

public sealed class DirIndexViewModel
{
    public required string DirTitle { get; init; }
    public required IReadOnlyList<CategoryGroup> Categories { get; init; }
}

public sealed class ArticleLink
{
    public required string Title { get; init; }
    public required string FileBaseName { get; init; }
}

public sealed class ArticleViewModel
{
    public required string DirTitle { get; init; }
    public required string Title { get; init; }
    public required string Category { get; init; }
    public required int Order { get; init; }
    public string Stars => Difficulty.StarsOf(Order);
    public string Label => Difficulty.LabelOf(Order);
    public required IHtmlContent BodyHtml { get; init; }
    public required IReadOnlyList<ArticleLink> RelatedArticles { get; init; }
}
