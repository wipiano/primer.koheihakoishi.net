---
title: "型変換演算子(implicit/explicit)の基本"
category: "文法"
order: 32
prerequisites:
  - file: operator-overloading.md
    note: operatorキーワードで演算子を定義する書き方が分かれば十分
next: []
related: []
---

`implicit`/`explicit`の付いた`operator`は、自分で定義した型を他の型に変換する方法をコンパイラに教えるキーワードだ。

- `implicit`を付けると、キャストなしで自動的に変換される
- `explicit`を付けると、`(型名)値`という明示的なキャストを書かないと変換されない

## なぜ必要か

`Money`のような独自の型を`decimal`に変換したい場面は多いが、既定では変換方法をコンパイラは知らない。変換演算子を定義すれば、キャストの書き方で型を行き来できるようになる。

## 動かしてみる

```csharp
var price = new Money(1980m);
decimal amount = (decimal)price;
Console.WriteLine(amount);

public class Money
{
    public decimal Amount { get; }

    public Money(decimal amount)
    {
        Amount = amount;
    }

    public static explicit operator decimal(Money money) => money.Amount;
}
```

```
1980
```

`static explicit operator decimal(Money money)`が、`Money`から`decimal`への変換方法を定義する。呼び出す側は`(decimal)price`とキャストを書いて初めて変換が実行される。`explicit`なので、キャストを書き忘れるとコンパイルエラーになり、意図しない箇所での変換を防げる。

## 最低限の理解

- `implicit`は失敗しない・情報を失わない変換にだけ使う。`int`から`decimal`のような安全な変換が典型
- 桁落ちや例外の可能性がある変換は`explicit`にして、呼び出す側にキャストを書かせて注意を促す
- 変換元・変換先どちらの型にも定義できるが、自分で定義できる型(独自クラス)の側に書くのが基本
- 双方向(型Aから型B、型Bから型Aの両方)に定義することもできるが、増やすほど読み手はどちらの変換が起きているか追いにくくなる

## ⚠️ implicitにすると意図しない箇所で型が変わる

```csharp
// NG: implicit にすると数値をそのまま渡すだけで Money に化けてしまう
PrintReceipt(1980);

void PrintReceipt(Money price) => Console.WriteLine(price.Amount);

public class Money
{
    public decimal Amount { get; }

    public Money(decimal amount)
    {
        Amount = amount;
    }

    public static implicit operator Money(decimal amount) => new Money(amount);
}
```

```
1980
```

動くには動くが、`PrintReceipt(1980)`だけを読んでも、`Money`を渡しているのか`decimal`を渡しているのかコード上から読み取れない。`new Money(...)`がどこにも書かれないまま値が`Money`として扱われてしまう。

```csharp
// OK: 変換演算子を持たせず、new で明示的に作らせる
PrintReceipt(new Money(1980));

void PrintReceipt(Money price) => Console.WriteLine(price.Amount);

public class Money
{
    public decimal Amount { get; }

    public Money(decimal amount)
    {
        Amount = amount;
    }
}
```

```
1980
```

## 課題

自分の業務で扱うデータを1つ選び、`decimal`や`string`のような既存の型との変換を`explicit`で定義してみる。`implicit`にすべきではない理由を1文で説明できるか確認する。
