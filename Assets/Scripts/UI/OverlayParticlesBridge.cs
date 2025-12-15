using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

public class OverlayParticlesBridge : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera fxCamera;
    [SerializeField] private RawImage fxImage;

    [Header("Layer")]
    [SerializeField] private int fxLayer = 0; // Set to UIFX layer in Inspector

    [Header("World Mapping / Size")]
    [SerializeField] private float zDistance = 10f;
    [SerializeField] private float fxOrthographicSize = 6f;
    [SerializeField] private float effectScale = 80f;

    private RenderTexture rt;
    private int lastW;
    private int lastH;

    private void Awake()
    {
        if (fxCamera == null)
        {
            Debug.LogError("OverlayParticlesBridge: fxCamera is null.");
            enabled = false;
            return;
        }

        if (fxImage == null)
        {
            Debug.LogError("OverlayParticlesBridge: fxImage is null.");
            enabled = false;
            return;
        }

        fxImage.raycastTarget = false;
        // Importante: NO hacemos SetAsLastSibling() aquí, porque tú controlas
        // el orden con tus FXCanvas/StarCanvas.

        fxCamera.orthographic = true;
        fxCamera.clearFlags = CameraClearFlags.SolidColor;
        fxCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
        fxCamera.cullingMask = 1 << fxLayer;

        ResizeIfNeeded(force: true);
    }

    private void LateUpdate()
    {
        ResizeIfNeeded(force: false);

        if (fxCamera != null)
            fxCamera.orthographicSize = fxOrthographicSize;

        if (fxCamera != null)
        {
            Vector3 camPos = fxCamera.transform.position;
            fxCamera.transform.position = new Vector3(0f, 0f, -zDistance);
        }
    }

    private void ResizeIfNeeded(bool force)
    {
        int w = Screen.width;
        int h = Screen.height;

        if (!force && w == lastW && h == lastH)
            return;

        lastW = w;
        lastH = h;

        if (rt != null)
        {
            fxCamera.targetTexture = null;
            rt.Release();
            Destroy(rt);
        }

        // URP RenderGraph requiere depthBufferBits != 0 (no None).
        var desc = new RenderTextureDescriptor(w, h);
        desc.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
        desc.depthBufferBits = 24;
        desc.msaaSamples = 1;
        desc.sRGB = (QualitySettings.activeColorSpace == ColorSpace.Linear);

        rt = new RenderTexture(desc);
        rt.name = "UIFX_RT";
        rt.Create();

        fxCamera.targetTexture = rt;
        fxImage.texture = rt;

        fxCamera.orthographicSize = fxOrthographicSize;
    }

    public GameObject SpawnAndFollow(GameObject prefab, RectTransform uiTarget)
    {
        if (prefab == null || uiTarget == null)
            return null;

        GameObject go = Instantiate(prefab);
        SetLayerRecursive(go, fxLayer);

        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = Vector3.one * effectScale;

        var follower = go.AddComponent<UIToWorldFollower>();
        follower.Init(uiTarget, fxCamera, zDistance);

        // Forzar scaling correcto para que el tamaño responda a effectScale
        // (y no se vuelva microscópico).
        var ps = go.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            ps.Play();
        }

        return go;
    }

    private static void SetLayerRecursive(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
            SetLayerRecursive(child.gameObject, layer);
    }
}

public class UIToWorldFollower : MonoBehaviour
{
    private RectTransform target;
    private Camera fxCamera;
    private float zDistance;

    public void Init(RectTransform target, Camera fxCamera, float zDistance)
    {
        this.target = target;
        this.fxCamera = fxCamera;
        this.zDistance = zDistance;
        UpdatePosition();
    }

    private void LateUpdate()
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        if (target == null || fxCamera == null)
            return;

        // En Canvas Overlay, camera = null para obtener screen point correcto.
        Vector2 sp = RectTransformUtility.WorldToScreenPoint(null, target.position);

        // Proyectar ese punto de pantalla al mundo de la fxCamera.
        Vector3 wp = fxCamera.ScreenToWorldPoint(
            new Vector3(sp.x, sp.y, zDistance)
        );

        transform.position = wp;
    }
}