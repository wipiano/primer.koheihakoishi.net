---
title: "構造体(struct)の基本"
category: "文法"
order: 11
prerequisites:
  - file: class-basic.md
    note: クラスの基本構文（フィールド・プロパティ・コンストラクタ）が分かれば十分
  - file: reference-vs-value-types.md
    note: 値型は代入するとコピーされるという基礎が分かれば十分
next:
  - file: readonly-struct.md
    note: structを不変にして意図しない書き換えを防ぐ書き方につながる
  - file: class-struct-record-basics.md
    note: class・struct・recordをどう使い分けるかの判断基準につながる
related: []
---

`struct` は、複数の値をひとつにまとめた値型を自分で定義するキーワードだ。

- `int` と同じ値型なので、代入や引数渡しのたびに中身がまるごとコピーされる
- 座標や金額のように、それ自体が1つの値である概念を表す
- 書き方はクラスとほぼ同じで、フィールド・プロパティ・コンストラクタを持てる

## なぜ必要か

座標(1, 2)や金額480円はそれ自体が値であり、2つの(1, 2)を区別する意味はない。こうした概念は、1つの実体を共有するのではなく、`int` のように値としてコピーして扱うのが自然だ。`struct` は「この型は値そのものだ」と宣言する手段で、参照型であるクラスとはそこが違う。

## 動かしてみる

```csharp
var p1 = new Point(1, 2);
var p2 = p1; // 値がまるごとコピーされ、p1とp2は別々の実体になる
p2.X = 100;  // p2を書き換えてもp1は1のまま

Console.WriteLine(p1.X);
Console.WriteLine(p2.X);

struct Point
{
    public int X;
    public int Y;

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }
}
```

```
1
100
```

もし `Point` をクラスで定義していたら、`p1` と `p2` は同じインスタンスを指すため、`p2.X` を変えると `p1.X` も `100` になっていた。

## 最低限の理解

- `struct` は値型（「参照型と値型の違い」で説明した、代入するたびに値そのものがコピーされる型）
- 座標・金額・IDのように、小さくて不変な値を表すのに向いている。フィールド数が少なければコピーの負担も小さい
- `struct` は他の `struct` やクラスを継承できないし、他から継承されることもない
- サイズが大きい、または内容を書き換えたい型はクラスにする。コピーの負担が増えるうえ、コピーのどちらを書き換えたつもりかで混乱しやすくなる

## ⚠️ プロパティ経由でstructのフィールドを直接書き換える

クラスのプロパティが返す `struct` に対して、そのフィールドを直接書き換えようとするとコンパイルエラーになる。プロパティの `get` が返すのは一時的なコピーであり、そのコピーを書き換えても呼び出し元には反映されないため、コンパイラが先に止める。

```csharp
// NG: プロパティが返すのは一時的なコピーなので書き換えられない
var box = new Box { Position = new Point(0, 0) };
box.Position.X = 10;

struct Point
{
    public int X;
    public int Y;

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }
}

class Box
{
    public Point Position { get; set; }
}
```

```
error CS1612: 変数ではないため、'Box.Position' の戻り値を変更できません
```

```csharp
// OK: 新しい値を丸ごと作り直して代入する
var box = new Box { Position = new Point(0, 0) };
box.Position = new Point(10, box.Position.Y);
Console.WriteLine(box.Position.X);

struct Point
{
    public int X;
    public int Y;

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }
}

class Box
{
    public Point Position { get; set; }
}
```

```
10
```

## 課題

自分の業務で扱う小さな値（金額、座標、IDなど）をひとつ選んで `struct` で定義する。変数に代入して片方の値を書き換え、もう片方が変わらないことを確認する。
