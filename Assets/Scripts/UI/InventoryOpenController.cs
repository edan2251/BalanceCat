using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryOpenController : MonoBehaviour
{
    public GameObject inventoryPanel;

    private Animator _animator;
    private bool _isOpen = false;

    public ShowCursor showCursor;

    void Start()
    {
        if (inventoryPanel != null)
        {
            _animator = inventoryPanel.GetComponent<Animator>();
        }

        _isOpen = false;
    }

    void Update()
    {
        //  I 키 입력 감지 -> 이거 Tab키로 연동 필요
        if (Input.GetKeyDown(KeyCode.I))
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
    }
}
