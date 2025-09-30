using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CharacterPortrait : MonoBehaviour
{
    [SerializeField] private Image _spriteImage;        // The UI Image to fade
    [SerializeField] private GameObject _characterRoot; // Outer slot container
    [SerializeField] private Transform _scaleTarget;    // PortraitRoot child to scale

    [Header("Speaker Tint Settings")]
    public Color speakerColor = Color.white; // Bright speaker
    public Color nonSpeakerColor = new Color(0.6f, 0.6f, 0.6f, 1f); // Dimmed non-speaker
    public float tintDuration = 0.25f; // How long to lerp tint
    
    [Header("Animation Settings")]
    public float animationDuration = 0.25f;
    public Vector3 idleScale = Vector3.one;
    public Vector3 talkingScale = new Vector3(1.1f, 1.1f, 1.1f);
    public float overshootMultiplier = 1.15f;

    [Header("Easing Curves")]
    public AnimationCurve talkCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve idleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine _scaleRoutine;
    private Coroutine _fadeRoutine;
    private string _currentCharacterName;
    private bool justShown = false;

    public string CurrentCharacterName => _currentCharacterName;

    // --- Show / Setup
    private bool isFadingIn;
    public void Setup(string characterName, Sprite initialSprite)
    {
        _currentCharacterName = characterName;
        _spriteImage.sprite = initialSprite;
        _characterRoot.SetActive(true);

        _scaleTarget.localScale = idleScale;
        Color c = _spriteImage.color;
        _spriteImage.color = new Color(c.r, c.g, c.b, 0f);

        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        isFadingIn = true;
        _fadeRoutine = StartCoroutine(FadeInCoroutine());

        justShown = true;
        Debug.Log($"[Setup] Showing '{characterName}' sprite='{initialSprite?.name}'");
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
            Debug.Log($"[SetSprite] {_currentCharacterName} sprite -> {newSprite.name}");
            _spriteImage.sprite = newSprite;
        }
    }

    // --- Scale States
    public void SetTalkingState()
    {
        if (!_characterRoot.activeSelf) return;
        if (_scaleRoutine != null) StopCoroutine(_scaleRoutine);

        if (justShown)
        {
            // First line: smooth to target, no pop
            _scaleRoutine = StartCoroutine(SmoothScaleTo(talkingScale, useOvershoot:false));
            justShown = false;
            Debug.Log($"[SetTalkingState] {_currentCharacterName} first line → no overshoot");
        }
        else
        {
            // Subsequent lines: overshoot pop
            _scaleRoutine = StartCoroutine(SmoothScaleTo(talkingScale, useOvershoot:true));
            Debug.Log($"[SetTalkingState] {_currentCharacterName} talking (overshoot pop)");
        }
    }

    public void SetIdleState()
    {
        if (!_characterRoot.activeSelf) return;
        if (_scaleRoutine != null) StopCoroutine(_scaleRoutine);

        _scaleRoutine = StartCoroutine(SmoothScaleTo(idleScale, useOvershoot:false));
        Debug.Log($"[SetIdleState] {_currentCharacterName} -> idle scale {idleScale}");
    }

    public void SetHiddenState()
    {
        Debug.Log($"[SetHiddenState] {_currentCharacterName}");
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeOutCoroutine());
    }

    // --- Scale Coroutine with optional overshoot ---
    private IEnumerator SmoothScaleTo(Vector3 endScale, bool useOvershoot)
    {
        Vector3 start = _scaleTarget.localScale;
        float t = 0f;
        float duration = animationDuration;
        Vector3 overshootScale = endScale * overshootMultiplier;

        Debug.Log($"[ScaleTo] {_currentCharacterName} start={start} → end={endScale} overshoot={useOvershoot}");

        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = Mathf.Clamp01(t / duration);

            if (useOvershoot)
            {
                // Use custom talkCurve to time the overshoot
                float curveValue = talkCurve.Evaluate(progress);

                if (progress < 0.6f)
                {
                    _scaleTarget.localScale = Vector3.Lerp(start, overshootScale, curveValue / 0.6f);
                }
                else
                {
                    _scaleTarget.localScale = Vector3.Lerp(overshootScale, endScale, (curveValue - 0.6f) / 0.4f);
                }
            }
            else
            {
                // Use custom idleCurve
                float curveValue = idleCurve.Evaluate(progress);
                _scaleTarget.localScale = Vector3.Lerp(start, endScale, curveValue);
            }

            yield return null;
        }

        _scaleTarget.localScale = endScale;
        Debug.Log($"[ScaleTo] {_currentCharacterName} ended at {_scaleTarget.localScale}");
        _scaleRoutine = null;
        justShown = false;
    }

    // --- Fade In/Out ---
    private IEnumerator FadeInCoroutine()
    {
        Debug.Log($"[FadeIn] {_currentCharacterName} starting");
        Debug.Log($"[FadeIn] {_currentCharacterName} alpha={_spriteImage.color.a}");
        Color start = new Color(1f, 1f, 1f, 0f);
        Color end = new Color(1f, 1f, 1f, 1f);

        float t = 0f;
        while (t < animationDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / animationDuration);
            _spriteImage.color = Color.Lerp(start, end, p);
            yield return null;
        }

        _spriteImage.color = end;
        isFadingIn = false; //  now tinting can apply
        Debug.Log($"[FadeIn] {_currentCharacterName} complete");
        _fadeRoutine = null;
    }

    private IEnumerator FadeOutCoroutine()
    {
        Debug.Log($"[FadeOut] {_currentCharacterName} starting");
        Color start = _spriteImage.color;
        Color end = new Color(start.r, start.g, start.b, 0f);

        float t = 0f;
        while (t < animationDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / animationDuration);
            _spriteImage.color = Color.Lerp(start, end, p);
            yield return null;
        }

        _spriteImage.color = end;
        _characterRoot.SetActive(false);
        Debug.Log($"[FadeOut] {_currentCharacterName} complete");
        _fadeRoutine = null;
    }
    
    private Coroutine _tintRoutine;

    public void ApplySpeakerTint(bool isSpeaker)
    {
        if (isFadingIn) return; // skip tint while fading
        if (_tintRoutine != null) StopCoroutine(_tintRoutine);

        // keep the current alpha (from fade coroutine)
        float currentAlpha = _spriteImage.color.a;

        Color target = isSpeaker ? speakerColor : nonSpeakerColor;
        target.a = currentAlpha; // preserve fade alpha

        _tintRoutine = StartCoroutine(SmoothTintTo(target));
    }

    private IEnumerator SmoothTintTo(Color target)
    {
        Color start = _spriteImage.color;
        float t = 0f;

        while (t < tintDuration)
        {
            t += Time.deltaTime;
            float progress = Mathf.Clamp01(t / tintDuration);

            _spriteImage.color = Color.Lerp(start, target, progress);
            yield return null;
        }

        _spriteImage.color = target;
        _tintRoutine = null;
    }
}