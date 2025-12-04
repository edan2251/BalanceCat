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
    public TMP_Text scoreText;

    public string weightFormat = "{0}";
    public string scoreFormat = "{0}";

    public bool followCursor = true;                // 커서 따라갈지 여부
    public Vector2 cursorOffset = new Vector2(16f, -16f); // 커서에서 우하단 오프셋
    public bool clampToScreen = true;               // 화면 밖으로 안 나가게
    public Vector2 screenPadding = new Vector2(8f, 8f);

    RectTransform _rt;
    Canvas _rootCanvas;

    // 툴팁 코너 타입
    private enum TooltipCorner
    {
        BottomRight,
        TopRight,
        BottomLeft,
        TopLeft
    }

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _rootCanvas = GetComponentInParent<Canvas>();

        if (_rt != null)
        {
            // 기본은 우하단(커서 기준) → pivot을 좌상단으로 시작
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

    // 코너별 pivot
    Vector2 GetPivotForCorner(TooltipCorner corner)
    {
        switch (corner)
        {
            case TooltipCorner.BottomRight: return new Vector2(0f, 1f); // 좌상
            case TooltipCorner.TopRight: return new Vector2(0f, 0f); // 좌하
            case TooltipCorner.BottomLeft: return new Vector2(1f, 1f); // 우상
            case TooltipCorner.TopLeft: return new Vector2(1f, 0f); // 우하
        }
        return new Vector2(0f, 1f);
    }

    // 코너별 커서 오프셋 (cursorOffset 절대값 사용)
    Vector2 GetOffsetForCorner(TooltipCorner corner, Vector2 baseAbsOffset)
    {
        switch (corner)
        {
            case TooltipCorner.BottomRight: // 커서 기준 우/하
                return new Vector2(+baseAbsOffset.x, -baseAbsOffset.y);
            case TooltipCorner.TopRight:    // 우/상
                return new Vector2(+baseAbsOffset.x, +baseAbsOffset.y);
            case TooltipCorner.BottomLeft:  // 좌/하
                return new Vector2(-baseAbsOffset.x, -baseAbsOffset.y);
            case TooltipCorner.TopLeft:     // 좌/상
                return new Vector2(-baseAbsOffset.x, +baseAbsOffset.y);
        }
        return new Vector2(+baseAbsOffset.x, -baseAbsOffset.y);
    }

    // 코너 자동 전환 + 화면 밖 방지
    void UpdatePositionToCursor()
    {
        RectTransform parentRect = _rt.parent as RectTransform;
        if (parentRect == null) return;

        Camera cam = _rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : _rootCanvas.worldCamera;

        Vector2 mousePos = Input.mousePosition;

        // clampToScreen 끄면: 기존처럼 우하단 고정 + 단순 offset만 사용
        if (!clampToScreen)
        {
            Vector2 screenPos = mousePos + cursorOffset;

            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPos, cam, out localPoint))
            {
                _rt.localPosition = localPoint;
            }
            return;
        }

        // === 화면 안에 최대한 들어오게 코너 자동 선택 ===
        // Tooltip 실제 픽셀 크기
        float scaleFactor = (_rootCanvas != null) ? _rootCanvas.scaleFactor : 1f;
        float w = _rt.rect.width * scaleFactor;
        float h = _rt.rect.height * scaleFactor;

        Vector2 absOffset = new Vector2(Mathf.Abs(cursorOffset.x), Mathf.Abs(cursorOffset.y));

        TooltipCorner[] order =
        {
            TooltipCorner.BottomRight, // 기본
            TooltipCorner.TopRight,
            TooltipCorner.BottomLeft,
            TooltipCorner.TopLeft
        };

        TooltipCorner chosenCorner = TooltipCorner.BottomRight;
        Vector2 chosenPivot = GetPivotForCorner(TooltipCorner.BottomRight);
        Vector2 chosenScreenPos = mousePos + GetOffsetForCorner(TooltipCorner.BottomRight, absOffset);
        bool found = false;

        foreach (var corner in order)
        {
            Vector2 pivot = GetPivotForCorner(corner);
            Vector2 offset = GetOffsetForCorner(corner, absOffset);
            Vector2 screenPos = mousePos + offset;

            // pivot 기준으로 사각형 화면 영역 계산
            float left = screenPos.x - pivot.x * w;
            float right = screenPos.x + (1f - pivot.x) * w;
            float bottom = screenPos.y - pivot.y * h;
            float top = screenPos.y + (1f - pivot.y) * h;

            if (left >= screenPadding.x &&
                right <= Screen.width - screenPadding.x &&
                bottom >= screenPadding.y &&
                top <= Screen.height - screenPadding.y)
            {
                chosenCorner = corner;
                chosenPivot = pivot;
                chosenScreenPos = screenPos;
                found = true;
                break;
            }
        }

        // 네 코너 다 안 들어오면, 우하단 기준으로 위치만 Clamp
        if (!found)
        {
            TooltipCorner corner = TooltipCorner.BottomRight;
            Vector2 pivot = GetPivotForCorner(corner);
            Vector2 offset = GetOffsetForCorner(corner, absOffset);
            Vector2 screenPos = mousePos + offset;

            float minX = screenPadding.x + pivot.x * w;
            float maxX = Screen.width - screenPadding.x - (1f - pivot.x) * w;
            float minY = screenPadding.y + pivot.y * h;
            float maxY = Screen.height - screenPadding.y - (1f - pivot.y) * h;

            screenPos.x = Mathf.Clamp(screenPos.x, minX, maxX);
            screenPos.y = Mathf.Clamp(screenPos.y, minY, maxY);

            chosenCorner = corner;
            chosenPivot = pivot;
            chosenScreenPos = screenPos;
        }

        // 선택된 코너의 pivot 적용
        _rt.pivot = chosenPivot;

        // 부모 Rect 기준 로컬 좌표로 배치
        Vector2 finalLocalPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, chosenScreenPos, cam, out finalLocalPoint))
        {
            _rt.localPosition = finalLocalPoint;
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
            weightText.text = string.Format(weightFormat, item.data.weight) + "Kg";

        if (scoreText)
            scoreText.text = string.Format(scoreFormat, item.data.score) + "점";

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (gameObject.activeSelf)
            gameObject.SetActive(false);
    }
}
