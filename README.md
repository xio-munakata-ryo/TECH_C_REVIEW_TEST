# TECH_C_REVIEW_TEST
ぷるりくえすとのてすと
## コーディング規約

### 命名規則
- **フィールド変数（クラス内で使用）**：`_camelCase`（先頭にアンダースコア）
- **公開変数**：`Public`（単語頭を大文字にそろえる）
- **ローカル変数（関数内のみ）**：`camelCase`（アンダースコアなし）
- **定数**：`MAX_SCORE`（スネークケース）

```csharp
[SerializeField] private float _duration;
private int _clearNum;
public int CountNum;

void Update()
{
    float currentTime = Time.time;
    int numToAdd = 1;
}
```

---

### 関数
- **PascalCase（大文字はじまり）**
- **「名詞＋動詞」形式で処理が明確になるように**
- **意味が明確な名前にする（型や役割がわかるように）**
- **/// で関数にサマリを付けること**

```csharp
private void SaveScore();
public int SetClearCount(int clearCount);
public void MovePlayer(Vector3 targetPosition);
private void PlayerEffect(string effectName, float duration);

/// <summary>
/// スコアを加算し、UIを更新する
/// </summary>
/// <param name="scoreToAdd">加算するスコア値</param>
public void AddScore(int scoreToAdd)
{
    _score += scoreToAdd;
    UpdateScoreUI();
}
```

---

### フィールド定義とインスペクター表示
- **外部から触らないもの → `private`**
- **外部から触るもの → `public`**
- **インスペクター上で値を確認したいもの → `[SerializeField]`**
- **インスペクター上で視認性高めるために、必要に応じて`[Hader("説明")]`を付けて分類・説明を明示する**
```csharp
[Header("移動速度"), SerializeField]
private float _moveSpeed;
[Header("ジャンプ力"), SerializeField]
private float _jumpForce;
[SerializeField] private float _speed;
public int ClearCount { get; private set; }
```

---


### 実装例
```csharp
/// <summary>
/// プレイヤーの移動とジャンプを制御するクラス
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("移動速度"), SerializeField]
    private float _moveSpeed;

    [Header("ジャンプ力"), SerializeField]
    private float _jumpForce;

    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// プレイヤーの移動処理
    /// </summary>
    /// <param name="direction">移動方向</param>
    public void Move(Vector3 direction)
    {
        Vector3 velocity = direction * _moveSpeed;
        _rigidbody.velocity = new Vector3(velocity.x, _rigidbody.velocity.y, velocity.z);
    }

    /// <summary>
    /// プレイヤーのジャンプ処理
    /// </summary>
    public void Jump()
    {
        _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
    }
}

```
---

## フォルダ構成について

今回のプロジェクトでは、チーム開発における管理のしやすさ・作業の分担・コンフリクトの軽減を目的として、以下のフォルダ構成を採用しています。

---

### ■ 基本方針

* **機能単位（Features）での管理を優先する**
* **共通素材は_Projectにまとめる**
* **フォルダの新規作成は原則相談すること**

---

### ■ フォルダ構成の例

```
Assets/
 ├─ _Project    ← 共通素材
 │   ├─ Animations
 │   ├─ Audios
 │   ├─ Effects
 │   ├─ Fonts
 │   ├─ Materials
 │   ├─ Models
 │   └─ Textures
 │
 ├─ Features    ← 機能単位で管理（メイン）
 │   ├─ Player
 │   │   ├─ Scripts
 │   │   ├─ Prefabs
 │   │   ├─ Animations
 │   │   └─ Effects
 │   │
 │   ├─ Enemy
 │   ├─ Stage
 │   └─ UI
 │
 ├─ Scenes
 │   ├─ Main    ← メインゲーム用シーン
 │   ├─ Test    ← 各自の検証・テスト用シーン
```

---

### ■ 各フォルダの役割

#### `_Project`

* プロジェクト全体で使用する共通素材を格納
* 例：共通SE、フォント、マテリアルなど

---

#### `Features`

* プロジェクトのメイン管理フォルダ
* 機能単位でフォルダを分け、その中に関連するアセットをまとめる

例：

* Playerに関するスクリプト・Prefab・Animationはすべて `Features/Player` に格納

---

#### `Scenes`

* `Main`：実際にゲームで使用するシーン
* `Test`：各自が自由に検証を行うためのシーン

※ Mainシーンは基本的に担当者のみが編集すること

---

### ■ 運用ルール

* 新しいフォルダを作成する場合は必ず一度相談すること
* アセットは必ず適切なフォルダに配置すること（直下に置かない）
* 機能に関するアセットは必ず `Features` 内にまとめること
* 共通で使用する素材のみ `_Project` に配置すること

---

### ■ 注意点

* 同一アセットの重複作成を避けること
* フォルダ構成を勝手に変更しないこと
* 命名規則に従ってファイルを作成すること

---

本ルールを守ることで、作業効率の向上およびトラブル防止につながります。
チーム全体で統一した運用を心がけてください。

---

## Git運用ルール

今回のプロジェクトでは、複数人での開発におけるコンフリクト防止と作業効率向上のため、以下のGit運用ルールを採用します。

---

### ■ ブランチ構成

```
main        ← リリース・完成版（常に安定）
develop     ← 開発のベースブランチ
feature/*   ← 各作業用ブランチ
```

---

### ■ 各ブランチの役割

#### `main`

* 完成版のみを管理するブランチ
* 基本的に直接コミット禁止

---

#### `develop`

* 開発の中心となるブランチ
* 各機能はここに統合される

---

#### `feature/*`

* 各作業ごとに作成するブランチ

例：

```
feature/player-move
feature/enemy-ai
feature/ui-score
```

---

### ■ 作業の流れ（GitHub Desktop）

本プロジェクトでは、GitHub Desktopを使用して作業を行います。
以下の手順に従って作業してください。

---

### ① 最新の状態を取得（必須）

1. GitHub Desktopを開く
2. 上部メニューの「Fetch origin」をクリック
3. 「Pull origin」をクリック

※ 作業前は必ず実行すること

---

### ② 作業前の必須手順（重要）

作業開始前は、必ず最新の `develop` ブランチの内容を自分のブランチに取り込んでください。
これを行わないと、コンフリクトや不具合の原因になります。

#### ■ 手順（GitHub Desktop）

1. 一度 `develop` ブランチに切り替える
2. 「Pull origin」で最新状態を取得
3. 自分の作業ブランチに戻る
4. 「Branch」→「Update from develop」を実行

---

### ③ ブランチを作成

1. 上部の「Current Branch」をクリック
2. 「New Branch」を選択
3. ブランチ名を入力して作成

#### ■ ブランチ名の例

```
feature/player-move
feature/enemy-ai
feature/ui-score
```

---

### ④ 作業・コミット

1. Unityで作業を行う
2. GitHub Desktopに戻る
3. 変更ファイルを確認
4. 左下にコミットメッセージを入力
5. 「Commit to feature/〇〇」をクリック

#### ■ コミットメッセージ例

```
プレイヤーの移動処理を追加
ジャンプのバグを修正
```

---

### ⑤ リモートに反映（Push）

1. 「Push origin」をクリック

---

### ⑥ developにマージ（Pull Request）

1. 「Branch」→「Create Pull Request」をクリック
2. GitHubの画面で内容を確認
3. レビュー担当者に確認を依頼する

※ Pull Requestを作成しただけではマージはされません

---

### ■ マージについて

* マージはレビュー後に実施する
* 原則として自己判断でマージしないこと
* レビュー完了後、問題がなければ担当者がマージを行う

---

### ⑦ ブランチ削除

作業が終わったブランチは削除する

---

### ■ 注意点

* 作業前・作業後は必ず「Pull / Push」を行うこと
* 作業開始前は必ずdevelopを取り込むこと
* 長時間作業した場合は途中でも取り込むこと
* 同じシーンを複数人で編集しない
* コンフリクトが出た場合は必ず相談する

---

### ■ よくあるミス

* Pullせずに作業してコンフリクト発生
* developを取り込まずに作業する
* mainブランチで作業してしまう
* コミットせずにUnityを閉じる

---

必ずこの手順に従い、チーム全体で統一した運用を行ってください。

