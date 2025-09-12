using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroEvents : MonoBehaviour
{
    public GameObject PanelNegro;
    public GameObject LuciaSprite;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(EventStarter());
        LuciaSprite.SetActive(false);

    }

    IEnumerator EventStarter()
    {
        yield return new WaitForSeconds(6);
        LuciaSprite.SetActive(true);
        //LuciaSprite.GetComponent<Animator>().Play("LuciaScaleUp");
        yield return new WaitForSeconds(1);
        PanelNegro.SetActive(true);


    }

}
