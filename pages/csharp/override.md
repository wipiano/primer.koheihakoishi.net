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

`virtual`と`override`は、基底クラスのメソッドの振る舞いを、派生クラスで差し替えるためのキーワードだ。

- 継承(あるクラスの機能を引き継いで新しいクラスを作ること)が前提。引き継ぐ元を基底クラス、引き継いだ側を派生クラスと呼ぶ
- `virtual`は基底クラス側で「このメソッドは差し替えてよい」と宣言する
- `override`は派生クラス側で実際に差し替える

## なぜ必要か

「従業員の説明を出せ」という呼びかけは1種類でも、実際の説明はマネージャーと一般社員で違う。呼び出す側がその違いを`if`で分けると、種類が増えるたびに呼び出す側を書き換えることになる。`virtual`/`override`は、呼びかけ方を基底クラスで1つに決め、実際の振る舞いは実体の型に任せる仕組みだ。

## 動かしてみる

```csharp
Employee employee = new Manager(); // 変数の型は Employee、実体は Manager
Console.WriteLine(employee.Describe()); // 実体である Manager の実装が呼ばれる

public class Employee
{
    public virtual string Describe() => "従業員"; // 差し替えてよい印
}

public class Manager : Employee
{
    public override string Describe() => base.Describe() + "(マネージャー)"; // base.〜 で差し替え前の実装を呼べる
}
```

```
従業員(マネージャー)
```

## 最低限の理解

- `virtual`は「このメソッドは派生クラスで上書きしてよい」という印。付いていないメソッドは`override`できない
- **`override`すると、基底クラス型の変数経由で呼び出しても、実行されるのは派生クラスの実装になる**
- `base.メソッド名()`で、上書きする前の基底クラスの処理を呼び出せる
- `override`するメソッドは、基底クラスと同じ戻り値の型・同じ引数でなければならない

## ⚠️ overrideを付け忘れる

`override`を書かずに派生クラスへ同名のメソッドを定義すると、基底クラスのメソッドとは別物の新しいメソッドとして扱われる。これを隠蔽と呼ぶ。ビルドは通るが警告(CS0114)が出て、呼び出し結果が変数の宣言型で決まってしまう。

```csharp
// NG: overrideを書き忘れると、基底クラスのメソッドとは別物として隠蔽される
Member member = new PremiumMember();
Console.WriteLine(member.GetDiscountRate()); // 宣言型 Member 側の実装が呼ばれる

public class Member
{
    public virtual double GetDiscountRate() => 0;
}

public class PremiumMember : Member
{
    public double GetDiscountRate() => 0.1; // 警告が出るだけで、差し替えにはならない
}
```

```
0
```

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
    public Employee() => Describe(); // 派生クラスの Describe が呼ばれる
    public virtual void Describe() => Console.WriteLine("従業員");
}

public class Manager : Employee
{
    private string _department;
    public Manager()
    {
        _department = "営業部"; // Describe が呼ばれた時点では、まだここに来ていない
    }
    public override void Describe() => Console.WriteLine(_department); // 空のまま出力される
}
```

```

```

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
