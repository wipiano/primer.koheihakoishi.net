---
title: "virtual/overrideの基本"
category: "文法"
order: 16
prerequisites:
  - file: class-basic.md
    note: クラスの定義とインスタンス化の書き方が分かっていれば十分です
  - file: tostring.md
    note: overrideキーワードでメソッドを差し替える書き方を知っていれば十分です
next:
  - file: interface-basic.md
    note: 実装を持たないインターフェースとの使い分けを学べます
  - file: equality.md
    note: Equalsのオーバーライドなど、override中心の話をさらに学べます
related: []
---

## この記事でわかること

- 基底クラスのメソッドを、派生クラスで安全に差し替える書き方が分かる
- `override`と`new`の違いが説明できる
- コンストラクタから`virtual`メソッドを呼んではいけない理由が分かる

## 一言でいうと

あるクラスの機能を引き継いで新しいクラスを作ることを継承といいます。元になるクラスを基底クラス、継承して作った新しいクラスを派生クラスと呼びます。`virtual`と`override`は、基底クラスで用意した処理の一部を、派生クラス側で差し替えるための仕組みです。

たとえば、ハンバーグの基本レシピに「ソースの部分はお店ごとに変えてよい」と注記されているとします。ある店はそのレシピを元に自分の店用レシピを作り、ソースだけ辛口ダレに書き換えました。プログラムの世界では、この注記にあたるのが`virtual`です。基底クラスのメソッドに`virtual`を付けると「派生クラスで差し替えてよい」という意味になり、派生クラス側で`override`を使うと実際に差し替えられます。ただし実際のレシピと違って、`override`しなければ基底クラスの処理がそのまま使われるだけで、何も書かなければ動作は変わりません。

## どんなときに困るのか

会員ランクごとに割引率を計算するコードを書いていて、最初は一般会員だけを想定していたとします。そこへプレミアム会員が増えると、if文で会員の種類を判定して割引率を分岐させることになります。さらに新しいランクが増えるたびに、同じような判定処理があちこちのメソッドに散らばっていき、1つ直し忘れただけで割引率がずれるバグを埋め込んでしまいます。

`virtual`と`override`を使うと、呼び出す側は「会員インスタンスの`GetDiscountRate()`を呼ぶ」とだけ書けばよくなります。会員の種類ごとの計算はそれぞれの派生クラスに閉じ込められるので、新しいランクを追加しても既存のif文を探し回る必要がなくなります。

## 動かしてみる

```csharp
Member member = new PremiumMember();
Console.WriteLine(member.GetDiscountRate());

public class Member
{
    public virtual double GetDiscountRate() => 0;
}

public class PremiumMember : Member
{
    public override double GetDiscountRate() => 0.1;
}
```

```
0.1
```

`member`変数は`Member`型として宣言していますが、実際に`new`しているのは`PremiumMember`です。`GetDiscountRate()`を呼び出すと、変数の宣言型である`Member`ではなく、実際に生成したインスタンスの型である`PremiumMember`の`override`済みのメソッドが実行されるため、0.1が出力されます。もし`PremiumMember`側で`override`していなければ、`Member`の実装である0がそのまま呼ばれます。

## 仕組み

`virtual`を付けたメソッドは「このメソッドは派生クラスで差し替えてよい」という印です。逆に`virtual`が付いていないメソッドを派生クラスで`override`しようとすると、コンパイルエラーになります。派生クラス側で`override`すると、そのメソッド名で呼び出したときに実際に実行されるのは常に派生クラスの実装になります。これは、呼び出し元の変数がどんな型で宣言されているかではなく、生成したインスタンスの実際の型を見て決まるためです。基底クラスの実装も使いたい場合は、`override`したメソッドの中で`base.メソッド名()`と書くと、差し替える前の基底クラスの処理を呼び出せます。

- `virtual`を付けていないメソッドは`override`できない
- `override`すると、基底クラス型の変数経由で呼び出しても派生クラスの実装が実行される
- `base.メソッド名()`で基底クラスの実装を呼び出せる
- `override`するメソッドは、基底クラスと同じ戻り値の型・同じ引数でなければならない

## 実務での使い方

基底クラスの処理をそのまま活かしつつ、一部だけ追加したいときは`base.メソッド名()`を組み合わせます。

```csharp
var logger = new InfoLogger();
logger.Write("処理を開始しました");

public class Logger
{
    public virtual void Write(string message) => Console.WriteLine(message);
}

public class InfoLogger : Logger
{
    public override void Write(string message) => base.Write($"[INFO] {message}");
}
```

```
[INFO] 処理を開始しました
```

`InfoLogger`は`Write`をまるごと書き直すのではなく、`base.Write(...)`を呼んで元の出力処理を使い回し、そこにプレフィックスを追加しているだけです。

- 基底クラスの処理はそのまま使いつつ何かを追加したいだけなら、`override`した中で`base.メソッド名()`を呼ぶ
- 完全に別の処理に置き換えたいなら、`base`を呼ばずに`override`内で書き直す

## よくある間違い

### overrideのつもりでnewを使ってしまう

`override`を書かずに派生クラスへ同名のメソッドを追加すると、`new`を付けているかどうかにかかわらず、基底クラスのメソッドとは別物として扱われます。これを隠蔽と呼びます。隠蔽されたメソッドは、呼び出す変数の宣言型によってどちらが呼ばれるかが決まってしまい、`override`のときとは挙動がまったく変わります。

```csharp
// NG: newは同名メソッドを隠すだけで、override扱いにはならない
Member member = new PremiumMember();
Console.WriteLine(member.GetDiscountRate());

public class Member
{
    public virtual double GetDiscountRate() => 0;
}

public class PremiumMember : Member
{
    public new double GetDiscountRate() => 0.1;
}
```

```
0
```

`new`を付けると、`PremiumMember`に別の`GetDiscountRate()`を新しく追加しただけになり、`Member`側のメソッドとは別物として隠蔽されます。`member`変数の宣言型は`Member`なので、呼ばれるのは`Member`側の`GetDiscountRate()`、つまり0になります。

```csharp
// OK: overrideで実際に差し替える
Member member = new PremiumMember();
Console.WriteLine(member.GetDiscountRate());

public class Member
{
    public virtual double GetDiscountRate() => 0;
}

public class PremiumMember : Member
{
    public override double GetDiscountRate() => 0.1;
}
```

```
0.1
```

### コンストラクタからvirtualメソッドを呼んでしまう

基底クラスのコンストラクタから`virtual`メソッドを呼ぶと、その時点で派生クラス側の`override`が実行されます。ところが、派生クラスのコンストラクタの中身はまだ1行も実行されていない段階なので、そこで設定するはずだったフィールドの値がまだ入っていません。

```csharp
// NG: 基底クラスのコンストラクタ内でvirtualメソッドを呼んでいる
var manager = new Manager();

public class Employee
{
    public Employee() => Describe();
    public virtual void Describe() => Console.WriteLine("従業員");
}

public class Manager : Employee
{
    private string _department;
    public Manager()
    {
        _department = "営業部";
    }
    public override void Describe() => Console.WriteLine(_department);
}
```

```
(空行)
```

`Manager`のインスタンスを生成すると、まず基底クラスである`Employee`のコンストラクタが実行され、その中で`Describe()`が呼ばれます。この時点では`Manager`自身のコンストラクタはまだ実行されておらず、`_department`への代入も済んでいないため、空の値のまま出力されてしまいます。

```csharp
// OK: コンストラクタからvirtualメソッドを呼ばず、生成が終わってから呼び出す
var manager = new Manager();
manager.Describe();

public class Employee
{
    public virtual void Describe() => Console.WriteLine("従業員");
}

public class Manager : Employee
{
    private string _department;
    public Manager()
    {
        _department = "営業部";
    }
    public override void Describe() => Console.WriteLine(_department);
}
```

```
営業部
```

## 用語まとめ

| 用語 | 意味 |
|---|---|
| 継承 | あるクラスの機能を引き継いで、新しいクラスを作ること |
| 基底クラス | 継承元になる、元になるクラス |
| 派生クラス | 基底クラスを継承して作った、新しいクラス |
| `virtual` | 基底クラスのメソッドに付けて、派生クラスで差し替えてよいと宣言するキーワード |
| `override` | 派生クラスで、基底クラスの`virtual`メソッドを実際に差し替えるキーワード |
| 隠蔽 | `override`を使わず同名メソッドを定義したときに起きる、基底クラスのメソッドとは別物として扱われる状態 |

## 理解度チェック

- [ ] 継承・基底クラス・派生クラスの関係を説明できる
- [ ] `virtual`と`override`の役割の違いを説明できる
- [ ] `override`と`new`による隠蔽の違いを説明できる
- [ ] `base.メソッド名()`で基底クラスの実装を呼び出せる
- [ ] コンストラクタから`virtual`メソッドを呼んではいけない理由を説明できる
- [ ] 上のNG例のどこが問題か指摘できる
