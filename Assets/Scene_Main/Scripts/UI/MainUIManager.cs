using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // DOTween 사용을 위해 추가

public class MainUIManager : MonoBehaviour
{
    // === 인스펙터 설정 ===
    [Tooltip("Start 버튼 등이 포함된 메인 UI 패널 GameObject를 연결하세요.")]
    public GameObject mainUIPanel;
    public GameObject PhysicsPanel;

    [Header("Fade 설정")]
    [Tooltip("페이드 애니메이션에 걸리는 시간 (초)")]
    public float fadeDuration = 0.5f;
    public Ease fadeEase = Ease.Linear;

    // === 내부 참조 ===
    private CanvasGroup mainUICanvasGroup;
    private CanvasGroup physicsCanvasGroup;

    void Awake()
    {
        // CanvasGroup 컴포넌트 가져오기 (없으면 추가해야 함)
        mainUICanvasGroup = GetCanvasGroup(mainUIPanel);
        physicsCanvasGroup = GetCanvasGroup(PhysicsPanel);

        // 초기 상태 설정
        if (mainUICanvasGroup != null) mainUICanvasGroup.alpha = 1;
        if (physicsCanvasGroup != null) physicsCanvasGroup.alpha = 1;

        // 초기에는 상호작용 가능하게 설정
        if (mainUICanvasGroup != null) mainUICanvasGroup.interactable = true;
        if (physicsCanvasGroup != null) physicsCanvasGroup.interactable = true;
    }

    // GameObject에 CanvasGroup을 가져오거나 추가하는 헬퍼 함수
    private CanvasGroup GetCanvasGroup(GameObject panel)
    {
        if (panel == null) return null;
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            // CanvasGroup이 없으면 경고 후 null 반환 또는 AddComponent하여 사용 가능
            Debug.LogError($"'{panel.name}'에 CanvasGroup 컴포넌트가 없습니다. Fade 애니메이션을 위해 필요합니다.");
            // cg = panel.AddComponent<CanvasGroup>(); // 필요하다면 여기서 추가
        }
        return cg;
    }

    // UI 패널을 숨깁니다 (Fade Out).
    public void HideUI()
    {
        // DOTween 시퀀스를 사용하여 두 패널을 동시에 페이드 아웃
        Sequence hideSequence = DOTween.Sequence();

        // 1. 상호작용 비활성화 (클릭 방지)
        if (mainUICanvasGroup != null) mainUICanvasGroup.interactable = false;
        if (physicsCanvasGroup != null) physicsCanvasGroup.interactable = false;

        // 2. 페이드 아웃 애니메이션
        if (mainUICanvasGroup != null)
        {
            hideSequence.Join(mainUICanvasGroup.DOFade(0, fadeDuration).SetEase(fadeEase));
        }

        if (physicsCanvasGroup != null)
        {
            hideSequence.Join(physicsCanvasGroup.DOFade(0, fadeDuration).SetEase(fadeEase));
        }

        // 3. 애니메이션 완료 후 GameObject 비활성화
        hideSequence.OnComplete(() =>
        {
            if (mainUIPanel != null) mainUIPanel.SetActive(false);
            if (PhysicsPanel != null) PhysicsPanel.SetActive(false);

            // NOTE: blocksRaycasts는 alpha가 0일 때 비활성화됩니다.
        });
    }

    // UI 패널을 보여줍니다 (Fade In).
    public void ShowUI()
    {
        // 1. GameObject 활성화
        if (mainUIPanel != null) mainUIPanel.SetActive(true);
        if (PhysicsPanel != null) PhysicsPanel.SetActive(true);

        // DOTween 시퀀스를 사용하여 두 패널을 동시에 페이드 인
        Sequence showSequence = DOTween.Sequence();

        // 2. 페이드 인 애니메이션
        if (mainUICanvasGroup != null)
        {
            // 시작 시 알파를 0으로 설정
            mainUICanvasGroup.alpha = 0;
            showSequence.Join(mainUICanvasGroup.DOFade(1, fadeDuration).SetEase(fadeEase));
        }

        if (physicsCanvasGroup != null)
        {
            // 시작 시 알파를 0으로 설정
            physicsCanvasGroup.alpha = 0;
            showSequence.Join(physicsCanvasGroup.DOFade(1, fadeDuration).SetEase(fadeEase));
        }

        // 3. 애니메이션 완료 후 상호작용 활성화
        showSequence.OnComplete(() =>
        {
            if (mainUICanvasGroup != null) mainUICanvasGroup.interactable = true;
            if (physicsCanvasGroup != null) physicsCanvasGroup.interactable = true;
        });
    }
}