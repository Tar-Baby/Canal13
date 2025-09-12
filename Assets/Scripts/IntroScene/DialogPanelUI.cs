using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class DialogPanelUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject contentParent;
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject choicesPanel;
    [SerializeField] private Button[] choiceButtons; // Array de 3 botones para las opciones
    [SerializeField] private Button finalButton; // Botón "Comenzar Show"
    [SerializeField] private GameObject namePanel; // panel de nombre de speaker
    [SerializeField] private TextMeshProUGUI speakerName;

    [Header("Custom Name Input UI")]
    [SerializeField] private GameObject customNameInputPanel;
    [SerializeField] private TMP_InputField customNameInput;
    [SerializeField] private Button confirmCustomNameButton;
    [SerializeField] private Button cancelCustomNameButton;
    [SerializeField] private GameObject nameSelectionPanel;
    
    [Header("Settings")]
    [SerializeField] private float typewriterSpeed = 0.05f;
    
    private bool isTyping = false;
    private string currentText = "";
    private DialogManager dialogManager;
    private bool hasShownText = false; // NUEVO: Controlar si ya se mostró texto
    private bool isFinalScreen = false; // NUEVO: Controlar si estamos en la pantalla final

    private void OnEnable()
    {
        DialogEvents.OnDialogStarted += ShowDialog;
        DialogEvents.OnDialogFinished += HideDialog;
        DialogEvents.OnDisplayDialog += DisplayDialogLine;
        DialogEvents.OnShowChoices += ShowChoices;
        DialogEvents.OnHideChoices += HideChoices;
        DialogEvents.OnShowFinalButton += ShowFinalButton;
    }

    private void OnDisable()
    {
        DialogEvents.OnDialogStarted -= ShowDialog;
        DialogEvents.OnDialogFinished -= HideDialog;
        DialogEvents.OnDisplayDialog -= DisplayDialogLine;
        DialogEvents.OnShowChoices -= ShowChoices;
        DialogEvents.OnHideChoices -= HideChoices;
        DialogEvents.OnShowFinalButton -= ShowFinalButton;
    }

    private void Start()
    {
        // Buscar el DialogManager usando la nueva forma recomendada
        dialogManager = Object.FindFirstObjectByType<DialogManager>();
        // Configurar botones
        //SetupButtons();
        SetupButtonListeners();
        
        AutoConnectUI();
        
        // Ocultar elementos inicialmente
        HideAllInteractiveElements();

        
        // Asegurarse de que el panel esté oculto al inicio
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }
        
        if (dialogText != null)
        {
            dialogText.gameObject.SetActive(false);
        }
 
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }
        
        if (contentParent != null)
        {
            contentParent.SetActive(false);
        }
        
        if (namePanel != null)
        {
            namePanel.SetActive(false);
        }
        
        if (customNameInputPanel != null)
        {
            customNameInputPanel.SetActive(false);
        }
        
    }
    

    private void ShowSpeakerName()
    {
        speakerName.text = dialogManager.GetCurrentSpeaker();
    }

    private void SetupButtons()
    {
        // Configurar botón continuar
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }
        
        // Configurar botones de opciones
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (choiceButtons[i] != null)
            {
                int buttonIndex = i; // Capturar el índice para el closure
                choiceButtons[i].onClick.RemoveAllListeners(); // Limpiar listeners anteriores
                choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(buttonIndex));
            }
        }
        
        // Configurar botón final
        if (finalButton != null)
        {
            finalButton.onClick.AddListener(OnFinalButtonClicked);
        }
    }
    
    
    
    [ContextMenu("Auto Connect UI")]
    private void AutoConnectUI()
    {
        Debug.Log("[TVShowSceneManager] Auto connecting UI elements...");
        

        if (customNameInputPanel == null)
        {
            GameObject foundPanel = GameObject.Find("CustomNameInputPanel");
            if (foundPanel != null)
            {
                customNameInputPanel = foundPanel;
                Debug.Log("✅ CustomNameInputPanel found and connected");
            }
        }

        if (customNameInput == null)
        {
            GameObject foundInput = GameObject.Find("CustomNameInput");
            if (foundInput != null)
            {
                customNameInput = foundInput.GetComponent<TMP_InputField>();
                if (customNameInput != null)
                {
                    SetupInputFieldForAndroid();
                    Debug.Log("✅ CustomNameInput found and configured for Android");
                }
            }
        }

        if (confirmCustomNameButton == null)
        {
            confirmCustomNameButton = FindButtonInPanel("Confirm", "Confirmar");
        }

        if (cancelCustomNameButton == null)
        {
            cancelCustomNameButton = FindButtonInPanel("Cancel", "Cancelar");
        }

        if (customNameInputPanel != null)
        {
            if (confirmCustomNameButton == null)
            {
                confirmCustomNameButton = CreateButton("ConfirmButton", "Confirmar", new Vector2(100, -100), Color.green);
            }

            if (cancelCustomNameButton == null)
            {
                cancelCustomNameButton = CreateButton("CancelButton", "Cancelar", new Vector2(-100, -100), Color.red);
            }
        }
        
            Debug.Log("[DialoguePanelUI] Auto UI connection completed");
        
    }
    
    private void SetupInputFieldForAndroid()
    {

        TextMeshProUGUI textComponent = customNameInput.transform.Find("Text Area/Text")?.GetComponent<TextMeshProUGUI>();
        if (textComponent == null)
        {
            textComponent = customNameInput.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (textComponent != null)
        {
            customNameInput.textComponent = textComponent;
            Debug.Log("Text component connected");
        }

        TextMeshProUGUI placeholder = customNameInput.transform.Find("Text Area/Placeholder")?.GetComponent<TextMeshProUGUI>();
        if (placeholder != null)
        {
            customNameInput.placeholder = placeholder;
            placeholder.text = "Escribe el nombre del programa...";
            Debug.Log("Placeholder connected");
        }

        customNameInput.contentType = TMP_InputField.ContentType.Standard;
        customNameInput.inputType = TMP_InputField.InputType.Standard;
        customNameInput.keyboardType = TouchScreenKeyboardType.Default;

        Debug.Log("InputField configured for Android native keyboard");
        
    }

    private Button FindButtonInPanel(params string[] buttonNames)
    {
        if (customNameInputPanel == null) return null;

        foreach (string name in buttonNames)
        {
            Transform found = customNameInputPanel.transform.Find(name);
            if (found != null)
            {
                Button button = found.GetComponent<Button>();
                if (button != null) return button;
            }
        }
        return null;
    }

    private Button CreateButton(string name, string text, Vector2 position, Color color)
    {
        if (customNameInputPanel == null) return null;

        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(customNameInputPanel.transform, false);

        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(120, 40);

        Image image = buttonObj.AddComponent<Image>();
        image.color = color;

        Button button = buttonObj.AddComponent<Button>();

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI textComp = textObj.AddComponent<TextMeshProUGUI>();
        textComp.text = text;
        textComp.color = Color.white;
        textComp.alignment = TextAlignmentOptions.Center;
        textComp.fontSize = 16f;
        
            Debug.Log($"Button '{name}' created");
        

        return button;
    }
    
    private void SetupButtonListeners()
    {
        if (choiceButtons[0] != null)
        {
            choiceButtons[0].onClick.RemoveAllListeners();
            choiceButtons[0].onClick.AddListener(() => OnNameSelected("Caso Piteado", 0));
        }

        if (choiceButtons[1] != null)
        {
            choiceButtons[1].onClick.RemoveAllListeners();
            choiceButtons[1].onClick.AddListener(() => OnNameSelected("El Gran Chongo", 1));
        }

        if (choiceButtons[2] != null)
        {
            choiceButtons[2].onClick.RemoveAllListeners();
            choiceButtons[2].onClick.AddListener(() => OnWriteCustomName());
        }
        

        if (confirmCustomNameButton != null)
        {
            confirmCustomNameButton.onClick.RemoveAllListeners();
            confirmCustomNameButton.onClick.AddListener(() => OnConfirmCustomName());
        }

        if (cancelCustomNameButton != null)
        {
            cancelCustomNameButton.onClick.RemoveAllListeners();
            cancelCustomNameButton.onClick.AddListener(() => OnCancelCustomName());
        }
        
        Debug.Log("Button listeners setup complete");
        
    }
    
    public void ShowNameSelectionInterface()
    {
        Debug.Log("[DialoguePanelUI] ShowNameSelectionInterface called");
        

        HideAllPanels();
        
        if (nameSelectionPanel != null)
        {
            nameSelectionPanel.SetActive(true);
            Debug.Log("[DialoguePanelUI] Name selection panel shown");
            
        }
        else
        {
            Debug.LogError("[DialoguePanelUI] Name selection panel is null!");
        }
    }
    
    private void HideAllPanels()
    {
        if (nameSelectionPanel != null)
            nameSelectionPanel.SetActive(false);
        
        if (customNameInputPanel != null)
            customNameInputPanel.SetActive(false);

        Debug.Log("[DialoguePanelUI] All panels hidden");
        
    }
    
    public void ShowCustomNameInput()
    {
        Debug.Log("[DialogPanelUI] ShowCustomNameInput called");
        
        
        if (customNameInputPanel != null)
        {
            customNameInputPanel.SetActive(true);
            Debug.Log("[DialogPanelUI] Custom name input panel activated");
            
            
            if (customNameInput != null)
            {
                customNameInput.text = "";
                customNameInput.Select();
                customNameInput.ActivateInputField();
                
               Debug.Log("Input field activated - Android keyboard should appear");
                
            }
            else
            {
                Debug.LogError("Custom name input field is null!");
            }
        }
        else
        {
            Debug.LogError("Custom name input panel is null!");
        }
    }

    public void HideCustomNameInput()
    {
       Debug.Log("HideCustomNameInput called");
        

        if (customNameInputPanel != null)
        {
            customNameInputPanel.SetActive(false);
            Debug.Log("Custom name input panel hidden");
            
        }
    }

    
    private void OnNameSelected(string showName, int choiceIndex)
    {
        Debug.Log($"[TVShowSceneManager] Name selected: {showName} (choice {choiceIndex})");
        //HideAllPanels();
        HideDialog();

        if (dialogManager != null)
        {
            dialogManager.MakeChoice(choiceIndex);
        }
        else
        {
            Debug.LogError("[DialoguePaneUI] Dialog manager not found!");
        }
    }

    private void OnWriteCustomName()
    {
        Debug.Log("Write custom name selected");
        if (dialogManager != null)
        {
            dialogManager.MakeChoice(2);
        }
        else
        {
            Debug.Log("Dialog manager not found!");
        }
    }

    private void OnConfirmCustomName()
    {
        Debug.Log("[DialoguePaneUI] Confirm custom name clicked");
        

        if (customNameInput != null)
        {
            string customName = customNameInput.text.Trim();
            
             Debug.Log($"[DialoguePaneUI] Custom name entered: '{customName}'");
            

            if (!string.IsNullOrEmpty(customName))
            {
                if (customName.Length <= 30)
                {
                    HideCustomNameInput();
                    
                    if (dialogManager != null)
                    {
                        dialogManager.OnCustomNameEntered(customName);
                    }
                    else
                    {
                        Debug.LogError("[TVShowSceneManager] Dialog manager not found!");
                    }
                }
                else
                {
                    Debug.LogWarning("[TVShowSceneManager] Name too long (max 30 characters)");
                }
            }
            else
            {
                Debug.LogWarning("[TVShowSceneManager] Empty name entered");
            }
        }
        else
        {
            Debug.LogError("[TVShowSceneManager] Custom name input field is null!");
        }
    }
    

    private void OnCancelCustomName()
    {
            Debug.Log("Cancel custom name clicked");
        

        HideCustomNameInput();
        
        // Mostrar de nuevo la selección de nombres
        if (choicesPanel != null)
        {
            choicesPanel.SetActive(false);
        }

        if (dialogManager != null)
        {
            dialogManager.OnCustomNameCancelled();
        }
        else
        {
            Debug.LogError("[TVShowSceneManager] Dialog manager not found!");
        }
    }

    private void Update()
    {
        // Permitir interacción si el panel de diálogo está activo
        if (dialogPanel != null && dialogPanel.activeInHierarchy && dialogManager != null)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                if (isTyping)
                {
                    // Si está escribiendo, completar el texto inmediatamente
                    CompleteTypewriter();
                }
                else if (!dialogManager.IsWaitingForChoice())
                {
                    // Si no está escribiendo y no esperamos elección, continuar al siguiente diálogo
                    OnContinueClicked();
                }
            }
        }
    }

    private void ShowDialog()
    {
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(true);
        }
        
        if (dialogText != null)
        {
            dialogText.gameObject.SetActive(true);
        }
        
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
        }
        
        if (contentParent != null)
        {
            contentParent.SetActive(true);
        }
        
        if (namePanel != null)
        {
            namePanel.SetActive(true);
        }
        

        
        hasShownText = false; // Reset cuando se inicia el diálogo
        HideAllInteractiveElements();
        
        Debug.Log("Dialog UI shown");
    isFinalScreen = false; // Reset al iniciar diálogo
    }

    private void HideDialog()
    {
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }
        
        if (contentParent != null)
        {
            contentParent.SetActive(false);
        }
        
        
        hasShownText = false; // Reset cuando se oculta
        
        Debug.Log("Dialog UI hidden");
    }

    public void DisplayDialogLine(string dialogLine)
    {
        currentText = dialogLine;
        hasShownText = false; // Reset antes de mostrar nuevo texto

        ShowSpeakerName();
        Debug.Log(dialogManager.GetCurrentSpeaker()); //tener el nombre del speaker

        
        // Ocultar solo elementos que no sean el botón continuar
        if (choicesPanel != null)
        {
            choicesPanel.SetActive(false);
        }
        if (finalButton != null)
        {
            finalButton.gameObject.SetActive(false);
        }
        
        // Asegurar que el botón continuar esté visible
        ShowContinueButton();
        
        // Limpiar el texto anterior
        if (dialogText != null)
        {
            dialogText.text = "";
        }
        
        // Iniciar efecto de máquina de escribir
        StartCoroutine(TypewriterEffect());
    }

    private IEnumerator TypewriterEffect()
    {
        isTyping = true;
        hasShownText = false; // Reset al comenzar a escribir
        
        for (int i = 0; i <= currentText.Length; i++)
        {
            if (dialogText != null)
            {
                dialogText.text = currentText.Substring(0, i);
            }
            
            yield return new WaitForSeconds(typewriterSpeed);
        }

        
        isTyping = false;
        hasShownText = true; // Marcar que ya se mostró el texto completo
        
        // Mostrar el botón continuar siempre que el texto esté completo
        ShowContinueButton();
    }

    private void CompleteTypewriter()
    {
        // Detener la corrutina del typewriter
        StopAllCoroutines();
        
        // Mostrar todo el texto inmediatamente
        if (dialogText != null)
        {
            dialogText.text = currentText;
        }
        
        isTyping = false;
        hasShownText = true; // Marcar que ya se mostró el texto
    }

    private void ShowChoices(List<string> choices)
    {
        // CORREGIDO: Ocultar botón continuar antes de mostrar opciones
        HideAllInteractiveElements();
        
        // Pequeño retraso para asegurar que el texto se vea antes de mostrar opciones
        StartCoroutine(ShowChoicesDelayed(choices, 0.1f));
    }

    private IEnumerator ShowChoicesDelayed(List<string> choices, float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log($"[ShowChoicesDelayed] choices.Count={choices.Count}, choiceButtons.Length={choiceButtons.Length}");
        // Mostrar panel de opciones
        if (choicesPanel != null)
        {
            choicesPanel.SetActive(true);
        }

        int numChoices = Mathf.Min(choices.Count, choiceButtons.Length);
        // Configurar solo los botones necesarios
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (choiceButtons[i] != null)
            {
                if (i < numChoices)
                {
                    Debug.Log($"[ShowChoicesDelayed] Activando botón {i} con texto: {choices[i]}");
                    choiceButtons[i].gameObject.SetActive(true);
                    // Obtener el texto del botón
                    TextMeshProUGUI buttonText = choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = choices[i];
                    }
                }
                else
                {
                    Debug.Log($"[ShowChoicesDelayed] Ocultando botón {i}");
                    choiceButtons[i].gameObject.SetActive(false);
                }
            }
        }
    }

    private void HideChoices()
    {
        if (choicesPanel != null)
        {
            choicesPanel.SetActive(false);
        }
        
        // Ocultar todos los botones de opción
        foreach (Button button in choiceButtons)
        {
            if (button != null)
            {
                button.gameObject.SetActive(false);
            }
        }
    }

    private void ShowContinueButton()
    {
        // Mostrar el botón siempre, excepto en la pantalla final
        if (continueButton != null && !isFinalScreen)
        {
            continueButton.gameObject.SetActive(true);
        }
    }

    private void ShowFinalButton()
    {
        // Ocultar todo menos el botón Comenzar
        isFinalScreen = true;
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(true); // Asegura que el panel de diálogo esté visible
        }
        if (choicesPanel != null)
        {
            choicesPanel.SetActive(false);
        }
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }
        if (finalButton != null)
        {
            finalButton.gameObject.SetActive(true);
        }
    }

    private void HideAllInteractiveElements()
    {
        // Ocultar opciones
        HideChoices();
        // Ocultar botón final
        if (finalButton != null)
        {
            finalButton.gameObject.SetActive(false);
        }
        // No ocultamos el botón continuar aquí
        
        isFinalScreen = false; // Reset bandera al ocultar todo
        
        // Asegurar que el botón continuar esté visible si no es pantalla final
        ShowContinueButton();
    }

    #region Button Events

    private void OnContinueClicked()
    {
        if (dialogManager != null && !dialogManager.IsWaitingForChoice() && !dialogManager.IsWaitingForCustomInput())
        {
            hasShownText = false; // Reset para la siguiente línea
            dialogManager.ContinueDialog();
        }
    }

    private void OnChoiceSelected(int choiceIndex)
    {
        Debug.Log($"[DialogPanelUI] OnChoiceSelected: Button {choiceIndex} clicked!");
        Debug.Log($"  DialogManager exists: {dialogManager != null}");
        if (dialogManager != null)
        {
            Debug.Log($"  IsWaitingForChoice: {dialogManager.IsWaitingForChoice()}");
            Debug.Log($"  IsWaitingForCustomInput: {dialogManager.IsWaitingForCustomInput()}");
        }
        
        if (dialogManager != null && dialogManager.IsWaitingForChoice())
        {
            hasShownText = false; // Reset para la siguiente línea
            dialogManager.MakeChoice(choiceIndex);
        }
        
        else
        {
                Debug.LogError($"[DialogPanelUI] Cannot make choice - Manager: {dialogManager!= null}, Waiting: {dialogManager?.IsWaitingForChoice()}");
        }
    }

    private void OnFinalButtonClicked()
    {
        if (dialogManager != null)
        {
            dialogManager.StartDemoShow();
        }
    }

    #endregion

    #region Public Methods

    public bool IsTyping()
    {
        return isTyping;
    }

    public void SetTypewriterSpeed(float speed)
    {
        typewriterSpeed = speed;
    }

    public bool HasShownText()
    {
        return hasShownText;
    }

    #endregion

}
