using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CharacterPortrait : MonoBehaviour
{
    [SerializeField]
    private Image _spriteImage; // The Image component that displays the sprite
    public Image SpriteImage => _spriteImage;

    [SerializeField]
    private GameObject _characterRoot; // The root GameObject of this character (for position/scale)

    [Header("Animation Settings (Copy from DialogueManager)")]
    public float animationDuration = 0.2f;
    public Vector3 talkingScale = new Vector3(1.05f, 1.05f, 1.05f);
    public float talkingOpacity = 1f;
    public Vector3 idleScale = Vector3.one;
    public float idleOpacity = 0.7f;
    public float hiddenOpacity = 0f; // For completely hidden characters

    private Coroutine _currentAnimationRoutine;
    private string _currentCharacterName; // Keep track of who is in this slot

    public string CurrentCharacterName => _currentCharacterName;

    public void Setup(string characterName, Sprite initialSprite)
    {
        _currentCharacterName = characterName;
        _spriteImage.sprite = initialSprite;
        _characterRoot.SetActive(true);
        // Immediately set to idle state if this character just appeared
        _characterRoot.transform.localScale = idleScale;
        Color currentColor = _spriteImage.color;
        _spriteImage.color = new Color(currentColor.r, currentColor.g,
                                       currentColor.b, idleOpacity);
    }

    public void Clear()
    {
        _currentCharacterName = null;
        _spriteImage.sprite = null;
        _characterRoot.SetActive(false);
    }

    public void SetSprite(Sprite newSprite)
    {
        if (_spriteImage.sprite != newSprite) // Only update if different
        {
            _spriteImage.sprite = newSprite;
        }
    }

    public void SetTalkingState()
    {
        Animate(_characterRoot.transform, _spriteImage, talkingScale,
                talkingOpacity);
    }

    public void SetIdleState()
    {
        Animate(_characterRoot.transform, _spriteImage, idleScale, idleOpacity);
    }

    public void SetHiddenState()
    {
        // Animate to hidden opacity, then deactivate if desired
        Animate(_characterRoot.transform, _spriteImage, idleScale,
                hiddenOpacity, () => _characterRoot.SetActive(false));
    }

    private void Animate(Transform targetTransform, Image targetImage,
                         Vector3 endScale, float endOpacity,
                         System.Action onComplete = null)
    {
        if (_currentAnimationRoutine != null)
        {
            StopCoroutine(_currentAnimationRoutine);
        }
        _currentAnimationRoutine = StartCoroutine(AnimateCoroutine(
            targetTransform, targetImage, endScale, endOpacity, onComplete));
    }

    private IEnumerator AnimateCoroutine(Transform targetTransform,
                                         Image targetImage, Vector3 endScale,
                                         float endOpacity,
                                         System.Action onComplete)
    {
        Vector3 startScale = targetTransform.localScale;
        Color startColor = targetImage.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b,
                                   endOpacity);

        float timer = 0f;
        while (timer < animationDuration)
        {
            float progress = timer / animationDuration;
            targetTransform.localScale =
                Vector3.Lerp(startScale, endScale, progress);
            targetImage.color = Color.Lerp(startColor, endColor, progress);

            timer += Time.deltaTime;
            yield return null;
        }

        targetTransform.localScale = endScale;
        targetImage.color = endColor;
        _currentAnimationRoutine = null;
        onComplete?.Invoke();
    }

    // Called when the GameObject is disabled (e.g., when dialogue ends)
    private void OnDisable()
    {
        if (_characterRoot != null)
        {
            _characterRoot.transform.localScale = idleScale;
            Color currentColor = _spriteImage.color;
            _spriteImage.color =
                new Color(currentColor.r, currentColor.g, currentColor.b,
                          hiddenOpacity); // Fade out completely
            _characterRoot.SetActive(false);
        }
        if (_currentAnimationRoutine != null)
        {
            StopCoroutine(_currentAnimationRoutine);
            _currentAnimationRoutine = null;
        }
    }
}
