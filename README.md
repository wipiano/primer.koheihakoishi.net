# primer.koheihakoishi.net

`pages/` 以下の技術解説記事（Markdown + frontmatter）から静的サイトを生成するリポジトリ。

## 構成

```
pages/                        記事の原稿（Markdown + frontmatter）
  csharp/                      ジャンル別ディレクトリ（例: C#入門）
    _directory.yml             このディレクトリの表示タイトル
    *.md                       記事本体
tools/SiteGenerator/           静的サイトジェネレータ本体（.NET コンソールアプリ）
dist/                          生成された静的サイト（gitignore対象、CIが都度生成）
.github/workflows/deploy.yml   GitHub Pages への自動デプロイ
CNAME                          GitHub Pages のカスタムドメイン設定
```

## サイトジェネレータ

`tools/SiteGenerator` は ASP.NET Core の Razor テンプレートエンジン（RazorLight）を
コンソールアプリから利用して、`pages/` 以下のMarkdownを次の2種類の静的ファイルに変換する。

- `*.html` … 通常の閲覧用ページ（コードブロックはクライアントサイドで highlight.js によりハイライトされる）
- `*.md` … AI／機械可読向けに、同じ内容をMarkdownのまま出力したページ（関連記事セクション付き）

ルートの `index.html` は各ジャンルディレクトリへのリンク一覧、各ジャンルの `index.html` は
frontmatterの `category` ごとにグループ化し `order` 昇順に並べた記事一覧を表示する。
各記事には `(order - 1) / 20` から求まる5段階の難易度（★表示）が付く。

### ローカルでの実行

```bash
dotnet run --project tools/SiteGenerator
# dist/ に生成される
```

引数で入出力先を指定することもできる:

```bash
dotnet run --project tools/SiteGenerator -- --pages ./pages --out ./dist
```

### 記事を追加する

`pages/<ジャンル>/*.md` にfrontmatter付きMarkdownを追加するだけでよい（C#記事は
`pages/csharp/.claude/skills/csharp-article/` のskillで生成する運用）。

frontmatterのキーは次の6つ。記事同士のリンクはすべてfrontmatterで表現し、本文中には書かない
（ジェネレータはMarkdown本文中のリンクを解決しないため）。

```yaml
title: "記事タイトル"
category: "文法"          # ディレクトリ内の目次でグループ化するカテゴリ名
order: 24                 # 1〜100。難易度と目次内の並び順
prerequisites:            # 先に読んでおくとよい記事。本文の前に「前提知識」として表示される
  - file: class-basic.md
    note: クラスと参照型の基本が分かっていれば十分です
next:                     # 次に読むとよい記事。本文の後に「次に読む」として表示される
  - file: readonly-struct.md
    note: structを不変にして安全に使う方法
related:                  # 前提でも次でもない関連記事。本文の後に「関連記事」として表示される
  - equality.md
```

`prerequisites` / `next` / `related` の参照先が同じディレクトリに見つからない場合、
および前提記事の `order` が本記事以上の場合は、生成時に警告を出す（ビルドは止めない）。新しいジャンルの
ディレクトリを追加した場合は、そのディレクトリ直下に `_directory.yml` を置いてタイトルを指定する:

```yaml
title: "表示したいタイトル"
```

## デプロイ

`main` ブランチへのpushで `.github/workflows/deploy.yml` が実行され、
サイトを生成してGitHub Pagesに公開する。事前に一度だけ、GitHubリポジトリの
Settings → Pages → Source を「GitHub Actions」に設定しておく必要がある。

カスタムドメインは `CNAME` ファイル（内容: `primer.koheihakoishi.net`）で指定している。
ドメインが異なる場合はこのファイルを書き換える。またドメイン側のDNSに
GitHub Pages を指すレコード（A/AAAA または CNAME）を設定しておくこと。
