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
SaveChanges、Transactionのroll back機能は、シングルスレッド環境で競合が発生し、スレッドセーフではありません。<br>
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
この機能の開発中、JavaScriptの使用練習しました。<br>
特にfetchとある関数で、HTTP請求をして、非同期の返信の動的処理など、よく練習しました。<br>
<br>
***
縮小画像リストのデータがストリーミング的に転送：<br>
<br>
ネット速度低い、帯域幅狭い、リストの数が多いなど状況で、画像リストのページを開き遅くなる。<br>
スムーズに閲覧するため、画像リストのページのデータは、ストリーミングで転送される方法しました。<br>
![describe1](https://github.com/ckfckf0730/CloudServiceTest/blob/main/readme/DataStreaming.gif)<br>
<br>
***
WebGlに基づいて、3Dレンダリング基本の開発：<br>
<br>
各行列の変換、光の効果、カメラの配置など、基本な機能の3Dレンダリング・モジュール作りました。
![describe1](https://github.com/ckfckf0730/CloudServiceTest/blob/main/readme/Rendering01.gif)<br>
<br>
DirectX 12と比べると、WebGLは初期化が大幅に簡略化されています。<br>
例えば、ハードウェア情報を調べたり、Direct用のウィンドウ情報を初期化したりする必要がありませんし、フェンスやコマンドリストを書く必要もありません。<br>
また、頂点バッファの作成や、ルートパラメータに相当する設定も簡潔です。<br>
<br>
しかし、欠点もあります。<br>
例えば、頂点情報を複雑な構造体で扱うことができず、基本的なfloat配列に変換する必要があります。<br>
また、右手座標系の使用も面倒です。<br>
使いやすいため、そしてDirectXのプロジェクトとよりよく同期させるために、<br>
上層のロジックでは左手座標系を使用して計算し、最終的に下層のロジックで左手座標系を右手座標系に変換してGLSLに渡すようにしています。<br>
<br>
***
WebGlに基く3Dレンダリングモジュールを使って、簡単な機能実現：<br>
<br>
Unityエンジンで、静的なSceneを構築、そのGameObjectのJsonデータ記録して、本プロジェクトに使用します。<br>
![describe1](https://github.com/ckfckf0730/CloudServiceTest/blob/main/readme/UnityScene.png)<br>
<br>
本プロジェクトが同じ大きさ、同じデフォルト方向のCube、Quadモデルを準備しています。<br>
UnityのSceneデータによって、同じ現場が作られます。<br>
さらに、UnityのGold Meterialを使ったGameObjectが、remarkの説明を記録して、本プロジェクトでこのモデルがサーバーからのAzure画像展示します。<br>
![describe1](https://github.com/ckfckf0730/CloudServiceTest/blob/main/readme/Rendering02.gif)<br>
<br>
<br>
***
オンライン動画ストリーミングの実現：<br>
<br>
前は画像のAzure保存と閲覧を作りましたが、今回その機能について、動画のAzure保存と放送を実現します。<br>
<br>
まずAzureのshare folderが一つのファイル4M以下しかアップロードできません。<br>
大きい動画ファイルをいくつ分割、share folderに送るのはできるが、<br>
AzureのBlob機能を使うと、この部分はもっと簡単です。<br>
最後Azureにアップロードした動画のSasUriを求めて、Htmlの"video"コンポーネント直接放送できます。
![describe1](https://github.com/ckfckf0730/CloudServiceTest/blob/main/readme/VideoStreaming.gif)<br>
<br>
<br>
***
ASP.NET Coreウェブサーバーに接続するAndroid Appの開発：<br>
<br>
ASP.NET Coreの基本的な開発技術は大体身に付けました。<br>
次はJavaとAndroidの開発を学びたいと思います。<br>
<br>
ASP.NET Coreはブラウザとアプリの両方に対応しているため、マルチプラットフォーム開発は技術向上に役立つと考えています。<br>
そして、Android StudioでAndroidプロジェクトを構築しました。<br>
GitHub URL: https://github.com/ckfckf0730/AndroidApp<br>
<br>
最初はAndroid Studioのプロジェクト構造を理解し、ウェブサーバーとのHTTP接続方法を学びます。<br>
HTTPSのウェブサイトではセキュリティ証明書が必要ですが、勉強用のプロジェクトには証明書がないため、まずそのセキュリティを無効にする必要があります。<br>
<br>
接続に成功したら、デフォルトページのHTMLデータが取得できます。<br>
![describe1](https://github.com/ckfckf0730/CloudServiceTest/blob/main/readme/AndroidApp01.png)<br>
<br>
<br>
***
Android AppのCookie処理：<br>
<br>
ASP.NET Coreのフレームワークは、サーバーとウェブページの間で、Cookieの通信と記録を自動的、暗黙的に処理しました。<br>
でもAndroid Appは自動的にCookieの処理できません。<br>
<br>
まず、サーバーの方がAndroidのCookie設置しなければならない。<br>
![describe1](https://github.com/ckfckf0730/CloudServiceTest/blob/main/readme/SetCookie.png)<br>
<br>
そして、okhttp3.CookieJar関するライブラリを使って、Cookieの受け、使い、保存をかんせいしました。<br>
<br>
<br>
***
Azureの画像アップロード、画像のリストの観覧、削除：<br>
<br>
ASP.NET CoreのIDEは一部のHttp操作を簡略化しました。<br>
例え、HTMLの"form"コンポーネントがHttpPostリクエストを適切にラップし、サーバーに自動的に送信することができます。
Android Appは自らそのリクエストをしょりしなければできません。
そしてAndroid Studioの仕組みを勉強しながら、Httpプロトコルもっと深く学んでいます。<br>
<br>
今Azureの画像の機能関するActiviyを完成しました。<br>
![describe1](https://github.com/ckfckf0730/CloudServiceTest/blob/main/readme/AndroidApp01.gif)<br>