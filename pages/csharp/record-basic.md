---
title: "レコード(record)の基本"
category: "文法"
order: 42
prerequisites:
  - file: class-basic.md
    note: クラスの基本構文とインスタンス化の流れが分かれば十分です
  - file: equality.md
    note: 等価性の意味とEquals/GetHashCodeの関係が分かれば十分です
next:
  - file: class-struct-record-basics.md
    note: recordとclass/structの使い分けの判断基準が身につきます
  - file: immutable.md
    note: recordが体現する不変設計の考え方を深く理解できます
related:
  - tostring.md
---

## この記事でわかること

- `record`を使うと、クラスに自分で書いていた等価性判定や`ToString`が自動で手に入ることが分かる
- `with`式で「一部のプロパティだけ変えた新しいインスタンス」を作れるようになる
- 配列や`List<T>`をプロパティに持つ`record`で等価性が思ったように働かない理由が説明できる
- `record`を可変にしてしまう間違いを避けて書ける

## 一言でいうと

`record`は、中身の値が同じなら「同じもの」とみなしてくれる型を、少ないコードで定義するための書き方です。

スマホの連絡先を思い浮かべてください。名前と電話番号が完全に一致する連絡先データが2件あれば、保存された場所が違っても「同じ人の連絡先」だと感じますよね。プログラムの世界でも、`record`を使うと中身のプロパティがすべて一致するインスタンス同士を「等しい」と判定できます。ただし、実際の連絡先アプリと違って、`record`は自分で書いたプロパティの並びだけを比較の対象にします。

## どんなときに困るのか

クラスで`User`を定義し、名前と年齢が同じ2つのインスタンスを`==`で比べたのにFalseが返ってきた、という経験をした人は多いはずです。「オブジェクトの等価性を正しく実装する」で説明した通り、クラスの`==`は既定では参照が同じかどうかしか見ていません。値の一致で比較したいだけなのに`Equals`と`GetHashCode`を自分で実装するのは手間がかかり、書き忘れや実装ミスも起きがちです。

`record`を知っていれば、こうした値の比較が必要な型を1行の宣言だけで済ませられます。等価性・`ToString`・後述の`with`式が最初からセットで手に入るので、手動実装にありがちなミスも防げます。

## 動かしてみる

```csharp
// 名前と年齢を持つUserを作って表示する
var user = new User("Taro", 20);
Console.WriteLine(user);

public record User(string Name, int Age);
```

```
User { Name = Taro, Age = 20 }
```

`record User(string Name, int Age);`という1行だけで、`Name`と`Age`という2つのプロパティを持つ型ができています。`new User("Taro", 20)`でインスタンスを作ると、`Console.WriteLine`はそのまま`User { Name = Taro, Age = 20 }`という文字列を出力します。クラスであれば、既定の実装を自分の型用に書き換える処理(オーバーライド)をして`ToString`を用意しない限り型名しか出ませんが、`record`はプロパティの中身まで自動で文字列にしてくれます。

## 仕組み

`record`は、`Equals`・`GetHashCode`・`==`・`!=`・`ToString`・`with`式をまとめて自動生成させるためのキーワードです。上のコードのように括弧付きでプロパティを並べる書き方を位置引数構文と呼び、この構文で定義したプロパティは既定で`init`になります。`init`は`get`と組み合わせて使うキーワードで、インスタンスを生成するときにだけ値を設定でき、生成後は変更できません。

なお、`record`は既定で参照型（『クラス(class)の基本』で説明した、`new`で生成したインスタンスをヒープに置き、変数には参照だけが入る型）になります。これは`record class`と書いたのと同じ意味です。structとして定義したい場合は`record struct`という書き方もありますが、使い分けの判断は次の記事に譲ります。

この仕組みのおかげで、`record`同士を`==`で比べると全プロパティの値を比較してくれます。試しに、名前と年齢が同じ2つの`User`を作って比べてみましょう。

```csharp
// 中身が同じ2つのUserを==で比べる
var a = new User("Taro", 20);
var b = new User("Taro", 20);
Console.WriteLine(a == b);

public record User(string Name, int Age);
```

```
True
```

`a`と`b`は別々に`new`したインスタンスですが、`Name`と`Age`がすべて一致しているので`True`が返ります。クラスであれば、同じコードは`False`になるところです。

もう1つ、`record`だけが持つ`with`式も見ておきましょう。`with`式は、既存のインスタンスをもとに、指定したプロパティだけ書き換えた新しいインスタンスを作る構文です。

```csharp
// withで一部のプロパティだけ変えた新しいインスタンスを作る
var user = new User("Taro", 20);
var older = user with { Age = 21 };
Console.WriteLine(older);
Console.WriteLine(user == older);

public record User(string Name, int Age);
```

```
User { Name = Taro, Age = 21 }
False
```

`user with { Age = 21 }`は、`user`の`Name`はそのまま引き継ぎ、`Age`だけを21に変えた新しい`User`を作ります。元の`user`は変更されないので、2つを`==`で比べると`Age`が違うため`False`になります。

まとめると、`record`について覚えておくルールは次の通りです。

- `record`は既定で参照型になる。これは`record class`と書いたのと同じ意味
- 位置引数構文で定義したプロパティは既定で`init`になり、生成後は変更できない
- `Equals`・`GetHashCode`・`==`・`!=`・`ToString`はプロパティの値をもとに自動生成される
- `with`式は元のインスタンスを変えず、一部のプロパティだけ変えた新しいインスタンスを作る

## 実務での使い方

`record`がよく使われるのは、外部とやり取りするデータの受け渡し役として使う型です。こうした型はDTO(Data Transfer Objectの略で、処理を持たずデータを運ぶことだけを目的にした型)と呼ばれ、値が一致していれば同じものとして扱いたい場面がほとんどなので`record`と相性が良いです。

```csharp
// APIから受け取ったレスポンスをDTOとして表す
var response = new ProductResponse("マグカップ", 800m);
Console.WriteLine(response);

public record ProductResponse(string Name, decimal Price);
```

```
ProductResponse { Name = マグカップ, Price = 800 }
```

もう1つの使いどころは値オブジェクトです。値オブジェクトとは、金額や座標のように値そのものが意味を持ち、値が同じなら区別する必要がない型を指します。

```csharp
// 金額を値オブジェクトとして表す
var price = new Money(1000m, "JPY");
var samePrice = new Money(1000m, "JPY");
Console.WriteLine(price == samePrice);

public record Money(decimal Amount, string Currency);
```

```
True
```

型を選ぶときの判断基準は次の通りです。

- 値の一致で比較したいデータ(DTO・値オブジェクト)には`record`を使う
- 可変な状態や、状態に応じたふるまいを持つオブジェクトにはクラスを使う

## よくある間違い

### 配列やListプロパティも値で比較されると思い込む

`record`の等価性は便利ですが、配列は例外です。`record`が生成する`Equals`は配列プロパティに対しても配列自体の`Equals`を呼ぶだけなので、中身が同じでも別の配列インスタンスなら「等しくない」と判定されてしまいます。

```csharp
// NG: 配列プロパティは中身が同じでも参照が違うと等しくならない
var a = new Order("1", new[] { "apple" });
var b = new Order("1", new[] { "apple" });
Console.WriteLine(a == b);

public record Order(string Id, string[] Items);
```

```
False
```

値で比較したいなら、`Equals`と`GetHashCode`を自分で書き足して、リストの中身を1件ずつ比較するようにします。

```csharp
// OK: Equalsを上書きしてリストの中身まで比較する
var a = new Order("1", new List<string> { "apple" });
var b = new Order("1", new List<string> { "apple" });
Console.WriteLine(a == b);

public record Order(string Id, IReadOnlyList<string> Items)
{
    public virtual bool Equals(Order? other) =>
        other is not null && Id == other.Id && Items.SequenceEqual(other.Items);

    public override int GetHashCode() => Id.GetHashCode();
}
```

```
True
```

### recordを可変にしてしまう

`record`のプロパティに`set`を使うと、いつでも値を書き換えられてしまいます。これでは「値が同じなら同じもの」という`record`の前提が崩れ、あるコードが知らないうちにインスタンスの中身を書き換え、別の場所で比較や`ToString`の結果が変わってしまうという事故につながります。

```csharp
// NG: recordなのにsetでいつでも変更できてしまう
var user = new User { Name = "Taro" };
user.Name = "Jiro";
Console.WriteLine(user);

public record User
{
    public string Name { get; set; } = "";
}
```

```
User { Name = Jiro }
```

位置引数構文か`init`を使い、生成後は変更できないようにします。

```csharp
// OK: 位置引数構文でNameをinit専用にする
var user = new User("Taro");
Console.WriteLine(user);

public record User(string Name);
```

```
User { Name = Taro }
```

## 注意点

`record`が自動生成する`ToString`と`Equals`は、既定ではすべてのプロパティを対象にします。そのため、パスワードやトークンのようなプロパティを持つ`record`をそのままログに出力すると、機密情報がそのまま文字列として出てしまいます。こうした値を持つ`record`では、`ToString`を自分でオーバーライドして該当のプロパティを除外するようにしてください。

## 用語まとめ

| 用語 | 意味 |
|---|---|
| `record` | 値の一致で比較する等価性・`ToString`・`with`式を自動生成する型の定義方法 |
| 位置引数構文 | `record User(string Name, int Age)`のように括弧内にプロパティを並べる書き方 |
| `init` | インスタンス生成時にだけ値を設定でき、生成後は変更できないプロパティのアクセサ |
| `with`式 | 既存のインスタンスをもとに、指定したプロパティだけ変えた新しいインスタンスを作る構文 |
| DTO | 処理を持たず、データを運ぶことだけを目的にした型 |
| 値オブジェクト | 値そのものに意味があり、値が同じなら区別する必要がない型 |

## 理解度チェック

- [ ] `record`を使うとクラスと比べて何が自動生成されるか説明できる
- [ ] 位置引数構文で定義したプロパティが既定で`init`になることを説明できる
- [ ] `with`式で一部のプロパティだけ変えたインスタンスを作れる
- [ ] 配列や`List<T>`プロパティを持つ`record`で等価性が期待通り働かない理由を説明できる
- [ ] `record`を可変にしてしまうNG例のどこが問題か指摘できる
- [ ] DTOと値オブジェクトに`record`を使うと良い理由を説明できる
