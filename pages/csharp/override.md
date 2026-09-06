---
title: "virtual/overrideの基本"
category: "文法"
order: 9
related:
  - class-basic.md
  - tostring.md
  - interface-basic.md
  - equality.md
---

## 概要

- `virtual`は基底クラスでメソッドを「派生クラスで上書き可能」と宣言するキーワード
- `override`は派生クラスでその実装を上書きするキーワード
- 読了後、基底クラスの共通処理を派生クラスで安全に差し替えられるようになる

## なぜ必要なのか

- 上書きの仕組みが無いと、型ごとの分岐（if/switch）が散乱しコードが複雑化する
- virtual/overrideを使うと、呼び出し側は型を意識せず共通のメソッド名で呼べる（ポリモーフィズム）
- インターフェースだけでは提供できない「既定実装＋一部だけ差し替え」ができる

## 仕組みと動作原理

- 基底クラスのメソッドに`virtual`を付けると、実行時にインスタンスの実際の型のメソッドが呼ばれる
- 派生クラスで`override`すると、基底クラス経由で呼んでも派生クラス側の実装が実行される（動的ディスパッチ）
- `override`を付けずに同名メソッドを定義すると`new`扱いになり、別メソッドとして隠蔽するだけになる
- 基底の実装も呼びたい場合は`base.メソッド名()`で明示的に呼び出す

## 基本的な書き方とコード例

- 最小の使い方

```csharp
public class Shape
{
    public virtual double GetArea() => 0;
}

public class Circle : Shape
{
    public double Radius { get; init; }
    public override double GetArea() => Math.PI * Radius * Radius;
}

Shape shape = new Circle { Radius = 2 };
Console.WriteLine(shape.GetArea()); // Circleの実装が呼ばれる
```

- baseで基底の処理を組み合わせる

```csharp
public class Logger
{
    public virtual void Write(string message) => Console.WriteLine(message);
}

public class TimestampLogger : Logger
{
    public override void Write(string message) => base.Write($"[{DateTime.Now:HH:mm:ss}] {message}");
}
```

- 使い分けの判断基準
  - 既定実装を提供しつつ一部だけ変えたい場合は`virtual`/`override`を使う
  - 実装を一切持たず契約だけ定義したい場合はインターフェースを使う

## よくある誤用・バグ

- **overrideを付け忘れてnewになる**
  - `new`は同名メソッドを隠すだけで、ポリモーフィズムが働かない

```csharp
// NG: override忘れでnew扱いになり、基底型経由では基底の実装が呼ばれる
public class Shape
{
    public virtual double GetArea() => 0;
}
public class Circle : Shape
{
    public double Radius { get; init; }
    public new double GetArea() => Math.PI * Radius * Radius; // newは意図せぬ隠蔽
}
Shape shape = new Circle { Radius = 2 };
Console.WriteLine(shape.GetArea()); // 0（基底の実装が呼ばれる）
```

```csharp
// OK: overrideを付けて動的ディスパッチさせる
public class Circle : Shape
{
    public double Radius { get; init; }
    public override double GetArea() => Math.PI * Radius * Radius;
}
Shape shape = new Circle { Radius = 2 };
Console.WriteLine(shape.GetArea()); // Circleの実装が呼ばれる
```

- **コンストラクタからvirtualメソッドを呼ぶ**
  - 派生クラスの初期化が終わる前にoverride先が実行され、未初期化フィールドを参照するバグになる

```csharp
// NG: 基底コンストラクタ内でvirtualメソッドを呼ぶ
public class Base
{
    public Base() => Initialize(); // 派生の初期化前に呼ばれる
    public virtual void Initialize() { }
}
public class Derived : Base
{
    private readonly string _name = "derived";
    public override void Initialize() => Console.WriteLine(_name); // まだ null
}
```

```csharp
// OK: コンストラクタからvirtualメソッドを呼ばない設計にする
public class Base
{
    public void Setup() => Initialize();
    protected virtual void Initialize() { }
}
```

## 理解度チェックリスト

- [ ] virtualとoverrideの役割の違いを説明できる
- [ ] overrideとnewの違いを説明できる
- [ ] base.メソッド名()で基底の実装を呼び出せる
- [ ] コンストラクタからvirtualメソッドを呼んではいけない理由を説明できる
- [ ] virtual/overrideとインターフェースの使い分けができる
