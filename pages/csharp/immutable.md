---
title: "イミュータブル設計の基本"
category: "設計"
order: 48
prerequisites:
  - file: record-basic.md
    note: recordのwith式で新しいインスタンスを返す書き方が分かれば十分
  - file: readonly-struct.md
    note: readonly structが生成後の変更を禁止する仕組みが分かれば十分
next: []
related:
  - class-struct-record-basics.md
---

イミュータブル(不変)とは、一度生成したインスタンスの状態を、その後変更できないようにする設計方針だ。

## なぜ必要か

同じインスタンスを複数の箇所で共有していると、どこかで値を書き換えたときに、その影響が他の箇所にまで及ぶ。原因を1行ずつ追わないと、どこで値が変わったか分からなくなる。イミュータブルにしておけば生成後は値が変わらないと保証できるので、この追跡が要らなくなる。

## 動かしてみる

```csharp
var price = new Money(1000m);
var discounted = price.Add(-100m);

Console.WriteLine(price.Amount);
Console.WriteLine(discounted.Amount);

public record Money(decimal Amount)
{
    public Money Add(decimal amount) => this with { Amount = Amount + amount };
}
```

```
1000
900
```

`Add`は`this`を書き換えず、`with`式(『レコード(record)の基本』で説明した、一部のプロパティだけ変えた新しいインスタンスを作る構文)で`Amount`だけ変えた新しい`Money`を返す。だから`price`は`Add`を呼んだ後も`1000`のままで、`price`を持っている別のコードには影響しない。「値を変えたい操作は、新しいインスタンスを返す」という形が、イミュータブル設計の基本パターンだ。

## 最低限の理解

- `init`アクセサ、`readonly`フィールド、`record`のいずれかを使い、生成後の書き換えをコンパイルエラーにする
- 値を変える操作は、既存のインスタンスを書き換えず、上の`Add`のように新しいインスタンスを返す形にする
- コレクションを持たせるなら、プロパティの型を`IReadOnlyList<T>`か`ImmutableList<T>`にする。`List<T>`のままだと後述の落とし穴にはまる
- イミュータブルなオブジェクトは値が変わらないので、スレッド(同時に動く処理の流れ)をまたいで複数箇所から安全に共有できる
- 更新が多く状態も大きいデータを無理にイミュータブルにすると、変更のたびにインスタンスを作り直すコストが増える。まずは可変のままでよい

## ⚠️ プロパティがinitでも中のListは書き換えられる

`init`で宣言したプロパティ自体は差し替えられないが、プロパティの型が`List<T>`だと、そのリストの中身は外から`Add`や`Remove`で変更できてしまう。

```csharp
// NG: プロパティはinitだが、Listの中身は外から変更できる
var cart = new Cart(new List<string> { "apple" });
cart.Items.Add("banana");

Console.WriteLine(cart.Items.Count);

public record Cart(List<string> Items);
```

```
2
```

```csharp
// OK: Add・Removeを持たない型にして、うっかり追加を防ぐ
var cart = new Cart(new List<string> { "apple" });
// cart.Items.Add("banana"); // コンパイルエラーになる: CS1061

Console.WriteLine(cart.Items.Count);

public record Cart(IReadOnlyList<string> Items);
```

```
1
```

ただし`IReadOnlyList<string>`にしても、渡した元の`List<string>`を呼び出し元がまだ持っていれば、その元のリストを書き換えることで中身が変わってしまう。書き換える手段自体を無くしたいなら`ImmutableList<T>`を使う。

```csharp
// OK: ImmutableList<T>ならAddしても新しいインスタンスが返るだけ
using System.Collections.Immutable;

var items = ImmutableList.Create("apple");
var cart = new Cart(items);
var updated = items.Add("banana");

Console.WriteLine(cart.Items.Count);
Console.WriteLine(updated.Count);

public record Cart(ImmutableList<string> Items);
```

```
1
2
```

## 課題

自分の担当ドメインのデータ(注文、商品など)を1つ選び、値を変える操作のたびに新しいインスタンスを返すメソッドを持つ`record`として定義してみる。
