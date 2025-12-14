using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance;

    [Header("Fade Settings")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private Color fadeColor = Color.black;

    private bool isFading = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Color inicial: transparente
        if (fadeImage != null)
        {
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        }
    }

    private void Start()
    {
        if (fadeImage != null)
            StartCoroutine(FadeInAtStart());
    }

    // ---------------------------------------------
    // MÉTODOS PÚBLICOS
    // ---------------------------------------------

    public void FadeToScene(string sceneName)
    {
        if (!isFading && fadeImage != null)
            StartCoroutine(FadeOutAndLoad(sceneName));
    }

    public void FadeIn(float duration = -1f)
    {
        if (!isFading && fadeImage != null)
            StartCoroutine(Fade(1f, 0f, duration > 0 ? duration : fadeDuration));
    }

    public void FadeOut(float duration = -1f)
    {
        if (!isFading && fadeImage != null)
            StartCoroutine(Fade(0f, 1f, duration > 0 ? duration : fadeDuration));
    }

    // ---------------------------------------------
    // CORRUTINAS
    // ---------------------------------------------

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        isFading = true;

        // Fade Out
        yield return Fade(0f, 1f, fadeDuration);

        // Cargar escena
        yield return SceneManager.LoadSceneAsync(sceneName);

        // Fade In
        yield return Fade(1f, 0f, fadeDuration);

        isFading = false;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        Color c = fadeColor;

        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            fadeImage.color = new Color(c.r, c.g, c.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        fadeImage.color = new Color(c.r, c.g, c.b, endAlpha);
    }

    private IEnumerator FadeInAtStart()
    {
        yield return Fade(1f, 0f, fadeDuration);
    }
}