# Hotaru Inventry System

## 概要
MA対応のアバター改変用ツールです。
オブジェクトオンオフアニメーションを手軽に作成できます。
boothでの配布ページは[こちら](https://hotaru86.booth.pm/items/5289934)

## VCCへの導入
VCCをインストール済みなら、[https://hotaru86.github.io/VPM/](https://hotaru86.github.io/VPM/)にアクセスし、「Add to VCC」を押すことで、VCCのリストに追加できます。

[VPM CLI](https://vcc.docs.vrchat.com/vpm/cli/)を使用する場合は、コマンドラインで以下のコマンドを入力することでも、VCCのリストに追加できます。
```
vpm add repo https://hotaru86.github.io/VPM/index.json
```

## プロジェクトへの導入
本ツールを使用するには、Modular Avatarの導入が必須です。
事前に、[https://modular-avatar.nadena.dev/ja](https://modular-avatar.nadena.dev/ja)から、Modular AvatarをVCCに導入してください。

本ツールを使用したいプロジェクトの「Manage Project」から、「Manager Package」に移動します。
VCCに導入済みのパッケージ一覧が表示されるので、HotaruInventrySystemの右にある「+」ボタンを押すか、「Installed version」からバージョンを選び、プロジェクトにインポートしてください。

※基本的に最新バージョン推奨です。

## 使い方
1. オンオフしたいオブジェクトを選択し、右クリックします。(AとBを同時にオンオフしたい場合、AとBを同時選択して右クリック)
2. 「HotaruInventrySystemに追加」を押します。複数選択も可能です。(複数オブジェクトをまとめてオンオフするアニメーションが生成されます。)
![SelectAndAdd](Website/Images/SelectAndAdd.png)

3. 表示されたウィンドウ上で、グループ名と、デフォルトでオンにするかどうかを設定し、「アニメーションを作成」ボタンを押します。
このウィンドウを見失ったときは、Unity画面上部の「Tools > HotaruInventrySystem」から、再度開けます。
<img src="Website/Images/SetGroupNameAndDefaultState.png" width="400">

4. アバターの子に「(生成日時)_HotaruInventrySystem」というオブジェクトが生成されていれば完了です！
作成したアニメーションを削除したいとき、再設定したいときは、この「(生成日時)_HotaruInventrySystem」オブジェクトを削除してください。
![Complete](Website/Images/Complete.png)

設定したオブジェクトのオンオフは、Expressionメニュー(Radialメニュー)から切り替えられます。
![MoveCheck](Website/Images/MoveCheck.png)

## 更新情報
- v1.0.0  リリース
- v1.0.1  ライセンスを明記
- v1.0.2  孫以下の子孫オブジェクトに設定ができないバグを修正
- v1.0.3  ビルド時にエラーが発生する問題を修正
- v1.0.4  軽微な修正
- v1.0.5  バージョン3.4.1のSDK-Avatarsでしか動作しない問題を修正
- v1.0.6  ツール内の誤字の修正
- v1.0.7  Unity Editor再起動時などに、AnimatorControllerのデータが消える問題を修正
- v1.0.8  Unity Editor再起動時などに、生成されたrootExMenu内のSubmenuAssetがNoneになる(参照が外れる)問題を修正
## ライセンス
[LICENSE](LICENSE.md)
