using UnityEngine;

/// <summary>
/// アイテム取得数を管理し、目標数に達したらシーン遷移する
/// </summary>
public class PickUpManager : MonoBehaviour
{
    public static PickUpManager Instance { get; private set; }

    [Header("クリア条件")]
    public int requiredCount = 3; // 何個取ったらクリアか

    private int currentCount = 0;

    void Awake()
    {
        // シングルトン（1つだけ存在する管理役）
        Instance = this;
    }

    /// <summary>
    /// PickUp.cs から呼ばれる。1個取得ごとに呼ぶ。
    /// </summary>
    public void OnPickedUp()
    {
        currentCount++;
        Debug.Log($"アイテム取得: {currentCount} / {requiredCount}");

        if (currentCount >= requiredCount)
        {
            Debug.Log("全部集めた！会話シーンへ移動！");
            SceneLoader.LoadNextScene();
        }
    }
}
