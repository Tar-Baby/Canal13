using UnityEngine;
using Ink.Runtime;

public class InkService : MonoBehaviour
{
    public static InkService Instance { get; private set; }
    public Story ActiveStory { get; private set; }

    public System.Action<Story> OnActiveStoryChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetActiveStory(Story story)
    {
        ActiveStory = story;
        OnActiveStoryChanged?.Invoke(story);
    }

    // Must use VariablesState.VariableObserver here:
    /*public void ObserveVar(string varName, VariablesState.VariableObserver observer)
    {
        ActiveStory?.variablesState?.ObserveVariable(varName, observer);
    }

    public void RemoveObserver(string varName, VariablesState.VariableObserver observer)
    {
        ActiveStory?.variablesState?.RemoveVariableObserver(observer, varName);
    }*/

    // Also include your GetVar/SetVar, Save/Load here (see next section)
}