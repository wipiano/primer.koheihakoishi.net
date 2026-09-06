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

- 値を変える操作は、既存のインスタンスを書き換えず、新しいインスタンスを返す
- `init`・`readonly`・`record`を使い、生成後の書き換えをコンパイルエラーにする
- いつ誰が読んでも同じ意味を持つので、複数の箇所から共有しても安全

## なぜ必要か

1000円という金額は、あとで900円に「変わる」ものではなく、900円は別の値だ。オブジェクトを不変にするとは、それを移り変わる状態ではなく、値として扱うと宣言することだ。値は生成した時点で意味が確定するので、どこで共有されようと、誰かに書き換えられた可能性を考えずに読める。

## 動かしてみる

```csharp
var price = new Money(1000m);
var discounted = price.Add(-100m); // priceは変わらず、新しいMoneyが返る

Console.WriteLine(price.Amount);
Console.WriteLine(discounted.Amount);

public record Money(decimal Amount)
{
    public Money Add(decimal amount) => this with { Amount = Amount + amount }; // thisは書き換えない
}
```

```
1000
900
```

`with`式(『レコード(record)の基本』で説明した、一部のプロパティだけ変えた新しいインスタンスを作る構文)を使うと、この形を1行で書ける。**値を変えたい操作は、新しいインスタンスを返す。**これがイミュータブル設計の基本パターンだ。

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
cart.Items.Add("banana"); // Itemsの差し替えは不可だが、中身への追加は通る

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
var updated = items.Add("banana"); // itemsは変わらず、要素が増えた別のリストが返る

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
