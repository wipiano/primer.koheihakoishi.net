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

`class`のインスタンスは既定では参照(メモリ上の場所)が同じかどうかで比較される。2つの変数が同じインスタンスを指しているかの判定を参照等価、プロパティの値がすべて一致しているかの判定を値等価と呼ぶ。

## 値が同じでも別物として扱われる

DictionaryのキーやHashSetの要素に自分で定義した型を使うと、値が同じインスタンスなのに見つからないという問題が起きる。原因の多くは、`Equals`を上書きしていないために既定の参照等価のまま比較されていることにある。

```csharp
var p1 = new Point(1, 2);
var p2 = new Point(1, 2);

Console.WriteLine(p1 == p2);

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
}
```

```
False
False
```

p1とp2はXとYの値が完全に一致しているが、`==`はFalseを返す。`class`の`==`は既定で`Equals`と同じ参照等価の判定をするため、別々に`new`した時点で別物として扱われる。HashSetの`Contains`も内部で`Equals`を使って要素を探すので、同じ値のp2を渡しても見つからない。

## 値で比較できるようにする

値が同じなら同じとみなしたい型は、`IEquatable<T>`を実装したうえで`Equals(object)`と`GetHashCode`もオーバーライド(『virtual/overrideの基本』で説明した、基底クラスのメンバーを派生クラスで上書きする仕組み)する。`IEquatable<T>`は型安全な比較用のメソッドを約束するインターフェース(『インターフェースの基本』で説明した、実装すべきメンバーを型で約束する仕組み)だ。

```csharp
var p1 = new Point(1, 2);
var p2 = new Point(1, 2);

Console.WriteLine(p1.Equals(p2));

var points = new HashSet<Point> { p1 };
Console.WriteLine(points.Contains(p2));

public class Point : IEquatable<Point>
{
    public int X { get; }
    public int Y { get; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public bool Equals(Point? other) => other is not null && X == other.X && Y == other.Y;
    public override bool Equals(object? obj) => Equals(obj as Point);
    public override int GetHashCode() => HashCode.Combine(X, Y);
}
```

```
True
True
```

`Equals(Point)`はobjectへのキャストなしに比較できる型安全な版で、`Equals(object)`はそれを呼ぶだけにする。`GetHashCode`は`HashCode.Combine`に、`Equals`で比較しているのと同じプロパティを渡して計算する。HashSetはこのハッシュコードで探す場所を絞り込んでから`Equals`で照合するので、両方を正しく実装して初めて`Contains`が値で見つけられる。

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
}
```

```
warning CS0659: 'Point' は Object.Equals(object o) をオーバーライドしますが、Object.GetHashCode() をオーバーライドしません。
True
False
```

`p1.Equals(p2)`はTrueなのに、同じ値のp2を`Contains`で調べるとFalseになる。`GetHashCode`が既定の参照ベースのままなので、p1とp2は別のハッシュコードを持ち、HashSetは違う場所を調べてしまう。

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
