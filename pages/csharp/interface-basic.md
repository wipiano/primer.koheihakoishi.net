---
title: "インターフェースの基本"
category: "文法"
order: 22
prerequisites:
  - file: class-basic.md
    note: クラスを定義してインスタンスを作れること
  - file: override.md
    note: virtual/overrideで基底クラスの実装を差し替える書き方が分かっていること
next:
  - file: equality.md
    note: インターフェース(IEquatable<T>など)を使った等価性判定の実装につながるから
related: []
---

## この記事でわかること

- インターフェースが「実装を持たない契約」であることを説明できる
- インターフェースを定義し、クラスに実装できる
- インターフェースに依存するコードを書いて、実装をあとから差し替えられる
- virtual/overrideとインターフェースをどう使い分けるか判断できる

## 一言でいうと

インターフェースは、「この名前の操作を、こういう引数と戻り値で必ず用意してください」という約束事です。中身の作り方までは決めず、外から見える形だけを決めます。

コンビニのレジを思い浮かべてください。レジ担当が新人でもベテランでも、「バーコードをスキャンする」「会計をする」「レシートを出す」という同じマニュアル通りに動けば、お客さんはだれがレジに立っていても困りません。プログラムの世界では、このマニュアルが`interface`で、担当者にあたるのがそれを実装するクラスです。マニュアルに書かれた操作さえ用意すれば、中身の担当者（実装）はいつでも交代できます。ただし、実際のマニュアルと違って、インターフェースは「手順の中身」までは一切決めません。

## どんなときに困るのか

通知メールを送るメソッドを、`EmailNotifier`というクラスを直接受け取る形で書いたとします。半年後、SMSでも通知できるようにしてほしいと頼まれました。メソッドの引数の型は`EmailNotifier`に固定されているので、そのままではSMS用のクラスを渡せません。メソッドを増やすか、型ごとの分岐を書くしかなく、呼び出し側のコードも直す羽目になります。さらに先輩から「本物のメール送信を試すテストは書きにくい。実際に送らないダミーの実装に差し替えられるようにしておいて」と指摘されることもあります。

呼び出し側が具体的なクラスではなくインターフェースに依存していれば、実装を追加・差し替えしても呼び出し側のコードは直さずに済みます。

## 動かしてみる

```csharp
// インターフェース越しにメソッドを呼ぶ
INotifier notifier = new EmailNotifier();
notifier.Notify("Hello");

public interface INotifier
{
    void Notify(string message);
}

public class EmailNotifier : INotifier
{
    public void Notify(string message) => Console.WriteLine($"Email: {message}");
}
```

```
Email: Hello
```

`notifier`という変数の型は`INotifier`ですが、中身は`EmailNotifier`のインスタンスです。`INotifier`は`Notify`という操作の名前と引数だけを決めていて、実際の処理は書いていません。処理の中身は`EmailNotifier`側の`public void Notify(string message) => ...`が持っています。だから`notifier.Notify("Hello")`を呼ぶと、`EmailNotifier`が用意した実装が実行され、`Email: Hello`と表示されます。

## 仕組み

インターフェースのメンバー（メソッドやプロパティ）は、既定で`public`扱いで、中身（実装）を持ちません。名前・引数・戻り値だけを決めた「空の枠」です。クラスが`: INotifier`のように書くと、そのクラスは枠に対応する中身をすべて用意する約束をしたことになります。約束を果たさない、つまり実装し忘れたメンバーがあると、コンパイルエラーになります。

もう1つ大事なのは、インターフェース型の変数から呼べるのは、そのインターフェースで決めた操作だけという点です。実装クラスが独自に追加のメソッドやプロパティを持っていても、変数の型が`INotifier`のままではそれらを呼べません。マニュアル（インターフェース）に載っていない作業は、マニュアル越しには頼めない、という感覚に近いものです。

覚えておくべきルールをまとめます。

- インターフェースのメンバーは実装を持たず、既定で`public`
- クラスが`: インターフェース名`と書くと、そのメンバーすべての実装が必須になる
- 実装し忘れたメンバーがあるとコンパイルエラーになる
- 1つのクラスは複数のインターフェースを実装できる（クラスの継承は1つまでだが、インターフェースの実装数に制限はない）
- インターフェース型の変数からは、そのインターフェースで定義されたメンバーしか呼び出せない

## 実務での使い方

呼び出し側をインターフェース型で受け取るようにしておくと、実装をあとから増やしても呼び出し側は変更不要になります。

```csharp
Send(new EmailNotifier(), "セール開始");
Send(new SmsNotifier(), "セール開始");

void Send(INotifier notifier, string message) => notifier.Notify(message);

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
Email: セール開始
SMS: セール開始
```

`Send`は`INotifier`にしか依存していないので、`SmsNotifier`という新しいクラスを追加しても`Send`自体は1行も変えていません。単体テスト（プログラムの一部分だけを取り出して、期待通り動くか自動で確認する仕組み）を書くときも同じ考え方が使えます。本物の`EmailNotifier`を使うと実際にメールが送られてしまうので、代わりに「呼ばれたことだけを記録する」テスト用の実装（モックと呼ばれます）を`INotifier`として渡せば、`Send`のコードを一切変えずにテストできます。

使い分けの判断基準です。

- 実装をあとから追加・差し替えたい、テストで本物ではない実装に入れ替えたい場合はインターフェースに依存させる
- 既定の実装を共有しつつ一部だけ変えたい場合は、virtual/override（『virtual/overrideの基本』で説明した、基底クラスの既定実装を派生クラスで差し替える仕組み）のある基底クラスを使う
- 実装が1つしかなく、差し替える予定もない場合は、無理にインターフェースを作らずクラスのまま使う

## よくある間違い

### メンバーを実装し忘れる

インターフェースが決めた操作の一部を実装しないまま`: インターフェース名`と書いてしまうと、コンパイルが通りません。メソッドを1つ書き足せば直るだけの単純なミスですが、エラーメッセージの意味が分からず戸惑いがちです。

```csharp
// NG: Notifyを実装していない
INotifier notifier = new EmailNotifier();
notifier.Notify("Hello");

public interface INotifier
{
    void Notify(string message);
}

public class EmailNotifier : INotifier
{
}
```

```
error CS0535: 'EmailNotifier' はインターフェイス メンバー 'INotifier.Notify(string)' を実装しません
```

```csharp
// OK: 決めた操作をすべて実装する
INotifier notifier = new EmailNotifier();
notifier.Notify("Hello");

public interface INotifier
{
    void Notify(string message);
}

public class EmailNotifier : INotifier
{
    public void Notify(string message) => Console.WriteLine($"Email: {message}");
}
```

```
Email: Hello
```

### インターフェース型から実装クラス独自のメンバーを呼ぼうとする

実装クラスに便利なプロパティを追加しても、変数の型がインターフェースのままだとそのプロパティは見えません。「クラスには定義してあるのに呼べない」と混乱しがちなポイントです。

```csharp
// NG: SentCountはINotifierにないメンバー
INotifier notifier = new EmailNotifier();
notifier.Notify("Hello");
Console.WriteLine(notifier.SentCount);

public interface INotifier
{
    void Notify(string message);
}

public class EmailNotifier : INotifier
{
    public int SentCount { get; private set; }
    public void Notify(string message)
    {
        SentCount++;
        Console.WriteLine($"Email: {message}");
    }
}
```

```
error CS1061: 'INotifier' に 'SentCount' の定義が含まれておらず、型 'INotifier' の最初の引数を受け付けるアクセス可能な拡張メソッド 'SentCount' が見つかりませんでした
```

```csharp
// OK: 実際の型が分かっているならその型として扱う
INotifier notifier = new EmailNotifier();
notifier.Notify("Hello");
if (notifier is EmailNotifier email)
{
    Console.WriteLine(email.SentCount);
}

public interface INotifier
{
    void Notify(string message);
}

public class EmailNotifier : INotifier
{
    public int SentCount { get; private set; }
    public void Notify(string message)
    {
        SentCount++;
        Console.WriteLine($"Email: {message}");
    }
}
```

```
Email: Hello
1
```

## 用語まとめ

| 用語 | 意味 |
|---|---|
| インターフェース | 実装を持たないメンバー(メソッドやプロパティ)の一覧を定義する型。名前・引数・戻り値だけを決める |
| 契約 | 「この名前の操作をこの形で用意する」という約束事 |
| 実装 | インターフェースが決めた操作の中身を実際に書くこと。またはその中身を書いたクラス |
| 単体テスト | プログラムの一部分だけを取り出して、期待通りに動くかを自動で確認する仕組み |
| モック | テストのために、本物の代わりに使う「呼ばれたことだけを記録する」ような簡易な実装 |

## 理解度チェック

- [ ] インターフェースとクラスの役割の違いを説明できる
- [ ] インターフェースを定義し、クラスに実装できる
- [ ] インターフェース型の変数を使って、実装を差し替えるコードを書ける
- [ ] インターフェース型の変数から呼べるメンバーの範囲を説明できる
- [ ] メンバーの実装忘れがなぜコンパイルエラーになるか説明できる
- [ ] virtual/overrideとインターフェースの使い分けができる
