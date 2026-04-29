using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // プレイヤーの移動速度
    public float speed = 10f;

    // Rigidbodyコンポーネントへの参照
    private Rigidbody rb;

    // 入力された移動ベクトル
    private Vector2 moveInput = Vector2.zero;

    void Start()
    {
        // Rigidbodyコンポーネントを取得
        rb = GetComponent<Rigidbody>();
    }

    // Input Systemからの移動入力を処理するメソッド
    void OnMove(InputValue movementValue)
    {
        // 入力値を2Dベクトルとして取得
        moveInput = movementValue.Get<Vector2>();
    }

    void FixedUpdate()
    {
        // 入力ベクトルを3D空間の移動ベクトルに変換
        // Y軸（垂直方向）の移動は0に設定
        Vector3 movement = new Vector3(moveInput.x, 0.0f, moveInput.y);

        // Rigidbodyに力を加えてプレイヤーを移動
        rb.AddForce(movement * speed);
    }
}