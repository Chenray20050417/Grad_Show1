using UnityEngine;
using UnityEngine.Video;

public class EndingVideoController : MonoBehaviour
{
    [Header("影片")]
    public VideoClip endingClip;

    [Header("播放完前往")]
    public string nextSceneName = "ComicScene4";

    [Header("跳過")]
    public bool allowKeyboardSkip = true;

    private VideoPlayer videoPlayer;
    private bool isLoadingNextScene;

    private void Start()
    {
        Time.timeScale = 1f;

        Camera targetCamera = Camera.main;
        if (targetCamera == null)
            targetCamera = FindObjectOfType<Camera>();

        if (targetCamera == null)
        {
            Debug.LogError("EndingVideoScene 找不到 Camera");
            LoadNextScene();
            return;
        }

        if (endingClip == null)
        {
            Debug.LogError("EndingVideoScene 沒有指定影片");
            LoadNextScene();
            return;
        }

        videoPlayer = targetCamera.gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
        videoPlayer.targetCamera = targetCamera;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        videoPlayer.isLooping = false;
        videoPlayer.clip = endingClip;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.Prepare();
    }

    private void Update()
    {
        if (!allowKeyboardSkip)
            return;

        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetMouseButtonDown(0))
        {
            LoadNextScene();
        }
    }

    private void OnDestroy()
    {
        if (videoPlayer == null)
            return;

        videoPlayer.loopPointReached -= OnVideoFinished;
        videoPlayer.prepareCompleted -= OnVideoPrepared;
    }

    private void OnVideoPrepared(VideoPlayer source)
    {
        source.Play();
    }

    private void OnVideoFinished(VideoPlayer source)
    {
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (isLoadingNextScene)
            return;

        isLoadingNextScene = true;

        if (videoPlayer != null)
            videoPlayer.Pause();

        SceneTransitionManager.LoadScene(nextSceneName);
    }
}
