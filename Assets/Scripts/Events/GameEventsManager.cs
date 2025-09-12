using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;


public class GameEventsManager : MonoBehaviour
{
    public static GameEventsManager instance { get; private set; }
    public InputEvents inputEvents;
    public DialogueEvents dialogueEvents;


    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than one instance of GameEventsManager in the scene");
        }
        instance = this;

        //inputEvents = new InputEvents();
        dialogueEvents = new DialogueEvents();
         
    }
    public event Action EventDialogueStarts;
    public event Action <string>EventDialogueEnds;


    public void DialogueStarts()
    {
        EventDialogueStarts?.Invoke();
    }

    public void DialogueEnds(string xyz)
    {
        EventDialogueEnds?.Invoke(xyz);
    }
}
