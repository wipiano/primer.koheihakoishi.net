---
title: "オブジェクトの等価性を正しく実装する"
category: "文法"
order: 27
prerequisites:
  - file: override.md
    note: virtual/overrideでメソッドを上書きする書き方が分かれば十分
  - file: interface-basic.md
    note: インターフェースを実装する書き方が分かれば十分
next:
  - file: record-basic.md
    note: この記事で書いた実装をrecordが自動で生成してくれる仕組みが分かる
  - file: operator-overloading.md
    note: ここで触れたoperator ==を含め、演算子を自分で定義する書き方につながる
related:
  - class-struct-record-basics.md
---

等価性とは、ある型のインスタンス2つを「同じ」とみなす基準のことだ。`Equals`と`GetHashCode`は、その基準を型自身に書き込むメソッドだ。

- 同じインスタンスを指しているかで判定するのが参照等価。`class`の既定はこれ
- プロパティの値がすべて一致するかで判定するのが値等価
- `HashSet`や`Dictionary`は、この2つのメソッドを使って要素を探す

## なぜ必要か

「何をもって同じとみなすか」は概念ごとに違う。注文は内容が同じでも別の注文だが、座標(1, 2)は誰が作っても同じ1つの点だ。中身が意味のすべてである型は、その基準を型に書かないかぎり、既定の参照等価で判定されてしまう。

## 動かしてみる

まず何も書かずに、値が同じ2つの`Point`を比べる。

```csharp
var p1 = new Point(1, 2);
var p2 = new Point(1, 2);
Console.WriteLine(p1 == p2); // 既定は参照等価。別々にnewしたので等しくない

var points = new HashSet<Point> { p1 };
Console.WriteLine(points.Contains(p2)); // ContainsもEqualsで探すので見つからない

public class Point
{
    public int X { get; }
    public int Y { get; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }
}
```

```
False
False
```

値で比較したい型は、`IEquatable<T>`を実装し、`Equals(object)`と`GetHashCode`をオーバーライド(『virtual/overrideの基本』で説明した、基底クラスのメンバーを派生クラスで上書きする仕組み)する。`IEquatable<T>`は、`object`ではなく自分の型を受け取る`Equals`を持つことを約束するインターフェース(『インターフェースの基本』で説明した、実装すべきメンバーを型で約束する仕組み)だ。

```csharp
var p1 = new Point(1, 2);
var p2 = new Point(1, 2);
Console.WriteLine(p1.Equals(p2));

var points = new HashSet<Point> { p1 };
Console.WriteLine(points.Contains(p2)); // GetHashCodeで場所を絞り、Equalsで照合する

public class Point : IEquatable<Point>
{
    public int X { get; }
    public int Y { get; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public bool Equals(Point? other) => other is not null && X == other.X && Y == other.Y; // 比較の本体
    public override bool Equals(object? obj) => Equals(obj as Point); // objectで来ても本体に流す
    public override int GetHashCode() => HashCode.Combine(X, Y); // Equalsと同じプロパティで計算する
}
```

```
True
True
```

## 最低限の理解

- **Equalsがtrueを返す2つのインスタンスは、必ず同じGetHashCodeを返さなければならない。** これを等価性の契約と呼ぶ
- `Equals`だけをオーバーライドして`GetHashCode`を直さないと警告CS0659が出る。HashSetやDictionaryでは値が一致していても見つからなくなる
- `GetHashCode`の計算に使うプロパティは、登録後に値を変えられないようにする。書き換えるとハッシュコードが変わり、以後見つからなくなるので`get`だけか`init`にする
- `==`演算子は`Equals`と連動しない。値で比較したいなら`operator ==`を別途定義する必要がある
- 値の一致だけで比較したいシンプルなデータ型なら、ここまでの実装を自動で持つ`record`に任せる選択肢もある

## ⚠️ Equalsだけ上書きしてGetHashCodeを直さない

```csharp
// NG: Equalsだけオーバーライドし、GetHashCodeは既定のまま
var p1 = new Point(1, 2);
var p2 = new Point(1, 2);
Console.WriteLine(p1.Equals(p2));

var points = new HashSet<Point> { p1 };
Console.WriteLine(points.Contains(p2)); // 参照ベースのハッシュコードで別の場所を調べてしまう

public class Point
{
    public int X { get; }
    public int Y { get; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override bool Equals(object? obj) =>
        obj is Point other && X == other.X && Y == other.Y;
}
```

```
warning CS0659: 'Point' は Object.Equals(object o) をオーバーライドしますが、Object.GetHashCode() をオーバーライドしません。
True
False
```

```csharp
// OK: Equalsと同じプロパティでGetHashCodeもセットで実装する
var p1 = new Point(1, 2);
var p2 = new Point(1, 2);

var points = new HashSet<Point> { p1 };
Console.WriteLine(points.Contains(p2));

public class Point
{
    public int X { get; }
    public int Y { get; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override bool Equals(object? obj) =>
        obj is Point other && X == other.X && Y == other.Y;

    public override int GetHashCode() => HashCode.Combine(X, Y);
}
```

```
True
```

## 課題

自分の業務でよく使うデータ(注文、商品など)を1つ選んでクラスにし、`IEquatable<T>`と`Equals(object)`、`GetHashCode`を実装して、`new`で2つ作った同じ値のインスタンスが`HashSet`で正しく見つかることを確認する。
