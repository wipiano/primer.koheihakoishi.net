---
title: "レコード(record)の基本"
category: "文法"
order: 42
prerequisites:
  - file: class-basic.md
    note: クラスの基本構文とインスタンス化の流れが分かれば十分
  - file: equality.md
    note: Equals/GetHashCodeによる値の等価性判定の仕組みが分かれば十分
next:
  - file: class-struct-record-basics.md
    note: recordをclass/structとどう使い分けるかの判断基準につながる
  - file: immutable.md
    note: recordが体現する不変設計の考え方をさらに深く理解できる
related:
  - tostring.md
---

`record`は、中身がそのまま意味になるデータを「値」として定義するキーワードだ。

- 値による等価性: プロパティの値がすべて一致すれば等しいと判定される
- 不変性: 生成後にプロパティを書き換えられない
- 中身を表示する`ToString`と、一部だけ変えた複製を作る`with`式を自動で持つ

## なぜ必要か

名前と年齢の組のようなデータは、どのインスタンスかではなく中身だけが意味を持つ。中身が同じなら同じものであり、中身を変えるなら別の値だ。クラスでこの性質を表すには`Equals`・`GetHashCode`・`ToString`を毎回手書きすることになるが、`record`はこれを型の宣言そのもので表す。

## 動かしてみる

```csharp
var user = new User("Taro", 20);
var other = new User("Taro", 20);
Console.WriteLine(user); // 中身が読める形で表示される
Console.WriteLine(user == other); // 別々にnewしても、値が同じなら等しい

public record User(string Name, int Age); // この1行でNameとAgeを持つ型ができる
```

```
User { Name = Taro, Age = 20 }
True
```

同じ`User`をクラスで書くと、省略されていた中身が見える。

```csharp
var user = new UserClass("Taro", 20);
var other = new UserClass("Taro", 20);
Console.WriteLine(user);
Console.WriteLine(user.Equals(other)); // 手書きクラスの==は参照比較のままなのでEqualsで比べる

public class UserClass
{
    public string Name { get; }
    public int Age { get; }

    public UserClass(string name, int age) // 値を受け取り、読み取り専用のプロパティに設定する
    {
        Name = name;
        Age = age;
    }

    public override bool Equals(object? obj) =>
        obj is UserClass other && Name == other.Name && Age == other.Age;

    public override int GetHashCode() => HashCode.Combine(Name, Age);

    public override string ToString() => $"UserClass {{ Name = {Name}, Age = {Age} }}";
}
```

```
UserClass { Name = Taro, Age = 20 }
True
```

`record`が自動生成しているのは、このコンストラクタと`Equals`・`GetHashCode`・`ToString`だ。

## 最低限の理解

- `record`のインスタンスは、`new`で作った後にプロパティの値を書き換えられない。書き換えようとするとコンパイルエラーになる
- プロパティの値がすべて同じインスタンス同士は`==`でTrueになる。手書きのクラスでは既定でFalseになる
- 元の値は変えず、一部のプロパティだけ変えた新しいインスタンスが欲しいときは`with`式を使う

```csharp
var user = new User("Taro", 20);
var older = user with { Age = 21 }; // userはそのまま。Ageだけ違う新しいインスタンスができる
Console.WriteLine(older);
Console.WriteLine(user == older);

public record User(string Name, int Age);
```

```
User { Name = Taro, Age = 21 }
False
```

## 現場での注意

C# 9より前(.NET Framework、C# 7/8など)のプロジェクトでは`record`は使えない。そうした現場に当たったときは、上の`UserClass`のような手書きクラスがそのまま本番の書き方になる。なお`record`は`struct`として定義する`record struct`という書き方もあるが、使い分けは次の記事に譲る。

## ⚠️ 配列やListをプロパティに持つと値等価が効かない

`record`の`Equals`は、配列プロパティに対しては配列自体の`Equals`を呼ぶだけになる。中身の要素が同じでも、別の配列インスタンスなら「等しくない」と判定される。

```csharp
// NG: 配列プロパティは中身が同じでも参照が違うと等しくならない
var a = new Order("1", new[] { "apple" });
var b = new Order("1", new[] { "apple" });
Console.WriteLine(a == b); // Idは一致するが、Itemsは別の配列なのでFalse

public record Order(string Id, string[] Items);
```

```
False
```

値で比較したいなら、`Equals`と`GetHashCode`を自分で書き足す。実装方法は「オブジェクトの等価性を正しく実装する」の通りで、リストの中身を1件ずつ比較する。

```csharp
// OK: Equalsを上書きしてリストの中身まで比較する
var a = new Order("1", new List<string> { "apple" });
var b = new Order("1", new List<string> { "apple" });
Console.WriteLine(a == b);

public record Order(string Id, IReadOnlyList<string> Items)
{
    public virtual bool Equals(Order? other) =>
        other is not null && Id == other.Id && Items.SequenceEqual(other.Items); // 要素を1件ずつ比べる

    public override int GetHashCode() => Id.GetHashCode();
}
```

```
True
```

## 課題

自分の業務でよく扱うデータ(注文、商品など)を1つ選んで`record`で定義し、`new`で2つ作って`==`で比較してみる。次に、同じ動作をする手書きクラスに書き換えて、何行増えるか数えてみる。
