using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    
    [SerializeField] GameObject fadeOut;
    [SerializeField] GameObject fadeIn;
    [SerializeField] private AudioSource buttonClick;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fadeIn.SetActive(true);
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
        yield return new WaitForSeconds(2);
        fadeIn.SetActive(false);
    }
}
