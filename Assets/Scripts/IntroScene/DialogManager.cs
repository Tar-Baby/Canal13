using UnityEngine;
using Ink.Runtime;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class DialogManager : MonoBehaviour
{
    [Header("Ink Settings")]
    [SerializeField] private TextAsset[] inkScripts = new TextAsset[5]; // Array de 5 scripts
    [SerializeField] private string[] inkNames = new string[5] { "Ink 1", "Ink 2", "Ink 3", "Ink 4", "Final Ink" };

    [Header("Auto Start Settings")]
    [SerializeField] private bool autoStartOnPlay = true;
    [SerializeField] private float delayBeforeStart = 1f;

    [Header("Show Components")]
    [SerializeField] private GameObject characterManager;
    [SerializeField] private GameObject sceneManager;

    [Header("UI Components")]
    [SerializeField] public GameObject backingPanel;
    [SerializeField] private DialogPanelUI dialoguePanelUI;   //importante la clase del objeto!
    
    [Header("Android Settings")]
    [SerializeField] private bool hideSystemKeyboard = false;
    
    private Story story;
    private bool dialogPlaying = false;
    private bool waitingForChoice = false;
    private bool waitingForCustomInput = false;
    private string currentSpeaker = "";
    private int currentInkIndex = 0; // Índice del ink actual

    // Diccionario para mantener variables sincronizadas entre inks
    private Dictionary<string, object> inkVariables = new Dictionary<string, object>();

    // Variables para restaurar estado al cancelar
    private List<Choice> savedChoices;
    private string savedDialogLine;
    
    private void Awake()
    {
        // Inicializar con el primer ink disponible
        InitializeFirstAvailableInk();
        
        if (Application.platform == RuntimePlatform.Android)
        {
            SetupAndroidInput();
        }
    }

    private void InitializeFirstAvailableInk()
    {
        // Buscar el primer ink que no esté vacío
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
        // No necesitamos external functions para este script simple
        // Todas las acciones se manejarán via tags o directamente
    }

    private void OnEnable()   //agregar esto a nuevo script de personajes y sus animators
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
        // Get references if not assigned
        if (characterManager == null)
            characterManager = FindFirstObjectByType<GameObject>();
        if (sceneManager == null)
            sceneManager = FindFirstObjectByType<GameObject>();

        // Auto start si está habilitado
        if (autoStartOnPlay)
        {
            StartCoroutine(AutoStartDialog());
        }
    }
    
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


    private IEnumerator AutoStartDialog()
    {
        yield return new WaitForSeconds(delayBeforeStart);
        StartDialog();
    }

    #region Dialog Control

    public void StartDialog()
    {
        // Empezar desde el primer ink disponible
        currentInkIndex = 0;
        LoadNextAvailableInk();
    }

    private void LoadNextAvailableInk()
    {
        // Buscar el siguiente ink disponible desde currentInkIndex
        for (int i = currentInkIndex; i < inkScripts.Length; i++)
        {
            if (inkScripts[i] != null)
            {
                currentInkIndex = i;
                
                // Transferir variables del ink anterior (si las hay)
                Dictionary<string, object> previousVariables = new Dictionary<string, object>(inkVariables);
                
                // Crear nueva historia
                story = new Story(inkScripts[i].text);
                BindExternalFunctions();
                
                // Restaurar variables importantes
                RestoreVariables(previousVariables);
                
                Debug.Log($"Loaded {inkNames[i]} (Index: {i})");
                
                // Empezar el diálogo
                EnterDialog("");
                return;
            }
        }
        
        // Si llegamos aquí, no hay más inks disponibles
        Debug.Log("No hay más scripts de Ink disponibles. Finalizando diálogos.");
        DialogEvents.DialogFinished();
    }

    private void RestoreVariables(Dictionary<string, object> previousVariables)
    {
        // Restaurar variables importantes como show_name
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
                // Si falla, mantener en el diccionario local
                inkVariables[kvp.Key] = kvp.Value;
            }
        }
    }

    private void EnterDialog(string knotName)
    {
        // Evitar entrar al diálogo si ya está activo
        if (dialogPlaying)
        {
            return;
        }

        dialogPlaying = true;
        waitingForChoice = false;
        waitingForCustomInput = false;

        Debug.Log("Entering dialogue for knot name:" + knotName);

        // Iniciar el diálogo con efectos del show
        DialogEvents.DialogStarted();

        // Set initial scene
        /*if (sceneManager != null)
        {
            // Mostrar a Lucia primero (según el storyboard)
            SceneManager.LoadScene("Intro");
        }*/

        // Mostrar la primera línea
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
            UpdateInkVariable("show_name", choiceText);
            Debug.Log($"[MakeChoice] Selected saved choice: '{choiceText}'");
            
            waitingForChoice = false;
            DialogEvents.HideChoices();

            // Procesar la opción seleccionada
            if (choiceText.Contains("Escribir nombre"))
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
            
            // Limpiar las opciones guardadas
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
        UpdateInkVariable("show_name", normalChoiceText);
        Debug.Log($"[MakeChoice] Selected choice: '{normalChoiceText}'");
        


        // Guardar estado para poder restaurar al cancelar
        if (normalChoiceText.Contains("Escribir nombre"))
        {
            savedChoices = new List<Choice>(story.currentChoices);
            savedDialogLine = "Qué nombre puedo ponerle al show?";
        }

        waitingForChoice = false;
        story.ChooseChoiceIndex(choiceIndex);

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
                dialoguePanelUI.DisplayDialogLine(line);


                if (!string.IsNullOrEmpty(currentSpeaker) && characterManager != null)
                {
                    //characterManager.OnDialogCharacterSpeak(currentSpeaker);
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
        //backingPanel.GetComponent<UnityEngine.UI.Image>().enabled = true;
        backingPanel.SetActive(true);

    }

    private void ProcessChoiceEffects(string choiceText)
    {
        // Handle specific choice effects based on name_program.json
        if (choiceText.Contains("Caso Piteado"))
        {
            // Reacciones de los personajes a "Mi Show Estrella"
            if (characterManager != null)
            {
                //characterManager.ChangeCharacterExpression("Lucia", 1); // Happy
                //characterManager.ChangeCharacterExpression("Carmen", 1); // Happy
                //characterManager.ChangeCharacterExpression("Leila", 0); // Mysterious
                //characterManager.PlayCharacterVoice("Lucia", 0);
            }
        }
        else if (choiceText.Contains("El Gran Chongo"))
        {
            // Reacciones a "El Gran Debut"
            if (characterManager != null)
            {
                //characterManager.ChangeCharacterExpression("Lucia", 3); // Excited
                //characterManager.ChangeCharacterExpression("Carmen", 1); // Happy
                //characterManager.ChangeCharacterExpression("Leila", 1); // Neutral
            }
        }
        else if (choiceText.Contains("Escribir nombre"))
        {
            // Activar input personalizado
            ShowCustomNameInput();
        }
        else if (choiceText.Contains("No decidir nombre ahora"))
        {
            // Reacciones negativas
            if (characterManager != null)
            {
                //characterManager.ChangeCharacterExpression("Lucia", 2); // Confused
                //characterManager.ChangeCharacterExpression("Carmen", 2); // Nervous
                //characterManager.ChangeCharacterExpression("Leila", 2); // Mysterious
            }
        }
    }

    private void ProcessAfterChoice()
    {
        
        if (waitingForCustomInput)
        {
            Debug.Log("Waiting for custom input, not continuing dialog");
            return;
        }
        
        // Procesar cualquier lógica tras la elección
        while (story.canContinue)
        {
            string line = story.Continue().Trim();

            // Procesar speakers basado en el contenido
            ProcessSpeakerFromLine(line);

            // Si la línea no está vacía, mostrarla y parar
            if (!string.IsNullOrEmpty(line))
            {
                DialogEvents.DisplayDialog(line);

                // Set current speaker for character highlighting
                if (!string.IsNullOrEmpty(currentSpeaker) && characterManager != null)
                {
                    //characterManager.OnDialogCharacterSpeak(currentSpeaker);
                }

                // Verificar si hay más contenido después de esta línea
                if (story.currentChoices.Count > 0)
                {
                    ShowChoices();
                }
                return; // Salir después de mostrar la primera línea no vacía
            }
        }

        // Si llegamos aquí y no hay más contenido, pasar al siguiente ink
        if (!story.canContinue && story.currentChoices.Count == 0)
        {
            // Avanzar al siguiente ink
            AdvanceToNextInk();
        }
    }

    private void ContinueOrExitStory()
    {
        if (story.canContinue)
        {
            // Obtener la siguiente línea de diálogo
            string currentLine = story.Continue().Trim();

            // Process speaker from line content
            ProcessSpeakerFromLine(currentLine);

            // Solo mostrar líneas que no estén vacías
            if (!string.IsNullOrEmpty(currentLine))
            {
                DialogEvents.DisplayDialog(currentLine);
                dialoguePanelUI.DisplayDialogLine(currentLine);

                // Handle character speaking
                if (!string.IsNullOrEmpty(currentSpeaker) && characterManager != null)
                {
                    //characterManager.OnDialogCharacterSpeak(currentSpeaker);
                }

                // Verificar si hay opciones después de esta línea
                if (story.currentChoices.Count > 0)
                {
                    ShowChoices();
                }
            }
            else
            {
                // Si la línea está vacía, continuar automáticamente
                ContinueOrExitStory();
            }
        }
        else
        {
            // No hay más líneas, verificar si hay opciones pendientes
            if (story.currentChoices.Count > 0)
            {
                ShowChoices();
            }
            else
            {
                // No hay más contenido, avanzar al siguiente ink
                AdvanceToNextInk();
            }
        }
    }

    private void AdvanceToNextInk()
    {
        // Guardar variables importantes antes de cambiar de ink
        SaveCurrentVariables();

        // Incrementar el índice
        currentInkIndex++;
        
        // Resetear estado
        dialogPlaying = false;
        waitingForChoice = false;
        waitingForCustomInput = false;


        Debug.Log($"Finished {inkNames[currentInkIndex - 1]}, advancing to next ink...");

        // Verificar si hay más inks disponibles
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
            // Hay más inks, cargar el siguiente
            LoadNextAvailableInk();
        }
        else
        {
            // No hay más inks, mostrar botón final
            ShowFinalButton();
        }
    }

    private void SaveCurrentVariables()
    {
        // Guardar variables importantes del ink actual
        if (story != null && story.variablesState != null)
        {
            try
            {
                // Guardar show_name si existe
                if (story.variablesState.GlobalVariableExistsWithName("show_name"))
                {
                    inkVariables["show_name"] = story.variablesState["show_name"];
                }
                
                // Agregar otras variables importantes aquí si las tienes
                
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Error saving variables: {e.Message}");
            }
        }
    }

    private void ProcessSpeakerFromLine(string line)
    {
        // Extraer el speaker del formato "Nombre: texto"
        if (line.Contains(":"))
        {
            string[] parts = line.Split(':');
            if (parts.Length > 1)
            {
                string speakerName = parts[0].Trim();

                // Mapear nombres del ink a nombres de personajes
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

        // Crear lista de opciones para enviar al UI
        List<string> choiceTexts = new List<string>();
        for (int i = 0; i < story.currentChoices.Count; i++)
        {
            choiceTexts.Add(story.currentChoices[i].text);
        }

        // Special handling for show name choices
        bool isNamingScene = choiceTexts.Exists(choice =>
            choice.Contains("Caso Piteado") ||
            choice.Contains("El Gran Chongo") ||
            choice.Contains("Escribir nombre"));

        if (isNamingScene && dialoguePanelUI != null)
        {
            // Mostrar la interfaz de selección de nombre
            dialoguePanelUI.ShowNameSelectionInterface();
        }

        // Enviar opciones al UI
        DialogEvents.ShowChoices(choiceTexts);
    }

    private void ShowFinalButton()
    {
        dialogPlaying = false;

        // Final scene setup
        if (sceneManager != null)
        {
            //sceneManager.SetupFinalScene();
        }

        // Mostrar solo el botón "Comenzar" (no el de continuar)
        DialogEvents.ShowFinalButton();

        Debug.Log($"All dialog finished - showing final button for show: {GetCurrentShowName()}");
    }

    public void StartDemoShow()
    {
        // Aquí puedes cargar la siguiente escena o iniciar el contenido principal
        Debug.Log($"¡Comenzando el show: {GetCurrentShowName()}!");
        SceneManager.LoadScene("PrimerCaso");


        DialogEvents.DialogFinished();
    }

    public void LoadNewScript(TextAsset newInkJSON)
    {
        if (newInkJSON != null)
        {
            // Reemplazar el ink actual
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


        // Hide dialog UI
        DialogEvents.DialogFinished();

        Debug.Log("Dialog stopped");
    }

    #region Show Integration Methods

    private void ShowCustomNameInput()
    {
        DialogEvents.HideChoices();
        
        // CRÍTICO: Ocultar el texto del diálogo de abajo
        DialogEvents.DisplayDialog("");
        
        // Pausar el diálogo mientras se espera el input
        waitingForChoice = true;

        // Mostrar el input personalizado
        if (dialoguePanelUI != null)
        {
            dialoguePanelUI.ShowCustomNameInput();
        }
        
        Debug.Log("Showing custom name input...");

    }
    
    

    // Método llamado cuando el usuario confirma el nombre personalizado
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

        if (dialoguePanelUI != null)
        {
            dialoguePanelUI.HideCustomNameInput();
        }

        // Continuar el diálogo
        ContinueFromCustomNameInput();
    }

    private void ContinueFromCustomNameInput()
    {
        
        // Continuar el diálogo
        while (story.canContinue)
        {
            Debug.Log("[ContinueFromCustomNameInput] Continuing dialog after custom name input");

            string line = story.Continue().Trim();
            ProcessSpeakerFromLine(line);
            backingPanel.SetActive(true);

            if (!string.IsNullOrEmpty(line))
            {
                //backingPanel.GetComponent<UnityEngine.UI.Image>().enabled = true;
                backingPanel.SetActive(true);
                DialogEvents.DisplayDialog(line);
                dialoguePanelUI.DisplayDialogLine(line);


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

        if (dialoguePanelUI != null)
        {
            dialoguePanelUI.HideCustomNameInput();
            dialoguePanelUI.ShowNameSelectionInterface();
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
            // Actualizar la variable en Ink
            story.variablesState[variableName] = value;
        }

        // Mantener copia local
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
        // Primero intentar obtener de Ink
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

    #endregion

}
