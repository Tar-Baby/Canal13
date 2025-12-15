using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class VideoPlayAndHoldLastFrame : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;

    private void Awake()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        videoPlayer.isLooping = false;
        videoPlayer.loopPointReached += OnFinished;
    }

    private void Start()
    {
        // Opcional pero recomendado para evitar frame negro inicial
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += OnPrepared;
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnFinished;
            videoPlayer.prepareCompleted -= OnPrepared;
        }
    }

    private void OnPrepared(VideoPlayer vp)
    {
        vp.Play();
    }

    private void OnFinished(VideoPlayer vp)
    {
        // Esto deja el último frame “congelado”
        vp.Pause();
    }
}
