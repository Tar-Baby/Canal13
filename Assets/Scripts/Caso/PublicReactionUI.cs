using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PublicReactionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image barraLlenar;
    [SerializeField] private Image barraVacia;
    [SerializeField] private Image barraMarco;
    [SerializeField] private Image estrella;
    [SerializeField] private TextMeshProUGUI ratingText;
    [SerializeField] private TextMeshProUGUI changeText;
    [SerializeField] private RectTransform ratingBarContainer;
    [SerializeField] private GameObject starBurstPrefab;
    [SerializeField] private AudioSource starSound;
    //[SerializeField] private OverlayParticlesBridge overlayParticles;

    private GameObject activeStarBurst; // referencia del efecto activo
    private Coroutine starShineRoutine;

    [Header("Particle Text Effect")]
    [SerializeField] private GameObject changeTextParticlePrefab;
    [SerializeField] private int particleCount = 10;
    [SerializeField] private float spread = 100f;

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

        // Por si se desactiva el objeto mientras el efecto está activo.
        DisableStarEffect();
    }

    private void Start()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        if (barraLlenar != null)
            barraLlenar.fillAmount = 0f;

        currentRating = 0;
        UpdateRatingDisplay(currentRating);
    }

    private void OnReactionChanged(int totalReaction, int change)
    {
        int newTotal = Mathf.Clamp(totalReaction, 0, maxRating);
        StartCoroutine(AnimateRatingChange(newTotal, change));
    }

    private IEnumerator AnimateRatingChange(int newTotal, int change)
    {
        int previousRating = currentRating;

        // --- Mostrar texto de cambio ---
        if (change != 0 && changeText != null)
        {
            changeText.text = change > 0 ? $"+{change}" : change.ToString();
            changeText.color = change > 0 ? positiveColor : negativeColor;
            changeText.gameObject.SetActive(true);
            StartCoroutine(AnimateChangeText());

            if (newTotal <= 0)
            {
                StartCoroutine(ShakeBar(0.6f, 20f));
                StartCoroutine(ShakeUI(changeText.rectTransform, 0.6f, 20f));
                StartCoroutine(
                    SpawnChangeTextParticlesAdvanced(change, negativeColor)
                );
                StartCoroutine(DelayedGameOverTransition(0.5f));
            }
            else if (change < 0)
            {
                StartCoroutine(ShakeBar(0.3f, 10f));
                StartCoroutine(ShakeUI(changeText.rectTransform, 0.3f, 12f));
                StartCoroutine(
                    SpawnChangeTextParticlesAdvanced(change, negativeColor)
                );
            }
            else if (change > 0)
            {
                StartCoroutine(
                    SpawnChangeTextParticlesAdvanced(change, positiveColor)
                );
            }
        }

        // --- Animar la barra ---
        float startValue = barraLlenar != null ? barraLlenar.fillAmount : 0f;
        float targetValue = Mathf.Clamp01((float)newTotal / maxRating);
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            float t = elapsedTime / animationDuration;
            float currentValue = Mathf.Lerp(startValue, targetValue, t);

            if (barraLlenar != null)
                barraLlenar.fillAmount = currentValue;

            UpdateRatingDisplay((int)(currentValue * maxRating));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (barraLlenar != null)
            barraLlenar.fillAmount = targetValue;

        UpdateRatingDisplay(newTotal);
        currentRating = newTotal;

        // --- Activar al llegar (o pasar) 100 y mantener hasta bajar de 100 ---
        bool wasMax = previousRating >= maxRating;
        bool isMax = currentRating >= maxRating;

        if (!wasMax && isMax)
            EnableStarEffect();
        else if (wasMax && !isMax)
            DisableStarEffect();
    }

    // --- Animación texto principal (+10 / -5) ---
    private IEnumerator AnimateChangeText()
    {
        if (changeText == null)
            yield break;

        changeText.gameObject.SetActive(true);
        changeText.alpha = 1f;

        Vector3 originalScale = changeText.transform.localScale;
        Vector3 targetScale = originalScale * 1.2f;

        float elapsed = 0f;
        float scaleDuration = 0.3f;

        while (elapsed < scaleDuration)
        {
            float t = elapsed / scaleDuration;
            changeText.transform.localScale = Vector3.Lerp(
                originalScale,
                targetScale,
                t
            );
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(1f);

        Color originalColor = changeText.color;
        elapsed = 0f;
        float fadeDuration = 0.5f;

        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            Color newColor = originalColor;
            newColor.a = Mathf.Lerp(1f, 0f, t);
            changeText.color = newColor;
            elapsed += Time.deltaTime;
            yield return null;
        }

        changeText.transform.localScale = originalScale;
        changeText.color = originalColor;
        changeText.gameObject.SetActive(false);
    }

    private void UpdateRatingDisplay(int rating)
    {
        if (ratingText != null)
        {
            float percent = ((float)rating / maxRating) * 100f;
            ratingText.text = $"Rating: {percent:0}%";
        }
    }

    private IEnumerator DelayedGameOverTransition(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (SceneFader.Instance != null)
            SceneFader.Instance.FadeToScene("GameOver");
        else
            SceneManager.LoadScene("GameOver");
    }

    private IEnumerator ShakeBar(float duration, float magnitude)
    {
        if (ratingBarContainer == null)
            yield break;

        Vector3 originalPos = ratingBarContainer.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;
            ratingBarContainer.localPosition =
                originalPos + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        ratingBarContainer.localPosition = originalPos;
    }

    private IEnumerator ShakeUI(RectTransform target, float duration, float magnitude)
    {
        if (target == null)
            yield break;

        Vector3 originalPos = target.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;
            target.localPosition = originalPos + new Vector3(offsetX, offsetY, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        target.localPosition = originalPos;
    }

    // --- Partículas centradas: área central, movimiento vertical +/- ---
    private IEnumerator SpawnChangeTextParticlesAdvanced(int change, Color color)
    {
        if (changeTextParticlePrefab == null || changeText == null)
            yield break;

        bool isPositive = change > 0;

        // más densidad visual
        int totalParticles = particleCount * 3;
        RectTransform canvasRect = changeText.canvas.GetComponent<RectTransform>();

        // Franja central de spawn
        float widthRange = canvasRect.rect.width * 0.95f; // casi todo el ancho
        float heightOffset = canvasRect.rect.height * 0.25f; // franja central
        float spawnPadding = (widthRange / totalParticles) * 0.8f;

        List<float> usedPositions = new List<float>();

        for (int i = 0; i < totalParticles; i++)
        {
            GameObject particle = Instantiate(
                changeTextParticlePrefab,
                changeText.transform.parent
            );
            particle.SetActive(true);

            TextMeshProUGUI tmp = particle.GetComponent<TextMeshProUGUI>();
            RectTransform rect = particle.GetComponent<RectTransform>();

            if (tmp == null || rect == null)
            {
                Destroy(particle);
                continue;
            }

            tmp.text = isPositive ? $"+{change}" : change.ToString();
            tmp.color = color;
            tmp.alpha = 0f;

            float startX;
            int maxTries = 10;
            do
            {
                startX = Random.Range(-widthRange / 2f, widthRange / 2f);
                maxTries--;
            } while (
                usedPositions.Exists(pos => Mathf.Abs(pos - startX) < spawnPadding)
                && maxTries > 0
            );

            usedPositions.Add(startX);
            float startY = Random.Range(-heightOffset, heightOffset);
            rect.localPosition = new Vector3(startX, startY, 0f);

            float startScale = Random.Range(0.1f, 2.8f);
            rect.localScale = Vector3.one * startScale;

            Vector3 direction = isPositive ? Vector3.up : Vector3.down;

            float speed = Random.Range(80f, 130f);
            float lifetime = Random.Range(1.0f, 1.7f);
            float targetScale = Random.Range(0.8f, 1.6f);

            float delay = Random.Range(0f, 0.1f);
            StartCoroutine(
                AnimateParticleRect(
                    rect,
                    tmp,
                    direction,
                    speed,
                    lifetime,
                    targetScale,
                    delay
                )
            );
        }

        yield break;
    }

    private IEnumerator AnimateParticleRect(
        RectTransform rect,
        TextMeshProUGUI tmp,
        Vector3 direction,
        float speed,
        float lifetime,
        float targetScale,
        float delay
    )
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        float noise = Random.Range(0.6f, 1.4f);
        float maxAlpha = Random.Range(0.5f, 0.85f);

        while (elapsed < lifetime)
        {
            float t = elapsed / lifetime;
            float ease = Mathf.SmoothStep(0f, 1f, t);

            rect.localPosition += direction * speed * ease * Time.deltaTime;
            rect.localPosition += new Vector3(
                Mathf.Sin(elapsed * 5f * noise) * 30f * Time.deltaTime,
                0f,
                0f
            );

            if (t < 0.25f)
                tmp.alpha = Mathf.Lerp(0f, maxAlpha, t / 0.25f);
            else if (t > 0.8f)
                tmp.alpha = Mathf.Lerp(maxAlpha, 0f, (t - 0.8f) / 0.2f);
            else
                tmp.alpha = maxAlpha;

            if (t < 0.25f)
            {
                float bounce = Mathf.Sin((t / 0.25f) * Mathf.PI);
                float scaleValue = Mathf.Lerp(
                    rect.localScale.x,
                    targetScale * 1.1f,
                    bounce
                );
                rect.localScale = Vector3.one * scaleValue;
            }
            else
            {
                rect.localScale = Vector3.Lerp(
                    rect.localScale,
                    Vector3.one * targetScale,
                    Time.deltaTime * 2.5f
                );
            }

            rect.Rotate(0f, 0f, Random.Range(-25f, 25f) * Time.deltaTime);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(rect.gameObject);
    }

    // Activa el efecto sobre la estrella (se mantiene mientras rating >= 100)
    private void EnableStarEffect()
    {
        if (starBurstPrefab == null || estrella == null)
            return;

        // Si ya está activo, evitar duplicarlo
        if (activeStarBurst != null)
            return;

        if (starSound != null)
            starSound.Play();

        activeStarBurst = Instantiate(starBurstPrefab, estrella.transform.parent);
        activeStarBurst.transform.position = estrella.transform.position;
        activeStarBurst.transform.localScale = Vector3.one;

        if (
            activeStarBurst.TryGetComponent<RectTransform>(out RectTransform burstRect)
            && estrella.TryGetComponent<RectTransform>(out RectTransform starRect)
        )
        {
            burstRect.anchorMin = starRect.anchorMin;
            burstRect.anchorMax = starRect.anchorMax;
            burstRect.anchoredPosition = starRect.anchoredPosition;
        }

        ParticleSystem system = activeStarBurst.GetComponent<ParticleSystem>();
        if (system != null)
            system.Play();

        if (starShineRoutine == null)
            starShineRoutine = StartCoroutine(StarShineLoop());
    }

    // Desactiva el efecto al bajar de 100%
    private void DisableStarEffect()
    {
        // 1) Apagar y destruir el efecto instanciado
        if (activeStarBurst != null)
        {
            var ps = activeStarBurst.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                // Detiene la emisión; deja que las partículas vivas mueran
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            // Destruir después de un momento para que termine de “desvanecer”
            Destroy(activeStarBurst, 1.5f);
            activeStarBurst = null;
        }

        // 2) Detener el brillo/rotación de la estrella
        if (starShineRoutine != null)
        {
            StopCoroutine(starShineRoutine);
            starShineRoutine = null;
        }

        // 3) Restaurar visual de la estrella (si aplica)
        if (estrella != null)
            estrella.transform.rotation = Quaternion.identity;
    }

    private void ResetStarVisuals()
    {
        if (estrella == null)
            return;

        estrella.transform.rotation = Quaternion.identity;

        Image starImg = estrella.GetComponent<Image>();
        if (starImg != null)
        {
            // Si quieres preservar el color original “de diseño”, puedes
            // serializarlo; por ahora lo dejamos tal cual si no se cambió.
            // Aquí lo reseteamos a su propio color actual (sin pulso).
            // (Si quieres reset fijo, dímelo y lo ajusto.)
        }
    }

    // Animación de rotación/brillo constante de la estrella mientras hay 100%
    private IEnumerator StarShineLoop()
    {
        if (estrella == null)
            yield break;

        float rotationSpeed = 60f; // grados por segundo
        float glowPulse = 1f;
        float t = 0f;

        Image starImg = estrella.GetComponent<Image>();
        Color baseColor = starImg != null ? starImg.color : Color.white;

        while (activeStarBurst != null)
        {
            estrella.transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

            if (starImg != null)
            {
                t += Time.deltaTime * 2f;
                float glow = (Mathf.Sin(t) + 1f) * 0.5f * glowPulse;
                starImg.color = Color.Lerp(baseColor, Color.white, glow);
            }

            yield return null;
        }

        estrella.transform.rotation = Quaternion.identity;
        if (starImg != null)
            starImg.color = baseColor;
    }

    public int GetCurrentRating() => currentRating;
}