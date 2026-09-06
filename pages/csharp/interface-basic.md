---
title: "インターフェースの基本"
category: "文法"
order: 22
prerequisites:
  - file: class-basic.md
    note: クラスを定義してインスタンス化できれば十分
  - file: override.md
    note: virtual/overrideで基底クラスの実装を差し替える書き方が分かっていれば十分
next:
  - file: equality.md
    note: IEquatable<T>のようなインターフェースを使った等価性判定の実装につながる
related: []
---

インターフェース(`interface`)は、メソッドの名前・引数・戻り値だけを決め、中身の実装は持たない型だ。「できることの約束」のようなものだが、決めているのは操作の一覧だけで、その中身の書き方までは決めない。

## なぜ必要か

クラスをそのまま引数の型にすると、後から別のクラスに差し替えたいときに引数の型ごと書き換える必要がある。呼び出す側がインターフェース型で受けておけば、渡すインスタンスの実装クラスだけを差し替えられる。

## 動かしてみる

```csharp
INotifier notifier = new EmailNotifier();
notifier.Notify("入荷しました");
notifier = new SmsNotifier();
notifier.Notify("入荷しました");

public interface INotifier
{
    void Notify(string message);
}

public class EmailNotifier : INotifier
{
    public void Notify(string message) => Console.WriteLine($"Email: {message}");
}

public class SmsNotifier : INotifier
{
    public void Notify(string message) => Console.WriteLine($"SMS: {message}");
}
```

```
Email: 入荷しました
SMS: 入荷しました
```

`notifier`の型は`INotifier`のまま変わらないが、代入するインスタンスは`EmailNotifier`から`SmsNotifier`に差し替えている。`notifier.Notify(...)`という呼び出し側の行は1つも書き換えていない。`INotifier`は`Notify`という名前・引数・戻り値だけを決めていて、実際の処理はそれぞれのクラスが`public void Notify(string message) => ...`として持っている。

## 最低限の理解

- インターフェースはメソッド(プロパティも書ける)の一覧を決めるだけで、実装(中身)を持たない
- クラスが`: インターフェース名`と書くと、そのメンバーすべての実装が必須になる。1つでも実装し忘れるとコンパイルエラー(CS0535)になる
- 型名の先頭に`I`を付ける決まりはないが、現場ではほぼ全員がこの慣習に従う。`INotifier`のように書く
- 呼び出す側の変数や引数をインターフェース型で受けておくと、実装クラスを差し替えても呼び出し側のコードは書き換えずに済む
- 基底クラスの既定実装を一部だけ差し替えたいなら`virtual`/`override`、実装をまるごと入れ替えたいならインターフェースを使う

## ⚠️ 実装が1つしかないのにインターフェース化する

差し替える予定が無いクラスにまでインターフェースを作ると、ファイルと型が2倍に増えるだけで見通しが悪くなる。差し替える計画ができてから作っても遅くない。

```csharp
// NG: PriceCalculatorの実装は1つしかなく、差し替える予定もない
IPriceCalculator calculator = new PriceCalculator();
Console.WriteLine(calculator.Calculate(1000));

public interface IPriceCalculator
{
    int Calculate(int price);
}

public class PriceCalculator : IPriceCalculator
{
    public int Calculate(int price) => (int)(price * 1.1);
}
```

```
1100
```

```csharp
// OK: 差し替える予定がないならクラスのまま使う
PriceCalculator calculator = new PriceCalculator();
Console.WriteLine(calculator.Calculate(1000));

public class PriceCalculator
{
    public int Calculate(int price) => (int)(price * 1.1);
}
```

```
1100
```

## 課題

自分の担当ドメインで、実装をあとから差し替える可能性があるクラスを1つ選び、インターフェースを定義してそのクラスに実装させてみる。あわせて、差し替える予定が本当にないクラスまでインターフェース化していないか見直してみる。
