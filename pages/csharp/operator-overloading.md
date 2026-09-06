---
title: "演算子オーバーロードの基本"
category: "文法"
order: 30
prerequisites:
  - file: equality.md
    note: Equals/GetHashCodeの実装と、==がEqualsと連動しないという話が分かれば十分
next:
  - file: type-conversion-operators.md
    note: 同じoperatorキーワードを使った、型変換演算子の定義につながる
related: []
---

`operator`は、`+`や`==`のような演算子を、自分で定義したクラスに対しても使えるようにするキーワードだ。

- 演算子ごとに`public static`なメソッドとして定義する
- 呼び出す側は組み込みの型と同じ書き方(`a + b`)で使える

## なぜ必要か

金額や座標は、`int`と同じように「足せる」「等しいか比べられる」という意味を本来持つ概念だ。演算子を定義するとは、その意味を型自身に持たせ、組み込みの型と同じ語彙で扱えるようにすることだ。定義しなければ、`Point`同士の`+`はコンパイルエラーになる。

## 動かしてみる

```csharp
var p1 = new Point(1, 2);
var p2 = new Point(3, 4);
var sum = p1 + p2; // コンパイラが下のoperator +を呼び出す

Console.WriteLine($"({sum.X}, {sum.Y})");

public class Point
{
    public int X { get; }
    public int Y { get; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static Point operator +(Point a, Point b) => new Point(a.X + b.X, a.Y + b.Y); // 2つのPointから新しいPointを返す
}
```

```
(4, 6)
```

`p1`も`p2`も書き換えず、結果は新しいインスタンスとして返す。組み込みの`int`の`+`と同じ振る舞いにそろえるのが基本だ。

## 最低限の理解

- 演算子は`public static`なメソッドとして定義する。インスタンスメソッドにはできない
- `+`を定義すれば、`a += b`も自動で使えるようになる(`a = a + b`と同じ扱いのため)
- **`==`と`!=`は必ずペアで定義する**。片方だけではコンパイルエラーになる
- `==`を定義するときは、Equals/GetHashCodeとの整合性(『オブジェクトの等価性を正しく実装する』で説明した契約)も守る必要がある
- 呼び出す側から見て意味が自然に伝わる演算子だけを定義する。曖昧な演算子の乱用はコードを読みにくくする

## ⚠️ ==だけ定義して!=を定義し忘れる

```csharp
// NG: == だけ定義していて != が無い
var p1 = new Point(1, 2);
var p2 = new Point(1, 2);
Console.WriteLine(p1 == p2);

public class Point
{
    public int X { get; }
    public int Y { get; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static bool operator ==(Point a, Point b) => a.X == b.X && a.Y == b.Y;
}
```

```
error CS0216: 演算子 'Point.operator ==(Point, Point)' を定義するには、合致する演算子 '!=' が必要です
```

```csharp
// OK: == と != を必ずペアで定義する
var p1 = new Point(1, 2);
var p2 = new Point(1, 2);
Console.WriteLine(p1 == p2);

public class Point
{
    public int X { get; }
    public int Y { get; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static bool operator ==(Point a, Point b) => a.X == b.X && a.Y == b.Y;
    public static bool operator !=(Point a, Point b) => !(a == b); // ==の否定として定義すれば矛盾しない
}
```

```
True
```

このままだとコンパイラは「`Equals`や`GetHashCode`をオーバーライドしていない」という警告(CS0660・CS0661)も出す。`HashSet`や`Dictionary`でも同じ判定結果にしたいなら、『オブジェクトの等価性を正しく実装する』の通り両方合わせて実装する。

## 課題

自分の業務で扱うデータ(金額、数量など)を1つ選び、`+`のように意味が自然に伝わる演算子を1つ定義してみる。
