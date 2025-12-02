using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class MainMenu : MonoBehaviour
{
    
    [SerializeField] GameObject fadeOut;
    [SerializeField] GameObject fadeIn;
    [SerializeField] private GameObject background;
    [SerializeField] private GameObject title;
    [SerializeField] private GameObject buttonNuevoJuego;
    [SerializeField] private GameObject buttonOpciones;
    [SerializeField] private GameObject buttonSalir;
    [SerializeField] private GameObject panelNegro;
    [SerializeField] private GameObject textoCita;
    [SerializeField] private AudioSource buttonClick;
    [SerializeField] private GameObject Chicas;
    [Header("Audio Mixer Settings")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string exposedParam = "MusicVolume";
    [SerializeField] private float fadeDuration = 3f;
    [SerializeField] private float targetVolumeDb = 0f; // normal loudness
    [SerializeField] private float minVolumeDb = -80f; // silent
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        panelNegro.SetActive(true);
        textoCita.SetActive(true); // 13 segundos de animacion
        DisableElements();
        StartCoroutine(DisableCita());
        StartCoroutine(EnableElements());
        StartCoroutine(PlayFade());
        StartCoroutine(StopFade());

        // Start the mixer audio fade-in
        if (audioMixer != null)
            StartCoroutine(FadeInMixer());
        
    }

    public void StartGame()
{
    buttonClick.Play();
    fadeOut.SetActive(true);
    StartCoroutine(FadeOutAndChangeScene("Intro"));
}

IEnumerator FadeOutAndChangeScene(string nextScene)
{
    // Fade out the mixer volume first
    yield return StartCoroutine(FadeOutMixer());

    // Optional: small delay for visual fade (match your fadeOut animation if needed)
    yield return new WaitForSeconds(0.5f);

    // Then load the next scene
    SceneManager.LoadScene(nextScene);
}

IEnumerator FadeOutMixer()
{
    float currentTime = 0f;
    float startDb;
    audioMixer.GetFloat(exposedParam, out startDb);
    float endDb = -50f; // silent

    while (currentTime < fadeDuration)
    {
        currentTime += Time.deltaTime;
        float newVolume =
            Mathf.Lerp(startDb, endDb, currentTime / fadeDuration);
        audioMixer.SetFloat(exposedParam, newVolume);
        yield return null;
    }

    audioMixer.SetFloat(exposedParam, endDb);
}

    public void QuitGame()   //en el futuro agregar panel de confirmacion para salir del juego
    {
        buttonClick.Play();
        Application.Quit();
        Debug.Log("Game has quit.");
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator TransferToIntroScene()
    {
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene("Intro");
    }

    IEnumerator StopFade()
    {
        yield return new WaitForSeconds(24);
        fadeIn.SetActive(false);
    }

    IEnumerator PlayFade()
    {
        yield return new WaitForSeconds(20);
        fadeIn.SetActive(true);
        
    }

    public void DisableElements()
    {
        background.SetActive(false);
        title.SetActive(false);
        buttonNuevoJuego.SetActive(false);
        buttonOpciones.SetActive(false);
        buttonSalir.SetActive(false);
        Chicas.SetActive(false);
    }

    IEnumerator EnableElements()
    {
        yield return new WaitForSeconds(20);
        background.SetActive(true);
        Chicas.SetActive(true);
        title.SetActive(true);
        buttonNuevoJuego.SetActive(true);
        buttonOpciones.SetActive(true);
        buttonSalir.SetActive(true);
        
    }

    IEnumerator DisableCita()
    {
        yield return new WaitForSeconds(20);
        panelNegro.SetActive(false);
        textoCita.SetActive(false);
    }

IEnumerator FadeInMixer()
{
    float currentTime = 0f;
    float startDb = -80f; // not full silence, starts barely audible
    audioMixer.SetFloat(exposedParam, startDb);

    while (currentTime < fadeDuration)
    {
        currentTime += Time.deltaTime;
        float newVolume = Mathf.Lerp(startDb, targetVolumeDb, currentTime / fadeDuration);
        audioMixer.SetFloat(exposedParam, newVolume);
        yield return null;
    }

    audioMixer.SetFloat(exposedParam, targetVolumeDb);
}
    
}
