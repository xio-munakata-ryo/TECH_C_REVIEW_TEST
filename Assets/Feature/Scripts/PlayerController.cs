using UnityEngine;
using UnityEngine.InputSystem; // 新しいInput Systemを使うおまじない

public class PlayerController : MonoBehaviour
{
    void Update()
    {
        float moveX = 0f;

        // キーボードの左右キー（A/D または ←/→）の入力を取得
        if (Keyboard.current != null)
        {
            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
            {
                moveX = -1f;
            }
            if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
            {
                moveX = 1f;
            }
        }

        // プレイヤーを左右に移動させる
        transform.Translate(new Vector3(moveX, 0, 0) * 10f * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("ゲームオーバー！");
            Time.timeScale = 0f;
        }
    }
}
