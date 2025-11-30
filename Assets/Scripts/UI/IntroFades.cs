using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class IntroFades : MonoBehaviour
{
    [SerializeField] GameObject fadeIn;
    [SerializeField] GameObject LobbyFondo;

    [SerializeField] private AudioSource textTyping;
    [SerializeField] private AudioSource welcomeMusic;

    [Header("Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string exposedParam = "MusicVolume";
    [SerializeField] private float currentVolumeDb = -15f; // scene start level

    void Awake()
    {
        // Make sure we don’t inherit -80 dB from previous scene
        audioMixer.SetFloat(exposedParam, currentVolumeDb);
        // Or later: float vol = PlayerPrefs.GetFloat(exposedParam, 0f); audioMixer.SetFloat(exposedParam, vol);
    }

    void Start()
    {
        if (fadeIn != null) fadeIn.SetActive(true);
        if (textTyping != null) textTyping.Play();
        StartCoroutine(StopIntroFade());
    }

    IEnumerator StopIntroFade()
    {
        yield return new WaitForSeconds(6f);
        if (welcomeMusic != null) welcomeMusic.Play();

        yield return new WaitForSeconds(1f);
        if (fadeIn != null) fadeIn.SetActive(false);
        if (LobbyFondo != null) LobbyFondo.SetActive(true);
    }
}