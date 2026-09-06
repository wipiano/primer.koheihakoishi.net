---
title: "イミュータブル設計の基本"
category: "設計"
order: 28
related:
  - readonly-struct.md
  - record-basic.md
  - class-struct-record-basics.md
---

## 概要

- イミュータブル（不変）とは、生成後にインスタンスの状態が一切変わらない設計方針
- マルチスレッド処理や値オブジェクトの設計で、バグを減らすための基本原則
- 読了後、可変設計との違いと、不変にすべき場面の判断基準を理解できる

## なぜ必要なのか

- 可変なオブジェクトを複数箇所で共有すると、どこで値が変わったか追跡しづらくバグの温床になる
- 不変にすると、生成時点で状態が確定するため、共有しても安全でスレッドセーフになる
- 可変設計は柔軟に見えるが、状態変化の追跡コストという見えない負債を生む

## 仕組みと動作原理

- 不変オブジェクトは、コンストラクタ・init専用プロパティ・readonlyフィールドだけで構成する
- 「値を変える」操作は、既存インスタンスを書き換えるのではなく、新しいインスタンスを作って返す形にする
- C#では`record`の`with`式、`readonly struct`、`init`アクセサが不変設計を後押しする言語機能
- 不変オブジェクト同士の共有は、コピーの心配なく複数箇所から安全に参照できる

## 基本的な書き方とコード例

- 可変設計と不変設計の比較

```csharp
// 可変設計: 状態を直接書き換える
public class MutablePoint
{
    public int X { get; set; }
    public int Y { get; set; }
}
```

```csharp
// 不変設計: 変更操作は新しいインスタンスを返す
public record ImmutablePoint(int X, int Y)
{
    public ImmutablePoint MoveBy(int dx, int dy) => this with { X = X + dx, Y = Y + dy };
}

var p1 = new ImmutablePoint(0, 0);
var p2 = p1.MoveBy(1, 1);
Console.WriteLine(p1); // ImmutablePoint { X = 0, Y = 0 }（変化しない）
```

- 使い分けの判断基準
  - 値オブジェクト（金額・座標・期間など）や、複数スレッドで共有する値は不変にする
  - 生成コストが高く、頻繁に一部だけ更新したい大きな状態は可変設計を検討する

## よくある誤用・バグ

- **「不変」のつもりでコレクションだけ可変のまま持つ**
  - プロパティは変更不可でも、中の`List<T>`が可変だと外部から書き換えられてしまう

```csharp
// NG: プロパティはinitだが、Listの中身は外から変更できる
public record Cart(List<string> Items);

var cart = new Cart(new List<string> { "apple" });
cart.Items.Add("banana"); // 不変のはずが変更できてしまう
```

```csharp
// OK: 変更不可なコレクション型を使う
public record Cart(IReadOnlyList<string> Items);

var cart = new Cart(new List<string> { "apple" });
// cart.Items.Add(...) はコンパイルエラーになる
```

- **不変にするために毎回全コピーしてパフォーマンスを悪化させる**
  - 大きなコレクションを持つ状態を不変設計にすると、更新のたびに全コピーが発生し重くなる場合がある

```csharp
// NG: 巨大なリストを毎回丸ごとコピーして新しいインスタンスを作る
public record Report(IReadOnlyList<int> Values)
{
    public Report AddValue(int v) => this with { Values = Values.Append(v).ToList() };
}
```

```csharp
// OK: 更新頻度が高く大きいデータは、不変コレクション専用の型（ImmutableList<T>等）を使う
using System.Collections.Immutable;
public record Report(ImmutableList<int> Values)
{
    public Report AddValue(int v) => this with { Values = Values.Add(v) }; // 構造共有で低コスト
}
```

## パフォーマンス・セキュリティ上の注意点

- 不変オブジェクトはロック無しで複数スレッドから安全に読み取れる（スレッドセーフティの確保が容易）
- 単純な全コピーによる不変更新は、大きなデータでは性能劣化を招くため`ImmutableList<T>`等の構造共有コレクションを検討する
- 不変オブジェクトは生成のたびに新しいインスタンスができるため、頻度が高い場合はGC負荷が増える点に留意する

## 理解度チェックリスト

- [ ] イミュータブル設計と可変設計の違いを説明できる
- [ ] 不変設計がスレッドセーフティにどう寄与するか説明できる
- [ ] recordのwith式を使った不変な更新操作を書ける
- [ ] プロパティが不変でもコレクションが可変なままの落とし穴を説明できる
- [ ] 不変設計がパフォーマンスに与える影響と対策を説明できる
