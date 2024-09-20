Azure Serviceを利用するAsp.net core webの画像保管サイトプロジェクトです。<br>
<br>
私の仕事の経験とこれまでの学習は、主にゲームプログラミングとグラフィック開発に関連する技術分野に集中しています。<br>
特に、基礎的なレンダリング技術とフレームワークのコアに関する研究はかなり深いレベルに達しています。<br>
しかし、他のプログラミング分野の学習は非常に限られています。<br>
そこで、自分の弱点である前後端開発技術を学ぶために、ASP.NET Core Webの学習プロジェクトを設定しました。<br>
これにより、自分のスキルの幅を広げ、職業範囲を拡張したいと考えています。<br>
<br>
使用したサードパーティAPIとAzureサービス：<br>
SQL Server<br>
AspNetCore.Identity<br>
Azure Storage<br>
Azure Computer Vision<br>
Azure Bing Search<br>
SixLabors.ImageSharp<br>
<br>
アップロードされたリストのページを展示します：<br>
![describe1](https://github.com/ckfckf0730/CloudServiceTest/blob/main/readme/presentation01.png)<br>
<br>
実装された機能と学習体験：<br>
<br>
本地データベースの構築と応用：<br>
<br>
Microsoft SQL Serverをインストールし、プロジェクトとこのデータベースのリンクを設定します。<br>
Asp.NET CoreのEntity Framework Code Firstの特徴を活用し、C#でデータ形式を構築するだけで、<br>
コンソールのAdd-MigrationおよびUpdate-Databaseコマンドを使って、データベースに自動的にテーブル形式を構築できます。<br>
<br>
アカウントの登録とログイン：<br>
Microsoft.AspNetCore.Identityがアカウントの機能はほぼ全部揃えました。<br>
アカウントのユーザーネームはフォルト形式がメールアドレスのです。<br>
でも本当のメールアドレスかないか、確認しなければならないです。<br>
そしてsmptのサービス利用して、自動的に登録したユーザへ確認メール送ります。<br>
注意事項はsmptを使用メールは、元のパスワード使わず、Application passwordが申し込み必要がある可能です。<br>
<br>
<br>
<br>
