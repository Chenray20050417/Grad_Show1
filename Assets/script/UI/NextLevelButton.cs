using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevelButton : MonoBehaviour
{
    [Header("下一關場景名稱")]
    [Tooltip("可留空。留空時會載入 Build Settings 裡目前場景的下一個場景。")]
    public string nextSceneName;

    [Header("沒有下一關時回到")]
    public string fallbackSceneName = "MainMenu";

    public void GoNextLevel()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log("前往下一關：" + nextSceneName);
            SceneManager.LoadScene(nextSceneName);
            return;
        }

        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log("前往下一關 Index：" + nextSceneIndex);
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("沒有下一關，回到：" + fallbackSceneName);
            SceneManager.LoadScene(fallbackSceneName);
        }
    }
}
