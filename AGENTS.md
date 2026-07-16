# AGENTS.md

## プロジェクト概要

本プロジェクトは、Unity 6、C#、Universal Render Pipeline 2Dを使用して開発するスマートフォン向けクリッカーゲームである。

プレイヤーは大木を伐採して丸太を獲得し、丸太を換金して得たお金で能力を強化する。

最終目的は、非常に大きな内部HPを持つ大木を、数値インフレによって切り倒すことである。

---

## 正とする仕様書

ゲーム仕様は、以下のファイルを正とする。

- `docs/GAME_DESIGN_DOCUMENT.md`

Unityの構造、責務、依存関係は、以下のファイルを正とする。

- `docs/UNITY_ARCHITECTURE.md`

仕様書と実装が矛盾する場合、勝手に実装を変更せず、矛盾点を報告すること。

未決定事項を推測して実装しないこと。

---

## 開発環境

- Unity 6
- Universal Render Pipeline 2D
- C#
- GitHub
- 対象プラットフォーム：iOS
- 画面方向：縦画面を予定

Unityのバージョンを勝手に変更しないこと。

新しいUnity Packageや外部ライブラリを導入する場合は、必要性を先に説明すること。

---

## 基本設計

本プロジェクトは、以下の層に分ける。

```text
Presentation
画面表示、入力、演出
        ↓
Application
処理の呼び出しと進行管理
        ↓
Domain
ゲームルールと数値計算
        ↓
State
プレイヤーの現在状態
        ↓
Infrastructure
セーブ、時刻、乱数、音声、外部機能
```

下位層から上位層へ直接依存させないこと。

---

## 責務分離ルール

### Presentation

Presentation層は、以下のみを担当する。

- ボタン入力
- タップ入力
- テキスト表示
- ゲージ表示
- 画像表示
- アニメーション
- 効果音再生の要求
- 画面の開閉

Presentation層で、以下を行ってはいけない。

- 丸太の加算
- お金の加算
- 攻撃力計算
- 換金額計算
- ガチャ抽選
- 道具破損判定
- セーブデータの直接操作

### Application

Application層は、以下を担当する。

- Presentationから要求を受け取る
- Domain処理を呼び出す
- Stateを更新する
- 処理結果をPresentationへ渡す
- 必要に応じてInfrastructureへ保存を要求する

### Domain

Domain層は、以下を担当する。

- 伐採量計算
- クールタイム判定
- 道具破損判定
- 修繕時間計算
- 換金額計算
- スキル価格計算
- ガチャ抽選
- キャラクター成長計算
- 放置報酬計算
- 大木へのダメージ計算

Domain層は、可能な限りMonoBehaviourに依存しない純粋なC#クラスとして実装すること。

### State

State層は、ゲーム内で変化する値を保持する。

例：

- 所持丸太
- 所持金
- 累計タップ回数
- 大木の残りHP
- 所持道具
- スキルレベル
- 所持キャラクター
- キャラクターレベル

状態は、複数のクラスから無制限に直接書き換えないこと。

### Infrastructure

Infrastructure層は、以下を担当する。

- JSONセーブ
- PlayerPrefs
- 時刻取得
- 乱数生成
- 音声再生
- アプリ一時停止と復帰
- 将来的な広告や課金

Domain層から、直接PlayerPrefsやファイルを操作しないこと。

---

## マスターデータと状態データ

マスターデータとプレイヤー状態を分離すること。

### マスターデータ

プレイ中に変化しない設定値。

例：

- 道具の名前
- 道具倍率
- 道具破損率
- キャラクターの基礎能力
- スキルの価格
- ガチャ排出率

原則としてScriptableObjectで管理する。

### プレイヤー状態

プレイによって変化する値。

例：

- 所持丸太
- 所持金
- キャラクターレベル
- 所持道具
- 現在装備中の道具
- スキルレベル

プレイヤー状態をScriptableObjectへ直接保存しないこと。

---

## MonoBehaviourの使用方針

MonoBehaviourは、Unityとの接続が必要なクラスに限定する。

使用してよい例：

- UI参照
- Update
- Coroutine
- Animator
- GameObject
- Scene
- 入力受付

使用を避ける例：

- 換金額計算
- ガチャ抽選
- スキル価格計算
- キャラクター成長計算
- セーブデータ検証

計算クラスをすべてMonoBehaviourにしないこと。

---

## GameManagerの方針

すべての機能を持つ巨大なGameManagerを作成しないこと。

以下の責務を1つのクラスへ集中させてはいけない。

- 伐採
- 換金
- ガチャ
- スキル
- キャラクター
- 道具
- セーブ
- 画面遷移
- 音声

起動処理が必要な場合は、小さな`GameBootstrap`を使用する。

`GameBootstrap`はゲームルールを持たないこと。

---

## Singletonの方針

Singletonを大量に使用しないこと。

以下のような依存を安易に追加しないこと。

```csharp
GameManager.Instance
SaveManager.Instance
GachaManager.Instance
SkillManager.Instance
```

依存関係は、可能な限りInspector、コンストラクタ、または`Initialize`メソッドで明示的に渡すこと。

---

## 初期試作の実装範囲

最初の試作では、以下のみを実装する。

- 木のタップ入力
- 1回の伐採で丸太を1獲得
- 所持丸太の表示
- 1秒のクリッククールタイム
- クールタイム中の伐採拒否
- クールタイムの表示に使用できる進行率

最初の試作では、以下を実装しない。

- 換金
- 所持金
- スキルツリー
- 道具
- 道具破損
- 修繕
- 木こり
- 商人
- 鍛冶職人
- ガチャ
- セーブ
- 放置報酬
- 大木HP
- 課金
- 広告

---

## 初期試作のクラス構成

初期試作では、以下を基本とする。

```text
MainScreenView
    ↓
ChoppingController
    ↓
ChoppingService
    ↓
PlayerGameState
```

候補ファイル：

```text
Assets/Scripts/State/PlayerGameState.cs

Assets/Scripts/Domain/Chopping/ChoppingConfig.cs
Assets/Scripts/Domain/Chopping/ChoppingResult.cs
Assets/Scripts/Domain/Chopping/ChoppingService.cs

Assets/Scripts/Application/ChoppingController.cs

Assets/Scripts/Presentation/MainScreen/MainScreenView.cs
```

必要以上にファイルを増やさないこと。

---

## 数値型

初期段階では、以下を使用する。

- 丸太：`double`
- 所持金：`double`
- 累計タップ回数：`long`
- クールタイム：`float`
- キャラクターレベル：`long`

独自大数クラスは、初期試作では実装しないこと。

---

## コーディング規則

### クラス名

PascalCaseを使用する。

```text
ChoppingService
PlayerGameState
MainScreenView
```

### メソッド名

PascalCaseを使用する。

```text
TryChop
ApplyManualChop
RefreshDisplay
```

### privateフィールド

先頭にアンダースコアを付ける。

```csharp
private double _ownedLogs;
private float _remainingCooldown;
```

### Inspector参照

原則として、`public`ではなく`SerializeField`付きのprivateフィールドを使用する。

```csharp
[SerializeField] private TMP_Text _ownedLogsText;
```

### 定義データ

ScriptableObjectの定義クラスには`Definition`を付ける。

```text
ToolDefinition
CharacterDefinition
SkillDefinition
GachaDefinition
```

---

## エラー処理

ユーザー操作として起こり得る失敗は、原則として例外ではなく処理結果として返す。

例：

- クールタイム中
- 修繕中
- 所持金不足
- 未解放
- ガチャ費用不足

不正なマスターデータやプログラム上の設計ミスは、例外またはUnity Consoleのエラーとして発見できるようにすること。

不正な状態を黙って無視しないこと。

---

## テスト方針

Domain層は、可能な限りEditMode Testで確認できる構造にする。

初期試作では、最低限以下を確認する。

- 伐採成功時に丸太を獲得できる
- 累計手動伐採回数が増える
- 負の丸太を追加できない
- クールタイム中は伐採できない
- クールタイム終了後に再び伐採できる

SceneやUIのテストが必要な場合は、Unityエディタでの確認手順を報告すること。

---

## SceneとPrefab

初期試作では、以下のSceneを使用する。

```text
Assets/Scenes/MainScene.unity
```

初期段階では、換金、ガチャ、キャラクター、スキルツリーなどの画面を作らないこと。

SceneやPrefabを変更した場合は、以下を報告すること。

- 変更したSceneまたはPrefab
- 追加したGameObject
- 必要なInspector設定
- 必要な参照の接続方法

---

## Unity生成ファイル

以下のフォルダやファイルを手動で編集しないこと。

- `Library`
- `Temp`
- `Logs`
- `obj`
- Unityが自動生成するプロジェクトファイル

既存の`.meta`ファイルを理由なく削除しないこと。

アセットを移動する場合は、可能な限りUnityエディタ上から移動すること。

---

## 実装前の確認

実装前に、以下を行うこと。

1. `docs/GAME_DESIGN_DOCUMENT.md`を読む
2. `docs/UNITY_ARCHITECTURE.md`を読む
3. 現在のフォルダ構成を確認する
4. 既存クラスの責務を確認する
5. 実装範囲と実装しない範囲を確認する
6. 未決定事項が必要なら、実装を止めて報告する

---

## 実装後の報告

実装後は、必ず以下を報告すること。

### 変更ファイル

追加または変更したファイルをすべて列挙する。

### 各ファイルの責務

各ファイルが何を担当するか説明する。

### Unityエディタ上の作業

以下を具体的に説明する。

- 作成するGameObject
- 付与するComponent
- Inspectorへ設定する参照
- ButtonのOnClick設定
- Sceneの保存場所

### 動作確認

Unityエディタ上での確認手順を説明する。

### テスト結果

実行したテスト、コンパイル確認、Console確認を報告する。

### 未実装

今回意図的に実装しなかった項目を明記する。

### 注意事項

手動設定が不足すると動かない部分や、今後の課題を明記する。

---

## 禁止事項

以下を禁止する。

- 仕様書にない機能を勝手に追加する
- 未決定事項を推測して実装する
- UIクラスでゲーム状態を直接加算する
- UIクラスでゲームルールを計算する
- Domain層からUIを操作する
- Domain層からPlayerPrefsを操作する
- プレイヤー状態をScriptableObjectへ保存する
- 巨大なGameManagerを作る
- Singletonを大量に追加する
- 初期試作で外部DIライブラリを導入する
- 初期試作で複雑なイベントバスを導入する
- 初期試作でガチャやセーブまで同時に実装する
- Unityのバージョンを変更する
- `.meta`ファイルを理由なく削除する
- mainブランチへ直接コミットする