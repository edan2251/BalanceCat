using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageZone : MonoBehaviour
{
    [SerializeField] string playerTag = "Player";

    public bool IsPlayerInside { get; private set; }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag)) IsPlayerInside = true;
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag)) IsPlayerInside = false;
    }
}
