---
title: "virtual/overrideの基本"
category: "文法"
order: 16
prerequisites:
  - file: class-basic.md
    note: クラスの定義とインスタンス化の書き方が分かれば十分
  - file: tostring.md
    note: overrideキーワードでメソッドを差し替える書き方を知っていれば十分
next:
  - file: interface-basic.md
    note: 実装を持たないインターフェースとの使い分けにつながる
  - file: equality.md
    note: Equalsのオーバーライドなど、override中心の理解をさらに深められる
related: []
---

継承(けいしょう)は、あるクラスの機能を引き継いで新しいクラスを作ることだ。継承元のクラスを基底クラス、継承して作った新しいクラスを派生クラスと呼ぶ。`virtual`は基底クラスのメソッドに付けて「派生クラスで上書きしてよい」と宣言するキーワードで、`override`は派生クラス側で実際に上書きするキーワードだ。

## 動かしてみる

```csharp
Employee employee = new Manager();
Console.WriteLine(employee.Describe());

public class Employee
{
    public virtual string Describe() => "従業員";
}

public class Manager : Employee
{
    public override string Describe() => base.Describe() + "(マネージャー)";
}
```

```
従業員(マネージャー)
```

`employee`変数は`Employee`型で宣言しているが、実際に`new`しているのは`Manager`だ。`Describe()`を呼び出すと、変数の宣言型ではなく、実際に生成したインスタンスの型である`Manager`の`override`済みの実装が実行される。`Manager`の`override`の中では`base.Describe()`と書くことで、上書きする前の`Employee`の実装も呼び出している。

## 最低限の理解

- `virtual`は「このメソッドは派生クラスで上書きしてよい」という印。付いていないメソッドは`override`できない
- **`override`すると、基底クラス型の変数経由で呼び出しても、実行されるのは派生クラスの実装になる**
- `base.メソッド名()`で、上書きする前の基底クラスの処理を呼び出せる
- `override`するメソッドは、基底クラスと同じ戻り値の型・同じ引数でなければならない

## ⚠️ overrideを付け忘れる

`override`を書かずに派生クラスへ同名のメソッドを定義すると、基底クラスのメソッドとは別物の新しいメソッドとして扱われる。これを隠蔽と呼ぶ。ビルドは通るが「override キーワードを追加してください」という警告(CS0114)が出て、呼び出し結果が変数の宣言型によって変わってしまう。

```csharp
// NG: overrideを書き忘れると、基底クラスのメソッドとは別物として隠蔽される
Member member = new PremiumMember();
Console.WriteLine(member.GetDiscountRate());

public class Member
{
    public virtual double GetDiscountRate() => 0;
}

public class PremiumMember : Member
{
    public double GetDiscountRate() => 0.1;
}
```

```
0
```

`member`変数の宣言型は`Member`なので、呼ばれるのは`Member`側の`GetDiscountRate()`であり0が出力される。`PremiumMember`に定義した`GetDiscountRate()`は、警告が出るだけで上書きにはならない。

```csharp
// OK: overrideを付けて実際に上書きする
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

## ⚠️ コンストラクタからvirtualメソッドを呼ぶ

基底クラスのコンストラクタから`virtual`メソッドを呼ぶと、その時点で実行されるのは派生クラス側の`override`だ。ところが派生クラスのコンストラクタ本体はまだ実行されておらず、そこで代入するはずだったフィールドの値がまだ入っていない。

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

```

`Manager`を生成すると、まず基底クラスである`Employee`のコンストラクタが実行され、その中で`Describe()`が呼ばれる。この時点では`Manager`自身のコンストラクタ本体はまだ実行されておらず、`_department`への代入も済んでいないため、空の値のまま出力される。

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

## 課題

自分が扱うドメインで基底クラス1つと派生クラス2つを作り、共通のメソッドを`virtual`にして、それぞれ異なる出力になることを確認する。次に、片方の派生クラスから`override`を外してビルドし、どんな警告が出て呼び出し結果がどう変わるか確認する。
