using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectModeController : MonoBehaviour
{
    public string selectModeSceneName = "selectmode";
    public string storySceneName = "CinematicTrailer";
    public string shoulderSceneName = "level1";
    public string squatSceneName = "level2";
    public string boxingSceneName = "Level3";

    private static SelectModeController instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null)
            return;

        GameObject obj = new GameObject("SelectModeController");
        instance = obj.AddComponent<SelectModeController>();
        DontDestroyOnLoad(obj);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryBindCurrentScene();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == selectModeSceneName)
            TryBindCurrentScene();
    }

    private void TryBindCurrentScene()
    {
        if (SceneManager.GetActiveScene().name != selectModeSceneName)
            return;

        BindButton("storybtn", StartStory);
        BindButton("shoulderbtn", StartShoulderTrial);
        BindButton("squatbtn", StartSquatTrial);
        BindButton("boxingbtn", StartBoxingTrial);
    }

    private void BindButton(string objectName, UnityEngine.Events.UnityAction action)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj == null)
        {
            Debug.LogWarning("找不到選擇模式按鈕：" + objectName);
            return;
        }

        Button button = obj.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogWarning(objectName + " 沒有 Button component");
            return;
        }

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);

        if (obj.GetComponent<SelectModeHoverShine>() == null)
            obj.AddComponent<SelectModeHoverShine>();
    }

    public void StartStory()
    {
        GameModeState.StartStoryMode();
        SceneTransitionManager.LoadScene(storySceneName);
    }

    public void StartShoulderTrial()
    {
        StartTrial(shoulderSceneName);
    }

    public void StartSquatTrial()
    {
        StartTrial(squatSceneName);
    }

    public void StartBoxingTrial()
    {
        StartTrial(boxingSceneName);
    }

    private void StartTrial(string sceneName)
    {
        GameModeState.StartTrialMode();
        SceneTransitionManager.LoadScene(sceneName);
    }
}
