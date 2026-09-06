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
`pages/csharp/.claude/skills/csharp-article/` のskillで生成する運用）。新しいジャンルの
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
