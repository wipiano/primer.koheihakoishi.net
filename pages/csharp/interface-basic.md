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

インターフェース(`interface`)は、「何ができるか」だけを決め、「どうやるか」を持たない型だ。

- メソッド(プロパティも書ける)の名前・引数・戻り値の一覧だけを定義する
- クラスがインターフェースを実装すると、その一覧をすべて持つことを型として保証する
- 変数や引数の型として使え、実装クラスが何であっても同じ呼び方ができる

## なぜ必要か

通知を送る側にとって大事なのは「通知できる」ことで、メールで送るかSMSで送るかではない。ところがクラスをそのまま型にすると、「何ができるか」と「どうやるか」がひとつに結び付き、呼び出す側が具体的な実装に縛られる。インターフェースはこの2つを分け、呼び出す側が「何ができるか」だけに依存できるようにする。

## 動かしてみる

```csharp
INotifier notifier = new EmailNotifier(); // 変数の型はインターフェース
notifier.Notify("入荷しました");
notifier = new SmsNotifier(); // 実装を差し替えても、呼び出す行は変わらない
notifier.Notify("入荷しました");

public interface INotifier
{
    void Notify(string message); // 名前・引数・戻り値だけ。中身は書かない
}

public class EmailNotifier : INotifier // : で実装することを宣言する
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

## 最低限の理解

- インターフェースは「何ができるか」の一覧を決めるだけで、実装(中身)を持たない
- クラスが`: インターフェース名`と書くと、その一覧すべての実装が必須になる。1つでも実装し忘れるとコンパイルエラー(CS0535)になる
- 型名の先頭に`I`を付ける決まりはないが、現場ではほぼ全員がこの慣習に従う。`INotifier`のように書く
- **呼び出す側の変数や引数は、実装クラスではなくインターフェース型で受ける**。実装を差し替えても呼び出す側は変わらない
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
