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

`record`は、単純なデータ型を簡潔に定義するためのキーワードだ。主な目的として、次の性質を満たすようなデータ型を定義するために使われる。

- 値による等価性: プロパティの値がすべて一致すれば等しいと判定される
- 不変性: すべてのプロパティが書き換え不能である

## なぜ必要か

DTO(データの受け渡しだけを目的にした型)や値オブジェクトのように、中身が同じなら同じものとして扱いたい型は多い。クラスで同じ動作をさせるには`Equals`・`GetHashCode`・`ToString`を自分で書く必要があり、型が増えるたびに同じコードを繰り返すことになる。`record`はこの繰り返しを1行にまとめる。

## 動かしてみる

```csharp
var user = new User("Taro", 20);
var other = new User("Taro", 20);
Console.WriteLine(user);
Console.WriteLine(user == other);

public record User(string Name, int Age);
```

```
User { Name = Taro, Age = 20 }
True
```

`record User(string Name, int Age);`という1行で、`Name`と`Age`を持つ型ができる。`new`で別々に作った`user`と`other`は、プロパティの値が同じなので`==`はTrueになる。

同じ`User`をクラスで書くと、省略されていた中身が見える。

```csharp
var user = new UserClass("Taro", 20);
var other = new UserClass("Taro", 20);
Console.WriteLine(user);
Console.WriteLine(user.Equals(other));

public class UserClass
{
    public string Name { get; }
    public int Age { get; }

    public UserClass(string name, int age)
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

`record`が自動生成しているのは、この`Equals`・`GetHashCode`・`ToString`と、値を受け取って読み取り専用のプロパティに設定するコンストラクタだ。手書きのクラスは`Equals`を上書きしても`==`演算子は既定で参照比較のままなので、上のコードでは`Equals`メソッドを呼んで比較している。

## 最低限の理解

- `record`のインスタンスは、`new`で作った後にプロパティの値を書き換えられない。書き換えようとするとコンパイルエラーになる
- プロパティの値がすべて同じインスタンス同士は`==`でTrueになる。手書きのクラスでは既定でFalseになる
- `with`式を使うと、元のインスタンスは変えずに一部のプロパティだけ変えた新しいインスタンスを作れる

```csharp
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

## 現場での注意

C# 9より前(.NET Framework、C# 7/8など)のプロジェクトでは`record`は使えない。そうした現場に当たったときは、上の`UserClass`のような手書きクラスがそのまま本番の書き方になる。なお`record`は`struct`として定義する`record struct`という書き方もあるが、使い分けは次の記事に譲る。

## ⚠️ 配列やListをプロパティに持つと値等価が効かない

`record`の`Equals`は、配列プロパティに対しては配列自体の`Equals`を呼ぶだけになる。中身の要素が同じでも、別の配列インスタンスなら「等しくない」と判定される。

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

値で比較したいなら、`Equals`と`GetHashCode`を自分で書き足す。実装方法は「オブジェクトの等価性を正しく実装する」の通りで、リストの中身を1件ずつ比較する。

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

## 課題

自分の業務でよく扱うデータ(注文、商品など)を1つ選んで`record`で定義し、`new`で2つ作って`==`で比較してみる。次に、同じ動作をする手書きクラスに書き換えて、何行増えるか数えてみる。
