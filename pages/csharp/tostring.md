---
title: "ToStringの使い方"
category: "文法"
order: 13
prerequisites:
  - file: class-basic.md
    note: クラスの定義方法とプロパティの書き方が分かっていれば十分です
next:
  - file: override.md
    note: ToStringで使ったoverrideの仕組みを、virtualとセットできちんと理解できます
related: []
---

## この記事でわかること

- 自分で作ったクラスのインスタンスを`Console.WriteLine`やログにそのまま渡したとき、中身が分かる文字列を表示できるようになる
- `ToString`がプログラムのどんな場面で自動的に呼ばれているかを説明できる
- `ToString`を書き換えるときにやってはいけないことが分かる

## 一言でいうと

`ToString`は、インスタンスの中身を人間が読める1行の文字列にして見せてくれる仕組みです。

宅配便の荷物には送り状が貼ってあり、箱を開けなくても品名や届け先がひと目で分かります。プログラムの世界では、`ToString`はこの送り状のようなもので、インスタンス（『クラス(class)の基本』で説明した、`new`で作った実体）の中身を開けなくても文字列で確認できます。ただし送り状と違って、`ToString`が何を表示するかは自分で自由に決められます。何もしなければ、白紙の送り状のように役に立たない表示になります。

## どんなときに困るのか

`Product`クラスを自分で作り、値段が正しく計算されているか確認したくて`Console.WriteLine(product)`と書いたとします。期待していたのは`Coffee (¥480)`のような中身の表示ですが、実際に出てくるのは`Product`という型の名前だけです。中身は何も分かりません。仕方なく`Console.WriteLine(product.Name)`のようにプロパティを1つずつ書き出すことになり、プロパティが増えるたびに表示用のコードを書き足す羽目になります。

`ToString`を正しく書き換えておけば、`Console.WriteLine(product)`と書くだけでいつでも中身が確認できます。デバッグのたびにプロパティを並べ直す手間がなくなります。

## 動かしてみる

```csharp
var product = new Product { Name = "Coffee", Price = 480 };
Console.WriteLine(product);

public class Product
{
    public string Name { get; init; } = "";
    public decimal Price { get; init; }

    public override string ToString() => $"{Name} ({Price:C})";
}
```

```
Coffee (¥480)
```

`Product`クラスの中に`public override string ToString() => ...`という1行を追加しただけです。`Console.WriteLine(product)`は`product`をそのまま渡していますが、内部で`product.ToString()`が呼ばれ、その戻り値の文字列が画面に表示されています。`{Price:C}`は数値を通貨の形式（¥480のような表示）に整形する書き方です。

## 仕組み

すべてのクラスは、何もしなくても`ToString`という名前のメソッドを最初から持っています。C#のすべての型が土台にしている共通の型に、あらかじめ用意されているメソッドだからです。ただし何も書き換えなければ、その中身は「このインスタンスの型の名前」を返すだけです。さきほど`Console.WriteLine(product)`で型名しか表示されなかったのは、このためです。

オーバーライドとは、あらかじめ用意されている処理を、自分の実装で上書きすることです。C#では`override`というキーワードを使って行います。`ToString`のように上書きしてよいと決められているメソッドにだけ使えます。なぜ上書きが許されているのか、その仕組み全体は次の記事「virtual/overrideの基本」で説明します。ここでは「`public override string ToString()`と書けば、型名の代わりに自分で組み立てた文字列を返せる」とだけ覚えておけば十分です。

`Console.WriteLine`だけでなく、文字列補間（`$"{product}"`のように変数を`{}`で埋め込む書き方）や文字列同士の`+`結合でも、インスタンスを文字列に変換する場面では必ず`ToString`が呼ばれます。ログ出力ライブラリの多くも、メッセージに埋め込んだインスタンスを表示するときに内部で同じことをしています。

- `ToString`をオーバーライドしないと、型の名前しか表示されない
- オーバーライドする`ToString`は`public override string ToString()`という形で書く（戻り値は必ず`string`）
- `Console.WriteLine`・文字列補間・文字列結合はすべて内部で`ToString`を呼んでいる

## 実務での使い方

一番よく使うのは、ログやデバッグ表示のために「このインスタンスを見れば状況が分かる」文字列を返す書き方です。

```csharp
var order = new Order { Id = 1001, CustomerName = "Yamada", Total = 3200 };
Console.WriteLine($"注文を受け付けました: {order}");

public class Order
{
    public int Id { get; init; }
    public string CustomerName { get; init; } = "";
    public decimal Total { get; init; }

    public override string ToString() => $"Order #{Id} ({CustomerName}, {Total:C})";
}
```

```
注文を受け付けました: Order #1001 (Yamada, ¥3,200)
```

`$"注文を受け付けました: {order}"`のように、文字列補間の`{}`の中にインスタンスを直接書くだけで`ToString`の結果が埋め込まれます。プロパティを1つずつ並べる必要はありません。

- 画面やログに人間が読みやすい形で表示したい場合は、自分で`ToString`をオーバーライドする
- `record`という、クラスに似た型の定義方法を使っている場合は`ToString`が自動で用意されるので、機密情報を含まない型ならそのまま使ってよい

## よくある間違い

### 機密情報までToStringに含めてしまう

`ToString`にすべてのプロパティを並べたくなりますが、パスワードやトークンのような値まで含めると、ログに出力するたびにその値が記録されてしまいます。書いた本人が気づかないまま、ログファイルに機密情報が残り続けることがあります。

```csharp
// NG: パスワードまでそのまま含めてしまう
var request = new LoginRequest { UserId = "yamada", Password = "p@ssw0rd" };
Console.WriteLine($"ログイン試行: {request}");

public class LoginRequest
{
    public string UserId { get; init; } = "";
    public string Password { get; init; } = "";

    public override string ToString() => $"UserId={UserId}, Password={Password}";
}
```

```
ログイン試行: UserId=yamada, Password=p@ssw0rd
```

```csharp
// OK: 表示してよい情報だけを含める
var request = new LoginRequest { UserId = "yamada", Password = "p@ssw0rd" };
Console.WriteLine($"ログイン試行: {request}");

public class LoginRequest
{
    public string UserId { get; init; } = "";
    public string Password { get; init; } = "";

    public override string ToString() => $"UserId={UserId}";
}
```

### ToStringに重い処理を書いてしまう

`ToString`はデバッガの画面表示やログ出力のたびに、こちらが意識していないタイミングで何度も呼ばれます。この中でデータベースへの問い合わせのような重い処理を書くと、画面を見るたびやログを1行出すたびに、その重い処理が実行されてしまいます。

```csharp
// NG: 呼び出すたびに重い処理が走ってしまう
var status = new OrderStatus { Id = 1001 };
Console.WriteLine(status);
Console.WriteLine(status);

public class OrderStatus
{
    public int Id { get; init; }

    public override string ToString() => Database.GetLatestStatus(Id);
}

public static class Database
{
    public static string GetLatestStatus(int id)
    {
        Console.WriteLine("(重い処理を実行中...)");
        return $"Status-{id}";
    }
}
```

```
(重い処理を実行中...)
Status-1001
(重い処理を実行中...)
Status-1001
```

```csharp
// OK: すでに持っている値だけを組み立てる
var status = new OrderStatus { Id = 1001, Status = "Shipped" };
Console.WriteLine(status);

public class OrderStatus
{
    public int Id { get; init; }
    public string Status { get; init; } = "";

    public override string ToString() => $"Id={Id}, Status={Status}";
}
```

## 注意点

`ToString`は自分がコード中で明示的に呼び出していなくても、`Console.WriteLine`・文字列補間・ログ出力ライブラリなどから、気づかないタイミングで呼ばれます。呼ばれるタイミングを完全にはコントロールできないため、「この`ToString`の結果がどこかのログに残っても問題ないか」を実装する時点で必ず確認してください。とくにパスワード・トークン・個人情報を持つクラスでは、`ToString`に何を含めるかを一度書いて終わりにせず、プロパティを追加するたびに見直す習慣をつけると安全です。

## 用語まとめ

| 用語 | 意味 |
|---|---|
| ToString | インスタンスを文字列に変換して返す、すべての型が持つメソッド |
| オーバーライド | あらかじめ用意されている処理を、自分の実装で上書きすること。C#では`override`キーワードで行う |
| 文字列補間 | `$"{変数名}"`のように、文字列の中に`{}`で値を埋め込む書き方 |
| record | クラスに似た型の定義方法の一つ。使うと`ToString`が自動で用意される |

## 理解度チェック

- [ ] `override`せずに`Console.WriteLine(instance)`を実行すると何が表示されるか説明できる
- [ ] `ToString`が暗黙のうちに呼ばれる場面を2つ以上挙げられる
- [ ] 自分で定義したクラスに`ToString`をオーバーライドできる
- [ ] `ToString`に機密情報を含めてはいけない理由を説明できる
- [ ] `ToString`に重い処理を書いてはいけない理由を説明できる
