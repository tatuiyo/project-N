using UnityEngine;

public class PickUp : MonoBehaviour
{
    void Update()
    {
        // オブジェクトを継続的に回転させる
        transform.Rotate(new Vector3(15, 30, 45) * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // トリガーに入ったオブジェクトがプレイヤーかどうかを確認
        if (other.gameObject.CompareTag("Player"))
        {
            // PickUpManagerに「1個取った」と通知する
            if (PickUpManager.Instance != null)
            {
                PickUpManager.Instance.OnPickedUp();
            }

            // このPickupオブジェクトを破棄（収集）
            Destroy(gameObject);
        }
    }
}