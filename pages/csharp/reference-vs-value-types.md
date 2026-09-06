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

C#の型は、変数が値そのものを持つ値型と、変数が実体の場所(参照)を持つ参照型に分かれる。

- `int`や`bool`のような組み込みの型は値型。変数の中に値そのものが入っている
- `class`で作った型は参照型。実体は1つで、変数はそれを指す名前になる
- 代入や引数渡しでコピーされるのは、値型なら値そのもの、参照型なら参照だけ

## なぜ必要か

10という数値はどこにあっても同じ10で、2つの10を区別する意味はない。一方「この商品」は実体が1つあり、複数の場所から同じものを指したい。値型は前者の概念を、参照型は後者の概念を表すために存在し、代入で何がコピーされるかはその違いの結果だ。

## 動かしてみる

まず値型の`int`で試す。

```csharp
var a = 10;
var b = a; // 値10そのものがbにコピーされる
b = 20;    // 以降bを書き換えてもaには影響しない
Console.WriteLine(a);
Console.WriteLine(b);
```

```
10
20
```

同じことを`class`で作った型でやると、結果が変わる。

```csharp
var p1 = new Point(1, 1);
var p2 = p1; // コピーされるのは参照だけ。実体は1つのまま
p2.X = 99;   // p1とp2は同じ実体を指すので、p1.Xも99になる
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

`new`で作った実体は、どの変数からも指されなくなると自動で回収される。自分で解放する必要はない。

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
