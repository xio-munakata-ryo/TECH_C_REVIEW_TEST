using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // 1. 新しいInput Systemを使うおまじないを追加

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    
    // 他のスクリプト（SpawnManager）から生存時間を読み取れるように公開
    public static float survivalTime = 0f;  

    void Start()
    {
        Time.timeScale = 1f;
        survivalTime = 0f;
    }

    void Update()
    {
        // ゲームが動いている（時間が止まっていない）ならカウントアップ
        if (Time.timeScale > 0f)
        {
            survivalTime += Time.deltaTime;
            scoreText.text = "生存時間: " + survivalTime.ToString("F2") + " 秒";
        }
        else
        {
            // 2. 新しいInput Systemの方式で、スペースキーまたはRキーの入力を判定
            if (Keyboard.current != null)
            {
                if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.rKey.wasPressedThisFrame)
                {
                    // 現在アクティブなシーンをはじめから再読み込みする
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }
        }
    }
}
