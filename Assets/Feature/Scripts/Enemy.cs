using UnityEngine;

public class Enemy : MonoBehaviour
{
    void Start()
    {
        // 物理挙動をコントロールするRigidbodyを取得
        Rigidbody rb = GetComponent<Rigidbody>();
        
        if (rb != null)
        {
            // 下方向（Vector3.down）に、3から12のランダムな初期速度を与える
            float randomSpeed = Random.Range(3f, 12f);
            rb.linearVelocity = Vector3.down * randomSpeed; 
            // ※Unity 2022以前をお使いの場合は「rb.velocity」に書き換えてください
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            Destroy(gameObject);
        }
    }
}
