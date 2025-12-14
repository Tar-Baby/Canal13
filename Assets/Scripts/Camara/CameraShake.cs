using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    [Header("Shake Settings")]
    [SerializeField] private float defaultDuration = 0.3f;
    [SerializeField] private float defaultMagnitude = 0.3f;
    [SerializeField] private AnimationCurve attenuationCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private Vector3 originalPosition;
    private Coroutine currentShake;

    private void Awake()
    {
        // No persiste, solo asegura una instancia local por escena
        Instance = this;

        // Guardar posición original
        originalPosition = transform.localPosition;
    }

    public void Shake(float duration, float magnitude)
    {
        if (currentShake != null)
            StopCoroutine(currentShake);

        currentShake = StartCoroutine(DoShake(duration, magnitude));
    }

    public void Shake()
    {
        Shake(defaultDuration, defaultMagnitude);
    }

    private IEnumerator DoShake(float duration, float magnitude)
    {
        originalPosition = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float damper = attenuationCurve.Evaluate(elapsed / duration);

            float offsetX = Random.Range(-1f, 1f) * magnitude * damper;
            float offsetY = Random.Range(-1f, 1f) * magnitude * damper;

            transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
        currentShake = null;
    }
}