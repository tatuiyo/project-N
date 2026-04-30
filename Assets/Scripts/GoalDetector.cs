using UnityEngine;

public class GoalDetector : MonoBehaviour
{
    [Header("玉のTag名")]
    public string ballTag = "Ball"; // 玉オブジェクトのTagをInspectorで合わせる

    private bool triggered = false; // 二重発火防止

    private void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.CompareTag(ballTag))
        {
            triggered = true;
            Debug.Log("ゴール！次のシーンへ移動します。");
            SceneLoader.LoadNextScene();
        }
    }
}
