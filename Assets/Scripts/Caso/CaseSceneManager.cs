using UnityEngine;
using UnityEngine.UI;
using TMPro; // Necesario para TextMeshProUGUI
using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;
using UnityEngine.SceneManagement; // Necesario para cargar escenas (al final del caso)
using System.Linq; // For LINQ operations like .FirstOrDefault

public class CaseSceneManager : MonoBehaviour
{
    [Header("Case UI References")]
    [SerializeField] private GameObject casePanel; // Referencia al GameObject "CaseDialoguePanelUI"
    [SerializeField] private TextMeshProUGUI caseText; // Referencia al TextMeshProUGUI "CaseText" (o DialogueText)
    [SerializeField] private GameObject namePanel; // panel de nombre de speaker
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private Button caseContinueButton; // Referencia al Button "ContinueButton"
    [SerializeField] private GameObject caseChoicesPanel; // Referencia al GameObject "CaseChoicesPanel" (o ChoicesPanel)
    [SerializeField] private Button[] caseChoiceButtons = new Button[4]; // Array de referencias a Button "ChoiceButton1", etc.
    [SerializeField] private Button caseFinalButton; // Referencia al Button "FinalButton" (si tu caso termina con un botón final)

    // Ya NO necesitamos una referencia para el icono de menú aquí,
    // porque estará SIEMPRE visible y no será controlado por este script para su visibilidad.
    // [SerializeField] private GameObject menuIconButton; // <--- ELIMINADO/COMENTADO

    [Header("Case INK")]
    [SerializeField] private TextAsset caseInkScript; // El archivo .json de Ink para la lógica de este caso (ej. main_show.json)

    [Header("Settings")]
    [SerializeField] private float typewriterSpeed = 0.05f; // Velocidad del efecto de máquina de escribir
    [SerializeField] private bool debugMode = true; // Modo depuración para logs adicionales
    
    [Header("Character Sprites & Portraits")]
    [SerializeField]
    private CharacterSpriteDatabase _spriteDatabase;
    [SerializeField]
    private List<CharacterPortrait> _characterPortraits; // All character slots

    // Internal tracking of which character is in which slot
    private Dictionary<string, CharacterPortrait> _activeCharacters =
        new Dictionary<string, CharacterPortrait>();
    private CharacterPortrait _currentSpeakerPortrait;

    // A queue for tags if you want to process them in a specific order
    // private Queue<string> _tagQueue = new Queue<string>();

    [Header("Show Data")]
    private string currentShowName = ""; // Nombre del show, recibido de la escena anterior (o por defecto para prueba)
    private int currentEpisodeRating = 0; // Rating actual, recibido de la escena anterior (o por defecto) 

    private bool isTyping = false; // Indica si el texto se está escribiendo
    private string currentText = ""; // Contenido del texto actual a mostrar
    private bool caseStarted = false; // Indica si el flujo del caso ha comenzado
    private bool waitingForChoice = false; // Indica si el sistema está esperando una elección del usuario

    private Story caseStory; // La historia de Ink para este caso
    private Dictionary<string, object> inkVariables = new Dictionary<string, object>();


    private void Start()
    {
        SetupCaseUI(); // Configura la UI del caso (oculta elementos al inicio)
        AutoConnectReferences(); // Intenta autoconectar las referencias si no están asignadas manualmente
        
        // --- INICIO DE CÓDIGO PARA AUTO-INICIO DE PRUEBA (SOLO PARA DESARROLLO EN EL EDITOR) ---
        // Este bloque se ejecuta si estás en el editor de Unity y ejecutas la escena 'Caso' directamente.
        // Proporciona valores por defecto y llama a StartCase() para iniciar la depuración.
        if (!caseStarted) 
        {
            Debug.Log("🎉 [CaseSceneManager] Auto-iniciando CaseSceneManager para PRUEBA DIRECTA de escena 'Caso'.");
            
            //if (caseStory.variablesState.GlobalVariableExistsWithName("show_name"))   //idea para acceder a variable global de ink
            //{
           //     inkVariables["show_name"] = caseStory.variablesState["show_name"];
           //     currentShowName = inkVariables["show_name"].ToString();
            //}
            //else currentShowName = "Caso de Prueba";
            
            
            // Asigna valores por defecto para showName y currentEpisodeRating para esta prueba directa.
            currentShowName = "Caso de Prueba";
            currentEpisodeRating = 10;
            StartCase(); // Inicia el caso automáticamente para depuración.
             // empezamos con 10 los Casos
        }
#if UNITY_EDITOR 
        
        
        
        
#endif
        
        
        #if UNITY_ANDROID
        
        
        #endif
        // --- FIN DE CÓDIGO PARA AUTO-INICIO DE PRUEBA ---
    }

    private void AutoConnectReferences()
    {
        // Intenta encontrar los GameObjects por su nombre si no han sido asignados en el Inspector.
        // Asegúrate de que estos nombres coincidan EXACTAMENTE con los de tu jerarquía en la escena "Caso".
        if (casePanel == null) casePanel = GameObject.Find("CaseDialoguePanelUI");
        if (caseText == null) caseText = GameObject.Find("CaseText")?.GetComponent<TextMeshProUGUI>(); // Asegúrate que el nombre es "CaseText" o "DialogueText"
        if (caseContinueButton == null) caseContinueButton = GameObject.Find("ContinueButton")?.GetComponent<Button>();
        if (caseChoicesPanel == null) caseChoicesPanel = GameObject.Find("CaseChoicesPanel"); // Asegúrate que el nombre es "CaseChoicesPanel" o "ChoicesPanel"
        if (caseFinalButton == null) caseFinalButton = GameObject.Find("FinalButton")?.GetComponent<Button>();

        // Autoconectar los botones de elección (ChoiceButton1, ChoiceButton2, etc.)
        for (int i = 0; i < caseChoiceButtons.Length; i++)
        {
            if (caseChoiceButtons[i] == null)
            {
                GameObject buttonObj = GameObject.Find($"ChoiceButton{i + 1}");
                if (buttonObj != null)
                {
                    caseChoiceButtons[i] = buttonObj.GetComponent<Button>();
                }
            }
        }

        // Ya NO necesitamos auto-conectar el icono de menú, ya que estará siempre visible.
        // if (menuIconButton == null) menuIconButton = GameObject.Find("MenuIcon"); // <--- ELIMINADO/COMENTADO

        if (debugMode)
        {
            Debug.Log($"[CaseSceneManager] Auto-connect complete in Case Scene. Panel: {casePanel != null}, Text: {caseText != null}");
        }
    }
    
    public object GetInkVariable(string variableName)
    {
        if (caseStory != null && caseStory.variablesState != null)
        {
            try
            {
                return caseStory.variablesState[variableName];
            }
            catch
            {
                // Si falla, usar la copia local
            }
        }

        return inkVariables.ContainsKey(variableName) ? inkVariables[variableName] : null;
    }

    private void SetupCaseUI()
    {
        // Oculta el panel principal del caso al inicio
        if (casePanel != null) casePanel.SetActive(false);

        // Configura los listeners y oculta los botones de elección
        for (int i = 0; i < caseChoiceButtons.Length; i++)
        {
            if (caseChoiceButtons[i] != null)
            {
                int buttonIndex = i;
                caseChoiceButtons[i].onClick.RemoveAllListeners();
                caseChoiceButtons[i].onClick.AddListener(() => OnCaseChoiceSelected(buttonIndex));
                caseChoiceButtons[i].gameObject.SetActive(false);
            }
        }

        // Oculta los paneles de opciones y botones específicos del caso al inicio
        if (caseChoicesPanel != null) caseChoicesPanel.SetActive(false);
        if (caseContinueButton != null) caseContinueButton.gameObject.SetActive(false); // Oculta el botón de continuar
        if (caseFinalButton != null) caseFinalButton.gameObject.SetActive(false); // Oculta el botón final

        // El script PublicReactionUI.cs en su propio Start() se encarga de ocultarse.
        // No necesitamos hacer GameObject.Find() y setearlo a false aquí, su propio script lo gestiona.
        
        // Ya NO necesitamos ocultar el icono de menú, ya que estará SIEMPRE visible.
        // if (menuIconButton != null) menuIconButton.SetActive(false); // <--- ELIMINADO/COMENTADO

        if (debugMode) Debug.Log("[CaseSceneManager] Case UI setup complete.");
    }

    // Este método es llamado por el DialogManager de la escena "Intro"
    // para pasar los datos iniciales del show a esta escena "Caso".
    public void SetShowData(string showName, int episodeRating)
    {
        currentShowName = showName;
        currentEpisodeRating = episodeRating;
        if (debugMode) Debug.Log($"[CaseSceneManager] Show data received: Name='{showName}', Rating: {episodeRating}");
        
        // Una vez que se reciben los datos, iniciar el caso
        // Esto es para asegurar que el caso no empiece sin los datos de la escena Intro.
        // Solo llamarlo si no ha sido iniciado ya (por ejemplo, por el auto-inicio de prueba para depuración).
        if (!caseStarted)
        {
            StartCase();
        }
    }

    // Inicializa la historia de Ink para el caso
    private void InitializeCaseInk()
    {
        Debug.Log("🔍 [CaseSceneManager] InitializeCaseInk() started...");
        if (caseInkScript == null)
        {
            Debug.LogError("❌ [CaseSceneManager] Case INK script not assigned! Please assign main_show.json.");
            return;
        }
        Debug.Log($"✅ [CaseSceneManager] INK script found: {caseInkScript.name}");

        try
        {
            caseStory = new Story(caseInkScript.text);
            Debug.Log($"✅ [CaseSceneManager] Story created successfully.");

            // Establecer variables globales de Ink si existen, usando los datos recibidos (currentShowName, currentEpisodeRating).
            if (caseStory.variablesState != null)
            {
                if (caseStory.variablesState.GlobalVariableExistsWithName("show_name"))
                {
                    caseStory.variablesState["show_name"] = currentShowName;
                }
                if (caseStory.variablesState.GlobalVariableExistsWithName("episode_rating"))
                {
                    caseStory.variablesState["episode_rating"] = currentEpisodeRating;
                }
            }
            Debug.Log("🎉 [CaseSceneManager] InitializeCaseInk() completed successfully!");
        }
        catch (System.Exception e) { Debug.LogError($"❌ Error initializing story: {e.Message}"); }
    }

    // Inicia el flujo del caso. Este es el punto de entrada principal.
    public void StartCase()
    {
        Debug.Log("[CaseSceneManager] StartCase() called in Case Scene.");
        
        // Verifica que todas las referencias UI esenciales estén asignadas.
        if (casePanel == null || caseText == null || caseContinueButton == null || caseChoicesPanel == null)
        {
            Debug.LogError("Essential UI references are null! Cannot start case. Check Inspector assignments.");
            return;
        }
        
        caseStarted = true; // El caso ha comenzado
        if (caseStory == null) InitializeCaseInk(); // Inicializa la historia Ink si no lo está.
        if (caseStory == null) { Debug.LogError("Case story is still null! Cannot proceed."); return; }

        // Activa el panel principal de la UI del caso y el PublicReactionUI en esta escena.
        casePanel.SetActive(true); // Activa el GameObject "CaseDialoguePanelUI"
        ActivatePublicReactionUI(); // Activa el PublicReactionUI.
        foreach (var portrait in _characterPortraits)
        {
            portrait.Clear();
        }
        _activeCharacters.Clear();

        // Asegúrate de que las opciones y el botón final estén ocultos al inicio.
        caseChoicesPanel.SetActive(false);
        if (caseFinalButton != null) caseFinalButton.gameObject.SetActive(false);

        Debug.Log("[CaseSceneManager] Case UI configured, starting INK dialog.");
        ContinueCaseStory(); // Comienza a reproducir la historia de Ink del caso.
    }

    // Este método ya no es necesario aquí. Fue diseñado para ocultar UI de la escena Intro.
    // La escena Intro se descarga cuando se carga la escena Caso.
    // private void HideMainDialogUI() { ... } 

    // Activa el PublicReactionUI en esta escena y lo inicializa.
    private void ActivatePublicReactionUI()
    {
        GameObject publicReactionUI = GameObject.Find("PublicReactionUI"); 
        if (publicReactionUI != null)
        {
            publicReactionUI.SetActive(true);
            DialogEvents.UpdateEpisodeRating(currentEpisodeRating, 0); 
            Debug.Log("[CaseSceneManager] PublicReactionUI activated and initialized in Case Scene.");
        }
        else
        {
            Debug.LogWarning("[CaseSceneManager] PublicReactionUI not found in THIS ('Caso') scene! Check hierarchy.");
        }
    }

    // Continúa la historia del caso usando Ink.
    private void ContinueCaseStory()
    {
        if (caseStory == null) { Debug.LogError("[CaseSceneManager] Case story is null! Cannot continue."); return; }

        if (caseStory.canContinue)
        {
            string currentLine = caseStory.Continue().Trim();
            
            // --- Process tags BEFORE text ---
            if (caseStory.currentTags != null && caseStory.currentTags.Count > 0)
            {
                ProcessTags(caseStory.currentTags);
            }
            
            CheckEpisodeRatingChanges();

            if (!string.IsNullOrEmpty(currentLine))
            {
                // Split into "SPEAKER: Dialogue"
                string speakerName = "";
                string dialogueContent = currentLine;

                if (currentLine.Contains(":"))
                {
                    string[] parts = currentLine.Split(new char[] { ':' }, 2);
                    speakerName = parts[0].Trim();
                    dialogueContent = parts[1].Trim();
                }

                // --- Set speaker name text ---
                if (speakerNameText != null)
                {
                    speakerNameText.text = speakerName;
                }

                // --- Animate current speaker ---
                if (speakerNameText != null) speakerNameText.text = speakerName;
                
                //  Update portraits immediately (fade/scale starts here)
                UpdateSpeakerAnimation(speakerName);

                // --- Show dialogue text ---
                ShowCaseText(dialogueContent);  // texto con nombres recortados

                //ShowCaseText(currentLine);  // texto completo, con nombres
                if (caseStory.currentChoices.Count > 0) StartCoroutine(ShowChoicesAfterText());
            }
            else
            {
                ContinueCaseStory(); // skip blank lines
                
            }
        }
    }

    // Coroutine para mostrar las opciones después del efecto de máquina de escribir.
    private IEnumerator ShowChoicesAfterText()
    {
        yield return new WaitUntil(() => !isTyping);
        yield return new WaitForSeconds(0.5f);
        ShowCaseChoicesFromStory();
    }

    // Muestra las opciones de diálogo obtenidas de la historia de Ink.
    private void ShowCaseChoicesFromStory()
    {
        if (caseStory.currentChoices.Count > 0)
        {
            List<string> choices = new List<string>();
            foreach (var choice in caseStory.currentChoices) choices.Add(choice.text);
            ShowCaseChoices(choices); 
        }
    }

    // Verifica si 'episode_rating' en Ink cambió y actualiza PublicReactionUI.
    private void CheckEpisodeRatingChanges()
    {
        if (caseStory != null && caseStory.variablesState != null)
        {
            try
            {
                if (caseStory.variablesState.GlobalVariableExistsWithName("episode_rating"))
                {
                    int newRating = (int)caseStory.variablesState["episode_rating"];
                    if (newRating != currentEpisodeRating)
                    {
                        int difference = newRating - currentEpisodeRating;
                        currentEpisodeRating = newRating;
                        DialogEvents.UpdateEpisodeRating(currentEpisodeRating, difference);
                        if (debugMode) Debug.Log($"📊 Episode rating changed: {currentEpisodeRating} (Difference: {difference})");
                    }
                }
            } catch (System.Exception e) { Debug.LogWarning($"⚠️ Error checking episode_rating variable: {e.Message}"); }
        }
    }

    // Muestra el texto del diálogo con efecto de máquina de escribir.
    public void ShowCaseText(string text)
    {
        if (!caseStarted) return;
        currentText = text;
        if (caseText != null) { caseText.text = currentText; StartCoroutine(TypewriterEffect()); } // <-- ¡CORRECCIÓN! caseText.text = currentText
        if (debugMode) Debug.Log($"[CaseSceneManager] Showing case text: {text}");
    }

    // Coroutine para el efecto de máquina de escribir.
    private IEnumerator TypewriterEffect()
    {
        isTyping = true;
        for (int i = 0; i <= currentText.Length; i++)
        {
            if (caseText != null) caseText.text = currentText.Substring(0, i);
            yield return new WaitForSeconds(typewriterSpeed);
        }
        isTyping = false;
    }

    // Muestra los botones de opciones del caso.
    public void ShowCaseChoices(List<string> choices)
    {
        if (debugMode) Debug.Log($"[CaseSceneManager] Showing {choices.Count} case choices.");
        waitingForChoice = true;
        if (caseChoicesPanel != null) caseChoicesPanel.SetActive(true);

        for (int i = 0; i < caseChoiceButtons.Length; i++)
        {
            if (caseChoiceButtons[i] != null)
            {
                if (i < choices.Count)
                {
                    caseChoiceButtons[i].gameObject.SetActive(true);
                    TextMeshProUGUI buttonText = caseChoiceButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null) { buttonText.text = choices[i]; buttonText.color = Color.white; }
                    if (debugMode) Debug.Log($"  Case Choice {i}: {choices[i]}");
                }
                else { caseChoiceButtons[i].gameObject.SetActive(false); }
            }
        }
        // Oculta el botón de continuar cuando aparecen las opciones.
        if (caseContinueButton != null) caseContinueButton.gameObject.SetActive(false);
    }

    // Oculta los botones de opciones del caso.
    public void HideCaseChoices()
    {
        if (debugMode) Debug.Log("[CaseSceneManager] Hiding case choices.");
        waitingForChoice = false;
        if (caseChoicesPanel != null) caseChoicesPanel.SetActive(false);
        foreach (Button button in caseChoiceButtons) { if (button != null) button.gameObject.SetActive(false); }
        // Vuelve a mostrar el botón de continuar cuando las opciones se ocultan.
        // if (caseContinueButton != null) caseContinueButton.gameObject.SetActive(true);
    }

    // Se llama cuando se selecciona una opción del caso.
    private void OnCaseChoiceSelected(int choiceIndex)
    {
        if (!waitingForChoice) return;
        if (debugMode) Debug.Log($"[CaseSceneManager] Case choice {choiceIndex} selected.");

        if (caseStory != null && choiceIndex >= 0 && choiceIndex < caseStory.currentChoices.Count)
        {
            caseStory.ChooseChoiceIndex(choiceIndex);
            HideCaseChoices();
            ContinueCaseStory();
        } else { Debug.LogWarning($"Invalid choice index selected: {choiceIndex}"); }
    }

    public bool IsCaseActive() { return caseStarted && casePanel != null && casePanel.activeInHierarchy; }

    // Termina el flujo del caso.
    public void EndCase()
    {
        if (debugMode) Debug.Log("[CaseSceneManager] Ending case.");
        caseStarted = false;
        waitingForChoice = false;
        ShowCaseText($"¡Gracias por acompañarnos en {currentShowName}!\nRating final: {currentEpisodeRating}");

        // Oculta PublicReactionUI al final del caso en esta escena.
        GameObject publicReactionUI = GameObject.Find("PublicReactionUI");
        if (publicReactionUI != null) { publicReactionUI.SetActive(false); }

        // Oculta el panel principal de la UI del diálogo del caso.
        // Esto dejará la escena 'Caso' activa, pero sin la UI del diálogo, solo el fondo y elementos siempre visibles.
        if (casePanel != null) casePanel.SetActive(false);

        // Ya NO necesitamos activar el icono de menú aquí, porque estará SIEMPRE visible.
        // if (menuIconButton != null) { menuIconButton.SetActive(true); Debug.Log("[CaseSceneManager] Icono de menú activado."); }

        // COMENTADO: Ya no cargamos una nueva escena automáticamente al final del caso.
        // Esto permite que el juego permanezca en la escena "Caso" y espere la interacción del usuario
        // con un menú o botón persistente que tú manejes.
        // StartCoroutine(LoadNextSceneAfterCase("MainMenuScene")); 
    }

    // Este método ya no se utiliza porque EndCase() ya no llama a LoadNextSceneAfterCase().
    // Puedes comentar o eliminar este método si ya no lo necesitas en tu lógica.
    // private IEnumerator LoadNextSceneAfterCase(string nextSceneName)
    // {
    //     yield return new WaitForSeconds(3f);
    //     if (casePanel != null) casePanel.SetActive(false);
    //     Debug.Log($"⏳ Loading next scene: {nextSceneName} after case completion.");
    //     SceneManager.LoadScene(nextSceneName);
    // }

    // Método público para continuar el diálogo del caso (para clicks o teclas).
    public void ContinueCaseDialog()
    {
        if (caseStarted && !isTyping && !waitingForChoice)
        {
            ContinueCaseStory();
        }
    }

    #region CharacterPortraits

    private void RefreshUI()
    {
        string text = caseStory.Continue();

        string speakerName = "";
        string dialogueContent = text;

        if (text.Contains(":"))
        {
            string[] parts = text.Split(new char[] { ':' }, 2);
            speakerName = parts[0].Trim();
            dialogueContent = parts[1].Trim();
        }

        //caseText.text = dialogueContent;
        speakerNameText.text = speakerName;

        // Animate speakers
        UpdateSpeakerAnimation(speakerName);
        
    }

    private void UpdateSpeakerAnimation(string currentSpeaker)
    {
        Debug.Log($"[UpdateSpeakerAnimation] Speaker='{currentSpeaker}'");

        if (string.IsNullOrEmpty(currentSpeaker))
        {
            // No speaker → narration → everyone idle
            foreach (var kvp in _activeCharacters)
                kvp.Value.SetIdleState();

            _currentSpeakerPortrait = null;
            return;
        }

        foreach (var kvp in _activeCharacters)
        {
            // normalize both sides for safe compare
            if (kvp.Key.ToUpper() == currentSpeaker.ToUpper())
            {
                kvp.Value.SetTalkingState();   // active speaker pops
                _currentSpeakerPortrait = kvp.Value;
            }
            else
            {
                kvp.Value.SetIdleState();      // others idle
            }
        }
        

        // Highlight current speaker
        if (!string.IsNullOrEmpty(currentSpeaker) &&
            _activeCharacters.TryGetValue(currentSpeaker, out CharacterPortrait speakerPortrait))
        {
            speakerPortrait.SetTalkingState();
        }
        else
        {
            // If narration (no speaker), everyone goes idle
            foreach (var kvp in _activeCharacters)
            {
                kvp.Value.SetIdleState();
            }
        }
    }

    private void ProcessTags(List<string> currentTags)
    {
        foreach (string tag in currentTags)
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
            else
            {
                Debug.Log($"Unhandled Ink tag: {tag}");
            }
        }
    }

    private void ShowCharacter(string characterName, string slotName)
    {
        // Find an available slot with the matching name (case-insensitive)
        CharacterPortrait targetSlot = _characterPortraits.FirstOrDefault(
            p =>
                p.name.EndsWith($"_{slotName}",
                                 System.StringComparison.OrdinalIgnoreCase));

        if (targetSlot == null)
        {
            Debug.LogError($"No character slot found matching '{slotName}'");
            return;
        }

        CharacterSpriteDatabase.CharacterEntry charEntry =
            _spriteDatabase.GetCharacterEntry(characterName);
        if (charEntry == null)
        {
            Debug.LogError(
                $"Character '{characterName}' not found in Sprite Database!");
            return;
        }

        // Check if the character is already in this slot or another
        if (_activeCharacters.TryGetValue(characterName, out CharacterPortrait existingPortrait))
        {
            if (existingPortrait == targetSlot)
            {
                // Already in the correct slot, just update expression
                Debug.Log($"Character {characterName} already in {slotName}.");
                return;
            }
            else
            {
                // Character is in a different slot, move them (hide old, show new)
                Debug.Log($"Moving {characterName} from {existingPortrait.name} to {targetSlot.name}.");
                existingPortrait.SetHiddenState(); // Fade out from old slot
                _activeCharacters.Remove(characterName);
            }
        }

        // Occupy the slot
        _activeCharacters[characterName] = targetSlot;
        targetSlot.Setup(characterName, charEntry.GetExpressionSprite("default"));
        targetSlot.SetIdleState(); // Make it appear as idle
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

    #endregion

    // Maneja la entrada del usuario para avanzar el diálogo o completar el typewriter.
    private void Update()
    {
        if (caseStarted && casePanel != null && casePanel.activeInHierarchy)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                if (isTyping)
                {
                    StopAllCoroutines();
                    // ASIGNACIÓN CORRECTA: Asigna el string 'currentText' a la propiedad '.text' de 'caseText'.
                    if (caseText != null) caseText.text = currentText; 
                    isTyping = false;
                }
                else if (!waitingForChoice)
                {
                    ContinueCaseDialog();
                }
            }
        }
    }
}