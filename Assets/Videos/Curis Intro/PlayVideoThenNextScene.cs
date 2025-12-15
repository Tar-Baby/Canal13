using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class PlayVideoThenNextScene : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private string nextSceneName = "Menu";

    private bool loading;

    private void Awake()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        videoPlayer.isLooping = false;
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void Start()
    {
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += OnPrepared;
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.prepareCompleted -= OnPrepared;
        }
    }

    private void OnPrepared(VideoPlayer vp)
    {
        vp.Play();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (loading)
            return;

        loading = true;
        SceneManager.LoadScene(nextSceneName);
    }
}