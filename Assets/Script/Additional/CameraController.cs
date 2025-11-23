using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    [Header("Targets")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform enemy;

    [Header("Zoom Levels")]
    [SerializeField] private float mainMenuZoom;
    [SerializeField] private float normalZoom;
    [SerializeField] private float playerZoom;

    [Header("Camera Move Speeds")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float zoomSpeed;

    [Header("Parallax Backgrounds")]
    [SerializeField] private ParallaxLayer[] parallaxLayers;

    private Camera cam;
    private Vector3 cameraVelocity = Vector3.zero;
    private bool isShaking = false;
    private float currentZoomVelocity;

    public bool isStarted = false;

    void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        if (isStarted) SetNormalView();
    }

    void LateUpdate()
    {
        // Parallax movement happens every frame
        UpdateParallax();
    }

    // ============================================================
    //                       MAIN VIEWS
    // ============================================================

    public void SetMainMenuView()
    {
        StopAllCoroutines();
        StartCoroutine(MoveToPosition(GetMenuCenter(), mainMenuZoom));
    }

    public void SetNormalView()
    {
        StopAllCoroutines();
        StartCoroutine(MoveToPosition(GetCenterBetweenTargets(), normalZoom));

        if (!isStarted) isStarted = true;
    }

    public void SetPlayerZoom()
    {
        StopAllCoroutines();
        StartCoroutine(MoveToPosition(player.position, playerZoom));
    }

    // ============================================================
    //                   CAMERA POSITION HELPERS
    // ============================================================

    private Vector3 GetMenuCenter()
    {
        // Usually the midpoint but further up/back
        return new Vector3((player.position.x + enemy.position.x) * 0.5f,
                           (player.position.y + enemy.position.y) * 0.5f + 2f,
                           transform.position.z);
    }

    private Vector3 GetCenterBetweenTargets()
    {
        return new Vector3((player.position.x + enemy.position.x) * 0.5f,
                           (player.position.y + enemy.position.y) * 0.5f + 3f,
                           transform.position.z);
    }

    // ============================================================
    //                    CAMERA MOVE / ZOOM
    // ============================================================

    private IEnumerator MoveToPosition(Vector3 targetPos, float targetZoom)
    {
        float threshold = 0.01f;  

        while (true)
        {
            // Smooth move
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPos,
                ref cameraVelocity,
                0.15f
            );

            cam.orthographicSize = Mathf.SmoothDamp(
                cam.orthographicSize,
                targetZoom,
                ref currentZoomVelocity,
                0.15f
            );

            bool posDone = Vector2.Distance(transform.position, targetPos) < threshold;
            bool zoomDone = Mathf.Abs(cam.orthographicSize - targetZoom) < threshold;

            if (posDone && zoomDone)
                break;

            yield return null;
        }

        transform.position = targetPos;
        cam.orthographicSize = targetZoom;
    }

    // ============================================================
    //                         CAMERA SHAKE
    // ============================================================

    public void Shake(float intensity, float duration)
    {
        if (!isShaking)
            StartCoroutine(ShakeRoutine(intensity, duration));
    }

    private IEnumerator ShakeRoutine(float intensity, float duration)
    {
        isShaking = true;

        Vector3 origPos = transform.position;

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;

            float offsetX = Random.Range(-1f, 1f) * intensity;
            float offsetY = Random.Range(-1f, 1f) * intensity;

            transform.position = origPos + new Vector3(offsetX, offsetY, 0);

            yield return null;
        }

        transform.position = origPos;
        isShaking = false;
    }

    // ============================================================
    //                        PARALLAX SYSTEM
    // ============================================================

    private void UpdateParallax()
    {
        foreach (var layer in parallaxLayers)
        {
            layer.UpdateLayer(transform);
        }
    }
}

