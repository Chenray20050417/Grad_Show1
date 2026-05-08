using UnityEngine;

public class LevelPassManager : MonoBehaviour
{
    [Header("Canvas")]
    public GameObject passCanvas;
    public GameObject failCanvas;

    [Header("系統")]
    public GameTimer gameTimer;
    public HealthUI healthUI;

    private bool resultShown = false;

    void Start()
    {
        if (passCanvas != null)
            passCanvas.SetActive(false);

        if (failCanvas != null)
            failCanvas.SetActive(false);
    }

    void Update()
    {
        if (resultShown) return;
        if (gameTimer == null || healthUI == null) return;

        if (gameTimer.currentTime <= 0)
        {
            resultShown = true;
            Time.timeScale = 0f;

            if (!healthUI.IsDead())
            {
                if (passCanvas != null)
                    passCanvas.SetActive(true);

                Debug.Log("時間到，血量大於 0，過關");
            }
            else
            {
                if (failCanvas != null)
                    failCanvas.SetActive(true);

                Debug.Log("時間到，血量為 0，失敗");
            }
        }
    }
}