namespace SiteGenerator.Models;

/// <summary>
/// order (1-100) から5段階の難易度表示を導出する。
/// レベル = (order - 1) / 20 （0〜4の整数）。
/// </summary>
public static class Difficulty
{
    private static readonly string[] Labels = ["入門", "初級", "中級", "上級", "エキスパート"];

    public static int LevelOf(int order)
    {
        var level = (Math.Max(order, 1) - 1) / 20;
        return Math.Clamp(level, 0, 4);
    }

    public static string LabelOf(int order) => Labels[LevelOf(order)];

    /// <summary>★を埋めた分だけ表示する5段階の星表記（例: "★★★☆☆"）。</summary>
    public static string StarsOf(int order)
    {
        var filled = LevelOf(order) + 1;
        return new string('★', filled) + new string('☆', 5 - filled);
    }
}
