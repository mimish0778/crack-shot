<div align="center">

# CRACK-SHOT

**2つの球の間をすり抜けて、ホールを狙え。**<br>
ゲートを通してからカップインする、ひと味ちがうミニゴルフパズル。

<br>

<img src="docs/screenshot.png" alt="CRACK-SHOT のプレイ画面" width="720">

<br><br>

[![Play](https://img.shields.io/badge/PLAY-unityroom-111111?style=flat-square)](https://unityroom.com/games/crack-shot)
[![Unity](https://img.shields.io/badge/Unity-2022.3.61f1-111111?style=flat-square&logo=unity&logoColor=white)](https://unity.com/)
[![License](https://img.shields.io/badge/License-MIT-111111?style=flat-square)](LICENSE)

**[▶ unityroom でプレイする](https://unityroom.com/games/crack-shot)**

</div>

<br>

## 遊び方

フィールドには3つの球があります。そのうち1つを打ち、残り2つの球を結ぶライン **GATE** を通過させましょう。
GATEを通過したうえでホールに入れれば、ステージクリアです。

| 判定 | 条件 |
| --- | --- |
| **CLEAR** | GATEを通過してからホールイン |
| **SUCCESS** | GATEを通過した（続けて次のショットへ） |
| **MISS** | GATEを通過しなかった／ほかの球に当たった／場外に出た／GATEを通らずにホールインした |

MISSになると、球は打つ前の位置に戻ります。少ない打数・短いタイムでのクリアを目指しましょう。

## 操作方法

| 操作 | 入力 |
| --- | --- |
| ショット | 左ドラッグで引っぱって離す（球の近くで離すとキャンセル） |
| 打つ球の切り替え | `←` `→` ／ `A` `D` |
| カメラ回転 | 右ドラッグ |
| ズーム | マウスホイール |
| 真上からの視点 | `↑` ／ `W` |
| 視点リセット | `↓` ／ `S` |

## 特徴

- **全4ステージ** — 球の配置とコースを読み解くパズル性
- **ベスト記録** — ステージごとに最少打数・最短タイムを保存
- **オンラインランキング** — 一部ステージは unityroom のランキングに対応
- **設定** — 音量やカメラ感度を調整可能

## 開発環境

- Unity 2022.3.61f1
- Universal Render Pipeline 14.0.12
- TextMesh Pro

## クレジット

- **BGM**：[踊る、宇宙の中で](https://dova-s.jp/bgm/detail/15926)（DOVA-SYNDROME）
- **フォント**：[Orbitron](https://fonts.google.com/specimen/Orbitron)（Google Fonts / SIL Open Font License）

## ライセンス

ソースコードは [MIT License](LICENSE) で公開しています。© 2026 Mimish

- BGM・効果音などの音素材は、ライセンス上の理由からリポジトリに含めていません。
- 同梱のフォント Orbitron には SIL Open Font License が適用されます。
