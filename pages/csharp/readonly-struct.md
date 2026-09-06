---
title: "readonly structの基本"
category: "文法"
order: 24
prerequisites:
  - file: struct-basic.md
    note: structが値型で、代入や引数渡しのたびにコピーされることが分かれば十分
next:
  - file: immutable.md
    note: readonly structで学んだ考え方を、クラスやrecordを含む設計全体の不変性へ広げられる
related:
  - class-struct-record-basics.md
---

`readonly struct`は、インスタンスを生成した後はすべてのフィールドとプロパティが変更できないことを、コンパイラが保証する`struct`だ。

## なぜ必要か

`struct`は値型で、代入や引数渡しのたびに値がコピーされる(「構造体(struct)の基本」の通り)。ただし`struct`自体は既定では可変で、プロパティやフィールドを自由に書き換えられる。コピーされる性質と可変な中身が組み合わさると、どのコピーに変更が反映されたのか分からなくなる事故が起きやすい。`readonly struct`は「そもそも書き換えられない」という制約を型に付け、この事故を未然に防ぐ。

## 定義してみる

```csharp
var point = new Point(1, 2);
Console.WriteLine($"X={point.X}, Y={point.Y}");

public readonly struct Point
{
    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int X { get; }
    public int Y { get; }
}
```

```
X=1, Y=2
```

`struct`に`readonly`を付けると、`X`や`Y`のようなプロパティはすべて`get`専用にする必要があり、値はコンストラクタで一度設定した後は書き換えられない。通常の`struct`なら`public int X { get; set; }`のように`set`を書けるが、`readonly struct`の中で同じことをすると次のようにコンパイルエラーになる。

```csharp
// NG: readonly structにsetプロパティを混在させる
public readonly struct Point
{
    public int X { get; set; }
    public int Y { get; }
}
```

```
error CS8341: 読み取り専用の構造体に含まれる自動実装インスタンスのプロパティは、読み取り専用である必要があります。
```

## 最低限の理解

- **`readonly struct`は、すべてのメンバーが変更不可であることをコンパイラが保証する。** 一部だけ可変にすることは許されず、`set`を1つ混ぜただけでもコンパイルエラーになる
- メソッドの引数に`in`を付けると、値をコピーせず参照として渡せる。渡した`struct`が可変だと、コンパイラは中で書き換えられていないか保証できないため、呼び出す直前にコピー(防御的コピー。意図しない変更を防ぐためコンパイラが自動で作る一時的な複製)を作ってから呼び出す
- `readonly struct`なら中身が絶対に変わらないと分かっているので、この防御的コピーを省略できる。`in`パラメータと組み合わせるとコピーのコストを抑えられる
- 生成した後に値を変えるつもりがない小さな値型(座標・金額・日付範囲など)は、まず`readonly struct`にする

## ⚠️ 可変なstructをinで渡しても変更が反映されない

`readonly`を付けていない可変な`struct`を`in`で渡し、その中で状態を変えるメソッドを呼び出しても、コンパイルエラーにはならない。だが実際に変更されるのは呼び出す直前に作られた防御的コピーで、呼び出し元にも呼び出したメソッドの中の変数にも反映されない。

```csharp
// NG: 可変なCounterをinで渡し、Incrementで変更したつもりになる
void ShowAfterIncrement(in Counter counter)
{
    counter.Increment();
    Console.WriteLine(counter.Value);
}

var counter = new Counter { Value = 0 };
ShowAfterIncrement(in counter);

public struct Counter
{
    public int Value;
    public void Increment() => Value++;
}
```

```
0
```

```csharp
// OK: 状態を変えず、新しい値を作って返す
var counter = new Counter(0);
counter = counter.Increment();
Console.WriteLine(counter.Value);

public readonly struct Counter
{
    public Counter(int value) => Value = value;
    public int Value { get; }
    public Counter Increment() => new Counter(Value + 1);
}
```

```
1
```

## 課題

自分が扱う値(金額、日時範囲、座標など)を1つ選び、`readonly struct`として定義してみる。コンストラクタで値を設定し、`in`パラメータを取るメソッドに渡して、防御的コピーが作られないことを確認する。
