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

プロパティをすべて`public`にすると、外部のコードがどこからでも自由に書き換えられ、想定していない値が入り込むのを止められない。`private`で触れる範囲を狭めれば、値を変えてよい経路をクラス自身に決めさせられる。

## 動かしてみる

```csharp
var product = new Product("コーヒー", 480);
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

`Price`の`set`アクセサに`private`が付いているので、値を変えられるのはクラスの内部だけになる。外から値を変えたいときは`ApplyDiscount`のようなメソッドを必ず経由することになり、そのメソッドの中でだけ「割引後の金額がマイナスにならない」というチェックを効かせられる。

## 最低限の理解

- 修飾子を何も書かないクラスのメンバーは、既定で`private`になる
- `{ get; }`だけにすると生成後は一切変更できない。`{ get; private set; }`にすると、読み取りは外部から自由だが書き込みはクラス内部のメソッド経由に絞れる
- フィールドは基本`private`にし、外部に見せたい値だけプロパティとして`public`で公開する
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
