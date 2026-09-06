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

`Money`と`decimal`のように、独自の型が既存の型と同じ量を表していることがある。変換演算子は、その対応関係を型自身に定義するものだ。変換が常に安全なのか(`implicit`)、読み手に注意を促すべきなのか(`explicit`)を決めるのも、型を作る側の責任になる。

## 動かしてみる

```csharp
var price = new Money(1980m);
decimal amount = (decimal)price; // explicitなので、キャストを書いて初めて変換される
Console.WriteLine(amount);

public class Money
{
    public decimal Amount { get; }

    public Money(decimal amount)
    {
        Amount = amount;
    }

    public static explicit operator decimal(Money money) => money.Amount; // Moneyからdecimalへの変換方法
}
```

```
1980
```

逆方向の`decimal`から`Money`への変換も、戻り値の型を`Money`にして同じ形で定義できる。キャストを書き忘れるとコンパイルエラーになるので、変換が起きる場所はコード上で必ず見える。

## 最低限の理解

- `implicit`は失敗しない・情報を失わない変換にだけ使う。`int`から`decimal`のような安全な変換が典型
- 桁落ちや例外の可能性がある変換は`explicit`にして、呼び出す側にキャストを書かせて注意を促す
- 変換元・変換先どちらの型にも定義できるが、自分で定義できる型(独自クラス)の側に書くのが基本
- 双方向(型Aから型B、型Bから型Aの両方)に定義することもできるが、増やすほど読み手はどちらの変換が起きているか追いにくくなる

## ⚠️ implicitにすると意図しない箇所で型が変わる

```csharp
// NG: implicit にすると数値をそのまま渡すだけで Money に化けてしまう
PrintReceipt(1980); // new Moneyがどこにも無いのに、Moneyとして渡っている

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

動くには動くが、`PrintReceipt(1980)`だけを読んでも、`Money`を渡しているのか`decimal`を渡しているのかコード上から読み取れない。

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
