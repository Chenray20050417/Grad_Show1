using UnityEngine;

public class WebCamDeviceLogger : MonoBehaviour
{
    void Start()
    {
        WebCamDevice[] devices = WebCamTexture.devices;

        Debug.Log("=== 攝影機列表 ===");

        for (int i = 0; i < devices.Length; i++)
        {
            Debug.Log(i + " : " + devices[i].name);
        }
    }
}