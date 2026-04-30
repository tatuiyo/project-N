using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    public float speed = 10f;

    [Header("ジャンプ設定")]
    public float jumpForce = 5f; // ジャンプの強さ

    private Rigidbody rb;
    private Vector2 moveInput = Vector2.zero;
    private int groundContactCount = 0; // 地面に触れているオブジェクト数

    // 地面に触れているか（接触数が1以上なら地上）
    private bool IsGrounded => groundContactCount > 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Input Systemの移動入力（既存）
    void OnMove(InputValue movementValue)
    {
        moveInput = movementValue.Get<Vector2>();
    }

    // スペースキーでジャンプ（Input SystemのJumpアクション）
    void OnJump(InputValue value)
    {
        if (IsGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        // 移動
        Vector3 movement = new Vector3(moveInput.x, 0.0f, moveInput.y);
        rb.AddForce(movement * speed);
    }

    // 地面に触れたとき
    void OnCollisionEnter(Collision collision)
    {
        groundContactCount++;
    }

    // 地面から離れたとき
    void OnCollisionExit(Collision collision)
    {
        groundContactCount--;
        if (groundContactCount < 0) groundContactCount = 0;
    }
}