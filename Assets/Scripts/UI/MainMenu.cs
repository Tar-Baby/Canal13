using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        panelNegro.SetActive(true);
        textoCita.SetActive(true); // 7 segundos de animacion
        DisableElements();
        StartCoroutine(DisableCita());
        StartCoroutine(EnableElements());
        StartCoroutine(PlayFade());
        StartCoroutine(StopFade());
        
    }

    public void StartGame()
    {
        buttonClick.Play();
        fadeOut.SetActive(true);
        StartCoroutine(TransferToIntroScene());
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
        yield return new WaitForSeconds(10);
        fadeIn.SetActive(false);
    }

    IEnumerator PlayFade()
    {
        yield return new WaitForSeconds(7);
        fadeIn.SetActive(true);
        
    }

    public void DisableElements()
    {
        background.SetActive(false);
        title.SetActive(false);
        buttonNuevoJuego.SetActive(false);
        buttonOpciones.SetActive(false);
        buttonSalir.SetActive(false);
    }

    IEnumerator EnableElements()
    {
        yield return new WaitForSeconds(7);
        background.SetActive(true);
        title.SetActive(true);
        buttonNuevoJuego.SetActive(true);
        buttonOpciones.SetActive(true);
        buttonSalir.SetActive(true);
        
    }

    IEnumerator DisableCita()
    {
        yield return new WaitForSeconds(7);
        panelNegro.SetActive(false);
        textoCita.SetActive(false);
    }
    
}
