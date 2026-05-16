using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartButton : MonoBehaviour
{
    public void RestartLevel()
    {
        Time.timeScale = 1f;

        SceneTransitionManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
