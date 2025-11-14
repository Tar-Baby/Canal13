using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroEvents : MonoBehaviour
{
    public GameObject Fondo;

    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(EventStarter());
        Fondo.SetActive(false);

    }

    IEnumerator EventStarter()
    {
        yield return new WaitForSeconds(6);
        Fondo.SetActive(true);
        yield return new WaitForSeconds(1);



    }

}
