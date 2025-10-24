using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // DOTween ����� ���� �߰�

public class MainUIManager : MonoBehaviour
{
    // === �ν����� ���� ===
    [Tooltip("Start ��ư ���� ���Ե� ���� UI �г� GameObject�� �����ϼ���.")]
    public GameObject mainUIPanel;
    public GameObject PhysicsPanel;

    [Header("Fade ����")]
    [Tooltip("���̵� �ִϸ��̼ǿ� �ɸ��� �ð� (��)")]
    public float fadeDuration = 0.5f;
    public Ease fadeEase = Ease.Linear;

    // === ���� ���� ===
    private CanvasGroup mainUICanvasGroup;
    private CanvasGroup physicsCanvasGroup;

    void Awake()
    {
        // CanvasGroup ������Ʈ �������� (������ �߰��ؾ� ��)
        mainUICanvasGroup = GetCanvasGroup(mainUIPanel);
        physicsCanvasGroup = GetCanvasGroup(PhysicsPanel);

        // �ʱ� ���� ����
        if (mainUICanvasGroup != null) mainUICanvasGroup.alpha = 1;
        if (physicsCanvasGroup != null) physicsCanvasGroup.alpha = 1;

        // �ʱ⿡�� ��ȣ�ۿ� �����ϰ� ����
        if (mainUICanvasGroup != null) mainUICanvasGroup.interactable = true;
        if (physicsCanvasGroup != null) physicsCanvasGroup.interactable = true;
    }

    // GameObject�� CanvasGroup�� �������ų� �߰��ϴ� ���� �Լ�
    private CanvasGroup GetCanvasGroup(GameObject panel)
    {
        if (panel == null) return null;
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            // CanvasGroup�� ������ ��� �� null ��ȯ �Ǵ� AddComponent�Ͽ� ��� ����
            Debug.LogError($"'{panel.name}'�� CanvasGroup ������Ʈ�� �����ϴ�. Fade �ִϸ��̼��� ���� �ʿ��մϴ�.");
            // cg = panel.AddComponent<CanvasGroup>(); // �ʿ��ϴٸ� ���⼭ �߰�
        }
        return cg;
    }

    // UI �г��� ����ϴ� (Fade Out).
    public void HideUI()
    {
        // DOTween �������� ����Ͽ� �� �г��� ���ÿ� ���̵� �ƿ�
        Sequence hideSequence = DOTween.Sequence();

        // 1. ��ȣ�ۿ� ��Ȱ��ȭ (Ŭ�� ����)
        if (mainUICanvasGroup != null) mainUICanvasGroup.interactable = false;
        if (physicsCanvasGroup != null) physicsCanvasGroup.interactable = false;

        // 2. ���̵� �ƿ� �ִϸ��̼�
        if (mainUICanvasGroup != null)
        {
            hideSequence.Join(mainUICanvasGroup.DOFade(0, fadeDuration).SetEase(fadeEase));
        }

        if (physicsCanvasGroup != null)
        {
            hideSequence.Join(physicsCanvasGroup.DOFade(0, fadeDuration).SetEase(fadeEase));
        }

        // 3. �ִϸ��̼� �Ϸ� �� GameObject ��Ȱ��ȭ
        hideSequence.OnComplete(() =>
        {
            if (mainUIPanel != null) mainUIPanel.SetActive(false);
            if (PhysicsPanel != null) PhysicsPanel.SetActive(false);

            // NOTE: blocksRaycasts�� alpha�� 0�� �� ��Ȱ��ȭ�˴ϴ�.
        });
    }

    // UI �г��� �����ݴϴ� (Fade In).
    public void ShowUI()
    {
        // 1. GameObject Ȱ��ȭ
        if (mainUIPanel != null) mainUIPanel.SetActive(true);
        if (PhysicsPanel != null) PhysicsPanel.SetActive(true);

        // DOTween �������� ����Ͽ� �� �г��� ���ÿ� ���̵� ��
        Sequence showSequence = DOTween.Sequence();

        // 2. ���̵� �� �ִϸ��̼�
        if (mainUICanvasGroup != null)
        {
            // ���� �� ���ĸ� 0���� ����
            mainUICanvasGroup.alpha = 0;
            showSequence.Join(mainUICanvasGroup.DOFade(1, fadeDuration).SetEase(fadeEase));
        }

        if (physicsCanvasGroup != null)
        {
            // ���� �� ���ĸ� 0���� ����
            physicsCanvasGroup.alpha = 0;
            showSequence.Join(physicsCanvasGroup.DOFade(1, fadeDuration).SetEase(fadeEase));
        }

        // 3. �ִϸ��̼� �Ϸ� �� ��ȣ�ۿ� Ȱ��ȭ
        showSequence.OnComplete(() =>
        {
            if (mainUICanvasGroup != null) mainUICanvasGroup.interactable = true;
            if (physicsCanvasGroup != null) physicsCanvasGroup.interactable = true;
        });
    }
}