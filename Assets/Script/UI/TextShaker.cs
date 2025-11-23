using UnityEngine;

public class TextShaker : MonoBehaviour
{
    public float magnitude = 0.1f; 
    private Vector3 originalPos;

    void Awake()
    {
        originalPos = transform.localPosition;
    }
    void Update()
    {
        float offsetX = Random.Range(-1f, 1f) * magnitude;
        float offsetY = Random.Range(-1f, 1f) * magnitude;

        transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0);
    }
}