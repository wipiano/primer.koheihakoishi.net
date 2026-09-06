---
title: "class / struct / record の使い分け"
category: "文法"
order: 45
prerequisites:
  - file: struct-basic.md
    note: structが代入・引数渡しのたびにコピーされる値型であることがわかっていれば十分です
  - file: record-basic.md
    note: recordが値等価・ToString・with式を自動生成する機能であることがわかっていれば十分です
next:
  - file: immutable.md
    note: recordのwith式を軸にした不変設計の考え方を、より深く学べます
related:
  - reference-vs-value-types.md
  - equality.md
---

## この記事でわかること

- DTO・座標や金額・状態を持つオブジェクトのような実務でよくある場面で、`class` / `struct` / `record` のどれを選ぶか判断できる
- `record`が`class`とも`struct`とも独立した軸のキーワードであることを説明できる
- 型の選択を誤ったときに実務でどんな不具合が起きるかを説明できる

## 一言でいうと

`class`・`struct`・`record`はどれも自分専用のデータの入れ物を作るキーワードですが、「代入したときに何が起きるか」と「同じかどうかをどう判定するか」がそれぞれ違います。

名刺交換の場面を考えてみてください。会社の代表電話番号を教えるのと、名刺そのものを手渡すのとでは意味が違います。代表電話番号を教えた場合、後で番号が変わればみんなが影響を受けます。同じ番号を指しているからです。一方、名刺そのものを手渡した場合、相手がその名刺にメモを書き込んでも、あなたの手元の名刺は変わりません。別々の紙だからです。プログラムの世界では、`class`で作った型は代表電話番号のように同じ実体を指す参照型で、`struct`で作った型は名刺そのもののように渡すたびにコピーされる値型です。ただし、実際の名刺とは違って、structのコピーが有利になるのは中身がごく小さいときだけです。

そして`record`は、この`class`か`struct`のどちらかに「中身が完全に一致するかどうかを自動で判定してくれる機能」を追加するものです。たとえるなら、名刺の内容(会社名・氏名)が一字一句同じかどうかを、誰が見ても同じ基準でチェックしてくれるスタンプのようなものです。

## どんなときに困るのか

実務でありがちなのが、注文明細のようなDTOを`class`で定義してテストを書いたときに起きる話です。同じ内容の`Order`を2つ作って`==`で比較したのに、テストが失敗する。中身は同じはずなのになぜ一致しないのか分からず、先輩に「classは参照等価だから」と言われても、それがどういう意味か分からなければ直しようがありません。

3つの型のどれを選ぶかが分かっていないと、こうした遠回りを何度も繰り返すことになります。判断基準を先に知っていれば、最初から目的に合った型を選べて、テストも書きやすくなります。

## 動かしてみる

```csharp
var order1 = new Order { Quantity = 1 };
var order2 = order1;
order2.Quantity = 5;
Console.WriteLine(order1.Quantity); // classは実体を共有する

var p1 = new Point { X = 1, Y = 1 };
var p2 = p1;
p2.X = 100;
Console.WriteLine(p1.X); // structはコピーされる

var m1 = new Money(100);
var m2 = new Money(100);
Console.WriteLine(m1 == m2); // recordは値が同じなら等しい

public class Order
{
    public int Quantity { get; set; }
}

public struct Point
{
    public int X { get; set; }
    public int Y { get; set; }
}

public record Money(int Amount);
```

```
5
1
True
```

`order2`は`order1`と同じ実体を指しているので、`order2.Quantity`を書き換えると`order1.Quantity`も5になります。`p2`は`p1`の値をコピーして作られているので、`p2.X`を書き換えても`p1.X`は1のままです。`m1`と`m2`は別々のインスタンスですが、`Amount`が同じ100なので`record`の`==`は値の一致を見てtrueを返します。同じ「代入」という操作でも、型の種類によって結果がまったく違うことが分かります。

## 仕組み

class・struct・recordの3つは、「代入したときにインスタンスがどう扱われるか」と「等価性(2つのインスタンスが同じとみなせるかどうか)をどう判定するか」という2つの設定の組み合わせで整理できます。

`class`は参照型で、代入すると同じ実体を共有します。等価性は既定で参照等価、つまり同じインスタンスかどうかで判定されます。`struct`は値型で、『構造体(struct)の基本』で見た通り代入・引数渡しのたびにコピーが作られ、コピー先を変えても元のインスタンスには影響しません。

`record`はこの2つとは別の軸にあるキーワードで、`class`にも`struct`にも重ねて使える修飾子です。『レコード(record)の基本』で見た通り、`record`を付けると値等価・`ToString`・`with`式が自動生成されます。何も付けずに`record`とだけ書いた場合は`record class`と同じ意味になり、参照型のまま値等価が手に入ります。値型としてのコピー特性も欲しい場合は、`record struct`と明示します。

- `class`は参照型。代入は実体の共有、等価性は既定で参照等価
- `struct`は値型。代入はコピー、独自に演算子を定義しない限り`==`は使えない
- `record`は`class`か`struct`に値等価・`ToString`・`with`式を追加する修飾子で、単体では存在しない
- `record`だけを書くと`record class`と同じ意味で参照型、値型にしたいときは`record struct`と書く

## 実務での使い方

```csharp
var original = new ProductDto("Note", 120);
var updated = original with { Price = 150 };
Console.WriteLine(updated);

public record ProductDto(string Name, decimal Price);
```

```
ProductDto { Name = Note, Price = 150 }
```

APIから受け取った商品データのように、中身が一致していれば同じとみなしたいDTOには`record`が向いています。`with`式で一部だけ変えた新しいインスタンスをそのままログに出せるのも実務で便利な点です。

```csharp
var cart = new ShoppingCart();
var sameCart = cart;
cart.Add("ノート");
Console.WriteLine(sameCart.Items.Count);

public class ShoppingCart
{
    public List<string> Items { get; } = new();
    public void Add(string item) => Items.Add(item);
}
```

```
1
```

一方、買い物かごのように生成後も状態が変わり続け、複数箇所から同じ状態を参照したいオブジェクトには`class`が向いています。`sameCart`は`cart`と同じ実体を指しているので、片方に追加した内容がもう片方からも見えます。

- 状態を持ち、生成後にプロパティを変更しながら複数箇所で共有したいオブジェクトは`class`を選ぶ
- 座標や金額のように小さく不変な値で、コピーされても困らないものは`struct`を選ぶ
- 中身が一致していれば同じとみなしたいDTO・値オブジェクトは`record`(既定は`record class`)を選ぶ
- 座標や金額のように小さい値で、値等価も欲しい場合は`record struct`を選ぶ

## よくある間違い

### classの==が常にfalseになる

DTOを`class`で定義したまま値の一致を確認しようとして、テストが通らなくなる失敗です。`class`の`==`は既定で参照等価なので、プロパティの値が同じでもインスタンスが別なら一致しません。

```csharp
var a = new Money { Amount = 100 };
var b = new Money { Amount = 100 };
Console.WriteLine(a == b); // NG: classの既定は参照等価なので中身が同じでもfalse

public class Money
{
    public int Amount { get; init; }
}
```

```
False
```

```csharp
var a = new Money(100);
var b = new Money(100);
Console.WriteLine(a == b); // OK: recordなら値の一致で比較できる

public record Money(int Amount);
```

```
True
```

### 可変なstructをリストの中で直接書き換えようとする

`List<T>`の要素が`struct`のとき、`points[0].X = 100`のように書くと、要素は値のコピーとして取り出されるため、コピーへの変更として扱われずコンパイルエラーになります。structはコピーされる値型だという性質が、こういう形で表面化します。

```csharp
var points = new List<Point> { new Point { X = 1, Y = 1 } };
points[0].X = 100; // NG: List<T>の要素は値のコピーなので直接書き換えられない

public struct Point
{
    public int X { get; set; }
    public int Y { get; set; }
}
```

```
error CS1612: 変数ではないため、'List<Point>.this[int]' の戻り値を変更できません
```

```csharp
var points = new List<Point> { new Point { X = 1, Y = 1 } };
var updated = points[0];
updated.X = 100;
points[0] = updated; // OK: 新しい値を作って丸ごと置き換える
Console.WriteLine(points[0].X);

public struct Point
{
    public int X { get; set; }
    public int Y { get; set; }
}
```

```
100
```

### recordにすれば値型になると思い込む

`record`は値等価を自動生成しますが、既定では参照型のままです。値型のコピー特性まで欲しい場合は、`record struct`と明示しないと得られません。

```csharp
var m1 = new Money(100);
Console.WriteLine(m1.GetType().IsValueType); // NG: recordは既定でclassと同じ参照型

public record Money(int Amount);
```

```
False
```

```csharp
var m2 = new Coordinate(1, 1);
Console.WriteLine(m2.GetType().IsValueType); // OK: 値型にしたいならrecord structと明示する

public record struct Coordinate(int X, int Y);
```

```
True
```

## 注意点

structをインターフェース型やobject型の変数に代入すると、ヒープにコピーが作られるボックス化が起きます。座標や金額を`struct`にしても、インターフェース越しに頻繁に扱うコードでは、狙った性能改善が得られないことがあります。

## 用語まとめ

| 用語 | 意味 |
|---|---|
| DTO | データを運ぶためだけに使う、値の入れ物としてのオブジェクト |
| 値オブジェクト | 中身の値によって同一性を判断したいデータを表すオブジェクト |
| 参照等価 | 同じインスタンス(同じ実体)を指しているかどうかで等しいと判定すること |
| 値等価 | インスタンスの中身(プロパティの値)が一致しているかどうかで等しいと判定すること |
| ボックス化 | 値型のデータを参照型として扱うために、ヒープ上にコピーを作る処理 |

## 理解度チェック

- [ ] `class`と`struct`で、代入したときの挙動がどう違うか説明できる
- [ ] `record`が`class`単体や`struct`単体と何が違うかを説明できる
- [ ] DTOのように値の一致で比較したいデータに、なぜ`record`が向くか説明できる
- [ ] 可変なstructをコレクションに入れて直接書き換えようとするとどうなるか説明できる
- [ ] 座標や金額のような小さい値に`struct`を選ぶ理由を説明できる
- [ ] 状態を持ち複数箇所で共有したいオブジェクトに`class`を選ぶ理由を説明できる
- [ ] ボックス化がいつ起きるかを一言で説明できる
