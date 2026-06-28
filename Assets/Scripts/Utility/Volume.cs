using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;


public class Volume : MonoBehaviour
{
    [SerializeField] AudioMixer m_mixer;
    [SerializeField] Slider m_masterVolume;
    [SerializeField] Slider m_musicSlider;
    [SerializeField] Slider m_sfxSlider;

    const string MIXER_MASTER = "MasterVolume";
    const string MIXER_MUSIC = "MusicVolume";
    const string MIXER_SFX = "SFXVolume";

    private void Awake()
    {
        m_masterVolume.onValueChanged.AddListener(SetMasterVolume);
        m_musicSlider.onValueChanged.AddListener(SetMusicVolume);
        m_sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    void SetMasterVolume(float value)
    {
        m_mixer.SetFloat(MIXER_MASTER, Mathf.Log10(value) * 40);
    }
    void SetMusicVolume(float value)
    {
        m_mixer.SetFloat(MIXER_MUSIC, Mathf.Log10(value) * 20);
    }
    void SetSFXVolume(float value)
    {
        m_mixer.SetFloat(MIXER_SFX, Mathf.Log10(value) * 20);
    }
}
