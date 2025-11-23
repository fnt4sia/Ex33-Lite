using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    public float parallaxStrength = 0.1f;

    private Vector3 lastCamPos;
    private float lastCamSize;

    private Vector3 initialScale;

    private void Start()
    {
        initialScale = transform.localScale;
        lastCamPos = Camera.main.transform.position;
        lastCamSize = Camera.main.orthographicSize;
    }

    public void UpdateLayer(Transform cam)
    {
        Vector3 camPos = cam.position;
        float camSize = Camera.main.orthographicSize;

        Vector3 camDelta = camPos - lastCamPos;
        transform.position += new Vector3(camDelta.x * parallaxStrength, 0, 0);

        float zoomFactor = camSize / lastCamSize;  
        transform.localScale = parallaxStrength * zoomFactor * initialScale;

        lastCamPos = camPos;
        lastCamSize = camSize;
    }
}
