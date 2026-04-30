using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    /// <summary>
    /// 現在のシーンの次のシーンへ移動する
    /// </summary>
    public static void LoadNextScene()
    {
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            // 全シーンクリア！タイトルや結末シーンへ
            Debug.Log("全シーンクリア！");
            SceneManager.LoadScene(0); // タイトルに戻る場合
        }
    }
}
