using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TVShowSetupHelper : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool debugMode = true;
    
    private TVShowSceneManager sceneManager;
    private DialogManagerINPUT dialogManagerINPUT;

    private void Start()
    {
        sceneManager = FindFirstObjectByType<TVShowSceneManager>();
        dialogManagerINPUT = FindFirstObjectByType<DialogManagerINPUT>();
        
        if (debugMode)
        {
            Debug.Log("[TVShowSetupHelper] Setup Helper initialized");
        }
    }

    // Método simple para mostrar el teclado nativo de Android
    public void ShowVirtualKeyboard()
    {
        if (debugMode)
        {
            Debug.Log("[TVShowSetupHelper] Android native keyboard will be shown by TMP_InputField");
        }
        // No necesitamos hacer nada aquí, el TMP_InputField maneja el teclado nativo automáticamente
    }

    public void HideVirtualKeyboard()
    {
        if (debugMode)
        {
            Debug.Log("[TVShowSetupHelper] Android native keyboard hidden");
        }
        // El teclado se oculta automáticamente cuando se pierde el foco del InputField
    }

    [ContextMenu("Debug UI Status")]
    public void DebugUIStatus()
    {
        DialogPanelUI dialogUI = FindFirstObjectByType<DialogPanelUI>();
        if (dialogUI != null)
        {
            // Usar reflexión para acceder al array de botones
            var choiceButtonsField = typeof(DialogPanelUI).GetField("choiceButtons", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (choiceButtonsField != null)
            {
                Button[] buttons = (Button[])choiceButtonsField.GetValue(dialogUI);
                Debug.Log($"Choice buttons array length: {buttons.Length}");
                
                for (int i = 0; i < buttons.Length; i++)
                {
                    if (buttons[i] != null)
                    {
                        Debug.Log($"  Button {i}: {buttons[i].name} - Active: {buttons[i].gameObject.activeInHierarchy}");
                    }
                    else
                    {
                        Debug.Log($"  Button {i}: NULL");
                    }
                }
            }
        }
        
        if (sceneManager != null)
        {
            Debug.Log("Scene Manager found ✅");
        }
        else
        {
            Debug.Log("Scene Manager missing ❌");
        }
        
        if (dialogManagerINPUT != null)
        {
            Debug.Log("Dialog Manager found ✅");
        }
        else
        {
            Debug.Log("Dialog Manager missing ❌");
        }
    }

    [ContextMenu("Test Custom Name Flow")]
    public void TestCustomNameFlow()
    {
        if (dialogManagerINPUT != null)
        {
            Debug.Log("=== Testing Custom Name Flow ===");
            Debug.Log($"Dialog Playing: {dialogManagerINPUT.IsDialogPlaying()}");
            Debug.Log($"Waiting for Choice: {dialogManagerINPUT.IsWaitingForChoice()}");
            Debug.Log($"Waiting for Custom Input: {dialogManagerINPUT.IsWaitingForCustomInput()}");
            
            // Simular entrada de nombre personalizado
            dialogManagerINPUT.OnCustomNameEntered("Test Show Name");
        }
    }
}