using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject enemyPrefab; // 生成する障害物

    void Start()
    {
        // 1秒後から、0.5秒ごとに「SpawnEnemy」関数を繰り返し実行する
        InvokeRepeating("SpawnEnemy", 1f, 0.5f);
    }

    void SpawnEnemy()
    {
        // 左右-5から5のランダムな位置に、上空（高さ10）から落とす
        float randomX = Random.Range(-5f, 5f);
        Vector3 spawnPos = new Vector3(randomX, 10f, 0f);
        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }
}
