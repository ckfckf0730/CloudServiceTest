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
AspNetCore.SignalR<br>
<br>
アップロードされたリストのページを展示します：<br>
![describe1](https://github.com/ckfckf0730/CloudServiceTest/blob/main/readme/presentation01.png)<br>
<br>
***
実装された機能と学習体験：<br>
<br>
***
地元データベースの構築と応用：<br>
<br>
Microsoft SQL Serverをインストールし、プロジェクトとこのデータベースのリンクを設定します。<br>
Asp.NET CoreのEntity Framework Code Firstの特徴を活用し、C#でデータ形式を構築するだけで、<br>
コンソールのAdd-MigrationおよびUpdate-Databaseコマンドを使って、データベースに自動的にテーブル形式を構築できます。<br>
<br>
***
アカウントの登録とログイン：<br>
<br>
Microsoft.AspNetCore.Identityがアカウントの機能はほぼ全部揃えました。<br>
アカウントのユーザーネームはフォルト形式がメールアドレスのです。<br>
でも本当のメールアドレスかないか、確認しなければならないです。<br>
そしてsmptのサービス利用して、自動的に登録したユーザへ確認メール送ります。<br>
注意事項はsmptを使用メールは、元のパスワード使わず、Application passwordが申し込み必要がある可能です。<br>
![describe1](https://github.com/ckfckf0730/CloudServiceTest/blob/main/readme/ConfirmEmail.png)<br>
<br>
***
Azureクラウドサービスのイメージアップロードと関連支援機能：<br>
<br>
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
<br>
Azure Computer Visionが画像のTag自動的に生成できる。<br>
ユーザがアップロードした画像のTagデータをデータベースに保存して、適当な時、関する内容のプロモーションができる。<br>
最初はAmazonのProduct Advertising APIを使用、商品の販売インフォメーション展示したいが、<br>
そのアカウントの登録が、実の店舗が必要らしいです。<br>
そして今Bing Searchを使用して、関連のインターネット画像表されます。<br>
<br>
***
Async Taskの使用、およびスレッドの安全：<br>
<br>
Unityゲーム開発の仕事中、Async Taskの使用はいくつの欠点がある、あまり使用ことがなっかた。<br>
Async Taskの直接の使用では、CallBack関数がサブスレッドから呼び出されます。<br>
UnityのLifeCycleはメインスレッド向けで、サブスレッドのCallBackは役立たないで、<br>
自作のマルチスレッド・モジュールを選んで、LifeCycle関数内のCallBack設計しました。<br>
<br>
でもAsp.net core mvcプロジェクトは、Async Task設計の向けます。<br>
各ControllerのデータmodelのScopedは確保できます。<br>
できれば、Taskのよく使用してはいいです。<br>
<br>
注意事項は、依存性注入されるサービスは、Task使用の時、スレッド安全が重要です。<br>
例え、このプロジェクトには、DatabaseServiceの使用時、<br>
SaveChanges、Transactionのroll back機能は、マルチスレッド環境で競合が発生し、スレッドセーフではありません。<br>
そのためDatabaseServiceの依存性注入はSingletonではなく、Scoped又はTransientを選ばなければなりません。<br>
他のサービスはまだ競合が発生するロジックが見えないが、もし実際発表のプロジェクトなら、よくテストしなければなりません。<br>
![describe1](https://github.com/ckfckf0730/CloudServiceTest/blob/main/readme/ServiceInjection.png)<br>
<br>
***
Chat機能の実現：<br>
<br>
Chatページとlayoutのフローティング・ウィンドウ両方も、この機能実現しました。
![describe1](https://github.com/ckfckf0730/CloudServiceTest/blob/main/readme/Chat01.png)<br>
![describe1](https://github.com/ckfckf0730/CloudServiceTest/blob/main/readme/Chat02.png)<br>
Microsoft.AspNetCore.SignalR使用して、非chatページでサーフィンする時、他のユーザにメッセージをもらいましたら、<br>
フローティング・ウィンドウが出て、新しいメッセージが着信したのを通知し、その内容のご覧と送信もできる。<br>
メッセージの内容は、データベースに探します。<br>
<br>
***
縮小画像リストのデータのストリーミング：<br>
<br>
ネット速度低い、帯域幅狭い、リストの数が多いなど状況で、画像リストのページを開き遅くなる。<br>
スムーズに閲覧するため、画像リストのページのデータは、ストリーミングで転送される方法しました。<br>
![describe1](https://github.com/ckfckf0730/CloudServiceTest/blob/main/readme/DataStreaming.gif)<br>
<br>
<br>