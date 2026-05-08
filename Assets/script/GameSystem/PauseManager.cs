using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseCanvas;

    public bool IsPaused { get; private set; } = false;

    void Start()
    {
        if (pauseCanvas != null)
            pauseCanvas.SetActive(false);

        Time.timeScale = 1f;
    }

    public void ShowPause()
    {
        if (IsPaused) return;

        IsPaused = true;

        if (pauseCanvas != null)
            pauseCanvas.SetActive(true);

        Time.timeScale = 0f;
        Debug.Log("暫停畫布顯示");
    }

    public void ResumeGame()
    {
        IsPaused = false;

        if (pauseCanvas != null)
            pauseCanvas.SetActive(false);

        Time.timeScale = 1f;
        Debug.Log("繼續遊戲");
    }
}