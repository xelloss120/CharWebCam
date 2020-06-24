# CharWebCam
普通のWebCamで3Dキャラクター(Unityちゃん)を動かすよ！

## 動かすのに必要なもの
Unity 2019.4  
Intelの第4世代以降のCPU

## できること
カメラに映った顔から、キャラクターの位置、頭の向き、表情を反映できます。  
起動時に選択した録音デバイスの音量に合わせて口が動きます。  
マウスでカメラ操作ができます。

## 使い方
動画制作や配信等でクロマキー合成してお使いください。  
調整や拡張はご自由に行ってください。

## 注意
カメラの初期化はRealSenseSDK任せなので、認識させたいカメラのみPCに接続してください。  
**※RealSenseSDKは、dllの絶対パスにASCII外の文字が含まれていると動作しません。絶対パスがASCIIのみで構成させている場所へUnityプロジェクトを置いてください。**

## MMDモデル(pmx等)の対応について
./CharWebCam/MMD_Sample/RS_Kotonoha.csと  
./CharWebCam/MMD_Sample/MM_Kotonoha.csは  
以下のMMDモデル向けですが、MMD4Mとモデルは規約に基づき同梱していません。  
http://www.nicovideo.jp/watch/sm24368983  
別途MMD4Mとモデルを追加した上で、Unityちゃんのシーンを元に作ってもらえればと思います。

参考
http://qiita.com/Hv2RMjHzDyqXVIr/items/7e1aca3506a05aa20644

## ライセンス
RealSenseSDK：使用バージョンに準拠  
ユニティちゃん：© Unity Technologies Japan/UCL  
その他(120作成部分)：MIT

## 免責
ご利用は自己責任で！！  
特にMMDモデルのご利用はMMD4Mの注意事項およびモデル付属の文書をよく読みましょう。  
そして版権モデルの場合は版権元の規約も読みましょう。

## 実行形式(exe)版
https://1drv.ms/u/s!Ass7Jg1DXnrDlBoBFcokzBZX2Tm9  
開発環境のPCと手持ちのノートで動作確認していますが、正直怪しいのでダメだったら教えてください。  
**※保存先のパスに全角が混ざるとダメなので、半角英数記号のみで構成される保存先(パス)に置いて実行してください。**

## プロ生ちゃん版
https://1drv.ms/u/s!Ass7Jg1DXnrDmR83V47_raOXKTUj  
ここで公開しているものを使ってプロ生ちゃん版を作ってみました。
