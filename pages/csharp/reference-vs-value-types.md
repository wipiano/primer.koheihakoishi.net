---
title: "参照型と値型の違い"
category: "文法"
order: 8
prerequisites:
  - file: class-basic.md
    note: classでインスタンス(実体)を作れることが分かっていれば十分
next:
  - file: struct-basic.md
    note: 値型を自分で定義するstructの書き方につながる
related:
  - class-struct-record-basics.md
---

`class`で作った型は参照型、`int`や`bool`のような組み込みの型は値型と呼ばれる。変数に代入したときにコピーされるものが違う。

## なぜ必要か

同じ「代入」でも、値型は値そのものがコピーされ、参照型は参照(実体がどこにあるかを示す情報)だけがコピーされる。この違いを知らないと、片方の変数を書き換えたつもりが別の変数まで変わってしまう挙動に出会ったとき、原因を追えない。

## 動かしてみる

まず値型の`int`で試す。

```csharp
var a = 10;
var b = a;
b = 20;
Console.WriteLine(a);
Console.WriteLine(b);
```

```
10
20
```

`b = a`の時点で、`a`の値10が`b`にコピーされる。以降`b`を書き換えても`a`には影響しない。`a`と`b`は代入した瞬間から別々の値になる。

同じことを`class`で作った型でやると、結果が変わる。

```csharp
var p1 = new Point(1, 1);
var p2 = p1;
p2.X = 99;
Console.WriteLine(p1.X);
Console.WriteLine(p2.X);

public class Point
{
    public int X { get; set; }
    public int Y { get; set; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }
}
```

```
99
99
```

`p2 = p1`でコピーされるのは`Point`の実体ではなく、参照だ。`p1`と`p2`は同じ1つの実体を指しているので、`p2.X`を書き換えると`p1.X`も99になる。参照先の実体はヒープと呼ばれる領域に置かれ、どの変数からも参照されなくなるとGC(ガベージコレクタ)が自動的に回収する。

## 最低限の理解

- 値型の変数は**値そのものを持つ**。`int`、`bool`、`double`など組み込みの型が値型で、代入や引数渡しのたびに値がコピーされる
- 参照型の変数は**実体の場所(参照)を持つ**。`class`で作った型はすべて参照型で、代入や引数渡しでは参照だけがコピーされ、実体は増えない
- メソッドに値型を渡すと渡るのは値のコピーなので、メソッドの中で書き換えても呼び出し元の変数は変わらない
- メソッドに参照型を渡すと渡るのは参照のコピーなので、メソッドの中でプロパティを書き換えると呼び出し元から見ても変わる
- 値型は自分で定義することもできる(`struct`というキーワードを使う)。組み込みの値型と同じ挙動になるが、書き方は次の記事で扱う

## ⚠️ メソッドに渡したクラスを中で書き換えると呼び出し元も変わる

参照型をメソッドの引数に渡して中でプロパティを書き換えると、渡っているのは参照なので呼び出し元の実体も書き換わる。計算結果だけを返すつもりのメソッドが、引数を直接書き換えてしまう間違いが起きやすい。

```csharp
// NG: 割引後の金額を計算するだけのつもりが、呼び出し元のTotalまで変えてしまう
var order = new Order(1000);
var discounted = CalculateDiscount(order);
Console.WriteLine(order.Total);

int CalculateDiscount(Order order)
{
    order.Total -= 100;
    return order.Total;
}

public class Order
{
    public int Total { get; set; }

    public Order(int total)
    {
        Total = total;
    }
}
```

```
900
```

```csharp
// OK: 引数のプロパティを書き換えず、計算結果だけを返す
var order = new Order(1000);
var discounted = CalculateDiscount(order);
Console.WriteLine(order.Total);
Console.WriteLine(discounted);

int CalculateDiscount(Order order) => order.Total - 100;

public class Order
{
    public int Total { get; set; }

    public Order(int total)
    {
        Total = total;
    }
}
```

```
1000
900
```

## 課題

自分の業務で扱うクラス(顧客、注文など)を1つ選び、そのインスタンスをメソッドの引数に渡してプロパティを書き換えるコードを書いてみる。呼び出し元の値がどう変わるか、実行して確かめる。
