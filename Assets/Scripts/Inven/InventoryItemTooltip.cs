using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemTooltip : MonoBehaviour
{
    [Header("UI Refs")]
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text weightText;

    public string weightFormat = "{0}";

    public bool followCursor = true;                // 커서 따라갈지 여부
    public Vector2 cursorOffset = new Vector2(16f, -16f); // 커서에서 우하단 오프셋
    public bool clampToScreen = true;               // 화면 밖으로 안 나가게
    public Vector2 screenPadding = new Vector2(8f, 8f);

    RectTransform _rt;
    Canvas _rootCanvas;

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _rootCanvas = GetComponentInParent<Canvas>();

        if (_rt != null)
        {
            _rt.pivot = new Vector2(0f, 1f);
        }

        Hide();
    }
    private void Update()
    {
        // 활성 + followCursor 일 때만 위치 갱신
        if (!followCursor) return;
        if (_rt == null || _rootCanvas == null) return;
        if (!gameObject.activeSelf) return;

        UpdatePositionToCursor();
    }

    void UpdatePositionToCursor()
    {
        // 마우스 위치 기준, 우하단으로 약간 띄우기
        Vector2 screenPos = (Vector2)Input.mousePosition + cursorOffset;

        if (clampToScreen)
        {
            float x = Mathf.Clamp(screenPos.x, screenPadding.x, Screen.width - screenPadding.x);
            float y = Mathf.Clamp(screenPos.y, screenPadding.y, Screen.height - screenPadding.y);
            screenPos = new Vector2(x, y);
        }

        Camera cam = _rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : _rootCanvas.worldCamera;

        //부모 Rect 기준 로컬 좌표로 배치
        RectTransform parentRect = _rt.parent as RectTransform;
        if (parentRect == null) return;

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPos, cam, out localPoint))
        {
            _rt.localPosition = localPoint;
        }
    }
    public void Show(ItemInstance item)
    {
        if (item == null || item.data == null)
        {
            Hide();
            return;
        }

        if (iconImage)
            iconImage.sprite = item.data.icon;

        if (nameText)
            nameText.text = item.data.displayName;

        if (descriptionText)
            descriptionText.text = item.data.description;

        if (weightText)
            weightText.text = string.Format(weightFormat, item.data.weight);

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (gameObject.activeSelf)
            gameObject.SetActive(false);
    }
}
