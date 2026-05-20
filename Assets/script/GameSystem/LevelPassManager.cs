using UnityEngine;
using System.Collections;

public class LevelPassManager : MonoBehaviour
{
    [Header("測試")]
    public bool showPassCanvasOnStart = false;
    public GameObject testCanvasOnStart;

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

        if (showPassCanvasOnStart && testCanvasOnStart == null)
            testCanvasOnStart = passCanvas;

        if (testCanvasOnStart != null)
            StartCoroutine(ShowTestCanvasOnStart());
    }

    IEnumerator ShowTestCanvasOnStart()
    {
        yield return null;

        resultShown = true;
        Time.timeScale = 0f;

        if (passCanvas != null)
            passCanvas.SetActive(passCanvas == testCanvasOnStart);

        if (failCanvas != null)
            failCanvas.SetActive(failCanvas == testCanvasOnStart);

        if (testCanvasOnStart != null)
            testCanvasOnStart.SetActive(true);
    }

    void Update()
    {
        if (resultShown) return;
        if (gameTimer == null || healthUI == null) return;

        if (healthUI.IsDead())
        {
            ShowFail();
            Debug.Log("血量為 0，失敗");
            return;
        }

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
                ShowFail();
                Debug.Log("時間到，血量為 0，失敗");
            }
        }
    }

    private void ShowFail()
    {
        resultShown = true;
        Time.timeScale = 0f;

        if (failCanvas != null)
            failCanvas.SetActive(true);
    }
}
