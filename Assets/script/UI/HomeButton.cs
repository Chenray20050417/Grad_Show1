using UnityEngine;

public class HomeButton : MonoBehaviour
{
    public void GoHome()
    {
        Time.timeScale = 1f;
        SceneTransitionManager.LoadScene("MainMenu");
    }
}
