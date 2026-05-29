using UnityEngine;

public class DisplayManager : MonoBehaviour
{
    public bool forceFullscreen = true;
    public int targetFrameRate = 60;

    private void Awake()
    {
        Application.targetFrameRate = targetFrameRate;

        if (!forceFullscreen)
            return;

        Resolution resolution = Screen.currentResolution;
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        Screen.SetResolution(resolution.width, resolution.height, FullScreenMode.FullScreenWindow);
    }

    private void Start()
    {
        for (int i = 1; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
        }
    }
}
