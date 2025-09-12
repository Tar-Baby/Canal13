using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class TVShowScene
{
    [Header("Scene Identity")]
    public string sceneName;
    public Sprite backgroundSprite;
    
    [Header("Visual Settings")]
    public Color lightingTint = Color.white;
    public Vector3 cameraPosition = new Vector3(0, 0, -10);
    public float cameraSize = 5f;
    
    [Header("Audio")]
    public AudioClip ambientSound;
    public bool hasSpecialLighting = false;
}

public class TVShowSceneManager : MonoBehaviour
{
    [Header("Scene Configuration")]
    [SerializeField] private TVShowScene[] availableScenes;
    [SerializeField] private int currentSceneIndex = 0;
    
    [Header("UI References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private AudioSource ambientAudioSource;
    [SerializeField] private Button[] choiceButtons; // Array de 3 botones para las opciones

    
    [Header("Name Selection UI")]
    [SerializeField] private GameObject nameSelectionPanel;
    [SerializeField] private Button casoPiteadoButton;
    [SerializeField] private Button elGranChongoButton;
    [SerializeField] private Button escribirNombreButton;
    [SerializeField] private Button noDecidirButton;
    
    [Header("Custom Name Input UI")]
    [SerializeField] private GameObject customNameInputPanel;
    [SerializeField] private TMP_InputField customNameInput;
    [SerializeField] private Button confirmCustomNameButton;
    [SerializeField] private Button cancelCustomNameButton;
    
    [Header("Final Scene UI")]
    [SerializeField] private GameObject finalPanel;
    [SerializeField] private TextMeshProUGUI showNameDisplay;
    [SerializeField] private Button continuarButton;
    
    [Header("Settings")]
    [SerializeField] private float transitionDuration; //= 1f;
    [SerializeField] private bool debugMode = true;
    
    //private DialogManager dialogManager;
    private DialogManagerINPUT dialogManagerINPUT;
    private bool isTransitioning = false;

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeComponents();
        CreateDefaultScenesIfNeeded();
    }

    private void Start()
    {
        //dialogManager = FindFirstObjectByType<DialogManager>();
        dialogManagerINPUT = FindFirstObjectByType<DialogManagerINPUT>();
        SetupButtonListeners();
        AutoConnectUI();
        SetupInitialScene();
    }

    #endregion

    #region Initialization

    private void CreateDefaultScenesIfNeeded()
    {
        if (availableScenes == null || availableScenes.Length == 0)
        {
            availableScenes = new TVShowScene[3];
            
            // Escena inicial
            availableScenes[0] = new TVShowScene
            {
                sceneName = "Initial Scene",
                lightingTint = Color.white,
                cameraPosition = new Vector3(0, 0, -10),
                cameraSize = 5f
            };
            
            // Escena de selección de nombre
            availableScenes[1] = new TVShowScene
            {
                sceneName = "Name Selection Scene",
                lightingTint = Color.white,
                cameraPosition = new Vector3(0, 0, -10),
                cameraSize = 5f
            };
            
            // Escena final
            availableScenes[2] = new TVShowScene
            {
                sceneName = "Final Scene",
                lightingTint = Color.white,
                cameraPosition = new Vector3(0, 0, -10),
                cameraSize = 5f
            };
            
            if (debugMode)
            {
                Debug.Log("[TVShowSceneManager] Created default scenes");
            }
        }
    }

    private void InitializeComponents()
    {
        if (backgroundImage == null)
            backgroundImage = FindFirstObjectByType<Image>();
        
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        if (ambientAudioSource == null)
            ambientAudioSource = GetComponent<AudioSource>();
        
        if (debugMode)
        {
            Debug.Log("[TVShowSceneManager] Components initialized");
        }
    }

    private void SetupButtonListeners()
    {
        if (casoPiteadoButton != null)
        {
            casoPiteadoButton.onClick.RemoveAllListeners();
            casoPiteadoButton.onClick.AddListener(() => OnNameSelected("Caso Piteado", 0));
        }

        if (elGranChongoButton != null)
        {
            elGranChongoButton.onClick.RemoveAllListeners();
            elGranChongoButton.onClick.AddListener(() => OnNameSelected("El Gran Chongo", 1));
        }

        if (escribirNombreButton != null)
        {
            escribirNombreButton.onClick.RemoveAllListeners();
            escribirNombreButton.onClick.AddListener(() => OnWriteCustomName());
        }

        if (noDecidirButton != null)
        {
            noDecidirButton.onClick.RemoveAllListeners();
            noDecidirButton.onClick.AddListener(() => OnNameSelected("No decidir nombre ahora", 3));
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

        if (continuarButton != null)
        {
            continuarButton.onClick.RemoveAllListeners();
            continuarButton.onClick.AddListener(() => OnContinueToDemoShow());
        }

        if (debugMode)
        {
            Debug.Log("[TVShowSceneManager] Button listeners setup complete");
        }
    }

    #endregion

    #region Auto Setup for Android

    [ContextMenu("Auto Connect UI")]
    private void AutoConnectUI()
    {
        if (debugMode)
        {
            Debug.Log("[TVShowSceneManager] Auto connecting UI elements...");
        }

        if (customNameInputPanel == null)
        {
            GameObject foundPanel = GameObject.Find("CustomNameInputPanel");
            if (foundPanel != null)
            {
                customNameInputPanel = foundPanel;
                if (debugMode) Debug.Log("✅ CustomNameInputPanel found and connected");
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
                    if (debugMode) Debug.Log("✅ CustomNameInput found and configured for Android");
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

        if (debugMode)
        {
            Debug.Log("[TVShowSceneManager] Auto UI connection completed");
        }
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
            if (debugMode) Debug.Log("✅ Text component connected");
        }

        TextMeshProUGUI placeholder = customNameInput.transform.Find("Text Area/Placeholder")?.GetComponent<TextMeshProUGUI>();
        if (placeholder != null)
        {
            customNameInput.placeholder = placeholder;
            placeholder.text = "Escribe el nombre del programa...";
            if (debugMode) Debug.Log("✅ Placeholder connected");
        }

        customNameInput.contentType = TMP_InputField.ContentType.Standard;
        customNameInput.inputType = TMP_InputField.InputType.Standard;
        customNameInput.keyboardType = TouchScreenKeyboardType.Default;

        if (debugMode)
        {
            Debug.Log("✅ InputField configured for Android native keyboard");
        }
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

        if (debugMode)
        {
            Debug.Log($"✅ Button '{name}' created");
        }

        return button;
    }

    #endregion

    #region Scene Management

    public void SetupInitialScene()
    {
        if (debugMode)
        {
            Debug.Log("[TVShowSceneManager] Setting up initial scene");
        }

        // No cambiar escena automáticamente, solo ocultar paneles
        HideAllPanels();
        isTransitioning = false; // Asegurar que no esté en transición
    }

    public void ChangeToScene(int sceneIndex)
    {
        if (availableScenes == null || sceneIndex < 0 || sceneIndex >= availableScenes.Length)
        {
            if (debugMode)
            {
                Debug.LogWarning($"[TVShowSceneManager] Cannot change to scene {sceneIndex} - Invalid index or no scenes available");
            }
            return;
        }

        if (isTransitioning)
        {
            if (debugMode)
            {
                Debug.LogWarning($"[TVShowSceneManager] Cannot change to scene {sceneIndex} - Already transitioning");
            }
            return;
        }

        StartCoroutine(TransitionToScene(sceneIndex));
    }

    private IEnumerator TransitionToScene(int sceneIndex)
    {
        isTransitioning = true;
        currentSceneIndex = sceneIndex;
        TVShowScene targetScene = availableScenes[sceneIndex];

        if (debugMode)
        {
            Debug.Log($"[TVShowSceneManager] Transitioning to scene: {targetScene.sceneName}");
        }

        ApplySceneSettings(targetScene);
        
        // Pequeña pausa para la transición
        yield return new WaitForSeconds(0.1f);

        isTransitioning = false;

        if (debugMode)
        {
            Debug.Log($"[TVShowSceneManager] Scene transition complete: {targetScene.sceneName}");
        }
    }

    private void ApplySceneSettings(TVShowScene scene)
    {
        if (backgroundImage != null && scene.backgroundSprite != null)
        {
            backgroundImage.sprite = scene.backgroundSprite;
            backgroundImage.color = scene.lightingTint;
        }

        if (mainCamera != null)
        {
            mainCamera.transform.position = scene.cameraPosition;
            mainCamera.orthographicSize = scene.cameraSize;
        }

        if (ambientAudioSource != null && scene.ambientSound != null)
        {
            ambientAudioSource.clip = scene.ambientSound;
            if (!ambientAudioSource.isPlaying)
            {
                ambientAudioSource.Play();
            }
        }
    }

    #endregion

    #region UI Panel Management

    private void HideAllPanels()
    {
        if (nameSelectionPanel != null)
            nameSelectionPanel.SetActive(false);
        
        if (customNameInputPanel != null)
            customNameInputPanel.SetActive(false);
        
        if (finalPanel != null)
            finalPanel.SetActive(false);

        if (debugMode)
        {
            Debug.Log("[TVShowSceneManager] All panels hidden");
        }
    }

    public void ShowNameSelectionInterface()
    {
        if (debugMode)
        {
            Debug.Log("[TVShowSceneManager] ShowNameSelectionInterface called");
        }

        HideAllPanels();
        
        if (nameSelectionPanel != null)
        {
            nameSelectionPanel.SetActive(true);
            if (debugMode)
            {
                Debug.Log("[TVShowSceneManager] Name selection panel shown");
            }
        }
        else
        {
            Debug.LogError("[TVShowSceneManager] Name selection panel is null!");
        }
    }

    public void ShowCustomNameInput()
    {
        if (debugMode)
        {
            Debug.Log("[TVShowSceneManager] ShowCustomNameInput called");
        }
        
        if (customNameInputPanel != null)
        {
            customNameInputPanel.SetActive(true);
            if (debugMode)
            {
                Debug.Log("[TVShowSceneManager] Custom name input panel activated");
            }
            
            if (customNameInput != null)
            {
                customNameInput.text = "";
                customNameInput.Select();
                customNameInput.ActivateInputField();
                
                if (debugMode)
                {
                    Debug.Log("[TVShowSceneManager] Input field activated - Android keyboard should appear");
                }
            }
            else
            {
                Debug.LogError("[TVShowSceneManager] Custom name input field is null!");
            }
        }
        else
        {
            Debug.LogError("[TVShowSceneManager] Custom name input panel is null!");
        }
    }

    public void HideCustomNameInput()
    {
        if (debugMode)
        {
            Debug.Log("[TVShowSceneManager] HideCustomNameInput called");
        }

        if (customNameInputPanel != null)
        {
            customNameInputPanel.SetActive(false);
            if (debugMode)
            {
                Debug.Log("[TVShowSceneManager] Custom name input panel hidden");
            }
        }
    }

    public void SetupFinalScene()
    {
        /*if (debugMode)
        {
            Debug.Log("[TVShowSceneManager] SetupFinalScene called");
        }

        HideAllPanels();
        
        if (finalPanel != null)
        {
            finalPanel.SetActive(true);
            if (debugMode)
            {
                Debug.Log("[TVShowSceneManager] Final panel shown");
            }
        }
        else
        {
            Debug.LogError("[TVShowSceneManager] Final panel is null!");
        }*/
    }

    #endregion

    #region Button Event Handlers

    private void OnNameSelected(string showName, int choiceIndex)
    {
        if (debugMode)
        {
            Debug.Log($"[TVShowSceneManager] Name selected: {showName} (choice {choiceIndex})");
        }

        HideAllPanels();

        //if (dialogManager != null)
       // {
        //    dialogManager.MakeChoice(choiceIndex);
        //}
        
        if (dialogManagerINPUT != null)
        {
            dialogManagerINPUT.MakeChoice(choiceIndex);
        }
        
        else
        {
            Debug.LogError("[TVShowSceneManager] Dialog manager not found!");
        }
    }

    private void OnWriteCustomName()
    {
        if (debugMode)
        {
            Debug.Log("[TVShowSceneManager] Write custom name selected");
        }

        //if (dialogManager != null)
        //{
        //    dialogManager.MakeChoice(2);

        //}
        
        if (dialogManagerINPUT != null)
        {
            dialogManagerINPUT.MakeChoice(2);
        }
        
        else
        {
            Debug.LogError("[TVShowSceneManager] Dialog manager not found!");
        }
    }

    private void OnConfirmCustomName()
    {
        if (debugMode)
        {
            Debug.Log("[TVShowSceneManager] Confirm custom name clicked");
        }

        if (customNameInput != null)
        {
            string customName = customNameInput.text.Trim();
            
            if (debugMode)
            {
                Debug.Log($"[TVShowSceneManager] Custom name entered: '{customName}'");
            }

            if (!string.IsNullOrEmpty(customName))
            {
                if (customName.Length <= 30)
                {
                    HideCustomNameInput();
                    
                    //if (dialogManager != null)
                    //{
                   //     dialogManager.OnCustomNameEntered(customName);

                    //}
                    
                    if (dialogManagerINPUT != null)
                    {
                        dialogManagerINPUT.OnCustomNameEntered(customName);
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
        if (debugMode)
        {
            Debug.Log("[TVShowSceneManager] Cancel custom name clicked");
        }

        HideCustomNameInput();
        
        // Mostrar de nuevo la selección de nombres
        ShowNameSelectionInterface();

        //if (dialogManager!= null)
        //{
        //    dialogManager.OnCustomNameCancelled();

        //}
        
        if (dialogManagerINPUT != null)
        {
            dialogManagerINPUT.OnCustomNameCancelled();
        }
        
        else
        {
            Debug.LogError("[TVShowSceneManager] Dialog manager not found!");
        }
    }

    private void OnContinueToDemoShow()
    {
        if (debugMode)
        {
            Debug.Log("[TVShowSceneManager] Continue to main show clicked");
        }

        //if (dialogManager != null || dialogManagerINPUT != null)
        //{
       //     dialogManager.StartDemoShow();

        //}
        
        if (dialogManagerINPUT != null)
        {
            dialogManagerINPUT.StartDemoShow();
        }
        
        else
        {
            Debug.LogError("[TVShowSceneManager] Dialog manager not found!");
        }
    }

    #endregion

    #region Public Methods

    public void UpdateShowNameDisplay(string showName)
    {
        if (showNameDisplay != null)
        {
            showNameDisplay.text = $"¡Bienvenidos a {showName}!";
            if (debugMode)
            {
                Debug.Log($"[TVShowSceneManager] Show name display updated: {showName}");
            }
        }
        else
        {
            Debug.LogWarning("[TVShowSceneManager] Show name display is null!");
        }
    }

    public string GetCurrentSceneName()
    {
        if (availableScenes != null && currentSceneIndex >= 0 && currentSceneIndex < availableScenes.Length)
        {
            return availableScenes[currentSceneIndex].sceneName;
        }
        return "Unknown";
    }

    public void SetDebugMode(bool enabled)
    {
        debugMode = enabled;
        if (debugMode)
        {
            Debug.Log("[TVShowSceneManager] Debug mode enabled");
        }
    }

    public bool IsTransitioning()
    {
        return isTransitioning;
    }

    #endregion

    #region Debug Methods

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugPrintStatus()
    {
        Debug.Log("=== TVShowSceneManager Status ===");
        Debug.Log($"Current Scene: {GetCurrentSceneName()} (Index: {currentSceneIndex})");
        Debug.Log($"Available Scenes: {(availableScenes != null ? availableScenes.Length : 0)}");
        Debug.Log($"Is Transitioning: {isTransitioning}");
        Debug.Log($"Name Selection Panel: {(nameSelectionPanel != null ? "Found" : "Missing")}");
        Debug.Log($"Custom Input Panel: {(customNameInputPanel != null ? "Found" : "Missing")}");
        Debug.Log($"Final Panel: {(finalPanel != null ? "Found" : "Missing")}");
        Debug.Log("================================");
    }

    #endregion
}