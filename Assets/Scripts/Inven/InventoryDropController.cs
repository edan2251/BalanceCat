using UnityEngine;

public class InventoryDropController : MonoBehaviour
{
    public Transform player;
    public float forwardOffset = 0.8f;
    public float upOffset = 0.2f;

    public StorageZone storageZone;
    public ScoreZone scoreZone;

    public bool Drop(ItemInstance inst)
    {
        if (inst == null || inst.data == null) return false;

        // 1. StorageZone (배달/점수) 체크
        if (storageZone != null && storageZone.IsPlayerInside && scoreZone != null)
        {
            if (InGameQuestManager.Instance != null)
            {
                InGameQuestManager.Instance.OnItemDelivered(inst.data.id);
            }

            scoreZone.AddScore(inst.Score);

            if (InGameQuestManager.Instance != null)
            {
                InGameQuestManager.Instance.CheckClearCondition();
            }

            return true;
        }

        // 2. 월드 드롭
        var prefab = inst.data.worldPrefab;
        if (prefab != null)
        {
            Vector3 pos = player.position + player.forward * forwardOffset + Vector3.up * upOffset;
            Quaternion rot = Quaternion.identity;

            Object.Instantiate(prefab, pos, rot);

            return true; 
        }

        return false;
    }
}