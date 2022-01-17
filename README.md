# MeatGenerator - Pseudo meat images generator
C#とOpenCvSharpがWeb Appでうごく！

## 20220116 Notice
C++ で作っていたものを，blazor wasmを使って作り直し，firebase でオンライン版を公開しました．

[Meat Generator ](https://japanicmj-meatgenerator.firebaseapp.com/)

少々重いかもしれませんが，しばらくお待ちください．

## who am i 
ミートジャッジングの練習用の、胸最長筋の脂肪交雑のような模様を出力するプログラムです。

楕円面積、色、白い点の数と大きさのそれぞれに一定の範囲内で乱数を与え、描画しているだけです。
白い点を脂肪交雑、とみなしてほしいのですが、これはフィルタを使用して脂肪交雑っぽくしようとしています。
本当はチューリングパターンを用いていい感じの時間発展のところで止めて脂肪交雑っぽくしよう、という計画でしたがまったくこのフィルタは誤っているので、一応動いているのは9割9分バグです。

使い方は自由ですが、できた模様を見比べて目利きの練習に使用してください。

答え合わせは、下に出力しているBCS、Size、BMSを参考にしてください。

Sizeは楕円の面積を計算しているだけです。
BCSは一応日本食肉格付け協会のBCS1－8までの範囲でランダムに割り振っていますのでおおよそスタンダードに近い色が出ているのではないかと思います。
BMSは楕円の面積と白く塗りつぶされている範囲の面積を算出して、
[口田先生の論文](https://www.jstage.jst.go.jp/article/chikusan1924/68/9/68_9_853/_article/-char/ja/)
を参考計算しています。
![分布 bms vs ロース芯内脂肪面積比](https://user-images.githubusercontent.com/47586322/149666629-f2e647a6-e9ce-4e91-b6cc-fbfb4ca0f16f.png)

y= 4.418x + -7.184
決定係数 R^2：  0.9797916591387299

なので，x = (y+7.184)/4.418でBMSを推定します．

## tools 
- [ASP.NET](https://dotnet.microsoft.com/en-us/apps/aspnet)
- [shimatさんのOpenCvShapr](https://github.com/shimat/opencvsharp)
- [firebase](https://firebase.google.com/)
