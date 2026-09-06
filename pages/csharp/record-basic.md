---
title: "レコード(record)の基本"
category: "文法"
order: 21
related:
  - class-basic.md
  - equality.md
  - immutable.md
  - class-struct-record-basics.md
---

## 概要

- recordはclass/structに「値の等価性」「ToString」「with式」を自動生成するコンパイラ機能
- DTOや値オブジェクトなど、中身の一致で比較したいデータの定義に向く
- 読了後、recordの基本構文と自動生成される機能を理解できる

## なぜ必要なのか

- classで値の等価性がほしい場合、Equals/GetHashCode/==を毎回手動実装するのは手間が多くミスも起きやすい
- recordを使えば、1行の宣言で値等価・ToString・不変プロパティ・with式がまとめて手に入る
- 手動実装との比較では、記述量が大幅に減りバグの入り込む余地も減る

## 仕組みと動作原理

- `record`は既定で参照型（`record class`と同義）。値型がほしい場合は`record struct`を使う
- 位置引数構文（`record User(string Name, int Age)`）で書くと、プロパティは既定で`init`（生成後変更不可）になる
- コンパイラが`Equals`・`GetHashCode`・`==`・`!=`・`ToString`・`with`式を自動生成する
- `with`式は「既存インスタンスの一部プロパティだけ変えた新しいインスタンス」を作る構文

## 基本的な書き方とコード例

- 最小の使い方

```csharp
public record User(string Name, int Age);

var user = new User("Taro", 20);
Console.WriteLine(user); // 出力: User { Name = Taro, Age = 20 }
```

- with式で一部だけ変更

```csharp
var user = new User("Taro", 20);
var older = user with { Age = 21 };

Console.WriteLine(user == older); // False（Ageが異なる）
Console.WriteLine(user with { } == user); // True（全プロパティ一致）
```

- 使い分けの判断基準
  - 値の一致で比較したいデータ（DTO・値オブジェクト）にはrecordを使う
  - 可変な状態や振る舞いを持つドメインオブジェクトにはclassを使う

## よくある誤用・バグ

- **配列やListプロパティを持つと値等価が期待通り働かない**
  - recordの自動生成する等価性は各プロパティの`Equals`を呼ぶだけで、配列・Listは参照比較になる

```csharp
// NG: 配列プロパティは中身が同じでも参照が違うと等しくならない
public record Order(string Id, string[] Items);

var a = new Order("1", new[] { "apple" });
var b = new Order("1", new[] { "apple" });
Console.WriteLine(a == b); // False（配列は参照比較）
```

```csharp
// OK: 値等価が必要なら独自にEqualsを実装する
public record Order(string Id, IReadOnlyList<string> Items)
{
    public virtual bool Equals(Order? other) =>
        other is not null && Id == other.Id && Items.SequenceEqual(other.Items);
    public override int GetHashCode() => Id.GetHashCode();
}
```

- **recordを可変にしてしまう**
  - `set`を使うとrecordの「不変で値の一致を保証する」という前提が崩れる

```csharp
// NG: recordなのにsetでいつでも変更できてしまう
public record User
{
    public string Name { get; set; } = "";
}
```

```csharp
// OK: 位置引数構文かinitで不変にする
public record User(string Name);
```

## パフォーマンス・セキュリティ上の注意点

- 自動生成される等価性は全プロパティを比較するため、プロパティ数や参照先が大きいと比較コストが増える
- 自動生成されるToStringは全プロパティを出力するため、機密情報を持つrecordでは明示的にToStringをオーバーライドして除外する

## 理解度チェックリスト

- [ ] recordがclassやstructとどう違うか説明できる
- [ ] recordの位置引数構文でプロパティが既定でinitになることを説明できる
- [ ] with式で一部プロパティだけ変えたインスタンスを作れる
- [ ] 配列・Listプロパティを持つrecordの等価性の落とし穴を説明できる
- [ ] recordを可変にすべきでない理由を説明できる
