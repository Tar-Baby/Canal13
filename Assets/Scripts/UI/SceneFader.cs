using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        // Patrón Singleton — solo uno en toda la vida del juego
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeImage != null)
        {
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
            StartCoroutine(FadeInAtStart());
        }
    }
        
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (fadeImage == null)
        {
            // Busca una nueva FadeImage en la nueva escena
            Image found = FindObjectOfType<Image>(true);
            if (found != null)
            {
                fadeImage = found;
                Debug.Log("[SceneFader] FadeImage reconectado en la nueva escena.");
            }
            else
            {
                Debug.LogWarning("[SceneFader] No se encontró FadeImage en esta escena.");
            }
        }
    }
    
    // =========================================================
    // 🔶 USO PRINCIPAL
    // =========================================================

    /// <summary>
    /// Hace un fade out → cambia a una escena → fade in.
    /// </summary>
    public void FadeToScene(string sceneName)
    {
        if (!isFading)
            StartCoroutine(FadeOutAndLoad(sceneName));
    }

    /// <summary>
    /// Hace un fade in u out dentro de la misma escena (sin cambiarla)
    /// </summary>
    public void FadeIn(float duration = -1f)
    {
        if (!isFading)
            StartCoroutine(Fade(1f, 0f, duration > 0 ? duration : fadeDuration));
    }

    public void FadeOut(float duration = -1f)
    {
        if (!isFading)
            StartCoroutine(Fade(0f, 1f, duration > 0 ? duration : fadeDuration));
    }

    // =========================================================
    // 🔶 CORRUTINAS DE FADE
    // =========================================================

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
        if (fadeImage == null)
        {
            Debug.LogWarning("SceneFader: No Fade Image assigned!");
            yield break;
        }

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
