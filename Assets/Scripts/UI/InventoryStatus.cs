using UnityEngine;
using UnityEngine.UI;

public class InventoryVisualizer : MonoBehaviour
{
    public Image leftFillImage; 
    public Image rightFillImage; 

    private RectTransform _leftRect;
    private RectTransform _rightRect;

    public InventorySideBias sideBias;
    public Inventory inventory;

    [Tooltip("왼쪽 인벤토리 무게")]
    [Range(0, 100)]
    public float leftWeight = 0f;

    [Tooltip("오른쪽 인벤토리 무게")]
    [Range(0, 100)]
    public float rightWeight = 0f;

    [Tooltip("최대 무게")]
    public float maxWeight;
    //weightAmount 는 그냥 왼쪽 오른쪽 더해서 currentWeight로 설정했습니다

    void Start()
    {
        _leftRect = leftFillImage.GetComponent<RectTransform>();
        _rightRect = rightFillImage.GetComponent<RectTransform>();

        _leftRect.anchoredPosition = Vector2.zero;
        _leftRect.sizeDelta = Vector2.zero;
        _rightRect.anchoredPosition = Vector2.zero;
        _rightRect.sizeDelta = Vector2.zero;

        if (sideBias == null)
        {
            sideBias = FindObjectOfType<InventorySideBias>();
        }
        if (inventory == null)
        {
            inventory = FindObjectOfType<Inventory>();
        }
    }

    void Update()
    {
        SyncFromSource();
        UpdateVisuals(leftWeight, rightWeight, maxWeight);
    }

    private void SyncFromSource()
    {
        if (inventory != null)
        {
            maxWeight = inventory.carryCapacity;
        }

        // 1) InventorySideBias에서 그대로 끌어오기
        if (sideBias != null)
        {
            leftWeight = sideBias.leftWeight;
            rightWeight = sideBias.rightWeight;

            if (maxWeight <= 0f)
            {
                if (sideBias.maxWeight > 0f)
                {
                    maxWeight = sideBias.maxWeight;
                }
                else if (inventory != null && inventory.carryCapacity > 0f)
                {
                    maxWeight = inventory.carryCapacity;
                }
                else
                {
                    maxWeight = leftWeight + rightWeight;
                }
            }

            return;
        }

        // 2) sideBias 없으면 Inventory에서 직접 계산
        if (inventory == null) return;

        inventory.EnsureReady();
        if (inventory.leftGrid == null || inventory.rightGrid == null) return;

        float lw = 0f;
        float rw = 0f;

        var leftPlacements = inventory.leftGrid.placements;
        if (leftPlacements != null)
        {
            foreach (var p in leftPlacements)
            {
                if (p != null && p.item != null)
                    lw += p.item.TotalWeight;
            }
        }

        var rightPlacements = inventory.rightGrid.placements;
        if (rightPlacements != null)
        {
            foreach (var p in rightPlacements)
            {
                if (p != null && p.item != null)
                    rw += p.item.TotalWeight;
            }
        }

        leftWeight = lw;
        rightWeight = rw;
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