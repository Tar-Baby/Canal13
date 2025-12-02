using UnityEngine;
using Ink.Runtime;
using System.Collections.Generic;
using System.Collections;
//using UnityEngine.UIElements;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // Necesario para TextMeshProUGUI
using System.Linq; // For LINQ operations like .FirstOrDefault

public class DialogManagerINPUT : MonoBehaviour
{
    [Header("Ink Settings")]
    [SerializeField] private TextAsset[] inkScripts = new TextAsset[5];
    [SerializeField] private string[] inkNames = new string[5] { "Ink 1", "Ink 2", "Ink 3", "Ink 4", "Final Ink" };

    [Header("Auto Start Settings")]
    [SerializeField] private bool autoStartOnPlay = true;
    [SerializeField] private float delayBeforeStart = 15f;

    [Header("Show Components")]
    [SerializeField] private TVShowCharacterManager characterManager;
    [SerializeField] private TVShowSceneManager sceneManager;

    [Header("UI Components")]
    [SerializeField] public GameObject backingPanel;
    [SerializeField] public DialogPanelUIINPUT dialoguePanelUIINPUT;   //importante la clase del objeto!
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI caseText; // Referencia al TextMeshProUGUI "CaseText" (o DialogueText)

    [Header ("Show Mechanics")]
    [SerializeField] private int currentEpisodeRating = 0;  //renombrar a episodeCurrentRating
    
    [Header("Android Settings")]
    [SerializeField] private bool hideSystemKeyboard = false;
    
    [Header("Text Effects Settings")]
    [SerializeField] private float typewriterSpeed = 0.05f; // Velocidad del efecto de máquina de escribir
    [SerializeField] private bool debugMode = true; // Modo depuración para logs adicionales
    [SerializeField] private float wiggleStrength = 5f;
    
    [System.Serializable]
    public class WigglePreset
    {
        public string presetName;
        public float strength = 5f;
        public float speed = 25f;
    }
    
    [SerializeField] private List<WigglePreset> wigglePresets = new List<WigglePreset>();
    private float currentWiggleStrength = 5f;
    private float currentWiggleSpeed = 25f;

    private bool wiggleActive = false;
    private Coroutine wiggleRoutine;

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
    
    public static System.Action<List<string>> OnTagsProcessed;
    
    [Header("Character Sprites & Portraits")]
    [SerializeField]
    private CharacterSpriteDatabase _spriteDatabase;
    [SerializeField]
    private List<CharacterPortrait> _characterPortraits; // All character slots

    // Internal tracking of which character is in which slot
    private Dictionary<string, CharacterPortrait> _activeCharacters =
        new Dictionary<string, CharacterPortrait>();
    private CharacterPortrait _currentSpeakerPortrait;
    
    [System.Serializable]
    public class NamedAudio
    {
        public string key;  // the label you’ll use in Ink, e.g. THEME1, APPLAUSE
        public AudioClip clip;  // assign the actual AudioClip here
    }
    [Header("Audio Libraries")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource sfxButtonSource;
    [SerializeField] private List<NamedAudio> musicLibrary;
    [SerializeField] private List<NamedAudio> sfxLibrary;
    
    [System.Serializable]
    public class SoundEffectPreset
    {
        public string presetName;          // e.g. "ECHO", "REVERB", "PHONE"
        [Header("Filter Settings")]
        public bool useEcho;
        public float echoDelay = 300f;
        public float echoDecay = 0.4f;

        public bool useReverb;
        public AudioReverbPreset reverbType = AudioReverbPreset.Room;

        public bool useLowPass;
        public float cutoffFrequency = 5000f;

        // future additions: pitch, volume, distortion etc.
    }
    
    [Header("SFX Filters & Presets")]
    [SerializeField] private AudioSource sfx_Source;
    [SerializeField] private AudioEchoFilter sfxEchoFilter;
    [SerializeField] private AudioReverbFilter sfxReverbFilter;
    [SerializeField] private AudioLowPassFilter sfxLowPassFilter;
    [SerializeField] private List<SoundEffectPreset> sfxPresets;
    
    // ===========================
// INK TAG PROCESSING
// ===========================
    private void HandleInkTags(List<string> tags)
    {
        if (tags == null || tags.Count == 0) return;

        // 1. Broadcast tags globally so UI or other systems can respond
        OnTagsProcessed?.Invoke(tags);

        // 2. Optional internal handling (examples)
        foreach (string rawTag in tags)
        {
            string tag = rawTag.Trim().ToLower();

 
            
            {
                Debug.Log($"[Ink Tag] {tag}");
            }
        }
    }

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
        DialogManagerINPUT.OnTagsProcessed += HandleTags;
       
    }

    private void OnDisable()
    {
        DialogEvents.OnEnterDialog -= EnterDialog;
        DialogEvents.OnUpdateInkVariable -= UpdateInkVariable;
        DialogManagerINPUT.OnTagsProcessed -= HandleTags;
    }

    private void Start()
    {
        UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;
        if (characterManager == null)
            characterManager = FindFirstObjectByType<TVShowCharacterManager>();
        if (sceneManager == null)
            sceneManager = FindFirstObjectByType<TVShowSceneManager>();

        if (autoStartOnPlay)
        {
            StartCoroutine(AutoStartDialog());
        }
        
        foreach (var portrait in _characterPortraits)
        {
            portrait.Clear();
        }
        _activeCharacters.Clear();
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
                InkService.Instance?.SetActiveStory(story);
                return;
            }

            InkService.Instance.SetActiveStory(story);
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
                
                // Register new active story with service
                InkService.Instance?.SetActiveStory(story);
                
                RestoreVariables(previousVariables);
                
                Debug.Log($"Loaded {inkNames[i]} (Index: {i})");
                
                EnterDialog("");
                return;
            }

            InkService.Instance.SetActiveStory(story);
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
            if (story.currentTags != null && story.currentTags.Count > 0)
            {
                HandleInkTags(story.currentTags);
            }
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
                characterManager.ChangeCharacterExpression("Lucía", 1);
                characterManager.ChangeCharacterExpression("Carmen", 1);
                characterManager.ChangeCharacterExpression("Lolita", 0);
                characterManager.PlayCharacterVoice("Lucía", 0);
            }
        }
        else if (choiceText.Contains("El Gran Debut"))
        {
            if (characterManager != null)
            {
                characterManager.ChangeCharacterExpression("Lucía", 3);
                characterManager.ChangeCharacterExpression("Carmen", 1);
                characterManager.ChangeCharacterExpression("Lolita", 1);
            }
        }
        else if (choiceText.Contains("No decidir"))
        {
            if (characterManager != null)
            {
                characterManager.ChangeCharacterExpression("Lucía", 2);
                characterManager.ChangeCharacterExpression("Carmen", 2);
                characterManager.ChangeCharacterExpression("Lolita", 2);
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
            
            if (story.currentTags != null && story.currentTags.Count > 0)
            {
                HandleInkTags(story.currentTags);
            }

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
            string speakerName = "";

            // Process speaker and rating vars
            speakerName = ProcessSpeakerFromLine(currentLine);
            CheckEpisodeRatingChanges();

            // >>> NEW: Handle Ink tags for this line <<<
            if (story.currentTags != null && story.currentTags.Count > 0)
            {
                HandleInkTags(story.currentTags);
            }

            // Show line if valid
            if (!string.IsNullOrEmpty(currentLine))
            {
                DialogEvents.DisplayDialog(currentLine);

                if (!string.IsNullOrEmpty(currentSpeaker) && characterManager != null)
                {
                    characterManager.OnDialogCharacterSpeak(currentSpeaker);
                }
                
                // --- Animate current speaker ---
                if (speakerNameText != null) speakerNameText.text = speakerName;
                
                //  Update portraits immediately (fade/scale starts here)
                UpdateSpeakerAnimation(speakerName);

                // If the current line produces choices, show them
                if (story.currentChoices.Count > 0)
                {
                    ShowChoices();
                }
            }
            else
            {
                // Skip blank lines
                ContinueOrExitStory();
            }
        }
        else
        {
            // END or CHOICES
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

    private string ProcessSpeakerFromLine(string line)
    {
        if (line.Contains(":"))
        {
            string[] parts = line.Split(':');
            if (parts.Length > 1)
            {
                string speakerName = parts[0].Trim();

                switch (speakerName)
                {
                    case "Lucía":
                        currentSpeaker = "Lucía";
                        break;
                    case "Carmen":
                        currentSpeaker = "Carmen";
                        break;
                    case "Lolita":
                        currentSpeaker = "Lolita";
                        break;
                    case "Rocío": 
                        currentSpeaker = "Rocío";
                        break;
                    case "Isaac": 
                        currentSpeaker = "Isaac";
                        break;
                    case "Héctor": 
                        currentSpeaker = "Héctor";
                        break;
                    default:
                        currentSpeaker = speakerName;
                        break;
                }
            }
        }

        return currentSpeaker;
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
        DialogEvents.DialogFinished(); //version input para ver que trip
        SceneManager.LoadScene("Caso");


        
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
            if (story.currentTags != null && story.currentTags.Count > 0)
            {
                HandleInkTags(story.currentTags);
            }
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
    

    private void UpdateSpeakerAnimation(string currentSpeaker)
    {
        Debug.Log($"[UpdateSpeakerAnimation] Speaker = '{currentSpeaker}'");

        if (string.IsNullOrEmpty(currentSpeaker) ||
            currentSpeaker.Equals("Narrador", System.StringComparison.OrdinalIgnoreCase))
        {
            foreach (var kvp in _activeCharacters)
            {
                // only adjust scale, leave fadeIn to finish
                kvp.Value.SetIdleState();

                // keep everyone bright, but preserve alpha (fade still runs)
                kvp.Value.ApplySpeakerTint(true);
            }
            _currentSpeakerPortrait = null;
            return;
        }

        foreach (var kvp in _activeCharacters)
        {
            if (string.Equals(kvp.Key, currentSpeaker, System.StringComparison.OrdinalIgnoreCase))
            {
                kvp.Value.SetTalkingState();   // current speaker pops & stays enlarged
                kvp.Value.ApplySpeakerTint(true);  // brighten speaker
                _currentSpeakerPortrait = kvp.Value;
            }
            else
            {
                kvp.Value.SetIdleState();      // everyone else idle
                kvp.Value.ApplySpeakerTint(false); // smoothly dim others
            }
        }
    }
    
    private void HandleTags(List<string> tags)
    {
        foreach (var tag in tags)
        {
            string trimmedTag = tag.Trim().ToUpper();

            // Format: #SHOW_CHARACTERNAME_SLOTNAME
            // Example: #SHOW_HERO_LEFT, #SHOW_VILLAIN_RIGHT
            if (trimmedTag.StartsWith("SHOW_"))
            {
                string[] parts = trimmedTag.Split('_', 3); // SHOW_NAME_SLOT
                if (parts.Length == 3)
                {
                    string characterName = parts[1];
                    string slotName = parts[2];
                    ShowCharacter(characterName, slotName);
                }
                else
                {
                    Debug.LogWarning($"Invalid SHOW tag: {tag}");
                }
            }
            // Format: #HIDE_CHARACTERNAME or #HIDE_ALL
            // Example: #HIDE_HERO, #HIDE_ALL
            else if (trimmedTag.StartsWith("HIDE_"))
            {
                string[] parts = trimmedTag.Split('_', 2);
                if (parts.Length == 2)
                {
                    string characterToHide = parts[1];
                    if (characterToHide == "ALL")
                    {
                        HideAllCharacters();
                    }
                    else
                    {
                        HideCharacter(characterToHide);
                    }
                }
                else
                {
                    Debug.LogWarning($"Invalid HIDE tag: {tag}");
                }
            }
            // Format: #EXPRESSION_CHARACTERNAME_EXPRESSIONNAME
            // Example: #EXPRESSION_HERO_HAPPY
            else if (trimmedTag.StartsWith("EXPRESSION_"))
            {
                string[] parts = trimmedTag.Split('_', 3);
                if (parts.Length == 3)
                {
                    string characterName = parts[1];
                    string expressionName = parts[2];
                    SetCharacterExpression(characterName, expressionName);
                }
                else
                {
                    Debug.LogWarning($"Invalid EXPRESSION tag: {tag}");
                }
            }
            // Format: #FADEALL
            else if (trimmedTag == "FADEALL")
            {
                FadeAllCharactersToIdle();
            }
            // Add more tag handlers here (e.g., #MOVE_CHARACTER_SLOT)

            
            else if (trimmedTag.StartsWith("WIGGLE"))
            {
                string[] parts = trimmedTag.Split('_');
                string presetName = parts.Length > 1 ? parts[1] : "DEFAULT";

                WigglePreset preset = wigglePresets
                    .FirstOrDefault(p => p.presetName.Equals(presetName, System.StringComparison.OrdinalIgnoreCase));

                if (preset != null)
                {
                    currentWiggleStrength = preset.strength;
                    currentWiggleSpeed = preset.speed;
                    Debug.Log($"[Wiggle] Using preset {preset.presetName}");
                }
                else
                {
                    currentWiggleStrength = 5f;
                    currentWiggleSpeed = 25f;
                }

                wiggleActive = true;

                if (wiggleRoutine != null) StopCoroutine(wiggleRoutine);
                wiggleRoutine = StartCoroutine(ContinuousWiggle());
            }
            else if (trimmedTag == "NO_WIGGLE")
            {
                wiggleActive = false;
                if (wiggleRoutine != null)
                {
                    StopCoroutine(wiggleRoutine);
                    wiggleRoutine = null;
                }
            }
            
            else if (trimmedTag.StartsWith("MUSIC_"))
            {
                string name = trimmedTag.Substring("MUSIC_".Length);

                if (name.Equals("STOP", System.StringComparison.OrdinalIgnoreCase))
                {
                    musicSource.Stop();
                    Debug.Log("[MUSIC] Stop");
                }
                else
                {
                    var clipData = musicLibrary.FirstOrDefault(m =>
                        m.key.Equals(name, System.StringComparison.OrdinalIgnoreCase));

                    if (clipData != null && clipData.clip != null)
                    {
                        musicSource.clip = clipData.clip;
                        musicSource.loop = true;
                        musicSource.Play();
                        Debug.Log($"[MUSIC] Playing {name}");
                    }
                    else
                    {
                        Debug.LogWarning($"[MUSIC] '{name}' not found in music library");
                    }
                }
            }
            else if (trimmedTag.StartsWith("SFX_"))
            {
                // Example tags:
                // #SFX_DOOR
                // #SFX_DOOR_ECHO
                // #SFX_AUDIENCE_CHEER_REVERB
                string[] parts = trimmedTag.Split('_');
                string clipKey = parts.Length > 1 ? parts[1] : "";
                string presetKey = parts.Length > 2 ? parts[2] : "";

                var clipData = sfxLibrary.FirstOrDefault(s =>
                    s.key.Equals(clipKey, System.StringComparison.OrdinalIgnoreCase));
                if (clipData == null || clipData.clip == null)
                {
                    Debug.LogWarning($"[SFX] '{clipKey}' not found.");
                    return;
                }

                // Route based on category in the tag
                AudioSource src = sfxSource;
                //if (trimmedTag.Contains("SOURCE")) src = sfxSource;
                //else
                if (trimmedTag.Contains("BUTTON")) src = sfxButtonSource;

                // Disable filters first
                DisableAllFilters(src);

                // Optional preset
                if (!string.IsNullOrEmpty(presetKey))
                {
                    var preset = sfxPresets.FirstOrDefault(p =>
                        p.presetName.Equals(presetKey, System.StringComparison.OrdinalIgnoreCase));
                    if (preset != null)
                        ApplyPresetToSource(src, preset);
                    else
                        Debug.LogWarning($"[SFX] Preset '{presetKey}' not found.");
                }

                // Play clip (filters apply automatically if active)
                src.clip = clipData.clip;
                src.Play();

                Debug.Log($"[SFX] {clipKey} (preset: {presetKey}) on {src.name}");
            }
            
            else
            {
                Debug.Log($"Unhandled Ink tag: {tag}");
            }
        }
    }
    
    private void ShowCharacter(string characterName, string slotName)
    {
        // Find correct slot
        CharacterPortrait targetSlot = _characterPortraits.FirstOrDefault(
            p => p.name.EndsWith($"_{slotName}", System.StringComparison.OrdinalIgnoreCase));

        if (targetSlot == null)
        {
            Debug.LogError($"No character slot found matching '{slotName}'");
            return;
        }

        var charEntry = _spriteDatabase.GetCharacterEntry(characterName);
        if (charEntry == null)
        {
            Debug.LogError($"Character '{characterName}' not found in Sprite Database!");
            return;
        }

        // Already active?
        if (_activeCharacters.TryGetValue(characterName, out CharacterPortrait existingPortrait))
        {
            if (existingPortrait == targetSlot)
            {
                Debug.Log($"Character {characterName} already in {slotName}.");
                return;
            }
            else
            {
                // move from old slot
                Debug.Log($"Moving {characterName} from {existingPortrait.name} to {targetSlot.name}.");
                existingPortrait.SetHiddenState(); 
                _activeCharacters.Remove(characterName);
            }
        }

        // Occupy the slot
        _activeCharacters[characterName] = targetSlot;
        targetSlot.Setup(characterName, charEntry.GetExpressionSprite("entrada"));

        // ⚠️ Removed targetSlot.SetIdleState(); (no extra idle forced)

        Debug.Log($"Showing {characterName} in {targetSlot.name}");
    }

    private void HideCharacter(string characterName)
    {
        if (_activeCharacters.TryGetValue(characterName,
                                          out CharacterPortrait portrait))
        {
            portrait.SetHiddenState();
            _activeCharacters.Remove(characterName);
            Debug.Log($"Hiding character: {characterName}");
            if (_currentSpeakerPortrait == portrait)
            {
                _currentSpeakerPortrait = null;
            }
        }
        else
        {
            Debug.LogWarning($"Character '{characterName}' is not currently active.");
        }
    }

    private void HideAllCharacters()
    {
        foreach (var pair in _activeCharacters)
        {
            pair.Value.SetHiddenState();
        }
        _activeCharacters.Clear();
        _currentSpeakerPortrait = null;
        Debug.Log("Hiding all characters.");
    }

    private void SetCharacterExpression(string characterName,
                                       string expressionName)
    {
        if (_activeCharacters.TryGetValue(characterName,
                                          out CharacterPortrait portrait))
        {
            CharacterSpriteDatabase.CharacterEntry charEntry =
                _spriteDatabase.GetCharacterEntry(characterName);
            if (charEntry != null)
            {
                Sprite newSprite = charEntry.GetExpressionSprite(expressionName);
                if (newSprite != null)
                {
                    portrait.SetSprite(newSprite);
                    Debug.Log(
                        $"Set {characterName} expression to {expressionName}");
                }
            }
        }
        else
        {
            Debug.LogWarning(
                $"Character '{characterName}' not active to change expression.");
        }
    }

    private void FadeAllCharactersToIdle()
    {
        foreach (var pair in _activeCharacters)
        {
            pair.Value.SetIdleState();
        }
        _currentSpeakerPortrait = null;
    }
    
    private void ApplyWiggleEffect()
    {
        caseText.ForceMeshUpdate();
        var textInfo = caseText.textInfo;
        int charCount = textInfo.characterCount;

        for (int c = 0; c < charCount; c++)
        {
            if (!textInfo.characterInfo[c].isVisible) continue;

            int vertexIndex = textInfo.characterInfo[c].vertexIndex;
            int matIndex = textInfo.characterInfo[c].materialReferenceIndex;
            Vector3[] vertices = textInfo.meshInfo[matIndex].vertices;

            float offsetX = Mathf.Sin((Time.time + c) * currentWiggleSpeed) * currentWiggleStrength;
            float offsetY = Mathf.Cos((Time.time + c) * currentWiggleSpeed * 0.5f)
                            * currentWiggleStrength * 0.5f;
            Vector3 offset = new Vector3(offsetX, offsetY, 0);

            vertices[vertexIndex + 0] += offset;
            vertices[vertexIndex + 1] += offset;
            vertices[vertexIndex + 2] += offset;
            vertices[vertexIndex + 3] += offset;
        }

        for (int m = 0; m < textInfo.meshInfo.Length; m++)
        {
            var meshInfo = textInfo.meshInfo[m];
            meshInfo.mesh.vertices = meshInfo.vertices;
            caseText.UpdateGeometry(meshInfo.mesh, m);
        }
    }

    
    private IEnumerator ContinuousWiggle()
    {
        while (wiggleActive)
        {
            ApplyWiggleEffect(); // same code that modifies TMP vertices
            yield return null;   // every frame
        }

        wiggleRoutine = null;
    }
    
    private void DisableAllFilters(AudioSource src)
    {
        foreach (var f in src.GetComponents<AudioBehaviour>())
            if (f is AudioEchoFilter || f is AudioReverbFilter || f is AudioLowPassFilter)
                ((Behaviour)f).enabled = false;
    }

    private void ApplyPresetToSource(AudioSource src, SoundEffectPreset preset)
    {
        if (preset == null) return;

        // Each source has its own attached filters
        var echo = src.GetComponent<AudioEchoFilter>();
        var reverb = src.GetComponent<AudioReverbFilter>();
        var low = src.GetComponent<AudioLowPassFilter>();

        if (echo && reverb && low)
        {
            echo.enabled = preset.useEcho;
            if (preset.useEcho)
            {
                echo.delay = preset.echoDelay;
                echo.decayRatio = preset.echoDecay;
            }

            reverb.enabled = preset.useReverb;
            if (preset.useReverb)
                reverb.reverbPreset = preset.reverbType;

            low.enabled = preset.useLowPass;
            if (preset.useLowPass)
                low.cutoffFrequency = preset.cutoffFrequency;
        }
    }
    
}