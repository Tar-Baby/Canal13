using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PublicReactionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider ratingBar;
    [SerializeField] private TextMeshProUGUI ratingText;
    [SerializeField] private TextMeshProUGUI changeText; // Para mostrar +10, +5, etc.
    
    [Header("Settings")]
    [SerializeField] private int maxRating = 100;
    [SerializeField] private float animationDuration = 1f;
    [SerializeField] private Color positiveColor = Color.green;
    [SerializeField] private Color negativeColor = Color.red;
    
    private int currentRating = 0;
    
    private void OnEnable()
    {
        DialogEvents.OnEpisodeRatingChanged += OnReactionChanged;
    }
    
    private void OnDisable()
    {
        DialogEvents.OnEpisodeRatingChanged -= OnReactionChanged;
    }
    
    private void Start()
    {
        InitializeUI();
    }
    
    private void InitializeUI()
    {
        if (ratingBar != null)
        {
            ratingBar.maxValue = maxRating;
            ratingBar.value = 0;  //empezamos con 10
        }
        
        UpdateRatingDisplay(0);
    }
    
    private void OnReactionChanged(int totalReaction, int change)
    {
        StartCoroutine(AnimateRatingChange(totalReaction, change));
    }
    
    private IEnumerator AnimateRatingChange(int newTotal, int change)
    {
        // Mostrar el cambio (+10, +5, etc.)
        if (changeText != null && change != 0)
        {
            changeText.text = change > 0 ? $"+{change}" : change.ToString();
            changeText.color = change > 0 ? positiveColor : negativeColor;
            changeText.gameObject.SetActive(true);
            
            // Animar el texto de cambio
            StartCoroutine(AnimateChangeText());
        }
        
        // Animar la barra
        if (ratingBar != null)
        {
            float startValue = ratingBar.value;
            float targetValue = Mathf.Clamp(newTotal, 0, maxRating);
            
            float elapsedTime = 0f;
            
            while (elapsedTime < animationDuration)
            {
                float t = elapsedTime / animationDuration;
                float currentValue = Mathf.Lerp(startValue, targetValue, t);
                
                ratingBar.value = currentValue;
                UpdateRatingDisplay((int)currentValue);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            ratingBar.value = targetValue;
            UpdateRatingDisplay(newTotal);
        }
        
        currentRating = newTotal;
    }
    
    private IEnumerator AnimateChangeText()
    {
        if (changeText == null) yield break;
        
        // Escalar hacia arriba
        Vector3 originalScale = changeText.transform.localScale;
        Vector3 targetScale = originalScale * 1.2f;
        
        float elapsedTime = 0f;
        float scaleDuration = 0.3f;
        
        while (elapsedTime < scaleDuration)
        {
            float t = elapsedTime / scaleDuration;
            changeText.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Mantener visible por un momento
        yield return new WaitForSeconds(1f);
        
        // Fade out
        Color originalColor = changeText.color;
        elapsedTime = 0f;
        float fadeDuration = 0.5f;
        
        while (elapsedTime < fadeDuration)
        {
            float t = elapsedTime / fadeDuration;
            Color newColor = originalColor;
            newColor.a = Mathf.Lerp(1f, 0f, t);
            changeText.color = newColor;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ocultar y resetear
        changeText.gameObject.SetActive(false);
        changeText.transform.localScale = originalScale;
        changeText.color = originalColor;
    }
    
    private void UpdateRatingDisplay(int rating)
    {
        if (ratingText != null)
        {
            ratingText.text = $"Rating: {rating}/{maxRating}";
        }
    }
    
    public int GetCurrentRating()
    {
        return currentRating;
    }
}