---
title: "構造体(struct)の基本"
category: "文法"
order: 11
related:
  - class-basic.md
  - reference-vs-value-types.md
  - readonly-struct.md
  - class-struct-record-basics.md
---

## 概要

- structはデータをまとめる値型の設計図。`class`と似た構文だが代入時の挙動が異なる
- 座標・金額・IDなど、小さくコピーされても問題ない値を表すのに向く
- 読了後、structの基本構文と値型ならではの挙動を理解できる

## なぜ必要なのか

- 小さな値をclassで表すと、都度ヒープ確保が発生しGC負荷が増える場合がある
- structを使うと、スタックや配列内に直接値を並べられ、アロケーションを抑えられる
- ただし万能ではなく、可変な大きい値をstructにすると逆にコピーコストで悪化する

## 仕組みと動作原理

- structは値型。変数はインスタンスの値そのものを保持し、代入・引数渡しのたびにコピーされる
- コピー後の値を変更しても、コピー元には影響しない
- classと違い、structには継承ができない（インターフェースの実装は可能）
- structでも既定コンストラクタや初期化子は使えるが、全フィールドの初期化が必要になる場面がある

## 基本的な書き方とコード例

- 最小の使い方

```csharp
public struct Point
{
    public int X { get; init; }
    public int Y { get; init; }
}

var p1 = new Point { X = 1, Y = 2 };
var p2 = p1; // 値がコピーされる
```

- コピーされることの確認

```csharp
var p1 = new Point { X = 1, Y = 2 };
var p2 = p1 with { X = 100 }; // structでもwith式は使える

Console.WriteLine(p1.X); // 1（p2の変更はp1に影響しない）
```

- 使い分けの判断基準
  - フィールド数が少なく（目安3〜4個以内）不変な値ならstructを検討する
  - 参照渡しで共有したい、または大きく可変なデータはclassを使う

## よくある誤用・バグ

- **可変structをプロパティ経由で変更する**
  - 一時的なコピーに対する変更になり、元の値が変わらない

```csharp
// NG: プロパティ経由のstructは一時コピーになるため変更が反映されない
public class Container
{
    public Point Position { get; set; }
}
var container = new Container { Position = new Point { X = 0, Y = 0 } };
container.Position.X = 10; // コンパイルエラー（一時コピーへの代入とみなされる）
```

```csharp
// OK: 新しい値ごと代入し直す
container.Position = new Point { X = 10, Y = container.Position.Y };
```

- **大きなstructを頻繁にコピーしてしまう**
  - フィールド数が多いstructをメソッド引数で値渡しすると、都度コピーコストがかかる

```csharp
// NG: 巨大なstructを値渡しし続ける
public struct LargeData
{
    public long A, B, C, D, E, F, G, H;
}
void Process(LargeData data) { /* 呼び出しごとに64バイトコピー */ }
```

```csharp
// OK: 大きい場合はclassにする、またはin参照で渡す
void Process(in LargeData data) { /* コピーせず参照で渡す */ }
```

## パフォーマンス・セキュリティ上の注意点

- structが大きくなるほどコピーコストが増える。目安として16バイト程度を超えたらclass化を検討する
- structをinterface型の変数へ代入・キャストするとボックス化が発生し、ヒープ確保とGC負荷が生じる
- 頻繁にコピーされる大きいstructは`in`パラメータで参照渡しにするとコピーを避けられる

## 理解度チェックリスト

- [ ] structが値型であることを説明できる
- [ ] structの代入がコピーになることを説明できる
- [ ] 可変structがバグの原因になりやすい理由を説明できる
- [ ] structのボックス化がいつ発生するかを説明できる
- [ ] structとclassの使い分けの判断基準を説明できる
- [ ] 大きいstructをinで渡すメリットを説明できる
