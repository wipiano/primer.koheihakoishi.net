---
title: "readonly structの基本"
category: "文法"
order: 17
related:
  - struct-basic.md
  - immutable.md
  - class-struct-record-basics.md
---

## 概要

- `readonly struct`は、すべてのフィールド・プロパティが生成後に変更不可であることをコンパイラに保証させる構文
- 不変な値型（座標・金額など）を安全かつ効率よく扱いたい場面で使う
- 読了後、readonly structを書き、通常のstructとの違いを説明できるようになる

## なぜ必要なのか

- 通常のstructはメンバーを可変にできてしまい、意図しない変更やコンパイラの防御的コピーの原因になる
- readonly structにすると、コンパイラが変更不可を保証し、不要な防御的コピーを避けられる
- structのメリット（値型としての軽さ）と不変性の安全性を両立できる

## 仕組みと動作原理

- `readonly struct`ではすべてのプロパティを`get`のみ（またはinit）にする必要があり、フィールドの直接変更は不可
- 通常のstructを`in`パラメータ経由で使うと、コンパイラは「本当に変更されないか」を保証できず、暗黙のコピー（防御的コピー）を作ることがある
- readonly structと明示すると、コンパイラは防御的コピーを省略できる
- メソッド自体を`readonly`にすることも可能（そのメソッドがstructを変更しないことを保証する）

## 基本的な書き方とコード例

- 最小の使い方

```csharp
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

- readonlyメソッドの明示

```csharp
public readonly struct Vector2
{
    public Vector2(float x, float y) => (X, Y) = (x, y);
    public float X { get; }
    public float Y { get; }

    public readonly float Length() => MathF.Sqrt(X * X + Y * Y); // 変更しないことを明示
}
```

- 使い分けの判断基準
  - 生成後に一切変更しない値型を作る場合は、通常のstructではなくreadonly structを選ぶ
  - `in`パラメータで頻繁に渡す値型は、防御的コピーを避けるためreadonly struct化を検討する

## よくある誤用・バグ

- **可変プロパティを混在させてコンパイルエラーになる**
  - readonly struct内で`set`を持つプロパティを定義すると、コンパイルエラーになる

```csharp
// NG: readonly structにsetプロパティを混在させる
public readonly struct Point
{
    public int X { get; set; } // コンパイルエラー
}
```

```csharp
// OK: initまたはコンストラクタ経由のみで値を設定する
public readonly struct Point
{
    public Point(int x) => X = x;
    public int X { get; }
}
```

- **通常のstructをreadonlyだと思い込む**
  - `readonly`修飾なしのstructを`in`パラメータで渡しても、変更されない保証はコンパイラには無い

```csharp
// NG: readonly structだと思い込み、in渡しの最適化を期待する
public struct Point { public int X { get; set; } }
void Show(in Point p) { /* コンパイラは変更可能性を排除できず防御的コピーが発生しうる */ }
```

```csharp
// OK: 不変性を保証したいならreadonly structとして定義する
public readonly struct Point { public int X { get; } }
```

## パフォーマンス・セキュリティ上の注意点

- readonly struct化すると、`in`パラメータ渡しの際にコンパイラが防御的コピーを省略できる
- 通常のstructを`in`で渡しても、readonly指定が無いと変更されない保証がなく防御的コピーが発生する場合がある
- 不変性が保証されるため、複数スレッドから読み取り専用で安全に共有できる

## 理解度チェックリスト

- [ ] readonly structが何を保証するか説明できる
- [ ] 通常のstructとreadonly structの違いを説明できる
- [ ] readonly structを書ける
- [ ] 防御的コピーとは何か、なぜ発生するか説明できる
- [ ] readonly structを使うべき場面を判断できる
