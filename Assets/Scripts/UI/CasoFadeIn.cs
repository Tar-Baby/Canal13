using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class CasoFadeIn : MonoBehaviour
{
    [Header("Screen Fade")]
    [SerializeField] private CanvasGroup blackFade;  // alpha = 1 in editor
    [SerializeField] private float visualFadeDuration = 0.75f;

    [Header("Audio Mixer (Music)")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string musicVolumeParam = "MusicVolume";

    // Since previous scene ended at -30 then snapped to -80 at the very end,
    // we can start from -30 for a seamless feel, or from -80 for true silence.
    [SerializeField] private float startDb = -30f;   // try -30f for seamless handoff
    [SerializeField] private float targetDb = -15f;  // your normal level (-15 or 0)
    [SerializeField] private float audioFadeDuration = 0.9f;

    [Header("Use saved volume")]
    [SerializeField] private bool useSavedVolume = false;

    private void Awake()
    {
        if (blackFade != null)
        {
            blackFade.gameObject.SetActive(true);
            blackFade.alpha = 1f;
            blackFade.blocksRaycasts = true;
            blackFade.interactable = true;
        }

        if (audioMixer != null)
        {
            float finalTarget = targetDb;
            if (useSavedVolume)
            {
                finalTarget = PlayerPrefs.GetFloat(musicVolumeParam, targetDb);
            }

            targetDb = finalTarget;
            audioMixer.SetFloat(musicVolumeParam, startDb);
        }
    }

    private void Start()
    {
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float t = 0f;
        float total = Mathf.Max(visualFadeDuration, audioFadeDuration);

        while (t < total)
        {
            t += Time.deltaTime;

            // Visual: 1 -> 0
            if (blackFade != null && visualFadeDuration > 0f)
            {
                float uv = Mathf.Clamp01(t / visualFadeDuration);
                blackFade.alpha = 1f - uv;
            }

            // Audio: startDb -> targetDb with easing
            if (audioMixer != null && audioFadeDuration > 0f)
            {
                float ua = Mathf.Clamp01(t / audioFadeDuration);
                float eased = ua * ua * (3f - 2f * ua);
                float newDb = Mathf.Lerp(startDb, targetDb, eased);
                audioMixer.SetFloat(musicVolumeParam, newDb);
            }

            yield return null;
        }

        if (blackFade != null)
        {
            blackFade.alpha = 0f;
            blackFade.blocksRaycasts = false;
            blackFade.interactable = false;
        }

        if (audioMixer != null)
        {
            audioMixer.SetFloat(musicVolumeParam, targetDb);
        }
    }
}
