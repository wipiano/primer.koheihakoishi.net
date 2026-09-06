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

`class` / `struct` / `record` はどれも独自の型を定義するキーワードだが、代入したときの挙動・`==`の既定・書き換え可否がそれぞれ違う。`struct`が値型としてコピーされる仕組みは「構造体(struct)の基本」、`record`の値等価と`with`式は「レコード(record)の基本」で説明した通りなので、ここでは3つを並べて比較し、使い分けの判断基準だけを扱う。

| 観点 | `class` | `struct` | `record` |
|---|---|---|---|
| 代入したときの挙動 | 実体を共有する(参照のコピー) | 値ごとコピーされる | 既定は`class`と同じ参照型。値型にしたいなら`record struct` |
| `==`の既定 | 参照等価(同じ実体かどうか) | 定義されていない。値の比較には`Equals`を使う | 値等価(全プロパティが一致すればtrue) |
| 書き換え可否 | `set`のあるプロパティは生成後も書き換え可能 | `set`のあるプロパティは書き換え可能。影響はコピー先だけ | 既定は`init`専用で生成後は書き換え不可。`with`で新しいインスタンスを作る |

## 動かしてみる

```csharp
var c1 = new PointClass { X = 1, Y = 1 };
var c2 = c1;
c2.X = 100;
Console.WriteLine(c1.X);
Console.WriteLine(c1 == c2);

var s1 = new PointStruct { X = 1, Y = 1 };
var s2 = s1;
s2.X = 100;
Console.WriteLine(s1.X);
Console.WriteLine(s1.Equals(s2));

var r1 = new PointRecord(1, 1);
var r2 = r1 with { X = 100 };
Console.WriteLine(r1.X);
Console.WriteLine(r1 == r2);

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

`c2`は`c1`と同じ実体を指すので、`c2.X`を変えると`c1.X`も100になり、`==`も参照等価でtrueになる。`s2`は`s1`の値をコピーして作られるため書き換えても`s1`は1のままだが、`struct`は`==`演算子を既定で持たないので値の比較には`Equals`を使う。`record`のプロパティは既定で`init`専用のため直接書き換えられず、`with`で新しいインスタンスを作る。値が異なる`r1`と`r2`を比較すると、`record`の`==`は値等価なのでfalseになる。

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
