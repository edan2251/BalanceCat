using UnityEngine;
using System.Collections.Generic;

public class PlatformGroupController : MonoBehaviour
{
    // 자식들에 있는 플랫폼 스크립트들을 저장할 리스트
    private List<WeightPlatform> _platforms = new List<WeightPlatform>();

    private void Awake()
    {
        // 내 자식들 중에 WeightPlatform 컴포넌트가 붙은 애들을 다 찾아옴 (자손 포함)
        WeightPlatform[] childPlatforms = GetComponentsInChildren<WeightPlatform>();
        _platforms.AddRange(childPlatforms);
    }

    // 외부(이벤트 등)에서 이 함수를 부르면 모든 발판이 멈춤
    public void LockAllPlatforms()
    {
        foreach (var platform in _platforms)
        {
            if (platform != null)
                platform.SetLock(true);
        }
    }

    // 다시 움직이게 하기
    public void UnlockAllPlatforms()
    {
        foreach (var platform in _platforms)
        {
            if (platform != null)
                platform.SetLock(false);
        }
    }
}