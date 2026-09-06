---
title: "class / struct / record の使い分け"
category: "文法"
order: 45
prerequisites:
  - file: struct-basic.md
    note: structが代入・引数渡しのたびにコピーされる値型であることが分かれば十分
  - file: record-basic.md
    note: recordが値等価・ToString・with式を自動生成する機能であることが分かれば十分
next:
  - file: immutable.md
    note: recordのwith式を軸にした不変設計の考え方につながる
related:
  - reference-vs-value-types.md
  - equality.md
---

`class` / `struct` / `record` はどれも独自の型を定義するキーワードだが、表している概念の種類が違う。

- `class`: 同一性を持ち、生成後に状態が変わっていくもの。実体は1つで、変数はそれを指す
- `struct`: 座標や金額のような値そのもの。代入のたびにコピーされる
- `record`: 中身の値が意味のすべてであるデータ。中身が同じなら同じものとして扱う

個々の仕組みは「構造体(struct)の基本」「レコード(record)の基本」で説明した通りなので、ここでは3つを並べて比較し、使い分けの判断基準だけを扱う。

| 観点 | `class` | `struct` | `record` |
|---|---|---|---|
| 代入したときの挙動 | 実体を共有する(参照のコピー) | 値ごとコピーされる | 既定は`class`と同じ参照型。値型にしたいなら`record struct` |
| `==`の既定 | 参照等価(同じ実体かどうか) | 定義されていない。値の比較には`Equals`を使う | 値等価(全プロパティが一致すればtrue) |
| 書き換え可否 | `set`のあるプロパティは生成後も書き換え可能 | `set`のあるプロパティは書き換え可能。影響はコピー先だけ | 既定は`init`専用で生成後は書き換え不可。`with`で新しいインスタンスを作る |

## 動かしてみる

```csharp
var c1 = new PointClass { X = 1, Y = 1 };
var c2 = c1;                 // 同じ実体を指す
c2.X = 100;                  // c1.Xも100になる
Console.WriteLine(c1.X);
Console.WriteLine(c1 == c2); // 参照等価。同じ実体なのでTrue

var s1 = new PointStruct { X = 1, Y = 1 };
var s2 = s1;                 // 値がコピーされる
s2.X = 100;                  // s1.Xは1のまま
Console.WriteLine(s1.X);
Console.WriteLine(s1.Equals(s2)); // structは==を既定で持たないのでEqualsで比較

var r1 = new PointRecord(1, 1);
var r2 = r1 with { X = 100 }; // recordは書き換えられないので、withで新しいインスタンスを作る
Console.WriteLine(r1.X);
Console.WriteLine(r1 == r2);  // 値等価。Xが違うのでFalse

public class PointClass
{
    public int X { get; set; }
    public int Y { get; set; }
}

public struct PointStruct
{
    public int X { get; set; }
    public int Y { get; set; }
}

public record PointRecord(int X, int Y);
```

```
100
True
1
False
1
False
```

## 最低限の理解

- 状態を持ち、生成後も書き換えながら複数箇所で共有したいオブジェクトは`class`を選ぶ
- 座標や金額のように小さく不変な値で、コピーされても困らないものは`struct`(不変にするなら`readonly struct`)を選ぶ
- 中身の一致で比較したいDTOや値オブジェクトは`record`を選ぶ。既定は参照型で、値型として扱いたいときだけ`record struct`にする
- 迷ったときは`class`から始める。参照等価や可変な状態が問題にならない場面がほとんどだ

## ⚠️ 可変なstructを`List<T>`の要素として直接書き換える

`List<T>`の要素が`struct`のとき、`points[0]`で取り出せるのは値のコピーだ。そのプロパティにそのまま代入しようとすると、コピーへの代入とみなされコンパイルエラーになる。

```csharp
var points = new List<PointStruct> { new PointStruct { X = 1, Y = 1 } };
points[0].X = 100; // NG: List<T>の要素は値のコピーなので直接書き換えられない

public struct PointStruct
{
    public int X { get; set; }
    public int Y { get; set; }
}
```

```
error CS1612: 変数ではないため、'List<PointStruct>.this[int]' の戻り値を変更できません
```

```csharp
var points = new List<PointStruct> { new PointStruct { X = 1, Y = 1 } };
var updated = points[0];
updated.X = 100;
points[0] = updated; // OK: 新しい値を作って丸ごと置き換える
Console.WriteLine(points[0].X);

public struct PointStruct
{
    public int X { get; set; }
    public int Y { get; set; }
}
```

```
100
```

## 課題

自分が扱うデータ(注文明細、座標、ユーザー情報など)を1つ選び、`class`・`struct`・`record`のどれで定義するのが適切か理由とともに判断し、実際に定義してみる。
