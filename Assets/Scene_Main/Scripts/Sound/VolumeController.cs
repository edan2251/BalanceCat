using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    [Header("Sliders")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    void Start()
    {
        // 1. 저장된 볼륨 값을 가져와서 슬라이더 위치 세팅
        float savedBGM = PlayerPrefs.GetFloat("BGM_Volume", 0.75f);
        float savedSFX = PlayerPrefs.GetFloat("SFX_Volume", 0.75f);

        if (bgmSlider != null)
        {
            bgmSlider.value = savedBGM;
            bgmSlider.onValueChanged.AddListener(OnBGMChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = savedSFX;
            sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        }
    }

    void OnBGMChanged(float value)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.SetBGMVolume(value);
    }

    void OnSFXChanged(float value)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.SetSFXVolume(value);
    }
}