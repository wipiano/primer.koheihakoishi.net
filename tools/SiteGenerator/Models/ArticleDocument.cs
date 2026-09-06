namespace SiteGenerator.Models;

/// <summary>
/// 1記事分のパース済みデータ。同一ディレクトリ内での相互参照解決に使う。
/// </summary>
public sealed class ArticleDocument
{
    /// <summary>ディレクトリ名（例: "csharp"）</summary>
    public required string DirKey { get; init; }

    /// <summary>拡張子を除いたファイル名（例: "class-basic"）。出力ファイル名・リンクの基点になる。</summary>
    public required string FileBaseName { get; init; }

    public required string Title { get; init; }
    public required string Category { get; init; }
    public required int Order { get; init; }

    /// <summary>frontmatterに書かれた前提記事（ファイル名 + 一言）。</summary>
    public required IReadOnlyList<ArticleReference> Prerequisites { get; init; }

    /// <summary>frontmatterに書かれた次に読む記事（ファイル名 + 一言）。</summary>
    public required IReadOnlyList<ArticleReference> Next { get; init; }

    /// <summary>frontmatterに書かれた関連記事のファイル名（拡張子付き、例: "struct-basic.md"）。</summary>
    public required IReadOnlyList<string> RelatedFileNames { get; init; }

    /// <summary>frontmatterを除いたMarkdown本文。</summary>
    public required string BodyMarkdown { get; init; }
}
