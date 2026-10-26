# CRACK-SHOT

2 つの球の間を通してからホールに入れる、3D のミニゴルフパズルゲームです。Unity で制作しました。

![CRACK-SHOT のプレイ画面](docs/screenshot.png)

**unityroom でプレイ:** https://unityroom.com/games/crack-shot

プレイ動画: [Video.mp4](Video.mp4)

## 遊び方

フィールドには 3 つの球があります。そのうち 1 つを選んで打ち、残り 2 つの球を結ぶ線（GATE）を横切らせてから、ホールに入れるとステージクリアです。

GATE の位置は、打つ瞬間の残り 2 つの球の位置で決まります。打った球が止まったときの結果は次のとおりです。

| 結果 | 条件 | その後 |
| --- | --- | --- |
| CLEAR | 同じ 1 打の中で GATE を通過し、そのままホールに入った | リザルト画面へ |
| SUCCESS | GATE を通過して止まった | 球の位置がそのまま残り、次の 1 打へ |
| MISS | GATE を通過せずに止まった / ほかの球に当たった / 場外に出た / GATE を通過せずにホールに入った | 1 打のペナルティを加え、すべての球を打つ前の位置に戻す |

SUCCESS で球の配置を変えながら、ゴールまでの道筋を作っていくのが基本の流れです。打数がパーより少ないほど良い結果になります。

## 操作方法

| 操作 | 入力 |
| --- | --- |
| ショット | 球の近くを左ドラッグで引っぱり、離して打つ（引いた方向の反対に飛ぶ） |
| ショットのキャンセル | 球のすぐ近く（表示される円の内側）で離す |
| 打つ球の切り替え | `←` `→` / `A` `D`、またはほかの球を左クリック |
| カメラの回転 | 右ドラッグ |
| ズーム | マウスホイール |
| 真上からの視点 | `↑` / `W` |
| 視点を元に戻す | `↓` / `S` |

カメラは選んでいる球を追いかけます。プレイ画面のボタンから、ステージ選択に戻ったり、ステージを最初からやり直したりできます。

## ステージ

全 4 ステージです。ステージ 2 と 4 には、左右に往復する障害物があります。

| ステージ | パー | 動く障害物 | unityroom ランキング |
| --- | --- | --- | --- |
| STAGE 1 | 2 | なし | なし |
| STAGE 2 | 3 | あり | なし |
| STAGE 3 | 6 | なし | あり |
| STAGE 4 | 8 | あり | あり |

## スコアと記録

- クリアすると、打数とタイム、パーとの差に応じた評価（BIRDIE、PAR、BOGEY など）が表示されます。
- ステージごとに最少打数を記録します。打数が同じ場合は、タイムが短いほうを記録します。記録はステージ選択画面でステージにカーソルを合わせると表示されます。
- STAGE 3 と STAGE 4 は unityroom のランキングに対応しています。打数が少ない順、同じ打数ならタイムが短い順に並びます。

## 設定

タイトル画面の設定ボタンから、次の項目を変更できます。設定はゲームを終了すると初期値に戻ります。

- 音量（マスター、BGM、効果音）
- カメラの回転速度とズーム速度
- カメラの左右・上下の操作の反転

## 開発環境

- Unity 2022.3.61f1
- Universal Render Pipeline 14.0.12
- TextMesh Pro 3.0.7
- [unityroom-client-library](https://github.com/naichilab/unityroom-client-library)（ランキング送信）

## プロジェクトを開く

1. リポジトリをクローンします。

   ```bash
   git clone https://github.com/mimish0778/crack-shot.git
   ```

2. Unity Hub で Unity 2022.3.61f1 をインストールし、クローンしたフォルダをプロジェクトとして開きます。unityroom-client-library は Git の URL から取得されるため、Git がインストールされている必要があります。
3. `Assets/Project/Scenes/TitleScene.unity` を開いて再生します。

シーンは TitleScene、SelectScene、PlayScene、ResultScene の 4 つで、この順にビルド設定に登録されています。

次のファイルはリポジトリに含まれていません。

- BGM と効果音（`Assets/Project/Audio/`）。ライセンス上の理由で除外しています。ない場合は音が鳴らないだけで、ゲームは動作します。
- unityroom の HMAC キー（`Assets/Project/Resources/HmacKey.txt`）。ない場合、ランキングへのスコア送信は行われません。

なお、エディタ上で再生すると、起動時に PlayerPrefs（保存されたベスト記録）が削除されます。

## プロジェクト構成

```
Assets/Project/
├── Data/       ステージごとの設定（ステージ名、パー、球の初期位置）
├── Fonts/      Orbitron フォント
├── Images/     UI のアイコン
├── Materials/
├── Prefabs/    球、ホール、常駐マネージャーなど
├── Scenes/     TitleScene / SelectScene / PlayScene / ResultScene
└── Scripts/
    ├── Ball/       球の物理挙動と、打つ球の選択
    ├── Camera/     カメラの追従・回転・ズーム
    ├── Core/       シングルトンなどの共通処理
    ├── Effects/    演出
    ├── Input/      入力とドラッグショット
    ├── Managers/   ゲーム進行、スコア、音、設定、シーン遷移
    ├── Stage/      GATE の判定、ホール、動く障害物
    └── UI/         各シーンの画面
```

## クレジット

- BGM: [踊る、宇宙の中で](https://dova-s.jp/bgm/detail/15926)（DOVA-SYNDROME）
- フォント: [Orbitron](https://fonts.google.com/specimen/Orbitron)（SIL Open Font License）

## ライセンス

ソースコードは [MIT License](LICENSE) で公開しています。

同梱のフォント Orbitron には SIL Open Font License 1.1 が適用されます（`Assets/Project/Fonts/Orbitron/OFL.txt`）。
