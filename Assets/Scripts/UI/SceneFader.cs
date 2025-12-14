using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance;

    [Header("Fade Settings")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private Color fadeColor = Color.black;

    [Header("Escenas donde el fade está activo")]
    [Tooltip("Escribe exactamente los nombres de las escenas que usarán el fade.")]
    [SerializeField] private string[] enabledScenes = { "Nivel1", "GameOver", "Creditos" };

    private bool isFading = false;

    private void Awake()
    {
        // Singleton persistente
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeImage != null)
        {
            // Color inicial: completamente transparente.
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        }
    }

    private void Start()
    {
        if (fadeImage != null)
            StartCoroutine(FadeInAtStart());
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
        // Determina si el fade se usará en la nueva escena.
        if (fadeImage != null)
        {
            bool active = enabledScenes.Contains(scene.name);
            fadeImage.enabled = active;

            if (!active)
            {
                // Asegura transparencia y evita toques indeseados
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
            }

            Debug.Log($"[SceneFader] Escena '{scene.name}' -> Fade activo: {active}");
        }
    }

    // ----------------------------------------------------------
    // MÉTODOS DE USO
    // ----------------------------------------------------------

    /// <summary>
    /// Hace un fade out → cambia a una escena → fade in, 
    /// solo si la escena destino está incluida en enabledScenes.
    /// </summary>
    public void FadeToScene(string sceneName)
    {
        // Si la escena destino NO está en la lista, cargar sin fade.
        if (!enabledScenes.Contains(sceneName))
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        if (!isFading && fadeImage != null)
            StartCoroutine(FadeOutAndLoad(sceneName));
    }

    public void FadeIn(float duration = -1f)
    {
        if (fadeImage == null || isFading) return;
        StartCoroutine(Fade(1f, 0f, duration > 0 ? duration : fadeDuration));
    }

    public void FadeOut(float duration = -1f)
    {
        if (fadeImage == null || isFading) return;
        StartCoroutine(Fade(0f, 1f, duration > 0 ? duration : fadeDuration));
    }

    // ----------------------------------------------------------
    // CORRUTINAS
    // ----------------------------------------------------------

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        isFading = true;

        // Verificar si en la escena actual el fade está habilitado
        Scene currentScene = SceneManager.GetActiveScene();
        bool currentSceneEnabled = enabledScenes.Contains(currentScene.name);

        // Fade out solo si la escena actual también es válida
        if (currentSceneEnabled)
            yield return Fade(0f, 1f, fadeDuration);

        // Cargar escena nueva
        yield return SceneManager.LoadSceneAsync(sceneName);

        // Si la nueva escena soporta fade, hacer fade in
        if (enabledScenes.Contains(sceneName))
            yield return Fade(1f, 0f, fadeDuration);
        else if (fadeImage != null)
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);

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
            if (fadeImage == null) yield break;

            float t = Mathf.Clamp01(elapsed / duration);
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            fadeImage.color = new Color(c.r, c.g, c.b, alpha);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (fadeImage != null)
            fadeImage.color = new Color(c.r, c.g, c.b, endAlpha);
    }

    private IEnumerator FadeInAtStart()
    {
        yield return Fade(1f, 0f, fadeDuration);
    }
}