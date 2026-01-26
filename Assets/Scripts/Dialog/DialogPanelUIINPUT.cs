using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Audio;

public class DialogPanelUIINPUT : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private GameObject continueButton;
    [SerializeField] private GameObject choicesPanel;
    [SerializeField] private Button[] choiceButtons = new Button[4]; // CAMBIADO: Array de 4 botones
    [SerializeField] private Button finalButton; // Botón "Comenzar"
    [SerializeField] private GameObject namePanel; // panel de nombre de speaker
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private RectTransform namePanelRect;
    [SerializeField] private float horizontalPadding = 30f; 
    
    [Header("Screen Fade")]
    // Full-screen black panel with CanvasGroup (alpha = 0 to start)
    [SerializeField] private CanvasGroup blackFade;
    [SerializeField] private float fadeOutDuration = 2f;
    [Header("Audio Mixer (Music)")]
    // Assign your AudioMixer and ensure the Music group's Volume is exposed as musicVolumeParam
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string musicVolumeParam = "MusicVolume";
    
    [Header("Settings")]
    [SerializeField] private float typewriterSpeed = 0.05f;
    [SerializeField] private bool debugMode = true; // AGREGADO: Para debug
    
    private bool isTyping = false;
    private string currentText = "";
    private DialogManagerINPUT dialogManagerINPUT;
    private bool hasShownText = false;
    private bool isFinalScreen = false;
    private bool isFadingOut = false;

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
        dialogManagerINPUT = Object.FindFirstObjectByType<DialogManagerINPUT>();
        
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }
        
        SetupButtons();
        HideAllInteractiveElements();
        
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
        
        if (namePanel != null)
        {
            namePanel.SetActive(false);
        }
        
        // Ensure black fade panel is transparent and not blocking at start
        if (blackFade != null)
            {
            blackFade.alpha = 0f;
            blackFade.blocksRaycasts = false;
            blackFade.interactable = false;
            }
        
    }
    
    private void ShowSpeakerName()
    {
        speakerNameText.text = dialogManagerINPUT.GetCurrentSpeaker();
    }

    private void SetupButtons()
    {
        //if (continueButton != null)
        //{
        //    continueButton.onClick.AddListener(OnContinueClicked);
        //}
        
        // MODIFICADO: Mejor manejo de botones con debug
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (choiceButtons[i] != null)
            {
                int buttonIndex = i;
                choiceButtons[i].onClick.RemoveAllListeners(); // Limpiar listeners anteriores
                choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(buttonIndex));
                
                if (debugMode)
                {
                    Debug.Log($"[DialogPanelUI] Button {i} ({choiceButtons[i].name}) configured");
                }
            }
            else
            {
                if (debugMode)
                {
                    Debug.LogWarning($"[DialogPanelUI] choiceButtons[{i}] is null!");
                }
            }
        }
        
        if (finalButton != null)
        {
            finalButton.onClick.RemoveAllListeners();
            finalButton.onClick.AddListener(OnFinalButtonClicked); //Leer AddListener
        }
    }
    
    private Coroutine _resizeRoutine;
    private void UpdateNameBox(string speakerName)
    {
        // Set the text
        speakerNameText.text = dialogManagerINPUT.GetCurrentSpeaker();

        // Force TMP to recalc width
        speakerNameText.ForceMeshUpdate();
        float targetWidth = speakerNameText.preferredWidth + horizontalPadding;

        // Smooth resize
        if (_resizeRoutine != null) StopCoroutine(_resizeRoutine);
        _resizeRoutine = StartCoroutine(SmoothResize(targetWidth));
    }
    
    private IEnumerator SmoothResize(float targetWidth)
    {
        float start = namePanelRect.rect.width;
        float t = 0f;
        float duration = 0.25f; // How fast the box resizes

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            float width = Mathf.Lerp(start, targetWidth, p);
            namePanelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            yield return null;
        }

        namePanelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
        _resizeRoutine = null;
    }

    private void Update()
    {
        if (dialogPanel != null && dialogPanel.activeInHierarchy && dialogManagerINPUT != null)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                if (isTyping)
                {
                    CompleteTypewriter();
                }
                else if (!dialogManagerINPUT.IsWaitingForChoice() && !dialogManagerINPUT.IsWaitingForCustomInput())
                {
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
        
        
        if (namePanel != null)
        {
            namePanel.SetActive(true);
        }
        
        hasShownText = false;
        HideAllInteractiveElements();
        
        if (debugMode) Debug.Log("[DialogPanelUI] Dialog UI shown");
        isFinalScreen = false;
    }

    private void HideDialog()
    {
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }
        
        hasShownText = false;
        
        if (debugMode) Debug.Log("[DialogPanelUI] Dialog UI hidden");
    }

    public void DisplayDialogLine(string dialogLine)
    {
        hasShownText = false;

        string speakerName = "";
        string dialogueContent = dialogLine;

// Split the line into "Name" and "Text" only once
        if (dialogLine.Contains(":"))
        {
            string[] parts = dialogLine.Split(new char[] { ':' }, 2);
            speakerName = parts[0].Trim();
            dialogueContent = parts[1].Trim();
        }

// Set the speaker name in the name box (optional)
        if (!string.IsNullOrEmpty(speakerName))
        {
            if (speakerNameText != null && namePanelRect != null)
            {
                UpdateNameBox(speakerName);
            }
            if (namePanel != null)
                namePanel.SetActive(true);
        }
        else
        {
            // Hide name panel if no speaker name
            if (namePanel != null)
                namePanel.SetActive(false);
        }

// Store only the dialogue text for typing
        currentText = dialogueContent;
        
        
        //ShowSpeakerName();
        Debug.Log(dialogManagerINPUT.GetCurrentSpeaker()); //tener el nombre del speaker
        
        if (choicesPanel != null)
        {
            choicesPanel.SetActive(false);
        }
        if (finalButton != null)
        {
            finalButton.gameObject.SetActive(false);
        }
        
        if (dialogText != null)
        {
            dialogText.text = "";
        }
        
        StartCoroutine(TypewriterEffect());
        ShowContinueButton();
    }

    private IEnumerator TypewriterEffect()
    {
        isTyping = true;
        hasShownText = false;
        
        for (int i = 0; i <= currentText.Length; i++)
        {
            if (dialogText != null)
            {
                dialogText.text = currentText.Substring(0, i);
            }
            
            yield return new WaitForSeconds(typewriterSpeed);
        }
        
        isTyping = false;
        hasShownText = true;
        
        ShowContinueButton();
    }

    private void CompleteTypewriter()
    {
        StopAllCoroutines();
        
        if (dialogText != null)
        {
            dialogText.text = currentText;
        }
        
        isTyping = false;
        hasShownText = true;
    }

    private void ShowChoices(List<string> choices)
    {
        HideAllInteractiveElements();
        PersistentCoroutineRunner.Instance.StartCoroutine(ShowChoicesDelayed(choices, 1f));
    }

    // MODIFICADO: Mejor debug y manejo de 4 opciones
    private IEnumerator ShowChoicesDelayed(List<string> choices, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Safety: confirm the object or its dependencies are still valid
        if (this == null || choicesPanel == null)
            yield break;

        if (!gameObject) yield break;

        // Your existing logic below
        if (debugMode)
        {
            Debug.Log($"[DialogPanelUI] ShowChoicesDelayed: {choices.Count} choices, {choiceButtons.Length} buttons");
            for (int i = 0; i < choices.Count; i++)
            {
                Debug.Log($"  Choice {i}: '{choices[i]}'");
            }
        }

        choicesPanel.SetActive(true);

        int numChoices = Mathf.Min(choices.Count, choiceButtons.Length);

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (choiceButtons[i] != null)
            {
                if (i < numChoices)
                {
                    choiceButtons[i].gameObject.SetActive(true);
                    TextMeshProUGUI buttonText =
                        choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = choices[i];
                    }
                }
                else
                {
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
        if (continueButton != null && !isFinalScreen)
        {
            continueButton.gameObject.SetActive(true);
        }
    }

    private void ShowFinalButton()
    {
        isFinalScreen = true;
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(true);
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
        HideChoices();
        if (finalButton != null)
        {
            finalButton.gameObject.SetActive(false);
        }
        
        isFinalScreen = false;
        continueButton.gameObject.SetActive(false);
    }

    #region Button Events

    private void OnContinueClicked()
    {
        if (dialogManagerINPUT != null && !dialogManagerINPUT.IsWaitingForChoice() && !dialogManagerINPUT.IsWaitingForCustomInput())
        {
            hasShownText = false;
            dialogManagerINPUT.ContinueDialog();
        }
    }

    // MODIFICADO: Mejor debug para selección de opciones
    private void OnChoiceSelected(int choiceIndex)
    {
        if (debugMode)
        {
            Debug.Log($"[DialogPanelUI] OnChoiceSelected: Button {choiceIndex} clicked!");
            Debug.Log($"  DialogManager exists: {dialogManagerINPUT != null}");
            if (dialogManagerINPUT != null)
            {
                Debug.Log($"  IsWaitingForChoice: {dialogManagerINPUT.IsWaitingForChoice()}");
                Debug.Log($"  IsWaitingForCustomInput: {dialogManagerINPUT.IsWaitingForCustomInput()}");
            }
        }

        if (dialogManagerINPUT != null && dialogManagerINPUT.IsWaitingForChoice())
        {
            hasShownText = false;
            dialogManagerINPUT.MakeChoice(choiceIndex);
        }
        else
        {
            if (debugMode)
            {
                Debug.LogError($"[DialogPanelUI] Cannot make choice - Manager: {dialogManagerINPUT != null}, Waiting: {dialogManagerINPUT?.IsWaitingForChoice()}");
            }
        }
    }

    private void OnFinalButtonClicked()
    {
        if (isFadingOut) return;
        isFadingOut = true;
        // Prevent more UI interaction during fade
        HideAllInteractiveElements();
        StartCoroutine(FadeOutSceneAudioAndScreen());
    }
    
    // Fades Music mixer to -80 dB and screen to black, then calls StartDemoShow.
    private IEnumerator FadeOutSceneAudioAndScreen()
    {
        float t = 0f;
        // Prepare black panel
                     if (blackFade != null) 
                     {
             blackFade.gameObject.SetActive(true);
             blackFade.blocksRaycasts = true;
             blackFade.interactable = true;
            }
        // Read current dB if mixer assigned
        float startDb = 0f;
        float endDb = -30f;
        if (audioMixer != null)
        {
                       audioMixer.GetFloat(musicVolumeParam, out startDb);
        }
        while (t < fadeOutDuration)
                    {
                        t += Time.deltaTime;
                        float u = Mathf.Clamp01(t / fadeOutDuration);
            
                            // Audio mixer fade
                                if (audioMixer != null)
                            {
                                float newDb = Mathf.Lerp(startDb, endDb, u);
                                audioMixer.SetFloat(musicVolumeParam, newDb);
                            }
            
                            // Screen fade
                                if (blackFade != null) blackFade.alpha = u;
            
                            yield return null;
                   }
        
                    // Ensure final states
                       if (audioMixer != null) audioMixer.SetFloat(musicVolumeParam, endDb);
                if (blackFade != null) blackFade.alpha = 1f;
        
                    // Continue your flow
                        if (dialogManagerINPUT != null)
                    {
                        dialogManagerINPUT.StartDemoShow();
                    }
        
                    isFadingOut = false;
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

    public void SetDebugMode(bool enabled)
    {
        debugMode = enabled;
    }

    #endregion
}