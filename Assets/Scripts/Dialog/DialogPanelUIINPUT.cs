using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

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
    [SerializeField] private TextMeshProUGUI speakerName;
    
    [Header("Settings")]
    [SerializeField] private float typewriterSpeed = 0.05f;
    [SerializeField] private bool debugMode = true; // AGREGADO: Para debug
    
    private bool isTyping = false;
    private string currentText = "";
    private DialogManagerINPUT dialogManagerINPUT;
    private bool hasShownText = false;
    private bool isFinalScreen = false;

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
        
    }
    
    private void ShowSpeakerName()
    {
        speakerName.text = dialogManagerINPUT.GetCurrentSpeaker();
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
            finalButton.onClick.AddListener(OnFinalButtonClicked); //Leer AddListener
        }
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
        currentText = dialogLine;
        hasShownText = false;
        
        ShowSpeakerName();
        Debug.Log(dialogManagerINPUT.GetCurrentSpeaker()); //tener el nombre del speaker
        
        if (choicesPanel != null)
        {
            choicesPanel.SetActive(false);
        }
        if (finalButton != null)
        {
            finalButton.gameObject.SetActive(false);
        }
        
        ShowContinueButton();
        
        if (dialogText != null)
        {
            dialogText.text = "";
        }
        
        StartCoroutine(TypewriterEffect());
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
        StartCoroutine(ShowChoicesDelayed(choices, 0.1f));
    }

    // MODIFICADO: Mejor debug y manejo de 4 opciones
    private IEnumerator ShowChoicesDelayed(List<string> choices, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (debugMode)
        {
            Debug.Log($"[DialogPanelUI] ShowChoicesDelayed: {choices.Count} choices, {choiceButtons.Length} buttons");
            for (int i = 0; i < choices.Count; i++)
            {
                Debug.Log($"  Choice {i}: '{choices[i]}'");
            }
        }
        
        if (choicesPanel != null)
        {
            choicesPanel.SetActive(true);
        }

        int numChoices = Mathf.Min(choices.Count, choiceButtons.Length);
        
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (choiceButtons[i] != null)
            {
                if (i < numChoices)
                {
                    choiceButtons[i].gameObject.SetActive(true);
                    
                    TextMeshProUGUI buttonText = choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = choices[i];
                        if (debugMode) Debug.Log($"[DialogPanelUI] Button {i} activated: '{choices[i]}'");
                    }
                    else
                    {
                        Text legacyText = choiceButtons[i].GetComponentInChildren<Text>();
                        if (legacyText != null)
                        {
                            legacyText.text = choices[i];
                            if (debugMode) Debug.Log($"[DialogPanelUI] Button {i} activated (legacy): '{choices[i]}'");
                        }
                        else
                        {
                            if (debugMode) Debug.LogError($"[DialogPanelUI] Button {i} has no text component!");
                        }
                    }
                }
                else
                {
                    choiceButtons[i].gameObject.SetActive(false);
                    if (debugMode) Debug.Log($"[DialogPanelUI] Button {i} hidden (not needed)");
                }
            }
            else
            {
                if (debugMode) Debug.LogError($"[DialogPanelUI] choiceButtons[{i}] is NULL!");
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
        if (dialogManagerINPUT != null)
        {
            //finalButton.GetComponent<Button>().onClick.Invoke();
            dialogManagerINPUT.StartDemoShow();
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

    public void SetDebugMode(bool enabled)
    {
        debugMode = enabled;
    }

    #endregion
}