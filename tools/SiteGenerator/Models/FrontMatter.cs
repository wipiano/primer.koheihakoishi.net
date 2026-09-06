namespace SiteGenerator.Models;

/// <summary>
/// 記事Markdownのfrontmatter（YAML）をそのままマッピングするモデル。
/// pages/**/.claude/skills/csharp-article/template.md のフォーマットに準拠。
/// 記事同士のリンクは本文には書かず、すべてfrontmatterで表現する
/// （ジェネレータはMarkdown本文中のリンクを解決しない）。
/// </summary>
public sealed class FrontMatter
{
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Order { get; set; }

    /// <summary>この記事を読む前に読んでおくとよい記事。本記事より order が小さいことが期待される。</summary>
    public List<ArticleReference> Prerequisites { get; set; } = new();

    /// <summary>この記事の次に読むとよい記事。</summary>
    public List<ArticleReference> Next { get; set; } = new();

    /// <summary>前提でも次でもないが関連する記事のファイル名（拡張子付き）。</summary>
    public List<string> Related { get; set; } = new();
}

/// <summary>
/// 他記事への参照。<c>file</c> はファイル名（拡張子付き、例: "struct-basic.md"）、
/// <c>note</c> は読者向けの一言（前提なら「何が分かっていれば十分か」、次なら「なぜ次に読むとよいか」）。
/// </summary>
public sealed class ArticleReference
{
    public string File { get; set; } = string.Empty;
    public string? Note { get; set; }
}
