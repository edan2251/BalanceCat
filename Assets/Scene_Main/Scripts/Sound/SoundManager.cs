using UnityEngine;
using UnityEngine.Audio;

// 사운드 넣는곳에 SoundManager.Instance.PlaySFX(SFX.ButtonClick); 추가해주기

//SoundManager.Instance.PlaySFX(SFX.Test);

//ChapterSelector.cs Start에 이거 넣어주기
//if (SoundManager.Instance != null)
//{
//    SoundManager.Instance.PlayMainMenuBGM();
//}
public enum SFX
{
    Test,  // 0 : 테스트 사운드
    ButtonClick, //1 : 버튼클릭
    Nyaong, //2 : 야옹소리
    Walk, //3 : 걷는 소리 ( 발자국 한번 )
    Jump, //4 : 점프소리
    Pickup, //5 : 아이템 획득
    OpenInventory, //6 : 인벤 열기
    MiniGameClearButton, //7 : 미니게임 버튼 성공하기
    MiniGameFail,    //8: 미니게임 실패 넘어짐
    QuestClearSound //9 : 퀘스트 클리어 사운드

}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Mixer")]
    public AudioMixer mainMixer;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("BGM List")]
    public AudioClip mainMenuBgm;
    public AudioClip[] chapterBgms;
    
    [Header("SFX List")]
    public AudioClip[] SfxClips;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }

        if (bgmSource != null) bgmSource.loop = true;  // BGM은 무조건 반복
        if (sfxSource != null) sfxSource.loop = false; // 효과음은 반복 안 함
    }

    void Start()
    {
        float bgmVol = PlayerPrefs.GetFloat("BGM_Volume", 0.75f);
        float sfxVol = PlayerPrefs.GetFloat("SFX_Volume", 0.75f);
        SetBGMVolume(bgmVol);
        SetSFXVolume(sfxVol);
    }

    public void PlayMainMenuBGM()
    {
        if (mainMenuBgm == null) return;

        if (bgmSource.clip == mainMenuBgm && bgmSource.isPlaying) return;

        bgmSource.clip = mainMenuBgm;
        bgmSource.Play();
    }

    public void PlayChapterBGM(int chapterIndex)
    {
        if (chapterIndex < 0 || chapterIndex >= chapterBgms.Length) return;
        if (bgmSource.clip == chapterBgms[chapterIndex] && bgmSource.isPlaying) return;
        bgmSource.clip = chapterBgms[chapterIndex];
        bgmSource.Play();
    }

    public void PlaySFX(SFX sfxName, float pitch = 1.0f)
    {
        int index = (int)sfxName;

        if (index >= 0 && index < SfxClips.Length)
        {
            AudioClip clip = SfxClips[index];
            if (clip == null) return;

            // 1. 일반 재생 (속도 변화 없음)
            if (Mathf.Approximately(pitch, 1.0f))
            {
                sfxSource.PlayOneShot(clip);
            }
            // 2. 속도 변환 재생 (별도의 임시 소스 생성)
            else
            {
                // 잠깐 쓸 AudioSource를 SoundManager 몸체에 붙임
                AudioSource tempSource = gameObject.AddComponent<AudioSource>();

                // 설정 복사 (믹서 연결 중요!)
                tempSource.clip = clip;
                tempSource.pitch = pitch;
                tempSource.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup;
                tempSource.volume = sfxSource.volume;
                tempSource.spatialBlend = sfxSource.spatialBlend; // 2D/3D 설정 유지

                tempSource.Play();

                // 재생이 끝나면(길이/속도) 컴포넌트 삭제 (청소)
                Destroy(tempSource, clip.length / pitch + 0.1f);
            }
        }
        else
        {
            Debug.LogWarning($"[SoundManager] {sfxName} 오디오 클립이 없습니다!");
        }
    }

    public void SetBGMVolume(float value) { PlayerPrefs.SetFloat("BGM_Volume", value); float db = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f; mainMixer.SetFloat("BGM", db); }
    public void SetSFXVolume(float value) { PlayerPrefs.SetFloat("SFX_Volume", value); float db = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f; mainMixer.SetFloat("SFX", db); }
}