using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class BalanceMiniGame : MonoBehaviour
{
    public static bool IsRunning { get; private set; }

    [Header("Refs")]
    [SerializeField] BalanceSafetyRing ring;     // (선택) 있으면 onExitRing에 자동 연결
    [SerializeField] PlayerMovement movement;    // 입력 잠금용
    [SerializeField] Animator animator;          // isStaggering / isFalling 파라미터 사용
    [SerializeField] Transform panel;            // 키 프리팹을 붙일 UI 패널(자식으로 생성)

    [SerializeField] InventorySlotBreaker slotBreaker;

    [Header("Key Prefab (Unified)")]
    [SerializeField] GameObject keyPrefab;

    [Header("Settings")]
    [SerializeField, Min(1)] int sequenceLength = 6;
    [SerializeField] bool highlightCurrent = true;   // 현재 키 강조(스케일 업)
    [SerializeField] float successUnlockDelay = 0f;  // 성공 시 바로 복귀(원하면 딜레이 조절)
    [SerializeField] float failDownDuration = 2f;    // 실패 시 넘어짐 유지 시간

    [SerializeField] float timeLimit = 10f;
    [SerializeField] Slider timerSlider;

    readonly char[] _pool = { 'Q', 'W', 'E', 'A', 'S', 'D' };
    Dictionary<char, GameObject> _prefabs;
    readonly List<char> _seq = new List<char>();
    readonly List<GameObject> _spawned = new List<GameObject>();
    readonly List<bool> _cleared = new List<bool>();

    readonly List<BalanceMiniGameKeyButton> _keyButtons = new List<BalanceMiniGameKeyButton>();

    int _idx;
    bool _running;
    float _remainTime;

    void Awake()
    {
        if (!movement) movement = GetComponentInParent<PlayerMovement>();
        if (!animator)
        {
            if (movement && movement.playerAnimator) animator = movement.playerAnimator;
            else animator = GetComponentInChildren<Animator>();
        }

        if (!slotBreaker) slotBreaker = FindObjectOfType<InventorySlotBreaker>();

        _prefabs = new Dictionary<char, GameObject>();
        if (keyPrefab != null)
        {
            foreach (var c in _pool)
            {
                _prefabs[c] = keyPrefab;
            }
        }

        if (ring) ring.onExitRing.AddListener(StartStaggerSequence);
        if (timerSlider) timerSlider.gameObject.SetActive(false);
    }

    void OnDisable()
    {
        IsRunning = false;
        if (panel) panel.gameObject.SetActive(false);
        ClearUI();
        SafeSetBool("isStaggering", false);
        SafeSetBool("isFalling", false);
        movement?.SetControlEnabled(true);
        if (timerSlider) timerSlider.gameObject.SetActive(false);
    }

    public void StartStaggerSequence()
    {
        if (_running || !panel) return;

        IsRunning = true;

        _remainTime = timeLimit;
        if (timerSlider)
        {
            bool useTimer = timeLimit > 0f;
            timerSlider.gameObject.SetActive(useTimer);
            timerSlider.minValue = 0f;
            timerSlider.maxValue = 1f;
            timerSlider.value = 1f;
        }

        panel.gameObject.SetActive(true);
        StartCoroutine(Co_StaggerAndMiniGame());
    }

    IEnumerator Co_StaggerAndMiniGame()
    {
        _running = true;

        // 1) 비틀거림 시작 + 조작성 잠금
        movement?.SetControlEnabled(false);
        SafeSetBool("isFalling", false);
        SafeSetBool("isStaggering", true);

        // 2) 시퀀스/UI 준비
        BuildSequence();
        BuildUI();
        FocusCurrent();

        // 3) 입력 루프
        while (_idx < _seq.Count)
        {
            if (timeLimit > 0f)
            {
                _remainTime -= Time.deltaTime;
                if (timerSlider)
                {
                    timerSlider.value = Mathf.Clamp01(_remainTime / timeLimit);
                }

                if (_remainTime <= 0f)
                {
                    // 시간 초과 실패
                    yield return Co_FailSequence();
                    yield break;
                }
            }

            char expected = _seq[_idx];
            if (TryReadKeyDown(out char pressed))
            {
                if (pressed == expected)
                {
                    MarkCleared(_idx);
                    _idx++;
                    FocusCurrent();
                }
                else
                {
                    // 잘못된 키 → 실패
                    yield return Co_FailSequence();
                    yield break;
                }
            }

            yield return null;
        }

        // 4) 성공 처리
        ClearUI();
        if (panel) panel.gameObject.SetActive(false);
        if (timerSlider) timerSlider.gameObject.SetActive(false);
        SafeSetBool("isStaggering", false);
        if (successUnlockDelay > 0f) yield return new WaitForSeconds(successUnlockDelay);
        movement?.SetControlEnabled(true);
        _running = false;
        IsRunning = false;
    }

    IEnumerator Co_FailSequence()
    {
        ClearUI();
        if (panel) panel.gameObject.SetActive(false);
        SafeSetBool("isStaggering", false);
        SafeSetBool("isFalling", true);

        if (slotBreaker != null)
        {
            slotBreaker.BreakRandomSlotAndDrop();
        }

        if (timerSlider) timerSlider.gameObject.SetActive(false);

        yield return new WaitForSeconds(failDownDuration);
        SafeSetBool("isFalling", false);
        movement?.SetControlEnabled(true);
        _running = false;
        IsRunning = false;
    }

    void BuildSequence()
    {
        _seq.Clear();
        _idx = 0;
        for (int i = 0; i < sequenceLength; i++)
        {
            int r = Random.Range(0, _pool.Length);
            _seq.Add(_pool[r]);
        }
    }

    void BuildUI()
    {
        ClearUI();

        for (int i = 0; i < _seq.Count; i++)
        {
            char key = _seq[i];
            if (!_prefabs.TryGetValue(key, out var prefab) || prefab == null)
            {
                Debug.LogWarning($"[MiniGame] Prefab for '{key}' is missing.");
                continue;
            }

            var go = Instantiate(prefab, panel);
            _spawned.Add(go);

            var keyUI = go.GetComponent<BalanceMiniGameKeyButton>();
            if (keyUI != null)
            {
                keyUI.Setup(key);
            }
            _keyButtons.Add(keyUI);

            _cleared.Add(false);
        }
    }

    void ClearUI()
    {
        for (int i = 0; i < _spawned.Count; i++)
            if (_spawned[i]) Destroy(_spawned[i]);
        _spawned.Clear();

        _cleared.Clear();
        _keyButtons.Clear();
    }

    void FocusCurrent()
    {
        // [변경] 색상 조절 없이 스프라이트/스케일만 상태 반영
        for (int i = 0; i < _spawned.Count; i++)
        {
            var go = _spawned[i];
            if (!go) continue;

            bool isCleared = (_cleared.Count > i && _cleared[i]);
            bool isCurrent = (i == _idx);

            var keyUI = (i < _keyButtons.Count) ? _keyButtons[i] : go.GetComponent<BalanceMiniGameKeyButton>();
            if (keyUI != null)
            {
                if (isCleared)
                {
                    keyUI.SetCleared();
                }
                else if (isCurrent && highlightCurrent)
                {
                    keyUI.SetCurrent();
                }
                else
                {
                    keyUI.SetNormal();
                }
            }
            else
            {
                if (isCleared)
                {
                    go.transform.localScale = Vector3.one * 0.9f;
                }
                else if (isCurrent && highlightCurrent)
                {
                    go.transform.localScale = Vector3.one * 1.15f;
                }
                else
                {
                    go.transform.localScale = Vector3.one;
                }
            }
        }
    }

    void MarkCleared(int i)
    {
        if (i < 0 || i >= _spawned.Count || !_spawned[i]) return;
        if (_cleared.Count > i)
            _cleared[i] = true;
    }

    bool TryReadKeyDown(out char key)
    {
        key = '\0';
        if (Input.GetKeyDown(KeyCode.Q)) { key = 'Q'; return true; }
        if (Input.GetKeyDown(KeyCode.W)) { key = 'W'; return true; }
        if (Input.GetKeyDown(KeyCode.E)) { key = 'E'; return true; }
        if (Input.GetKeyDown(KeyCode.A)) { key = 'A'; return true; }
        if (Input.GetKeyDown(KeyCode.S)) { key = 'S'; return true; }
        if (Input.GetKeyDown(KeyCode.D)) { key = 'D'; return true; }
        return false;
    }

    void SafeSetBool(string param, bool v)
    {
        if (animator && animator.HasParameter(param))
            animator.SetBool(param, v);
    }
}

// Animator 파라미터 유틸(옵션)
public static class AnimatorExt
{
    static readonly Dictionary<Animator, HashSet<int>> _cache = new Dictionary<Animator, HashSet<int>>();

    public static bool HasParameter(this Animator anim, string name)
    {
        if (!anim) return false;
        if (!_cache.TryGetValue(anim, out var set))
        {
            set = new HashSet<int>();
            foreach (var p in anim.parameters) set.Add(p.nameHash);
            _cache[anim] = set;
        }
        return set.Contains(Animator.StringToHash(name));
    }
}
