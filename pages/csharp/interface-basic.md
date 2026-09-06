---
title: "インターフェースの基本"
category: "文法"
order: 13
related:
  - class-basic.md
  - override.md
  - equality.md
---

## 概要

- インターフェースは「実装を持たない契約（メソッド・プロパティのシグネチャ）」を定義する型
- 複数のclass/structに共通の振る舞いを持たせたい場合に使う
- 読了後、インターフェースを定義・実装し、依存を抽象化できるようになる

## なぜ必要なのか

- 具体的なclassに直接依存すると、実装を差し替えるたびに呼び出し側の修正が必要になる
- インターフェースに依存すれば、実装を切り替えても呼び出し側のコードは変更不要
- 単体テストでモック実装に差し替える際にも、インターフェースが前提になる

## 仕組みと動作原理

- インターフェースのメンバーは既定で`public`かつ実装を持たない
- classやstructが`: IInterfaceName`と書くと、そのメンバーの実装が強制される
- 1つのclassは複数のインターフェースを実装できる（多重継承はできないが多重実装は可能）
- インターフェース型の変数からは、そのインターフェースで定義されたメンバーしか呼べない

## 基本的な書き方とコード例

- 最小の使い方

```csharp
public interface INotifier
{
    void Notify(string message);
}

public class EmailNotifier : INotifier
{
    public void Notify(string message) => Console.WriteLine($"Email: {message}");
}

INotifier notifier = new EmailNotifier();
notifier.Notify("Hello");
```

- 複数実装を切り替える

```csharp
public class SmsNotifier : INotifier
{
    public void Notify(string message) => Console.WriteLine($"SMS: {message}");
}

void Send(INotifier notifier, string message) => notifier.Notify(message);

Send(new EmailNotifier(), "test");
Send(new SmsNotifier(), "test"); // 呼び出し側は変更不要
```

- 使い分けの判断基準
  - 実装を差し替えたい、テストでモックにしたい場合はインターフェースに依存させる
  - 既定実装を共有しつつ一部だけ変えたい場合は`virtual`/`override`のある基底classを使う

## よくある誤用・バグ

- **なんでもインターフェース化してしまう**
  - 実装が1つしかなく変更予定もないのにインターフェースを作ると、無駄な間接参照が増える

```csharp
// NG: 実装が1つしかないのに常にインターフェース越しにする
public interface IOrderIdGenerator
{
    string Generate();
}
public class OrderIdGenerator : IOrderIdGenerator
{
    public string Generate() => Guid.NewGuid().ToString();
}
```

```csharp
// OK: 差し替え・モック化の必要が出るまではclassのまま使う
public class OrderIdGenerator
{
    public string Generate() => Guid.NewGuid().ToString();
}
```

- **インターフェースに実装詳細を持ち込む**
  - インターフェースは契約であり、具体的なコレクション型などの実装詳細を強制すべきではない

```csharp
// NG: インターフェース越しに内部実装（Listである必要性）を強制する
public interface IOrder
{
    List<string> Items { get; set; }
}
```

```csharp
// OK: 契約は必要最小限の操作にとどめる
public interface IOrder
{
    IReadOnlyList<string> Items { get; }
    void AddItem(string item);
}
```

## 理解度チェックリスト

- [ ] インターフェースとclassの役割の違いを説明できる
- [ ] インターフェースを定義し、classに実装できる
- [ ] インターフェース型の変数越しに実装を切り替えるコードを書ける
- [ ] インターフェース化のやりすぎがなぜ問題かを説明できる
- [ ] virtual/overrideとインターフェースの使い分けができる
