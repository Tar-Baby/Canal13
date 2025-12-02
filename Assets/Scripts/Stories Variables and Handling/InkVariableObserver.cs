using UnityEngine;
using TMPro;
using Ink.Runtime;

[AddComponentMenu("Ink/Ink Variable Observer (Generic)")]
public class InkVariableObserver : MonoBehaviour
{
    [Header("Ink")]
    [Tooltip("Global Ink variable to observe (declared with VAR in Ink).")]
    [SerializeField] private string variableName = "show_name";

    [Header("Optional UI Binding")]
    [SerializeField] private TextMeshProUGUI targetLabel;
    [SerializeField] private string emptyFallback = "Programa Sin Nombre";

    [Header("Debug")]
    [SerializeField] private bool logDebug = false;

    private bool _subscribed;

    private void OnEnable()
    {
        if (InkService.Instance != null)
            InkService.Instance.OnActiveStoryChanged += OnActiveStoryChanged;

        TrySubscribe();
    }

    private void Start()
    {
        if (!_subscribed) TrySubscribe();
    }

    private void OnDisable()
    {
        TryUnsubscribe();

        if (InkService.Instance != null)
            InkService.Instance.OnActiveStoryChanged -= OnActiveStoryChanged;
    }

    private void OnActiveStoryChanged(Story s)
    {
        TryUnsubscribe();
        TrySubscribe();
    }

    private void TrySubscribe()
    {
        if (_subscribed) return;
        if (InkService.Instance == null || InkService.Instance.ActiveStory == null) return;
        if (string.IsNullOrWhiteSpace(variableName)) return;

        //InkService.Instance.ObserveVar(variableName, OnInkVarChanged);
        _subscribed = true;

        //object current = InkService.Instance.GetVar<object>(variableName);
        //OnInkVarChanged(variableName, current);

        //if (logDebug) Debug.Log($"[InkVariableObserver] Subscribed to '{variableName}'. Initial='{current}'");
    }

    private void TryUnsubscribe()
    {
        if (!_subscribed) return;
        if (InkService.Instance == null || InkService.Instance.ActiveStory == null) return;

        //InkService.Instance.RemoveObserver(variableName, OnInkVarChanged);
        _subscribed = false;

        if (logDebug) Debug.Log($"[InkVariableObserver] Unsubscribed from '{variableName}'.");
    }

    // Signature must match VariablesState.VariableObserver
    private void OnInkVarChanged(string name, object value)
    {
        string textVal = value?.ToString() ?? "";

        if (targetLabel != null)
            targetLabel.text = string.IsNullOrEmpty(textVal) ? emptyFallback : textVal;

        if (logDebug) Debug.Log($"[InkVariableObserver] {name} -> '{textVal}'");
    }
}