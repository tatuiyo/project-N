using UnityEngine;

public class PickUp : MonoBehaviour
{
    void Update()
    {
        // オブジェクトを継続的に回転させる
        // X軸around 15度/秒、Y軸around 30度/秒、Z軸around 45度/秒の速度で回転
        transform.Rotate(new Vector3(15, 30, 45) * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // トリガーに入ったオブジェクトがプレイヤーかどうかを確認
        if (other.gameObject.CompareTag("Player"))
        {
            // プレイヤーだった場合、このPickupオブジェクトを破棄（収集）
            Destroy(gameObject);

            // ここに、スコア加算やサウンド再生などの追加処理を入れることができます
        }
    }
}