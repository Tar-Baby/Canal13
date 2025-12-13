using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PublicReactionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image barraLlenar; // La imagen que se llenará (Barra_llenar)
    [SerializeField] private Image barraVacia;  // Opcional, solo decorativo (Barra_vacía)
    [SerializeField] private Image barraMarco;  // Imagen frontal decorativa (Barra)
    [SerializeField] private Image estrella;    // Imagen fija decorativa (Estrella)
    [SerializeField] private TextMeshProUGUI ratingText;
    [SerializeField] private TextMeshProUGUI changeText;
    
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
        if (barraLlenar != null)
            barraLlenar.fillAmount = 0f;
        
        UpdateRatingDisplay(0);
    }

    private void OnReactionChanged(int totalReaction, int change)
    {
        StartCoroutine(AnimateRatingChange(totalReaction, change));
    }

    private IEnumerator AnimateRatingChange(int newTotal, int change)
    {
        // Mostrar el cambio (+10, -5)
        if (changeText != null && change != 0)
        {
            changeText.text = change > 0 ? $"+{change}" : change.ToString();
            changeText.color = change > 0 ? positiveColor : negativeColor;
            changeText.gameObject.SetActive(true);

            StartCoroutine(AnimateChangeText());
        }

        // Animar llenado
        if (barraLlenar != null)
        {
            float startFill = barraLlenar.fillAmount;
            float targetFill = Mathf.Clamp01((float)newTotal / maxRating);
            float elapsedTime = 0f;

            while (elapsedTime < animationDuration)
            {
                float t = elapsedTime / animationDuration;
                float currentFill = Mathf.Lerp(startFill, targetFill, t);

                barraLlenar.fillAmount = currentFill;
                UpdateRatingDisplay(Mathf.RoundToInt(currentFill * maxRating));

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            barraLlenar.fillAmount = targetFill;
            UpdateRatingDisplay(newTotal);
        }

        currentRating = newTotal;
    }

    private IEnumerator AnimateChangeText()
    {
        if (changeText == null) yield break;

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

        yield return new WaitForSeconds(1f);

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

        changeText.gameObject.SetActive(false);
        changeText.transform.localScale = originalScale;
        changeText.color = originalColor;
    }

    private void UpdateRatingDisplay(int rating)
    {
        if (ratingText != null)
            //ratingText.text = $"Rating: {rating}/{maxRating}";
            ratingText.text = $"Rating: {rating}%";
    }

    public int GetCurrentRating() => currentRating;
}