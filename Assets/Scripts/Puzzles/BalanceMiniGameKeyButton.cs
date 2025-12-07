using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class BalanceMiniGameKeyButton : MonoBehaviour
{
    [Header("Refs")]
    public Image image;                 // 버튼 배경 이미지
    public TextMeshProUGUI label;       // 가운데 키 문자(Q/W/E/A/S/D)

    [Header("Sprites")]
    public Sprite normalSprite;         // 기본 상태 이미지
    public Sprite currentSprite;        // 현재 눌러야 하는 상태 이미지
    public Sprite clearedSprite;        // 이미 눌러서 클리어된 상태 이미지

    [Header("Size")]
    public float currentWidth = 155f;   // 선택된 키 가로
    public float currentHeight = 120f;  // 선택된 키 세로

    public RectTransform targetRect;

    RectTransform _rect;
    Vector2 _baseSize;
    bool _sizeCached;

    void Awake()
    {
        CacheBaseSize();
    }

    void OnValidate()
    {
        CacheBaseSize();
    }

    void CacheBaseSize()
    {
        if (targetRect != null)
            _rect = targetRect;
        else if (_rect == null)
            _rect = GetComponent<RectTransform>();

        if (_rect != null && !_sizeCached)
        {
            _baseSize = _rect.sizeDelta;
            _sizeCached = true;
        }
    }

    public void Setup(char key)
    {
        CacheBaseSize();

        if (label != null)
            label.text = key.ToString();

        SetNormal();
    }

    public void SetNormal()
    {
        CacheBaseSize();

        if (image != null)
            image.sprite = normalSprite;

        if (_rect != null)
            _rect.sizeDelta = _baseSize;

        transform.localScale = Vector3.one;
    }

    public void SetCurrent()
    {
        CacheBaseSize();

        if (image != null)
            image.sprite = currentSprite != null ? currentSprite : normalSprite;

        if (_rect != null)
            _rect.sizeDelta = new Vector2(currentWidth, currentHeight);

        transform.localScale = Vector3.one;
    }

    public void SetCleared()
    {
        CacheBaseSize();

        if (image != null)
            image.sprite = clearedSprite != null ? clearedSprite : normalSprite;

        if (_rect != null)
            _rect.sizeDelta = _baseSize;

        transform.localScale = Vector3.one;
    }
}
