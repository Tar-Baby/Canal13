using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class IntroFades : MonoBehaviour
{
    [SerializeField] GameObject fadeOut;
    [SerializeField] GameObject fadeIn;
    [SerializeField] GameObject LobbyFondo;

    [SerializeField] private AudioSource textTyping;
    [SerializeField] private AudioSource welcomeMusic;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string exposedParam = "MusicVolume";
    [SerializeField] private float defaultVolumeDb = 0f;
    [SerializeField] private float currentVolumeDb = -15f;
    
    void Start()
    {
        fadeIn.SetActive(true);
        textTyping.Play();
        StartCoroutine(StopIntroFade());
    }
    
    IEnumerator StopIntroFade()
    {
        yield return new WaitForSeconds(6);
        //audioMixer.SetFloat(exposedParam, currentVolumeDb);
        welcomeMusic.Play();
        yield return new WaitForSeconds(1);
        fadeIn.SetActive(false);
        LobbyFondo.SetActive(true);
        
    }
    
    void Awake()
    {
        audioMixer.SetFloat(exposedParam, currentVolumeDb);
        
        //audioMixer.SetFloat("MusicVolume", 0f);
        // en el futuro usar para AudioSettingsManager y Player Preferences
        /*float volumeDb = PlayerPrefs.GetFloat(exposedParam, 0f);
        audioMixer.SetFloat(exposedParam, volumeDb);*/
    }
}
