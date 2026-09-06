---
title: "アクセス修飾子(public/private)の基本"
category: "文法"
order: 6
prerequisites:
  - file: class-basic.md
    note: クラスのプロパティ・コンストラクタの書き方が分かれば十分
next:
  - file: immutable.md
    note: private setで書き換えを制限する発想を、record中心の完全な不変設計へ発展させる
related: []
---

`public`と`private`は、クラスのメンバー(プロパティ・メソッドなど)を、外部からどこまで使えるようにするかを決めるキーワードだ。

- `public`を付けたメンバーは、クラスの外から自由に読み書き・呼び出しができる
- `private`を付けたメンバーは、そのクラスの内部からしか使えない

## なぜ必要か

「価格はマイナスにならない」のようなルールを守る責任は、その値を持つオブジェクト自身にある。ところがプロパティがすべて`public`だと、どこからでも書き換えられ、オブジェクトは自分の状態に責任を持てない。`private`で触れる範囲を型自身が決めることで、値を変えてよい経路をオブジェクトの内側に集められる。

## 動かしてみる

```csharp
var product = new Product("コーヒー", 480);
product.ApplyDiscount(100); // 値を変える経路はこのメソッドだけ
Console.WriteLine(product.Price);

public class Product
{
    public string Name { get; }
    public decimal Price { get; private set; } // 読み取りは外から、書き込みはクラス内部だけ

    public Product(string name, decimal price)
    {
        Name = name;
        Price = price;
    }

    public void ApplyDiscount(decimal amount)
    {
        if (amount > Price) return; // 経路がここだけなので、このチェックを素通りできない
        Price -= amount;
    }
}
```

```
380
```

## 最低限の理解

- 修飾子を何も書かないクラスのメンバーは、既定で`private`になる
- `{ get; }`だけにすると生成後は一切変更できない。`{ get; private set; }`にすると、読み取りは外部から自由だが書き込みはクラス内部のメソッド経由に絞れる
- フィールドは基本`private`にし、外部に見せたい値だけプロパティとして`public`で公開する
- 迷ったら`private`から始める。後から`public`に広げるのは簡単だが、狭めるのは使っている箇所すべてに影響する
- **`private`にしたメンバーは、そのクラスの中でしか名前で呼べない**。外部からは存在しないものとして扱われる

## ⚠️ setがpublicのままだと検証をすり抜ける

```csharp
// NG: Priceがpublic setのままだと、外から直接代入して検証をすり抜けられる
var product = new Product("コーヒー", 480);
product.Price = -1000;
Console.WriteLine(product.Price);

public class Product
{
    public string Name { get; }
    public decimal Price { get; set; }

    public Product(string name, decimal price)
    {
        Name = name;
        Price = price;
    }

    public void ApplyDiscount(decimal amount)
    {
        if (amount > Price) return;
        Price -= amount;
    }
}
```

```
-1000
```

`ApplyDiscount`の中でどれだけ検証を書いても、`Price`が`public set`のままなら外から直接代入してその検証を素通りできる。

```csharp
// OK: setをprivateにして、外からの直接代入をコンパイルエラーにする
var product = new Product("コーヒー", 480);
// product.Price = -1000; // コンパイルエラーになる: CS0272
product.ApplyDiscount(100);
Console.WriteLine(product.Price);

public class Product
{
    public string Name { get; }
    public decimal Price { get; private set; }

    public Product(string name, decimal price)
    {
        Name = name;
        Price = price;
    }

    public void ApplyDiscount(decimal amount)
    {
        if (amount > Price) return;
        Price -= amount;
    }
}
```

```
380
```

## 課題

自分の業務で扱うデータを1つ選び、外から自由に書き換えられると困る値を`private set`にして、専用のメソッドを経由しないと変更できない形にしてみる。
