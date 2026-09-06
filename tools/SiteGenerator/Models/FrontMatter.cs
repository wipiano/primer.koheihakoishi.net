namespace SiteGenerator.Models;

/// <summary>
/// 記事Markdownのfrontmatter（YAML）をそのままマッピングするモデル。
/// pages/**/.claude/skills/csharp-article/template.md のフォーマットに準拠。
/// </summary>
public sealed class FrontMatter
{
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Order { get; set; }
    public List<string> Related { get; set; } = new();
}
