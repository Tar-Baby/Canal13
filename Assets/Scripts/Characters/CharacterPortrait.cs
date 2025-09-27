using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CharacterPortrait : MonoBehaviour
{
    [SerializeField] private Image _spriteImage;
    [SerializeField] private GameObject _characterRoot;
    
    [Header("Scaler Target")]
    [SerializeField] private Transform _scaleTarget; // <- drag "PortraitRoot" object here

    [Header("Animation Settings")]
    public float fadesAnimationDuration = 0.3f;
    public Vector3 talkingScale = new Vector3(1.05f, 1.05f, 1.05f);
    public Vector3 idleScale = Vector3.one;

    [Header("Scale Animation Settings")]
    public float overshootMultiplier = 1.15f;
// 🎨 Use Unity's curve editor for pop easing
    public AnimationCurve talkEasingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve idleEasingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private Coroutine _fadeRoutine;
    private Coroutine _scaleRoutine;

    private string _currentCharacterName;
    public string CurrentCharacterName => _currentCharacterName;

    public void Setup(string characterName, Sprite initialSprite)
    {
        _currentCharacterName = characterName;
        _spriteImage.sprite = initialSprite;
        _characterRoot.SetActive(true);

        // Force invisible at start
        _spriteImage.color = new Color(1f, 1f, 1f, 0f);
        _spriteImage.rectTransform.localScale = idleScale;

        // Start fade in
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeInCoroutine());
        
        Debug.Log($"[Setup] Showing {characterName} with sprite {initialSprite?.name}");
    }

    public void Clear()
    {
        Debug.Log($"[Clear] Clearing portrait {_currentCharacterName}");
        
        _currentCharacterName = null;
        _spriteImage.sprite = null;
        _characterRoot.SetActive(false);
    }

    public void SetSprite(Sprite newSprite)
    {
        if (newSprite != null && _spriteImage.sprite != newSprite)
        {
            Debug.Log($"[SetSprite] {_currentCharacterName} sprite changed to {newSprite.name}");
            _spriteImage.sprite = newSprite;
        }
    }

    // --- Talking/Idle scale (scale only, alpha untouched) ---
    public void SetTalkingState()
    {
        if (!_characterRoot.activeSelf) return;
        Debug.Log($"[SetTalkingState] {_currentCharacterName}");
        if (_scaleRoutine != null) StopCoroutine(_scaleRoutine);
        _scaleRoutine = StartCoroutine(ScaleTo(talkingScale, overshoot: true)); // pop!
    }

    public void SetIdleState()
    {
        if (!_characterRoot.activeSelf) return;
        Debug.Log($"[SetIdleState] {_currentCharacterName}");
        if (_scaleRoutine != null) StopCoroutine(_scaleRoutine);
        _scaleRoutine = StartCoroutine(ScaleTo(idleScale, overshoot: false)); // no overshoot
    }

    // --- Hide with fade out ---
    public void SetHiddenState()
    {
        Debug.Log($"[SetHiddenState] {_currentCharacterName}");
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeOutCoroutine());
    }

    // --- Coroutines ---
    // --- Scale Coroutine with optional overshoot ---
    private IEnumerator ScaleTo(Vector3 endScale, bool overshoot = false)
    {
        Vector3 start = _scaleTarget.localScale;
        float t = 0f;
        float duration = fadesAnimationDuration;
        // Overshoot = make the target slightly larger
        Vector3 overshootScale = endScale * overshootMultiplier;

        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = Mathf.Clamp01(t / duration);

            if (overshoot)
            {
                // 🎨 Evaluate custom curve from Inspector
                //float curveValue = talkEasingCurve.Evaluate(progress);

                if (progress < 0.6f)
                {
                    if (progress < 0.6f)
                        _scaleTarget.localScale = Vector3.Lerp(start, overshootScale, progress / 0.6f);
                    else
                        _scaleTarget.localScale = Vector3.Lerp(overshootScale, endScale, (progress - 0.6f) / 0.4f);
                }
                else
                {
                    float curveValue = idleEasingCurve.Evaluate(progress);
                    _scaleTarget.localScale = Vector3.LerpUnclamped(start, endScale, curveValue);
                }
            }
            else
            {
                // Idle uses the idle curve
                float curveValue = idleEasingCurve.Evaluate(progress);
                _scaleTarget.localScale = Vector3.LerpUnclamped(start, endScale, curveValue);
            }

            Debug.Log($"[ScaleTo] {_currentCharacterName} scale now {_scaleTarget.localScale}");
            yield return null;
        }

        // ✅ lock exact final scale
        _scaleTarget.localScale = endScale;
        Debug.Log($"[ScaleTo] {_currentCharacterName} ended at {_scaleTarget.localScale}");
        _scaleRoutine = null;
    }

    private IEnumerator FadeInCoroutine()
    {
        Debug.Log($"[FadeIn] {_currentCharacterName} starting fade in");
        
        Color start = new Color(1f, 1f, 1f, 0f);
        Color end = new Color(1f, 1f, 1f, 1f);

        float t = 0f;
        while (t < fadesAnimationDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / fadesAnimationDuration);
            _spriteImage.color = Color.Lerp(start, end, p);
            yield return null;
        }
        // ✅ Lock at full opacity
        _spriteImage.color = end;
        Debug.Log($"[FadeIn] {_currentCharacterName} completed fade in");
        _fadeRoutine = null;
    }

    private IEnumerator FadeOutCoroutine()
    {
        Debug.Log($"[FadeOut] {_currentCharacterName} starting fade out");
        
        Color start = _spriteImage.color;
        Color end = new Color(start.r, start.g, start.b, 0f);

        float t = 0f;
        while (t < fadesAnimationDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / fadesAnimationDuration);
            _spriteImage.color = Color.Lerp(start, end, p);
            yield return null;
        }
        // ✅ Lock at full transparency
        _spriteImage.color = end;
        _characterRoot.SetActive(false);
        Debug.Log($"[FadeOut] {_currentCharacterName} completed fade out");
        _fadeRoutine = null;
    }
}