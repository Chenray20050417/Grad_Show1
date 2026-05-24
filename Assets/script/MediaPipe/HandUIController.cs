using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Mediapipe.Tasks.Components.Containers;

public class HandUIController : MonoBehaviour
{
    public RectTransform fingerCursor;

    [Header("Hover 深淺效果")]
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(0.65f, 0.65f, 0.65f, 1f);

    private Button currentButton;
    private Image currentImage;

    private bool wasFist = false;
    private bool requestClick = false;

    public void CheckHand(NormalizedLandmarks lm)
    {
        if (lm.landmarks == null || lm.landmarks.Count < 21)
            return;

        bool isFist =
            lm.landmarks[8].y > lm.landmarks[6].y &&
            lm.landmarks[12].y > lm.landmarks[10].y &&
            lm.landmarks[16].y > lm.landmarks[14].y &&
            lm.landmarks[20].y > lm.landmarks[18].y;

        if (isFist && !wasFist)
            requestClick = true;

        wasFist = isFist;
    }

    void Update()
    {
        UpdateHover();

        if (requestClick)
        {
            requestClick = false;

            if (currentButton != null)
            {
                Debug.Log("握拳按下：" + currentButton.name);

                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayGestureClick();

                currentButton.onClick.Invoke();
            }
        }
    }

    void UpdateHover()
    {
        Button hoverButton = GetButtonUnderCursor();

        if (hoverButton == currentButton)
            return;

        if (currentImage != null)
            currentImage.color = normalColor;

        currentButton = hoverButton;
        currentImage = null;

        if (currentButton != null)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayButtonHover();

            currentImage = currentButton.GetComponent<Image>();

            if (currentImage != null)
                currentImage.color = hoverColor;
        }
    }

    Button GetButtonUnderCursor()
    {
        if (fingerCursor == null || EventSystem.current == null)
            return null;

        PointerEventData pointerData = new PointerEventData(EventSystem.current);

        pointerData.position =
            RectTransformUtility.WorldToScreenPoint(null, fingerCursor.position);

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            Button btn = result.gameObject.GetComponent<Button>();
            if (btn != null) return btn;

            btn = result.gameObject.GetComponentInParent<Button>();
            if (btn != null) return btn;
        }

        return null;
    }
}
