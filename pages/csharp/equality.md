---
title: "オブジェクトの等価性を正しく実装する"
category: "文法"
order: 27
prerequisites:
  - file: override.md
    note: virtual/overrideでメソッドを上書きする書き方が分かれば十分
  - file: interface-basic.md
    note: インターフェースを実装する書き方が分かれば十分
next:
  - file: record-basic.md
    note: この記事で書いた実装をrecordが自動で生成してくれる仕組みが分かる
related:
  - class-struct-record-basics.md
---

## この記事でわかること

- 値が同じインスタンスを「同じ」と判定できる型を自分で作れる
- EqualsをオーバーライドするときにGetHashCodeも直さなければならない理由を説明できる
- `IEquatable<T>`を実装した型を書ける
- DictionaryやHashSetで「同じ値のはずなのに見つからない」バグを避けられる

## 一言でいうと

等価性とは、2つの物が「同じ」と言えるかどうかを判断するためのルールのことです。

たとえば、双子の姉妹がまったく同じ服を着て並んでいたとします。見た目はそっくりでも、「この人とその人は同一人物か」と聞かれれば、双子は別々の人間なので違うと答えます。一方で「服や髪型などの特徴がすべて一致しているか」と聞かれれば、一致していると答えられます。プログラムの世界では、前者を「参照等価」、後者を「値等価」と呼びます。ただし、実際の双子と違って、プログラムでは自分の作った型について、どちらの意味で「同じ」と判定するかを自分で決めて実装できます。

## どんなときに困るのか

座標を表す`Point`という型を自分で作り、複数の地点を管理するのに`HashSet<Point>`を使ったとします。同じ座標`(1, 2)`を表すインスタンスをもう一つ作って`Contains`で調べても、結果はfalseになります。値は完全に同じなのに、別物として扱われてしまうのです。テストコードで「同じ値のはずなのに一致しない」と表示され、原因を追ってようやく`Equals`をオーバーライドしていなかったことに気づく、というのはよくある話です。

Equals・GetHashCode・`IEquatable<T>`を正しく実装しておけば、値が同じインスタンスをDictionaryやHashSetで正しく検索でき、`==`による比較も期待通りに動くようになります。

## 動かしてみる

```csharp
var p1 = new Point(1, 2);
var p2 = new Point(1, 2);

Console.WriteLine(p1.Equals(p2));
Console.WriteLine(p1 == p2);

public class Point
{
    public int X { get; }
    public int Y { get; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }
}
```

```
False
False
```

p1とp2は、XとYの値がどちらも1と2で完全に一致しています。それでも`Equals`はFalseを返します。これは、`class`が既定で持っている`Equals`が、値の中身ではなく「同じインスタンス(同じメモリ上の場所)を指しているか」を調べる仕組みだからです。p1とp2は別々に`new`したインスタンスなので、中身が同じでも、既定の`Equals`からは別物として扱われます。`==`演算子も既定では同じ判定をするため、こちらもFalseになります。

## 仕組み

`Equals`は、2つのインスタンスが等しいかどうかを判定するメソッドです。すべての型はobjectから引き継いだ`Equals`を持っていて、その既定の中身は「参照等価」、つまり同じインスタンスかどうかの判定でした。値が同じかどうかで比較したい場合は、先ほどのコードのように、`Equals`を自分でオーバーライド（『virtual/overrideの基本』で説明した、基底クラスのメソッドを派生クラスで上書きする仕組み）する必要があります。

もう一つ欠かせないのが`GetHashCode`です。`GetHashCode`は、インスタンスを表す1つの数値(ハッシュコード)を返すメソッドです。図書館で本を探す場面を想像してみてください。何百冊もある本棚を端から端まで探すのは大変なので、図書館はジャンルごとに棚番号を割り振っておきます。`GetHashCode`が返す数値はこの「棚番号」のようなもので、DictionaryやHashSetは、まずこの番号で棚を絞り込んでから探すことで高速に検索しています。

ここに、この記事でいちばん大事なルールがあります。**Equalsがtrueを返す2つのインスタンスは、必ず同じGetHashCodeを返さなければなりません。** これを等価性の契約と呼びます。もし`Equals`は「同じ」と判定するのに`GetHashCode`が違う棚番号を返してしまうと、DictionaryやHashSetは探したい要素と違う棚を調べてしまい、実際にはコレクションの中にあるのに「見つからない」という結果になります。`Equals`をオーバーライドしたら、必ず`GetHashCode`もセットで見直す必要があるのはこのためです。

`IEquatable<T>`は、型安全な比較用のメソッドを約束するインターフェース（『インターフェースの基本』で説明した、実装すべきメソッドを型で約束する仕組み）です。`IEquatable<T>`を実装すると、`Equals(T)`という、objectへのキャストが要らない比較メソッドを持てます。

覚えておくべきルールは次の通りです。

- `Equals`をオーバーライドしたら、`GetHashCode`も必ずセットでオーバーライドする
- `GetHashCode`の計算には、`Equals`で比較しているのと同じプロパティを使う
- `==`演算子は既定では`Equals`と連動しない。値の比較に使いたいなら演算子オーバーロードも別に必要

## 実務での使い方

実務でよく使うのは、`IEquatable<T>`を実装したうえで、`Equals(object)`と`GetHashCode`もセットでオーバーライドする書き方です。

```csharp
var points = new HashSet<Point> { new Point(1, 2) };
Console.WriteLine(points.Contains(new Point(1, 2)));

public class Point : IEquatable<Point>
{
    public int X { get; }
    public int Y { get; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public bool Equals(Point? other) => other is not null && X == other.X && Y == other.Y;
    public override bool Equals(object? obj) => Equals(obj as Point);
    public override int GetHashCode() => HashCode.Combine(X, Y);
}
```

```
True
```

`GetHashCode`の実装には`HashCode.Combine`という標準の道具を使っています。これは、複数の値を渡すと、それらをうまく組み合わせた1つのハッシュコードを計算してくれるメソッドです。棚番号の計算式を自分で考える必要はありません。

判断が必要な場面は次の通りです。

- DictionaryのキーやHashSetの要素として使う型で、値が同じなら同じとみなしたい場合は、Equals・GetHashCode・`IEquatable<T>`をセットで実装する
- 値の一致だけで比較したいシンプルなデータ型なら、`record`を使うという選択肢もある。`record`を使うと、ここまで説明してきたEquals・GetHashCode・`IEquatable<T>`の実装が自動で手に入る

## よくある間違い

### GetHashCodeを直さずEqualsだけオーバーライドしてしまう

`Equals`だけをオーバーライドして満足してしまうのは、初心者がよくやってしまう間違いです。`Equals`の比較では同じと判定されるのに、`GetHashCode`は既定の（参照ベースの）ままなので、HashSetの中では違う棚に置かれてしまいます。

```csharp
// NG: Equalsだけオーバーライドし、GetHashCodeは既定のまま
var a = new Point(1, 2);
var b = new Point(1, 2);
Console.WriteLine(a.Equals(b));

var set = new HashSet<Point> { a };
Console.WriteLine(set.Contains(b));

public class Point
{
    public int X { get; }
    public int Y { get; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override bool Equals(object? obj) =>
        obj is Point other && X == other.X && Y == other.Y;
}
```

```
warning CS0659: 'Point' は Object.Equals(object o) をオーバーライドしますが、Object.GetHashCode() をオーバーライドしません。
True
False
```

`a.Equals(b)`はTrueなのに、同じ値を持つ`b`で`Contains`を調べるとFalseになります。これがまさに等価性の契約違反です。コンパイラも警告でこの状態を教えてくれています。

```csharp
// OK: Equalsと同じ判定基準でGetHashCodeもセットで実装する
var a = new Point(1, 2);
var b = new Point(1, 2);

var set = new HashSet<Point> { a };
Console.WriteLine(set.Contains(b));

public class Point
{
    public int X { get; }
    public int Y { get; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override bool Equals(object? obj) =>
        obj is Point other && X == other.X && Y == other.Y;

    public override int GetHashCode() => HashCode.Combine(X, Y);
}
```

```
True
```

### ハッシュコードの計算に使うプロパティを登録後に変更してしまう

HashSetやDictionaryに要素を追加した後で、`GetHashCode`の計算に使っているプロパティを書き換えると、その要素は見つからなくなります。追加した時点の棚番号と、書き換えた後に計算し直した棚番号がずれてしまうからです。

```csharp
// NG: ハッシュコードの計算に使うプロパティを登録後に変更する
var product = new Product("ノート");
var set = new HashSet<Product> { product };

product.Name = "消しゴム"; // 登録後に変更してしまう

Console.WriteLine(set.Contains(product));

public class Product : IEquatable<Product>
{
    public string Name { get; set; }

    public Product(string name)
    {
        Name = name;
    }

    public bool Equals(Product? other) => other is not null && Name == other.Name;
    public override bool Equals(object? obj) => Equals(obj as Product);
    public override int GetHashCode() => Name.GetHashCode();
}
```

```
False
```

`product`はたしかにHashSetの中にありますが、`Contains`は今の`Name`("消しゴム")から棚番号を計算して探しに行きます。登録したときの棚番号とは違うため、見つかりません。

```csharp
// OK: ハッシュコードの計算に使うプロパティは変更できないようにする
var product = new Product("ノート");
var set = new HashSet<Product> { product };

Console.WriteLine(set.Contains(product));

public class Product : IEquatable<Product>
{
    public string Name { get; }

    public Product(string name)
    {
        Name = name;
    }

    public bool Equals(Product? other) => other is not null && Name == other.Name;
    public override bool Equals(object? obj) => Equals(obj as Product);
    public override int GetHashCode() => Name.GetHashCode();
}
```

```
True
```

### Equalsは実装したのに==演算子を実装し忘れる

`Equals`と`==`は別々の仕組みです。`Equals`をオーバーライドしても、`==`演算子を自分でオーバーロードしない限り、`==`は既定のまま(参照等価)で動き続けます。

```csharp
// NG: Equalsは実装したが==演算子はオーバーロードしていない
var p1 = new Point(1, 2);
var p2 = new Point(1, 2);

Console.WriteLine(p1.Equals(p2));
Console.WriteLine(p1 == p2);

public class Point : IEquatable<Point>
{
    public int X { get; }
    public int Y { get; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public bool Equals(Point? other) => other is not null && X == other.X && Y == other.Y;
    public override bool Equals(object? obj) => Equals(obj as Point);
    public override int GetHashCode() => HashCode.Combine(X, Y);
}
```

```
True
False
```

`Equals`はTrueなのに、同じ値のはずの`p1 == p2`はFalseになります。値が同じかどうかを`==`で比較したいなら、演算子オーバーロードが別途必要です。

```csharp
// OK: ==と!=も一緒にオーバーロードする
var p1 = new Point(1, 2);
var p2 = new Point(1, 2);

Console.WriteLine(p1.Equals(p2));
Console.WriteLine(p1 == p2);

public class Point : IEquatable<Point>
{
    public int X { get; }
    public int Y { get; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public bool Equals(Point? other) => other is not null && X == other.X && Y == other.Y;
    public override bool Equals(object? obj) => Equals(obj as Point);
    public override int GetHashCode() => HashCode.Combine(X, Y);

    public static bool operator ==(Point? left, Point? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(Point? left, Point? right) => !(left == right);
}
```

```
True
True
```

## 注意点

`IEquatable<T>`を実装せず`Equals(object)`だけで比較を繰り返すと、特に`struct`(値型)を比較するときにパフォーマンスが落ちることがあります。`Equals(object)`を呼ぶには引数を一度object型として扱う必要があり、値型の場合はこの変換を「ボックス化」と呼びます。ボックス化が起きると、値をヒープ上に一時的にコピーする処理が挟まり、比較のたびに余分なコストがかかります。`IEquatable<T>`を実装しておけば、型そのもの(T)として直接比較でき、このコストを避けられます。

## 用語まとめ

| 用語 | 意味 |
|---|---|
| 参照等価 | 2つの変数が同じインスタンス(同じ場所)を指しているかどうかの判定 |
| 値等価 | 2つのインスタンスの中身の値が一致しているかどうかの判定 |
| ハッシュコード | インスタンスを表す1つの数値。GetHashCodeが返す |
| 等価性の契約 | Equalsがtrueを返す2つのインスタンスは、必ず同じGetHashCodeを返さなければならないというルール |
| `IEquatable<T>` | 型安全な比較用のメソッドEquals(T)を約束するインターフェース |
| ボックス化 | 値型を一時的にobject型として扱うために、ヒープ上にコピーする処理 |

## 理解度チェック

- [ ] 参照等価と値等価の違いを説明できる
- [ ] Equalsをオーバーライドするとき、なぜGetHashCodeも一緒に直す必要があるか説明できる
- [ ] ハッシュコードの役割をたとえを使わずに説明できる
- [ ] `IEquatable<T>`を実装したクラスを書ける
- [ ] 可変なプロパティをハッシュコードの計算に使うと、なぜ危険か説明できる
- [ ] ==演算子とEqualsの関係を説明できる
