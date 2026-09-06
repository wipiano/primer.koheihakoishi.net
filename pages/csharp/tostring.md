---
title: "ToStringの使い方"
category: "文法"
order: 13
prerequisites:
  - file: class-basic.md
    note: クラスの定義とプロパティの書き方が分かれば十分
next:
  - file: override.md
    note: ToStringで使ったoverrideの仕組みそのものを深く理解できる
related: []
---

`ToString`は、インスタンスを人が読める1つの文字列で表すメソッドだ。すべての型が持っており、既定の動作を`override`で書き換えて使う。

- 「この型を文字で表すならこう」という表現を、型自身に持たせる
- `Console.WriteLine`や文字列補間は、渡されたものの`ToString`を暗黙に呼ぶ
- 何も書き換えなければ、型の名前を返すだけ

## なぜ必要か

インスタンスを画面やログに出すとき、どのプロパティをどう並べれば人に伝わるかを知っているのは、その型自身だ。表示する側が毎回プロパティを取り出して組み立てると、同じ整形コードが型の外に散らばる。`ToString`に表現を持たせれば、表示側は中身を知らずに「文字にしてくれ」と頼むだけで済む。

## 動かしてみる

```csharp
var product = new Product { Name = "Coffee", Price = 480 };
Console.WriteLine(product); // 内部で product.ToString() が呼ばれる

public class Product
{
    public string Name { get; init; } = "";
    public decimal Price { get; init; }
}
```

```
Product
```

既定の`ToString`は型の名前を返すだけなので、中身は分からない。`override`(親から受け継いだメソッドを自分用に書き換える印。仕組みは『virtual/overrideの基本』で扱う)で表現を書き換える。

```csharp
var product = new Product { Name = "Coffee", Price = 480 };
Console.WriteLine(product);

public class Product
{
    public string Name { get; init; } = "";
    public decimal Price { get; init; }

    public override string ToString() => $"{Name} {Price}円"; // この1行で表現を決める
}
```

```
Coffee 480円
```

## 最低限の理解

- 上書きする`ToString`は`public override string ToString()`という形で書く。戻り値は必ず`string`
- **`Console.WriteLine`だけでなく、文字列補間`$"{obj}"`や多くのログ出力ライブラリも、内部で暗黙に`ToString`を呼ぶ**
- 呼ばれるタイミングを完全にはコントロールできないので、I/Oやデータベース呼び出しのような重い処理は書かない
- 表示用の文字列を型の外で組み立てたくなったら、その整形は`ToString`に置けないか考える

## ⚠️ 機密情報を含めるとログに漏れる

`ToString`に全プロパティを並べたくなるが、パスワードやトークンまで含めると、ログに出力するたびにその値が記録されてしまう。

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
// OK: 表示してよい情報だけ含める
var request = new LoginRequest { UserId = "yamada", Password = "p@ssw0rd" };
Console.WriteLine($"ログイン試行: {request}");

public class LoginRequest
{
    public string UserId { get; init; } = "";
    public string Password { get; init; } = "";

    public override string ToString() => $"UserId={UserId}";
}
```

```
ログイン試行: UserId=yamada
```

## 課題

自分の業務でよく扱うクラス(注文や商品など)を1つ選んで`ToString`をオーバーライドし、`Console.WriteLine`で中身が読めることを確認する。次に、その中にログへ出してはいけないプロパティが無いか見直す。
