using UnityEngine;
using System.Collections.Generic;
using System;

public class DialogEvents 
{
// Event declarations
    public static event Action<string> OnEnterDialog;
    public static event Action<string> OnDisplayDialog;
    public static event Action<List<string>> OnShowChoices;
    public static event Action OnHideChoices;
    public static event Action OnShowFinalButton;
    public static event Action OnDialogStarted;
    public static event Action OnDialogFinished;
    public static event Action<string, object> OnUpdateInkVariable;
    
    //Evento para cambios de reaccion del publico
    public static event Action<int, int> OnEpisodeRatingChanged; // (total, cambio) luego cambiar a RATING EN TODOS LOS SCRIPTS de dialogEvetns, dialogManagerINPUT y CasseSceneManagerScritp y el INK
    

    // Event triggers
    public static void EnterDialog(string knotName)
    {
        OnEnterDialog?.Invoke(knotName);
    }

    public static void DisplayDialog(string dialogText)
    {
        OnDisplayDialog?.Invoke(dialogText);
    }

    public static void ShowChoices(List<string> choices)
    {
        OnShowChoices?.Invoke(choices);
    }

    public static void HideChoices()
    {
        OnHideChoices?.Invoke();
    }

    public static void ShowFinalButton()
    {
        OnShowFinalButton?.Invoke();
    }

    public static void DialogStarted()
    {
        OnDialogStarted?.Invoke();
    }

    public static void DialogFinished()
    {
        OnDialogFinished?.Invoke();
    }

    public static void UpdateInkVariable(string variableName, object value)
    {
        OnUpdateInkVariable?.Invoke(variableName, value);
    }

    public static void UpdateEpisodeRating(int episodeRating, int change) //luego cambiar por episodeRating
    {
        OnEpisodeRatingChanged?.Invoke(episodeRating, change);
    }
    
}
