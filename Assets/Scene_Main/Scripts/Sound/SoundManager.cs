using UnityEngine;
using UnityEngine.Audio;

// 사운드 넣는곳에 SoundManager.Instance.PlaySFX(SFX.Drop); 추가해주기

//ChapterSelector.cs Start에 이거 넣어주기
//if (SoundManager.Instance != null)
//{
//    SoundManager.Instance.PlayMainMenuBGM();
//}
public enum SFX
{
    Drop,       // 0번: 아이템 드롭
    Clear,      // 1번: 성공
    Fail,       // 2번: 실패
    Fall        // 3번: 꽈당
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

    public void PlaySFX(SFX sfxName)
    {
        int index = (int)sfxName;

        if (index >= 0 && index < SfxClips.Length)
        {
            if (SfxClips[index] != null)
                sfxSource.PlayOneShot(SfxClips[index]);
        }
        else
        {
            Debug.LogWarning($"[SoundManager] {sfxName}에 해당하는 오디오 클립이 없습니다!");
        }
    }

    public void SetBGMVolume(float value) { PlayerPrefs.SetFloat("BGM_Volume", value); float db = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f; mainMixer.SetFloat("BGM", db); }
    public void SetSFXVolume(float value) { PlayerPrefs.SetFloat("SFX_Volume", value); float db = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f; mainMixer.SetFloat("SFX", db); }
}