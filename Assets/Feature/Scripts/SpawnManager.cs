using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    private Transform playerTransform;

    void Start()
    {
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }

        InvokeRepeating("SpawnEnemy", 1f, 0.5f);
    }

    void SpawnEnemy()
    {
        float spawnX = 0f;

        // 【難易度変化の計算】
        // 基本の20% ＋「生存秒数 × 2%」を足していく（例: 20秒生き残ると 20 + 40 = 60% になる）
        float currentAimChance = 20f + (GameManager.survivalTime * 2f);
        
        // 狙撃確率が上がりすぎて無理ゲーにならないよう、最大80%でストップさせる
        currentAimChance = Mathf.Min(currentAimChance, 80f);

        // デバッグログに現在の狙撃確率を表示（確認用）
        Debug.Log("現在の狙撃確率: " + currentAimChance.ToString("F1") + "%");

        // 計算した確率でプレイヤーの頭上を狙う
        if (Random.Range(0f, 100f) < currentAimChance && playerTransform != null)
        {
            spawnX = playerTransform.position.x;
            Debug.Log("★プレイヤー狙撃！");
        }
        else
        {
            spawnX = Random.Range(-5f, 5f);
        }

        Vector3 spawnPos = new Vector3(spawnX, 10f, 0f);
        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }
}
