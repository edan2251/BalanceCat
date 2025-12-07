using UnityEngine;

public class InventoryDropController : MonoBehaviour
{
    public Transform player;
    public float forwardOffset = 0.8f;
    public float upOffset = 0.2f;

    public StorageZone storageZone;
    public ScoreZone scoreZone;

    public void Drop(ItemInstance inst)
    {
        if (inst == null || inst.data == null) return;

        if (storageZone != null && storageZone.IsPlayerInside && scoreZone != null)
        {
            scoreZone.AddScore(inst.Score);
            return;
        }

        var prefab = inst.data.worldPrefab;
        if (prefab == null) return;

        Vector3 pos = player.position + player.forward * forwardOffset + Vector3.up * upOffset;

        Quaternion rot = Quaternion.identity;
        var go = Object.Instantiate(prefab, pos, rot);
    }
}