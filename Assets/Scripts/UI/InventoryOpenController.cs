using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryOpenController : MonoBehaviour
{
    public GameObject inventoryPanel;
    public GameObject storagePanel;
    public StorageZone storageZone;

    private Animator _animator;
    private bool _isOpen = false;

    public ShowCursor showCursor;
    public PlayerMovement playerMovement;
    public ThirdPersonCam cameraController;
    public CinemachineBrain cineBrain;

    void Start()
    {
        if (inventoryPanel != null)
        {
            _animator = inventoryPanel.GetComponent<Animator>();
        }

        if (storagePanel != null)
        {
            storagePanel.SetActive(false);
        }

        if (playerMovement == null)
        {
            playerMovement = FindObjectOfType<PlayerMovement>();
        }

        if (cameraController == null)
        {
            cameraController = FindObjectOfType<ThirdPersonCam>();
        }

        if (cineBrain == null)
        {
            var cam = Camera.main;
            if (cam != null)
            {
                cineBrain = cam.GetComponent<CinemachineBrain>();
            }
        }

        _isOpen = false;
    }

    void Update()
    {
        //  I 키 입력 감지 -> 이거 Tab키로 연동 필요
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        _isOpen = !_isOpen; 

        if (_animator != null)
        {
            _animator.SetBool("isOpen", _isOpen);
        }

        if (_isOpen)
        {
            if (storageZone != null && storageZone.IsPlayerInside && storagePanel != null)
            {
                storagePanel.SetActive(true);
            }
        }
        else
        {
            if (storagePanel != null)
            {
                storagePanel.SetActive(false);
            }
        }

        if (showCursor != null)
        {
            if (_isOpen)
            {
                showCursor.UnlockCursor(); 
            }
            else
            {
                showCursor.LockCursor();  
            }
        }
        else
        {
            Debug.LogWarning("ShowCursor인벤토리에연결필요");
        }

        //if (playerMovement != null)
        //{
        //    Rigidbody rb = playerMovement.rb != null
        //        ? playerMovement.rb : playerMovement.GetComponent<Rigidbody>();

        //    if (_isOpen)
        //    {
        //        if (rb != null)
        //        {
        //            rb.velocity = Vector3.zero;
        //            rb.angularVelocity = Vector3.zero;
        //            rb.isKinematic = true;
        //        }
        //    }
        //    else
        //    {
        //        if (rb != null)
        //        {
        //            rb.isKinematic = false;
        //        }
        //    }

        //    playerMovement.enabled = !_isOpen;
        //}

        //if (cameraController != null)
        //{
        //    cameraController.enabled = !_isOpen;
        //}

        //if (cineBrain != null)
        //{
        //    cineBrain.enabled = !_isOpen;
        //}
    }
}
