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

`ToString`は、インスタンスの中身を1つの文字列にして返すメソッドで、既定の動作を`override`で書き換えて使う。

## 動かしてみる

何も書き換えない状態でクラスのインスタンスを`Console.WriteLine`に渡すと、中身は分からない。

```csharp
var product = new Product { Name = "Coffee", Price = 480 };
Console.WriteLine(product);

public class Product
{
    public string Name { get; init; } = "";
    public decimal Price { get; init; }
}
```

```
Product
```

`Product`のプロパティは何も指定していないのに、既定の`ToString`は型の名前を返すだけになる。中身を見せたいなら`ToString`を書き換える。

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

`public override string ToString() => ...`という1行を追加しただけだ。`Console.WriteLine(product)`は内部で`product.ToString()`を呼び、その戻り値を表示している。`override`は、親から受け継いだメソッドを自分用に書き換える印で、仕組みは次の記事に譲る。`{Price:C}`は数値を通貨形式に整形する書き方だ。

## 最低限の理解

- 何も書き換えなければ、`ToString`は型の名前を返すだけ
- **`Console.WriteLine`だけでなく、文字列補間`$"{obj}"`や多くのログ出力ライブラリも、内部で暗黙に`ToString`を呼ぶ**
- 呼ばれるタイミングを完全にはコントロールできないので、I/Oやデータベース呼び出しのような重い処理は書かない
- 上書きする`ToString`は`public override string ToString()`という形で書く。戻り値は必ず`string`

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
