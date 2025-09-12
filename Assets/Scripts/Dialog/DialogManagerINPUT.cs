using UnityEngine;
using Ink.Runtime;
using System.Collections.Generic;
using System.Collections;
//using UnityEngine.UIElements;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DialogManagerINPUT : MonoBehaviour
{
    [Header("Ink Settings")]
    [SerializeField] private TextAsset[] inkScripts = new TextAsset[5];
    [SerializeField] private string[] inkNames = new string[5] { "Ink 1", "Ink 2", "Ink 3", "Ink 4", "Final Ink" };

    [Header("Auto Start Settings")]
    [SerializeField] private bool autoStartOnPlay = true;
    [SerializeField] private float delayBeforeStart = 1f;

    [Header("Show Components")]
    [SerializeField] private TVShowCharacterManager characterManager;
    [SerializeField] private TVShowSceneManager sceneManager;

    [Header("UI Components")]
    [SerializeField] public GameObject backingPanel;
    [SerializeField] public DialogPanelUIINPUT dialoguePanelUIINPUT;   //importante la clase del objeto!

    [Header ("Show Mechanics")]
    [SerializeField] private int currentEpisodeRating = 0;  //renombrar a episodeCurrentRating
    
    [Header("Android Settings")]
    [SerializeField] private bool hideSystemKeyboard = false;

    private Story story;
    private bool dialogPlaying = false;
    private bool waitingForChoice = false;
    private bool waitingForCustomInput = false;
    private string currentSpeaker = "";
    private int currentInkIndex = 0;

    private Dictionary<string, object> inkVariables = new Dictionary<string, object>();
    
    // Variables para restaurar estado al cancelar
    private List<Choice> savedChoices;
    private string savedDialogLine;

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeFirstAvailableInk();
        
        if (Application.platform == RuntimePlatform.Android)
        {
            SetupAndroidInput();
        }
    }

    private void OnEnable()
    {
        DialogEvents.OnEnterDialog += EnterDialog;
        DialogEvents.OnUpdateInkVariable += UpdateInkVariable;
    }

    private void OnDisable()
    {
        DialogEvents.OnEnterDialog -= EnterDialog;
        DialogEvents.OnUpdateInkVariable -= UpdateInkVariable;
    }

    private void Start()
    {
        if (characterManager == null)
            characterManager = FindFirstObjectByType<TVShowCharacterManager>();
        if (sceneManager == null)
            sceneManager = FindFirstObjectByType<TVShowSceneManager>();

        if (autoStartOnPlay)
        {
            StartCoroutine(AutoStartDialog());
        }
    }

    #endregion

    #region Initialization

    private void SetupAndroidInput()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        
        if (Application.platform == RuntimePlatform.Android)
        {
            if (hideSystemKeyboard)
            {
                TouchScreenKeyboard.hideInput = true;
                Debug.Log("System keyboard hidden on Android");
            }
            else
            {
                TouchScreenKeyboard.hideInput = false;
                Debug.Log("System keyboard enabled on Android");
            }
        }
        
        Debug.Log("Android input setup completed");
    }

    private void InitializeFirstAvailableInk()
    {
        for (int i = 0; i < inkScripts.Length; i++)
        {
            if (inkScripts[i] != null)
            {
                currentInkIndex = i;
                story = new Story(inkScripts[i].text);
                BindExternalFunctions();
                Debug.Log($"Initialized with {inkNames[i]} (Index: {i})");
                return;
            }
        }
        
        Debug.LogError("No hay ningún script de Ink asignado!");
    }

    private void BindExternalFunctions()
    {
        // Funciones externas si las necesitas
    }

    private IEnumerator AutoStartDialog()
    {
        yield return new WaitForSeconds(delayBeforeStart);
        StartDialog();
    }

    #endregion

    #region Dialog Control

    public void StartDialog()
    {
        currentInkIndex = 0;
        LoadNextAvailableInk();
    }

    private void LoadNextAvailableInk()
    {
        for (int i = currentInkIndex; i < inkScripts.Length; i++)
        {
            if (inkScripts[i] != null)
            {
                currentInkIndex = i;
                
                Dictionary<string, object> previousVariables = new Dictionary<string, object>(inkVariables);
                
                story = new Story(inkScripts[i].text);
                BindExternalFunctions();
                
                RestoreVariables(previousVariables);
                
                Debug.Log($"Loaded {inkNames[i]} (Index: {i})");
                
                EnterDialog("");
                return;
            }
        }
        
        Debug.Log("No hay más scripts de Ink disponibles. Finalizando diálogos.");
        DialogEvents.DialogFinished();
    }

    private void RestoreVariables(Dictionary<string, object> previousVariables)
    {
        foreach (var kvp in previousVariables)
        {
            try
            {
                if (story.variablesState.GlobalVariableExistsWithName(kvp.Key))
                {
                    story.variablesState[kvp.Key] = kvp.Value;
                    Debug.Log($"Restored variable: {kvp.Key} = {kvp.Value}");
                }
            }
            catch
            {
                inkVariables[kvp.Key] = kvp.Value;
            }
        }
    }

    private void EnterDialog(string knotName)
    {
        if (dialogPlaying)
        {
            return;
        }

        dialogPlaying = true;
        waitingForChoice = false;
        waitingForCustomInput = false;

        DialogEvents.DialogStarted();

        if (sceneManager != null)
        {
            sceneManager.SetupInitialScene();
        }

        ContinueOrExitStory();
    }

    public void ContinueDialog()
    {
        if (!dialogPlaying || waitingForChoice || waitingForCustomInput)
        {
            return;
        }

        ContinueOrExitStory();
    }

    // MÉTODO CORREGIDO: Maneja tanto opciones normales como restauradas
    public void MakeChoice(int choiceIndex)
    {
        Debug.Log($"[MakeChoice] choiceIndex={choiceIndex}, waitingForChoice={waitingForChoice}");
        
        // NUEVO: Si estamos usando opciones guardadas (después de cancelar)
        if (savedChoices != null && savedChoices.Count > 0)
        {
            if (choiceIndex < 0 || choiceIndex >= savedChoices.Count)
            {
                Debug.LogWarning($"Invalid saved choice index: {choiceIndex}");
                return;
            }
            
            string choiceText = savedChoices[choiceIndex].text;
            Debug.Log($"[MakeChoice] Selected saved choice: '{choiceText}'");
            
            waitingForChoice = false;
            DialogEvents.HideChoices();

            // Procesar la opción seleccionada
            if (choiceText.Contains("Escribir nombre")) //ESTO MUESTRA EL PANEL INPUT
            {
                // Volver a mostrar el input personalizado
                waitingForCustomInput = true;
                ShowCustomNameInput();

            }
            else
            {
                // Para las otras opciones, necesitamos simular la selección en Ink
                // Buscar la opción correspondiente en story.currentChoices
                for (int i = 0; i < story.currentChoices.Count; i++)
                {
                    if (story.currentChoices[i].text == choiceText)
                    {
                        story.ChooseChoiceIndex(i);
                        CheckEpisodeRatingChanges();
                        ProcessChoiceEffects(choiceText);
                        ProcessAfterChoice();

                        // Limpiar las opciones guardadas después de usarlas
                        //savedChoices = null;
                        //savedDialogLine = null;
                        return;
                    }
                }

                Debug.LogError($"Could not find matching choice in story: {choiceText}");
            }
            
            // Limpiar las opciones guardadas, orignalmente no estan comentadas
            //savedChoices = null;
            //savedDialogLine = null;
            return;
        }
        
        // CÓDIGO ORIGINAL para opciones normales
        if (!waitingForChoice || story.currentChoices == null || choiceIndex < 0 || choiceIndex >= story.currentChoices.Count)
        {
            Debug.LogWarning($"Invalid choice index: {choiceIndex}");
            return;
        }

        string normalChoiceText = story.currentChoices[choiceIndex].text;
        Debug.Log($"[MakeChoice] Selected choice: '{normalChoiceText}'");

        // Guardar estado para poder restaurar al cancelar
        if (normalChoiceText.Contains("Escribir nombre"))
        {
            savedChoices = new List<Choice>(story.currentChoices);
            savedDialogLine = "Qué nombre puedo ponerle al show? ";
        }

        waitingForChoice = false;
        story.ChooseChoiceIndex(choiceIndex);
        CheckEpisodeRatingChanges();


        DialogEvents.HideChoices();

        // Verificar si es la opción personalizada
        if (normalChoiceText.Contains("Escribir nombre"))
        {
            ProcessCustomNameChoice();
            //backingPanel.GetComponent<UnityEngine.UI.Image>().enabled = false;
            backingPanel.SetActive(false);

            
        }
        else
        {
            ProcessChoiceEffects(normalChoiceText);
            ProcessAfterChoice();
        }
    }

    private void CheckEpisodeRatingChanges()
    {
        if (story != null && story.variablesState != null)
        {
            try
            {
                if (story.variablesState.GlobalVariableExistsWithName("episode_rating"))
                {
                    int newRating = (int)story.variablesState["episode_rating"];
                    if (newRating != currentEpisodeRating)
                    {
                        int difference = newRating - currentEpisodeRating;
                        currentEpisodeRating = newRating;
                        DialogEvents.UpdateEpisodeRating(currentEpisodeRating, difference);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error checking episode_rating: {e.Message}");
            }
        }
    }
    
    
    private void ProcessCustomNameChoice()
    {
        Debug.Log("[ProcessCustomNameChoice] Processing custom name choice");
        
        // Continuar el diálogo hasta llegar al knot wait_for_custom_name
        while (story.canContinue)
        {
            string line = story.Continue().Trim();
            ProcessSpeakerFromLine(line);

            if (!string.IsNullOrEmpty(line))
            {
                DialogEvents.DisplayDialog(line);
                dialoguePanelUIINPUT.DisplayDialogLine(line);


                if (!string.IsNullOrEmpty(currentSpeaker) && characterManager != null)
                {
                    characterManager.OnDialogCharacterSpeak(currentSpeaker);
                }
            }

            // Verificar si hemos llegado al knot wait_for_custom_name
            if (story.state.currentPathString.Contains("wait_for_custom_name"))
            {
                Debug.Log("[ProcessCustomNameChoice] Reached wait_for_custom_name knot");
                waitingForCustomInput = true;
                ShowCustomNameInput();
                return;
            }
        }

        // Si no encontramos el knot, mostrar input de todas formas
        Debug.LogWarning("[ProcessCustomNameChoice] Did not find wait_for_custom_name knot, showing input anyway");
        waitingForCustomInput = true;
        ShowCustomNameInput();
    }

    private void ProcessChoiceEffects(string choiceText)
    {
        if (choiceText.Contains("Mi Show Estrella"))
        {
            if (characterManager != null)
            {
                characterManager.ChangeCharacterExpression("Lucia", 1);
                characterManager.ChangeCharacterExpression("Carmen", 1);
                characterManager.ChangeCharacterExpression("Leila", 0);
                characterManager.PlayCharacterVoice("Lucia", 0);
            }
        }
        else if (choiceText.Contains("El Gran Debut"))
        {
            if (characterManager != null)
            {
                characterManager.ChangeCharacterExpression("Lucia", 3);
                characterManager.ChangeCharacterExpression("Carmen", 1);
                characterManager.ChangeCharacterExpression("Leila", 1);
            }
        }
        else if (choiceText.Contains("No decidir"))
        {
            if (characterManager != null)
            {
                characterManager.ChangeCharacterExpression("Lucia", 2);
                characterManager.ChangeCharacterExpression("Carmen", 2);
                characterManager.ChangeCharacterExpression("Leila", 2);
            }
            Debug.Log("Opción 'No decidir' seleccionada");
        }
    }

    private void ProcessAfterChoice()
    {
        if (waitingForCustomInput)
        {
            Debug.Log("Waiting for custom input, not continuing dialog");
            return;
        }
        
        while (story.canContinue)
        {
            string line = story.Continue().Trim();

            ProcessSpeakerFromLine(line);
            CheckEpisodeRatingChanges();

            if (!string.IsNullOrEmpty(line))
            {
                DialogEvents.DisplayDialog(line);

                if (!string.IsNullOrEmpty(currentSpeaker) && characterManager != null)
                {
                    characterManager.OnDialogCharacterSpeak(currentSpeaker);
                }

                if (story.currentChoices.Count > 0)
                {
                    ShowChoices();
                }
                return;
            }
        }

        if (!story.canContinue && story.currentChoices.Count == 0)
        {
            AdvanceToNextInk();
        }
    }

    private void ContinueOrExitStory()
    {
        if (story.canContinue)
        {
            string currentLine = story.Continue().Trim();

            ProcessSpeakerFromLine(currentLine);
            CheckEpisodeRatingChanges();


            if (!string.IsNullOrEmpty(currentLine))
            {
                DialogEvents.DisplayDialog(currentLine);

                if (!string.IsNullOrEmpty(currentSpeaker) && characterManager != null)
                {
                    characterManager.OnDialogCharacterSpeak(currentSpeaker);
                }

                if (story.currentChoices.Count > 0)
                {
                    ShowChoices();
                }
            }
            else
            {
                ContinueOrExitStory();
            }
        }
        else
        {
            if (story.currentChoices.Count > 0)
            {
                ShowChoices();
            }
            else
            {
                AdvanceToNextInk();
            }
        }
    }

    private void AdvanceToNextInk()
    {
        SaveCurrentVariables();

        currentInkIndex++;
        
        dialogPlaying = false;
        waitingForChoice = false;
        waitingForCustomInput = false;

        Debug.Log($"Finished {inkNames[currentInkIndex - 1]}, advancing to next ink...");

        bool hasMoreInks = false;
        for (int i = currentInkIndex; i < inkScripts.Length; i++)
        {
            if (inkScripts[i] != null)
            {
                hasMoreInks = true;
                break;
            }
        }

        if (hasMoreInks)
        {
            LoadNextAvailableInk();
        }
        else
        {
            ShowFinalButton();
        }
    }

    private void SaveCurrentVariables()
    {
        if (story != null && story.variablesState != null)
        {
            try
            {
                if (story.variablesState.GlobalVariableExistsWithName("show_name"))
                {
                    inkVariables["show_name"] = story.variablesState["show_name"];
                }
                if (story.variablesState.GlobalVariableExistsWithName("episode_rating"))  // luego cambiar por episode_rating
                {
                    inkVariables["show_name"] = story.variablesState["episode_rating"];
                }
                
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Error saving variables: {e.Message}");
            }
        }
    }

    private void ProcessSpeakerFromLine(string line)
    {
        if (line.Contains(":"))
        {
            string[] parts = line.Split(':');
            if (parts.Length > 1)
            {
                string speakerName = parts[0].Trim();

                switch (speakerName)
                {
                    case "Lucia":
                        currentSpeaker = "Lucia";
                        break;
                    case "Carmen":
                        currentSpeaker = "Carmen";
                        break;
                    case "Lolita":
                        currentSpeaker = "Lolita";
                        break;
                    case "Rocio": 
                        currentSpeaker = "Rocio";
                        break;
                    case "Isaac": 
                        currentSpeaker = "Isaac";
                        break;
                    case "Hector": 
                        currentSpeaker = "Hector";
                        break;
                    default:
                        currentSpeaker = speakerName;
                        break;
                }
            }
        }
    }

    private void ShowChoices()
    {
        waitingForChoice = true;

        List<string> choiceTexts = new List<string>();
        for (int i = 0; i < story.currentChoices.Count; i++)
        {
            choiceTexts.Add(story.currentChoices[i].text);
        }

        bool isNamingScene = choiceTexts.Exists(choice =>
            choice.Contains("Caso Piteado") ||
            choice.Contains("El Gran Chongo") ||
            choice.Contains("Escribir nombre") ||
            choice.Contains("No decidir"));

        if (isNamingScene && sceneManager != null)
        {
            sceneManager.ShowNameSelectionInterface();
        }

        DialogEvents.ShowChoices(choiceTexts);
    }

    private void ShowFinalButton()
    {
        dialogPlaying = false;

        if (sceneManager != null)
        {
            sceneManager.SetupFinalScene();
            
            string finalShowName = GetCurrentShowName();
            sceneManager.UpdateShowNameDisplay(finalShowName);
        }

        DialogEvents.ShowFinalButton();

        Debug.Log($"All dialog finished - showing final button for show: {GetCurrentShowName()}");
    }

    public void StartDemoShow()
    {
        // Aquí puedes cargar la siguiente escena o iniciar el contenido principal
        Debug.Log($"¡Comenzando el show: {GetCurrentShowName()}!");
        SceneManager.LoadScene("Caso");


        DialogEvents.DialogFinished(); //version input para ver que trip
    }

    public void LoadNewScript(TextAsset newInkJSON)
    {
        if (newInkJSON != null)
        {
            inkScripts[currentInkIndex] = newInkJSON;
            story = new Story(newInkJSON.text);
            BindExternalFunctions();

            Debug.Log($"New Ink script loaded at index {currentInkIndex}: {newInkJSON.name}");
        }
    }

    public void StopDialog()
    {
        dialogPlaying = false;
        waitingForChoice = false;
        waitingForCustomInput = false;

        DialogEvents.DialogFinished();

        Debug.Log("Dialog stopped");
    }

    #endregion

    #region Show Integration Methods

    // Ocultar el diálogo de abajo
    private void ShowCustomNameInput()
    {
        DialogEvents.HideChoices();
        
        
        // CRÍTICO: Ocultar el texto del diálogo de abajo
        DialogEvents.DisplayDialog("");

        if (sceneManager != null)
        {
            sceneManager.ShowCustomNameInput();
        }
        
        Debug.Log("Showing custom name input...");
    }

    // Sin restricción de caracteres ni longitud
    public void OnCustomNameEntered(string customName)
    {
        if (string.IsNullOrEmpty(customName?.Trim()))
        {
            Debug.LogWarning("Nombre vacío, manteniendo input activo");
            return;
        }

        string trimmedName = customName.Trim();
        
        Debug.Log($"Custom name entered: {trimmedName}");
        
        // Establecer la variable en Ink
        UpdateInkVariable("show_name", trimmedName);
        
        waitingForCustomInput = false;

        if (sceneManager != null)
        {
            sceneManager.HideCustomNameInput();
        }

        // Continuar el diálogo
        ContinueFromCustomNameInput();
    }

    private void ContinueFromCustomNameInput()
    {
        Debug.Log("[ContinueFromCustomNameInput] Continuing dialog after custom name input");
        
        // Continuar el diálogo
        while (story.canContinue)
        {
            string line = story.Continue().Trim();
            ProcessSpeakerFromLine(line);

            if (!string.IsNullOrEmpty(line))
            {
                //backingPanel.GetComponent<UnityEngine.UI.Image>().enabled = true;
                backingPanel.SetActive(true);


                DialogEvents.DisplayDialog(line);

                if (!string.IsNullOrEmpty(currentSpeaker) && characterManager != null)
                {
                    characterManager.OnDialogCharacterSpeak(currentSpeaker);
                }

                if (story.currentChoices.Count > 0)
                {
                    ShowChoices();
                }
                return;
            }
        }

        // Si no hay más contenido, avanzar al siguiente ink
        if (!story.canContinue && story.currentChoices.Count == 0)
        {
            AdvanceToNextInk();
        }
    }

    // MÉTODO CORREGIDO: Enfoque más simple y confiable
    public void OnCustomNameCancelled()
    {
        Debug.Log("Custom name input cancelled - restoring previous state");
        
        waitingForChoice = true;
        waitingForCustomInput = false;

        if (sceneManager != null)
        {
            sceneManager.HideCustomNameInput();
            sceneManager.ShowNameSelectionInterface();
        }

        // NUEVO ENFOQUE: En lugar de manipular el estado de Ink,
        // simplemente mostrar las opciones guardadas directamente
        if (savedChoices != null && savedChoices.Count > 0)
        {
            // Mostrar el diálogo guardado
            if (!string.IsNullOrEmpty(savedDialogLine))
            {
                DialogEvents.DisplayDialog(savedDialogLine);
            }
            
            // Crear lista de textos de opciones
            List<string> choiceTexts = new List<string>();
            foreach (var choice in savedChoices)
            {
                choiceTexts.Add(choice.text);
            }
            
            // Mostrar las opciones usando el evento
            DialogEvents.ShowChoices(choiceTexts);
            backingPanel.SetActive(true);

            
            Debug.Log($"[OnCustomNameCancelled] Restored {choiceTexts.Count} choices");
        }
        else
        {
            Debug.LogError("[OnCustomNameCancelled] No saved choices to restore!");
        }
    }

    private string GetCurrentShowName()
    {
        object showName = GetInkVariable("show_name");
        
        if (showName != null && !string.IsNullOrEmpty(showName.ToString().Trim()))
        {
            return showName.ToString().Trim();
        }
        
        return "Programa Sin Nombre";
    }

    #endregion

    #region Variable Management

    private void UpdateInkVariable(string variableName, object value)
    {
        if (story != null && story.variablesState != null)
        {
            story.variablesState[variableName] = value;
            Debug.Log($"Ink variable set: {variableName} = {value}");
        }

        if (inkVariables.ContainsKey(variableName))
        {
            inkVariables[variableName] = value;
        }
        else
        {
            inkVariables.Add(variableName, value);
        }

        Debug.Log($"Variable updated: {variableName} = {value}");
    }

    #endregion

    #region Public Getters

    public bool IsDialogPlaying()
    {
        return dialogPlaying;
    }

    public bool IsWaitingForChoice()
    {
        return waitingForChoice;
    }

    public bool IsWaitingForCustomInput()
    {
        return waitingForCustomInput;
    }

    public string GetCurrentSpeaker()
    {
        return currentSpeaker;
    }

    public object GetInkVariable(string variableName)
    {
        if (story != null && story.variablesState != null)
        {
            try
            {
                return story.variablesState[variableName];
            }
            catch
            {
                // Si falla, usar la copia local
            }
        }

        return inkVariables.ContainsKey(variableName) ? inkVariables[variableName] : null;
    }

    public int GetCurrentInkIndex()
    {
        return currentInkIndex;
    }

    public string GetCurrentInkName()
    {
        return inkNames[currentInkIndex];
    }

    public bool IsSystemKeyboardHidden()
    {
        return hideSystemKeyboard;
    }
    
    public int GetCurrentEpisodeRating()    
    {
        return currentEpisodeRating;
    }

    #endregion
}