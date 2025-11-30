using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettingsManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string exposedParam = "MusicVolume";
    [SerializeField] private Slider volumeSlider;

    void Start()
    {
        // Load saved volume if exists
        float savedVolume = PlayerPrefs.GetFloat(exposedParam, 0f);
        audioMixer.SetFloat(exposedParam, savedVolume);
        volumeSlider.value = savedVolume;
    }

    public void OnVolumeSliderChanged(float newVolume)
    {
        // Set mixer volume live as user moves the slider
        audioMixer.SetFloat(exposedParam, newVolume);

        // Save the new value for future sessions
        PlayerPrefs.SetFloat(exposedParam, newVolume);
    }
}