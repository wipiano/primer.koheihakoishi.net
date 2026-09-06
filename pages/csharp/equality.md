---
title: "オブジェクトの等価性を正しく実装する"
category: "文法"
order: 19
related:
  - override.md
  - interface-basic.md
  - record-basic.md
  - class-struct-record-basics.md
---

## 概要

- 等価性とは「2つのインスタンスが同じとみなせるか」を判定する仕組み
- `Equals`・`GetHashCode`・`IEquatable<T>`はセットで正しく実装する必要がある
- 読了後、値の一致で比較したい型に正しく等価性を実装できるようになる

## なぜ必要なのか

- 既定の等価性（参照等価）のままDictionaryやHashSetのキーに使うと、意図通りに検索できない
- Equals/GetHashCodeを正しく実装すれば、値の一致でコレクション検索や比較ができる
- 自前実装は手間とミスが多いため、可能な場合はrecordの自動生成に任せるのが実務的な選択

## 仕組みと動作原理

- `Equals(object)`は値の一致を判定するメソッド。既定（object由来）は参照等価
- `GetHashCode()`はハッシュテーブル（Dictionary・HashSet）で高速に検索するためのキー
- **契約**: 「Equalsがtrueを返す2つのインスタンスは、必ず同じGetHashCodeを返す」必要がある
- `IEquatable<T>`を実装すると、ボックス化なしの型安全な`Equals(T)`が使え、パフォーマンスが向上する
- `==`演算子は既定では`Equals`と連動しない。演算子オーバーロードも別途必要（recordは自動生成する）

## 基本的な書き方とコード例

- 最小の実装（class）

```csharp
public class Money : IEquatable<Money>
{
    public int Amount { get; init; }

    public bool Equals(Money? other) => other is not null && Amount == other.Amount;
    public override bool Equals(object? obj) => Equals(obj as Money);
    public override int GetHashCode() => Amount.GetHashCode();
}
```

- recordに任せる（同じ効果を1行で得る）

```csharp
public record Money(int Amount);

var a = new Money(100);
var b = new Money(100);
Console.WriteLine(a.Equals(b)); // True
Console.WriteLine(a == b); // True（==も自動生成される）
```

- 使い分けの判断基準
  - 値の一致で比較したい単純なデータはrecordに任せて自前実装を避ける
  - 既存のclass階層に等価性だけ追加したい場合はIEquatable\<T\>を手動実装する

## よくある誤用・バグ

- **GetHashCodeを実装せずEqualsだけ上書きする**
  - 契約違反となり、HashSet・Dictionaryで同じ値なのに別キー扱いされる

```csharp
// NG: Equalsだけオーバーライドし、GetHashCodeを既定のままにする
public class Money
{
    public int Amount { get; init; }
    public override bool Equals(object? obj) => obj is Money m && Amount == m.Amount;
    // GetHashCodeが未実装 → 参照ベースのハッシュのまま
}
var set = new HashSet<Money> { new Money { Amount = 100 } };
Console.WriteLine(set.Contains(new Money { Amount = 100 })); // False（意図に反する）
```

```csharp
// OK: Equalsと一致するGetHashCodeを必ずセットで実装する
public class Money
{
    public int Amount { get; init; }
    public override bool Equals(object? obj) => obj is Money m && Amount == m.Amount;
    public override int GetHashCode() => Amount.GetHashCode();
}
```

- **可変フィールドをハッシュキーに使う**
  - HashSet/Dictionaryに入れた後でハッシュ計算に使うプロパティを変更すると、要素が見つからなくなる

```csharp
// NG: ハッシュに使うプロパティをキーとして登録後に変更する
public class Item
{
    public string Name { get; set; } = "";
    public override int GetHashCode() => Name.GetHashCode();
}
var item = new Item { Name = "A" };
var set = new HashSet<Item> { item };
item.Name = "B"; // 登録後に変更
Console.WriteLine(set.Contains(item)); // False（バケット位置がずれるため）
```

```csharp
// OK: ハッシュ計算に使うプロパティは不変にする（initやrecordを使う）
public record Item(string Name);
```

## パフォーマンス・セキュリティ上の注意点

- `IEquatable<T>`を実装するとボックス化を回避でき、コレクション検索が高速になる
- recordの自動生成する等価性は全プロパティを比較するため、大きなオブジェクトやコレクションを持つ場合は比較コストが増える
- ハッシュキーに使うプロパティは登録後に変更しない（不変にする）ことで、コレクションからの検索漏れを防ぐ

## 理解度チェックリスト

- [ ] Equals/GetHashCode/IEquatable\<T\>の役割の違いを説明できる
- [ ] 「Equalsが等しいならGetHashCodeも等しい」という契約を説明できる
- [ ] recordが等価性をどのように自動生成するか説明できる
- [ ] GetHashCode未実装がHashSet/Dictionaryでバグになる理由を説明できる
- [ ] 可変フィールドをハッシュキーに使ってはいけない理由を説明できる
