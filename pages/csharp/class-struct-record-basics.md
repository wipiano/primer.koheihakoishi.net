---
title: "class / struct / record の使い分け"
category: "文法"
order: 20
related: []
---

## 概要

- class / struct / record は型を定義するキーワードだが、メモリ配置と等価性の扱いが異なる
- データの入れ物（DTO・値オブジェクト）を設計する場面で、どれを選ぶかが実務でよく問われる
- 読了後、参照型と値型の違いを理解し、用途に応じて3つを使い分けられるようになる

## なぜ必要なのか

- 選択を誤ると、意図しない値のコピーや、想定外の等価判定バグにつながる
- 適切に使い分けると、不要なアロケーションを避けつつバグの少ない等価比較ができる
- record登場以前は、値の等価性が欲しい場合にEquals/GetHashCode/==を手動でオーバーライドする必要があった

## 仕組みと動作原理

- **参照型**: インスタンスはヒープに置かれ、変数はその参照（アドレス）を保持する。`class`と既定の`record`が該当
- **値型**: インスタンスの値そのものを変数が保持し、代入時に値がコピーされる。`struct`と`record struct`が該当
- **等価性**: `class`は既定で参照等価（同一インスタンスか）。`record`は既定で値等価（全プロパティが一致するか）
- `record`は「class/structに値等価・ToString・with式を自動生成する」コンパイラ機能
- `record`単体は`record class`と同義。値型として使いたい場合は`record struct`と明示する
- structは代入・引数渡しでコピーされるため、コピー先での変更は元のインスタンスに影響しない

## 基本的な書き方とコード例

- 最小の使い方

```csharp
public class Person
{
    public string Name { get; set; } = "";
    public int Age { get; set; }
}

public struct Point
{
    public int X { get; set; }
    public int Y { get; set; }
}

public record User(string Name, int Age);
```

- よく使うバリエーション: recordのwith式とrecord struct

```csharp
var user1 = new User("Taro", 20);
var user2 = user1 with { Age = 21 }; // 一部だけ変更した新しいインスタンス

Console.WriteLine(user1 == user2); // False（値は違う）
Console.WriteLine(user1 with { } == user1); // True（全プロパティ一致）

public record struct Vector2(float X, float Y); // 値型のrecord
```

- 使い分けの判断基準
  - 可変な状態を持ち、参照を共有したい → `class`
  - 小さく不変な値（座標・金額など）で、コピーされても困らない → `struct`
  - 等価性が「中身の一致」で決まるDTO・値オブジェクト → `record`（既定は`record class`）
  - 小さいrecordで値型のコピー特性も欲しい → `record struct`

## よくある誤用・バグ

- **可変なstructによる意図しない挙動**
  - structは値としてコピーされるため、コレクション経由での変更が反映されない

```csharp
// NG: リストの要素を直接変更したつもりが反映されない
var points = new List<Point> { new Point { X = 1, Y = 1 } };
points[0].X = 100; // 挙動が直感に反する、または構文的に扱いづらい
```

```csharp
// OK: 新しい値で丸ごと置き換える
var points = new List<Point> { new Point { X = 1, Y = 1 } };
points[0] = new Point { X = 100, Y = points[0].Y };
```

- **classでの等価比較の誤解**
  - `class`は既定で参照等価のため、中身が同じでも`==`はfalseになる

```csharp
// NG: 中身が同じでも別インスタンスならfalse
public class Money
{
    public int Amount { get; init; }
}
var a = new Money { Amount = 100 };
var b = new Money { Amount = 100 };
Console.WriteLine(a == b); // False
```

```csharp
// OK: 値の一致で比較したいならrecordにする
public record Money(int Amount);
var a = new Money(100);
var b = new Money(100);
Console.WriteLine(a == b); // True
```

## パフォーマンス・セキュリティ上の注意点

- structが大きくなるほどコピーコストが増える。目安として16バイト程度を超えたらclass化を検討する
- structをinterface型の変数に代入・キャストするとボックス化が発生し、ヒープ確保とGC負荷が生じる
- ボックス化を避けたい場合は、structをinterface経由で扱わずジェネリクスで直接型を扱う

## 理解度チェックリスト

- [ ] class・struct・recordの代入時の挙動の違いを説明できる
- [ ] classとrecordの等価比較（==）の既定動作の違いを説明できる
- [ ] record structがどのような場面で有効か説明できる
- [ ] with式を使ってrecordの一部プロパティだけ変更したインスタンスを作れる
- [ ] 可変なstructがバグの原因になりやすい理由を説明できる
- [ ] structのボックス化がいつ発生するかを説明できる
- [ ] DTOや値オブジェクトを設計する際にどの型を選ぶか判断できる
