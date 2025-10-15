using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [Header("상호 작용 정보")]
    public string objectName = "아이템";
    public string interactionText = "[E] 상호 작용";
    public InteractionType interactionType = InteractionType.Item;

    [Header("하이라이트 설정")]
    public Color highlightColor = Color.yellow;
    public float highlightIntensity = 1.5f;

    public Renderer objectRenderer;

    private Color originalColor;
    private bool isHighlighted = false;

    public enum InteractionType
    {
        Item,       
        NPC
    }

    protected virtual void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if(objectRenderer != null )
        {
            originalColor = objectRenderer.material.color;
        }
        gameObject.layer = 8;
    }

    public virtual void OnPlayerEnter()
    {
        HighlightObject();
    }

    public virtual void OnPlayerExit()
    {
        RemoveHighlight();
    }

    protected virtual void HighlightObject()
    {
        if(objectRenderer != null && !isHighlighted)
        {
            objectRenderer.material.color = highlightColor;
            objectRenderer.material.SetFloat("_Emission", highlightIntensity);
            isHighlighted = true;
        }
    }

    protected virtual void RemoveHighlight()
    {
        if (objectRenderer != null && isHighlighted)
        {
            objectRenderer.material.color = originalColor;
            objectRenderer.material.SetFloat("_Emission", 0f);
            isHighlighted = false;
        }
    }

    protected virtual void CollectItem()
    {
        Destroy(gameObject);
    }

    protected virtual void TalkToNPC()
    {
        Debug.Log($"{objectName}와 대화를 시작합니다.");
    }

    public virtual void Interact()
    {
        switch(interactionType)
        { 
            case InteractionType.Item:
                CollectItem();
                break;
            case InteractionType.NPC:
                TalkToNPC();
                break;
        }
    }

    public virtual string GetInteractionText()
    {
        return interactionText;
    }
}
