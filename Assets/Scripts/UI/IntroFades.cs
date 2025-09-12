using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class IntroFades : MonoBehaviour
{
    [SerializeField] GameObject fadeOut;
    [SerializeField] GameObject fadeIn;
    [SerializeField] GameObject LobbyFondo;

    [SerializeField] private AudioSource textTyping;
    [SerializeField] private AudioSource welcomeMusic;
    
    void Start()
    {
        fadeIn.SetActive(true);
        textTyping.Play();
        StartCoroutine(StopIntroFade());
    }
    
    IEnumerator StopIntroFade()
    {
        yield return new WaitForSeconds(5);
        fadeIn.SetActive(false);
        LobbyFondo.SetActive(true);
        welcomeMusic.Play();
    }
}
