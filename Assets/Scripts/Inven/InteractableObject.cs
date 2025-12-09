using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [Header("상호 작용 정보")]
    public string objectName = "아이템";
    public string interactionText = "[E] 줍기";
    public InteractionType interactionType = InteractionType.Item;

    [Header("하이라이트 설정 (스프라이트 전용)")]
    public Color highlightColor = new Color(1f, 1f, 1f, 1f);
    public float scaleAmount = 1.2f; 
    public float animationSpeed = 10f; 

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isHighlighted = false;

    public enum InteractionType
    {
        Item,
        NPC
    }

    protected virtual void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        else
        {
            var rend = GetComponent<Renderer>();
            if (rend != null) originalColor = rend.material.color;
        }

        originalScale = transform.localScale;
        targetScale = originalScale;

        gameObject.layer = 8; 
    }

    protected virtual void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
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
        if (!isHighlighted)
        {
            if (spriteRenderer != null) spriteRenderer.color = highlightColor;
            targetScale = originalScale * scaleAmount;
            isHighlighted = true;
        }
    }

    protected virtual void RemoveHighlight()
    {
        if (isHighlighted)
        {
            if (spriteRenderer != null) spriteRenderer.color = originalColor;
            targetScale = originalScale; 
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
        switch (interactionType)
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