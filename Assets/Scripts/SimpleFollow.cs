using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    // 追跡する対象（Playerオブジェクト）
    public Transform target;

    // カメラとターゲットの初期相対位置
    private Vector3 offset;

    void Start()
    {
        // ゲーム開始時のカメラとターゲットの相対位置を計算し、保存
        offset = transform.position - target.position;
    }

    void LateUpdate()
    {
        // ターゲットの現在位置に、初期オフセットを加えてカメラの位置を更新
        transform.position = target.position + offset;
    }
}