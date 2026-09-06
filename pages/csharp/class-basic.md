---
title: "クラス(class)の基本"
category: "文法"
order: 5
prerequisites: []
next:
  - file: reference-vs-value-types.md
    note: クラスの変数を代入したときに何が起きるかを、参照と値の違いから理解できる
  - file: tostring.md
    note: 作ったインスタンスの中身をわかりやすい文字列で表示する方法がわかる
  - file: immutable.md
    note: プロパティをgetだけにする以外の不変設計の手法を体系的に学べる
related:
  - class-struct-record-basics.md
  - access-modifiers-basic.md
---

`class`は、ひとつの概念に含まれるデータとメソッド（振る舞い）をまとめて1つの型として定義するキーワードだ。

- データの置き場所をプロパティ、初期化処理をコンストラクタとして持たせる
- 「商品」のように名前を付けられる概念を、1つの型として扱えるようにする

## なぜ必要か

商品名・価格のようなセットで扱うデータを別々の変数で持ち歩くと、メソッドに渡す順番を間違えるミスが起きやすい。`class`でひとまとめにすれば、渡すのは1つの値だけで済む。

ここでは基本として、外部に公開されるプロパティとコンストラクタのみを定義した`Product`クラスについて考える。

## 動かしてみる

```csharp
var product = new Product("コーヒー", 480);
Console.WriteLine($"{product.Name}: {product.Price}円");

public class Product
{
    public string Name { get; }
    public decimal Price { get; set; }

    public Product(string name, decimal price)
    {
        Name = name;
        Price = price;
    }
}
```

```
コーヒー: 480円
```

`public class Product`の中の`Name`と`Price`がプロパティ、`Product(string name, decimal price)`がコンストラクタだ。`new Product("コーヒー", 480)`と書いた時点でコンストラクタが呼ばれ、`Name`と`Price`に値が設定されたインスタンスができる。以後は`product.Name`のようにドットでつないで値を読み書きする。

コンストラクタを自分で書かなければ、C#は何もしない引数なしのコンストラクタを自動的に用意する。その場合は`new Product()`のように書け、プロパティは`{ get; set; }`にしてあとから代入することになる。

## 最低限の理解

- `class`を定義しただけでは何も作られない。`new`を書いて初めてインスタンスができ、同じクラスから作った複数のインスタンスはそれぞれ別の値を持つ
- 生成後に変えたくない値は、コンストラクタで受け取り`get`だけのプロパティにする。`Price`のようにあとから変わってよい値だけ`set`も付ける
- **コンストラクタを書くと、値を渡さない`new Product()`という呼び出し方はできなくなる**。初期化を強制したいときはこの形にする
- クラスの変数を別の変数に代入しても、値はコピーされない。同じインスタンスを2つの名前で指すだけになる。仕組みは「参照型と値型の違い」で詳しく扱う

## ⚠️ 変数を代入すると両方変わる

`b = a`と書いても`a`の中身がまるごと`b`にコピーされるわけではない。`a`と`b`は同じインスタンスを指すだけなので、片方のプロパティを書き換えると、もう片方から見える値も変わる。

```csharp
// NG: b への代入のつもりが a まで変わる
var a = new Product("コーヒー", 480);
var b = a;
b.Price = 600;
Console.WriteLine(a.Price);

public class Product
{
    public string Name { get; }
    public decimal Price { get; set; }

    public Product(string name, decimal price)
    {
        Name = name;
        Price = price;
    }
}
```

```
600
```

別のインスタンスが欲しいときは、代入ではなく`new`でもう1つ作る。

```csharp
// OK: 別のインスタンスが欲しいときは new でもう1つ作る
var a = new Product("コーヒー", 480);
var b = new Product(a.Name, 600);
Console.WriteLine(a.Price);

public class Product
{
    public string Name { get; }
    public decimal Price { get; set; }

    public Product(string name, decimal price)
    {
        Name = name;
        Price = price;
    }
}
```

```
480
```

## 課題

自分の業務で扱うデータを1つ選んで`class`にまとめ、コンストラクタで初期化を強制する形で定義してみる。次に、その変数を別の変数に代入してからプロパティを書き換え、元の変数の値がどう見えるか予想してから実行して確かめる。
