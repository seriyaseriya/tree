# Unityアーキテクチャ仕様書

更新日：2026-07-16  
ステータス：初期設計  
対象エンジン：Unity 6  
レンダリング方式：Universal Render Pipeline 2D  
使用言語：C#  
関連仕様書：`docs/GAME_DESIGN_DOCUMENT.md`

---

## 1. この仕様書の目的

本仕様書は、木こりクリッカーゲームにおけるUnityプロジェクトの構造、クラスの責務、データの流れ、依存関係を定義する。

本ゲームでは、以下の問題を防ぐことを目的とする。

- 1つのGameManagerに処理が集中する
- UIクラスがゲーム内の数値を直接変更する
- SceneやPrefabにゲームルールが埋め込まれる
- セーブデータとマスターデータが混在する
- 同じ計算処理が複数箇所に重複する
- 機能追加によって既存システムが壊れる
- Codexが既存の責務を無視して実装する
- 共同開発時に同じファイルの競合が頻発する

本仕様書を、Unity実装における基本設計の正とする。

---

## 2. 基本設計方針

本ゲームは、以下の層に分けて実装する。

```text
Presentation
画面表示、ボタン、アニメーション
        ↓
Application
入力の受付、ゲーム処理の呼び出し
        ↓
Domain
伐採、換金、強化、ガチャなどのゲームルール
        ↓
State
現在の所持資源、進行状態、所持キャラクター
        ↓
Infrastructure
セーブ、時刻取得、乱数、外部機能
```

原則として、上の層から下の層を呼び出す。

下の層が、上の層に直接依存してはいけない。

---

## 3. 各層の責務

### 3.1 Presentation層

Presentation層は、画面表示とユーザー操作を担当する。

主な責務は以下とする。

- ボタン入力の検知
- テキスト表示
- 数値表示
- ゲージ表示
- キャラクター画像の表示
- タップ演出
- アニメーション
- 効果音再生の要求
- 画面の開閉
- 画面遷移

Presentation層は、ゲーム内の数値を直接変更してはいけない。

禁止例：

```csharp
public void OnClickTree()
{
    logs += 1;
}
```

許可される流れ：

```text
TreeButton
    ↓
ChoppingController
    ↓
ChoppingService
    ↓
GameState更新
    ↓
MainScreenView更新
```

Presentation層では、攻撃力、換金額、ガチャ結果などの計算を行わない。

---

### 3.2 Application層

Application層は、画面からの入力を受け取り、必要なゲーム処理を順番に実行する。

主な責務は以下とする。

- 木を切る要求を受け付ける
- 丸太を換金する要求を受け付ける
- スキルを購入する要求を受け付ける
- ガチャを引く要求を受け付ける
- キャラクターを編成する要求を受け付ける
- 道具を購入または変更する
- 複数のDomain処理を組み合わせる
- 処理結果をPresentation層へ通知する
- 必要に応じてセーブを要求する

Application層は、UnityのButtonやTextMeshProなどの具体的なUI部品に依存しないことを目標とする。

---

### 3.3 Domain層

Domain層は、ゲームルールと数値計算を担当する。

主な責務は以下とする。

- タップ伐採量の計算
- クリッククールタイムの判定
- 道具破損判定
- 修繕時間の計算
- 木こりによる自動伐採量の計算
- 換金額の計算
- スキル購入条件の判定
- スキル価格の計算
- ガチャ抽選
- キャラクター重複時のレベル処理
- 商人倍率の決定
- 鍛冶職人効果の計算
- 大木へのダメージ計算
- 放置報酬の計算

Domain層の計算は、可能な限りMonoBehaviourに依存しない純粋なC#クラスとして実装する。

同じ入力に対して同じ出力を返す処理は、純粋関数として実装することを優先する。

例：

```csharp
public static double CalculateSalePrice(
    double logAmount,
    double basePrice,
    double merchantMultiplier,
    double skillMultiplier)
{
    return logAmount
        * basePrice
        * merchantMultiplier
        * skillMultiplier;
}
```

Domain層は以下に依存しない。

- GameObject
- Transform
- Button
- Image
- TextMeshPro
- Animator
- Scene
- PlayerPrefs
- Unityの画面表示

---

### 3.4 State層

State層は、現在のゲーム進行状態を保持する。

主な保持対象は以下とする。

- 所持丸太
- 所持金
- 累計獲得丸太
- 累計獲得金額
- 累計タップ回数
- 大木の残りHP
- スキルレベル
- 所持道具
- 装備中の道具
- 所持キャラクター
- キャラクターレベル
- メイン木こり
- ガチャ解放状態
- 長押し自動伐採の解放状態
- 最終保存時刻

State層は、ゲーム内で変化する値を保持する。

ScriptableObjectに現在の所持金やキャラクターレベルを保存してはいけない。

---

### 3.5 Infrastructure層

Infrastructure層は、ゲームルール以外の技術的な処理を担当する。

主な責務は以下とする。

- JSONセーブ
- セーブデータの読み込み
- ファイルへの書き込み
- PlayerPrefsによる設定保存
- 現在時刻の取得
- 乱数生成
- Unityライフサイクルとの接続
- アプリの一時停止と復帰
- 効果音やBGMの再生
- 将来的な広告、課金、Game Center連携

Domain層が直接PlayerPrefsやファイルを操作してはいけない。

---

## 4. データの分類

本ゲームのデータは、以下の3種類に分ける。

### 4.1 マスターデータ

開発者が設定し、通常プレイ中には変更されないデータ。

例：

- 道具の名前
- 道具の基礎倍率
- 道具の基礎破損率
- 道具の修繕時間
- キャラクターの名前
- キャラクターのランク
- キャラクターの基礎能力
- スキルの基礎価格
- ガチャの排出率
- ガチャの解放費用

マスターデータは、原則としてScriptableObjectで管理する。

---

### 4.2 プレイヤー状態データ

プレイによって変化するデータ。

例：

- 所持丸太
- 所持金
- スキルレベル
- キャラクターレベル
- 所持道具
- 現在装備中の道具
- 大木の残りHP

プレイヤー状態データは、C#のシリアライズ可能なクラスとして保持する。

保存時にはJSONへ変換する。

---

### 4.3 一時的な実行時データ

アプリ実行中だけ必要なデータ。

例：

- 現在のクリッククールタイム残量
- 現在修繕中か
- 長押し中か
- 現在表示している画面
- アニメーション再生中か
- ガチャ演出中か
- 一時的な通知メッセージ

一時的な実行時データは、原則としてセーブ対象にしない。

ただし、修繕終了時刻など、アプリ終了後にも継続する必要がある情報は保存対象とする。

---

## 5. ScriptableObjectの使用方針

ScriptableObjectは、マスターデータの定義に使用する。

### 5.1 使用する対象

以下のデータはScriptableObjectで管理する候補とする。

- `ToolDefinition`
- `CharacterDefinition`
- `SkillDefinition`
- `GachaDefinition`
- `GameBalanceDefinition`
- `TreeDefinition`

### 5.2 使用しない対象

以下の値はScriptableObjectに直接保存しない。

- 所持丸太
- 所持金
- 現在のキャラクターレベル
- 現在装備している道具
- 現在の大木HP
- スキルの現在レベル
- ガチャの解放状態

ScriptableObjectの値をプレイ中に書き換えて、セーブデータとして扱ってはいけない。

---

## 6. 推奨フォルダ構成

Unityプロジェクト内は、以下の構成を基本とする。

```text
Assets/
├─ Art/
│  ├─ Backgrounds/
│  ├─ Characters/
│  ├─ Tools/
│  ├─ Trees/
│  ├─ UI/
│  └─ Icons/
├─ Audio/
│  ├─ BGM/
│  └─ SE/
├─ Materials/
├─ Prefabs/
│  ├─ Characters/
│  ├─ UI/
│  ├─ Effects/
│  └─ Gameplay/
├─ Scenes/
├─ ScriptableObjects/
│  ├─ Characters/
│  ├─ Tools/
│  ├─ Skills/
│  ├─ Gacha/
│  └─ Balance/
├─ Scripts/
│  ├─ Application/
│  ├─ Domain/
│  │  ├─ Chopping/
│  │  ├─ Selling/
│  │  ├─ Tools/
│  │  ├─ Characters/
│  │  ├─ Skills/
│  │  ├─ Gacha/
│  │  ├─ Tree/
│  │  └─ Offline/
│  ├─ State/
│  ├─ Presentation/
│  │  ├─ MainScreen/
│  │  ├─ SellScreen/
│  │  ├─ GachaScreen/
│  │  ├─ CharacterScreen/
│  │  ├─ SkillTreeScreen/
│  │  └─ OptionsScreen/
│  ├─ Infrastructure/
│  │  ├─ Save/
│  │  ├─ Time/
│  │  ├─ Random/
│  │  └─ Audio/
│  └─ Common/
└─ Tests/
   ├─ EditMode/
   └─ PlayMode/
```

最初の試作段階では、使わないフォルダを無理に作成しなくてもよい。

必要になった段階で追加する。

最初の基本伐採機能では、以下のフォルダを新規に使用する。

```text
Assets/Scripts/Application
Assets/Scripts/Domain
Assets/Scripts/State
Assets/Scripts/Presentation
```

既存の空フォルダである`Core`、`Gameplay`、`Save`、`UI`、`Utility`は、最初の基本伐採機能では使用しない。また、この段階では削除しない。

---

## 7. 初期試作で使用するクラス

最初の実装段階では、以下の基本伐採機能だけを実装する。

- 木のタップ
- 1回の伐採で丸太を1獲得
- 所持丸太表示
- 累計手動伐採回数
- 1秒のクリッククールタイム
- クールタイム中の伐採拒否
- クールタイム進行率のUI反映

換金、スキル、道具、ガチャ、セーブ、放置報酬、大木HPは、この段階では実装しない。

初期クラス構成は以下を基本とする。

```text
MainScreenView
    ↓
ChoppingController
    ↓
ChoppingService
    ↓
PlayerGameState
```

### 7.1 PlayerGameState

責務：

- プレイヤーの現在状態を保持する
- 所持丸太を保持する
- 累計タップ回数を保持する
- 状態変更用の最小限のメソッドを提供する

初期段階の保持項目：

```text
ownedLogs
totalLogsEarned
totalManualChops
```

禁止事項：

- UIを更新しない
- 効果音を再生しない
- MonoBehaviourにしない
- Buttonの処理を持たない
- クールタイムの経過処理を持たない

---

### 7.2 ChoppingService

責務：

- 現在伐採できるか判定する
- 1回の伐採で獲得する丸太量を計算する
- 伐採結果を作成する
- クールタイム開始に必要な情報を返す

初期段階では以下を扱う。

- 基礎獲得丸太量
- クリッククールタイム
- 伐採可能判定

将来的には以下を追加する。

- 道具倍率
- 木こり固有能力
- 鍛冶職人倍率
- 道具破損判定
- 大木へのダメージ

禁止事項：

- UIを直接操作しない
- Sceneを検索しない
- セーブしない
- 効果音を再生しない

---

### 7.3 ChoppingController

責務：

- Presentation層から伐採要求を受け取る
- `ChoppingService`を呼び出す
- `PlayerGameState`を更新する
- クールタイムを管理する
- `Time.unscaledDeltaTime`を使用してクールタイムを進行させる
- クールタイム進行率を計算し、画面へ渡す
- 処理結果を`MainScreenView`へ明示的に通知する

Unity上ではMonoBehaviourとして実装してよい。

ただし、計算処理は可能な限り`ChoppingService`へ委譲する。

禁止事項：

- タップ獲得量の計算式を直接持たない
- TextMeshProへ直接複雑な表示文字列を組み立てない
- セーブ形式を直接知識として持たない

---

### 7.4 MainScreenView

責務：

- 所持丸太数を表示する
- 木のタップ入力を受け取る
- クールタイムゲージを表示する
- 伐採可能または不可能な状態を表示する
- 伐採演出を再生する

禁止事項：

- 所持丸太を直接加算しない
- クールタイムのルールを決めない
- 攻撃力計算を行わない
- 道具破損判定を行わない
- セーブデータを直接読み書きしない

---

## 8. 初期試作のデータフロー

木をタップした場合の処理は以下とする。

```text
1. プレイヤーが木をタップする

2. MainScreenViewが入力を検知する

3. MainScreenViewがChoppingControllerへ伐採を要求する

4. ChoppingControllerがTryChopを実行する

5. クールタイム中の場合、CooldownActiveを含む伐採結果を返して終了する

6. 伐採可能な場合、ChoppingControllerがChoppingServiceを呼び出す

7. ChoppingServiceが獲得丸太量を計算し、伐採結果を返す

8. ChoppingControllerがPlayerGameStateを更新する

9. ChoppingControllerがクリッククールタイムを開始する

10. MainScreenViewが最新の所持丸太を表示する

11. MainScreenViewがタップ演出を再生する
```

---

## 9. 伐採結果データ

伐採処理では、複数の値を個別に返すのではなく、結果用データへまとめる。

最初の基本伐採機能では、以下の値だけを含める。

```csharp
public readonly struct ChoppingResult
{
    public double EarnedLogs { get; }
    public float CooldownSeconds { get; }
    public ChoppingFailureReason FailureReason { get; }

    public ChoppingResult(
        double earnedLogs,
        float cooldownSeconds,
        ChoppingFailureReason failureReason)
    {
        EarnedLogs = earnedLogs;
        CooldownSeconds = cooldownSeconds;
        FailureReason = failureReason;
    }
}
```

`FailureReason`が`None`の場合のみ伐採成功として状態を更新する。クールタイム中は`CooldownActive`を返す。

大木HPは今回実装しないため、`TreeDamage`は含めない。`TreeDamage`は大木HP機能の実装時に追加する。

将来的には以下を追加できる。

- 道具が破損したか
- 修繕時間
- クリティカルが発生したか
- 固有能力が発動したか
- 大木の状態が変化したか

Presentation層は、ゲームルールを再計算せず、結果データを表示に使用する。

---

## 10. ゲーム状態の更新方針

Stateの値を、複数のクラスから無制限に書き換えてはいけない。

以下のような直接変更は避ける。

```csharp
gameState.OwnedLogs += amount;
```

原則として、状態変更用のメソッドを使用する。

```csharp
gameState.AddLogs(amount);
```

例：

```csharp
[Serializable]
public sealed class PlayerGameState
{
    public double OwnedLogs { get; private set; }
    public double TotalLogsEarned { get; private set; }
    public long TotalManualChops { get; private set; }

    public void ApplyManualChop(double earnedLogs)
    {
        if (earnedLogs < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(earnedLogs));
        }

        OwnedLogs += earnedLogs;
        TotalLogsEarned += earnedLogs;
        TotalManualChops++;
    }
}
```

これにより、不正な値や更新漏れを防ぐ。

---

## 11. 時間管理

ゲーム内の時間処理は、用途によって分ける。

### 11.1 画面内クールタイム

最初の基本伐採機能のクリッククールタイムには、`Time.unscaledDeltaTime`を使用する。

クールタイムの初期値は1秒とする。`ChoppingController`は残り時間を進行させ、0から1のクールタイム進行率を`MainScreenView`へ渡す。

### 11.2 放置時間

放置報酬の計算には、`Time.time`や`Time.deltaTime`を使用しない。

保存した日時と現在日時の差から計算する。

時刻取得は抽象化する。

```csharp
public interface ITimeProvider
{
    DateTime UtcNow { get; }
}
```

実装例：

```csharp
public sealed class SystemTimeProvider : ITimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
```

これにより、放置報酬のテストがしやすくなる。

---

## 12. 乱数管理

道具破損判定やガチャ抽選では乱数を使用する。

Domain処理が直接`UnityEngine.Random`へ強く依存しないようにする。

候補：

```csharp
public interface IRandomProvider
{
    double NextDouble();
    int Range(int minInclusive, int maxExclusive);
}
```

これにより、テスト時に結果を固定できる。

初期試作では乱数を使用しないため、実装は道具破損またはガチャ実装時に追加する。

---

## 13. イベントと画面更新

ゲーム状態を更新した後、Presentation層を更新する方法は以下のいずれかとする。

最初の基本伐採機能ではイベント方式を使用せず、`ChoppingController`が`MainScreenView`へ明示的に表示更新を要求する。

```text
Controller
    ↓
View.Refresh(...)
```

機能が増えた場合は、状態変更イベントの導入を検討する。

例：

```csharp
public event Action<double> OwnedLogsChanged;
```

ただし、イベントを必要以上に増やして処理経路が分かりにくくならないようにする。

初期試作では、複雑なイベントバスや外部DIライブラリを導入しない。

---

## 14. Scene構成

初期試作では、Sceneは1つとする。

対象画面の方向は縦画面とする。

```text
Assets/Scenes/MainScene.unity
```

初期Hierarchy案：

```text
MainScene
├─ Main Camera
├─ GameRoot
│  └─ ChoppingController
├─ EventSystem
└─ Canvas
   ├─ SafeArea
   │  ├─ Header
   │  │  ├─ OwnedLogsText
   │  │  └─ LogsPerTapText
   │  ├─ MainArea
   │  │  ├─ TreeButton
   │  │  ├─ CooldownGauge
   │  │  └─ TapEffectRoot
   │  └─ BottomNavigation
   └─ MainScreenView
```

初期段階では、換金、ガチャ、スキルなどの画面は作らない。

Hierarchyには`SafeArea`を用意する。ただし、端末のノッチへ自動対応するSafe Areaスクリプトは、最初の基本伐採機能では実装しない。

---

## 15. SceneとPrefabの方針

### 15.1 Sceneに置くもの

- Main Camera
- EventSystem
- ゲーム全体のルート
- 画面全体のCanvas
- Scene固有の制御オブジェクト

### 15.2 Prefabにするもの

- キャラクター表示
- ガチャ結果カード
- スキルノード
- 道具一覧項目
- 所持キャラクター一覧項目
- 共通ダイアログ
- 共通ボタン
- タップエフェクト

初期試作では、すべてを無理にPrefab化しない。

複数箇所で使用するもの、または独立して編集したいものをPrefab化する。

---

## 16. 画面遷移方針

初期リリースでは、以下の主要画面を想定する。

- 伐採画面
- 換金画面
- ガチャ画面
- キャラクター画面
- スキルツリー画面
- オプション画面

可能であれば、すべてを別Sceneにせず、同じScene内のUIパネルとして管理する。

理由：

- 画面切り替えを高速にできる
- ゲーム状態を保持しやすい
- 画面数が少ない
- Scene間の参照問題を減らせる
- スマートフォン向けのタブ切り替えと相性がよい

大木伐採完了演出やエンディングは、必要に応じて別Sceneにする。

---

## 17. セーブ設計方針

セーブ処理は以下の流れとする。

```text
PlayerGameState
    ↓
SaveDataへ変換
    ↓
JSONへ変換
    ↓
端末内へ保存
```

読み込み時：

```text
端末内のJSON
    ↓
SaveDataへ変換
    ↓
検証
    ↓
PlayerGameStateを生成
```

Presentation層やDomain層から、直接ファイルへ保存してはいけない。

セーブ処理はInfrastructure層へ集約する。

詳細は`docs/SAVE_SYSTEM.md`で定義する。

---

## 18. 数値型の方針

本ゲームは数値が大きくインフレするため、整数型の上限に注意する。

初期段階では以下を使用する。

- キャラクターレベル：`long`
- 累計タップ回数：`long`
- クールタイム：`float`
- 丸太、所持金、攻撃力：`double`

ただし、`double`で表現できない規模までインフレさせる場合は、独自の大数表現を導入する。

候補：

```text
mantissa × 10^exponent
```

例：

```text
1.25 × 10^120
```

初期試作では独自大数クラスを実装しない。

ゲームバランス仕様の決定後に、必要性を判断する。

---

## 19. エラー処理方針

不正な状態を黙って無視しない。

例：

- 負の丸太を追加しない
- 負の価格を使用しない
- 存在しないキャラクターIDを処理しない
- 未解放ガチャを実行しない
- 所持金不足で購入処理を進めない
- クールタイム中に伐採処理を実行しない

ユーザー操作として起こり得る失敗は、例外ではなく結果データとして返す。

例：

```csharp
public enum ChoppingFailureReason
{
    None,
    CooldownActive
}
```

最初の基本伐採機能では`TryChop`形式を使用し、上記2種類だけを扱う。`ToolRepairing`などの失敗理由は、対応機能の実装時に追加する。

プログラムの設計ミスや不正なマスターデータは、例外またはUnity Consoleのエラーとして発見できるようにする。

---

## 20. テスト方針

Domain層の計算ロジックは、EditMode Testで確認できる構造にする。

初期試作で最低限確認する項目：

- 伐採成功時に丸太が増える
- 1回の伐採で累計タップ数が1増える
- 負の丸太を加算できない
- クールタイム中は伐採できない
- クールタイム終了後は再び伐採できる

将来的に追加するテスト：

- 換金額計算
- スキル価格計算
- 道具破損判定
- 修繕時間計算
- ガチャ排出率
- 重複レベルアップ
- 最強商人の選択
- 放置報酬計算
- セーブデータ移行

---

## 21. 命名規則

### 21.1 クラス名

PascalCaseを使用する。

```text
ChoppingService
PlayerGameState
MainScreenView
ToolDefinition
```

### 21.2 メソッド名

PascalCaseを使用する。

```text
TryChop
CalculateLogAmount
ApplyManualChop
RefreshDisplay
```

### 21.3 privateフィールド

キャメルケースの先頭にアンダースコアを付ける。

```csharp
private double _ownedLogs;
private float _remainingCooldown;
```

### 21.4 SerializeField

privateフィールドとして定義する。

```csharp
[SerializeField] private TMP_Text _ownedLogsText;
```

原則として、Inspector設定のためだけにpublicフィールドを使用しない。

### 21.5 ScriptableObject

定義クラスには`Definition`を使用する。

```text
ToolDefinition
CharacterDefinition
SkillDefinition
GachaDefinition
```

### 21.6 状態データ

プレイヤーの状態には`State`または`SaveData`を使用する。

```text
PlayerGameState
PlayerSaveData
OwnedCharacterState
```

---

## 22. MonoBehaviourの使用方針

MonoBehaviourは、Unityとの接続が必要なクラスに限定する。

使用してよい例：

- ボタン入力
- Update処理
- Coroutine
- Animator制御
- Unity UI参照
- Scene上のGameObject管理

使用を避ける例：

- 換金額計算
- ガチャ抽選ロジック
- スキル価格計算
- キャラクターレベル計算
- セーブデータ検証
- 大木HP計算

計算クラスをすべてMonoBehaviourにしない。

---

## 23. GameManagerの方針

すべての機能を管理する巨大な`GameManager`は作成しない。

禁止される責務集中の例：

```text
GameManager
├─ タップ処理
├─ 換金
├─ ガチャ
├─ セーブ
├─ 音
├─ 画面遷移
├─ キャラクター
├─ スキル
└─ 道具
```

必要に応じて、起動処理だけを担当する小さなクラスを作成する。

候補：

```text
GameBootstrap
```

`GameBootstrap`の責務：

- 必要なサービスを生成する
- セーブデータを読み込む
- 初期状態を生成する
- Controllerへ依存関係を渡す
- 初期画面を表示する

ゲームルールは持たない。

---

## 24. 依存関係の受け渡し

初期段階では、複雑なDIフレームワークを導入しない。

以下の方法を使用する。

- Inspector参照
- コンストラクタ
- `Initialize`メソッド
- GameBootstrapからの明示的な受け渡し

例：

```csharp
public void Initialize(
    PlayerGameState gameState,
    ChoppingService choppingService)
{
    _gameState = gameState;
    _choppingService = choppingService;
}
```

静的なSingletonを大量に使用しない。

---

## 25. Singletonの使用方針

原則としてSingletonをゲームシステムの中心に使用しない。

避ける例：

```text
GameManager.Instance
SaveManager.Instance
AudioManager.Instance
GachaManager.Instance
SkillManager.Instance
```

Singletonが増えると、以下の問題が起きる。

- 依存関係が見えにくい
- テストしにくい
- 初期化順序に依存する
- Scene切り替え時に問題が起きる
- Codexがどこからでも状態を書き換えやすくなる

オーディオなど、ゲーム全体で1つだけ存在することが自然な機能についても、必要性を確認してから導入する。

---

## 26. 初期試作で実装しない設計

初期試作では、以下を導入しない。

- 外部DIコンテナ
- 複雑なイベントバス
- Addressables
- オンライン通信
- クラウドセーブ
- 課金
- 広告
- Game Center
- 独自大数クラス
- 複数Sceneによる画面管理
- ECS
- DOTS
- 大規模な状態管理ライブラリ

必要になってから追加する。

---

## 27. Codexへの実装ルール

Codexへ実装を依頼する際は、以下を守る。

- `GAME_DESIGN_DOCUMENT.md`をゲーム仕様の正とする
- `UNITY_ARCHITECTURE.md`を実装構造の正とする
- 実装範囲を明記する
- 実装しない範囲を明記する
- 既存クラスの責務を壊さない
- UIでゲーム計算を行わない
- マスターデータと状態データを分離する
- 変更ファイルを最後に報告させる
- Unityエディタ上の手動設定を報告させる
- テスト方法を報告させる
- 未確定事項を推測で実装させない
- SceneやPrefabの変更を必要最小限にする

---

## 28. 初期実装の完了条件

最初の基本伐採機能は、以下を満たした時点で完了とする。

- 木のボタンを押せる
- 1回の伐採で丸太を1獲得する
- 所持丸太数が画面へ表示される
- 累計手動伐採回数が1回の成功につき1増える
- 伐採後に1秒のクールタイムが発生する
- クールタイム中は伐採できない
- クールタイム終了後は再度伐採できる
- クールタイム進行率がUIへ反映される
- 連打しても不正に丸太が増えない
- Consoleにエラーが表示されない
- UIが直接丸太を加算していない
- ゲーム状態、伐採処理、UI表示が分離されている

---

## 29. 初期実装予定ファイル

最初の実装では、以下のファイルを候補とする。

```text
Assets/Scripts/State/PlayerGameState.cs

Assets/Scripts/Domain/Chopping/ChoppingConfig.cs
Assets/Scripts/Domain/Chopping/ChoppingResult.cs
Assets/Scripts/Domain/Chopping/ChoppingService.cs

Assets/Scripts/Application/ChoppingController.cs

Assets/Scripts/Presentation/MainScreen/MainScreenView.cs
```

必要に応じて以下を追加する。

```text
Assets/Tests/EditMode/ChoppingServiceTests.cs
Assets/Tests/EditMode/PlayerGameStateTests.cs
```

初期段階でファイルを増やしすぎない。

---

## 30. 今後の拡張順序

基本伐採機能の完成後、以下の順番で拡張する。

1. 換金
2. 所持金
3. 攻撃力強化
4. スピード強化
5. 道具データ
6. 道具倍率
7. 道具破損
8. 修繕
9. 木こり自動伐採
10. キャラクター所持
11. メイン木こり編成
12. ガチャ
13. 重複レベルアップ
14. 商人
15. 鍛冶職人
16. セーブ
17. 放置報酬
18. 大木の進行段階
19. 大木伐採完了
20. iOS実機対応

各機能は、可能な限り独立したfeatureブランチで実装する。

---

## 31. 未決定事項

以下は今後の仕様書で決定する。

- 最小クールタイム
- 大木へのダメージと獲得丸太量を常に同じにするか
- 木こりの自動伐採の時間管理方式
- 道具破損中に木こりが働くか
- 鍛冶職人の効果の重複方式
- 放置中に大木へダメージを与えるか
- 使用する数値型の最終決定
- ゲーム起動時の依存関係構築方法
- 画面更新をイベント方式に移行する時期
- 独自大数クラスの導入時期
- 大木伐採後の状態管理

未決定事項は、推測で実装しない。

---

## 32. 本仕様書の変更ルール

アーキテクチャを変更する場合は、以下を行う。

1. 変更理由を明確にする
2. 既存の責務への影響を確認する
3. 本仕様書を先に更新する
4. 必要に応じてGitHub Issueを作成する
5. 実装を変更する
6. Pull Requestで相互確認する

実装に合わせて仕様書を黙って変更するのではなく、設計変更として扱う。
