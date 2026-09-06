---
title: "readonly structの基本"
category: "文法"
order: 24
prerequisites:
  - file: struct-basic.md
    note: structが値型で、代入や引数渡しのたびにコピーされることが分かっていれば十分です
next:
  - file: immutable.md
    note: readonly structで学んだ考え方を、クラスやrecordを含む設計全体の不変性へ広げられます
related:
  - class-struct-record-basics.md
---

## この記事でわかること

- readonly structが何を保証する構文か説明できる
- 可変なstructがどんな落とし穴を生むか説明できる
- 防御的コピーとは何か、なぜ発生するかを説明できる
- readonly structを自分で書ける

## 一言でいうと

readonly structとは、一度作ったら中身を書き換えられない箱です。

宅配便の伝票を思い浮かべてください。伝票は一度発行すると、宛先や中身をあとから書き換えることはできません。届け先を変えたいときは、古い伝票を破棄して新しい伝票を発行し直します。プログラムの世界でも、readonly structのインスタンスは生成した瞬間に値が確定し、あとから書き換えることができません。値を変えたいときは、新しいインスタンスを作り直します。ただし実際の伝票と違って、readonly structは「書き換えようとするコード」自体をコンパイラが見つけてエラーにしてくれる点が便利です。

## どんなときに困るのか

structを「小さいクラスのようなもの」として気軽に扱っていると、可変なプロパティやメソッドを持つstructをつい書いてしまいます。あるとき、座標を表すPointというstructをメソッドの引数で受け取り、その中で値を変更するメソッドを呼んだとします。structは値型なので、渡された時点でコピーが作られていることがあり、変更したつもりの値が呼び出し元にも呼び出し先自身にも反映されていない、という事態が起こります。しかもこの変更漏れは実行時にエラーにならず、値を出力して初めて気づくため、原因を探すのに時間がかかります。

readonly structを使うと、そもそも中身を書き換えるコード自体がコンパイルエラーになります。書いた瞬間に「このstructは変更できない」と分かるので、実行してから悩む時間がなくなります。

## 動かしてみる

```csharp
// readonly structで座標を表す
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

`Point`には`readonly`という修飾子が付いています。これにより`X`と`Y`は`get`しか持てず、コンストラクタで一度設定した値は、あとから書き換えられません。試しにこのコードの下に`point.X = 100;`という行を足してみると、「プロパティ`Point.X`は読み取り専用のため代入できない」という内容のコンパイルエラー（CS0200）になります。readonly structは、こうした書き換えを実行前の段階で止めてくれます。

## 仕組み

readonly structが何を保証しているかを、もう少し詳しく見ていきます。`readonly struct`と宣言すると、すべてのプロパティは`get`のみか`init`しか持てなくなり、フィールドを直接書き換える文もコンパイルエラーになります。つまり、インスタンスが持つデータは、コンストラクタで設定された後は一切変わりません。

ここで、structをメソッドに渡すときの話をします。structの引数は既定では値渡し（コピー）ですが、大きなstructだとコピーのコストが気になります。そこで`in`パラメータという渡し方があります。`in`を付けた引数は、コピーせずに元の値への参照を渡しながら、「このメソッドの中では変更しない」という約束をコンパイラと交わす渡し方です。

問題は、渡されたstructが可変（readonly structでない）な場合です。コンパイラは、そのstructのメソッドが内部でフィールドを書き換えていないかまでは判断できません。そこで安全のために、そのメソッドを呼ぶ直前にこっそりコピーを作り、そのコピーに対してメソッドを呼び出します。この自動で作られる一時的なコピーを防御的コピーと呼びます。防御的コピーは気づかないところで発生するため、意図しない動作の原因になります。

readonly structだと、コンパイラは「このstructは絶対に変更されない」と分かっているので、この防御的コピーを作る必要がありません。安心して参照のまま扱えるからです。

- readonly structでは、すべてのプロパティを`get`専用または`init`にする必要がある
- readonly struct内でフィールドやプロパティを直接書き換える文は書けない
- 可変なstructを`in`パラメータで渡すと、呼び出し先で防御的コピーが発生することがある
- readonly structなら、コンパイラは防御的コピーを省略できる

## 実務での使い方

上のPointを使います。座標のような小さい値をメソッドに何度も渡す場面では、`in`パラメータと組み合わせるのが定番です。

```csharp
double CalcDistance(in Point a, in Point b)
{
    var dx = a.X - b.X;
    var dy = a.Y - b.Y;
    return Math.Sqrt(dx * dx + dy * dy);
}

var origin = new Point(0, 0);
var target = new Point(3, 4);
Console.WriteLine(CalcDistance(in origin, in target));
```

```
5
```

`Point`はreadonly structなので、`in`で渡してもコンパイラは防御的コピーを作らずに済みます。呼び出す側もコピーのコストを心配せず、そのまま値を渡せます。

- 座標・金額・日付範囲のような、生成後に値を変えない小さな型はreadonly structにする
- 同じstructを何度もメソッド引数として渡すコードでは、readonly structと`in`パラメータの組み合わせを検討する

## よくある間違い

### readonly structにsetプロパティを混在させてしまう

readonly structのつもりで書いていても、うっかり`set`を持つプロパティを混ぜるとコンパイルエラーになります。readonly structは「すべてのメンバーが変更不可」であることが前提のため、一部だけ可変にすることは許されません。

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

```csharp
// OK: すべてinitまたはコンストラクタ経由でのみ値を設定する
var point = new Point(1, 2);
Console.WriteLine(point.X);

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

### 可変なstructをinで渡し、変更したつもりになる

readonly structではない可変なstructを`in`パラメータで渡し、その中で状態を変えるメソッドを呼び出すと、防御的コピーに対して変更が実行されます。コンパイルエラーにはならないため、呼び出したメソッドの中でさえ、期待した変更が反映されていないことに気づきにくいという問題があります。

```csharp
// NG: 可変なCounterをinで渡し、Incrementで変更したつもりになる
void ShowAfterIncrement(in Counter counter)
{
    counter.Increment(); // 防御的コピーに対して実行される
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

`Increment()`を呼んだのに`Value`が`0`のままなのは、`counter.Increment()`がコンパイラの作った防御的コピーに対して実行され、パラメータ`counter`自体は変わらないためです。

```csharp
// OK: 変更ではなく、新しい値を作って返す設計にする
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

readonly structにすると、そもそもフィールドを書き換えるメソッドを定義できません。値を変えたいときは、新しいインスタンスを作って変数に代入し直す形にするしかなく、書き換えたつもりで変わっていないという事故が起きなくなります。

## 用語まとめ

| 用語 | 意味 |
|---|---|
| readonly struct | 生成後にすべてのフィールド・プロパティが変更できないことをコンパイラが保証するstruct |
| 防御的コピー | 可変なstructを`in`パラメータなどで渡した際、変更を防ぐためにコンパイラが自動で作る一時的なコピー |
| in パラメータ | コピーせず値への参照を渡しつつ、メソッド内で変更しないことを約束する引数の渡し方 |

## 理解度チェック

- [ ] readonly structが何を保証するか説明できる
- [ ] 可変なstructをメソッド越しに変更しようとして起きる落とし穴を説明できる
- [ ] 防御的コピーとは何か、なぜ発生するかを説明できる
- [ ] readonly structにするとなぜ防御的コピーを避けられるか説明できる
- [ ] readonly structを自分で書ける
- [ ] readonly structを使うべき場面を判断できる
