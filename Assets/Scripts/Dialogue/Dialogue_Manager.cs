using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;

public class Dialogue_Manager : MonoBehaviour
{
    [Header("Ink Story")]
    [SerializeField] private TextAsset inkJson;
    private Story story;
    
    private bool dialoguePlaying = true;

    private void Awake()
    {
        story = new Story(inkJson.text);
        GameEventsManager.instance.DialogueStarts();//avisas quiero activar
        GameEventsManager.instance.DialogueEnds("hello");
    }

    private void OnEnable()
    {
        //GameEventsManager.instance.dialogueEvents.onEnterDialogue += EnterDialogue;
        //GameEventsManager.instance.inputEvents.onSubmitPressed += SubmitPressed;
        GameEventsManager.instance.EventDialogueStarts += myfunc; //reaccionas
        GameEventsManager.instance.EventDialogueEnds += otrafunc;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.dialogueEvents.onEnterDialogue -= EnterDialogue;
        GameEventsManager.instance.inputEvents.onSubmitPressed -= SubmitPressed;
    }

    void myfunc()
    {
        
    }

    void otrafunc(string abc)
    {
        
    }
    private void SubmitPressed()
    {
        Debug.Log("Historia");
        if (!dialoguePlaying)
        {
            return;
        }
        
        ContinueOrExitStory();
    }
    
    private void EnterDialogue(string knotName)
    {
        if (dialoguePlaying)
        {
            return;
        }
        dialoguePlaying = true;
        
        GameEventsManager.instance.dialogueEvents.DialogueStarted();

        if (!knotName.Equals(""))
        {
            story.ChoosePathString(knotName);
        }
        else
        {
            Debug.LogWarning("Knot name is empty");
        }

        ContinueOrExitStory();
        Debug.Log("Entering dialogue for knot name:" + knotName);
    }

    private void ContinueOrExitStory()
    {
        if (story.canContinue)
        {
            string dialogueLine = story.Continue();
            GameEventsManager.instance.dialogueEvents.DisplayDialogue(dialogueLine);
            
            Debug.Log(dialogueLine);
        }
        else
        {
            ExitDialogue();
        }
    }

    private void ExitDialogue()
    {
        Debug.Log("Exiting dialogue");
        dialoguePlaying = false;
        
        story.ResetState();
    }
}
