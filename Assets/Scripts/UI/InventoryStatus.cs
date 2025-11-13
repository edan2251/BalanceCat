using UnityEngine;
using UnityEngine.UI;

public class InventoryVisualizer : MonoBehaviour
{
    public Image leftFillImage; 
    public Image rightFillImage; 

    private RectTransform _leftRect;
    private RectTransform _rightRect;

    [Tooltip("왼쪽 인벤토리 무게")]
    [Range(0, 100)]
    public float leftWeight = 0f;

    [Tooltip("오른쪽 인벤토리 무게")]
    [Range(0, 100)]
    public float rightWeight = 0f;

    [Tooltip("최대 무게")]
    public float maxWeight = 100f;
    //weightAmount 는 그냥 왼쪽 오른쪽 더해서 currentWeight로 설정했습니다

    void Start()
    {
        _leftRect = leftFillImage.GetComponent<RectTransform>();
        _rightRect = rightFillImage.GetComponent<RectTransform>();

        _leftRect.anchoredPosition = Vector2.zero;
        _leftRect.sizeDelta = Vector2.zero;
        _rightRect.anchoredPosition = Vector2.zero;
        _rightRect.sizeDelta = Vector2.zero;
    }

    void Update()
    {
        UpdateVisuals(leftWeight, rightWeight, maxWeight);
    }

    public void UpdateVisuals(float leftWeight, float rightWeight, float maxWeight)
    {
        float currentWeight = leftWeight + rightWeight;

        float fillPercent = 0f;
        if (maxWeight > 0)
        {
            fillPercent = Mathf.Clamp01(currentWeight / maxWeight);
        }
        leftFillImage.fillAmount = fillPercent;
        rightFillImage.fillAmount = fillPercent;

        float leftRatio = 0.5f;

        if (currentWeight > 0)
        {
            leftRatio = leftWeight / currentWeight;
        }

        if (fillPercent <= 0)
        {
            leftFillImage.gameObject.SetActive(false);
            rightFillImage.gameObject.SetActive(false);
        }
        else
        {
            leftFillImage.gameObject.SetActive(true);
            rightFillImage.gameObject.SetActive(true);
        }

        _leftRect.anchorMax = new Vector2(leftRatio, 1);

        _rightRect.anchorMin = new Vector2(leftRatio, 0);
    }
}