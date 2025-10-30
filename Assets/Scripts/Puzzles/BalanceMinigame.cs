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

    [Header("Key Prefabs (Q W E A S D)")]
    [SerializeField] GameObject prefabQ;
    [SerializeField] GameObject prefabW;
    [SerializeField] GameObject prefabE;
    [SerializeField] GameObject prefabA;
    [SerializeField] GameObject prefabS;
    [SerializeField] GameObject prefabD;

    [Header("Settings")]
    [SerializeField, Min(1)] int sequenceLength = 6;
    [SerializeField] bool highlightCurrent = true;   // 현재 키 강조(스케일 업)
    [SerializeField] float successUnlockDelay = 0f;  // 성공 시 바로 복귀(원하면 딜레이 조절)
    [SerializeField] float failDownDuration = 2f;    // 실패 시 넘어짐 유지 시간
    [SerializeField] Color highlightColor = Color.yellow;

    readonly char[] _pool = { 'Q', 'W', 'E', 'A', 'S', 'D' };
    Dictionary<char, GameObject> _prefabs;
    readonly List<char> _seq = new List<char>();
    readonly List<GameObject> _spawned = new List<GameObject>();
    readonly List<Color> _origColors = new List<Color>();
    readonly List<bool> _cleared = new List<bool>();
    int _idx;
    bool _running;

    void Awake()
    {
        // 레퍼런스 폴백
        if (!movement) movement = GetComponentInParent<PlayerMovement>();
        if (!animator)
        {
            if (movement && movement.playerAnimator) animator = movement.playerAnimator;
            else animator = GetComponentInChildren<Animator>();
        }

        // 프리팹 매핑
        _prefabs = new Dictionary<char, GameObject>
        {
            { 'Q', prefabQ }, { 'W', prefabW }, { 'E', prefabE },
            { 'A', prefabA }, { 'S', prefabS }, { 'D', prefabD },
        };

        // BalanceSafetyRing 이벤트 자동 연결(있을 때만)
        if (ring) ring.onExitRing.AddListener(StartStaggerSequence); // 링 밖 이탈 시 시작
    }

    void OnDisable()
    {
        IsRunning = false;
        if (panel) panel.gameObject.SetActive(false);
        ClearUI();
        SafeSetBool("isStaggering", false);
        SafeSetBool("isFalling", false);
        movement?.SetControlEnabled(true);
    }

    // === 외부에서 호출(Inspector 이벤트로 연결해도 됨) ===
    public void StartStaggerSequence()
    {
        if (_running || !panel) return;

        IsRunning = true;
        var inv = FindObjectOfType<InventoryToggle>();
        inv?.ForceClose();

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

        // 3) 입력 루프(정확히 6개 맞추면 성공)
        while (_idx < _seq.Count)
        {
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
                    // 실패 처리
                    ClearUI();
                    if (panel) panel.gameObject.SetActive(false);
                    SafeSetBool("isStaggering", false);
                    SafeSetBool("isFalling", true);
                    yield return new WaitForSeconds(failDownDuration);
                    SafeSetBool("isFalling", false);
                    movement?.SetControlEnabled(true);
                    _running = false;
                    IsRunning = false;
                    yield break;
                }
            }
            yield return null;
        }

        // 4) 성공 처리
        ClearUI();
        if (panel) panel.gameObject.SetActive(false);
        SafeSetBool("isStaggering", false);
        if (successUnlockDelay > 0f) yield return new WaitForSeconds(successUnlockDelay);
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

            var g = go.GetComponentInChildren<Graphic>();
            _origColors.Add(g ? g.color : Color.white);
            _cleared.Add(false);
        }
    }

    void ClearUI()
    {
        for (int i = 0; i < _spawned.Count; i++)
            if (_spawned[i]) Destroy(_spawned[i]);
        _spawned.Clear();

        _origColors.Clear();
        _cleared.Clear();
    }

    void FocusCurrent()
    {
        if (!highlightCurrent) return;

        for (int i = 0; i < _spawned.Count; i++)
        {
            var go = _spawned[i];
            if (!go) continue;

            // 완료된 칸은 건드리지 않음
            if (_cleared.Count > i && _cleared[i])
            {
                go.transform.localScale = Vector3.one * 0.9f;
                continue;
            }

            var g = go.GetComponentInChildren<Graphic>();
            bool isCurrent = (i == _idx);

            go.transform.localScale = isCurrent ? Vector3.one * 1.15f : Vector3.one;
           
            if (g)
            {
                if (isCurrent) g.color = highlightColor;
                else if (_origColors.Count > i) g.color = _origColors[i];
            }
        }
    }

    void MarkCleared(int i)
    {
        if (i < 0 || i >= _spawned.Count || !_spawned[i]) return;

        _cleared[i] = true;

        var g = _spawned[i].GetComponentInChildren<Graphic>();
        if (g)
        {
            // [MOD] 원래색의 알파만 낮춰 흐리게
            Color baseCol = (_origColors.Count > i) ? _origColors[i] : g.color;
            g.color = new Color(baseCol.r, baseCol.g, baseCol.b, 0.35f);
        }
        _spawned[i].transform.localScale = Vector3.one * 0.9f;
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
    static readonly Dictionary<Animator, HashSet<int>> _cache = new();

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
