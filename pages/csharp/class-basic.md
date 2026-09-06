---
title: "クラス(class)の基本"
category: "文法"
order: 5
related:
  - struct-basic.md
  - override.md
  - tostring.md
  - interface-basic.md
  - class-struct-record-basics.md
---

## 概要

- クラスはデータとふるまい（フィールド・プロパティ・メソッド）をひとまとめにした参照型の設計図
- オブジェクト指向設計の基本単位で、業務ロジックの大半はクラスとして書かれる
- 読了後、クラスの基本構文とインスタンス化の流れを理解できる

## なぜ必要なのか

- 関連するデータと処理をまとめないと、引数だらけの関数が散乱し保守性が下がる
- クラスを使うと状態と操作を1つの型に閉じ込め、名前空間的にも整理できる
- 静的メソッドの寄せ集め（プロシージャ指向）と比べ、状態を持つオブジェクトを自然に表現できる

## 仕組みと動作原理

- classは参照型。`new`で生成したインスタンスはヒープに置かれ、変数には参照（アドレス）が入る
- 同じインスタンスを複数の変数で共有でき、一方を変更すると他方からも変更が見える
- コンストラクタはインスタンス生成時の初期化処理。書かなければ既定の引数なしコンストラクタが暗黙に生成される
- プロパティ（`{ get; set; }`）はフィールドへのアクセスをカプセル化する構文

## 基本的な書き方とコード例

- 最小の使い方

```csharp
public class Product
{
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}

var product = new Product { Name = "Coffee", Price = 480 };
Console.WriteLine(product.Name);
```

- コンストラクタで初期化を強制する

```csharp
public class Product
{
    public Product(string name, decimal price)
    {
        Name = name;
        Price = price;
    }

    public string Name { get; }
    public decimal Price { get; }
}

var product = new Product("Coffee", 480);
```

- 使い分けの判断基準
  - 初期化必須の値がある場合はコンストラクタで受け取り、`get`のみのプロパティにする
  - 生成後に値を変える必要がある場合のみ`set`を付ける

## よくある誤用・バグ

- **参照の共有を意識しない代入**
  - クラス変数の代入はインスタンスのコピーではなく参照のコピー

```csharp
// NG: b を変更したつもりが a も変わる（同じインスタンスを指しているため）
var a = new Product("Coffee", 480);
var b = a;
b.Price = 600;
Console.WriteLine(a.Price); // 600
```

```csharp
// OK: 別インスタンスが必要ならコンストラクタで新規作成する
var a = new Product("Coffee", 480);
var b = new Product(a.Name, a.Price);
b.Price = 600;
Console.WriteLine(a.Price); // 480
```

- **nullチェック漏れ**
  - 参照型は`null`を代入できるため、未初期化のまま使うとNullReferenceExceptionになる

```csharp
// NG: Nullable参照型を無視してnullのまま使う
Product? product = FindProduct("unknown");
Console.WriteLine(product.Name); // 警告を無視するとNullReferenceException
```

```csharp
// OK: null許容を明示し、使用前にチェックする
Product? product = FindProduct("unknown");
if (product is not null)
{
    Console.WriteLine(product.Name);
}
```

## 理解度チェックリスト

- [ ] クラスが参照型であることを説明できる
- [ ] インスタンスの代入が参照のコピーであることを説明できる
- [ ] コンストラクタとプロパティを使った基本的なクラスを書ける
- [ ] getのみのプロパティとget/setプロパティの使い分けができる
- [ ] 参照共有によるバグの原因を説明できる
