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
***
実装された機能と学習体験：<br>
<br>
***
本地データベースの構築と応用：<br>
<br>
Microsoft SQL Serverをインストールし、プロジェクトとこのデータベースのリンクを設定します。<br>
Asp.NET CoreのEntity Framework Code Firstの特徴を活用し、C#でデータ形式を構築するだけで、<br>
コンソールのAdd-MigrationおよびUpdate-Databaseコマンドを使って、データベースに自動的にテーブル形式を構築できます。<br>
<br>
***
アカウントの登録とログイン：<br>
Microsoft.AspNetCore.Identityがアカウントの機能はほぼ全部揃えました。<br>
アカウントのユーザーネームはフォルト形式がメールアドレスのです。<br>
でも本当のメールアドレスかないか、確認しなければならないです。<br>
そしてsmptのサービス利用して、自動的に登録したユーザへ確認メール送ります。<br>
注意事項はsmptを使用メールは、元のパスワード使わず、Application passwordが申し込み必要がある可能です。<br>
![describe1](https://github.com/ckfckf0730/CloudServiceTest/blob/main/readme/ConfirmEmail.png)<br>
<br>
***
Azureクラウドサービスのイメージアップロードと関連支援機能：<br>
Azureのサイトにストレージのサービス申し込み、接続の配置などしたら、ファイルのアップロードができる。<br>
![describe1](https://github.com/ckfckf0730/CloudServiceTest/blob/main/readme/AzureStorageList.png)<br>
拡張子が変えやすいので、画像のみアップロードできるため、ファイルの4バイとのheader分析し、ファイルしが画像かないか分かれます。<br>
SixLabors.ImageSharp使用して、縮小画像作ります。<br>
元画像と縮小画像をAzureに転送します。<br>
Azure画像のGuid、所有者のGuidなど、全てのインフォメーションが地元のデータベースに保存します。<br>
![describe1](https://github.com/ckfckf0730/CloudServiceTest/blob/main/readme/Database01.png)<br>
そしてユーザがアップロードされた画像をチェックしたい時、複数の縮小画像を画面に展示して、次の操作用意されます。<br>
<br>
***
Azureの画像分析AI使用Tag生成と、好み内容のプロモーション：<br>
Azure Computer Visionが画像のTag自動的に生成できる。<br>
ユーザがアップロードした画像のTagデータをデータベースに保存して、適当な時、関する内容のプロモーションができる。<br>
最初はAmazonのProduct Advertising APIを使用、商品の販売インフォメーション展示したいが、<br>
そのアカウントの登録が、実の店舗が必要らしいです。<br>
そして今Bing Searchを使用して、関連のインターネット画像表されます。<br>
<br>
<br>
<br>
