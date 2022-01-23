# MeatGenerator - Pseudo meat images generator

疑似ロース芯画像ジェネレータ

## 新しい機能
採点機能を実装してみました．

[Meat Generator V2.0](https://japanicmj-meatgenerator.firebaseapp.com/)

表示されるまで10秒ほどお待ちください．
それ以上待っても表示されなかったら，どこかがおかしいので連絡をください．

### 採点について
このプログラムでは実際の採点方法を真似しています．

ユーザーが入力した数列(2341など)と生成した画像から生成した答えの数列(1234など)を比較して，
- スプリットの算出
- スプリットを使用した採点
をして，点数を表示します．

スプリットとは2者比較した際の点数差，のようなもので，それを使って減点方式で最終的な点数を決定します．

スプリットとか採点方法については
[テキサスA&MのエクステンションセンターのPDF](https://texas4-h.tamu.edu/wp-content/uploads/2015/09/photo_judging_contest_reasons2.pdf)
をご覧ください．

採点のポイントは，見分けにくいペアでは減点は小さくが，見分けやすいペアでは減点は大きく，実行されるということです．

__見分けやすいペアを落とさずに取っていく__

ことがジャッジングセオリーです．

## このリポジトリについて

ミートジャッジングの練習用の、胸最長筋の脂肪交雑のような模様を出力するプログラムです。

楕円面積、色、白い点の数と大きさのそれぞれに一定の範囲内で乱数を与え、描画しています．
白い点を脂肪交雑、とみなしてほしいのですが、これはフィルタを使用して脂肪交雑っぽく表現しています．
本当はチューリングパターンを用いていい感じの時間発展のところで止めて脂肪交雑っぽくしよう、という計画でした.(将来実装？)

- Areaは楕円の面積です．
- Beef Color Standard (BCS)は一応日本食肉格付け協会のBCS1－8までの範囲でランダムに生成しています．おおよそスタンダードに近い色が出ているのではないかと思います。
- Beef Marbling Standard (BMS)は楕円の面積と白く塗りつぶされている範囲の面積を算出し，その比を計算して推定しています．

[口田先生の論文](https://www.jstage.jst.go.jp/article/chikusan1924/68/9/68_9_853/_article/-char/ja/)
を参考に面積比からBMSを計算しています。

論文ではBMSを説明変数としたとき面積比は
![分布 bms vs ロース芯内脂肪面積比](https://user-images.githubusercontent.com/47586322/149666629-f2e647a6-e9ce-4e91-b6cc-fbfb4ca0f16f.png)

y= 4.418x + -7.184
決定係数 R^2：  0.98

となるようです．

逆推定
x = (y+7.184)/4.418
をしてみて，面積比からBMSを推定します．

## 支えていただいているツール
- [ASP.NET](https://dotnet.microsoft.com/en-us/apps/aspnet)
- [shimatさんのOpenCvShapr](https://github.com/shimat/opencvsharp)
- [firebase](https://firebase.google.com/)
