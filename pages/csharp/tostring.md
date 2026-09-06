---
title: "ToStringの使い方"
category: "文法"
order: 7
related:
  - class-basic.md
  - override.md
---

## 概要

- `ToString()`はオブジェクトを文字列表現に変換する、すべての型が持つメソッド
- ログ出力・デバッグ表示・文字列結合で暗黙的に呼ばれる
- 読了後、`ToString()`を安全かつ意図通りにオーバーライドできるようになる

## なぜ必要なのか

- オーバーライドしないと型名しか表示されず、ログやデバッグ時に中身が分からない
- オーバーライドすると、デバッグ効率が上がりログの可読性も向上する
- 文字列結合のたびに手動でプロパティを並べる代わりに、一元化した表現を使い回せる

## 仕組みと動作原理

- `object`が持つ`virtual string ToString()`を`override`で上書きする
- 文字列補間（`$"{obj}"`）やログ出力ライブラリは内部で`ToString()`を呼び出す
- `override`していないclassの`ToString()`は既定で型のフルネームを返す
- recordは`ToString()`が自動生成され、全プロパティを`Prop = Value`形式で出力する

## 基本的な書き方とコード例

- 最小の使い方

```csharp
public class Product
{
    public string Name { get; init; } = "";
    public decimal Price { get; init; }

    public override string ToString() => $"{Name} ({Price:C})";
}

var product = new Product { Name = "Coffee", Price = 480 };
Console.WriteLine(product); // 出力: Coffee (¥480)
```

- recordの自動生成ToStringとの比較

```csharp
public record ProductDto(string Name, decimal Price);

var dto = new ProductDto("Coffee", 480);
Console.WriteLine(dto);
// 出力: ProductDto { Name = Coffee, Price = 480 }
```

- 使い分けの判断基準
  - ログ・UI表示用に整形したい文字列がある場合は`class`で`ToString()`を独自実装する
  - デバッグ目的で全プロパティをそのまま見たいだけならrecordの自動生成に任せる

## よくある誤用・バグ

- **機密情報をToStringに含めてしまう**
  - ログ出力時に`ToString()`が呼ばれ、意図せずパスワードやトークンが記録される

```csharp
// NG: 自動生成・手動実装問わず機密情報を含めるとログに漏洩する
public record LoginRequest(string UserId, string Password);
logger.LogInformation("Login: {Request}", request); // Password がログに残る
```

```csharp
// OK: 機密情報は明示的に除外してToStringを実装する
public record LoginRequest(string UserId, string Password)
{
    public override string ToString() => $"LoginRequest {{ UserId = {UserId} }}";
}
```

- **ToStringに重い処理を書いてしまう**
  - デバッガのウォッチウィンドウやログ出力のたびに呼ばれるため、DB呼び出し等を書くと性能劣化する

```csharp
// NG: ToString内でI/Oを行うと呼び出すたびに重くなる
public override string ToString() => _repository.GetLatestStatus(Id); // 都度DBアクセス
```

```csharp
// OK: 既に保持している値だけを組み立てる
public override string ToString() => $"{Name} (Id={Id})";
```

## パフォーマンス・セキュリティ上の注意点

- `ToString()`はログ・デバッガから頻繁に呼ばれるため、I/Oや重い計算を含めない
- recordの自動生成ToStringは全プロパティを出力するため、機密情報を持つ型では明示的にオーバーライドして除外する
- 文字列結合が多い場合は`ToString()`より`StringBuilder`やソース生成の活用を検討する

## 理解度チェックリスト

- [ ] ToStringがいつ暗黙的に呼ばれるかを説明できる
- [ ] classでToStringをオーバーライドできる
- [ ] recordの自動生成ToStringの出力形式を説明できる
- [ ] ToStringに機密情報を含めてはいけない理由を説明できる
- [ ] ToStringに重い処理を書いてはいけない理由を説明できる
